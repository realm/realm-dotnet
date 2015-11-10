using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnitTests;

namespace RealmNet
{
    internal static class NativeObjectSchema
    {
        internal static IntPtr create([MarshalAs(UnmanagedType.LPStr)] string name)
        {
            Logger.LogCall($"{nameof(name)} = \"{name}\"");
            return (IntPtr) 0;
        }

        internal static void add_property(IntPtr objectSchema, string name, IntPtr type,
            string objectType, IntPtr isPrimary, IntPtr isIndexed, IntPtr isNullable)
        {
            Logger.LogCall($"{nameof(name)} = \"{name}\", {nameof(type)} = {type}");
        }
    }
}
