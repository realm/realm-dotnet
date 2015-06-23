using System;
using System.Runtime.InteropServices;
using Interop.Config;

// TODO: Replace this with CriticalHandle
using TableHandle = System.IntPtr;

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

    #region new_table

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

    #region table_add_column

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

    #region table_add_empty_row

    [DllImport(InteropConfig.L64, EntryPoint = "table_add_empty_row", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr table_add_empty_row64(TableHandle tableHandle, IntPtr numRows);

    [DllImport(InteropConfig.L32, EntryPoint = "table_add_empty_row", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr table_add_empty_row32(TableHandle tableHandle, IntPtr numRows);

    public static long table_add_empty_row32(TableHandle tableHandle, long numberOfRows)
    {
        if (InteropConfig.Is64Bit)
            return (long)table_add_empty_row64(tableHandle, (IntPtr)numberOfRows);
        else
            return (long)table_add_empty_row32(tableHandle, (IntPtr)numberOfRows);
    }

    #endregion
}