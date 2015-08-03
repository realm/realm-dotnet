using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InteropShared;
using RealmNet.Interop;

namespace RealmNet
{
    public interface ICoreProvider
    {
        ISharedGroupHandle CreateSharedGroup(string filename); 
        bool HasTable(IGroupHandle groupHandle, string tableName);
        void AddTable(IGroupHandle groupHandle, string tableName);
        void AddColumnToTable(IGroupHandle groupHandle, string tableName, string columnName, Type columnType);
        long AddEmptyRow(IGroupHandle groupHandle, string tableName);

        T GetValue<T>(IGroupHandle groupHandle, string tableName, string propertyName, long rowIndex);
        void SetValue<T>(IGroupHandle groupHandle, string tableName, string propertyName, long rowIndex, T value);

        IQueryHandle CreateQuery(IGroupHandle groupHandle, string tableName);
        void QueryEqual(IQueryHandle queryHandle, string columnName, object value);

        IEnumerable<long> ExecuteQuery(IQueryHandle queryHandle, Type objectType);

        IGroupHandle NewGroup();
        IGroupHandle NewGroupFromFile(string path, GroupOpenMode openMode);
        void GroupCommit(IGroupHandle groupHandle);
        bool GroupIsEmpty(IGroupHandle groupHandle);
        long GroupSize(IGroupHandle groupHandle);
    }
}
