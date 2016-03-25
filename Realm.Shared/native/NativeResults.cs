/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;
using System.Runtime.InteropServices;

namespace Realms
{
    internal static class NativeResults
    {
        
        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_is_same_internal_results",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr is_same_internal_results(ResultsHandle lhs, ResultsHandle rhs);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_create_for_table", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr create_for_table(SharedRealmHandle sharedRealm, TableHandle handle, IntPtr objectSchema);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_create_for_query", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr create_for_query(SharedRealmHandle sharedRealm, QueryHandle queryPtr, IntPtr objectSchema);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_create_for_query", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr create_for_query_sorted(SharedRealmHandle sharedRealm, QueryHandle queryPtr, IntPtr objectSchema, SortOrderHandle sortOrderHandle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_destroy", CallingConvention = CallingConvention.Cdecl)]
        public static extern void destroy(IntPtr resultsHandle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_get_row", CallingConvention = CallingConvention.Cdecl)]
        internal static extern RowHandle get_row(ResultsHandle results, IntPtr index);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_clear", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void clear(ResultsHandle results);
    }
}
