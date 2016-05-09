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
        private Metadata _metadata;

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
            _metadata = realm.Metadata[GetType()];
        }

        internal class Metadata
        {
            internal TableHandle Table;

            internal Weaving.IRealmObjectHelper Helper;

            internal Dictionary<string, IntPtr> ColumnIndices;
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

            var rowIndex = _rowHandle.RowIndex;
            var badUTF8msg = $"Corrupted string UTF8 in {propertyName}";

            int bufferSizeNeededChars = 128;
            // First alloc this thread
            if (_realm.stringGetBuffer==IntPtr.Zero) {  // first get of a string in this Realm
                _realm.stringGetBuffer = Marshal.AllocHGlobal((IntPtr)(bufferSizeNeededChars * sizeof(char)));
                _realm.stringGetBufferLen = bufferSizeNeededChars;
            }    

            bool isNull = false;

            // try to read
            int bytesRead = (int)NativeTable.get_string(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex, _realm.stringGetBuffer,
                (IntPtr)_realm.stringGetBufferLen, out isNull);
            if (bytesRead == -1)
            {
                // bad UTF-8 data unable to transcode, vastly unlikely error but could be corrupt file
                throw new RealmInvalidDatabaseException(badUTF8msg);
            }
            if (bytesRead > _realm.stringGetBufferLen)  // need a bigger buffer
            {
                Marshal.FreeHGlobal(_realm.stringGetBuffer);
                _realm.stringGetBuffer = Marshal.AllocHGlobal((IntPtr)(bytesRead * sizeof(char)));
                _realm.stringGetBufferLen = bytesRead;
                // try to read with big buffer
                bytesRead = (int)NativeTable.get_string(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex, _realm.stringGetBuffer,
                    (IntPtr)_realm.stringGetBufferLen, out isNull);
                if (bytesRead == -1)  // bad UTF-8 in full string
                    throw new RealmInvalidDatabaseException(badUTF8msg);
                Debug.Assert(bytesRead <= _realm.stringGetBufferLen);
            }  // needed re-read with expanded buffer

            if (bytesRead == 0)
            {
                if (isNull)
                    return null;
                
                return "";
            }

            return Marshal.PtrToStringUni(_realm.stringGetBuffer, bytesRead);
            // leaving buffer sitting allocated for quick reuse next time we read a string                
        } // GetStringValue

        protected char GetCharValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var rowIndex = _rowHandle.RowIndex;

            var value = NativeTable.get_int64(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex);
            return (char) value;
        }

        protected char? GetNullableCharValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var rowIndex = _rowHandle.RowIndex;

            var retVal = 0L;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeTable.get_nullable_int64(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr) rowIndex, ref retVal));
            return hasValue ? (char)retVal : (char?) null;
        }

        protected byte GetByteValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var rowIndex = _rowHandle.RowIndex;

            var value = NativeTable.get_int64(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex);
            return (byte) value;
        }

        protected byte? GetNullableByteValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var rowIndex = _rowHandle.RowIndex;

            var retVal = 0L;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeTable.get_nullable_int64(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr) rowIndex, ref retVal));
            return hasValue ? (byte)retVal : (byte?) null;
        }

        protected short GetInt16Value(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var rowIndex = _rowHandle.RowIndex;

            var value = NativeTable.get_int64(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex);
            return (short) value;
        }

        protected short? GetNullableInt16Value(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var rowIndex = _rowHandle.RowIndex;

            var retVal = 0L;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeTable.get_nullable_int64(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr) rowIndex, ref retVal));
            return hasValue ? (short)retVal : (short?) null;
        }

        protected int GetInt32Value(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var rowIndex = _rowHandle.RowIndex;

            var value = NativeTable.get_int64(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex);
            return (int) value;
        }

        protected int? GetNullableInt32Value(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var rowIndex = _rowHandle.RowIndex;

            var retVal = 0L;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeTable.get_nullable_int64(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr) rowIndex, ref retVal));
            return hasValue ? (int)retVal : (int?) null;
        }

        protected long GetInt64Value(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var rowIndex = _rowHandle.RowIndex;

            return NativeTable.get_int64(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex);
        }

        protected long? GetNullableInt64Value(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var rowIndex = _rowHandle.RowIndex;

            var retVal = 0L;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeTable.get_nullable_int64(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr) rowIndex, ref retVal));
            return hasValue ? retVal : (long?) null;
        }

        protected float GetSingleValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var rowIndex = _rowHandle.RowIndex;

            return NativeTable.get_float(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex);
        }

        protected float? GetNullableSingleValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var rowIndex = _rowHandle.RowIndex;

            var retVal = 0.0f;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeTable.get_nullable_float(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr) rowIndex, ref retVal));
            return hasValue ? retVal : (float?) null;
        }

        protected double GetDoubleValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var rowIndex = _rowHandle.RowIndex;

            return NativeTable.get_double(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex);
        }

        protected double? GetNullableDoubleValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var rowIndex = _rowHandle.RowIndex;

            var retVal = 0.0d;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeTable.get_nullable_double(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex, ref retVal));
            return hasValue ? retVal : (double?) null;
        }

        protected bool GetBooleanValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var rowIndex = _rowHandle.RowIndex;

            return MarshalHelpers.IntPtrToBool(NativeTable.get_bool(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex));
        }

        protected bool? GetNullableBooleanValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var rowIndex = _rowHandle.RowIndex;

            var retVal = IntPtr.Zero;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeTable.get_nullable_bool(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex, ref retVal));
            return hasValue ? MarshalHelpers.IntPtrToBool(retVal) : (bool?) null;
        }

        protected DateTimeOffset GetDateTimeOffsetValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var rowIndex = _rowHandle.RowIndex;

            var unixTimeMS = NativeTable.get_timestamp_milliseconds(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex);
            return DateTimeOffsetExtensions.FromRealmUnixTimeMilliseconds(unixTimeMS);
        }

        protected DateTimeOffset? GetNullableDateTimeOffsetValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var rowIndex = _rowHandle.RowIndex;

            long unixTimeMS = 0;
            var hasValue = MarshalHelpers.IntPtrToBool(NativeTable.get_nullable_timestamp_milliseconds(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex, ref unixTimeMS));
            return hasValue ? DateTimeOffsetExtensions.FromRealmUnixTimeMilliseconds(unixTimeMS) : (DateTimeOffset?)null;
        }

        protected RealmList<T> GetListValue<T>(string propertyName) where T : RealmObject
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var listHandle = _metadata.Table.TableLinkList (_metadata.ColumnIndices[propertyName], _rowHandle);
            return new RealmList<T>(this, listHandle);
        }

        protected T GetObjectValue<T>(string propertyName) where T : RealmObject
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var rowIndex = _rowHandle.RowIndex;
            var linkedRowPtr = NativeTable.get_link (_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex);
            return (T)MakeRealmObject(typeof(T), linkedRowPtr);
        }

        #endregion

        #region Setters

        protected void SetStringValue(string propertyName, string value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var rowIndex = _rowHandle.RowIndex;

            if (value != null)
                NativeTable.set_string(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex, value, (IntPtr)value.Length);
            else
                NativeTable.set_null(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex);
        }

        protected void SetStringValueUnique(string propertyName, string value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            if (value == null)
                throw new ArgumentException("Object identifiers cannot be null");

            var rowIndex = _rowHandle.RowIndex;

            NativeTable.set_string_unique(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex, value, (IntPtr)value.Length);
        }

        protected void SetCharValue(string propertyName, char value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var rowIndex = _rowHandle.RowIndex;

            NativeTable.set_int64(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex, value);
        }

        protected void SetCharValueUnique(string propertyName, char value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var rowIndex = _rowHandle.RowIndex;

            NativeTable.set_int64_unique(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex, value);
        }

        protected void SetNullableCharValue(string propertyName, char? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var rowIndex = _rowHandle.RowIndex;

            if (value.HasValue)
                NativeTable.set_int64(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex, value.Value);
            else
                NativeTable.set_null(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex);
        }

        protected void SetByteValue(string propertyName, byte value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var rowIndex = _rowHandle.RowIndex;

            NativeTable.set_int64(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex, value);
        }

        protected void SetByteValueUnique(string propertyName, byte value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var rowIndex = _rowHandle.RowIndex;

            NativeTable.set_int64_unique(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex, value);
        }

        protected void SetNullableByteValue(string propertyName, byte? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var rowIndex = _rowHandle.RowIndex;

            if (value.HasValue)
                NativeTable.set_int64(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex, value.Value);
            else
                NativeTable.set_null(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex);
        }

        protected void SetInt16Value(string propertyName, short value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var rowIndex = _rowHandle.RowIndex;

            NativeTable.set_int64(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex, value);
        }

        protected void SetInt16ValueUnique(string propertyName, short value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var rowIndex = _rowHandle.RowIndex;

            NativeTable.set_int64_unique(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex, value);
        }

        protected void SetNullableInt16Value(string propertyName, short? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var rowIndex = _rowHandle.RowIndex;

            if (value.HasValue)
                NativeTable.set_int64(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex, value.Value);
            else
                NativeTable.set_null(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex);
        }

        protected void SetInt32Value(string propertyName, int value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var rowIndex = _rowHandle.RowIndex;

            NativeTable.set_int64(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex, value);
        }

        protected void SetInt32ValueUnique(string propertyName, int value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var rowIndex = _rowHandle.RowIndex;

            NativeTable.set_int64_unique(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex, value);
        }

        protected void SetNullableInt32Value(string propertyName, int? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var rowIndex = _rowHandle.RowIndex;

            if (value.HasValue)
                NativeTable.set_int64(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex, value.Value);
            else
                NativeTable.set_null(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex);
        }

        protected void SetInt64Value(string propertyName, long value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var rowIndex = _rowHandle.RowIndex;

            NativeTable.set_int64(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex, value);
        }

        protected void SetInt64ValueUnique(string propertyName, long value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var rowIndex = _rowHandle.RowIndex;

            NativeTable.set_int64_unique(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex, value);
        }

        protected void SetNullableInt64Value(string propertyName, long? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var rowIndex = _rowHandle.RowIndex;

            if (value.HasValue)
                NativeTable.set_int64(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex, value.Value);
            else
                NativeTable.set_null(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex);
        }

        protected void SetSingleValue(string propertyName, float value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var rowIndex = _rowHandle.RowIndex;

            NativeTable.set_float(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex, value);
        }

        protected void SetNullableSingleValue(string propertyName, float? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var rowIndex = _rowHandle.RowIndex;

            if (value.HasValue)
                NativeTable.set_float(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex, value.Value);
            else
                NativeTable.set_null(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex);
        }

        protected void SetDoubleValue(string propertyName, double value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var rowIndex = _rowHandle.RowIndex;

            NativeTable.set_double(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex, value);
        }

        protected void SetNullableDoubleValue(string propertyName, double? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var rowIndex = _rowHandle.RowIndex;

            if (value.HasValue)
                NativeTable.set_double(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex, value.Value);
            else
                NativeTable.set_null(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex);
        }

        protected void SetBooleanValue(string propertyName, bool value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var rowIndex = _rowHandle.RowIndex;

            NativeTable.set_bool(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex, MarshalHelpers.BoolToIntPtr(value));
        }

        protected void SetNullableBooleanValue(string propertyName, bool? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var rowIndex = _rowHandle.RowIndex;

            if (value.HasValue)
                NativeTable.set_bool(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex, MarshalHelpers.BoolToIntPtr(value.Value));
            else
                NativeTable.set_null(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex);
        }

        protected void SetDateTimeOffsetValue(string propertyName, DateTimeOffset value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var rowIndex = _rowHandle.RowIndex;

            var marshalledValue = value.ToRealmUnixTimeMilliseconds();
            NativeTable.set_timestamp_milliseconds(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex, marshalledValue);
        }

        protected void SetNullableDateTimeOffsetValue(string propertyName, DateTimeOffset? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var rowIndex = _rowHandle.RowIndex;

            if (value.HasValue)
            {
                var marshalledValue = value.Value.ToRealmUnixTimeMilliseconds();
                NativeTable.set_timestamp_milliseconds(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr) rowIndex, marshalledValue);
            }
            else
                NativeTable.set_null(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex);
        }

        // TODO make not generic
        protected void SetObjectValue<T>(string propertyName, T value) where T : RealmObject
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var rowIndex = _rowHandle.RowIndex;
            if (value == null)
            {
                NativeTable.clear_link(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex);
            }
            else
            {
                if (!value.IsManaged)
                    _realm.Manage(value);
                NativeTable.set_link(_metadata.Table, _metadata.ColumnIndices[propertyName], (IntPtr)rowIndex, (IntPtr)value.RowHandle.RowIndex);
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
            var ret = _realm.Metadata[objectType].Helper.CreateInstance();
            var relatedHandle = Realm.CreateRowHandle (rowPtr, _realm.SharedRealmHandle);
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
