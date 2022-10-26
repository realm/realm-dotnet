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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Realms.Dynamic;
using Realms.Exceptions;
using Realms.Sync;
using Realms.Sync.Exceptions;

namespace Realms.Tests.Sync
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

                await WaitForUploadAsync(realm);

                var documents = await GetRemoteObjects<BasicAsymmetricObject>(
                    flxConfig.User, nameof(BasicAsymmetricObject.PartitionLike), partitionLike);

                Assert.That(documents.Length, Is.EqualTo(4));
                Assert.That(documents.Where(x => x.PartitionLike == partitionLike).Count, Is.EqualTo(4));
            });
        }

        [Test]
        public void AddCollection_WithSomeObjectsAlreadyAdded_Throws()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var flxConfig = await GetFLXIntegrationConfigAsync();
                flxConfig.Schema = new[] { typeof(BasicAsymmetricObject) };
                using var realm = await GetRealmAsync(flxConfig);
                var partitionLike = Guid.NewGuid().ToString();

                Assert.Throws<ArgumentException>(() =>
                {
                    realm.Write(() =>
                    {
                        var doubleObj = new BasicAsymmetricObject { PartitionLike = partitionLike };
                        realm.Add(new BasicAsymmetricObject[]
                        {
                            new BasicAsymmetricObject { PartitionLike = partitionLike },
                            doubleObj,
                        });

                        realm.Add(new BasicAsymmetricObject[]
                        {
                            doubleObj,
                            new BasicAsymmetricObject { PartitionLike = partitionLike }
                        });
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
                ObjectId id = default;

                var flxConfig = await GetFLXIntegrationConfigAsync();
                flxConfig.Schema = new[] { typeof(AsymmetricObjectWithAllTypes) };
                using var realm = await GetRealmAsync(flxConfig);

                realm.Write(() =>
                {
                    var hugeObj = AsymmetricObjectWithAllTypes.CreateWithData(ObjectSize);
                    id = hugeObj.Id;
                    realm.Add(hugeObj);
                });

                await WaitForUploadAsync(realm);
                var documents = await GetRemoteObjects<AsymmetricObjectWithAllTypes>(flxConfig.User, "_id", BsonValue.Create(id));
                Assert.That(documents.Length, Is.EqualTo(1));
                Assert.That(documents[0].ByteArrayProperty.Count, Is.EqualTo(ObjectSize));
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

                Assert.That(asymmetribObj.IsManaged);
                Assert.That(asymmetribObj.IsValid, Is.False);

                var ex = Assert.Throws<RealmInvalidObjectException>(() => _ = asymmetribObj.PartitionLike);
                Assert.That(ex.Message.Contains("Attempted to access detached row"));
            });
        }

        [Test]
        public void AddSameAsymmetricObjTwice_Throws()
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

                realm.Write(() =>
                {
                    realm.Add(asymmetricObj);
                    Assert.Throws<ArgumentException>(() =>
                    {
                        realm.Add(asymmetricObj);
                    });
                });
            });
        }

        [TestCaseSource(nameof(SetAndGetValueCases))]
        [TestCaseSource(nameof(SetAndReplaceWithNullCases))]
        public void SetAndRemotelyReadValue(string propertyName, object propertyValue)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                ObjectId id = default;
                var flxConfig = await GetFLXIntegrationConfigAsync();
                flxConfig.Schema = new[] { typeof(AsymmetricObjectWithAllTypes) };

                using var realm = await GetRealmAsync(flxConfig);

                realm.Write(() =>
                {
                    var asymmetricObjAllTypes = new AsymmetricObjectWithAllTypes { RequiredStringProperty = string.Empty };
                    id = asymmetricObjAllTypes.Id;
                    TestHelpers.SetPropertyValue(asymmetricObjAllTypes, propertyName, propertyValue);
                    realm.Add(asymmetricObjAllTypes);
                });

                await WaitForUploadAsync(realm);
                var documents = await GetRemoteObjects<AsymmetricObjectWithAllTypes>(
                    flxConfig.User, "_id", BsonValue.Create(id));

                Assert.That(documents.Length, Is.EqualTo(1));
                Assert.That(TestHelpers.GetPropertyValue(documents.Single(), propertyName), Is.EqualTo(propertyValue));
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

                var ex = Assert.Throws<RealmSchemaValidationException>(() => GetRealm(config));
                Assert.That(ex.Message, Does.Contain($"Asymmetric table '{nameof(BasicAsymmetricObject)}' not allowed in partition based sync"));
            });
        }

        [Test]
        public void AsymmetricObjectInLocalRealm_Throws()
        {
            var config = (RealmConfiguration)RealmConfiguration.DefaultConfiguration;
            config.Schema = new[] { typeof(BasicAsymmetricObject) };

            var ex = Assert.Throws<RealmSchemaValidationException>(() => GetRealm(config));
            Assert.That(ex.Message, Does.Contain($"Asymmetric table '{nameof(BasicAsymmetricObject)}' not allowed in a local Realm"));
        }

        [Test]
        public void EmbeddedObject_WhenParentAccessed_ReturnsParent()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var flxConfig = await GetFLXIntegrationConfigAsync();
                flxConfig.Schema = new[] {
                    typeof(AsymmetricObjectWithEmbeddedRecursiveObject),
                    typeof(EmbeddedLevel1),
                    typeof(EmbeddedLevel2),
                    typeof(EmbeddedLevel3)
                };
                using var realm = await GetRealmAsync(flxConfig);

                var parent = new AsymmetricObjectWithEmbeddedRecursiveObject
                {
                    RecursiveObject = new EmbeddedLevel1
                    {
                        Child = new EmbeddedLevel2
                        {
                            Child = new EmbeddedLevel3()
                        }
                    }
                };

                realm.Write(() =>
                {
                    realm.Add(parent);

                    Assert.That(parent, Is.EqualTo(parent.RecursiveObject.Parent));

                    var firstChild = parent.RecursiveObject;
                    Assert.That(firstChild, Is.EqualTo(firstChild.Child.Parent));

                    var secondChild = firstChild.Child;
                    Assert.That(secondChild, Is.EqualTo(secondChild.Child.Parent));
                });
            });
        }

        [Test]
        public void EmbeddedObject_WhenParentAccessedInList_ReturnsParent()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var flxConfig = await GetFLXIntegrationConfigAsync();
                flxConfig.Schema = new[] { typeof(AsymmetricObjectWithEmbeddedListObject), typeof(EmbeddedIntPropertyObject) };
                using var realm = await GetRealmAsync(flxConfig);

                var parent = new AsymmetricObjectWithEmbeddedListObject();
                parent.EmbeddedListObject.Add(new EmbeddedIntPropertyObject());

                realm.Write(() =>
                {
                    realm.Add(parent);

                    Assert.That(parent, Is.EqualTo(parent.EmbeddedListObject.Single().Parent));
                });
            });
        }

        [Test]
        public void EmbeddedObject_WhenParentAccessedInDictionary_ReturnsParent()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var flxConfig = await GetFLXIntegrationConfigAsync();
                flxConfig.Schema = new[] { typeof(AsymmetricObjectWithEmbeddedDictionaryObject), typeof(EmbeddedIntPropertyObject) };
                using var realm = await GetRealmAsync(flxConfig);

                var parent = new AsymmetricObjectWithEmbeddedDictionaryObject();
                parent.EmbeddedDictionaryObject.Add("child", new EmbeddedIntPropertyObject());

                realm.Write(() =>
                {
                    realm.Add(parent);

                    Assert.That(parent, Is.EqualTo(parent.EmbeddedDictionaryObject["child"].Parent));
                });
            });
        }

        [Test]
        public void EmbeddedObjectUnmanaged_WhenParentAccessed_ReturnsNull()
        {
            var parent = new AsymmetricObjectWithEmbeddedRecursiveObject
            {
                RecursiveObject = new EmbeddedLevel1
                {
                    Child = new EmbeddedLevel2
                    {
                        Child = new EmbeddedLevel3()
                    }
                }
            };

            Assert.That(parent.RecursiveObject.Parent, Is.Null);

            var firstChild = parent.RecursiveObject;
            Assert.That(firstChild.Child.Parent, Is.Null);

            var secondChild = firstChild.Child;
            Assert.That(secondChild.Child.Parent, Is.Null);
        }

        [Test]
        public void NonEmbeddedObject_WhenParentAccessed_Throws()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var flxConfig = await GetFLXIntegrationConfigAsync();
                flxConfig.Schema = new[] { typeof(BasicAsymmetricObject) };
                using var realm = await GetRealmAsync(flxConfig);

                var topLevel = new BasicAsymmetricObject
                {
                    PartitionLike = Guid.NewGuid().ToString()
                };

                realm.Write(() =>
                {
                    realm.Add(topLevel);

                    // Objects not implementing IEmbeddedObject will not have the "Parent" field,
                    // but the "GetParent" method is still accessible on its accessor. It should
                    // throw as it should not be used for such objects.
                    Assert.Throws<InvalidOperationException>(() => ((IRealmObjectBase)topLevel).Accessor.GetParent());
                });
            });
        }

        [Test]
        public void DynamicAccess([Values(true, false)] bool isDynamic)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var flxConfig = await GetFLXIntegrationConfigAsync();
                flxConfig.IsDynamic = isDynamic;
                flxConfig.Schema = new[] { typeof(AsymmetricObjectWithAllTypes) };
                using var realm = await GetRealmAsync(flxConfig);

                realm.Write(() =>
                {
                    var asymmetricObj = (AsymmetricObject)(object)realm.DynamicApi.CreateObject(nameof(AsymmetricObjectWithAllTypes), ObjectId.GenerateNewId());

                    if (isDynamic)
                    {
                        Assert.That(asymmetricObj, Is.InstanceOf<DynamicAsymmetricObject>());
                    }
                    else
                    {
                        Assert.That(asymmetricObj, Is.InstanceOf<AsymmetricObjectWithAllTypes>());
                    }

                    asymmetricObj.DynamicApi.Set(nameof(AsymmetricObjectWithAllTypes.CharProperty), 'F');
                    asymmetricObj.DynamicApi.Set(nameof(AsymmetricObjectWithAllTypes.NullableCharProperty), 'o');
                    asymmetricObj.DynamicApi.Set(nameof(AsymmetricObjectWithAllTypes.StringProperty), "o");

                    Assert.That(asymmetricObj.DynamicApi.Get<char>(nameof(AllTypesObject.CharProperty)), Is.EqualTo('F'));
                    Assert.That(asymmetricObj.DynamicApi.Get<char?>(nameof(AllTypesObject.NullableCharProperty)), Is.EqualTo('o'));
                    Assert.That(asymmetricObj.DynamicApi.Get<string>(nameof(AllTypesObject.StringProperty)), Is.EqualTo("o"));
                });

