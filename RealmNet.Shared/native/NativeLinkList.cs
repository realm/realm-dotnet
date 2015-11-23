/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;
using System.Runtime.InteropServices;

namespace RealmNet
{
    internal static class NativeLinkList
    {
        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "linklist_add",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern void add(LinkListHandle linklistHandle, IntPtr row_ndx);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "linklist_size",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr size(LinkListHandle linklistHandle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "linklist_destroy", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void destroy(IntPtr linklistInternalHandle);
    }
}
