﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RealmNet
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
        public bool IsInvalid => false;
    }


    public class MockCoreProvider : ICoreProvider
    {
        private class MockTransaction : IGroupHandle
        {
            private bool _isClosed = false;

            public void Dispose()
            {
                _isClosed = true;
            }

            public bool IsClosed { get { return _isClosed; } }
            public bool IsInvalid { get { return false; } }
        }


        private class MockSharedGroupHandle : ISharedGroupHandle
        {
            public bool IsClosed { get; }
            public bool IsInvalid { get; }
            public IGroupHandle StartTransaction(TransactionState read)
            {
                State = read;
                return new MockTransaction();
            }

            public void SharedGroupCommit()
            {
                State = TransactionState.Ready;
            }

            public void SharedGroupRollback()
            {
                State = TransactionState.Ready;
            }

            public void SharedGroupEndRead()
            {
            }

            public TransactionState State { get; private set; }
            public void Dispose()
            {
                
            }
        }



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
            return new MockSharedGroupHandle();
        }

        public bool HasTable(IGroupHandle groupHandle, string tableName)
        {
            bool ret = _tables.ContainsKey(tableName);
            notifyOnCall ($"HasTable({tableName}) return {ret}");
            return ret;
        }

        public void AddTable(IGroupHandle groupHandle, string tableName)
        {
            notifyOnCall ($"AddTable({tableName})");
            _tables.Add(tableName, new MockTable());
        }

        public void AddColumnToTable(IGroupHandle groupHandle, string tableName, string columnName, Type columnType)
        {
            notifyOnCall ($"AddColumnToTable({tableName}, col={columnName}, type={columnType})");
            _tables[tableName].AddColumn(columnName, columnType);
        }

        public IRowHandle AddEmptyRow(IGroupHandle groupHandle, string tableName)
        {
            var table = _tables[tableName];
            table.Rows.Add( new object[table.Columns.Count] );
            var numRows = table.Rows.Count;
            notifyOnCall($"AddEmptyRow({tableName}) now has {numRows} rows");
            return new FakeRowHandle { RowIndex = numRows - 1 };  // index of added row
        }

        public void RemoveRow(IGroupHandle groupHandle, string tableName, IRowHandle rowHandle)
        {
            throw new NotImplementedException();
        }

        public T GetValue<T>(IGroupHandle groupHandle, string tableName, string propertyName, IRowHandle rowHandle)
        {
            var table = _tables[tableName];
            var expectedType = table.Columns[propertyName];
            var colIndex = table.ColumnIndexes[propertyName];
            Debug.Assert(expectedType == typeof(T));

            var index = (int)rowHandle.RowIndex;
            var row = table.Rows[index];
            var ret = (T)row[colIndex];
            notifyOnCall ($"GetValue({tableName}, prop={propertyName}, row={index}) returns {ret}");
            return ret;
        }

        public void SetValue<T>(IGroupHandle groupHandle, string tableName, string propertyName, IRowHandle rowHandle, T value)
        {
            var index = (int)rowHandle.RowIndex;

            notifyOnCall ($"SetValue({tableName}, prop={propertyName}, row={index}, val={value})");
            var table = _tables[tableName];
            var expectedType = table.Columns[propertyName];
            var colIndex = table.ColumnIndexes[propertyName];
            Debug.Assert(expectedType == typeof(T));

            var row = table.Rows[index];
            row[colIndex] = value;
        }

        public IList<T> GetListValue<T>(IGroupHandle groupHandle, string tableName, string propertyName, IRowHandle rowHandle)
        {
            throw new NotImplementedException();
        }

        public void SetListValue<T>(IGroupHandle groupHandle, string tableName, string propertyName, IRowHandle rowHandle, IList<T> value)
        {
            throw new NotImplementedException();
        }

        public IQueryHandle CreateQuery(IGroupHandle groupHandle, string tableName)
        {
            notifyOnCall ($"CreateQuery({tableName})");
            return new MockQuery(tableName);
        }

        public void AddQueryEqual(IQueryHandle queryHandle, string columnName, object value)
        {
            notifyOnCall ($"AddQueryEqual(col={columnName}, val={value})");
        }

        public void AddQueryNotEqual(IQueryHandle queryHandle, string columnName, object value)
        {
            notifyOnCall($"AddQueryNotEqual(col={columnName}, val={value})");
        }

        public void AddQueryLessThan(IQueryHandle queryHandle, string columnName, object value)
        {
            notifyOnCall($"AddQueryLessThan(col={columnName}, val={value})");
        }

        public void AddQueryLessThanOrEqual(IQueryHandle queryHandle, string columnName, object value)
        {
            notifyOnCall($"AddQueryLessThanOrEqual(col={columnName}, val={value})");
        }

        public void AddQueryGreaterThan(IQueryHandle queryHandle, string columnName, object value)
        {
            notifyOnCall($"AddQueryGreaterThan(col={columnName}, val={value})");
        }

        public void AddQueryGreaterThanOrEqual(IQueryHandle queryHandle, string columnName, object value)
        {
            notifyOnCall($"AddQueryGreaterThanOrEqual(col={columnName}, val={value})");
        }

        public void AddQueryGroupBegin(IQueryHandle queryHandle)
        {
            notifyOnCall($"AddQueryGroupBegin");
        }

        public void AddQueryGroupEnd(IQueryHandle queryHandle)
        {
            notifyOnCall($"AddQueryGroupEnd");
        }

        public void AddQueryAnd(IQueryHandle queryHandle)
        {
            notifyOnCall($"AddQueryAnd");
        }

        public void AddQueryOr(IQueryHandle queryHandle)
        {
            notifyOnCall($"AddQueryOr");
        }


        public IEnumerable<IRowHandle> ExecuteQuery(IQueryHandle queryHandle, Type objectType)
        {
            notifyOnCall($"ExecuteQuery");
            return new List<IRowHandle>();
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

    public class FakeRowHandle : IRowHandle
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool IsClosed { get; }
        public bool IsInvalid { get; }
        public bool IsAttached { get; }
        public long RowIndex { get; set;  }
    }
}

