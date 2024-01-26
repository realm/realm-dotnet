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
using System.Linq;
using System.Runtime.InteropServices;
using Realms.Native;

namespace Realms
{
    internal class SetHandle : CollectionHandleBase
    {
        private static class NativeMethods
        {
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_clear", CallingConvention = CallingConvention.Cdecl)]
            public static extern void clear(SetHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_get_size", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr size(SetHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr handle);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_add_notification_callback", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr add_notification_callback(SetHandle handle, IntPtr managedSetHandle,
                KeyPathsCollectionType type, IntPtr callback, MarshaledVector<StringValue> keypaths, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_get_is_valid", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool get_is_valid(SetHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_get_thread_safe_reference", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_thread_safe_reference(SetHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_snapshot", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr snapshot(SetHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_freeze", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr freeze(SetHandle handle, SharedRealmHandle frozen_realm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_get_value", CallingConvention = CallingConvention.Cdecl)]
            public static extern void get_value(SetHandle handle, IntPtr link_ndx, out PrimitiveValue value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_add_value", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool add_value(SetHandle handle, PrimitiveValue value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_contains_value", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool contains_value(SetHandle handle, PrimitiveValue value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_remove_value", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool remove_value(SetHandle handle, PrimitiveValue value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_except_with", CallingConvention = CallingConvention.Cdecl)]
            public static extern void except_with(SetHandle handle, CollectionHandleBase other_handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_intersect_with", CallingConvention = CallingConvention.Cdecl)]
            public static extern void intersect_with(SetHandle handle, CollectionHandleBase other_handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_symmetric_except_with", CallingConvention = CallingConvention.Cdecl)]
            public static extern void symmetric_except_with(SetHandle handle, CollectionHandleBase other_handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_union_with", CallingConvention = CallingConvention.Cdecl)]
            public static extern void union_with(SetHandle handle, CollectionHandleBase other_handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_is_subset_of", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool is_subset_of(SetHandle handle, CollectionHandleBase other_handle, [MarshalAs(UnmanagedType.I1)] bool proper, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_is_superset_of", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool is_superset_of(SetHandle handle, CollectionHandleBase other_handle, [MarshalAs(UnmanagedType.I1)] bool proper, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_overlaps", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool overlaps(SetHandle handle, CollectionHandleBase other_handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_set_set_equals", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool set_equals(SetHandle handle, CollectionHandleBase other_handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "set_to_results", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr to_results(SetHandle set, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "set_get_filtered_results", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_filtered_results(SetHandle results,
                [MarshalAs(UnmanagedType.LPWStr)] string query_buf, IntPtr query_len,
                [MarshalAs(UnmanagedType.LPArray), In] NativeQueryArgument[] arguments, IntPtr args_count,
                out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "set_find_value", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr find_value(SetHandle setHandle, PrimitiveValue value, out NativeException ex);
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

        public SetHandle(SharedRealmHandle root, IntPtr handle) : base(root, handle)
        {
        }

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

            using Arena arena = new();
            var nativeKeyPathsArray = MarshaledVector<StringValue>
                .AllocateFrom(keyPathsCollection.GetStrings().Select(p => StringValue.AllocateFrom(p, arena)).ToArray(), arena);

            var result = NativeMethods.add_notification_callback(this, managedObjectHandle,
                keyPathsCollection.Type, callback, nativeKeyPathsArray, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new(Root!, result);
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

            return new(result);
        }

        public ResultsHandle ToResults()
        {
            EnsureIsOpen();

            var ptr = NativeMethods.to_results(this, out var ex);
            ex.ThrowIfNecessary();

            return new(Root!, ptr);
        }

        protected override IntPtr GetFilteredResultsCore(string query, NativeQueryArgument[] arguments, out NativeException ex)
            => NativeMethods.get_filtered_results(this, query, query.IntPtrLength(), arguments, (IntPtr)arguments.Length, out ex);

        public override CollectionHandleBase Freeze(SharedRealmHandle frozenRealmHandle)
        {
            EnsureIsOpen();

            var result = NativeMethods.freeze(this, frozenRealmHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new SetHandle(frozenRealmHandle, result);
        }

        public RealmValue GetValueAtIndex(int index, Realm realm)
        {
            EnsureIsOpen();

            NativeMethods.get_value(this, (IntPtr)index, out var result, out var ex);
            ex.ThrowIfNecessary();
            return new(result, realm);
        }

        public bool Add(in RealmValue value)
        {
            var (primitive, handles) = value.ToNative();
            var result = NativeMethods.add_value(this, primitive, out var nativeException);
            handles?.Dispose();
            nativeException.ThrowIfNecessary();
            return result;
        }

        public bool Contains(in RealmValue value)
        {
            EnsureIsOpen();

            var (primitive, handles) = value.ToNative();
            var result = NativeMethods.contains_value(this, primitive, out var nativeException);
            handles?.Dispose();
            nativeException.ThrowIfNecessary();
            return result;
        }

        public bool Remove(in RealmValue value)
        {
            EnsureIsOpen();

            var (primitive, handles) = value.ToNative();
            var result = NativeMethods.remove_value(this, primitive, out var nativeException);
            handles?.Dispose();
            nativeException.ThrowIfNecessary();
            return result;
        }

        public override void Unbind() => NativeMethods.destroy(handle);

        public int Find(in RealmValue value)
        {
            EnsureIsOpen();

            var (primitive, handles) = value.ToNative();
            var result = NativeMethods.find_value(this, primitive, out var nativeException);
            handles?.Dispose();
            nativeException.ThrowIfNecessary();
            return (int)result;
        }

        protected override IntPtr SnapshotCore(out NativeException ex) => NativeMethods.snapshot(this, out ex);

        #region Set methods

        public void ExceptWith(CollectionHandleBase other)
        {
            EnsureIsOpen();

            NativeMethods.except_with(this, other, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void IntersectWith(CollectionHandleBase other)
        {
            EnsureIsOpen();

            NativeMethods.intersect_with(this, other, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void SymmetricExceptWith(CollectionHandleBase other)
        {
            EnsureIsOpen();

            NativeMethods.symmetric_except_with(this, other, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void UnionWith(CollectionHandleBase other)
        {
            EnsureIsOpen();

            NativeMethods.union_with(this, other, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public bool IsSubsetOf(CollectionHandleBase other, bool proper)
        {
            EnsureIsOpen();

            var result = NativeMethods.is_subset_of(this, other, proper, out var nativeException);
            nativeException.ThrowIfNecessary();

            return result;
        }

        public bool IsSupersetOf(CollectionHandleBase other, bool proper)
        {
            EnsureIsOpen();

            var result = NativeMethods.is_superset_of(this, other, proper, out var nativeException);
            nativeException.ThrowIfNecessary();

            return result;
        }

        public bool Overlaps(CollectionHandleBase other)
        {
            EnsureIsOpen();

            var result = NativeMethods.overlaps(this, other, out var nativeException);
            nativeException.ThrowIfNecessary();

            return result;
        }

        public bool SetEquals(CollectionHandleBase other)
        {
            EnsureIsOpen();

            var result = NativeMethods.set_equals(this, other, out var nativeException);
            nativeException.ThrowIfNecessary();

            return result;
        }

        #endregion
    }
}
