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
using Realms.Native;

namespace Realms
{
    // tables and tableviews will always return query c++ classes that must be unbound
    // all other query returning calls (on query itself) will return the same object as was called,
    // and therefore have been changed to void calls in c++ part of binding
    // so these handles always represent a qeury object that should be released when not used anymore
    // the C# binding methods on query simply return self to add the . nottation again
    // A query will be a child of whatever root its creator has as root (queries are usually created by tableviews and tables)
    internal class QueryHandle : RealmHandle
    {
        // This is a delegate type meant to represent one of the "query operator" methods such as float_less and bool_equal
        internal delegate void Operation<T>(QueryHandle queryPtr, ColumnKey columnKey, T value);

        private static class NativeMethods
        {
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1121 // Use built-in type alias

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_binary_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void binary_equal(QueryHandle queryPtr, ColumnKey columnKey, IntPtr buffer, IntPtr bufferLength, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_binary_not_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void binary_not_equal(QueryHandle queryPtr, ColumnKey columnKey, IntPtr buffer, IntPtr bufferLength, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_string_contains", CallingConvention = CallingConvention.Cdecl)]
            public static extern void string_contains(QueryHandle queryPtr, ColumnKey columnKey,
                        [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen, [MarshalAs(UnmanagedType.I1)] bool caseSensitive, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_string_starts_with", CallingConvention = CallingConvention.Cdecl)]
            public static extern void string_starts_with(QueryHandle queryPtr, ColumnKey columnKey,
                        [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen, [MarshalAs(UnmanagedType.I1)] bool caseSensitive, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_string_ends_with", CallingConvention = CallingConvention.Cdecl)]
            public static extern void string_ends_with(QueryHandle queryPtr, ColumnKey columnKey,
                        [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen, [MarshalAs(UnmanagedType.I1)] bool caseSensitive, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_string_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void string_equal(QueryHandle queryPtr, ColumnKey columnKey,
                        [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen, [MarshalAs(UnmanagedType.I1)] bool caseSensitive, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_string_not_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void string_not_equal(QueryHandle queryPtr, ColumnKey columnKey,
                        [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen, [MarshalAs(UnmanagedType.I1)] bool caseSensitive, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_string_like", CallingConvention = CallingConvention.Cdecl)]
            public static extern void string_like(QueryHandle queryPtr, ColumnKey columnKey,
                        [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen, [MarshalAs(UnmanagedType.I1)] bool caseSensitive, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_bool_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void bool_equal(QueryHandle queryPtr, ColumnKey columnKey, IntPtr value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_bool_not_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void bool_not_equal(QueryHandle queryPtr, ColumnKey columnKey, IntPtr value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void int_equal(QueryHandle queryPtr, ColumnKey columnKey, IntPtr value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_not_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void int_not_equal(QueryHandle queryPtr, ColumnKey columnKey, IntPtr value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_less", CallingConvention = CallingConvention.Cdecl)]
            public static extern void int_less(QueryHandle queryPtr, ColumnKey columnKey, IntPtr value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_less_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void int_less_equal(QueryHandle queryPtr, ColumnKey columnKey, IntPtr value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_greater", CallingConvention = CallingConvention.Cdecl)]
            public static extern void int_greater(QueryHandle queryPtr, ColumnKey columnKey, IntPtr value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_greater_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void int_greater_equal(QueryHandle queryPtr, ColumnKey columnKey, IntPtr value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_long_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void long_equal(QueryHandle queryPtr, ColumnKey columnKey, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_long_not_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void long_not_equal(QueryHandle queryPtr, ColumnKey columnKey, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_long_less", CallingConvention = CallingConvention.Cdecl)]
            public static extern void long_less(QueryHandle queryPtr, ColumnKey columnKey, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_long_less_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void long_less_equal(QueryHandle queryPtr, ColumnKey columnKey, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_long_greater", CallingConvention = CallingConvention.Cdecl)]
            public static extern void long_greater(QueryHandle queryPtr, ColumnKey columnKey, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_long_greater_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void long_greater_equal(QueryHandle queryPtr, ColumnKey columnKey, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_float_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void float_equal(QueryHandle queryPtr, ColumnKey columnKey, Single value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_float_not_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void float_not_equal(QueryHandle queryPtr, ColumnKey columnKey, Single value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_float_less", CallingConvention = CallingConvention.Cdecl)]
            public static extern void float_less(QueryHandle queryPtr, ColumnKey columnKey, Single value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_float_less_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void float_less_equal(QueryHandle queryPtr, ColumnKey columnKey, Single value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_float_greater", CallingConvention = CallingConvention.Cdecl)]
            public static extern void float_greater(QueryHandle queryPtr, ColumnKey columnKey, Single value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_float_greater_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void float_greater_equal(QueryHandle queryPtr, ColumnKey columnKey, Single value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_double_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void double_equal(QueryHandle queryPtr, ColumnKey columnKey, Double value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_double_not_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void double_not_equal(QueryHandle queryPtr, ColumnKey columnKey, Double value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_double_less", CallingConvention = CallingConvention.Cdecl)]
            public static extern void double_less(QueryHandle queryPtr, ColumnKey columnKey, Double value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_double_less_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void double_less_equal(QueryHandle queryPtr, ColumnKey columnKey, Double value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_double_greater", CallingConvention = CallingConvention.Cdecl)]
            public static extern void double_greater(QueryHandle queryPtr, ColumnKey columnKey, Double value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_double_greater_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void double_greater_equal(QueryHandle queryPtr, ColumnKey columnKey, Double value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_timestamp_ticks_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void timestamp_ticks_equal(QueryHandle queryPtr, ColumnKey columnKey, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_timestamp_ticks_not_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void timestamp_ticks_not_equal(QueryHandle queryPtr, ColumnKey columnKey, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_timestamp_ticks_less", CallingConvention = CallingConvention.Cdecl)]
            public static extern void timestamp_ticks_less(QueryHandle queryPtr, ColumnKey columnKey, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_timestamp_ticks_less_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void timestamp_ticks_less_equal(QueryHandle queryPtr, ColumnKey columnKey, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_timestamp_ticks_greater", CallingConvention = CallingConvention.Cdecl)]
            public static extern void timestamp_ticks_greater(QueryHandle queryPtr, ColumnKey columnKey, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_timestamp_ticks_greater_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void timestamp_ticks_greater_equal(QueryHandle queryPtr, ColumnKey columnKey, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_object_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void query_object_equal(QueryHandle queryPtr, ColumnKey columnKey, ObjectHandle objectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_null_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void null_equal(QueryHandle queryPtr, ColumnKey columnKey, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_null_not_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void null_not_equal(QueryHandle queryPtr, ColumnKey columnKey, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_get_column_key", CallingConvention = CallingConvention.Cdecl)]
            public static extern void get_column_key(QueryHandle queryPtr,
                        [MarshalAs(UnmanagedType.LPWStr)] string columnName, IntPtr columnNameLen, out ColumnKey key, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_not", CallingConvention = CallingConvention.Cdecl)]
            public static extern void not(QueryHandle queryHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_group_begin", CallingConvention = CallingConvention.Cdecl)]
            public static extern void group_begin(QueryHandle queryHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_group_end", CallingConvention = CallingConvention.Cdecl)]
            public static extern void group_end(QueryHandle queryHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_or", CallingConvention = CallingConvention.Cdecl)]
            public static extern void or(QueryHandle queryHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr queryHandle);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_count", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr count(QueryHandle QueryHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_create_results", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr create_results(QueryHandle queryPtr, SharedRealmHandle sharedRealm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_create_sorted_results", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr create_sorted_results(QueryHandle queryPtr, SharedRealmHandle sharedRealm,
                [MarshalAs(UnmanagedType.LPArray), In] SortDescriptorBuilder.Clause.Marshalable[] sortClauses, IntPtr clauseCount,
                [MarshalAs(UnmanagedType.LPArray), In] IntPtr[] flattenedPropertyIndices,
                out NativeException ex);

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore SA1121 // Use built-in type alias
        }

        public QueryHandle(RealmHandle root, IntPtr handle) : base(root, handle)
        {
        }

        protected override void Unbind()
        {
            NativeMethods.destroy(handle);
        }

        public NumericQueryMethods NumericEqualMethods => new NumericQueryMethods(IntEqual, LongEqual, FloatEqual, DoubleEqual);

        public NumericQueryMethods NumericNotEqualMethods => new NumericQueryMethods(IntNotEqual, LongNotEqual, FloatNotEqual, DoubleNotEqual);

        public NumericQueryMethods NumericLessMethods => new NumericQueryMethods(IntLess, LongLess, FloatLess, DoubleLess);

        public NumericQueryMethods NumericLessEqualMethods => new NumericQueryMethods(IntLessEqual, LongLessEqual, FloatLessEqual, DoubleLessEqual);

        public NumericQueryMethods NumericGreaterMethods => new NumericQueryMethods(IntGreater, LongGreater, FloatGreater, DoubleGreater);

        public NumericQueryMethods NumericGreaterEqualMethods => new NumericQueryMethods(IntGreaterEqual, LongGreaterEqual, FloatGreaterEqual, DoubleGreaterEqual);

        public void BinaryEqual(ColumnKey columnKey, IntPtr buffer, IntPtr bufferLength)
        {
            NativeMethods.binary_equal(this, columnKey, buffer, bufferLength, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void BinaryNotEqual(ColumnKey columnKey, IntPtr buffer, IntPtr bufferLength)
        {
            NativeMethods.binary_not_equal(this, columnKey, buffer, bufferLength, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        /// <summary>
        /// If the user hasn't specified it, should be caseSensitive=true.
        /// </summary>
        public void StringContains(ColumnKey columnKey, string value, bool caseSensitive)
        {
            NativeMethods.string_contains(this, columnKey, value, (IntPtr)value.Length, caseSensitive, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        /// <summary>
        /// If the user hasn't specified it, should be <c>caseSensitive = true</c>.
        /// </summary>
        public void StringStartsWith(ColumnKey columnKey, string value, bool caseSensitive)
        {
            NativeMethods.string_starts_with(this, columnKey, value, (IntPtr)value.Length, caseSensitive, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        /// <summary>
        /// If the user hasn't specified it, should be <c>caseSensitive = true</c>.
        /// </summary>
        public void StringEndsWith(ColumnKey columnKey, string value, bool caseSensitive)
        {
            NativeMethods.string_ends_with(this, columnKey, value, (IntPtr)value.Length, caseSensitive, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        /// <summary>
        /// If the user hasn't specified it, should be <c>caseSensitive = true</c>.
        /// </summary>
        public void StringEqual(ColumnKey columnKey, string value, bool caseSensitive)
        {
            NativeMethods.string_equal(this, columnKey, value, (IntPtr)value.Length, caseSensitive, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        /// <summary>
        /// If the user hasn't specified it, should be <c>caseSensitive = true</c>.
        /// </summary>
        public void StringNotEqual(ColumnKey columnKey, string value, bool caseSensitive)
        {
            NativeMethods.string_not_equal(this, columnKey, value, (IntPtr)value.Length, caseSensitive, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void StringLike(ColumnKey columnKey, string value, bool caseSensitive)
        {
            NativeException nativeException;
            if (value == null)
            {
                NativeMethods.null_equal(this, columnKey, out nativeException);
            }
            else
            {
                NativeMethods.string_like(this, columnKey, value, (IntPtr)value.Length, caseSensitive, out nativeException);
            }

            nativeException.ThrowIfNecessary();
        }

        public void BoolEqual(ColumnKey columnKey, bool value)
        {
            NativeMethods.bool_equal(this, columnKey, MarshalHelpers.BoolToIntPtr(value), out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void BoolNotEqual(ColumnKey columnKey, bool value)
        {
            NativeMethods.bool_not_equal(this, columnKey, MarshalHelpers.BoolToIntPtr(value), out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void IntEqual(ColumnKey columnKey, int value)
        {
            NativeMethods.int_equal(this, columnKey, (IntPtr)value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void IntNotEqual(ColumnKey columnKey, int value)
        {
            NativeMethods.int_not_equal(this, columnKey, (IntPtr)value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void IntLess(ColumnKey columnKey, int value)
        {
            NativeMethods.int_less(this, columnKey, (IntPtr)value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void IntLessEqual(ColumnKey columnKey, int value)
        {
            NativeMethods.int_less_equal(this, columnKey, (IntPtr)value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void IntGreater(ColumnKey columnKey, int value)
        {
            NativeMethods.int_greater(this, columnKey, (IntPtr)value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void IntGreaterEqual(ColumnKey columnKey, int value)
        {
            NativeMethods.int_greater_equal(this, columnKey, (IntPtr)value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void LongEqual(ColumnKey columnKey, long value)
        {
            NativeMethods.long_equal(this, columnKey, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void LongNotEqual(ColumnKey columnKey, long value)
        {
            NativeMethods.long_not_equal(this, columnKey, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void LongLess(ColumnKey columnKey, long value)
        {
            NativeMethods.long_less(this, columnKey, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void LongLessEqual(ColumnKey columnKey, long value)
        {
            NativeMethods.long_less_equal(this, columnKey, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void LongGreater(ColumnKey columnKey, long value)
        {
            NativeMethods.long_greater(this, columnKey, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void LongGreaterEqual(ColumnKey columnKey, long value)
        {
            NativeMethods.long_greater_equal(this, columnKey, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void FloatEqual(ColumnKey columnKey, float value)
        {
            NativeMethods.float_equal(this, columnKey, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void FloatNotEqual(ColumnKey columnKey, float value)
        {
            NativeMethods.float_not_equal(this, columnKey, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void FloatLess(ColumnKey columnKey, float value)
        {
            NativeMethods.float_less(this, columnKey, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void FloatLessEqual(ColumnKey columnKey, float value)
        {
            NativeMethods.float_less_equal(this, columnKey, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void FloatGreater(ColumnKey columnKey, float value)
        {
            NativeMethods.float_greater(this, columnKey, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void FloatGreaterEqual(ColumnKey columnKey, float value)
        {
            NativeMethods.float_greater_equal(this, columnKey, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void DoubleEqual(ColumnKey columnKey, double value)
        {
            NativeMethods.double_equal(this, columnKey, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void DoubleNotEqual(ColumnKey columnKey, double value)
        {
            NativeMethods.double_not_equal(this, columnKey, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void DoubleLess(ColumnKey columnKey, double value)
        {
            NativeMethods.double_less(this, columnKey, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void DoubleLessEqual(ColumnKey columnKey, double value)
        {
            NativeMethods.double_less_equal(this, columnKey, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void DoubleGreater(ColumnKey columnKey, double value)
        {
            NativeMethods.double_greater(this, columnKey, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void DoubleGreaterEqual(ColumnKey columnKey, double value)
        {
            NativeMethods.double_greater_equal(this, columnKey, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void TimestampTicksEqual(ColumnKey columnKey, DateTimeOffset value)
        {
            NativeMethods.timestamp_ticks_equal(this, columnKey, value.ToUniversalTime().Ticks, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void TimestampTicksNotEqual(ColumnKey columnKey, DateTimeOffset value)
        {
            NativeMethods.timestamp_ticks_not_equal(this, columnKey, value.ToUniversalTime().Ticks, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void TimestampTicksLess(ColumnKey columnKey, DateTimeOffset value)
        {
            NativeMethods.timestamp_ticks_less(this, columnKey, value.ToUniversalTime().Ticks, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void TimestampTicksLessEqual(ColumnKey columnKey, DateTimeOffset value)
        {
            NativeMethods.timestamp_ticks_less_equal(this, columnKey, value.ToUniversalTime().Ticks, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void TimestampTicksGreater(ColumnKey columnKey, DateTimeOffset value)
        {
            NativeMethods.timestamp_ticks_greater(this, columnKey, value.ToUniversalTime().Ticks, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void TimestampTicksGreaterEqual(ColumnKey columnKey, DateTimeOffset value)
        {
            NativeMethods.timestamp_ticks_greater_equal(this, columnKey, value.ToUniversalTime().Ticks, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void ObjectEqual(ColumnKey columnKey, ObjectHandle objectHandle)
        {
            NativeMethods.query_object_equal(this, columnKey, objectHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void NullEqual(ColumnKey columnKey)
        {
            NativeMethods.null_equal(this, columnKey, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void NullNotEqual(ColumnKey columnKey)
        {
            NativeMethods.null_not_equal(this, columnKey, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public ColumnKey GetColumnKey(string columnName)
        {
            NativeMethods.get_column_key(this, columnName, (IntPtr)columnName.Length, out var result, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public void Not()
        {
            NativeMethods.not(this, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void GroupBegin()
        {
            NativeMethods.group_begin(this, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void GroupEnd()
        {
            NativeMethods.group_end(this, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Or()
        {
            NativeMethods.or(this, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public int Count()
        {
            var result = NativeMethods.count(this, out var nativeException);
            nativeException.ThrowIfNecessary();
            return (int)result;
        }

        public ResultsHandle CreateResults(SharedRealmHandle sharedRealm)
        {
            var result = NativeMethods.create_results(this, sharedRealm, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new ResultsHandle(sharedRealm, result);
        }

        public ResultsHandle CreateSortedResults(SharedRealmHandle sharedRealm, SortDescriptorBuilder sortDescriptorBuilder)
        {
            var marshaledValues = sortDescriptorBuilder.Flatten();
            var result = NativeMethods.create_sorted_results(this, sharedRealm, marshaledValues.Item2, (IntPtr)marshaledValues.Item2.Length, marshaledValues.Item1, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new ResultsHandle(sharedRealm, result);
        }

        public struct NumericQueryMethods
        {
            public readonly Action<ColumnKey, int> Int;

            public readonly Action<ColumnKey, long> Long;

            public readonly Action<ColumnKey, float> Float;

            public readonly Action<ColumnKey, double> Double;

            public NumericQueryMethods(Action<ColumnKey, int> intQuery, Action<ColumnKey, long> longQuery,
                Action<ColumnKey, float> floatQuery, Action<ColumnKey, double> doubleQuery)
            {
                Int = intQuery;
                Long = longQuery;
                Float = floatQuery;
                Double = doubleQuery;
            }
        }
    }
}
