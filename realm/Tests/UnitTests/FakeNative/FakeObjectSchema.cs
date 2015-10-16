using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RealmNet
{
    internal static class NativeObjectSchema
    {
        internal static IntPtr create([MarshalAs(UnmanagedType.LPStr)] string name)
        {
            return (IntPtr) 0;
        }

        internal static IntPtr object_schema_new(string name)
        {
            return (IntPtr) 0;
        }

        internal static void add_property(IntPtr objectSchema, string name, IntPtr type,
            string objectType, IntPtr isPrimary, IntPtr isIndexed, IntPtr isNullable)
        {
        }
    }
}
