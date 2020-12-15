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
using Realms.Exceptions;
using Realms.Native;

namespace Realms
{
    internal class ObjectHandle : NotifiableObjectHandleBase
    {
        private static class NativeMethods
        {
#pragma warning disable IDE1006 // Naming Styles
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

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_list", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_list(ObjectHandle handle, IntPtr propertyIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_set", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_set(ObjectHandle handle, IntPtr propertyIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_dictionary", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_dictionary(ObjectHandle handle, IntPtr propertyIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_add_int64", CallingConvention = CallingConvention.Cdecl)]
            public static extern Int64 add_int64(ObjectHandle handle, IntPtr propertyIndex, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_remove", CallingConvention = CallingConvention.Cdecl)]
            public static extern void remove(ObjectHandle handle, RealmHandle realmHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_equals_object", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool equals_object(ObjectHandle handle, ObjectHandle otherHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_backlinks", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_backlinks(ObjectHandle objectHandle, IntPtr property_index, out NativeException nativeException);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_backlinks_for_type", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_backlinks_for_type(ObjectHandle objectHandle, TableHandle source_table, IntPtr source_property_index, out NativeException nativeException);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_thread_safe_reference", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_thread_safe_reference(ObjectHandle objectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_add_notification_callback", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr add_notification_callback(ObjectHandle objectHandle, IntPtr managedObjectHandle, NotificationCallbackDelegate callback, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_backlink_count", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_backlink_count(ObjectHandle objectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_is_frozen", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool get_is_frozen(ObjectHandle objectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_freeze", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr freeze(ObjectHandle handle, SharedRealmHandle frozen_realm, out NativeException ex);

#pragma warning restore SA1121 // Use built-in type alias
#pragma warning restore IDE1006 // Naming Styles
        }

        public bool IsValid
        {
            get
            {
                var result = NativeMethods.get_is_valid(this, out var nativeException);
                nativeException.ThrowIfNecessary();
                return result;
            }
        }

        [Preserve]
        public ObjectHandle(RealmHandle root, IntPtr handle) : base(root, handle)
        {
        }

        public override bool Equals(object obj)
        {
            // If parameter is null, return false.
            if (obj is null)
            {
                return false;
            }

            // Optimization for a common success case.
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (!(obj is ObjectHandle otherHandle))
            {
                return false;
            }

            var result = NativeMethods.equals_object(this, otherHandle, out var nativeException);
            nativeException.ThrowIfNecessary();

            return result;
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

        protected override void Unbind()
        {
            NativeMethods.destroy(handle);
        }

        public RealmValue GetValue(string propertyName, RealmObjectBase.Metadata metadata, Realm realm)
        {
            var propertyIndex = metadata.PropertyIndices[propertyName];
            NativeMethods.get_value(this, propertyIndex, out var result, out var nativeException);
            nativeException.ThrowIfNecessary();

            if (result.Type != RealmValueType.Object)
            {
                return new RealmValue(result, this, propertyIndex);
            }

            var objectHandle = result.AsObject(Root);
            metadata.Schema.TryFindProperty(propertyName, out var property);
            return new RealmValue(realm.MakeObject(realm.Metadata[property.ObjectType], objectHandle));
        }

        public void SetValue(IntPtr propertyIndex, in RealmValue value, Realm realm)
        {
            // We need to special-handle objects because they need to be managed before we can set them.
            if (value.Type == RealmValueType.Object)
            {
                switch (value.AsRealmObject())
                {
                    case RealmObject realmObj when !realmObj.IsManaged:
                        realm.Add(realmObj);
                        break;
                    case EmbeddedObject embeddedObj:
                        if (embeddedObj.IsManaged)
                        {
                            throw new RealmException("Can't link to an embedded object that is already managed.");
                        }

                        var handle = CreateEmbeddedObjectForProperty(propertyIndex);
                        realm.ManageEmbedded(embeddedObj, handle);
                        return;
                }
            }

            var (primitive, handles) = value.ToNative();
            NativeMethods.set_value(this, propertyIndex, primitive, out var nativeException);
            handles?.Dispose();
            nativeException.ThrowIfNecessary();
        }

        public IntPtr GetRealmList(IntPtr propertyIndex)
        {
            var result = NativeMethods.get_list(this, propertyIndex, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public IntPtr GetRealmSet(IntPtr propertyIndex)
        {
            var result = NativeMethods.get_set(this, propertyIndex, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public IntPtr GetRealmDictionary(IntPtr propertyIndex)
        {
            var result = NativeMethods.get_dictionary(this, propertyIndex, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public long AddInt64(IntPtr propertyIndex, long value)
        {
            var result = NativeMethods.add_int64(this, propertyIndex, value, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public void SetValueUnique(IntPtr propertyIndex, in RealmValue value)
        {
            NativeMethods.get_value(this, propertyIndex, out var result, out var nativeException);
            nativeException.ThrowIfNecessary();
            var currentValue = new RealmValue(result, this, propertyIndex);

            if (!currentValue.Equals(value))
            {
                throw new InvalidOperationException("Once set, primary key properties may not be modified.");
            }
        }

        public void RemoveFromRealm(SharedRealmHandle realmHandle)
        {
            NativeMethods.remove(this, realmHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public RealmList<T> GetList<T>(Realm realm, IntPtr propertyIndex, string objectType)
        {
            var metadata = objectType == null ? null : realm.Metadata[objectType];
            var listHandle = new ListHandle(Root, GetRealmList(propertyIndex));
            return new RealmList<T>(realm, listHandle, metadata);
        }

        public RealmSet<T> GetSet<T>(Realm realm, IntPtr propertyIndex, string objectType)
        {
            var setHandle = new SetHandle(Root, GetRealmSet(propertyIndex));
            var metadata = objectType == null ? null : realm.Metadata[objectType];
            return new RealmSet<T>(realm, setHandle, metadata);
        }

        public RealmDictionary<TValue> GetDictionary<TValue>(Realm realm, IntPtr propertyIndex, string objectType)
        {
            var dictionaryHandle = new DictionaryHandle(Root, GetRealmDictionary(propertyIndex));
            var metadata = objectType == null ? null : realm.Metadata[objectType];
            return new RealmDictionary<TValue>(realm, dictionaryHandle, metadata);
        }

        public ObjectHandle CreateEmbeddedObjectForProperty(IntPtr propertyIndex)
        {
            var objPtr = NativeMethods.create_embedded_link(this, propertyIndex, out var ex);
            ex.ThrowIfNecessary();
            return new ObjectHandle(Root, objPtr);
        }

        public ResultsHandle GetBacklinks(IntPtr propertyIndex)
        {
            var resultsPtr = NativeMethods.get_backlinks(this, propertyIndex, out var nativeException);
            nativeException.ThrowIfNecessary();

            return new ResultsHandle(this, resultsPtr);
        }

        public ResultsHandle GetBacklinksForType(TableHandle table, IntPtr propertyIndex)
        {
            var resultsPtr = NativeMethods.get_backlinks_for_type(this, table, propertyIndex, out var nativeException);
            nativeException.ThrowIfNecessary();

            return new ResultsHandle(this, resultsPtr);
        }

        public int GetBacklinkCount()
        {
            var result = NativeMethods.get_backlink_count(this, out var nativeException);
            nativeException.ThrowIfNecessary();
            return (int)result;
        }

        public override ThreadSafeReferenceHandle GetThreadSafeReference()
        {
            var result = NativeMethods.get_thread_safe_reference(this, out var nativeException);
            nativeException.ThrowIfNecessary();

            return new ThreadSafeReferenceHandle(result);
        }

        public override NotificationTokenHandle AddNotificationCallback(IntPtr managedObjectHandle, NotificationCallbackDelegate callback)
        {
            var result = NativeMethods.add_notification_callback(this, managedObjectHandle, callback, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new NotificationTokenHandle(this, result);
        }

        public ObjectHandle Freeze(SharedRealmHandle frozenRealmHandle)
        {
            var result = NativeMethods.freeze(this, frozenRealmHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new ObjectHandle(frozenRealmHandle, result);
        }
    }
}