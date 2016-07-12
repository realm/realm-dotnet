using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Realms
{
    [Preserve(AllMembers = true)]
    internal static class RealmObjectOps
    {
        public static string GetStringValue(Realm realm, TableHandle table, RowHandle row, IntPtr columnIndex)
        {
            int bufferSizeNeededChars = 128;
            // First alloc this thread
            if (realm.stringGetBuffer == IntPtr.Zero)
            {  // first get of a string in this Realm
                realm.stringGetBuffer = Marshal.AllocHGlobal((IntPtr)(bufferSizeNeededChars * sizeof(char)));
                realm.stringGetBufferLen = bufferSizeNeededChars;
            }

            bool isNull = false;

            // try to read
            int bytesRead = (int)NativeTable.get_string(table, columnIndex, row.RowIndex, realm.stringGetBuffer,
                (IntPtr)realm.stringGetBufferLen, out isNull);
            if (bytesRead == -1)
            {
                // bad UTF-8 data unable to transcode, vastly unlikely error but could be corrupt file
                throw new RealmInvalidDatabaseException("Corrupted string UTF8");
            }
            if (bytesRead > realm.stringGetBufferLen)  // need a bigger buffer
            {
                Marshal.FreeHGlobal(realm.stringGetBuffer);
                realm.stringGetBuffer = Marshal.AllocHGlobal((IntPtr)(bytesRead * sizeof(char)));
                realm.stringGetBufferLen = bytesRead;
                // try to read with big buffer
                bytesRead = (int)NativeTable.get_string(table, columnIndex, row.RowIndex, realm.stringGetBuffer,
                    (IntPtr)realm.stringGetBufferLen, out isNull);
                if (bytesRead == -1)  // bad UTF-8 in full string
                    throw new RealmInvalidDatabaseException("Corrupted string UTF8");
                Debug.Assert(bytesRead <= realm.stringGetBufferLen);
            }

            if (bytesRead == 0)
            {
                if (isNull)
                    return null;

                return "";
            }

            return Marshal.PtrToStringUni(realm.stringGetBuffer, bytesRead);
            // leaving buffer sitting allocated for quick reuse next time we read a string                
        }

        public static char GetCharValue(TableHandle table, RowHandle row, IntPtr columnIndex)
        {
            return (char)NativeTable.get_int64(table, columnIndex, row.RowIndex);
        }

        public static char? GetNullableCharValue(TableHandle table, RowHandle row, IntPtr columnIndex)
        {
            var retVal = 0L;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeTable.get_nullable_int64(table, columnIndex, row.RowIndex, ref retVal));
            return hasValue ? (char?)retVal : null;
        }

        public static byte GetByteValue(TableHandle table, RowHandle row, IntPtr columnIndex)
        {
            return (byte)NativeTable.get_int64(table, columnIndex, row.RowIndex);
        }

        public static byte? GetNullableByteValue(TableHandle table, RowHandle row, IntPtr columnIndex)
        {
            var retVal = 0L;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeTable.get_nullable_int64(table, columnIndex, row.RowIndex, ref retVal));
            return hasValue ? (byte?)retVal : null;
        }

        public static short GetInt16Value(TableHandle table, RowHandle row, IntPtr columnIndex)
        {
            return (short)NativeTable.get_int64(table, columnIndex, row.RowIndex);
        }

        public static short? GetNullableInt16Value(TableHandle table, RowHandle row, IntPtr columnIndex)
        {
            var retVal = 0L;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeTable.get_nullable_int64(table, columnIndex, row.RowIndex, ref retVal));
            return hasValue ? (short?)retVal : null;
        }

        public static int GetInt32Value(TableHandle table, RowHandle row, IntPtr columnIndex)
        {
            return (int)NativeTable.get_int64(table, columnIndex, row.RowIndex);
        }

        public static int? GetNullableInt32Value(TableHandle table, RowHandle row, IntPtr columnIndex)
        {
            var retVal = 0L;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeTable.get_nullable_int64(table, columnIndex, row.RowIndex, ref retVal));
            return hasValue ? (int?)retVal : null;
        }

        public static long GetInt64Value(TableHandle table, RowHandle row, IntPtr columnIndex)
        {
            return NativeTable.get_int64(table, columnIndex, row.RowIndex);
        }

        public static long? GetNullableInt64Value(TableHandle table, RowHandle row, IntPtr columnIndex)
        {
            var retVal = 0L;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeTable.get_nullable_int64(table, columnIndex, row.RowIndex, ref retVal));
            return hasValue ? (long?)retVal : null;
        }

        public static float GetSingleValue(TableHandle table, RowHandle row, IntPtr columnIndex)
        {
            return NativeTable.get_float(table, columnIndex, row.RowIndex);
        }

        public static float? GetNullableSingleValue(TableHandle table, RowHandle row, IntPtr columnIndex)
        {
            var retVal = .0f;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeTable.get_nullable_float(table, columnIndex, row.RowIndex, ref retVal));
            return hasValue ? (float?)retVal : null;
        }

        public static double GetDoubleValue(TableHandle table, RowHandle row, IntPtr columnIndex)
        {
            return NativeTable.get_double(table, columnIndex, row.RowIndex);
        }

        public static double? GetNullableDoubleValue(TableHandle table, RowHandle row, IntPtr columnIndex)
        {
            var retVal = .0d;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeTable.get_nullable_double(table, columnIndex, row.RowIndex, ref retVal));
            return hasValue ? (double?)retVal : null;
        }

        public static bool GetBooleanValue(TableHandle table, RowHandle row, IntPtr columnIndex)
        {
            return MarshalHelpers.IntPtrToBool(NativeTable.get_bool(table, columnIndex, row.RowIndex));
        }

        public static bool? GetNullableBooleanValue(TableHandle table, RowHandle row, IntPtr columnIndex)
        {
            var retVal = IntPtr.Zero;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeTable.get_nullable_bool(table, columnIndex, row.RowIndex, ref retVal));
            return hasValue ? MarshalHelpers.IntPtrToBool(retVal) : (bool?)null;
        }

        public static DateTimeOffset GetDateTimeOffsetValue(TableHandle table, RowHandle row, IntPtr columnIndex)
        {
            var unixTimeMS = NativeTable.get_timestamp_milliseconds(table, columnIndex, row.RowIndex);
            return DateTimeOffsetExtensions.FromRealmUnixTimeMilliseconds(unixTimeMS);
        }

        public static DateTimeOffset? GetNullableDateTimeOffsetValue(TableHandle table, RowHandle row, IntPtr columnIndex)
        {
            long unixTimeMS = 0;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeTable.get_nullable_timestamp_milliseconds(table, columnIndex, row.RowIndex, ref unixTimeMS));
            return hasValue ? DateTimeOffsetExtensions.FromRealmUnixTimeMilliseconds(unixTimeMS) : (DateTimeOffset?)null;
        }

        public static RealmList<T> GetListValue<T>(Realm realm, TableHandle table, RowHandle row, IntPtr columnIndex, string objectType) where T : RealmObject
        {
            var listHandle = table.TableLinkList(columnIndex, row);
            return new RealmList<T>(realm, listHandle, objectType);
        }

        public static T GetObjectValue<T>(Realm realm, TableHandle table, RowHandle row, IntPtr columnIndex, string objectType) where T : RealmObject
        {
            var linkedRowPtr = NativeTable.get_link(table, columnIndex, row.RowIndex);
            if (linkedRowPtr == IntPtr.Zero)
                return null;

            return (T)realm.MakeObjectForRow(objectType, Realm.CreateRowHandle(linkedRowPtr, realm.SharedRealmHandle));
        }

        public static byte[] GetByteArrayValue(TableHandle table, RowHandle row, IntPtr columnIndex)
        {
            int bufferSize;
            IntPtr buffer;
            if (NativeTable.get_binary(table, columnIndex, row.RowIndex, out buffer, out bufferSize) != IntPtr.Zero)
            {
                var bytes = new byte[bufferSize];
                Marshal.Copy(buffer, bytes, 0, bufferSize);
                return bytes;
            }

            return null;
        }

        public static void SetStringValue(TableHandle table, RowHandle row, IntPtr columnIndex, string value)
        {
            if (value != null)
                NativeTable.set_string(table, columnIndex, row.RowIndex, value, (IntPtr)value.Length);
            else
                NativeTable.set_null(table, columnIndex, row.RowIndex);
        }

        public static void SetStringValueUnique(TableHandle table, RowHandle row, IntPtr columnIndex, string value)
        {
            if (value == null)
                throw new ArgumentException("Object identifiers cannot be null");

            NativeTable.set_string_unique(table, columnIndex, row.RowIndex, value, (IntPtr)value.Length);
        }

        public static void SetCharValue(TableHandle table, RowHandle row, IntPtr columnIndex, char value)
        {
            NativeTable.set_int64(table, columnIndex, row.RowIndex, value);
        }

        public static void SetCharValueUnique(TableHandle table, RowHandle row, IntPtr columnIndex, char value)
        {
            NativeTable.set_int64_unique(table, columnIndex, row.RowIndex, value);
        }

        public static void SetNullableCharValue(TableHandle table, RowHandle row, IntPtr columnIndex, char? value)
        {
            if (value.HasValue)
                NativeTable.set_int64(table, columnIndex, row.RowIndex, value.Value);
            else
                NativeTable.set_null(table, columnIndex, row.RowIndex);
        }

        public static void SetByteValue(TableHandle table, RowHandle row, IntPtr columnIndex, byte value)
        {
            NativeTable.set_int64(table, columnIndex, row.RowIndex, value);
        }

        public static void SetByteValueUnique(TableHandle table, RowHandle row, IntPtr columnIndex, byte value)
        {
            NativeTable.set_int64_unique(table, columnIndex, row.RowIndex, value);
        }

        public static void SetNullableByteValue(TableHandle table, RowHandle row, IntPtr columnIndex, byte? value)
        {
            if (value.HasValue)
                NativeTable.set_int64(table, columnIndex, row.RowIndex, value.Value);
            else
                NativeTable.set_null(table, columnIndex, row.RowIndex);
        }

        public static void SetInt16Value(TableHandle table, RowHandle row, IntPtr columnIndex, short value)
        {
            NativeTable.set_int64(table, columnIndex, row.RowIndex, value);
        }

        public static void SetInt16ValueUnique(TableHandle table, RowHandle row, IntPtr columnIndex, short value)
        {
            NativeTable.set_int64_unique(table, columnIndex, row.RowIndex, value);
        }

        public static void SetNullableInt16Value(TableHandle table, RowHandle row, IntPtr columnIndex, short? value)
        {
            if (value.HasValue)
                NativeTable.set_int64(table, columnIndex, row.RowIndex, value.Value);
            else
                NativeTable.set_null(table, columnIndex, row.RowIndex);
        }

        public static void SetInt32Value(TableHandle table, RowHandle row, IntPtr columnIndex, int value)
        {
            NativeTable.set_int64(table, columnIndex, row.RowIndex, value);
        }

        public static void SetInt32ValueUnique(TableHandle table, RowHandle row, IntPtr columnIndex, int value)
        {
            NativeTable.set_int64_unique(table, columnIndex, row.RowIndex, value);
        }

        public static void SetNullableInt32Value(TableHandle table, RowHandle row, IntPtr columnIndex, int? value)
        {
            if (value.HasValue)
                NativeTable.set_int64(table, columnIndex, row.RowIndex, value.Value);
            else
                NativeTable.set_null(table, columnIndex, row.RowIndex);
        }

        public static void SetInt64Value(TableHandle table, RowHandle row, IntPtr columnIndex, long value)
        {
            NativeTable.set_int64(table, columnIndex, row.RowIndex, value);
        }

        public static void SetInt64ValueUnique(TableHandle table, RowHandle row, IntPtr columnIndex, long value)
        {
            NativeTable.set_int64_unique(table, columnIndex, row.RowIndex, value);
        }

        public static void SetNullableInt64Value(TableHandle table, RowHandle row, IntPtr columnIndex, long? value)
        {
            if (value.HasValue)
                NativeTable.set_int64(table, columnIndex, row.RowIndex, value.Value);
            else
                NativeTable.set_null(table, columnIndex, row.RowIndex);
        }

        public static void SetSingleValue(TableHandle table, RowHandle row, IntPtr columnIndex, float value)
        {
            NativeTable.set_float(table, columnIndex, row.RowIndex, value);
        }

        public static void SetNullableSingleValue(TableHandle table, RowHandle row, IntPtr columnIndex, float? value)
        {
            if (value.HasValue)
                NativeTable.set_float(table, columnIndex, row.RowIndex, value.Value);
            else
                NativeTable.set_null(table, columnIndex, row.RowIndex);
        }

        public static void SetDoubleValue(TableHandle table, RowHandle row, IntPtr columnIndex, double value)
        {
            NativeTable.set_double(table, columnIndex, row.RowIndex, value);
        }

        public static void SetNullableDoubleValue(TableHandle table, RowHandle row, IntPtr columnIndex, double? value)
        {
            if (value.HasValue)
                NativeTable.set_double(table, columnIndex, row.RowIndex, value.Value);
            else
                NativeTable.set_null(table, columnIndex, row.RowIndex);
        }

        public static void SetBooleanValue(TableHandle table, RowHandle row, IntPtr columnIndex, bool value)
        {
            NativeTable.set_bool(table, columnIndex, row.RowIndex, MarshalHelpers.BoolToIntPtr(value));
        }

        public static void SetNullableBooleanValue(TableHandle table, RowHandle row, IntPtr columnIndex, bool? value)
        {
            if (value.HasValue)
                NativeTable.set_bool(table, columnIndex, row.RowIndex, MarshalHelpers.BoolToIntPtr(value.Value));
            else
                NativeTable.set_null(table, columnIndex, row.RowIndex);
        }

        public static void SetDateTimeOffsetValue(TableHandle table, RowHandle row, IntPtr columnIndex, DateTimeOffset value)
        {
            var marshalledValue = value.ToRealmUnixTimeMilliseconds();
            NativeTable.set_timestamp_milliseconds(table, columnIndex, row.RowIndex, marshalledValue);
        }

        public static void SetNullableDateTimeOffsetValue(TableHandle table, RowHandle row, IntPtr columnIndex, DateTimeOffset? value)
        {
            if (value.HasValue)
            {
                var marshalledValue = value.Value.ToRealmUnixTimeMilliseconds();
                NativeTable.set_timestamp_milliseconds(table, columnIndex, row.RowIndex, marshalledValue);
            }
            else
                NativeTable.set_null(table, columnIndex, row.RowIndex);
        }

        public static void SetObjectValue<T>(Realm realm, TableHandle table, RowHandle row, IntPtr columnIndex, T value) where T : RealmObject
        {
            if (value == null)
            {
                NativeTable.clear_link(table, columnIndex, row.RowIndex);
            }
            else
            {
                if (!value.IsManaged)
                    realm.Manage(value);
                NativeTable.set_link(table, columnIndex, row.RowIndex, value.RowHandle.RowIndex);
            }
        }

        public static void SetByteArrayValue(TableHandle table, RowHandle row, IntPtr columnIndex, byte[] value)
        {
            if (value == null)
            {
                NativeTable.set_null(table, columnIndex, row.RowIndex);
            }
            else if (value.Length == 0)
            {
                // empty byte arrays are expressed in terms of a BinaryData object with a dummy pointer and zero size
                // that's how core differentiates between empty and null buffers
                NativeTable.set_binary(table, columnIndex, row.RowIndex, (IntPtr)0x1, IntPtr.Zero);
            }
            else
            {
                unsafe
                {
                    fixed (byte* buffer = value)
                    {
                        NativeTable.set_binary(table, columnIndex, row.RowIndex, (IntPtr)buffer, (IntPtr)value.LongLength);
                    }
                }
            }
        }
    }
}
