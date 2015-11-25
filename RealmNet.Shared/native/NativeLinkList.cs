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

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "linklist_insert",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern void insert(LinkListHandle linklistHandle, IntPtr link_ndx, IntPtr row_ndx);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "linklist_erase",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern void erase(LinkListHandle linklistHandle, IntPtr row_ndx);
        
        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "linklist_clear",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern void clear(LinkListHandle linklistHandle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "linklist_get",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr get(LinkListHandle linklistHandle, IntPtr link_ndx);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "linklist_find",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr find(LinkListHandle linklistHandle, IntPtr link_ndx, IntPtr start_from);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "linklist_size",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr size(LinkListHandle linklistHandle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "linklist_destroy", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void destroy(IntPtr linklistInternalHandle);
    }
}
