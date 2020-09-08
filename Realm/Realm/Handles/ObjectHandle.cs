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
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool get_is_valid(ObjectHandle objectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_key", CallingConvention = CallingConvention.Cdecl)]
            public static extern void get_key(ObjectHandle objectHandle, out ObjectKey key, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr objectHandle);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_primitive", CallingConvention = CallingConvention.Cdecl)]
            public static extern void get_primitive(ObjectHandle handle, ColumnKey columnKey, ref PrimitiveValue value, out NativeException ex);

            // value is IntPtr rather than PrimitiveValue due to a bug in .NET Core on Linux and Mac
            // that causes incorrect marshalling of the struct.
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_primitive", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_primitive(ObjectHandle handle, ColumnKey columnKey, IntPtr value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_string", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_string(ObjectHandle handle, ColumnKey columnKey,
                [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_string", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_string(ObjectHandle handle, ColumnKey columnKey,
                IntPtr buffer, IntPtr bufsize, [MarshalAs(UnmanagedType.I1)] out bool isNull, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_link", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_link(ObjectHandle handle, ColumnKey columnKey, ObjectHandle targetHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_link", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_link(ObjectHandle handle, ColumnKey columnKey, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_create_embedded", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr create_embedded_link(ObjectHandle handle, ColumnKey columnKey, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_clear_link", CallingConvention = CallingConvention.Cdecl)]
            public static extern void clear_link(ObjectHandle handle, ColumnKey columnKey, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_list", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_list(ObjectHandle handle, ColumnKey columnKey, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_null", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_null(ObjectHandle handle, ColumnKey columnKey, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_add_int64", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add_int64(ObjectHandle handle, ColumnKey columnKey, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_binary", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr set_binary(ObjectHandle handle, ColumnKey columnKey,
                IntPtr buffer, IntPtr bufferLength, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_binary", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_binary(ObjectHandle handle, ColumnKey columnKey,
                IntPtr buffer, IntPtr bufferLength, [MarshalAs(UnmanagedType.I1)] out bool is_null, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_remove", CallingConvention = CallingConvention.Cdecl)]
            public static extern void remove(ObjectHandle handle, RealmHandle realmHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_equals_object", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool equals_object(ObjectHandle handle, ObjectHandle otherHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_backlinks", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_backlinks(ObjectHandle objectHandle, IntPtr property_index, out NativeException nativeException);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_backlinks_for_type", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_backlinks_for_type(ObjectHandle objectHandle, TableHandle source_table, ColumnKey source_column_key, out NativeException nativeException);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_thread_safe_reference", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_thread_safe_reference(ObjectHandle objectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_add_notification_callback", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr add_notification_callback(ObjectHandle objectHandle, IntPtr managedObjectHandle, NotificationCallbackDelegate callback, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_backlink_count", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_backlink_count(ObjectHandle objectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_is_frozen", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
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

        public PrimitiveValue GetPrimitive(ColumnKey columnKey, PropertyType type)
        {
            var result = new PrimitiveValue { Type = type };

            NativeMethods.get_primitive(this, columnKey, ref result, out var nativeException);
            nativeException.ThrowIfNecessary();

            return result;
        }

        public unsafe void SetPrimitive(ColumnKey columnKey, PrimitiveValue value)
        {
            PrimitiveValue* valuePtr = &value;
            NativeMethods.set_primitive(this, columnKey, new IntPtr(valuePtr), out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void SetString(ColumnKey columnKey, string value)
        {
            NativeException nativeException;
            if (value != null)
            {
                NativeMethods.set_string(this, columnKey, value, (IntPtr)value.Length, out nativeException);
            }
            else
            {
                NativeMethods.set_null(this, columnKey, out nativeException);
            }

            nativeException.ThrowIfNecessary();
        }

        public void SetStringUnique(ColumnKey columnKey, string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value), "Object identifiers cannot be null");
            }

            if (GetString(columnKey) != value)
            {
                throw new InvalidOperationException("Once set, primary key properties may not be modified.");
            }
        }

        public string GetString(ColumnKey columnKey)
        {
            return MarshalHelpers.GetString((IntPtr buffer, IntPtr length, out bool isNull, out NativeException ex) => NativeMethods.get_string(this, columnKey, buffer, length, out isNull, out ex));
        }

        public void SetLink(ColumnKey columnKey, ObjectHandle targetHandle)
        {
            NativeMethods.set_link(this, columnKey, targetHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void ClearLink(ColumnKey columnKey)
        {
            NativeMethods.clear_link(this, columnKey, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public bool TryGetLink(ColumnKey columnKey, out ObjectHandle objectHandle)
        {
            var result = NativeMethods.get_link(this, columnKey, out var nativeException);
            nativeException.ThrowIfNecessary();

            if (result == IntPtr.Zero)
            {
                objectHandle = null;
                return false;
            }

            objectHandle = new ObjectHandle(Root, result);
            return true;
        }

        public IntPtr GetLinklist(ColumnKey columnKey)
        {
            var result = NativeMethods.get_list(this, columnKey, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public void AddInt64(ColumnKey columnKey, long value)
        {
            NativeMethods.add_int64(this, columnKey, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void SetPrimitiveUnique(ColumnKey columnKey, PrimitiveValue value)
        {
            if (!GetPrimitive(columnKey, value.Type).Equals(value))
            {
                throw new InvalidOperationException("Once set, primary key properties may not be modified.");
            }
        }

        public unsafe void SetByteArray(ColumnKey columnKey, byte[] value)
        {
            MarshalHelpers.SetByteArray(value, (IntPtr buffer, IntPtr bufferSize, bool hasValue, out NativeException ex) =>
            {
                if (hasValue)
                {
                    NativeMethods.set_binary(this, columnKey, buffer, bufferSize, out ex);
                }
                else
                {
                    NativeMethods.set_null(this, columnKey, out ex);
                }
            });
        }

        public byte[] GetByteArray(ColumnKey columnKey)
        {
            return MarshalHelpers.GetByteArray((IntPtr buffer, IntPtr bufferLength, out bool isNull, out NativeException ex) =>
                NativeMethods.get_binary(this, columnKey, buffer, bufferLength, out isNull, out ex));
        }

        public void RemoveFromRealm(SharedRealmHandle realmHandle)
        {
            NativeMethods.remove(this, realmHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public RealmList<T> GetList<T>(Realm realm, ColumnKey columnKey, string objectType)
        {
            var listHandle = new ListHandle(Root ?? this, GetLinklist(columnKey));
            var metadata = objectType == null ? null : realm.Metadata[objectType];
            return new RealmList<T>(realm, listHandle, metadata);
        }

        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The RealmObjectBase instance will own its handle.")]
        public T GetObject<T>(Realm realm, ColumnKey columnKey, string objectType)
            where T : RealmObjectBase
        {
            if (TryGetLink(columnKey, out var objectHandle))
            {
                return (T)realm.MakeObject(realm.Metadata[objectType], objectHandle);
            }

            return null;
        }

        public void SetObject(RealmObjectBase parent, ColumnKey columnKey, RealmObjectBase @object)
        {
            if (@object == null)
            {
                ClearLink(columnKey);
            }
            else if (@object is RealmObject realmObj)
            {
                if (!realmObj.IsManaged)
                {
                    parent.Realm.Add(realmObj);
                }

                SetLink(columnKey, realmObj.ObjectHandle);
            }
            else if (@object is EmbeddedObject embeddedObj)
            {
                if (embeddedObj.IsManaged)
                {
                    throw new NotSupportedException("Can't link to an embedded object that is already managed.");
                }

                var objPtr = NativeMethods.create_embedded_link(this, columnKey, out var ex);
                ex.ThrowIfNecessary();
                var handle = new ObjectHandle(parent.Realm.SharedRealmHandle, objPtr);
                parent.Realm.ManageEmbedded(embeddedObj, handle);
            }
            else
            {
                throw new NotSupportedException($"Tried to add an object of type {@object.GetType().FullName} which does not inherit from RealmObject or EmbeddedObject");
            }
        }

        public ResultsHandle GetBacklinks(IntPtr propertyIndex)
        {
            var resultsPtr = NativeMethods.get_backlinks(this, propertyIndex, out var nativeException);
            nativeException.ThrowIfNecessary();

            return new ResultsHandle(this, resultsPtr);
        }

        public ResultsHandle GetBacklinksForType(TableHandle table, ColumnKey columnKey)
        {
            var resultsPtr = NativeMethods.get_backlinks_for_type(this, table, columnKey, out var nativeException);
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