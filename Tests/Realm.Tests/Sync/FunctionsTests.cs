////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
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
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using NUnit.Framework;

namespace Realms.Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class FunctionsTests : SyncTestBase
    {
        private readonly List<string> _conventionsToRemove = new List<string>();

        [Test]
        public void CallFunction_ReturnsResult()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();
                var result = await user.Functions.CallAsync("sumFunc", 1, 2, 3);

                Assert.That(result.AsInt64, Is.EqualTo(6));
            });
        }

        [Test]
        public void CallFunctionGeneric_ReturnsResult()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();
                var result = await user.Functions.CallAsync<int>("sumFunc", 1, 2, 3);

                Assert.That(result, Is.EqualTo(6));
            });
        }

        [Test]
        public void CallFunction_NoArguments()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();
                var result = await user.Functions.CallAsync<int>("sumFunc");

                Assert.That(result, Is.EqualTo(0));
            });
        }

        [Test]
        public void CallFunction_WrongGeneric()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();
                var ex = await TestHelpers.AssertThrows<Exception>(() => user.Functions.CallAsync<string>("sumFunc", 1, 2));

                Assert.That(ex.Message, Does.Contain("Cannot deserialize"));
            });
        }

        [Test]
        public void CallFunction_WithAnonymousParams_ReturnsBsonResult()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();
                var first = new
                {
                    intValue = 1,
                    floatValue = 1.2,
                    stringValue = "Hello ",
                    objectId = ObjectId.GenerateNewId(),
                    arr = new[] { 1, 2, 3 },
                    date = DateTime.UtcNow,
                    child = new
                    {
                        intValue = 2
                    }
                };

                var second = new
                {
                    intValue = 2,
                    floatValue = 2.3,
                    stringValue = "world",
                    objectId = ObjectId.GenerateNewId(),
                    date = DateTime.UtcNow.AddHours(5),
                    arr = new[] { 4, 5, 6 },
                    child = new
                    {
                        intValue = 3
                    }
                };

                var result = await user.Functions.CallAsync("documentFunc", first, second);

                Assert.That(result["intValue"].AsInt64, Is.EqualTo(3));
                Assert.That(result["floatValue"].AsDouble, Is.EqualTo(3.5));
                Assert.That(result["stringValue"].AsString, Is.EqualTo("Hello world"));
                Assert.That(result["objectId"].AsObjectId, Is.EqualTo(first.objectId));
                AssertDateTimeEquals(result["date"].ToUniversalTime(), second.date);
                Assert.That(result["arr"].AsBsonArray.Select(a => a.AsInt32), Is.EquivalentTo(new[] { 1, 4 }));
                Assert.That(result["child"]["intValue"].AsInt64, Is.EqualTo(5));
            });
        }

        [Test]
        public void CallFunction_WithTypedParams_ReturnsTypedResult()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                AddCamelCaseConvention();

                var user = await GetUserAsync();
                var first = new FunctionArgument
                {
                    IntValue = 1,
                    FloatValue = 1.2,
                    StringValue = "Hello ",
                    ObjectId = ObjectId.GenerateNewId(),
                    Arr = new[] { 1, 2, 3 },
                    Date = DateTime.UtcNow,
                    Child = new Child
                    {
                        IntValue = 2
                    }
                };

                var second = new FunctionArgument
                {
                    IntValue = 2,
                    FloatValue = 2.3,
                    StringValue = "world",
                    ObjectId = ObjectId.GenerateNewId(),
                    Date = DateTime.UtcNow.AddHours(5),
                    Arr = new[] { 4, 5, 6 },
                    Child = new Child
                    {
                        IntValue = 3
                    }
                };

                var result = await user.Functions.CallAsync<FunctionArgument>("documentFunc", first, second);

                Assert.That(result.IntValue, Is.EqualTo(3));
                Assert.That(result.FloatValue, Is.EqualTo(3.5));
                Assert.That(result.StringValue, Is.EqualTo("Hello world"));
                AssertDateTimeEquals(result.Date, second.Date);
                Assert.That(result.ObjectId, Is.EqualTo(first.ObjectId));
                Assert.That(result.Arr, Is.EquivalentTo(new[] { 1, 4 }));
                Assert.That(result.Child.IntValue, Is.EqualTo(5));
            });
        }

        [TestCaseSource(nameof(DeserializationTestCases))]
        public void CallFunction_AndTestDeserialization(dynamic arg)
        {
            TestDeserialization(arg);
        }

        [TestCaseSource(nameof(DeserializationTestCases))]
        public void CallFunction_AndTestBsonValue(dynamic arg)
        {
            TestBsonValue(arg);
        }

        [Test]
        public void CallFunction_AndTestDeserialization_Null()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();

                string str = null;
                var result = await user.Functions.CallAsync<string>("mirror", str);

                Assert.That(result, Is.Null);
            });
        }

        private void TestDeserialization<T>(T val)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();

                var result = await user.Functions.CallAsync<T>("mirror", val);

                if (val is DateTime date && result is DateTime dateResult)
                {
                    AssertDateTimeEquals(date, dateResult);
                }
                else
                {
                    Assert.That(result, Is.EqualTo(val));
                }

                var arr = new[] { val };
                var arrResult = await user.Functions.CallAsync<T[]>("mirror", arr);

                if (arr is DateTime[] dateArr && arrResult is DateTime[] dateArrResult)
                {
                    Assert.That(dateArr.Length, Is.EqualTo(dateArrResult.Length));
                    for (var i = 0; i < dateArr.Length; i++)
                    {
                        AssertDateTimeEquals(dateArr[i], dateArrResult[i]);
                    }
                }
                else
                {
                    Assert.That(arrResult, Is.EquivalentTo(arr));
                }
            });
        }

        private void TestBsonValue<T>(T val)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();

                var result = await user.Functions.CallAsync("mirror", val);

                if (val is DateTime date)
                {
                    AssertDateTimeEquals(result.ToUniversalTime(), date);
                }
                else
                {
                    Assert.That(result.ToString().ToLower(), Is.EqualTo(val.ToString().ToLower()));
                }

                var arr = new[] { val };
                var arrResult = await user.Functions.CallAsync("mirror", arr);

                if (arr is DateTime[] dateArr)
                {
                    var dateArrResult = arrResult.AsBsonArray.Select(a => a.ToUniversalTime()).ToArray();
                    Assert.That(dateArrResult.Length, Is.EqualTo(dateArr.Length));
                    for (var i = 0; i < dateArr.Length; i++)
                    {
                        AssertDateTimeEquals(dateArrResult[i], dateArr[i]);
                    }
                }
                else
                {
                    Assert.That(
                        arrResult.AsBsonArray.Select(a => a.ToString().ToLower()),
                        Is.EquivalentTo(arr.Select(a => a.ToString().ToLower())));
                }
            });
        }

        protected override void CustomTearDown()
        {
            base.CustomTearDown();

            foreach (var convention in _conventionsToRemove)
            {
                ConventionRegistry.Remove(convention);
            }

            _conventionsToRemove.Clear();
        }

        private static void AssertDateTimeEquals(DateTime first, DateTime second)
        {
            var diff = (first - second).TotalMilliseconds;
            Assert.That(Math.Abs(diff), Is.LessThan(1), $"Expected {first} to equal {second} with millisecond precision, but it didn't.");
        }

        private void AddCamelCaseConvention()
        {
            const string name = "camel case";
            var pack = new ConventionPack();
            pack.Add(new CamelCaseElementNameConvention());
            ConventionRegistry.Register(name, pack, _ => true);

            _conventionsToRemove.Add(name);
        }

        private class FunctionArgument
        {
            public int IntValue { get; set; }

            public double FloatValue { get; set; }

            public string StringValue { get; set; }

            public ObjectId ObjectId { get; set; }

            public int[] Arr { get; set; }

            public Child Child { get; set; }

            public DateTime Date { get; set; }
        }

        private class Child
        {
            public int IntValue { get; set; }
        }

        public static readonly object[] DeserializationTestCases = new[]
        {
            new object[] { 1 },
            new object[] { 0 },
            new object[] { -1 },
            new object[] { int.MinValue },
            new object[] { int.MaxValue },
            new object[] { 1L },
            new object[] { -1L },
            new object[] { 0L },
            new object[] { long.MinValue },
            new object[] { long.MaxValue },
            new object[] { 1.54 },
            new object[] { 0.00 },
            new object[] { -1.224 },

            // These don't roundtrip correctly: https://github.com/realm/realm-object-store/issues/1106
            // new object[] { double.MinValue },
            // new object[] { double.MaxValue },

            // These are unsupported by core - it has a limit of 19 digits for string parsing
            // new object[] { Decimal128.MinValue },
            // new object[] { Decimal128.MaxValue },
            new object[] { new Decimal128(1.2M) },
            new object[] { new Decimal128(0) },
            new object[] { new Decimal128(-1.53464399239324M) },
            new object[] { "fooo" },
            new object[] { string.Empty },
            new object[] { ObjectId.Parse("5f766ae78d273beeab5b0e6b") },
            new object[] { ObjectId.Empty },
            new object[] { new DateTime(202065954, DateTimeKind.Utc) },
            new object[] { new DateTime(3333444, DateTimeKind.Utc) },
            new object[] { new DateTime(0, DateTimeKind.Utc) },
            new object[] { DateTime.MinValue },
            new object[] { DateTime.MaxValue },
            new object[] { true },
            new object[] { false },
            new object[] { (short)1 },
            new object[] { (short)0 },
            new object[] { (short)-1 },
            new object[] { short.MinValue },
            new object[] { short.MaxValue },
        };
    }
}
