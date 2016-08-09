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
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Realms
{
    internal static class NativeTable
    {
        public static IntPtr AddEmptyRow(TableHandle tableHandle)
        {
            NativeException nativeException;
            var result = add_empty_row(tableHandle, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_add_empty_row", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr add_empty_row(TableHandle tableHandle, out NativeException ex);

        public static void SetDateTimeOffset(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex, DateTimeOffset value)
        {
            NativeException nativeException;
            var ticks = value.ToUniversalTime().Ticks;
            set_timestamp_ticks(tableHandle, columnIndex, rowIndex, ticks, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public static void SetNullableDateTimeOffset(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex, DateTimeOffset? value)
        {
            NativeException nativeException;
            if (value.HasValue)
            {
                var ticks = value.Value.ToUniversalTime().Ticks;
                set_timestamp_ticks(tableHandle, columnIndex, rowIndex, ticks, out nativeException);
            }
            else
                set_null(tableHandle, columnIndex, rowIndex, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_timestamp_ticks", CallingConvention = CallingConvention.Cdecl)]
        private static extern void set_timestamp_ticks(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx, Int64 value, out NativeException ex);

        public static DateTimeOffset GetDateTimeOffset(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex)
        {
            NativeException nativeException;
            var ticks = get_timestamp_ticks(tableHandle, columnIndex, rowIndex, out nativeException);
            nativeException.ThrowIfNecessary();
            return new DateTimeOffset(ticks, TimeSpan.Zero);
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_timestamp_ticks", CallingConvention = CallingConvention.Cdecl)]
        private static extern Int64 get_timestamp_ticks(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex, out NativeException ex);

        public static DateTimeOffset? GetNullableDateTimeOffset(TableHandle tableHandle, IntPtr columnIndex,
            IntPtr rowIndex)
        {
            NativeException nativeException;
            long ticks;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeTable.get_nullable_timestamp_ticks(tableHandle, columnIndex, rowIndex, out ticks, out nativeException));
            nativeException.ThrowIfNecessary();
            return hasValue ? new DateTimeOffset(ticks, TimeSpan.Zero) : (DateTimeOffset?)null;
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_nullable_timestamp_ticks", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr get_nullable_timestamp_ticks(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex, out long retVal, out NativeException ex);

        public static void SetString(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex, string value)
        {
            NativeException nativeException;
            if (value != null)
                set_string(tableHandle, columnIndex, rowIndex, value, (IntPtr)value.Length, out nativeException);
            else
                set_null(tableHandle, columnIndex, rowIndex, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern void set_string(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen, out NativeException ex);

        public static void SetStringUnique(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex, string value)
        {
            if (value == null)
                throw new ArgumentException("Object identifiers cannot be null");

            NativeException nativeException;
            set_string_unique(tableHandle, columnIndex, rowIndex, value, (IntPtr)value.Length, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_string_unique", CallingConvention = CallingConvention.Cdecl)]
        private static extern void set_string_unique(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen, out NativeException ex);

        public static string GetString(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex)
        {
            var bufferSizeNeededChars = 128;
            // First alloc this thread

            var stringGetBuffer = Marshal.AllocHGlobal((IntPtr)(bufferSizeNeededChars * sizeof(char)));
            var stringGetBufferLen = bufferSizeNeededChars;

            bool isNull;
            NativeException nativeException;

            // try to read
            var bytesRead = (int)get_string(tableHandle, columnIndex, rowIndex, stringGetBuffer,
                (IntPtr)stringGetBufferLen, out isNull, out nativeException);
            nativeException.ThrowIfNecessary();
            if (bytesRead == -1)
            {
                throw new RealmInvalidDatabaseException("Corrupted string data");
            }
            if (bytesRead > stringGetBufferLen)  // need a bigger buffer
            {
                Marshal.FreeHGlobal(stringGetBuffer);
                stringGetBuffer = Marshal.AllocHGlobal((IntPtr)(bytesRead * sizeof(char)));
                stringGetBufferLen = bytesRead;
                // try to read with big buffer
                bytesRead = (int)get_string(tableHandle, columnIndex, rowIndex, stringGetBuffer,
                    (IntPtr)stringGetBufferLen, out isNull, out nativeException);
                nativeException.ThrowIfNecessary();
                if (bytesRead == -1)  // bad UTF-8 in full string
                    throw new RealmInvalidDatabaseException("Corrupted string data");
                Debug.Assert(bytesRead <= stringGetBufferLen);
            }  // needed re-read with expanded buffer

            return bytesRead != 0 ? Marshal.PtrToStringUni(stringGetBuffer, bytesRead) : (isNull ? null : "");
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr get_string(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex,
            IntPtr buffer, IntPtr bufsize, [MarshalAs(UnmanagedType.I1)] out bool isNull, out NativeException ex);

        public static void SetLink(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex, IntPtr targetRowIndex)
        {
            NativeException nativeException;
            set_link(tableHandle, columnIndex, rowIndex, targetRowIndex, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_link", CallingConvention = CallingConvention.Cdecl)]
        private static extern void set_link(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx, IntPtr targetRowNdx, out NativeException ex);

        public static void ClearLink(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex)
        {
            NativeException nativeException;
            clear_link(tableHandle, columnIndex, rowIndex, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_clear_link", CallingConvention = CallingConvention.Cdecl)]
        private static extern void clear_link(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx, out NativeException ex);

        public static IntPtr GetLink(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex)
        {
            NativeException nativeException;
            var result = get_link(tableHandle, columnIndex, rowIndex, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_link", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr get_link(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex, out NativeException ex);

        public static IntPtr GetLinklist(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex)
        {
            NativeException nativeException;
            var result = get_linklist(tableHandle, columnIndex, rowIndex, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_linklist", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr get_linklist(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex, out NativeException ex);

        public static bool LinklistIsEmpty(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex)
        {
            NativeException nativeException;
            var result = linklist_is_empty(tableHandle, columnIndex, rowIndex, out nativeException);
            nativeException.ThrowIfNecessary();
            return MarshalHelpers.IntPtrToBool(result);
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_linklist_is_empty", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr linklist_is_empty(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_null", CallingConvention = CallingConvention.Cdecl)]
        private static extern void set_null(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx, out NativeException ex);

        public static void SetBoolean(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex, bool value)
        {
            NativeException nativeException;
            set_bool(tableHandle, columnIndex, rowIndex, MarshalHelpers.BoolToIntPtr(value), out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public static void SetNullableBoolean(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex, bool? value)
        {
            NativeException nativeException;
            if (value.HasValue)
                set_bool(tableHandle, columnIndex, rowIndex, MarshalHelpers.BoolToIntPtr(value.Value), out nativeException);
            else
                set_null(tableHandle, columnIndex, rowIndex, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern void set_bool(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx, IntPtr value, out NativeException ex);

        public static bool GetBoolean(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex)
        {
            NativeException nativeException;
            var result = get_bool(tableHandle, columnIndex, rowIndex, out nativeException);
            nativeException.ThrowIfNecessary();
            return MarshalHelpers.IntPtrToBool(result);
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr get_bool(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex, out NativeException ex);

        public static bool? GetNullableBoolean(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex)
        {
            NativeException nativeException;
            IntPtr value;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeTable.get_nullable_bool(tableHandle, columnIndex, rowIndex, out value, out nativeException));
            nativeException.ThrowIfNecessary();
            return hasValue ? MarshalHelpers.IntPtrToBool(value) : (bool?)null;
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_nullable_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr get_nullable_bool(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex, out IntPtr retVal, out NativeException ex);

        public static void SetInt64(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex, long value)
        {
            NativeException nativeException;
            set_int64(tableHandle, columnIndex, rowIndex, value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public static void SetNullableInt64(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex, long? value)
        {
            NativeException nativeException;
            if (value.HasValue)
                set_int64(tableHandle, columnIndex, rowIndex, value.Value, out nativeException);
            else
                set_null(tableHandle, columnIndex, rowIndex, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_int64", CallingConvention = CallingConvention.Cdecl)]
        private static extern void set_int64(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx, Int64 value, out NativeException ex);

        public static void SetInt64Unique(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex, long value)
        {
            NativeException nativeException;
            set_int64_unique(tableHandle, columnIndex, rowIndex, value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_int64_unique", CallingConvention = CallingConvention.Cdecl)]
        private static extern void set_int64_unique(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx, Int64 value, out NativeException ex);

        public static long GetInt64(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex)
        {
            NativeException nativeException;
            var result = get_int64(tableHandle, columnIndex, rowIndex, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_int64", CallingConvention = CallingConvention.Cdecl)]
        private static extern Int64 get_int64(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex, out NativeException ex);

        public static long? GetNullableInt64(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex)
        {
            NativeException nativeException;
            long value;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeTable.get_nullable_int64(tableHandle, columnIndex, rowIndex, out value, out nativeException));
            nativeException.ThrowIfNecessary();
            return hasValue ? value : (long?)null;
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_nullable_int64", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr get_nullable_int64(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex, out Int64 retVal, out NativeException ex);

        public static void SetSingle(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex, float value)
        {
            NativeException nativeException;
            set_float(tableHandle, columnIndex, rowIndex, value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public static void SetNullableSingle(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex, float? value)
        {
            NativeException nativeException;
            if (value.HasValue)
                set_float(tableHandle, columnIndex, rowIndex, value.Value, out nativeException);
            else
                set_null(tableHandle, columnIndex, rowIndex, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern void set_float(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx, float value, out NativeException ex);

        public static float GetSingle(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex)
        {
            NativeException nativeException;
            var result = get_float(tableHandle, columnIndex, rowIndex, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern float get_float(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex, out NativeException ex);

        public static float? GetNullableSingle(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex)
        {
            NativeException nativeException;
            float value;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeTable.get_nullable_float(tableHandle, columnIndex, rowIndex, out value, out nativeException));
            nativeException.ThrowIfNecessary();
            return hasValue ? value : (float?)null;
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_nullable_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr get_nullable_float(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex, out float retVal, out NativeException ex);

        public static void SetDouble(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex, double value)
        {
            NativeException nativeException;
            set_double(tableHandle, columnIndex, rowIndex, value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public static void SetNullableDouble(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex, double? value)
        {
            NativeException nativeException;
            if (value.HasValue)
                set_double(tableHandle, columnIndex, rowIndex, value.Value, out nativeException);
            else
                set_null(tableHandle, columnIndex, rowIndex, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern void set_double(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx, double value, out NativeException ex);

        public static double GetDouble(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex)
        {
            NativeException nativeException;
            var result = get_double(tableHandle, columnIndex, rowIndex, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern double get_double(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex, out NativeException ex);

        public static double? GetNullableDouble(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex)
        {
            NativeException nativeException;
            double value;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeTable.get_nullable_double(tableHandle, columnIndex, rowIndex, out value, out nativeException));
            nativeException.ThrowIfNecessary();
            return hasValue ? value : (double?)null;
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_nullable_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr get_nullable_double(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex, out double retVal, out NativeException ex);

        public static unsafe void SetByteArray(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex, byte[] value)
        {
            NativeException nativeException;
            if (value == null)
            {
                set_null(tableHandle, columnIndex, rowIndex, out nativeException);
            }
            else if (value.Length == 0)
            {
                // empty byte arrays are expressed in terms of a BinaryData object with a dummy pointer and zero size
                // that's how core differentiates between empty and null buffers
                set_binary(tableHandle, columnIndex, rowIndex, (IntPtr)0x1, IntPtr.Zero, out nativeException);
            }
            else
            {
                fixed (byte* buffer = value)
                {
                    set_binary(tableHandle, columnIndex, rowIndex, (IntPtr)buffer, (IntPtr)value.LongLength, out nativeException);
                }
            }
            nativeException.ThrowIfNecessary();
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_binary", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr set_binary(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex,
            IntPtr buffer, IntPtr bufferLength, out NativeException ex);

        public static byte[] GetByteArray(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex)
        {
            NativeException nativeException;
            int bufferSize;
            IntPtr buffer;
            var hasValue = get_binary(tableHandle, columnIndex, rowIndex, out buffer, out bufferSize, out nativeException) != IntPtr.Zero;
            nativeException.ThrowIfNecessary();

            if (hasValue)
            {
                var bytes = new byte[bufferSize];
                Marshal.Copy(buffer, bytes, 0, bufferSize);
                return bytes;
            }

            return null;
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_binary", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr get_binary(TableHandle tableHandle, IntPtr columnIndex, IntPtr rowIndex,
            out IntPtr retBuffer, out int retBufferLength, out NativeException ex);

        public static IntPtr Where(TableHandle tableHandle)
        {
            NativeException nativeException;
            var result = where(tableHandle, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_where", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr where(TableHandle handle, out NativeException ex);

        public static long CountAll(TableHandle tableHandle)
        {
            NativeException nativeException;
            var result = count_all(tableHandle, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_count_all", CallingConvention = CallingConvention.Cdecl)]
        private static extern Int64 count_all(TableHandle handle, out NativeException ex);

        public static void Unbind(IntPtr tablePointer)
        {
            NativeException nativeException;
            unbind(tablePointer, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_unbind", CallingConvention = CallingConvention.Cdecl)]
        private static extern void unbind(IntPtr tableHandle, out NativeException ex);

        public static void RemoveRow(TableHandle tableHandle, RowHandle rowHandle)
        {
            NativeException nativeException;
            remove_row(tableHandle, rowHandle, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_remove_row", CallingConvention = CallingConvention.Cdecl)]
        private static extern void remove_row(TableHandle tableHandle, RowHandle rowHandle, out NativeException ex);

         //returns -1 if the column string does not match a column index
        public static IntPtr GetColumnIndex(TableHandle tableHandle, string name)
        {
            NativeException nativeException;
            var result = get_column_index(tableHandle, name, (IntPtr)name.Length, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_column_index", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr get_column_index(TableHandle tablehandle,
            [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr nameLen, out NativeException ex);

        public static IntPtr CreateResults(TableHandle tableHandle, SharedRealmHandle sharedRealmHandle, IntPtr objectSchema)
        {
            NativeException nativeException;
            var result = create_results(tableHandle, sharedRealmHandle, objectSchema, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_create_results", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr create_results(TableHandle handle, SharedRealmHandle sharedRealm, IntPtr objectSchema, out NativeException ex);

        public static IntPtr CreateSortedResults(TableHandle tableHandle, SharedRealmHandle sharedRealmHandle, IntPtr objectSchema, SortOrderHandle sortOrderHandle)
        {
            NativeException nativeException;
            var result = create_sorted_results(tableHandle, sharedRealmHandle, objectSchema, sortOrderHandle, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_create_sorted_results", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr create_sorted_results(TableHandle handle, SharedRealmHandle sharedRealm, IntPtr objectSchema, SortOrderHandle sortOrderHandle, out NativeException ex);
    }
}
