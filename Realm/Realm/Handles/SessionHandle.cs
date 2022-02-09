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
using Realms.Exceptions;
using Realms.Exceptions.Sync;
using Realms.Logging;
using Realms.Native;
using Realms.Sync.ErrorHandling;
using Realms.Sync.Exceptions;
using Realms.Sync.Native;

namespace Realms.Sync
{
    internal class SessionHandle : RealmHandle
    {
        private static class NativeMethods
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void SessionErrorCallback(IntPtr session_handle_ptr,
                                                      ErrorCode error_code,
                                                      PrimitiveValue message,
                                                      IntPtr user_info_pairs,
                                                      IntPtr user_info_pairs_len,
                                                      [MarshalAs(UnmanagedType.U1)] bool is_client_reset,
                                                      IntPtr managed_sync_config_handle);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void SessionProgressCallback(IntPtr progress_token_ptr, ulong transferred_bytes, ulong transferable_bytes);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void SessionWaitCallback(IntPtr task_completion_source, int error_code, PrimitiveValue message);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate bool NotifyBeforeClientReset(IntPtr before_frozen, IntPtr managed_sync_config_handle);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate bool NotifyAfterClientReset(IntPtr before_frozen, IntPtr after, IntPtr managed_sync_config_handle);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_install_callbacks", CallingConvention = CallingConvention.Cdecl)]
            public static extern void install_syncsession_callbacks(SessionErrorCallback error_callback, SessionProgressCallback progress_callback, SessionWaitCallback wait_callback, NotifyBeforeClientReset notify_before, NotifyAfterClientReset notify_after);

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
            public static extern void wait(SessionHandle session, IntPtr task_completion_source, ProgressDirection direction, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_report_error_for_testing", CallingConvention = CallingConvention.Cdecl)]
            public static extern void report_error_for_testing(SessionHandle session, int error_code, SessionErrorCategory error_category, [MarshalAs(UnmanagedType.LPWStr)] string message, IntPtr message_len, [MarshalAs(UnmanagedType.U1)] bool is_fatal);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_stop", CallingConvention = CallingConvention.Cdecl)]
            public static extern void stop(SessionHandle session, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_shutdown_and_wait", CallingConvention = CallingConvention.Cdecl)]
            public static extern void shutdown_and_wait(SessionHandle session, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_start", CallingConvention = CallingConvention.Cdecl)]
            public static extern void start(SessionHandle session, out NativeException ex);
        }

        public override bool ForceRootOwnership => true;

        [Preserve]
        public SessionHandle(SharedRealmHandle root, IntPtr handle) : base(root, handle)
        {
        }

        public static void Initialize()
        {
            NativeMethods.SessionErrorCallback error = HandleSessionError;
            NativeMethods.SessionProgressCallback progress = HandleSessionProgress;
            NativeMethods.SessionWaitCallback wait = HandleSessionWaitCallback;
            NativeMethods.NotifyBeforeClientReset beforeReset = NotifyBeforeClientReset;
            NativeMethods.NotifyAfterClientReset afterReset = NotifyAfterClientReset;

            GCHandle.Alloc(error);
            GCHandle.Alloc(progress);
            GCHandle.Alloc(wait);
            GCHandle.Alloc(beforeReset);
            GCHandle.Alloc(afterReset);

            NativeMethods.install_syncsession_callbacks(error, progress, wait, beforeReset, afterReset);
        }

        public bool TryGetUser(out SyncUserHandle userHandle)
        {
            var ptr = NativeMethods.get_user(this);
            if (ptr == IntPtr.Zero)
            {
                userHandle = null;
                return false;
            }

            userHandle = new SyncUserHandle(ptr);
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

        public async Task WaitAsync(ProgressDirection direction)
        {
            var tcs = new TaskCompletionSource<object>();
            var tcsHandle = GCHandle.Alloc(tcs);

            try
            {
                NativeMethods.wait(this, GCHandle.ToIntPtr(tcsHandle), direction, out var ex);
                ex.ThrowIfNecessary();

                await tcs.Task;
            }
            finally
            {
                tcsHandle.Free();
            }
        }

        public IntPtr GetRawPointer()
        {
            return NativeMethods.get_raw_pointer(this);
        }

        public void ReportErrorForTesting(int errorCode, SessionErrorCategory errorCategory, string errorMessage, bool isFatal)
        {
            NativeMethods.report_error_for_testing(this, errorCode, errorCategory, errorMessage, (IntPtr)errorMessage.Length, isFatal);
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

        /// <summary>
        /// Terminates the sync session and releases the Realm file it was using.
        /// </summary>
        public void ShutdownAndWait()
        {
            NativeMethods.shutdown_and_wait(this, out var ex);
            ex.ThrowIfNecessary();
        }

        public override void Unbind() => NativeMethods.destroy(handle);

        [MonoPInvokeCallback(typeof(NativeMethods.SessionErrorCallback))]
        private static void HandleSessionError(IntPtr sessionHandlePtr, ErrorCode errorCode, PrimitiveValue message, IntPtr userInfoPairs, IntPtr userInfoPairsLength, bool isClientReset, IntPtr managedSyncConfigurationBaseHandle)
        {
            try
            {
                using var handle = new SessionHandle(null, sessionHandlePtr);
                var session = new Session(handle);
                var messageString = message.AsString();
                SessionException exception;
                var syncConfigBaseHandle = GCHandle.FromIntPtr(managedSyncConfigurationBaseHandle);
                var syncConfigurationBase = (SyncConfigurationBase)syncConfigBaseHandle.Target;

                if (isClientReset)
                {
                    var userInfo = StringStringPair.UnmarshalDictionary(userInfoPairs, userInfoPairsLength.ToInt32());
                    exception = new ClientResetException(session.User.App, messageString, errorCode, userInfo);

                    if (syncConfigurationBase.ClientResetHandler != null)
                    {
                        switch (syncConfigurationBase.ClientResetHandler)
                        {
                            case DiscardLocalResetHandler conf:
                                conf.ManualResetFallback?.Invoke(session, (ClientResetException)exception);
                                break;
                            case ManualRecoveryHandler conf:
                                conf.OnClientReset?.Invoke(session, (ClientResetException)exception);
                                break;
                            default:
                                throw new Exception($"ClientResetHandler of type {syncConfigurationBase.ClientResetHandler.GetType().Name} is unhandled");
                        }

                        // after deprecation: this won't be require anymore when Session.Error will be fully deprecated
                        return;
                    }
                }
                else if (errorCode == ErrorCode.PermissionDenied)
                {
                    var userInfo = StringStringPair.UnmarshalDictionary(userInfoPairs, userInfoPairsLength.ToInt32());
                    exception = new PermissionDeniedException(session.User.App, messageString, userInfo);
                }
                else
                {
                    exception = new SessionException(messageString, errorCode);
                }

                if (syncConfigurationBase.OnSessionError != null)
                {
                    syncConfigurationBase.OnSessionError.Invoke(session, exception);
                }
                else
                {
                    // after deprecation: this will need to go when Session.Error is fully deprecated
                    Session.RaiseError(session, exception);
                }
            }
            catch (Exception ex)
            {
                Logger.Default.Log(LogLevel.Warn, $"An error has occurred while handling a session error: {ex}");
            }
        }

        [MonoPInvokeCallback(typeof(NativeMethods.NotifyBeforeClientReset))]
        private static bool NotifyBeforeClientReset(IntPtr beforeFrozen, IntPtr managedSyncConfigurationHandle)
        {
            var syncConfigHandle = GCHandle.FromIntPtr(managedSyncConfigurationHandle);
            var syncConfiguration = (SyncConfigurationBase)syncConfigHandle.Target;

            if (syncConfiguration.ClientResetHandler == null)
            {
                return true;
            }

            var discardLocalResetHandler = (DiscardLocalResetHandler)syncConfiguration.ClientResetHandler;
            var schema = syncConfiguration.GetSchema();

            try
            {
                if (discardLocalResetHandler.OnBeforeReset != null)
                {
                    using var realmBefore = new Realm(new UnownedRealmHandle(beforeFrozen), syncConfiguration, schema);
                    discardLocalResetHandler.OnBeforeReset(realmBefore);
                }
            }
            catch (Exception ex)
            {
                Logger.Default.Log(LogLevel.Error, $"An error has occurred while executing SyncConfigurationBase.OnBeforeReset during a client reset: {ex}");
                return false;
            }

            return true;
        }

        [MonoPInvokeCallback(typeof(NativeMethods.NotifyAfterClientReset))]
        private static bool NotifyAfterClientReset(IntPtr beforeFrozen, IntPtr after, IntPtr managedSyncConfigurationHandle)
        {
            var syncConfigHandle = GCHandle.FromIntPtr(managedSyncConfigurationHandle);
            var syncConfiguration = (SyncConfigurationBase)syncConfigHandle.Target;

            // no clientResetHandler means don't do anything for this callback
            if (syncConfiguration.ClientResetHandler == null)
            {
                return true;
            }

            var discardLocalResetHandler = (DiscardLocalResetHandler)syncConfiguration.ClientResetHandler;
            var schema = syncConfiguration.GetSchema();

            try
            {
                if (discardLocalResetHandler.OnAfterReset != null)
                {
                    using var realmBefore = new Realm(new UnownedRealmHandle(beforeFrozen), syncConfiguration, schema);
                    using var realmAfter = new Realm(new UnownedRealmHandle(after), syncConfiguration, schema);
                    discardLocalResetHandler.OnAfterReset(realmBefore, realmAfter);
                }
            }
            catch (Exception ex)
            {
                Logger.Default.Log(LogLevel.Error, $"An error has occurred while executing SyncConfigurationBase.OnBeforeReset during a client reset: {ex}");
                return false;
            }

            return true;
        }

        [MonoPInvokeCallback(typeof(NativeMethods.SessionProgressCallback))]
        private static void HandleSessionProgress(IntPtr tokenPtr, ulong transferredBytes, ulong transferableBytes)
        {
            var token = (ProgressNotificationToken)GCHandle.FromIntPtr(tokenPtr).Target;
            token.Notify(transferredBytes, transferableBytes);
        }

        [MonoPInvokeCallback(typeof(NativeMethods.SessionWaitCallback))]
        private static void HandleSessionWaitCallback(IntPtr taskCompletionSource, int error_code, PrimitiveValue message)
        {
            var handle = GCHandle.FromIntPtr(taskCompletionSource);
            var tcs = (TaskCompletionSource<object>)handle.Target;

            if (error_code == 0)
            {
                tcs.TrySetResult(null);
            }
            else
            {
                var inner = new SessionException(message.AsString(), (ErrorCode)error_code);
                const string OuterMessage = "A system error occurred while waiting for completion. See InnerException for more details";
                tcs.TrySetException(new RealmException(OuterMessage, inner));
            }
        }
    }
}
