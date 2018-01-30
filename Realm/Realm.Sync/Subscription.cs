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
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Realms.Native;

namespace Realms.Sync
{
    public class Subscription<T> : INotifyPropertyChanged
    {
        private static readonly SubscriptionHandle.SubscriptionCallbackDelegate SubscriptionCallback = SubscriptionCallbackImpl;

        private readonly SubscriptionHandle _handle;

        private event PropertyChangedEventHandler _propertyChanged;
        private SubscriptionTokenHandle _subscriptionToken;

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                if (_propertyChanged == null)
                {
                    SubscribeForNotifications();
                }

                _propertyChanged += value;
            }

            remove
            {
                _propertyChanged -= value;

                if (_propertyChanged == null)
                {
                    UnsubscribeFromNotifications();
                }
            }
        }

        public SubscriptionState State => _handle.GetState();

        public Exception Error => _handle.GetError();

        public IQueryable<T> Query { get; }

        internal Subscription(SubscriptionHandle handle, RealmResults<T> query)
        {
            _handle = handle;
            Query = query;
        }

        public Task WaitForSynchronizationAsync()
        {
            var tcs = new TaskCompletionSource<object>();

            Action resolveTcs = () =>
            {
                switch (State)
                {
                    case SubscriptionState.Initialized:
                        tcs.TrySetResult(null);
                        break;
                    case SubscriptionState.Error:
                        tcs.TrySetException(Error);
                        break;
                    default:
                        throw new Exception("Unexpected State value.");
                }
            };

            if (State == SubscriptionState.Uninitialized)
            {
                PropertyChangedEventHandler handler = null;
                handler = new PropertyChangedEventHandler((sender, args) =>
                {
                    if (args.PropertyName == nameof(State) && State != SubscriptionState.Uninitialized)
                    {
                        PropertyChanged -= handler;
                        resolveTcs();
                    }
                });
                PropertyChanged += handler;
            }
            else
            {
                resolveTcs();
            }

            return tcs.Task;
        }

        private void SubscribeForNotifications()
        {
            Debug.Assert(_subscriptionToken == null, "_subscriptionToken must be null before subscribing.");

            var managedSubscriptionHandle = GCHandle.Alloc(this, GCHandleType.Weak);
            _subscriptionToken = _handle.AddNotificationCallback(GCHandle.ToIntPtr(managedSubscriptionHandle), SubscriptionCallback);
        }

        private void UnsubscribeFromNotifications()
        {
            _subscriptionToken?.Dispose();
            _subscriptionToken = null;
        }

        [NativeCallback(typeof(SubscriptionHandle.SubscriptionCallbackDelegate))]
        private static void SubscriptionCallbackImpl(IntPtr managedHandle)
        {
            if (GCHandle.FromIntPtr(managedHandle).Target is Subscription<T> subscription)
            {
                subscription._propertyChanged?.Invoke(subscription, new PropertyChangedEventArgs(nameof(State)));
                if (subscription.State == SubscriptionState.Error)
                {
                    subscription._propertyChanged?.Invoke(subscription, new PropertyChangedEventArgs(nameof(Error)));
                }
            }
        }
    }
}
