using System;
using System.Runtime.InteropServices;
using Interop.Config;

namespace RealmNet
{
    internal static class NativeSharedGroup
    {
        //todo:add return value to rollback if c++ threw an exception
        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_group_rollback", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr rollback(SharedGroupHandle handle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_group_end_read", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr end_read(SharedGroupHandle handle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_group_delete", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void delete(IntPtr handle);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_group_commit", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr commit(SharedGroupHandle sharedGroupHandle);

        //this is complicated.
        //The call to begin_read must result in us always having two things inside a sharedgroup
        //handle : the shared group pointer, AND the shared group transaction state set to InReadTransaction

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_group_begin_read", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr begin_read(SharedGroupHandle sharedGroupPtr);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_group_begin_write", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr begin_write(SharedGroupHandle sharedGroupPtr);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "new_shared_group_file_defaults", CallingConvention = CallingConvention.Cdecl)]
        internal static extern SharedGroupHandle new_shared_group_file_defaults(
            [MarshalAs(UnmanagedType.LPWStr)] string fileName,
            IntPtr fileNameLen);

        [DllImport(InteropConfig.DLL_NAME, EntryPoint = "new_shared_group_file", CallingConvention = CallingConvention.Cdecl)]
        internal static extern SharedGroupHandle new_shared_group_file(
            [MarshalAs(UnmanagedType.LPWStr)] string fileName,
            IntPtr fileNameLen, IntPtr noCreate, IntPtr durabilityLevel);
	}
}