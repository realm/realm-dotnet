using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
//using System.Text;
using RealmNet.Interop;
using RealmNet;

namespace InteropShared
{
    public class MockTable
    {
        public Dictionary<string, Type> Columns = new Dictionary<string, Type>();
        public Dictionary<string, int> ColumnIndexes = null;
        public List<object[]> Rows = new List<object[]>();

        public void AddColumn(string columnName, Type columnType)
        {
            Columns.Add(columnName, columnType);
            // recalc column indexes
            int index = 0;
            ColumnIndexes = new Dictionary<string, int>();
            foreach (var key in Columns.Keys)
            {
                ColumnIndexes[key] = index++;
            }
        }
    }

    public class MockQuery : IQueryHandle
    {
        public MockQuery(string tableName)
        {
            queryTable = tableName;
        }

        private readonly string queryTable;
        public void Dispose()
        {
        }

        public bool IsClosed => false;
    }

    public class MockCoreProvider : ICoreProvider
    {
        private Dictionary<string, MockTable> _tables = new Dictionary<string, MockTable>();
        private Action<String> notifyOnCall;

        public MockCoreProvider (Action<String> externalNotifier)
        {
            notifyOnCall = externalNotifier;
        }

        public MockCoreProvider () : this ((ignored) => {})  // delegate to construct with 
        {}


        public ISharedGroupHandle CreateSharedGroup(string filename)
        {
            throw new NotImplementedException();
        }

        public bool HasTable(string tableName)
        {
            bool ret = _tables.ContainsKey(tableName);
            notifyOnCall ($"HasTable({tableName}) return {ret}");
            return ret;
        }

        public void AddTable(string tableName)
        {
            notifyOnCall ($"AddTable({tableName})");
            _tables.Add(tableName, new MockTable());
        }

        public void AddColumnToTable(string tableName, string columnName, Type columnType)
        {
            notifyOnCall ($"AddColumnToTable({tableName}, col={columnName}, type={columnType})");
            _tables[tableName].AddColumn(columnName, columnType);
        }

        public long AddEmptyRow(string tableName)
        {
            var table = _tables[tableName];
            table.Rows.Add( new object[table.Columns.Count] );
            var numRows = table.Rows.Count;
            notifyOnCall($"AddEmptyRow({tableName}) now has {numRows} rows");
            return numRows;
        }

        public T GetValue<T>(string tableName, string propertyName, long rowIndex)
        {
            var table = _tables[tableName];
            Type expectedType = table.Columns[propertyName];
            int colIndex = table.ColumnIndexes[propertyName];
            Debug.Assert(expectedType == typeof(T));

            int index = (int)rowIndex;
            var row = _tables[tableName].Rows[index];
            T ret = (T)row[colIndex];
            notifyOnCall ($"GetValue({tableName}, prop={propertyName}, row={rowIndex}) returns {ret}");
            return ret;
        }

        public void SetValue<T>(string tableName, string propertyName, long rowIndex, T value)
        {
            notifyOnCall ($"SetValue({tableName}, prop={propertyName}, row={rowIndex}, val={value})");
            var table = _tables[tableName];
            Type expectedType = table.Columns[propertyName];
            int colIndex = table.ColumnIndexes[propertyName];
            Debug.Assert(expectedType == typeof(T));

            int index = (int)rowIndex;
            var row = _tables[tableName].Rows[index];
            row[colIndex] = value;
        }

        public IQueryHandle CreateQuery(string tableName)
        {
            notifyOnCall ($"CreateQuery({tableName})");
            return new MockQuery(tableName);
        }

        public void QueryEqual(IQueryHandle queryHandle, string columnName, object value)
        {
            notifyOnCall ($"QueryEqual(col={columnName}, val={value})");
        }

        public System.Collections.IEnumerable ExecuteQuery(IQueryHandle queryHandle, Type objectType)
        {
            IEnumerable ret = default(Array);
//            notifyOnCall ($"ExecuteQuery found {ret.Count()})");
            notifyOnCall ($"ExecuteQuery");
            return ret;
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

