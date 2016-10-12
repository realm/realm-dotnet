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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Realms.Native;

namespace Realms
{
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter")]
    [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1121:UseBuiltInTypeAlias")]
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

        public static void SetDateTimeOffset(ObjectHandle handle, IntPtr propertyIndex, DateTimeOffset value)
        {
            NativeException nativeException;
            var ticks = value.ToUniversalTime().Ticks;
            set_timestamp_ticks(handle, propertyIndex, ticks, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public static void SetNullableDateTimeOffset(ObjectHandle handle, IntPtr propertyIndex, DateTimeOffset? value)
        {
            NativeException nativeException;
            if (value.HasValue)
            {
                var ticks = value.Value.ToUniversalTime().Ticks;
                set_timestamp_ticks(handle, propertyIndex, ticks, out nativeException);
            }
            else
            {
                set_null(handle, propertyIndex, out nativeException);
            }

            nativeException.ThrowIfNecessary();
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_timestamp_ticks", CallingConvention = CallingConvention.Cdecl)]
        private static extern void set_timestamp_ticks(ObjectHandle handle, IntPtr propertyIndex, Int64 value, out NativeException ex);

        public static DateTimeOffset GetDateTimeOffset(ObjectHandle handle, IntPtr propertyIndex)
        {
            NativeException nativeException;
            var ticks = get_timestamp_ticks(handle, propertyIndex, out nativeException);
            nativeException.ThrowIfNecessary();
            return new DateTimeOffset(ticks, TimeSpan.Zero);
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_timestamp_ticks", CallingConvention = CallingConvention.Cdecl)]
        private static extern Int64 get_timestamp_ticks(ObjectHandle handle, IntPtr propertyIndex, out NativeException ex);

        public static DateTimeOffset? GetNullableDateTimeOffset(ObjectHandle handle, IntPtr propertyIndex)
        {
            NativeException nativeException;
            long ticks;
            var hasValue = MarshalHelpers.IntPtrToBool(get_nullable_timestamp_ticks(handle, propertyIndex, out ticks, out nativeException));
            nativeException.ThrowIfNecessary();
            return hasValue ? new DateTimeOffset(ticks, TimeSpan.Zero) : (DateTimeOffset?)null;
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_nullable_timestamp_ticks", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr get_nullable_timestamp_ticks(ObjectHandle handle, IntPtr propertyIndex, out Int64 retVal, out NativeException ex);

        public static void SetString(ObjectHandle handle, IntPtr propertyIndex, string value)
        {
            NativeException nativeException;
            if (value != null)
            {
                set_string(handle, propertyIndex, value, (IntPtr)value.Length, out nativeException);
            }
            else
            {
                set_null(handle, propertyIndex, out nativeException);
            }

            nativeException.ThrowIfNecessary();
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern void set_string(ObjectHandle handle, IntPtr propertyIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen, out NativeException ex);

        public static void SetStringUnique(ObjectHandle handle, IntPtr propertyIndex, string value)
        {
            if (value == null)
            {
                throw new ArgumentException("Object identifiers cannot be null");
            }

            NativeException nativeException;
            set_string_unique(handle, propertyIndex, value, (IntPtr)value.Length, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_string_unique", CallingConvention = CallingConvention.Cdecl)]
        private static extern void set_string_unique(ObjectHandle handle, IntPtr propertyIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen, out NativeException ex);

        public static string GetString(ObjectHandle handle, IntPtr propertyIndex)
        {
            var bufferSizeNeededChars = 128;
            
            // First alloc this thread
            var stringGetBuffer = Marshal.AllocHGlobal((IntPtr)(bufferSizeNeededChars * sizeof(char)));
            var stringGetBufferLen = bufferSizeNeededChars;

            bool isNull;
            NativeException nativeException;

            // try to read
            var bytesRead = (int)get_string(handle, propertyIndex, stringGetBuffer,
                (IntPtr)stringGetBufferLen, out isNull, out nativeException);
            nativeException.ThrowIfNecessary();
            if (bytesRead == -1)
            {
                throw new RealmInvalidDatabaseException("Corrupted string data");
            }

            if (bytesRead > stringGetBufferLen) // need a bigger buffer
            {
                Marshal.FreeHGlobal(stringGetBuffer);
                stringGetBuffer = Marshal.AllocHGlobal((IntPtr)(bytesRead * sizeof(char)));
                stringGetBufferLen = bytesRead;

                // try to read with big buffer
                bytesRead = (int)get_string(handle, propertyIndex, stringGetBuffer,
                    (IntPtr)stringGetBufferLen, out isNull, out nativeException);
                nativeException.ThrowIfNecessary();
                if (bytesRead == -1) // bad UTF-8 in full string
                {
                    throw new RealmInvalidDatabaseException("Corrupted string data");
                }

                Debug.Assert(bytesRead <= stringGetBufferLen, "Buffer must have overflowed.");
            } // needed re-read with expanded buffer

            return bytesRead != 0 ? Marshal.PtrToStringUni(stringGetBuffer, bytesRead) : (isNull ? null : string.Empty);
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr get_string(ObjectHandle handle, IntPtr propertyIndex,
            IntPtr buffer, IntPtr bufsize, [MarshalAs(UnmanagedType.I1)] out bool isNull, out NativeException ex);

        public static void SetLink(ObjectHandle handle, IntPtr propertyIndex, ObjectHandle targetHandle)
        {
            NativeException nativeException;
            set_link(handle, propertyIndex, targetHandle, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_link", CallingConvention = CallingConvention.Cdecl)]
        private static extern void set_link(ObjectHandle handle, IntPtr propertyIndex, ObjectHandle targetHandle, out NativeException ex);

        public static void ClearLink(ObjectHandle handle, IntPtr propertyIndex)
        {
            NativeException nativeException;
            clear_link(handle, propertyIndex, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_clear_link", CallingConvention = CallingConvention.Cdecl)]
        private static extern void clear_link(ObjectHandle handle, IntPtr propertyIndex, out NativeException ex);

        public static IntPtr GetLink(ObjectHandle handle, IntPtr propertyIndex)
        {
            NativeException nativeException;
            var result = get_link(handle, propertyIndex, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_link", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr get_link(ObjectHandle handle, IntPtr propertyIndex, out NativeException ex);

        public static IntPtr GetLinklist(ObjectHandle handle, IntPtr propertyIndex)
        {
            NativeException nativeException;
            var result = get_linklist(handle, propertyIndex, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_linklist", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr get_linklist(ObjectHandle handle, IntPtr propertyIndex, out NativeException ex);

        public static bool LinklistIsEmpty(ObjectHandle handle, IntPtr propertyIndex)
        {
            NativeException nativeException;
            var result = linklist_is_empty(handle, propertyIndex, out nativeException);
            nativeException.ThrowIfNecessary();
            return MarshalHelpers.IntPtrToBool(result);
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_linklist_is_empty", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr linklist_is_empty(ObjectHandle handle, IntPtr propertyIndex, out NativeException ex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_null", CallingConvention = CallingConvention.Cdecl)]
        private static extern void set_null(ObjectHandle handle, IntPtr propertyIndex, out NativeException ex);

        public static void SetBoolean(ObjectHandle handle, IntPtr propertyIndex, bool value)
        {
            NativeException nativeException;
            set_bool(handle, propertyIndex, MarshalHelpers.BoolToIntPtr(value), out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public static void SetNullableBoolean(ObjectHandle handle, IntPtr propertyIndex, bool? value)
        {
            NativeException nativeException;
            if (value.HasValue)
            {
                set_bool(handle, propertyIndex, MarshalHelpers.BoolToIntPtr(value.Value), out nativeException);
            }
            else
            {
                set_null(handle, propertyIndex, out nativeException);
            }

            nativeException.ThrowIfNecessary();
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern void set_bool(ObjectHandle handle, IntPtr propertyIndex, IntPtr value, out NativeException ex);

        public static bool GetBoolean(ObjectHandle handle, IntPtr propertyIndex)
        {
            NativeException nativeException;
            var result = get_bool(handle, propertyIndex, out nativeException);
            nativeException.ThrowIfNecessary();
            return MarshalHelpers.IntPtrToBool(result);
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr get_bool(ObjectHandle handle, IntPtr propertyIndex, out NativeException ex);

        public static bool? GetNullableBoolean(ObjectHandle handle, IntPtr propertyIndex)
        {
            NativeException nativeException;
            IntPtr value;
            var hasValue = MarshalHelpers.IntPtrToBool(get_nullable_bool(handle, propertyIndex, out value, out nativeException));
            nativeException.ThrowIfNecessary();
            return hasValue ? MarshalHelpers.IntPtrToBool(value) : (bool?)null;
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_nullable_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr get_nullable_bool(ObjectHandle handle, IntPtr propertyIndex, out IntPtr retVal, out NativeException ex);

        public static void SetInt64(ObjectHandle handle, IntPtr propertyIndex, long value)
        {
            NativeException nativeException;
            set_int64(handle, propertyIndex, value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public static void SetNullableInt64(ObjectHandle handle, IntPtr propertyIndex, long? value)
        {
            NativeException nativeException;
            if (value.HasValue)
            {
                set_int64(handle, propertyIndex, value.Value, out nativeException);
            }
            else
            {
                set_null(handle, propertyIndex, out nativeException);
            }

            nativeException.ThrowIfNecessary();
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_int64", CallingConvention = CallingConvention.Cdecl)]
        private static extern void set_int64(ObjectHandle handle, IntPtr propertyIndex, Int64 value, out NativeException ex);

        public static void SetInt64Unique(ObjectHandle handle, IntPtr propertyIndex, long value)
        {
            NativeException nativeException;
            set_int64_unique(handle, propertyIndex, value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_int64_unique", CallingConvention = CallingConvention.Cdecl)]
        private static extern void set_int64_unique(ObjectHandle handle, IntPtr propertyIndex, Int64 value, out NativeException ex);

        public static long GetInt64(ObjectHandle handle, IntPtr propertyIndex)
        {
            NativeException nativeException;
            var result = get_int64(handle, propertyIndex, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_int64", CallingConvention = CallingConvention.Cdecl)]
        private static extern Int64 get_int64(ObjectHandle handle, IntPtr propertyIndex, out NativeException ex);

        public static long? GetNullableInt64(ObjectHandle handle, IntPtr propertyIndex)
        {
            NativeException nativeException;
            long value;
            var hasValue = MarshalHelpers.IntPtrToBool(get_nullable_int64(handle, propertyIndex, out value, out nativeException));
            nativeException.ThrowIfNecessary();
            return hasValue ? value : (long?)null;
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_nullable_int64", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr get_nullable_int64(ObjectHandle handle, IntPtr propertyIndex, out Int64 retVal, out NativeException ex);

        public static void SetSingle(ObjectHandle handle, IntPtr propertyIndex, float value)
        {
            NativeException nativeException;
            set_float(handle, propertyIndex, value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public static void SetNullableSingle(ObjectHandle handle, IntPtr propertyIndex, float? value)
        {
            NativeException nativeException;
            if (value.HasValue)
            {
                set_float(handle, propertyIndex, value.Value, out nativeException);
            }
            else
            {
                set_null(handle, propertyIndex, out nativeException);
            }

            nativeException.ThrowIfNecessary();
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern void set_float(ObjectHandle handle, IntPtr propertyIndex, Single value, out NativeException ex);

        public static float GetSingle(ObjectHandle handle, IntPtr propertyIndex)
        {
            NativeException nativeException;
            var result = get_float(handle, propertyIndex, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern Single get_float(ObjectHandle handle, IntPtr propertyIndex, out NativeException ex);

        public static float? GetNullableSingle(ObjectHandle handle, IntPtr propertyIndex)
        {
            NativeException nativeException;
            float value;
            var hasValue = MarshalHelpers.IntPtrToBool(get_nullable_float(handle, propertyIndex, out value, out nativeException));
            nativeException.ThrowIfNecessary();
            return hasValue ? value : (float?)null;
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_nullable_float", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr get_nullable_float(ObjectHandle handle, IntPtr propertyIndex, out Single retVal, out NativeException ex);

        public static void SetDouble(ObjectHandle handle, IntPtr propertyIndex, double value)
        {
            NativeException nativeException;
            set_double(handle, propertyIndex, value, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        public static void SetNullableDouble(ObjectHandle handle, IntPtr propertyIndex, double? value)
        {
            NativeException nativeException;
            if (value.HasValue)
            {
                set_double(handle, propertyIndex, value.Value, out nativeException);
            }
            else
            {
                set_null(handle, propertyIndex, out nativeException);
            }

            nativeException.ThrowIfNecessary();
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern void set_double(ObjectHandle handle, IntPtr propertyIndex, Double value, out NativeException ex);

        public static double GetDouble(ObjectHandle handle, IntPtr propertyIndex)
        {
            NativeException nativeException;
            var result = get_double(handle, propertyIndex, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern Double get_double(ObjectHandle handle, IntPtr propertyIndex, out NativeException ex);

        public static double? GetNullableDouble(ObjectHandle handle, IntPtr propertyIndex)
        {
            NativeException nativeException;
            double value;
            var hasValue = MarshalHelpers.IntPtrToBool(get_nullable_double(handle, propertyIndex, out value, out nativeException));
            nativeException.ThrowIfNecessary();
            return hasValue ? value : (double?)null;
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_nullable_double", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr get_nullable_double(ObjectHandle handle, IntPtr propertyIndex, out Double retVal, out NativeException ex);

        public static unsafe void SetByteArray(ObjectHandle handle, IntPtr propertyIndex, byte[] value)
        {
            NativeException nativeException;
            if (value == null)
            {
                set_null(handle, propertyIndex, out nativeException);
            }
            else if (value.Length == 0)
            {
                // empty byte arrays are expressed in terms of a BinaryData object with a dummy pointer and zero size
                // that's how core differentiates between empty and null buffers
                set_binary(handle, propertyIndex, (IntPtr)0x1, IntPtr.Zero, out nativeException);
            }
            else
            {
                fixed (byte* buffer = value)
                {
                    set_binary(handle, propertyIndex, (IntPtr)buffer, (IntPtr)value.LongLength, out nativeException);
                }
            }

            nativeException.ThrowIfNecessary();
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_binary", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr set_binary(ObjectHandle handle, IntPtr propertyIndex,
            IntPtr buffer, IntPtr bufferLength, out NativeException ex);

        public static byte[] GetByteArray(ObjectHandle handle, IntPtr propertyIndex)
        {
            NativeException nativeException;
            int bufferSize;
            IntPtr buffer;
            var hasValue = get_binary(handle, propertyIndex, out buffer, out bufferSize, out nativeException) != IntPtr.Zero;
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
        private static extern IntPtr get_binary(ObjectHandle handle, IntPtr propertyIndex,
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

        public static void RemoveRow(ObjectHandle handle)
        {
            NativeException nativeException;
            remove_row(handle, out nativeException);
            nativeException.ThrowIfNecessary();
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_remove_row", CallingConvention = CallingConvention.Cdecl)]
        private static extern void remove_row(ObjectHandle handle, out NativeException ex);

        // returns -1 if the column string does not match a column index
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

        public static IntPtr CreateResults(TableHandle tableHandle, SharedRealmHandle sharedRealmHandle)
        {
            NativeException nativeException;
            var result = create_results(tableHandle, sharedRealmHandle, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_create_results", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr create_results(TableHandle handle, SharedRealmHandle sharedRealm, out NativeException ex);

        public static IntPtr CreateSortedResults(TableHandle tableHandle, SharedRealmHandle sharedRealmHandle, SortDescriptorBuilder sortDescriptorBuilder)
        {
            NativeException nativeException;
            var marshaledValues = sortDescriptorBuilder.Flatten();
            var result = create_sorted_results(tableHandle, sharedRealmHandle, marshaledValues.Item2, (IntPtr)marshaledValues.Item2.Length, marshaledValues.Item1, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_create_sorted_results", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr create_sorted_results(TableHandle handle, SharedRealmHandle sharedRealm,
                [MarshalAs(UnmanagedType.LPArray), In]SortDescriptorBuilder.Clause.Marshalable[] sortClauses, IntPtr clauseCount,
                [MarshalAs(UnmanagedType.LPArray), In]IntPtr[] flattenedColumnIndices,
                out NativeException ex);

        internal static IntPtr RowForPrimaryKey(TableHandle tableHandle, int primaryKeyColumnIndex, string id)
        {
            NativeException nativeException;
            var result = row_for_string_primarykey(tableHandle, (IntPtr)primaryKeyColumnIndex, id, (IntPtr)id.Length, out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "row_for_string_primarykey", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr row_for_string_primarykey(TableHandle handle, IntPtr propertyIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen, out NativeException ex);

        internal static IntPtr RowForPrimaryKey(TableHandle tableHandle, int primaryKeyColumnIndex, long id)
        {
            NativeException nativeException;
            var result = row_for_int_primarykey(tableHandle, (IntPtr)primaryKeyColumnIndex, id,  out nativeException);
            nativeException.ThrowIfNecessary();
            return result;
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "row_for_int_primarykey", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr row_for_int_primarykey(TableHandle handle, IntPtr propertyIndex, Int64 value, out NativeException ex);
    }
}