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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Realms.Exceptions;
using Realms.Native;
using Realms.Schema;

namespace Realms
{
    internal class ObjectHandle : NotifiableObjectHandleBase
    {
        private static class NativeMethods
        {
#pragma warning disable IDE0049 // Naming Styles
#pragma warning disable SA1121 // Use built-in type alias

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_is_valid", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool get_is_valid(ObjectHandle objectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr objectHandle);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_value", CallingConvention = CallingConvention.Cdecl)]
            public static extern void get_value(ObjectHandle handle, IntPtr propertyIndex, out PrimitiveValue value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_value", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_value(ObjectHandle handle, IntPtr propertyIndex, PrimitiveValue value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_create_embedded", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr create_embedded_link(ObjectHandle handle, IntPtr propertyIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_parent", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_parent(ObjectHandle handle, out TableKey tableKey, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_list", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_list(ObjectHandle handle, IntPtr propertyIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_set", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_set(ObjectHandle handle, IntPtr propertyIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_dictionary", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_dictionary(ObjectHandle handle, IntPtr propertyIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_add_int64", CallingConvention = CallingConvention.Cdecl)]
            public static extern Int64 add_int64(ObjectHandle handle, IntPtr propertyIndex, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_remove", CallingConvention = CallingConvention.Cdecl)]
            public static extern void remove(ObjectHandle handle, SharedRealmHandle realmHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_equals_object", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool equals_object(ObjectHandle handle, ObjectHandle otherHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_hashcode", CallingConvention = CallingConvention.Cdecl)]
            public static extern Int32 get_hashcode(ObjectHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_backlinks", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_backlinks(ObjectHandle objectHandle, IntPtr property_index, out NativeException nativeException);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_backlinks_for_type", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_backlinks_for_type(ObjectHandle objectHandle, TableKey table_key, IntPtr source_property_index, out NativeException nativeException);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_thread_safe_reference", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_thread_safe_reference(ObjectHandle objectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_add_notification_callback", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr add_notification_callback(ObjectHandle objectHandle, IntPtr managedObjectHandle, [MarshalAs(UnmanagedType.LPArray), In] IntPtr[] property_indices, IntPtr property_count, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_backlink_count", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_backlink_count(ObjectHandle objectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_freeze", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr freeze(ObjectHandle handle, SharedRealmHandle frozen_realm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_schema", CallingConvention = CallingConvention.Cdecl)]
            public static extern void get_schema(ObjectHandle objectHandle, IntPtr callback, out NativeException ex);

#pragma warning restore SA1121 // Use built-in type alias
#pragma warning restore IDE0049 // Naming Styles
        }

        public bool IsValid
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

        [Preserve]
        public ObjectHandle(SharedRealmHandle root, IntPtr handle) : base(root, handle)
        {
        }

        public bool ObjEquals(ObjectHandle other)
        {
            EnsureIsOpen();

            var result = NativeMethods.equals_object(this, other, out var nativeException);
            nativeException.ThrowIfNecessary();

            return result;
        }

        public int GetObjHash()
        {
            EnsureIsOpen();

            var result = NativeMethods.get_hashcode(this, out var nativeException);
            nativeException.ThrowIfNecessary();

            return result;
        }

        public override void Unbind() => NativeMethods.destroy(handle);

        public RealmValue GetValue(string propertyName, Metadata metadata, Realm realm)
        {
            EnsureIsOpen();

            var propertyIndex = metadata.GetPropertyIndex(propertyName);
            NativeMethods.get_value(this, propertyIndex, out var result, out var nativeException);
            nativeException.ThrowIfNecessary();

            return new RealmValue(result, realm, this, propertyIndex);
        }

        public RealmSchema GetSchema()
        {
            EnsureIsOpen();

            RealmSchema result = null;
            Action<Native.Schema> callback = (nativeSmallSchema) => result = RealmSchema.CreateFromObjectStoreSchema(nativeSmallSchema);
            var callbackHandle = GCHandle.Alloc(callback);

            try
            {
                NativeMethods.get_schema(this, GCHandle.ToIntPtr(callbackHandle), out var nativeException);
                nativeException.ThrowIfNecessary();
            }
            finally
            {
                callbackHandle.Free();
            }

            return result;
        }

        public void SetValue(string propertyName, Metadata metadata, in RealmValue value, Realm realm)
        {
            EnsureIsOpen();

            var propertyIndex = metadata.GetPropertyIndex(propertyName);

            // We need to special-handle objects because they need to be managed before we can set them.
            if (value.Type == RealmValueType.Object)
            {
                switch (value.AsIRealmObject())
                {
                    case IRealmObject realmObj when !realmObj.IsManaged:
                        realm.Add(realmObj);
                        break;
                    case IEmbeddedObject embeddedObj:
                        if (embeddedObj.IsManaged)
                        {
                            throw new RealmException($"Can't link to an embedded object that is already managed. Attempted to set {value} to {metadata.Schema.Name}.{propertyName}");
                        }

                        if (GetProperty(propertyName, metadata).Type.IsRealmValue())
                        {
                            throw new NotSupportedException($"A RealmValue cannot contain an embedded object. Attempted to set {value} to {metadata.Schema.Name}.{propertyName}");
                        }

                        var embeddedHandle = CreateEmbeddedObjectForProperty(propertyName, metadata);
                        realm.ManageEmbedded(embeddedObj, embeddedHandle);
                        return;

                    // Asymmetric objects can't reach this path unless the user explicitly sets them as
                    // a RealmValue property on the object.
                    // This is because:
                    // * For plain asymmetric objects (not contained within a RealmValue), the weaver
                    //   raises a compilation error since asymmetric objects can't be linked to.
                    case IAsymmetricObject:
                        throw new NotSupportedException($"Asymmetric objects cannot be linked to and cannot be contained in a RealmValue. Attempted to set {value} to {metadata.Schema.Name}.{propertyName}");
                }
            }

            var (primitive, handles) = value.ToNative();
            NativeMethods.set_value(this, propertyIndex, primitive, out var nativeException);
            handles?.Dispose();
            nativeException.ThrowIfNecessary();
        }

        public long AddInt64(IntPtr propertyIndex, long value)
        {
            EnsureIsOpen();

            var result = NativeMethods.add_int64(this, propertyIndex, value, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public void SetValueUnique(string propertyName, Metadata metadata, in RealmValue value)
        {
            EnsureIsOpen();

            var propertyIndex = metadata.GetPropertyIndex(propertyName);

            NativeMethods.get_value(this, propertyIndex, out var result, out var nativeException);
            nativeException.ThrowIfNecessary();

            // Objects can't be PKs, so realm: null is fine.
            var currentValue = new RealmValue(result, realm: null, this, propertyIndex);

            if (!currentValue.Equals(value))
            {
                throw new InvalidOperationException($"Once set, primary key properties may not be modified. Current primary key value: {currentValue}, new value: {value}");
            }
        }

        public void RemoveFromRealm(SharedRealmHandle realmHandle)
        {
            EnsureIsOpen();

            NativeMethods.remove(this, realmHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public RealmList<T> GetList<T>(Realm realm, string propertyName, Metadata metadata, string objectType)
        {
            EnsureIsOpen();

            var propertyIndex = metadata.GetPropertyIndex(propertyName);
            var listPtr = NativeMethods.get_list(this, propertyIndex, out var nativeException);
            nativeException.ThrowIfNecessary();

            var listHandle = new ListHandle(Root, listPtr);
            var objectMetadata = objectType == null ? null : realm.Metadata[objectType];
            return new RealmList<T>(realm, listHandle, objectMetadata);
        }

        public RealmSet<T> GetSet<T>(Realm realm, string propertyName, Metadata metadata, string objectType)
        {
            EnsureIsOpen();

            var propertyIndex = metadata.GetPropertyIndex(propertyName);
            var setPtr = NativeMethods.get_set(this, propertyIndex, out var nativeException);
            nativeException.ThrowIfNecessary();

            var setHandle = new SetHandle(Root, setPtr);
            var objectMetadata = objectType == null ? null : realm.Metadata[objectType];
            return new RealmSet<T>(realm, setHandle, objectMetadata);
        }

        public RealmDictionary<TValue> GetDictionary<TValue>(Realm realm, string propertyName, Metadata metadata, string objectType)
        {
            EnsureIsOpen();

            var propertyIndex = metadata.GetPropertyIndex(propertyName);
            var dictionaryPtr = NativeMethods.get_dictionary(this, propertyIndex, out var nativeException);
            nativeException.ThrowIfNecessary();

            var dictionaryHandle = new DictionaryHandle(Root, dictionaryPtr);
            var objectMetadata = objectType == null ? null : realm.Metadata[objectType];
            return new RealmDictionary<TValue>(realm, dictionaryHandle, objectMetadata);
        }

        public ObjectHandle CreateEmbeddedObjectForProperty(string propertyName, Metadata metadata)
        {
            EnsureIsOpen();

            var propertyIndex = metadata.GetPropertyIndex(propertyName);
            var objPtr = NativeMethods.create_embedded_link(this, propertyIndex, out var ex);
            ex.ThrowIfNecessary();
            return new ObjectHandle(Root, objPtr);
        }

        public ObjectHandle GetParent(out TableKey tableKey)
        {
            EnsureIsOpen();

            var parentObjPtr = NativeMethods.get_parent(this, out tableKey, out var nativeException);
            nativeException.ThrowIfNecessary();

            return new ObjectHandle(Root, parentObjPtr);
        }

        public ResultsHandle GetBacklinks(string propertyName, Metadata metadata)
        {
            EnsureIsOpen();

            var propertyIndex = metadata.GetPropertyIndex(propertyName);
            var resultsPtr = NativeMethods.get_backlinks(this, propertyIndex, out var nativeException);
            nativeException.ThrowIfNecessary();

            return new ResultsHandle(Root, resultsPtr);
        }

        public ResultsHandle GetBacklinksForType(TableKey tableKey, string propertyName, Metadata metadata)
        {
            EnsureIsOpen();

            var propertyIndex = metadata.GetPropertyIndex(propertyName);
            var resultsPtr = NativeMethods.get_backlinks_for_type(this, tableKey, propertyIndex, out var nativeException);
            nativeException.ThrowIfNecessary();

            return new ResultsHandle(Root, resultsPtr);
        }

        public int GetBacklinkCount()
        {
            EnsureIsOpen();

            var result = NativeMethods.get_backlink_count(this, out var nativeException);
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

        public override NotificationTokenHandle AddNotificationCallback(IntPtr managedObjectHandle, IntPtr[] propertyIndices = null)
        {
            EnsureIsOpen();
            IntPtr[] pIArray = propertyIndices == null ? Array.Empty<IntPtr>() : propertyIndices.ToArray();
            var result = NativeMethods.add_notification_callback(this, managedObjectHandle, pIArray, (IntPtr)pIArray.Length, out var nativeException);
            nativeException.ThrowIfNecessary();

            return new NotificationTokenHandle(Root, result);
        }

        public ObjectHandle Freeze(SharedRealmHandle frozenRealmHandle)
        {
            EnsureIsOpen();

            var result = NativeMethods.freeze(this, frozenRealmHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new ObjectHandle(frozenRealmHandle, result);
        }

        private static Property GetProperty(string propertyName, Metadata metadata)
        {
            if (metadata.Schema.TryFindProperty(propertyName, out var result))
            {
                return result;
            }

            throw new MissingMemberException(metadata.Schema.Name, propertyName);
        }
    }
}
