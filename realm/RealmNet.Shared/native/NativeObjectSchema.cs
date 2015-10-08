using System;
using System.Runtime.InteropServices;

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
