using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RealmNet
{
    public interface ICoreProvider
    {
        bool HasTable(string tableName);
        void AddTable(string tableName);
        void AddColumnToTable(string tableName, string columnName, Type columnType);
        long AddEmptyRow(string tableName);

        T GetValue<T>(string tableName, string propertyName, long rowIndex);
        void SetValue<T>(string tableName, string propertyName, long rowIndex, T value);

        ICoreQueryHandle CreateQuery(string tableName);
        void QueryEqual(ICoreQueryHandle queryHandle, string columnName, object value);

        IEnumerable ExecuteQuery(ICoreQueryHandle queryHandle, Type objectType);
    }
}
