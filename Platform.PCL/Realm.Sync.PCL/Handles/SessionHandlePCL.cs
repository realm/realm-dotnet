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
using System.Runtime.InteropServices;
using Realms;

namespace Realms.Sync
{
    internal class SessionHandle : RealmHandle
    {
        public SyncUserHandle GetUser()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        public string GetServerUri()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return string.Empty;
        }

        public SessionState GetState()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return default(SessionState);
        }

        public void RefreshAccessToken(string accessToken, string serverPath)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public static IntPtr SessionForRealm(SharedRealmHandle realm)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return IntPtr.Zero;
        }

        protected override void Unbind()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }
    }
}
