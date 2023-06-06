////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
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
using System.Threading.Tasks;
using MongoDB.Bson;
using NUnit.Framework;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class ListOfPrimitivesTests : RealmInstanceTest
    {
        #region TestCaseSources

        private static object?[] GetTestCases<T>(IEnumerable<T?[]> values)
            where T : struct
        {
            var cases = new List<object?>
            {
                new object?[] { null }
            };

            var filteredValues = values.Select(v => v.Where(i => i.HasValue).Select(i => i!.Value).ToArray())
                                       .Where(v => v.Any());

            foreach (var item in filteredValues)
            {
                cases.Add(new object?[] { item });
            }

            return cases.ToArray();
        }

        private static object?[] GetTestCases<T>(IEnumerable<T?[]> values)
            where T : class
        {
            var cases = new List<object?>
            {
                new object?[] { null }
            };

            var filteredValues = values.Select(v => v.Where(i => i != null).ToArray())
                                       .Where(v => v.Any());

            foreach (var item in filteredValues)
            {
                cases.Add(new object?[] { item });
            }

            return cases.ToArray();
        }

        private static object?[] GetNullableTestCases<T>(IEnumerable<T[]> values)
        {
            var cases = new List<object?>
            {
                new object?[] { null }
            };

            foreach (var item in values)
            {
                cases.Add(new object?[] { item });
            }

            return cases.ToArray();
        }

        private static readonly IEnumerable<bool?[]> _booleanValues = new[]
        {
            new bool?[] { true },
            new bool?[] { null },
            new bool?[] { true, true, null, false },
        };

        public static readonly object?[] BooleanTestValues = GetTestCases(_booleanValues);

        public static readonly object?[] NullableBooleanTestValues = GetNullableTestCases(_booleanValues);

        private static readonly IEnumerable<byte?[]> _byteValues = new[]
        {
            new byte?[] { 0 },
            new byte?[] { null },
            new byte?[] { byte.MinValue, byte.MaxValue, 0 },
            new byte?[] { 1, 2, 3, null },
        };

        public static readonly object?[] ByteTestValues = GetTestCases(_byteValues);

        public static readonly object?[] NullableByteTestValues = GetNullableTestCases(_byteValues);

        private static readonly IEnumerable<char?[]> _charValues = new[]
        {
            new char?[] { 'a' },
            new char?[] { null },
            new char?[] { char.MinValue, 'z' },
            new char?[] { 'a', 'b', 'c', 'b', null }
        };

        public static readonly object?[] CharTestValues = GetTestCases(_charValues);

        public static readonly object?[] NullableCharTestValues = GetNullableTestCases(_charValues);

        private static readonly IEnumerable<double?[]> _doubleValues = new[]
        {
            new double?[] { 1.4 },
            new double?[] { null },
            new double?[] { double.MinValue, double.MaxValue, 0 },
            new double?[] { -1, 3.4, null, 5.3, 9 }
        };

        public static readonly object?[] DoubleTestValues = GetTestCases(_doubleValues);

        public static readonly object?[] NullableDoubleTestValues = GetNullableTestCases(_doubleValues);

        private static readonly IEnumerable<short?[]> _shortValues = new[]
        {
            new short?[] { 1 },
            new short?[] { null },
            new short?[] { short.MaxValue, short.MinValue, 0 },
            new short?[] { 3, -1, null, 45, null },
        };

        public static readonly object?[] Int16TestValues = GetTestCases(_shortValues);

        public static readonly object?[] NullableInt16TestValues = GetNullableTestCases(_shortValues);

        private static readonly IEnumerable<int?[]> _intValues = new[]
        {
            new int?[] { 1 },
            new int?[] { null },
            new int?[] { int.MaxValue, int.MinValue, 0 },
            new int?[] { -5, 3, 9, null, 350 },
        };

        public static readonly object?[] Int32TestValues = GetTestCases(_intValues);

        public static readonly object?[] NullableInt32TestValues = GetNullableTestCases(_intValues);

        private static readonly IEnumerable<long?[]> _longValues = new[]
        {
            new long?[] { 1 },
            new long?[] { null },
            new long?[] { long.MaxValue, long.MinValue, 0 },
            new long?[] { 4, -39, 81, null, -69324 },
        };

        public static readonly object?[] Int64TestValues = GetTestCases(_longValues);

        public static readonly object?[] NullableInt64TestValues = GetNullableTestCases(_longValues);

        private static readonly IEnumerable<decimal?[]> _decimalValues = new[]
        {
            new decimal?[] { 1.4M },
            new decimal?[] { null },
            new decimal?[] { decimal.MinValue, decimal.MaxValue, 0 },
            new decimal?[] { -1, 3.4M, null, 5.3M, 9.54375843758349634963634M }
        };

        public static readonly object?[] DecimalTestValues = GetTestCases(_decimalValues);

        public static readonly object?[] NullableDecimalTestValues = GetNullableTestCases(_decimalValues);

        private static readonly IEnumerable<Decimal128?[]> _decimal128Values = new[]
        {
            new Decimal128?[] { 1.4M },
            new Decimal128?[] { null },
            new Decimal128?[] { Decimal128.MinValue, decimal.MinValue, decimal.MaxValue, Decimal128.MaxValue, 0 },
            new Decimal128?[] { -1, 3.4M, null, 5.3M, -23.424389584396384963M }
        };

        public static readonly object?[] Decimal128TestValues = GetTestCases(_decimal128Values);

        public static readonly object?[] NullableDecimal128TestValues = GetNullableTestCases(_decimal128Values);

        private static readonly IEnumerable<ObjectId?[]> _objectIdValues = new[]
        {
            new ObjectId?[] { new ObjectId("5f651b09f6cddff534c3cddf") },
            new ObjectId?[] { null },
            new ObjectId?[] { ObjectId.Empty, TestHelpers.GenerateRepetitiveObjectId(0), TestHelpers.GenerateRepetitiveObjectId(byte.MaxValue) },
            new ObjectId?[] { new ObjectId("5f651b2930643efeef987e5d"), TestHelpers.GenerateRepetitiveObjectId(byte.MaxValue), null, new ObjectId("5f651c4cf755604f2fbf7440") }
        };

        public static readonly object?[] ObjectIdTestValues = GetTestCases(_objectIdValues);

        public static readonly object?[] NullableObjectIdTestValues = GetNullableTestCases(_objectIdValues);

        private static readonly IEnumerable<Guid?[]> _guidValues = new[]
        {
            new Guid?[] { Guid.Parse("d31e0d4c-fa23-48eb-8d24-0b2a7288922c") },
            new Guid?[] { null },
            new Guid?[] { Guid.Empty, Guid.Parse("44e2d58e-f234-41c2-a156-dacddbb72a83") },
            new Guid?[] { Guid.Parse("d31e0d4c-fa23-48eb-8d24-0b2a7288922c"), Guid.Parse("7ca0a661-1146-4c94-81cd-87a7ba9e9d0a"), null },
        };

        public static readonly object?[] GuidTestValues = GetTestCases(_guidValues);

        public static readonly object?[] NullableGuidTestValues = GetNullableTestCases(_guidValues);

        private static readonly DateTimeOffset _someDate = new(2021, 12, 3, 4, 5, 6, TimeSpan.FromHours(-1));
        private static readonly IEnumerable<DateTimeOffset?[]> _dateValues = new[]
        {
            new DateTimeOffset?[] { _someDate.AddDays(-4) },
            new DateTimeOffset?[] { null },
            new DateTimeOffset?[] { DateTimeOffset.MinValue, DateTimeOffset.MaxValue, _someDate },
            new DateTimeOffset?[] { _someDate.AddDays(5), null, _someDate.AddDays(-39), _someDate.AddDays(81), _someDate.AddDays(-69324) },
        };

        public static readonly object?[] DateTestValues = GetTestCases(_dateValues);

        public static readonly object?[] NullableDateTestValues = GetNullableTestCases(_dateValues);

        private static readonly IEnumerable<string?[]> _stringValues = new[]
        {
            new string?[] { string.Empty },
            new string?[] { null },
            new string?[] { " " },
            new string?[] { "abc", "cdf", "az" },
            new string?[] { "a", null, "foo", "bar", null },
        };

        public static readonly object?[] StringTestValues = GetTestCases(_stringValues);

        public static readonly object?[] NullableStringTestValues = GetNullableTestCases(_stringValues);

        private static readonly IEnumerable<byte[]?[]> _byteArrayValues = new[]
        {
            new byte[]?[] { Array.Empty<byte>() },
            new byte[]?[] { null },
            new byte[]?[] { new byte[] { 0 } },
            new byte[]?[] { new byte[] { 0, byte.MinValue, byte.MaxValue } },
            new byte[]?[] { TestHelpers.GetBytes(3), TestHelpers.GetBytes(5), TestHelpers.GetBytes(7) },
            new byte[]?[] { TestHelpers.GetBytes(1), null, TestHelpers.GetBytes(3), TestHelpers.GetBytes(3), null },
        };

        public static readonly object?[] ByteArrayTestValues = GetTestCases(_byteArrayValues);

        public static readonly object?[] NullableByteArrayTestValues = GetNullableTestCases(_byteArrayValues);

        private static readonly IEnumerable<RealmValue[]> _realmValueValues = new[]
        {
            new RealmValue[] { RealmValue.Null },
            new RealmValue[] { RealmValue.Null, 1, true, "abc" },
            new RealmValue[] { int.MinValue, long.MaxValue, string.Empty },
            new RealmValue[]
            {
                RealmValue.Null,
                10,
                false,
                "cde",
                new byte[] { 0, 1, 2 },
                DateTimeOffset.FromUnixTimeSeconds(1616137641),
                1.5f,
                3.5d,
                5m,
                new ObjectId("5f63e882536de46d71877979"),
                new Guid("{F2952191-A847-41C3-8362-497F92CB7D24}"),
                new IntPropertyObject { Int = 10 },
            }
        };

        // Technically RealmValue can't be nullable and _realmValueValues will not contain null
        public static readonly object?[] RealmValueTestValues = GetNullableTestCases(_realmValueValues);

        #endregion TestCaseSources

        static ListOfPrimitivesTests()
        {
            BooleanTestValues = GetTestCases(_booleanValues);
            NullableBooleanTestValues = GetNullableTestCases(_booleanValues);
        }

        #region Managed Tests

        [TestCaseSource(nameof(BooleanTestValues))]
        public void Test_ManagedBooleanList(bool[] values)
        {
            RunManagedTests(obj => obj.BooleanList, values);
        }

        [TestCaseSource(nameof(ByteTestValues))]
        public void Test_ManagedByteList(byte[] values)
        {
            RunManagedTests(obj => obj.ByteList, values);
        }

        [TestCaseSource(nameof(CharTestValues))]
        public void Test_ManagedCharList(char[] values)
        {
            RunManagedTests(obj => obj.CharList, values);
        }

        [TestCaseSource(nameof(DoubleTestValues))]
        public void Test_ManagedDoubleList(double[] values)
        {
            RunManagedTests(obj => obj.DoubleList, values);
        }

        [TestCaseSource(nameof(Int16TestValues))]
        public void Test_ManagedInt16List(short[] values)
        {
            RunManagedTests(obj => obj.Int16List, values);
        }

        [TestCaseSource(nameof(Int32TestValues))]
        public void Test_ManagedInt32List(int[] values)
        {
            RunManagedTests(obj => obj.Int32List, values);
        }

        [TestCaseSource(nameof(Int64TestValues))]
        public void Test_ManagedInt64List(long[] values)
        {
            RunManagedTests(obj => obj.Int64List, values);
        }

        [TestCaseSource(nameof(DecimalTestValues))]
        public void Test_ManagedDecimalList(decimal[] values)
        {
            RunManagedTests(obj => obj.DecimalList, values);
        }

        [TestCaseSource(nameof(Decimal128TestValues))]
        public void Test_ManagedDecimal128List(Decimal128[] values)
        {
            RunManagedTests(obj => obj.Decimal128List, values);
        }

        [TestCaseSource(nameof(ObjectIdTestValues))]
        public void Test_ManagedObjectIdList(ObjectId[] values)
        {
            RunManagedTests(obj => obj.ObjectIdList, values);
        }

        [TestCaseSource(nameof(GuidTestValues))]
        public void Test_ManagedGuidList(Guid[] values)
        {
            RunManagedTests(obj => obj.GuidList, values);
        }

        [TestCaseSource(nameof(DateTestValues))]
        public void Test_ManagedDateTimeOffsetList(DateTimeOffset[] values)
        {
            RunManagedTests(obj => obj.DateTimeOffsetList, values);
        }

        [TestCaseSource(nameof(StringTestValues))]
        public void Test_ManagedStringList(string[] values)
        {
            RunManagedTests(obj => obj.StringList, values);
        }

        [TestCaseSource(nameof(ByteArrayTestValues))]
        public void Test_ManagedByteArrayList(byte[][] values)
        {
            RunManagedTests(obj => obj.ByteArrayList, values);
        }

        [TestCaseSource(nameof(NullableBooleanTestValues))]
        public void Test_ManagedNullableBooleanList(bool?[] values)
        {
            RunManagedTests(obj => obj.NullableBooleanList, values);
        }

        [TestCaseSource(nameof(NullableByteTestValues))]
        public void Test_ManagedNullableByteList(byte?[] values)
        {
            RunManagedTests(obj => obj.NullableByteList, values);
        }

        [TestCaseSource(nameof(NullableCharTestValues))]
        public void Test_ManagedNullableCharList(char?[] values)
        {
            RunManagedTests(obj => obj.NullableCharList, values);
        }

        [TestCaseSource(nameof(NullableDoubleTestValues))]
        public void Test_ManagedNullableDoubleList(double?[] values)
        {
            RunManagedTests(obj => obj.NullableDoubleList, values);
        }

        [TestCaseSource(nameof(NullableInt16TestValues))]
        public void Test_ManagedNullableInt16List(short?[] values)
        {
            RunManagedTests(obj => obj.NullableInt16List, values);
        }

        [TestCaseSource(nameof(NullableInt32TestValues))]
        public void Test_ManagedNullableInt32List(int?[] values)
        {
            RunManagedTests(obj => obj.NullableInt32List, values);
        }

        [TestCaseSource(nameof(NullableInt64TestValues))]
        public void Test_ManagedNullableInt64List(long?[] values)
        {
            RunManagedTests(obj => obj.NullableInt64List, values);
        }

        [TestCaseSource(nameof(NullableDecimalTestValues))]
        public void Test_ManagedNullableDecimalList(decimal?[] values)
        {
            RunManagedTests(obj => obj.NullableDecimalList, values);
        }

        [TestCaseSource(nameof(NullableDecimal128TestValues))]
        public void Test_ManagedNullableDecimal128List(Decimal128?[] values)
        {
            RunManagedTests(obj => obj.NullableDecimal128List, values);
        }

        [TestCaseSource(nameof(NullableObjectIdTestValues))]
        public void Test_ManagedNullableObjectIdList(ObjectId?[] values)
        {
            RunManagedTests(obj => obj.NullableObjectIdList, values);
        }

        [TestCaseSource(nameof(NullableGuidTestValues))]
        public void Test_ManagedNullableGuidList(Guid?[] values)
        {
            RunManagedTests(obj => obj.NullableGuidList, values);
        }

        [TestCaseSource(nameof(NullableDateTestValues))]
        public void Test_ManagedNullableDateTimeOffsetList(DateTimeOffset?[] values)
        {
            RunManagedTests(obj => obj.NullableDateTimeOffsetList, values);
        }

        [TestCaseSource(nameof(NullableStringTestValues))]
        public void Test_ManagedNullableStringList(string[] values)
        {
            RunManagedTests(obj => obj.NullableStringList, values);
        }

        [TestCaseSource(nameof(NullableByteArrayTestValues))]
        public void Test_ManagedNullableByteArrayList(byte[][] values)
        {
            RunManagedTests(obj => obj.NullableByteArrayList, values);
        }

        [TestCaseSource(nameof(RealmValueTestValues))]
        public void Test_ManagedRealmValueList(RealmValue[] values)
        {
            RunManagedTests(obj => obj.RealmValueList, values);
        }

        [TestCase]
        public void RequiredStringList_CanAddEmptyString()
        {
            var obj = new ObjectWithRequiredStringList();
            _realm.Write(() => _realm.Add(obj));

            _realm.Write(() => obj.Strings.Add(string.Empty));

            Assert.That(obj.Strings.Count, Is.EqualTo(1));
            Assert.That(obj.Strings[0], Is.EqualTo(string.Empty));
        }

        [TestCase]
        public void RequiredStringList_CanNotAddNullString()
        {
            var obj = new ObjectWithRequiredStringList();
            _realm.Write(() => _realm.Add(obj));

            var ex = Assert.Throws<ArgumentException>(() => _realm.Write(() => obj.Strings.Add(null!)))!;
            Assert.That(ex.Message, Does.Contain("Attempted to add null to a list of required values"));
        }

        [TestCase]
        public void RequiredStringList_WhenContainsEmptyString_CanAddToRealm()
        {
            var obj = new ObjectWithRequiredStringList();
            obj.Strings.Add(string.Empty);
            obj.Strings.Add("strings.NonEmpty");
            _realm.Write(() => _realm.Add(obj));

            Assert.That(obj.Strings.Count, Is.EqualTo(2));
            Assert.That(obj.Strings[0], Is.Empty);
            Assert.That(obj.Strings[1], Is.Not.Empty);
        }

        [TestCase]
        public void RequiredStringList_WhenContainsNull_CanNotAddToRealm()
        {
            var obj = new ObjectWithRequiredStringList();
            obj.Strings.Add(null!);
            obj.Strings.Add("strings.NonEmpty");
            var ex = Assert.Throws<ArgumentException>(() => _realm.Write(() => _realm.Add(obj)))!;
            Assert.That(ex.Message, Does.Contain("Attempted to add null to a list of required values"));
        }

        #endregion Managed Tests

        #region Unmanaged Tests

        [TestCaseSource(nameof(BooleanTestValues))]
        public void Test_UnmanagedBooleanList(bool[] values)
        {
            RunUnmanagedTests(o => o.BooleanList, values);
        }

        [TestCaseSource(nameof(ByteTestValues))]
        public void Test_UnmanagedByteList(byte[] values)
        {
            RunUnmanagedTests(o => o.ByteList, values);
        }

        [TestCaseSource(nameof(CharTestValues))]
        public void Test_UnmanagedCharList(char[] values)
        {
            RunUnmanagedTests(o => o.CharList, values);
        }

        [TestCaseSource(nameof(DoubleTestValues))]
        public void Test_UnmanagedDoubleList(double[] values)
        {
            RunUnmanagedTests(o => o.DoubleList, values);
        }

        [TestCaseSource(nameof(Int16TestValues))]
        public void Test_UnmanagedInt16List(short[] values)
        {
            RunUnmanagedTests(o => o.Int16List, values);
        }

        [TestCaseSource(nameof(Int32TestValues))]
        public void Test_UnmanagedInt32List(int[] values)
        {
            RunUnmanagedTests(o => o.Int32List, values);
        }

        [TestCaseSource(nameof(Int64TestValues))]
        public void Test_UnmanagedInt64List(long[] values)
        {
            RunUnmanagedTests(o => o.Int64List, values);
        }

        [TestCaseSource(nameof(DecimalTestValues))]
        public void Test_UnmanagedDecimalList(decimal[] values)
        {
            RunUnmanagedTests(obj => obj.DecimalList, values);
        }

        [TestCaseSource(nameof(Decimal128TestValues))]
        public void Test_UnmanagedDecimal128List(Decimal128[] values)
        {
            RunUnmanagedTests(obj => obj.Decimal128List, values);
        }

        [TestCaseSource(nameof(ObjectIdTestValues))]
        public void Test_UnmanagedObjectIdList(ObjectId[] values)
        {
            RunUnmanagedTests(obj => obj.ObjectIdList, values);
        }

        [TestCaseSource(nameof(GuidTestValues))]
        public void Test_UnmanagedGuidList(Guid[] values)
        {
            RunUnmanagedTests(obj => obj.GuidList, values);
        }

        [TestCaseSource(nameof(DateTestValues))]
        public void Test_UnmanagedDateTimeOffsetList(DateTimeOffset[] values)
        {
            RunUnmanagedTests(o => o.DateTimeOffsetList, values);
        }

        [TestCaseSource(nameof(NullableBooleanTestValues))]
        public void Test_UnmanagedNullableBooleanList(bool?[] values)
        {
            RunUnmanagedTests(o => o.NullableBooleanList, values);
        }

        [TestCaseSource(nameof(NullableByteTestValues))]
        public void Test_UnmanagedNullableByteList(byte?[] values)
        {
            RunUnmanagedTests(o => o.NullableByteList, values);
        }

        [TestCaseSource(nameof(NullableCharTestValues))]
        public void Test_UnmanagedNullableCharList(char?[] values)
        {
            RunUnmanagedTests(o => o.NullableCharList, values);
        }

        [TestCaseSource(nameof(NullableDoubleTestValues))]
        public void Test_UnmanagedNullableDoubleList(double?[] values)
        {
            RunUnmanagedTests(o => o.NullableDoubleList, values);
        }

        [TestCaseSource(nameof(NullableInt16TestValues))]
        public void Test_UnmanagedNullableInt16List(short?[] values)
        {
            RunUnmanagedTests(o => o.NullableInt16List, values);
        }

        [TestCaseSource(nameof(NullableInt32TestValues))]
        public void Test_UnmanagedNullableInt32List(int?[] values)
        {
            RunUnmanagedTests(o => o.NullableInt32List, values);
        }

        [TestCaseSource(nameof(NullableInt64TestValues))]
        public void Test_UnmanagedNullableInt64List(long?[] values)
        {
            RunUnmanagedTests(o => o.NullableInt64List, values);
        }

        [TestCaseSource(nameof(NullableDecimalTestValues))]
        public void Test_UnmanagedNullableDecimalList(decimal?[] values)
        {
            RunUnmanagedTests(obj => obj.NullableDecimalList, values);
        }

        [TestCaseSource(nameof(NullableDecimal128TestValues))]
        public void Test_UnmanagedNullableDecimal128List(Decimal128?[] values)
        {
            RunUnmanagedTests(obj => obj.NullableDecimal128List, values);
        }

        [TestCaseSource(nameof(NullableObjectIdTestValues))]
        public void Test_UnmanagedNullableObjectIdList(ObjectId?[] values)
        {
            RunUnmanagedTests(obj => obj.NullableObjectIdList, values);
        }

        [TestCaseSource(nameof(NullableGuidTestValues))]
        public void Test_UnmanagedNullableGuidList(Guid?[] values)
        {
            RunUnmanagedTests(obj => obj.NullableGuidList, values);
        }

        [TestCaseSource(nameof(NullableDateTestValues))]
        public void Test_UnmanagedNullableDateTimeOffsetList(DateTimeOffset?[] values)
        {
            RunUnmanagedTests(o => o.NullableDateTimeOffsetList, values);
        }

        // Unmanaged string lists can always contain null
        [TestCaseSource(nameof(NullableStringTestValues))]
        public void Test_UnmanagedStringList(string[] values)
        {
            RunUnmanagedTests(o => o.NullableStringList, values);
        }

        // Unmanaged byte[] lists can always contain null
        [TestCaseSource(nameof(NullableByteArrayTestValues))]
        public void Test_UnmanagedByteArrayList(byte[][] values)
        {
            RunUnmanagedTests(o => o.NullableByteArrayList, values);
        }

        [TestCaseSource(nameof(RealmValueTestValues))]
        public void Test_UnmanagedRealmValueList(RealmValue[] values)
        {
            RunUnmanagedTests(obj => obj.RealmValueList, values);
        }

        #endregion Unmanaged Tests

        #region Utils

        private void RunManagedTests<T>(Func<ListsObject, IList<T>> listGetter, T[] testList)
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var listObject = new ListsObject();
                var list = listGetter(listObject);

                var testData = new ListTestCaseData<T>(testList);
                testData.Seed(list);

                _realm.Write(() => _realm.Add(listObject));

                var managedList = listGetter(listObject);

                Assert.That(list, Is.Not.SameAs(managedList));

                RunTestsCore(testData, managedList);

                await testData.AssertThreadSafeReference(managedList);
                testData.AssertNotifications(managedList);
            }, timeout: 100000);
        }

        private static void RunUnmanagedTests<T>(Func<ListsObject, IList<T>> listGetter, T[] testList)
        {
            var listObject = new ListsObject();
            var list = listGetter(listObject);

            var testData = new ListTestCaseData<T>(testList);
            testData.Seed(list);

            RunTestsCore(testData, list);
        }

        private static void RunTestsCore<T>(ListTestCaseData<T> testData, IList<T> list)
        {
            testData.AssertEquality(list);
            testData.AssertCount(list);
            testData.AssertAccessByIndex(list);
            testData.AssertAccessByIterator(list);
            testData.AssertContains(list);
            testData.AssertIndexOf(list);

            testData.AssertInsert(list);
            testData.AssertMove(list);
            testData.AssertSet(list);
            testData.AssertRemove(list);
            testData.AssertRemoveAt(list);
            testData.AssertClear(list);
        }

        public class ListTestCaseData<T>
        {
            private readonly List<T> referenceList = new();

            public ListTestCaseData(params T[] listData)
            {
                listData ??= Array.Empty<T>();

                referenceList.AddRange(listData);
            }

            public void Seed(IList<T> list)
            {
                WriteIfNecessary(list, () =>
                {
                    list.Clear();

                    for (var i = 0; i < referenceList.Count; i++)
                    {
                        list.Add(referenceList[i]);
                    }
                });
            }

            public void AssertAccessByIterator(IList<T> list)
            {
                var iterator = 0;
                foreach (var item in list)
                {
                    Assert.That(item, Is.EqualTo(referenceList[iterator++]));
                }
            }

            public void AssertAccessByIndex(IList<T> list)
            {
                for (var i = 0; i < referenceList.Count; i++)
                {
                    Assert.That(list[i], Is.EqualTo(referenceList[i]));
                }

                Assert.That(() => list[-1], Throws.TypeOf<ArgumentOutOfRangeException>());
                Assert.That(() => list[list.Count], Throws.TypeOf<ArgumentOutOfRangeException>());
            }

            public void AssertEquality(IList<T> list)
            {
                Assert.That(list, Is.EquivalentTo(referenceList));
            }

            public void AssertIndexOf(IList<T> list)
            {
                foreach (var val in referenceList)
                {
                    Assert.That(list.IndexOf(val), Is.EqualTo(referenceList.IndexOf(val)));
                }
            }

            public void AssertContains(IList<T> list)
            {
                for (var i = 0; i < referenceList.Count; i++)
                {
                    var rv = referenceList[i];
                    Assert.That(list.Contains(rv), Is.True);
                }
            }

            public void AssertCount(IList<T> list)
            {
                Assert.That(list.Count, Is.EqualTo(referenceList.Count));
            }

            public void AssertInsert(IList<T> list)
            {
                if (!referenceList.Any())
                {
                    return;
                }

                Seed(list);

                var toInsert = referenceList[TestHelpers.Random.Next(0, referenceList.Count)];

                WriteIfNecessary(list, () =>
                {
                    list.Insert(0, toInsert);
                    list.Insert(list.Count, toInsert);

                    Assert.That(() => list.Insert(-1, toInsert), Throws.TypeOf<ArgumentOutOfRangeException>());
                    Assert.That(() => list.Insert(list.Count + 1, toInsert), Throws.TypeOf<ArgumentOutOfRangeException>());
                });

                Assert.That(list.First(), Is.EqualTo(toInsert));
                Assert.That(list.Last(), Is.EqualTo(toInsert));
            }

            public void AssertClear(IList<T> list)
            {
                WriteIfNecessary(list, () =>
                {
                    list.Clear();
                });

                Assert.That(list.Count, Is.EqualTo(0));
            }

            public void AssertSet(IList<T> list)
            {
                if (!referenceList.Any())
                {
                    return;
                }

                Seed(list);

                var indexToSet = TestHelpers.Random.Next(0, referenceList.Count);
                var valueToSet = referenceList[TestHelpers.Random.Next(0, referenceList.Count)];

                WriteIfNecessary(list, () =>
                {
                    list[indexToSet] = valueToSet;

                    Assert.That(list[indexToSet], Is.EqualTo(valueToSet));
                    Assert.That(() => list[-1] = valueToSet, Throws.TypeOf<ArgumentOutOfRangeException>());
                    Assert.That(() => list[list.Count] = valueToSet, Throws.TypeOf<ArgumentOutOfRangeException>());
                });
            }

            public void AssertMove(IList<T> list)
            {
                if (!referenceList.Any())
                {
                    return;
                }

                Seed(list);

                var from = TestHelpers.Random.Next(0, list.Count);
                var to = TestHelpers.Random.Next(0, list.Count);

                WriteIfNecessary(list, () =>
                {
                    list.Move(from, to);

                    Assert.That(() => list.Move(-1, to), Throws.TypeOf<ArgumentOutOfRangeException>());
                    Assert.That(() => list.Move(from, -1), Throws.TypeOf<ArgumentOutOfRangeException>());
                    Assert.That(() => list.Move(list.Count + 1, to), Throws.TypeOf<ArgumentOutOfRangeException>());
                    Assert.That(() => list.Move(from, list.Count + 1), Throws.TypeOf<ArgumentOutOfRangeException>());
                });

                Assert.That(list[to], Is.EqualTo(referenceList[from]));
            }

            public void AssertRemove(IList<T> list)
            {
                if (!referenceList.Any())
                {
                    return;
                }

                Seed(list);

                var copyReferenceList = referenceList.ToList();
                var toRemove = copyReferenceList[TestHelpers.Random.Next(copyReferenceList.Count)];

                copyReferenceList.Remove(toRemove);

                WriteIfNecessary(list, () =>
                {
                    list.Remove(toRemove);

                    Assert.That(() => list.RemoveAt(-1), Throws.TypeOf<ArgumentOutOfRangeException>());
                    Assert.That(() => list.RemoveAt(list.Count), Throws.TypeOf<ArgumentOutOfRangeException>());
                });

                Assert.That(list, Is.EquivalentTo(copyReferenceList));
            }

            public void AssertRemoveAt(IList<T> list)
            {
                if (!referenceList.Any())
                {
                    return;
                }

                Seed(list);

                var copyReferenceList = referenceList.ToList();
                var toRemoveIndex = TestHelpers.Random.Next(copyReferenceList.Count);

                copyReferenceList.RemoveAt(toRemoveIndex);

                WriteIfNecessary(list, () =>
                {
                    list.RemoveAt(toRemoveIndex);

                    Assert.That(() => list.RemoveAt(-1), Throws.TypeOf<ArgumentOutOfRangeException>());
                    Assert.That(() => list.RemoveAt(list.Count), Throws.TypeOf<ArgumentOutOfRangeException>());
                });

                Assert.That(list, Is.EquivalentTo(copyReferenceList));
            }

            public async Task AssertThreadSafeReference(IList<T> list)
            {
                Assert.That(list, Is.TypeOf<RealmList<T>>());

                var tsr = ThreadSafeReference.Create(list);
                var originalThreadId = Environment.CurrentManagedThreadId;

                await Task.Run(() =>
                {
                    Assert.That(Environment.CurrentManagedThreadId, Is.Not.EqualTo(originalThreadId));

                    using var bgRealm = Realm.GetInstance(list.AsRealmCollection().Realm.Config);
                    var backgroundList = bgRealm.ResolveReference(tsr)!;

                    for (var i = 0; i < backgroundList.Count; i++)
                    {
                        Assert.That(backgroundList[i], Is.EqualTo(referenceList[i]));
                    }
                });
            }

            public void AssertNotifications(IList<T> list)
            {
                if (!referenceList.Any())
                {
                    return;
                }

                Assert.That(list, Is.TypeOf<RealmList<T>>());

                var realm = list.AsRealmCollection().Realm;

                var changeSetList = new List<ChangeSet>();
                using var token = list.SubscribeForNotifications((collection, changes) =>
                {
                    if (changes != null)
                    {
                        changeSetList.Add(changes);
                    }
                });

                // Add
                Seed(list);

                VerifyNotifications(realm, changeSetList, () =>
                {
                    Assert.That(changeSetList[0].InsertedIndices, Is.EquivalentTo(Enumerable.Range(0, referenceList.Count)));
                });

                // Insert
                var toInsert = referenceList[TestHelpers.Random.Next(0, referenceList.Count)];

                WriteIfNecessary(list, () =>
                {
                    list.Insert(0, toInsert);
                    list.Insert(list.Count, toInsert);
                });

                VerifyNotifications(realm, changeSetList, () =>
                {
                    Assert.That(changeSetList[0].InsertedIndices, Is.EquivalentTo(new[] { 0, list.Count - 1 }));
                });

                // Remove
                realm.Write(() =>
                {
                    list.Remove(toInsert);
                    list.RemoveAt(list.Count - 1);
                });

                VerifyNotifications(realm, changeSetList, () =>
                {
                    Assert.That(changeSetList[0].DeletedIndices, Is.EquivalentTo(new[] { 0, list.Count + 1 }));
                });

                // Set
                var indexToSet = TestHelpers.Random.Next(0, referenceList.Count);
                var previousValue = list[indexToSet];
                var valueToSet = referenceList[TestHelpers.Random.Next(0, referenceList.Count)];

                WriteIfNecessary(list, () =>
                {
                    list[indexToSet] = valueToSet;
                });

                VerifyNotifications(realm, changeSetList, () =>
                {
                    Assert.That(changeSetList[0].ModifiedIndices, Is.EquivalentTo(new[] { indexToSet }));
                });

                WriteIfNecessary(list, () =>
                {
                    list[indexToSet] = previousValue;
                });

                VerifyNotifications(realm, changeSetList, () =>
                {
                    Assert.That(changeSetList[0].ModifiedIndices, Is.EquivalentTo(new[] { indexToSet }));
                });

                // Move
                var from = TestHelpers.Random.Next(0, list.Count);
                var to = TestHelpers.Random.Next(0, list.Count);

                realm.Write(() =>
                {
                    list.Move(from, to);
                });

                if (from != to)
                {
                    VerifyNotifications(realm, changeSetList, () =>
                    {
                        Assert.That(changeSetList[0].Moves.Length, Is.EqualTo(1));
                        var move = changeSetList[0].Moves[0];

                        // Moves may be reported with swapped from/to arguments if the elements are adjacent
                        if (move.From == to)
                        {
                            Assert.That(move.From, Is.EqualTo(to));
                            Assert.That(move.To, Is.EqualTo(from));
                        }
                        else
                        {
                            Assert.That(move.From, Is.EqualTo(from));
                            Assert.That(move.To, Is.EqualTo(to));
                        }
                    });
                }

                // Clear
                realm.Write(() =>
                {
                    list.Clear();
                });

                VerifyNotifications(realm, changeSetList, () =>
                {
                    Assert.That(changeSetList[0].DeletedIndices, Is.EquivalentTo(Enumerable.Range(0, referenceList.Count)));
                });
            }

            private static void VerifyNotifications(Realm realm, List<ChangeSet> notifications, Action verifier)
            {
                realm.Refresh();
                Assert.That(notifications.Count, Is.EqualTo(1));
                verifier();
                notifications.Clear();
            }

            private static void WriteIfNecessary(IEnumerable<T> collection, Action writeAction)
            {
                Transaction? transaction = null;
                try
                {
                    if (collection is RealmCollectionBase<T> realmCollection)
                    {
                        transaction = realmCollection.Realm.BeginWrite();
                    }

                    writeAction();

                    transaction?.Commit();
                }
                catch
                {
                    transaction?.Rollback();
                    throw;
                }
            }
        }

        #endregion
    }
}