#if !UNITY
                realm.Write(() =>
                {
                    dynamic asymmetricObj = realm.DynamicApi.CreateObject(nameof(AsymmetricObjectWithAllTypes), ObjectId.GenerateNewId());
                    if (isDynamic)
                    {
                        Assert.That(asymmetricObj, Is.InstanceOf<DynamicAsymmetricObject>());
                    }
                    else
                    {
                        Assert.That(asymmetricObj, Is.InstanceOf<AsymmetricObjectWithAllTypes>());
                    }

                    asymmetricObj.CharProperty = 'F';
                    asymmetricObj.NullableCharProperty = 'o';
                    asymmetricObj.StringProperty = "o";

                    Assert.That((char)asymmetricObj.CharProperty, Is.EqualTo('F'));
                    Assert.That((char)asymmetricObj.NullableCharProperty, Is.EqualTo('o'));
                    Assert.That(asymmetricObj.StringProperty, Is.EqualTo("o"));
                });
#endif
            });
        }

        private static Task<T[]> GetRemoteObjects<T>(User user, string remoteFieldName, BsonValue fieldValue)
            where T : class
        {
            var mongoClient = user.GetMongoClient("BackingDB");
            var db = mongoClient.GetDatabase(SyncTestHelpers.RemoteMongoDBName("FLX"));
            var collection = db.GetCollection<T>(typeof(T).Name);
            var filter = new BsonDocument
            {
                {
                    remoteFieldName, new BsonDocument
                    {
                        { "$eq", fieldValue }
                    }
                }
            };
            return collection.FindAsync(filter);
        }

        [Explicit]
        private class BasicAsymmetricObject : AsymmetricObject
        {
            [PrimaryKey, MapTo("_id")]
            public ObjectId Id { get; private set; } = ObjectId.GenerateNewId();

            public string PartitionLike { get; set; }
        }

        [Explicit]
        public class AsymmetricObjectWithAllTypes : AsymmetricObject
        {
            [PrimaryKey, MapTo("_id")]
            public ObjectId Id { get; private set; } = ObjectId.GenerateNewId();

            public char CharProperty { get; set; }

            public byte ByteProperty { get; set; }

            public short Int16Property { get; set; }

            public int Int32Property { get; set; }

            public long Int64Property { get; set; }

            public float SingleProperty { get; set; }

            public double DoubleProperty { get; set; }

            public bool BooleanProperty { get; set; }

            public decimal DecimalProperty { get; set; }

            public Decimal128 Decimal128Property { get; set; }

            public ObjectId ObjectIdProperty { get; set; }

            public Guid GuidProperty { get; set; }

            [Required]
            public string RequiredStringProperty { get; set; }

            public string StringProperty { get; set; }

            public byte[] ByteArrayProperty { get; set; }

            public char? NullableCharProperty { get; set; }

            public byte? NullableByteProperty { get; set; }

            public short? NullableInt16Property { get; set; }

            public int? NullableInt32Property { get; set; }

            public long? NullableInt64Property { get; set; }

            public float? NullableSingleProperty { get; set; }

            public double? NullableDoubleProperty { get; set; }

            public bool? NullableBooleanProperty { get; set; }

            public DateTimeOffset? NullableDateTimeOffsetProperty { get; set; }

            public decimal? NullableDecimalProperty { get; set; }

            public Decimal128? NullableDecimal128Property { get; set; }

            public ObjectId? NullableObjectIdProperty { get; set; }

            public Guid? NullableGuidProperty { get; set; }

            public static AsymmetricObjectWithAllTypes CreateWithData(int dataSize)
            {
                var data = new byte[dataSize];
                TestHelpers.Random.NextBytes(data);
                return new AsymmetricObjectWithAllTypes
                {
                    ByteArrayProperty = data,
                    RequiredStringProperty = string.Empty,
                };
            }

            // We can't test against the following types as they are not Bson deserializable

            // public DateTimeOffset DateTimeOffsetProperty { get; set; }

            // public RealmInteger<byte> ByteCounterProperty { get; set; }

            // public RealmInteger<short> Int16CounterProperty { get; set; }

            // public RealmInteger<int> Int32CounterProperty { get; set; }

            // public RealmInteger<long> Int64CounterProperty { get; set; }

            // public RealmValue RealmValueProperty { get; set; }
        }

        [Explicit]
        private class AsymmetricObjectWithEmbeddedListObject : AsymmetricObject
        {
            [PrimaryKey, MapTo("_id")]
            public ObjectId Id { get; private set; } = ObjectId.GenerateNewId();

            public IList<EmbeddedIntPropertyObject> EmbeddedListObject { get; }
        }

        [Explicit]
        private class AsymmetricObjectWithEmbeddedRecursiveObject : AsymmetricObject
        {
            [PrimaryKey, MapTo("_id")]
            public ObjectId Id { get; private set; } = ObjectId.GenerateNewId();

            public EmbeddedLevel1 RecursiveObject { get; set; }
        }

        [Explicit]
        private class AsymmetricObjectWithEmbeddedDictionaryObject : AsymmetricObject
        {
            [PrimaryKey, MapTo("_id")]
            public ObjectId Id { get; private set; } = ObjectId.GenerateNewId();

            public IDictionary<string, EmbeddedIntPropertyObject> EmbeddedDictionaryObject { get; }
        }
    }
}
