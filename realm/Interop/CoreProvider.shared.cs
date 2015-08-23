using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using InteropShared;
using System.Runtime.InteropServices;

namespace RealmNet.Interop
{
    public class CoreProvider : ICoreProvider
    {

        #region helpers
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

        internal static IntPtr BoolToIntPtr(Boolean value)
        {
            return value ? (IntPtr)1 : (IntPtr)0;
        }

        internal static Boolean IntPtrToBool(IntPtr value)
        {
            return (IntPtr)1 == value;
        }


        private static IntPtr StrAllocateBuffer(out long currentBufferSizeChars, long bufferSizeNeededChars)
        {
            currentBufferSizeChars = bufferSizeNeededChars;
            return Marshal.AllocHGlobal((IntPtr)(bufferSizeNeededChars * sizeof(char)));
            //allocHGlobal instead of  AllocCoTaskMem because allcHGlobal allows lt 2 gig on 64 bit (not that .net supports that right now, but at least this allocation will work with lt 32 bit strings)   
        }

        private static string StrBufToStr(IntPtr buffer, int bufferSizeNeededChars)
        {
            string retStr = bufferSizeNeededChars > 0 ? Marshal.PtrToStringUni(buffer, bufferSizeNeededChars) : "";
            //return "" if the string is empty, otherwise copy data from the buffer
            Marshal.FreeHGlobal(buffer);
            return retStr;
        }
        private static Boolean StrBufferOverflow(IntPtr buffer, long currentBufferSizeChars, long bufferSizeNeededChars)
        {
            if (currentBufferSizeChars < bufferSizeNeededChars)
            {
                Marshal.FreeHGlobal(buffer);

                return true;
            }
            return false;
        }

        #endregion  // helpers


        public ISharedGroupHandle CreateSharedGroup(string filename)
        {
            return UnsafeNativeMethods.new_shared_group_file(filename, (IntPtr)filename.Length, (IntPtr)0, (IntPtr)0);
        }

        public bool HasTable(IGroupHandle groupHandle, string tableName)
        {
            var gh = groupHandle as GroupHandle;
            return UnsafeNativeMethods.group_has_table(gh, tableName, (IntPtr)tableName.Length) == (IntPtr)1;
        }

        private TableHandle GetTable(IGroupHandle groupHandle, string tableName)
        {
            var gh = groupHandle as GroupHandle;
            return gh.GetTable(tableName);
        }

        public void AddTable(IGroupHandle groupHandle, string tableName)
        {
            GetTable(groupHandle, tableName);
        }

        private IntPtr GetColumnIndex(TableHandle tableHandle, string columnName)
        {
            return UnsafeNativeMethods.table_get_column_index(tableHandle, columnName, (IntPtr)columnName.Length);
        }

        public void AddColumnToTable(IGroupHandle groupHandle, string tableName, string columnName, Type columnType)
        {
            var columnIndex = UnsafeNativeMethods.table_add_column(GetTable(groupHandle, tableName), RealmColType(columnType), columnName, (IntPtr)columnName.Length);
        }

        public long AddEmptyRow(IGroupHandle groupHandle, string tableName)
        {
            return (long)UnsafeNativeMethods.table_add_empty_row(GetTable(groupHandle, tableName), (IntPtr)1); 
        }

        public T GetValue<T>(IGroupHandle groupHandle, string tableName, string propertyName, long rowIndex)
        {
            var tableHandle = GetTable(groupHandle, tableName);
            var columnIndex = GetColumnIndex(tableHandle, propertyName);

            if (typeof(T) == typeof(string))
            {
                long bufferSizeNeededChars = 16;
                IntPtr buffer;
                long currentBufferSizeChars;

                do
                {
                    buffer = StrAllocateBuffer(out currentBufferSizeChars, bufferSizeNeededChars);
                    bufferSizeNeededChars = (long)UnsafeNativeMethods.table_get_string(tableHandle, columnIndex, (IntPtr)rowIndex, buffer,
                            (IntPtr)currentBufferSizeChars);

                } while (StrBufferOverflow(buffer, currentBufferSizeChars, bufferSizeNeededChars));
                return (T)Convert.ChangeType(StrBufToStr(buffer, (int)bufferSizeNeededChars), typeof(T));
            }
            else if (typeof(T) == typeof(bool))
            {
                var value = IntPtrToBool( UnsafeNativeMethods.table_get_bool(tableHandle, columnIndex, (IntPtr)rowIndex) );
                return (T)Convert.ChangeType(value, typeof(T));
            }
            else if (typeof(T) == typeof(int))  // System.Int32 regardless of bitness
            {
                var value = UnsafeNativeMethods.table_get_int64(tableHandle, columnIndex, (IntPtr)rowIndex);
                return (T)Convert.ChangeType(value, typeof(T));
            }
            else if (typeof(T) == typeof(Int64)) 
            {
                var value = UnsafeNativeMethods.table_get_int64(tableHandle, columnIndex, (IntPtr)rowIndex);
                return (T)Convert.ChangeType(value, typeof(T));
            }
            else
                throw new Exception ("Unsupported type " + typeof(T).Name);
        }

