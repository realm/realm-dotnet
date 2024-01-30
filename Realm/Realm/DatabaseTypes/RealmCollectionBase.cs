////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Realms.Exceptions;
using Realms.Helpers;
using Realms.Schema;
using static Realms.NotifiableObjectHandleBase;

namespace Realms
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "This should not be directly accessed by users.")]
    [SuppressMessage("Design", "CA1010:Generic interface should also be implemented", Justification = "IList conformance is needed for UWP databinding. IList<T> is not necessary.")]
    public abstract class RealmCollectionBase<T>
        : INotifiable<NotifiableObjectHandleBase.CollectionChangeSet>,
          IRealmCollection<T>,
          IList,
          IThreadConfined,
          IMetadataObject
    {
        private readonly Lazy<NotificationCallbacks<T>> _notificationCallbacks;

        internal readonly bool _isEmbedded;

        internal readonly Lazy<CollectionHandleBase> Handle;

        internal readonly Metadata? Metadata;

        internal bool IsDynamic;

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "This is the private event - the public is uppercased.")]
        private event NotifyCollectionChangedEventHandler? _collectionChanged;

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "This is the private event - the public is uppercased.")]
        private event PropertyChangedEventHandler? _propertyChanged;

        public event NotifyCollectionChangedEventHandler? CollectionChanged
        {
            add
            {
                UpdateCollectionChangedSubscriptionIfNecessary(isSubscribed: true);

                _collectionChanged += value;
            }

            remove
            {
                _collectionChanged -= value;

                UpdateCollectionChangedSubscriptionIfNecessary(isSubscribed: false);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged
        {
            add
            {
                UpdateCollectionChangedSubscriptionIfNecessary(isSubscribed: true);

                _propertyChanged += value;
            }

            remove
            {
                _propertyChanged -= value;

                UpdateCollectionChangedSubscriptionIfNecessary(isSubscribed: false);
            }
        }

        [IgnoreDataMember, XmlIgnore]
#if NET6_0_OR_GREATER
        [System.Text.Json.Serialization.JsonIgnore]
#endif
        public int Count
        {
            get => IsValid ? Handle.Value.Count() : 0;
        }

        [IgnoreDataMember, XmlIgnore]
#if NET6_0_OR_GREATER
        [System.Text.Json.Serialization.JsonIgnore]
#endif
        public ObjectSchema? ObjectSchema => Metadata?.Schema;

        Metadata? IMetadataObject.Metadata => Metadata;

        [IgnoreDataMember, XmlIgnore]
#if NET6_0_OR_GREATER
        [System.Text.Json.Serialization.JsonIgnore]
#endif
        public bool IsManaged => Realm != null;

        [IgnoreDataMember, XmlIgnore]
#if NET6_0_OR_GREATER
        [System.Text.Json.Serialization.JsonIgnore]
#endif
        public bool IsValid => Handle.Value.IsValid;

        [IgnoreDataMember, XmlIgnore]
#if NET6_0_OR_GREATER
        [System.Text.Json.Serialization.JsonIgnore]
#endif
        public bool IsFrozen => Realm?.IsFrozen == true;

        [IgnoreDataMember, XmlIgnore]
#if NET6_0_OR_GREATER
        [System.Text.Json.Serialization.JsonIgnore]
#endif
        public Realm Realm { get; }

        IThreadConfinedHandle IThreadConfined.Handle => Handle.Value;

        public T this[int index]
        {
            get
            {
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return GetValueAtIndex(index);
            }
        }

        internal RealmCollectionBase(Realm realm, Metadata? metadata)
        {
            Realm = realm;
            Handle = new Lazy<CollectionHandleBase>(GetOrCreateHandle);
            Metadata = metadata;
            _isEmbedded = metadata?.Schema.BaseType == ObjectSchema.ObjectType.EmbeddedObject;
            _notificationCallbacks = new(() => new(this));
        }

        ~RealmCollectionBase()
        {
            if (_notificationCallbacks.IsValueCreated)
            {
                _notificationCallbacks.Value.RemoveAll();
            }
        }

        internal abstract CollectionHandleBase GetOrCreateHandle();

        internal abstract RealmCollectionBase<T> CreateCollection(Realm realm, CollectionHandleBase handle);

        internal RealmResults<T> GetFilteredResults(string query, QueryArgument[] arguments)
        {
            var resultsHandle = Handle.Value.GetFilteredResults(query, arguments);
            return new RealmResults<T>(Realm, resultsHandle, Metadata);
        }

        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The returned collection must own its Realm.")]
        public IRealmCollection<T> Freeze()
        {
            if (IsFrozen)
            {
                return this;
            }

            var frozenRealm = Realm.Freeze();
            var frozenHandle = Handle.Value.Freeze(frozenRealm.SharedRealmHandle);
            return CreateCollection(frozenRealm, frozenHandle);
        }

        public IDisposable SubscribeForNotifications(NotificationCallbackDelegate<T> callback, KeyPathsCollection? keyPathsCollection = null)
        {
            keyPathsCollection ??= KeyPathsCollection.Full;

            if (keyPathsCollection.Type == KeyPathsCollectionType.Explicit && !ContainsRealmObjects())
            {
                throw new InvalidOperationException("Key paths can be used only with collections of Realm objects");
            }

            return SubscribeForNotificationsImpl(callback, keyPathsCollection);
        }

        internal IDisposable SubscribeForNotificationsImpl(NotificationCallbackDelegate<T> callback, KeyPathsCollection keyPathsCollection)
        {
            Argument.NotNull(callback, nameof(callback));

            if (keyPathsCollection.Type == KeyPathsCollectionType.Explicit)
            {
                var managedResultsHandle = GCHandle.Alloc(this, GCHandleType.Weak);
                var callbackHandle = GCHandle.Alloc(callback, GCHandleType.Weak);

                var token = Handle.Value.AddNotificationCallback(GCHandle.ToIntPtr(managedResultsHandle), keyPathsCollection,
                    GCHandle.ToIntPtr(callbackHandle));

                return NotificationToken.Create(callback, c => token.Dispose());
            }

            // For notifications with type Default or Shallow we cache the callbacks on the managed level, to avoid creating multiple notifications in core
            _notificationCallbacks.Value.Add(callback, keyPathsCollection);
            return NotificationToken.Create(callback, c => UnsubscribeFromNotifications(c, keyPathsCollection.Type));
        }

        protected virtual bool ContainsRealmObjects()
        {
            return typeof(IRealmObjectBase).IsAssignableFrom(typeof(T));
        }

        protected abstract T GetValueAtIndex(int index);

        protected void AddToRealmIfNecessary(in RealmValue value)
        {
            if (value.Type != RealmValueType.Object)
            {
                return;
            }

            var robj = value.AsIRealmObject();

            if (robj.IsManaged && !robj.Realm.IsSameInstance(Realm))
            {
                throw new RealmException("Can't add to the collection an object that is already in another realm.");
            }

            switch (robj)
            {
                case IRealmObject topLevel:
                    if (!robj.IsManaged)
                    {
                        Realm.Add(topLevel);
                    }

                    break;

                // Embedded and asymmetric objects can't reach this path unless the user explicitly adds
                // them to the collection as RealmValues (e.g. IList<RealmValue>).
                // This is because:
                // * Plain embedded objects (not contained within a RealmValue), beside RealmSet, are
                //   added by each collection handle (e.g. _listHandle.AddEmbedded()) in the respective
                //   method (e.g. in RealmList.Add(), RealmList.Insert(), RealmDictionary.Add(), etc.)
                //   rather than reaching RealmCollectionBase.AddToRealmIfNecessary().
                // * For plain asymmetric objects, the weaver raises a compilation error since asymmetric
                //   objects can't be linked to.
                case IEmbeddedObject:
                    Debug.Assert(typeof(T) == typeof(RealmValue) || typeof(T) == typeof(KeyValuePair<string, RealmValue>), $"Expected a RealmValue to contain the IEmbeddedObject, but was a {typeof(T).Name}");
                    throw new NotSupportedException("A RealmValue cannot contain an embedded object.");
                case IAsymmetricObject:
                    Debug.Assert(typeof(T) == typeof(RealmValue) || typeof(T) == typeof(KeyValuePair<string, RealmValue>), $"Expected a RealmValue to contain the IAsymmetricObject, but was a {typeof(T).Name}");
                    throw new NotSupportedException("A RealmValue cannot contain an asymmetric object.");
                default:
                    throw new NotSupportedException($"{robj.GetType().Name} is not a valid Realm object type.");
            }
        }

        protected static IEmbeddedObject EnsureUnmanagedEmbedded(in RealmValue value)
        {
            var result = value.AsRealmObject<IEmbeddedObject>();
            if (result.IsManaged)
            {
                throw new RealmException("Can't add to the collection an embedded object that is already managed.");
            }

            return result;
        }

        private void UnsubscribeFromNotifications(NotificationCallbackDelegate<T> callback, KeyPathsCollectionType type)
        {
            _notificationCallbacks.Value.Remove(callback, type);
        }

        #region INotifyCollectionChanged

        private void OnChange(IRealmCollection<T> sender, ChangeSet? change)
        {
            if (!sender.IsValid)
            {
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
            else if (change != null)
            {
                if (change.IsCleared)
                {
                    RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    RaisePropertyChanged();
                    return;
                }

                if (change.Moves.Length > 0 &&
                    change.Moves.Length == change.InsertedIndices.Length &&
                    change.Moves.Length == change.DeletedIndices.Length)
                {
                    var ordered = change.Moves.OrderBy(m => m.From);
                    var movedPositions = -1;
                    var isConsecutiveMove = true;
                    foreach (var item in ordered)
                    {
                        if (movedPositions == -1)
                        {
                            movedPositions = item.To - item.From;
                        }
                        else if (item.To - item.From != movedPositions)
                        {
                            isConsecutiveMove = false;
                            break;
                        }
                    }

                    if (isConsecutiveMove)
                    {
                        var initialItem = ordered.First();
                        var movedItems = Enumerable.Range(initialItem.To, change.Moves.Length)
                                                   .Select(i => sender[i])
                                                   .ToList();

                        RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, movedItems, initialItem.To, initialItem.From));
                    }
                    else
                    {
                        RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }

                    RaisePropertyChanged();
                    return;
                }

                // InvalidRealmObject is used to go around a bug in WPF (<see href="https://github.com/realm/realm-dotnet/issues/1903">#1903</see>)
                var raiseRemoved = TryGetConsecutive(change.DeletedIndices, _ => InvalidObject.Instance, out var removedItems, out var removedStartIndex);

                var raiseAdded = TryGetConsecutive(change.InsertedIndices, i => this[i], out var addedItems, out var addedStartIndex);

                var raiseReplaced = TryGetConsecutive(change.NewModifiedIndices, i => this[i], out var replacedItems, out var replacedStartIndex);

                // Only raise specialized notifications if we have exactly one change type to report
                if (raiseAdded + raiseReplaced + raiseRemoved == 1)
                {
                    if (removedItems != null)
                    {
                        RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItems, removedStartIndex));
                        RaisePropertyChanged();
                    }
                    else if (addedItems != null)
                    {
                        RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, addedItems, addedStartIndex));
                        RaisePropertyChanged();
                    }
                    else if (replacedItems != null)
                    {
                        // Until we get a snapshot of the old collection, we won't be able to provide meaningful value for old items.
                        var oldItems = Enumerable.Range(0, replacedItems.Count).Select(_ => InvalidObject.Instance).ToList();
                        RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, replacedItems, oldItems, replacedStartIndex));
                        RaisePropertyChanged();
                    }
                    else
                    {
                        Debug.Assert(false, "This should never happen");
                    }
                }
                else
                {
                    RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    RaisePropertyChanged();
                }
            }
        }

        private void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            _collectionChanged?.Invoke(this, args);
        }

        private void RaisePropertyChanged()
        {
            _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
            _propertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        }

        private static int TryGetConsecutive(int[] indices, Func<int, object?> getter, out IList? items, out int startIndex)
        {
            items = null;

            if (indices.Length > 0)
            {
                startIndex = indices.Min();
                if (indices.Max() - startIndex == indices.Length - 1)
                {
                    items = Enumerable.Range(startIndex, indices.Length)
                                      .Select(getter)
                                      .ToList();

                    return 1;
                }
            }

            startIndex = -1;
            return 0;
        }

        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "This is called when subscribing to events and the dispose token is retained by the collection.")]
        private void UpdateCollectionChangedSubscriptionIfNecessary(bool isSubscribed)
        {
            if (_collectionChanged == null && _propertyChanged == null)
            {
                if (isSubscribed)
                {
                    SubscribeForNotificationsImpl(OnChange, KeyPathsCollection.Shallow);
                }
                else
                {
                    UnsubscribeFromNotifications(OnChange, KeyPathsCollectionType.Shallow);
                }
            }
        }

        #endregion INotifyCollectionChanged

        void INotifiable<CollectionChangeSet>.NotifyCallbacks(CollectionChangeSet? changes, KeyPathsCollectionType type, IntPtr callbackNative)
        {
            ChangeSet? changeset = null;
            if (changes != null)
            {
                var actualChanges = changes.Value;
                changeset = new ChangeSet(
                    insertedIndices: actualChanges.Insertions.ToEnumerable().Select(i => (int)i).ToArray(),
                    modifiedIndices: actualChanges.Modifications.ToEnumerable().Select(i => (int)i).ToArray(),
                    newModifiedIndices: actualChanges.Modifications_New.ToEnumerable().Select(i => (int)i).ToArray(),
                    deletedIndices: actualChanges.Deletions.ToEnumerable().Select(i => (int)i).ToArray(),
                    moves: actualChanges.Moves.ToEnumerable().Select(m => new ChangeSet.Move((int)m.From, (int)m.To)).ToArray(),
                    cleared: actualChanges.Cleared);
            }

            if (type == KeyPathsCollectionType.Explicit
                && GCHandle.FromIntPtr(callbackNative).Target is NotificationCallbackDelegate<T> callback)
            {
                callback(this, changeset);
                return;
            }

            _notificationCallbacks.Value.Notify(changeset, type);
        }

        public IEnumerator<T> GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator(); // using our class generic type, just redirect the legacy get

        #region IList

        public virtual bool IsReadOnly => (Realm?.Config as RealmConfiguration)?.IsReadOnly == true;

        public bool IsFixedSize => false;

        public bool IsSynchronized => false;

        public object SyncRoot => this;

        object? IList.this[int index]
        {
            get => this[index];
            set => throw new NotSupportedException();
        }

        public void Clear() => Handle.Value.Clear();

        public int IndexOf(object? value)
        {
            if (value is not null && value is not T)
            {
                throw new ArgumentException($"value must be of type {typeof(T).FullName}, but got {value.GetType().FullName}", nameof(value));
            }

            return IndexOf((T?)value);
        }

        public bool Contains(object? value)
        {
            if (value is not null && value is not T)
            {
                throw new ArgumentException($"value must be of type {typeof(T).FullName}, but got {value.GetType().FullName}", nameof(value));
            }

            return Contains((T?)value);
        }

        public virtual bool Contains([AllowNull] T value) => IndexOf(value) > -1;

        public abstract int IndexOf([AllowNull] T value);

        public void CopyTo(T[] array, int arrayIndex)
        {
            Argument.NotNull(array, nameof(array));

            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }

            if (arrayIndex + Count > array.Length)
            {
                throw new ArgumentException($"Specified array doesn't have enough capacity to perform the copy. Needed: {arrayIndex + Count}, available: {array.Length}", nameof(array));
            }

            foreach (var obj in this)
            {
                array[arrayIndex++] = obj;
            }
        }

        public virtual int Add(object? value)
        {
            if (value is not T tValue)
            {
                throw new NotSupportedException($"Can't add an item of type {value?.GetType().Name ?? "null"} to a list of {typeof(T).Name}");
            }

            Add(tValue);
            return Count - 1;
        }

        public virtual void Insert(int index, object? value)
        {
            if (value is not T tValue)
            {
                throw new NotSupportedException($"Can't add an item of type {value?.GetType().Name ?? "null"} to a list of {typeof(T).Name}");
            }

            Insert(index, tValue);
        }

        public void Remove(object? value)
        {
            if (value is T tValue)
            {
                Remove(tValue);
            }
        }

        public virtual void RemoveAt(int index) => throw new NotSupportedException();

        public void CopyTo(Array array, int index)
        {
            Argument.NotNull(array, nameof(array));

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (index + Count > array.Length)
            {
                throw new ArgumentException($"Specified array doesn't have enough capacity to perform the copy. Needed: {index + Count}, available: {array.Length}", nameof(array));
            }

            foreach (var obj in this)
            {
                array.SetValue(obj, index++);
            }
        }

        #endregion IList

        // ReSharper disable once MemberCanBePrivate.Global - this needs to be public for the dynamic API
        public class Enumerator : IEnumerator<T>
        {
            private readonly RealmCollectionBase<T> _enumerating;
            private readonly bool _shouldDisposeHandle;
            private int _index;

            internal Enumerator(RealmCollectionBase<T> parent)
            {
                _index = -1;

                // If we didn't snapshot the parent, we should not dispose the results handle, otherwise we'll invalidate the
                // parent collection after iterating it. Only collections of objects support snapshotting and we do not need to
                // snapshot if the collection is frozen.
                _shouldDisposeHandle = parent is { IsValid: true, IsFrozen: false, Handle.Value.CanSnapshot: true, Metadata: not null };
                _enumerating = _shouldDisposeHandle ? new RealmResults<T>(parent.Realm, parent.Handle.Value.Snapshot(), parent.Metadata!) : parent;
            }

            public T Current => _enumerating[_index];

            object? IEnumerator.Current => Current;

            public bool MoveNext()
            {
                var index = _index + 1;
                if (index >= _enumerating.Count)
                {
                    return false;
                }

                _index = index;
                return true;
            }

            public void Reset()
            {
                _index = -1; // by definition BEFORE first item
            }

            public void Dispose()
            {
                if (_shouldDisposeHandle)
                {
                    _enumerating.Handle.Value.Close();
                }
            }
        }
    }

    /// <summary>
    /// IRealmList is only implemented by RealmList and serves to expose the ListHandle without knowing the generic param.
    /// </summary>
    /// <typeparam name="THandle">The type of the handle for the collection.</typeparam>
    internal interface IRealmCollectionBase<out THandle> : IMetadataObject
        where THandle : CollectionHandleBase
    {
        /// <summary>
        /// Gets the native handle for that collection.
        /// </summary>
        THandle NativeHandle { get; }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "NotificationCallbacks are tightly coupled with the collection and it's easier to reason about when they're in the same file.")]
    internal class NotificationCallbacks<T>
    {
        private readonly RealmCollectionBase<T> _parent;

        private readonly Dictionary<KeyPathsCollectionType, (NotificationTokenHandle? Token, bool DeliveredInitialNotification, List<NotificationCallbackDelegate<T>> Callbacks)> _subscriptions = new();

        public NotificationCallbacks(RealmCollectionBase<T> parent)
        {
            _parent = parent;
        }

        public void Add(NotificationCallbackDelegate<T> callback, KeyPathsCollection keyPathsCollection)
        {
            var kpcType = keyPathsCollection.Type;
            if (_subscriptions.TryGetValue(kpcType, out var subscription))
            {
                if (subscription.DeliveredInitialNotification)
                {
                    // If Core already delivered the initial notification, we need to manually invoke the callback as it won't be invoked by Core.
                    // It's part of the SubscribeForNotifications API contract that an initial callback with `null` changes is always delivered.
                    callback(_parent, null);
                }

                // If we have a subscription already, we just add the callback to the list we're managing
                subscription.Callbacks.Add(callback);
            }
            else
            {
                // If this is a new subscription, we store it in the backing dictionary, then we subscribe outside of transaction
                _subscriptions[kpcType] = (null, false, new() { callback });
                _parent.Realm.ExecuteOutsideTransaction(() =>
                {
                    // It's possible that we unsubscribed in the meantime, so only add a notification callback if we still have callbacks
                    if (_subscriptions.TryGetValue(kpcType, out var sub) && sub.Callbacks.Count > 0)
                    {
                        var managedResultsHandle = GCHandle.Alloc(_parent, GCHandleType.Weak);
                        _subscriptions[kpcType] = (_parent.Handle.Value.AddNotificationCallback(GCHandle.ToIntPtr(managedResultsHandle), keyPathsCollection), sub.DeliveredInitialNotification, sub.Callbacks);
                    }
                });
            }
        }

        public bool Remove(NotificationCallbackDelegate<T> callback, KeyPathsCollectionType kpcType)
        {
            if (_subscriptions.TryGetValue(kpcType, out var subscription))
            {
                subscription.Callbacks.Remove(callback);
                if (subscription.Callbacks.Count == 0)
                {
                    subscription.Token?.Dispose();
                    _subscriptions.Remove(kpcType);
                }

                return true;
            }

            return false;
        }

        public void Notify(ChangeSet? changes, KeyPathsCollectionType kpcType)
        {
            if (_subscriptions.TryGetValue(kpcType, out var subscription))
            {
                if (changes == null)
                {
                    _subscriptions[kpcType] = (subscription.Token, true, subscription.Callbacks);
                }

                foreach (var callback in subscription.Callbacks.ToArray())
                {
                    callback(_parent, changes);
                }
            }
        }

        public void RemoveAll()
        {
            foreach (var token in _subscriptions.Values.Select(c => c.Token))
            {
                token?.Dispose();
            }
        }
    }

    /// <summary>
    /// Special invalid object that is used to avoid an exception in WPF
    /// when deleting an element from a collection bound to UI (<see href="https://github.com/realm/realm-dotnet/issues/1903">#1903</see>).
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This is a special object that has a very limited meaning in the project.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class InvalidObject
    {
        private InvalidObject()
        {
        }

        internal static InvalidObject Instance { get; } = new();

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            // This is to resolve the WPF bug
            return true;
        }
    }
}
