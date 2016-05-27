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
    internal static class NativeQuery
    {
        // This is a delegate type meant to represent one of the "query operator" methods such as float_less and bool_equal
        internal delegate void Operation<T>(QueryHandle queryPtr, IntPtr columnIndex, T value);

        //c++ each of the operator functions such as string_equal
        //returns q again, the QueryHandle object is re-used and keeps its pointer.
        //so high-level stuff should also return self, to enable stacking of operations QueryHandle.dothis().dothat()

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_string_contains", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void string_contains(QueryHandle queryPtr, IntPtr columnIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_string_starts_with", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void string_starts_with(QueryHandle queryPtr, IntPtr columnIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_string_ends_with", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void string_ends_with(QueryHandle queryPtr, IntPtr columnIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_string_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void string_equal(QueryHandle queryPtr, IntPtr columnIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_string_not_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void string_not_equal(QueryHandle queryPtr, IntPtr columnIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_bool_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void bool_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_bool_not_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void bool_not_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void int_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_not_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void int_not_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_less", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void int_less(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_less_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void int_less_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_greater", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void int_greater(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_greater_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void int_greater_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_float_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void float_equal(QueryHandle queryPtr, IntPtr columnIndex, float value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_float_not_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void float_not_equal(QueryHandle queryPtr, IntPtr columnIndex, float value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_float_less", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void float_less(QueryHandle queryPtr, IntPtr columnIndex, float value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_float_less_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void float_less_equal(QueryHandle queryPtr, IntPtr columnIndex, float value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_float_greater", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void float_greater(QueryHandle queryPtr, IntPtr columnIndex, float value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_float_greater_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void float_greater_equal(QueryHandle queryPtr, IntPtr columnIndex, float value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_double_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void double_equal(QueryHandle queryPtr, IntPtr columnIndex, double value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_double_not_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void double_not_equal(QueryHandle queryPtr, IntPtr columnIndex, double value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_double_less", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void double_less(QueryHandle queryPtr, IntPtr columnIndex, double value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_double_less_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void double_less_equal(QueryHandle queryPtr, IntPtr columnIndex, double value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_double_greater", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void double_greater(QueryHandle queryPtr, IntPtr columnIndex, double value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_double_greater_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void double_greater_equal(QueryHandle queryPtr, IntPtr columnIndex, double value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_timestamp_milliseconds_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void timestamp_milliseconds_equal(QueryHandle queryPtr, IntPtr columnIndex, Int64 value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_timestamp_milliseconds_not_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void timestamp_milliseconds_not_equal(QueryHandle queryPtr, IntPtr columnIndex, Int64 value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_timestamp_milliseconds_less", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void timestamp_milliseconds_less(QueryHandle queryPtr, IntPtr columnIndex, Int64 value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_timestamp_milliseconds_less_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void timestamp_milliseconds_less_equal(QueryHandle queryPtr, IntPtr columnIndex, Int64 value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_timestamp_milliseconds_greater", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void timestamp_milliseconds_greater(QueryHandle queryPtr, IntPtr columnIndex, Int64 value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_timestamp_milliseconds_greater_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void timestamp_milliseconds_greater_equal(QueryHandle queryPtr, IntPtr columnIndex, Int64 value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_find", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr findDirect(QueryHandle queryHandle, IntPtr beginAtRow);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_get_column_index", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr get_column_index(QueryHandle queryPtr,
        [MarshalAs(UnmanagedType.LPWStr)] String columnName, IntPtr columnNameLen);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_not", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void not(QueryHandle queryHandle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_group_begin", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void group_begin(QueryHandle queryHandle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_group_end", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void group_end(QueryHandle queryHandle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_or", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void or(QueryHandle queryHandle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_destroy", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void destroy(IntPtr queryHandle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_count", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr count(QueryHandle QueryHandle);

        public static double average(QueryHandle QueryHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

    }
}