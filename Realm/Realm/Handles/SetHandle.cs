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
    internal class SetHandle : CollectionHandleBase
    {
        private static class NativeMethods
        {
#pragma warning disable IDE1006 // Naming Styles

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_clear", CallingConvention = CallingConvention.Cdecl)]
            public static extern void clear(SetHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_size", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr size(SetHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr listInternalHandle);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_add_notification_callback", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr add_notification_callback(SetHandle handle, IntPtr managedSetHandle, NotificationCallbackDelegate callback, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_get_is_valid", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool get_is_valid(SetHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_get_thread_safe_reference", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_thread_safe_reference(SetHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_snapshot", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr snapshot(SetHandle list, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_get_is_frozen", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool get_is_frozen(SetHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_freeze", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr freeze(SetHandle handle, SharedRealmHandle frozen_realm, out NativeException ex);

            #region get

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_get_object", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_object(SetHandle handle, IntPtr link_ndx, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_get_primitive", CallingConvention = CallingConvention.Cdecl)]
            public static extern void get_primitive(SetHandle handle, IntPtr link_ndx, ref PrimitiveValue value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_get_string", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_string(SetHandle handle, IntPtr link_ndx, IntPtr buffer, IntPtr bufsize,
                [MarshalAs(UnmanagedType.U1)] out bool isNull, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_get_binary", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_binary(SetHandle handle, IntPtr link_ndx, IntPtr buffer, IntPtr bufsize,
                [MarshalAs(UnmanagedType.U1)] out bool isNull, out NativeException ex);

            #endregion

            #region add

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_add_object", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool add_object(SetHandle handle, ObjectHandle objectHandle, out NativeException ex);

            // value is IntPtr rather than PrimitiveValue due to a bug in .NET Core on Linux and Mac
            // that causes incorrect marshalling of the struct.
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_add_primitive", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool add_primitive(SetHandle handle, IntPtr value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_add_string", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool add_string(SetHandle handle, [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLength, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_add_binary", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool add_binary(SetHandle handle, IntPtr buffer, IntPtr bufferLength,
                [MarshalAs(UnmanagedType.U1)] bool has_value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_add_embedded", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern IntPtr add_embedded(SetHandle handle, out NativeException ex);

            #endregion

            #region find

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_contains_object", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool contains_object(SetHandle handle, ObjectHandle objectHandle, out NativeException ex);

            // value is IntPtr rather than PrimitiveValue due to a bug in .NET Core on Linux and Mac
            // that causes incorrect marshalling of the struct.
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_contains_primitive", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool contains_primitive(SetHandle handle, IntPtr value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_contains_string", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool contains_string(SetHandle handle, [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_contains_binary", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool contains_binary(SetHandle handle, IntPtr buffer, IntPtr bufsize,
                [MarshalAs(UnmanagedType.U1)] bool has_value, out NativeException ex);

            #endregion

            #region find

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_remove_object", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool remove_object(SetHandle handle, ObjectHandle objectHandle, out NativeException ex);

            // value is IntPtr rather than PrimitiveValue due to a bug in .NET Core on Linux and Mac
            // that causes incorrect marshalling of the struct.
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_remove_primitive", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool remove_primitive(SetHandle handle, IntPtr value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_remove_string", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool remove_string(SetHandle handle, [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_remove_binary", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool remove_binary(SetHandle handle, IntPtr buffer, IntPtr bufsize,
                [MarshalAs(UnmanagedType.U1)] bool has_value, out NativeException ex);

            #endregion
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

        public SetHandle(RealmHandle root, IntPtr handle) : base(root, handle)
        {
        }

        protected override void Unbind()
        {
            NativeMethods.destroy(handle);
        }

        public override void Clear()
        {
            NativeMethods.clear(this, out var nativeException);
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
            throw new NotImplementedException("Sets can't be filtered yet.");
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
            return new SetHandle(frozenRealmHandle, result);
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

        public bool Add(ObjectHandle objectHandle)
        {
            var result = NativeMethods.add_object(this, objectHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public unsafe bool Add(PrimitiveValue value)
        {
            PrimitiveValue* valuePtr = &value;
            var result = NativeMethods.add_primitive(this, new IntPtr(valuePtr), out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public bool Add(string value)
        {
            var result = NativeMethods.add_string(this, value, value.IntPtrLength(), out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public unsafe bool Add(byte[] value)
        {
            var result = false;
            MarshalHelpers.SetByteArray(value, (IntPtr buffer, IntPtr bufferSize, bool hasValue, out NativeException ex) =>
                result = NativeMethods.add_binary(this, buffer, bufferSize, hasValue, out ex));

            return result;
        }

        public ObjectHandle AddEmbedded()
        {
            var result = NativeMethods.add_embedded(this, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new ObjectHandle(Root, result);
        }

        #endregion

        #region Find

        public bool Contains(ObjectHandle objectHandle)
        {
            var result = NativeMethods.contains_object(this, objectHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public unsafe bool Contains(PrimitiveValue value)
        {
            PrimitiveValue* valuePtr = &value;
            var result = NativeMethods.contains_primitive(this, new IntPtr(valuePtr), out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public bool Contains(string value)
        {
            var result = NativeMethods.contains_string(this, value, value.IntPtrLength(), out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public unsafe bool Contains(byte[] value)
        {
            var result = false;
            MarshalHelpers.SetByteArray(value, (IntPtr buffer, IntPtr bufferSize, bool hasValue, out NativeException ex) =>
            {
                result = NativeMethods.contains_binary(this, buffer, bufferSize, hasValue, out ex);
            });

            return result;
        }

        #endregion

        #region Remove

        public bool Remove(ObjectHandle objectHandle)
        {
            var result = NativeMethods.remove_object(this, objectHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public unsafe bool Remove(PrimitiveValue value)
        {
            PrimitiveValue* valuePtr = &value;
            var result = NativeMethods.remove_primitive(this, new IntPtr(valuePtr), out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public bool Remove(string value)
        {
            var result = NativeMethods.remove_string(this, value, value.IntPtrLength(), out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public unsafe bool Remove(byte[] value)
        {
            var result = false;
            MarshalHelpers.SetByteArray(value, (IntPtr buffer, IntPtr bufferSize, bool hasValue, out NativeException ex) =>
                result = NativeMethods.remove_binary(this, buffer, bufferSize, hasValue, out ex));

            return result;
        }

        #endregion

    }
}