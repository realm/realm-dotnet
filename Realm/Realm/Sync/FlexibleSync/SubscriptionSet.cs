////////////////////////////////////////////////////////////////////////////
//
// Copyright 2021 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Realms.Helpers;
using Realms.Sync.Exceptions;

namespace Realms.Sync
{
    /// <summary>
    /// A collection representing the set of active subscriptions for a <see cref="Realm"/>
    /// instance. This is used in combination with <see cref="FlexibleSyncConfiguration"/> to
    /// declare the set of queries you want to synchronize with the server. You can access and
    /// read the subscription set freely, but mutating it must happen in an <see cref="Update"/>
    /// block.
    /// </summary>
    /// <remarks>
    /// Any changes to the subscription set will be persisted locally and be available the next
    /// time the application starts up - i.e. it's not necessary to subscribe for the same query
    /// every time. Updating the subscription set can be done while offline, and only the latest
    /// update will be sent to the server whenever connectivity is restored.
    /// <br/>
    /// It is strongly recommended that you batch updates as much as possible and request the
    /// dataset your application needs upfront. Updating the set of active subscriptions for a
    /// Realm is an expensive operation serverside, even if there's very little data that needs
    /// downloading.
    /// </remarks>
    public class SubscriptionSet : IReadOnlyList<Subscription>
    {
        private SubscriptionSetHandle _handle;

        private bool IsClosed => _handle.IsClosed;

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        /// <value>The number of elements in the collection.</value>
        public int Count => _handle.GetCount();

        /// <summary>
        /// Gets the state of the subscription set.
        /// </summary>
        /// <value>The subscription set's state.</value>
        public SubscriptionSetState State => _handle.GetState();

        internal long Version => _handle.GetVersion();

        /// <summary>
        /// Gets the error associated with this subscription set, if any. This will
        /// only be non-null if <see cref="State"/> is <see cref="SubscriptionSetState.Error"/>.
        /// </summary>
        /// <value>
        /// The <see cref="Exception"/> that provides more details for why the subscription set
        /// was rejected by the server.
        /// </value>
        public Exception? Error
        {
            get
            {
                var errorMessage = _handle.GetErrorMessage();
                return errorMessage == null ? null : new SubscriptionException(errorMessage);
            }
        }

        /// <summary>
        /// Gets the <see cref="Subscription"/> at the specified index in the set.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns>The <see cref="Subscription"/> at the specified index in the set.</returns>
        public Subscription this[int index]
        {
            get
            {
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return _handle.GetAtIndex(index);
            }
        }

        internal SubscriptionSet(SubscriptionSetHandle handle)
        {
            _handle = handle;
        }

        /// <summary>
        /// Finds a subscription by name.
        /// </summary>
        /// <param name="name">The name of the subscription.</param>
        /// <returns>
        /// A <see cref="Subscription"/> instance where <see cref="Subscription.Name"/> is equal to
        /// <paramref name="name"/> if the subscription set contains a subscription with the provided name;
        /// <c>null</c> otherwise.
        /// </returns>
        public Subscription Find(string name) => _handle.Find(name);

        /// <summary>
        /// Finds a subscription by query.
        /// </summary>
        /// <typeparam name="T">The type of objects in the query.</typeparam>
        /// <param name="query">The query describing the subscription.</param>
        /// <returns>
        /// A <see cref="Subscription"/> instance where <see cref="Subscription.Query"/> matches
        /// the provided <paramref name="query"/>; <c>null</c> if the subscription set doesn't contain
        /// a match.
        /// </returns>
        public Subscription Find<T>(IQueryable<T> query)
            where T : IRealmObject
        {
            var results = Argument.EnsureType<RealmResults<T>>(query, $"{nameof(query)} must be a query obtained by calling Realm.All.", nameof(query));
            return _handle.Find(results.ResultsHandle);
        }

