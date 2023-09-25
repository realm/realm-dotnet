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
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Realms.Helpers;
using Realms.Schema;

using static Realms.Tests.TestHelpers;

namespace Realms.Tests.Serialization
{
    [TestFixture(true)]
    [TestFixture(false)]
    public class SerializationTests : RealmInstanceTest
    {
        private readonly bool _managed;

        public SerializationTests(bool managed)
        {
            _managed = managed;
        }

        protected override void CustomSetUp()
        {
            SerializationHelper.Initialize();

            base.CustomSetUp();
        }

        public static readonly object[] ATOTestCases = new[]
        {
            new object[] { CreateTestCase("Empty object", new AllTypesObject()) },
            new object[]
            {
                CreateTestCase("All values", new AllTypesObject
                {
                    BooleanProperty = true,
                    ByteArrayProperty = TestHelpers.GetBytes(5),
                    ByteCounterProperty = 8,
                    ByteProperty = 255,
                    CharProperty = 'C',
                    DateTimeOffsetProperty = new DateTimeOffset(348984933, TimeSpan.Zero),
                    Decimal128Property = 4932.539258328M,
                    DecimalProperty = 4884884883.99999999999M,
                    DoubleProperty = 34934.123456,
                    GuidProperty = Guid.NewGuid(),
                    Int16CounterProperty = 256,
                    Int16Property = 999,
                    Int32CounterProperty = 99999999,
                    Int32Property = 49394939,
                    Int64CounterProperty = 99999999999999999,
                    Int64Property = 889898965342443,
                    NullableBooleanProperty = false,
                    NullableByteProperty = 255,
                    NullableCharProperty = 'W',
                    NullableDateTimeOffsetProperty = new DateTimeOffset(999999999, TimeSpan.Zero),
                    NullableDecimal128Property = -439583757.981723545234132M,
                    NullableDecimalProperty = -123456789.0987654321M,
                    NullableDoubleProperty = -939493523532,
                    NullableGuidProperty = Guid.NewGuid(),
                    NullableInt16Property = -123,
                    NullableInt32Property = -94329532,
                    NullableInt64Property = -92349592395923523,
                    NullableObjectIdProperty = ObjectId.GenerateNewId(),
                    NullableSingleProperty = -21412.12f,
                    ObjectIdProperty = ObjectId.GenerateNewId(),
                    RealmValueProperty = "this is a string",
                    RequiredStringProperty = "bla bla",
                    SingleProperty = 123.45f,
                    StringProperty = "foo bar"
                })
            },
            new object[] { CreateTestCase("Bool RealmValue", new AllTypesObject { RealmValueProperty = true }) },
            new object[] { CreateTestCase("Int RealmValue", new AllTypesObject { RealmValueProperty = 123 }) },
            new object[] { CreateTestCase("Long RealmValue", new AllTypesObject { RealmValueProperty = 9999999999 }) },
            new object[] { CreateTestCase("Null RealmValue", new AllTypesObject { RealmValueProperty = RealmValue.Null }) },
            new object[] { CreateTestCase("String RealmValue", new AllTypesObject { RealmValueProperty = "abc" }) },
            new object[] { CreateTestCase("Data RealmValue", new AllTypesObject { RealmValueProperty = TestHelpers.GetBytes(10) }) },
            new object[] { CreateTestCase("Float RealmValue", new AllTypesObject { RealmValueProperty = 15.2f }) },
            new object[] { CreateTestCase("Double RealmValue", new AllTypesObject { RealmValueProperty = -123.45678909876 }) },
            new object[] { CreateTestCase("Decimal RealmValue", new AllTypesObject { RealmValueProperty = 1.1111111111111111111M }) },
            new object[] { CreateTestCase("Decimal RealmValue", new AllTypesObject { RealmValueProperty = 1.1111111111111111111M }) },
            new object[] { CreateTestCase("Decimal128 RealmValue", new AllTypesObject { RealmValueProperty = new Decimal128(2.1111111111111111111M) }) },
            new object[] { CreateTestCase("ObjectId RealmValue", new AllTypesObject { RealmValueProperty = ObjectId.GenerateNewId() }) },
            new object[] { CreateTestCase("Guid RealmValue", new AllTypesObject { RealmValueProperty = Guid.NewGuid() }) },
        };

        public static readonly object[] CollectionTestCases = CollectionsObject.RealmSchema
            .Where(p => p.Type.IsCollection(out _) && !p.Type.HasFlag(PropertyType.Object))
            .Select(p => new object[]
            {
                CreateTestCase(p)
            })
            .ToArray();

