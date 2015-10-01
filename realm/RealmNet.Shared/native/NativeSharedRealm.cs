using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Interop.Config;

namespace RealmNet
{
    internal static class NativeSharedRealm
    {
        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_open", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr open(SchemaHandle schemaHandle, string path, IntPtr readOnly,
            IntPtr durability, string encryptionKey);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_delete", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr delete(IntPtr sharedRealm);

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
    }
}
