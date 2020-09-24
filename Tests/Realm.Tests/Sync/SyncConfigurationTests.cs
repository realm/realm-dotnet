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
using NUnit.Framework;
using Realms.Sync;

namespace Realms.Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    [Ignore("V10TODO: Enable when sync API are wired up.")]
    public class SyncConfigurationTests : SyncTestBase
    {
        [Test]
        public void SyncConfiguration_WithoutPath()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var user = await SyncTestHelpers.GetFakeUserAsync(_app);
                var config = new SyncConfiguration("foo-bar", user);

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
                var user = await SyncTestHelpers.GetFakeUserAsync(_app);
                var config = new SyncConfiguration("foo-bar", user, "myrealm.realm");

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
                var user = await SyncTestHelpers.GetFakeUserAsync(_app);

                var path = Path.GetTempFileName();
                var config = new SyncConfiguration(123, user, path);

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
                var user = await SyncTestHelpers.GetFakeUserAsync(_app);
                var key = Enumerable.Range(0, 63).Select(i => (byte)i).ToArray();

                var config = new SyncConfiguration("foo-bar", user)
                {
                    EncryptionKey = TestHelpers.GetEncryptionKey(key)
                };

                Assert.That(() => GetRealm(config), Throws.Nothing);
            });
        }

        [Test]
        public void SyncConfiguration_CanBeSetAsRealmConfigurationDefault()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await SyncTestHelpers.GetUserAsync(_app);

                RealmConfiguration.DefaultConfiguration = new SyncConfiguration("abc", user);

                using (var realm = GetRealm(null))
                {
                    Assert.That(realm.Config, Is.TypeOf<SyncConfiguration>());
                    var syncConfig = (SyncConfiguration)realm.Config;
                    Assert.That(syncConfig.User.Id, Is.EqualTo(user.Id));
                    Assert.That(syncConfig.Partition, Is.EqualTo("abc"));
                }
            });
        }

        [TestCase(LogLevel.Debug)]
        [TestCase(LogLevel.Info)]
        public void SyncConfiguration_LoggerFactory_Test(LogLevel logLevel)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                throw new NotImplementedException();
                //var logBuilder = new StringBuilder();

                //SyncConfiguration.CustomLogger = (message, level) =>
                //{
                //    logBuilder.AppendLine($"[{level}] {message}");
                //};
                //SyncConfiguration.LogLevel = logLevel;

                //var config = await SyncTestHelpers.GetIntegrationConfigAsync(Guid.NewGuid().ToString());
                //using (var realm = await GetRealmAsync(config))
                //{
                //    realm.Write(() =>
                //    {
                //        realm.Add(new Person());
                //    });

                //    await SyncTestHelpers.WaitForUploadAsync(realm);
                //}

                //var log = logBuilder.ToString();

                //Assert.That(log, Does.Contain($"[{logLevel}]"));
                //Assert.That(log, Does.Not.Contain($"[{logLevel - 1}]"));
            });
        }
    }
}