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

            public unsafe delegate void RefreshAccessTokenCallbackDelegate(IntPtr session_handle_ptr);

            public unsafe delegate void SessionErrorCallback(IntPtr session_handle_ptr, ErrorCode error_code, sbyte* message_buf, IntPtr message_len, IntPtr user_info_pairs, int user_info_pairs_len);

            public unsafe delegate void SessionProgressCallback(IntPtr progress_token_ptr, ulong transferred_bytes, ulong transferable_bytes);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncmanager_configure_file_system", CallingConvention = CallingConvention.Cdecl)]
            public static extern unsafe void configure_file_system([MarshalAs(UnmanagedType.LPWStr)] string base_path, IntPtr base_path_leth, 
                                                                   UserPersistenceMode* userPersistence, byte[] encryptionKey,
                                                                   [MarshalAs(UnmanagedType.I1)] bool resetMetadataOnError,
                                                                   out NativeException exception);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_install_syncsession_callbacks", CallingConvention = CallingConvention.Cdecl)]
            public static extern unsafe void install_syncsession_callbacks(RefreshAccessTokenCallbackDelegate refresh_callback, SessionErrorCallback error_callback, SessionProgressCallback progress_callback);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncmanager_get_path_for_realm", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_path_for_realm(SyncUserHandle user, [MarshalAs(UnmanagedType.LPWStr)] string url, IntPtr url_len, IntPtr buffer, IntPtr bufsize, out NativeException ex);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncmanager_immediately_run_file_actions", CallingConvention = CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool immediately_run_file_actions([MarshalAs(UnmanagedType.LPWStr)] string path, IntPtr path_len, out NativeException ex);
        }

        static unsafe SharedRealmHandleExtensions()
        {
            NativeMethods.install_syncsession_callbacks(RefreshAccessTokenCallback, HandleSessionError, HandleSessionProgress);
        }

        public static SharedRealmHandle OpenWithSync(Configuration configuration, Native.SyncConfiguration syncConfiguration, RealmSchema schema, byte[] encryptionKey)
        {
            DoInitialFileSystemConfiguration();

            var marshaledSchema = new SharedRealmHandle.SchemaMarshaler(schema);

            NativeException nativeException;
            var result = NativeMethods.open_with_sync(configuration, syncConfiguration, marshaledSchema.Objects, marshaledSchema.Objects.Length, marshaledSchema.Properties, encryptionKey, out nativeException);
            nativeException.ThrowIfNecessary();

            var handle = new SharedRealmHandle();
            handle.SetHandle(result);
            return handle;
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
                ConfigureFileSystem(null, null, false);
            }
        }

        public static unsafe void ConfigureFileSystem(UserPersistenceMode? userPersistenceMode, byte[] encryptionKey, bool resetMetadataOnError)
        {
            // mark the file system as configured in case this is called directly
            // so that it isn't reconfigured with default values in DoInitialFileSystemConfiguration
            Interlocked.Exchange(ref _fileSystemConfigured, 1);

            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            UserPersistenceMode mode;
            UserPersistenceMode* modePtr = null;
            if (userPersistenceMode.HasValue)
            {
                mode = userPersistenceMode.Value;
                modePtr = &mode;
            }

            NativeException ex;
            NativeMethods.configure_file_system(basePath, (IntPtr)basePath.Length, modePtr, encryptionKey, resetMetadataOnError, out ex);
            ex.ThrowIfNecessary();
        }

        public static void ResetForTesting(UserPersistenceMode? userPersistenceMode = null)
        {
            NativeCommon.reset_for_testing();
            ConfigureFileSystem(userPersistenceMode, null, false);
        }

        public static bool ImmediatelyRunFileActions(string path)
        {
            NativeException ex;
            var result = NativeMethods.immediately_run_file_actions(path, (IntPtr)path.Length, out ex);
            ex.ThrowIfNecessary();

            return result;
        }

        [NativeCallback(typeof(NativeMethods.RefreshAccessTokenCallbackDelegate))]
        private static unsafe void RefreshAccessTokenCallback(IntPtr sessionHandlePtr)
        {
            var session = Session.Create(sessionHandlePtr);
            AuthenticationHelper.RefreshAccessToken(session).ContinueWith(_ => session.Handle.Dispose());
        }

        [NativeCallback(typeof(NativeMethods.SessionErrorCallback))]
        private static unsafe void HandleSessionError(IntPtr sessionHandlePtr, ErrorCode errorCode, sbyte* messageBuffer, IntPtr messageLength, IntPtr userInfoPairs, int userInfoPairsLength)
        {
            var session = Session.Create(sessionHandlePtr);
            var message = new string(messageBuffer, 0, (int)messageLength, Encoding.UTF8);

            SessionException exception;

            if (errorCode.IsClientResetError())
            {
                var userInfo = MarshalErrorUserInfo(userInfoPairs, userInfoPairsLength);
                exception = new ClientResetException(message, userInfo);
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
    }
}
