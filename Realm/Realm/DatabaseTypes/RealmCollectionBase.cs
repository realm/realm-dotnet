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

namespace Realms
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "This should not be directly accessed by users.")]
    public abstract class RealmCollectionBase<T>
        : INotifiable<NotifiableObjectHandleBase.CollectionChangeSet>,
          IRealmCollection<T>,
          IThreadConfined,
          IMetadataObject
    {
        private readonly List<NotificationCallbackDelegate<T>> _callbacks = new();

        private NotificationTokenHandle _notificationToken;

        private bool _deliveredInitialNotification;

        internal readonly bool _isEmbedded;

        internal readonly Lazy<CollectionHandleBase> Handle;

        internal readonly Metadata Metadata;

        internal bool IsDynamic;

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "This is the private event - the public is uppercased.")]
        private event NotifyCollectionChangedEventHandler _collectionChanged;

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "This is the private event - the public is uppercased.")]
        private event PropertyChangedEventHandler _propertyChanged;

        public event NotifyCollectionChangedEventHandler CollectionChanged
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

        public event PropertyChangedEventHandler PropertyChanged
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

        [IgnoreDataMember]
        public int Count
        {
            get
            {
                if (!IsValid)
                {
                    return 0;
                }

                return Handle.Value.Count();
            }
        }

        [IgnoreDataMember, XmlIgnore] // XmlIgnore seems to be needed here as IgnoreDataMember is not sufficient for XmlSerializer.
        public ObjectSchema ObjectSchema => Metadata?.Schema;

        Metadata IMetadataObject.Metadata => Metadata;

        [IgnoreDataMember]
        public bool IsManaged => Realm != null;

        [IgnoreDataMember]
        public bool IsValid => Handle.Value.IsValid;

        [IgnoreDataMember]
        public bool IsFrozen => Realm?.IsFrozen == true;

        [IgnoreDataMember]
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

        internal RealmCollectionBase(Realm realm, Metadata metadata)
        {
            Realm = realm;
            Handle = new Lazy<CollectionHandleBase>(GetOrCreateHandle);
            Metadata = metadata;
            _isEmbedded = metadata?.Schema.BaseType == ObjectSchema.ObjectType.EmbeddedObject;
        }

        ~RealmCollectionBase()
        {
            UnsubscribeFromNotifications();
        }

        internal abstract CollectionHandleBase GetOrCreateHandle();

        internal abstract RealmCollectionBase<T> CreateCollection(Realm realm, CollectionHandleBase handle);

        internal RealmResults<T> GetFilteredResults(string query, RealmValue[] arguments)
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

        public IDisposable SubscribeForNotifications(NotificationCallbackDelegate<T> callback)
        {
            Argument.NotNull(callback, nameof(callback));

            if (_callbacks.Count == 0)
            {
                SubscribeForNotifications();
            }
            else if (_deliveredInitialNotification)
            {
                callback(this, null, null);
            }

            _callbacks.Add(callback);

            return NotificationToken.Create(callback, UnsubscribeFromNotifications);
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
                // * Plain embedded objects, beside RealmSet, are added by each collection handle
                //   (e.g. _listHandle.AddEmbedded()) in the respective method (e.g. in RealmList.Add(),
                //   RealmList.Insert(), RealmDictionary.Add(), etc.) rather than reaching
                //   RealmCollectionBase.AddToRealmIfNecessary().
                // * For plain asymmetric objects, the weaver raises a compilation error since asymmetric
                //   objects can't be linked to.
                case IEmbeddedObject:
                    Debug.Assert(typeof(T) == typeof(RealmValue), $"Expected a RealmValue to contain the IEmbeddedObject, but was a {typeof(T).Name}");
                    throw new NotSupportedException("A RealmValue cannot contain an embedded object.");
                case IAsymmetricObject:
                    Debug.Assert(typeof(T) == typeof(RealmValue), $"Expected a RealmValue to contain the IAsymmetricObject, but was a {typeof(T).Name}");
                    throw new NotSupportedException("A RealmValue cannot contain an asymmetric object.");
                default:
                    throw new NotSupportedException($"{robj.GetType().Name} is not a valid Realm object type.");
            }
        }

        protected static IEmbeddedObject EnsureUnmanagedEmbedded(in RealmValue value)
        {
            var result = value.AsRealmObject<IEmbeddedObject>();
            if (result?.IsManaged == true)
            {
                throw new RealmException("Can't add to the collection an embedded object that is already managed.");
            }

            return result;
        }

        private void UnsubscribeFromNotifications(NotificationCallbackDelegate<T> callback)
        {
            if (_callbacks.Remove(callback) &&
                _callbacks.Count == 0)
            {
                UnsubscribeFromNotifications();
            }
        }

        private void SubscribeForNotifications()
        {
            Debug.Assert(_notificationToken == null, "_notificationToken must be null before subscribing.");

            Realm.ExecuteOutsideTransaction(() =>
            {
                var managedResultsHandle = GCHandle.Alloc(this, GCHandleType.Weak);
                _notificationToken = Handle.Value.AddNotificationCallback(GCHandle.ToIntPtr(managedResultsHandle));
            });
        }

        private void UnsubscribeFromNotifications()
        {
            _notificationToken?.Dispose();
            _notificationToken = null;
            _deliveredInitialNotification = false;
        }

        #region INotifyCollectionChanged

        private void OnChange(IRealmCollection<T> sender, ChangeSet change, Exception error)
        {
            if (error != null)
            {
                Realm.NotifyError(error);
            }
            else if (!sender.IsValid)
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

                if (raiseAdded || raiseRemoved)
                {
                    if ((raiseAdded && raiseRemoved) ||
                        (raiseAdded && addedItems == null) ||
                        (raiseRemoved && removedItems == null))
                    {
                        RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                        RaisePropertyChanged();
                        return;
                    }

                    if (removedItems != null)
                    {
                        RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItems, removedStartIndex));
                        RaisePropertyChanged();
                    }

                    if (addedItems != null)
                    {
                        RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, addedItems, addedStartIndex));
                        RaisePropertyChanged();
                    }
                }
            }
        }

        protected void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            _collectionChanged?.Invoke(this, args);
        }

        protected void RaisePropertyChanged()
        {
            _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
            _propertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        }

        private static bool TryGetConsecutive(int[] indices, Func<int, object> getter, out IList items, out int startIndex)
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
                }

                return true;
            }

            startIndex = -1;
            return false;
        }

        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "This is called when subscribing to events and the dispose token is retained by the collection.")]
        private void UpdateCollectionChangedSubscriptionIfNecessary(bool isSubscribed)
        {
            if (_collectionChanged == null && _propertyChanged == null)
            {
                if (isSubscribed)
                {
                    SubscribeForNotifications(OnChange);
                }
                else
                {
                    UnsubscribeFromNotifications(OnChange);
                }
            }
        }

        #endregion INotifyCollectionChanged

        void INotifiable<NotifiableObjectHandleBase.CollectionChangeSet>.NotifyCallbacks(NotifiableObjectHandleBase.CollectionChangeSet? changes)
        {
            ChangeSet changeset = null;
            if (changes != null)
            {
                var actualChanges = changes.Value;
                changeset = new ChangeSet(
                    insertedIndices: actualChanges.Insertions.AsEnumerable().Select(i => (int)i).ToArray(),
                    modifiedIndices: actualChanges.Modifications.AsEnumerable().Select(i => (int)i).ToArray(),
                    newModifiedIndices: actualChanges.Modifications_New.AsEnumerable().Select(i => (int)i).ToArray(),
                    deletedIndices: actualChanges.Deletions.AsEnumerable().Select(i => (int)i).ToArray(),
                    moves: actualChanges.Moves.AsEnumerable().Select(m => new ChangeSet.Move((int)m.From, (int)m.To)).ToArray(),
                    cleared: actualChanges.Cleared);
            }
            else
            {
                _deliveredInitialNotification = true;
            }

            foreach (var callback in _callbacks.ToArray())
            {
                callback(this, changeset, null);
            }
        }

        public IEnumerator<T> GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator(); // using our class generic type, just redirect the legacy get

        #region IList

        public virtual bool IsReadOnly => (Realm?.Config as RealmConfiguration)?.IsReadOnly == true;

        public void Clear() => Handle.Value.Clear();

        public int IndexOf(object value)
        {
            if (value != null && !(value is T))
            {
                throw new ArgumentException($"value must be of type {typeof(T).FullName}, but got {value?.GetType().FullName}", nameof(value));
            }

            return IndexOf((T)value);
        }

        public bool Contains(object value)
        {
            if (value != null && !(value is T))
            {
                throw new ArgumentException($"value must be of type {typeof(T).FullName}, but got {value?.GetType().FullName}", nameof(value));
            }

            return Contains((T)value);
        }

        public virtual bool Contains(T value) => IndexOf(value) > -1;

        public abstract int IndexOf(T value);

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

        #endregion IList

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
                _shouldDisposeHandle = parent.IsValid && !parent.IsFrozen && parent.Handle.Value.CanSnapshot && parent.Metadata != null;
                _enumerating = _shouldDisposeHandle ? new RealmResults<T>(parent.Realm, parent.Handle.Value.Snapshot(), parent.Metadata) : parent;
            }

            public T Current => _enumerating[_index];

            object IEnumerator.Current => Current;

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
    internal interface IRealmCollectionBase<THandle> : IMetadataObject
        where THandle : CollectionHandleBase
    {
        /// <summary>
        /// Gets the native handle for that collection.
        /// </summary>
        THandle NativeHandle { get; }
    }

    /// <summary>
    /// Special invalid object that is used to avoid an exception in WPF
    /// when deleting an element from a collection bound to UI (<see href="https://github.com/realm/realm-dotnet/issues/1903">#1903</see>).
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This is a special object that has a very limited meaning in the project.")]
    internal sealed class InvalidObject
    {
        private InvalidObject()
        {
        }

        public static InvalidObject Instance { get; } = new InvalidObject();

        // The method is overriden to avoid the bug in WPF
        public override bool Equals(object obj)
        {
            return true;
        }
    }
}
