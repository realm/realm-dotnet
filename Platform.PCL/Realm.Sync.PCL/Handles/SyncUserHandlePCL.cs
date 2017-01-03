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
        public string GetIdentity()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return string.Empty;
        }

        public string GetRefreshToken()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return string.Empty;
        }

        public string GetServerUrl()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return string.Empty;
        }

        public UserState GetState()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return default(UserState);
        }

        public void LogOut()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public static SyncUserHandle GetSyncUser(string identity, string refreshToken, string authServerUrl, bool isAdmin)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        public static SyncUserHandle GetCurrentUser()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        public static IEnumerable<SyncUserHandle> GetAllLoggedInUsers()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        protected override void Unbind()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        private static SyncUserHandle GetHandle(IntPtr ptr)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }
    }
}
