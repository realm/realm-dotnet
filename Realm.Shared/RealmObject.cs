/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Realms
{
    /// <summary>
    /// Base for any object that can be persisted in a Realm.
    /// </summary>
    public class RealmObject
    {
        private Realm _realm;
        private RowHandle _rowHandle;

        internal Realm Realm => _realm;
        internal RowHandle RowHandle => _rowHandle;

        /// <summary>
        /// Allows you to check if the object has been associated with a Realm, either at creation or via Realm.Manage.
        /// </summary>
        public bool IsManaged => _realm != null;

        internal void _Manage(Realm realm, RowHandle rowHandle)
        {
            _realm = realm;
            _rowHandle = rowHandle;
        }

        internal void _CopyDataFromBackingFieldsToRow()
        {
            Debug.Assert(this.IsManaged);

            var thisType = this.GetType();
            var wovenProperties = from prop in thisType.GetProperties()
                                  let backingField = prop.GetCustomAttributes(false)
                                                         .OfType<WovenPropertyAttribute>()
                                                         .Select(a => a.BackingFieldName)
                                                         .SingleOrDefault()
                                  where backingField != null
                                  select new { Info = prop, Field = thisType.GetField(backingField, BindingFlags.Instance | BindingFlags.NonPublic) };

            foreach (var prop in wovenProperties)
            {
                var value = prop.Field.GetValue(this);
                if (prop.Info.PropertyType.IsGenericType)
                {
                    var genericType = prop.Info.PropertyType.GetGenericTypeDefinition();
                    if (genericType == typeof(RealmList<>))
                    {
                        continue;
                    }
                }

                prop.Info.SetValue(this, value, null);
            }
        }


        #region Getters

        protected string GetStringValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            long bufferSizeNeededChars = 128;
            IntPtr buffer;
            long currentBufferSizeChars;

            do
            {
                buffer = MarshalHelpers.StrAllocateBuffer(out currentBufferSizeChars, bufferSizeNeededChars);
                bufferSizeNeededChars = (long)NativeTable.get_string(tableHandle, columnIndex, (IntPtr)rowIndex, buffer,
                        (IntPtr)currentBufferSizeChars);

            } while (MarshalHelpers.StrBufferOverflow(buffer, currentBufferSizeChars, bufferSizeNeededChars));
            return MarshalHelpers.StrBufToStr(buffer, (int) bufferSizeNeededChars);
        }

        protected char GetCharValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            var value = NativeTable.get_int64(tableHandle, columnIndex, (IntPtr)rowIndex);
            return (char) value;
        }

        protected char? GetNullableCharValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            var retVal = 0L;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeTable.get_nullable_int64(tableHandle, columnIndex, (IntPtr) rowIndex, ref retVal));
            return hasValue ? (char)retVal : (char?) null;
        }

        protected byte GetByteValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            var value = NativeTable.get_int64(tableHandle, columnIndex, (IntPtr)rowIndex);
            return (byte) value;
        }

        protected byte? GetNullableByteValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            var retVal = 0L;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeTable.get_nullable_int64(tableHandle, columnIndex, (IntPtr) rowIndex, ref retVal));
            return hasValue ? (byte)retVal : (byte?) null;
        }

        protected short GetInt16Value(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            var value = NativeTable.get_int64(tableHandle, columnIndex, (IntPtr)rowIndex);
            return (short) value;
        }

        protected short? GetNullableInt16Value(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            var retVal = 0L;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeTable.get_nullable_int64(tableHandle, columnIndex, (IntPtr) rowIndex, ref retVal));
            return hasValue ? (short)retVal : (short?) null;
        }

        protected int GetInt32Value(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            var value = NativeTable.get_int64(tableHandle, columnIndex, (IntPtr)rowIndex);
            return (int) value;
        }

        protected int? GetNullableInt32Value(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            var retVal = 0L;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeTable.get_nullable_int64(tableHandle, columnIndex, (IntPtr) rowIndex, ref retVal));
            return hasValue ? (int)retVal : (int?) null;
        }

        protected long GetInt64Value(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            return NativeTable.get_int64(tableHandle, columnIndex, (IntPtr)rowIndex);
        }

        protected long? GetNullableInt64Value(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            var retVal = 0L;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeTable.get_nullable_int64(tableHandle, columnIndex, (IntPtr) rowIndex, ref retVal));
            return hasValue ? retVal : (long?) null;
        }

        protected float GetSingleValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            return NativeTable.get_float(tableHandle, columnIndex, (IntPtr)rowIndex);
        }

        protected float? GetNullableSingleValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            var retVal = 0.0f;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeTable.get_nullable_float(tableHandle, columnIndex, (IntPtr) rowIndex, ref retVal));
            return hasValue ? retVal : (float?) null;
        }

        protected double GetDoubleValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            return NativeTable.get_double(tableHandle, columnIndex, (IntPtr)rowIndex);
        }

        protected double? GetNullableDoubleValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            var retVal = 0.0d;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeTable.get_nullable_double(tableHandle, columnIndex, (IntPtr)rowIndex, ref retVal));
            return hasValue ? retVal : (double?) null;
        }

        protected bool GetBooleanValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            return MarshalHelpers.IntPtrToBool(NativeTable.get_bool(tableHandle, columnIndex, (IntPtr)rowIndex));
        }

        protected bool? GetNullableBooleanValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            var retVal = IntPtr.Zero;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeTable.get_nullable_bool(tableHandle, columnIndex, (IntPtr)rowIndex, ref retVal));
            return hasValue ? MarshalHelpers.IntPtrToBool(retVal) : (bool?) null;
        }

        protected DateTimeOffset GetDateTimeOffsetValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            var unixTimeSeconds = NativeTable.get_datetime_seconds(tableHandle, columnIndex, (IntPtr)rowIndex);
            return DateTimeOffsetExtensions.FromUnixTimeSeconds(unixTimeSeconds);
        }

        protected DateTimeOffset? GetNullableDateTimeOffsetValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            long unixTimeSeconds = 0;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeTable.get_nullable_datetime_seconds(tableHandle, columnIndex, (IntPtr)rowIndex, ref unixTimeSeconds));
            return hasValue ? DateTimeOffsetExtensions.FromUnixTimeSeconds(unixTimeSeconds) : (DateTimeOffset?)null;
        }

        protected RealmList<T> GetListValue<T>(string propertyName) where T : RealmObject
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var listHandle = tableHandle.TableLinkList (columnIndex, _rowHandle);
            return new RealmList<T>(this, listHandle);
        }

        protected T GetObjectValue<T>(string propertyName) where T : RealmObject
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;
            var linkedRowPtr = NativeTable.get_link (tableHandle, columnIndex, (IntPtr)rowIndex);
            return (T)MakeRealmObject(typeof(T), linkedRowPtr);
        }

        #endregion

        #region Setters

        protected void SetStringValue(string propertyName, string value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            if (value != null)
                NativeTable.set_string(tableHandle, columnIndex, (IntPtr)rowIndex, value, (IntPtr)value.Length);
            else
                NativeTable.set_null(tableHandle, columnIndex, (IntPtr)rowIndex);
        }

        protected void SetStringValueUnique(string propertyName, string value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            if (value == null)
                throw new ArgumentException("Object identifiers cannot be null");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            NativeTable.set_string_unique(tableHandle, columnIndex, (IntPtr)rowIndex, value, (IntPtr)value.Length);
        }

        protected void SetCharValue(string propertyName, char value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            NativeTable.set_int64(tableHandle, columnIndex, (IntPtr)rowIndex, value);
        }

        protected void SetCharValueUnique(string propertyName, char value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            NativeTable.set_int64_unique(tableHandle, columnIndex, (IntPtr)rowIndex, value);
        }

        protected void SetNullableCharValue(string propertyName, char? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            if (value.HasValue)
                NativeTable.set_int64(tableHandle, columnIndex, (IntPtr)rowIndex, value.Value);
            else
                NativeTable.set_null(tableHandle, columnIndex, (IntPtr)rowIndex);
        }

        protected void SetByteValue(string propertyName, byte value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            NativeTable.set_int64(tableHandle, columnIndex, (IntPtr)rowIndex, value);
        }

        protected void SetByteValueUnique(string propertyName, byte value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            NativeTable.set_int64_unique(tableHandle, columnIndex, (IntPtr)rowIndex, value);
        }

        protected void SetNullableByteValue(string propertyName, byte? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            if (value.HasValue)
                NativeTable.set_int64(tableHandle, columnIndex, (IntPtr)rowIndex, value.Value);
            else
                NativeTable.set_null(tableHandle, columnIndex, (IntPtr)rowIndex);
        }

        protected void SetInt16Value(string propertyName, short value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            NativeTable.set_int64(tableHandle, columnIndex, (IntPtr)rowIndex, value);
        }

        protected void SetInt16ValueUnique(string propertyName, short value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            NativeTable.set_int64_unique(tableHandle, columnIndex, (IntPtr)rowIndex, value);
        }

        protected void SetNullableInt16Value(string propertyName, short? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            if (value.HasValue)
                NativeTable.set_int64(tableHandle, columnIndex, (IntPtr)rowIndex, value.Value);
            else
                NativeTable.set_null(tableHandle, columnIndex, (IntPtr)rowIndex);
        }

        protected void SetInt32Value(string propertyName, int value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            NativeTable.set_int64(tableHandle, columnIndex, (IntPtr)rowIndex, value);
        }

        protected void SetInt32ValueUnique(string propertyName, int value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            NativeTable.set_int64_unique(tableHandle, columnIndex, (IntPtr)rowIndex, value);
        }

        protected void SetNullableInt32Value(string propertyName, int? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            if (value.HasValue)
                NativeTable.set_int64(tableHandle, columnIndex, (IntPtr)rowIndex, value.Value);
            else
                NativeTable.set_null(tableHandle, columnIndex, (IntPtr)rowIndex);
        }

        protected void SetInt64Value(string propertyName, long value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            NativeTable.set_int64(tableHandle, columnIndex, (IntPtr)rowIndex, value);
        }

        protected void SetInt64ValueUnique(string propertyName, long value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            NativeTable.set_int64_unique(tableHandle, columnIndex, (IntPtr)rowIndex, value);
        }

        protected void SetNullableInt64Value(string propertyName, long? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            if (value.HasValue)
                NativeTable.set_int64(tableHandle, columnIndex, (IntPtr)rowIndex, value.Value);
            else
                NativeTable.set_null(tableHandle, columnIndex, (IntPtr)rowIndex);
        }

        protected void SetSingleValue(string propertyName, float value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            NativeTable.set_float(tableHandle, columnIndex, (IntPtr)rowIndex, value);
        }

        protected void SetNullableSingleValue(string propertyName, float? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            if (value.HasValue)
                NativeTable.set_float(tableHandle, columnIndex, (IntPtr)rowIndex, value.Value);
            else
                NativeTable.set_null(tableHandle, columnIndex, (IntPtr)rowIndex);
        }

        protected void SetDoubleValue(string propertyName, double value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            NativeTable.set_double(tableHandle, columnIndex, (IntPtr)rowIndex, value);
        }

        protected void SetNullableDoubleValue(string propertyName, double? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            if (value.HasValue)
                NativeTable.set_double(tableHandle, columnIndex, (IntPtr)rowIndex, value.Value);
            else
                NativeTable.set_null(tableHandle, columnIndex, (IntPtr)rowIndex);
        }

        protected void SetBooleanValue(string propertyName, bool value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            NativeTable.set_bool(tableHandle, columnIndex, (IntPtr)rowIndex, MarshalHelpers.BoolToIntPtr(value));
        }

        protected void SetNullableBooleanValue(string propertyName, bool? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            if (value.HasValue)
                NativeTable.set_bool(tableHandle, columnIndex, (IntPtr)rowIndex, MarshalHelpers.BoolToIntPtr(value.Value));
            else
                NativeTable.set_null(tableHandle, columnIndex, (IntPtr)rowIndex);
        }

        protected void SetDateTimeOffsetValue(string propertyName, DateTimeOffset value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            var marshalledValue = value.ToUnixTimeSeconds();
            NativeTable.set_datetime_seconds(tableHandle, columnIndex, (IntPtr)rowIndex, marshalledValue);
        }

        protected void SetNullableDateTimeOffsetValue(string propertyName, DateTimeOffset? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            if (value.HasValue)
            {
                var marshalledValue = value.Value.ToUnixTimeSeconds();
                NativeTable.set_datetime_seconds(tableHandle, columnIndex, (IntPtr) rowIndex, marshalledValue);
            }
            else
                NativeTable.set_null(tableHandle, columnIndex, (IntPtr)rowIndex);
        }

        // TODO make not generic
        protected void SetObjectValue<T>(string propertyName, T value) where T : RealmObject
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;
            if (value == null)
            {
                NativeTable.clear_link(tableHandle, columnIndex, (IntPtr)rowIndex);
            }
            else
            {
                if (!value.IsManaged)
                    _realm.Manage(value);
                NativeTable.set_link(tableHandle, columnIndex, (IntPtr)rowIndex, (IntPtr)value.RowHandle.RowIndex);
            }
        }

        #endregion

        /**
         * Shared factory to make an object in the realm from a known row
         * @param rowPtr may be null if a relationship lookup has failed.
        */
        internal RealmObject MakeRealmObject(Type objectType, IntPtr rowPtr) {
            if (rowPtr == IntPtr.Zero)
                return null;  // typically no related object
            var ret = (RealmObject)Activator.CreateInstance(objectType);
            var relatedHandle = Realm.CreateRowHandle (rowPtr);
            ret._Manage(_realm, relatedHandle);
            return ret;
        }


        /// <summary>
        /// Compare objects with identity query for persistent objects.
        /// </summary>
        /// <remarks>Persisted RealmObjects map their properties directly to the realm with no caching so multiple instances of a given object always refer to the same store.</remarks>
        /// <param name="obj"></param>
        /// <returns>True when objects are the same memory object or refer to the same persisted object.</returns>
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

            // If run-time types are not exactly the same, return false. 
            if (this.GetType() != obj.GetType())
                return false;

            // Return true if the fields match. 
            // Note that the base class is not invoked because it is 
            // System.Object, which defines Equals as reference equality. 
            return RowHandle.Equals(((RealmObject)obj).RowHandle);
        }

    }
}
