////////////////////////////////////////////////////////////////////////////
//
// Copyright 2017 Realm Inc.
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
using System.Threading.Tasks;
using Realms;
using Realms.Sync;

namespace Tests.Sync
{
    public abstract class SyncTestBase
    {
        protected static Task<User> GetUser()
        {
            var credentials = Constants.CreateCredentials();
            return User.LoginAsync(credentials, new Uri($"http://{Constants.ServerUrl}"));
        }

        protected static async Task<Realm> GetFakeRealm(bool isUserAdmin)
        {
            var user = await User.LoginAsync(Credentials.AccessToken("foo:bar", Guid.NewGuid().ToString(), isUserAdmin), new Uri("http://localhost:9080"));
            var serverUri = new Uri("realm://localhost:9080/foobar");
            return Realm.GetInstance(new SyncConfiguration(user, serverUri));
        }

        protected static async Task<Realm> GetIntegrationRealm(string path)
        {
            var user = await GetUser();
            var config = new SyncConfiguration(user, new Uri($"realm://{Constants.ServerUrl}/~/{path}"));
            return Realm.GetInstance(config);
        }
    }
}
