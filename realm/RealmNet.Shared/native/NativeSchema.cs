using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Interop.Config;

namespace RealmNet
{
    internal static class NativeSchema
    {
        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "schema_new", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr schema_new([MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] [In,Out] IntPtr[] objectSchemas, IntPtr objectSchemaCount);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "schema_generate", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr generate();
    }
}
