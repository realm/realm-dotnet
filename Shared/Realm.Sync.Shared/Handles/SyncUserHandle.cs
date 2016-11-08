////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;

namespace Realms.Sync
{
    internal class SyncUserHandle : RealmHandle
    {
        private static class NativeMethods
        {
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_get_sync_user", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_sync_user([MarshalAs(UnmanagedType.LPWStr)] string identity, IntPtr identity_len,
                                                      [MarshalAs(UnmanagedType.LPWStr)] string refresh_token, IntPtr refresh_token_len,
                                                      [MarshalAs(UnmanagedType.LPWStr)] string auth_server_url, IntPtr auth_server_url_len,
                                                      [MarshalAs(UnmanagedType.I1)] bool is_admin, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncuser_get_identity", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_identity(SyncUserHandle user, IntPtr buffer, IntPtr buffer_length, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncuser_get_refresh_token", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_refresh_token(SyncUserHandle user, IntPtr buffer, IntPtr buffer_length, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncuser_get_server_url", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_server_url(SyncUserHandle user, IntPtr buffer, IntPtr buffer_length, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncuser_get_state", CallingConvention = CallingConvention.Cdecl)]
            public static extern UserState get_state(SyncUserHandle user, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncuser_log_out", CallingConvention = CallingConvention.Cdecl)]
            public static extern void log_out(SyncUserHandle user, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncuser_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr syncuserHandle);
        }

        public string Identity
        {
            get
            {
                return MarshalHelpers.GetString((IntPtr buffer, IntPtr length, out bool isNull, out NativeException ex) =>
                {
                    isNull = false;
                    return NativeMethods.get_identity(this, buffer, length, out ex);
                });
            }
        }

        public string RefreshToken
        {
            get
            {
                return MarshalHelpers.GetString((IntPtr buffer, IntPtr length, out bool isNull, out NativeException ex) =>
                {
                    isNull = false;
                    return NativeMethods.get_refresh_token(this, buffer, length, out ex);
                });
            }
        }

        public string ServerUrl
        {
            get
            {
                return MarshalHelpers.GetString((IntPtr buffer, IntPtr length, out bool isNull, out NativeException ex) =>
                {
                    isNull = false;
                    return NativeMethods.get_server_url(this, buffer, length, out ex);
                });
            }
        }

        public UserState State
        {
            get
            {
                NativeException ex;
                var result = NativeMethods.get_state(this, out ex);
                ex.ThrowIfNecessary();
                return result;
            }
        }

        public void LogOut()
        {
            NativeException ex;
            NativeMethods.log_out(this, out ex);
            ex.ThrowIfNecessary();
        }

        public static SyncUserHandle GetSyncUser(string identity, string refreshToken, string authServerUrl, bool isAdmin)
        {
            NativeException ex;
            var userPtr = NativeMethods.get_sync_user(identity, (IntPtr)identity.Length,
                                                      refreshToken, (IntPtr)refreshToken.Length,
                                                      authServerUrl, (IntPtr)authServerUrl?.Length,
                                                      isAdmin, out ex);
            ex.ThrowIfNecessary();

            var handle = new SyncUserHandle();
            handle.SetHandle(userPtr);
            return handle;
        }

        protected override void Unbind()
        {
            NativeMethods.destroy(handle);
        }
    }
}
