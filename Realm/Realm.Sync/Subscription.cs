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
    public class Subscription<T> : INotifyPropertyChanged
    {
        private static readonly SubscriptionHandle.SubscriptionCallbackDelegate SubscriptionCallback = SubscriptionCallbackImpl;

        private readonly SubscriptionHandle _handle;
        private readonly TaskCompletionSource<object> _syncTcs = new TaskCompletionSource<object>();

        private SubscriptionTokenHandle _subscriptionToken;

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        public SubscriptionState State { get; private set; }

        public Exception Error { get; private set; }

        public IQueryable<T> Results { get; }

        internal Subscription(SubscriptionHandle handle, RealmResults<T> query)
        {
            Results = query;

            _handle = handle;
            State = _handle.GetState();

            var managedSubscriptionHandle = GCHandle.Alloc(this, GCHandleType.Weak);
            _subscriptionToken = _handle.AddNotificationCallback(GCHandle.ToIntPtr(managedSubscriptionHandle), SubscriptionCallback);
        }

        public Task WaitForSynchronizationAsync()
        {
            return _syncTcs.Task;
        }

        public async Task UnsubscribeAsync()
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
