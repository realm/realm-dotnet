using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using InteropShared;
using System.Runtime.InteropServices;
using System.Linq;
using System.IO;

namespace RealmNet.Interop
{
    public class CoreProvider : ICoreProvider
    {
        #region helpers

        static IntPtr BoolToIntPtr(Boolean value)
        {
            return value ? (IntPtr)1 : (IntPtr)0;
        }

        static Boolean IntPtrToBool(IntPtr value)
        {
            return (IntPtr)1 == value;
        }

        static IntPtr StrAllocateBuffer(out long currentBufferSizeChars, long bufferSizeNeededChars)
        {
            currentBufferSizeChars = bufferSizeNeededChars;
            return Marshal.AllocHGlobal((IntPtr)(bufferSizeNeededChars * sizeof(char)));
            //allocHGlobal instead of  AllocCoTaskMem because allcHGlobal allows lt 2 gig on 64 bit (not that .net supports that right now, but at least this allocation will work with lt 32 bit strings)   
        }
        
        static string StrBufToStr(IntPtr buffer, int bufferSizeNeededChars)
        {
            string retStr = bufferSizeNeededChars > 0 ? Marshal.PtrToStringUni(buffer, bufferSizeNeededChars) : "";
            //return "" if the string is empty, otherwise copy data from the buffer
            Marshal.FreeHGlobal(buffer);
            return retStr;
        }

        static Boolean StrBufferOverflow(IntPtr buffer, long currentBufferSizeChars, long bufferSizeNeededChars)
        {
            if (currentBufferSizeChars < bufferSizeNeededChars)
            {
                Marshal.FreeHGlobal(buffer);

                return true;
            }
            return false;
        }

        // returns magic numbers from core, formerly the enum DataType in UnsafeNativeMethods.shared.cs
        internal static IntPtr RealmColType(Type columnType)
        {
            // ordered in decreasing likelihood of type
            if (columnType == typeof(string))
                return (IntPtr)2;
            if (columnType == typeof(int))
                return (IntPtr)0;
            if (columnType == typeof(float))
                return (IntPtr)9;
            if (columnType == typeof(double))
                return (IntPtr)10;
            if (columnType == typeof(DateTime))
                return (IntPtr)7;
            if (columnType == typeof(bool))
                return (IntPtr)1;
            /*
            TODO
                    Binary = 4,
                    Table = 5,
                    Mixed = 6,

            */
            throw new NotImplementedException();
        }

        #endregion  // helpers

        public ISharedGroupHandle CreateSharedGroup(string filename)
        {
            if (filename == null)
            {
                const string realmFilename = "db.realm";
                #if __IOS__
                string libraryPath;
                if (UIKit.UIDevice.CurrentDevice.CheckSystemVersion(8, 0))  // > ios 8
                {
                    libraryPath = Foundation.NSFileManager.DefaultManager.GetUrls (Foundation.NSSearchPathDirectory.LibraryDirectory, Foundation.NSSearchPathDomain.User) [0].Path;
                } 
                else 
                {
                    var docdir = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
                    libraryPath = Path.GetFullPath(Path.Combine (docdir, "..", "Library")); 
                }

                filename = Path.Combine(libraryPath, realmFilename);
                #else
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal); // Documents folder
                filename = Path.Combine(documentsPath, realmFilename);
                #endif
            }

            return NativeSharedGroup.new_shared_group_file(filename, (IntPtr)filename.Length, (IntPtr)0, (IntPtr)0);
        }

        public bool HasTable(IGroupHandle groupHandle, string tableName)
        {
            var gh = groupHandle as GroupHandle;
            return NativeGroup.has_table(gh, tableName, (IntPtr)tableName.Length) == (IntPtr)1;
        }

        private TableHandle GetTable(IGroupHandle groupHandle, string tableName)
        {
            var gh = groupHandle as GroupHandle;
            return gh.GetTable(tableName);
        }

