////////////////////////////////////////////////////////////////////////////
//
// Copyright 2024 Realm Inc.
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
using Baas;
using MongoDB.Bson;
using NUnit.Framework;
using Realms.Sync;
using Realms.Sync.Exceptions;

namespace Realms.Tests.Sync
{
    // The model must match BaasClient.Schemas.Nullables
    [MapTo("Nullables"), Explicit]
    public partial class NullablesV0 : IRealmObject
    {
        [PrimaryKey, MapTo("_id")]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        public ObjectId Differentiator { get; set; }

        public bool? BoolValue { get; set; }

        public int? IntValue { get; set; }

        public double? DoubleValue { get; set; }

        public Decimal128? DecimalValue { get; set; }

        public DateTimeOffset? DateValue { get; set; }

        public string? StringValue { get; set; }

        public ObjectId? ObjectIdValue { get; set; }

        public Guid? UuidValue { get; set; }

        public byte[]? BinaryValue { get; set; }
    }

    [MapTo("Nullables"), Explicit]
    public partial class NullablesV1 : IRealmObject
    {
        [PrimaryKey, MapTo("_id")]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        public ObjectId Differentiator { get; set; }

        public bool BoolValue { get; set; }

        public int IntValue { get; set; }

        public double DoubleValue { get; set; }

        public Decimal128 DecimalValue { get; set; }

        public DateTimeOffset DateValue { get; set; }

        public string StringValue { get; set; } = string.Empty;

        public ObjectId ObjectIdValue { get; set; }

        public Guid UuidValue { get; set; }

        public byte[] BinaryValue { get; set; } = Array.Empty<byte>();

        public string WillBeRemoved { get; set; } = string.Empty;
    }

    [TestFixture, Preserve(AllMembers = true)]
    public class SyncMigrationTests : SyncTestBase
    {
        private async Task<Realm> OpenRealm(ObjectId differentiator, Type schema, ulong schemaVersion)
        {
            var app = App.Create(SyncTestHelpers.GetAppConfig(AppConfigType.StaticSchema));
            var config = await GetFLXIntegrationConfigAsync(app);
            config.SchemaVersion = schemaVersion;
            config.Schema = new[]
            {
                schema
            };

            config.PopulateInitialSubscriptions = (r) =>
            {
                r.Subscriptions.Add(r.DynamicApi.All("Nullables").Filter("Differentiator == $0", differentiator));
            };

            var realm = GetRealm(config);

            await WaitForSubscriptionsAsync(realm);

            return realm;
        }

        [Test, Ignore("TODO: restore once https://mongodb.slack.com/archives/C04NACGT7J7/p1716383151806129 is resolved")]
        public void Model_CanMigratePropertyOptionality()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var differentiator = ObjectId.GenerateNewId();
                var date = new DateTimeOffset(2015, 12, 13, 11, 24, 59, TimeSpan.Zero);
                var oid = ObjectId.GenerateNewId();
                var uuid = Guid.NewGuid();
                var binary = new byte[]
                {
                    1, 2, 3
                };

                var realmv0 = await OpenRealm(differentiator, typeof(NullablesV0), schemaVersion: 0);
                var objv0 = realmv0.Write(() => realmv0.Add(new NullablesV0
                {
                    BoolValue = true,
                    DateValue = date,
                    DecimalValue = 324.987m,
                    DoubleValue = -999.87654321,
                    IntValue = 42,
                    ObjectIdValue = oid,
                    StringValue = "bla bla",
                    UuidValue = uuid,
                    BinaryValue = binary,
                    Differentiator = differentiator,
                }));

                await WaitForUploadAsync(realmv0);

                var realmv1 = await OpenRealm(differentiator, typeof(NullablesV1), schemaVersion: 1);
                var objv1 = realmv1.All<NullablesV1>().Single();

                Assert.That(objv1.BoolValue, Is.EqualTo(true));
                Assert.That(objv1.DateValue, Is.EqualTo(date));
                Assert.That(objv1.DecimalValue, Is.EqualTo(new Decimal128(324.987m)));
                Assert.That(objv1.DoubleValue, Is.EqualTo(-999.87654321));
                Assert.That(objv1.IntValue, Is.EqualTo(42));
                Assert.That(objv1.ObjectIdValue, Is.EqualTo(oid));
                Assert.That(objv1.StringValue, Is.EqualTo("bla bla"));
                Assert.That(objv1.UuidValue, Is.EqualTo(uuid));
                Assert.That(objv1.BinaryValue, Is.EqualTo(binary));
                Assert.That(objv1.WillBeRemoved, Is.EqualTo(string.Empty));

