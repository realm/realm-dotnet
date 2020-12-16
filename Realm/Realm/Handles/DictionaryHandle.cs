////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Realms.Native;

namespace Realms
{
    internal class DictionaryHandle : CollectionHandleBase
    {
        private static class NativeMethods
        {
#pragma warning disable IDE1006 // Naming Styles

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_dictionary_clear", CallingConvention = CallingConvention.Cdecl)]
            public static extern void clear(DictionaryHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_dictionary_get_size", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr size(DictionaryHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_dictionary_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr listInternalHandle);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_dictionary_add_notification_callback", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr add_notification_callback(DictionaryHandle handle, IntPtr managedDictionaryHandle, NotificationCallbackDelegate callback, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_dictionary_get_is_valid", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool get_is_valid(DictionaryHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_dictionary_get_thread_safe_reference", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_thread_safe_reference(DictionaryHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_dictionary_get_is_frozen", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool get_is_frozen(DictionaryHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_dictionary_freeze", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr freeze(DictionaryHandle handle, SharedRealmHandle frozen_realm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_dictionary_get_at_index", CallingConvention = CallingConvention.Cdecl)]
            public static extern void get_at_index(DictionaryHandle handle, IntPtr index, out PrimitiveValue key, out PrimitiveValue value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_dictionary_try_get", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool try_get_value(DictionaryHandle handle, PrimitiveValue key, out PrimitiveValue value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_dictionary_set", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_value(DictionaryHandle handle, PrimitiveValue key, PrimitiveValue value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_dictionary_add", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add_value(DictionaryHandle handle, PrimitiveValue key, PrimitiveValue value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_dictionary_contains_key", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool contains_key(DictionaryHandle handle, PrimitiveValue key, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_dictionary_remove", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool remove(DictionaryHandle handle, PrimitiveValue value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_dictionary_get_values", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_values(DictionaryHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_dictionary_get_keys", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_keys(DictionaryHandle handle, out NativeException ex);

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

        public DictionaryHandle(RealmHandle root, IntPtr handle) : base(root, handle)
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

        public override ResultsHandle GetFilteredResults(string query)
        {
            throw new NotImplementedException("Dictionaries can't be filtered yet.");
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
            return new DictionaryHandle(frozenRealmHandle, result);
        }

        public bool TryGet(string key, RealmObjectBase.Metadata metadata, Realm realm, out RealmValue value)
        {
            RealmValue keyValue = key;
            var (primitiveKey, keyHandles) = keyValue.ToNative();
            var containsValue = NativeMethods.try_get_value(this, primitiveKey, out var result, out var nativeException);
            keyHandles?.Dispose();
            nativeException.ThrowIfNecessary();

            if (!containsValue)
            {
                value = default;
                return false;
            }

            if (result.Type != RealmValueType.Object)
            {
                value = new RealmValue(result);
            }
            else
            {
                var objectHandle = result.AsObject(Root);

                if (metadata == null)
                {
                    throw new NotImplementedException("Mixed objects are not supported yet.");
                }

                value = new RealmValue(realm.MakeObject(metadata, objectHandle));
            }

            return true;
        }

        public KeyValuePair<string, TValue> GetValueAtIndex<TValue>(int index, RealmObjectBase.Metadata metadata, Realm realm)
        {
            NativeMethods.get_at_index(this, (IntPtr)index, out var key, out var primitiveValue, out var ex);
            ex.ThrowIfNecessary();
            var value = ToRealmValue(primitiveValue, metadata, realm);
            return new KeyValuePair<string, TValue>(key.AsString(), value.As<TValue>());
        }

        public unsafe void Set(string key, in RealmValue value)
        {
            var (primitive, valueHandles) = value.ToNative();

            RealmValue keyValue = key;
            var (primitiveKey, keyHandles) = keyValue.ToNative();

            NativeMethods.set_value(this, primitiveKey, primitive, out var nativeException);
            valueHandles?.Dispose();
            keyHandles?.Dispose();
            nativeException.ThrowIfNecessary();
        }

        public unsafe void Add(string key, in RealmValue value)
        {
            var (primitive, handles) = value.ToNative();

            RealmValue keyValue = key;
            var (primitiveKey, keyHandles) = keyValue.ToNative();

            NativeMethods.add_value(this, primitiveKey, primitive, out var nativeException);
            handles?.Dispose();
            keyHandles?.Dispose();
            nativeException.ThrowIfNecessary();
        }

        public unsafe bool ContainsKey(string key)
        {
            RealmValue keyValue = key;
            var (primitiveKey, keyHandles) = keyValue.ToNative();

            var result = NativeMethods.contains_key(this, primitiveKey, out var nativeException);
            keyHandles?.Dispose();
            nativeException.ThrowIfNecessary();

            return result;
        }

        public unsafe bool Remove(string key)
        {
            RealmValue keyValue = key;
            var (primitiveKey, keyHandles) = keyValue.ToNative();

            var result = NativeMethods.remove(this, primitiveKey, out var nativeException);
            keyHandles?.Dispose();
            nativeException.ThrowIfNecessary();

            return result;
        }

        public ResultsHandle GetValues()
        {
            var resultsPtr = NativeMethods.get_values(this, out var ex);
            ex.ThrowIfNecessary();
            return new ResultsHandle(Root ?? this, resultsPtr);
        }

        public ResultsHandle GetKeys()
        {
            var resultsPtr = NativeMethods.get_keys(this, out var ex);
            ex.ThrowIfNecessary();
            return new ResultsHandle(Root ?? this, resultsPtr);
        }
    }
}