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
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NUnit.Framework;
using Realms.Helpers;

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

        [TestCaseSource(nameof(ATOTestCases))]
        public void RealmObject_NoLinks_Serializes(TestCaseData<AllTypesObject> testCase)
        {
            var ato = testCase.Value;
            if (_managed)
            {
                _realm.Write(() =>
                {
                    _realm.Add(ato);
                });
            }

            var json = ato.ToJson();
            var deserialized = BsonSerializer.Deserialize<AllTypesObject>(json);

            foreach (var prop in ato.ObjectSchema)
            {
                var pi = typeof(AllTypesObject).GetProperty(prop.ManagedName, BindingFlags.Public | BindingFlags.Instance)!;
                var actual = pi.GetValue(deserialized);
                var expected = pi.GetValue(ato);

                if (expected is RealmValue rv)
                {
                    if (rv.Type == RealmValueType.Float)
                    {
                        // Json doesn't have a float type, so the deserialized value will be double
                        Assert.That((double)(RealmValue)actual!, Is.EqualTo((double)rv.AsFloat()));
                        continue;
                    }
                }

                Assert.That(pi.GetValue(deserialized), Is.EqualTo(pi.GetValue(ato)));
            }
        }

        private static TestCaseData<T> CreateTestCase<T>(string description, T value) => new TestCaseData<T>(description, value);

        public class TestCaseData<T>
        {
            private readonly string _description;

            public T Value { get; }

            public TestCaseData(string description, T value)
            {
                _description = description;
                Value = value;
            }

            public override string ToString() => _description;
        }
    }
}
