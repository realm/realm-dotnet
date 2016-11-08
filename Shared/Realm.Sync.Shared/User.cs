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
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Realms.Sync
{
    [Flags]
    public enum LoginMode
    {
        UseExistingAccount = 1,
        CreateAccount = 2
    }

    public enum UserState
    {
        LoggedOut,
        Active,
        Error
    }

    public class User
    {
        public string RefreshToken => Handle.RefreshToken;

        public string Identity => Handle.Identity;

        public Uri ServerUri
        {
            get
            {
                var serverUrl = Handle.ServerUrl;
                if (string.IsNullOrEmpty(serverUrl))
                {
                    return null;
                }

                return new Uri(serverUrl);
            }
        }

        public UserState State => Handle.State;

        internal readonly SyncUserHandle Handle;

        internal User(SyncUserHandle handle)
        {
            Handle = handle;
        }

        public static async Task<User> LoginAsync(Credentials credentials, string serverUrl, LoginMode loginMode = LoginMode.UseExistingAccount)
        {
            if (credentials.IdentityProvider == Credentials.Providers.AccessToken)
            {
                var identity = (string)credentials.UserInfo[Credentials.Keys.Identity];
                var isAdmin = (bool)credentials.UserInfo[Credentials.Keys.IsAdmin];
                return new User(SyncUserHandle.GetSyncUser(identity, credentials.Token, serverUrl, isAdmin));
            }

            return null;
        }

        public void LogOut()
        {
            Handle.LogOut();
        }
    }
}