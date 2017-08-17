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
    internal class ListHandle : CollectionHandleBase
    {
        private static class NativeMethods
        {
            #region add

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_add", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add(ListHandle listHandle, ObjectHandle objectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_add_int64", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add_int64(ListHandle listHandle, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_add_nullable_int64", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add_nullable_int64(ListHandle listHandle, Int64 value, [MarshalAs(UnmanagedType.I1)] bool has_value, out NativeException ex);

            #endregion

            #region insert

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_insert", CallingConvention = CallingConvention.Cdecl)]
            public static extern void insert(ListHandle listHandle, IntPtr targetIndex, ObjectHandle objectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_insert_int64", CallingConvention = CallingConvention.Cdecl)]
            public static extern void insert_int64(ListHandle listHandle, IntPtr targetIndex, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_insert_nullable_int64", CallingConvention = CallingConvention.Cdecl)]
            public static extern void insert_nullable_int64(ListHandle listHandle, IntPtr targetIndex, Int64 value, [MarshalAs(UnmanagedType.I1)] bool has_value, out NativeException ex);

            #endregion

            #region get

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_get", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get(ListHandle listHandle, IntPtr link_ndx, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_get_int64", CallingConvention = CallingConvention.Cdecl)]
            public static extern Int64 get_int64(ListHandle listHandle, IntPtr link_ndx, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_get_nullable_int64", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool get_nullable_int64(ListHandle listHandle, IntPtr link_ndx, out Int64 retVal, out NativeException ex);

            #endregion

            #region find

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_find", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr find(ListHandle listHandle, ObjectHandle objectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_find_int64", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr find_int64(ListHandle listHandle, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_find_nullable_int64", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr find_nullable_int64(ListHandle listHandle, Int64 value, [MarshalAs(UnmanagedType.I1)] bool has_value, out NativeException ex);

            #endregion

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_erase", CallingConvention = CallingConvention.Cdecl)]
            public static extern void erase(ListHandle listHandle, IntPtr rowIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_clear", CallingConvention = CallingConvention.Cdecl)]
            public static extern void clear(ListHandle listHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_size", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr size(ListHandle listHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr listInternalHandle);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_add_notification_callback", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr add_notification_callback(ListHandle listHandle, IntPtr managedListHandle, NotificationCallbackDelegate callback, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_move", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr move(ListHandle listHandle, IntPtr sourceIndex, IntPtr targetIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_get_is_valid", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool get_is_valid(ListHandle listHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_get_thread_safe_reference", CallingConvention = CallingConvention.Cdecl)]
            public static extern ThreadSafeReferenceHandle get_thread_safe_reference(ListHandle listHandle, out NativeException ex);
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

        public ListHandle(RealmHandle root) : base(root)
        {
        }

        protected override void Unbind()
        {
            NativeMethods.destroy(handle);
        }

        #region GetAtIndex

        public override IntPtr GetObjectAtIndex(int index)
        {
            var result = NativeMethods.get(this, (IntPtr)index, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public long GetInt64AtIndex(int index)
        {
            var result = NativeMethods.get_int64(this, (IntPtr)index, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public long? GetNullableInt64AtIndex(int index)
        {
            var hasValue = NativeMethods.get_nullable_int64(this, (IntPtr)index, out var value, out var nativeException);
            nativeException.ThrowIfNecessary();
            return hasValue ? value : (long?)null;
        }

        #endregion

        #region Add

        public void Add(ObjectHandle objectHandle)
        {
            NativeMethods.add(this, objectHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Add(long value)
        {
            NativeMethods.add_int64(this, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Add(long? value)
        {
            NativeMethods.add_nullable_int64(this, value.GetValueOrDefault(), value.HasValue, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        #endregion

        #region Insert

        public void Insert(int targetIndex, ObjectHandle objectHandle)
        {
            NativeMethods.insert(this, (IntPtr)targetIndex, objectHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Insert(int targetIndex, long value)
        {
            NativeMethods.insert_int64(this, (IntPtr)targetIndex, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Insert(int targetIndex, long? value)
        {
            NativeMethods.insert_nullable_int64(this, (IntPtr)targetIndex, value.GetValueOrDefault(), value.HasValue, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        #endregion

        #region Find

        public int Find(ObjectHandle objectHandle)
        {
            var result = NativeMethods.find(this, objectHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
            return (int)result;
        }

        public int Find(long value)
        {
            var result = NativeMethods.find_int64(this, value, out var nativeException);
            nativeException.ThrowIfNecessary();
            return (int)result;
        }

        public int Find(long? value)
        {
            var result = NativeMethods.find_nullable_int64(this, value.GetValueOrDefault(), value.HasValue, out var nativeException);
            nativeException.ThrowIfNecessary();
            return (int)result;
        }

        #endregion

        public void Erase(IntPtr rowIndex)
        {
            NativeMethods.erase(this, rowIndex, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Clear()
        {
            NativeMethods.clear(this, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Move(IntPtr sourceIndex, IntPtr targetIndex)
        {
            NativeMethods.move(this, sourceIndex, targetIndex, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public override IntPtr AddNotificationCallback(IntPtr managedObjectHandle, NotificationCallbackDelegate callback)
        {
            var result = NativeMethods.add_notification_callback(this, managedObjectHandle, callback, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public override int Count()
        {
            var result = NativeMethods.size(this, out var nativeException);
            nativeException.ThrowIfNecessary();
            return (int)result;
        }

        public override ThreadSafeReferenceHandle GetThreadSafeReference()
        {
            var result = NativeMethods.get_thread_safe_reference(this, out var nativeException);
            nativeException.ThrowIfNecessary();

            return result;
        }
    }
}