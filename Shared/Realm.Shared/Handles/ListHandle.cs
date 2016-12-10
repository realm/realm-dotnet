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
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_add", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add(ListHandle listHandle, ObjectHandle objectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_insert", CallingConvention = CallingConvention.Cdecl)]
            public static extern void insert(ListHandle listHandle, IntPtr targetIndex, ObjectHandle objectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_erase", CallingConvention = CallingConvention.Cdecl)]
            public static extern void erase(ListHandle listHandle, IntPtr rowIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_clear", CallingConvention = CallingConvention.Cdecl)]
            public static extern void clear(ListHandle listHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_get", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get(ListHandle listHandle, IntPtr link_ndx, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_find", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr find(ListHandle listHandle, ObjectHandle objectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_size", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr size(ListHandle listHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr listInternalHandle);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_add_notification_callback", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr add_notification_callback(ListHandle listHandle, IntPtr managedListHandle, NotificationCallbackDelegate callback, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_move", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr move(ListHandle listHandle, ObjectHandle objectHandle, IntPtr targetIndex, out NativeException ex);
        }

        internal ListHandle(RealmHandle root) : base(root)
        {
        }

        protected override void Unbind()
        {
            NativeMethods.destroy(handle);
        }

        public void Add(ObjectHandle objectHandle)
        {
            NativeException nativeException;
            NativeMethods.add(this, objectHandle, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Insert(IntPtr targetIndex, ObjectHandle objectHandle)
        {
            NativeException nativeException;
            NativeMethods.insert(this, targetIndex, objectHandle, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Erase(IntPtr rowIndex)
        {
            NativeException nativeException;
            NativeMethods.erase(this, rowIndex, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Clear()
        {
            NativeException nativeException;
            NativeMethods.clear(this, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public IntPtr Find(ObjectHandle objectHandle)
        {
            NativeException nativeException;
            var result = NativeMethods.find(this, objectHandle, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public void Move(ObjectHandle objectHandle, IntPtr targetIndex)
        {
            NativeException nativeException;
            NativeMethods.move(this, objectHandle, targetIndex, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public override IntPtr AddNotificationCallback(IntPtr managedCollectionHandle, NotificationCallbackDelegate callback)
        {
            NativeException nativeException;
            var result = NativeMethods.add_notification_callback(this, managedCollectionHandle, callback, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public override IntPtr GetObjectAtIndex(int index)
        {
            NativeException nativeException;
            var result = NativeMethods.get(this, (IntPtr)index, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public override int Count()
        {
            NativeException nativeException;
            var result = NativeMethods.size(this, out nativeException);
            nativeException.ThrowIfNecessary();
            return (int)result;
        }
    }
}