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
                var partitionValue = $"partition_tests_{Guid.NewGuid()}";
                var config1 = await GetIntegrationConfigAsync(partitionValue);
                var config2 = await GetIntegrationConfigAsync(partitionValue);

                await RunPartitionKeyTestsCore(config1, config2);
            });
        }

        [Test]
        public void OpenRealm_Int64PK_Works()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var partitionValue = TestHelpers.Random.Next(0, int.MaxValue);
                var config1 = await GetIntegrationConfigAsync(partitionValue);
                var config2 = await GetIntegrationConfigAsync(partitionValue);

                await RunPartitionKeyTestsCore(config1, config2);
            });
        }

        [Test]
        public void OpenRealm_ObjectIdPK_Works()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var partitionValue = ObjectId.GenerateNewId();
                var config1 = await GetIntegrationConfigAsync(partitionValue);
                var config2 = await GetIntegrationConfigAsync(partitionValue);

                await RunPartitionKeyTestsCore(config1, config2);
            });
        }

        [Test]
        public void OpenRealm_GuidPK_Works()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var partitionValue = Guid.NewGuid();
                var config1 = await GetIntegrationConfigAsync(partitionValue);
                var config2 = await GetIntegrationConfigAsync(partitionValue);

                await RunPartitionKeyTestsCore(config1, config2);
            });
        }

        private async Task RunPartitionKeyTestsCore(SyncConfiguration config1, SyncConfiguration config2)
        {
            var objectClasses = new Type[]
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

            config1.Schema = objectClasses;
            config2.Schema = objectClasses;

            using var realm1 = await GetRealmAsync(config1);
            using var realm2 = await GetRealmAsync(config2);

            var fromRealm1 = realm1.Write(() =>
            {
                return (
                    realm1.Add(new PrimaryKeyInt64Object { Id = 1234567890987654321 }),
                    realm1.Add(new PrimaryKeyObjectIdObject { Id = ObjectId.GenerateNewId() }),
                    realm1.Add(new PrimaryKeyGuidObject { Id = Guid.NewGuid() }),
                    realm1.Add(new RequiredPrimaryKeyStringObject { Id = "abcdef" }),
                    realm1.Add(new PrimaryKeyNullableInt64Object { Id = null }),
                    realm1.Add(new PrimaryKeyNullableObjectIdObject { Id = null }),
                    realm1.Add(new PrimaryKeyNullableGuidObject { Id = null }),
                    realm1.Add(new PrimaryKeyStringObject { Id = null }));
            });

            await WaitForUploadAsync(realm1);
            await WaitForDownloadAsync(realm2);

            Assert.That(realm2.Find<PrimaryKeyInt64Object>(fromRealm1.Item1.Id)?.IsValid, Is.True);
            Assert.That(realm2.Find<PrimaryKeyObjectIdObject>(fromRealm1.Item2.Id)?.IsValid, Is.True);
            Assert.That(realm2.Find<PrimaryKeyGuidObject>(fromRealm1.Item3.Id)?.IsValid, Is.True);
            Assert.That(realm2.Find<RequiredPrimaryKeyStringObject>(fromRealm1.Item4.Id)?.IsValid, Is.True);
            Assert.That(realm2.Find<PrimaryKeyNullableInt64Object>(fromRealm1.Item5.Id)?.IsValid, Is.True);
            Assert.That(realm2.Find<PrimaryKeyNullableGuidObject>(fromRealm1.Item6.Id)?.IsValid, Is.True);
            Assert.That(realm2.Find<PrimaryKeyNullableGuidObject>(fromRealm1.Item7.Id)?.IsValid, Is.True);
            Assert.That(realm2.Find<PrimaryKeyStringObject>(fromRealm1.Item8.Id)?.IsValid, Is.True);

            var fromRealm2 = realm2.Write(() =>
            {
                return (
                    realm2.Add(new PrimaryKeyInt64Object { Id = 0 }),
                    realm2.Add(new PrimaryKeyObjectIdObject { Id = ObjectId.GenerateNewId() }),
                    realm2.Add(new PrimaryKeyGuidObject { Id = Guid.NewGuid() }),
                    realm2.Add(new RequiredPrimaryKeyStringObject { Id = string.Empty }),
                    realm2.Add(new PrimaryKeyNullableInt64Object { Id = 123 }),
                    realm2.Add(new PrimaryKeyNullableObjectIdObject { Id = ObjectId.GenerateNewId() }),
                    realm2.Add(new PrimaryKeyNullableGuidObject { Id = Guid.NewGuid() }),
                    realm2.Add(new PrimaryKeyStringObject { Id = "hola" }));
            });

            await WaitForUploadAsync(realm2);
            await WaitForDownloadAsync(realm1);

            Assert.That(realm1.Find<PrimaryKeyInt64Object>(fromRealm2.Item1.Id)?.IsValid, Is.True);
            Assert.That(realm1.Find<PrimaryKeyObjectIdObject>(fromRealm2.Item2.Id)?.IsValid, Is.True);
            Assert.That(realm1.Find<PrimaryKeyGuidObject>(fromRealm2.Item3.Id)?.IsValid, Is.True);
            Assert.That(realm1.Find<RequiredPrimaryKeyStringObject>(fromRealm2.Item4.Id)?.IsValid, Is.True);
            Assert.That(realm1.Find<PrimaryKeyNullableInt64Object>(fromRealm2.Item5.Id)?.IsValid, Is.True);
            Assert.That(realm1.Find<PrimaryKeyNullableObjectIdObject>(fromRealm2.Item6.Id)?.IsValid, Is.True);
            Assert.That(realm1.Find<PrimaryKeyNullableGuidObject>(fromRealm2.Item7.Id)?.IsValid, Is.True);
            Assert.That(realm1.Find<PrimaryKeyStringObject>(fromRealm2.Item8.Id)?.IsValid, Is.True);
        }
    }
}
