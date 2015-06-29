using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RealmNet
{
    public interface ICoreRow
    {
        T GetValue<T>(string propertyName);
        void SetValue<T>(string propertyName, T value);
    }

    public interface ICoreProvider
    {
        bool HasTable(string tableName);
        void AddTable(string tableName);
        void AddColumnToTable(string tableName, string columnName, Type columnType);
        ICoreRow AddEmptyRow(string tableName);

        ICoreQueryHandle CreateQuery(string tableName);
        void QueryEqual(ICoreQueryHandle queryHandle, string columnName, object value);

        IEnumerable ExecuteQuery(ICoreQueryHandle queryHandle, Type objectType);
    }
}
