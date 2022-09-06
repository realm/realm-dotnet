﻿////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
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
using NUnit.Framework.Internal;
using Realms.Exceptions;
using Realms.Sync;
using Realms.Sync.Exceptions;
using Realms.Tests.Sync;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class AsymmetricObjectTests : SyncTestBase
    {
        public static object[] SetAndGetValueCases =
        {
            new object[] { "CharProperty", '0' },
            new object[] { "ByteProperty", (byte)100 },
            new object[] { "Int16Property", (short)100 },
            new object[] { "Int32Property", 100 },
            new object[] { "Int64Property", 100L },
            new object[] { "SingleProperty", 123.123f },
            new object[] { "DoubleProperty", 123.123 },
            new object[] { "BooleanProperty", true },
            new object[] { "ByteArrayProperty", new byte[] { 0xde, 0xad, 0xbe, 0xef } },
            new object[] { "ByteArrayProperty", Array.Empty<byte>() },
            new object[] { "StringProperty", "hello" },
            new object[] { "DecimalProperty", 123.456M },
            new object[] { "DecimalProperty", decimal.MinValue },
            new object[] { "DecimalProperty", decimal.MaxValue },
            new object[] { "DecimalProperty", decimal.One },
            new object[] { "DecimalProperty", decimal.MinusOne },
            new object[] { "DecimalProperty", decimal.Zero },
            new object[] { "Decimal128Property", new Decimal128(564.42343424323) },
            new object[] { "Decimal128Property", new Decimal128(decimal.MinValue) },
            new object[] { "Decimal128Property", new Decimal128(decimal.MaxValue) },
            new object[] { "Decimal128Property", Decimal128.MinValue },
            new object[] { "Decimal128Property", Decimal128.MaxValue },
            new object[] { "Decimal128Property", Decimal128.Zero },
            new object[] { "ObjectIdProperty", ObjectId.Empty },
            new object[] { "ObjectIdProperty", new ObjectId("5f63e882536de46d71877979") },
            new object[] { "GuidProperty", Guid.Empty },
            new object[] { "GuidProperty", Guid.Parse("{C4EC8CEF-D62A-405E-83BB-B0A3D8DABB36}") },
        };

        public static object[] SetAndReplaceWithNullCases =
        {
            new object[] { "NullableCharProperty", '0' },
            new object[] { "NullableByteProperty", (byte)100 },
            new object[] { "NullableInt16Property", (short)100 },
            new object[] { "NullableInt32Property", 100 },
            new object[] { "NullableInt64Property", 100L },
            new object[] { "NullableSingleProperty", 123.123f },
            new object[] { "NullableDoubleProperty", 123.123 },
            new object[] { "NullableBooleanProperty", true },
            new object[] { "NullableDecimalProperty", 123.456M },
            new object[] { "NullableDecimal128Property", new Decimal128(123.456) },
            new object[] { "ByteArrayProperty", new byte[] { 0xde, 0xad, 0xbe, 0xef } },
            new object[] { "ByteArrayProperty", Array.Empty<byte>() },
            new object[] { "StringProperty", "hello" },
            new object[] { "StringProperty", string.Empty },
            new object[] { "NullableObjectIdProperty", new ObjectId("5f63e882536de46d71877979") },
            new object[] { "NullableGuidProperty", Guid.Parse("{C4EC8CEF-D62A-405E-83BB-B0A3D8DABB36}") }
        };

        [Test]
        public void AddAsymmetricObjNotInSchema_Throws()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var flxConfig = await GetFLXIntegrationConfigAsync();
                using var realm = await GetRealmAsync(flxConfig);

                Assert.Throws<ArgumentException>(() =>
                {
                    realm.Write(() =>
                    {
                        realm.Add(new BasicAsymmetricObject());
                    });
                });
            });
        }

        [Test]
        public void AddCollectionOfAsymmetricObjs()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var flxConfig = await GetFLXIntegrationConfigAsync();
                flxConfig.Schema = new[] { typeof(BasicAsymmetricObject) };
                using var realm = await GetRealmAsync(flxConfig);
                var partitionLike = Guid.NewGuid().ToString();

                Assert.DoesNotThrow(() =>
                {
                    realm.Write(() =>
                    {
                        realm.Add(new BasicAsymmetricObject[]
                        {
                            new BasicAsymmetricObject { PartitionLike = partitionLike },
                            new BasicAsymmetricObject { PartitionLike = partitionLike },
                            new BasicAsymmetricObject { PartitionLike = partitionLike },
                            new BasicAsymmetricObject { PartitionLike = partitionLike },
                        });
                    });
                });

                await realm.SyncSession.WaitForUploadAsync();

                var documents = await GetObjFromRemoteThroughMongoClient<BasicAsymmetricObject>(
                    flxConfig.User, nameof(BasicAsymmetricObject.PartitionLike), partitionLike);

                Assert.That(documents.Length, Is.EqualTo(4));
                Assert.That(documents.Where(x => x.PartitionLike == partitionLike).Count, Is.EqualTo(4));
            });
        }

        [Test]
        public void AddHugeAsymmetricObj()
        {
            const int ObjectSize = 1_000_000;

            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var objId = ObjectId.Empty;

                var flxConfig = await GetFLXIntegrationConfigAsync();
                flxConfig.Schema = new[] { typeof(HugeSyncAsymmetricObject) };
                using var realm = await GetRealmAsync(flxConfig);

                realm.Write(() =>
                {
                    var hugeObj = new HugeSyncAsymmetricObject(ObjectSize);
                    objId = hugeObj.Id;
                    realm.Add(hugeObj);
                });

                await realm.SyncSession.WaitForUploadAsync();
                var documents = await GetObjFromRemoteThroughMongoClient<HugeSyncAsymmetricObject>(flxConfig.User, "_id", objId);
                Assert.That(documents.Single().Data.Count, Is.EqualTo(ObjectSize));
            });
        }

        [Test]
        public void AccessAsymmetricObjAfterAddedToRealm_Throws()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var partitionLike = Guid.NewGuid().ToString();
                var flxConfig = await GetFLXIntegrationConfigAsync();
                flxConfig.Schema = new[] { typeof(BasicAsymmetricObject) };
                using var realm = await GetRealmAsync(flxConfig);

                var asymmetribObj = new BasicAsymmetricObject
                {
                    PartitionLike = partitionLike
                };

                realm.Write(() =>
                {
                    realm.Add(asymmetribObj);

                });

                Assert.Throws<RealmInvalidObjectException>(() =>
                {
                    _ = asymmetribObj.PartitionLike;
                }, "Attempted to access a detached row");
            });
        }

        [Test]
        public void AddSameAsymmetricObjTwice_DoesntThrow()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var flxConfig = await GetFLXIntegrationConfigAsync();
                flxConfig.Schema = new[] { typeof(BasicAsymmetricObject) };
                using var realm = await GetRealmAsync(flxConfig);
                var partitionLike = Guid.NewGuid().ToString();
                var asymmetricObj = new BasicAsymmetricObject
                {
                    PartitionLike = partitionLike
                };

                Assert.DoesNotThrow(() =>
                {
                    realm.Write(() =>
                    {
                        realm.Add(asymmetricObj);
                    });

                    realm.Write(() =>
                    {
                        realm.Add(asymmetricObj);
                    });
                });

                await realm.SyncSession.WaitForUploadAsync();

                var foundObjects = await GetObjFromRemoteThroughMongoClient<BasicAsymmetricObject>(
                    flxConfig.User, nameof(BasicAsymmetricObject.PartitionLike), partitionLike);

                Assert.That(foundObjects.Single().PartitionLike, Is.EqualTo(partitionLike));
            });
        }

        [TestCaseSource(nameof(SetAndGetValueCases))]
        [TestCaseSource(nameof(SetAndReplaceWithNullCases))]
        public void SetAndRemotelyReadValue(string propertyName, object propertyValue)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                Guid objId = Guid.Empty;
                var flxConfig = await GetFLXIntegrationConfigAsync();
                flxConfig.Schema = new[] { typeof(AsymmetricObjectWithAllTypes) };

                using var realm = await GetRealmAsync(flxConfig);

                realm.Write(() =>
                {
                    var asymmetricObjAllTypes = new AsymmetricObjectWithAllTypes { RequiredStringProperty = string.Empty };
                    objId = asymmetricObjAllTypes.Id;
                    TestHelpers.SetPropertyValue(asymmetricObjAllTypes, propertyName, propertyValue);
                    realm.Add(asymmetricObjAllTypes);
                });

                await realm.SyncSession.WaitForUploadAsync();
                var documents = await GetObjFromRemoteThroughMongoClient<AsymmetricObjectWithAllTypes>(
                    flxConfig.User, "_id", BsonValue.Create(objId));

                foreach (var doc in documents)
                {
                    Assert.That(TestHelpers.GetPropertyValue(doc, propertyName), Is.EqualTo(propertyValue));
                }
            });
        }

        [Test]
        public void MixAddingObjectAsymmetricAndNot()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var partitionLike = Guid.NewGuid().ToString();
                var id = new Random().Next();
                var flxConfig = await GetFLXIntegrationConfigAsync();
                flxConfig.Schema = new[] { typeof(BasicAsymmetricObject), typeof(PrimaryKeyInt32Object) };

                flxConfig.PopulateInitialSubscriptions = (realm) =>
                {
                    var query = realm.All<PrimaryKeyInt32Object>().Where(n => n.Id == id);
                    realm.Subscriptions.Add(query);
                };

                using var realm = await GetRealmAsync(flxConfig);

                Assert.DoesNotThrow(() =>
                {
                    realm.Write(() =>
                    {
                        realm.Add(new BasicAsymmetricObject
                        {
                            PartitionLike = partitionLike
                        });

                        realm.Add(new PrimaryKeyInt32Object
                        {
                            Id = id
                        });
                    });
                });
            });
        }

        [Test]
        public void AsymmetricObjectInPbs_Throws()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var config = await GetIntegrationConfigAsync();
                config.Schema = new[] { typeof(BasicAsymmetricObject) };
                var tcs = new TaskCompletionSource<object>();

                config.OnSessionError = (session, error) =>
                {
                    try
                    {
                        Assert.That(error, Is.InstanceOf<SessionException>());
                        Assert.That(error.ErrorCode, Is.EqualTo(ErrorCode.InvalidSchemaChange));
                        Assert.That(error.Message.Contains("asymmetric tables are not supported for partition-based sync"), Is.True);
                    }
                    catch(Exception e)
                    {
                        tcs.TrySetException(e);
                    }

                    tcs.TrySetResult(null);
                };

                using var realm = await GetRealmAsync(config);

                await tcs.Task;
            });
        }

        [Test]
        public void AsymmetricObjectInLocalRealm_Throws()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var config = (RealmConfiguration)RealmConfiguration.DefaultConfiguration;
                config.Schema = new[] { typeof(BasicAsymmetricObject) };

                Exception ex = null;
                try
                {
                    using var realm = await GetRealmAsync(config);
                }
                catch (Exception e)
                {
                    ex = e;
                }

                Assert.That(ex, Is.InstanceOf<RealmSchemaValidationException>());
                Assert.That(ex.Message.Contains($"Asymmetric table \'{nameof(BasicAsymmetricObject)}\'"), Is.True);
            });
        }

        private static Task<T[]> GetObjFromRemoteThroughMongoClient<T>(User user, string remoteFieldName, BsonValue fieldValue, string mongoClientCondition = MongoClientCondition.Equality)
            where T : class
        {
            var mongoClient = user.GetMongoClient("BackingDB");
            var db = mongoClient.GetDatabase("FLX_local");
            var collection = db.GetCollection<T>(typeof(T).Name);
            var filter = new BsonDocument
            {
                {
                    remoteFieldName, new BsonDocument
                    {
                        { mongoClientCondition, fieldValue }
                    }
                }
            };
            return collection.FindAsync(filter);
        }

        private static class MongoClientCondition
        {
            public const string Equality = "$eq";
        }

        [Explicit]
        private class AsymmetricContainsEmbeddedObject : AsymmetricObject
        {
            [PrimaryKey, MapTo("_id")]
            public Guid Id { get; set; } = Guid.NewGuid();

            public string PartitionLike { get; set; }

            public EmbeddedIntPropertyObject InnerObj { get; set; }
        }

        [Explicit]
        private class BasicAsymmetricObject : AsymmetricObject
        {
            [PrimaryKey, MapTo("_id")]
            public Guid Id { get; set; } = Guid.NewGuid();

            public string PartitionLike { get; set; }
        }
    }
}