        public void AddTable(IGroupHandle groupHandle, string tableName)
        {
            GetTable(groupHandle, tableName);
        }

        private IntPtr GetColumnIndex(TableHandle tableHandle, string columnName)
        {
            return NativeTable.get_column_index(tableHandle, columnName, (IntPtr)columnName.Length);
        }

        public void AddColumnToTable(IGroupHandle groupHandle, string tableName, string columnName, Type columnType)
        {
            var columnIndex = NativeTable.add_column(GetTable(groupHandle, tableName), RealmColType(columnType), columnName, (IntPtr)columnName.Length);
        }

        public IRowHandle AddEmptyRow(IGroupHandle groupHandle, string tableName)
        {
            return NativeTable.add_empty_row(GetTable(groupHandle, tableName)); 
        }

        public void RemoveRow(IGroupHandle groupHandle, string tableName, IRowHandle rowHandle)
        {
            NativeTable.remove_row(GetTable(groupHandle, tableName), (RowHandle)rowHandle);
        }

        public T GetValue<T>(IGroupHandle groupHandle, string tableName, string propertyName, IRowHandle rowHandle)
        {
            var tableHandle = GetTable(groupHandle, tableName);
            var columnIndex = GetColumnIndex(tableHandle, propertyName);

            // TODO: This is not threadsafe. table_get_* should take an IRowHandle instead.
            var rowIndex = rowHandle.RowIndex;

            if (typeof(T) == typeof(string))
            {
                long bufferSizeNeededChars = 16;
                IntPtr buffer;
                long currentBufferSizeChars;

                do
                {
                    buffer = StrAllocateBuffer(out currentBufferSizeChars, bufferSizeNeededChars);
                    bufferSizeNeededChars = (long)NativeTable.get_string(tableHandle, columnIndex, (IntPtr)rowIndex, buffer,
                            (IntPtr)currentBufferSizeChars);

                } while (StrBufferOverflow(buffer, currentBufferSizeChars, bufferSizeNeededChars));
                return (T)Convert.ChangeType(StrBufToStr(buffer, (int)bufferSizeNeededChars), typeof(T));
            }
            else if (typeof(T) == typeof(bool))
            {
                var value = IntPtrToBool( NativeTable.get_bool(tableHandle, columnIndex, (IntPtr)rowIndex) );
                return (T)Convert.ChangeType(value, typeof(T));
            }
            else if (typeof(T) == typeof(int))  // System.Int32 regardless of bitness
            {
                var value = NativeTable.get_int64(tableHandle, columnIndex, (IntPtr)rowIndex);
                return (T)Convert.ChangeType(value, typeof(T));
            }
            else if (typeof(T) == typeof(Int64)) 
            {
                var value = NativeTable.get_int64(tableHandle, columnIndex, (IntPtr)rowIndex);
                return (T)Convert.ChangeType(value, typeof(T));
            }
            else
                throw new Exception ("Unsupported type " + typeof(T).Name);
        }

        public void SetValue<T>(IGroupHandle groupHandle, string tableName, string propertyName, IRowHandle rowHandle, T value)
        {
            var tableHandle = GetTable(groupHandle, tableName);
            var columnIndex = GetColumnIndex(tableHandle, propertyName);

            // TODO: This is not threadsafe. table_get_* should take an IRowHandle instead.
            var rowIndex = ((RowHandle) rowHandle).RowIndex;

            if (typeof(T) == typeof(string))
            {
                var str = value.ToString();
                NativeTable.set_string(tableHandle, columnIndex, (IntPtr)rowIndex, str, (IntPtr)str.Length);
            }
            else if (typeof(T) == typeof(bool))
            {
                var marshalledValue = BoolToIntPtr((bool)Convert.ChangeType(value, typeof(bool)));
                NativeTable.set_bool(tableHandle, columnIndex, (IntPtr)rowIndex, marshalledValue);
            }
            else if (typeof(T) == typeof(int))  // System.Int32 regardless of bitness
            {
                Int64 marshalledValue = Convert.ToInt64(value);
                NativeTable.set_int64(tableHandle, columnIndex, (IntPtr)rowIndex, marshalledValue);
            }
            else if (typeof(T) == typeof(Int64))
            {
                Int64 marshalledValue = Convert.ToInt64(value);
                NativeTable.set_int64(tableHandle, columnIndex, (IntPtr)rowIndex, marshalledValue);
            }
            else
                throw new Exception ("Unsupported type " + typeof(T).Name);
        }

