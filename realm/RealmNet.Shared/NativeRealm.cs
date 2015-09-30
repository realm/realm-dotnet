using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Interop.Config;

namespace RealmNet
{
    internal static class NativeRealm
    {
        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_object_schema_new", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr realm_object_schema_new([MarshalAs(UnmanagedType.LPStr)]string name);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_schema_new", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr realm_schema_new([MarshalAs(UnmanagedType.LPArray)]IntPtr[] objectSchemas, IntPtr objectSchemaCount);



    }
}
