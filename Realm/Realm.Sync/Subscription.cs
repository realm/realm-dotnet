////////////////////////////////////////////////////////////////////////////
//
// Copyright 2018 Realm Inc.
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
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Realms.Exceptions;
using Realms.Native;

namespace Realms.Sync
{
    /// <summary>
    /// A class that represents a subscription to a set of objects in a synced Realm.
    /// <para/>
    /// When partial sync is enabled for a synced Realm, the only objects that the server synchronizes to the
    /// client are those that match a sync subscription registered by that client. A subscription consists of
    /// of a query (represented by an <c>IQueryable{T}</c>) and an optional name.
    /// <para/>
    /// The state of the subscription can be observed by subscribing to the <see cref="PropertyChanged"/> event handler.
    /// <para/>
    /// Subscriptions are created by calling <see cref="CollectionPartialSyncExtensions.SubscribeToObjects"/>.
    /// </summary>
    /// <typeparam name="T">The type of the objects that make up the subscription query.</typeparam>
    public class Subscription<T> : INotifyPropertyChanged
    {
        private static readonly SubscriptionHandle.SubscriptionCallbackDelegate SubscriptionCallback = SubscriptionCallbackImpl;

        private readonly SubscriptionHandle _handle;
        private readonly TaskCompletionSource<object> _syncTcs = new TaskCompletionSource<object>();

        private SubscriptionTokenHandle _subscriptionToken;

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets a value indicating the state of this subscription.
        /// </summary>
        /// <value>The state of the subscription.</value>
        public SubscriptionState State { get; private set; }

        /// <summary>
        /// Gets a value indicating what error (if any) has occurred while processing the subscription.
        /// If the <see cref="State"/> is not <see cref="SubscriptionState.Error"/>, this will be <c>null</c>.
        /// </summary>
        /// <value>An instance of <see cref="Exception"/> if an error has occurred; <c>null</c> otherwise.</value>
        public Exception Error { get; private set; }

        /// <summary>
        /// Gets the query that this subscription is associated with. Regardless of the state of the subscription,
        /// this value will reflect the results in the local Realm. This allows you to databind to this property
        /// immediately and show the last synchronized data. If the <see cref="State"/> is <see cref="SubscriptionState.Invalidated"/>,
        /// the values returned will not be an adequate representation of the state of the remote Realm.
        /// </summary>
        /// <value>
        /// A queryable collection that can be further filtered, ordered, or observed for changes.
        /// </value>
        public IQueryable<T> Results { get; }

        internal Subscription(SubscriptionHandle handle, RealmResults<T> query)
        {
            Results = query;

            _handle = handle;
            State = _handle.GetState();

            var managedSubscriptionHandle = GCHandle.Alloc(this, GCHandleType.Weak);
            _subscriptionToken = _handle.AddNotificationCallback(GCHandle.ToIntPtr(managedSubscriptionHandle), SubscriptionCallback);
        }

        /// <summary>
        /// Waits for the subscription to complete synchronizing (equivalent to transitioning to the
        /// <see cref="SubscriptionState.Complete"/> state.
        /// </summary>
        /// <returns>
        /// An awaitable task, that, upon completion, indicates that the objects matching the specified query
        /// have been synchronized to the local Realm.
        /// </returns>
        public Task WaitForSynchronizationAsync()
        {
            return _syncTcs.Task;
        }

        internal async Task UnsubscribeAsync()
        {
            _handle.Unsubscribe();

            var tcs = new TaskCompletionSource<object>();
            PropertyChangedEventHandler handler = null;
            handler = new PropertyChangedEventHandler((s, e) =>
            {
                switch (State)
                {
                    case SubscriptionState.Invalidated:
                        tcs.TrySetResult(null);
                        break;
                    case SubscriptionState.Error:
                        tcs.TrySetException(Error);
                        break;
                }
            });

            PropertyChanged += handler;
            await tcs.Task;
            PropertyChanged -= handler;
            _subscriptionToken.Dispose();
        }

        private void ReloadState()
        {
            if (_handle.IsClosed)
            {
                return;
            }

            var newState = _handle.GetState();
            if (newState != State)
            {
                try
                {
                    // If we encounter an unexpected value, assume it's an error.
                    if (!Enum.IsDefined(typeof(SubscriptionState), newState))
                    {
                        newState = SubscriptionState.Error;
                    }

                    State = newState;
                    switch (State)
                    {
                        case SubscriptionState.Error:
                            Error = _handle.GetError() ?? new RealmException($"An unknown error has occurred. State: {_handle.GetState()}");
                            _syncTcs.TrySetException(Error);
                            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Error)));
                            break;
                        case SubscriptionState.Complete:
                            _syncTcs.TrySetResult(null);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _syncTcs.TrySetException(ex);
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
            }
        }

        [NativeCallback(typeof(SubscriptionHandle.SubscriptionCallbackDelegate))]
        private static void SubscriptionCallbackImpl(IntPtr managedHandle)
        {
            if (GCHandle.FromIntPtr(managedHandle).Target is Subscription<T> subscription)
            {
                subscription.ReloadState();
            }
        }
    }
}
