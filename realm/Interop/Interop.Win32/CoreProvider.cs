using System;
using RealmNet;
using System.Collections.Generic;

using System.Collections;
using RealmNet.Interop;

namespace Interop.Providers
{
    public class Table
    {
        public TableHandle TableHandle;
        public Dictionary<string, long> Columns = new Dictionary<string, long>();
    }

    internal class CoreQueryHandle : ICoreQueryHandle
    {
        public QueryHandle QueryHandle;
        public Table Table;
    }

    public class CoreProvider : ICoreProvider
    {
        private Dictionary<string, Table> _tables = new Dictionary<string, Table>();

        public bool HasTable(string tableName)
        {
            return _tables.ContainsKey(tableName);
        }

        public void AddTable(string tableName)
        {
            var tableHandle = UnsafeNativeMethods.new_table();
            _tables[tableName] = new Table() { TableHandle = tableHandle };
        }

        public void AddColumnToTable(string tableName, string columnName, Type columnType)
        {
            var tableHandle = _tables[tableName].TableHandle;
            UnsafeNativeMethods.DataType dataType = UnsafeNativeMethods.DataType.Int;
            if (columnType == typeof(string))
                dataType = UnsafeNativeMethods.DataType.String;
            else if (columnType == typeof(bool))
                dataType = UnsafeNativeMethods.DataType.Bool; 
                
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
        public ICoreQueryHandle CreateQuery(string tableName)
        {
            var table = _tables[tableName];
            var tableHandle = table.TableHandle;
            return new CoreQueryHandle() { /*QueryHandle = UnsafeNativeMethods.table_where(tableHandle),*/ Table = table };
        }

        public void QueryEqual(ICoreQueryHandle queryHandle, string columnName, object value)
        {
            var coreQuery = queryHandle as CoreQueryHandle;
            var columnIndex = coreQuery.Table.Columns[columnName];
            if (value.GetType() == typeof(bool))
                UnsafeNativeMethods.query_bool_equal(coreQuery.QueryHandle, columnIndex, (bool)value);
            else if (value.GetType() == typeof(string))
                UnsafeNativeMethods.query_string_equal(coreQuery.QueryHandle, columnIndex, (string)value);
        }

        public System.Collections.IEnumerable ExecuteQuery(ICoreQueryHandle queryHandle, Type objectType)
        {
            var coreQuery = queryHandle as CoreQueryHandle;
            var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(objectType));
            var add = list.GetType().GetMethod("Add");

            long nextRowIndex = 0;
            while (nextRowIndex != -1)
            {
                var rowIndex = UnsafeNativeMethods.query_find(coreQuery.QueryHandle, nextRowIndex);
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
    }
}
