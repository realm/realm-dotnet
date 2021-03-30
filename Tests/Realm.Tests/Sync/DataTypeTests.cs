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
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using NUnit.Framework;

namespace Realms.Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class DataTypeSynchronizationTests : SyncTestBase
    {
        #region Boolean

        [Test]
        public void Set_Boolean() => TestSetCore(o => o.BooleanSet, true, false);

        [Test]
        public void Dict_Boolean() => TestDictionaryCore(o => o.BooleanDict, true, false);

        #endregion

        #region Byte

        [Test]
        public void Set_Byte() => TestSetCore(o => o.ByteSet, (byte)9, (byte)255);

        [Test]
        public void Dict_Byte() => TestDictionaryCore(o => o.ByteDict, (byte)9, (byte)255);

        #endregion

        #region Int16

        [Test]
        public void Set_Int16() => TestSetCore(o => o.Int16Set, (short)55, (short)987);

        [Test]
        public void Dict_Int16() => TestDictionaryCore(o => o.Int16Dict, (short)55, (short)987);

        #endregion

        #region Int32

        [Test]
        public void Set_Int32() => TestSetCore(o => o.Int32Set, 987, 123);

        [Test]
        public void Dict_Int32() => TestDictionaryCore(o => o.Int32Dict, 555, 666);

        #endregion

        #region Int64

        [Test]
        public void Set_Int64() => TestSetCore(o => o.Int64Set, 12345678910111213, 987654321);

        [Test]
        public void Dict_Int64() => TestDictionaryCore(o => o.Int64Dict, 9999999999999L, 1111111111111111111L);

        #endregion

        #region Byte

        [Test]
        public void Set_Double() => TestSetCore(o => o.DoubleSet, 123.456, 789.123);

        [Test]
        public void Dict_Double() => TestDictionaryCore(o => o.DoubleDict, 99999.555555, 8777778.12312456);

        #endregion

        #region Decimal

        // TODO: use more precise numbers once https://jira.mongodb.org/browse/REALMC-8475 is done.
        [Test]
        public void Set_Decimal() => TestSetCore(o => o.DecimalSet, 123.7777777777777m, 999.99999999999m);

        [Test]
        public void Dict_Decimal() => TestDictionaryCore(o => o.DecimalDict, 987654321.7777777m, 999.99999999999m);

        #endregion

        #region Decimal128

        // TODO: use more precise numbers once https://jira.mongodb.org/browse/REALMC-8475 is done.
        [Test]
        public void Set_Decimal128() => TestSetCore(o => o.Decimal128Set, 123.7777777777777m, 999.99999999999m);

        [Test]
        public void Dict_Decimal128() => TestDictionaryCore(o => o.Decimal128Dict, 1.123456789m, 987654321.7777m);

        #endregion

        #region ObjectId

        [Test]
        public void Set_ObjectId() => TestSetCore(o => o.ObjectIdSet, ObjectId.GenerateNewId(), ObjectId.GenerateNewId());

        [Test]
        public void Dict_ObjectId() => TestDictionaryCore(o => o.ObjectIdDict, ObjectId.GenerateNewId(), ObjectId.GenerateNewId());

        #endregion

        #region DateTimeOffset

        [Test]
        public void Set_DateTimeOffset() => TestSetCore(o => o.DateTimeOffsetSet, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        [Test]
        public void Dict_DateTimeOffset() => TestDictionaryCore(o => o.DateTimeOffsetDict, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        #endregion

        #region String

        [Test]
        public void Set_String() => TestSetCore(o => o.StringSet, "abc", "cde");

        [Test]
        public void Dict_String() => TestDictionaryCore(o => o.StringDict, "hohoho", string.Empty);

        #endregion

        #region Byte

        [Test]
        public void Set_Binary() => TestSetCore(o => o.ByteArraySet, TestHelpers.GetBytes(5), TestHelpers.GetBytes(6), (a, b) => a.SequenceEqual(b));

        [Test]
        public void Dict_Binary() => TestDictionaryCore(o => o.ByteArrayDict, TestHelpers.GetBytes(10), TestHelpers.GetBytes(15), (a, b) => a.SequenceEqual(b));

        #endregion

        #region Object

        [Test]
        public void Set_Object() => TestSetCore(o => o.ObjectSet, new IntPropertyObject { Int = 5 }, new IntPropertyObject { Int = 456 }, (a, b) => a.Int == b.Int);

        // TODO: implement when https://jira.mongodb.org/browse/REALMC-8495 is fixed
        //[Test]
        //public void Dict_Object() => TestDictionaryCore(o => o.BooleanDict, true, false);

        #endregion

        #region RealmValue

        [Test]
        public void Set_RealmValue1() => TestSetCore(o => o.RealmValueSet, "abc", 10);

        [Test]
        public void Set_RealmValue2() => TestSetCore(o => o.RealmValueSet, new ObjectId("5f63e882536de46d71877979"), new Guid("{F2952191-A847-41C3-8362-497F92CB7D24}"));

        [Test]
        public void Set_RealmValue3() => TestSetCore(o => o.RealmValueSet, new byte[] { 0, 1, 2 }, DateTimeOffset.FromUnixTimeSeconds(1616137641));

        [Test]
        public void Set_RealmValue4() => TestSetCore(o => o.RealmValueSet, true, new IntPropertyObject { Int = 10 });

        [Test]
        public void Dict_RealmValue1() => TestDictionaryCore(o => o.RealmValueDict, "abc", 10);

        [Test]
        public void Dict_RealmValue2() => TestDictionaryCore(o => o.RealmValueDict, new ObjectId("5f63e882536de46d71877979"), new Guid("{F2952191-A847-41C3-8362-497F92CB7D24}"));

        [Test]
        public void Dict_RealmValue3() => TestDictionaryCore(o => o.RealmValueDict, new byte[] { 0, 1, 2 }, DateTimeOffset.FromUnixTimeSeconds(1616137641));

        [Test]
        public void Dict_RealmValue4() => TestDictionaryCore(o => o.RealmValueDict, true, new IntPropertyObject { Int = 10 });

        #endregion

        private void TestSetCore<T>(Func<SyncCollectionsObject, ISet<T>> getter, T item1, T item2, Func<T, T, bool> equalsOverride = null)
        {
            if (equalsOverride == null)
            {
                equalsOverride = (a, b) => a.Equals(b);
            }

            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var partition = Guid.NewGuid().ToString();
                var realm1 = await GetIntegrationRealmAsync(partition);
                var realm2 = await GetIntegrationRealmAsync(partition);

                var obj1 = realm1.Write(() =>
                {
                    return realm1.Add(new SyncCollectionsObject());
                });

                await WaitForUploadAsync(realm1);
                await WaitForDownloadAsync(realm2);

                var obj2 = realm2.Find<SyncCollectionsObject>(obj1.Id);

                var set1 = getter(obj1);
                var set2 = getter(obj2);

                // Assert Add works from both sides
                realm1.Write(() =>
                {
                    set1.Add(item1);
                });

                await WaitForCollectionChangeAsync(set2.AsRealmCollection());

                Assert.That(set1, Is.EquivalentTo(set2).Using(equalsOverride));

                realm2.Write(() =>
                {
                    set2.Add(item2);
                });

                await WaitForCollectionChangeAsync(set1.AsRealmCollection());

                Assert.That(set1, Is.EquivalentTo(set2).Using(equalsOverride));

                // Assert Remove works
                realm2.Write(() =>
                {
                    set2.Remove(set2.First());
                });

                await WaitForCollectionChangeAsync(set1.AsRealmCollection());

                Assert.That(set1, Is.EquivalentTo(set2).Using(equalsOverride));

                // Assert Clear works
                realm1.Write(() =>
                {
                    set1.Clear();
                });

                await WaitForCollectionChangeAsync(set2.AsRealmCollection());

                Assert.That(set1, Is.Empty);
                Assert.That(set2, Is.Empty);
            }, ensureNoSessionErrors: true);
        }

        private void TestDictionaryCore<T>(Func<SyncCollectionsObject, IDictionary<string, T>> getter, T item1, T item2, Func<T, T, bool> equalsOverride = null)
        {
            var comparer = new Func<KeyValuePair<string, T>, KeyValuePair<string, T>, bool>((a, b) =>
            {
                return a.Key == b.Key && (equalsOverride?.Invoke(a.Value, b.Value) ?? a.Value.Equals(b.Value));
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

                await WaitForUploadAsync(realm1);
                await WaitForDownloadAsync(realm2);

                var obj2 = realm2.Find<SyncCollectionsObject>(obj1.Id);

                var dict1 = getter(obj1);
                var dict2 = getter(obj2);

                var key1 = "a";
                var key2 = "b";

                // Assert Add works from both sides
                realm1.Write(() =>
                {
                    dict1.Add(key1, item1);
                });

                await WaitForCollectionChangeAsync(dict2.AsRealmCollection());

                Assert.That(dict1, Is.EquivalentTo(dict2).Using(comparer));

                realm2.Write(() =>
                {
                    dict2[key2] = item2;
                });

                await WaitForCollectionChangeAsync(dict1.AsRealmCollection());

                Assert.That(dict1, Is.EquivalentTo(dict2).Using(comparer));

                // Assert Update works
                realm1.Write(() =>
                {
                    dict1[key1] = item2;
                });

                await WaitForCollectionChangeAsync(dict2.AsRealmCollection());

                Assert.That(dict2, Is.EquivalentTo(dict1).Using(comparer));

                // Assert Remove works
                realm2.Write(() =>
                {
                    dict2.Remove(key1);
                });

                await WaitForCollectionChangeAsync(dict1.AsRealmCollection());

                Assert.That(dict1, Is.EquivalentTo(dict2).Using(comparer));

                // Assert Clear works
                realm1.Write(() =>
                {
                    dict1.Clear();
                });

                await WaitForCollectionChangeAsync(dict2.AsRealmCollection());

                Assert.That(dict1, Is.Empty);
                Assert.That(dict2, Is.Empty);
            }, ensureNoSessionErrors: true);
        }

        private void TestPropertyCore<T>(Func<SyncAllTypesObject, T> getter, Action<SyncAllTypesObject, T> setter, T item1, T item2, Func<T, T,  bool> equalsOverride = null)
        {
            if (equalsOverride == null)
            {
                equalsOverride = (a, b) => a.Equals(b);
            }

            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var partition = Guid.NewGuid().ToString();
                var realm1 = await GetIntegrationRealmAsync(partition);
                var realm2 = await GetIntegrationRealmAsync(partition);

                var obj1 = realm1.Write(() =>
                {
                    return realm1.Add(new SyncAllTypesObject());
                });

                await WaitForUploadAsync(realm1);
                await WaitForDownloadAsync(realm2);

                var obj2 = realm2.Find<SyncAllTypesObject>(obj1.Id);

                realm1.Write(() =>
                {
                    setter(obj1, item1);
                });

                await WaitForPropertyChangedAsync(obj2);

                var prop1 = getter(obj1);
                var prop2 = getter(obj2);

                Assert.That(prop1, Is.EqualTo(prop2).Using<T>(equalsOverride));
                Assert.That(prop1, Is.EqualTo(item1).Using<T>(equalsOverride));

                realm1.Write(() =>
                {
                    setter(obj2, item2);
                });

                await WaitForPropertyChangedAsync(obj1);

                prop1 = getter(obj1);
                prop2 = getter(obj2);

                Assert.That(prop1, Is.EqualTo(prop2).Using<T>(equalsOverride));
                Assert.That(prop1, Is.EqualTo(item1).Using<T>(equalsOverride));
            }, ensureNoSessionErrors: true);
        }

        private static async Task WaitForPropertyChangedAsync(RealmObject realmObject, int timeout = 10 * 1000)
        {
            var tcs = new TaskCompletionSource<object>();
            realmObject.PropertyChanged += RealmObject_PropertyChanged;

            void RealmObject_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                if (e != null)
                {
                    tcs.TrySetResult(null);
                }
            }

            await tcs.Task.Timeout(timeout);
            realmObject.PropertyChanged -= RealmObject_PropertyChanged;  //TODO Need to check this for correctness
        }

        private static async Task WaitForCollectionChangeAsync<T>(IRealmCollection<T> collection, int timeout = 10 * 1000)
        {
            var tcs = new TaskCompletionSource<object>();
            using var token = collection.SubscribeForNotifications((collection, changes, error) =>
            {
                if (changes != null)
                {
                    tcs.TrySetResult(null);
                }
            });

            await tcs.Task.Timeout(timeout);
        }
    }
}
