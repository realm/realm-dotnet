using System;
using System.Runtime.InteropServices;

namespace RealmNet
{
    internal static class NativeSchema
    {
        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "schema_initializer_create",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr initializer_create();

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "schema_initializer_destroy",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern void initializer_destroy(IntPtr initializer);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "schema_initializer_add_object_schema",
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern void initializer_add_object_schema(SchemaInitializerHandle initializer, IntPtr objectSchema);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "schema_new", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr schema_new(SchemaInitializerHandle schemaInitializer);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "schema_generate", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr generate();
    }
}
