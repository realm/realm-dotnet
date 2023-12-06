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
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using NUnit.Framework;
using static Realms.Tests.TestHelpers;

namespace Realms.Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class FunctionsTests : SyncTestBase
    {
        private readonly ConcurrentQueue<StrongBox<string>> _conventionsToRemove = new();

        [Test]
        public void CallFunction_ReturnsResult()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();
                var result = await user.Functions.CallAsync("sumFunc", 1L, 2L, 3L);

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
            TestHelpers.IgnoreOnUnity("Anonymous objects need lambda compilation, which doesn't work on IL2CPP");

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
                        intValue = 2L
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
                        intValue = 3L
                    }
                };

                var result = await user.Functions.CallAsync("documentFunc", first, second);

                Assert.That(result["intValue"].AsInt32, Is.EqualTo(3));
                Assert.That(result["floatValue"].AsDouble, Is.EqualTo(3.5));
                Assert.That(result["stringValue"].AsString, Is.EqualTo("Hello world"));
                Assert.That(result["objectId"].AsObjectId, Is.EqualTo(first.objectId));
                AssertDateTimeEquals(result["date"].ToUniversalTime(), second.date);
                Assert.That(result["arr"].AsBsonArray.Select(a => a.AsInt32), Is.EquivalentTo(new[] { 1, 4 }));
                Assert.That(result["child"]["intValue"].AsInt64, Is.EqualTo(5));
            });
        }

        [Test]
        public void CallFunction_WithBsonDocument_ReturnsBsonResult()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();
                var first = new BsonDocument
                {
                    { "intValue", 1 },
                    { "floatValue", 1.2 },
                    { "stringValue", "Hello " },
                    { "objectId", ObjectId.GenerateNewId() },
                    { "arr", new BsonArray { 1, 2, 3 } },
                    { "date", DateTime.UtcNow },
                    { "child", new BsonDocument { { "intValue", 2L } } }
                };

                var second = new BsonDocument
                {
                    { "intValue", 2 },
                    { "floatValue", 2.3 },
                    { "stringValue", "world" },
                    { "objectId", ObjectId.GenerateNewId() },
                    { "date", DateTime.UtcNow.AddHours(5) },
                    { "arr", new BsonArray { 4, 5, 6 } },
                    { "child", new BsonDocument { { "intValue", 3L } } }
                };

                var result = await user.Functions.CallAsync("documentFunc", first, second);

                Assert.That(result["intValue"].AsInt32, Is.EqualTo(3));
                Assert.That(result["floatValue"].AsDouble, Is.EqualTo(3.5));
                Assert.That(result["stringValue"].AsString, Is.EqualTo("Hello world"));
                Assert.That(result["objectId"].AsObjectId, Is.EqualTo(first["objectId"].AsObjectId));
                AssertDateTimeEquals(result["date"].ToUniversalTime(), second["date"].ToUniversalTime());
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
                Assert.That(result.Child!.IntValue, Is.EqualTo(5));
            });
        }

        #region TestDeserialization

        [Test]
        public void CallFunction_AndTestDeserialization_Short([ValueSource(nameof(ShortTestCases))] short arg)
        {
            TestDeserialization(arg);
        }

        [Test]
        public void CallFunction_AndTestDeserialization_Int([ValueSource(nameof(IntTestCases))] int arg)
        {
            TestDeserialization(arg);
        }

        [Test]
        public void CallFunction_AndTestDeserialization_Long([ValueSource(nameof(LongTestCases))] long arg)
        {
            TestDeserialization(arg);
        }

        [Test]
        public void CallFunction_AndTestDeserialization_Float([ValueSource(nameof(FloatTestCases))] float arg)
        {
            TestDeserialization(arg);
        }

        [Test]
        public void CallFunction_AndTestDeserialization_Double([ValueSource(nameof(DoubleTestCases))] double arg)
        {
            TestDeserialization(arg);
        }

        [Test]
        public void CallFunction_AndTestDeserialization_Decimal([ValueSource(nameof(DecimalTestCases))] decimal arg)
        {
            TestDeserialization(arg);
        }

        [Test]
        public void CallFunction_AndTestDeserialization_Decimal128([ValueSource(nameof(Decimal128TestCases))] Decimal128 arg)
        {
            TestDeserialization(arg);
        }

        [Test]
        public void CallFunction_AndTestDeserialization_String([ValueSource(nameof(StringTestCases))] string arg)
        {
            TestDeserialization(arg);
        }

        [Test]
        public void CallFunction_AndTestDeserialization_DateTime([ValueSource(nameof(DateTimeTestCases))] DateTime arg)
        {
            TestDeserialization(arg);
        }

        [Test]
        public void CallFunction_AndTestDeserialization_DateTimeOffset([ValueSource(nameof(DateTimeOffsetTestCases))] DateTimeOffset arg)
        {
            TestDeserialization(arg);
        }

        [Test]
        public void CallFunction_AndTestDeserialization_ObjectId([ValueSource(nameof(ObjectIdTestCases))] ObjectId arg)
        {
            TestDeserialization(arg);
        }

        [Test]
        public void CallFunction_AndTestDeserialization_Guid([ValueSource(nameof(GuidTestCases))] Guid arg)
        {
            TestDeserialization(arg);
        }

        [Test]
        public void CallFunction_AndTestDeserialization_Boolean([ValueSource(nameof(BooleanTestCases))] bool arg)
        {
            TestDeserialization(arg);
        }

        [Test]
        public void CallFunction_AndTestDeserialization_Null()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();

                string? str = null;
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

        #endregion

        #region TestBsonValue

        [Test]
        public void CallFunction_AndTestBsonValue_Short([ValueSource(nameof(ShortTestCases))] short arg)
        {
            TestBsonValue(arg);
        }

        [Test]
        public void CallFunction_AndTestBsonValue_Int([ValueSource(nameof(IntTestCases))] int arg)
        {
            TestBsonValue(arg);
        }

        [Test]
        public void CallFunction_AndTestBsonValue_Long([ValueSource(nameof(LongTestCases))] long arg)
        {
            TestBsonValue(arg);
        }

        [Test]
        [Ignore("BsonValue can't represent float.")]
        public void CallFunction_AndTestBsonValue_Float([ValueSource(nameof(FloatTestCases))] float arg)
        {
            TestBsonValue(arg);
        }

        [Test]
        public void CallFunction_AndTestBsonValue_Double([ValueSource(nameof(DoubleTestCases))] double arg)
        {
            TestBsonValue(arg);
        }

        [Test]
        public void CallFunction_AndTestBsonValue_Decimal([ValueSource(nameof(DecimalTestCases))] decimal arg)
        {
            if (arg is decimal.MinValue or decimal.MaxValue)
            {
                // MongoDB.Bson serializes MinValue/MaxValue as Decimal128.MinValue/MaxValue:
                // https://github.com/mongodb/mongo-csharp-driver/blob/b2668fb80c8d45be58a8009e336006c9545c1581/src/MongoDB.Bson/Serialization/Options/RepresentationConverter.cs#L153-L160
                Assert.Ignore("MongoDB.Bson representation for decimal.MinValue/MaxValue is incorrect.");
            }

            TestBsonValue(arg);
        }

        [Test]
        public void CallFunction_AndTestBsonValue_Decimal128([ValueSource(nameof(Decimal128TestCases))] Decimal128 arg)
        {
            TestBsonValue(arg);
        }

        [Test]
        public void CallFunction_AndTestBsonValue_String([ValueSource(nameof(StringTestCases))] string arg)
        {
            TestBsonValue(arg);
        }

        [Test]
        public void CallFunction_AndTestBsonValue_DateTime([ValueSource(nameof(DateTimeTestCases))] DateTime arg)
        {
            TestBsonValue(arg);
        }

        [Test]
        public void CallFunction_AndTestBsonValue_DateTimeOffset([ValueSource(nameof(DateTimeOffsetTestCases))] DateTimeOffset arg)
        {
            TestBsonValue(arg);
        }

        [Test]
        public void CallFunction_AndTestBsonValue_ObjectId([ValueSource(nameof(ObjectIdTestCases))] ObjectId arg)
        {
            TestBsonValue(arg);
        }

        [Test]
        public void CallFunction_AndTestBsonValue_Guid([ValueSource(nameof(ObjectIdTestCases))] ObjectId arg)
        {
            TestBsonValue(arg);
        }

        [Test]
        public void CallFunction_AndTestBsonValue_Boolean([ValueSource(nameof(BooleanTestCases))] bool arg)
        {
            TestBsonValue(arg);
        }

        private void TestBsonValue<T>(T val)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

                var user = await GetUserAsync();

                var result = await user.Functions.CallAsync("mirror", val);

                if (val is DateTime date)
                {
                    AssertDateTimeEquals(result.ToUniversalTime(), date);
                }
                else if (val is DateTimeOffset dto)
                {
                    AssertDateTimeOffsetEquals(result, dto);
                }
                else
                {
                    Assert.That(result.ToString()!.ToLower(), Is.EqualTo(val!.ToString()!.ToLower()));
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
                else if (arr is DateTimeOffset[] dtoArr)
                {
                    var dtoArrResult = arrResult.AsBsonArray;
                    Assert.That(dtoArrResult.Count, Is.EqualTo(dtoArr.Length));
                    for (var i = 0; i < dtoArr.Length; i++)
                    {
                        AssertDateTimeOffsetEquals(dtoArrResult[i], dtoArr[i]);
                    }
                }
                else
                {
                    Assert.That(
                        arrResult.AsBsonArray.Select(a => a.ToString()!.ToLower()),
                        Is.EquivalentTo(arr.Select(a => a!.ToString()!.ToLower())));
                }
            });
        }

        #endregion

        protected override void CustomTearDown()
        {
            base.CustomTearDown();

            _conventionsToRemove.DrainQueue(ConventionRegistry.Remove);
        }

        private static void AssertDateTimeEquals(DateTime first, DateTime second)
        {
            var diff = (first - second).TotalMilliseconds;
            Assert.That(Math.Abs(diff), Is.LessThan(1), $"Expected {first} to equal {second} with millisecond precision, but it didn't.");
        }

        private static void AssertDateTimeOffsetEquals(BsonValue first, DateTimeOffset second)
        {
            Assert.That(first.IsString);
            var firstDto = DateTimeOffset.Parse(first.AsString);
            Assert.That(firstDto, Is.EqualTo(second));
        }

        private void AddCamelCaseConvention()
        {
            const string name = "camel case";
            var pack = new ConventionPack();
            pack.Add(new CamelCaseElementNameConvention());
            ConventionRegistry.Register(name, pack, _ => true);

            _conventionsToRemove.Enqueue(name);
        }

        private class FunctionArgument
        {
            public int IntValue { get; set; }

            public double FloatValue { get; set; }

            public string? StringValue { get; set; }

            public ObjectId ObjectId { get; set; }

            public int[]? Arr { get; set; }

            public Child? Child { get; set; }

            public DateTime Date { get; set; }
        }

        private class Child
        {
            public int IntValue { get; set; }
        }

        public static readonly int[] IntTestCases = new[]
        {
            1,
            0,
            -1,
            int.MinValue,
            int.MaxValue,
        };

        public static readonly long[] LongTestCases = new[]
        {
            1L,
            -1L,
            0L,
            long.MinValue,
            long.MaxValue,
        };

        public static readonly double[] DoubleTestCases = new[]
        {
            1.54,
            0.00,
            -1.224,

            // These don't roundtrip correctly: https://github.com/realm/realm-object-store/issues/1106
            // double.MinValue,
            // double.MaxValue,
        };

        public static readonly float[] FloatTestCases = new[]
        {
            1.54f,
            0.00f,
            -1.224f,
            float.MinValue,
            float.MaxValue,
        };

        public static readonly Decimal128[] Decimal128TestCases = new[]
        {
            Decimal128.MinValue,
            Decimal128.MaxValue,
            new Decimal128(1.2M),
            new Decimal128(0),
            new Decimal128(-1.53464399239324M),
        };

        public static readonly decimal[] DecimalTestCases = new[]
        {
            decimal.MinValue,
            decimal.MaxValue,
            1.2M,
            0M,
            -1.53464399239324M,
        };

        public static readonly string[] StringTestCases = new[]
        {
            "fooo",
            string.Empty,
        };

        public static readonly ObjectId[] ObjectIdTestCases = new[]
        {
            ObjectId.Parse("5f766ae78d273beeab5b0e6b"),
            ObjectId.Empty,
        };

        public static readonly Guid[] GuidTestCases = new[]
        {
            Guid.Parse("86CA0BAD-F069-4713-AD42-F9175579B83D"),
            Guid.Empty,
        };

        public static readonly DateTime[] DateTimeTestCases = new[]
        {
            new DateTime(202065954, DateTimeKind.Utc),
            new DateTime(3333444, DateTimeKind.Utc),
            new DateTime(0, DateTimeKind.Utc),
            DateTime.MinValue,
            DateTime.MaxValue,
        };

        public static readonly DateTimeOffset[] DateTimeOffsetTestCases = new[]
        {
            new DateTimeOffset(202065955, TimeSpan.Zero),
            new DateTimeOffset(3333444, TimeSpan.Zero),
            new DateTimeOffset(0, TimeSpan.Zero),
            DateTimeOffset.MinValue,
            DateTimeOffset.MaxValue,
        };

        public static readonly bool[] BooleanTestCases = new[]
        {
            true,
            false,
        };

        public static readonly short[] ShortTestCases = new[]
        {
            (short)1,
            (short)0,
            (short)-1,
            short.MinValue,
            short.MaxValue,
        };
    }
}
