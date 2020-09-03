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
using System.Linq;
using System.Runtime.InteropServices;

namespace Realms.Sync
{
    internal class SyncUserHandle : RealmHandle
    {
        private static class NativeMethods
        {
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1121 // Use built-in type alias

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncuser_get_identity", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_identity(SyncUserHandle user, IntPtr buffer, IntPtr buffer_length, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncuser_get_refresh_token", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_refresh_token(SyncUserHandle user, IntPtr buffer, IntPtr buffer_length, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncuser_get_state", CallingConvention = CallingConvention.Cdecl)]
            public static extern UserState get_state(SyncUserHandle user, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncuser_log_out", CallingConvention = CallingConvention.Cdecl)]
            public static extern void log_out(SyncUserHandle user, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_get_current_sync_user", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_current_user(out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_get_logged_in_users", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_logged_in_users([Out] IntPtr[] users, IntPtr bufsize, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncuser_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr syncuserHandle);

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore SA1121 // Use built-in type alias
        }

        static SyncUserHandle()
        {
            NativeCommon.Initialize();
        }

        [Preserve]
        public SyncUserHandle(IntPtr handle) : base(null, handle)
        {
        }

        public string GetIdentity()
        {
            return MarshalHelpers.GetString((IntPtr buffer, IntPtr length, out bool isNull, out NativeException ex) =>
            {
                isNull = false;
                return NativeMethods.get_identity(this, buffer, length, out ex);
            });
        }

        public string GetRefreshToken()
        {
            return MarshalHelpers.GetString((IntPtr buffer, IntPtr length, out bool isNull, out NativeException ex) =>
            {
                isNull = false;
                return NativeMethods.get_refresh_token(this, buffer, length, out ex);
            });
        }

        public UserState GetState()
        {
            var result = NativeMethods.get_state(this, out var ex);
            ex.ThrowIfNecessary();
            return result;
        }

        public void LogOut()
        {
            NativeMethods.log_out(this, out var ex);
            ex.ThrowIfNecessary();
        }

        public static bool TryGetCurrentUser(out SyncUserHandle handle)
        {
            var userPtr = NativeMethods.get_current_user(out var ex);
            ex.ThrowIfNecessary();

            if (userPtr == IntPtr.Zero)
            {
                handle = null;
                return false;
            }

            handle = new SyncUserHandle(userPtr);
            return true;
        }

        public static IEnumerable<SyncUserHandle> GetAllLoggedInUsers()
        {
            return MarshalHelpers.GetCollection<IntPtr>(NativeMethods.get_logged_in_users, bufferSize: 8)
                                 .Select(h => new SyncUserHandle(h));
        }

        protected override void Unbind()
        {
            NativeMethods.destroy(handle);
        }
    }
}
