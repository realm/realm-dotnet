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
    internal class SharedRealmHandle : RealmHandle
    {
        private static class NativeMethods
        {
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_open", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr open(SchemaHandle schemaHandle, [MarshalAs(UnmanagedType.LPWStr)]string path, IntPtr pathLength, IntPtr readOnly,
                IntPtr durability, byte[] encryptionKey, UInt64 schemaVersion, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_bind_to_managed_realm_handle", CallingConvention = CallingConvention.Cdecl)]
            public static extern void bind_to_managed_realm_handle(SharedRealmHandle sharedRealm, IntPtr managedRealmHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr sharedRealm);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_close_realm",
                CallingConvention = CallingConvention.Cdecl)]
            public static extern void close_realm(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_begin_transaction",
                CallingConvention = CallingConvention.Cdecl)]
            public static extern void begin_transaction(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_commit_transaction",
                CallingConvention = CallingConvention.Cdecl)]
            public static extern void commit_transaction(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_cancel_transaction",
                CallingConvention = CallingConvention.Cdecl)]
            public static extern void cancel_transaction(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_is_in_transaction",
                CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr is_in_transaction(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_refresh",
                CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr refresh(SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_get_table",
                CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_table(SharedRealmHandle sharedRealm, [MarshalAs(UnmanagedType.LPWStr)]string tableName, IntPtr tableNameLength, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_is_same_instance",
                CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr is_same_instance(SharedRealmHandle lhs, SharedRealmHandle rhs, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_get_schema_version",
                CallingConvention = CallingConvention.Cdecl)]
            public static extern UInt64 get_schema_version(SharedRealmHandle sharedRealm, out NativeException ex);
        }

        [Preserve]
        public SharedRealmHandle()
        {
        }

        protected override void Unbind()
        {
            NativeMethods.destroy(handle);
        }

        public IntPtr Open(SchemaHandle schemaHandle, string path, bool readOnly, bool durability, byte[] encryptionKey, 
                ulong schemaVersion)
        {
            NativeException nativeException;
            var result = NativeMethods.open(schemaHandle, path, (IntPtr)path.Length, MarshalHelpers.BoolToIntPtr(readOnly), 
                    MarshalHelpers.BoolToIntPtr(durability), encryptionKey, schemaVersion, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public void CloseRealm()
        {
            NativeException nativeException;
            NativeMethods.close_realm(this, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void BindToManagedRealmHandle(IntPtr managedRealmHandle)
        {
            NativeException nativeException;
            NativeMethods.bind_to_managed_realm_handle(this, managedRealmHandle, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void BeginTransaction()
        {
            NativeException nativeException;
            NativeMethods.begin_transaction(this, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void CommitTransaction()
        {
            NativeException nativeException;
            NativeMethods.commit_transaction(this, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void CancelTransaction()
        {
            NativeException nativeException;
            NativeMethods.cancel_transaction(this, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public bool IsInTransaction()
        {
            NativeException nativeException;
            var result = NativeMethods.is_in_transaction(this, out nativeException);
            nativeException.ThrowIfNecessary();
            return MarshalHelpers.IntPtrToBool(result);
        }

        public bool Refresh()
        {
            NativeException nativeException;
            var result = NativeMethods.refresh(this, out nativeException);
            nativeException.ThrowIfNecessary();
            return MarshalHelpers.IntPtrToBool(result);
        }

        public IntPtr GetTable(string tableName)
        {
            NativeException nativeException;
            var result = NativeMethods.get_table(this, tableName, (IntPtr)tableName.Length, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public bool IsSameInstance(SharedRealmHandle rhs)
        {
            NativeException nativeException;
            var result = NativeMethods.is_same_instance(this, rhs, out nativeException);
            nativeException.ThrowIfNecessary();
            return MarshalHelpers.IntPtrToBool(result);
        }

        public ulong GetSchemaVersion()
        {
            NativeException nativeException;
            var result = NativeMethods.get_schema_version(this, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }
    }
}
