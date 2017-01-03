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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Realms
{
    internal class ObjectHandle : RealmHandle
    {
        public bool IsValid
        {
            get
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                return false;
            }
        }

        // keep this one even though warned that it is not used. It is in fact used by marshalling
        // used by P/Invoke to automatically construct a TableHandle when returning a size_t as a TableHandle
        [Preserve]
        public ObjectHandle(SharedRealmHandle sharedRealmHandle) : base(sharedRealmHandle)
        {
        }

        public override bool Equals(object obj)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }

        protected override void Unbind()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        // acquire a ListHandle from object_get_list And set root in an atomic fashion 
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands"), SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        internal ListHandle TableLinkList(IntPtr propertyIndex)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }
    
        public void SetDateTimeOffset(IntPtr propertyIndex, DateTimeOffset value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public void SetNullableDateTimeOffset(IntPtr propertyIndex, DateTimeOffset? value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public DateTimeOffset GetDateTimeOffset(IntPtr propertyIndex)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return default(DateTimeOffset);
        }

        public DateTimeOffset? GetNullableDateTimeOffset(IntPtr propertyIndex)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        public void SetString(IntPtr propertyIndex, string value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public void SetStringUnique(IntPtr propertyIndex, string value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public string GetString(IntPtr propertyIndex)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return string.Empty;
        }

        public void SetLink(IntPtr propertyIndex, ObjectHandle targetHandle)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public void ClearLink(IntPtr propertyIndex)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public IntPtr GetLink(IntPtr propertyIndex)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return IntPtr.Zero;
        }

        public IntPtr GetLinklist(IntPtr propertyIndex)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return IntPtr.Zero;
        }

        public bool LinklistIsEmpty(IntPtr propertyIndex)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }

        public void SetBoolean(IntPtr propertyIndex, bool value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public void SetNullableBoolean(IntPtr propertyIndex, bool? value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public bool GetBoolean(IntPtr propertyIndex)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }

        public bool? GetNullableBoolean(IntPtr propertyIndex)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        public void SetInt64(IntPtr propertyIndex, long value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public void SetNullableInt64(IntPtr propertyIndex, long? value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public void SetInt64Unique(IntPtr propertyIndex, long value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public void SetNullableInt64Unique(IntPtr propertyIndex, long? value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public long GetInt64(IntPtr propertyIndex)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return 0;
        }

        public long? GetNullableInt64(IntPtr propertyIndex)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        public void SetSingle(IntPtr propertyIndex, float value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public void SetNullableSingle(IntPtr propertyIndex, float? value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public float GetSingle(IntPtr propertyIndex)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return 0.0f;
        }

        public float? GetNullableSingle(IntPtr propertyIndex)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        public void SetDouble(IntPtr propertyIndex, double value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public void SetNullableDouble(IntPtr propertyIndex, double? value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public double GetDouble(IntPtr propertyIndex)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return 0.0;
        }

        public double? GetNullableDouble(IntPtr propertyIndex)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        public void SetByteArray(IntPtr propertyIndex, byte[] value)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public byte[] GetByteArray(IntPtr propertyIndex)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        public void RemoveFromRealm(SharedRealmHandle realmHandle)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public RealmList<T> GetList<T>(Realm realm, IntPtr propertyIndex, string objectType) where T : RealmObject
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        public T GetObject<T>(Realm realm, IntPtr propertyIndex, string objectType) where T : RealmObject
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        public void SetObject(Realm realm, IntPtr propertyIndex, RealmObject @object)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public ResultsHandle GetBacklinks(IntPtr propertyIndex)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }
    }
}