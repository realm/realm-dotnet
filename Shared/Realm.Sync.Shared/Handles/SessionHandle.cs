﻿////////////////////////////////////////////////////////////////////////////
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
    internal class SessionHandle : RealmHandle
    {
        private static class NativeMethods
        {
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_refresh_access_token", CallingConvention = CallingConvention.Cdecl)]
            public static extern void refresh_access_token(SessionHandle session,
                                                           [MarshalAs(UnmanagedType.LPWStr)] string access_token, IntPtr access_token_len,
                                                           [MarshalAs(UnmanagedType.LPWStr)] string server_path, IntPtr server_path_len,
                                                           out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr handle);
        }

        public void RefreshAccessToken(string accessToken, string serverPath)
        {
            NativeException ex;
            NativeMethods.refresh_access_token(this, accessToken, (IntPtr)accessToken.Length, serverPath, (IntPtr)serverPath.Length, out ex);
            ex.ThrowIfNecessary();
        }

        protected override void Unbind()
        {
            NativeMethods.destroy(handle);
        }
    }
}
