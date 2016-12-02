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
using NUnit.Framework;
using Realms;
using Realms.Sync;
using Realms.Sync.Permissions;

namespace Tests.Sync.Shared
{
    [TestFixture, Preserve(AllMembers = true)]
    public class PermissionsTests
    {
        [TestCase("http", "realm")]
        [TestCase("https", "realms")]
        public async void User_GetManagementRealm(string authScheme, string syncScheme)
        {
            const string UriPattern = "{0}://some.fake.server:12345";
            var user = await User.LoginAsync(Credentials.AccessToken("foo:bar", Guid.NewGuid().ToString(), isAdmin: true), new Uri(string.Format(UriPattern, authScheme)));

            using (var realm = user.GetManagementRealm())
            {
                var configuration = (SyncConfiguration)realm.Config;
                Assert.That(configuration.User, Is.EqualTo(user));
                Assert.That(configuration.ServerUri.ToString(), Is.EqualTo(string.Format(UriPattern + "/~/__management", syncScheme)));
            }
        }
    }
}
