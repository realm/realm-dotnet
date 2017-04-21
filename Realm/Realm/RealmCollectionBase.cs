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

namespace Realms
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented")]
    public abstract class RealmCollectionBase<T> 
        : NotificationsHelper.INotifiable, 
          IRealmCollection<T>, 
          INotifyCollectionChanged, 
          INotifyPropertyChanged,
          ISchemaSource,
          IThreadConfined
    {
        private readonly List<NotificationCallbackDelegate<T>> _callbacks = new List<NotificationCallbackDelegate<T>>();
        internal readonly RealmObject.Metadata Metadata;

        private NotificationTokenHandle _notificationToken;

        private event NotifyCollectionChangedEventHandler _collectionChanged;

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
                if (Handle.Value.IsInvalid)
                {
                    return 0;
                }

                return Handle.Value.Count();
            }
        }

        public Schema.ObjectSchema ObjectSchema => Metadata.Schema;

        RealmObject.Metadata IThreadConfined.Metadata => Metadata;

        public bool IsManaged => Realm != null;

        public bool IsValid => Handle.Value.IsValid;

        IThreadConfinedHandle IThreadConfined.Handle => Handle.Value;

        internal readonly Realm Realm;
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

                var objectPtr = Handle.Value.GetObjectAtIndex(index);
                if (objectPtr == IntPtr.Zero)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return (T)(object)Realm.MakeObject(Metadata, objectPtr);
            }
        }

        public IDisposable SubscribeForNotifications(NotificationCallbackDelegate<T> callback)
        {
            if (_callbacks.Count == 0)
            {
                SubscribeForNotifications();
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

            var managedResultsHandle = GCHandle.Alloc(this);
            var token = new NotificationTokenHandle(Handle.Value);
            var tokenHandle = Handle.Value.AddNotificationCallback(GCHandle.ToIntPtr(managedResultsHandle), NotificationsHelper.NotificationCallback);

            token.SetHandle(tokenHandle);

            _notificationToken = token;
        }

        private void UnsubscribeFromNotifications()
        {
            _notificationToken?.Dispose();
            _notificationToken = null;
        }

        #region INotifyCollectionChanged

        private void OnChange(IRealmCollection<T> sender, ChangeSet change, Exception error)
        {
            if (error != null)
            {
                Realm.NotifyError(error);
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

                IList removedItems;
                int removedStartIndex;
                var raiseRemoved = TryGetConsecutive(change.DeletedIndices, _ => default(T), out removedItems, out removedStartIndex);

                IList addedItems;
                int addedStartIndex;
                var raiseAdded = TryGetConsecutive(change.InsertedIndices, i => this[i], out addedItems, out addedStartIndex);

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

        #endregion

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
                    deletedIndices: actualChanges.Deletions.AsEnumerable().Select(i => (int)i).ToArray(),
                    moves: actualChanges.Moves.AsEnumerable().Select(m => new ChangeSet.Move((int)m.From, (int)m.To)).ToArray());
            }

            foreach (var callback in _callbacks.ToArray())
            {
                callback(this, changeset, managedException);
            }
        }

        public IEnumerator<T> GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator(); // using our class generic type, just redirect the legacy get

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

        internal class Enumerator : IEnumerator<T>
        {
            private readonly RealmCollectionBase<T> _enumerating;
            private int _index;

            internal Enumerator(RealmCollectionBase<T> parent)
            {
                _index = -1;
                _enumerating = parent;
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
            }
        }
    }
}