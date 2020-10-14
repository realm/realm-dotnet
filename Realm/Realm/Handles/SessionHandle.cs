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
using System.Text;
using System.Threading.Tasks;
using Realms.Exceptions;
using Realms.Native;
using Realms.Sync.Exceptions;
using Realms.Sync.Native;

namespace Realms.Sync
{
    internal class SessionHandle : RealmHandle
    {
        private static class NativeMethods
        {
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1121 // Use built-in type alias
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public unsafe delegate void SessionErrorCallback(IntPtr session_handle_ptr, ErrorCode error_code, byte* message_buf, IntPtr message_len, IntPtr user_info_pairs, int user_info_pairs_len, [MarshalAs(UnmanagedType.U1)] bool is_client_reset);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void SessionProgressCallback(IntPtr progress_token_ptr, ulong transferred_bytes, ulong transferable_bytes);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public unsafe delegate void SessionWaitCallback(IntPtr task_completion_source, int error_code, byte* message_buf, IntPtr message_len);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_install_callbacks", CallingConvention = CallingConvention.Cdecl)]
            public static extern void install_syncsession_callbacks(SessionErrorCallback error_callback, SessionProgressCallback progress_callback, SessionWaitCallback wait_callback);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_get_user", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_user(SessionHandle session);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_get_state", CallingConvention = CallingConvention.Cdecl)]
            public static extern SessionState get_state(SessionHandle session, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_get_path", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_path(SessionHandle session, IntPtr buffer, IntPtr buffer_length, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_get_raw_pointer", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_raw_pointer(SessionHandle session);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_destroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void destroy(IntPtr handle);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_register_progress_notifier", CallingConvention = CallingConvention.Cdecl)]
            public static extern ulong register_progress_notifier(SessionHandle session,
                                                                  IntPtr token_ptr,
                                                                  ProgressDirection direction,
                                                                  [MarshalAs(UnmanagedType.U1)] bool is_streaming,
                                                                  out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_unregister_progress_notifier", CallingConvention = CallingConvention.Cdecl)]
            public static extern void unregister_progress_notifier(SessionHandle session, ulong token, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_wait", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern void wait(SessionHandle session, IntPtr task_completion_source, ProgressDirection direction, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_report_error_for_testing", CallingConvention = CallingConvention.Cdecl)]
            public static extern void report_error_for_testing(SessionHandle session, int error_code, [MarshalAs(UnmanagedType.LPWStr)] string message, IntPtr message_len, [MarshalAs(UnmanagedType.U1)] bool is_fatal);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_stop", CallingConvention = CallingConvention.Cdecl)]
            public static extern void stop(SessionHandle session, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_start", CallingConvention = CallingConvention.Cdecl)]
            public static extern void start(SessionHandle session, out NativeException ex);

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore SA1121 // Use built-in type alias
        }

        static SessionHandle()
        {
            NativeCommon.Initialize();
        }

        [Preserve]
        public SessionHandle(IntPtr handle) : base(null, handle)
        {
        }

        public static unsafe void InstallCallbacks()
        {
            NativeMethods.SessionErrorCallback error = HandleSessionError;
            NativeMethods.SessionProgressCallback progress = HandleSessionProgress;
            NativeMethods.SessionWaitCallback wait = HandleSessionWaitCallback;

            GCHandle.Alloc(error);
            GCHandle.Alloc(progress);
            GCHandle.Alloc(wait);

            NativeMethods.install_syncsession_callbacks(error, progress, wait);
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

        public ulong RegisterProgressNotifier(GCHandle managedHandle, ProgressDirection direction, ProgressMode mode)
        {
            var isStreaming = mode == ProgressMode.ReportIndefinitely;
            var token = NativeMethods.register_progress_notifier(this, GCHandle.ToIntPtr(managedHandle), direction, isStreaming, out var ex);
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

        protected override void Unbind()
        {
            NativeMethods.destroy(handle);
        }

        [MonoPInvokeCallback(typeof(NativeMethods.SessionErrorCallback))]
        private static unsafe void HandleSessionError(IntPtr sessionHandlePtr, ErrorCode errorCode, byte* messageBuffer, IntPtr messageLength, IntPtr userInfoPairs, int userInfoPairsLength, bool isClientReset)
        {
            var handle = new SessionHandle(sessionHandlePtr);
            var session = new Session(handle);
            var message = Encoding.UTF8.GetString(messageBuffer, (int)messageLength);

            SessionException exception;

            if (isClientReset)
            {
                var userInfo = StringStringPair.UnmarshalDictionary(userInfoPairs, userInfoPairsLength);
                exception = new ClientResetException(session.User.App, message, userInfo);
            }
            else if (errorCode == ErrorCode.PermissionDenied)
            {
                var userInfo = StringStringPair.UnmarshalDictionary(userInfoPairs, userInfoPairsLength);
                exception = new PermissionDeniedException(session.User.App, message, userInfo);
            }
            else
            {
                exception = new SessionException(message, errorCode);
            }

            Session.RaiseError(session, exception);
        }

        [MonoPInvokeCallback(typeof(NativeMethods.SessionProgressCallback))]
        private static void HandleSessionProgress(IntPtr tokenPtr, ulong transferredBytes, ulong transferableBytes)
        {
            var token = (ProgressNotificationToken)GCHandle.FromIntPtr(tokenPtr).Target;
            token.Notify(transferredBytes, transferableBytes);
        }

        [MonoPInvokeCallback(typeof(NativeMethods.SessionWaitCallback))]
        private static unsafe void HandleSessionWaitCallback(IntPtr taskCompletionSource, int error_code, byte* messageBuffer, IntPtr messageLength)
        {
            var handle = GCHandle.FromIntPtr(taskCompletionSource);
            var tcs = (TaskCompletionSource<object>)handle.Target;

            try
            {
                if (error_code == 0)
                {
                    tcs.TrySetResult(null);
                }
                else
                {
                    var inner = new SessionException(Encoding.UTF8.GetString(messageBuffer, (int)messageLength), (ErrorCode)error_code);
                    const string OuterMessage = "A system error occurred while waiting for completion. See InnerException for more details";
                    tcs.TrySetException(new RealmException(OuterMessage, inner));
                }
            }
            finally
            {
                handle.Free();
            }
        }
    }
}
