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
                    if (genericType == typeof(IList<>))
                    {
                        var elementType = prop.Info.PropertyType.GetGenericArguments().Single();
                        var getListValue = typeof(RealmObject).GetMethod("GetListValue", BindingFlags.Instance | BindingFlags.NonPublic)
                                                                     .MakeGenericMethod(elementType);
                        var add = getListValue.ReturnType.GetMethod("Add");

                        // TODO: get rid of all this reflection. Handle the [MapTo] attribute
                        var realmList = getListValue.Invoke(this, new object[] { prop.Info.Name });
                        prop.Field.SetValue(this, realmList);
                        foreach (var item in value as IEnumerable)
                        {
                            add.Invoke(realmList, new[] { item });
                        }

                        continue;
                    }
                    else if (genericType == typeof(RealmList<>))
                    {
                        continue;
                    }
                }

                prop.Info.SetValue(this, value, null);
            }
        }

        protected T GetValue<T>(string propertyName)
        {
            if (_realm == null)
                throw new Exception("This object is not managed. Create through CreateObject");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            if (typeof(T) == typeof(string))
            {
                long bufferSizeNeededChars = 16;
                IntPtr buffer;
                long currentBufferSizeChars;

                do
                {
                    buffer = MarshalHelpers.StrAllocateBuffer(out currentBufferSizeChars, bufferSizeNeededChars);
                    bufferSizeNeededChars = (long)NativeTable.get_string(tableHandle, columnIndex, (IntPtr)rowIndex, buffer,
                            (IntPtr)currentBufferSizeChars);

                } while (MarshalHelpers.StrBufferOverflow(buffer, currentBufferSizeChars, bufferSizeNeededChars));
                return (T)Convert.ChangeType(MarshalHelpers.StrBufToStr(buffer, (int)bufferSizeNeededChars), typeof(T));
            }
            if (typeof(T) == typeof(bool))
            {
                var value = MarshalHelpers.IntPtrToBool( NativeTable.get_bool(tableHandle, columnIndex, (IntPtr)rowIndex) );
                return (T)Convert.ChangeType(value, typeof(T));
            }
            if (typeof(T) == typeof(int))  // System.Int32 regardless of process being 32 or 64bit
            {
                var value = NativeTable.get_int64(tableHandle, columnIndex, (IntPtr)rowIndex);
                return (T)Convert.ChangeType(value, typeof(T));
            }
            if (typeof(T) == typeof(Int64)) 
            {
                var value = NativeTable.get_int64(tableHandle, columnIndex, (IntPtr)rowIndex);
                return (T)Convert.ChangeType(value, typeof(T));
            }
            if (typeof(T) == typeof(float)) 
            {
                var value = NativeTable.get_float(tableHandle, columnIndex, (IntPtr)rowIndex);
                return (T)Convert.ChangeType(value, typeof(T));
            }
            if (typeof(T) == typeof(double)) 
            {
                var value = NativeTable.get_double(tableHandle, columnIndex, (IntPtr)rowIndex);
                return (T)Convert.ChangeType(value, typeof(T));
            }
            if (typeof(T) == typeof(DateTimeOffset))
            {
                var unixTimeSeconds = NativeTable.get_datetime_seconds(tableHandle, columnIndex, (IntPtr)rowIndex);
                var value = DateTimeOffsetExtensions.FromUnixTimeSeconds(unixTimeSeconds);
                return (T)(object)value;
            }
            else
                throw new Exception ("Unsupported type " + typeof(T).Name);
        }

        protected void SetValue<T>(string propertyName, T value)
        {
            if (_realm == null)
                throw new Exception("This object is not managed. Create through CreateObject");

            if (!_realm.IsInTransaction)
                throw new RealmOutsideTransactionException("Cannot set values outside transaction");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;

            if (typeof(T) == typeof(string)) 
            {
                var str = value as string;
                NativeTable.set_string (tableHandle, columnIndex, (IntPtr)rowIndex, str, (IntPtr)(str?.Length ?? 0));
            } 
            else if (typeof(T) == typeof(bool)) 
            {
                var marshalledValue = MarshalHelpers.BoolToIntPtr ((bool)Convert.ChangeType (value, typeof(bool)));
                NativeTable.set_bool (tableHandle, columnIndex, (IntPtr)rowIndex, marshalledValue);
            } 
            else if (typeof(T) == typeof(int)) 
            {  // System.Int32 regardless of process being 32 or 64bit
                Int64 marshalledValue = Convert.ToInt64 (value);
                NativeTable.set_int64 (tableHandle, columnIndex, (IntPtr)rowIndex, marshalledValue);
            } 
            else if (typeof(T) == typeof(Int64)) 
            {
                Int64 marshalledValue = Convert.ToInt64 (value);
                NativeTable.set_int64 (tableHandle, columnIndex, (IntPtr)rowIndex, marshalledValue);
            } 
            else if (typeof(T) == typeof(float)) 
            {
                float marshalledValue = Convert.ToSingle (value);
                NativeTable.set_float (tableHandle, columnIndex, (IntPtr)rowIndex, marshalledValue);
            } 
            else if (typeof(T) == typeof(double)) 
            {
                double marshalledValue = Convert.ToDouble (value);
                NativeTable.set_double (tableHandle, columnIndex, (IntPtr)rowIndex, marshalledValue);
            }
            else if (typeof(T) == typeof(DateTimeOffset))
            {
                Int64 marshalledValue = ((DateTimeOffset)(object)value).ToUnixTimeSeconds();
                NativeTable.set_datetime_seconds(tableHandle, columnIndex, (IntPtr)rowIndex, marshalledValue);
            }
            else
                throw new Exception ("Unsupported type " + typeof(T).Name);
        }


        protected RealmList<T> GetListValue<T>(string propertyName) where T : RealmObject
        {
            if (_realm == null)
                throw new Exception("This object is not managed. Create through CreateObject");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var listHandle = tableHandle.TableLinkList (columnIndex, _rowHandle);
            return new RealmList<T>(this, listHandle);
        }

        protected void SetListValue<T>(string propertyName, RealmList<T> value) where T : RealmObject
        {
            throw new NotImplementedException ("Setting a relationship list is not yet implemented");
        }


        /**
         * Shared factory to make an object in the realm from a known row
         * @param rowPtr may be null if a relationship lookup has failed.
        */ 
        internal RealmObject MakeRealmObject(System.Type objectType, IntPtr rowPtr) {
            if (rowPtr == (IntPtr)0)
                return null;  // typically no related object
            RealmObject ret = (RealmObject)Activator.CreateInstance(objectType);
            var relatedHandle = Realm.CreateRowHandle (rowPtr);
            ret._Manage(_realm, relatedHandle);
            return ret;
        }

        protected T GetObjectValue<T>(string propertyName) where T : RealmObject
        {
            if (_realm == null)
                throw new Exception("This object is not managed. Create through CreateObject");

            var tableHandle = _realm._tableHandles[GetType()];
            var columnIndex = NativeTable.get_column_index(tableHandle, propertyName, (IntPtr)propertyName.Length);
            var rowIndex = _rowHandle.RowIndex;
            var linkedRowPtr = NativeTable.get_link (tableHandle, columnIndex, (IntPtr)rowIndex);
            return (T)MakeRealmObject(typeof(T), linkedRowPtr);
        }

        // TODO make not generic
        protected void SetObjectValue<T>(string propertyName, T value) where T : RealmObject
        {
            if (_realm == null)
                throw new Exception("This object is not managed. Create through CreateObject");

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
