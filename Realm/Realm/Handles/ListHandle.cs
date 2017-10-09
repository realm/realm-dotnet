﻿////////////////////////////////////////////////////////////////////////////
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
            #region add

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_add_object", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add_object(ListHandle listHandle, ObjectHandle objectHandle, out NativeException ex);

            // value is IntPtr rather than PrimitiveValue due to a bug in .NET Core on Linux and Mac
            // that causes incorrect marshalling of the struct.
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_add_primitive", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add_primitive(ListHandle listHandle, IntPtr value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_add_string", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add_string(ListHandle listHandle, [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLength,
                [MarshalAs(UnmanagedType.I1)] bool has_value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_add_binary", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add_binary(ListHandle listHandle, IntPtr buffer, IntPtr bufferLength,
                [MarshalAs(UnmanagedType.I1)] bool has_value, out NativeException ex);

            #endregion

            #region insert

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_insert_object", CallingConvention = CallingConvention.Cdecl)]
            public static extern void insert_object(ListHandle listHandle, IntPtr targetIndex, ObjectHandle objectHandle, out NativeException ex);

            // value is IntPtr rather than PrimitiveValue due to a bug in .NET Core on Linux and Mac
            // that causes incorrect marshalling of the struct.
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_insert_primitive", CallingConvention = CallingConvention.Cdecl)]
            public static extern void insert_primitive(ListHandle listHandle, IntPtr targetIndex, IntPtr value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_insert_string", CallingConvention = CallingConvention.Cdecl)]
            public static extern void insert_string(ListHandle listHandle, IntPtr targetIndex, [MarshalAs(UnmanagedType.LPWStr)] string value,
                IntPtr valueLen, [MarshalAs(UnmanagedType.I1)] bool has_value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_insert_binary", CallingConvention = CallingConvention.Cdecl)]
            public static extern void insert_binary(ListHandle listHandle, IntPtr targetIndex, IntPtr buffer, IntPtr bufferLength,
                [MarshalAs(UnmanagedType.I1)] bool has_value, out NativeException ex);

            #endregion

            #region get

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_get_object", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_object(ListHandle listHandle, IntPtr link_ndx, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_get_primitive", CallingConvention = CallingConvention.Cdecl)]
            public static extern void get_primitive(ListHandle listHandle, IntPtr link_ndx, ref PrimitiveValue value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_get_string", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_string(ListHandle listHandle, IntPtr link_ndx, IntPtr buffer, IntPtr bufsize,
                [MarshalAs(UnmanagedType.I1)] out bool isNull, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_get_binary", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_binary(ListHandle listHandle, IntPtr link_ndx, IntPtr buffer, IntPtr bufsize,
                [MarshalAs(UnmanagedType.I1)] out bool isNull, out NativeException ex);

            #endregion

            #region find

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_find_object", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr find_object(ListHandle listHandle, ObjectHandle objectHandle, out NativeException ex);

            // value is IntPtr rather than PrimitiveValue due to a bug in .NET Core on Linux and Mac
            // that causes incorrect marshalling of the struct.
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_find_primitive", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr find_primitive(ListHandle listHandle, IntPtr value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_find_string", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr find_string(ListHandle listHandle, [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen,
                [MarshalAs(UnmanagedType.I1)] bool has_value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_find_binary", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr find_binary(ListHandle listHandle, IntPtr buffer, IntPtr bufsize,
                [MarshalAs(UnmanagedType.I1)] bool has_value, out NativeException ex);

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
            public static extern IntPtr get_thread_safe_reference(ListHandle listHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_snapshot", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr snapshot(ListHandle list, out NativeException ex);
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

        #region Add

        public void Add(ObjectHandle objectHandle)
        {
            NativeMethods.add_object(this, objectHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public unsafe void Add(PrimitiveValue value)
        {
            PrimitiveValue* valuePtr = &value;
            NativeMethods.add_primitive(this, new IntPtr(valuePtr), out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Add(string value)
        {
            NativeMethods.add_string(this, value, (IntPtr)(value?.Length ?? 0), value != null, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public unsafe void Add(byte[] value)
        {
            MarshalHelpers.SetByteArray(value, (IntPtr buffer, IntPtr bufferSize, bool hasValue, out NativeException ex) =>
                NativeMethods.add_binary(this, buffer, bufferSize, hasValue, out ex));
        }

        #endregion

        #region Insert

        public void Insert(int targetIndex, ObjectHandle objectHandle)
        {
            NativeMethods.insert_object(this, (IntPtr)targetIndex, objectHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public unsafe void Insert(int targetIndex, PrimitiveValue value)
        {
            PrimitiveValue* valuePtr = &value;
            NativeMethods.insert_primitive(this, (IntPtr)targetIndex, new IntPtr(valuePtr), out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Insert(int targetIndex, string value)
        {
            var hasValue = value != null;
            value = value ?? string.Empty;
            NativeMethods.insert_string(this, (IntPtr)targetIndex, value, (IntPtr)value.Length, hasValue, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public unsafe void Insert(int targetIndex, byte[] value)
        {
            MarshalHelpers.SetByteArray(value, (IntPtr buffer, IntPtr bufferSize, bool hasValue, out NativeException ex) =>
                NativeMethods.insert_binary(this, (IntPtr)targetIndex, buffer, bufferSize, hasValue, out ex));
        }

        #endregion

        #region Find

        public int Find(ObjectHandle objectHandle)
        {
            var result = NativeMethods.find_object(this, objectHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
            return (int)result;
        }

        public unsafe int Find(PrimitiveValue value)
        {
            PrimitiveValue* valuePtr = &value;
            var result = NativeMethods.find_primitive(this, new IntPtr(valuePtr), out var nativeException);
            nativeException.ThrowIfNecessary();
            return (int)result;
        }

        public int Find(string value)
        {
            var hasValue = value != null;
            value = value ?? string.Empty;
            var result = NativeMethods.find_string(this, value, (IntPtr)value.Length, hasValue, out var nativeException);
            nativeException.ThrowIfNecessary();
            return (int)result;
        }

        public unsafe int Find(byte[] value)
        {
            var result = IntPtr.Zero;
            MarshalHelpers.SetByteArray(value, (IntPtr buffer, IntPtr bufferSize, bool hasValue, out NativeException ex) =>
            {
                result = NativeMethods.find_binary(this, buffer, bufferSize, hasValue, out ex);
            });

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
    }
}