        public static readonly object[] LinksTestCases = new[]
        {
            new object[]
            {
                CreateTestCase("Single link", new LinksObject("first")
                {
                    Link = new("second") { Value = 2 },
                    Value = 1,
                }),
            },
            new object[]
            {
                CreateTestCase("List", new LinksObject("first")
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
                CreateTestCase("Dictionary", new LinksObject("first")
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
                CreateTestCase("Set", new LinksObject("first")
                {
                    Set =
                    {
                        new("list.1") { Value = 100 },
                        new("list.2") { Value = 200 },
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

        [TestCaseSource(nameof(ATOTestCases))]
        public void RealmObject_NoLinks_Serializes(TestCaseData<AllTypesObject> testCase)
        {
            var ato = testCase.Value;
            AddIfNecessary(ato);

            var json = SerializationHelper.ToNativeJson(ato);
            var deserialized = BsonSerializer.Deserialize<AllTypesObject>(json);

            AssertAreEqual(deserialized, ato);
        }

        [TestCaseSource(nameof(LinksTestCases))]
        public void RealmObject_Links_Serializes(TestCaseData<LinksObject> testCase)
        {
            var linksObj = testCase.Value;
            AddIfNecessary(linksObj);

            var json = SerializationHelper.ToNativeJson(linksObj);
            AssertPropertyInJson(linksObj.Value, expectContains: true);

            var deserialized = BsonSerializer.Deserialize<LinksObject>(json);

            Assert.That(linksObj.Id, Is.EqualTo(deserialized.Id));
            Assert.That(linksObj.Value, Is.EqualTo(deserialized.Value));

            if (linksObj.Link is not null)
            {
                // Only the Id should be serialized here.
                var expected = new LinksObject(linksObj.Link.Id);
                AssertAreEqual(deserialized.Link, expected);
                AssertPropertyInJson(linksObj.Link.Value, expectContains: false);
            }

            var expectedList = linksObj.List.Select(o => new LinksObject(o.Id)).ToList();
            Assert.That(deserialized.List.Count, Is.EqualTo(expectedList.Count));

            for (var i = 0; i < expectedList.Count; i++)
            {
                AssertAreEqual(deserialized.List[i], expectedList[i], $"element: {i}");
                AssertPropertyInJson(expectedList[i].Value, expectContains: false);
            }

            Assert.That(deserialized.Dictionary.Count, Is.EqualTo(linksObj.Dictionary.Count));
            foreach (var kvp in linksObj.Dictionary)
            {
                Assert.That(deserialized.Dictionary.ContainsKey(kvp.Key), Is.True, $"Expected to contain key: {kvp.Key}");
                if (kvp.Value is null)
                {
                    Assert.That(deserialized.Dictionary[kvp.Key], Is.Null);
                }
                else
                {
                    AssertAreEqual(deserialized.Dictionary[kvp.Key], new LinksObject(kvp.Value.Id), $"key: {kvp.Key}");
                    AssertPropertyInJson(kvp.Value.Value, expectContains: false);
                }
            }

            Assert.That(deserialized.Set.Count, Is.EqualTo(linksObj.Set.Count));

            foreach (var item in linksObj.Set)
            {
                var match = deserialized.Set.SingleOrDefault(o => o.Id == item.Id);
                AssertAreEqual(match, new LinksObject(item.Id), $"element: {item.Value}");
                AssertPropertyInJson(item.Value, expectContains: false);
            }

            void AssertPropertyInJson(int value, bool expectContains)
            {
                var does = new ConstraintExpression();
                if (!expectContains)
                {
                    does = does.Not;
                }

                Assert.That(json, does.Contains($"\"$numberInt\" : \"{value}\""));
            }
        }

        [TestCaseSource(nameof(CollectionTestCases))]
        public void CollectionsObject_Serializes(TestCaseData<Property> testCase)
        {
            var prop = testCase.Value;
            var obj = new CollectionsObject();

            DataGenerator.FillCollection(obj.GetProperty<IEnumerable>(prop), 5);

            AddIfNecessary(obj);

            var json = SerializationHelper.ToNativeJson(obj);
            var deserialized = BsonSerializer.Deserialize<CollectionsObject>(json);

            var actual = deserialized.GetProperty<IEnumerable>(prop);
            var expected = obj.GetProperty<IEnumerable>(prop);

            AssertAreEqual(actual, expected, $"property: {prop.Name}");
        }

        private void AddIfNecessary(IRealmObject obj)
        {
            if (_managed)
            {
                _realm.Write(() =>
                {
                    _realm.Add(obj);
                });
            }
        }
    }
}
