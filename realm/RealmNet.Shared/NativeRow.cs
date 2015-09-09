using System;
using System.Runtime.InteropServices;
using Interop.Config;

namespace RealmNet
{
    internal static class NativeRow
    {
        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "row_get_row_index", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr row_get_row_index(RowHandle rowHandle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "row_get_is_attached",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr row_get_is_attached(RowHandle rowHandle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "row_delete", CallingConvention = CallingConvention.Cdecl)]
        public static extern void row_delete(RowHandle rowHandle);
    }
}