        public IList<T> GetListValue<T>(IGroupHandle groupHandle, string tableName, string propertyName, IRowHandle rowHandle)
        {
            throw new NotImplementedException();
        }

        public void SetListValue<T>(IGroupHandle groupHandle, string tableName, string propertyName, IRowHandle rowHandle, IList<T> value)
        {
            throw new NotImplementedException();
        }

        #region Queries
        public IQueryHandle CreateQuery(IGroupHandle groupHandle, string tableName)
        {
            var tableHandle = GetTable(groupHandle, tableName);
            var queryHandle = tableHandle.TableWhere();

            //At this point sh is invalid due to its handle being uninitialized, but the root is set correctly
            //a finalize at this point will not leak anything and the handle will not do anything

            //now, set the TableView handle...
            RuntimeHelpers.PrepareConstrainedRegions();//the following finally will run with no out-of-band exceptions
            try
            { }
            finally
            {
                queryHandle.SetHandle(NativeTable.where(tableHandle));
            }//at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly
            return queryHandle;
        }

        public void AddQueryEqual(IQueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = NativeQuery.get_column_index((QueryHandle)queryHandle, columnName, (IntPtr)columnName.Length);

            var valueType = value.GetType();
            if (value.GetType() == typeof(string))
            {
                string valueStr = (string)value;
                NativeQuery.string_equal((QueryHandle)queryHandle, columnIndex, valueStr, (IntPtr)valueStr.Length);
            }
            else if (valueType == typeof(bool))
                NativeQuery.bool_equal((QueryHandle)queryHandle, columnIndex, BoolToIntPtr((bool)value));
            else if (valueType == typeof(int))
                NativeQuery.int_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((int)value));
            else if (valueType == typeof(float))
                ;// see issue 68 NativeQuery.float_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((float)value));
            else if (valueType == typeof(double))
                ;// see issue 68 NativeQuery.double_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((double)value));
            else
                throw new NotImplementedException();
        }

        public void AddQueryNotEqual(IQueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = NativeQuery.get_column_index((QueryHandle)queryHandle, columnName, (IntPtr)columnName.Length);

            var valueType = value.GetType();
            if (value.GetType() == typeof(string))
            {
                string valueStr = (string)value;
                NativeQuery.string_not_equal((QueryHandle)queryHandle, columnIndex, valueStr, (IntPtr)valueStr.Length);
            }
            else if (valueType == typeof(bool))
                NativeQuery.bool_not_equal((QueryHandle)queryHandle, columnIndex, BoolToIntPtr((bool)value));
            else if (valueType == typeof(int))
                NativeQuery.int_not_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((int)value));
            else if (valueType == typeof(float))
                ;// see issue 68 NativeQuery.float_not_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((float)value));
            else if (valueType == typeof(double))
                ;// see issue 68 NativeQuery.double_not_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((double)value));
            else
                throw new NotImplementedException();
        }

        public void AddQueryLessThan(IQueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = NativeQuery.get_column_index((QueryHandle)queryHandle, columnName, (IntPtr)columnName.Length);

            var valueType = value.GetType();
            if (valueType == typeof(int))
                NativeQuery.int_less((QueryHandle)queryHandle, columnIndex, (IntPtr)((int)value));
            else if (valueType == typeof(float))
                ;// see issue 68 NativeQuery.float_less((QueryHandle)queryHandle, columnIndex, (IntPtr)((float)value));
            else if (valueType == typeof(double))
                ;// see issue 68 NativeQuery.double_less((QueryHandle)queryHandle, columnIndex, (IntPtr)((double)value));
            else if (valueType == typeof(string) || valueType == typeof(bool))
                throw new Exception("Unsupported type " + valueType.Name);
            else
                throw new NotImplementedException();
        }

        public void AddQueryLessThanOrEqual(IQueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = NativeQuery.get_column_index((QueryHandle)queryHandle, columnName, (IntPtr)columnName.Length);

            var valueType = value.GetType();
            if (valueType == typeof(int))
                NativeQuery.int_less_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((int)value));
            else if (valueType == typeof(float))
                ;// see issue 68 NativeQuery.float_less_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((float)value));
            else if (valueType == typeof(double))
                ;// see issue 68 NativeQuery.double_less_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((double)value));
            else if (valueType == typeof(string) || valueType == typeof(bool))
                throw new Exception("Unsupported type " + valueType.Name);
            else
                throw new NotImplementedException();
        }

        public void AddQueryGreaterThan(IQueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = NativeQuery.get_column_index((QueryHandle)queryHandle, columnName, (IntPtr)columnName.Length);

            var valueType = value.GetType();
            if (valueType == typeof(int))
                NativeQuery.int_greater((QueryHandle)queryHandle, columnIndex, (IntPtr)((int)value));
            else if (valueType == typeof(float))
                ;// see issue 68 NativeQuery.float_greater((QueryHandle)queryHandle, columnIndex, (IntPtr)((float)value));
            else if (valueType == typeof(double))
                ;// see issue 68 NativeQuery.double_greater((QueryHandle)queryHandle, columnIndex, (IntPtr)((double)value));
            else if (valueType == typeof(string) || valueType == typeof(bool))
                throw new Exception("Unsupported type " + valueType.Name);
            else
                throw new NotImplementedException();
        }

        public void AddQueryGreaterThanOrEqual(IQueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = NativeQuery.get_column_index((QueryHandle)queryHandle, columnName, (IntPtr)columnName.Length);

            var valueType = value.GetType();
            if (valueType == typeof(int))
                NativeQuery.int_greater_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((int)value));
            else if (valueType == typeof(float))
                ;// see issue 68 NativeQuery.float_greater_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((float)value));
            else if (valueType == typeof(double))
                ;// see issue 68 NativeQuery.double_greater_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((double)value));
            else if (valueType == typeof(string) || valueType == typeof(bool))
                throw new Exception("Unsupported type " + valueType.Name);
            else
                throw new NotImplementedException();
        }

        public void AddQueryGroupBegin(IQueryHandle queryHandle)
        {
            NativeQuery.group_begin((QueryHandle)queryHandle);
        }

        public void AddQueryGroupEnd(IQueryHandle queryHandle)
        {
            NativeQuery.group_end((QueryHandle)queryHandle);
        }

        public void AddQueryAnd(IQueryHandle queryHandle)
        {
           // does nothing as subsequent groups automatically ANDed
        }

        public void AddQueryOr(IQueryHandle queryHandle)
        {
            NativeQuery.or((QueryHandle)queryHandle);
        }


        public IEnumerable<IRowHandle> ExecuteQuery(IQueryHandle queryHandle, Type objectType)
        {
            long nextRowIndex = 0;
            while (nextRowIndex != -1)
            {
                var rowHandle = NativeQuery.find((QueryHandle)queryHandle, (IntPtr)nextRowIndex);
                if (!rowHandle.IsInvalid)
                {
                    nextRowIndex = rowHandle.RowIndex + 1;
                    yield return rowHandle;
                }
                else
                {
                    yield break;
                }
            }
        }
        #endregion  // Queries

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
