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
using Realms.Native;

namespace Realms.Sync
{
    public class Subscription<T> : INotifyPropertyChanged, IDisposable
    {
        private static readonly SubscriptionHandle.SubscriptionCallbackDelegate SubscriptionCallback = SubscriptionCallbackImpl;

        private readonly SubscriptionHandle _handle;
        private readonly TaskCompletionSource<object> _syncTcs = new TaskCompletionSource<object>();

        private SubscriptionTokenHandle _subscriptionToken;

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        public SubscriptionState State { get; private set; }

        public Exception Error { get; private set; }

        public IQueryable<T> Results { get; private set; }

        internal Subscription(SubscriptionHandle handle, RealmResults<T> query)
        {
            State = SubscriptionState.Pending;
            Results = query;

            _handle = handle;

            var managedSubscriptionHandle = GCHandle.Alloc(this, GCHandleType.Weak);
            _subscriptionToken = _handle.AddNotificationCallback(GCHandle.ToIntPtr(managedSubscriptionHandle), SubscriptionCallback);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _subscriptionToken?.Dispose();
            _subscriptionToken = null;
            _handle.Dispose();
        }

        public Task WaitForSynchronizationAsync()
        {
            return _syncTcs.Task;
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
                string propToNotify = null;
                State = newState;
                switch (State)
                {
                    case SubscriptionState.Error:
                        Error = _handle.GetError();
                        propToNotify = nameof(Error);
                        _syncTcs.TrySetException(Error);
                        break;
                    case SubscriptionState.Complete:
                        var oldResults = (RealmResults<T>)Results;
                        var realm = oldResults.Realm;
                        var resultsHandle = _handle.GetResults(realm.SharedRealmHandle);
                        Results = new RealmResults<T>(realm, oldResults.Metadata, resultsHandle);
                        propToNotify = nameof(Results);
                        _syncTcs.TrySetResult(null);
                        break;
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propToNotify));
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
