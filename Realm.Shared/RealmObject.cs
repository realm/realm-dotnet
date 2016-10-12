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
using System.Collections.Generic;
using System.Diagnostics;

namespace Realms
{
    /// <summary>
    /// Base for any object that can be persisted in a Realm.
    /// </summary>
    /// <remarks>
    /// Has a Preserve attribute to attempt to preserve all subtypes without having to weave.
    /// </remarks>
    [Preserve(AllMembers = true, Conditional = false)]
    public class RealmObject
    {
        private Realm _realm;
        private ObjectHandle _objectHandle;
        private Metadata _metadata;

        internal ObjectHandle ObjectHandle => _objectHandle;

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
        public bool IsValid => _objectHandle?.IsValid != false;

        /// <summary>
        /// The <see cref="Realm"/> instance this object belongs to, or <code>null</code> if it is unmanaged.
        /// </summary>
        public Realm Realm => _realm;

        /// <summary>
        /// The <see cref="Schema.ObjectSchema"/> instance that describes how the <see cref="Realm"/> this object belongs to sees it.
        /// </summary>
        public Schema.ObjectSchema ObjectSchema => _metadata?.Schema;

        internal void _Manage(Realm realm, ObjectHandle objectHandle, Metadata metadata)
        {
            _realm = realm;
            _objectHandle = objectHandle;
            _metadata = metadata;
        }

        internal class Metadata
        {
            internal TableHandle Table;

            internal Weaving.IRealmObjectHelper Helper;

            internal Dictionary<string, IntPtr> PropertyIndices;

            internal int PrimaryKeyColumnIndex;

            internal Schema.ObjectSchema Schema;
        }

        internal void _CopyDataFromBackingFieldsToRow()
        {
            Debug.Assert(IsManaged, "Can't copy from unmanaged object.");
            _metadata.Helper.CopyToRealm(this);
        }

        #region Getters

        protected string GetStringValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return NativeTable.GetString(_objectHandle, _metadata.PropertyIndices[propertyName]);
        }

