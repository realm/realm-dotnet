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
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
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
                                                      SyncError error,
                                                      IntPtr managed_sync_config_handle);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void SessionProgressCallback(double progressEstimate);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void SessionWaitCallback(IntPtr task_completion_source, int error_code, PrimitiveValue message);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void SessionPropertyChangedCallback(IntPtr managed_session, NotifiableProperty property);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate IntPtr NotifyBeforeClientReset(IntPtr before_frozen, IntPtr managed_sync_config_handle);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate IntPtr NotifyAfterClientReset(IntPtr before_frozen, IntPtr after, IntPtr managed_sync_config_handle, bool did_recover);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_install_callbacks", CallingConvention = CallingConvention.Cdecl)]
            public static extern void install_syncsession_callbacks(SessionErrorCallback error_callback,
                                                                    SessionProgressCallback progress_callback,
                                                                    SessionWaitCallback wait_callback,
                                                                    SessionPropertyChangedCallback property_changed_callback,
                                                                    NotifyBeforeClientReset notify_before,
                                                                    NotifyAfterClientReset notify_after);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_get_user", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_user(SessionHandle session);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_get_state", CallingConvention = CallingConvention.Cdecl)]
            public static extern SessionState get_state(SessionHandle session, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_get_connection_state", CallingConvention = CallingConvention.Cdecl)]
            public static extern ConnectionState get_connection_state(SessionHandle session, out NativeException ex);

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

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_register_property_changed_callback", CallingConvention = CallingConvention.Cdecl)]
            public static extern SessionNotificationToken register_property_changed_callback(SessionHandle session, IntPtr managed_session_handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_unregister_property_changed_callback", CallingConvention = CallingConvention.Cdecl)]
            public static extern void unregister_property_changed_callback(IntPtr session, SessionNotificationToken token, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_wait", CallingConvention = CallingConvention.Cdecl)]
            public static extern void wait(SessionHandle session, IntPtr task_completion_source, ProgressDirection direction, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_report_error_for_testing", CallingConvention = CallingConvention.Cdecl)]
            public static extern void report_error_for_testing(SessionHandle session, int error_code, [MarshalAs(UnmanagedType.LPWStr)] string message, IntPtr message_len, [MarshalAs(UnmanagedType.U1)] bool is_fatal, int action);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_stop", CallingConvention = CallingConvention.Cdecl)]
            public static extern void stop(SessionHandle session, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_shutdown_and_wait", CallingConvention = CallingConvention.Cdecl)]
            public static extern void shutdown_and_wait(SessionHandle session, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncsession_start", CallingConvention = CallingConvention.Cdecl)]
            public static extern void start(SessionHandle session, out NativeException ex);
        }

        private SessionNotificationToken? _notificationToken;

        public override bool ForceRootOwnership => true;

        [Preserve]
        public SessionHandle(SharedRealmHandle? root, IntPtr handle) : base(root, handle)
        {
        }

        public static void Initialize()
        {
            NativeMethods.SessionErrorCallback error = HandleSessionError;
            NativeMethods.SessionProgressCallback progress = HandleSessionProgress;
            NativeMethods.SessionWaitCallback wait = HandleSessionWaitCallback;
            NativeMethods.SessionPropertyChangedCallback propertyChanged = HandleSessionPropertyChangedCallback;
            NativeMethods.NotifyBeforeClientReset beforeReset = NotifyBeforeClientReset;
            NativeMethods.NotifyAfterClientReset afterReset = NotifyAfterClientReset;

            GCHandle.Alloc(error);
            GCHandle.Alloc(progress);
            GCHandle.Alloc(wait);
            GCHandle.Alloc(propertyChanged);
            GCHandle.Alloc(beforeReset);
            GCHandle.Alloc(afterReset);

            NativeMethods.install_syncsession_callbacks(error, progress, wait, propertyChanged, beforeReset, afterReset);
        }

        public SyncUserHandle GetUser()
        {
            var ptr = NativeMethods.get_user(this);
            if (ptr == IntPtr.Zero)
            {
                throw new RealmException("Unable to obtain user for session. This likely means the session is being torn down.");
            }

            return new(ptr);
        }

        public SessionState GetState()
        {
            var state = NativeMethods.get_state(this, out var ex);
            ex.ThrowIfNecessary();
            return state;
        }

        public ConnectionState GetConnectionState()
        {
            var connectionState = NativeMethods.get_connection_state(this, out var ex);
            ex.ThrowIfNecessary();
            return connectionState;
        }

        public string GetPath()
        {
            return MarshalHelpers.GetString((IntPtr buffer, IntPtr length, out bool isNull, out NativeException ex) =>
            {
                isNull = false;
                return NativeMethods.get_path(this, buffer, length, out ex);
            })!;
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

        public void SubscribeNotifications(Session session)
        {
            Debug.Assert(!_notificationToken.HasValue, $"{nameof(_notificationToken)} must be null before subscribing.");

            var managedSessionHandle = GCHandle.Alloc(session, GCHandleType.Weak);
            var sessionPointer = GCHandle.ToIntPtr(managedSessionHandle);
            _notificationToken = NativeMethods.register_property_changed_callback(this, sessionPointer, out var ex);
            ex.ThrowIfNecessary();
        }

        public void UnsubscribeNotifications()
        {
            if (_notificationToken.HasValue)
            {
                // This needs to use the handle directly because it's being called in Unbind. At this point the SafeHandle is closed, which means we'll
                // get an error if we attempted to marshal it to native. The raw pointer is fine though and we can use it.
                NativeMethods.unregister_property_changed_callback(handle, _notificationToken.Value, out var ex);
                _notificationToken = null;
                ex.ThrowIfNecessary();
            }
        }

        public Task WaitAsync(ProgressDirection direction, CancellationToken? cancellationToken)
        {
            var tcs = new TaskCompletionSource();
            if (cancellationToken?.IsCancellationRequested == true)
            {
                tcs.TrySetCanceled(cancellationToken.Value);
                return tcs.Task;
            }

            // The tcsHandles is freed in HandleSessionWaitCallback. It's important that we don't free it on cancellation
            // as the cancellation doesn't really cancel the native wait operation. That will eventually complete and it needs
            // to have the GCHandle at this point, otherwise we'll get a hard crash on Mono.
            var tcsHandle = GCHandle.Alloc(tcs);

            cancellationToken?.Register(() => tcs.TrySetCanceled(cancellationToken.Value));

            try
            {
                NativeMethods.wait(this, GCHandle.ToIntPtr(tcsHandle), direction, out var ex);
                ex.ThrowIfNecessary();
            }
            catch
            {
                // If we failed to register a waiter, we can free the handle as we won't get a native callback here anyway
                tcsHandle.Free();
                throw;
            }

            return tcs.Task;
        }

        public IntPtr GetRawPointer()
        {
            return NativeMethods.get_raw_pointer(this);
        }

        public void ReportErrorForTesting(int errorCode, string errorMessage, bool isFatal, ServerRequestsAction action)
        {
            NativeMethods.report_error_for_testing(this, errorCode, errorMessage, (IntPtr)errorMessage.Length, isFatal, (int)action);
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

        public override void Unbind()
        {
            UnsubscribeNotifications();
            NativeMethods.destroy(handle);
        }

        [MonoPInvokeCallback(typeof(NativeMethods.SessionErrorCallback))]
        private static void HandleSessionError(IntPtr sessionHandlePtr, SyncError error, IntPtr managedSyncConfigurationBaseHandle)
        {
            try
            {
                // Filter out end of input, which the client seems to have started reporting
                if (error.error_code == (ErrorCode)1)
                {
                    return;
                }

                using var handle = new SessionHandle(null, sessionHandlePtr);
                var session = new Session(handle);
                string messageString = error.message!;
                var syncConfigHandle = GCHandle.FromIntPtr(managedSyncConfigurationBaseHandle);
                var syncConfig = (SyncConfigurationBase)syncConfigHandle.Target!;

                if (error.is_client_reset)
                {
                    var userInfo = error.user_info_pairs.ToEnumerable().ToDictionary(kvp => (string)kvp.Key!, kvp => (string?)kvp.Value);
                    var clientResetEx = new ClientResetException(session.User.App, messageString, error.error_code, userInfo);

                    syncConfig.ClientResetHandler.ManualClientReset?.Invoke(clientResetEx);
                    return;
                }

                SessionException exception;
                if (error.error_code == ErrorCode.CompensatingWrite)
                {
                    var compensatingWrites = error.compensating_writes
                        .ToEnumerable()
                        .Select(c => new CompensatingWriteInfo(c.object_name!, c.reason!, new RealmValue(c.primary_key)))
                        .ToArray();
                    exception = new CompensatingWriteException(messageString, compensatingWrites);
                }
                else
                {
                    exception = new SessionException(messageString, error.error_code);
                }

                exception.HelpLink = error.log_url;
                syncConfig.OnSessionError?.Invoke(session, exception);
            }
            catch (Exception ex)
            {
                Logger.Default.Log(LogLevel.Warn, $"An error has occurred while handling a session error: {ex}");
            }
        }

        [MonoPInvokeCallback(typeof(NativeMethods.NotifyBeforeClientReset))]
        private static IntPtr NotifyBeforeClientReset(IntPtr beforeFrozen, IntPtr managedSyncConfigurationHandle)
        {
            SyncConfigurationBase? syncConfig = null;

            try
            {
                var syncConfigHandle = GCHandle.FromIntPtr(managedSyncConfigurationHandle);
                syncConfig = (SyncConfigurationBase)syncConfigHandle.Target!;

                var cb = syncConfig.ClientResetHandler switch
                {
                    DiscardUnsyncedChangesHandler handler => handler.OnBeforeReset,
                    RecoverUnsyncedChangesHandler handler => handler.OnBeforeReset,
                    RecoverOrDiscardUnsyncedChangesHandler handler => handler.OnBeforeReset,
                    _ => throw new NotSupportedException($"ClientResetHandlerBase of type {syncConfig.ClientResetHandler.GetType()} is not handled yet")
                };

                if (cb != null)
                {
                    var schema = syncConfig.Schema;
                    using var realmBefore = new Realm(new UnownedRealmHandle(beforeFrozen), syncConfig, schema);
                    cb.Invoke(realmBefore);
                }

                return IntPtr.Zero;
            }
            catch (Exception ex)
            {
                var handlerType = syncConfig is null ? "ClientResetHandler" : syncConfig.ClientResetHandler.GetType().Name;
                Logger.Default.Log(LogLevel.Error, $"An error has occurred while executing {handlerType}.OnBeforeReset during a client reset: {ex}");

                var exHandle = GCHandle.Alloc(ex);
                return GCHandle.ToIntPtr(exHandle);
            }
        }

        [MonoPInvokeCallback(typeof(NativeMethods.NotifyAfterClientReset))]
        private static IntPtr NotifyAfterClientReset(IntPtr beforeFrozen, IntPtr after, IntPtr managedSyncConfigurationHandle, bool didRecover)
        {
            SyncConfigurationBase? syncConfig = null;

            try
            {
                var syncConfigHandle = GCHandle.FromIntPtr(managedSyncConfigurationHandle);
                syncConfig = (SyncConfigurationBase)syncConfigHandle.Target!;

                var cb = syncConfig.ClientResetHandler switch
                {
                    DiscardUnsyncedChangesHandler handler => handler.OnAfterReset,
                    RecoverUnsyncedChangesHandler handler => handler.OnAfterReset,
                    RecoverOrDiscardUnsyncedChangesHandler handler => didRecover ? handler.OnAfterRecovery : handler.OnAfterDiscard,
                    _ => throw new NotSupportedException($"ClientResetHandlerBase of type {syncConfig.ClientResetHandler.GetType()} is not handled yet")
                };

                if (cb != null)
                {
                    var schema = syncConfig.Schema;
                    using var realmBefore = new Realm(new UnownedRealmHandle(beforeFrozen), syncConfig, schema);
                    using var realmAfter = new Realm(new UnownedRealmHandle(after), syncConfig, schema);
                    cb.Invoke(realmBefore, realmAfter);
                }

                return IntPtr.Zero;
            }
            catch (Exception ex)
            {
                var handlerType = syncConfig is null ? "ClientResetHandler" : syncConfig.ClientResetHandler.GetType().Name;
                Logger.Default.Log(LogLevel.Error, $"An error has occurred while executing {handlerType}.OnAfterReset during a client reset: {ex}");

                var exHandle = GCHandle.Alloc(ex);
                return GCHandle.ToIntPtr(exHandle);
            }
        }

        [MonoPInvokeCallback(typeof(NativeMethods.SessionProgressCallback))]
        private static void HandleSessionProgress(IntPtr tokenPtr, double progressEstimate)
        {
            var token = (ProgressNotificationToken?)GCHandle.FromIntPtr(tokenPtr).Target;
            token?.Notify(progressEstimate);
        }

        [MonoPInvokeCallback(typeof(NativeMethods.SessionWaitCallback))]
        private static void HandleSessionWaitCallback(IntPtr taskCompletionSource, int error_code, PrimitiveValue message)
        {
            var handle = GCHandle.FromIntPtr(taskCompletionSource);
            var tcs = (TaskCompletionSource)handle.Target!;

            if (error_code == 0)
            {
                tcs.TrySetResult();
            }
            else
            {
                var inner = new SessionException(message.AsString(), (ErrorCode)error_code);
                const string OuterMessage = "A system error occurred while waiting for completion. See InnerException for more details";
                tcs.TrySetException(new RealmException(OuterMessage, inner));
            }

            handle.Free();
        }

        [MonoPInvokeCallback(typeof(NativeMethods.SessionPropertyChangedCallback))]
        private static void HandleSessionPropertyChangedCallback(IntPtr managedSessionHandle, NotifiableProperty property)
        {
            try
            {
                if (managedSessionHandle == IntPtr.Zero)
                {
                    return;
                }

                var propertyName = property switch
                {
                    NotifiableProperty.ConnectionState => nameof(Session.ConnectionState),
                    _ => throw new NotSupportedException($"Unexpected notifiable property value: {property}")
                };
                var session = (Session?)GCHandle.FromIntPtr(managedSessionHandle).Target;
                if (session is null)
                {
                    // We're taking a weak handle to the session, so it's possible that it's been collected
                    return;
                }

                ThreadPool.QueueUserWorkItem(_ =>
                {
                    session.RaisePropertyChanged(propertyName);
                });
            }
            catch (Exception ex)
            {
                Logger.Default.Log(LogLevel.Error, $"An error has occurred while raising a property changed event: {ex}");
            }
        }

        private enum NotifiableProperty : byte
        {
            ConnectionState = 0
        }
    }
}
