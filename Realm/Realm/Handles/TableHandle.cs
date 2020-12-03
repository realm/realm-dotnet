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
using Realms.Native;

namespace Realms
{
    internal class TableHandle : RealmHandle
    {
        private static class NativeMethods
        {
#pragma warning disable IDE1006 // Naming Styles

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr tableHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_create_results", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr create_results(TableHandle handle, SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_object", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_object(TableHandle table, SharedRealmHandle realm, ObjectKey objectKey, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_object_for_primarykey", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_object_for_primarykey(TableHandle handle, SharedRealmHandle realmHandle, PrimitiveValue value, out NativeException ex);

#pragma warning restore IDE1006 // Naming Styles
        }

        [Preserve]
        public TableHandle(RealmHandle root, IntPtr handle) : base(root, handle)
        {
        }

        protected override void Unbind()
        {
            NativeMethods.destroy(handle, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public ResultsHandle CreateResults(SharedRealmHandle sharedRealmHandle)
        {
            var result = NativeMethods.create_results(this, sharedRealmHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new ResultsHandle(sharedRealmHandle, result);
        }

        public ObjectHandle Get(SharedRealmHandle realmHandle, ObjectKey objectKey)
        {
            var result = NativeMethods.get_object(this, realmHandle, objectKey, out var nativeException);
            nativeException.ThrowIfNecessary();

            if (result == IntPtr.Zero)
            {
                return null;
            }

            return new ObjectHandle(realmHandle, result);
        }

        public unsafe bool TryFind(SharedRealmHandle realmHandle, in RealmValue id, out ObjectHandle objectHandle)
        {
            var (primitiveValue, handles) = id.ToNative();
            var result = NativeMethods.get_object_for_primarykey(this, realmHandle, primitiveValue, out var ex);
            handles?.Dispose();
            ex.ThrowIfNecessary();

            if (result == IntPtr.Zero)
            {
                objectHandle = null;
                return false;
            }

            objectHandle = new ObjectHandle(realmHandle, result);
            return true;
        }
    }
}