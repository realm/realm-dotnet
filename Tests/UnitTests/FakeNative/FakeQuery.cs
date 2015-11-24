using System;
using System.Runtime.InteropServices;
using UnitTests;

namespace RealmNet
{
    internal static class NativeQuery
    {
        internal static  void string_equal(QueryHandle queryPtr, IntPtr columnIndex,
             string value, IntPtr valueLen)
        {
            throw new NotImplementedException();
        }

        internal static void string_not_equal(QueryHandle queryPtr, IntPtr columnIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen)
        {
            throw new NotImplementedException();
        }

        internal static void bool_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value)
        {
            throw new NotImplementedException();
        }

        internal static void bool_not_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value)
        {
            throw new NotImplementedException();
        }

        internal static void int_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value)
        {
            throw new NotImplementedException();
        }

        internal static void float_equal(QueryHandle queryPtr, IntPtr columnIndex, float value)
        {
            throw new NotImplementedException();
        }

        internal static void double_equal(QueryHandle queryPtr, IntPtr columnIndex, double value)
        {
            throw new NotImplementedException();
        }

        internal static void int_not_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value)
        {
            throw new NotImplementedException();
        }

        internal static void float_not_equal(QueryHandle queryPtr, IntPtr columnIndex, float value)
        {
            throw new NotImplementedException();
        }

        internal static void double_not_equal(QueryHandle queryPtr, IntPtr columnIndex, double value)
        {
            throw new NotImplementedException();
        }

        internal static void int_less(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value)
        {
            throw new NotImplementedException();
        }

        internal static void float_less(QueryHandle queryPtr, IntPtr columnIndex, float value)
        {
            throw new NotImplementedException();
        }

        internal static void double_less(QueryHandle queryPtr, IntPtr columnIndex, double value)
        {
            throw new NotImplementedException();
        }

        internal static void int_less_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value)
        {
            throw new NotImplementedException();
        }

        internal static void float_less_equal(QueryHandle queryPtr, IntPtr columnIndex, float value)
        {
            throw new NotImplementedException();
        }

        internal static void double_less_equal(QueryHandle queryPtr, IntPtr columnIndex, double value)
        {
            throw new NotImplementedException();
        }

        internal static void int_greater(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value)
        {
            throw new NotImplementedException();
        }

        internal static void float_greater(QueryHandle queryPtr, IntPtr columnIndex, float value)
        {
            throw new NotImplementedException();
        }

        internal static void double_greater(QueryHandle queryPtr, IntPtr columnIndex, double value)
        {
            throw new NotImplementedException();
        }

        internal static void int_greater_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value)
        {
            throw new NotImplementedException();
        }

        internal static void float_greater_equal(QueryHandle queryPtr, IntPtr columnIndex, float value)
        {
            throw new NotImplementedException();
        }

        internal static void double_greater_equal(QueryHandle queryPtr, IntPtr columnIndex, double value)
        {
            throw new NotImplementedException();
        }

        internal static RowHandle find(QueryHandle queryHandle, IntPtr lastMatch)
        {
            Logger.LogCall();
            return new RowHandle();
        }

        internal static IntPtr get_column_index(QueryHandle queryPtr, String columnName, IntPtr columnNameLen)
        {
            throw new NotImplementedException();
        }

        internal static void group_begin(QueryHandle queryHandle)
        {
            throw new NotImplementedException();
        }

        internal static void group_end(QueryHandle queryHandle)
        {
            throw new NotImplementedException();
        }

        internal static void or(QueryHandle queryHandle)
        {
            throw new NotImplementedException();
        }

        internal static void destroy(IntPtr queryHandle)
        {
            throw new NotImplementedException();
        }

        public static long count(QueryHandle QueryHandle, long start, long end, long limit)
        {
            throw new NotImplementedException();
        }

        public static double average(QueryHandle QueryHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

    }
}