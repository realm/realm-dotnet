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
using System.Text;
using NUnit.Framework;
using Realms.Sync;
using Realms.Tests.Database;

namespace Realms.Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class SyncConfigurationTests : SyncTestBase
    {
        [Test]
        public void SyncConfiguration_WithoutPath()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var user = await SyncTestHelpers.GetFakeUserAsync();
                var serverUri = new Uri("realm://localhost:9080/foobar");
                var config = new SyncConfiguration(serverUri, user);

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
            TestHelpers.RunAsyncTest(async () =>
            {
                var user = await SyncTestHelpers.GetFakeUserAsync();
                var serverUri = new Uri("realm://localhost:9080/foobar");
                var config = new SyncConfiguration(serverUri, user, "myrealm.realm");

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
            TestHelpers.RunAsyncTest(async () =>
            {
                var user = await SyncTestHelpers.GetFakeUserAsync();
                var serverUri = new Uri("realm://localhost:9080/foobar");

                var path = Path.GetTempFileName();
                var config = new SyncConfiguration(serverUri, user, path);

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
            TestHelpers.RunAsyncTest(async () =>
            {
                var user = await SyncTestHelpers.GetFakeUserAsync();
                var key = Enumerable.Range(0, 63).Select(i => (byte)i).ToArray();

                var config = new SyncConfiguration(new Uri("realm://foobar"), user)
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
            TestHelpers.RunAsyncTest(async () =>
            {
                var user = await SyncTestHelpers.GetFakeUserAsync();

                Assert.That(() => new SyncConfiguration(new Uri(url), user), Throws.TypeOf<ArgumentException>());
            });
        }

        [Test]
        public void DefaultConfiguration_WhenNoUserLoggedIn_ShouldThrow()
        {
            Assert.That(() => new SyncConfiguration(new Uri("realms://foo/bar")), Throws.TypeOf<ArgumentException>().And.Message.Contains("The user must be explicitly specified when the number of logged-in users is not 1."));
        }

        [Test]
        public void DefaultConfiguration_WhenMoreThanOneUserLoggedIn_ShouldThrow()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                await SyncTestHelpers.GetFakeUserAsync();
                await SyncTestHelpers.GetFakeUserAsync();

                Assert.That(() => new SyncConfiguration(new Uri("realms://foo/bar")), Throws.TypeOf<ArgumentException>().And.Message.Contains("The user must be explicitly specified when the number of logged-in users is not 1."));
            });
        }

        [TestCase("http", "realm")]
        [TestCase("https", "realms")]
        public void DefaultConfiguration_WhenOneUserLoggedIn_ShouldWork(string userScheme, string realmScheme)
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                await SyncTestHelpers.GetFakeUserAsync(scheme: userScheme);

                var config = new SyncConfiguration(new Uri("/foo", UriKind.Relative));
                Assert.That(config.ServerUri.Scheme, Is.EqualTo(realmScheme));
                Assert.That(config.ServerUri.Segments, Is.EqualTo(new[] { "/", "foo" }));
            });
        }

        [Test]
        public void SyncConfiguration_CanBeSetAsRealmConfigurationDefault()
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                var user = await SyncTestHelpers.GetUserAsync();

                RealmConfiguration.DefaultConfiguration = new SyncConfiguration(new Uri("/foo", UriKind.Relative));

                using (var realm = GetRealm(null))
                {
                    Assert.That(realm.Config, Is.TypeOf<SyncConfiguration>());
                    var syncConfig = (SyncConfiguration)realm.Config;
                    Assert.That(syncConfig.User.Identity, Is.EqualTo(user.Identity));
                    Assert.That(syncConfig.ServerUri.Segments, Is.EqualTo(new[] { "/", "foo" }));
                }
            });
        }

        [TestCase("bar", "/bar")]
        [TestCase("/bar", "/bar")]
        [TestCase("/~/bar", "/~/bar")]
        [TestCase("~/bar", "/~/bar")]
        public void SyncConfiguration_WithRelativeUri_ResolvesCorrectly(string path, string expected)
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var user = await SyncTestHelpers.GetFakeUserAsync();
                var syncConfiguration = new SyncConfiguration(new Uri(path, UriKind.Relative), user);
                Assert.That(syncConfiguration.ServerUri.AbsoluteUri, Is.EqualTo($"realm://{SyncTestHelpers.FakeRosUrl}{expected}"));
            });
        }

        [TestCase(LogLevel.Debug)]
        [TestCase(LogLevel.Info)]
        public void SyncConfiguration_LoggerFactory_Test(LogLevel logLevel)
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                var logBuilder = new StringBuilder();

                SyncConfiguration.CustomLogger = (message, level) =>
                {
                    logBuilder.AppendLine($"[{level}] {message}");
                };
                SyncConfiguration.LogLevel = logLevel;

                var config = await SyncTestHelpers.GetIntegrationConfigAsync(Guid.NewGuid().ToString());
                using (var realm = await GetRealmAsync(config))
                {
                    realm.Write(() =>
                    {
                        realm.Add(new Person());
                    });

                    await SyncTestHelpers.WaitForUploadAsync(realm);
                }

                var log = logBuilder.ToString();

                Assert.That(log, Does.Contain($"[{logLevel}]"));
                Assert.That(log, Does.Not.Contain($"[{logLevel - 1}]"));
            });
        }
    }
}