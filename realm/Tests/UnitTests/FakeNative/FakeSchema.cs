using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RealmNet
{
    internal static class NativeSchema
    {
        internal static IntPtr create(SchemaInitializerHandle schemaInitializerHandle)
        {
            return (IntPtr) 0;
        }

        internal static IntPtr initializer_create()
        {
            return (IntPtr) 0;
        }

        internal static void initializer_destroy(IntPtr initializerPtr)
        {
        }

        internal static void initializer_add_object_schema(SchemaInitializerHandle initializer, IntPtr objectSchema)
        {
        }
    }
}
