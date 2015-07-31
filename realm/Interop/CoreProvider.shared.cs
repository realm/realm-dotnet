using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using InteropShared;
using RealmNet;
using RealmNet.Interop;

namespace RealmNet.Interop
{
    public class Table_
    {
        public TableHandle TableHandle;
        public Dictionary<string, long> Columns = new Dictionary<string, long>();
    }

    public class CoreProvider : ICoreProvider
    {
        private Dictionary<string, Table_> _tables = new Dictionary<string, Table_>();

        public ISharedGroupHandle CreateSharedGroup(string filename)
        {
            return UnsafeNativeMethods.new_shared_group_file_defaults(filename);
        }

        public bool HasTable(IGroupHandle groupHandle, string tableName)
        {
            return _tables.ContainsKey(tableName);
        }

        public void AddTable(IGroupHandle groupHandle, string tableName)
        {
            var gh = groupHandle as GroupHandle;
            var tableHandle = gh.GetTable(tableName);
            _tables[tableName] = new Table_ { TableHandle = tableHandle };
        }

        public void AddColumnToTable(string tableName, string columnName, Type columnType)
        {
            var tableHandle = _tables[tableName].TableHandle;
            DataType dataType = DataType.Int;
            if (columnType == typeof(string))
                dataType = DataType.String;
            else if (columnType == typeof(bool))
                dataType = DataType.Bool; 
                
            var columnIndex = UnsafeNativeMethods.table_add_column(tableHandle, dataType, columnName);
            _tables[tableName].Columns[columnName] = columnIndex;
        }

        public long AddEmptyRow(string tableName)
        {
            var tableHandle = _tables[tableName].TableHandle;
            var rowIndex = UnsafeNativeMethods.table_add_empty_row(tableHandle, 1); 
            return rowIndex;
        }

        public T GetValue<T>(string tableName, string propertyName, long rowIndex)
        {
            var table = _tables[tableName];
            var columnIndex = table.Columns[propertyName];

            if (typeof(T) == typeof(string))
            {
                var value = UnsafeNativeMethods.table_get_string(table.TableHandle, columnIndex, rowIndex);
                return (T)Convert.ChangeType(value, typeof(T));
            }
            else if (typeof(T) == typeof(bool))
            {
                var value = UnsafeNativeMethods.table_get_bool(table.TableHandle, columnIndex, rowIndex);
                return (T)Convert.ChangeType(value, typeof(T));
            }
            else
                throw new Exception ("Unsupported type " + typeof(T).Name);
        }

        public void SetValue<T>(string tableName, string propertyName, long rowIndex, T value)
        {
            var table = _tables[tableName];
            var columnIndex = table.Columns[propertyName];

            if (typeof(T) == typeof(string))
            {
                UnsafeNativeMethods.table_set_string(table.TableHandle, columnIndex, rowIndex, value.ToString());
            }
            else if (typeof(T) == typeof(bool))
            {
                UnsafeNativeMethods.table_set_bool(table.TableHandle, columnIndex, rowIndex, (bool)Convert.ChangeType(value, typeof(bool)));
            }
            else
                throw new Exception ("Unsupported type " + typeof(T).Name);
        }

        public IQueryHandle CreateQuery(string tableName)
        {
            var table = _tables[tableName];
            var tableHandle = table.TableHandle;
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

        public void QueryEqual(IQueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = UnsafeNativeMethods.query_get_column_index((QueryHandle)queryHandle, columnName);

            if (value.GetType() == typeof(bool))
                UnsafeNativeMethods.query_bool_equal((QueryHandle)queryHandle, columnIndex, (bool)value);
            else if (value.GetType() == typeof(string))
                UnsafeNativeMethods.query_string_equal((QueryHandle)queryHandle, columnIndex, (string)value);
        }

        public IEnumerable ExecuteQuery(IQueryHandle queryHandle, Type objectType)
        {
            var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(objectType));
            var add = list.GetType().GetMethod("Add");

            long nextRowIndex = 0;
            while (nextRowIndex != -1)
            {
                var rowIndex = UnsafeNativeMethods.query_find((QueryHandle)queryHandle, nextRowIndex);
                if (rowIndex != -1)
                {
                    var o = Activator.CreateInstance(objectType);
                    ((RealmObject)o)._Manage(this, rowIndex);
                    add.Invoke(list, new [] { o });

                    nextRowIndex = rowIndex + 1;
                }
                else
                    nextRowIndex = -1;
            }

            return (IEnumerable)list;
        }

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
