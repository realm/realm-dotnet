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
    internal class ResultsHandle : CollectionHandleBase
    {
        private static class NativeMethods
        {
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1121 // Use built-in type alias

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_is_same_internal_results", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool is_same_internal_results(ResultsHandle lhs, ResultsHandle rhs, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr resultsHandle);

            #region get

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_get_object", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_object(ResultsHandle results, IntPtr index, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_get_primitive", CallingConvention = CallingConvention.Cdecl)]
            public static extern void get_primitive(ResultsHandle results, IntPtr link_ndx, ref PrimitiveValue value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_get_string", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_string(ResultsHandle results, IntPtr link_ndx, IntPtr buffer, IntPtr bufsize,
                [MarshalAs(UnmanagedType.I1)] out bool isNull, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_get_binary", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_binary(ResultsHandle results, IntPtr link_ndx, IntPtr buffer, IntPtr bufsize,
                [MarshalAs(UnmanagedType.I1)] out bool isNull, out NativeException ex);

            #endregion

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_count", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr count(ResultsHandle results, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_clear", CallingConvention = CallingConvention.Cdecl)]
            public static extern void clear(ResultsHandle results, SharedRealmHandle realmHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_add_notification_callback", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr add_notification_callback(ResultsHandle results, IntPtr managedResultsHandle, NotificationCallbackDelegate callback, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_get_query", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_query(ResultsHandle results, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_get_is_valid", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool get_is_valid(ResultsHandle results, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_get_thread_safe_reference", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_thread_safe_reference(ResultsHandle results, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_snapshot", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr snapshot(ResultsHandle results, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_get_filtered_results", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_filtered_results(ResultsHandle results, [MarshalAs(UnmanagedType.LPWStr)] string query_buf, IntPtr query_len, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_find_object", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr find_object(ResultsHandle results, ObjectHandle objectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_get_is_frozen", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool get_is_frozen(ResultsHandle results, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_freeze", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr freeze(ResultsHandle handle, SharedRealmHandle frozen_realm, out NativeException ex);

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore SA1121 // Use built-in type alias
        }

        public override bool IsValid
        {
            get
            {
                var result = NativeMethods.get_is_valid(this, out var nativeException);
                nativeException.ThrowIfNecessary();
                return result;
            }
        }

        // keep this one even though warned that it is not used. It is in fact used by marshalling
        // used by P/Invoke to automatically construct a ResultsHandle when returning a size_t as a ResultsHandle
        [Preserve]
        public ResultsHandle() : this(null, IntPtr.Zero)
        {
        }

        [Preserve]
        public ResultsHandle(RealmHandle root, IntPtr handle) : base(root, handle)
        {
        }

        protected override void Unbind()
        {
            NativeMethods.destroy(handle);
        }

        #region GetAtIndex

        protected override IntPtr GetObjectAtIndexCore(IntPtr index, out NativeException nativeException) =>
            NativeMethods.get_object(this, index, out nativeException);

        protected override void GetPrimitiveAtIndexCore(IntPtr index, ref PrimitiveValue result, out NativeException nativeException) =>
            NativeMethods.get_primitive(this, index, ref result, out nativeException);

        public override string GetStringAtIndex(int index)
        {
            return MarshalHelpers.GetString((IntPtr buffer, IntPtr length, out bool isNull, out NativeException ex) =>
                NativeMethods.get_string(this, (IntPtr)index, buffer, length, out isNull, out ex));
        }

        public override byte[] GetByteArrayAtIndex(int index)
        {
            return MarshalHelpers.GetByteArray((IntPtr buffer, IntPtr bufferLength, out bool isNull, out NativeException ex) =>
                NativeMethods.get_binary(this, (IntPtr)index, buffer, bufferLength, out isNull, out ex));
        }

        #endregion

        public override int Count()
        {
            var result = NativeMethods.count(this, out var nativeException);
            nativeException.ThrowIfNecessary();
            return (int)result;
        }

        public void Clear(SharedRealmHandle realmHandle)
        {
            NativeMethods.clear(this, realmHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public QueryHandle CreateQuery()
        {
            var result = NativeMethods.get_query(this, out var nativeException);
            nativeException.ThrowIfNecessary();

            return new QueryHandle(Root ?? this, result);
        }

        public override NotificationTokenHandle AddNotificationCallback(IntPtr managedObjectHandle, NotificationCallbackDelegate callback)
        {
            var result = NativeMethods.add_notification_callback(this, managedObjectHandle, callback, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new NotificationTokenHandle(this, result);
        }

        public override bool Equals(object obj)
        {
            // If parameter is null, return false.
            if (obj is null)
            {
                return false;
            }

            // Optimization for a common success case.
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (!(obj is ResultsHandle resultsHandle))
            {
                return false;
            }

            var result = NativeMethods.is_same_internal_results(this, resultsHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public override ThreadSafeReferenceHandle GetThreadSafeReference()
        {
            var result = NativeMethods.get_thread_safe_reference(this, out var nativeException);
            nativeException.ThrowIfNecessary();

            return new ThreadSafeReferenceHandle(result);
        }

        public override ResultsHandle Snapshot()
        {
            var ptr = NativeMethods.snapshot(this, out var ex);
            ex.ThrowIfNecessary();

            return new ResultsHandle(this, ptr);
        }

        public override ResultsHandle GetFilteredResults(string query)
        {
            var ptr = NativeMethods.get_filtered_results(this, query, (IntPtr)query.Length, out var ex);
            ex.ThrowIfNecessary();
            return new ResultsHandle(this, ptr);
        }

        public int Find(ObjectHandle objectHandle)
        {
            var result = NativeMethods.find_object(this, objectHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
            return (int)result;
        }

        public override bool IsFrozen
        {
            get
            {
                var result = NativeMethods.get_is_frozen(this, out var nativeException);
                nativeException.ThrowIfNecessary();
                return result;
            }
        }

        public ResultsHandle Freeze(SharedRealmHandle frozenRealmHandle)
        {
            var result = NativeMethods.freeze(this, frozenRealmHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new ResultsHandle(frozenRealmHandle, result);
        }
    }
}
