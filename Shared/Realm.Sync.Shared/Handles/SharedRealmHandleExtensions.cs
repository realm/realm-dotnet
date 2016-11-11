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
using Realms;

namespace Realms.Sync
{
    internal static class SharedRealmHandleExtensions
    {
        private static class NativeMethods
        {
            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "shared_realm_open_with_sync", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr open_with_sync(Realms.Native.Configuration configuration, Native.SyncConfiguration sync_configuration,
                [MarshalAs(UnmanagedType.LPArray), In] Realms.Native.SchemaObject[] objects, int objects_length,
                [MarshalAs(UnmanagedType.LPArray), In] Realms.Native.SchemaProperty[] properties,
                byte[] encryptionKey,
                out NativeException ex);

            public unsafe delegate void RefreshAccessTokenCallbackDelegate(IntPtr user_handle_ptr, IntPtr session_handle_ptr, sbyte* url_buf, IntPtr url_len);

            [DllImport(InteropConfig.DLL_NAME, EntryPoint = "realm_initialize_sync", CallingConvention = CallingConvention.Cdecl)]
            public static extern void initialize_sync([MarshalAs(UnmanagedType.LPWStr)] string base_path, IntPtr base_path_leth, RefreshAccessTokenCallbackDelegate refresh_callback);
        }

        static unsafe SharedRealmHandleExtensions()
        {
            InitializeSync(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), RefreshAccessTokenCallback);
        }

        public static SharedRealmHandle OpenWithSync(Realms.Native.Configuration configuration, Native.SyncConfiguration syncConfiguration, RealmSchema schema, byte[] encryptionKey)
        {
            var marshaledSchema = new SharedRealmHandle.SchemaMarshaler(schema);

            NativeException nativeException;
            var result = NativeMethods.open_with_sync(configuration, syncConfiguration, marshaledSchema.Objects, marshaledSchema.Objects.Length, marshaledSchema.Properties, encryptionKey, out nativeException);
            nativeException.ThrowIfNecessary();

            var handle = new SharedRealmHandle();
            handle.SetHandle(result);
            return handle;
        }

        private static void InitializeSync(string basePath, NativeMethods.RefreshAccessTokenCallbackDelegate refreshCallback)
        {
            NativeMethods.initialize_sync(basePath, (IntPtr)basePath.Length, refreshCallback);
        }

        #if __IOS__
        [ObjCRuntime.MonoPInvokeCallback(typeof(NativeMethods.RefreshAccessTokenCallbackDelegate))]
        #endif
        private static unsafe void RefreshAccessTokenCallback(IntPtr userHandlePtr, IntPtr sessionHandlePtr, sbyte* urlBuffer, IntPtr urlLength)
        {
            var userHandle = new SyncUserHandle();
            userHandle.SetHandle(userHandlePtr);
            var user = new User(userHandle);

            var sessionHandle = new SessionHandle();
            sessionHandle.SetHandle(sessionHandlePtr);

            var realmUri = new Uri(new string(urlBuffer, 0, (int)urlLength, System.Text.Encoding.UTF8));

            user.RefreshAccessToken(realmUri.AbsolutePath)
                .ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    // TODO: raise error event on the session
                }
                else
                {
                    sessionHandle.RefreshAccessToken(t.Result.Item1, t.Result.Item2);
                }
            });
        }
    }
}
