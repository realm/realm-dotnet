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
using System.Linq;
using System.Runtime.InteropServices;
using Realms.Native;

namespace Realms
{
    internal class DictionaryHandle : CollectionHandleBase
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct DictionaryChangeSet
        {
            public MarshaledVector<PrimitiveValue> Deletions;
            public MarshaledVector<PrimitiveValue> Insertions;
            public MarshaledVector<PrimitiveValue> Modifications;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate void KeyNotificationCallback(IntPtr managedHandle, DictionaryChangeSet* changes);

        private static class NativeMethods
        {
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_dictionary_clear", CallingConvention = CallingConvention.Cdecl)]
            public static extern void clear(DictionaryHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_dictionary_get_size", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr size(DictionaryHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_dictionary_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr listInternalHandle);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_dictionary_add_notification_callback", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr add_notification_callback(DictionaryHandle handle, IntPtr managedDictionaryHandle,
                KeyPathsCollectionType type, IntPtr callback, MarshaledVector<StringValue> keypaths, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_dictionary_add_key_notification_callback", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr add_key_notification_callback(DictionaryHandle handle, IntPtr managedDictionaryHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_dictionary_get_is_valid", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool get_is_valid(DictionaryHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_dictionary_get_thread_safe_reference", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_thread_safe_reference(DictionaryHandle handle, out NativeException ex);

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

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_dictionary_add_embedded", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr add_embedded(DictionaryHandle handle, PrimitiveValue key, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_dictionary_set_embedded", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr set_embedded(DictionaryHandle handle, PrimitiveValue key, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_dictionary_contains_key", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool contains_key(DictionaryHandle handle, PrimitiveValue key, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_dictionary_remove", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool remove(DictionaryHandle handle, PrimitiveValue key, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_dictionary_remove_value", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool remove(DictionaryHandle handle, PrimitiveValue key, PrimitiveValue value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_dictionary_get_values", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_values(DictionaryHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_dictionary_get_keys", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_keys(DictionaryHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_dictionary_get_filtered_results", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_filtered_results(DictionaryHandle handle,
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

        public DictionaryHandle(SharedRealmHandle root, IntPtr handle) : base(root, handle)
        {
        }

        public override void Unbind() => NativeMethods.destroy(handle);

        public override void Clear()
        {
            EnsureIsOpen();

            NativeMethods.clear(this, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public override NotificationTokenHandle AddNotificationCallback(IntPtr managedObjectHandle,
            KeyPathsCollection keyPathsCollection, IntPtr callback = default)
        {
            EnsureIsOpen();

            using Arena arena = new Arena();
            var nativeKeyPathsArray = MarshaledVector<StringValue>
                .AllocateFrom(keyPathsCollection.GetStrings().Select(p => StringValue.AllocateFrom(p, arena)).ToArray(), arena);

            var result = NativeMethods.add_notification_callback(this, managedObjectHandle,
                keyPathsCollection.Type, callback, nativeKeyPathsArray, out var nativeException);
            return new NotificationTokenHandle(Root!, result);
        }

        public NotificationTokenHandle AddKeyNotificationCallback(IntPtr managedObjectHandle)
        {
            EnsureIsOpen();

            var result = NativeMethods.add_key_notification_callback(this, managedObjectHandle, out var nativeException);
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

        protected override IntPtr GetFilteredResultsCore(string query, NativeQueryArgument[] arguments, out NativeException ex)
            => NativeMethods.get_filtered_results(this, query, query.IntPtrLength(), arguments, (IntPtr)arguments.Length, out ex);

        public override CollectionHandleBase Freeze(SharedRealmHandle frozenRealmHandle)
        {
            EnsureIsOpen();

            var result = NativeMethods.freeze(this, frozenRealmHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new DictionaryHandle(frozenRealmHandle, result);
        }

        public bool TryGet(string key, Realm realm, out RealmValue value)
        {
            EnsureIsOpen();

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

            value = new RealmValue(result, realm);
            return true;
        }

        public KeyValuePair<string, TValue> GetValueAtIndex<TValue>(int index, Realm realm)
        {
            EnsureIsOpen();

            NativeMethods.get_at_index(this, (IntPtr)index, out var key, out var primitiveValue, out var ex);
            ex.ThrowIfNecessary();
            var value = new RealmValue(primitiveValue, realm);
            return new KeyValuePair<string, TValue>(key.AsString(), value.As<TValue>());
        }

        public void Set(string key, in RealmValue value)
        {
            EnsureIsOpen();

            var (primitive, valueHandles) = value.ToNative();

            RealmValue keyValue = key;
            var (primitiveKey, keyHandles) = keyValue.ToNative();

            NativeMethods.set_value(this, primitiveKey, primitive, out var nativeException);
            valueHandles?.Dispose();
            keyHandles?.Dispose();
            nativeException.ThrowIfNecessary();
        }

        public void Add(string key, in RealmValue value)
        {
            EnsureIsOpen();

            var (primitive, handles) = value.ToNative();

            RealmValue keyValue = key;
            var (primitiveKey, keyHandles) = keyValue.ToNative();

            NativeMethods.add_value(this, primitiveKey, primitive, out var nativeException);
            handles?.Dispose();
            keyHandles?.Dispose();
            nativeException.ThrowIfNecessary();
        }

        public ObjectHandle AddEmbedded(string key)
        {
            EnsureIsOpen();

            RealmValue keyValue = key;
            var (primitiveKey, keyHandles) = keyValue.ToNative();

            var result = NativeMethods.add_embedded(this, primitiveKey, out var nativeException);
            keyHandles?.Dispose();
            nativeException.ThrowIfNecessary();

            return new ObjectHandle(Root!, result);
        }

        public ObjectHandle SetEmbedded(string key)
        {
            EnsureIsOpen();

            RealmValue keyValue = key;
            var (primitiveKey, keyHandles) = keyValue.ToNative();

            var result = NativeMethods.set_embedded(this, primitiveKey, out var nativeException);
            keyHandles?.Dispose();
            nativeException.ThrowIfNecessary();

            return new ObjectHandle(Root!, result);
        }

        public bool ContainsKey(string key)
        {
            EnsureIsOpen();

            RealmValue keyValue = key;
            var (primitiveKey, keyHandles) = keyValue.ToNative();

            var result = NativeMethods.contains_key(this, primitiveKey, out var nativeException);
            keyHandles?.Dispose();
            nativeException.ThrowIfNecessary();

            return result;
        }

        public bool Remove(string key)
        {
            EnsureIsOpen();

            RealmValue keyValue = key;
            var (primitiveKey, keyHandles) = keyValue.ToNative();

            var result = NativeMethods.remove(this, primitiveKey, out var nativeException);
            keyHandles?.Dispose();
            nativeException.ThrowIfNecessary();

            return result;
        }

        public bool Remove(string key, in RealmValue value)
        {
            EnsureIsOpen();

            var (primitiveValue, valueHandles) = value.ToNative();

            RealmValue keyValue = key;
            var (primitiveKey, keyHandles) = keyValue.ToNative();

            var result = NativeMethods.remove(this, primitiveKey, primitiveValue, out var nativeException);

            valueHandles?.Dispose();
            keyHandles?.Dispose();
            nativeException.ThrowIfNecessary();

            return result;
        }

        public ResultsHandle GetValues()
        {
            EnsureIsOpen();

            var resultsPtr = NativeMethods.get_values(this, out var ex);
            ex.ThrowIfNecessary();
            return new ResultsHandle(Root!, resultsPtr);
        }

        public ResultsHandle GetKeys()
        {
            EnsureIsOpen();

            var resultsPtr = NativeMethods.get_keys(this, out var ex);
            ex.ThrowIfNecessary();
            return new ResultsHandle(Root!, resultsPtr);
        }

        [MonoPInvokeCallback(typeof(KeyNotificationCallback))]
        public static unsafe void NotifyDictionaryChanged(IntPtr managedHandle, DictionaryChangeSet* changes)
        {
            if (GCHandle.FromIntPtr(managedHandle).Target is INotifiable<DictionaryChangeSet> notifiable)
            {
                //TODO Check if it makes sense to do something different for dictionaries, so we don't need to pass default here
                notifiable.NotifyCallbacks(changes == null ? null : *changes, KeyPathsCollectionType.Default);
            }
        }
    }
}
