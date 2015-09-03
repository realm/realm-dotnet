using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Interop.Config;
using InteropShared;

namespace RealmNet.Interop
{
    internal static class NativeGroup
    {
        internal static void delete(GroupHandle groupHandle)
        {
            throw new NotImplementedException("NativeGroup.delete");
            //TODO FIX THIS AND COMMENT IN GroupHandle.shared.cs Unbind
        }
        

        internal static IntPtr get_table_by_index(GroupHandle groupHandle, long tableIndex)
        {
            throw new NotImplementedException("NativeGroup.get_table_by_index");
            //TODO FIX THIS AND COMMENT IN GroupHandle.shared.cs GetTable
        }


        public static string to_string(GroupHandle groupHandle)
        {
            throw new NotImplementedException("NativeGroup.to_string");
            //TODO FIX THIS AND COMMENT IN GroupHandle.shared.cs ToString
        }


        //If the name exists in the group, the table associated with the name is returned
        //if the name does not exist in the group, a new table is created and returned
        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "group_get_or_add_table", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr get_or_add_table(GroupHandle groupHandle,
            [MarshalAs(UnmanagedType.LPWStr)] String tableName, IntPtr tableNameLen);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "group_has_table", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr has_table(GroupHandle groupHandle,
            [MarshalAs(UnmanagedType.LPWStr)] String tableName, IntPtr tableNameLen);

	}
}