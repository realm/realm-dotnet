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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
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
            var path = Path.Combine(InteropConfig.DefaultStorageFolder, Guid.NewGuid().ToString());
            var config = GetFakeConfig(optionalPath: path);

            Realm.DeleteRealm(config);
            var file = new FileInfo(config.DatabasePath);
            Assert.That(file.Exists, Is.False);

            using (var realm = GetRealm(config))
            {
            }

            file = new FileInfo(config.DatabasePath);
            Assert.That(file.Exists);
            Assert.That(config.DatabasePath, Is.EqualTo(path));
        }

        [Test]
        public void SyncConfiguration_WithEncryptionKey_DoesntThrow()
        {
            var key = Enumerable.Range(0, 63).Select(i => (byte)i).ToArray();

            var config = GetFakeConfig();
            config.EncryptionKey = TestHelpers.GetEncryptionKey(key);

            Assert.That(() => GetRealm(config), Throws.Nothing);
        }

        public class CollectionsClass : RealmObject
        {
            [MapTo("_id")]
            [PrimaryKey]
            public ObjectId Id { get; private set; } = ObjectId.GenerateNewId();

            public IList<char> CharList { get; }

            public IList<byte> ByteList { get; }

            public IList<short> Int16List { get; }

            public IList<int> Int32List { get; }

            public IList<long> Int64List { get; }

            public IList<float> FloatList { get; }

            public IList<double> DoubleList { get; }

            public IList<bool> BooleanList { get; }

            public IList<decimal> DecimalList { get; }

            public IList<Decimal128> Decimal128List { get; }

            public IList<ObjectId> ObjectIdList { get; }

            [Required]
            public IList<string> StringList { get; }

            [Required]
            public IList<byte[]> ByteArrayList { get; }

            public IList<DateTimeOffset> DateTimeOffsetList { get; }

            public IList<RealmValue> RealmValueList { get; }

            public ISet<char> CharSet { get; }

            public ISet<byte> ByteSet { get; }

            public ISet<short> Int16Set { get; }

            public ISet<int> Int32Set { get; }

            public ISet<long> Int64Set { get; }

            public ISet<float> FloatSet { get; }

            public ISet<double> DoubleSet { get; }

            public ISet<bool> BooleanSet { get; }

            public ISet<decimal> DecimalSet { get; }

            public ISet<Decimal128> Decimal128Set { get; }

            public ISet<ObjectId> ObjectIdSet { get; }

            [Required]
            public ISet<string> StringSet { get; }

            [Required]
            public ISet<byte[]> ByteArraySet { get; }

            public ISet<DateTimeOffset> DateTimeOffsetSet { get; }

            public ISet<RealmValue> RealmValueSet { get; }

            public IDictionary<string, char> CharDict { get; }

            public IDictionary<string, byte> ByteDict { get; }

            public IDictionary<string, short> Int16Dict { get; }

            public IDictionary<string, int> Int32Dict { get; }

            public IDictionary<string, long> Int64Dict { get; }

            public IDictionary<string, float> FloatDict { get; }

            public IDictionary<string, double> DoubleDict { get; }

            public IDictionary<string, bool> BooleanDict { get; }

            public IDictionary<string, decimal> DecimalDict { get; }

            public IDictionary<string, Decimal128> Decimal128Dict { get; }

            public IDictionary<string, ObjectId> ObjectIdDict { get; }

            [Required]
            public IDictionary<string, string> StringDict { get; }

            [Required]
            public IDictionary<string, byte[]> ByteArrayDict { get; }

            public IDictionary<string, DateTimeOffset> DateTimeOffsetDict { get; }

            public IDictionary<string, RealmValue> RealmValueDict { get; }
        }

        [Test]
        public void IssueWithCore4839()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                // var user = await GetUserAsync();
                var user = GetFakeUser();

                var config = new SyncConfiguration(Guid.NewGuid().ToString(), user)
                {
                    ObjectClasses = new[] { typeof(CollectionsClass) },
                    SessionStopPolicy = SessionStopPolicy.Immediately
                };

                var realm = Realm.GetInstance(config);

                // Dispose calls SharedRealm::Close
                realm.Dispose();

                var sw = new Stopwatch();
                sw.Start();
                while (sw.ElapsedMilliseconds < 30_000)
                {
                    try
                    {
                        // Calls Realm::delete_files
                        Realm.DeleteRealm(realm.Config);
                        break;
                    }
                    catch
                    {
                        Task.Delay(50).Wait();
                    }
                }

                Assert.That(sw.ElapsedMilliseconds, Is.LessThan(2000));
            });
        }

        protected override void CustomTearDown()
        {
        }

        [Test]
        public void SyncConfiguration_CanBeSetAsRealmConfigurationDefault()
        {
            var config = GetFakeConfig();
            RealmConfiguration.DefaultConfiguration = config;

            var realm = GetRealm();

            Assert.That(realm.Config, Is.TypeOf<SyncConfiguration>());
            var syncConfig = (SyncConfiguration)realm.Config;
            Assert.That(syncConfig.User.Id, Is.EqualTo(config.User.Id));
            Assert.That(syncConfig.Partition, Is.EqualTo(config.Partition));
        }
    }
}
