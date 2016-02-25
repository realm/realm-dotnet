/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;
using System.Runtime.InteropServices;

namespace Realms
{
    internal static class NativeSharedRealm
    {
        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_open", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr open(SchemaHandle schemaHandle, [MarshalAs(UnmanagedType.LPWStr)]string path, IntPtr pathLength, IntPtr readOnly,
            IntPtr durability, byte[] encryptionKey, UInt64 schemaVersion);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_destroy", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr destroy(IntPtr sharedRealm);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_has_table",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr has_table(SharedRealmHandle sharedRealm, string tableName);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_begin_transaction",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern void begin_transaction(SharedRealmHandle sharedRealm);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_commit_transaction",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern void commit_transaction(SharedRealmHandle sharedRealm);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_cancel_transaction",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern void cancel_transaction(SharedRealmHandle sharedRealm);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_is_in_transaction",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr is_in_transaction(SharedRealmHandle sharedRealm);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_refresh",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern void refresh(SharedRealmHandle sharedRealm);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_get_table",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr get_table(SharedRealmHandle sharedRealm, [MarshalAs(UnmanagedType.LPWStr)]string tableName, IntPtr tableNameLength);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_is_same_instance",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr is_same_instance(SharedRealmHandle lhs, SharedRealmHandle rhs);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_get_schema_version",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern UInt64 get_schema_version(SharedRealmHandle sharedRealm);
    }
}