        protected char GetCharValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return (char)NativeTable.GetInt64(_objectHandle, _metadata.PropertyIndices[propertyName]);
        }

        protected char? GetNullableCharValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return (char?)NativeTable.GetNullableInt64(_objectHandle, _metadata.PropertyIndices[propertyName]);
        }

        protected byte GetByteValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return (byte)NativeTable.GetInt64(_objectHandle, _metadata.PropertyIndices[propertyName]);
        }

        protected byte? GetNullableByteValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return (byte?)NativeTable.GetNullableInt64(_objectHandle, _metadata.PropertyIndices[propertyName]);
        }

        protected short GetInt16Value(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return (short)NativeTable.GetInt64(_objectHandle, _metadata.PropertyIndices[propertyName]);
        }

        protected short? GetNullableInt16Value(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return (short?)NativeTable.GetNullableInt64(_objectHandle, _metadata.PropertyIndices[propertyName]);
        }

        protected int GetInt32Value(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return (int)NativeTable.GetInt64(_objectHandle, _metadata.PropertyIndices[propertyName]);
        }

        protected int? GetNullableInt32Value(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return (int?)NativeTable.GetNullableInt64(_objectHandle, _metadata.PropertyIndices[propertyName]);
        }

        protected long GetInt64Value(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return NativeTable.GetInt64(_objectHandle, _metadata.PropertyIndices[propertyName]);
        }

        protected long? GetNullableInt64Value(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return NativeTable.GetNullableInt64(_objectHandle, _metadata.PropertyIndices[propertyName]);
        }

        protected float GetSingleValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return NativeTable.GetSingle(_objectHandle, _metadata.PropertyIndices[propertyName]);
        }

        protected float? GetNullableSingleValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return NativeTable.GetNullableSingle(_objectHandle, _metadata.PropertyIndices[propertyName]);
        }

        protected double GetDoubleValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return NativeTable.GetDouble(_objectHandle, _metadata.PropertyIndices[propertyName]);
        }

        protected double? GetNullableDoubleValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return NativeTable.GetNullableDouble(_objectHandle, _metadata.PropertyIndices[propertyName]);
        }

        protected bool GetBooleanValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return NativeTable.GetBoolean(_objectHandle, _metadata.PropertyIndices[propertyName]);
        }

        protected bool? GetNullableBooleanValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return NativeTable.GetNullableBoolean(_objectHandle, _metadata.PropertyIndices[propertyName]);
        }

        protected DateTimeOffset GetDateTimeOffsetValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return NativeTable.GetDateTimeOffset(_objectHandle, _metadata.PropertyIndices[propertyName]);
        }

        protected DateTimeOffset? GetNullableDateTimeOffsetValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return NativeTable.GetNullableDateTimeOffset(_objectHandle, _metadata.PropertyIndices[propertyName]);
        }

        protected IList<T> GetListValue<T>(string propertyName) where T : RealmObject
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            Schema.Property property;
            _metadata.Schema.TryFindProperty(propertyName, out property);
            var relatedMeta = _realm.Metadata[property.ObjectType];

            var listHandle = _objectHandle.TableLinkList(_metadata.PropertyIndices[propertyName]);
            return new RealmList<T>(_realm, listHandle, relatedMeta);
        }

        protected T GetObjectValue<T>(string propertyName) where T : RealmObject
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            var linkedRowPtr = NativeTable.GetLink(_objectHandle, _metadata.PropertyIndices[propertyName]);
            if (linkedRowPtr == IntPtr.Zero)
            {
                return null;
            }

            Schema.Property property;
            _metadata.Schema.TryFindProperty(propertyName, out property);
            var objectType = property.ObjectType;
            return (T)_realm.MakeObjectForRow(objectType, linkedRowPtr);
        }

        protected byte[] GetByteArrayValue(string propertyName)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            return NativeTable.GetByteArray(_objectHandle, _metadata.PropertyIndices[propertyName]);
        }

        #endregion

        #region Setters

        protected void SetStringValue(string propertyName, string value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
            {
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");
            }

            NativeTable.SetString(_objectHandle, _metadata.PropertyIndices[propertyName], value);
        }

        protected void SetStringValueUnique(string propertyName, string value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
            {
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");
            }

            NativeTable.SetStringUnique(_objectHandle, _metadata.PropertyIndices[propertyName], value);
        }

        protected void SetCharValue(string propertyName, char value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
            {
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");
            }

            NativeTable.SetNullableInt64(_objectHandle, _metadata.PropertyIndices[propertyName], value);
        }

        protected void SetCharValueUnique(string propertyName, char value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
            {
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");
            }

            NativeTable.SetInt64Unique(_objectHandle, _metadata.PropertyIndices[propertyName], value);
        }

        protected void SetNullableCharValue(string propertyName, char? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
            {
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");
            }

            NativeTable.SetNullableInt64(_objectHandle, _metadata.PropertyIndices[propertyName], value);
        }

        protected void SetByteValue(string propertyName, byte value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
            {
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");
            }

            NativeTable.SetNullableInt64(_objectHandle, _metadata.PropertyIndices[propertyName], value);
        }

        protected void SetByteValueUnique(string propertyName, byte value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
            {
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");
            }

            NativeTable.SetInt64Unique(_objectHandle, _metadata.PropertyIndices[propertyName], value);
        }

        protected void SetNullableByteValue(string propertyName, byte? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
            {
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");
            }

            NativeTable.SetNullableInt64(_objectHandle, _metadata.PropertyIndices[propertyName], value);
        }

        protected void SetInt16Value(string propertyName, short value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
            {
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");
            }

            NativeTable.SetNullableInt64(_objectHandle, _metadata.PropertyIndices[propertyName], value);
        }

        protected void SetInt16ValueUnique(string propertyName, short value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
            {
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");
            }

            NativeTable.SetInt64Unique(_objectHandle, _metadata.PropertyIndices[propertyName], value);
        }

        protected void SetNullableInt16Value(string propertyName, short? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
            {
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");
            }

            NativeTable.SetNullableInt64(_objectHandle, _metadata.PropertyIndices[propertyName], value);
        }

        protected void SetInt32Value(string propertyName, int value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
            {
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");
            }

            NativeTable.SetNullableInt64(_objectHandle, _metadata.PropertyIndices[propertyName], value);
        }

        protected void SetInt32ValueUnique(string propertyName, int value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
            {
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");
            }

            NativeTable.SetInt64Unique(_objectHandle, _metadata.PropertyIndices[propertyName], value);
        }

        protected void SetNullableInt32Value(string propertyName, int? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
            {
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");
            }

            NativeTable.SetNullableInt64(_objectHandle, _metadata.PropertyIndices[propertyName], value);
        }

        protected void SetInt64Value(string propertyName, long value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
            {
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");
            }

            NativeTable.SetNullableInt64(_objectHandle, _metadata.PropertyIndices[propertyName], value);
        }

        protected void SetInt64ValueUnique(string propertyName, long value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
            {
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");
            }

            NativeTable.SetInt64Unique(_objectHandle, _metadata.PropertyIndices[propertyName], value);
        }

        protected void SetNullableInt64Value(string propertyName, long? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
            {
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");
            }

            NativeTable.SetNullableInt64(_objectHandle, _metadata.PropertyIndices[propertyName], value);
        }

        protected void SetSingleValue(string propertyName, float value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
            {
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");
            }

            NativeTable.SetSingle(_objectHandle, _metadata.PropertyIndices[propertyName], value);
        }

        protected void SetNullableSingleValue(string propertyName, float? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
            {
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");
            }

            NativeTable.SetNullableSingle(_objectHandle, _metadata.PropertyIndices[propertyName], value);
        }

        protected void SetDoubleValue(string propertyName, double value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
            {
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");
            }

            NativeTable.SetDouble(_objectHandle, _metadata.PropertyIndices[propertyName], value);
        }

        protected void SetNullableDoubleValue(string propertyName, double? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
            {
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");
            }

            NativeTable.SetNullableDouble(_objectHandle, _metadata.PropertyIndices[propertyName], value);
        }

        protected void SetBooleanValue(string propertyName, bool value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
            {
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");
            }

            NativeTable.SetBoolean(_objectHandle, _metadata.PropertyIndices[propertyName], value);
        }

        protected void SetNullableBooleanValue(string propertyName, bool? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
            {
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");
            }

            NativeTable.SetNullableBoolean(_objectHandle, _metadata.PropertyIndices[propertyName], value);
        }

        protected void SetDateTimeOffsetValue(string propertyName, DateTimeOffset value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
            {
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");
            }

            NativeTable.SetDateTimeOffset(_objectHandle, _metadata.PropertyIndices[propertyName], value);
        }

        protected void SetNullableDateTimeOffsetValue(string propertyName, DateTimeOffset? value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
            {
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");
            }

            NativeTable.SetNullableDateTimeOffset(_objectHandle, _metadata.PropertyIndices[propertyName], value);
        }

        // Originally a generic fallback, now used only for RealmObject To-One relationship properties
        // most other properties handled with woven type-specific setters above
        protected void SetObjectValue<T>(string propertyName, T value) where T : RealmObject
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
            {
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");
            }

            if (value == null)
            {
                NativeTable.ClearLink(_objectHandle, _metadata.PropertyIndices[propertyName]);
            }
            else
            {
                if (!value.IsManaged)
                {
                    _realm.Manage(value);
                }

                NativeTable.SetLink(_objectHandle, _metadata.PropertyIndices[propertyName], value.ObjectHandle);
            }
        }

        protected void SetByteArrayValue(string propertyName, byte[] value)
        {
            Debug.Assert(_realm != null, "Object is not managed, but managed access was attempted");

            if (!_realm.IsInTransaction)
            {
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");
            }

            NativeTable.SetByteArray(_objectHandle, _metadata.PropertyIndices[propertyName], value);
        }

        #endregion

        /// <summary>
        /// Compare objects with identity query for persistent objects.
        /// </summary>
        /// <remarks>Persisted RealmObjects map their properties directly to the realm with no caching so multiple instances of a given object always refer to the same store.</remarks>
        /// <param name="obj">Object being compared against to see if is the same C# object or maps to the same managed object in Realm.</param>
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
            if (GetType() != obj.GetType())
            {
                return false;
            }

            // standalone objects cannot participate in the same store check
            if (!IsManaged)
            {
                return false;
            }

            // Return true if the fields match. 
            // Note that the base class is not invoked because it is 
            // System.Object, which defines Equals as reference equality. 
            return ObjectHandle.Equals(((RealmObject)obj).ObjectHandle);
        }
    }
}