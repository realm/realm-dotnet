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

    public class User : ISerializable
    {
        public static User CurrentUser { get; private set; }

        public static async Task<User> LoginAsync(Credentials credentials, string serverUrl, LoginMode loginMode = LoginMode.UseExistingAccount)
        {
            throw new NotImplementedException();
        }

        public static User AdminUser(string adminToken)
        {
            return new User(SyncUserHandle.GetSyncUser(Guid.NewGuid().ToString(), adminToken, null, true));
        }

        public string AccessToken { get; private set; }

        public string Identity { get; private set; }

        public bool IsValid { get; private set; }

        public void LogOut()
        {
            throw new NotImplementedException();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }

        private SyncUserHandle _syncUserHandle;

        internal SyncUserHandle SyncUserHandle => _syncUserHandle;

        private User(SyncUserHandle userHandle)
        {
            this._syncUserHandle = userHandle;
        }
    }
}