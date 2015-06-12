using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TightDbCSharp;

namespace RealmIO
{
    public class CoreProvider : ICoreProvider
    {
        private Dictionary<string, Table> _tables = new Dictionary<string, Table>();

        public bool HasTable(string tableName)
        {
            return _tables.ContainsKey(tableName);
        }

        public void AddTable(string tableName)
        {
            _tables[tableName] = new Table();
        }

        public void AddColumnToTable(string tableName, string columnName, Type columnType)
        {
            var table = _tables[tableName];

            if (columnType == typeof (string))
                table.AddStringColumn(columnName);
            else if (columnType == typeof (bool))
                table.AddBoolColumn(columnName);
            else
                throw new Exception("Columntype " + columnType.Name + " not supported");
        }

        public ICoreRow AddEmptyRow(string tableName)
        {
            var table = _tables[tableName];
            table.AddEmptyRow(1);
            return new CoreRow(table.Last());
            //return table.AddEmptyRow(1);
        }

        public T GetValue<T>(string tableName, long rowIndex, string propertyName)
        {
            var table = _tables[tableName];

            if (typeof (T) == typeof (string))
                return (T)(object)table.GetString(propertyName, rowIndex);
            else if (typeof (T) == typeof (bool))
                return (T) (object) table.GetBoolean(propertyName, rowIndex);
            else
                throw new Exception("Property type " + typeof (T).Name + " not supported");
        }

        public void SetValue<T>(string tableName, long rowIndex, string propertyName, T value)
        {
            var table = _tables[tableName];
            var columnIndex = table.GetColumnIndex(propertyName);

            if (typeof (T) == typeof (string))
                table.SetString(columnIndex, rowIndex, (string) (object) value);
            else if (typeof (T) == typeof (bool))
                table.SetBoolean(columnIndex, rowIndex, (bool) (object) value);
            else
                throw new Exception("Property type " + typeof (T).Name + " not supported");
        }

        public ICoreQueryHandle CreateQuery(string tableName)
        {
            var table = _tables[tableName];

            return new CoreQueryHandle() { Query = table.Where() };
        }

        public void QueryEqual(ICoreQueryHandle queryHandle, string columnName, object value)
        {
            var query = ((CoreQueryHandle)queryHandle).Query;
            query.Equal(columnName, (bool) value);
        }

        public IEnumerable ExecuteQuery(ICoreQueryHandle queryHandle, Type objectType)
        {
            var query = ((CoreQueryHandle) queryHandle).Query;
            var list = Activator.CreateInstance(typeof (List<>).MakeGenericType(objectType));
            var add = list.GetType().GetTypeInfo().GetDeclaredMethod("Add");

            foreach (var r in query)
            {
                var o = Activator.CreateInstance(objectType);
                ((RealmObject)o)._Manage(new CoreRow(r));
                add.Invoke(list, new [] { o });
                //yield return o;
            }
            return (IEnumerable)list;
        }
    }

    public class CoreQueryHandle : ICoreQueryHandle
    {
        public Query Query;
    }

    public class CoreRow : ICoreRow
    {
        private TableRow _tableRow;

        public CoreRow(TableRow tableRow)
        {
            this._tableRow = tableRow;
        }

        public T GetValue<T>(string propertyName)
        {
            if (typeof (T) == typeof (string))
                return (T) (object) _tableRow.GetString(propertyName);
            else if (typeof (T) == typeof (bool))
                return (T) (object) _tableRow.GetBoolean(propertyName);
            else
                throw new Exception("Property type " + typeof (T).Name + " not supported");
        }

        public void SetValue<T>(string propertyName, T value)
        {
            if (typeof (T) == typeof (string))
                _tableRow.SetString(propertyName, (string) (object)value);
            else if (typeof (T) == typeof (bool))
                _tableRow.SetBoolean(propertyName, (bool) (object) value);
            else
                throw new Exception("Property type " + typeof (T).Name + " not supported");
        }
    }
}
