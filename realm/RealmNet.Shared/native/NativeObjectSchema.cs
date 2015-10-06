using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Interop.Config;

namespace RealmNet
{
    internal static class NativeObjectSchema
    {
        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_schema_new", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr object_schema_new([MarshalAs(UnmanagedType.LPStr)]string name);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_schema_add_property",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern void add_property(IntPtr objectSchema, string name, IntPtr type,
            string objectType, IntPtr isPrimary, IntPtr isIndexed, IntPtr isNullable);
    }
}
