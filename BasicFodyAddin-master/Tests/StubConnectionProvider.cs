using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Tests
{
    public class StubCoreProvider : RealmIO.ICoreProvider
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

        public void AddBulk(string tableName, dynamic[] data)
        {
            if (!Tables.ContainsKey(tableName))
                Tables[tableName] = new Table();

            var table = Tables[tableName];

            foreach(var inputRow in data)
            {
                var row = new Dictionary<string, object>();

                foreach(var prop in inputRow.GetType().GetProperties())
                {
                    string propName = prop.Name;
                    object propValue = prop.GetValue(inputRow);
                    Type propType = prop.PropertyType;

                    //Debug.WriteLine(propType.Name + " " + propName + " = " + propValue);

                    if (!table.Columns.ContainsKey(propName))
                        table.Columns[propName] = propType;

                    row[propName] = propValue;
                }
                table.Rows.Add(row);
            }
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
