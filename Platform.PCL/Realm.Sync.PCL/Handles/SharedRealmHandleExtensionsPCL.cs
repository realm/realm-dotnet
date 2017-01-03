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
using System.Threading;
using Realms;

namespace Realms.Sync
{
    internal static class SharedRealmHandleExtensions
    {
        static SharedRealmHandleExtensions()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        /* TODO - work out if PCL needs this and if so, how to build
        public static SharedRealmHandle OpenWithSync(Realms.Native.Configuration configuration, Native.SyncConfiguration syncConfiguration, RealmSchema schema, byte[] encryptionKey)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }
        */

        public static string GetRealmPath(User user, Uri serverUri)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return string.Empty;
        }

        // Configure the SyncMetadataManager with default values if it hasn't been configured already
        public static void DoInitialFileSystemConfiguration()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public static void ConfigureFileSystem(UserPersistenceMode? userPersistenceMode, byte[] encryptionKey, bool resetMetadataOnError)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public static void ResetForTesting(UserPersistenceMode? userPersistenceMode = null)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        /* TODO - if needed for PCL, work out alternative implementation
         * uses sbyte which is only available in unsafe
        private static  void RefreshAccessTokenCallback(IntPtr userHandlePtr, IntPtr sessionHandlePtr, sbyte* urlBuffer, IntPtr urlLength)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        private static void HandleSessionError(IntPtr sessionHandlePtr, ErrorCode errorCode, sbyte* messageBuffer, IntPtr messageLength, SessionErrorKind error)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }
        */
    }
}
