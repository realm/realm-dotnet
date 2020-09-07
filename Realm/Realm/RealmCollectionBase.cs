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
using Realms.Helpers;
using Realms.Schema;

namespace Realms
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "This should not be directly accessed by users.")]
    public abstract class RealmCollectionBase<T>
        : NotificationsHelper.INotifiable,
          IRealmCollection<T>,
          ISchemaSource,
          IThreadConfined
    {
        protected static readonly PropertyType _argumentType = typeof(T).ToPropertyType(out _);

        private readonly List<NotificationCallbackDelegate<T>> _callbacks = new List<NotificationCallbackDelegate<T>>();

        internal readonly RealmObject.Metadata Metadata;

        private NotificationTokenHandle _notificationToken;
        private bool _deliveredInitialNotification;

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

        public ObjectSchema ObjectSchema => Metadata?.Schema;

        RealmObject.Metadata IThreadConfined.Metadata => Metadata;

        public bool IsManaged => Realm != null;

        public bool IsValid => Handle.Value.IsValid;

        public bool IsFrozen => Handle.Value.IsFrozen;

        public Realm Realm { get; }

        public abstract IRealmCollection<T> Freeze();

        IThreadConfinedHandle IThreadConfined.Handle => Handle.Value;

        internal readonly Lazy<CollectionHandleBase> Handle;

        internal RealmCollectionBase(Realm realm, RealmObject.Metadata metadata)
        {
            Realm = realm;
            Handle = new Lazy<CollectionHandleBase>(CreateHandle);
            Metadata = metadata;
        }

        ~RealmCollectionBase()
        {
            UnsubscribeFromNotifications();
        }

        internal abstract CollectionHandleBase CreateHandle();

        public T this[int index]
        {
            get
            {
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                switch (_argumentType)
                {
                    case PropertyType.Object | PropertyType.Nullable:
                        if (!Handle.Value.TryGetObjectAtIndex(index, out var objectHandle))
                        {
                            throw new ArgumentOutOfRangeException(nameof(index));
                        }

                        return Operator.Convert<RealmObject, T>(Realm.MakeObject(Metadata, objectHandle));

                    case PropertyType.String:
                    case PropertyType.String | PropertyType.Nullable:
                        return Operator.Convert<string, T>(Handle.Value.GetStringAtIndex(index));

                    case PropertyType.Data:
                    case PropertyType.Data | PropertyType.Nullable:
                        return Operator.Convert<byte[], T>(Handle.Value.GetByteArrayAtIndex(index));

                    default:
                        return Handle.Value.GetPrimitiveAtIndex(index, _argumentType).Get<T>();
                }
            }
        }

        public RealmCollectionBase<T> Snapshot()
        {
            var handle = Handle.Value.Snapshot();
            return new RealmResults<T>(Realm, Metadata, handle);
        }

        internal RealmResults<T> GetFilteredResults(string query)
        {
            var handle = Handle.Value.GetFilteredResults(query);
            return new RealmResults<T>(Realm, Metadata, handle);
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

            return new NotificationToken(this, callback);
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
                var managedResultsHandle = GCHandle.Alloc(this);
                _notificationToken = Handle.Value.AddNotificationCallback(GCHandle.ToIntPtr(managedResultsHandle), NotificationsHelper.NotificationCallback);
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

                var raiseRemoved = TryGetConsecutive(change.DeletedIndices, _ => default, out var removedItems, out var removedStartIndex);

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

        private static bool TryGetConsecutive(int[] indices, Func<int, T> getter, out IList items, out int startIndex)
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

        void NotificationsHelper.INotifiable.NotifyCallbacks(NotifiableObjectHandleBase.CollectionChangeSet? changes, NativeException? exception)
        {
            var managedException = exception?.Convert();
            ChangeSet changeset = null;
            if (changes != null)
            {
                var actualChanges = changes.Value;
                changeset = new ChangeSet(
                    insertedIndices: actualChanges.Insertions.AsEnumerable().Select(i => (int)i).ToArray(),
                    modifiedIndices: actualChanges.Modifications.AsEnumerable().Select(i => (int)i).ToArray(),
                    newModifiedIndices: actualChanges.Modifications_New.AsEnumerable().Select(i => (int)i).ToArray(),
                    deletedIndices: actualChanges.Deletions.AsEnumerable().Select(i => (int)i).ToArray(),
                    moves: actualChanges.Moves.AsEnumerable().Select(m => new ChangeSet.Move((int)m.From, (int)m.To)).ToArray());
            }
            else
            {
                _deliveredInitialNotification = true;
            }

            foreach (var callback in _callbacks.ToArray())
            {
                callback(this, changeset, managedException);
            }
        }

        public IEnumerator<T> GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator(); // using our class generic type, just redirect the legacy get

        #region IList

        public bool IsFixedSize => false;

        public virtual bool IsReadOnly => true;

        public bool IsSynchronized => false;

        public object SyncRoot => null;

        public virtual int Add(object value) => throw new NotSupportedException();

        public virtual void Clear() => throw new NotSupportedException();

        public bool Contains(object value) => IndexOf(value) > -1;

        public int IndexOf(object value)
        {
            if (value != null && !(value is T))
            {
                throw new ArgumentException($"value must be of type {typeof(T).FullName}, but got {value?.GetType().FullName}", nameof(value));
            }

            return IndexOf((T)value);
        }

        public abstract int IndexOf(T value);

        public virtual void Insert(int index, object value) => throw new NotSupportedException();

        public virtual void Remove(object value) => throw new NotSupportedException();

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

            var list = (IList)array;
            foreach (var obj in this)
            {
                list[index++] = obj;
            }
        }

        #endregion IList

        private class NotificationToken : IDisposable
        {
            private RealmCollectionBase<T> _collection;
            private NotificationCallbackDelegate<T> _callback;

            internal NotificationToken(RealmCollectionBase<T> collection, NotificationCallbackDelegate<T> callback)
            {
                _collection = collection;
                _callback = callback;
            }

            public void Dispose()
            {
                _collection.UnsubscribeFromNotifications(_callback);
                _callback = null;
                _collection = null;
            }
        }

        public class Enumerator : IEnumerator<T>
        {
            private readonly RealmCollectionBase<T> _enumerating;
            private int _index;

            internal Enumerator(RealmCollectionBase<T> parent)
            {
                _index = -1;
                _enumerating = parent.Snapshot();
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
                _enumerating.Handle.Value.Close();
            }
        }
    }
}