        /// <summary>
        /// Update the subscription set and send the request to the server in the background.
        /// </summary>
        /// <remarks>
        /// Calling <see cref="Update"/> is a prerequisite for
        /// mutating the subscription set - e.g. by calling <see cref="Add{T}(IQueryable{T}, SubscriptionOptions)"/>,
        /// <see cref="Remove(Subscription)"/>, or <see cref="RemoveAll(bool)"/>.
        /// <br/>
        /// Calling this may update the content of this <see cref="SubscriptionSet"/> - e.g. if another
        /// <see cref="Update"/> was called on a background thread or if the <see cref="State"/> changed.
        /// <br/>
        /// If you want to wait for the server to acknowledge and send back the data that matches the updated
        /// subscriptions, use <see cref="WaitForSynchronizationAsync"/>.
        /// </remarks>
        /// <param name="action">
        /// Action to execute, adding or removing subscriptions to this set.
        /// </param>
        public void Update(Action action)
        {
            EnsureReadonly();
            Argument.NotNull(action, nameof(action));

            var oldHandle = _handle;
            var writeableHandle = _handle.BeginWrite();

            // We need to set the writable handle as the current subscription set handle
            // to allow performing operations that modify it.
            _handle = writeableHandle;

            try
            {
                action();

                // Committing the write will generate a new readonly subscription set handle that
                // we need to set to _handle.
                _handle = _handle.CommitWrite();

                oldHandle.Dispose();
                writeableHandle.Dispose();
            }
            catch
            {
                // We need to immediately unbind the subscription set handle to rollback the transaction. If we
                // don't do it, it will be unbound when the Realm closes or GC collects it which is too late.
                // Using .Dispose here instead of .Unbind will only schedule it for unbinding which is not what
                // we want.
                writeableHandle.Unbind();

                // If an error occurs - revert to the old subscription set handle.
                _handle = oldHandle;
                throw;
            }
        }

        /// <summary>
        /// Adds a query to the set of active subscriptions. The query will be joined via an OR statement
        /// with any existing queries for the same type.
        /// </summary>
        /// <typeparam name="T">The type of objects in the query results.</typeparam>
        /// <param name="query">The query that will be matched on the server.</param>
        /// <param name="options">
        /// The subscription options controlling the name and/or the type of insert that will be performed.
        /// </param>
        /// <remarks>
        /// Adding a query that already exists is a no-op and the existing subscription will be returned.
        /// </remarks>
        /// <returns>The subscription that represents the specified query.</returns>
        public Subscription Add<T>(IQueryable<T> query, SubscriptionOptions? options = null)
            where T : IRealmObject
        {
            EnsureWritable();

            var results = Argument.EnsureType<RealmResults<T>>(query, $"{nameof(query)} must be a query obtained by calling Realm.All.", nameof(query));
            return _handle.Add(results.ResultsHandle, options ?? new());
        }

        /// <summary>
        /// Removes a subscription with the specified <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the subscription to remove.</param>
        /// <returns>
        /// <c>true</c> if the subscription existed in this subscription set and was removed; <c>false</c> otherwise.
        /// </returns>
        public bool Remove(string name)
        {
            EnsureWritable();

            Argument.NotNullOrEmpty(name, nameof(name));
            return _handle.Remove(name);
        }

        /// <summary>
        /// Removes a subscription with the specified <paramref name="query"/>.
        /// </summary>
        /// <typeparam name="T">The type of objects in the query results.</typeparam>
        /// <param name="query">The query whose matching subscription should be removed.</param>
        /// <param name="removeNamed">A flag indicating whether to also remove named subscriptions. Default is false.</param>
        /// <returns>
        /// <c>true</c> if the subscription existed in this subscription set and was removed; <c>false</c> otherwise.
        /// </returns>
        public int Remove<T>(IQueryable<T> query, bool removeNamed = false)
            where T : IRealmObject
        {
            EnsureWritable();

            var results = Argument.EnsureType<RealmResults<T>>(query, $"{nameof(query)} must be a query obtained by calling Realm.All.", nameof(query));
            return _handle.Remove(results.ResultsHandle, removeNamed);
        }

