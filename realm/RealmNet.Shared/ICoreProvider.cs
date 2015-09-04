using System;
using System.Collections.Generic;

namespace RealmNet
{
    public interface ICoreProvider
    {
        ISharedGroupHandle CreateSharedGroup(string filename); 
        bool HasTable(IGroupHandle groupHandle, string tableName);
        void AddTable(IGroupHandle groupHandle, string tableName);
        void AddColumnToTable(IGroupHandle groupHandle, string tableName, string columnName, Type columnType);
        IRowHandle AddEmptyRow(IGroupHandle groupHandle, string tableName);

        void RemoveRow(IGroupHandle groupHandle, string tableName, IRowHandle rowHandle);

        T GetValue<T>(IGroupHandle groupHandle, string tableName, string propertyName, IRowHandle rowHandle);
        void SetValue<T>(IGroupHandle groupHandle, string tableName, string propertyName, IRowHandle rowHandle, T value);
        IList<T> GetListValue<T>(IGroupHandle groupHandle, string tableName, string propertyName, IRowHandle rowHandle);
        void SetListValue<T>(IGroupHandle groupHandle, string tableName, string propertyName, IRowHandle rowHandle, IList<T> value);

        IQueryHandle CreateQuery(IGroupHandle groupHandle, string tableName);
        void AddQueryEqual(IQueryHandle queryHandle, string columnName, object value);
        void AddQueryNotEqual(IQueryHandle queryHandle, string columnName, object value);
        void AddQueryLessThan(IQueryHandle queryHandle, string columnName, object value);
        void AddQueryLessThanOrEqual(IQueryHandle queryHandle, string columnName, object value);
        void AddQueryGreaterThan(IQueryHandle queryHandle, string columnName, object value);
        void AddQueryGreaterThanOrEqual(IQueryHandle queryHandle, string columnName, object value);
        void AddQueryGroupBegin(IQueryHandle queryHandle);
        void AddQueryGroupEnd(IQueryHandle queryHandle);
        void AddQueryAnd(IQueryHandle queryHandle);
        void AddQueryOr(IQueryHandle queryHandle);

        IEnumerable<IRowHandle> ExecuteQuery(IQueryHandle queryHandle, Type objectType);

        IGroupHandle NewGroup();
        IGroupHandle NewGroupFromFile(string path, GroupOpenMode openMode);
        void GroupCommit(IGroupHandle groupHandle);
        bool GroupIsEmpty(IGroupHandle groupHandle);
        long GroupSize(IGroupHandle groupHandle);
    }
}
