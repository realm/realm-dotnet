using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tests
{
    public class StubConnectionProvider : RealmIO.ICoreProvider
    {
        public class Table
        {
            public Dictionary<string, Type> Columns = new Dictionary<string, Type>();
            public List<Dictionary<string, object>> Rows = new List<Dictionary<string, object>>(); 
        }

        public Dictionary<string, Table> Tables = new Dictionary<string, Table>();
        
        public bool HasTable(string tableName)
        {
            return Tables.ContainsKey(tableName);
        }

        public void AddTable(string tableName)
        {
            Tables[tableName] = new Table();
        }

        public void AddColumnToTable(string tableName, string columnName, Type columnType)
        {
            Tables[tableName].Columns[columnName] = columnType;
        }

        public int InsertEmptyRow(string tableName)
        {
            Tables[tableName].Rows.Add(new Dictionary<string, object>());
            return Tables[tableName].Rows.Count - 1;
        }

        public T GetValue<T>(string tableName, int rowIndex, string propertyName)
        {
            return (T)Tables[tableName].Rows[rowIndex][propertyName];
        }

        public void SetValue<T>(string tableName, int rowIndex, string propertyName, T value)
        {
            Tables[tableName].Rows[rowIndex][propertyName] = value;
        }
    }
}
