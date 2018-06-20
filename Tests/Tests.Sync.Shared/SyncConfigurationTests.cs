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
using System.IO;
using System.Linq;
using Nito.AsyncEx;
using NUnit.Framework;
using Realms;
using Realms.Sync;

namespace Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class SyncConfigurationTests : SyncTestBase
    {
        [Test]
        public void SyncConfiguration_WithoutPath()
        {
            AsyncContext.Run(async () =>
            {
                var user = await SyncTestHelpers.GetFakeUserAsync();
                var serverUri = new Uri("realm://localhost:9080/foobar");
                var config = new FullSyncConfiguration(serverUri, user);

                var file = new FileInfo(config.DatabasePath);
                Assert.That(file.Exists, Is.False);

                using (var realm = GetRealm(config))
                {
                }

                file = new FileInfo(config.DatabasePath);
                Assert.That(file.Exists);
            });
        }

        [Test]
        public void SyncConfiguration_WithRelativePath()
        {
            AsyncContext.Run(async () =>
            {
                var user = await SyncTestHelpers.GetFakeUserAsync();
                var serverUri = new Uri("realm://localhost:9080/foobar");
                var config = new FullSyncConfiguration(serverUri, user, "myrealm.realm");

                var file = new FileInfo(config.DatabasePath);
                Assert.That(file.Exists, Is.False);

                using (var realm = GetRealm(config))
                {
                }

                file = new FileInfo(config.DatabasePath);
                Assert.That(file.Exists);
                Assert.That(config.DatabasePath.EndsWith("myrealm.realm"));
            });
        }

        [Test]
        public void SyncConfiguration_WithAbsolutePath()
        {
            AsyncContext.Run(async () =>
            {
                var user = await SyncTestHelpers.GetFakeUserAsync();
                var serverUri = new Uri("realm://localhost:9080/foobar");

                var path = Path.GetTempFileName();
                var config = new FullSyncConfiguration(serverUri, user, path);

                Realm.DeleteRealm(config);
                var file = new FileInfo(config.DatabasePath);
                Assert.That(file.Exists, Is.False);

                using (var realm = GetRealm(config))
                {
                }

                file = new FileInfo(config.DatabasePath);
                Assert.That(file.Exists);
                Assert.That(config.DatabasePath, Is.EqualTo(path));
            });
        }

        [Test]
        public void SyncConfiguration_WithEncryptionKey_DoesntThrow()
        {
            AsyncContext.Run(async () =>
            {
                var user = await SyncTestHelpers.GetFakeUserAsync();
                var key = Enumerable.Range(0, 63).Select(i => (byte)i).ToArray();

                var config = new FullSyncConfiguration(new Uri("realm://foobar"), user)
                {
                    EncryptionKey = TestHelpers.GetEncryptionKey(key)
                };

                Assert.That(() => GetRealm(config), Throws.Nothing);
            });
        }

        [TestCase("http://localhost/~/foo")]
        [TestCase("https://localhost/~/foo")]
        [TestCase("foo://bar/~/foo")]
        public void SyncConfiguration_WrongProtocolTests(string url)
        {
            AsyncContext.Run(async () =>
            {
                var user = await SyncTestHelpers.GetFakeUserAsync();

                Assert.That(() => new FullSyncConfiguration(new Uri(url), user), Throws.TypeOf<ArgumentException>());
            });
        }

        [Test]
        public void DefaultConfiguration_WhenNoUserLoggedIn_ShouldThrow()
        {
            Assert.That(() => new QueryBasedSyncConfiguration(), Throws.TypeOf<ArgumentException>().And.Message.Contains("The user must be explicitly specified when the number of logged-in users is not 1."));
        }

        [Test]
        public void DefaultConfiguration_WhenMoreThanOneUserLoggedIn_ShouldThrow()
        {
            AsyncContext.Run(async () =>
            {
                await SyncTestHelpers.GetFakeUserAsync();
                await SyncTestHelpers.GetFakeUserAsync();

                Assert.That(() => new QueryBasedSyncConfiguration(), Throws.TypeOf<ArgumentException>().And.Message.Contains("The user must be explicitly specified when the number of logged-in users is not 1."));
            });
        }

        [TestCase("http", "realm")]
        [TestCase("https", "realms")]
        public void DefaultConfiguration_WhenOneUserLoggedIn_ShouldWork(string userScheme, string realmScheme)
        {
            AsyncContext.Run(async () =>
            {
                await SyncTestHelpers.GetFakeUserAsync(scheme: userScheme);

                var config = new QueryBasedSyncConfiguration();
                Assert.That(!config.IsFullSync);
                Assert.That(config.ServerUri.Scheme, Is.EqualTo(realmScheme));
                Assert.That(config.ServerUri.Segments, Is.EqualTo(new[] { "/", "default" }));
            });
        }

        [Test]
        public void SyncConfiguration_CanBeSetAsRealmConfigurationDefault()
        {
            SyncTestHelpers.RequiresRos();

            AsyncContext.Run(async () =>
            {
                var user = await SyncTestHelpers.GetUserAsync();

                RealmConfiguration.DefaultConfiguration = new QueryBasedSyncConfiguration();

                using (var realm = GetRealm(null))
                {
                    Assert.That(realm.Config, Is.TypeOf<QueryBasedSyncConfiguration>());
                    var syncConfig = (QueryBasedSyncConfiguration)realm.Config;
                    Assert.That(syncConfig.User.Identity, Is.EqualTo(user.Identity));
                    Assert.That(syncConfig.ServerUri.Segments, Is.EqualTo(new[] { "/", "default" }));
                }
            });
        }

        [TestCase("bar", "/bar")]
        [TestCase("/bar", "/bar")]
        [TestCase("/~/bar", "/~/bar")]
        [TestCase("~/bar", "/~/bar")]
        public void SyncConfiguration_WithRelativeUri_ResolvesCorrectly(string path, string expected)
        {
            AsyncContext.Run(async () =>
            {
                var user = await SyncTestHelpers.GetFakeUserAsync();
                var syncConfiguration = new FullSyncConfiguration(new Uri(path, UriKind.Relative), user);
                Assert.That(syncConfiguration.ServerUri.AbsoluteUri, Is.EqualTo($"realm://{SyncTestHelpers.FakeRosUrl}{expected}"));
            });
        }
    }
}