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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;

namespace Realms
{
    internal class SharedRealmHandle : RealmHandle
    {
        [Preserve]
        public SharedRealmHandle()
        {
        }

        protected override void Unbind()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        /* TODO - if needed for PCL may need to change implementation
        public IntPtr Open(Native.Configuration configuration, RealmSchema schema, byte[] encryptionKey)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return IntPtr.Zero;
        }
        */

        public void CloseRealm()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public void BindToManagedRealmHandle(IntPtr managedRealmHandle)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public void AddObservedObject(IntPtr managedRealmHandle, ObjectHandle objectHandle, IntPtr managedRealmObjectHandle)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public void RemoveObservedObject(IntPtr managedRealmObjectHandle)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public void BeginTransaction()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public void CommitTransaction()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public void CancelTransaction()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public bool IsInTransaction()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }

        public bool Refresh()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }

        public IntPtr GetTable(string tableName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return IntPtr.Zero;
        }

        public bool IsSameInstance(SharedRealmHandle other)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }

        public ulong GetSchemaVersion()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return 0;
        }

        public bool Compact()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }
    }
}