                var realmv2 = await OpenRealm(differentiator, typeof(NullablesV0), schemaVersion: 2);
                var objv2 = realmv2.All<NullablesV0>().Single();

                Assert.That(objv2.BoolValue, Is.EqualTo(true));
                Assert.That(objv2.DateValue, Is.EqualTo(date));
                Assert.That(objv2.DecimalValue, Is.EqualTo(new Decimal128(324.987m)));
                Assert.That(objv2.DoubleValue, Is.EqualTo(-999.87654321));
                Assert.That(objv2.IntValue, Is.EqualTo(42));
                Assert.That(objv2.ObjectIdValue, Is.EqualTo(oid));
                Assert.That(objv2.StringValue, Is.EqualTo("bla bla"));
                Assert.That(objv2.UuidValue, Is.EqualTo(uuid));
                Assert.That(objv2.BinaryValue, Is.EqualTo(binary));

                realmv0.Write(() =>
                {
                    objv0.BoolValue = null;
                    objv0.DateValue = null;
                    objv0.DecimalValue = null;
                    objv0.DoubleValue = null;
                    objv0.IntValue = null;
                    objv0.ObjectIdValue = null;
                    objv0.StringValue = null;
                    objv0.UuidValue = null;
                    objv0.BinaryValue = null;
                });

                await WaitForUploadAsync(realmv0);
                await WaitForDownloadAsync(realmv1);
                await WaitForDownloadAsync(realmv2);

                Assert.That(objv1.BoolValue, Is.EqualTo(false));
                Assert.That(objv1.DateValue, Is.EqualTo(new DateTimeOffset(1, 1, 1, 0, 0, 0, TimeSpan.Zero)));
                Assert.That(objv1.DecimalValue, Is.EqualTo(Decimal128.Zero));
                Assert.That(objv1.DoubleValue, Is.EqualTo(0));
                Assert.That(objv1.IntValue, Is.EqualTo(0));
                Assert.That(objv1.ObjectIdValue, Is.EqualTo(ObjectId.Empty));
                Assert.That(objv1.StringValue, Is.EqualTo(string.Empty));
                Assert.That(objv1.UuidValue, Is.EqualTo(Guid.Empty));
                Assert.That(objv1.BinaryValue, Is.EqualTo(Array.Empty<byte>()));
                Assert.That(objv1.WillBeRemoved, Is.EqualTo(string.Empty));

