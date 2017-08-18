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
using Realms.Native;

namespace Realms
{
    internal class ListHandle : CollectionHandleBase
    {
        private static class NativeMethods
        {
            #region add

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_add", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add(ListHandle listHandle, ObjectHandle objectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_add_primitive", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add_primitive(ListHandle listHandle, PrimitiveValue value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_add_nullable_bool", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add_nullable_bool(ListHandle listHandle, [MarshalAs(UnmanagedType.I1)] bool value, [MarshalAs(UnmanagedType.I1)] bool has_value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_add_int64", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add_int64(ListHandle listHandle, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_add_nullable_int64", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add_nullable_int64(ListHandle listHandle, Int64 value, [MarshalAs(UnmanagedType.I1)] bool has_value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_add_float", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add_float(ListHandle listHandle, float value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_add_nullable_float", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add_nullable_float(ListHandle listHandle, float value, [MarshalAs(UnmanagedType.I1)] bool has_value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_add_double", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add_double(ListHandle listHandle, double value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_add_nullable_double", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add_nullable_double(ListHandle listHandle, double value, [MarshalAs(UnmanagedType.I1)] bool has_value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_add_timestamp_ticks", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add_timestamp_ticks(ListHandle listHandle, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_add_nullable_timestamp_ticks", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add_nullable_timestamp_ticks(ListHandle listHandle, Int64 value, [MarshalAs(UnmanagedType.I1)] bool has_value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_add_string", CallingConvention = CallingConvention.Cdecl)]
            public static extern void add_string(ListHandle listHandle, [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen,
                [MarshalAs(UnmanagedType.I1)] bool has_value, out NativeException ex);

            #endregion

            #region insert

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_insert", CallingConvention = CallingConvention.Cdecl)]
            public static extern void insert(ListHandle listHandle, IntPtr targetIndex, ObjectHandle objectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_add_primitive", CallingConvention = CallingConvention.Cdecl)]
            public static extern void insert_primitive(ListHandle listHandle, IntPtr targetIndex, PrimitiveValue value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_insert_nullable_bool", CallingConvention = CallingConvention.Cdecl)]
            public static extern void insert_nullable_bool(ListHandle listHandle, IntPtr targetIndex, [MarshalAs(UnmanagedType.I1)] bool value, [MarshalAs(UnmanagedType.I1)] bool has_value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_insert_int64", CallingConvention = CallingConvention.Cdecl)]
            public static extern void insert_int64(ListHandle listHandle, IntPtr targetIndex, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_insert_nullable_int64", CallingConvention = CallingConvention.Cdecl)]
            public static extern void insert_nullable_int64(ListHandle listHandle, IntPtr targetIndex, Int64 value, [MarshalAs(UnmanagedType.I1)] bool has_value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_insert_float", CallingConvention = CallingConvention.Cdecl)]
            public static extern void insert_float(ListHandle listHandle, IntPtr targetIndex, float value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_insert_nullable_float", CallingConvention = CallingConvention.Cdecl)]
            public static extern void insert_nullable_float(ListHandle listHandle, IntPtr targetIndex, float value, [MarshalAs(UnmanagedType.I1)] bool has_value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_insert_double", CallingConvention = CallingConvention.Cdecl)]
            public static extern void insert_double(ListHandle listHandle, IntPtr targetIndex, double value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_insert_nullable_double", CallingConvention = CallingConvention.Cdecl)]
            public static extern void insert_nullable_double(ListHandle listHandle, IntPtr targetIndex, double value, [MarshalAs(UnmanagedType.I1)] bool has_value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_insert_timestamp_ticks", CallingConvention = CallingConvention.Cdecl)]
            public static extern void insert_timestamp_ticks(ListHandle listHandle, IntPtr targetIndex, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_insert_nullable_timestamp_ticks", CallingConvention = CallingConvention.Cdecl)]
            public static extern void insert_nullable_timestamp_ticks(ListHandle listHandle, IntPtr targetIndex, Int64 value, [MarshalAs(UnmanagedType.I1)] bool has_value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_insert_string", CallingConvention = CallingConvention.Cdecl)]
            public static extern void insert_string(ListHandle listHandle, IntPtr targetIndex, [MarshalAs(UnmanagedType.LPWStr)] string value,
                IntPtr valueLen, [MarshalAs(UnmanagedType.I1)] bool has_value, out NativeException ex);

            #endregion

            #region get

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_get", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get(ListHandle listHandle, IntPtr link_ndx, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_get_bool", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool get_bool(ListHandle listHandle, IntPtr link_ndx, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_get_nullable_bool", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool get_nullable_bool(ListHandle listHandle, IntPtr link_ndx, [MarshalAs(UnmanagedType.I1)] out bool retVal, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_get_int64", CallingConvention = CallingConvention.Cdecl)]
            public static extern Int64 get_int64(ListHandle listHandle, IntPtr link_ndx, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_get_nullable_int64", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool get_nullable_int64(ListHandle listHandle, IntPtr link_ndx, out Int64 retVal, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_get_float", CallingConvention = CallingConvention.Cdecl)]
            public static extern float get_float(ListHandle listHandle, IntPtr link_ndx, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_get_nullable_float", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool get_nullable_float(ListHandle listHandle, IntPtr link_ndx, out float retVal, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_get_double", CallingConvention = CallingConvention.Cdecl)]
            public static extern double get_double(ListHandle listHandle, IntPtr link_ndx, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_get_nullable_double", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool get_nullable_double(ListHandle listHandle, IntPtr link_ndx, out double retVal, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_get_timestamp_ticks", CallingConvention = CallingConvention.Cdecl)]
            public static extern Int64 get_timestamp_ticks(ListHandle listHandle, IntPtr link_ndx, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_get_nullable_timestamp_ticks", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool get_nullable_timestamp_ticks(ListHandle listHandle, IntPtr link_ndx, out Int64 retVal, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_get_string", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_string(ListHandle listHandle, IntPtr link_ndx, IntPtr buffer, IntPtr bufsize,
                [MarshalAs(UnmanagedType.I1)] out bool isNull, out NativeException ex);

            #endregion

            #region find

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_find", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr find(ListHandle listHandle, ObjectHandle objectHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_find_bool", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr find_bool(ListHandle listHandle, [MarshalAs(UnmanagedType.I1)] bool value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_find_nullable_bool", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr find_nullable_bool(ListHandle listHandle, [MarshalAs(UnmanagedType.I1)] bool value, [MarshalAs(UnmanagedType.I1)] bool has_value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_find_int64", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr find_int64(ListHandle listHandle, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_find_nullable_int64", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr find_nullable_int64(ListHandle listHandle, Int64 value, [MarshalAs(UnmanagedType.I1)] bool has_value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_find_float", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr find_float(ListHandle listHandle, float value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_find_nullable_float", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr find_nullable_float(ListHandle listHandle, float value, [MarshalAs(UnmanagedType.I1)] bool has_value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_find_double", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr find_double(ListHandle listHandle, double value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_find_nullable_double", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr find_nullable_double(ListHandle listHandle, double value, [MarshalAs(UnmanagedType.I1)] bool has_value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_find_timestamp_ticks", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr find_timestamp_ticks(ListHandle listHandle, Int64 value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_find_nullable_timestamp_ticks", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr find_nullable_timestamp_ticks(ListHandle listHandle, Int64 value, [MarshalAs(UnmanagedType.I1)] bool has_value, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_find_string", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr find_string(ListHandle listHandle, [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen,
                [MarshalAs(UnmanagedType.I1)] bool has_value, out NativeException ex);

            #endregion

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_erase", CallingConvention = CallingConvention.Cdecl)]
            public static extern void erase(ListHandle listHandle, IntPtr rowIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_clear", CallingConvention = CallingConvention.Cdecl)]
            public static extern void clear(ListHandle listHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_size", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr size(ListHandle listHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr listInternalHandle);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_add_notification_callback", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr add_notification_callback(ListHandle listHandle, IntPtr managedListHandle, NotificationCallbackDelegate callback, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_move", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr move(ListHandle listHandle, IntPtr sourceIndex, IntPtr targetIndex, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_get_is_valid", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool get_is_valid(ListHandle listHandle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "list_get_thread_safe_reference", CallingConvention = CallingConvention.Cdecl)]
            public static extern ThreadSafeReferenceHandle get_thread_safe_reference(ListHandle listHandle, out NativeException ex);
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

        public ListHandle(RealmHandle root) : base(root)
        {
        }

        protected override void Unbind()
        {
            NativeMethods.destroy(handle);
        }

        #region GetAtIndex

        public override IntPtr GetObjectAtIndex(int index)
        {
            var result = NativeMethods.get(this, (IntPtr)index, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public bool GetBoolAtIndex(int index)
        {
            var result = NativeMethods.get_bool(this, (IntPtr)index, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public bool? GetNullableBoolAtIndex(int index)
        {
            var hasValue = NativeMethods.get_nullable_bool(this, (IntPtr)index, out var value, out var nativeException);
            nativeException.ThrowIfNecessary();
            return hasValue ? value : (bool?)null;
        }

        public long GetInt64AtIndex(int index)
        {
            var result = NativeMethods.get_int64(this, (IntPtr)index, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public long? GetNullableInt64AtIndex(int index)
        {
            var hasValue = NativeMethods.get_nullable_int64(this, (IntPtr)index, out var value, out var nativeException);
            nativeException.ThrowIfNecessary();
            return hasValue ? value : (long?)null;
        }

        public float GetFloatAtIndex(int index)
        {
            var result = NativeMethods.get_float(this, (IntPtr)index, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public float? GetNullableFloatAtIndex(int index)
        {
            var hasValue = NativeMethods.get_nullable_float(this, (IntPtr)index, out var value, out var nativeException);
            nativeException.ThrowIfNecessary();
            return hasValue ? value : (float?)null;
        }

        public double GetDoubleAtIndex(int index)
        {
            var result = NativeMethods.get_double(this, (IntPtr)index, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        public double? GetNullableDoubleAtIndex(int index)
        {
            var hasValue = NativeMethods.get_nullable_double(this, (IntPtr)index, out var value, out var nativeException);
            nativeException.ThrowIfNecessary();
            return hasValue ? value : (double?)null;
        }

        public DateTimeOffset GetDateTimeOffsetAtIndex(int index)
        {
            var ticks = NativeMethods.get_timestamp_ticks(this, (IntPtr)index, out var nativeException);
            nativeException.ThrowIfNecessary();
            return new DateTimeOffset(ticks, TimeSpan.Zero);
        }

        public DateTimeOffset? GetNullableDateTimeOffsetAtIndex(int index)
        {
            var hasValue = NativeMethods.get_nullable_timestamp_ticks(this, (IntPtr)index, out var ticks, out var nativeException);
            nativeException.ThrowIfNecessary();
            return hasValue ? new DateTimeOffset(ticks, TimeSpan.Zero) : (DateTimeOffset?)null;
        }

        public string GetStringAtIndex(int index)
        {
            return MarshalHelpers.GetString((IntPtr buffer, IntPtr length, out bool isNull, out NativeException ex) =>
                NativeMethods.get_string(this, (IntPtr)index, buffer, length, out isNull, out ex));
        }

        #endregion

        #region Add

        public void Add(ObjectHandle objectHandle)
        {
            NativeMethods.add(this, objectHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Add(PrimitiveValue value)
        {
            NativeMethods.add_primitive(this, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Add(bool? value)
        {
            NativeMethods.add_nullable_bool(this, value.GetValueOrDefault(), value.HasValue, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Add(long value)
        {
            NativeMethods.add_int64(this, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Add(long? value)
        {
            NativeMethods.add_nullable_int64(this, value.GetValueOrDefault(), value.HasValue, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Add(float value)
        {
            NativeMethods.add_float(this, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Add(float? value)
        {
            NativeMethods.add_nullable_float(this, value.GetValueOrDefault(), value.HasValue, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Add(double value)
        {
            NativeMethods.add_double(this, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Add(double? value)
        {
            NativeMethods.add_nullable_double(this, value.GetValueOrDefault(), value.HasValue, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Add(DateTimeOffset value)
        {
            NativeMethods.add_timestamp_ticks(this, value.ToUniversalTime().Ticks, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Add(DateTimeOffset? value)
        {
            NativeMethods.add_nullable_timestamp_ticks(this, value.GetValueOrDefault().ToUniversalTime().Ticks, value.HasValue, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Add(string value)
        {
            NativeMethods.add_string(this, value, (IntPtr)(value?.Length ?? 0), value != null, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        #endregion

        #region Insert

        public void Insert(int targetIndex, ObjectHandle objectHandle)
        {
            NativeMethods.insert(this, (IntPtr)targetIndex, objectHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Insert(int targetIndex, PrimitiveValue value)
        {
            NativeMethods.insert_primitive(this, (IntPtr)targetIndex, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Insert(int targetIndex, bool? value)
        {
            NativeMethods.insert_nullable_bool(this, (IntPtr)targetIndex, value.GetValueOrDefault(), value.HasValue, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Insert(int targetIndex, long value)
        {
            NativeMethods.insert_int64(this, (IntPtr)targetIndex, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Insert(int targetIndex, long? value)
        {
            NativeMethods.insert_nullable_int64(this, (IntPtr)targetIndex, value.GetValueOrDefault(), value.HasValue, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Insert(int targetIndex, float value)
        {
            NativeMethods.insert_float(this, (IntPtr)targetIndex, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Insert(int targetIndex, float? value)
        {
            NativeMethods.insert_nullable_float(this, (IntPtr)targetIndex, value.GetValueOrDefault(), value.HasValue, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Insert(int targetIndex, double value)
        {
            NativeMethods.insert_double(this, (IntPtr)targetIndex, value, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Insert(int targetIndex, double? value)
        {
            NativeMethods.insert_nullable_double(this, (IntPtr)targetIndex, value.GetValueOrDefault(), value.HasValue, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Insert(int targetIndex, DateTimeOffset value)
        {
            NativeMethods.insert_timestamp_ticks(this, (IntPtr)targetIndex, value.ToUniversalTime().Ticks, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Insert(int targetIndex, DateTimeOffset? value)
        {
            NativeMethods.insert_nullable_timestamp_ticks(this, (IntPtr)targetIndex, value.GetValueOrDefault().ToUniversalTime().Ticks, value.HasValue, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Insert(int targetIndex, string value)
        {
            var hasValue = value != null;
            value = value ?? string.Empty;
            NativeMethods.insert_string(this, (IntPtr)targetIndex, value, (IntPtr)value.Length, hasValue, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        #endregion

        #region Find

        public int Find(ObjectHandle objectHandle)
        {
            var result = NativeMethods.find(this, objectHandle, out var nativeException);
            nativeException.ThrowIfNecessary();
            return (int)result;
        }

        public int Find(bool value)
        {
            var result = NativeMethods.find_bool(this, value, out var nativeException);
            nativeException.ThrowIfNecessary();
            return (int)result;
        }

        public int Find(bool? value)
        {
            var result = NativeMethods.find_nullable_bool(this, value.GetValueOrDefault(), value.HasValue, out var nativeException);
            nativeException.ThrowIfNecessary();
            return (int)result;
        }

        public int Find(long value)
        {
            var result = NativeMethods.find_int64(this, value, out var nativeException);
            nativeException.ThrowIfNecessary();
            return (int)result;
        }

        public int Find(long? value)
        {
            var result = NativeMethods.find_nullable_int64(this, value.GetValueOrDefault(), value.HasValue, out var nativeException);
            nativeException.ThrowIfNecessary();
            return (int)result;
        }

        public int Find(float value)
        {
            var result = NativeMethods.find_float(this, value, out var nativeException);
            nativeException.ThrowIfNecessary();
            return (int)result;
        }

        public int Find(float? value)
        {
            var result = NativeMethods.find_nullable_float(this, value.GetValueOrDefault(), value.HasValue, out var nativeException);
            nativeException.ThrowIfNecessary();
            return (int)result;
        }

        public int Find(double value)
        {
            var result = NativeMethods.find_double(this, value, out var nativeException);
            nativeException.ThrowIfNecessary();
            return (int)result;
        }

        public int Find(double? value)
        {
            var result = NativeMethods.find_nullable_double(this, value.GetValueOrDefault(), value.HasValue, out var nativeException);
            nativeException.ThrowIfNecessary();
            return (int)result;
        }

        public int Find(DateTimeOffset value)
        {
            var result = NativeMethods.find_timestamp_ticks(this, value.ToUniversalTime().Ticks, out var nativeException);
            nativeException.ThrowIfNecessary();
            return (int)result;
        }

        public int Find(DateTimeOffset? value)
        {
            var result = NativeMethods.find_nullable_timestamp_ticks(this, value.GetValueOrDefault().ToUniversalTime().Ticks, value.HasValue, out var nativeException);
            nativeException.ThrowIfNecessary();
            return (int)result;
        }

        public int Find(string value)
        {
            var hasValue = value != null;
            value = value ?? string.Empty;
            var result = NativeMethods.find_string(this, value, (IntPtr)value.Length, hasValue, out var nativeException);
            nativeException.ThrowIfNecessary();
            return (int)result;
        }

        #endregion

        public void Erase(IntPtr rowIndex)
        {
            NativeMethods.erase(this, rowIndex, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Clear()
        {
            NativeMethods.clear(this, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public void Move(IntPtr sourceIndex, IntPtr targetIndex)
        {
            NativeMethods.move(this, sourceIndex, targetIndex, out var nativeException);
            nativeException.ThrowIfNecessary();
        }

        public override IntPtr AddNotificationCallback(IntPtr managedObjectHandle, NotificationCallbackDelegate callback)
        {
            var result = NativeMethods.add_notification_callback(this, managedObjectHandle, callback, out var nativeException);
            nativeException.ThrowIfNecessary();
            return result;
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

            return result;
        }
    }
}