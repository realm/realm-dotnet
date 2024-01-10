////////////////////////////////////////////////////////////////////////////
//
// Copyright 2023 Realm Inc.
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Baas;
using MongoDB.Bson;
using NUnit.Framework;
using Realms.Schema;
using Realms.Sync;
using Realms.Sync.Exceptions;

using static Realms.Tests.TestHelpers;

namespace Realms.Tests.Sync
{
#if !TEST_WEAVER
    [TestFixture, Preserve(AllMembers = true)]
    public class StaticQueriesTests : SyncTestBase
    {
        private const string ServiceName = "BackingDB";

        [Test]
        public void RealmObjectAPI_Collections()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var props = SyncCollectionsObject.RealmSchema
                    .Where(p => p.Type.IsCollection(out _) && !p.Type.HasFlag(PropertyType.Object))
                    .ToArray();

                var collection = await GetCollection<SyncCollectionsObject>(AppConfigType.FlexibleSync);
                var obj1 = new SyncCollectionsObject();
                FillCollectionProps(obj1);

                var syncObj2 = new SyncCollectionsObject();
                FillCollectionProps(syncObj2);

                await collection.InsertOneAsync(obj1);

                using var realm = await GetFLXIntegrationRealmAsync();
                var syncObjects = await realm.All<SyncCollectionsObject>().Where(o => o.Id == obj1.Id || o.Id == syncObj2.Id).SubscribeAsync();
                await syncObjects.WaitForEventAsync((sender, _) => sender.Count > 0);

                var syncObj1 = syncObjects.Single();

                AssertProps(obj1, syncObj1);

                realm.Write(() => realm.Add(syncObj2));

                var filter = new { _id = syncObj2.Id };

                var obj2 = await WaitForConditionAsync(() => collection.FindOneAsync(filter), item => Task.FromResult(item != null));

                AssertProps(syncObj2, obj2);

                void FillCollectionProps(SyncCollectionsObject obj)
                {
                    foreach (var prop in props)
                    {
                        DataGenerator.FillCollection(obj.GetProperty<IEnumerable>(prop), 5);
                    }
                }

