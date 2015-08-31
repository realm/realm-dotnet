using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Interop.Config;
using InteropShared;

namespace RealmNet.Interop
{
    internal static class NativeQuery
    {

        //c++ each of the operator functions such as query_string_equal
        //returns q again, the QueryHandle object is re-used and keeps its pointer.
        //so high-level stuff should also return self, to enable stacking of operations QueryHandle.dothis().dothat()

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_string_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void query_string_equal(QueryHandle queryPtr, IntPtr columnIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_string_not_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void query_string_not_equal(QueryHandle queryPtr, IntPtr columnIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_bool_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void query_bool_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_bool_not_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void query_bool_not_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void query_int_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_not_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void query_int_not_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_less", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void query_int_less(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_less_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void query_int_less_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_greater", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void query_int_greater(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_greater_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void query_int_greater_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

        public static void query_int_between(QueryHandle QueryHandle, long columnIndex, long lowValue, long highValue)
        {
            throw new NotImplementedException();
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_find", CallingConvention = CallingConvention.Cdecl)]
        internal static extern RowHandle query_find(QueryHandle queryHandle, IntPtr lastMatch);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_get_column_index", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr query_get_column_index(QueryHandle queryPtr,
        [MarshalAs(UnmanagedType.LPWStr)] String columnName, IntPtr columnNameLen);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_group_begin", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void query_group_begin(QueryHandle queryHandle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_group_end", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void query_group_end(QueryHandle queryHandle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_or", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void query_or(QueryHandle queryHandle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_delete", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void query_delete(QueryHandle queryHandle);

        public static long query_count(QueryHandle QueryHandle, long start, long end, long limit)
        {
            throw new NotImplementedException();
        }
        public static double query_average(QueryHandle QueryHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

	}
}