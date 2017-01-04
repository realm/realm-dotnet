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
using System.Threading.Tasks;
using NUnit.Framework;
using Realms;
using Realms.Sync;
using Realms.Sync.Exceptions;

using ExplicitAttribute = NUnit.Framework.ExplicitAttribute;

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
                var session = realm.GetSession();
                Assert.That(session.User, Is.EqualTo(user));
                Assert.That(session.ServerUri, Is.EqualTo(serverUri));
            }
        }

        [Test]
        public void Realm_GetSession_WhenLocalRealm_ShouldThrow()
        {
            using (var realm = Realm.GetInstance())
            {
                Assert.Throws<ArgumentException>(() => realm.GetSession());
            }
        }

        [Test]
        public async void Realm_GetSession_ShouldReturnSameObject()
        {
            var user = await User.LoginAsync(Credentials.AccessToken("foo:bar", Guid.NewGuid().ToString(), true), new Uri("http://localhost:9080"));
            var serverUri = new Uri("realm://localhost:9080/foobar");
            using (var realm = Realm.GetInstance(new SyncConfiguration(user, serverUri)))
            {
                var session1 = realm.GetSession();
                var session2 = realm.GetSession();
                Assert.That(session1, Is.SameAs(session2));
            }
        }

        [Test, Explicit, Timeout(1000)]
        public async void Session_Error_WhenInvalidRefreshToken()
        {
            var errors = new List<Exception>();

            var user = await User.LoginAsync(Credentials.AccessToken("foo:bar", Guid.NewGuid().ToString(), isAdmin: false), new Uri("http://localhost:9080"));
            var serverUri = new Uri("realm://localhost:9080/foobar");
            using (var realm = Realm.GetInstance(new SyncConfiguration(user, serverUri)))
            {
                var session = realm.GetSession();
                session.Error += (o, e) => errors.Add(e.Exception);

                while (!errors.Any())
                {
                    await Task.Yield();
                }

                var authErrors = errors.OfType<AuthenticationException>().ToList();
                Assert.That(authErrors.Count, Is.EqualTo(1));
                Assert.That(authErrors[0].ErrorCode, Is.EqualTo(ErrorCode.InvalidCredentials));
            }
        }

        [Test, Explicit, Timeout(1000)]
        public async void Session_Error_WhenInvalidAccessToken()
        {
            var errors = new List<Exception>();

            var user = await User.LoginAsync(Credentials.AccessToken("foo:bar", Guid.NewGuid().ToString(), isAdmin: true), new Uri("http://localhost:9080"));
            var serverUri = new Uri("realm://localhost:9080/foobar");
            using (var realm = Realm.GetInstance(new SyncConfiguration(user, serverUri)))
            {
                var session = realm.GetSession();
                session.Error += (o, e) => errors.Add(e.Exception);

                while (!errors.Any())
                {
                    await Task.Yield();
                }

                var sessionErrors = errors.OfType<SessionErrorException>().ToList();
                Assert.That(sessionErrors.Count, Is.EqualTo(1));
                Assert.That(sessionErrors[0].ErrorCode, Is.EqualTo(ErrorCode.BadUserAuthentication));
            }
        }
    }
}