        /// <summary>
        /// Removes the provided <paramref name="subscription"/> from this subscription set.
        /// </summary>
        /// <param name="subscription">The subscription to remove.</param>
        /// <returns>
        /// <c>true</c> if the subscription existed in this subscription set and was removed; <c>false</c> otherwise.
        /// </returns>
        public bool Remove(Subscription subscription)
        {
            EnsureWritable();

            Argument.NotNull(subscription, nameof(subscription));

            return _handle.Remove(subscription.Id);
        }

        /// <summary>
        /// Removes all subscriptions for a specified type.
        /// </summary>
        /// <typeparam name="T">The type of objects whose subscriptions should be removed.</typeparam>
        /// <param name="removeNamed">A flag indicating whether to also remove named subscriptions. Default is false.</param>
        /// <returns>The number of subscriptions that existed for this type and were removed.</returns>
        public int RemoveAll<T>(bool removeNamed = false)
            where T : IRealmObject => RemoveAll(typeof(T).GetMappedOrOriginalName(), removeNamed);

        /// <summary>
        /// Removes all subscriptions for the provided <paramref name="className"/>.
        /// </summary>
        /// <param name="className">The name of the type whose subscriptions are to be removed.</param>
        /// <param name="removeNamed">A flag indicating whether to also remove named subscriptions. Default is false.</param>
        /// <returns>The number of subscriptions that existed for this type and were removed.</returns>
        public int RemoveAll(string className, bool removeNamed = false)
        {
            EnsureWritable();

            Argument.NotNullOrEmpty(className, nameof(className));
            return _handle.RemoveAll(className, removeNamed);
        }

        /// <summary>
        /// Removes all subscriptions from this subscription set.
        /// </summary>
        /// <param name="removeNamed">A flag indicating whether to also remove named subscriptions. Default is false.</param>
        /// <returns>The number of subscriptions that existed in the set and were removed.</returns>
        public int RemoveAll(bool removeNamed = false)
        {
            EnsureWritable();

            return _handle.RemoveAll(removeNamed);
        }

        /// <summary>
        /// Waits for the server to acknowledge the subscription set and return the matching objects.
        /// </summary>
        /// <remarks>
        /// If the <see cref="State"/> of the subscription set is <see cref="SubscriptionSetState.Complete"/>
        /// the returned <see cref="Task"/> will complete immediately. If the <see cref="State"/> is
        /// <see cref="SubscriptionSetState.Error"/>, the returned task will be immediately rejected with an
        /// error.
        /// <br/>
        /// If the change results in removing objects from the Realm - e.g. because subscriptions have been
        /// removed, then those objects will have been removed prior to the returned task completing.
        /// </remarks>
        /// <returns>
        /// An awaitable task, whose successful completion indicates that the server has processed the
        /// subscription change and has sent all the data that matches the new subscriptions.
        /// </returns>
        public Task WaitForSynchronizationAsync() => _handle.WaitForStateChangeAsync();

        /// <inheritdoc/>
        public IEnumerator<Subscription> GetEnumerator() => new Enumerator(this);

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private void EnsureWritable()
        {
            if (_handle.IsReadonly)
            {
                throw new InvalidOperationException("You can't mutate the subscription set outside of a Update or UpdateAsync callback.");
            }
        }

        private void EnsureReadonly()
        {
            if (!_handle.IsReadonly)
            {
                throw new InvalidOperationException("You can't Update/UpdateAsync on a subscription set that is already being updated.");
            }
        }

        private class Enumerator : IEnumerator<Subscription>
        {
            private SubscriptionSet? _enumerating;
            private int _index;

            internal Enumerator(SubscriptionSet parent)
            {
                _index = -1;
                _enumerating = parent;
            }

            public Subscription Current
            {
                get
                {
                    ThrowIfDisposed();

                    return _enumerating[_index];
                }
            }

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                ThrowIfDisposed();

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
                ThrowIfDisposed();

                _enumerating = null;
            }

            [MemberNotNull(nameof(_enumerating))]
            private void ThrowIfDisposed()
            {
                if (_enumerating is null)
                {
                    throw new ObjectDisposedException(nameof(Enumerator));
                }
            }
        }
    }
}
