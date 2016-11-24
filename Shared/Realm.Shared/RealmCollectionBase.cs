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
    public abstract class RealmCollectionBase<T> : RealmCollectionNativeHelper.Interface, INotifyCollectionChanged
    {
        /// <summary>
        /// A callback that will be invoked each time the contents of a <see cref="RealmResults{T}"/> have changed.
        /// </summary>
        /// <param name="sender">The <see cref="RealmResults{T}"/> being monitored for changes.</param>
        /// <param name="changes">The <see cref="ChangeSet"/> describing the changes to a <see cref="RealmResults{T}"/>, or <c>null</c> if an error occured.</param>
        /// <param name="error">An exception that might have occurred while asynchronously monitoring a <see cref="RealmResults{T}"/> for changes, or <c>null</c> if no errors occured.</param>
        public delegate void NotificationCallbackDelegate(RealmCollectionBase<T> sender, ChangeSet changes, Exception error);

        private readonly List<NotificationCallbackDelegate> _callbacks = new List<NotificationCallbackDelegate>();
        private readonly Lazy<CollectionHandleBase> _handle;

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

        internal Realm Realm { get; }

        internal CollectionHandleBase Handle => _handle.Value;

        protected RealmCollectionBase(Realm realm)
        {
            Realm = realm;
            _handle = new Lazy<CollectionHandleBase>(CreateHandle);
        }

        ~RealmCollectionBase()
        {
            UnsubscribeFromNotifications();
        }

        internal abstract CollectionHandleBase CreateHandle();

        protected abstract T ItemAtIndex(int index);

        /// <summary>
        /// Register a callback to be invoked each time this <see cref="RealmResults{T}"/> changes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The callback will be asynchronously invoked with the initial <see cref="RealmResults{T}" />, and then called again after each write transaction
        /// which changes either any of the objects in the collection, or which objects are in the collection.
        /// The <c>changes</c> parameter will be <c>null</c> the first time the callback is invoked with the initial results.
        /// For each call after that, it will contain information about which rows in the results were added, removed or modified.
        /// </para>
        /// <para>
        /// If a write transaction did not modify any objects in this <see cref="RealmResults{T}" />, the callback is not invoked at all.
        /// If an error occurs the callback will be invoked with <c>null</c> for the <c>sender</c> parameter and a non-<c>null</c> <c>error</c>.
        /// Currently the only errors that can occur are when opening the <see cref="Realms.Realm" /> on the background worker thread.
        /// </para>
        /// <para>
        /// At the time when the block is called, the <see cref="RealmResults{T}" /> object will be fully evaluated and up-to-date, and as long as you do not perform a write transaction on the same thread
        /// or explicitly call <see cref="Realm.Refresh" />, accessing it will never perform blocking work.
        /// </para>
        /// <para>
        /// Notifications are delivered via the standard event loop, and so can't be delivered while the event loop is blocked by other activity.
        /// When notifications can't be delivered instantly, multiple notifications may be coalesced into a single notification.
        /// This can include the notification with the initial collection.
        /// </para>
        /// </remarks>
        /// <param name="callback">The callback to be invoked with the updated <see cref="RealmResults{T}" />.</param>
        /// <returns>
        /// A subscription token. It must be kept alive for as long as you want to receive change notifications.
        /// To stop receiving notifications, call <see cref="IDisposable.Dispose" />.
        /// </returns>
        public IDisposable SubscribeForNotifications(NotificationCallbackDelegate callback)
        {
            if (_callbacks.Count == 0)
            {
                SubscribeForNotifications();
            }

            _callbacks.Add(callback);

            return new NotificationToken(this, callback);
        }

        private void UnsubscribeFromNotifications(NotificationCallbackDelegate callback)
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
            var token = new NotificationTokenHandle(Handle);
            var tokenHandle = Handle.AddNotificationCallback(GCHandle.ToIntPtr(managedResultsHandle), RealmCollectionNativeHelper.NotificationCallback);

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
            Debug.Assert(_notificationToken != null, "_notificationToken must not be null to unsubscribe.");

            _notificationToken?.Dispose();
            _notificationToken = null;
        }

        #region INotifyCollectionChanged

        private void OnChange(RealmCollectionBase<T> sender, ChangeSet change, Exception error)
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
                var raiseAdded = TryGetConsecutive(change.InsertedIndices, ItemAtIndex, out addedItems, out addedStartIndex);

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

        /// <summary>
        /// A <see cref="ChangeSet" /> describes the changes inside a <see cref="RealmResults{T}" /> since the last time the notification callback was invoked.
        /// </summary>
        public class ChangeSet
        {
            /// <summary>
            /// The indices in the new version of the <see cref="RealmResults{T}" /> which were newly inserted.
            /// </summary>
            public readonly int[] InsertedIndices;

            /// <summary>
            /// The indices in the new version of the <see cref="RealmResults{T}"/> which were modified. This means that the property of an object at that index was modified
            /// or the property of another object it's related to.
            /// </summary>
            public readonly int[] ModifiedIndices;

            /// <summary>
            /// The indices of objects in the previous version of the <see cref="RealmResults{T}"/> which have been removed from this one.
            /// </summary>
            public readonly int[] DeletedIndices;

            internal ChangeSet(int[] insertedIndices, int[] modifiedIndices, int[] deletedIndices)
            {
                InsertedIndices = insertedIndices;
                ModifiedIndices = modifiedIndices;
                DeletedIndices = deletedIndices;
            }
        }

        private class NotificationToken : IDisposable
        {
            private RealmCollectionBase<T> _collection;
            private NotificationCallbackDelegate _callback;

            internal NotificationToken(RealmCollectionBase<T> collection, NotificationCallbackDelegate callback)
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