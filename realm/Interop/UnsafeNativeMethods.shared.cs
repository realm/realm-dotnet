using System;
using System.Runtime.InteropServices;
using Interop.Config;

// TODO: Replace this with CriticalHandle
using TableHandle = System.IntPtr;
using QueryHandle = System.IntPtr;

public static class UnsafeNativeMethods
{
    private static DataType IntPtrToDataType(IntPtr value)
    {
        return (DataType) value;
    }

    private static IntPtr DataTypeToIntPtr(DataType value)
    {
        return (IntPtr) value;
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

    [DllImport(InteropConfig.L64, EntryPoint="realm_get_wrapper_ver", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr GetWrapperVer();

    [DllImport(InteropConfig.L64, EntryPoint="realm_get_ver_minor", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GetMinorVer();

    #region helpers

    private static IntPtr StrAllocateBuffer(out long currentBufferSizeChars, long bufferSizeNeededChars)
    {
        currentBufferSizeChars = bufferSizeNeededChars;
        return Marshal.AllocHGlobal((IntPtr) (bufferSizeNeededChars*sizeof (char)));
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
        return value ? (IntPtr) 1 : (IntPtr) 0;
    }

    private static Boolean IntPtrToBool(IntPtr value)
    {
        return (IntPtr) 1 == value;
    }

    #endregion

    #region public static TableHandle new_table()

    //tightdb_c_cs_API size_t new_table()
    //The TableHandle will be initialized with root=null and that is exactly what we want
    //when we ask for a freestanding table
    //in the call to new_table we atomically acquire a TableHandle that can finalize itself if need be
    [DllImport(InteropConfig.L64, EntryPoint = "new_table", CallingConvention = CallingConvention.Cdecl)]
    private static extern TableHandle new_table64();

    [DllImport(InteropConfig.L32, EntryPoint = "new_table", CallingConvention = CallingConvention.Cdecl)]
    private static extern TableHandle new_table32();

    public static TableHandle new_table()
    {
        return InteropConfig.Is64Bit ? new_table64() : new_table32();
    }

    #endregion

    #region public static long table_add_column(TableHandle tableHandle, DataType type, string name)

    [DllImport(InteropConfig.L64, EntryPoint = "table_add_column", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr table_add_column64(TableHandle tableHandle, IntPtr type,
        [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr nameLen);

    [DllImport(InteropConfig.L32, EntryPoint = "table_add_column", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr table_add_column32(TableHandle tableHandle, IntPtr type,
        [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr nameLen);

    public static long table_add_column(TableHandle tableHandle, DataType type, string name)
    {
        if (InteropConfig.Is64Bit)
            return (long)table_add_column64(tableHandle, DataTypeToIntPtr(type), name, (IntPtr)name.Length);
        else
            return (long)table_add_column32(tableHandle, DataTypeToIntPtr(type), name, (IntPtr)name.Length);
    }

    #endregion

    #region public static long table_add_empty_row(TableHandle tableHandle, long numberOfRows)

    [DllImport(InteropConfig.L64, EntryPoint = "table_add_empty_row", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr table_add_empty_row64(TableHandle tableHandle, IntPtr numRows);

    [DllImport(InteropConfig.L32, EntryPoint = "table_add_empty_row", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr table_add_empty_row32(TableHandle tableHandle, IntPtr numRows);

    public static long table_add_empty_row(TableHandle tableHandle, long numberOfRows)
    {
        if (InteropConfig.Is64Bit)
            return (long)table_add_empty_row64(tableHandle, (IntPtr)numberOfRows);
        else
            return (long)table_add_empty_row32(tableHandle, (IntPtr)numberOfRows);
    }

    #endregion

    #region public static void table_set_string(TableHandle tableHandle, long columnIndex, long rowIndex, string value)

    //        TIGHTDB_C_CS_API void table_set_int(Table* TablePtr, size_t column_ndx, size_t row_ndx, int64_t value)
    [DllImport(InteropConfig.L64, EntryPoint = "table_set_string", CallingConvention = CallingConvention.Cdecl)]
    private static extern void table_set_string64(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
        [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

    [DllImport(InteropConfig.L32, EntryPoint = "table_set_string", CallingConvention = CallingConvention.Cdecl)]
    private static extern void table_set_string32(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
        [MarshalAs(UnmanagedType.LPWStr)] string value, IntPtr valueLen);

    public static void table_set_string(TableHandle tableHandle, long columnIndex, long rowIndex, string value)
    {
        if (InteropConfig.Is64Bit)
            table_set_string64(tableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, value, (IntPtr)value.Length);
        else
            table_set_string32(tableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, value, (IntPtr)value.Length);
    }

    #endregion

    #region public static string table_get_string(TableHandle tableHandle, long columnIndex, long rowIndex)

    [DllImport(InteropConfig.L64, EntryPoint = "table_get_string", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr table_get_string64(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex,
        IntPtr buffer, IntPtr bufsize);

    [DllImport(InteropConfig.L32, EntryPoint = "table_get_string", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr table_get_string32(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex,
        IntPtr buffer, IntPtr bufsize);
    
    public static string table_get_string(TableHandle tableHandle, long columnIndex, long rowIndex)
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
                        (IntPtr) currentBufferSizeChars);

            else
                bufferSizeNeededChars =
                    (long)
                    table_get_string32(tableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, buffer,
                        (IntPtr) currentBufferSizeChars);

        } while (StrBufferOverflow(buffer, currentBufferSizeChars, bufferSizeNeededChars));
        return StrBufToStr(buffer, (int) bufferSizeNeededChars);
    }

    #endregion

    #region public static void table_set_bool(TableHandle tableHandle, long columnIndex, long rowIndex, bool value)

    [DllImport(InteropConfig.L64, EntryPoint = "table_set_bool", CallingConvention = CallingConvention.Cdecl)]
    private static extern void table_set_bool64(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx, IntPtr value);

    [DllImport(InteropConfig.L32, EntryPoint = "table_set_bool", CallingConvention = CallingConvention.Cdecl)]
    private static extern void table_set_bool32(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx, IntPtr value);

    public static void table_set_bool(TableHandle tableHandle, long columnIndex, long rowIndex, bool value)
    {
        var marshalledValue = BoolToIntPtr(value);

        if (InteropConfig.Is64Bit)
            table_set_bool64(tableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, marshalledValue);
        else
            table_set_bool32(tableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex, marshalledValue);
    }

    #endregion

    #region public static string table_get_bool(TableHandle tableHandle, long columnIndex, long rowIndex)

    [DllImport(InteropConfig.L64, EntryPoint = "table_get_bool", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr table_get_bool64(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex);

    [DllImport(InteropConfig.L32, EntryPoint = "table_get_bool", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr table_get_bool32(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex);
    
    public static bool table_get_bool(TableHandle tableHandle, long columnIndex, long rowIndex)
    {
        if (InteropConfig.Is64Bit)
            return IntPtrToBool(table_get_bool64(tableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex));
        else
            return IntPtrToBool(table_get_bool64(tableHandle, (IntPtr)columnIndex, (IntPtr)rowIndex));
    }

    #endregion

#region public static QueryHandle table_where(TableHandle tableHandle)

    [DllImport(InteropConfig.L64, EntryPoint = "table_where", CallingConvention = CallingConvention.Cdecl)]
    private static extern QueryHandle table_where64(TableHandle handle);

    [DllImport(InteropConfig.L32, EntryPoint = "table_where", CallingConvention = CallingConvention.Cdecl)]
    private static extern QueryHandle table_where32(TableHandle handle);

    public static QueryHandle table_where(TableHandle tableHandle)
    {
        if (InteropConfig.Is64Bit)
            return table_where64(tableHandle);
        else
            return table_where32(tableHandle);
    }

#endregion

#region public static void query_bool_equal(QueryHandle queryHandle, long columnIndex, bool value)

    //in tightdb c++ this function returns q again, the query object is re-used and keeps its pointer.
    //so high-level stuff should also return self, to enable stacking of operations query.dothis().dothat()
    [DllImport(InteropConfig.L64, EntryPoint = "query_bool_equal", CallingConvention = CallingConvention.Cdecl)]
    private static extern void query_bool_equal64(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

    [DllImport(InteropConfig.L32, EntryPoint = "query_bool_equal", CallingConvention = CallingConvention.Cdecl)]
    private static extern void query_bool_equal32(QueryHandle queryPtr, IntPtr columnIndex, IntPtr value);

    public static void query_bool_equal(QueryHandle queryHandle, long columnIndex, bool value)
    {
        var ipValue = BoolToIntPtr(value);
        if (InteropConfig.Is64Bit)
            query_bool_equal64(queryHandle, (IntPtr)columnIndex, ipValue);
        else
            query_bool_equal32(queryHandle, (IntPtr)columnIndex, ipValue);
    }

#endregion

#region public static long QueryFind(Query query, long lastMatch)

    [DllImport(InteropConfig.L64, EntryPoint = "query_find", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr query_find64(QueryHandle queryHandle, IntPtr lastMatch);

    [DllImport(InteropConfig.L32, EntryPoint = "query_find", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr query_find32(QueryHandle queryHandle, IntPtr lastMatch);

    public static long query_find(QueryHandle queryHandle, long lastMatch)
    {
        if (InteropConfig.Is64Bit)
            return (long)query_find64(queryHandle, (IntPtr)lastMatch);
        else
            return (long)query_find32(queryHandle, (IntPtr)lastMatch);
    }

#endregion

}