        public void SetValue<T>(IGroupHandle groupHandle, string tableName, string propertyName, long rowIndex, T value)
        {
            var tableHandle = GetTable(groupHandle, tableName);
            var columnIndex = GetColumnIndex(tableHandle, propertyName);

            if (typeof(T) == typeof(string))
            {
                var str = value.ToString();
                UnsafeNativeMethods.table_set_string(tableHandle, columnIndex, (IntPtr)rowIndex, str, (IntPtr)str.Length);
            }
            else if (typeof(T) == typeof(bool))
            {
                var marshalledValue = BoolToIntPtr((bool)Convert.ChangeType(value, typeof(bool)));
                UnsafeNativeMethods.table_set_bool(tableHandle, columnIndex, (IntPtr)rowIndex, marshalledValue);
            }
            else if (typeof(T) == typeof(int))  // System.Int32 regardless of bitness
            {
                Int64 marshalledValue = Convert.ToInt64(value);
                UnsafeNativeMethods.table_set_int64(tableHandle, columnIndex, (IntPtr)rowIndex, marshalledValue);
            }
            else if (typeof(T) == typeof(Int64))
            {
                Int64 marshalledValue = Convert.ToInt64(value);
                UnsafeNativeMethods.table_set_int64(tableHandle, columnIndex, (IntPtr)rowIndex, marshalledValue);
            }
            else
                throw new Exception ("Unsupported type " + typeof(T).Name);
        }

        #region Queries
        public IQueryHandle CreateQuery(IGroupHandle groupHandle, string tableName)
        {
            var tableHandle = GetTable(groupHandle, tableName);
            var queryHandle = tableHandle.TableWhere();

            //At this point sh is invalid due to its handle being uninitialized, but the root is set correctly
            //a finalize at this point will not leak anything and the handle will not do anything

            //now, set the TableView handle...
            RuntimeHelpers.PrepareConstrainedRegions();//the following finally will run with no out-of-band exceptions
            try
            { }
            finally
            {
                queryHandle.SetHandle(UnsafeNativeMethods.table_where(tableHandle));
            }//at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly
            return queryHandle;
        }

        public void AddQueryEqual(IQueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = UnsafeNativeMethods.query_get_column_index((QueryHandle)queryHandle, columnName, (IntPtr)columnName.Length);

            var valueType = value.GetType();
            if (value.GetType() == typeof(string))
            {
                string valueStr = (string)value;
                UnsafeNativeMethods.query_string_equal((QueryHandle)queryHandle, columnIndex, valueStr, (IntPtr)valueStr.Length);
            }
            else if (valueType == typeof(bool))
                UnsafeNativeMethods.query_bool_equal((QueryHandle)queryHandle, columnIndex, BoolToIntPtr((bool)value));
            else if (valueType == typeof(int))
                UnsafeNativeMethods.query_int_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((int)value));
            else if (valueType == typeof(float))
                ;// see issue 68 UnsafeNativeMethods.query_float_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((float)value));
            else if (valueType == typeof(double))
                ;// see issue 68 UnsafeNativeMethods.query_double_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((double)value));
            else
                throw new NotImplementedException();
        }

