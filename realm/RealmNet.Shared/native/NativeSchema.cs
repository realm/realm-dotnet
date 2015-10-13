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
        internal static extern void initializer_add_object_schema(SchemaInitializerHandle initializer, ObjectSchemaHandle objectSchema);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "schema_create", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr create(SchemaInitializerHandle schemaInitializer);
    }
}
