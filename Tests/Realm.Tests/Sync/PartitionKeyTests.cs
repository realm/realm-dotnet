﻿////////////////////////////////////////////////////////////////////////////
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

        private async Task RunPartitionKeyTestsCore(PartitionSyncConfiguration config1, PartitionSyncConfiguration config2)
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

            config1.ObjectClasses = objectClasses;
            config2.ObjectClasses = objectClasses;

            using var realm1 = await GetRealmAsync(config1);
            using var realm2 = await GetRealmAsync(config2);

            var fromRealm1 = realm1.Write(() =>
            {
                return (
                    realm1.Add(new PrimaryKeyInt64Object { Int64Property = 1234567890987654321 }),
                    realm1.Add(new PrimaryKeyObjectIdObject { ObjectIdProperty = ObjectId.GenerateNewId() }),
                    realm1.Add(new PrimaryKeyGuidObject { GuidProperty = Guid.NewGuid() }),
                    realm1.Add(new RequiredPrimaryKeyStringObject { StringProperty = "abcdef" }),
                    realm1.Add(new PrimaryKeyNullableInt64Object { Int64Property = null }),
                    realm1.Add(new PrimaryKeyNullableObjectIdObject { ObjectIdProperty = null }),
                    realm1.Add(new PrimaryKeyNullableGuidObject { GuidProperty = null }),
                    realm1.Add(new PrimaryKeyStringObject { StringProperty = null }));
            });

            await WaitForUploadAsync(realm1);
            await WaitForDownloadAsync(realm2);

            Assert.That(realm2.Find<PrimaryKeyInt64Object>(fromRealm1.Item1.Int64Property)?.IsValid, Is.True);
            Assert.That(realm2.Find<PrimaryKeyObjectIdObject>(fromRealm1.Item2.ObjectIdProperty)?.IsValid, Is.True);
            Assert.That(realm2.Find<PrimaryKeyGuidObject>(fromRealm1.Item3.GuidProperty)?.IsValid, Is.True);
            Assert.That(realm2.Find<RequiredPrimaryKeyStringObject>(fromRealm1.Item4.StringProperty)?.IsValid, Is.True);
            Assert.That(realm2.Find<PrimaryKeyNullableInt64Object>(fromRealm1.Item5.Int64Property)?.IsValid, Is.True);
            Assert.That(realm2.Find<PrimaryKeyNullableGuidObject>(fromRealm1.Item6.ObjectIdProperty)?.IsValid, Is.True);
            Assert.That(realm2.Find<PrimaryKeyNullableGuidObject>(fromRealm1.Item7.GuidProperty)?.IsValid, Is.True);
            Assert.That(realm2.Find<PrimaryKeyStringObject>(fromRealm1.Item8.StringProperty)?.IsValid, Is.True);

            var fromRealm2 = realm2.Write(() =>
            {
                return (
                    realm2.Add(new PrimaryKeyInt64Object { Int64Property = 0 }),
                    realm2.Add(new PrimaryKeyObjectIdObject { ObjectIdProperty = ObjectId.GenerateNewId() }),
                    realm2.Add(new PrimaryKeyGuidObject { GuidProperty = Guid.NewGuid() }),
                    realm2.Add(new RequiredPrimaryKeyStringObject { StringProperty = string.Empty }),
                    realm2.Add(new PrimaryKeyNullableInt64Object { Int64Property = 123 }),
                    realm2.Add(new PrimaryKeyNullableObjectIdObject { ObjectIdProperty = ObjectId.GenerateNewId() }),
                    realm2.Add(new PrimaryKeyNullableGuidObject { GuidProperty = Guid.NewGuid() }),
                    realm2.Add(new PrimaryKeyStringObject { StringProperty = "hola" }));
            });

            await WaitForUploadAsync(realm2);
            await WaitForDownloadAsync(realm1);

            Assert.That(realm1.Find<PrimaryKeyInt64Object>(fromRealm2.Item1.Int64Property)?.IsValid, Is.True);
            Assert.That(realm1.Find<PrimaryKeyObjectIdObject>(fromRealm2.Item2.ObjectIdProperty)?.IsValid, Is.True);
            Assert.That(realm1.Find<PrimaryKeyGuidObject>(fromRealm2.Item3.GuidProperty)?.IsValid, Is.True);
            Assert.That(realm1.Find<RequiredPrimaryKeyStringObject>(fromRealm2.Item4.StringProperty)?.IsValid, Is.True);
            Assert.That(realm1.Find<PrimaryKeyNullableInt64Object>(fromRealm2.Item5.Int64Property)?.IsValid, Is.True);
            Assert.That(realm1.Find<PrimaryKeyNullableObjectIdObject>(fromRealm2.Item6.ObjectIdProperty)?.IsValid, Is.True);
            Assert.That(realm1.Find<PrimaryKeyNullableGuidObject>(fromRealm2.Item7.GuidProperty)?.IsValid, Is.True);
            Assert.That(realm1.Find<PrimaryKeyStringObject>(fromRealm2.Item8.StringProperty)?.IsValid, Is.True);
        }
    }
}
