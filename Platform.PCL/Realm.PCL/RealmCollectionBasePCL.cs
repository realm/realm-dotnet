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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Realms
{
    internal abstract class RealmCollectionBase<T> : RealmCollectionNativeHelper.Interface, IRealmCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            }

            remove
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            }

            remove
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            }
        }

        public int Count
        {
            get
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                return 0;
            }
        }

        public Schema.ObjectSchema ObjectSchema
        {
            get
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                return default(Schema.ObjectSchema);
            }
        }

        protected RealmCollectionBase(Realm realm, RealmObject.Metadata metadata)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        ~RealmCollectionBase()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        protected abstract CollectionHandleBase CreateHandle();

        public T this[int index]
        {
            get
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                return default(T);
            }
        }

        public IDisposable SubscribeForNotifications(NotificationCallbackDelegate<T> callback)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        private void UnsubscribeFromNotifications(NotificationCallbackDelegate<T> callback)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        private void SubscribeForNotifications()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        private void UnsubscribeFromNotifications()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        #region INotifyCollectionChanged

        private void OnChange(IRealmCollection<T> sender, ChangeSet change, Exception error)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        protected void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        protected void RaisePropertyChanged()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        private static bool TryGetConsecutive(int[] indices, Func<int, T> getter, out IList items, out int startIndex)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            items = null;
            startIndex = 0;
            return false;
        }

        private void UpdateCollectionChangedSubscriptionIfNecessary(bool isSubscribed)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        #endregion

        void RealmCollectionNativeHelper.Interface.NotifyCallbacks(CollectionHandleBase.CollectionChangeSet? changes, NativeException? exception)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public IEnumerator<T> GetEnumerator() 
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator(); // using our class generic type, just redirect the legacy get

        private class NotificationToken : IDisposable
        {
            internal NotificationToken(RealmCollectionBase<T> collection, NotificationCallbackDelegate<T> callback)
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            }

            public void Dispose()
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            }
        }

        public class Enumerator : IEnumerator<T>
        {
            internal Enumerator(RealmCollectionBase<T> parent)
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            }

            public T Current 
            {
                get
                {
                    RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                    return default(T);
                }
            }

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                return false;
            }

            public void Reset()
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            }

            public void Dispose()
            {
            }
        }
    }
}