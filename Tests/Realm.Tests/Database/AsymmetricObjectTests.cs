////////////////////////////////////////////////////////////////////////////
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
using Realms.Sync.Exceptions;
using Realms.Tests.Sync;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class AsymmetricObjectTests : SyncTestBase
    {
        // name format: Action_OptionalCondition_Expectation


        // TODO andrea:
        // 1. STARTED, RAN INTO ISSUES - add server side check to see if content on server is fine
        // 2. DONE - enlarge Asymmetric test classes to have all types
        // 3. dynamic tests for AsymmetricObjects
        // 4. DONE - AsymmetricObject after being written in a transaction it'll raise an exception is accessed

        public static object[] SetAndGetValueCases =
        {
            new object[] { "CharProperty", '0', typeof(char) },
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
            new object[] { "DateTimeOffsetProperty", new DateTimeOffset(1956, 6, 1, 0, 0, 0, TimeSpan.Zero) },
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

        [Test]
        public void AddAsymmetricObj_NotInSchema_Throws()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var flxConfig = await GetFLXIntegrationConfigAsync();
                flxConfig.Schema = new[] { typeof(BasicAsymmetricObject) };
                using var realm = await GetFLXIntegrationRealmAsync(flxConfig: flxConfig);

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
        public void AddHugeAsymmetricObj()
        {
            const int ObjectSize = 1_000_000;

            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var objId = ObjectId.Empty;

                var flxConfig = await GetFLXIntegrationConfigAsync();
                flxConfig.Schema = new[] { typeof(HugeSyncAsymmetricObject) };
                using var realm = await GetFLXIntegrationRealmAsync(flxConfig: flxConfig);

                realm.Write(() =>
                {
                    var hugeObj = new HugeSyncAsymmetricObject(ObjectSize);
                    objId = hugeObj.Id;
                    realm.Add(hugeObj);
                });

                await realm.SyncSession.WaitForUploadAsync();

                try
                {

                    var mongoClient = flxConfig.User.GetMongoClient("BackingDB");
                    var db = mongoClient.GetDatabase("FLX_local");
                    var collection = db.GetCollection<HugeSyncAsymmetricObject>(nameof(HugeSyncAsymmetricObject));
                    var filter = new BsonDocument
                    {
                        {
                            "_id", new BsonDocument
                            {
                                { "$eq", objId }
                            }
                        }
                    };
                    HugeSyncAsymmetricObject[] documents = null;
                    do
                    {
                        documents = await collection.FindAsync(filter);
                        await Task.Delay(100);
                    }
                    while (documents.Length == 0);

                    Assert.That(documents.Single().Data.Count, Is.EqualTo(ObjectSize));
                }
                catch(Exception e)
                {
                    var gg = 3;
                }
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
                using var realm = await GetFLXIntegrationRealmAsync(flxConfig: flxConfig);

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
                    var _ = asymmetribObj.PartitionLike;
                }, "Attempted to access a detached row");
            });
        }

        [TestCaseSource(nameof(SetAndGetValueCases))]
        public void SetAndRemotelyReadValue(string propertyName, object propertyValue, Type type)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                Guid objId = Guid.Empty;
                var flxConfig = await GetFLXIntegrationConfigAsync();
                flxConfig.Schema = new[] { typeof(AsymmetricObjectWithAllTypes) };

                using var realm = await GetFLXIntegrationRealmAsync(flxConfig: flxConfig);

                realm.Write(() =>
                {
                    var asymmetricObjAllTypes = new AsymmetricObjectWithAllTypes { RequiredStringProperty = string.Empty };
                    objId = asymmetricObjAllTypes.Id;
                    TestHelpers.SetPropertyValue(asymmetricObjAllTypes, propertyName, propertyValue);
                    realm.Add(asymmetricObjAllTypes);
                });

                await realm.SyncSession.WaitForUploadAsync();
                Exception e = null;
                AsymmetricObjectWithAllTypes[] documents = null;

                do
                {
                    e = null;
                    var mongoClient = flxConfig.User.GetMongoClient("BackingDB");
                    var db = mongoClient.GetDatabase("FLX_local");
                    var collection = db.GetCollection<AsymmetricObjectWithAllTypes>(nameof(AsymmetricObjectWithAllTypes));
                    BsonDocument filter = null;
                    try
                    {

                        filter = new BsonDocument
                        {
                            {
                                propertyName, new BsonDocument
                                {
                                    new BsonElement("$eq", (char)propertyValue)//(object)propertyValue)
                                }
                            },
                        };

                        //{
                        //    {
                        //        "_id", new BsonDocument
                        //        {
                        //            { "$eq", new BsonBinaryData(objId, GuidRepresentation.Standard) }
                        //        }
                        //    },
                        //};


                        documents = await collection.FindAsync(filter);
                        await Task.Delay(2000);


                        Assert.That(TestHelpers.GetPropertyValue(documents.Single(), propertyName), Is.EqualTo(propertyValue));
                    }
                    catch (Exception ex)
                    {
                        e = ex;
                        var d = 1;
                    }
                } while (e != null || documents.Length == 0);


            }, 999999999);
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

                using var realm = await GetFLXIntegrationRealmAsync(flxConfig: flxConfig);

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

        [Test, NUnit.Framework.Explicit("Once Daniel Tabacaru's work is done on \"error actions\" this will be an \"application bug\" action: https://github.com/10gen/baas/blob/9f32d54aa79aff6dfb36a6c07742594e38b07441/realm/sync/protocol/protocol_errors.go#L439")]
        public void AsymmetricObjectInPbs_Throws()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var config = await GetIntegrationConfigAsync();
                config.Schema = new[] { typeof(BasicAsymmetricObject) };
                var tcs = new TaskCompletionSource<object>();

                config.OnSessionError = (session, error) =>
                {
                    Assert.That(error, Is.InstanceOf<SessionException>());
                    //Assert.That(error.ErrorCode, Is.EqualTo(ErrorCode.ApplicationBug));
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
