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
using System.Diagnostics.CodeAnalysis;
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
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented")]
    internal class QueryHandle : RealmHandle
    {
        // This is a delegate type meant to represent one of the "query operator" methods such as float_less and bool_equal
        internal delegate void Operation<T>(QueryHandle queryPtr, IntPtr columnIndex, T value);

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1121:UseBuiltInTypeAlias")]
        private static class NativeMethods
        {
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_binary_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void binary_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr buffer, IntPtr bufferLength, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_binary_not_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void binary_not_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr buffer, IntPtr bufferLength, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_string_contains", CallingConvention = CallingConvention.Cdecl)]
            public static extern void string_contains(QueryHandle queryPtr, IntPtr columnIndex,
                        [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen, [MarshalAs(UnmanagedType.I1)] bool caseSensitive, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_string_starts_with", CallingConvention = CallingConvention.Cdecl)]
            public static extern void string_starts_with(QueryHandle queryPtr, IntPtr columnIndex,
                        [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen, [MarshalAs(UnmanagedType.I1)] bool caseSensitive, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_string_ends_with", CallingConvention = CallingConvention.Cdecl)]
            public static extern void string_ends_with(QueryHandle queryPtr, IntPtr columnIndex,
                        [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen, [MarshalAs(UnmanagedType.I1)] bool caseSensitive, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_string_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void string_equal(QueryHandle queryPtr, IntPtr columnIndex,
                        [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen, [MarshalAs(UnmanagedType.I1)] bool caseSensitive, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_string_not_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void string_not_equal(QueryHandle queryPtr, IntPtr columnIndex,
                        [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen, [MarshalAs(UnmanagedType.I1)] bool caseSensitive, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_bool_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void bool_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_bool_not_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void bool_not_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void int_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_not_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void int_not_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_less", CallingConvention = CallingConvention.Cdecl)]
            public static extern void int_less(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_less_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void int_less_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_greater", CallingConvention = CallingConvention.Cdecl)]
            public static extern void int_greater(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_greater_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void int_greater_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_long_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void long_equal(QueryHandle queryPtr, IntPtr columnIndex, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_long_not_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void long_not_equal(QueryHandle queryPtr, IntPtr columnIndex, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_long_less", CallingConvention = CallingConvention.Cdecl)]
            public static extern void long_less(QueryHandle queryPtr, IntPtr columnIndex, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_long_less_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void long_less_equal(QueryHandle queryPtr, IntPtr columnIndex, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_long_greater", CallingConvention = CallingConvention.Cdecl)]
            public static extern void long_greater(QueryHandle queryPtr, IntPtr columnIndex, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_long_greater_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void long_greater_equal(QueryHandle queryPtr, IntPtr columnIndex, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_float_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void float_equal(QueryHandle queryPtr, IntPtr columnIndex, Single value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_float_not_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void float_not_equal(QueryHandle queryPtr, IntPtr columnIndex, Single value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_float_less", CallingConvention = CallingConvention.Cdecl)]
            public static extern void float_less(QueryHandle queryPtr, IntPtr columnIndex, Single value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_float_less_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void float_less_equal(QueryHandle queryPtr, IntPtr columnIndex, Single value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_float_greater", CallingConvention = CallingConvention.Cdecl)]
            public static extern void float_greater(QueryHandle queryPtr, IntPtr columnIndex, Single value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_float_greater_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void float_greater_equal(QueryHandle queryPtr, IntPtr columnIndex, Single value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_double_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void double_equal(QueryHandle queryPtr, IntPtr columnIndex, Double value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_double_not_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void double_not_equal(QueryHandle queryPtr, IntPtr columnIndex, Double value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_double_less", CallingConvention = CallingConvention.Cdecl)]
            public static extern void double_less(QueryHandle queryPtr, IntPtr columnIndex, Double value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_double_less_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void double_less_equal(QueryHandle queryPtr, IntPtr columnIndex, Double value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_double_greater", CallingConvention = CallingConvention.Cdecl)]
            public static extern void double_greater(QueryHandle queryPtr, IntPtr columnIndex, Double value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_double_greater_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void double_greater_equal(QueryHandle queryPtr, IntPtr columnIndex, Double value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_timestamp_ticks_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void timestamp_ticks_equal(QueryHandle queryPtr, IntPtr columnIndex, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_timestamp_ticks_not_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void timestamp_ticks_not_equal(QueryHandle queryPtr, IntPtr columnIndex, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_timestamp_ticks_less", CallingConvention = CallingConvention.Cdecl)]
            public static extern void timestamp_ticks_less(QueryHandle queryPtr, IntPtr columnIndex, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_timestamp_ticks_less_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void timestamp_ticks_less_equal(QueryHandle queryPtr, IntPtr columnIndex, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_timestamp_ticks_greater", CallingConvention = CallingConvention.Cdecl)]
            public static extern void timestamp_ticks_greater(QueryHandle queryPtr, IntPtr columnIndex, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_timestamp_ticks_greater_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void timestamp_ticks_greater_equal(QueryHandle queryPtr, IntPtr columnIndex, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_object_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void query_object_equal(QueryHandle queryPtr, IntPtr columnIndex, ObjectHandle objectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_null_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void null_equal(QueryHandle queryPtr, IntPtr columnIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_null_not_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void null_not_equal(QueryHandle queryPtr, IntPtr columnIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_find", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr findDirect(QueryHandle queryHandle, IntPtr beginAtIndex, SharedRealmHandle realmHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_find_next", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr findNext(QueryHandle queryHandle, ObjectHandle previousObject, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_get_column_index", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_column_index(QueryHandle queryPtr,
                        [MarshalAs(UnmanagedType.LPWStr)] string columnName, IntPtr columnNameLen, out NativeException ex);

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
            public static extern IntPtr create_sorted_results(QueryHandle queryPtr, SharedRealmHandle sharedRealm, TableHandle tablePtr,
                [MarshalAs(UnmanagedType.LPArray), In]SortDescriptorBuilder.Clause.Marshalable[] sortClauses, IntPtr clauseCount,
                [MarshalAs(UnmanagedType.LPArray), In]IntPtr[] flattenedPropertyIndices,
                out NativeException ex);
        }

        public QueryHandle(RealmHandle root) : base(root)
        {
        }

        protected override void Unbind()
        {
            NativeMethods.destroy(handle);
        }

        public void BinaryEqual(IntPtr columnIndex, IntPtr buffer, IntPtr bufferLength)
        {
            NativeException nativeException;
            NativeMethods.binary_equal(this, columnIndex, buffer, bufferLength, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void BinaryNotEqual(IntPtr columnIndex, IntPtr buffer, IntPtr bufferLength)
        {
            NativeException nativeException;
            NativeMethods.binary_not_equal(this, columnIndex, buffer, bufferLength, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        /// <summary>
        /// If the user hasn't specified it, should be caseSensitive=true.
        /// </summary>
        public void StringContains(IntPtr columnIndex, string value, bool caseSensitive)
        {
            NativeException nativeException;
            NativeMethods.string_contains(this, columnIndex, value, (IntPtr)value.Length, caseSensitive, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        /// <summary>
        /// If the user hasn't specified it, should be <c>caseSensitive = true</c>.
        /// </summary>
        public void StringStartsWith(IntPtr columnIndex, string value, bool caseSensitive)
        {
            NativeException nativeException;
            NativeMethods.string_starts_with(this, columnIndex, value, (IntPtr)value.Length, caseSensitive, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        /// <summary>
        /// If the user hasn't specified it, should be <c>caseSensitive = true</c>.
        /// </summary>
        public void StringEndsWith(IntPtr columnIndex, string value, bool caseSensitive)
        {
            NativeException nativeException;
            NativeMethods.string_ends_with(this, columnIndex, value, (IntPtr)value.Length, caseSensitive, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        /// <summary>
        /// If the user hasn't specified it, should be <c>caseSensitive = true</c>.
        /// </summary>
        public void StringEqual(IntPtr columnIndex, string value, bool caseSensitive)
        {
            NativeException nativeException;
            NativeMethods.string_equal(this, columnIndex, value, (IntPtr)value.Length, caseSensitive, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        /// <summary>
        /// If the user hasn't specified it, should be <c>caseSensitive = true</c>.
        /// </summary>
        public void StringNotEqual(IntPtr columnIndex, string value, bool caseSensitive)
        {
            NativeException nativeException;
            NativeMethods.string_not_equal(this, columnIndex, value, (IntPtr)value.Length, caseSensitive, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void BoolEqual(IntPtr columnIndex, bool value)
        {
            NativeException nativeException;
            NativeMethods.bool_equal(this, columnIndex, MarshalHelpers.BoolToIntPtr(value), out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void BoolNotEqual(IntPtr columnIndex, bool value)
        {
            NativeException nativeException;
            NativeMethods.bool_not_equal(this, columnIndex, MarshalHelpers.BoolToIntPtr(value), out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void IntEqual(IntPtr columnIndex, int value)
        {
            NativeException nativeException;
            NativeMethods.int_equal(this, columnIndex, (IntPtr)value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void IntNotEqual(IntPtr columnIndex, int value)
        {
            NativeException nativeException;
            NativeMethods.int_not_equal(this, columnIndex, (IntPtr)value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void IntLess(IntPtr columnIndex, int value)
        {
            NativeException nativeException;
            NativeMethods.int_less(this, columnIndex, (IntPtr)value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void IntLessEqual(IntPtr columnIndex, int value)
        {
            NativeException nativeException;
            NativeMethods.int_less_equal(this, columnIndex, (IntPtr)value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void IntGreater(IntPtr columnIndex, int value)
        {
            NativeException nativeException;
            NativeMethods.int_greater(this, columnIndex, (IntPtr)value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void IntGreaterEqual(IntPtr columnIndex, int value)
        {
            NativeException nativeException;
            NativeMethods.int_greater_equal(this, columnIndex, (IntPtr)value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void LongEqual(IntPtr columnIndex, long value)
        {
            NativeException nativeException;
            NativeMethods.long_equal(this, columnIndex, value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void LongNotEqual(IntPtr columnIndex, long value)
        {
            NativeException nativeException;
            NativeMethods.long_not_equal(this, columnIndex, value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void LongLess(IntPtr columnIndex, long value)
        {
            NativeException nativeException;
            NativeMethods.long_less(this, columnIndex, value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void LongLessEqual(IntPtr columnIndex, long value)
        {
            NativeException nativeException;
            NativeMethods.long_less_equal(this, columnIndex, value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void LongGreater(IntPtr columnIndex, long value)
        {
            NativeException nativeException;
            NativeMethods.long_greater(this, columnIndex, value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void LongGreaterEqual(IntPtr columnIndex, long value)
        {
            NativeException nativeException;
            NativeMethods.long_greater_equal(this, columnIndex, value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void FloatEqual(IntPtr columnIndex, float value)
        {
            NativeException nativeException;
            NativeMethods.float_equal(this, columnIndex, value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void FloatNotEqual(IntPtr columnIndex, float value)
        {
            NativeException nativeException;
            NativeMethods.float_not_equal(this, columnIndex, value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void FloatLess(IntPtr columnIndex, float value)
        {
            NativeException nativeException;
            NativeMethods.float_less(this, columnIndex, value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void FloatLessEqual(IntPtr columnIndex, float value)
        {
            NativeException nativeException;
            NativeMethods.float_less_equal(this, columnIndex, value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void FloatGreater(IntPtr columnIndex, float value)
        {
            NativeException nativeException;
            NativeMethods.float_greater(this, columnIndex, value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void FloatGreaterEqual(IntPtr columnIndex, float value)
        {
            NativeException nativeException;
            NativeMethods.float_greater_equal(this, columnIndex, value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void DoubleEqual(IntPtr columnIndex, double value)
        {
            NativeException nativeException;
            NativeMethods.double_equal(this, columnIndex, value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void DoubleNotEqual(IntPtr columnIndex, double value)
        {
            NativeException nativeException;
            NativeMethods.double_not_equal(this, columnIndex, value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void DoubleLess(IntPtr columnIndex, double value)
        {
            NativeException nativeException;
            NativeMethods.double_less(this, columnIndex, value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void DoubleLessEqual(IntPtr columnIndex, double value)
        {
            NativeException nativeException;
            NativeMethods.double_less_equal(this, columnIndex, value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void DoubleGreater(IntPtr columnIndex, double value)
        {
            NativeException nativeException;
            NativeMethods.double_greater(this, columnIndex, value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void DoubleGreaterEqual(IntPtr columnIndex, double value)
        {
            NativeException nativeException;
            NativeMethods.double_greater_equal(this, columnIndex, value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void TimestampTicksEqual(IntPtr columnIndex, DateTimeOffset value)
        {
            NativeException nativeException;
            NativeMethods.timestamp_ticks_equal(this, columnIndex, value.ToUniversalTime().Ticks, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void TimestampTicksNotEqual(IntPtr columnIndex, DateTimeOffset value)
        {
            NativeException nativeException;
            NativeMethods.timestamp_ticks_not_equal(this, columnIndex, value.ToUniversalTime().Ticks, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void TimestampTicksLess(IntPtr columnIndex, DateTimeOffset value)
        {
            NativeException nativeException;
            NativeMethods.timestamp_ticks_less(this, columnIndex, value.ToUniversalTime().Ticks, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void TimestampTicksLessEqual(IntPtr columnIndex, DateTimeOffset value)
        {
            NativeException nativeException;
            NativeMethods.timestamp_ticks_less_equal(this, columnIndex, value.ToUniversalTime().Ticks, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void TimestampTicksGreater(IntPtr columnIndex, DateTimeOffset value)
        {
            NativeException nativeException;
            NativeMethods.timestamp_ticks_greater(this, columnIndex, value.ToUniversalTime().Ticks, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void TimestampTicksGreaterEqual(IntPtr columnIndex, DateTimeOffset value)
        {
            NativeException nativeException;
            NativeMethods.timestamp_ticks_greater_equal(this, columnIndex, value.ToUniversalTime().Ticks, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void ObjectEqual(IntPtr columnIndex, ObjectHandle objectHandle)
        {
            NativeException nativeException;
            NativeMethods.query_object_equal(this, columnIndex, objectHandle, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void NullEqual(IntPtr columnIndex)
        {
            NativeException nativeException;
            NativeMethods.null_equal(this, columnIndex, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void NullNotEqual(IntPtr columnIndex)
        {
            NativeException nativeException;
            NativeMethods.null_not_equal(this, columnIndex, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public IntPtr FindNext(ObjectHandle afterObject)
        {
            NativeException nativeException;
            var result = NativeMethods.findNext(this, afterObject, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public IntPtr FindDirect(SharedRealmHandle sharedRealm, IntPtr? beginAtIndex = null)
        {
            NativeException nativeException;
            var result = NativeMethods.findDirect(this, beginAtIndex ?? IntPtr.Zero, sharedRealm, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public IntPtr GetColumnIndex(string columnName)
        {
            NativeException nativeException;
            var result = NativeMethods.get_column_index(this, columnName, (IntPtr)columnName.Length, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public void Not()
        {
            NativeException nativeException;
            NativeMethods.not(this, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void GroupBegin()
        {
            NativeException nativeException;
            NativeMethods.group_begin(this, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void GroupEnd()
        {
            NativeException nativeException;
            NativeMethods.group_end(this, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Or()
        {
            NativeException nativeException;
            NativeMethods.or(this, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public int Count()
        {
            NativeException nativeException;
            var result = NativeMethods.count(this, out nativeException);
            nativeException.ThrowIfNecessary();
            return (int)result;
        }

        public IntPtr CreateResults(SharedRealmHandle sharedRealm)
        {
            NativeException nativeException;
            var result = NativeMethods.create_results(this, sharedRealm, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public IntPtr CreateSortedResults(SharedRealmHandle sharedRealm, SortDescriptorBuilder sortDescriptorBuilder)
        {
            NativeException nativeException;
            var marshaledValues = sortDescriptorBuilder.Flatten();
            var result = NativeMethods.create_sorted_results(this, sharedRealm, sortDescriptorBuilder.TableHandle, marshaledValues.Item2, (IntPtr)marshaledValues.Item2.Length, marshaledValues.Item1, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }
    }
}
