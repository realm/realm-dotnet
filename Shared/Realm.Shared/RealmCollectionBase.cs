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
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Realms
{
    internal abstract class RealmCollectionBase<T> : RealmCollectionNativeHelper.Interface, IRealmCollection<T>, INotifyCollectionChanged
    {
        private readonly List<NotificationCallbackDelegate<T>> _callbacks = new List<NotificationCallbackDelegate<T>>();

        private NotificationTokenHandle _notificationToken;

        private event NotifyCollectionChangedEventHandler _collectionChanged;

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                if (_collectionChanged == null)
                {
                    SubscribeForNotifications(OnChange);
                }

                _collectionChanged += value;
            }

            remove
            {
                _collectionChanged -= value;

                if (_collectionChanged == null)
                {
                    UnsubscribeFromNotifications(OnChange);
                }
            }
        }

        public abstract int Count { get; }

        protected readonly Realm Realm;
        protected readonly Lazy<CollectionHandleBase> Handle;
        protected readonly RealmObject.Metadata TargetMetadata;

        protected RealmCollectionBase(Realm realm, RealmObject.Metadata metadata)
        {
            Realm = realm;
            Handle = new Lazy<CollectionHandleBase>(CreateHandle);
            TargetMetadata = metadata;
        }

        ~RealmCollectionBase()
        {
            UnsubscribeFromNotifications();
        }

        protected abstract CollectionHandleBase CreateHandle();

        public T this[int index]
        {
            get
            {
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException();
                }

                var objectPtr = Handle.Value.GetObjectAtIndex(index);
                return (T)(object)Realm.MakeObject(TargetMetadata, objectPtr);
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
            var tokenHandle = Handle.Value.AddNotificationCallback(GCHandle.ToIntPtr(managedResultsHandle), RealmCollectionNativeHelper.NotificationCallback);

            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                token.SetHandle(tokenHandle);
            }

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
                        return;
                    }

                    if (removedItems != null)
                    {
                        RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItems, removedStartIndex));
                    }

                    if (addedItems != null)
                    {
                        RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, addedItems, addedStartIndex));
                    }
                }
            }
        }

        protected void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            _collectionChanged?.Invoke(this, args);
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

        #endregion

        void RealmCollectionNativeHelper.Interface.NotifyCallbacks(CollectionHandleBase.CollectionChangeSet? changes, NativeException? exception)
        {
            var managedException = exception?.Convert();
            ChangeSet changeset = null;
            if (changes != null)
            {
                var actualChanges = changes.Value;
                changeset = new ChangeSet(
                    insertedIndices: actualChanges.Insertions.AsEnumerable().Select(i => (int)i).ToArray(),
                    modifiedIndices: actualChanges.Modifications.AsEnumerable().Select(i => (int)i).ToArray(),
                    deletedIndices: actualChanges.Deletions.AsEnumerable().Select(i => (int)i).ToArray());
            }

            foreach (var callback in _callbacks)
            {
                callback(this, changeset, managedException);
            }
        }

        public abstract IEnumerator<T> GetEnumerator();

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
    }
}