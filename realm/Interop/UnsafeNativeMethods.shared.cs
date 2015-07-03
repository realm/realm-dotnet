using System;
using System.Runtime.InteropServices;
using Interop.Config;

namespace RealmNet.Interop
{

public static class UnsafeNativeMethods
{
    private static DataType IntPtrToDataType(IntPtr value)
    {
        return (DataType)value;
    }

    private static IntPtr DataTypeToIntPtr(DataType value)
    {
        return (IntPtr)value;
    }

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

    //        TIGHTDB_C_CS_API void table_set_int(Table* TablePtr, size_t column_ndx, size_t row_ndx, int64_t value)
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

    #endregion

    #region internal static void query_bool_equal(QueryHandle queryHandle, long columnIndex, bool value)

    //in tightdb c++ this function returns q again, the query object is re-used and keeps its pointer.
    //so high-level stuff should also return self, to enable stacking of operations query.dothis().dothat()
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

    //in tightdb c++ this function returns q again, the query object is re-used and keeps its pointer.
    //so high-level stuff should also return self, to enable stacking of operations query.dothis().dothat()
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

    #region internal static long QueryFind(Query query, long lastMatch)

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

    internal static void table_unbind(RealmNet.Interop.TableHandle tableHandle)
    {
        throw new NotImplementedException();
    }

    internal static IntPtr table_copy_table(RealmNet.Interop.TableHandle tableHandle)
    {
        throw new NotImplementedException();
    }

    internal static IntPtr table_get_sub_table(RealmNet.Interop.TableHandle tableHandle, long columnIndex, long rowIndex)
    {
        throw new NotImplementedException();
    }

    internal static IntPtr table_distinct(RealmNet.Interop.TableHandle tableHandle, long columnIndex)
    {
        throw new NotImplementedException();
    }

    internal static IntPtr table_find_all_int(RealmNet.Interop.TableHandle tableHandle, long columnIndex, long value)
    {
        throw new NotImplementedException();
    }

    internal static IntPtr table_find_all_bool(RealmNet.Interop.TableHandle tableHandle, long columnIndex, bool value)
    {
        throw new NotImplementedException();
    }

    internal static IntPtr table_find_all_date_time(RealmNet.Interop.TableHandle tableHandle, long columnIndex, DateTime value)
    {
        throw new NotImplementedException();
    }

    internal static IntPtr table_find_all_float(RealmNet.Interop.TableHandle tableHandle, long columnIndex, float value)
    {
        throw new NotImplementedException();
    }

    internal static IntPtr table_find_all_double(RealmNet.Interop.TableHandle tableHandle, long columnIndex, double value)
    {
        throw new NotImplementedException();
    }

    internal static IntPtr table_find_all_string(RealmNet.Interop.TableHandle tableHandle, long columnIndex, string value)
    {
        throw new NotImplementedException();
    }

    internal static IntPtr table_find_all_empty_binary(RealmNet.Interop.TableHandle tableHandle, long columnIndex)
    {
        throw new NotImplementedException();
    }

    internal static IntPtr table_find_all_binary(RealmNet.Interop.TableHandle tableHandle, long columnIndex, IntPtr valuePointer, IntPtr length)
    {
        throw new NotImplementedException();
    }

    internal static IntPtr table_get_spec(RealmNet.Interop.TableHandle tableHandle)
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

    internal static void shared_group_delete(IntPtr handle)
    {
        throw new NotImplementedException();
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

    internal static IntPtr spec_get_spec(SpecHandle specHandle, long columnIndex)
    {
        throw new NotImplementedException();
    }

    internal static void spec_deallocate(SpecHandle specHandle)
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
}
}