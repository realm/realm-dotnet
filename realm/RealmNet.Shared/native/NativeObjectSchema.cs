using System;
using System.Runtime.InteropServices;

namespace RealmNet
{
    internal static class NativeObjectSchema
    {
        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_schema_create", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr create([MarshalAs(UnmanagedType.LPStr)]string name);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_schema_destroy", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr destroy(IntPtr objectSchemaPtr);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_schema_add_property",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern void add_property(ObjectSchemaHandle objectSchemaHandle, string name, IntPtr type,
            string objectType, IntPtr isPrimary, IntPtr isIndexed, IntPtr isNullable);
    }
}
