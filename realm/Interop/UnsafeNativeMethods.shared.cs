using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Interop.Config;

namespace RealmNet.Interop
{
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

        [DllImport(InteropConfig.L64, EntryPoint = "realm_get_wrapper_ver", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr GetWrapperVer();

        [DllImport(InteropConfig.L64, EntryPoint = "realm_get_ver_minor", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GetMinorVer();

        #region helpers

        private static IntPtr StrAllocateBuffer(out long currentBufferSizeChars, long bufferSizeNeededChars)
        {
            currentBufferSizeChars = bufferSizeNeededChars;
            return Marshal.AllocHGlobal((IntPtr)(bufferSizeNeededChars * sizeof(char)));
            //allocHGlobal instead of  AllocCoTaskMem because allcHGlobal allows lt 2 gig on 64 bit (not that .net supports that right now, but at least this allocation will work with lt 32 bit strings)   
        }

        private static Boolean StrBufferOverflow(IntPtr buffer, long currentBufferSizeChars, long bufferSizeNeededChars)
        {
            if (currentBufferSizeChars < bufferSizeNeededChars)
            {
                Marshal.FreeHGlobal(buffer);

                return true;
            }
            return false;
        }

        private static string StrBufToStr(IntPtr buffer, int bufferSizeNeededChars)
        {
            string retStr = bufferSizeNeededChars > 0 ? Marshal.PtrToStringUni(buffer, bufferSizeNeededChars) : "";
            //return "" if the string is empty, otherwise copy data from the buffer
            Marshal.FreeHGlobal(buffer);
            return retStr;
        }

        private static IntPtr BoolToIntPtr(Boolean value)
        {
            return value ? (IntPtr)1 : (IntPtr)0;
        }

        private static Boolean IntPtrToBool(IntPtr value)
        {
            return (IntPtr)1 == value;
        }

        #endregion

        #region internal static TableHandle new_table()

        [DllImport(InteropConfig.L64, EntryPoint = "new_table", CallingConvention = CallingConvention.Cdecl)]
        private static extern TableHandle new_table64();

        [DllImport(InteropConfig.L32, EntryPoint = "new_table", CallingConvention = CallingConvention.Cdecl)]
        private static extern TableHandle new_table32();

        internal static TableHandle new_table()
        {
            if (InteropConfig.Is64Bit)
                return new_table64();
            else
                return new_table32();
        }

        #endregion

        #region internal static long table_add_column(TableHandle tableHandle, DataType type, string name)

        [DllImport(InteropConfig.L64, EntryPoint = "table_add_column", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_add_column64(TableHandle tableHandle, IntPtr type,
            [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr nameLen);

        [DllImport(InteropConfig.L32, EntryPoint = "table_add_column", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_add_column32(TableHandle tableHandle, IntPtr type,
            [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr nameLen);

        internal static long table_add_column(TableHandle tableHandle, DataType type, string name)
        {
            if (InteropConfig.Is64Bit)
                return (long)table_add_column64(tableHandle, DataTypeToIntPtr(type), name, (IntPtr)name.Length);
            else
                return (long)table_add_column32(tableHandle, DataTypeToIntPtr(type), name, (IntPtr)name.Length);
        }

        #endregion

        #region internal static long table_add_empty_row(TableHandle tableHandle, long numberOfRows)

        [DllImport(InteropConfig.L64, EntryPoint = "table_add_empty_row", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_add_empty_row64(TableHandle tableHandle, IntPtr numRows);

        [DllImport(InteropConfig.L32, EntryPoint = "table_add_empty_row", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_add_empty_row32(TableHandle tableHandle, IntPtr numRows);

        internal static long table_add_empty_row(TableHandle tableHandle, long numberOfRows)
        {
            if (InteropConfig.Is64Bit)
                return (long)table_add_empty_row64(tableHandle, (IntPtr)numberOfRows);
            else
                return (long)table_add_empty_row32(tableHandle, (IntPtr)numberOfRows);
        }

        #endregion

        #region internal static void table_set_string(TableHandle tableHandle, long columnIndex, long rowIndex, string value)

        //        TIGHTDB_C_CS_API void table_set_int(TableHandle* TablePtr, size_t column_ndx, size_t row_ndx, int64_t value)
        [DllImport(InteropConfig.L64, EntryPoint = "table_set_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_string64(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

        [DllImport(InteropConfig.L32, EntryPoint = "table_set_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_string32(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

        internal static void table_set_string(TableHandle tableHandle, long columnIndex, long rowIndex, string value)
        {
            if (InteropConfig.Is64Bit)
                table_set_string64(tableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, value, (IntPtr)value.Length);
            else
                table_set_string32(tableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, value, (IntPtr)value.Length);
        }

        #endregion

        #region internal static string table_get_string(TableHandle tableHandle, long columnIndex, long rowIndex)

        [DllImport(InteropConfig.L64, EntryPoint = "table_get_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_string64(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex,
            IntPtr buffer, IntPtr bufsize);

        [DllImport(InteropConfig.L32, EntryPoint = "table_get_string", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_string32(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex,
            IntPtr buffer, IntPtr bufsize);

        internal static string table_get_string(TableHandle tableHandle, long columnIndex, long rowIndex)
        {
            long bufferSizeNeededChars = 16;
            IntPtr buffer;
            long currentBufferSizeChars;

            do
            {
                buffer = StrAllocateBuffer(out currentBufferSizeChars, bufferSizeNeededChars);

                if (InteropConfig.Is64Bit)
                    bufferSizeNeededChars =
                        (long)
                        table_get_string64(tableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, buffer,
                            (IntPtr)currentBufferSizeChars);

                else
                    bufferSizeNeededChars =
                        (long)
                        table_get_string32(tableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, buffer,
                            (IntPtr)currentBufferSizeChars);

            } while (StrBufferOverflow(buffer, currentBufferSizeChars, bufferSizeNeededChars));
            return StrBufToStr(buffer, (int)bufferSizeNeededChars);
        }

        #endregion

        #region internal static void table_set_bool(TableHandle tableHandle, long columnIndex, long rowIndex, bool value)

        [DllImport(InteropConfig.L64, EntryPoint = "table_set_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_bool64(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx, IntPtr value);

        [DllImport(InteropConfig.L32, EntryPoint = "table_set_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern void table_set_bool32(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx, IntPtr value);

        internal static void table_set_bool(TableHandle tableHandle, long columnIndex, long rowIndex, bool value)
        {
            var marshalledValue = BoolToIntPtr(value);

            if (InteropConfig.Is64Bit)
                table_set_bool64(tableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, marshalledValue);
            else
                table_set_bool32(tableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, marshalledValue);
        }

        #endregion

        #region internal static string table_get_bool(TableHandle tableHandle, long columnIndex, long rowIndex)

        [DllImport(InteropConfig.L64, EntryPoint = "table_get_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_bool64(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex);

        [DllImport(InteropConfig.L32, EntryPoint = "table_get_bool", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_get_bool32(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex);

        internal static bool table_get_bool(TableHandle tableHandle, long columnIndex, long rowIndex)
        {
            if (InteropConfig.Is64Bit)
                return IntPtrToBool(table_get_bool64(tableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex));
            else
                return IntPtrToBool(table_get_bool64(tableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex));
        }

        #endregion

        #region internal static QueryHandle table_where(TableHandle tableHandle)

        [DllImport(InteropConfig.L64, EntryPoint = "table_where", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_where64(TableHandle handle);

        [DllImport(InteropConfig.L32, EntryPoint = "table_where", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr table_where32(TableHandle handle);

        internal static IntPtr table_where(TableHandle tableHandle)
        {
            if (InteropConfig.Is64Bit)
                return table_where64(tableHandle);
            else
                return table_where32(tableHandle);
        }

        internal static void table_view_sort(TableViewHandle TableViewHandle, long columnIndex, bool ascending)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region internal static void query_bool_equal(QueryHandle queryHandle, long columnIndex, bool value)

        //in tightdb c++ this function returns q again, the QueryHandle object is re-used and keeps its pointer.
        //so high-level stuff should also return self, to enable stacking of operations QueryHandle.dothis().dothat()
        [DllImport(InteropConfig.L64, EntryPoint = "query_bool_equal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void query_bool_equal64(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

        [DllImport(InteropConfig.L32, EntryPoint = "query_bool_equal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void query_bool_equal32(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

        internal static void query_bool_equal(QueryHandle queryHandle, long columnIndex, bool value)
        {
            var ipValue = BoolToIntPtr(value);
            if (InteropConfig.Is64Bit)
                query_bool_equal64(queryHandle, (IntPtr)columnIndex, ipValue);
            else
                query_bool_equal32(queryHandle, (IntPtr)columnIndex, ipValue);
        }

        #endregion

        #region internal static void query_string_equal(QueryHandle queryHandle, long columnIndex, string value)

        //in tightdb c++ this function returns q again, the QueryHandle object is re-used and keeps its pointer.
        //so high-level stuff should also return self, to enable stacking of operations QueryHandle.dothis().dothat()
        [DllImport(InteropConfig.L64, EntryPoint = "query_string_equal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void query_string_equal64(QueryHandle queryPtr, IntPtr columnIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

        [DllImport(InteropConfig.L32, EntryPoint = "query_string_equal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void query_string_equal32(QueryHandle queryPtr, IntPtr columnIndex,
            [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

        internal static void query_string_equal(QueryHandle queryHandle, long columnIndex, string value)
        {
            if (InteropConfig.Is64Bit)
                query_string_equal64(queryHandle, (IntPtr)columnIndex, value, (IntPtr)value.Length);
            else
                query_string_equal32(queryHandle, (IntPtr)columnIndex, value, (IntPtr)value.Length);
        }

        #endregion

        #region internal static long QueryFind(QueryHandle QueryHandle, long lastMatch)

        [DllImport(InteropConfig.L64, EntryPoint = "query_find", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr query_find64(QueryHandle queryHandle, IntPtr lastMatch);

        [DllImport(InteropConfig.L32, EntryPoint = "query_find", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr query_find32(QueryHandle queryHandle, IntPtr lastMatch);

        internal static long query_find(QueryHandle queryHandle, long lastMatch)
        {
            if (InteropConfig.Is64Bit)
                return (long)query_find64(queryHandle, (IntPtr)lastMatch);
            else
                return (long)query_find32(queryHandle, (IntPtr)lastMatch);
        }

        #endregion

        #region internal static long query_get_column_index(QueryHandle queryHandle, string columnName)

        [DllImport(InteropConfig.L64, EntryPoint = "query_get_column_index", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr query_get_column_index64(QueryHandle queryPtr,
        [MarshalAs(UnmanagedType.LPWStr)] String columnName, IntPtr columnNameLen);

        [DllImport(InteropConfig.L32, EntryPoint = "query_get_column_index", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr query_get_column_index32(QueryHandle queryPtr,
            [MarshalAs(UnmanagedType.LPWStr)] String columnName, IntPtr columnNameLen);

        internal static long query_get_column_index(QueryHandle queryHandle, string columnName)
        {
            if (InteropConfig.Is64Bit)
                return (long)query_get_column_index64(queryHandle, columnName, (IntPtr)columnName.Length);
            else
                return (long)query_get_column_index32(queryHandle, columnName, (IntPtr)columnName.Length);
        }

        #endregion


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

        internal static IntPtr shared_group_rollback(SharedGroupHandle sharedGroupHandle)
        {
            throw new NotImplementedException();
        }

        internal static IntPtr shared_group_end_read(SharedGroupHandle sharedGroupHandle)
        {
            throw new NotImplementedException();
        }

        [DllImport(InteropConfig.L64, EntryPoint = "shared_group_delete", CallingConvention = CallingConvention.Cdecl)]
        private static extern void shared_group_delete64(IntPtr handle);

        [DllImport(InteropConfig.L32, EntryPoint = "shared_group_delete", CallingConvention = CallingConvention.Cdecl)]
        private static extern void shared_group_delete32(IntPtr handle);

        public static void shared_group_delete(IntPtr sharedGroupHandle)
        {
            if (InteropConfig.Is64Bit)
                shared_group_delete64(sharedGroupHandle);
            else
                shared_group_delete32(sharedGroupHandle);
        }

        internal static IntPtr shared_group_commit(SharedGroupHandle sharedGroupHandle)
        {
            throw new NotImplementedException();
        }

        internal static IntPtr shared_group_begin_read(SharedGroupHandle sharedGroupHandle)
        {
            throw new NotImplementedException();
        }

        internal static IntPtr shared_group_begin_write(SharedGroupHandle sharedGroupHandle)
        {
            throw new NotImplementedException();
        }

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

        internal static IntPtr group_get_table(GroupHandle groupHandle, string name)
        {
            throw new NotImplementedException();
        }

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

        public static void group_new_file(GroupHandle @GroupHandle, string path, Group.OpenMode openMode)
        {
            throw new NotImplementedException();
        }

        public static void group_frombinary_data(GroupHandle @GroupHandle, byte[] binaryGroup)
        {
            throw new NotImplementedException();
        }

        public static bool group_has_table(GroupHandle @GroupHandle, string tableName)
        {
            throw new NotImplementedException();
        }

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

        public static object group_to_string(GroupHandle @GroupHandle)
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

        [DllImport(InteropConfig.L64, EntryPoint = "new_shared_group_file_defaults", CallingConvention = CallingConvention.Cdecl)]
        private static extern SharedGroupHandle new_shared_group_file_defaults64(
            [MarshalAs(UnmanagedType.LPWStr)] string fileName,
            IntPtr fileNameLen);

        [DllImport(InteropConfig.L32, EntryPoint = "new_shared_group_file_defaults", CallingConvention = CallingConvention.Cdecl)]
        private static extern SharedGroupHandle new_shared_group_file_defaults32(
            [MarshalAs(UnmanagedType.LPWStr)] string fileName,
            IntPtr fileNameLen);

        public static SharedGroupHandle new_shared_group_file_defaults(string filename)
        {
            if (InteropConfig.Is64Bit)
                return new_shared_group_file_defaults64(filename, (IntPtr)filename.Length);
            else
                return new_shared_group_file_defaults32(filename, (IntPtr)filename.Length);
        }

        public static bool table_is_attached(TableHandle TableHandle)
        {
            throw new NotImplementedException();
        }

        public static void new_shared_group_file(SharedGroupHandle sharedGroup, string fileName, bool noCreate, DurabilityLevel durabilityLevel)
        {
            throw new NotImplementedException();
        }

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

        public static long table_get_column_index(TableHandle TableHandle, string name)
        {
            throw new NotImplementedException();
        }

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

        public static long table_get_int(TableHandle TableHandle, long columnIndex, long rowIndex)
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

        public static void table_set_long(TableHandle TableHandle, long columnIndex, long rowIndex, long value)
        {
            throw new NotImplementedException();
        }

        public static void table_set_int(TableHandle TableHandle, long columnIndex, long rowIndex, int value)
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