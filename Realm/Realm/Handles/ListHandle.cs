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
    internal class ListHandle : CollectionHandleBase
    {
        private static class NativeMethods
        {
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_add_value", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add_value(ListHandle listHandle, PrimitiveValue value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_add_embedded", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr add_embedded(ListHandle listHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_add_list", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr add_list(ListHandle listHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_set_value", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_value(ListHandle listHandle, IntPtr targetIndex, PrimitiveValue value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_set_embedded", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr set_embedded(ListHandle listHandle, IntPtr targetIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_set_list", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr set_list(ListHandle listHandle, IntPtr targetIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_insert_value", CallingConvention = CallingConvention.Cdecl)]
            public static extern void insert_value(ListHandle listHandle, IntPtr targetIndex, PrimitiveValue value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_insert_embedded", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr insert_embedded(ListHandle listHandle, IntPtr targetIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_insert_list", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr insert_list(ListHandle listHandle, IntPtr targetIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_get_value", CallingConvention = CallingConvention.Cdecl)]
            public static extern void get_value(ListHandle listHandle, IntPtr link_ndx, out PrimitiveValue value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_find_value", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr find_value(ListHandle listHandle, PrimitiveValue value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_erase", CallingConvention = CallingConvention.Cdecl)]
            public static extern void erase(ListHandle listHandle, IntPtr rowIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_clear", CallingConvention = CallingConvention.Cdecl)]
            public static extern void clear(ListHandle listHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_size", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr size(ListHandle listHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr listInternalHandle);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_add_notification_callback", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr add_notification_callback(ListHandle listHandle, IntPtr managedListHandle, [MarshalAs(UnmanagedType.U1)] bool shallow, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_move", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr move(ListHandle listHandle, IntPtr sourceIndex, IntPtr targetIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_get_is_valid", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool get_is_valid(ListHandle listHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_get_thread_safe_reference", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_thread_safe_reference(ListHandle listHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_snapshot", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr snapshot(ListHandle list, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_freeze", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr freeze(ListHandle handle, SharedRealmHandle frozen_realm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_to_results", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr to_results(ListHandle list, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_get_filtered_results", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_filtered_results(ListHandle results,
                [MarshalAs(UnmanagedType.LPWStr)] string query_buf, IntPtr query_len,
                [MarshalAs(UnmanagedType.LPArray), In] NativeQueryArgument[] arguments, IntPtr args_count,
                out NativeException ex);
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

        public ListHandle(SharedRealmHandle root, IntPtr handle) : base(root, handle)
        {
        }

        public RealmValue GetValueAtIndex(int index, Realm realm)
        {
            EnsureIsOpen();

            NativeMethods.get_value(this, (IntPtr)index, out var result, out var ex);
            ex.ThrowIfNecessary();
            return new RealmValue(result, realm);
        }

        public void Add(in RealmValue value)
        {
            EnsureIsOpen();

            var (primitive, handles) = value.ToNative();
            NativeMethods.add_value(this, primitive, out var nativeException);
            handles?.Dispose();
            nativeException.ThrowIfNecessary();
        }

        public ObjectHandle AddEmbedded()
        {
            EnsureIsOpen();

            var result = NativeMethods.add_embedded(this, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new ObjectHandle(Root!, result);
        }

        public ListHandle AddList()
        {
            EnsureIsOpen();

            var listPtr = NativeMethods.add_list(this, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new ListHandle(Root!, listPtr);
        }

        public void Set(int targetIndex, in RealmValue value)
        {
            EnsureIsOpen();

            var (primitive, handles) = value.ToNative();
            NativeMethods.set_value(this, (IntPtr)targetIndex, primitive, out var nativeException);
            handles?.Dispose();
            nativeException.ThrowIfNecessary();
        }

        public ObjectHandle SetEmbedded(int targetIndex)
        {
            EnsureIsOpen();

            var result = NativeMethods.set_embedded(this, (IntPtr)targetIndex, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new ObjectHandle(Root!, result);
        }

        public ListHandle SetList(int targetIndex)
        {
            EnsureIsOpen();

            var listPtr = NativeMethods.set_list(this, (IntPtr)targetIndex, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new ListHandle(Root!, listPtr);
        }

        public void Insert(int targetIndex, in RealmValue value)
        {
            EnsureIsOpen();

            var (primitive, handles) = value.ToNative();
            NativeMethods.insert_value(this, (IntPtr)targetIndex, primitive, out var nativeException);
            handles?.Dispose();
            nativeException.ThrowIfNecessary();
        }

        public ObjectHandle InsertEmbedded(int targetIndex)
        {
            EnsureIsOpen();

            var result = NativeMethods.insert_embedded(this, (IntPtr)targetIndex, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new ObjectHandle(Root!, result);
        }

        public ListHandle InsertList(int targetIndex)
        {
            EnsureIsOpen();

            var listPtr = NativeMethods.insert_list(this, (IntPtr)targetIndex, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new ListHandle(Root!, listPtr);
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

        public void Erase(IntPtr rowIndex)
        {
            EnsureIsOpen();

            NativeMethods.erase(this, rowIndex, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public override void Clear()
        {
            EnsureIsOpen();

            NativeMethods.clear(this, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Move(IntPtr sourceIndex, IntPtr targetIndex)
        {
            EnsureIsOpen();

            NativeMethods.move(this, sourceIndex, targetIndex, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public override NotificationTokenHandle AddNotificationCallback(IntPtr managedObjectHandle, bool shallow)
        {
            EnsureIsOpen();

            var result = NativeMethods.add_notification_callback(this, managedObjectHandle, shallow, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new NotificationTokenHandle(Root!, result);
        }

        public override int Count()
        {
            EnsureIsOpen();

            var result = NativeMethods.size(this, out var nativeException);
            nativeException.ThrowIfNecessary();
            return (int)result;
        }

        public override ThreadSafeReferenceHandle GetThreadSafeReference()
        {
            EnsureIsOpen();

            var result = NativeMethods.get_thread_safe_reference(this, out var nativeException);
            nativeException.ThrowIfNecessary();

            return new ThreadSafeReferenceHandle(result);
        }

        public ResultsHandle ToResults()
        {
            EnsureIsOpen();

            var ptr = NativeMethods.to_results(this, out var ex);
            ex.ThrowIfNecessary();

            return new ResultsHandle(Root!, ptr);
        }

        public override CollectionHandleBase Freeze(SharedRealmHandle frozenRealmHandle)
        {
            EnsureIsOpen();

            var result = NativeMethods.freeze(this, frozenRealmHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new ListHandle(frozenRealmHandle, result);
        }

        public override void Unbind() => NativeMethods.destroy(handle);

        protected override IntPtr GetFilteredResultsCore(string query, NativeQueryArgument[] arguments, out NativeException ex)
            => NativeMethods.get_filtered_results(this, query, query.IntPtrLength(), arguments, (IntPtr)arguments.Length, out ex);

        protected override IntPtr SnapshotCore(out NativeException ex) => NativeMethods.snapshot(this, out ex);
    }
}