                Assert.That(objv2.BoolValue, Is.Null);
                Assert.That(objv2.DateValue, Is.Null);
                Assert.That(objv2.DecimalValue, Is.Null);
                Assert.That(objv2.DoubleValue, Is.Null);
                Assert.That(objv2.IntValue, Is.Null);
                Assert.That(objv2.ObjectIdValue, Is.Null);
                Assert.That(objv2.StringValue, Is.Null);
                Assert.That(objv2.UuidValue, Is.Null);
                Assert.That(objv2.BinaryValue, Is.Null);
            });
        }

        [Test]
        public void Model_CanRemoveField()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var differentiator = ObjectId.GenerateNewId();
                var realmv1 = await OpenRealm(differentiator, typeof(NullablesV1), schemaVersion: 1);

                var objv1 = realmv1.Write(() => realmv1.Add(new NullablesV1
                {
                    Differentiator = differentiator,
                    BoolValue = true,
                    DateValue = DateTimeOffset.UtcNow,
                    DecimalValue = Decimal128.MaxValue,
                    DoubleValue = 5.555,
                    IntValue = 123,
                    ObjectIdValue = ObjectId.GenerateNewId(),
                    StringValue = "foo bar",
                    UuidValue = Guid.NewGuid(),
                    BinaryValue = Array.Empty<byte>(),
                    WillBeRemoved = "this should go away!"
                }));

                Assert.That(objv1.WillBeRemoved, Is.EqualTo("this should go away!"));

                await WaitForUploadAsync(realmv1);

                var realmv2 = await OpenRealm(differentiator, typeof(NullablesV0), schemaVersion: 2);
                var id2 = realmv2.Write(() => realmv2.Add(new NullablesV0
                {
                    Differentiator = differentiator
                })).Id;

                await WaitForUploadAsync(realmv2);
                await WaitForDownloadAsync(realmv1);

                var objv2 = realmv1.Find<NullablesV1>(id2)!;
                Assert.That(objv2.WillBeRemoved, Is.EqualTo(string.Empty));
            });
        }

        [Test]
        public void Migration_FailsWithFutureVersion()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var ex = await TestHelpers.AssertThrows<SessionException>(() => OpenRealm(ObjectId.GenerateNewId(), typeof(NullablesV0), schemaVersion: 3));
                Assert.That(ex.Message, Does.Contain("Client provided invalid schema version: client presented schema version \"3\" is greater than latest schema version \"2\""));
            });
        }

        [Test, Ignore("TODO: restore once https://mongodb.slack.com/archives/C04NACGT7J7/p1716383151806129 is resolved")]
        public void SameRealm_CanBeMigratedThroughConsecutiveVersions()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var differentiator = ObjectId.GenerateNewId();
                var realm = await OpenRealm(differentiator, typeof(NullablesV0), schemaVersion: 0);
                var id = ObjectId.GenerateNewId();
                realm.Write(() => realm.Add(new NullablesV0
                {
                    Id = id,
                    Differentiator = differentiator
                }));

                realm.Dispose();

                var configv1 = realm.Config.Clone();

                configv1.SchemaVersion = 1;
                configv1.Schema = new[]
                {
                    typeof(NullablesV1)
                };

                realm = await GetRealmAsync(configv1);

                var objv1 = realm.All<NullablesV1>().Single();

                Assert.That(objv1.Id, Is.EqualTo(id));
                Assert.That(objv1.Differentiator, Is.EqualTo(differentiator));
                Assert.That(objv1.BoolValue, Is.EqualTo(false));
                Assert.That(objv1.DateValue, Is.EqualTo(new DateTimeOffset(1, 1, 1, 0, 0, 0, TimeSpan.Zero)));
                Assert.That(objv1.DecimalValue, Is.EqualTo(Decimal128.Zero));
                Assert.That(objv1.DoubleValue, Is.EqualTo(0));
                Assert.That(objv1.IntValue, Is.EqualTo(0));
                Assert.That(objv1.ObjectIdValue, Is.EqualTo(ObjectId.Empty));
                Assert.That(objv1.StringValue, Is.EqualTo(string.Empty));
                Assert.That(objv1.UuidValue, Is.EqualTo(Guid.Empty));
                Assert.That(objv1.BinaryValue, Is.EqualTo(Array.Empty<byte>()));
                Assert.That(objv1.WillBeRemoved, Is.EqualTo(string.Empty));

                realm.Dispose();

                var configv2 = realm.Config.Clone();

                configv2.SchemaVersion = 2;
                configv2.Schema = new[]
                {
                    typeof(NullablesV0)
                };

                realm = await GetRealmAsync(configv2);
                var objv2 = realm.All<NullablesV0>().Single();

                Assert.That(objv2.Id, Is.EqualTo(id));
                Assert.That(objv2.Differentiator, Is.EqualTo(differentiator));
                Assert.That(objv2.BoolValue, Is.Null);
                Assert.That(objv2.DateValue, Is.Null);
                Assert.That(objv2.DecimalValue, Is.Null);
                Assert.That(objv2.DoubleValue, Is.Null);
                Assert.That(objv2.IntValue, Is.Null);
                Assert.That(objv2.ObjectIdValue, Is.Null);
                Assert.That(objv2.StringValue, Is.Null);
                Assert.That(objv2.UuidValue, Is.Null);
                Assert.That(objv2.BinaryValue, Is.Null);
            });
        }

        [Test, Ignore("TODO: restore once https://mongodb.slack.com/archives/C04NACGT7J7/p1716383151806129 is resolved")]
        public void SameRealm_CanBeMigratedSkippingVersions()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var differentiator = ObjectId.GenerateNewId();
                var realm = await OpenRealm(differentiator, typeof(NullablesV0), schemaVersion: 0);
                var id = ObjectId.GenerateNewId();
                realm.Write(() => realm.Add(new NullablesV0
                {
                    Id = id,
                    Differentiator = differentiator
                }));

                realm.Dispose();

                var configv2 = realm.Config.Clone();
                configv2.SchemaVersion = 2;

                realm = await GetRealmAsync(configv2);
                var objv2 = realm.All<NullablesV0>().Single();

                Assert.That(objv2.Id, Is.EqualTo(id));
                Assert.That(objv2.Differentiator, Is.EqualTo(differentiator));
                Assert.That(objv2.BoolValue, Is.Null);
                Assert.That(objv2.DateValue, Is.Null);
                Assert.That(objv2.DecimalValue, Is.Null);
                Assert.That(objv2.DoubleValue, Is.Null);
                Assert.That(objv2.IntValue, Is.Null);
                Assert.That(objv2.ObjectIdValue, Is.Null);
                Assert.That(objv2.StringValue, Is.Null);
                Assert.That(objv2.UuidValue, Is.Null);
                Assert.That(objv2.BinaryValue, Is.Null);
            });
        }
    }
}
