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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Realms.Exceptions;
using Realms.Native;
using Realms.Schema;
using Realms.Sync.Exceptions;
using Realms.Sync.Native;

namespace Realms.Sync
{
    internal static class SharedRealmHandleExtensions
    {
        // This is int, because Interlocked.Exchange cannot work with narrower types such as bool.
        private static int _fileSystemConfigured;

        private static class NativeMethods
        {
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_open_with_sync", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr open_with_sync(Configuration configuration, Native.SyncConfiguration sync_configuration,
                [MarshalAs(UnmanagedType.LPArray), In] SchemaObject[] objects, int objects_length,
                [MarshalAs(UnmanagedType.LPArray), In] SchemaProperty[] properties,
                byte[] encryptionKey,
                out NativeException ex);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void RefreshAccessTokenCallbackDelegate(IntPtr session_handle_ptr);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public unsafe delegate void SessionErrorCallback(IntPtr session_handle_ptr, ErrorCode error_code, byte* message_buf, IntPtr message_len, IntPtr user_info_pairs, int user_info_pairs_len);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void SessionProgressCallback(IntPtr progress_token_ptr, ulong transferred_bytes, ulong transferable_bytes);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public unsafe delegate void SessionWaitCallback(IntPtr task_completion_source, int error_code, byte* message_buf, IntPtr message_len);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public unsafe delegate void LogMessageCallback(byte* message_buf, IntPtr message_len, LogLevel logLevel);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncmanager_configure", CallingConvention = CallingConvention.Cdecl)]
            public static extern unsafe void configure([MarshalAs(UnmanagedType.LPWStr)] string base_path, IntPtr base_path_length,
                                                       [MarshalAs(UnmanagedType.LPWStr)] string user_agent, IntPtr user_agent_length,
                                                       UserPersistenceMode* userPersistence, byte[] encryptionKey,
                                                       [MarshalAs(UnmanagedType.I1)] bool resetMetadataOnError,
                                                       out NativeException exception);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_install_syncsession_callbacks", CallingConvention = CallingConvention.Cdecl)]
            public static extern void install_syncsession_callbacks(RefreshAccessTokenCallbackDelegate refresh_callback, SessionErrorCallback error_callback, SessionProgressCallback progress_callback, SessionWaitCallback wait_callback);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncmanager_get_path_for_realm", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_path_for_realm(SyncUserHandle user, [MarshalAs(UnmanagedType.LPWStr)] string url, IntPtr url_len, IntPtr buffer, IntPtr bufsize, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncmanager_immediately_run_file_actions", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool immediately_run_file_actions([MarshalAs(UnmanagedType.LPWStr)] string path, IntPtr path_len, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncmanager_cancel_pending_file_actions", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool cancel_pending_file_actions([MarshalAs(UnmanagedType.LPWStr)] string path, IntPtr path_len, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncmanager_reconnect", CallingConvention = CallingConvention.Cdecl)]
            public static extern void reconnect();

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncmanager_get_session", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_session([MarshalAs(UnmanagedType.LPWStr)] string path, IntPtr path_len, Native.SyncConfiguration configuration, byte[] encryptionKey, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncmanager_set_log_level", CallingConvention = CallingConvention.Cdecl)]
            public static extern unsafe void set_log_level(LogLevel* level, out NativeException exception);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncmanager_set_user_agent", CallingConvention = CallingConvention.Cdecl)]
            public static extern unsafe void set_user_agent([MarshalAs(UnmanagedType.LPWStr)] string user_agent, IntPtr user_agent_length, out NativeException exception);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncmanager_get_log_level", CallingConvention = CallingConvention.Cdecl)]
            public static extern LogLevel get_log_level();

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncmanager_set_log_callback", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_log_callback(LogMessageCallback callback, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncmanager_get_realm_privileges", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern byte get_realm_privileges(SharedRealmHandle handle, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncmanager_get_class_privileges", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern byte get_class_privileges(SharedRealmHandle handle,
                                                           [MarshalAs(UnmanagedType.LPWStr)] string class_name, IntPtr class_name_len,
                                                           out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncmanager_get_object_privileges", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern byte get_object_privileges(SharedRealmHandle handle, ObjectHandle objectHandle, out NativeException ex);
        }

        static unsafe SharedRealmHandleExtensions()
        {
            NativeCommon.Initialize();

            NativeMethods.RefreshAccessTokenCallbackDelegate refresh = RefreshAccessTokenCallback;
            NativeMethods.SessionErrorCallback error = HandleSessionError;
            NativeMethods.SessionProgressCallback progress = HandleSessionProgress;
            NativeMethods.SessionWaitCallback wait = HandleSessionWaitCallback;

            GCHandle.Alloc(refresh);
            GCHandle.Alloc(error);
            GCHandle.Alloc(progress);
            GCHandle.Alloc(wait);

            NativeMethods.install_syncsession_callbacks(refresh, error, progress, wait);
        }

        public static SharedRealmHandle OpenWithSync(Configuration configuration, Native.SyncConfiguration syncConfiguration, RealmSchema schema, byte[] encryptionKey)
        {
            DoInitialFileSystemConfiguration();

            var marshaledSchema = new SharedRealmHandle.SchemaMarshaler(schema);

            var result = NativeMethods.open_with_sync(configuration, syncConfiguration, marshaledSchema.Objects, marshaledSchema.Objects.Length, marshaledSchema.Properties, encryptionKey, out var nativeException);
            nativeException.ThrowIfNecessary();

            return new SharedRealmHandle(result);
        }

        public static string GetRealmPath(User user, Uri serverUri)
        {
            DoInitialFileSystemConfiguration();
            return MarshalHelpers.GetString((IntPtr buffer, IntPtr bufferLength, out bool isNull, out NativeException ex) =>
            {
                isNull = false;
                return NativeMethods.get_path_for_realm(user.Handle, serverUri.AbsoluteUri, (IntPtr)serverUri.AbsoluteUri.Length, buffer, bufferLength, out ex);
            });
        }

        // Configure the SyncMetadataManager with default values if it hasn't been configured already
        public static void DoInitialFileSystemConfiguration()
        {
            if (Interlocked.Exchange(ref _fileSystemConfigured, 1) == 0)
            {
                Configure(null, null, false);
            }
        }

        public static unsafe void Configure(UserPersistenceMode? userPersistenceMode, byte[] encryptionKey, bool resetMetadataOnError, string basePath = null)
        {
            // mark the file system as configured in case this is called directly
            // so that it isn't reconfigured with default values in DoInitialFileSystemConfiguration
            Interlocked.Exchange(ref _fileSystemConfigured, 1);

            RealmException.AddOverrider(RealmExceptionCodes.RealmIncompatibleSyncedFile, (message, path) => new IncompatibleSyncedFileException(message, path));

            if (basePath == null)
            {
                basePath = InteropConfig.DefaultStorageFolder;
            }

            UserPersistenceMode mode;
            UserPersistenceMode* modePtr = null;
            if (userPersistenceMode.HasValue)
            {
                mode = userPersistenceMode.Value;
                modePtr = &mode;
            }

            var userAgent = SyncConfigurationBase.GetSDKUserAgent();
            NativeMethods.configure(
                basePath, (IntPtr)basePath.Length,
                userAgent, (IntPtr)userAgent.Length,
                modePtr, encryptionKey, resetMetadataOnError, out var ex);
            ex.ThrowIfNecessary();
        }

        public static unsafe void SetLogLevel(LogLevel level)
        {
            var levelPtr = &level;

            NativeMethods.set_log_level(levelPtr, out var ex);
            ex.ThrowIfNecessary();
        }

        public static void SetUserAgent(string userAgent)
        {
            NativeMethods.set_user_agent(userAgent, (IntPtr)userAgent.Length, out var ex);
            ex.ThrowIfNecessary();
        }

        public static LogLevel GetLogLevel()
        {
            return NativeMethods.get_log_level();
        }

        public static unsafe void InstallLogCallback()
        {
            NativeMethods.LogMessageCallback nativeCallback = HandleLogMessage;
            GCHandle.Alloc(nativeCallback);
            NativeMethods.set_log_callback(nativeCallback, out var ex);
            ex.ThrowIfNecessary();
        }

        public static void ResetForTesting(UserPersistenceMode? userPersistenceMode = null)
        {
            NativeCommon.reset_for_testing();
            Configure(userPersistenceMode, null, false);
        }

        public static bool ImmediatelyRunFileActions(string path)
        {
            var result = NativeMethods.immediately_run_file_actions(path, (IntPtr)path.Length, out var ex);
            ex.ThrowIfNecessary();

            return result;
        }

        public static bool CancelPendingFileActions(string path)
        {
            var result = NativeMethods.cancel_pending_file_actions(path, (IntPtr)path.Length, out var ex);
            ex.ThrowIfNecessary();

            return result;
        }

        public static void ReconnectSessions()
        {
            NativeMethods.reconnect();
        }

        public static SessionHandle GetSession(string path, Native.SyncConfiguration configuration, byte[] encryptionKey)
        {
            DoInitialFileSystemConfiguration();

            var result = NativeMethods.get_session(path, (IntPtr)path.Length, configuration, encryptionKey, out var nativeException);
            nativeException.ThrowIfNecessary();

            return new SessionHandle(result);
        }

        public static RealmPrivileges GetPrivileges(this SharedRealmHandle handle)
        {
            var result = NativeMethods.get_realm_privileges(handle, out var ex);
            ex.ThrowIfNecessary();
            return (RealmPrivileges)result;
        }

        public static ClassPrivileges GetPrivileges(this SharedRealmHandle handle, string className)
        {
            var result = NativeMethods.get_class_privileges(handle, className, (IntPtr)className.Length, out var ex);
            ex.ThrowIfNecessary();
            return (ClassPrivileges)result;
        }

        public static ObjectPrivileges GetPrivileges(this SharedRealmHandle handle, ObjectHandle objectHandle)
        {
            var result = NativeMethods.get_object_privileges(handle, objectHandle, out var ex);
            ex.ThrowIfNecessary();
            return (ObjectPrivileges)result;
        }

        [NativeCallback(typeof(NativeMethods.RefreshAccessTokenCallbackDelegate))]
        private static void RefreshAccessTokenCallback(IntPtr sessionHandlePtr)
        {
            var handle = new SessionHandle(sessionHandlePtr);
            var session = new Session(handle);
            AuthenticationHelper.RefreshAccessTokenAsync(session).ContinueWith(_ => session.CloseHandle());
        }

        [NativeCallback(typeof(NativeMethods.SessionErrorCallback))]
        private static unsafe void HandleSessionError(IntPtr sessionHandlePtr, ErrorCode errorCode, byte* messageBuffer, IntPtr messageLength, IntPtr userInfoPairs, int userInfoPairsLength)
        {
            var handle = new SessionHandle(sessionHandlePtr);
            var session = new Session(handle);
            var message = Encoding.UTF8.GetString(messageBuffer, (int)messageLength);

            SessionException exception;

            if (errorCode.IsClientResetError())
            {
                var userInfo = MarshalErrorUserInfo(userInfoPairs, userInfoPairsLength);
                exception = new ClientResetException(message, userInfo);
            }
            else if (errorCode == ErrorCode.PermissionDenied)
            {
                var userInfo = MarshalErrorUserInfo(userInfoPairs, userInfoPairsLength);
                exception = new PermissionDeniedException(message, userInfo);
            }
            else
            {
                exception = new SessionException(message, errorCode);
            }

            Session.RaiseError(session, exception);
        }

        private static Dictionary<string, string> MarshalErrorUserInfo(IntPtr userInfoPairs, int userInfoPairsLength)
        {
            return Enumerable.Range(0, userInfoPairsLength)
                             .Select(i => Marshal.PtrToStructure<StringStringPair>(IntPtr.Add(userInfoPairs, i * StringStringPair.Size)))
                             .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        [NativeCallback(typeof(NativeMethods.SessionProgressCallback))]
        private static void HandleSessionProgress(IntPtr tokenPtr, ulong transferredBytes, ulong transferableBytes)
        {
            var token = (SyncProgressObservable.ProgressNotificationToken)GCHandle.FromIntPtr(tokenPtr).Target;
            token.Notify(transferredBytes, transferableBytes);
        }

        [NativeCallback(typeof(NativeMethods.SessionWaitCallback))]
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
                    const string outerMessage = "A system error occurred while waiting for completion. See InnerException for more details";
                    tcs.TrySetException(new RealmException(outerMessage, inner));
                }
            }
            finally
            {
                handle.Free();
            }
        }

        [NativeCallback(typeof(NativeMethods.LogMessageCallback))]
        private static unsafe void HandleLogMessage(byte* messageBuffer, IntPtr messageLength, LogLevel level)
        {
            // TODO: pass to user-defined callback
            var message = Encoding.UTF8.GetString(messageBuffer, (int)messageLength);
            Console.WriteLine($"[{level}] {message}");
        }
    }
}