        public void AddQueryNotEqual(IQueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = UnsafeNativeMethods.query_get_column_index((QueryHandle)queryHandle, columnName, (IntPtr)columnName.Length);

            var valueType = value.GetType();
            if (value.GetType() == typeof(string))
            {
                string valueStr = (string)value;
                UnsafeNativeMethods.query_string_not_equal((QueryHandle)queryHandle, columnIndex, valueStr, (IntPtr)valueStr.Length);
            }
            else if (valueType == typeof(bool))
                UnsafeNativeMethods.query_bool_not_equal((QueryHandle)queryHandle, columnIndex, BoolToIntPtr((bool)value));
            else if (valueType == typeof(int))
                UnsafeNativeMethods.query_int_not_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((int)value));
            else if (valueType == typeof(float))
                ;// see issue 68 UnsafeNativeMethods.query_float_not_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((float)value));
            else if (valueType == typeof(double))
                ;// see issue 68 UnsafeNativeMethods.query_double_not_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((double)value));
            else
                throw new NotImplementedException();
        }

        public void AddQueryLessThan(IQueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = UnsafeNativeMethods.query_get_column_index((QueryHandle)queryHandle, columnName, (IntPtr)columnName.Length);

            var valueType = value.GetType();
            if (valueType == typeof(int))
                UnsafeNativeMethods.query_int_less((QueryHandle)queryHandle, columnIndex, (IntPtr)((int)value));
            else if (valueType == typeof(float))
                ;// see issue 68 UnsafeNativeMethods.query_float_less((QueryHandle)queryHandle, columnIndex, (IntPtr)((float)value));
            else if (valueType == typeof(double))
                ;// see issue 68 UnsafeNativeMethods.query_double_less((QueryHandle)queryHandle, columnIndex, (IntPtr)((double)value));
            else if (valueType == typeof(string) || valueType == typeof(bool))
                throw new Exception("Unsupported type " + valueType.Name);
            else
                throw new NotImplementedException();
        }

        public void AddQueryLessThanOrEqual(IQueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = UnsafeNativeMethods.query_get_column_index((QueryHandle)queryHandle, columnName, (IntPtr)columnName.Length);

            var valueType = value.GetType();
            if (valueType == typeof(int))
                UnsafeNativeMethods.query_int_less_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((int)value));
            else if (valueType == typeof(float))
                ;// see issue 68 UnsafeNativeMethods.query_float_less_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((float)value));
            else if (valueType == typeof(double))
                ;// see issue 68 UnsafeNativeMethods.query_double_less_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((double)value));
            else if (valueType == typeof(string) || valueType == typeof(bool))
                throw new Exception("Unsupported type " + valueType.Name);
            else
                throw new NotImplementedException();
        }

        public void AddQueryGreaterThan(IQueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = UnsafeNativeMethods.query_get_column_index((QueryHandle)queryHandle, columnName, (IntPtr)columnName.Length);

            var valueType = value.GetType();
            if (valueType == typeof(int))
                UnsafeNativeMethods.query_int_greater((QueryHandle)queryHandle, columnIndex, (IntPtr)((int)value));
            else if (valueType == typeof(float))
                ;// see issue 68 UnsafeNativeMethods.query_float_greater((QueryHandle)queryHandle, columnIndex, (IntPtr)((float)value));
            else if (valueType == typeof(double))
                ;// see issue 68 UnsafeNativeMethods.query_double_greater((QueryHandle)queryHandle, columnIndex, (IntPtr)((double)value));
            else if (valueType == typeof(string) || valueType == typeof(bool))
                throw new Exception("Unsupported type " + valueType.Name);
            else
                throw new NotImplementedException();
        }

        public void AddQueryGreaterThanOrEqual(IQueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = UnsafeNativeMethods.query_get_column_index((QueryHandle)queryHandle, columnName, (IntPtr)columnName.Length);

            var valueType = value.GetType();
            if (valueType == typeof(int))
                UnsafeNativeMethods.query_int_greater_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((int)value));
            else if (valueType == typeof(float))
                ;// see issue 68 UnsafeNativeMethods.query_float_greater_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((float)value));
            else if (valueType == typeof(double))
                ;// see issue 68 UnsafeNativeMethods.query_double_greater_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((double)value));
            else if (valueType == typeof(string) || valueType == typeof(bool))
                throw new Exception("Unsupported type " + valueType.Name);
            else
                throw new NotImplementedException();
        }

        public void AddQueryGroupBegin(IQueryHandle queryHandle)
        {
            UnsafeNativeMethods.query_group_begin((QueryHandle)queryHandle);
        }

        public void AddQueryGroupEnd(IQueryHandle queryHandle)
        {
            UnsafeNativeMethods.query_group_end((QueryHandle)queryHandle);
        }

        public void AddQueryAnd(IQueryHandle queryHandle)
        {
           // does nothing as subsequent groups automatically ANDed
        }

        public void AddQueryOr(IQueryHandle queryHandle)
        {
            UnsafeNativeMethods.query_or((QueryHandle)queryHandle);
        }


        public IEnumerable<long> ExecuteQuery(IQueryHandle queryHandle, Type objectType)
        {
            long nextRowIndex = 0;
            while (nextRowIndex != -1)
            {
                long rowIndex = (long)UnsafeNativeMethods.query_find((QueryHandle)queryHandle, (IntPtr)nextRowIndex);
                if (rowIndex != -1)
                {
                    nextRowIndex = rowIndex + 1;
                    yield return rowIndex;
                }
                else
                {
                    yield break;
                }
            }
        }
        #endregion  // Queries

        public IGroupHandle NewGroup()
        {
            throw new NotImplementedException();
        }

        public IGroupHandle NewGroupFromFile(string path, GroupOpenMode openMode)
        {
            throw new NotImplementedException();
        }

        public void GroupCommit(IGroupHandle groupHandle)
        {
            throw new NotImplementedException();
        }

        public bool GroupIsEmpty(IGroupHandle groupHandle)
        {
            throw new NotImplementedException();
        }

        public long GroupSize(IGroupHandle groupHandle)
        {
            throw new NotImplementedException();
        }
    }
}
