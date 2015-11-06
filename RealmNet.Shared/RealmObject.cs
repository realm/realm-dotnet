using System;
using System.Runtime.InteropServices;

namespace RealmNet
{
    public class RealmObject
    {
        private Realm _realm;
        private RowHandle _rowHandle;

        internal RowHandle RowHandle => _rowHandle;

        internal void _Manage(Realm realm, RowHandle rowHandle)
        {
            _realm = realm;
            _rowHandle = rowHandle;
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
            else if (typeof(T) == typeof(bool))
            {
                var value = MarshalHelpers.IntPtrToBool( NativeTable.get_bool(tableHandle, columnIndex, (IntPtr)rowIndex) );
                return (T)Convert.ChangeType(value, typeof(T));
            }
            else if (typeof(T) == typeof(int))  // System.Int32 regardless of bitness
            {
                var value = NativeTable.get_int64(tableHandle, columnIndex, (IntPtr)rowIndex);
                return (T)Convert.ChangeType(value, typeof(T));
            }
            else if (typeof(T) == typeof(Int64)) 
            {
                var value = NativeTable.get_int64(tableHandle, columnIndex, (IntPtr)rowIndex);
                return (T)Convert.ChangeType(value, typeof(T));
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
                NativeTable.set_string(tableHandle, columnIndex, (IntPtr)rowIndex, str, (IntPtr)(str?.Length ?? 0));
            }
            else if (typeof(T) == typeof(bool))
            {
                var marshalledValue = MarshalHelpers.BoolToIntPtr((bool)Convert.ChangeType(value, typeof(bool)));
                NativeTable.set_bool(tableHandle, columnIndex, (IntPtr)rowIndex, marshalledValue);
            }
            else if (typeof(T) == typeof(int))  // System.Int32 regardless of bitness
            {
                Int64 marshalledValue = Convert.ToInt64(value);
                NativeTable.set_int64(tableHandle, columnIndex, (IntPtr)rowIndex, marshalledValue);
            }
            else if (typeof(T) == typeof(Int64))
            {
                Int64 marshalledValue = Convert.ToInt64(value);
                NativeTable.set_int64(tableHandle, columnIndex, (IntPtr)rowIndex, marshalledValue);
            }
            else
                throw new Exception ("Unsupported type " + typeof(T).Name);
        }

        public override bool Equals(object p)
        {
            // If parameter is null, return false. 
            if (ReferenceEquals(p, null))
            {
                return false;
            }

            // Optimization for a common success case. 
            if (ReferenceEquals(this, p))
            {
                return true;
            }

            // If run-time types are not exactly the same, return false. 
            if (this.GetType() != p.GetType())
                return false;

            // Return true if the fields match. 
            // Note that the base class is not invoked because it is 
            // System.Object, which defines Equals as reference equality. 
            return RowHandle.Equals(((RealmObject)p).RowHandle);
        }
    }
}
