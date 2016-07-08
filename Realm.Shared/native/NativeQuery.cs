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

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_binary_equal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void binary_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr buffer, IntPtr bufferLength, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_binary_not_equal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void binary_not_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr buffer, IntPtr bufferLength, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_string_contains", CallingConvention = CallingConvention.Cdecl)]
        private static extern void string_contains(QueryHandle queryPtr, IntPtr columnIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_string_starts_with", CallingConvention = CallingConvention.Cdecl)]
        private static extern void string_starts_with(QueryHandle queryPtr, IntPtr columnIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_string_ends_with", CallingConvention = CallingConvention.Cdecl)]
        private static extern void string_ends_with(QueryHandle queryPtr, IntPtr columnIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_string_equal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void string_equal(QueryHandle queryPtr, IntPtr columnIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_string_not_equal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void string_not_equal(QueryHandle queryPtr, IntPtr columnIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_bool_equal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void bool_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_bool_not_equal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void bool_not_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_equal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void int_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_not_equal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void int_not_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_less", CallingConvention = CallingConvention.Cdecl)]
        private static extern void int_less(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_less_equal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void int_less_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_greater", CallingConvention = CallingConvention.Cdecl)]
        private static extern void int_greater(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_greater_equal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void int_greater_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_long_equal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void long_equal(QueryHandle queryPtr, IntPtr columnIndex, Int64 value, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_long_not_equal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void long_not_equal(QueryHandle queryPtr, IntPtr columnIndex, Int64 value, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_long_less", CallingConvention = CallingConvention.Cdecl)]
        private static extern void long_less(QueryHandle queryPtr, IntPtr columnIndex, Int64 value, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_long_less_equal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void long_less_equal(QueryHandle queryPtr, IntPtr columnIndex, Int64 value, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_long_greater", CallingConvention = CallingConvention.Cdecl)]
        private static extern void long_greater(QueryHandle queryPtr, IntPtr columnIndex, Int64 value, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_long_greater_equal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void long_greater_equal(QueryHandle queryPtr, IntPtr columnIndex, Int64 value, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_float_equal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void float_equal(QueryHandle queryPtr, IntPtr columnIndex, float value, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_float_not_equal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void float_not_equal(QueryHandle queryPtr, IntPtr columnIndex, float value, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_float_less", CallingConvention = CallingConvention.Cdecl)]
        private static extern void float_less(QueryHandle queryPtr, IntPtr columnIndex, float value, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_float_less_equal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void float_less_equal(QueryHandle queryPtr, IntPtr columnIndex, float value, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_float_greater", CallingConvention = CallingConvention.Cdecl)]
        private static extern void float_greater(QueryHandle queryPtr, IntPtr columnIndex, float value, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_float_greater_equal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void float_greater_equal(QueryHandle queryPtr, IntPtr columnIndex, float value, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_double_equal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void double_equal(QueryHandle queryPtr, IntPtr columnIndex, double value, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_double_not_equal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void double_not_equal(QueryHandle queryPtr, IntPtr columnIndex, double value, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_double_less", CallingConvention = CallingConvention.Cdecl)]
        private static extern void double_less(QueryHandle queryPtr, IntPtr columnIndex, double value, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_double_less_equal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void double_less_equal(QueryHandle queryPtr, IntPtr columnIndex, double value, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_double_greater", CallingConvention = CallingConvention.Cdecl)]
        private static extern void double_greater(QueryHandle queryPtr, IntPtr columnIndex, double value, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_double_greater_equal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void double_greater_equal(QueryHandle queryPtr, IntPtr columnIndex, double value, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_timestamp_milliseconds_equal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void timestamp_milliseconds_equal(QueryHandle queryPtr, IntPtr columnIndex, Int64 value, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_timestamp_milliseconds_not_equal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void timestamp_milliseconds_not_equal(QueryHandle queryPtr, IntPtr columnIndex, Int64 value, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_timestamp_milliseconds_less", CallingConvention = CallingConvention.Cdecl)]
        private static extern void timestamp_milliseconds_less(QueryHandle queryPtr, IntPtr columnIndex, Int64 value, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_timestamp_milliseconds_less_equal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void timestamp_milliseconds_less_equal(QueryHandle queryPtr, IntPtr columnIndex, Int64 value, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_timestamp_milliseconds_greater", CallingConvention = CallingConvention.Cdecl)]
        private static extern void timestamp_milliseconds_greater(QueryHandle queryPtr, IntPtr columnIndex, Int64 value, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_timestamp_milliseconds_greater_equal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void timestamp_milliseconds_greater_equal(QueryHandle queryPtr, IntPtr columnIndex, Int64 value, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_find", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr findDirect(QueryHandle queryHandle, IntPtr beginAtRow, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_get_column_index", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr get_column_index(QueryHandle queryPtr,
        [MarshalAs(UnmanagedType.LPWStr)] String columnName, IntPtr columnNameLen, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_not", CallingConvention = CallingConvention.Cdecl)]
        private static extern void not(QueryHandle queryHandle, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_group_begin", CallingConvention = CallingConvention.Cdecl)]
        private static extern void group_begin(QueryHandle queryHandle, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_group_end", CallingConvention = CallingConvention.Cdecl)]
        private static extern void group_end(QueryHandle queryHandle, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_or", CallingConvention = CallingConvention.Cdecl)]
        private static extern void or(QueryHandle queryHandle, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_destroy", CallingConvention = CallingConvention.Cdecl)]
        private static extern void destroy(IntPtr queryHandle, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_count", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr count(QueryHandle QueryHandle, out NativeException ex);

        public static double average(QueryHandle QueryHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

    }
}
