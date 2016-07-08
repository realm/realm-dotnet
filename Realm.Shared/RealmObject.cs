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

        /// <summary>
        /// Returns true if this object is managed and represents a row in the database.
        /// If a managed object has been removed from the Realm, it is no longer valid and accessing properties on it
        /// will throw an exception.
        /// Unmanaged objects are always considered valid.
        /// </summary>
        public bool IsValid => _rowHandle?.IsAttached != false;

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

            internal Schema.Object Schema;
        }

        internal void _CopyDataFromBackingFieldsToRow()
        {
            Debug.Assert(this.IsManaged);

            foreach (var property in _metadata.Schema)
            {
                if (property.Type == Schema.PropertyType.Array)
                    continue;

                var field = property.PropertyInfo.DeclaringType.GetField(property.PropertyInfo.GetCustomAttribute<WovenPropertyAttribute>().BackingFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                var value = field.GetValue(this);
                property.PropertyInfo.SetValue(this, value, null);;
            }
        }

        #region Getters
        protected string GetStringValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return NativeTable.GetString(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex);
        }

        protected char GetCharValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return (char) NativeTable.GetInt64(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex);
        }

        protected char? GetNullableCharValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return (char?) NativeTable.GetInt64(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex);
        }

        protected byte GetByteValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return (byte)NativeTable.GetInt64(_metadata.Table, _metadata.ColumnIndices[propertyName],  _rowHandle.RowIndex);
        }

        protected byte? GetNullableByteValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return (byte?)NativeTable.GetNullableInt64(_metadata.Table, _metadata.ColumnIndices[propertyName],  _rowHandle.RowIndex);
        }

        protected short GetInt16Value(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return (short)NativeTable.GetInt64(_metadata.Table, _metadata.ColumnIndices[propertyName],  _rowHandle.RowIndex);
        }

        protected short? GetNullableInt16Value(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return (short?)NativeTable.GetNullableInt64(_metadata.Table, _metadata.ColumnIndices[propertyName],  _rowHandle.RowIndex);
        }

        protected int GetInt32Value(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return (int)NativeTable.GetInt64(_metadata.Table, _metadata.ColumnIndices[propertyName],  _rowHandle.RowIndex);
        }

        protected int? GetNullableInt32Value(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return (int?)NativeTable.GetNullableInt64(_metadata.Table, _metadata.ColumnIndices[propertyName],  _rowHandle.RowIndex);
        }

        protected long GetInt64Value(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return NativeTable.GetInt64(_metadata.Table, _metadata.ColumnIndices[propertyName],  _rowHandle.RowIndex);
        }

        protected long? GetNullableInt64Value(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return NativeTable.GetNullableInt64(_metadata.Table, _metadata.ColumnIndices[propertyName],  _rowHandle.RowIndex);
        }

        protected float GetSingleValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return NativeTable.GetFloat(_metadata.Table, _metadata.ColumnIndices[propertyName],  _rowHandle.RowIndex);
        }

        protected float? GetNullableSingleValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return NativeTable.GetNullableFloat(_metadata.Table, _metadata.ColumnIndices[propertyName],  _rowHandle.RowIndex);
        }

        protected double GetDoubleValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return NativeTable.GetDouble(_metadata.Table, _metadata.ColumnIndices[propertyName],  _rowHandle.RowIndex);
        }

        protected double? GetNullableDoubleValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return NativeTable.GetNullableDouble(_metadata.Table, _metadata.ColumnIndices[propertyName],  _rowHandle.RowIndex);
        }

        protected bool GetBooleanValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return NativeTable.GetBool(_metadata.Table, _metadata.ColumnIndices[propertyName],  _rowHandle.RowIndex);
        }

        protected bool? GetNullableBooleanValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return NativeTable.GetNullableBool(_metadata.Table, _metadata.ColumnIndices[propertyName],  _rowHandle.RowIndex);
        }

        protected DateTimeOffset GetDateTimeOffsetValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return NativeTable.GetTimestampMilliseconds(_metadata.Table, _metadata.ColumnIndices[propertyName],  _rowHandle.RowIndex);
        }

        protected DateTimeOffset? GetNullableDateTimeOffsetValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return NativeTable.GetNullableTimestampMilliseconds(_metadata.Table, _metadata.ColumnIndices[propertyName],  _rowHandle.RowIndex);
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
            var linkedRowPtr = NativeTable.GetLink (_metadata.Table, _metadata.ColumnIndices[propertyName], rowIndex);
            return (T)MakeRealmObject(typeof(T), linkedRowPtr);
        }

        protected byte[] GetByteArrayValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return NativeTable.GetBinary(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex);
        }

        #endregion

        #region Setters

        protected void SetStringValue(string propertyName, string value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeTable.SetString(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex, value);
        }

        protected void SetStringValueUnique(string propertyName, string value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeTable.SetStringUnique(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex, value);
        }

        protected void SetCharValue(string propertyName, char value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeTable.SetNullableInt64(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex, value);
        }

        protected void SetCharValueUnique(string propertyName, char value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeTable.SetInt64Unique(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex, value);
        }

        protected void SetNullableCharValue(string propertyName, char? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeTable.SetNullableInt64(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex, value);
        }

        protected void SetByteValue(string propertyName, byte value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeTable.SetNullableInt64(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex, value);
        }

        protected void SetByteValueUnique(string propertyName, byte value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeTable.SetInt64Unique(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex, value);
        }

        protected void SetNullableByteValue(string propertyName, byte? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeTable.SetNullableInt64(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex, value);
        }

        protected void SetInt16Value(string propertyName, short value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeTable.SetNullableInt64(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex, value);
        }

        protected void SetInt16ValueUnique(string propertyName, short value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeTable.SetInt64Unique(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex, value);
        }

        protected void SetNullableInt16Value(string propertyName, short? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeTable.SetNullableInt64(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex, value);
        }

        protected void SetInt32Value(string propertyName, int value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeTable.SetNullableInt64(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex, value);
        }

        protected void SetInt32ValueUnique(string propertyName, int value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeTable.SetInt64Unique(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex, value);
        }

        protected void SetNullableInt32Value(string propertyName, int? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeTable.SetNullableInt64(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex, value);
        }

        protected void SetInt64Value(string propertyName, long value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeTable.SetNullableInt64(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex, value);
        }

        protected void SetInt64ValueUnique(string propertyName, long value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeTable.SetInt64Unique(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex, value);
        }

        protected void SetNullableInt64Value(string propertyName, long? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeTable.SetNullableInt64(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex, value);
        }

        protected void SetSingleValue(string propertyName, float value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeTable.SetFloat(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex, value);
        }

        protected void SetNullableSingleValue(string propertyName, float? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeTable.SetNullableFloat(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex, value);
        }

        protected void SetDoubleValue(string propertyName, double value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeTable.SetDouble(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex, value);
        }

        protected void SetNullableDoubleValue(string propertyName, double? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeTable.SetNullableDouble(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex, value);
        }

        protected void SetBooleanValue(string propertyName, bool value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeTable.SetBool(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex, value);
        }

        protected void SetNullableBooleanValue(string propertyName, bool? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeTable.SetNullableBool(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex, value);
        }

        protected void SetDateTimeOffsetValue(string propertyName, DateTimeOffset value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeTable.SetTimestampMilliseconds(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex, value);
        }

        protected void SetNullableDateTimeOffsetValue(string propertyName, DateTimeOffset? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeTable.SetNullableTimestampMilliseconds(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex, value);
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
                NativeTable.ClearLink(_metadata.Table, _metadata.ColumnIndices[propertyName], rowIndex);
            }
            else
            {
                if (!value.IsManaged)
                    _realm.Manage(value);
                NativeTable.SetLink(_metadata.Table, _metadata.ColumnIndices[propertyName], rowIndex, value.RowHandle.RowIndex);
            }
        }

        protected void SetByteArrayValue(string propertyName, byte[] value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeTable.SetBinary(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex, value);
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

            // standalone objects cannot participate in the same store check
            if (!IsManaged)
                return false;

            // Return true if the fields match. 
            // Note that the base class is not invoked because it is 
            // System.Object, which defines Equals as reference equality. 
            return RowHandle.Equals(((RealmObject)obj).RowHandle);
        }

    }
}
