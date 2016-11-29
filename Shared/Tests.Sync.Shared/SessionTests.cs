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

namespace Tests.Sync.Shared
{
    [TestFixture, Preserve(AllMembers = true)]
    public class SessionTests
    {
        [Test]
        public async void Realm_GetSession_WhenSyncedRealm()
        {
            var user = await User.LoginAsync(Credentials.AccessToken("foo:bar", Guid.NewGuid().ToString(), true), new Uri("http://localhost:9080"));
            var serverUri = new Uri("realm://localhost:9080/foobar");
            using (var realm = Realm.GetInstance(new SyncConfiguration(user, serverUri)))
            {
                var session = realm.GetSyncSession();
                Assert.That(session.User, Is.EqualTo(user));
                Assert.That(session.ServerUri, Is.EqualTo(serverUri));
            }
        }

        [Test]
        public void Realm_GetSession_WhenLocalRealm_ShouldThrow()
        {
            using (var realm = Realm.GetInstance())
            {
                Assert.Throws<ArgumentException>(() => realm.GetSyncSession());
            }
        }

        [Test]
        public async void Realm_GetSession_ShouldReturnSameObject()
        {
            var user = await User.LoginAsync(Credentials.AccessToken("foo:bar", Guid.NewGuid().ToString(), true), new Uri("http://localhost:9080"));
            var serverUri = new Uri("realm://localhost:9080/foobar");
            using (var realm = Realm.GetInstance(new SyncConfiguration(user, serverUri)))
            {
                var session1 = realm.GetSyncSession();
                var session2 = realm.GetSyncSession();
                Assert.That(session1, Is.SameAs(session2));
            }
        }
    }
}
