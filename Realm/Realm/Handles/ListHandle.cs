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
#pragma warning disable IDE1006 // Naming Styles

            #region add

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_add_object", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add_object(ListHandle listHandle, ObjectHandle objectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_add_primitive", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add_primitive(ListHandle listHandle, PrimitiveValue value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_add_embedded", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr add_embedded(ListHandle listHandle, out NativeException ex);

            #endregion

            #region set

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_set_object", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_object(ListHandle listHandle, IntPtr targetIndex, ObjectHandle objectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_set_primitive", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_primitive(ListHandle listHandle, IntPtr targetIndex, PrimitiveValue value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_set_embedded", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr set_embedded(ListHandle listHandle, IntPtr targetIndex, out NativeException ex);

            #endregion

            #region insert

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_insert_object", CallingConvention = CallingConvention.Cdecl)]
            public static extern void insert_object(ListHandle listHandle, IntPtr targetIndex, ObjectHandle objectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_insert_primitive", CallingConvention = CallingConvention.Cdecl)]
            public static extern void insert_primitive(ListHandle listHandle, IntPtr targetIndex, PrimitiveValue value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_insert_embedded", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr insert_embedded(ListHandle listHandle, IntPtr targetIndex, out NativeException ex);

            #endregion

            #region get

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_get_object", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_object(ListHandle listHandle, IntPtr link_ndx, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_get_primitive", CallingConvention = CallingConvention.Cdecl)]
            public static extern void get_primitive(ListHandle listHandle, IntPtr link_ndx, out PrimitiveValue value, out NativeException ex);

            #endregion

            #region find

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_find_object", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr find_object(ListHandle listHandle, ObjectHandle objectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_find_primitive", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr find_primitive(ListHandle listHandle, PrimitiveValue value, out NativeException ex);

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
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool get_is_valid(ListHandle listHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_get_thread_safe_reference", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_thread_safe_reference(ListHandle listHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_snapshot", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr snapshot(ListHandle list, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_get_is_frozen", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool get_is_frozen(ListHandle list, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_freeze", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr freeze(ListHandle handle, SharedRealmHandle frozen_realm, out NativeException ex);

#pragma warning restore IDE1006 // Naming Styles
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

        public ListHandle(RealmHandle root, IntPtr handle) : base(root, handle)
        {
        }

        protected override void Unbind()
        {
            NativeMethods.destroy(handle);
        }

        #region GetAtIndex

        protected override IntPtr GetObjectAtIndexCore(IntPtr index, out NativeException nativeException) =>
            NativeMethods.get_object(this, index, out nativeException);

        protected override void GetPrimitiveAtIndexCore(IntPtr index, out PrimitiveValue result, out NativeException nativeException) =>
            NativeMethods.get_primitive(this, index, out result, out nativeException);

        #endregion

        #region Add

        public void Add(ObjectHandle objectHandle)
        {
            NativeMethods.add_object(this, objectHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public unsafe void Add(RealmValue value)
        {
            var (primitive, gcHandle) = value.ToNative();
            NativeMethods.add_primitive(this, primitive, out var nativeException);
            gcHandle?.Free();
            nativeException.ThrowIfNecessary();
        }

        public ObjectHandle AddEmbedded()
        {
            var result = NativeMethods.add_embedded(this, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new ObjectHandle(Root, result);
        }

        #endregion

        #region Set

        public void Set(int targetIndex, ObjectHandle objectHandle)
        {
            NativeMethods.set_object(this, (IntPtr)targetIndex, objectHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public unsafe void Set(int targetIndex, RealmValue value)
        {
            var (primitive, gcHandle) = value.ToNative();
            NativeMethods.set_primitive(this, (IntPtr)targetIndex, primitive, out var nativeException);
            gcHandle?.Free();
            nativeException.ThrowIfNecessary();
        }

        public ObjectHandle SetEmbedded(int targetIndex)
        {
            var result = NativeMethods.set_embedded(this, (IntPtr)targetIndex, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new ObjectHandle(Root, result);
        }

        #endregion

        #region Insert

        public void Insert(int targetIndex, ObjectHandle objectHandle)
        {
            NativeMethods.insert_object(this, (IntPtr)targetIndex, objectHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public unsafe void Insert(int targetIndex, RealmValue value)
        {
            var (primitive, gcHandle) = value.ToNative();
            NativeMethods.insert_primitive(this, (IntPtr)targetIndex, primitive, out var nativeException);
            gcHandle?.Free();
            nativeException.ThrowIfNecessary();
        }

        public ObjectHandle InsertEmbedded(int targetIndex)
        {
            var result = NativeMethods.insert_embedded(this, (IntPtr)targetIndex, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new ObjectHandle(Root, result);
        }

        #endregion

        #region Find

        public int Find(ObjectHandle objectHandle)
        {
            var result = NativeMethods.find_object(this, objectHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
            return (int)result;
        }

        public unsafe int Find(RealmValue value)
        {
            var (primitive, gcHandle) = value.ToNative();
            var result = NativeMethods.find_primitive(this, primitive, out var nativeException);
            gcHandle?.Free();
            nativeException.ThrowIfNecessary();
            return (int)result;
        }

        #endregion

        public void Erase(IntPtr rowIndex)
        {
            NativeMethods.erase(this, rowIndex, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public override void Clear()
        {
            NativeMethods.clear(this, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Move(IntPtr sourceIndex, IntPtr targetIndex)
        {
            NativeMethods.move(this, sourceIndex, targetIndex, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public override NotificationTokenHandle AddNotificationCallback(IntPtr managedObjectHandle, NotificationCallbackDelegate callback)
        {
            var result = NativeMethods.add_notification_callback(this, managedObjectHandle, callback, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new NotificationTokenHandle(this, result);
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

            return new ThreadSafeReferenceHandle(result);
        }

        public override ResultsHandle Snapshot()
        {
            var ptr = NativeMethods.snapshot(this, out var ex);
            ex.ThrowIfNecessary();

            return new ResultsHandle(Root ?? this, ptr);
        }

        public override ResultsHandle GetFilteredResults(string query)
        {
            throw new NotImplementedException("Lists can't be filtered yet.");
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

        public override CollectionHandleBase Freeze(SharedRealmHandle frozenRealmHandle)
        {
            var result = NativeMethods.freeze(this, frozenRealmHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new ListHandle(frozenRealmHandle, result);
        }
    }
}