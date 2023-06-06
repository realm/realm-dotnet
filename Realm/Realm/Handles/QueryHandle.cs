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
        internal delegate void Operation<T>(QueryHandle queryPtr, SharedRealmHandle realm, IntPtr propertyIndex, T value);

        private static class NativeMethods
        {
#pragma warning disable IDE1006 // Naming Styles

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_string_contains", CallingConvention = CallingConvention.Cdecl)]
            public static extern void string_contains(QueryHandle queryPtr, SharedRealmHandle realm, IntPtr property_ndx,
                PrimitiveValue primitive, [MarshalAs(UnmanagedType.U1)] bool caseSensitive, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_string_starts_with", CallingConvention = CallingConvention.Cdecl)]
            public static extern void string_starts_with(QueryHandle queryPtr, SharedRealmHandle realm, IntPtr property_ndx,
                        PrimitiveValue primitive, [MarshalAs(UnmanagedType.U1)] bool caseSensitive, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_string_ends_with", CallingConvention = CallingConvention.Cdecl)]
            public static extern void string_ends_with(QueryHandle queryPtr, SharedRealmHandle realm, IntPtr property_ndx,
                        PrimitiveValue primitive, [MarshalAs(UnmanagedType.U1)] bool caseSensitive, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_string_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void string_equal(QueryHandle queryPtr, SharedRealmHandle realm, IntPtr property_ndx,
                        PrimitiveValue primitive, [MarshalAs(UnmanagedType.U1)] bool caseSensitive, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_string_not_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void string_not_equal(QueryHandle queryPtr, SharedRealmHandle realm, IntPtr property_ndx,
                        PrimitiveValue primitive, [MarshalAs(UnmanagedType.U1)] bool caseSensitive, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_string_like", CallingConvention = CallingConvention.Cdecl)]
            public static extern void string_like(QueryHandle queryPtr, SharedRealmHandle realm, IntPtr property_ndx,
                        PrimitiveValue primitive, [MarshalAs(UnmanagedType.U1)] bool caseSensitive, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_string_fts", CallingConvention = CallingConvention.Cdecl)]
            public static extern void string_fts(QueryHandle queryPtr, SharedRealmHandle realm, IntPtr property_ndx,
                PrimitiveValue primitive, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_primitive_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void primitive_equal(QueryHandle queryPtr, SharedRealmHandle realm, IntPtr property_ndx, PrimitiveValue primitive, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_primitive_not_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void primitive_not_equal(QueryHandle queryPtr, SharedRealmHandle realm, IntPtr property_ndx, PrimitiveValue primitive, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_primitive_less", CallingConvention = CallingConvention.Cdecl)]
            public static extern void primitive_less(QueryHandle queryPtr, SharedRealmHandle realm, IntPtr property_ndx, PrimitiveValue primitive, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_primitive_less_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void primitive_less_equal(QueryHandle queryPtr, SharedRealmHandle realm, IntPtr property_ndx, PrimitiveValue primitive, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_primitive_greater", CallingConvention = CallingConvention.Cdecl)]
            public static extern void primitive_greater(QueryHandle queryPtr, SharedRealmHandle realm, IntPtr property_ndx, PrimitiveValue primitive, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_primitive_greater_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void primitive_greater_equal(QueryHandle queryPtr, SharedRealmHandle realm, IntPtr property_ndx, PrimitiveValue primitive, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_null_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void null_equal(QueryHandle queryPtr, SharedRealmHandle realm, IntPtr property_ndx, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_null_not_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void null_not_equal(QueryHandle queryPtr, SharedRealmHandle realm, IntPtr property_ndx, out NativeException ex);

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
            public static extern IntPtr count(QueryHandle QueryHandle, SortDescriptorHandle sortDescriptor, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_create_results", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr create_results(QueryHandle queryPtr, SharedRealmHandle sharedRealm, SortDescriptorHandle sortDescriptor, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_realm_value_type_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void realm_value_type_equal(QueryHandle queryPtr, SharedRealmHandle realm, IntPtr property_ndx, RealmValueType realm_value_type, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_realm_value_type_not_equal", CallingConvention = CallingConvention.Cdecl)]
            public static extern void realm_value_type_not_equal(QueryHandle queryPtr, SharedRealmHandle realm, IntPtr property_ndx, RealmValueType realm_value_type, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_geowithin", CallingConvention = CallingConvention.Cdecl)]
            public static extern void query_geowithin(QueryHandle queryPtr, SharedRealmHandle realm, IntPtr property_ndx, NativeQueryArgument geo_value, out NativeException ex);

#pragma warning restore IDE1006 // Naming Styles
        }

        public QueryHandle(SharedRealmHandle root, IntPtr handle) : base(root, handle)
        {
        }

        public override void Unbind() => NativeMethods.destroy(handle);

        /// <summary>
        /// If the user hasn't specified it, should be caseSensitive=true.
        /// </summary>
        public void StringContains(SharedRealmHandle realm, IntPtr propertyIndex, in RealmValue value, bool caseSensitive)
        {
            EnsureIsOpen();

            var (primitive, handles) = value.ToNative();
            NativeMethods.string_contains(this, realm, propertyIndex, primitive, caseSensitive, out var nativeException);
            handles?.Dispose();
            nativeException.ThrowIfNecessary();
        }

        /// <summary>
        /// If the user hasn't specified it, should be <c>caseSensitive = true</c>.
        /// </summary>
        public void StringStartsWith(SharedRealmHandle realm, IntPtr propertyIndex, in RealmValue value, bool caseSensitive)
        {
            EnsureIsOpen();

            var (primitive, handles) = value.ToNative();
            NativeMethods.string_starts_with(this, realm, propertyIndex, primitive, caseSensitive, out var nativeException);
            handles?.Dispose();
            nativeException.ThrowIfNecessary();
        }

        /// <summary>
        /// If the user hasn't specified it, should be <c>caseSensitive = true</c>.
        /// </summary>
        public void StringEndsWith(SharedRealmHandle realm, IntPtr propertyIndex, in RealmValue value, bool caseSensitive)
        {
            EnsureIsOpen();

            var (primitive, handles) = value.ToNative();
            NativeMethods.string_ends_with(this, realm, propertyIndex, primitive, caseSensitive, out var nativeException);
            handles?.Dispose();
            nativeException.ThrowIfNecessary();
        }

        /// <summary>
        /// If the user hasn't specified it, should be <c>caseSensitive = true</c>.
        /// </summary>
        public void StringEqual(SharedRealmHandle realm, IntPtr propertyIndex, in RealmValue value, bool caseSensitive)
        {
            EnsureIsOpen();

            var (primitive, handles) = value.ToNative();
            NativeMethods.string_equal(this, realm, propertyIndex, primitive, caseSensitive, out var nativeException);
            handles?.Dispose();
            nativeException.ThrowIfNecessary();
        }

        /// <summary>
        /// If the user hasn't specified it, should be <c>caseSensitive = true</c>.
        /// </summary>
        public void StringNotEqual(SharedRealmHandle realm, IntPtr propertyIndex, in RealmValue value, bool caseSensitive)
        {
            EnsureIsOpen();

            var (primitive, handles) = value.ToNative();
            NativeMethods.string_not_equal(this, realm, propertyIndex, primitive, caseSensitive, out var nativeException);
            handles?.Dispose();
            nativeException.ThrowIfNecessary();
        }

        public void StringLike(SharedRealmHandle realm, IntPtr propertyIndex, in RealmValue value, bool caseSensitive)
        {
            EnsureIsOpen();

            NativeException nativeException;
            if (value.Type == RealmValueType.Null)
            {
                NativeMethods.null_equal(this, realm, propertyIndex, out nativeException);
            }
            else
            {
                var (primitive, handles) = value.ToNative();
                NativeMethods.string_like(this, realm, propertyIndex, primitive, caseSensitive, out nativeException);
                handles?.Dispose();
            }

            nativeException.ThrowIfNecessary();
        }

        public void StringFTS(SharedRealmHandle realm, IntPtr propertyIndex, in RealmValue value)
        {
            EnsureIsOpen();

            var (primitive, handles) = value.ToNative();
            NativeMethods.string_fts(this, realm, propertyIndex, primitive, out var ex);
            handles?.Dispose();

            ex.ThrowIfNecessary();
        }

        public void ValueEqual(SharedRealmHandle realm, IntPtr propertyIndex, in RealmValue value)
        {
            EnsureIsOpen();

            var (primitive, handles) = value.ToNative();
            NativeMethods.primitive_equal(this, realm, propertyIndex, primitive, out var nativeException);
            handles?.Dispose();
            nativeException.ThrowIfNecessary();
        }

        public void ValueNotEqual(SharedRealmHandle realm, IntPtr propertyIndex, in RealmValue value)
        {
            EnsureIsOpen();

            var (primitive, handles) = value.ToNative();
            NativeMethods.primitive_not_equal(this, realm, propertyIndex, primitive, out var nativeException);
            handles?.Dispose();
            nativeException.ThrowIfNecessary();
        }

        public void ValueLess(SharedRealmHandle realm, IntPtr propertyIndex, in RealmValue value)
        {
            EnsureIsOpen();

            var (primitive, handles) = value.ToNative();
            NativeMethods.primitive_less(this, realm, propertyIndex, primitive, out var nativeException);
            handles?.Dispose();
            nativeException.ThrowIfNecessary();
        }

        public void ValueLessEqual(SharedRealmHandle realm, IntPtr propertyIndex, in RealmValue value)
        {
            EnsureIsOpen();

            var (primitive, handles) = value.ToNative();
            NativeMethods.primitive_less_equal(this, realm, propertyIndex, primitive, out var nativeException);
            handles?.Dispose();
            nativeException.ThrowIfNecessary();
        }

        public void ValueGreater(SharedRealmHandle realm, IntPtr propertyIndex, in RealmValue value)
        {
            EnsureIsOpen();

            var (primitive, handles) = value.ToNative();
            NativeMethods.primitive_greater(this, realm, propertyIndex, primitive, out var nativeException);
            handles?.Dispose();
            nativeException.ThrowIfNecessary();
        }

        public void ValueGreaterEqual(SharedRealmHandle realm, IntPtr propertyIndex, in RealmValue value)
        {
            EnsureIsOpen();

            var (primitive, handles) = value.ToNative();
            NativeMethods.primitive_greater_equal(this, realm, propertyIndex, primitive, out var nativeException);
            handles?.Dispose();
            nativeException.ThrowIfNecessary();
        }

        public void NullEqual(SharedRealmHandle realm, IntPtr propertyIndex)
        {
            EnsureIsOpen();

            NativeMethods.null_equal(this, realm, propertyIndex, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void NullNotEqual(SharedRealmHandle realm, IntPtr propertyIndex)
        {
            EnsureIsOpen();

            NativeMethods.null_not_equal(this, realm, propertyIndex, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void RealmValueTypeEqual(SharedRealmHandle realm, IntPtr propertyIndex, RealmValueType type)
        {
            EnsureIsOpen();

            NativeMethods.realm_value_type_equal(this, realm, propertyIndex, type, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void RealmValueTypeNotEqual(SharedRealmHandle realm, IntPtr propertyIndex, RealmValueType type)
        {
            EnsureIsOpen();

            NativeMethods.realm_value_type_not_equal(this, realm, propertyIndex, type, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Not()
        {
            EnsureIsOpen();

            NativeMethods.not(this, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void GroupBegin()
        {
            EnsureIsOpen();

            NativeMethods.group_begin(this, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void GroupEnd()
        {
            EnsureIsOpen();

            NativeMethods.group_end(this, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Or()
        {
            EnsureIsOpen();

            NativeMethods.or(this, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public int Count(SortDescriptorHandle sortDescriptor)
        {
            EnsureIsOpen();

            var result = NativeMethods.count(this, sortDescriptor, out var nativeException);
            nativeException.ThrowIfNecessary();
            return (int)result;
        }

        public void GeoWithin(SharedRealmHandle realm, IntPtr propertyIndex, GeoShapeBase value)
        {
            EnsureIsOpen();

            QueryArgument arg = value;

            var (nativeArg, handlesToCleanup) = arg.ToNative();
            NativeMethods.query_geowithin(this, realm, propertyIndex, nativeArg, out var nativeException);
            handlesToCleanup?.Dispose();

            nativeException.ThrowIfNecessary();
        }

        public ResultsHandle CreateResults(SharedRealmHandle sharedRealm, SortDescriptorHandle sortDescriptor)
        {
            EnsureIsOpen();

            var result = NativeMethods.create_results(this, sharedRealm, sortDescriptor, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new ResultsHandle(sharedRealm, result);
        }
    }
}
