using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RealmNet;
using RealmNet.Interop;

namespace Tests.TestHelpers
{
    public class CoreProviderStub : ICoreProvider
    {
        public class Table
        {
            public Dictionary<string, Type> Columns = new Dictionary<string, Type>();
            public List<Dictionary<string, object>> Rows = new List<Dictionary<string, object>>(); 
        }

        public Dictionary<string, Table> Tables = new Dictionary<string, Table>();
        public List<Query> Queries = new List<Query>();

        public ISharedGroupHandle CreateSharedGroup(string filename)
        {
            throw new NotImplementedException();
        }

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

        public long AddEmptyRow(string tableName)
        {
            Tables[tableName].Rows.Add(new Dictionary<string, object>());
            return Tables[tableName].Rows.Count - 1;
        }

        public T GetValue<T>(string tableName, string propertyName, long rowIndex)
        {
            return (T)Tables[tableName].Rows[(int)rowIndex][propertyName];
        }

        public void SetValue<T>(string tableName, string propertyName, long rowIndex, T value)
        {
            Tables[tableName].Rows[(int)rowIndex][propertyName] = value;
        }

        public IQueryHandle CreateQuery(string tableName)
        {
            var q = new Query {TableName = tableName};
            Queries.Add(q);
            return q;
        }

        public void QueryEqual(IQueryHandle queryHandle, string columnName, object value)
        {
            var q = (Query) queryHandle;
            q.Sequence.Add(new Query.SequenceElement { Name = "Equal", Field = columnName, Value = value });
        }

        public IEnumerable ExecuteQuery(IQueryHandle queryHandle, Type objectType)
        {
            var result = Activator.CreateInstance(typeof (List<>).MakeGenericType(objectType));
            return (IEnumerable)result;
            //Oreturn (IQueryable)Activator.CreateInstance(typeof(RealmQuery<>).MakeGenericType(elementType), new object[] { this, expression });
        }

        // Non-interface helpers:

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

        public class Query : IQueryHandle
        {
            public string TableName;

            public class SequenceElement
            {
                public string Name;
                public string Field;
                public object Value;
            }

            public List<SequenceElement> Sequence = new List<SequenceElement>();
        }
    }
}