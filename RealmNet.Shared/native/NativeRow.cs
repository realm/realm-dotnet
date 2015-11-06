﻿using System;
using System.Runtime.InteropServices;

namespace RealmNet
{
    internal static class NativeRow
    {
        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "row_get_row_index", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr row_get_row_index(RowHandle rowHandle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "row_get_is_attached",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr row_get_is_attached(RowHandle rowHandle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "row_destroy", CallingConvention = CallingConvention.Cdecl)]
        public static extern void destroy(IntPtr rowHandle);
    }
}
