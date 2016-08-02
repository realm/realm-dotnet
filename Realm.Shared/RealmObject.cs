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

        internal RowHandle RowHandle => _rowHandle;
        internal Metadata ObjectMetadata => _metadata;

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

        /// <summary>
        /// The <see cref="Realm"/> instance this object belongs to, or <code>null</code> if it is unmanaged.
        /// </summary>
        public Realm Realm => _realm;

        /// <summary>
        /// The <see cref="Schema.ObjectSchema"/> instance that describes how the <see cref="Realm"/> this object belongs to sees it.
        /// </summary>
        public Schema.ObjectSchema ObjectSchema => _metadata?.Schema;

        internal void _Manage(Realm realm, RowHandle rowHandle, Metadata metadata)
        {
            _realm = realm;
            _rowHandle = rowHandle;
            _metadata = metadata;
        }

        internal class Metadata
        {
            internal TableHandle Table;

            internal Weaving.IRealmObjectHelper Helper;

            internal Dictionary<string, IntPtr> ColumnIndices;

            internal Schema.ObjectSchema Schema;
        }

        internal void _CopyDataFromBackingFieldsToRow()
        {
            Debug.Assert(this.IsManaged);

            foreach (var property in _metadata.Schema)
            {
                var field = property.PropertyInfo.DeclaringType.GetField( 
                               property.PropertyInfo.GetCustomAttribute<WovenPropertyAttribute>().BackingFieldName, 
                               BindingFlags.Instance | BindingFlags.NonPublic
                            );
                var value = field?.GetValue(this);
                if (value != null) {
                    var listValue = value as IEnumerable<RealmObject>;
                    if (listValue != null)  // assume it is IList NOT a RealmList so need to wipe afer copy
                    {
                    // cope with ReplaceListGetter creating a getter which assumes 
                    // a backing field for a managed IList is already a RealmList, so null it first
                        field.SetValue(this, null);  // now getter will create a RealmList below
                        var realmList = (ICopyValuesFrom)property.PropertyInfo.GetValue(this, null);
                        realmList.CopyValuesFrom(listValue);
                    } else {
                        property.PropertyInfo.SetValue(this, value, null);
                    }
                }  // only null if blank relationship or string so leave as default
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

            return (char?) NativeTable.GetNullableInt64(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex);
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

            return NativeTable.GetSingle(_metadata.Table, _metadata.ColumnIndices[propertyName],  _rowHandle.RowIndex);
        }

        protected float? GetNullableSingleValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return NativeTable.GetNullableSingle(_metadata.Table, _metadata.ColumnIndices[propertyName],  _rowHandle.RowIndex);
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

            return NativeTable.GetBoolean(_metadata.Table, _metadata.ColumnIndices[propertyName],  _rowHandle.RowIndex);
        }

        protected bool? GetNullableBooleanValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return NativeTable.GetNullableBoolean(_metadata.Table, _metadata.ColumnIndices[propertyName],  _rowHandle.RowIndex);
        }

        protected DateTimeOffset GetDateTimeOffsetValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return NativeTable.GetDateTimeOffset(_metadata.Table, _metadata.ColumnIndices[propertyName],  _rowHandle.RowIndex);
        }

        protected DateTimeOffset? GetNullableDateTimeOffsetValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return NativeTable.GetNullableDateTimeOffset(_metadata.Table, _metadata.ColumnIndices[propertyName],  _rowHandle.RowIndex);
        }

        protected IList<T> GetListValue<T>(string propertyName) where T : RealmObject
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            Schema.Property property;
            _metadata.Schema.TryFindProperty(propertyName, out property);
            var relatedMeta = _realm.Metadata[property.ObjectType];

            var listHandle = _metadata.Table.TableLinkList (_metadata.ColumnIndices[propertyName], _rowHandle.RowIndex);
            return new RealmList<T>(_realm, listHandle, relatedMeta);
        }

        protected T GetObjectValue<T>(string propertyName) where T : RealmObject
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var rowIndex = _rowHandle.RowIndex;
            var linkedRowPtr = NativeTable.GetLink (_metadata.Table, _metadata.ColumnIndices[propertyName], rowIndex);
            if (linkedRowPtr == IntPtr.Zero)
                return null;

            Schema.Property property;
            _metadata.Schema.TryFindProperty(propertyName, out property);
            var objectType = property.ObjectType;
            return (T)_realm.MakeObjectForRow(objectType, linkedRowPtr);
        }

        protected byte[] GetByteArrayValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return NativeTable.GetByteArray(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex);
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

            NativeTable.SetSingle(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex, value);
        }

        protected void SetNullableSingleValue(string propertyName, float? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeTable.SetNullableSingle(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex, value);
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

            NativeTable.SetBoolean(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex, value);
        }

        protected void SetNullableBooleanValue(string propertyName, bool? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeTable.SetNullableBoolean(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex, value);
        }

        protected void SetDateTimeOffsetValue(string propertyName, DateTimeOffset value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeTable.SetDateTimeOffset(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex, value);
        }

        protected void SetNullableDateTimeOffsetValue(string propertyName, DateTimeOffset? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            NativeTable.SetNullableDateTimeOffset(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex, value);
        }


        // Originally a generic fallback, now used only for RealmObject To-One relationship properties
        // most other properties handled with woven type-specific setters above
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

            NativeTable.SetByteArray(_metadata.Table, _metadata.ColumnIndices[propertyName], _rowHandle.RowIndex, value);
        }

        #endregion

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
