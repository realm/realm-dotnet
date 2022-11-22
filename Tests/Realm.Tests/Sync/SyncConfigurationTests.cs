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
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NUnit.Framework;
using Realms.Sync;

namespace Realms.Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class SyncConfigurationTests : SyncTestBase
    {
        [Test]
        public void SyncConfiguration_WithoutPath()
        {
            var config = GetFakeConfig();

            var file = new FileInfo(config.DatabasePath);
            Assert.That(file.Exists, Is.False);

            using (var realm = GetRealm(config))
            {
            }

            file = new FileInfo(config.DatabasePath);
            Assert.That(file.Exists);
        }

        [Test]
        public void SyncConfiguration_WithRelativePath()
        {
            var config = GetFakeConfig(optionalPath: "myrealm.realm");

            var file = new FileInfo(config.DatabasePath);
            Assert.That(file.Exists, Is.False);

            using (var realm = GetRealm(config))
            {
            }

            file = new FileInfo(config.DatabasePath);
            Assert.That(file.Exists);
            Assert.That(config.DatabasePath.EndsWith("myrealm.realm"));
        }

        [Test]
        public void SyncConfiguration_WithAbsolutePath()
        {
            var path = Path.Combine(InteropConfig.GetDefaultStorageFolder("realm"), Guid.NewGuid().ToString());
            var config = GetFakeConfig(optionalPath: path);

            Realm.DeleteRealm(config);
            var file = new FileInfo(config.DatabasePath);
            Assert.That(file.Exists, Is.False);

            using (var realm = GetRealm(config))
            {
            }

            file = new FileInfo(config.DatabasePath);
            Assert.That(file.Exists);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && TestHelpers.IsUnity)
            {
                // Unity on Windows has a peculiar behavior where Path.Combine will generate paths with
                // forward slashes rather than backslashes.
                path = path.Replace('/', '\\');
            }

            Assert.That(config.DatabasePath, Is.EqualTo(path));
        }

        [Test]
        public void Test_SyncConfigRelease()
        {
            WeakReference weakConfigRef = null;
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                weakConfigRef = new WeakReference(await GetIntegrationConfigAsync());
                using var realm = await GetRealmAsync((PartitionSyncConfiguration)weakConfigRef.Target);
                var session = GetSession(realm);
                Assert.That(weakConfigRef.Target, Is.Not.Null);
            });
            TearDown();

            for (var i = 0; i < 50; i++)
            {
                GC.Collect();
                if (!weakConfigRef.IsAlive)
                {
                    break;
                }

                Task.Delay(100).Wait();
            }

            Assert.That(weakConfigRef.Target, Is.Null);
        }

        [Test]
        public void SyncConfiguration_WithEncryptionKey_DoesntThrow()
        {
            var key = Enumerable.Range(0, 63).Select(i => (byte)i).ToArray();

            var config = GetFakeConfig();
            config.EncryptionKey = TestHelpers.GetEncryptionKey(key);

            Assert.That(() => GetRealm(config), Throws.Nothing);
        }

        [Test]
        public void SyncConfiguration_CanBeSetAsRealmConfigurationDefault()
        {
            var config = GetFakeConfig();
            RealmConfiguration.DefaultConfiguration = config;

            var realm = GetRealm();

            Assert.That(realm.Config, Is.TypeOf<PartitionSyncConfiguration>());
            var syncConfig = (PartitionSyncConfiguration)realm.Config;
            Assert.That(syncConfig.User.Id, Is.EqualTo(config.User.Id));
            Assert.That(syncConfig.Partition, Is.EqualTo(config.Partition));
        }

        [Test]
        [Obsolete("Tests obsolete functionality")]
        public void ObsoleteSyncConfiguration_ProxiesToNewOne()
        {
            var user = GetFakeUser();
            var config = new SyncConfiguration("foo", user);

            Assert.That(config is PartitionSyncConfiguration);
            Assert.That(config.Partition, Is.TypeOf<string>());

            var partitionConfig = (PartitionSyncConfiguration)config;
            Assert.That(partitionConfig.Partition, Is.TypeOf<RealmValue>());
            Assert.That(partitionConfig.Partition.AsString(), Is.EqualTo("foo"));
            Assert.That(partitionConfig.User, Is.EqualTo(user));
        }
    }
}