                void AssertProps(SyncCollectionsObject expected, SyncCollectionsObject actual)
                {
                    foreach (var prop in props)
                    {
                        var expectedProp = expected.GetProperty<IEnumerable>(prop);
                        var actualProp = actual.GetProperty<IEnumerable>(prop);

                        Assert.That(actualProp, Is.EquivalentTo(expectedProp).Using((object a, object e) => AreValuesEqual(a, e)), $"Expected collections to match for {prop.ManagedName}");
                    }
                }
            }, timeout: 120000);
        }

        public static readonly object[] PrimitiveTestCases = new[]
        {
            new object[] { CreateTestCase("Empty object", new SyncAllTypesObject()) },
            new object[]
            {
                CreateTestCase("All values", new SyncAllTypesObject
                {
                    BooleanProperty = true,
                    ByteArrayProperty = GetBytes(5),
                    ByteProperty = 255,
                    CharProperty = 'C',
                    DateTimeOffsetProperty = new DateTimeOffset(638380790696454240, TimeSpan.Zero),
                    Decimal128Property = 4932.539258328M,
                    DecimalProperty = 4884884883.99999999999M,
                    DoubleProperty = 34934.123456,
                    GuidProperty = Guid.NewGuid(),
                    Int16Property = 999,
                    Int32Property = 49394939,
                    Int64Property = 889898965342443,
                    ObjectIdProperty = ObjectId.GenerateNewId(),
                    RealmValueProperty = "this is a string",
                    StringProperty = "foo bar"
                })
            },
            new object[] { CreateTestCase("Bool RealmValue", new SyncAllTypesObject { RealmValueProperty = true }) },
            new object[] { CreateTestCase("Int RealmValue", new SyncAllTypesObject { RealmValueProperty = 123 }) },
            new object[] { CreateTestCase("Long RealmValue", new SyncAllTypesObject { RealmValueProperty = 9999999999 }) },
            new object[] { CreateTestCase("Null RealmValue", new SyncAllTypesObject { RealmValueProperty = RealmValue.Null }) },
            new object[] { CreateTestCase("String RealmValue", new SyncAllTypesObject { RealmValueProperty = "abc" }) },
            new object[] { CreateTestCase("Data RealmValue", new SyncAllTypesObject { RealmValueProperty = GetBytes(10) }) },
            new object[] { CreateTestCase("Float RealmValue", new SyncAllTypesObject { RealmValueProperty = 15.2f }) },
            new object[] { CreateTestCase("Double RealmValue", new SyncAllTypesObject { RealmValueProperty = -123.45678909876 }) },
            new object[] { CreateTestCase("Decimal RealmValue", new SyncAllTypesObject { RealmValueProperty = 1.1111111111111111111M }) },
            new object[] { CreateTestCase("Decimal RealmValue", new SyncAllTypesObject { RealmValueProperty = 1.1111111111111111111M }) },
            new object[] { CreateTestCase("Decimal128 RealmValue", new SyncAllTypesObject { RealmValueProperty = new Decimal128(2.1111111111111111111M) }) },
            new object[] { CreateTestCase("ObjectId RealmValue", new SyncAllTypesObject { RealmValueProperty = ObjectId.GenerateNewId() }) },
            new object[] { CreateTestCase("Guid RealmValue", new SyncAllTypesObject { RealmValueProperty = Guid.NewGuid() }) },
        };

        [TestCaseSource(nameof(PrimitiveTestCases))]
        public void RealmObjectAPI_Primitive_AtlasToRealm(TestCaseData<SyncAllTypesObject> testCase)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var props = SyncAllTypesObject.RealmSchema
                    .Where(p => !p.Type.HasFlag(PropertyType.Object))
                    .ToArray();

                var collection = await GetCollection<SyncAllTypesObject>(AppConfigType.FlexibleSync);
                var obj = testCase.Value;

                await collection.InsertOneAsync(obj);

                using var realm = await GetFLXIntegrationRealmAsync();
                var syncObjects = await realm.All<SyncAllTypesObject>().Where(o => o.Id == obj.Id).SubscribeAsync();
                await syncObjects.WaitForEventAsync((sender, _) => sender.Count > 0);

                var syncObj = syncObjects.Single();

                AssertProps(props, obj, syncObj);
            }, timeout: 120000);
        }

        [TestCaseSource(nameof(PrimitiveTestCases))]
        public void RealmObjectAPI_Primitive_RealmToAtlas(TestCaseData<SyncAllTypesObject> testCase)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var props = SyncAllTypesObject.RealmSchema
                    .Where(p => !p.Type.HasFlag(PropertyType.Object))
                    .ToArray();

                var collection = await GetCollection<SyncAllTypesObject>(AppConfigType.FlexibleSync);
                var obj = testCase.Value;

                using var realm = await GetFLXIntegrationRealmAsync();
                await realm.All<SyncAllTypesObject>().Where(o => o.Id == obj.Id).SubscribeAsync();
                realm.Write(() => realm.Add(obj));

                var filter = new { _id = obj.Id };

                var syncObj = await WaitForConditionAsync(() => collection.FindOneAsync(filter), item => Task.FromResult(item != null));

                AssertProps(props, obj, syncObj);
            }, timeout: 120000);
        }

        public static readonly object[] CounterTestCases = new[]
        {
            new object[]
            {
                CreateTestCase("All values", new CounterObject
                {
                    Id = 1,
                    ByteProperty = 255,
                    Int16Property = 999,
                    Int32Property = 49394939,
                    Int64Property = 889898965342443,
                    NullableByteProperty = 255,
                    NullableInt16Property = 999,
                    NullableInt32Property = 49394939,
                    NullableInt64Property = 889898965342443
                })
            },
            new object[]
            {
                CreateTestCase("Nullable values", new CounterObject
                {
                    Id = 2,
                    ByteProperty = 255,
                    Int16Property = 999,
                    Int32Property = 49394939,
                    Int64Property = 889898965342443,
                    NullableByteProperty = null,
                    NullableInt16Property = null,
                    NullableInt32Property = null,
                    NullableInt64Property = null,
                })
            },
        };

        [TestCaseSource(nameof(CounterTestCases))]
        public void RealmObjectAPI_Counter_AtlasToRealm(TestCaseData<CounterObject> testCase)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var props = CounterObject.RealmSchema.ToArray();

                var collection = await GetCollection<CounterObject>(AppConfigType.FlexibleSync);
                var obj = testCase.Value;

                await collection.InsertOneAsync(obj);

                using var realm = await GetFLXIntegrationRealmAsync();
                var syncObjects = await realm.All<CounterObject>().Where(o => o.Id == obj.Id).SubscribeAsync();
                await syncObjects.WaitForEventAsync((sender, _) => sender.Count > 0);

                var syncObj = syncObjects.Single();

                AssertProps(props, obj, syncObj);
            }, timeout: 120000);
        }

        [TestCaseSource(nameof(CounterTestCases))]
        public void RealmObjectAPI_Counter_RealmToAtlas(TestCaseData<CounterObject> testCase)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var props = CounterObject.RealmSchema.ToArray();

                var collection = await GetCollection<CounterObject>(AppConfigType.FlexibleSync);
                var obj = testCase.Value;

                using var realm = await GetFLXIntegrationRealmAsync();
                await realm.All<CounterObject>().Where(o => o.Id == obj.Id).SubscribeAsync();
                realm.Write(() => realm.Add(obj));

                var filter = new { _id = obj.Id };

                var syncObj = await WaitForConditionAsync(() => collection.FindOneAsync(filter), item => Task.FromResult(item != null));

                AssertProps(props, obj, syncObj);
            }, timeout: 120000);
        }

        public static readonly object[] AsymmetricTestCases = new[]
        {
            new object[]
            {
                CreateTestCase("Base", new BasicAsymmetricObject { PartitionLike = "testString" })
            },
        };

        [TestCaseSource(nameof(AsymmetricTestCases))]
        public void RealmObjectAPI_Asymmetric_RealmToAtlas(TestCaseData<BasicAsymmetricObject> testCase)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection<BasicAsymmetricObject>(AppConfigType.FlexibleSync);
                var obj = testCase.Value;
                var stringProperty = obj.PartitionLike;

                var filter = new { _id = obj.Id };

                using var realm = await GetFLXIntegrationRealmAsync();
                realm.Write(() => realm.Add(obj));

                var syncObj = await WaitForConditionAsync(() => collection.FindOneAsync(filter), item => Task.FromResult(item != null));

                Assert.That(stringProperty, Is.EqualTo(syncObj.PartitionLike));
            }, timeout: 120000);
        }

        public static readonly object[] ObjectTestCases = new[]
        {
            new object[]
            {
                CreateTestCase("All values", new SyncAllTypesObject
                {
                    ObjectProperty = new IntPropertyObject { Int = 23 },
                })
            },
        };

        [TestCaseSource(nameof(ObjectTestCases))]
        public void RealmObjectAPI_Object_AtlasToRealm(TestCaseData<SyncAllTypesObject> testCase)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var syncAllTypesCollection = await GetCollection<SyncAllTypesObject>(AppConfigType.FlexibleSync);
                var intPropertyCollection = await GetCollection<IntPropertyObject>(AppConfigType.FlexibleSync);

                var obj = testCase.Value;

                await syncAllTypesCollection.InsertOneAsync(obj);

                using var realm = await GetFLXIntegrationRealmAsync();
                var syncAllTypesObjects = await realm.All<SyncAllTypesObject>().Where(o => o.Id == obj.Id).SubscribeAsync();
                var intPropertyObjects = await realm.All<IntPropertyObject>().Where(o => o.Id == obj.ObjectProperty!.Id).SubscribeAsync();

                await syncAllTypesObjects.WaitForEventAsync((sender, _) => sender.Count > 0);

                var syncObj = syncAllTypesObjects.Single();

                // The object property is null, because we didn't add the object yet to Atlas
                Assert.That(syncObj.ObjectProperty, Is.Null);

                await intPropertyCollection.InsertOneAsync(obj.ObjectProperty!);
                await intPropertyObjects.WaitForEventAsync((sender, _) => sender.Count > 0);

                Assert.That(syncObj.ObjectProperty!.Id, Is.EqualTo(obj.ObjectProperty!.Id));
                Assert.That(syncObj.ObjectProperty!.Int, Is.EqualTo(obj.ObjectProperty!.Int));
            }, timeout: 120000);
        }

        [TestCaseSource(nameof(ObjectTestCases))]
        public void RealmObjectAPI_Object_RealmToAtlas(TestCaseData<SyncAllTypesObject> testCase)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var syncAllTypesCollection = await GetCollection<SyncAllTypesObject>(AppConfigType.FlexibleSync);
                var intPropertyCollection = await GetCollection<IntPropertyObject>(AppConfigType.FlexibleSync);

                var obj = testCase.Value;

                using var realm = await GetFLXIntegrationRealmAsync();
                await realm.All<SyncAllTypesObject>().Where(o => o.Id == obj.Id).SubscribeAsync();
                await realm.All<IntPropertyObject>().Where(o => o.Id == obj.ObjectProperty!.Id).SubscribeAsync();
                realm.Write(() => realm.Add(obj));

                var syncAllTypeObj = await WaitForConditionAsync(() => syncAllTypesCollection.FindOneAsync(new { _id = obj.Id }), item => Task.FromResult(item != null));
                var intPropertyObj = await WaitForConditionAsync(() => intPropertyCollection.FindOneAsync(new { _id = obj.ObjectProperty!.Id }), item => Task.FromResult(item != null));

                Assert.That(syncAllTypeObj.ObjectProperty!.Id, Is.EqualTo(obj.ObjectProperty!.Id));
                Assert.That(syncAllTypeObj.ObjectProperty!.Int, Is.Not.EqualTo(obj.ObjectProperty!.Int));

                Assert.That(intPropertyObj.Id, Is.EqualTo(obj.ObjectProperty.Id));
                Assert.That(intPropertyObj.Int, Is.EqualTo(obj.ObjectProperty.Int));
            }, timeout: 120000);
        }

        public static readonly object[] LinksTestCases = new[]
        {
            new object[]
            {
                CreateTestCase("Single link", new LinksObject("singleLink")
                {
                    Link = new("second") { Value = 2 },
                    Value = 1,
                }),
            },
            new object[]
            {
                CreateTestCase("List", new LinksObject("listLink")
                {
                    List =
                    {
                        new("list.1") { Value = 100 },
                        new("list.2") { Value = 200 },
                    },
                    Value = 987
                }),
            },
            new object[]
            {
                CreateTestCase("Dictionary", new LinksObject("dictLink")
                {
                    Dictionary =
                    {
                        ["key_1"] = new("dict.1") { Value = 100 },
                        ["key_null"] = null,
                        ["key_2"] = new("dict.2") { Value = 200 },
                    },
                    Value = 999
                })
            },
            new object[]
            {
                CreateTestCase("Set", new LinksObject("setLink")
                {
                    Set =
                    {
                        new("set.1") { Value = 100 },
                        new("set.2") { Value = 200 },
                    },
                    Value = 123
                }),
            },
            new object[]
            {
                CreateTestCase("All types", new LinksObject("parent")
                {
                    Value = 1,
                    Link = new("link") { Value = 2 },
                    List =
                    {
                        new("list.1") { Value = 3 },
                        new("list.2") { Value = 4 },
                    },
                    Set =
                    {
                        new("set.1") { Value = 5 },
                        new("set.2") { Value = 6 },
                    },
                    Dictionary =
                    {
                        ["dict_1"] = new("dict.1") { Value = 7 },
                        ["dict_2"] = new("dict.2") { Value = 8 },
                        ["dict_null"] = null
                    }
                }),
            }
        };

        [TestCaseSource(nameof(LinksTestCases))]
        public void RealmObjectAPI_Links_AtlasToRealm(TestCaseData<LinksObject> testCase)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection<LinksObject>(AppConfigType.FlexibleSync);

                var obj = testCase.Value;

                var elementsToInsert = obj.List.Concat(obj.Set).Concat(obj.Dictionary.Values.Where(d => d is not null)).Concat(new[] { obj });

                if (obj.Link is not null)
                {
                    elementsToInsert = elementsToInsert.Concat(new[] { obj.Link });
                }

                await collection.InsertManyAsync(elementsToInsert!);

                // How many objects we expect
                var totalCount = obj.List.Count + obj.Set.Count + obj.Dictionary.Where(d => d.Value != null).Count() + 1;

                using var realm = await GetFLXIntegrationRealmAsync();
                var linkObjs = await realm.All<LinksObject>().SubscribeAsync();

                await linkObjs.WaitForEventAsync((sender, _) => sender.Count >= totalCount);

                var linkObj = realm.Find<LinksObject>(obj.Id);

                AssertEqual(linkObj!.Link, obj.Link);

                Assert.That(linkObj.List.Count, Is.EqualTo(obj.List.Count));

                for (int i = 0; i < linkObj.List.Count; i++)
                {
                    AssertEqual(linkObj.List[i], obj.List[i]);
                }

                Assert.That(linkObj.Dictionary.Count, Is.EqualTo(obj.Dictionary.Count));

                foreach (var key in obj.Dictionary.Keys)
                {
                    Assert.That(linkObj.Dictionary.ContainsKey(key));
                    AssertEqual(linkObj.Dictionary[key], obj.Dictionary[key]);
                }

                Assert.That(linkObj.Set.Count, Is.EqualTo(obj.Set.Count));

                var orderedOriginalSet = obj.Set.OrderBy(a => a.Id).ToList();
                var orderedRetrievedSet = linkObj.Set.OrderBy(a => a.Id).ToList();

                for (int i = 0; i < orderedOriginalSet.Count; i++)
                {
                    AssertEqual(orderedRetrievedSet[i], orderedOriginalSet[i]);
                }

                static void AssertEqual(LinksObject? retrieved, LinksObject? original)
                {
                    if (original is null)
                    {
                        Assert.That(retrieved, Is.Null);
                    }
                    else
                    {
                        Assert.That(retrieved, Is.Not.Null);
                        Assert.That(retrieved!.Id, Is.EqualTo(original!.Id));
                        Assert.That(retrieved!.Value, Is.EqualTo(original!.Value));
                    }
                }
            }, timeout: 120000);
        }

        [TestCaseSource(nameof(LinksTestCases))]
        public void RealmObjectAPI_Links_RealmToAtlas(TestCaseData<LinksObject> testCase)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection<LinksObject>(AppConfigType.FlexibleSync);

                var obj = testCase.Value;

                using var realm = await GetFLXIntegrationRealmAsync();
                await realm.All<LinksObject>().SubscribeAsync();

                realm.Write(() => realm.Add(obj));
                await WaitForUploadAsync(realm);

                var linkObj = await WaitForConditionAsync(() => collection.FindOneAsync(new { _id = obj.Id }), item => Task.FromResult(item != null));

                await AssertEqual(collection, linkObj.Link, obj.Link);

                for (int i = 0; i < linkObj.List.Count; i++)
                {
                    await AssertEqual(collection, linkObj.List[i], obj.List[i]);
                }

                Assert.That(linkObj.Dictionary.Count, Is.EqualTo(obj.Dictionary.Count));

                foreach (var key in obj.Dictionary.Keys)
                {
                    Assert.That(linkObj.Dictionary.ContainsKey(key));
                    await AssertEqual(collection, linkObj.Dictionary[key], obj.Dictionary[key]);
                }

                Assert.That(linkObj.Set.Count, Is.EqualTo(obj.Set.Count));

                var orderedOriginalSet = obj.Set.OrderBy(a => a.Id).ToList();
                var orderedRetrievedSet = linkObj.Set.OrderBy(a => a.Id).ToList();

                for (int i = 0; i < orderedOriginalSet.Count; i++)
                {
                    await AssertEqual(collection, orderedRetrievedSet[i], orderedOriginalSet[i]);
                }

                static async Task AssertEqual(MongoClient.Collection<LinksObject> collection, LinksObject? partiallyRetrieved, LinksObject? original)
                {
                    if (original is null)
                    {
                        Assert.That(partiallyRetrieved, Is.Null);
                        return;
                    }

                    // The partiallyRetrieved object should contain only the id, and not other fields
                    Assert.That(partiallyRetrieved, Is.Not.Null);
                    Assert.That(partiallyRetrieved!.Id, Is.EqualTo(original.Id));
                    Assert.That(partiallyRetrieved.Value, Is.Not.EqualTo(original.Value));

                    var fullyRetrieved = await WaitForConditionAsync(() => collection.FindOneAsync(new { _id = original.Id }), item => Task.FromResult(item != null));

                    Assert.That(fullyRetrieved.Id, Is.EqualTo(original.Id));
                    Assert.That(fullyRetrieved.Value, Is.EqualTo(original.Value));
                }
            }, timeout: 120000);
        }

        public static readonly object[] RealmValueLinkTestCases = new[]
        {
            new object[]
            {
                CreateTestCase("Single link", new RealmValueObject
                {
                    RealmValueProperty = new IntPropertyObject { Int = 2 },
                }),
            },
            new object[]
            {
                CreateTestCase("List", new RealmValueObject
                {
                    RealmValueList =
                    {
                        new IntPropertyObject { Int = 100 },
                        new IntPropertyObject { Int = 200 },
                    },
                }),
            },
            new object[]
            {
                CreateTestCase("Dictionary", new RealmValueObject
                {
                    RealmValueDictionary =
                    {
                        ["key_1"] = new IntPropertyObject { Int = 100 },
                        ["key_null"] = RealmValue.Null,
                        ["key_2"] = new IntPropertyObject { Int = 200 },
                    },
                })
            },
            new object[]
            {
                CreateTestCase("Set", new RealmValueObject
                {
                    RealmValueSet =
                    {
                        new IntPropertyObject { Int = 100 },
                        new IntPropertyObject { Int = 200 },
                    },
                }),
            },
            new object[]
            {
                CreateTestCase("All types", new RealmValueObject
                {
                    RealmValueProperty = new IntPropertyObject { Int = 2 },
                    RealmValueList =
                    {
                        new IntPropertyObject { Int = 3 },
                        new IntPropertyObject { Int = 4 },
                    },
                    RealmValueSet =
                    {
                        new IntPropertyObject { Int = 5 },
                        new IntPropertyObject { Int = 6 },
                    },
                    RealmValueDictionary =
                    {
                        ["dict_1"] = new IntPropertyObject { Int = 7 },
                        ["dict_2"] = new IntPropertyObject { Int = 8 },
                        ["dict_null"] = RealmValue.Null
                    }
                }),
            }
        };

        // TODO This is going to be fixed with https://jira.mongodb.org/browse/BAAS-27410
        [TestCaseSource(nameof(RealmValueLinkTestCases))]
        public void RealmObjectAPI_RealmValueLinks_AtlasToRealm(TestCaseData<RealmValueObject> testCase)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var realmValCollection = await GetCollection<RealmValueObject>(AppConfigType.FlexibleSync);
                var intCollection = await GetCollection<IntPropertyObject>(AppConfigType.FlexibleSync);

                var obj = testCase.Value;
                await realmValCollection.InsertOneAsync(obj);

                var elementsToInsert = obj.RealmValueList.Select(o => o.As<IntPropertyObject>())
                .Concat(obj.RealmValueSet.Select(o => o.As<IntPropertyObject>())
                .Concat(obj.RealmValueDictionary.Values.Select(o => o.As<IntPropertyObject>()).Where(v => v is not null)));

                if (obj.RealmValueProperty != RealmValue.Null)
                {
                    elementsToInsert = elementsToInsert.Concat(new[] { obj.RealmValueProperty.As<IntPropertyObject>() });
                }

                await intCollection.InsertManyAsync(elementsToInsert!);

                // How many objects we expect
                var totalCount = obj.RealmValueList.Count + obj.RealmValueSet.Count + obj.RealmValueDictionary.Where(d => d.Value != RealmValue.Null).Count();

                using var realm = await GetFLXIntegrationRealmAsync();
                var intObjs = await realm.All<RealmValueObject>().SubscribeAsync();
                var realmObjs = await realm.All<RealmValueObject>().SubscribeAsync();

                await intObjs.WaitForEventAsync((sender, _) => sender.Count >= totalCount);
                await realmObjs.WaitForEventAsync((sender, _) => sender.Count >= 1);

                var realmValObj = realm.Find<RealmValueObject>(obj.Id);

                AssertEqual(realmValObj!.RealmValueProperty, obj.RealmValueProperty);

                Assert.That(realmValObj.RealmValueList.Count, Is.EqualTo(obj.RealmValueList.Count));

                for (int i = 0; i < realmValObj.RealmValueList.Count; i++)
                {
                    AssertEqual(realmValObj.RealmValueList[i], obj.RealmValueList[i]);
                }

                Assert.That(realmValObj.RealmValueDictionary.Count, Is.EqualTo(obj.RealmValueDictionary.Count));

                foreach (var key in obj.RealmValueDictionary.Keys)
                {
                    Assert.That(realmValObj.RealmValueDictionary.ContainsKey(key));
                    AssertEqual(realmValObj.RealmValueDictionary[key], obj.RealmValueDictionary[key]);
                }

                Assert.That(realmValObj.RealmValueSet.Count, Is.EqualTo(obj.RealmValueSet.Count));

                var orderedOriginalSet = obj.RealmValueSet.OrderBy(a => a.As<IntPropertyObject>().Id).ToList();
                var orderedRetrievedSet = realmValObj.RealmValueSet.OrderBy(a => a.As<IntPropertyObject>().Id).ToList();

                for (int i = 0; i < orderedOriginalSet.Count; i++)
                {
                    AssertEqual(orderedRetrievedSet[i], orderedOriginalSet[i]);
                }

                static void AssertEqual(RealmValue retrieved, RealmValue original)
                {
                    if (original == RealmValue.Null)
                    {
                        Assert.That(retrieved, Is.EqualTo(RealmValue.Null));
                        return;
                    }

                    Assert.That(retrieved.Type, Is.EqualTo(RealmValueType.Object));
                    Assert.That(original.Type, Is.EqualTo(RealmValueType.Object));

                    var retrievedAsObj = retrieved.As<IntPropertyObject>();
                    var originalAsObj = original.As<IntPropertyObject>();

                    Assert.That(retrievedAsObj.Id, Is.EqualTo(originalAsObj.Id));
                    Assert.That(retrievedAsObj.Int, Is.EqualTo(originalAsObj.Int));
                }
            }, timeout: 120000);
        }

        [TestCaseSource(nameof(RealmValueLinkTestCases))]
        public void RealmObjectAPI_RealmValueLinks_RealmToAtlas(TestCaseData<RealmValueObject> testCase)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var realmValCollection = await GetCollection<RealmValueObject>(AppConfigType.FlexibleSync);
                var intCollection = await GetCollection<IntPropertyObject>(AppConfigType.FlexibleSync);

                var obj = testCase.Value;

                using var realm = await GetFLXIntegrationRealmAsync();
                await realm.All<RealmValueObject>().SubscribeAsync();
                await realm.All<IntPropertyObject>().SubscribeAsync();

                realm.Write(() => realm.Add(obj));
                await WaitForUploadAsync(realm);

                var realmValObj = await WaitForConditionAsync(() => realmValCollection.FindOneAsync(new { _id = obj.Id }), item => Task.FromResult(item != null));

                await AssertEqual(intCollection, realmValObj.RealmValueProperty, obj.RealmValueProperty);

                for (int i = 0; i < realmValObj.RealmValueList.Count; i++)
                {
                    await AssertEqual(intCollection, realmValObj.RealmValueList[i], obj.RealmValueList[i]);
                }

                Assert.That(realmValObj.RealmValueDictionary.Count, Is.EqualTo(obj.RealmValueDictionary.Count));

                foreach (var key in obj.RealmValueDictionary.Keys)
                {
                    Assert.That(realmValObj.RealmValueDictionary.ContainsKey(key));
                    await AssertEqual(intCollection, realmValObj.RealmValueDictionary[key], obj.RealmValueDictionary[key]);
                }

                Assert.That(realmValObj.RealmValueSet.Count, Is.EqualTo(obj.RealmValueSet.Count));

                var orderedOriginalSet = obj.RealmValueSet.OrderBy(a => a.As<IntPropertyObject>().Id).ToList();
                var orderedRetrievedSet = realmValObj.RealmValueSet.OrderBy(a => a.As<IntPropertyObject>().Id).ToList();

                for (int i = 0; i < orderedOriginalSet.Count; i++)
                {
                    await AssertEqual(intCollection, orderedRetrievedSet[i], orderedOriginalSet[i]);
                }

                static async Task AssertEqual(MongoClient.Collection<IntPropertyObject> collection, RealmValue retrieved, RealmValue original)
                {
                    if (original == RealmValue.Null)
                    {
                        Assert.That(retrieved, Is.EqualTo(RealmValue.Null));
                        return;
                    }

                    Assert.That(retrieved.Type, Is.EqualTo(RealmValueType.Object));
                    Assert.That(original.Type, Is.EqualTo(RealmValueType.Object));

                    var retrievedAsObj = retrieved.As<IntPropertyObject>();
                    var originalAsObj = original.As<IntPropertyObject>();

                    Assert.That(retrievedAsObj.Id, Is.EqualTo(originalAsObj.Id));
                    Assert.That(retrievedAsObj.Int, Is.Not.EqualTo(originalAsObj.Int));

                    var fullyRetrieved = await WaitForConditionAsync(() => collection.FindOneAsync(new { _id = originalAsObj.Id }), item => Task.FromResult(item != null));

                    Assert.That(fullyRetrieved.Id, Is.EqualTo(originalAsObj.Id));
                    Assert.That(fullyRetrieved.Int, Is.EqualTo(originalAsObj.Int));
                }
            }, timeout: 120000);
        }

        public static readonly object[] EmbeddedTestCases =
        {
            new object[]
            {
                CreateTestCase("Single", new ObjectWithEmbeddedProperties
                {
                    AllTypesObject = new()
                    {
                        BooleanProperty = true,
                        ByteArrayProperty = new byte[] { 1, 2, 3 },
                        DoubleProperty = 3.14,
                        Int32Property = 4,
                        StringProperty = "bla bla"
                    }
                })
            },
            new object[]
            {
                CreateTestCase("Recursive", new ObjectWithEmbeddedProperties
                {
                    RecursiveObject = new()
                    {
                        String = "Top",
                        Child = new()
                        {
                            String = "Middle",
                            Child = new()
                            {
                                String = "Bottom"
                            }
                        }
                    }
                })
            },

            new object[]
            {
                CreateTestCase("List", new ObjectWithEmbeddedProperties
                {
                    ListOfAllTypesObjects =
                    {
                        new()
                        {
                            BooleanProperty = true,
                            ByteArrayProperty = new byte[] { 1, 2, 3 },
                            DoubleProperty = 3.14,
                            Int32Property = 4,
                            StringProperty = "bla bla"
                        },
                        new()
                        {
                            BooleanProperty = false,
                            ByteArrayProperty = new byte[] { 4, 1, 2, 3 },
                            DoubleProperty = 6.14,
                            Int32Property = 6,
                            StringProperty = "oh oh"
                        }
                    },
                })
            },
            new object[]
            {
                CreateTestCase("Dictionary", new ObjectWithEmbeddedProperties
                {
                    DictionaryOfAllTypesObjects =
                    {
                        ["key1"] = new()
                        {
                            BooleanProperty = true,
                            ByteArrayProperty = new byte[] { 1, 2, 3 },
                            DoubleProperty = 3.14,
                            Int32Property = 4,
                            StringProperty = "bla bla"
                        },
                        ["key2"] = new()
                        {
                            BooleanProperty = false,
                            ByteArrayProperty = new byte[] { 4, 1, 2, 3 },
                            DoubleProperty = 6.14,
                            Int32Property = 6,
                            StringProperty = "oh oh"
                        },
                        ["key3"] = null,
                    },
                })
            },
            new object[]
            {
                CreateTestCase("All types", new ObjectWithEmbeddedProperties
                {
                    AllTypesObject = new()
                    {
                        BooleanProperty = true,
                        ByteArrayProperty = new byte[] { 1, 2, 3 },
                        DoubleProperty = 3.14,
                        Int32Property = 4,
                        StringProperty = "bla bla"
                    },
                    RecursiveObject = new()
                    {
                        String = "Top",
                        Child = new()
                        {
                            String = "Middle",
                            Child = new()
                            {
                                String = "Bottom"
                            }
                        }
                    },
                    ListOfAllTypesObjects =
                    {
                        new()
                        {
                            BooleanProperty = true,
                            ByteArrayProperty = new byte[] { 5, 1, 2, 3 },
                            DoubleProperty = 6.78,
                            Int32Property = 2,
                            StringProperty = "blas blas"
                        },
                        new()
                        {
                            BooleanProperty = false,
                            ByteArrayProperty = new byte[] { 4, 1, 2, 3 },
                            DoubleProperty = 6.14,
                            Int32Property = 6,
                            StringProperty = "oh oh"
                        }
                    },
                    DictionaryOfAllTypesObjects =
                    {
                        ["key1"] = new()
                        {
                            BooleanProperty = true,
                            ByteArrayProperty = new byte[] { 1, 6, 3 },
                            DoubleProperty = 3.14,
                            Int32Property = 4,
                            StringProperty = "hej hej"
                        },
                        ["key2"] = new()
                        {
                            BooleanProperty = false,
                            ByteArrayProperty = new byte[] { 4, 1, 6, 3 },
                            DoubleProperty = 16.14,
                            Int32Property = 62,
                            StringProperty = "oha oha"
                        },
                        ["key3"] = null,
                    },
                })
            }
        };

        [TestCaseSource(nameof(EmbeddedTestCases))]
        public void RealmObjectAPI_Embedded_AtlasToRealm(TestCaseData<ObjectWithEmbeddedProperties> testCase)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection<ObjectWithEmbeddedProperties>(AppConfigType.FlexibleSync);
                var obj = testCase.Value;

                await collection.InsertOneAsync(obj);

                using var realm = await GetFLXIntegrationRealmAsync();
                var syncObjects = await realm.All<ObjectWithEmbeddedProperties>().Where(o => o.PrimaryKey == obj.PrimaryKey).SubscribeAsync();
                await syncObjects.WaitForEventAsync((sender, _) => sender.Count > 0);

                var syncObj = syncObjects.Single();

                AssertEmbedded(syncObj, obj);
            }, timeout: 120000);
        }

        [TestCaseSource(nameof(EmbeddedTestCases))]
        public void RealmObjectAPI_Embedded_RealmToAtlas(TestCaseData<ObjectWithEmbeddedProperties> testCase)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection<ObjectWithEmbeddedProperties>(AppConfigType.FlexibleSync);
                var obj = testCase.Value;

                using var realm = await GetFLXIntegrationRealmAsync();
                await realm.All<ObjectWithEmbeddedProperties>().SubscribeAsync();

                realm.Write(() => realm.Add(obj));
                await WaitForUploadAsync(realm);

                var syncObj = await WaitForConditionAsync(() => collection.FindOneAsync(new { _id = obj.PrimaryKey }), item => Task.FromResult(item != null));

                AssertEmbedded(syncObj, obj);
            }, timeout: 120000);
        }

        [Test]
        public void RealmObjectAPI_ExtraFields_AtlasToRealm()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollectionAsBson(nameof(PrimaryKeyStringObject), AppConfigType.FlexibleSync);

                const string primaryKey = "primaryKeyVal";

                var doc = new BsonDocument
                {
                    { "_id", primaryKey },
                    { "Value", "myVal" },
                    { "ExtraField", "extraFieldVal" },
                    { "ExtraField2", new BsonDocument { { "inner", 22 } } },
                    { "ExtraField3", new BsonArray(new[] { 1, 2, 3, 4 }) },
                };

                await collection.InsertOneAsync(doc);

                using var realm = await GetFLXIntegrationRealmAsync();
                var syncObjects = await realm.All<PrimaryKeyStringObject>().Where(o => o.Id == primaryKey).SubscribeAsync();
                await syncObjects.WaitForEventAsync((sender, _) => sender.Count > 0);

                var syncObj = syncObjects.Single();

                Assert.That(syncObj, Is.Not.Null);
                Assert.That(syncObj.Value, Is.EqualTo(doc["Value"].AsString));
            }, timeout: 120000);
        }

        [Test]
        public void RealmObjectAPI_ExtraFields_IgnoredWhenUsingTypedCollection()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var app = App.Create(SyncTestHelpers.GetAppConfig(AppConfigType.FlexibleSync));
                var user = await GetUserAsync(app);

                var config = GetFLXIntegrationConfig(user);

                using var realm = await GetRealmAsync(config);
                var client = user.GetMongoClient(ServiceName);
                var db = client.GetDatabase(SyncTestHelpers.SyncMongoDBName(AppConfigType.FlexibleSync));

                const string collectionName = "test";
                var bsonCollection = db.GetCollection(collectionName);
                await bsonCollection.DeleteManyAsync(new object());

                const string primaryKey = "primaryKeyVal";

                var doc = new BsonDocument
                {
                    { "_id", primaryKey },
                    { "Value", "myVal" },
                    { "ExtraField", "extraFieldVal" },
                    { "ExtraField2", new BsonDocument { { "inner", 22 } } },
                    { "ExtraField3", new BsonArray(new[] { 1, 2, 3, 4 }) },
                };

                await bsonCollection.InsertOneAsync(doc);

                var typedCollection = db.GetCollection<PrimaryKeyStringObject>(collectionName);

                var retrieved = await typedCollection.FindOneAsync(new { _id = primaryKey });

                Assert.That(retrieved, Is.Not.Null);
                Assert.That(retrieved.Value, Is.EqualTo(doc["Value"].AsString));
            }, timeout: 120000);
        }

        [Test]
        public void RealmObjectAPI_MismatchedType_ThrowsOnInsertWhenCollectionInSchema()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollectionAsBson(nameof(PrimaryKeyStringObject), AppConfigType.FlexibleSync);

                const string primaryKey = "primaryKeyVal";

                var doc = new BsonDocument
                {
                    { "_id", primaryKey },
                    { "Value", ObjectId.GenerateNewId() }, // Wrong type
                };

                var ex = await TestHelpers.AssertThrows<AppException>(() => collection.InsertOneAsync(doc));
                Assert.That(ex.Message, Does.Contain("insert not permitted"));
            }, timeout: 120000);
        }

        [Test]
        public void RealmObjectAPI_MismatchedType_ThrowsWhenDeserialized()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var app = App.Create(SyncTestHelpers.GetAppConfig(AppConfigType.FlexibleSync));
                var user = await GetUserAsync(app);

                var config = GetFLXIntegrationConfig(user);

                using var realm = await GetRealmAsync(config);
                var client = user.GetMongoClient(ServiceName);
                var db = client.GetDatabase(SyncTestHelpers.SyncMongoDBName(AppConfigType.FlexibleSync));

                const string collectionName = "test";
                var bsonCollection = db.GetCollection(collectionName);
                await bsonCollection.DeleteManyAsync(new object());

                const string primaryKey = "primaryKeyVal";

                var doc = new BsonDocument
                {
                    { "_id", primaryKey },
                    { "Value", ObjectId.GenerateNewId() }, // Wrong type
                };

                await bsonCollection.InsertOneAsync(doc);

                var typedCollection = db.GetCollection<PrimaryKeyStringObject>(collectionName);

                var ex = await TestHelpers.AssertThrows<SerializationException>(() => typedCollection.FindOneAsync(new { _id = primaryKey }));
                Assert.That(ex.Message, Does.Contain("Error while deserializing property Value: Cannot deserialize a 'String' from BsonType 'ObjectId'"));
            }, timeout: 120000);
        }

        [Test]
        public void RealmObjectAPI_MissingField_ThrowsOnInsertWhenCollectionInSchema()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollectionAsBson(nameof(IntPropertyObject), AppConfigType.FlexibleSync);

                var primaryKey = ObjectId.GenerateNewId();

                var doc = new BsonDocument
                {
                    { "_id", primaryKey },
                    { "Int", 23 }, // Missing the GuidProperty field
                };

                var ex = await TestHelpers.AssertThrows<Realms.Sync.Exceptions.AppException>(() => collection.InsertOneAsync(doc));
                Assert.That(ex.Message, Does.Contain("insert not permitted"));
            }, timeout: 120000);
        }

        [Test]
        public void RealmObjectAPI_MissingField_GetsDefaultValueWhenDeserialized()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var app = App.Create(SyncTestHelpers.GetAppConfig(AppConfigType.FlexibleSync));
                var user = await GetUserAsync(app);

                var config = GetFLXIntegrationConfig(user);

                using var realm = await GetRealmAsync(config);
                var client = user.GetMongoClient(ServiceName);
                var db = client.GetDatabase(SyncTestHelpers.SyncMongoDBName(AppConfigType.FlexibleSync));

                const string collectionName = "test";
                var bsonCollection = db.GetCollection(collectionName);
                await bsonCollection.DeleteManyAsync(new object());

                var primaryKey = ObjectId.GenerateNewId();

                var doc = new BsonDocument
                {
                    { "_id", primaryKey },
                    { "Int", 23 }, // Missing the GuidProperty field
                };

                await bsonCollection.InsertOneAsync(doc);

                var typedCollection = db.GetCollection<IntPropertyObject>(collectionName);
                var retrieved = await typedCollection.FindOneAsync(new { _id = primaryKey });

                Assert.That(retrieved.Id, Is.EqualTo(primaryKey));
                Assert.That(retrieved.Int, Is.EqualTo(doc["Int"].AsInt32));
                Assert.That(retrieved.GuidProperty, Is.EqualTo(default(Guid)));
            }, timeout: 120000);
        }

        // Retrieves the MongoClient.Collection for a specific object type and removes everything that's eventually there already
        private async Task<MongoClient.Collection<T>> GetCollection<T>(string appConfigType = AppConfigType.Default)
            where T : class, IRealmObjectBase
        {
            var app = App.Create(SyncTestHelpers.GetAppConfig(appConfigType));
            var user = await GetUserAsync(app);

            SyncConfigurationBase config = appConfigType == AppConfigType.FlexibleSync ? GetFLXIntegrationConfig(user) : GetIntegrationConfig(user);

            using var realm = await GetRealmAsync(config);
            var client = user.GetMongoClient(ServiceName);
            var collection = client.GetCollection<T>();
            await collection.DeleteManyAsync(new object());

            return collection;
        }

        private async Task<MongoClient.Collection<BsonDocument>> GetCollectionAsBson(string collectionName, string appConfigType = AppConfigType.Default)
        {
            var app = App.Create(SyncTestHelpers.GetAppConfig(appConfigType));
            var user = await GetUserAsync(app);

            SyncConfigurationBase config = appConfigType == AppConfigType.FlexibleSync ? GetFLXIntegrationConfig(user) : GetIntegrationConfig(user);

            using var realm = await GetRealmAsync(config);
            var client = user.GetMongoClient(ServiceName);
            var db = client.GetDatabase(SyncTestHelpers.SyncMongoDBName(appConfigType));

            var collection = db.GetCollection(collectionName);
            await collection.DeleteManyAsync(new object());

            return collection;
        }

        private static void AssertEmbedded(ObjectWithEmbeddedProperties syncObj, ObjectWithEmbeddedProperties obj)
        {
            var props = EmbeddedAllTypesObject.RealmSchema
                 .Where(p => !p.Type.HasFlag(PropertyType.Object) && !p.Type.IsComputed())
                 .ToArray();

            AssertEquals(syncObj.AllTypesObject, obj.AllTypesObject);

            Assert.That(syncObj.ListOfAllTypesObjects.Count, Is.EqualTo(obj.ListOfAllTypesObjects.Count));

            for (int i = 0; i < obj.ListOfAllTypesObjects.Count; i++)
            {
                AssertEquals(syncObj.ListOfAllTypesObjects[i], obj.ListOfAllTypesObjects[i]);
            }

            Assert.That(syncObj.DictionaryOfAllTypesObjects.Count, Is.EqualTo(obj.DictionaryOfAllTypesObjects.Count));

            foreach (var key in obj.DictionaryOfAllTypesObjects.Keys)
            {
                Assert.That(syncObj.DictionaryOfAllTypesObjects.ContainsKey(key));
                AssertEquals(syncObj.DictionaryOfAllTypesObjects[key], obj.DictionaryOfAllTypesObjects[key]);
            }

            if (obj.RecursiveObject is null)
            {
                Assert.That(syncObj.RecursiveObject, Is.Null);
            }
            else
            {
                // For simplicity, if the top level is not null, then we assume the lower levels are not null either.
                Assert.That(syncObj.RecursiveObject, Is.Not.Null);

                Assert.That(syncObj.RecursiveObject!.String, Is.EqualTo(obj.RecursiveObject.String));
                Assert.That(syncObj.RecursiveObject.Child!.String, Is.EqualTo(obj.RecursiveObject.Child!.String));
                Assert.That(syncObj.RecursiveObject.Child.Child!.String, Is.EqualTo(obj.RecursiveObject.Child.Child!.String));
            }

            void AssertEquals(EmbeddedAllTypesObject? retrieved, EmbeddedAllTypesObject? original)
            {
                if (original is null)
                {
                    Assert.That(retrieved, Is.Null);
                }
                else
                {
                    Assert.That(retrieved, Is.Not.Null);
                    AssertProps(props, original, retrieved!);
                }
            }
        }

        private static void AssertProps(IEnumerable<Property> props, IRealmObjectBase expected, IRealmObjectBase actual)
        {
            foreach (var prop in props)
            {
                var expectedProp = expected.GetProperty<object>(prop);
                var actualProp = actual.GetProperty<object>(prop);

                AssertAreEqual(actualProp, expectedProp, $"property: {prop.Name}");
            }
        }
    }
#endif
}
