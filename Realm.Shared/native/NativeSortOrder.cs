/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;
using System.Runtime.InteropServices;

namespace Realms
{
    internal static class NativeSortOrder
    {

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "sortorder_create_for_table", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr create_for_table(TableHandle handle);
        
        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "sortorder_destroy", CallingConvention = CallingConvention.Cdecl)]
        public static extern void destroy(IntPtr sortHandle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "sortorder_add_clause", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void add_sort_clause(SortOrderHandle sortOrderHandle,
            [MarshalAs(UnmanagedType.LPWStr)] String columnName, IntPtr columnNameLen, IntPtr ascending);
        
    }
}
