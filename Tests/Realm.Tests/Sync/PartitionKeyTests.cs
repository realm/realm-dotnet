////////////////////////////////////////////////////////////////////////////
//
// Copyright 2021 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
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
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using NUnit.Framework;
using Realms.Sync;

namespace Realms.Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class PartitionKeyTests : SyncTestBase
    {
        [Test]
        public void OpenRealm_StringPK_Works()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var partitionValue = Guid.NewGuid().ToString();
                var config1 = await GetIntegrationConfigAsync(partitionValue).Timeout(20000);
                var config2 = await GetIntegrationConfigAsync(partitionValue).Timeout(20000);

                await RunPartitionKeyTestsCore(config1, config2);
            }, timeout: 120_000);
        }

        [Test]
        public void OpenRealm_Int64PK_Works()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var partitionValue = TestHelpers.Random.Next(int.MinValue, int.MaxValue);
                var config1 = await GetIntegrationConfigAsync(partitionValue).Timeout(10000);
                var config2 = await GetIntegrationConfigAsync(partitionValue).Timeout(10000);

                await RunPartitionKeyTestsCore(config1, config2);
            }, timeout: 60000);
        }

        [Test]
        public void OpenRealm_ObjectIdPK_Works()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var partitionValue = ObjectId.GenerateNewId();
                var config1 = await GetIntegrationConfigAsync(partitionValue).Timeout(10000);
                var config2 = await GetIntegrationConfigAsync(partitionValue).Timeout(10000);

                await RunPartitionKeyTestsCore(config1, config2);
            }, timeout: 60000);
        }

        [Test]
        public void OpenRealm_GuidPK_Works()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var partitionValue = Guid.NewGuid();
                var config1 = await GetIntegrationConfigAsync(partitionValue).Timeout(10000);
                var config2 = await GetIntegrationConfigAsync(partitionValue).Timeout(10000);

                await RunPartitionKeyTestsCore(config1, config2);
            }, timeout: 60000);
        }

        private async Task RunPartitionKeyTestsCore(PartitionSyncConfiguration config1, PartitionSyncConfiguration config2)
        {
            var schema = new Type[]
            {
                typeof(PrimaryKeyInt64Object),
                typeof(PrimaryKeyObjectIdObject),
                typeof(PrimaryKeyGuidObject),
                typeof(RequiredPrimaryKeyStringObject),
                typeof(PrimaryKeyNullableInt64Object),
                typeof(PrimaryKeyNullableObjectIdObject),
                typeof(PrimaryKeyNullableGuidObject),
                typeof(PrimaryKeyStringObject),
            };

            config1.Schema = schema;
            config2.Schema = schema;

            using var realm1 = await GetRealmAsync(config1).Timeout(5000);
            using var realm2 = await GetRealmAsync(config2).Timeout(5000);

            await AssertChangePropagation(realm1, realm2).Timeout(15000);
            await AssertChangePropagation(realm2, realm1).Timeout(15000);

            async Task AssertChangePropagation(Realm first, Realm second)
            {
                var ids = new
                {
                    Long = TestHelpers.Random.Next(),
                    ObjectId = ObjectId.GenerateNewId(),
                    Guid = Guid.NewGuid(),
                    String = Guid.NewGuid().ToString(),
                };

                var intObjectToAdd = new PrimaryKeyInt64Object { Id = ids.Long };
                first.Write(() =>
                {
                    first.Add(intObjectToAdd);
                    first.Add(new PrimaryKeyObjectIdObject { Id = ids.ObjectId });
                    first.Add(new PrimaryKeyGuidObject { Id = ids.Guid });
                    first.Add(new RequiredPrimaryKeyStringObject { Id = ids.String });
                    first.Add(new PrimaryKeyNullableInt64Object { Id = ids.Long });
                    first.Add(new PrimaryKeyNullableObjectIdObject { Id = ids.ObjectId });
                    first.Add(new PrimaryKeyNullableGuidObject { Id = ids.Guid });
                    first.Add(new PrimaryKeyStringObject { Id = ids.String });
                });

                await WaitForObjectAsync(intObjectToAdd, second);

                AssertFind<PrimaryKeyInt64Object>(second, ids.Long);
                AssertFind<PrimaryKeyObjectIdObject>(second, ids.ObjectId);
                AssertFind<PrimaryKeyGuidObject>(second, ids.Guid);
                AssertFind<RequiredPrimaryKeyStringObject>(second, ids.String);
                AssertFind<PrimaryKeyNullableInt64Object>(second, ids.Long);
                AssertFind<PrimaryKeyNullableObjectIdObject>(second, ids.ObjectId);
                AssertFind<PrimaryKeyNullableGuidObject>(second, ids.Guid);
                AssertFind<PrimaryKeyStringObject>(second, ids.String);
            }

            void AssertFind<T>(Realm realm, RealmValue id)
                where T : IRealmObject
            {
                Assert.That(realm.FindCore<T>(id)?.IsValid, Is.True, $"Failed to find {typeof(T).Name} with id {id}. Objects in Realm: {realm.All<T>().ToArray().Select(o => o.DynamicApi.Get<RealmValue>("_id").ToString()).Join()}");
            }
        }
    }
}
