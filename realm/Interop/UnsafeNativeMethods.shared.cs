using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Interop.Config;
using InteropShared;

namespace RealmNet.Interop
{
    // TODO probably remove this and the helpers IntPtrToDataType and DataTypeToIntPtr 
    // when some of the NotImplemented accessors below are finished
    public enum DataType
    {
        Int = 0,
        Bool = 1,
        String = 2,
        Binary = 4,
        Table = 5,
        Mixed = 6,
        Date = 7,
        Float = 9,
        Double = 10,
    }

    internal static class UnsafeNativeMethods
    {
        private static DataType IntPtrToDataType(IntPtr value)
        {
            return (DataType)value;
        }

        private static IntPtr DataTypeToIntPtr(DataType value)
        {
            return (IntPtr)value;
        }

        /*
        AD Note I think these can probably go but will just comment out for now
        If they were in regular use they would have linkage to 32bit as well?

        [DllImport(InteropConfig.L64, EntryPoint = "realm_get_wrapper_ver", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr GetWrapperVer();

        [DllImport(InteropConfig.L64, EntryPoint = "realm_get_ver_minor", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GetMinorVer();
        */


// TODO work out if new_table should be mapped as it's not currently in use
/*
        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "new_table", CallingConvention = CallingConvention.Cdecl)]
        private static extern TableHandle new_table();
*/

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_add_column", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr table_add_column(TableHandle tableHandle, IntPtr type,
            [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr nameLen);


        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_add_empty_row", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr table_add_empty_row(TableHandle tableHandle, IntPtr numRows);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_string", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void table_set_string(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_string", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr table_get_string(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex,
            IntPtr buffer, IntPtr bufsize);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_bool", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void table_set_bool(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx, IntPtr value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_bool", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr table_get_bool(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_set_int64", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void table_set_int64(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx, Int64 value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_int64", CallingConvention = CallingConvention.Cdecl)]
        internal static extern Int64 table_get_int64(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_where", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr table_where(TableHandle handle);

        //c++ each of the operator functions such as query_string_equal
        //returns q again, the QueryHandle object is re-used and keeps its pointer.
        //so high-level stuff should also return self, to enable stacking of operations QueryHandle.dothis().dothat()


        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_string_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void query_string_equal(QueryHandle queryPtr, IntPtr columnIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_string_not_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void query_string_not_equal(QueryHandle queryPtr, IntPtr columnIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_bool_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void query_bool_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_bool_not_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void query_bool_not_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void query_int_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_not_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void query_int_not_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_less", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void query_int_less(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_less_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void query_int_less_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_greater", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void query_int_greater(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_int_greater_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void query_int_greater_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_float_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void query_float_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_float_not_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void query_float_not_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_double_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void query_double_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_double_not_equal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void query_double_not_equal(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_find", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr query_find(QueryHandle queryHandle, IntPtr lastMatch);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "query_get_column_index", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr query_get_column_index(QueryHandle queryPtr,
        [MarshalAs(UnmanagedType.LPWStr)] String columnName, IntPtr columnNameLen);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "group", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr query_begin_group(QueryHandle queryHandle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "end_group", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr query_end_group(QueryHandle queryHandle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "Or", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr query_or(QueryHandle queryHandle);

        public static string group_to_string(GroupHandle groupHandle)
        {
            throw new NotImplementedException();
        }


        internal static IntPtr tableview_find_all_int(TableViewHandle tableViewHandle, long columnIndex, long value)
        {
            throw new NotImplementedException();
        }

        internal static IntPtr tableview_find_all_bool(TableViewHandle tableViewHandle, long columnIndex, bool value)
        {
            throw new NotImplementedException();
        }

        internal static IntPtr tableview_find_all_datetime(TableViewHandle tableViewHandle, long columnIndex, DateTime value)
        {
            throw new NotImplementedException();
        }

        internal static string table_view_to_string(TableViewHandle TableViewHandle, long limit)
        {
            throw new NotImplementedException();
        }

        internal static IntPtr tableview_find_all_float(TableViewHandle tableViewHandle, long columnIndex, float value)
        {
            throw new NotImplementedException();
        }

        internal static IntPtr tableview_find_all_double(TableViewHandle tableViewHandle, long columnIndex, double value)
        {
            throw new NotImplementedException();
        }

        internal static IntPtr tableview_find_all_string(TableViewHandle tableViewHandle, long columnIndex, string value)
        {
            throw new NotImplementedException();
        }

        internal static IntPtr tableview_get_subtable(TableViewHandle tableViewHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        internal static void tableview_unbind(TableViewHandle tableViewHandle)
        {
            throw new NotImplementedException();
        }

        internal static void table_unbind(TableHandle tableHandle)
        {
            throw new NotImplementedException();
        }

        internal static IntPtr table_copy_table(TableHandle tableHandle)
        {
            throw new NotImplementedException();
        }

        internal static IntPtr table_get_sub_table(TableHandle tableHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        internal static IntPtr table_distinct(TableHandle tableHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        internal static IntPtr table_find_all_int(TableHandle tableHandle, long columnIndex, long value)
        {
            throw new NotImplementedException();
        }

        internal static IntPtr table_find_all_bool(TableHandle tableHandle, long columnIndex, bool value)
        {
            throw new NotImplementedException();
        }

        internal static IntPtr table_find_all_date_time(TableHandle tableHandle, long columnIndex, DateTime value)
        {
            throw new NotImplementedException();
        }

        internal static IntPtr table_find_all_float(TableHandle tableHandle, long columnIndex, float value)
        {
            throw new NotImplementedException();
        }

        internal static IntPtr table_find_all_double(TableHandle tableHandle, long columnIndex, double value)
        {
            throw new NotImplementedException();
        }

        internal static IntPtr table_find_all_string(TableHandle tableHandle, long columnIndex, string value)
        {
            throw new NotImplementedException();
        }

        internal static IntPtr table_find_all_empty_binary(TableHandle tableHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        internal static IntPtr table_find_all_binary(TableHandle tableHandle, long columnIndex, IntPtr valuePointer, IntPtr length)
        {
            throw new NotImplementedException();
        }

        internal static IntPtr table_get_spec(TableHandle tableHandle)
        {
            throw new NotImplementedException();
        }

        //todo:add return value to rollback if c++ threw an exception
        [DllImport(InteropConfig.L64, EntryPoint = "shared_group_rollback", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr shared_group_rollback64(SharedGroupHandle handle);

        [DllImport(InteropConfig.L32, EntryPoint = "shared_group_rollback", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr shared_group_rollback32(SharedGroupHandle handle);


        //called by SharedGroupHandle atomically
        public static IntPtr shared_group_rollback(SharedGroupHandle sharedGroupHandle)
        {
            return (InteropConfig.Is64Bit)
                ? shared_group_rollback64(sharedGroupHandle)
                : shared_group_rollback32(sharedGroupHandle);
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_group_end_read", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr shared_group_end_read(SharedGroupHandle handle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_group_delete", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void shared_group_delete(IntPtr handle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_group_commit", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr shared_group_commit(SharedGroupHandle sharedGroupHandle);

        //this is complicated.
        //The call to shared_group_begin_read must result in us always having two things inside a sharedgroup
        //handle : the shared group pointer, AND the shared group transaction state set to InReadTransaction

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_group_begin_read", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr shared_group_begin_read(SharedGroupHandle sharedGroupPtr);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_group_begin_write", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr shared_group_begin_write(SharedGroupHandle sharedGroupPtr);

        internal static IntPtr query_find_all(QueryHandle queryHandle, long start, long end, long limit)
        {
            throw new NotImplementedException();
        }

        internal static IntPtr query_find_all_np(QueryHandle queryHandle)
        {
            throw new NotImplementedException();
        }

        internal static void query_delete(QueryHandle queryHandle)
        {
            throw new NotImplementedException();
        }

        internal static void group_delete(GroupHandle groupHandle)
        {
            throw new NotImplementedException();
        }

        //If the name exists in the group, the table associated with the name is returned
        //if the name does not exist in the group, a new table is created and returned
        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "group_get_or_add_table", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr group_get_or_add_table(GroupHandle groupHandle,
            [MarshalAs(UnmanagedType.LPWStr)] String tableName, IntPtr tableNameLen);

        internal static IntPtr group_get_table_by_index(GroupHandle groupHandle, long tableIndex)
        {
            throw new NotImplementedException();
        }

        public static void group_new(GroupHandle @GroupHandle, bool readOnly)
        {
            throw new NotImplementedException();
        }

        public static bool group_equals(GroupHandle @GroupHandle, GroupHandle otherGroup)
        {
            throw new NotImplementedException();
        }

        public static void group_new_file(GroupHandle @GroupHandle, string path, GroupOpenMode openMode)
        {
            throw new NotImplementedException();
        }

        public static void group_frombinary_data(GroupHandle @GroupHandle, byte[] binaryGroup)
        {
            throw new NotImplementedException();
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "group_has_table", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr group_has_table(GroupHandle groupHandle,
            [MarshalAs(UnmanagedType.LPWStr)] String tableName, IntPtr tableNameLen);

        public static long query_count(QueryHandle QueryHandle, long start, long end, long limit)
        {
            throw new NotImplementedException();
        }

        public static void group_write(GroupHandle @GroupHandle, string path)
        {
            throw new NotImplementedException();
        }

        public static byte[] group_write_to_memory(GroupHandle @GroupHandle)
        {
            throw new NotImplementedException();
        }

        public static void group_commit(GroupHandle @GroupHandle)
        {
            throw new NotImplementedException();
        }

        public static void query_int_greater(QueryHandle QueryHandle, long getColumnIndex, long value)
        {
            throw new NotImplementedException();
        }

        public static bool group_is_empty(GroupHandle @GroupHandle)
        {
            throw new NotImplementedException();
        }

        public static long group_size(GroupHandle @GroupHandle)
        {
            throw new NotImplementedException();
        }

        public static void query_int_between(QueryHandle QueryHandle, long columnIndex, long lowValue, long highValue)
        {
            throw new NotImplementedException();
        }

        public static double query_average(QueryHandle QueryHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "new_shared_group_file_defaults", CallingConvention = CallingConvention.Cdecl)]
        internal static extern SharedGroupHandle new_shared_group_file_defaults(
            [MarshalAs(UnmanagedType.LPWStr)] string fileName,
            IntPtr fileNameLen);

        public static bool table_is_attached(TableHandle TableHandle)
        {
            throw new NotImplementedException();
        }

/*        public static void new_shared_group_file(SharedGroupHandle sharedGroup, string fileName, bool noCreate, DurabilityLevel durabilityLevel)
        {
            throw new NotImplementedException();
        }*/

        public static void shared_group_reserve(SharedGroupHandle sharedGroup, long bytesToReserve)
        {
            throw new NotImplementedException();
        }

        public static bool shared_group_has_changed(SharedGroupHandle sharedGroup)
        {
            throw new NotImplementedException();
        }

        public static bool table_has_shared_spec(TableHandle TableHandle)
        {
            throw new NotImplementedException();
        }

        public static void table_view_clear(TableViewHandle TableViewHandle)
        {
            throw new NotImplementedException();
        }

        public static void table_new(TableHandle TableHandle, bool isReadOnly)
        {
            throw new NotImplementedException();
        }

        public static void table_rename_column(TableHandle TableHandle, long columnIndex, string newName)
        {
            throw new NotImplementedException();
        }

        public static DateTime table_view_get_mixed_date_time(TableViewHandle TableViewHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static long table_view_get_column_index(TableViewHandle TableViewHandle, string name)
        {
            throw new NotImplementedException();
        }

        public static void table_remove_column(TableHandle TableHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static long table_view_sum_long(TableViewHandle TableViewHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static double table_view_sum_float(TableViewHandle TableViewHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static double table_view_sum_double(TableViewHandle TableViewHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static long table_view_minimum_long(TableViewHandle TableViewHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static bool spec_equals_spec(TableHandle tableA, TableHandle tableB)
        {
            throw new NotImplementedException();
        }

        public static float table_view_minimum_float(TableViewHandle TableViewHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static double table_view_minimum_double(TableViewHandle TableViewHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static DateTime table_view_minimum_date_time(TableViewHandle TableViewHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static void table_clear(TableHandle TableHandle)
        {
            throw new NotImplementedException();
        }

        public static long table_get_column_count(TableHandle TableHandle)
        {
            throw new NotImplementedException();
        }

        public static void table_view_sort(TableViewHandle TableViewHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static string table_get_column_name(TableHandle TableHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "table_get_column_index", CallingConvention = CallingConvention.Cdecl)]
         //returns -1 if the column string does not match a column index
       internal static extern IntPtr table_get_column_index(TableHandle tablehandle,
            [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr nameLen);

        public static long table_view_maximum(TableViewHandle TableViewHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static float table_view_maximum_float(TableViewHandle TableViewHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static double table_view_maximum_double(TableViewHandle TableViewHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static DateTime table_view_maximum_date_time(TableViewHandle TableViewHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static double table_view_average_long(TableViewHandle TableViewHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static double table_view_average_float(TableViewHandle TableViewHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static DataType table_get_column_type(TableHandle TableHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static double table_view_average_double(TableViewHandle TableViewHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static long table_view_count_long(TableViewHandle TableViewHandle, long columnIndex, long target)
        {
            throw new NotImplementedException();
        }

        public static long table_view_count_float(TableViewHandle TableViewHandle, long columnIndex, float target)
        {
            throw new NotImplementedException();
        }

        public static long table_view_count_string(TableViewHandle TableViewHandle, long columnIndex, string target)
        {
            throw new NotImplementedException();
        }

        public static void table_insert_empty_row(TableHandle TableHandle, long rowIndex, long rowsToInsert)
        {
            throw new NotImplementedException();
        }

        public static long table_view_count_double(TableViewHandle TableViewHandle, long columnIndex, double target)
        {
            throw new NotImplementedException();
        }

        public static long table_view_get_source_index(TableViewHandle TableViewHandle, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static string table_view_to_json(TableViewHandle TableViewHandle)
        {
            throw new NotImplementedException();
        }

        public static string table_view_to_string(TableViewHandle TableViewHandle)
        {
            throw new NotImplementedException();
        }

        public static string table_view_row_to_string(TableViewHandle TableViewHandle, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static void table_remove(TableHandle TableHandle, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static void table_view_remove_row(TableViewHandle TableViewHandle, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static DataType table_get_mixed_type(TableHandle TableHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static DateTime table_get_mixed_date_time(TableHandle TableHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static void table_view_set_mixed_float(TableViewHandle TableViewHandle, long columnIndex, long rowIndex, float value)
        {
            throw new NotImplementedException();
        }

        public static void table_optimize(TableHandle TableHandle)
        {
            throw new NotImplementedException();
        }

        public static void table_view_set_mixed_double(TableViewHandle TableViewHandle, long columnIndex, long rowIndex, double value)
        {
            throw new NotImplementedException();
        }

        public static string table_to_string(TableHandle TableHandle)
        {
            throw new NotImplementedException();
        }

        public static void table_view_set_mixed_date(TableViewHandle TableViewHandle, long columnIndex, long rowIndex, DateTime value)
        {
            throw new NotImplementedException();
        }

        public static bool table_view_get_mixed_bool(TableViewHandle TableViewHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static string table_row_to_string(TableHandle TableHandle, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static string table_view_get_mixed_string(TableViewHandle TableViewHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static string table_to_json(TableHandle TableHandle)
        {
            throw new NotImplementedException();
        }

        public static byte[] table_view_get_mixed_binary(TableViewHandle TableViewHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static long table_view_get_sub_table_size(TableViewHandle TableViewHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static byte[] table_view_get_binary(TableViewHandle TableViewHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static void table_view_clear_sub_table(TableViewHandle TableViewHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static void table_view_set_long(TableViewHandle TableViewHandle, long columnIndex, long rowIndex, long value)
        {
            throw new NotImplementedException();
        }

        public static void table_view_set_int(TableViewHandle TableViewHandle, long columnIndex, long rowIndex, int value)
        {
            throw new NotImplementedException();
        }

        public static void table_view_set_bool(TableViewHandle TableViewHandle, long columnIndex, long rowIndex, bool value)
        {
            throw new NotImplementedException();
        }

        public static void table_view_set_date(TableViewHandle TableViewHandle, long columnIndex, long rowIndex, DateTime value)
        {
            throw new NotImplementedException();
        }

        public static void table_view_set_float(TableViewHandle TableViewHandle, long columnIndex, long rowIndex, float value)
        {
            throw new NotImplementedException();
        }

        public static void table_view_set_double(TableViewHandle TableViewHandle, long columnIndex, long rowIndex, double value)
        {
            throw new NotImplementedException();
        }

        public static void table_view_set_string(TableViewHandle TableViewHandle, long columnIndex, long rowIndex, string value)
        {
            throw new NotImplementedException();
        }

        public static void table_view_set_binary(TableViewHandle TableViewHandle, long columnIndex, long rowIndex, byte[] value)
        {
            throw new NotImplementedException();
        }

        public static void table_view_set_mixed_long(TableViewHandle TableViewHandle, long columnIndex, long rowIndex, long value)
        {
            throw new NotImplementedException();
        }

        public static void table_view_set_mixed_int(TableViewHandle TableViewHandle, long columnIndex, long rowIndex, int value)
        {
            throw new NotImplementedException();
        }

        public static void table_view_set_mixed_bool(TableViewHandle TableViewHandle, long columnIndex, long rowIndex, bool value)
        {
            throw new NotImplementedException();
        }

        public static void table_view_set_mixed_string(TableViewHandle TableViewHandle, long columnIndex, long rowIndex, string value)
        {
            throw new NotImplementedException();
        }

        public static void table_view_set_mixed_binary(TableViewHandle TableViewHandle, long columnIndex, long rowIndex, byte[] value)
        {
            throw new NotImplementedException();
        }

        public static void table_view_set_mixed_empty_sub_table(TableViewHandle TableViewHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static void table_view_set_mixed_sub_table(TableViewHandle TableViewHandle, long columnIndex, long rowIndex, TableHandle source)
        {
            throw new NotImplementedException();
        }

        public static void table_view_set_sub_table(TableViewHandle TableViewHandle, long columnIndex, long rowIndex, TableHandle value)
        {
            throw new NotImplementedException();
        }

        public static string tableview_get_string(TableViewHandle TableViewHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static long table_view_get_mixed_int(TableViewHandle TableViewHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static double table_view_get_mixed_double(TableViewHandle TableViewHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static float table_view_get_mixed_float(TableViewHandle TableViewHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static DateTime table_view_get_date_time(TableViewHandle TableViewHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static DataType table_view_get_mixed_type(TableViewHandle TableViewHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static string table_view_get_column_name(TableViewHandle TableViewHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static long table_view_get_column_count(TableViewHandle TableViewHandle)
        {
            throw new NotImplementedException();
        }

        public static DataType table_view_get_column_type(TableViewHandle TableViewHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static long table_view_size(TableViewHandle TableViewHandle)
        {
            throw new NotImplementedException();
        }

        public static bool table_view_get_bool(TableViewHandle TableViewHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static bool table_has_index(TableHandle TableHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static long table_view_get_int(TableViewHandle TableViewHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static double table_view_get_double(TableViewHandle TableViewHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static float table_view_get_float(TableViewHandle TableViewHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static long table_view_find_first_binary(TableViewHandle TableViewHandle, long columnIndex, byte[] value)
        {
            throw new NotImplementedException();
        }

        public static long table_view_find_first_int(TableViewHandle TableViewHandle, long columnIndex, long value)
        {
            throw new NotImplementedException();
        }

        public static long table_view_find_first_string(TableViewHandle TableViewHandle, long columnIndex, string value)
        {
            throw new NotImplementedException();
        }

        public static long table_view_find_first_double(TableViewHandle TableViewHandle, long columnIndex, double value)
        {
            throw new NotImplementedException();
        }

        public static long table_view_find_first_float(TableViewHandle TableViewHandle, long columnIndex, float value)
        {
            throw new NotImplementedException();
        }

        public static long table_view_find_first_date(TableViewHandle TableViewHandle, long columnIndex, DateTime value)
        {
            throw new NotImplementedException();
        }

        public static long table_view_find_first_bool(TableViewHandle TableViewHandle, long columnIndex, bool value)
        {
            throw new NotImplementedException();
        }

        public static byte[] table_get_binary(TableHandle TableHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static void table_set_binary(TableHandle TableHandle, long columnIndex, long rowIndex, byte[] value)
        {
            throw new NotImplementedException();
        }

        public static void table_add_int(TableHandle TableHandle, long columnIndex, long value)
        {
            throw new NotImplementedException();
        }

        public static void table_set_sub_table(TableHandle TableHandle, long columnIndex, long rowIndex, TableHandle value)
        {
            throw new NotImplementedException();
        }

        public static long table_get_sub_table_size(TableHandle TableHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static void table_clear_sub_table(TableHandle TableHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static long table_count_long(TableHandle TableHandle, long columnIndex, long target)
        {
            throw new NotImplementedException();
        }

        public static long table_count_string(TableHandle TableHandle, long columnIndex, string target)
        {
            throw new NotImplementedException();
        }

        public static long table_count_float(TableHandle TableHandle, long columnIndex, float target)
        {
            throw new NotImplementedException();
        }

        public static long table_count_double(TableHandle TableHandle, long columnIndex, double target)
        {
            throw new NotImplementedException();
        }

        public static long table_sum_long(TableHandle TableHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static double table_sum_float(TableHandle TableHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static double table_sum_double(TableHandle TableHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static long table_maximum_long(TableHandle TableHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static float table_maximum_float(TableHandle TableHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static double table_maximum_double(TableHandle TableHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static DateTime table_maximum_date_time(TableHandle TableHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static long table_minimum(TableHandle TableHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static float table_minimum_float(TableHandle TableHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static double table_minimum_double(TableHandle TableHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static DateTime table_minimum_date_time(TableHandle TableHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static double table_average(TableHandle TableHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static double table_average_float(TableHandle TableHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static double table_average_double(TableHandle TableHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static long table_find_first_int(TableHandle TableHandle, long columnIndex, long value)
        {
            throw new NotImplementedException();
        }

        public static long table_find_first_bool(TableHandle TableHandle, long columnIndex, bool value)
        {
            throw new NotImplementedException();
        }

        public static long table_find_first_date(TableHandle TableHandle, long columnIndex, DateTime value)
        {
            throw new NotImplementedException();
        }

        public static long table_find_first_float(TableHandle TableHandle, long columnIndex, float value)
        {
            throw new NotImplementedException();
        }

        public static long table_find_first_double(TableHandle TableHandle, long columnIndex, double value)
        {
            throw new NotImplementedException();
        }

        public static long table_find_first_string(TableHandle TableHandle, long columnIndex, string value)
        {
            throw new NotImplementedException();
        }

        public static long table_find_first_binary(TableHandle TableHandle, long columnIndex, byte[] value)
        {
            throw new NotImplementedException();
        }

        public static void table_set_mixed_float(TableHandle TableHandle, long columnIndex, long rowIndex, float value)
        {
            throw new NotImplementedException();
        }

        public static void table_set_float(TableHandle TableHandle, long columnIndex, long rowIndex, float value)
        {
            throw new NotImplementedException();
        }

        public static void table_set_mixed_double(TableHandle TableHandle, long columnIndex, long rowIndex, double value)
        {
            throw new NotImplementedException();
        }

        public static void table_set_double(TableHandle TableHandle, long columnIndex, long rowIndex, double value)
        {
            throw new NotImplementedException();
        }

        public static void table_set_mixed_date(TableHandle TableHandle, long columnIndex, long rowIndex, DateTime value)
        {
            throw new NotImplementedException();
        }

        public static void table_set_date(TableHandle TableHandle, long columnIndex, long rowIndex, DateTime value)
        {
            throw new NotImplementedException();
        }

        public static bool table_get_mixed_bool(TableHandle TableHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static string table_get_mixed_string(TableHandle TableHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static byte[] table_get_mixed_binary(TableHandle TableHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static long table_size(TableHandle TableHandle)
        {
            throw new NotImplementedException();
        }

        public static double table_get_double(TableHandle TableHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static float table_get_float(TableHandle TableHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static void table_set_mixed_long(TableHandle TableHandle, long columnIndex, long rowIndex, long value)
        {
            throw new NotImplementedException();
        }

        public static void table_set_mixed_int(TableHandle TableHandle, long columnIndex, long rowIndex, int value)
        {
            throw new NotImplementedException();
        }

        public static void table_set_mixed_bool(TableHandle TableHandle, long columnIndex, long rowIndex, bool value)
        {
            throw new NotImplementedException();
        }

        public static void table_set_mixed_string(TableHandle TableHandle, long columnIndex, long rowIndex, string value)
        {
            throw new NotImplementedException();
        }

        public static void table_set_mixed_binary(TableHandle TableHandle, long columnIndex, long rowIndex, byte[] value)
        {
            throw new NotImplementedException();
        }

        public static void table_set_mixed_sub_table(TableHandle TableHandle, long columnIndex, long rowIndex, TableHandle source)
        {
            throw new NotImplementedException();
        }

        public static void table_set_mixed_empty_sub_table(TableHandle TableHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static long table_get_mixed_int(TableHandle TableHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static double table_get_mixed_double(TableHandle TableHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static float table_get_mixed_float(TableHandle TableHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static DateTime table_get_date_time(TableHandle TableHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static void table_set_index(TableHandle TableHandle, long columnIndex)
        {
            throw new NotImplementedException();
        }

        public static long table_add_sub_column(TableHandle TableHandle, IList<long> path, DataType dataType, string columnName)
        {
            throw new NotImplementedException();
        }

        public static void table_rename_sub_column(TableHandle TableHandle, IList<long> path, string name)
        {
            throw new NotImplementedException();
        }

        public static void table_remove_sub_column(TableHandle TableHandle, IList<long> path)
        {
            throw new NotImplementedException();
        }
    }
}