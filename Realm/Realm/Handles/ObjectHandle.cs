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
using System.Diagnostics.CodeAnalysis;
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
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1121 // Use built-in type alias

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_is_valid", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool get_is_valid(ObjectHandle objectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_key", CallingConvention = CallingConvention.Cdecl)]
            public static extern void get_key(ObjectHandle objectHandle, out ObjectKey key, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr objectHandle);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_primitive", CallingConvention = CallingConvention.Cdecl)]
            public static extern void get_primitive(ObjectHandle handle, IntPtr propertyIndex, ref PrimitiveValue value, out NativeException ex);

            // value is IntPtr rather than PrimitiveValue due to a bug in .NET Core on Linux and Mac
            // that causes incorrect marshalling of the struct.
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_primitive", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_primitive(ObjectHandle handle, IntPtr propertyIndex, IntPtr value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_string", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_string(ObjectHandle handle, IntPtr propertyIndex,
                [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_string", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_string(ObjectHandle handle, IntPtr propertyIndex,
                IntPtr buffer, IntPtr bufsize, [MarshalAs(UnmanagedType.U1)] out bool isNull, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_link", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_link(ObjectHandle handle, IntPtr propertyIndex, ObjectHandle targetHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_link", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_link(ObjectHandle handle, IntPtr propertyIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_create_embedded", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr create_embedded_link(ObjectHandle handle, IntPtr propertyIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_clear_link", CallingConvention = CallingConvention.Cdecl)]
            public static extern void clear_link(ObjectHandle handle, IntPtr propertyIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_list", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_list(ObjectHandle handle, IntPtr propertyIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_null", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_null(ObjectHandle handle, IntPtr propertyIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_add_int64", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add_int64(ObjectHandle handle, IntPtr propertyIndex, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_binary", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr set_binary(ObjectHandle handle, IntPtr propertyIndex,
                IntPtr buffer, IntPtr bufferLength, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_binary", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_binary(ObjectHandle handle, IntPtr propertyIndex,
                IntPtr buffer, IntPtr bufferLength, [MarshalAs(UnmanagedType.U1)] out bool is_null, out NativeException ex);

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

        public PrimitiveValue GetPrimitive(IntPtr propertyIndex, PropertyType type)
        {
            var result = new PrimitiveValue { Type = type };

            NativeMethods.get_primitive(this, propertyIndex, ref result, out var nativeException);
            nativeException.ThrowIfNecessary();

            return result;
        }

        public unsafe void SetPrimitive(IntPtr propertyIndex, PrimitiveValue value)
        {
            PrimitiveValue* valuePtr = &value;
            NativeMethods.set_primitive(this, propertyIndex, new IntPtr(valuePtr), out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void SetString(IntPtr propertyIndex, string value)
        {
            NativeException nativeException;
            if (value != null)
            {
                NativeMethods.set_string(this, propertyIndex, value, (IntPtr)value.Length, out nativeException);
            }
            else
            {
                NativeMethods.set_null(this, propertyIndex, out nativeException);
            }

            nativeException.ThrowIfNecessary();
        }

        public void SetStringUnique(IntPtr propertyIndex, string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value), "Object identifiers cannot be null");
            }

            if (GetString(propertyIndex) != value)
            {
                throw new InvalidOperationException("Once set, primary key properties may not be modified.");
            }
        }

        public string GetString(IntPtr propertyIndex)
        {
            return MarshalHelpers.GetString((IntPtr buffer, IntPtr length, out bool isNull, out NativeException ex) => NativeMethods.get_string(this, propertyIndex, buffer, length, out isNull, out ex));
        }

        public void SetLink(IntPtr propertyIndex, ObjectHandle targetHandle)
        {
            NativeMethods.set_link(this, propertyIndex, targetHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void ClearLink(IntPtr propertyIndex)
        {
            NativeMethods.clear_link(this, propertyIndex, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public bool TryGetLink(IntPtr propertyIndex, out ObjectHandle objectHandle)
        {
            var result = NativeMethods.get_link(this, propertyIndex, out var nativeException);
            nativeException.ThrowIfNecessary();

            if (result == IntPtr.Zero)
            {
                objectHandle = null;
                return false;
            }

            objectHandle = new ObjectHandle(Root, result);
            return true;
        }

        public IntPtr GetLinklist(IntPtr propertyIndex)
        {
            var result = NativeMethods.get_list(this, propertyIndex, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public void AddInt64(IntPtr propertyIndex, long value)
        {
            NativeMethods.add_int64(this, propertyIndex, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void SetPrimitiveUnique(IntPtr propertyIndex, PrimitiveValue value)
        {
            if (!GetPrimitive(propertyIndex, value.Type).Equals(value))
            {
                throw new InvalidOperationException("Once set, primary key properties may not be modified.");
            }
        }

        public unsafe void SetByteArray(IntPtr propertyIndex, byte[] value)
        {
            MarshalHelpers.SetByteArray(value, (IntPtr buffer, IntPtr bufferSize, bool hasValue, out NativeException ex) =>
            {
                if (hasValue)
                {
                    NativeMethods.set_binary(this, propertyIndex, buffer, bufferSize, out ex);
                }
                else
                {
                    NativeMethods.set_null(this, propertyIndex, out ex);
                }
            });
        }

        public byte[] GetByteArray(IntPtr propertyIndex)
        {
            return MarshalHelpers.GetByteArray((IntPtr buffer, IntPtr bufferLength, out bool isNull, out NativeException ex) =>
                NativeMethods.get_binary(this, propertyIndex, buffer, bufferLength, out isNull, out ex));
        }

        public void RemoveFromRealm(SharedRealmHandle realmHandle)
        {
            NativeMethods.remove(this, realmHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public RealmList<T> GetList<T>(Realm realm, IntPtr propertyIndex, string objectType)
        {
            var listHandle = new ListHandle(Root, GetLinklist(propertyIndex));
            var metadata = objectType == null ? null : realm.Metadata[objectType];
            return new RealmList<T>(realm, listHandle, metadata);
        }

        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The RealmObjectBase instance will own its handle.")]
        public T GetObject<T>(Realm realm, IntPtr propertyIndex, string objectType)
            where T : RealmObjectBase
        {
            if (TryGetLink(propertyIndex, out var objectHandle))
            {
                return (T)realm.MakeObject(realm.Metadata[objectType], objectHandle);
            }

            return null;
        }

        public void SetObject(Realm realm, IntPtr propertyIndex, RealmObjectBase @object)
        {
            if (@object == null)
            {
                ClearLink(propertyIndex);
            }
            else if (@object is RealmObject realmObj)
            {
                if (!realmObj.IsManaged)
                {
                    realm.Add(realmObj);
                }

                SetLink(propertyIndex, realmObj.ObjectHandle);
            }
            else if (@object is EmbeddedObject embeddedObj)
            {
                if (embeddedObj.IsManaged)
                {
                    throw new RealmException("Can't link to an embedded object that is already managed.");
                }

                var handle = CreateEmbeddedObjectForProperty(propertyIndex);
                realm.ManageEmbedded(embeddedObj, handle);
            }
            else
            {
                throw new NotSupportedException($"Tried to add an object of type {@object.GetType().FullName} which does not inherit from RealmObject or EmbeddedObject");
            }
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

        public ObjectKey GetKey()
        {
            NativeMethods.get_key(this, out var result, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public ObjectHandle Freeze(SharedRealmHandle frozenRealmHandle)
        {
            var result = NativeMethods.freeze(this, frozenRealmHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new ObjectHandle(frozenRealmHandle, result);
        }
    }
}