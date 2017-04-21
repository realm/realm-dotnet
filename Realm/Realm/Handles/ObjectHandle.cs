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
using System.Linq;
using System.Runtime.InteropServices;

namespace Realms
{
    internal class ObjectHandle : NotifiableObjectHandleBase
    {
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter")]
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1121:UseBuiltInTypeAlias")]
        private static class NativeMethods
        {
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_is_valid", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool get_is_valid(ObjectHandle objectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_row_index", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_row_index(ObjectHandle objectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr objectHandle);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_timestamp_ticks", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_timestamp_ticks(ObjectHandle handle, IntPtr propertyIndex, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_timestamp_ticks", CallingConvention = CallingConvention.Cdecl)]
            public static extern Int64 get_timestamp_ticks(ObjectHandle handle, IntPtr propertyIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_nullable_timestamp_ticks", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_nullable_timestamp_ticks(ObjectHandle handle, IntPtr propertyIndex, out Int64 retVal, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_string", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_string(ObjectHandle handle, IntPtr propertyIndex,
                [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_string_unique", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_string_unique(ObjectHandle handle, IntPtr propertyIndex,
                [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_string", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_string(ObjectHandle handle, IntPtr propertyIndex,
                IntPtr buffer, IntPtr bufsize, [MarshalAs(UnmanagedType.I1)] out bool isNull, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_link", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_link(ObjectHandle handle, IntPtr propertyIndex, ObjectHandle targetHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_link", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_link(ObjectHandle handle, IntPtr propertyIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_clear_link", CallingConvention = CallingConvention.Cdecl)]
            public static extern void clear_link(ObjectHandle handle, IntPtr propertyIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_list_is_empty", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr list_is_empty(ObjectHandle handle, IntPtr propertyIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_list", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_list(ObjectHandle handle, IntPtr propertyIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_null", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_null(ObjectHandle handle, IntPtr propertyIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_null_unique", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_null_unique(ObjectHandle handle, IntPtr propertyIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_bool", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_bool(ObjectHandle handle, IntPtr propertyIndex, IntPtr value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_bool", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_bool(ObjectHandle handle, IntPtr propertyIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_nullable_bool", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_nullable_bool(ObjectHandle handle, IntPtr propertyIndex, out IntPtr retVal, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_int64", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_int64(ObjectHandle handle, IntPtr propertyIndex, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_int64_unique", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_int64_unique(ObjectHandle handle, IntPtr propertyIndex, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_int64", CallingConvention = CallingConvention.Cdecl)]
            public static extern Int64 get_int64(ObjectHandle handle, IntPtr propertyIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_nullable_int64", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_nullable_int64(ObjectHandle handle, IntPtr propertyIndex, out Int64 retVal, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_float", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_float(ObjectHandle handle, IntPtr propertyIndex, Single value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_float", CallingConvention = CallingConvention.Cdecl)]
            public static extern Single get_float(ObjectHandle handle, IntPtr propertyIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_nullable_float", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_nullable_float(ObjectHandle handle, IntPtr propertyIndex, out Single retVal, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_double", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_double(ObjectHandle handle, IntPtr propertyIndex, Double value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_double", CallingConvention = CallingConvention.Cdecl)]
            public static extern Double get_double(ObjectHandle handle, IntPtr propertyIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_nullable_double", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_nullable_double(ObjectHandle handle, IntPtr propertyIndex, out Double retVal, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_set_binary", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr set_binary(ObjectHandle handle, IntPtr propertyIndex,
                IntPtr buffer, IntPtr bufferLength, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_binary", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_binary(ObjectHandle handle, IntPtr propertyIndex,
                IntPtr buffer, IntPtr bufferLength, [MarshalAs(UnmanagedType.I1)] out bool is_null, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_remove_row", CallingConvention = CallingConvention.Cdecl)]
            public static extern void remove_row(ObjectHandle handle, RealmHandle realmHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_equals_object", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool equals_object(ObjectHandle handle, ObjectHandle otherHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_backlinks", CallingConvention = CallingConvention.Cdecl)]
            public static extern ResultsHandle get_backlinks(ObjectHandle objectHandle, IntPtr propertyIndex, out NativeException nativeException);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_get_thread_safe_reference", CallingConvention = CallingConvention.Cdecl)]
            public static extern ThreadSafeReferenceHandle get_thread_safe_reference(ObjectHandle objectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "object_add_notification_callback", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr add_notification_callback(ObjectHandle objectHandle, IntPtr managedObjectHandle, NotificationCallbackDelegate callback, out NativeException ex);
        }

        public bool IsValid
        {
            get
            {
                NativeException nativeException;
                var result = NativeMethods.get_is_valid(this, out nativeException);
                nativeException.ThrowIfNecessary();
                return result;
            }
        }

        // keep this one even though warned that it is not used. It is in fact used by marshalling
        // used by P/Invoke to automatically construct a TableHandle when returning a size_t as a TableHandle
        [Preserve]
        public ObjectHandle(SharedRealmHandle sharedRealmHandle) : base(sharedRealmHandle)
        {
        }

        public override bool Equals(object obj)
        {
            // If parameter is null, return false. 
            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            // Optimization for a common success case. 
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            var otherHandle = obj as ObjectHandle;
            if (ReferenceEquals(otherHandle, null))
            {
                return false;
            }

            NativeException nativeException;
            var result = NativeMethods.equals_object(this, otherHandle, out nativeException);
            nativeException.ThrowIfNecessary();

            return result;
        }

        protected override void Unbind()
        {
            NativeMethods.destroy(handle);
        }

        // acquire a ListHandle from object_get_list And set root in an atomic fashion 
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands"), SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        internal ListHandle TableLinkList(IntPtr propertyIndex)
        {
            var listHandle = new ListHandle(Root ?? this);
            listHandle.SetHandle(this.GetLinklist(propertyIndex));
            return listHandle;
        }
    
        public void SetDateTimeOffset(IntPtr propertyIndex, DateTimeOffset value)
        {
            NativeException nativeException;
            var ticks = value.ToUniversalTime().Ticks;
            NativeMethods.set_timestamp_ticks(this, propertyIndex, ticks, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void SetNullableDateTimeOffset(IntPtr propertyIndex, DateTimeOffset? value)
        {
            NativeException nativeException;
            if (value.HasValue)
            {
                var ticks = value.Value.ToUniversalTime().Ticks;
                NativeMethods.set_timestamp_ticks(this, propertyIndex, ticks, out nativeException);
            }
            else
            {
                NativeMethods.set_null(this, propertyIndex, out nativeException);
            }

            nativeException.ThrowIfNecessary();
        }

        public DateTimeOffset GetDateTimeOffset(IntPtr propertyIndex)
        {
            NativeException nativeException;
            var ticks = NativeMethods.get_timestamp_ticks(this, propertyIndex, out nativeException);
            nativeException.ThrowIfNecessary();
            return new DateTimeOffset(ticks, TimeSpan.Zero);
        }

        public DateTimeOffset? GetNullableDateTimeOffset(IntPtr propertyIndex)
        {
            NativeException nativeException;
            long ticks;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeMethods.get_nullable_timestamp_ticks(this, propertyIndex, out ticks, out nativeException));
            nativeException.ThrowIfNecessary();
            return hasValue ? new DateTimeOffset(ticks, TimeSpan.Zero) : (DateTimeOffset?)null;
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

            NativeException nativeException;
            NativeMethods.set_string_unique(this, propertyIndex, value, (IntPtr)value.Length, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public string GetString(IntPtr propertyIndex)
        {
            return MarshalHelpers.GetString((IntPtr buffer, IntPtr length, out bool isNull, out NativeException ex) => NativeMethods.get_string(this, propertyIndex, buffer, length, out isNull, out ex));
        }

        public void SetLink(IntPtr propertyIndex, ObjectHandle targetHandle)
        {
            NativeException nativeException;
            NativeMethods.set_link(this, propertyIndex, targetHandle, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void ClearLink(IntPtr propertyIndex)
        {
            NativeException nativeException;
            NativeMethods.clear_link(this, propertyIndex, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public IntPtr GetLink(IntPtr propertyIndex)
        {
            NativeException nativeException;
            var result = NativeMethods.get_link(this, propertyIndex, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public IntPtr GetLinklist(IntPtr propertyIndex)
        {
            NativeException nativeException;
            var result = NativeMethods.get_list(this, propertyIndex, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public bool LinklistIsEmpty(IntPtr propertyIndex)
        {
            NativeException nativeException;
            var result = NativeMethods.list_is_empty(this, propertyIndex, out nativeException);
            nativeException.ThrowIfNecessary();
            return MarshalHelpers.IntPtrToBool(result);
        }

        public void SetBoolean(IntPtr propertyIndex, bool value)
        {
            NativeException nativeException;
            NativeMethods.set_bool(this, propertyIndex, MarshalHelpers.BoolToIntPtr(value), out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void SetNullableBoolean(IntPtr propertyIndex, bool? value)
        {
            NativeException nativeException;
            if (value.HasValue)
            {
                NativeMethods.set_bool(this, propertyIndex, MarshalHelpers.BoolToIntPtr(value.Value), out nativeException);
            }
            else
            {
                NativeMethods.set_null(this, propertyIndex, out nativeException);
            }

            nativeException.ThrowIfNecessary();
        }

        public bool GetBoolean(IntPtr propertyIndex)
        {
            NativeException nativeException;
            var result = NativeMethods.get_bool(this, propertyIndex, out nativeException);
            nativeException.ThrowIfNecessary();
            return MarshalHelpers.IntPtrToBool(result);
        }

        public bool? GetNullableBoolean(IntPtr propertyIndex)
        {
            NativeException nativeException;
            IntPtr value;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeMethods.get_nullable_bool(this, propertyIndex, out value, out nativeException));
            nativeException.ThrowIfNecessary();
            return hasValue ? MarshalHelpers.IntPtrToBool(value) : (bool?)null;
        }

        public void SetInt64(IntPtr propertyIndex, long value)
        {
            NativeException nativeException;
            NativeMethods.set_int64(this, propertyIndex, value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void SetNullableInt64(IntPtr propertyIndex, long? value)
        {
            NativeException nativeException;
            if (value.HasValue)
            {
                NativeMethods.set_int64(this, propertyIndex, value.Value, out nativeException);
            }
            else
            {
                NativeMethods.set_null(this, propertyIndex, out nativeException);
            }

            nativeException.ThrowIfNecessary();
        }

        public void SetInt64Unique(IntPtr propertyIndex, long value)
        {
            NativeException nativeException;
            NativeMethods.set_int64_unique(this, propertyIndex, value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void SetNullableInt64Unique(IntPtr propertyIndex, long? value)
        {
            NativeException nativeException;
            if (value.HasValue)
            {
                NativeMethods.set_int64_unique(this, propertyIndex, value.Value, out nativeException);
            }
            else
            {
                NativeMethods.set_null_unique(this, propertyIndex, out nativeException);
            }

            nativeException.ThrowIfNecessary();
        }

        public long GetInt64(IntPtr propertyIndex)
        {
            NativeException nativeException;
            var result = NativeMethods.get_int64(this, propertyIndex, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public long? GetNullableInt64(IntPtr propertyIndex)
        {
            NativeException nativeException;
            long value;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeMethods.get_nullable_int64(this, propertyIndex, out value, out nativeException));
            nativeException.ThrowIfNecessary();
            return hasValue ? value : (long?)null;
        }

        public void SetSingle(IntPtr propertyIndex, float value)
        {
            NativeException nativeException;
            NativeMethods.set_float(this, propertyIndex, value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void SetNullableSingle(IntPtr propertyIndex, float? value)
        {
            NativeException nativeException;
            if (value.HasValue)
            {
                NativeMethods.set_float(this, propertyIndex, value.Value, out nativeException);
            }
            else
            {
                NativeMethods.set_null(this, propertyIndex, out nativeException);
            }

            nativeException.ThrowIfNecessary();
        }

        public float GetSingle(IntPtr propertyIndex)
        {
            NativeException nativeException;
            var result = NativeMethods.get_float(this, propertyIndex, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public float? GetNullableSingle(IntPtr propertyIndex)
        {
            NativeException nativeException;
            float value;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeMethods.get_nullable_float(this, propertyIndex, out value, out nativeException));
            nativeException.ThrowIfNecessary();
            return hasValue ? value : (float?)null;
        }

        public void SetDouble(IntPtr propertyIndex, double value)
        {
            NativeException nativeException;
            NativeMethods.set_double(this, propertyIndex, value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void SetNullableDouble(IntPtr propertyIndex, double? value)
        {
            NativeException nativeException;
            if (value.HasValue)
            {
                NativeMethods.set_double(this, propertyIndex, value.Value, out nativeException);
            }
            else
            {
                NativeMethods.set_null(this, propertyIndex, out nativeException);
            }

            nativeException.ThrowIfNecessary();
        }

        public double GetDouble(IntPtr propertyIndex)
        {
            NativeException nativeException;
            var result = NativeMethods.get_double(this, propertyIndex, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public double? GetNullableDouble(IntPtr propertyIndex)
        {
            NativeException nativeException;
            double value;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeMethods.get_nullable_double(this, propertyIndex, out value, out nativeException));
            nativeException.ThrowIfNecessary();
            return hasValue ? value : (double?)null;
        }

        public unsafe void SetByteArray(IntPtr propertyIndex, byte[] value)
        {
            NativeException nativeException;
            if (value == null)
            {
                NativeMethods.set_null(this, propertyIndex, out nativeException);
            }
            else if (value.Length == 0)
            {
                // empty byte arrays are expressed in terms of a BinaryData object with a dummy pointer and zero size
                // that's how core differentiates between empty and null buffers
                NativeMethods.set_binary(this, propertyIndex, (IntPtr)0x1, IntPtr.Zero, out nativeException);
            }
            else
            {
                fixed (byte* buffer = value)
                {
                    NativeMethods.set_binary(this, propertyIndex, (IntPtr)buffer, (IntPtr)value.LongCount(), out nativeException);
                }
            }

            nativeException.ThrowIfNecessary();
        }

        public byte[] GetByteArray(IntPtr propertyIndex)
        {
            return GetByteArrayBuffer(propertyIndex, 0);
        }

        private unsafe byte[] GetByteArrayBuffer(IntPtr propertyIndex, int size)
        {
            // Initially called with size = 0, we make a native call just to get the size of the buffer.
            var bytes = new byte[size];
            bool isNull;
            NativeException nativeException;

            int actualSize;
            fixed (byte* buffer = bytes)
            {
                actualSize = (int)NativeMethods.get_binary(this, propertyIndex, (IntPtr)buffer, (IntPtr)size, out isNull, out nativeException);
            }
            nativeException.ThrowIfNecessary();

            if (isNull)
            {
                return null;
            }

            if (actualSize > size)
            {
                return GetByteArrayBuffer(propertyIndex, actualSize);
            }

            return bytes;
        }

        public void RemoveFromRealm(SharedRealmHandle realmHandle)
        {
            NativeException nativeException;
            NativeMethods.remove_row(this, realmHandle, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public RealmList<T> GetList<T>(Realm realm, IntPtr propertyIndex, string objectType) where T : RealmObject
        {
            var listHandle = this.TableLinkList(propertyIndex);
            return new RealmList<T>(realm, listHandle, realm.Metadata[objectType]);
        }

        public T GetObject<T>(Realm realm, IntPtr propertyIndex, string objectType) where T : RealmObject
        {
            var linkedObjectPtr = GetLink(propertyIndex);
            if (linkedObjectPtr == IntPtr.Zero)
            {
                return null;
            }

            return (T)realm.MakeObject(objectType, linkedObjectPtr);
        }

        public void SetObject(Realm realm, IntPtr propertyIndex, RealmObject @object)
        {
            if (@object == null)
            {
                this.ClearLink(propertyIndex);
            }
            else
            {
                if (!@object.IsManaged)
                {
                    realm.Add(@object);
                }

                this.SetLink(propertyIndex, @object.ObjectHandle);
            }
        }

        public ResultsHandle GetBacklinks(IntPtr propertyIndex)
        {
            NativeException nativeException;
            var resultsHandle = NativeMethods.get_backlinks(this, propertyIndex, out nativeException);
            nativeException.ThrowIfNecessary();

            return resultsHandle;
        }

        public override ThreadSafeReferenceHandle GetThreadSafeReference()
        {
            NativeException nativeException;
            var result = NativeMethods.get_thread_safe_reference(this, out nativeException);
            nativeException.ThrowIfNecessary();

            return result;
        }

        public override IntPtr AddNotificationCallback(IntPtr managedObjectHandle, NotificationCallbackDelegate callback)
        {
            NativeException nativeException;
            var result = NativeMethods.add_notification_callback(this, managedObjectHandle, callback, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }
    }
}