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
using System.Threading;
using Realms;

namespace Realms.Sync
{
    internal static class SharedRealmHandleExtensions
    {
        // This is int, because Interlocked.Exchange cannot work with narrower types such as bool.
        private static int _fileSystemConfigured = 0;

        private static class NativeMethods
        {
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_open_with_sync", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr open_with_sync(Realms.Native.Configuration configuration, Native.SyncConfiguration sync_configuration,
                [MarshalAs(UnmanagedType.LPArray), In] Realms.Native.SchemaObject[] objects, int objects_length,
                [MarshalAs(UnmanagedType.LPArray), In] Realms.Native.SchemaProperty[] properties,
                byte[] encryptionKey,
                out NativeException ex);

            public unsafe delegate void RefreshAccessTokenCallbackDelegate(IntPtr user_handle_ptr, IntPtr session_handle_ptr, sbyte* url_buf, IntPtr url_len);

            public unsafe delegate void SessionErrorCallback(IntPtr session_handle_ptr, ErrorCode error_code, sbyte* message_buf, IntPtr message_len, SessionErrorKind error);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncmanager_configure_file_system", CallingConvention = CallingConvention.Cdecl)]
            public static extern unsafe void configure_file_system([MarshalAs(UnmanagedType.LPWStr)] string base_path, IntPtr base_path_leth, 
                                                                   UserPersistenceMode* userPersistence, byte[] encryptionKey,
                                                                   [MarshalAs(UnmanagedType.I1)] bool resetMetadataOnError,
                                                                   out NativeException exception);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_install_syncsession_callbacks", CallingConvention = CallingConvention.Cdecl)]
            public static extern unsafe void install_syncsession_callbacks(RefreshAccessTokenCallbackDelegate refresh_callback, SessionErrorCallback session_error_callback);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_syncmanager_get_path_for_realm", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr get_path_for_realm(SyncUserHandle user, [MarshalAs(UnmanagedType.LPWStr)] string url, IntPtr url_len, IntPtr buffer, IntPtr bufsize, out NativeException ex);
        }

        static unsafe SharedRealmHandleExtensions()
        {
            NativeMethods.install_syncsession_callbacks(RefreshAccessTokenCallback, HandleSessionError);
        }

        public static SharedRealmHandle OpenWithSync(Realms.Native.Configuration configuration, Native.SyncConfiguration syncConfiguration, RealmSchema schema, byte[] encryptionKey)
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
            // TODO: this should kill all active sessions (although no idea how we're supposed to do that). #1045
            NativeCommon.reset_for_testing();
            ConfigureFileSystem(userPersistenceMode, null, false);
        }

        #if __IOS__
        [ObjCRuntime.MonoPInvokeCallback(typeof(NativeMethods.RefreshAccessTokenCallbackDelegate))]
        #endif
        private static unsafe void RefreshAccessTokenCallback(IntPtr userHandlePtr, IntPtr sessionHandlePtr, sbyte* urlBuffer, IntPtr urlLength)
        {
            var userHandle = new SyncUserHandle();
            userHandle.SetHandle(userHandlePtr);
            var user = new User(userHandle);

            var session = Session.SessionForPointer(sessionHandlePtr);

            var realmUri = new Uri(new string(urlBuffer, 0, (int)urlLength, System.Text.Encoding.UTF8));

            user.RefreshAccessToken(realmUri.AbsolutePath)
                .ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    session.RaiseError(t.Exception.GetBaseException());
                }
                else
                {
                    session.Handle.RefreshAccessToken(t.Result.Item1, t.Result.Item2);
                }
            }).ContinueWith(t =>
            {
                userHandle.Dispose();
            });
        }

        #if __IOS__
        [ObjCRuntime.MonoPInvokeCallback(typeof(NativeMethods.SessionErrorCallback))]
        #endif
        private static unsafe void HandleSessionError(IntPtr sessionHandlePtr, ErrorCode errorCode, sbyte* messageBuffer, IntPtr messageLength, SessionErrorKind error)
        {
            var session = Session.SessionForPointer(sessionHandlePtr);
            var message = new string(messageBuffer, 0, (int)messageLength, System.Text.Encoding.UTF8);
            session.RaiseError(new SessionErrorException(message, error, errorCode));
        }
    }
}
