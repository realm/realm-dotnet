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
using System.Threading.Tasks;

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

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_get_path", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_path(SessionHandle session, IntPtr buffer, IntPtr buffer_length, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_get_from_path", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_from_path([MarshalAs(UnmanagedType.LPWStr)] string path, IntPtr path_len, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_get_raw_pointer", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_raw_pointer(SessionHandle session);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr handle);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_register_progress_notifier", CallingConvention = CallingConvention.Cdecl)]
            public static extern ulong register_progress_notifier(SessionHandle session,
                                                                  IntPtr token_ptr,
                                                                  ProgressDirection direction,
                                                                  [MarshalAs(UnmanagedType.I1)] bool is_streaming,
                                                                  out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_unregister_progress_notifier", CallingConvention = CallingConvention.Cdecl)]
            public static extern void unregister_progress_notifier(SessionHandle session, ulong token, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_wait", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern void wait(SessionHandle session, IntPtr task_completion_source, ProgressDirection direction, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_report_error_for_testing", CallingConvention = CallingConvention.Cdecl)]
            public static extern void report_error_for_testing(SessionHandle session, int error_code, [MarshalAs(UnmanagedType.LPWStr)] string message, IntPtr message_len, [MarshalAs(UnmanagedType.I1)] bool is_fatal);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_stop", CallingConvention = CallingConvention.Cdecl)]
            public static extern void stop(SessionHandle session, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_start", CallingConvention = CallingConvention.Cdecl)]
            public static extern void start(SessionHandle session, out NativeException ex);
        }

        static SessionHandle()
        {
            NativeCommon.Initialize();
        }

        [Preserve]
        public SessionHandle(IntPtr handle) : base(null, handle)
        {
        }

        public bool TryGetUser(out SyncUserHandle handle)
        {
            var ptr = NativeMethods.get_user(this);
            if (ptr == IntPtr.Zero)
            {
                handle = null;
                return false;
            }

            handle = new SyncUserHandle(ptr);
            return true;
        }

        public string GetServerUri()
        {
            return MarshalHelpers.GetString((IntPtr buffer, IntPtr length, out bool isNull, out NativeException ex) =>
            {
                isNull = false;
                return NativeMethods.get_uri(this, buffer, length, out ex);
            });
        }

        public SessionState GetState()
        {
            var state = NativeMethods.get_state(this, out var ex);
            ex.ThrowIfNecessary();
            return state;
        }

        public string GetPath()
        {
            return MarshalHelpers.GetString((IntPtr buffer, IntPtr length, out bool isNull, out NativeException ex) =>
            {
                isNull = false;
                return NativeMethods.get_path(this, buffer, length, out ex);
            });
        }

        public void RefreshAccessToken(string accessToken, string serverPath)
        {
            NativeMethods.refresh_access_token(this, accessToken, (IntPtr)accessToken.Length, serverPath, (IntPtr)serverPath.Length, out var ex);
            ex.ThrowIfNecessary();
        }

        public ulong RegisterProgressNotifier(IntPtr tokenPtr, ProgressDirection direction, ProgressMode mode)
        {
            var isStreaming = mode == ProgressMode.ReportIndefinitely;
            var token = NativeMethods.register_progress_notifier(this, tokenPtr, direction, isStreaming, out var ex);
            ex.ThrowIfNecessary();
            return token;
        }

        public void UnregisterProgressNotifier(ulong token)
        {
            NativeMethods.unregister_progress_notifier(this, token, out var ex);
            ex.ThrowIfNecessary();
        }

        public void Wait(TaskCompletionSource<object> tcs, ProgressDirection direction)
        {
            var tcsHandle = GCHandle.Alloc(tcs);
            NativeMethods.wait(this, GCHandle.ToIntPtr(tcsHandle), direction, out var ex);
            ex.ThrowIfNecessary();
        }

        public IntPtr GetRawPointer()
        {
            return NativeMethods.get_raw_pointer(this);
        }

        public void ReportErrorForTesting(int errorCode, string errorMessage, bool isFatal)
        {
            NativeMethods.report_error_for_testing(this, errorCode, errorMessage, (IntPtr)errorMessage.Length, isFatal);
        }

        public void Stop()
        {
            NativeMethods.stop(this, out var ex);
            ex.ThrowIfNecessary();
        }

        public void Start()
        {
            NativeMethods.start(this, out var ex);
            ex.ThrowIfNecessary();
        }

        public static SessionHandle GetSessionForPath(string path)
        {
            var ptr = NativeMethods.get_from_path(path, (IntPtr)path.Length, out var ex);
            ex.ThrowIfNecessary();
            return new SessionHandle(ptr);
        }

        protected override void Unbind()
        {
            NativeMethods.destroy(handle);
        }
    }
}
