using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Realms.Sync.Native
{
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter")]
    internal struct SyncConfiguration
    {
        private IntPtr sync_user_ptr;

        internal SyncUserHandle SyncUserHandle
        {
            set
            {
                sync_user_ptr = value.DangerousGetHandle();
            }
        }

        [MarshalAs(UnmanagedType.LPWStr)]
        private string url;
        private IntPtr url_len;

        internal string Url
        {
            set
            {
                url = value;
                url_len = (IntPtr)value.Length;
            }
        }
    }
}
