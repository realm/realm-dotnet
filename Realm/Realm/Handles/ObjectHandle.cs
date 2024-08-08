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
using System.Linq;
using System.Runtime.InteropServices;
using Realms.Exceptions;
using Realms.Extensions;
using Realms.Helpers;
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

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_unset_property", CallingConvention = CallingConvention.Cdecl)]
            public static extern bool unset_property(ObjectHandle handle, StringValue propertyName, bool throw_on_unsuccessful, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_value_by_name", CallingConvention = CallingConvention.Cdecl)]
            public static extern bool get_value_by_name(ObjectHandle handle, StringValue propertyName, out PrimitiveValue value, bool throw_on_missing_property, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_value_by_name", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_value_by_name(ObjectHandle handle, StringValue propertyName, PrimitiveValue value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_collection_value", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr set_collection_value(ObjectHandle handle, IntPtr propertyIndex, RealmValueType type, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_collection_value_by_name", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr set_collection_value_by_name(ObjectHandle handle, StringValue propertyName, RealmValueType type, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_extra_properties", CallingConvention = CallingConvention.Cdecl)]
            public static extern StringsContainer get_extra_properties(ObjectHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_has_property", CallingConvention = CallingConvention.Cdecl)]
            public static extern bool has_property(ObjectHandle handle, StringValue propertyName, out NativeException ex);

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
            public static extern IntPtr add_notification_callback(ObjectHandle objectHandle, IntPtr managedObjectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_backlink_count", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_backlink_count(ObjectHandle objectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_freeze", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr freeze(ObjectHandle handle, SharedRealmHandle frozen_realm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_schema", CallingConvention = CallingConvention.Cdecl)]
            public static extern void get_schema(ObjectHandle objectHandle, IntPtr callback, bool include_extra_properties, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_property", CallingConvention = CallingConvention.Cdecl)]
            public static extern bool get_property(ObjectHandle objectHandle, StringValue propertyName, out SchemaProperty property, out NativeException ex);
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

        public ObjectSchema GetSchema(bool includeExtraProperties = false)
        {
            EnsureIsOpen();

            ObjectSchema? result = null;
            Action<Native.Schema> callback = (nativeSmallSchema) => result = new ObjectSchema(nativeSmallSchema.objects[0]);
            var callbackHandle = GCHandle.Alloc(callback);

            try
            {
                NativeMethods.get_schema(this, GCHandle.ToIntPtr(callbackHandle), includeExtraProperties, out var nativeException);
                nativeException.ThrowIfNecessary();
            }
            finally
            {
                callbackHandle.Free();
            }

            return result!;
        }

        public bool TryGetProperty(string propertyName, out Property property)
        {
            EnsureIsOpen();

            using Arena arena = new();
            var propertyNameNative = StringValue.AllocateFrom(propertyName, arena);

            var propertyFound = NativeMethods.get_property(this, propertyNameNative, out var schemaProp, out var nativeException);
            nativeException.ThrowIfNecessary();

            if (propertyFound)
            {
                property = new Property(schemaProp);
                return true;
            }

            property = default;
            return false;
        }

        public RealmValue GetValue(string propertyName, Metadata metadata, Realm realm)
        {
            TryGetValueInternal(propertyName, metadata, realm, out var value, throwOnMissingProperty: true);
            return value;
        }

        public bool TryGetValue(string propertyName, Metadata metadata, Realm realm, out RealmValue value)
        {
            return TryGetValueInternal(propertyName, metadata, realm, out value, throwOnMissingProperty: false);
        }

        private bool TryGetValueInternal(string propertyName, Metadata metadata, Realm realm, out RealmValue value,
            bool throwOnMissingProperty)
        {
            EnsureIsOpen();

            if (metadata.TryGetPropertyIndex(propertyName, out var propertyIndex,
                throwOnMissing: !realm.Config.RelaxedSchema))
            {
                NativeMethods.get_value(this, propertyIndex, out var result, out var nativeException);
                nativeException.ThrowIfNecessary();

                value = new RealmValue(result, realm, this, propertyIndex);
                return true;
            }
            else
            {
                using Arena arena = new();
                var propertyNameNative = StringValue.AllocateFrom(propertyName, arena);

                var propFound = NativeMethods.get_value_by_name(this, propertyNameNative, out var result, throwOnMissingProperty, out var nativeException);
                nativeException.ThrowIfNecessary();

                value = new RealmValue(result, realm, this);
                return propFound;
            }
        }

        public void SetValue(string propertyName, Metadata metadata, in RealmValue value, Realm realm)
        {
            EnsureIsOpen();

            if (metadata.TryGetPropertyIndex(propertyName, out var propertyIndex, throwOnMissing: !realm.Config.RelaxedSchema))
            {
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
                                NativeMethods.get_value(this, propertyIndex, out var existingValue, out var ex);
                                ex.ThrowIfNecessary();
                                if (existingValue.TryGetObjectHandle(realm, out var existingObjectHandle) &&
                                    embeddedObj.GetObjectHandle()!.ObjEquals(existingObjectHandle))
                                {
                                    // We're trying to set an object to the same value - treat it as a no-op.
                                    return;
                                }

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
                else if (value.Type.IsCollection())
                {
                    var collectionPtr = NativeMethods.set_collection_value(this, propertyIndex, value.Type, out var collNativeException);
                    collNativeException.ThrowIfNecessary();

                    switch (value.Type)
                    {
                        case RealmValueType.List:
                            CollectionHelpers.PopulateCollection(realm, new ListHandle(Root!, collectionPtr), value);
                            break;
                        case RealmValueType.Dictionary:
                            CollectionHelpers.PopulateCollection(realm, new DictionaryHandle(Root!, collectionPtr), value);
                            break;
                        default:
                            break;
                    }

                    return;
                }

                var (primitive, handles) = value.ToNative();
                NativeMethods.set_value(this, propertyIndex, primitive, out var nativeException);
                handles?.Dispose();
                nativeException.ThrowIfNecessary();
            }
            else
            {
                using Arena arena = new();
                var propertyNameNative = StringValue.AllocateFrom(propertyName, arena);

                if (value.Type == RealmValueType.Object)
                {
                    switch (value.AsIRealmObject())
                    {
                        case IRealmObject realmObj when !realmObj.IsManaged:
                            realm.Add(realmObj);
                            break;
                        case IEmbeddedObject:
                            throw new NotSupportedException($"A RealmValue cannot contain an embedded object. Attempted to set {value} to {metadata.Schema.Name}.{propertyName}");
                        case IAsymmetricObject:
                            throw new NotSupportedException($"Asymmetric objects cannot be linked to and cannot be contained in a RealmValue. Attempted to set {value} to {metadata.Schema.Name}.{propertyName}");
                    }
                }
                else if (value.Type.IsCollection())
                {
                    var collectionPtr = NativeMethods.set_collection_value_by_name(this, propertyNameNative, value.Type, out var collNativeException);
                    collNativeException.ThrowIfNecessary();

                    switch (value.Type)
                    {
                        case RealmValueType.List:
                            CollectionHelpers.PopulateCollection(realm, new ListHandle(Root!, collectionPtr), value);
                            break;
                        case RealmValueType.Dictionary:
                            CollectionHelpers.PopulateCollection(realm, new DictionaryHandle(Root!, collectionPtr), value);
                            break;
                        default:
                            break;
                    }

                    return;
                }

                var (primitive, handles) = value.ToNative();
                NativeMethods.set_value_by_name(this, propertyNameNative, primitive, out var nativeException);
                handles?.Dispose();
                nativeException.ThrowIfNecessary();
            }
        }

        public void UnsetProperty(string propertyName)
        {
            TryUnsetPropertyInternal(propertyName, throwOnUnsuccessful: true);
        }

        public bool TryUnsetProperty(string propertyName)
        {
            return TryUnsetPropertyInternal(propertyName, throwOnUnsuccessful: false);
        }

        private bool TryUnsetPropertyInternal(string propertyName, bool throwOnUnsuccessful)
        {
            EnsureIsOpen();

            using Arena arena = new();
            var propertyNameNative = StringValue.AllocateFrom(propertyName, arena);

            var propertyFound = NativeMethods.unset_property(this, propertyNameNative, throwOnUnsuccessful, out var nativeException);
            nativeException.ThrowIfNecessary();
            return propertyFound;
        }

        //TODO This is not used atm. We could remove it
        public IEnumerable<string> GetExtraProperties()
        {
            EnsureIsOpen();

            var value = NativeMethods.get_extra_properties(this, out var nativeException);
            nativeException.ThrowIfNecessary();

            return value.Strings.ToEnumerable().Select(v => v.ToDotnetString()!);
        }

        public bool HasProperty(string propertyName)
        {
            EnsureIsOpen();

            using Arena arena = new();
            var propertyNameNative = StringValue.AllocateFrom(propertyName, arena);

            var value = NativeMethods.has_property(this, propertyNameNative, out var nativeException);
            nativeException.ThrowIfNecessary();

            return value;
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

            metadata.TryGetPropertyIndex(propertyName, out var propertyIndex, throwOnMissing: true);

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

        public RealmList<T> GetList<T>(Realm realm, string propertyName, Metadata metadata, string? objectType)
        {
            EnsureIsOpen();

            metadata.TryGetPropertyIndex(propertyName, out var propertyIndex, throwOnMissing: true);
            var listPtr = NativeMethods.get_list(this, propertyIndex, out var nativeException);
            nativeException.ThrowIfNecessary();

            var listHandle = new ListHandle(Root!, listPtr);
            var objectMetadata = objectType == null ? null : realm.Metadata[objectType];
            return new RealmList<T>(realm, listHandle, objectMetadata);
        }

        public RealmSet<T> GetSet<T>(Realm realm, string propertyName, Metadata metadata, string? objectType)
        {
            EnsureIsOpen();

            metadata.TryGetPropertyIndex(propertyName, out var propertyIndex, throwOnMissing: true);
            var setPtr = NativeMethods.get_set(this, propertyIndex, out var nativeException);
            nativeException.ThrowIfNecessary();

            var setHandle = new SetHandle(Root!, setPtr);
            var objectMetadata = objectType == null ? null : realm.Metadata[objectType];
            return new RealmSet<T>(realm, setHandle, objectMetadata);
        }

        public RealmDictionary<TValue> GetDictionary<TValue>(Realm realm, string propertyName, Metadata metadata, string? objectType)
        {
            EnsureIsOpen();

            metadata.TryGetPropertyIndex(propertyName, out var propertyIndex, throwOnMissing: true);
            var dictionaryPtr = NativeMethods.get_dictionary(this, propertyIndex, out var nativeException);
            nativeException.ThrowIfNecessary();

            var dictionaryHandle = new DictionaryHandle(Root!, dictionaryPtr);
            var objectMetadata = objectType == null ? null : realm.Metadata[objectType];
            return new RealmDictionary<TValue>(realm, dictionaryHandle, objectMetadata);
        }

        public ObjectHandle CreateEmbeddedObjectForProperty(string propertyName, Metadata metadata)
        {
            EnsureIsOpen();

            metadata.TryGetPropertyIndex(propertyName, out var propertyIndex, throwOnMissing: true);
            var objPtr = NativeMethods.create_embedded_link(this, propertyIndex, out var ex);
            ex.ThrowIfNecessary();
            return new ObjectHandle(Root!, objPtr);
        }

        public ObjectHandle GetParent(out TableKey tableKey)
        {
            EnsureIsOpen();

            var parentObjPtr = NativeMethods.get_parent(this, out tableKey, out var nativeException);
            nativeException.ThrowIfNecessary();

            return new ObjectHandle(Root!, parentObjPtr);
        }

        public ResultsHandle GetBacklinks(string propertyName, Metadata metadata)
        {
            EnsureIsOpen();

            metadata.TryGetPropertyIndex(propertyName, out var propertyIndex, throwOnMissing: true);
            var resultsPtr = NativeMethods.get_backlinks(this, propertyIndex, out var nativeException);
            nativeException.ThrowIfNecessary();

            return new ResultsHandle(Root!, resultsPtr);
        }

        public ResultsHandle GetBacklinksForType(TableKey tableKey, string propertyName, Metadata metadata)
        {
            EnsureIsOpen();

            metadata.TryGetPropertyIndex(propertyName, out var propertyIndex, throwOnMissing: true);
            var resultsPtr = NativeMethods.get_backlinks_for_type(this, tableKey, propertyIndex, out var nativeException);
            nativeException.ThrowIfNecessary();

            return new ResultsHandle(Root!, resultsPtr);
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

        public NotificationTokenHandle AddNotificationCallback(IntPtr managedObjectHandle)
        {
            EnsureIsOpen();
            var result = NativeMethods.add_notification_callback(this, managedObjectHandle, out var nativeException);
            nativeException.ThrowIfNecessary();

            return new NotificationTokenHandle(Root!, result);
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
