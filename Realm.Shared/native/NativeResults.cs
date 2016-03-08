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
        public static extern IntPtr isSameInternalResults(ResultsHandle lhs, ResultsHandle rhs);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "results_destroy", CallingConvention = CallingConvention.Cdecl)]
        public static extern void destroy(IntPtr resultsHandle);
    }
}
