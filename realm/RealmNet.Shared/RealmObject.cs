using System;
using System.Runtime.InteropServices;

namespace RealmNet
{
    public class RealmObject
    {
        private Realm _realm;
        private IRowHandle _rowHandle;

        internal void _Manage(Realm realm, IRowHandle rowHandle)
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
                    buffer = StrAllocateBuffer(out currentBufferSizeChars, bufferSizeNeededChars);
                    bufferSizeNeededChars = (long)NativeTable.get_string(tableHandle, columnIndex, (IntPtr)rowIndex, buffer,
                            (IntPtr)currentBufferSizeChars);

                } while (StrBufferOverflow(buffer, currentBufferSizeChars, bufferSizeNeededChars));
                return (T)Convert.ChangeType(StrBufToStr(buffer, (int)bufferSizeNeededChars), typeof(T));
            }
            else if (typeof(T) == typeof(bool))
            {
                var value = IntPtrToBool( NativeTable.get_bool(tableHandle, columnIndex, (IntPtr)rowIndex) );
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
        }

        #region helpers

        static IntPtr BoolToIntPtr(Boolean value)
        {
            return value ? (IntPtr)1 : (IntPtr)0;
        }

        static Boolean IntPtrToBool(IntPtr value)
        {
            return (IntPtr)1 == value;
        }

        static IntPtr StrAllocateBuffer(out long currentBufferSizeChars, long bufferSizeNeededChars)
        {
            currentBufferSizeChars = bufferSizeNeededChars;
            return Marshal.AllocHGlobal((IntPtr)(bufferSizeNeededChars * sizeof(char)));
            //allocHGlobal instead of  AllocCoTaskMem because allcHGlobal allows lt 2 gig on 64 bit (not that .net supports that right now, but at least this allocation will work with lt 32 bit strings)   
        }

        static string StrBufToStr(IntPtr buffer, int bufferSizeNeededChars)
        {
            string retStr = bufferSizeNeededChars > 0 ? Marshal.PtrToStringUni(buffer, bufferSizeNeededChars) : "";
            //return "" if the string is empty, otherwise copy data from the buffer
            Marshal.FreeHGlobal(buffer);
            return retStr;
        }

        static Boolean StrBufferOverflow(IntPtr buffer, long currentBufferSizeChars, long bufferSizeNeededChars)
        {
            if (currentBufferSizeChars < bufferSizeNeededChars)
            {
                Marshal.FreeHGlobal(buffer);

                return true;
            }
            return false;
        }

        // returns magic numbers from core, formerly the enum DataType in UnsafeNativeMethods.shared.cs
        internal static IntPtr RealmColType(Type columnType)
        {
            // ordered in decreasing likelihood of type
            if (columnType == typeof(string))
                return (IntPtr)2;
            if (columnType == typeof(int))
                return (IntPtr)0;
            if (columnType == typeof(float))
                return (IntPtr)9;
            if (columnType == typeof(double))
                return (IntPtr)10;
            if (columnType == typeof(DateTime))
                return (IntPtr)7;
            if (columnType == typeof(bool))
                return (IntPtr)1;
            /*
            TODO
                    Binary = 4,
                    Table = 5,
                    Mixed = 6,

            */
            throw new NotImplementedException();
        }

        #endregion  // helpers
    }
}
