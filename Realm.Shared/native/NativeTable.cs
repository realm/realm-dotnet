////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////
 
using System;
using System.Runtime.InteropServices;

namespace Realms
{
    internal static class NativeTable
    {
        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_add_column", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr add_column(TableHandle tableHandle, IntPtr type,
            [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr nameLen);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_add_empty_row", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr add_empty_row(TableHandle tableHandle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_timestamp_milliseconds", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void set_timestamp_milliseconds(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx, Int64 value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_timestamp_milliseconds", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Int64 get_timestamp_milliseconds(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_nullable_timestamp_milliseconds", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr get_nullable_timestamp_milliseconds(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex, ref long retVal);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_string", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void set_string(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_string_unique", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void set_string_unique(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_string", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr get_string(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex,
            IntPtr buffer, IntPtr bufsize, [MarshalAs(UnmanagedType.I1)] out bool isNull);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_link", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void set_link(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx, IntPtr targetRowNdx);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_clear_link", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void clear_link(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_link", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr get_link(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_linklist", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr get_linklist(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_null", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void set_null(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_bool", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void set_bool(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx, IntPtr value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_bool", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr get_bool(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_nullable_bool", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr get_nullable_bool(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex, ref IntPtr retVal);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_int64", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void set_int64(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx, Int64 value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_int64_unique", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void set_int64_unique(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx, Int64 value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_int64", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Int64 get_int64(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_nullable_int64", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr get_nullable_int64(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex, ref Int64 retVal);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_float", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void set_float(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx, float value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_float", CallingConvention = CallingConvention.Cdecl)]
        internal static extern float get_float(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_nullable_float", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr get_nullable_float(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex, ref float retVal);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_double", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void set_double(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx, double value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_double", CallingConvention = CallingConvention.Cdecl)]
        internal static extern double get_double(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_nullable_double", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr get_nullable_double(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex, ref double retVal);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_where", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr where(TableHandle handle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_count_all", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Int64 count_all(TableHandle handle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_unbind", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void unbind(IntPtr tableHandle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_remove_row", CallingConvention = CallingConvention.Cdecl)]
        public static extern void remove_row(TableHandle tableHandle, RowHandle rowHandle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_column_index", CallingConvention = CallingConvention.Cdecl)]
         //returns -1 if the column string does not match a column index
       internal static extern IntPtr get_column_index(TableHandle tablehandle,
            [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr nameLen);

    }
}