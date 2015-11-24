/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;
using System.Runtime.InteropServices;

namespace RealmNet
{
#if !DISABLE_NATIVE
    internal static class NativeTable
    {
        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_add_column", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr add_column(TableHandle tableHandle, IntPtr type,
            [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr nameLen);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_add_empty_row", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr add_empty_row(TableHandle tableHandle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_datetime_seconds", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void set_datetime_seconds(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx, Int64 value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_datetime_seconds", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Int64 get_datetime_seconds(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_string", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void set_string(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_string", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr get_string(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex,
            IntPtr buffer, IntPtr bufsize);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_bool", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void set_bool(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx, IntPtr value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_bool", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr get_bool(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_int64", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void set_int64(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx, Int64 value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_int64", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Int64 get_int64(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_float", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void set_float(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx, float value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_float", CallingConvention = CallingConvention.Cdecl)]
        internal static extern float get_float(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_double", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void set_double(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx, double value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_double", CallingConvention = CallingConvention.Cdecl)]
        internal static extern double get_double(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_where", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr where(TableHandle handle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_unbind", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void unbind(IntPtr tableHandle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_remove_row", CallingConvention = CallingConvention.Cdecl)]
        public static extern void remove_row(TableHandle tableHandle, RowHandle rowHandle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_column_index", CallingConvention = CallingConvention.Cdecl)]
         //returns -1 if the column string does not match a column index
       internal static extern IntPtr get_column_index(TableHandle tablehandle,
            [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr nameLen);

    }
#endif
}