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
using System.Runtime.InteropServices;

namespace Realms
{
    internal class ResultsHandle : CollectionHandleBase
    {
        // keep this one even though warned that it is not used. It is in fact used by marshalling
        // used by P/Invoke to automatically construct a ResultsHandle when returning a size_t as a ResultsHandle
        [Preserve]
        public ResultsHandle() : base(null)
        {
        }

        protected override void Unbind()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public override IntPtr GetObjectAtIndex(int index)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return IntPtr.Zero;
        }

        public override int Count()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return 0;
        }

        public void Clear(SharedRealmHandle realmHandle)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public override IntPtr AddNotificationCallback(IntPtr managedCollectionHandle, NotificationCallbackDelegate callback)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return IntPtr.Zero;
        }

        public override bool Equals(object p)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }
    }
}
