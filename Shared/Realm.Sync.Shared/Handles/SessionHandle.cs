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
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Realms.Sync
{
    internal class SessionHandle : RealmHandle
    {
        private static class NativeMethods
        {
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_refresh_access_token", CallingConvention = CallingConvention.Cdecl)]
            public static extern void refresh_access_token(SessionHandle session,
                                                           [MarshalAs(UnmanagedType.LPWStr)] string access_token, IntPtr access_token_len,
                                                           [MarshalAs(UnmanagedType.LPWStr)] string server_path, IntPtr server_path_len,
                                                           out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_get_user", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_user(SessionHandle session);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_get_uri", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_uri(SessionHandle session, IntPtr buffer, IntPtr buffer_length, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_get_state", CallingConvention = CallingConvention.Cdecl)]
            public static extern SessionState get_state(SessionHandle session, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_get_from_realm", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_from_realm(SharedRealmHandle realm, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_get_raw_pointer", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_raw_pointer(SessionHandle session);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr handle);
        }

        public SyncUserHandle User
        {
            get
            {
                var handle = new SyncUserHandle();
                var ptr = NativeMethods.get_user(this);
                handle.SetHandle(ptr);
                return handle;
            }
        }

        public string ServerUri
        {
            get
            {
                return MarshalHelpers.GetString((IntPtr buffer, IntPtr length, out bool isNull, out NativeException ex) =>
                {
                    isNull = false;
                    return NativeMethods.get_uri(this, buffer, length, out ex);
                });
            }
        }

        public SessionState State
        {
            get
            {
                NativeException ex;
                var state = NativeMethods.get_state(this, out ex);
                ex.ThrowIfNecessary();
                return state;
            }
        }

        public void RefreshAccessToken(string accessToken, string serverPath)
        {
            NativeException ex;
            NativeMethods.refresh_access_token(this, accessToken, (IntPtr)accessToken.Length, serverPath, (IntPtr)serverPath.Length, out ex);
            ex.ThrowIfNecessary();
        }

        public static IntPtr SessionForRealm(SharedRealmHandle realm)
        {
            NativeException ex;
            var ptr = NativeMethods.get_from_realm(realm, out ex);
            ex.ThrowIfNecessary();
            return ptr;
        }

        protected override void Unbind()
        {
            NativeMethods.destroy(handle);
        }

        internal class Comparer : IEqualityComparer<SessionHandle>
        {
            public bool Equals(SessionHandle x, SessionHandle y)
            {
                return NativeMethods.get_raw_pointer(x) == NativeMethods.get_raw_pointer(y);
            }

            public int GetHashCode(SessionHandle obj)
            {
                return NativeMethods.get_raw_pointer(obj).GetHashCode();
            }
        }
    }
}
