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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baas;
using MongoDB.Bson;
using Nito.AsyncEx;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Realms.Exceptions;
using Realms.Extensions;
using Realms.Helpers;
using Realms.Logging;
using Realms.Sync;
using Realms.Sync.Exceptions;
using Realms.Tests.Database;

namespace Realms.Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class DataTypeSynchronizationTests : SyncTestBase
    {
        private readonly RealmValueWithCollections.RealmValueComparer _rvComparer = new();

        #region Boolean

        [Test]
        public void List_Boolean() => TestListCore(o => o.BooleanList, true, false);

        [Test]
        public void Set_Boolean() => TestSetCore(o => o.BooleanSet, true, false);

        [Test]
        public void Dict_Boolean() => TestDictionaryCore(o => o.BooleanDict, true, false);

        [Test]
        public void Property_Boolean() => TestPropertyCore(o => o.BooleanProperty, (o, rv) => o.BooleanProperty = rv, true, false);

        #endregion

        #region Byte

        [Test]
        public void List_Byte() => TestListCore(o => o.ByteList, (byte)9, (byte)255);

        [Test]
        public void Set_Byte() => TestSetCore(o => o.ByteSet, (byte)9, (byte)255);

        [Test]
        public void Dict_Byte() => TestDictionaryCore(o => o.ByteDict, (byte)9, (byte)255);

        [Test]
        public void Property_Byte() => TestPropertyCore(o => o.ByteProperty, (o, rv) => o.ByteProperty = rv, (byte)9, (byte)255);

        #endregion

        #region Int16

        [Test]
        public void List_Int16() => TestListCore(o => o.Int16List, (short)55, (short)987);

        [Test]
        public void Set_Int16() => TestSetCore(o => o.Int16Set, (short)55, (short)987);

        [Test]
        public void Dict_Int16() => TestDictionaryCore(o => o.Int16Dict, (short)55, (short)987);

        [Test]
        public void Property_Int16() => TestPropertyCore(o => o.Int16Property, (o, rv) => o.Int16Property = rv, (short)55, (short)987);

        #endregion

        #region Int32

        [Test]
        public void List_Int32() => TestListCore(o => o.Int32List, 987, 123);

        [Test]
        public void Set_Int32() => TestSetCore(o => o.Int32Set, 987, 123);

        [Test]
        public void Dict_Int32() => TestDictionaryCore(o => o.Int32Dict, 555, 666);

        [Test]
        public void Property_Int32() => TestPropertyCore(o => o.Int32Property, (o, rv) => o.Int32Property = rv, 987, 123);

        #endregion

        #region Int64

        [Test]
        public void List_Int64() => TestListCore(o => o.Int64List, 12345678910111213, 987654321);

        [Test]
        public void Set_Int64() => TestSetCore(o => o.Int64Set, 12345678910111213, 987654321);

        [Test]
        public void Dict_Int64() => TestDictionaryCore(o => o.Int64Dict, 9999999999999L, 1111111111111111111L);

        [Test]
        public void Property_Int64() => TestPropertyCore(o => o.Int64Property, (o, rv) => o.Int64Property = rv, 12345678910111213, 987654321);

        #endregion

        #region Byte

        [Test]
        public void List_Double() => TestListCore(o => o.DoubleList, 123.456, 789.123);

        [Test]
        public void Set_Double() => TestSetCore(o => o.DoubleSet, 123.456, 789.123);

        [Test]
        public void Dict_Double() => TestDictionaryCore(o => o.DoubleDict, 99999.555555, 8777778.12312456);

        [Test]
        public void Property_Double() => TestPropertyCore(o => o.DoubleProperty, (o, rv) => o.DoubleProperty = rv, 99999.555555, 8777778.12312456);

        #endregion

        #region Float

        [Test]
        public void List_Float() => TestListCore(o => o.FloatList, 43.24f, 0.4f);

        [Test]
        public void Set_Float() => TestSetCore(o => o.FloatSet, 43.24f, 0.4f);

        [Test]
        public void Dict_Float() => TestDictionaryCore(o => o.FloatDict, 43.24f, 0.4f);

        [Test]
        public void Property_Float() => TestPropertyCore(o => o.FloatProperty, (o, rv) => o.FloatProperty = rv, 43.24f, 0.4f);

        #endregion

        #region Decimal

        [Test]
        public void List_Decimal() => TestListCore(o => o.DecimalList, 123.7777772342322347777777m, 999.99222222999999999m);

        [Test]
        public void Set_Decimal() => TestSetCore(o => o.DecimalSet, 123.7777774444447777777m, 999.000099999999999m);

        [Test]
        public void Dict_Decimal() => TestDictionaryCore(o => o.DecimalDict, 987654321.7777777m, 999.99999999999999999999m);

        [Test]
        public void Property_Decimal() => TestPropertyCore(o => o.DecimalProperty, (o, rv) => o.DecimalProperty = rv, 987654321.7777777999999999999999999m, 999.99999999999m);

        #endregion

        #region Decimal128

        [Test]
        public void List_Decimal128() => TestListCore(o => o.Decimal128List, 123.7777771111111117777777m, 999.99999333333333999999m);

        [Test]
        public void Set_Decimal128() => TestSetCore(o => o.Decimal128Set, 123.777444447777777777m, 999.99999999999m);

        [Test]
        public void Dict_Decimal128() => TestDictionaryCore(o => o.Decimal128Dict, 1.123456789m, 987654321.77777777777777777777m);

        [Test]
        public void Property_Decimal128() => TestPropertyCore(o => o.Decimal128Property, (o, rv) => o.Decimal128Property = rv, 1.123456789m, 987654321.7777m);

        #endregion

        #region ObjectId

        [Test]
        public void List_ObjectId() => TestListCore(o => o.ObjectIdList, ObjectId.GenerateNewId(), ObjectId.GenerateNewId());

        [Test]
        public void Set_ObjectId() => TestSetCore(o => o.ObjectIdSet, ObjectId.GenerateNewId(), ObjectId.GenerateNewId());

        [Test]
        public void Dict_ObjectId() => TestDictionaryCore(o => o.ObjectIdDict, ObjectId.GenerateNewId(), ObjectId.GenerateNewId());

        [Test]
        public void Property_ObjectId() => TestPropertyCore(o => o.ObjectIdProperty, (o, rv) => o.ObjectIdProperty = rv, ObjectId.GenerateNewId(), ObjectId.GenerateNewId());

        #endregion

        #region DateTimeOffset

        [Test]
        public void List_DateTimeOffset() => TestListCore(o => o.DateTimeOffsetList, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        [Test]
        public void Set_DateTimeOffset() => TestSetCore(o => o.DateTimeOffsetSet, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        [Test]
        public void Dict_DateTimeOffset() => TestDictionaryCore(o => o.DateTimeOffsetDict, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        [Test]
        public void Property_DateTimeOffset() => TestPropertyCore(o => o.DateTimeOffsetProperty, (o, rv) => o.DateTimeOffsetProperty = rv, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        #endregion

        #region String

        [Test]
        public void List_String() => TestListCore(o => o.StringList, "abc", "cde");

        [Test]
        public void Set_String() => TestSetCore(o => o.StringSet, "abc", "cde");

        [Test]
        public void Dict_String() => TestDictionaryCore(o => o.StringDict, "hohoho", string.Empty);

        [Test]
        public void Property_String() => TestPropertyCore(o => o.StringProperty, (o, rv) => o.StringProperty = rv, "abc", "cde");

        #endregion

        #region Binary

        [Test]
        public void List_Binary() => TestListCore(o => o.ByteArrayList, TestHelpers.GetBytes(5), TestHelpers.GetBytes(6), (a, b) => a.SequenceEqual(b));

        [Test]
        public void Set_Binary() => TestSetCore(o => o.ByteArraySet, TestHelpers.GetBytes(5), TestHelpers.GetBytes(6), (a, b) => a.SequenceEqual(b));

        [Test]
        public void Dict_Binary() => TestDictionaryCore(o => o.ByteArrayDict, TestHelpers.GetBytes(10), TestHelpers.GetBytes(15), (a, b) => a.SequenceEqual(b));

        [Test]
        public void Property_Binary() => TestPropertyCore(o => o.ByteArrayProperty, (o, rv) => o.ByteArrayProperty = rv, TestHelpers.GetBytes(5), TestHelpers.GetBytes(10), (a, b) => a!.SequenceEqual(b!));

        #endregion

        #region Object

        [Test]
        public void List_Object() => TestListCore(o => o.ObjectList, new IntPropertyObject { Int = 5 }, new IntPropertyObject { Int = 456 }, (a, b) => a.Int == b.Int);

        [Test]
        public void Set_Object() => TestSetCore(o => o.ObjectSet, new IntPropertyObject { Int = 5 }, new IntPropertyObject { Int = 456 }, (a, b) => a.Int == b.Int);

        [Test]
        public void Dict_Object() => TestDictionaryCore(o => o.ObjectDict, new IntPropertyObject { Int = 5 }, new IntPropertyObject { Int = 456 }, (a, b) => a?.Int == b?.Int);

        #endregion

        #region EmbeddedObject

        [Test]
        public void List_EmbeddedObject() => TestListCore(o => o.EmbeddedObjectList, new EmbeddedIntPropertyObject { Int = 5 }, new EmbeddedIntPropertyObject { Int = 456 }, (a, b) => a.Int == b.Int);

        [Test]
        public void Dict_EmbeddedObject() => TestDictionaryCore(o => o.EmbeddedObjectDict, new EmbeddedIntPropertyObject { Int = 5 }, new EmbeddedIntPropertyObject { Int = 456 }, (a, b) => a?.Int == b?.Int);

        #endregion

        #region RealmValue

        public static readonly object[] RealmTestPrimitiveValues = new[]
        {
            new object[] { (RealmValue)"abc", (RealmValue)10 },
            new object[] { (RealmValue)new ObjectId("5f63e882536de46d71877979"), (RealmValue)new Guid("{F2952191-A847-41C3-8362-497F92CB7D24}") },
            new object[] { (RealmValue)new byte[] { 0, 1, 2 }, (RealmValue)DateTimeOffset.FromUnixTimeSeconds(1616137641) },
            new object[] { (RealmValue)true, RealmValue.Object(new IntPropertyObject { Int = 10 }) },
            new object[] { RealmValue.Null, (RealmValue)5m },
            new object[] { (RealmValue)12.5f, (RealmValue)15d },
        };

        public static readonly object[] RealmTestValuesWithCollections = RealmTestPrimitiveValues.Concat( new[]
        {
            new object[] { (RealmValue)12.5f, (RealmValue)15d }, new object[]
            {
                (RealmValue)new List<RealmValue>
                {
                    RealmValue.Null,
                    1,
                    true,
                    "string",
                    new byte[] { 0, 1, 2 },
                    new DateTimeOffset(1234, 5, 6, 7, 8, 9, TimeSpan.Zero),
                    1f,
                    2d,
                    3m,
                    new ObjectId("5f63e882536de46d71877979"),
                    Guid.Parse("3809d6d9-7618-4b3d-8044-2aa35fd02f31"),
                    // new InternalObject { IntProperty = 10, StringProperty = "brown" },
                    // innerList,
                    // innerDict,
                },
                (RealmValue)15d
            },
            new object[]
            {
                (RealmValue)new Dictionary<string, RealmValue>
                {
                    { "key1", RealmValue.Null },
                    { "key2", 1 },
                    { "key3", true },
                    { "key4", "string" },
                    { "key5", new byte[] { 0, 1, 2, 3 } },
                    { "key6", new DateTimeOffset(1234, 5, 6, 7, 8, 9, TimeSpan.Zero) },
                    { "key7", 1f },
                    { "key8", 2d },
                    { "key9", 3m },
                    { "key10", new ObjectId("5f63e882536de46d71877979") },
                    { "key11", Guid.Parse("3809d6d9-7618-4b3d-8044-2aa35fd02f31") },
                    // new InternalObject { IntProperty = 10, StringProperty = "brown" },
                    // innerList,
                    // innerDict,
                },
                (RealmValue)15d
            },
        }).ToArray();

        [TestCaseSource(nameof(RealmTestValuesWithCollections))]
        public void List_RealmValue(RealmValue first, RealmValue second) => TestListCore(o => o.RealmValueList,
            Clone(first), Clone(second), equalsOverride: RealmValueEquals);

        [TestCaseSource(nameof(RealmTestPrimitiveValues))]
        public void Set_RealmValue(RealmValue first, RealmValue second) => TestSetCore(o => o.RealmValueSet,
            Clone(first), Clone(second), equalsOverride: RealmValueEquals);

        [TestCaseSource(nameof(RealmTestValuesWithCollections))]
        public void Dict_RealmValue(RealmValue first, RealmValue second) => TestDictionaryCore(o => o.RealmValueDict,
            Clone(first), Clone(second), equalsOverride: RealmValueEquals);

        [TestCaseSource(nameof(RealmTestValuesWithCollections))]
        public void Property_RealmValue(RealmValue first, RealmValue second) => TestPropertyCore(
            o => o.RealmValueProperty, (o, rv) => o.RealmValueProperty = rv, Clone(first), Clone(second),
            equalsOverride: RealmValueEquals);

        #endregion

        private void TestListCore<T>(Func<SyncCollectionsObject, IList<T>> getter, T item1, T item2,
            Func<T, T, bool>? equalsOverride = null)
        {
            equalsOverride ??= (a, b) => a?.Equals(b) == true;

            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var partition = Guid.NewGuid().ToString();
                var realm1 = await GetIntegrationRealmAsync(partition);
                var realm2 = await GetIntegrationRealmAsync(partition);

                var obj1 = realm1.Write(() =>
                {
                    return realm1.Add(new SyncCollectionsObject());
                });

                var obj2 = await WaitForObjectAsync(obj1, realm2);

                var list1 = getter(obj1);
                var list2 = getter(obj2);

                // Assert Add works from both sides
                realm1.Write(() =>
                {
                    list1.Add(item1);
                });

                await WaitForCollectionAsync(list2, list1, equalsOverride, "add from 1 shows up in 2");

                realm2.Write(() =>
                {
                    list2.Add(item2);
                });

                await WaitForCollectionAsync(list1, list2, equalsOverride, "add from 2 shows up in 1");

                // Assert Remove works
                realm2.Write(() =>
                {
                    list2.Remove(list2.First());
                });

                await WaitForCollectionAsync(list1, list2, equalsOverride, "remove from 2 shows up in 1");

                // Assert Clear works
                realm1.Write(() =>
                {
                    list1.Clear();
                });

                await TestHelpers.WaitForConditionAsync(() => !list2.Any(), errorMessage: "clear from 1 shows up in 2");

                Assert.That(list1, Is.Empty);
                Assert.That(list2, Is.Empty);
            });
        }

        private void TestSetCore<T>(Func<SyncCollectionsObject, ISet<T>> getter, T item1, T item2, Func<T, T, bool>? equalsOverride = null)
        {
            equalsOverride ??= (a, b) => a?.Equals(b) == true;

            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var partition = Guid.NewGuid().ToString();
                var realm1 = await GetIntegrationRealmAsync(partition);
                var realm2 = await GetIntegrationRealmAsync(partition);

                var obj1 = realm1.Write(() =>
                {
                    return realm1.Add(new SyncCollectionsObject());
                });

                var obj2 = await WaitForObjectAsync(obj1, realm2);

                var set1 = getter(obj1);
                var set2 = getter(obj2);

                // Assert Add works from both sides
                realm1.Write(() =>
                {
                    set1.Add(item1);
                });

                await WaitForCollectionAsync(set2, set1, equalsOverride, "add from 1 shows  up in 2");

                realm2.Write(() =>
                {
                    set2.Add(item2);
                });

                await WaitForCollectionAsync(set1, set2, equalsOverride, "add from 2 shows up in 1");

                // Assert Remove works
                realm2.Write(() =>
                {
                    set2.Remove(set2.First());
                });

                await WaitForCollectionAsync(set1, set2, equalsOverride, "remove from 2 shows up in 1");

                // Assert Clear works
                realm1.Write(() =>
                {
                    set1.Clear();
                });

                await TestHelpers.WaitForConditionAsync(() => !set2.Any(), errorMessage: "clear from 1 shows up in 2");

                Assert.That(set1, Is.Empty);
                Assert.That(set2, Is.Empty);
            });
        }

        private void TestDictionaryCore<T>(Func<SyncCollectionsObject, IDictionary<string, T>> getter, T item1, T item2, Func<T, T, bool>? equalsOverride = null)
        {
            var comparer = new Func<KeyValuePair<string, T>, KeyValuePair<string, T>, bool>((a, b) =>
            {
                return a.Key == b.Key && (equalsOverride?.Invoke(a.Value, b.Value) ?? a.Value?.Equals(b.Value) == true);
            });

            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var partition = Guid.NewGuid().ToString();
                var realm1 = await GetIntegrationRealmAsync(partition);
                var realm2 = await GetIntegrationRealmAsync(partition);

                var obj1 = realm1.Write(() =>
                {
                    return realm1.Add(new SyncCollectionsObject());
                });

                var obj2 = await WaitForObjectAsync(obj1, realm2, "initial obj from 1 shows up in 2");

                var dict1 = getter(obj1);
                var dict2 = getter(obj2);

                var key1 = "a";
                var key2 = "b";

                // Assert Add works from both sides
                realm1.Write(() =>
                {
                    dict1.Add(key1, item1);
                });

                await WaitForCollectionAsync(dict2, dict1, comparer, "add from 1 shows up in 2");

                realm2.Write(() =>
                {
                    dict2[key2] = item2;
                });

                await WaitForCollectionAsync(dict1, dict2, comparer, "add from 2 shows up in 1");

                // Assert Update works
                // item2 might belong to realm2, so let's find the equivalent in realm1
                item2 = CloneOrLookup(item2, realm1);

                realm1.Write(() =>
                {
                    dict1[key1] = item2;
                });

                await WaitForCollectionAsync(dict2, dict1, comparer, "set from 1 shows up in 2");

                // Assert Remove works
                realm2.Write(() =>
                {
                    dict2.Remove(key1);
                });

                await WaitForCollectionAsync(dict1, dict2, comparer, "remove from 2 shows up in 1");

                // Assert Clear works
                realm1.Write(() =>
                {
                    dict1.Clear();
                });

                await TestHelpers.WaitForConditionAsync(() => !dict2.Any(), errorMessage: "clear from 1 shows up in 2");

                Assert.That(dict1, Is.Empty);
                Assert.That(dict2, Is.Empty);
            }, timeout: 60_000);
        }

        private void TestPropertyCore<T>(Func<SyncAllTypesObject, T> getter, Action<SyncAllTypesObject, T> setter,
            T item1, T item2, Func<T, T, bool>? equalsOverride = null)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var realm1 = await GetFLXIntegrationRealmAsync();

                realm1.Subscriptions.Update(() =>
                {
                    realm1.Subscriptions.Add(realm1.All<SyncAllTypesObject>());
                    realm1.Subscriptions.Add(realm1.All<IntPropertyObject>());
                });

                var realm2 = await GetFLXIntegrationRealmAsync();
                realm2.Subscriptions.Update(() =>
                {
                    realm2.Subscriptions.Add(realm2.All<SyncAllTypesObject>());
                    realm2.Subscriptions.Add(realm2.All<IntPropertyObject>());
                });

                var obj1 = realm1.Write(() =>
                {
                    return realm1.Add(new SyncAllTypesObject());
                });

                var obj2 = await WaitForObjectAsync(obj1, realm2);

                realm1.Write(() =>
                {
                    setter(obj1, item1);
                });

                await WaitForPropertyChangedAsync(obj2);

                var prop1 = getter(obj1);
                var prop2 = getter(obj2);

                Assert.That(item1, Is.EqualTo(prop1).Using(_rvComparer));
                Assert.That(prop1, Is.EqualTo(prop2).Using(_rvComparer));

                realm2.Write(() =>
                {
                    setter(obj2, item2);
                });

                await WaitForPropertyChangedAsync(obj1);

                prop1 = getter(obj1);
                prop2 = getter(obj2);

                Assert.That(item2, Is.EqualTo(prop2).Using(_rvComparer));
                Assert.That(prop2, Is.EqualTo(prop1).Using(_rvComparer));
            });
        }

        [Test]
        public void Bootstrap()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var realm1 = await GetFLXIntegrationRealmAsync();
                realm1.Subscriptions.Update(() =>
                {
                    realm1.Subscriptions.Add(realm1.All<SyncAllTypesObject>());
                    realm1.Subscriptions.Add(realm1.All<IntPropertyObject>());
                });

                var child = new IntPropertyObject();
                var valuesValue = new List<RealmValue>()
                {
                    1,
                    (RealmValue)"Realm",
                    (RealmValue)child,
                    (RealmValue)new List<RealmValue>() { 1, "Realm", child },
                    (RealmValue)new Dictionary<string, RealmValue>()
                    {
                        { "key1", 1 }, { "key2", "Realm" }, { "key3", child },
                    }
                };

                var (parentId, childId) = realm1.Write(() =>
                {
                    var parent = realm1.Add(new SyncAllTypesObject());
                    parent.StringProperty = "PARENT";
                    parent.ObjectProperty = child;
                    parent.RealmValueProperty = valuesValue;
                    return (parent.Id, child.Id);
                });

                await realm1.SyncSession.WaitForUploadAsync();
                realm1.Dispose();

                var realm2 = await GetFLXIntegrationRealmAsync();
                realm2.Subscriptions.Update(() =>
                {
                    realm2.Subscriptions.Add(realm2.All<SyncAllTypesObject>());
                    realm2.Subscriptions.Add(realm2.All<IntPropertyObject>());
                });
                await realm2.SyncSession.WaitForDownloadAsync();

                var syncedParent =
                    await TestHelpers.WaitForConditionAsync(() => realm2.FindCore<SyncAllTypesObject>(parentId),
                        o => o != null);
                var syncedChild =
                    await TestHelpers.WaitForConditionAsync(() => realm2.FindCore<IntPropertyObject>(childId),
                        o => o != null);
                var syncedValues = syncedParent.RealmValueProperty.AsList();
                Assert.AreEqual(valuesValue[0], syncedValues[0]);
                Assert.AreEqual(valuesValue[1], syncedValues[1]);
                Assert.AreEqual(childId, syncedValues[2].AsRealmObject<IntPropertyObject>().Id);
                var nestedExpectedList = valuesValue[3].AsList();
                var nestedSyncedList = syncedValues[3].AsList();
                Assert.AreEqual(nestedExpectedList[0], nestedSyncedList[0]);
                Assert.AreEqual(nestedExpectedList[1], nestedSyncedList[1]);
                Assert.AreEqual(childId, nestedSyncedList[2].AsRealmObject<IntPropertyObject>().Id);

                var nestedExpectedDictionary = valuesValue[4].AsDictionary();
                var nestedSyncedDictionary = syncedValues[4].AsDictionary();
                Assert.AreEqual(nestedExpectedDictionary["key1"], nestedSyncedDictionary["key1"]);
                Assert.AreEqual(nestedExpectedDictionary["key2"], nestedSyncedDictionary["key2"]);
                Assert.AreEqual(childId, nestedSyncedDictionary["key3"].AsRealmObject<IntPropertyObject>().Id);
            });
        }

        public static readonly IList<RealmValue> RealmValueCollectionTestValues = new List<RealmValue>()
        {
            (RealmValue)"abc",
            (RealmValue)new ObjectId("5f63e882536de46d71877979"),
            (RealmValue)new byte[] { 0, 1, 2 },
            (RealmValue)DateTimeOffset.FromUnixTimeSeconds(1616137641),
            (RealmValue)true,
            RealmValue.Null,
            (RealmValue)5m,
            (RealmValue)12.5f,
            (RealmValue)15d,
            (RealmValue)new List<RealmValue>
            {
                RealmValue.Null,
                1,
                true,
                "string",
                new byte[] { 0, 1, 2 },
                new DateTimeOffset(1234, 5, 6, 7, 8, 9, TimeSpan.Zero),
                1f,
                2d,
                3m,
                new ObjectId("5f63e882536de46d71877979"),
                Guid.Parse("3809d6d9-7618-4b3d-8044-2aa35fd02f31"),
                // new InternalObject { IntProperty = 10, StringProperty = "brown" },
                // innerList,
                // innerDict,
            },
            (RealmValue)new Dictionary<string, RealmValue>
            {
                { "key1", RealmValue.Null },
                { "key2", 1 },
                { "key3", true },
                { "key4", "string" },
                { "key5", new byte[] { 0, 1, 2, 3 } },
                { "key6", new DateTimeOffset(1234, 5, 6, 7, 8, 9, TimeSpan.Zero) },
                { "key7", 1f },
                { "key8", 2d },
                { "key9", 3m },
                { "key10", new ObjectId("5f63e882536de46d71877979") },
                { "key11", Guid.Parse("3809d6d9-7618-4b3d-8044-2aa35fd02f31") },
                // new InternalObject { IntProperty = 10, StringProperty = "brown" },
                // innerList,
                // innerDict,
            },
        };


        [Test]
        public void ListManipulations()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var realm1 = await GetFLXIntegrationRealmAsync();
                var realm2 = await GetFLXIntegrationRealmAsync();

                realm1.Subscriptions.Update(() => { realm1.Subscriptions.Add(realm1.All<SyncAllTypesObject>()); });
                realm2.Subscriptions.Update(() => { realm2.Subscriptions.Add(realm2.All<SyncAllTypesObject>()); });

                var obj1 = realm1.Write(() =>
                {
                    var o = realm1.Add(new SyncAllTypesObject());
                    o.RealmValueProperty = new List<RealmValue>();
                    return o;
                });

                await WaitForObjectAsync(obj1, realm2);

                // Append elements one by one and verify that they are synced
                foreach (var realmTestValue in RealmValueCollectionTestValues)
                {
                    realm1.Write(() =>
                    {
                        obj1.RealmValueProperty.AsList().Add(realmTestValue);
                    });
                    await realm1.SyncSession.WaitForUploadAsync();

                    await realm2.SyncSession.WaitForDownloadAsync();
                    var obj2 = await WaitForObjectAsync(obj1, realm2);
                    var expectedValues = obj1.RealmValueProperty.AsList();
                    var actualValues = obj2.RealmValueProperty.AsList();
                    Assert.That(expectedValues, Is.EqualTo(actualValues).Using(_rvComparer));
                }

                // Remove elements one by one and verify that changes are synced
                foreach (var realmTestValue in RealmValueCollectionTestValues)
                {
                    realm1.Write(() =>
                    {
                        obj1.RealmValueProperty.AsList().RemoveAt(0);
                    });
                    await realm1.SyncSession.WaitForUploadAsync();

                    await realm2.SyncSession.WaitForDownloadAsync();
                    var obj2 = await WaitForObjectAsync(obj1, realm2);
                    var expectedValues = obj1.RealmValueProperty.AsList();
                    var actualValues = obj2.RealmValueProperty.AsList();
                    Assert.That(expectedValues, Is.EqualTo(actualValues).Using(_rvComparer));
                }

                // Insert/override elements at index 0 and verify that changes are synced
                foreach (var realmTestValue in RealmValueCollectionTestValues)
                {
                    realm1.Write(() =>
                    {
                        if (obj1.RealmValueProperty.AsList().Count == 0)
                        {
                            obj1.RealmValueProperty.AsList().Insert(0, realmTestValue);
                        }
                        else
                        {
                            obj1.RealmValueProperty.AsList()[0] = realmTestValue;
                        }
                    });
                    await realm1.SyncSession.WaitForUploadAsync();

                    await realm2.SyncSession.WaitForDownloadAsync();
                    var obj2 = await WaitForObjectAsync(obj1, realm2);
                    var expectedValues = obj1.RealmValueProperty.AsList();
                    var actualValues = obj2.RealmValueProperty.AsList();
                    Assert.That(expectedValues, Is.EqualTo(actualValues).Using(_rvComparer));
                }
            });
        }

        [Test]
        public void DictionaryManipulations()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var realm1 = await GetFLXIntegrationRealmAsync();
                realm1.Subscriptions.Update(() =>
                {
                    realm1.Subscriptions.Add(realm1.All<SyncAllTypesObject>());
                });
                var realm2 = await GetFLXIntegrationRealmAsync();
                realm2.Subscriptions.Update(() =>
                {
                    realm2.Subscriptions.Add(realm2.All<SyncAllTypesObject>());
                });

                var obj1 = realm1.Write(() =>
                {
                    var o = realm1.Add(new SyncAllTypesObject());
                    o.RealmValueProperty = new Dictionary<string, RealmValue>();
                    return o;
                });

                await WaitForObjectAsync(obj1, realm2);
                foreach (var (index, realmTestValue) in RealmValueCollectionTestValues.Select((value, index) => (index, value)))
                {
                    realm1.Write(() => { obj1.RealmValueProperty.AsDictionary()[$"{index}"] = realmTestValue; });

                    await realm1.SyncSession.WaitForUploadAsync();
                    await realm2.SyncSession.WaitForDownloadAsync();
                    var obj2 = await WaitForObjectAsync(obj1, realm2);
                    var expectedValues = obj1.RealmValueProperty.AsDictionary();
                    var actualValues = obj2.RealmValueProperty.AsDictionary();
                    Assert.That(expectedValues, Is.EqualTo(actualValues).Using(_rvComparer));
                }

                foreach (var (index, realmTestvalue) in RealmValueCollectionTestValues.Select((value, index) => (index, value)))
                {
                    realm1.Write(() => { obj1.RealmValueProperty.AsDictionary().Remove($"{index}"); });
                    await realm1.SyncSession.WaitForUploadAsync();

                    await realm2.SyncSession.WaitForDownloadAsync();
                    var obj2 = await WaitForObjectAsync(obj1, realm2);
                    var expectedValues = obj1.RealmValueProperty.AsDictionary();
                    var actualValues = obj2.RealmValueProperty.AsDictionary();
                    Assert.That(expectedValues, Is.EqualTo(actualValues).Using(_rvComparer));
                }
            });
        }

        [Test]
        public void CollectionMerge()
        {
            Logger.LogLevel = LogLevel.All;
            Logger.Default = Logger.File("sync.log");
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var realm1 = await GetFLXIntegrationRealmAsync();
                realm1.Subscriptions.Update(() =>
                {
                    realm1.Subscriptions.Add(realm1.All<SyncAllTypesObject>());
                });
                var realm2 = await GetFLXIntegrationRealmAsync();
                realm2.Subscriptions.Update(() =>
                {
                    realm2.Subscriptions.Add(realm2.All<SyncAllTypesObject>());
                });

                var obj1 = realm1.Write(() =>
                {
                    var o = realm1.Add(new SyncAllTypesObject());
                    o.RealmValueProperty = new Dictionary<string, RealmValue>
                    {
                        { "list", new List<RealmValue> { 1, 2, 3 } },
                        { "dictionary", new Dictionary<string, RealmValue>() { { "key1", 1 } } },
                    };
                    return o;
                });

                var obj2 = await WaitForObjectAsync(obj1, realm2);

                realm1.SyncSession.Stop();
                realm2.SyncSession.Stop();

                realm1.Write(() =>
                {
                    var list = obj1.RealmValueProperty.AsDictionary()["list"].AsList();
                    list.RemoveAt(0);
                    list.Add(4);
                    var dictionary = obj1.RealmValueProperty.AsDictionary()["dictionary"].AsDictionary();
                    dictionary.Remove("key1");
                    dictionary["key2"] = 2;
                });
                realm2.Write(() =>
                {
                    var list = obj2.RealmValueProperty.AsDictionary()["list"].AsList();
                    list.RemoveAt(0);
                    list.Add(5);
                    var dictionary = obj2.RealmValueProperty.AsDictionary()["dictionary"].AsDictionary();
                    dictionary.Remove("key1");
                    dictionary["key3"] = 3;
                });

                realm1.SyncSession.Start();
                realm2.SyncSession.Start();

                await realm1.SyncSession.WaitForUploadAsync();
                await realm2.SyncSession.WaitForUploadAsync();
                await realm1.SyncSession.WaitForDownloadAsync();
                await realm2.SyncSession.WaitForDownloadAsync();

                var list1 = obj1.RealmValueProperty.AsDictionary()["list"].AsList();
                var dictionary1 = obj1.RealmValueProperty.AsDictionary()["dictionary"].AsDictionary();
                var list2 = obj1.RealmValueProperty.AsDictionary()["list"].AsList();
                var dictionary2 = obj1.RealmValueProperty.AsDictionary()["dictionary"].AsDictionary();

                Assert.That(list1, Contains.Value(2));
                Assert.That(list1, Contains.Value(3));
                Assert.That(list1, Contains.Value(4));
                Assert.That(list1, Contains.Value(5));
                Assert.That(list1, Is.EqualTo(list2).Using(_rvComparer));

                Assert.That(dictionary1, Contains.Key("key2"));
                Assert.That(dictionary1, Contains.Key("key3"));
                Assert.That(dictionary1, Is.EqualTo(dictionary2).Using(_rvComparer));
            });
        }

        private static RealmValue Clone(RealmValue original)
        {
            if (original.Type != RealmValueType.Object)
            {
                return original;
            }

            var robj = original.AsIRealmObject();
            var clone = (IRealmObjectBase)Activator.CreateInstance(robj.GetType())!;
            var properties = robj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.CanRead && !p.HasCustomAttribute<PrimaryKeyAttribute>());

            foreach (var prop in properties)
            {
                prop.SetValue(clone, prop.GetValue(robj));
            }

            return RealmValue.Object(clone);
        }

        private static T CloneOrLookup<T>(T value, Realm targetRealm)
        {
            // If embedded - we need to clone as it might already be assigned to a different property
            if (value is IEmbeddedObject eobj)
            {
                return Operator.Convert<IRealmObjectBase, T>(Clone(RealmValue.Object(eobj)).AsIRealmObject());
            }

            // If IRealmObject - we need to look up the existing equivalent in the correct realm
            if (value is IRealmObject robj)
            {
                // item2 belongs to realm2 - we want to look up the equivalent in realm1 to add it to dict1
                Assert.That(robj.GetObjectMetadata()!.Helper.TryGetPrimaryKeyValue(robj, out var pk), Is.True);
                var item2InRealm1 = targetRealm.DynamicApi.FindCore(robj.ObjectSchema!.Name, Operator.Convert<RealmValue>(pk))!;
                return Operator.Convert<IRealmObject, T>(item2InRealm1);
            }

            // If RealmValue that is holding an object, call CloneOrLookup
            if (value is RealmValue rvalue && rvalue.Type == RealmValueType.Object)
            {
                var cloned = CloneOrLookup(rvalue.AsIRealmObject(), targetRealm);
                return Operator.Convert<IRealmObjectBase, T>(cloned);
            }

            return value;
        }

        private static bool RealmValueEquals(RealmValue a, RealmValue b)
        {
            if (a.Equals(b))
            {
                return true;
            }

            // Special handling the object case as they might be "equivalent" but belonging to different realms
            if (a.Type != RealmValueType.Object || b.Type != RealmValueType.Object)
            {
                return false;
            }

            var objA = a.AsIRealmObject();
            var objB = b.AsIRealmObject();

            if (objA.GetType() != objB.GetType())
            {
                return false;
            }

            foreach (var prop in objA.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(o => o.CanWrite && o.CanRead))
            {
                if (prop.GetValue(objA)?.Equals(prop.GetValue(objB)) != true)
                {
                    return false;
                }
            }

            return true;
        }

        private static async Task WaitForPropertyChangedAsync(IRealmObject realmObject, int timeout = 10 * 1000)
        {
            var tcs = new TaskCompletionSource();
            (realmObject as INotifyPropertyChanged)!.PropertyChanged += RealmObject_PropertyChanged;

            void RealmObject_PropertyChanged(object? sender, PropertyChangedEventArgs? e)
            {
                if (e != null)
                {
                    tcs.TrySetResult();
                }
            }

            await tcs.Task.Timeout(timeout);
            (realmObject as INotifyPropertyChanged)!.PropertyChanged -= RealmObject_PropertyChanged;
        }

        private static async Task WaitForCollectionAsync<T>(IEnumerable<T> first, IEnumerable<T> second, Func<T, T, bool> comparer, string message)
        {
            comparer ??= EqualityComparer<T>.Default.Equals;

            await TestHelpers.WaitForConditionAsync(() => IsEquivalent(first, second, comparer), errorMessage: message);
            Assert.That(first, Is.EquivalentTo(second).Using(comparer));
        }

        private static bool IsEquivalent<T>(IEnumerable<T> first, IEnumerable<T> second, Func<T, T, bool> comparer)
        {
            var copy1 = first.ToList();
            var copy2 = second.ToList();

            while (copy1.Count > 0)
            {
                var item = copy1[0];
                copy1.RemoveAt(0);
                var success = false;
                for (var j = 0; j < copy2.Count; j++)
                {
                    if (comparer(copy2[j], item))
                    {
                        success = true;
                        copy2.RemoveAt(j);
                    }
                }

                if (!success)
                {
                    return false;
                }
            }

            return copy2.Count == 0;
        }
    }
}
