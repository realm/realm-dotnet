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
#pragma warning disable SA1121 // Use built-in type alias

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr tableHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_create_results", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr create_results(TableHandle handle, SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_object", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_object(TableHandle table, SharedRealmHandle realm, ObjectKey objectKey, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_object_for_string_primarykey", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_object_for_string_primarykey(TableHandle handle, SharedRealmHandle realmHandle,
                [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_object_for_primitive_primarykey", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_object_for_primitive_primarykey(TableHandle handle, SharedRealmHandle realmHandle, PrimitiveValue value, out NativeException ex);

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore SA1121 // Use built-in type alias
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

        public bool TryFind(SharedRealmHandle realmHandle, string id, out ObjectHandle objectHandle)
        {
            var result = NativeMethods.get_object_for_string_primarykey(this, realmHandle, id, (IntPtr)(id?.Length ?? 0), out var ex);
            return TryFindCore(realmHandle, result, ex, out objectHandle);
        }

        public unsafe bool TryFind(SharedRealmHandle realmHandle, PrimitiveValue id, out ObjectHandle objectHandle)
        {
            var result = NativeMethods.get_object_for_primitive_primarykey(this, realmHandle, id, out var ex);
            return TryFindCore(realmHandle, result, ex, out objectHandle);
        }

        private static bool TryFindCore(SharedRealmHandle realmHandle, IntPtr objectPtr, NativeException nativeException, out ObjectHandle objectHandle)
        {
            nativeException.ThrowIfNecessary();
            if (objectPtr == IntPtr.Zero)
            {
                objectHandle = null;
                return false;
            }

            objectHandle = new ObjectHandle(realmHandle, objectPtr);
            return true;
        }
    }
}