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

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_timestamp_ticks", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_timestamp_ticks(ObjectHandle handle, ColumnKey columnKey, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_timestamp_ticks", CallingConvention = CallingConvention.Cdecl)]
            public static extern Int64 get_timestamp_ticks(ObjectHandle handle, ColumnKey columnKey, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_nullable_timestamp_ticks", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool get_nullable_timestamp_ticks(ObjectHandle handle, ColumnKey columnKey, out Int64 retVal, out NativeException ex);

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

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_clear_link", CallingConvention = CallingConvention.Cdecl)]
            public static extern void clear_link(ObjectHandle handle, ColumnKey columnKey, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_list", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_list(ObjectHandle handle, ColumnKey columnKey, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_null", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_null(ObjectHandle handle, ColumnKey columnKey, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_bool", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_bool(ObjectHandle handle, ColumnKey columnKey, [MarshalAs(UnmanagedType.U1)] bool value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_bool", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool get_bool(ObjectHandle handle, ColumnKey columnKey, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_nullable_bool", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool get_nullable_bool(ObjectHandle handle, ColumnKey columnKey, out IntPtr retVal, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_int64", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_int64(ObjectHandle handle, ColumnKey columnKey, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_add_int64", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add_int64(ObjectHandle handle, ColumnKey columnKey, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_int64", CallingConvention = CallingConvention.Cdecl)]
            public static extern Int64 get_int64(ObjectHandle handle, ColumnKey columnKey, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_nullable_int64", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool get_nullable_int64(ObjectHandle handle, ColumnKey columnKey, out Int64 retVal, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_float", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_float(ObjectHandle handle, ColumnKey columnKey, Single value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_float", CallingConvention = CallingConvention.Cdecl)]
            public static extern Single get_float(ObjectHandle handle, ColumnKey columnKey, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_nullable_float", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool get_nullable_float(ObjectHandle handle, ColumnKey columnKey, out Single retVal, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_double", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_double(ObjectHandle handle, ColumnKey columnKey, Double value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_double", CallingConvention = CallingConvention.Cdecl)]
            public static extern Double get_double(ObjectHandle handle, ColumnKey columnKey, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_nullable_double", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool get_nullable_double(ObjectHandle handle, ColumnKey columnKey, out Double retVal, out NativeException ex);

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

        public void SetDateTimeOffset(ColumnKey columnKey, DateTimeOffset value)
        {
            var ticks = value.ToUniversalTime().Ticks;
            NativeMethods.set_timestamp_ticks(this, columnKey, ticks, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void SetNullableDateTimeOffset(ColumnKey columnKey, DateTimeOffset? value)
        {
            NativeException nativeException;
            if (value.HasValue)
            {
                var ticks = value.Value.ToUniversalTime().Ticks;
                NativeMethods.set_timestamp_ticks(this, columnKey, ticks, out nativeException);
            }
            else
            {
                NativeMethods.set_null(this, columnKey, out nativeException);
            }

            nativeException.ThrowIfNecessary();
        }

        public DateTimeOffset GetDateTimeOffset(ColumnKey columnKey)
        {
            var ticks = NativeMethods.get_timestamp_ticks(this, columnKey, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new DateTimeOffset(ticks, TimeSpan.Zero);
        }

        public DateTimeOffset? GetNullableDateTimeOffset(ColumnKey columnKey)
        {
            var hasValue = NativeMethods.get_nullable_timestamp_ticks(this, columnKey, out var ticks, out var nativeException);
            nativeException.ThrowIfNecessary();
            return hasValue ? new DateTimeOffset(ticks, TimeSpan.Zero) : (DateTimeOffset?)null;
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

        public void SetBoolean(ColumnKey columnKey, bool value)
        {
            NativeMethods.set_bool(this, columnKey, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void SetNullableBoolean(ColumnKey columnKey, bool? value)
        {
            NativeException nativeException;
            if (value.HasValue)
            {
                NativeMethods.set_bool(this, columnKey, value.Value, out nativeException);
            }
            else
            {
                NativeMethods.set_null(this, columnKey, out nativeException);
            }

            nativeException.ThrowIfNecessary();
        }

        public bool GetBoolean(ColumnKey columnKey)
        {
            var result = NativeMethods.get_bool(this, columnKey, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public bool? GetNullableBoolean(ColumnKey columnKey)
        {
            var hasValue = NativeMethods.get_nullable_bool(this, columnKey, out var value, out var nativeException);
            nativeException.ThrowIfNecessary();
            return hasValue ? MarshalHelpers.IntPtrToBool(value) : (bool?)null;
        }

        public void SetInt64(ColumnKey columnKey, long value)
        {
            NativeMethods.set_int64(this, columnKey, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void AddInt64(ColumnKey columnKey, long value)
        {
            NativeMethods.add_int64(this, columnKey, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void SetNullableInt64(ColumnKey columnKey, long? value)
        {
            NativeException nativeException;
            if (value.HasValue)
            {
                NativeMethods.set_int64(this, columnKey, value.Value, out nativeException);
            }
            else
            {
                NativeMethods.set_null(this, columnKey, out nativeException);
            }

            nativeException.ThrowIfNecessary();
        }

        public void SetInt64Unique(ColumnKey columnKey, long value)
        {
            if (GetInt64(columnKey) != value)
            {
                throw new InvalidOperationException("Once set, primary key properties may not be modified.");
            }
        }

        public void SetNullableInt64Unique(ColumnKey columnKey, long? value)
        {
            if (GetNullableInt64(columnKey) != value)
            {
                throw new InvalidOperationException("Once set, primary key properties may not be modified.");
            }
        }

        public long GetInt64(ColumnKey columnKey)
        {
            var result = NativeMethods.get_int64(this, columnKey, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public long? GetNullableInt64(ColumnKey columnKey)
        {
            var hasValue = NativeMethods.get_nullable_int64(this, columnKey, out var value, out var nativeException);
            nativeException.ThrowIfNecessary();
            return hasValue ? value : (long?)null;
        }

        public void SetSingle(ColumnKey columnKey, float value)
        {
            NativeMethods.set_float(this, columnKey, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void SetNullableSingle(ColumnKey columnKey, float? value)
        {
            NativeException nativeException;
            if (value.HasValue)
            {
                NativeMethods.set_float(this, columnKey, value.Value, out nativeException);
            }
            else
            {
                NativeMethods.set_null(this, columnKey, out nativeException);
            }

            nativeException.ThrowIfNecessary();
        }

        public float GetSingle(ColumnKey columnKey)
        {
            var result = NativeMethods.get_float(this, columnKey, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public float? GetNullableSingle(ColumnKey columnKey)
        {
            var hasValue = NativeMethods.get_nullable_float(this, columnKey, out var value, out var nativeException);
            nativeException.ThrowIfNecessary();
            return hasValue ? value : (float?)null;
        }

        public void SetDouble(ColumnKey columnKey, double value)
        {
            NativeMethods.set_double(this, columnKey, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void SetNullableDouble(ColumnKey columnKey, double? value)
        {
            NativeException nativeException;
            if (value.HasValue)
            {
                NativeMethods.set_double(this, columnKey, value.Value, out nativeException);
            }
            else
            {
                NativeMethods.set_null(this, columnKey, out nativeException);
            }

            nativeException.ThrowIfNecessary();
        }

        public double GetDouble(ColumnKey columnKey)
        {
            var result = NativeMethods.get_double(this, columnKey, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public double? GetNullableDouble(ColumnKey columnKey)
        {
            var hasValue = NativeMethods.get_nullable_double(this, columnKey, out var value, out var nativeException);
            nativeException.ThrowIfNecessary();
            return hasValue ? value : (double?)null;
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

        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The RealmObject instance will own its handle.")]
        public T GetObject<T>(Realm realm, ColumnKey columnKey, string objectType)
            where T : RealmObject
        {
            if (TryGetLink(columnKey, out var objectHandle))
            {
                return (T)realm.MakeObject(realm.Metadata[objectType], objectHandle);
            }

            return null;
        }

        public void SetObject(Realm realm, ColumnKey columnKey, RealmObject @object)
        {
            if (@object == null)
            {
                ClearLink(columnKey);
            }
            else
            {
                if (!@object.IsManaged)
                {
                    realm.Add(@object);
                }

                SetLink(columnKey, @object.ObjectHandle);
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