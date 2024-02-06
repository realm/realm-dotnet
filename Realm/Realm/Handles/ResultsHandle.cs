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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Realms.Helpers;
using Realms.Native;

namespace Realms
{
    internal class ResultsHandle : CollectionHandleBase
    {
        private static class NativeMethods
        {
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr resultsHandle);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_get_value", CallingConvention = CallingConvention.Cdecl)]
            public static extern void get_value(ResultsHandle results, IntPtr link_ndx, out PrimitiveValue value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_count", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr count(ResultsHandle results, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_clear", CallingConvention = CallingConvention.Cdecl)]
            public static extern void clear(ResultsHandle results, SharedRealmHandle realmHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_add_notification_callback", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr add_notification_callback(ResultsHandle results, IntPtr managedResultsHandle,
                KeyPathsCollectionType type, IntPtr callback, StringValue[] keypaths, IntPtr keypaths_len, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_get_query", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_query(ResultsHandle results, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_get_is_valid", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool get_is_valid(ResultsHandle results, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_get_thread_safe_reference", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_thread_safe_reference(ResultsHandle results, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_snapshot", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr snapshot(ResultsHandle results, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_get_filtered_results", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_filtered_results(ResultsHandle results,
                [MarshalAs(UnmanagedType.LPWStr)] string query_buf, IntPtr query_len,
                [MarshalAs(UnmanagedType.LPArray), In] NativeQueryArgument[] arguments, IntPtr args_count,
                out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_find_value", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr find_value(ResultsHandle results, PrimitiveValue value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_get_descriptor_ordering", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_sort_descriptor(ResultsHandle results, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_freeze", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr freeze(ResultsHandle handle, SharedRealmHandle frozen_realm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_get_description", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_description(ResultsHandle resultsHandle, IntPtr buffer, IntPtr bufferLength, out NativeException ex);
        }

        public override bool IsValid
        {
            get
            {
                if (IsClosed || Root?.IsClosed == true)
                {
                    return false;
                }

                var result = NativeMethods.get_is_valid(this, out var nativeException);
                nativeException.ThrowIfNecessary();
                return result;
            }
        }

        public override bool CanSnapshot => true;

        public string Description
        {
            get
            {
                EnsureIsOpen();

                return MarshalHelpers.GetString((IntPtr buffer, IntPtr bufferLength, out bool isNull, out NativeException ex) =>
                {
                    isNull = false;
                    return NativeMethods.get_description(this, buffer, bufferLength, out ex);
                })!;
            }
        }

        [Preserve]
        public ResultsHandle(SharedRealmHandle root, IntPtr handle) : base(root, handle)
        {
        }

        public RealmValue GetValueAtIndex(int index, Realm realm)
        {
            EnsureIsOpen();

            NativeMethods.get_value(this, (IntPtr)index, out var result, out var ex);
            ex.ThrowIfNecessary();
            return new RealmValue(result, realm);
        }

        public override int Count()
        {
            EnsureIsOpen();

            var result = NativeMethods.count(this, out var nativeException);
            nativeException.ThrowIfNecessary();
            return (int)result;
        }

        public void Clear(SharedRealmHandle realmHandle)
        {
            EnsureIsOpen();

            NativeMethods.clear(this, realmHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public QueryHandle GetQuery()
        {
            EnsureIsOpen();

            var result = NativeMethods.get_query(this, out var nativeException);
            nativeException.ThrowIfNecessary();

            return new QueryHandle(Root!, result);
        }

        public SortDescriptorHandle GetSortDescriptor()
        {
            EnsureIsOpen();

            var result = NativeMethods.get_sort_descriptor(this, out var nativeException);
            nativeException.ThrowIfNecessary();

            return new SortDescriptorHandle(Root!, result);
        }

        public override NotificationTokenHandle AddNotificationCallback(IntPtr managedObjectHandle,
            KeyPathsCollection keyPathsCollection, IntPtr callback)
        {
            EnsureIsOpen();

            using Arena arena = new();
            var nativeKeyPathsArray = keyPathsCollection.GetStrings().Select(p => StringValue.AllocateFrom(p, arena)).ToArray();

            var result = NativeMethods.add_notification_callback(this, managedObjectHandle,
                keyPathsCollection.Type, callback, nativeKeyPathsArray, (IntPtr)nativeKeyPathsArray.Length, out var nativeException);
            nativeException.ThrowIfNecessary();

            return new NotificationTokenHandle(Root!, result);
        }

        public override ThreadSafeReferenceHandle GetThreadSafeReference()
        {
            EnsureIsOpen();

            var result = NativeMethods.get_thread_safe_reference(this, out var nativeException);
            nativeException.ThrowIfNecessary();

            return new ThreadSafeReferenceHandle(result);
        }

        public int Find(in RealmValue value)
        {
            EnsureIsOpen();

            var (primitive, handles) = value.ToNative();
            var result = NativeMethods.find_value(this, primitive, out var nativeException);
            handles?.Dispose();
            nativeException.ThrowIfNecessary();
            return (int)result;
        }

        public override CollectionHandleBase Freeze(SharedRealmHandle frozenRealmHandle)
        {
            EnsureIsOpen();

            var result = NativeMethods.freeze(this, frozenRealmHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new ResultsHandle(frozenRealmHandle, result);
        }

        public override void Clear() => throw new NotSupportedException("Clearing a Results collection is not supported.");

        protected override IntPtr SnapshotCore(out NativeException ex) => NativeMethods.snapshot(this, out ex);

        protected override IntPtr GetFilteredResultsCore(string query, NativeQueryArgument[] arguments, out NativeException ex)
            => NativeMethods.get_filtered_results(this, query, query.IntPtrLength(), arguments, (IntPtr)arguments.Length, out ex);

        public override void Unbind() => NativeMethods.destroy(handle);
    }
}
