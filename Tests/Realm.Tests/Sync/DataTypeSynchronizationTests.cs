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
using System.Reflection;
using System.Threading.Tasks;
using MongoDB.Bson;
using NUnit.Framework;
using Realms.Helpers;

namespace Realms.Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    [RequiresBaas(EnsureNoSessionErrors = true)]
    public class DataTypeSynchronizationTests : SyncTestBase
    {
        #region Boolean

        [Test]
        public Task List_Boolean() => TestListCoreAsync(o => o.BooleanList, true, false);

        [Test]
        public Task Set_Boolean() => TestSetCoreAsync(o => o.BooleanSet, true, false);

        [Test]
        public Task Dict_Boolean() => TestDictionaryCoreAsync(o => o.BooleanDict, true, false);

        [Test]
        public Task Property_Boolean() => TestPropertyCoreAsync(o => o.BooleanProperty, (o, rv) => o.BooleanProperty = rv, true, false);

        #endregion

        #region Byte

        [Test]
        public Task List_Byte() => TestListCoreAsync(o => o.ByteList, (byte)9, (byte)255);

        [Test]
        public Task Set_Byte() => TestSetCoreAsync(o => o.ByteSet, (byte)9, (byte)255);

        [Test]
        public Task Dict_Byte() => TestDictionaryCoreAsync(o => o.ByteDict, (byte)9, (byte)255);

        [Test]
        public Task Property_Byte() => TestPropertyCoreAsync(o => o.ByteProperty, (o, rv) => o.ByteProperty = rv, (byte)9, (byte)255);

        #endregion

        #region Int16

        [Test]
        public Task List_Int16() => TestListCoreAsync(o => o.Int16List, (short)55, (short)987);

        [Test]
        public Task Set_Int16() => TestSetCoreAsync(o => o.Int16Set, (short)55, (short)987);

        [Test]
        public Task Dict_Int16() => TestDictionaryCoreAsync(o => o.Int16Dict, (short)55, (short)987);

        [Test]
        public Task Property_Int16() => TestPropertyCoreAsync(o => o.Int16Property, (o, rv) => o.Int16Property = rv, (short)55, (short)987);

        #endregion

        #region Int32

        [Test]
        public Task List_Int32() => TestListCoreAsync(o => o.Int32List, 987, 123);

        [Test]
        public Task Set_Int32() => TestSetCoreAsync(o => o.Int32Set, 987, 123);

        [Test]
        public Task Dict_Int32() => TestDictionaryCoreAsync(o => o.Int32Dict, 555, 666);

        [Test]
        public Task Property_Int32() => TestPropertyCoreAsync(o => o.Int32Property, (o, rv) => o.Int32Property = rv, 987, 123);

        #endregion

        #region Int64

        [Test]
        public Task List_Int64() => TestListCoreAsync(o => o.Int64List, 12345678910111213, 987654321);

        [Test]
        public Task Set_Int64() => TestSetCoreAsync(o => o.Int64Set, 12345678910111213, 987654321);

        [Test]
        public Task Dict_Int64() => TestDictionaryCoreAsync(o => o.Int64Dict, 9999999999999L, 1111111111111111111L);

        [Test]
        public Task Property_Int64() => TestPropertyCoreAsync(o => o.Int64Property, (o, rv) => o.Int64Property = rv, 12345678910111213, 987654321);

        #endregion

        #region Byte

        [Test]
        public Task List_Double() => TestListCoreAsync(o => o.DoubleList, 123.456, 789.123);

        [Test]
        public Task Set_Double() => TestSetCoreAsync(o => o.DoubleSet, 123.456, 789.123);

        [Test]
        public Task Dict_Double() => TestDictionaryCoreAsync(o => o.DoubleDict, 99999.555555, 8777778.12312456);

        [Test]
        public Task Property_Double() => TestPropertyCoreAsync(o => o.DoubleProperty, (o, rv) => o.DoubleProperty = rv, 99999.555555, 8777778.12312456);

        #endregion

        #region Float

        [Test]
        public Task List_Float() => TestListCoreAsync(o => o.FloatList, 43.24f, 0.4f);

        [Test]
        public Task Set_Float() => TestSetCoreAsync(o => o.FloatSet, 43.24f, 0.4f);

        [Test]
        public Task Dict_Float() => TestDictionaryCoreAsync(o => o.FloatDict, 43.24f, 0.4f);

        [Test]
        public Task Property_Float() => TestPropertyCoreAsync(o => o.FloatProperty, (o, rv) => o.FloatProperty = rv, 43.24f, 0.4f);

        #endregion

        #region Decimal

        [Test]
        public Task List_Decimal() => TestListCoreAsync(o => o.DecimalList, 123.7777772342322347777777m, 999.99222222999999999m);

        [Test]
        public Task Set_Decimal() => TestSetCoreAsync(o => o.DecimalSet, 123.7777774444447777777m, 999.000099999999999m);

        [Test]
        public Task Dict_Decimal() => TestDictionaryCoreAsync(o => o.DecimalDict, 987654321.7777777m, 999.99999999999999999999m);

        [Test]
        public Task Property_Decimal() => TestPropertyCoreAsync(o => o.DecimalProperty, (o, rv) => o.DecimalProperty = rv, 987654321.7777777999999999999999999m, 999.99999999999m);

        #endregion

        #region Decimal128

        [Test]
        public Task List_Decimal128() => TestListCoreAsync(o => o.Decimal128List, 123.7777771111111117777777m, 999.99999333333333999999m);

        [Test]
        public Task Set_Decimal128() => TestSetCoreAsync(o => o.Decimal128Set, 123.777444447777777777m, 999.99999999999m);

        [Test]
        public Task Dict_Decimal128() => TestDictionaryCoreAsync(o => o.Decimal128Dict, 1.123456789m, 987654321.77777777777777777777m);

        [Test]
        public Task Property_Decimal128() => TestPropertyCoreAsync(o => o.Decimal128Property, (o, rv) => o.Decimal128Property = rv, 1.123456789m, 987654321.7777m);

        #endregion

        #region ObjectId

        [Test]
        public Task List_ObjectId() => TestListCoreAsync(o => o.ObjectIdList, ObjectId.GenerateNewId(), ObjectId.GenerateNewId());

        [Test]
        public Task Set_ObjectId() => TestSetCoreAsync(o => o.ObjectIdSet, ObjectId.GenerateNewId(), ObjectId.GenerateNewId());

        [Test]
        public Task Dict_ObjectId() => TestDictionaryCoreAsync(o => o.ObjectIdDict, ObjectId.GenerateNewId(), ObjectId.GenerateNewId());

        [Test]
        public Task Property_ObjectId() => TestPropertyCoreAsync(o => o.ObjectIdProperty, (o, rv) => o.ObjectIdProperty = rv, ObjectId.GenerateNewId(), ObjectId.GenerateNewId());

        #endregion

        #region DateTimeOffset

        [Test]
        public Task List_DateTimeOffset() => TestListCoreAsync(o => o.DateTimeOffsetList, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        [Test]
        public Task Set_DateTimeOffset() => TestSetCoreAsync(o => o.DateTimeOffsetSet, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        [Test]
        public Task Dict_DateTimeOffset() => TestDictionaryCoreAsync(o => o.DateTimeOffsetDict, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        [Test]
        public Task Property_DateTimeOffset() => TestPropertyCoreAsync(o => o.DateTimeOffsetProperty, (o, rv) => o.DateTimeOffsetProperty = rv, DateTimeOffset.MinValue, DateTimeOffset.MaxValue);

        #endregion

        #region String

        [Test]
        public Task List_String() => TestListCoreAsync(o => o.StringList, "abc", "cde");

        [Test]
        public Task Set_String() => TestSetCoreAsync(o => o.StringSet, "abc", "cde");

        [Test]
        public Task Dict_String() => TestDictionaryCoreAsync(o => o.StringDict, "hohoho", string.Empty);

        [Test]
        public Task Property_String() => TestPropertyCoreAsync(o => o.StringProperty, (o, rv) => o.StringProperty = rv, "abc", "cde");

        #endregion

        #region Byte

        [Test]
        public Task List_Binary() => TestListCoreAsync(o => o.ByteArrayList, TestHelpers.GetBytes(5), TestHelpers.GetBytes(6), (a, b) => a.SequenceEqual(b));

        [Test]
        public Task Set_Binary() => TestSetCoreAsync(o => o.ByteArraySet, TestHelpers.GetBytes(5), TestHelpers.GetBytes(6), (a, b) => a.SequenceEqual(b));

        [Test]
        public Task Dict_Binary() => TestDictionaryCoreAsync(o => o.ByteArrayDict, TestHelpers.GetBytes(10), TestHelpers.GetBytes(15), (a, b) => a.SequenceEqual(b));

        [Test]
        public Task Property_Binary() => TestPropertyCoreAsync(o => o.ByteArrayProperty, (o, rv) => o.ByteArrayProperty = rv, TestHelpers.GetBytes(5), TestHelpers.GetBytes(10), (a, b) => a.SequenceEqual(b));

        #endregion

        #region Object

        [Test]
        public Task List_Object() => TestListCoreAsync(o => o.ObjectList, new IntPropertyObject { Int = 5 }, new IntPropertyObject { Int = 456 }, (a, b) => a.Int == b.Int);

        [Test]
        public Task Set_Object() => TestSetCoreAsync(o => o.ObjectSet, new IntPropertyObject { Int = 5 }, new IntPropertyObject { Int = 456 }, (a, b) => a.Int == b.Int);

        [Test]
        public Task Dict_Object() => TestDictionaryCoreAsync(o => o.ObjectDict, new IntPropertyObject { Int = 5 }, new IntPropertyObject { Int = 456 }, (a, b) => a.Int == b.Int);

        #endregion

        #region EmbeddedObject

        [Test]
        public Task List_EmbeddedObject() => TestListCoreAsync(o => o.EmbeddedObjectList, new EmbeddedIntPropertyObject { Int = 5 }, new EmbeddedIntPropertyObject { Int = 456 }, (a, b) => a.Int == b.Int);

        [Test]
        public Task Dict_EmbeddedObject() => TestDictionaryCoreAsync(o => o.EmbeddedObjectDict, new EmbeddedIntPropertyObject { Int = 5 }, new EmbeddedIntPropertyObject { Int = 456 }, (a, b) => a.Int == b.Int);

        #endregion

        #region RealmValue

        public static readonly object[] RealmTestValues = new[]
        {
            new object[] { (RealmValue)"abc", (RealmValue)10 },
            new object[] { (RealmValue)new ObjectId("5f63e882536de46d71877979"), (RealmValue)new Guid("{F2952191-A847-41C3-8362-497F92CB7D24}") },
            new object[] { (RealmValue)new byte[] { 0, 1, 2 }, (RealmValue)DateTimeOffset.FromUnixTimeSeconds(1616137641) },
            new object[] { (RealmValue)true, (RealmValue)new IntPropertyObject { Int = 10 } },
            new object[] { RealmValue.Null, (RealmValue)5m },
            new object[] { (RealmValue)12.5f, (RealmValue)15d },
        };

        [TestCaseSource(nameof(RealmTestValues))]
        public Task List_RealmValue(RealmValue first, RealmValue second) => TestListCoreAsync(o => o.RealmValueList, Clone(first), Clone(second), equalsOverride: RealmValueEquals);

        [TestCaseSource(nameof(RealmTestValues))]
        public Task Set_RealmValue(RealmValue first, RealmValue second) => TestSetCoreAsync(o => o.RealmValueSet, Clone(first), Clone(second), equalsOverride: RealmValueEquals);

        [TestCaseSource(nameof(RealmTestValues))]
        public Task Dict_RealmValue(RealmValue first, RealmValue second) => TestDictionaryCoreAsync(o => o.RealmValueDict, Clone(first), Clone(second), equalsOverride: RealmValueEquals);

        [TestCaseSource(nameof(RealmTestValues))]
        public Task Property_RealmValue(RealmValue first, RealmValue second) => TestPropertyCoreAsync(o => o.RealmValueProperty, (o, rv) => o.RealmValueProperty = rv, Clone(first), Clone(second), equalsOverride: RealmValueEquals);

        #endregion

        private async Task TestListCoreAsync<T>(Func<SyncCollectionsObject, IList<T>> getter, T item1, T item2, Func<T, T, bool> equalsOverride = null)
        {
            if (equalsOverride == null)
            {
                equalsOverride = (a, b) => a.Equals(b);
            }

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

            await WaitForCollectionChangeAsync(list2.AsRealmCollection());

            Assert.That(list1, Is.EquivalentTo(list2).Using(equalsOverride), "Add from list1 should arrive at list2");

            realm2.Write(() =>
            {
                list2.Add(item2);
            });

            await WaitForCollectionChangeAsync(list1.AsRealmCollection());

            Assert.That(list1, Is.EquivalentTo(list2).Using(equalsOverride), "Add from list2 should arrive at list1");

            // Assert Remove works
            realm2.Write(() =>
            {
                list2.Remove(list2.First());
            });

            await WaitForCollectionChangeAsync(list1.AsRealmCollection());

            Assert.That(list1, Is.EquivalentTo(list2).Using(equalsOverride), "Remove from list2 should arrive at list1");

            // Assert Clear works
            realm1.Write(() =>
            {
                list1.Clear();
            });

            await TestHelpers.WaitForConditionAsync(() => !list2.Any());

            Assert.That(list1, Is.Empty);
            Assert.That(list2, Is.Empty);
        }

        private async Task TestSetCoreAsync<T>(Func<SyncCollectionsObject, ISet<T>> getter, T item1, T item2, Func<T, T, bool> equalsOverride = null)
        {
            if (equalsOverride == null)
            {
                equalsOverride = (a, b) => a.Equals(b);
            }

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

            await TestHelpers.WaitForConditionAsync(() => !set2.Any());

            Assert.That(set1, Is.Empty);
            Assert.That(set2, Is.Empty);
        }

        private async Task TestDictionaryCoreAsync<T>(Func<SyncCollectionsObject, IDictionary<string, T>> getter, T item1, T item2, Func<T, T, bool> equalsOverride = null)
        {
            var comparer = new Func<KeyValuePair<string, T>, KeyValuePair<string, T>, bool>((a, b) =>
            {
                return a.Key == b.Key && (equalsOverride?.Invoke(a.Value, b.Value) ?? a.Value.Equals(b.Value));
            });

            var partition = Guid.NewGuid().ToString();
            var realm1 = await GetIntegrationRealmAsync(partition);
            var realm2 = await GetIntegrationRealmAsync(partition);

            var obj1 = realm1.Write(() =>
            {
                return realm1.Add(new SyncCollectionsObject());
            });

            var obj2 = await WaitForObjectAsync(obj1, realm2);

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
            // item2 might belong to realm2, so let's find the equivalent in realm1
            item2 = CloneOrLookup(item2, realm1);

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
        }

        private async Task TestPropertyCoreAsync<T>(Func<SyncAllTypesObject, T> getter, Action<SyncAllTypesObject, T> setter, T item1, T item2, Func<T, T, bool> equalsOverride = null)
        {
            if (equalsOverride == null)
            {
                equalsOverride = (a, b) => a.Equals(b);
            }

            var partition = Guid.NewGuid().ToString();
            var realm1 = await GetIntegrationRealmAsync(partition);
            var realm2 = await GetIntegrationRealmAsync(partition);

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

            Assert.That(equalsOverride(prop1, prop2), Is.True);
            Assert.That(equalsOverride(prop1, item1), Is.True);

            realm2.Write(() =>
            {
                setter(obj2, item2);
            });

            await WaitForPropertyChangedAsync(obj1);

            prop1 = getter(obj1);
            prop2 = getter(obj2);

            Assert.That(equalsOverride(prop1, prop2), Is.True);
            Assert.That(equalsOverride(prop2, item2), Is.True);
        }

        private static RealmValue Clone(RealmValue original)
        {
            if (original.Type != RealmValueType.Object)
            {
                return original;
            }

            var robj = original.AsRealmObject();
            var clone = (RealmObjectBase)Activator.CreateInstance(robj.GetType());
            var properties = robj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.CanRead && !p.HasCustomAttribute<PrimaryKeyAttribute>());

            foreach (var prop in properties)
            {
                prop.SetValue(clone, prop.GetValue(robj));
            }

            return clone;
        }

        private static T CloneOrLookup<T>(T value, Realm targetRealm)
        {
            // If embedded - we need to clone as it might already be assigned to a different property
            if (value is EmbeddedObject eobj)
            {
                return Operator.Convert<RealmObjectBase, T>(Clone(eobj).AsRealmObject());
            }

            // If RealmObject - we need to look up the existing equivalent in the correct realm
            if (value is RealmObject robj)
            {
                // item2 belongs to realm2 - we want to look up the equivalent in realm1 to add it to dict1
                Assert.That(robj.ObjectMetadata.Helper.TryGetPrimaryKeyValue(robj, out var pk), Is.True);
                var item2InRealm1 = targetRealm.DynamicApi.FindCore(robj.ObjectSchema.Name, Operator.Convert<RealmValue>(pk));
                return Operator.Convert<RealmObject, T>(item2InRealm1);
            }

            // If RealmValue that is holding an object, call CloneOrLookup
            if (value is RealmValue rvalue && rvalue.Type == RealmValueType.Object)
            {
                var cloned = CloneOrLookup(rvalue.AsRealmObject(), targetRealm);
                return Operator.Convert<RealmObjectBase, T>(cloned);
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

            var objA = a.AsRealmObject();
            var objB = b.AsRealmObject();

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
            realmObject.PropertyChanged -= RealmObject_PropertyChanged;
        }

        private static async Task WaitForCollectionChangeAsync<T>(IRealmCollection<T> collection, int timeout = 10 * 1000)
        {
            var tcs = new TaskCompletionSource<object>();
            using var token = collection.SubscribeForNotifications((collection, changes, error) =>
            {
                if (error != null)
                {
                    tcs.TrySetException(error);
                }

                if (changes != null)
                {
                    tcs.TrySetResult(null);
                }
            });

            await tcs.Task.Timeout(timeout);
        }
    }
}
