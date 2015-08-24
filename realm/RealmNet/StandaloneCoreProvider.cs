using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RealmNet.Interop;
using RealmNet;
using InteropShared;

namespace RealmNet

{

    /// <summary>
    /// Backing for "standalone objects" at least before they are assigned to a Realm
    /// </summary>
    public class StandaloneCoreProvider : ICoreProvider
    {
        private class StandaloneBackingTable
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
        static StandaloneCoreProvider sharedStandalones;

        private Dictionary<string, StandaloneBackingTable> _tables = new Dictionary<string, StandaloneBackingTable>();

        // TODO decide if this should be using thread-local instances
        public static StandaloneCoreProvider GetInstance()
        {
            if (sharedStandalones == null)
                sharedStandalones = new StandaloneCoreProvider();
            return sharedStandalones;
        }


        public ISharedGroupHandle CreateSharedGroup(string filename)
        {
            throw new NotImplementedException();
        }

        public bool HasTable(IGroupHandle groupHandle, string tableName)
        {
            bool ret = _tables.ContainsKey(tableName);
            return ret;
        }

        public void AddTable(IGroupHandle groupHandle, string tableName)
        {
            _tables.Add(tableName, new StandaloneBackingTable());
        }

        public void AddColumnToTable(IGroupHandle groupHandle, string tableName, string columnName, Type columnType)
        {
            _tables[tableName].AddColumn(columnName, columnType);
        }

        public long AddEmptyRow(IGroupHandle groupHandle, string tableName)
        {
            var table = _tables[tableName];
            table.Rows.Add( new object[table.Columns.Count] );
            var numRows = table.Rows.Count;
            return numRows - 1;  // index of added row
        }

        public T GetValue<T>(IGroupHandle groupHandle, string tableName, string propertyName, long rowIndex)
        {
            var table = _tables[tableName];
            Type expectedType = table.Columns[propertyName];
            int colIndex = table.ColumnIndexes[propertyName];
            Debug.Assert(expectedType == typeof(T));

            int index = (int)rowIndex;
            var row = _tables[tableName].Rows[index];
            T ret = (T)row[colIndex];
            return ret;
        }

        public void SetValue<T>(IGroupHandle groupHandle, string tableName, string propertyName, long rowIndex, T value)
        {
            var table = _tables[tableName];
            Type expectedType = table.Columns[propertyName];
            int colIndex = table.ColumnIndexes[propertyName];
            Debug.Assert(expectedType == typeof(T));

            int index = (int)rowIndex;
            var row = _tables[tableName].Rows[index];
            row[colIndex] = value;
        }

        public void RemoveRow(IGroupHandle groupHandle, string tableName, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public IQueryHandle CreateQuery(IGroupHandle groupHandle, string tableName)
        {
            return null;
        }

        public void AddQueryEqual(IQueryHandle queryHandle, string columnName, object value)
        {
            throw new NotImplementedException();
        }

        public void AddQueryNotEqual(IQueryHandle queryHandle, string columnName, object value)
        {
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

        public void AddQueryGroupBegin(IQueryHandle queryHandle)
        {
            throw new NotImplementedException();
        }

        public void AddQueryGroupEnd(IQueryHandle queryHandle)
        {
            throw new NotImplementedException();
        }

        public void AddQueryAnd(IQueryHandle queryHandle)
        {
            throw new NotImplementedException();
        }

        public void AddQueryOr(IQueryHandle queryHandle)
        {
            throw new NotImplementedException();
        }


        public IEnumerable<long> ExecuteQuery(IQueryHandle queryHandle, Type objectType)
        {
            throw new NotImplementedException();
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

