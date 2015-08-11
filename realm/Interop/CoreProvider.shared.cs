using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using InteropShared;
using RealmNet;
using RealmNet.Interop;

namespace RealmNet.Interop
{
    public class CoreProvider : ICoreProvider
    {
        public ISharedGroupHandle CreateSharedGroup(string filename)
        {
            return UnsafeNativeMethods.new_shared_group_file_defaults(filename);
        }

        public bool HasTable(IGroupHandle groupHandle, string tableName)
        {
            var gh = groupHandle as GroupHandle;
            return UnsafeNativeMethods.group_has_table(gh, tableName);
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

        private long GetColumnIndex(TableHandle tableHandle, string columnName)
        {
            return UnsafeNativeMethods.table_get_column_index(tableHandle, columnName);
        }

        public void AddColumnToTable(IGroupHandle groupHandle, string tableName, string columnName, Type columnType)
        {
            var tableHandle = GetTable(groupHandle, tableName);
            DataType dataType = DataType.Int;
            if (columnType == typeof(string))
                dataType = DataType.String;
            else if (columnType == typeof(bool))
                dataType = DataType.Bool; 
                
            var columnIndex = UnsafeNativeMethods.table_add_column(tableHandle, dataType, columnName);
        }

        public long AddEmptyRow(IGroupHandle groupHandle, string tableName)
        {
            var tableHandle = GetTable(groupHandle, tableName);
            var rowIndex = UnsafeNativeMethods.table_add_empty_row(tableHandle, 1); 
            return rowIndex;
        }

        public T GetValue<T>(IGroupHandle groupHandle, string tableName, string propertyName, long rowIndex)
        {
            var tableHandle = GetTable(groupHandle, tableName);
            var columnIndex = GetColumnIndex(tableHandle, propertyName);

            if (typeof(T) == typeof(string))
            {
                var value = UnsafeNativeMethods.table_get_string(tableHandle, columnIndex, rowIndex);
                return (T)Convert.ChangeType(value, typeof(T));
            }
            else if (typeof(T) == typeof(bool))
            {
                var value = UnsafeNativeMethods.table_get_bool(tableHandle, columnIndex, rowIndex);
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
                UnsafeNativeMethods.table_set_string(tableHandle, columnIndex, rowIndex, value.ToString());
            }
            else if (typeof(T) == typeof(bool))
            {
                UnsafeNativeMethods.table_set_bool(tableHandle, columnIndex, rowIndex, (bool)Convert.ChangeType(value, typeof(bool)));
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
            var columnIndex = UnsafeNativeMethods.query_get_column_index((QueryHandle)queryHandle, columnName);

            var valueType = value.GetType();
            if (value.GetType() == typeof(string))
                UnsafeNativeMethods.query_string_equal((QueryHandle)queryHandle, columnIndex, (string)value);
            else if (valueType == typeof(bool))
                UnsafeNativeMethods.query_bool_equal((QueryHandle)queryHandle, columnIndex, (bool)value);
            else if (valueType == typeof(int))
                UnsafeNativeMethods.query_int_equal((QueryHandle)queryHandle, columnIndex, (int)value);
            else if (valueType == typeof(float))
                UnsafeNativeMethods.query_float_equal((QueryHandle)queryHandle, columnIndex, (float)value);
            else if (valueType == typeof(double))
                UnsafeNativeMethods.query_double_equal((QueryHandle)queryHandle, columnIndex, (double)value);
            else
                throw new NotImplementedException();
        }

        public void AddQueryNotEqual(IQueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = UnsafeNativeMethods.query_get_column_index((QueryHandle)queryHandle, columnName);

            var valueType = value.GetType();
            if (value.GetType() == typeof(string))
                UnsafeNativeMethods.query_string_not_equal((QueryHandle)queryHandle, columnIndex, (string)value);
            else if (valueType == typeof(bool))
                UnsafeNativeMethods.query_bool_not_equal((QueryHandle)queryHandle, columnIndex, (bool)value);
            else if (valueType == typeof(int))
                UnsafeNativeMethods.query_int_not_equal((QueryHandle)queryHandle, columnIndex, (int)value);
            else if (valueType == typeof(float))
                UnsafeNativeMethods.query_float_not_equal((QueryHandle)queryHandle, columnIndex, (float)value);
            else if (valueType == typeof(double))
                UnsafeNativeMethods.query_double_not_equal((QueryHandle)queryHandle, columnIndex, (double)value);
            else
                throw new NotImplementedException();
        }

        public void AddQueryLessThan(IQueryHandle queryHandle, string columnName, object value)
        {
            throw new NotImplementedException();
        }

        public void AddQueryLessThanOrEqual(IQueryHandle queryHandle, string columnName, object value)
        {
            throw new NotImplementedException();
        }

        public void AddQueryGreaterThan(IQueryHandle queryHandle, string columnName, object value)
        {
            throw new NotImplementedException();
        }

        public void AddQueryGreaterThanOrEqual(IQueryHandle queryHandle, string columnName, object value)
        {
            throw new NotImplementedException();
        }


        public IEnumerable<long> ExecuteQuery(IQueryHandle queryHandle, Type objectType)
        {
            long nextRowIndex = 0;
            while (nextRowIndex != -1)
            {
                var rowIndex = UnsafeNativeMethods.query_find((QueryHandle)queryHandle, nextRowIndex);
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
