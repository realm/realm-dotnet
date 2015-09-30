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
    }
}
