using System;
using RealmIO;

using TableHandle = System.IntPtr;
using System.Collections.Generic;

namespace Interop.Providers
{
    public class CoreRow : ICoreRow
    {
        private TableHandle _tableHandle;
        public long RowIndex;

        public CoreRow(TableHandle tableHandle, long rowIndex)
        {
            this._tableHandle = tableHandle;
            this.RowIndex = rowIndex;
        }

        public T GetValue<T>(string propertyName)
        {
            if (typeof(T) != typeof(string))
                throw new Exception("Only strings supported at the moment");

            var columnIndex = 0;

            var value = UnsafeNativeMethods.table_get_string(_tableHandle, columnIndex, RowIndex);
            return (T)Convert.ChangeType(value, typeof(T));
        }

        public void SetValue<T>(string propertyName, T value)
        {
            if (typeof(T) != typeof(string))
                throw new Exception("Only strings supported at the moment");

            var columnIndex = 0;

            UnsafeNativeMethods.table_set_string(_tableHandle, columnIndex, RowIndex, value.ToString());
        }
    }

    public class CoreProvider : ICoreProvider
    {
        public class Table
        {
            public TableHandle TableHandle;
        }

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
            var columnIndex = UnsafeNativeMethods.table_add_column(tableHandle, UnsafeNativeMethods.DataType.String, columnName);
        }

        public ICoreRow AddEmptyRow(string tableName)
        {
            var tableHandle = _tables[tableName].TableHandle;
            var rowIndex = UnsafeNativeMethods.table_add_empty_row(tableHandle, 1); 
            return new CoreRow(tableHandle, rowIndex);
        }

        public ICoreQueryHandle CreateQuery(string tableName)
        {
            throw new NotImplementedException();
        }

        public void QueryEqual(ICoreQueryHandle queryHandle, string columnName, object value)
        {
            throw new NotImplementedException();
        }

        public System.Collections.IEnumerable ExecuteQuery(ICoreQueryHandle queryHandle, Type objectType)
        {
            throw new NotImplementedException();
        }
    }
}
