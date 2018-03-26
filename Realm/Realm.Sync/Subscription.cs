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
    public class Subscription<T> : INotifyPropertyChanged, IDisposable
    {
        private static readonly SubscriptionHandle.SubscriptionCallbackDelegate SubscriptionCallback = SubscriptionCallbackImpl;

        private readonly SubscriptionHandle _handle;
        private readonly Realm _realm;
        private readonly RealmObject.Metadata _metadata;

        private SubscriptionTokenHandle _subscriptionToken;

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        public SubscriptionState State { get; private set; }

        public Exception Error { get; private set; }

        public IQueryable<T> Results { get; private set; }
        {
            get
            {
                var handle = _handle.GetResults(_realm.SharedRealmHandle);
                return new RealmResults<T>(_realm, _metadata, handle);
            }
        }

        internal Subscription(SubscriptionHandle handle, RealmResults<T> query)
        {
            _handle = handle;
            _realm = query.Realm;
            _metadata = query.Metadata;
        }

        public void Dispose()
        {
            if (_subscriptionToken != null)
            {
                
            }
        }

        public Task WaitForSynchronizationAsync()
        {
            var tcs = new TaskCompletionSource<object>();

            bool resolveTcs()
            {
                switch (State)
                {
                    case SubscriptionState.Complete:
                        tcs.TrySetResult(null);
                        return true;
                    case SubscriptionState.Error:
                        tcs.TrySetException(Error);
                        return true;
                    default:
                        return false;
                }
            }

            if (!resolveTcs())
            {
                PropertyChangedEventHandler handler = null;
                handler = new PropertyChangedEventHandler((sender, args) =>
                {
                    if (args.PropertyName == nameof(State) && resolveTcs())
                    {
                        PropertyChanged -= handler;
                    }
                });
                PropertyChanged += handler;
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
