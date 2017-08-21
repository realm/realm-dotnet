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
using Nito.AsyncEx;
using NUnit.Framework;
using Realms;

namespace Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class ListOfPrimitivesTests : RealmInstanceTest
    {
        private readonly Random _random = new Random();

        private ListsObject _listsObject;

        protected override void CustomSetUp()
        {
            base.CustomSetUp();

            _realm.Write(() =>
            {
                _listsObject = _realm.Add(new ListsObject());
            });
        }

        #region TestCaseSources

        private static readonly IEnumerable<bool?[]> _booleanValues = new[]
        {
            new bool?[] { true },
            new bool?[] { null },
            new bool?[] { true, true, null, false },
        };

        public static IEnumerable<object> BooleanTestValues()
        {
            yield return new object[] { null };
            var values = _booleanValues.Select(v => v.Where(b => b.HasValue).Select(b => b.Value).ToArray());
            foreach (var value in values.Where(a => a.Any()))
            {
                yield return new object[] { value.ToArray() };
            }
        }

        public static IEnumerable<object> NullableBooleanTestValues()
        {
            yield return new object[] { null };
            foreach (var item in _booleanValues)
            {
                yield return new object[] { item };
            }
        }

        private static readonly IEnumerable<byte?[]> _byteValues = new[]
        {
            new byte?[] { 0 },
            new byte?[] { null },
            new byte?[] { byte.MinValue, byte.MaxValue, 0 },
            new byte?[] { 1, 2, 3, null },
        };

        public static IEnumerable<object> ByteTestValues()
        {
            yield return new object[] { null };
            var values = _byteValues.Select(v => v.Where(b => b.HasValue).Select(b => b.Value).ToArray());
            foreach (var value in values.Where(a => a.Any()))
            {
                yield return new object[] { value.ToArray() };
            }
        }

        public static IEnumerable<object> NullableByteTestValues()
        {
            yield return new object[] { null };
            foreach (var value in _byteValues)
            {
                yield return new object[] { value.ToArray() };
            }
        }

        private static readonly IEnumerable<char?[]> _charValues = new[]
        {
            new char?[] { 'a' },
            new char?[] { null },
            new char?[] { char.MinValue, char.MaxValue },
            new char?[] { 'a', 'b', 'c', 'b', null }
        };

        public static IEnumerable<object> CharTestValues()
        {
            yield return new object[] { null };
            var values = _charValues.Select(v => v.Where(b => b.HasValue).Select(b => b.Value).ToArray());
            foreach (var value in values.Where(a => a.Any()))
            {
                yield return new object[] { value.ToArray() };
            }
        }

        public static IEnumerable<object> NullableCharTestValues()
        {
            yield return new object[] { null };
            foreach (var value in _charValues)
            {
                yield return new object[] { value.ToArray() };
            }
        }

        private static readonly IEnumerable<double?[]> _doubleValues = new[]
        {
            new double?[] { 1.4 },
            new double?[] { null },
            new double?[] { double.MinValue, double.MaxValue, 0 },
            new double?[] { -1, 3.4, null, 5.3, 9 }
        };

        public static IEnumerable<object> DoubleTestValues()
        {
            yield return new object[] { null };
            var values = _doubleValues.Select(v => v.Where(b => b.HasValue).Select(b => b.Value).ToArray());
            foreach (var value in values.Where(a => a.Any()))
            {
                yield return new object[] { value.ToArray() };
            }
        }

        public static IEnumerable<object> NullableDoubleTestValues()
        {
            yield return new object[] { null };
            foreach (var value in _doubleValues)
            {
                yield return new object[] { value.ToArray() };
            }
        }

        private static readonly IEnumerable<short?[]> _shortValues = new[]
        {
            new short?[] { 1 },
            new short?[] { null },
            new short?[] { short.MaxValue, short.MinValue, 0 },
            new short?[] { 3, -1, null, 45, null },
        };

        public static IEnumerable<object> Int16TestValues()
        {
            yield return new object[] { null };
            var values = _shortValues.Select(v => v.Where(b => b.HasValue).Select(b => b.Value).ToArray());
            foreach (var value in values.Where(a => a.Any()))
            {
                yield return new object[] { value.ToArray() };
            }
        }

        public static IEnumerable<object> NullableInt16TestValues()
        {
            yield return new object[] { null };
            foreach (var value in _shortValues)
            {
                yield return new object[] { value.ToArray() };
            }
        }

        private static readonly IEnumerable<int?[]> _intValues = new[]
        {
            new int?[] { 1 },
            new int?[] { null },
            new int?[] { int.MaxValue, int.MinValue, 0 },
            new int?[] { -5, 3, 9, null, 350 },
        };

        public static IEnumerable<object> Int32TestValues()
        {
            yield return new object[] { null };
            var values = _intValues.Select(v => v.Where(b => b.HasValue).Select(b => b.Value).ToArray());
            foreach (var value in values.Where(a => a.Any()))
            {
                yield return new object[] { value.ToArray() };
            }
        }

        public static IEnumerable<object> NullableInt32TestValues()
        {
            yield return new object[] { null };
            foreach (var value in _intValues)
            {
                yield return new object[] { value.ToArray() };
            }
        }

        private static readonly IEnumerable<long?[]> _longValues = new[]
        {
            new long?[] { 1 },
            new long?[] { null },
            new long?[] { long.MaxValue, long.MinValue, 0 },
            new long?[] { 4, -39, 81, null, -69324 },
        };

        public static IEnumerable<object> Int64TestValues()
        {
            yield return new object[] { null };
            var values = _longValues.Select(v => v.Where(b => b.HasValue).Select(b => b.Value).ToArray());
            foreach (var value in values.Where(a => a.Any()))
            {
                yield return new object[] { value.ToArray() };
            }
        }

        public static IEnumerable<object> NullableInt64TestValues()
        {
            yield return new object[] { null };
            foreach (var value in _longValues)
            {
                yield return new object[] { value.ToArray() };
            }
        }

        private static readonly IEnumerable<DateTimeOffset?[]> _dateValues = new[]
        {
            new DateTimeOffset?[] { DateTimeOffset.UtcNow.AddDays(-4) },
            new DateTimeOffset?[] { null },
            new DateTimeOffset?[] { DateTimeOffset.MinValue, DateTimeOffset.MaxValue, DateTimeOffset.UtcNow },
            new DateTimeOffset?[] { DateTimeOffset.UtcNow.AddDays(5), null, DateTimeOffset.UtcNow.AddDays(-39), DateTimeOffset.UtcNow.AddDays(81), DateTimeOffset.UtcNow.AddDays(-69324) },
        };

        public static IEnumerable<object> DateTestValues()
        {
            yield return new object[] { null };
            var values = _dateValues.Select(v => v.Where(b => b.HasValue).Select(b => b.Value).ToArray());
            foreach (var value in values.Where(a => a.Any()))
            {
                yield return new object[] { value.ToArray() };
            }
        }

        public static IEnumerable<object> NullableDateTestValues()
        {
            yield return new object[] { null };
            foreach (var value in _dateValues)
            {
                yield return new object[] { value.ToArray() };
            }
        }

        public static IEnumerable<object> StringTestValues()
        {
            yield return new object[] { null };
            yield return new object[] { new string[] { string.Empty } };
            yield return new object[] { new string[] { null } };
            yield return new object[] { new string[] { " " } };
            yield return new object[] { new string[] { "abc", "cdf", "az" } };
            yield return new object[] { new string[] { "a", null, "foo", "bar", null } };
        }

        public static IEnumerable<object> ByteArrayTestValues()
        {
            yield return new object[] { null };
            yield return new object[] { new byte[][] { new byte[0] } };
            yield return new object[] { new byte[][] { null } };
            yield return new object[] { new byte[][] { new byte[] { 0 } } };
            yield return new object[] { new byte[][] { new byte[] { 0, byte.MinValue, byte.MaxValue } } };
            yield return new object[] { new byte[][] { TestHelpers.GetBytes(3), TestHelpers.GetBytes(5), TestHelpers.GetBytes(7) } };
            yield return new object[] { new byte[][] { TestHelpers.GetBytes(1), null, TestHelpers.GetBytes(3), TestHelpers.GetBytes(3), null } };
        }

        #endregion

        #region SmokeTests

        [TestCaseSource(nameof(BooleanTestValues))]
        public void Test_BooleanList(bool[] values)
        {
            SmokeTest(_listsObject.BooleanList, values);
        }

        [TestCaseSource(nameof(ByteTestValues))]
        public void Test_ByteCounterList(byte[] values)
        {
            SmokeTest(_listsObject.ByteCounterList, values.ToInteger());
        }

        [TestCaseSource(nameof(ByteTestValues))]
        public void Test_ByteList(byte[] values)
        {
            SmokeTest(_listsObject.ByteList, values);
        }

        [TestCaseSource(nameof(CharTestValues))]
        public void Test_CharList(char[] values)
        {
            SmokeTest(_listsObject.CharList, values);
        }

        [TestCaseSource(nameof(DoubleTestValues))]
        public void Test_DoubleList(double[] values)
        {
            SmokeTest(_listsObject.DoubleList, values);
        }

        [TestCaseSource(nameof(Int16TestValues))]
        public void Test_Int16CounterList(short[] values)
        {
            SmokeTest(_listsObject.Int16CounterList, values.ToInteger());
        }

        [TestCaseSource(nameof(Int16TestValues))]
        public void Test_Int16List(short[] values)
        {
            SmokeTest(_listsObject.Int16List, values);
        }

        [TestCaseSource(nameof(Int32TestValues))]
        public void Test_Int32CounterList(int[] values)
        {
            SmokeTest(_listsObject.Int32CounterList, values.ToInteger());
        }

        [TestCaseSource(nameof(Int32TestValues))]
        public void Test_Int32List(int[] values)
        {
            SmokeTest(_listsObject.Int32List, values);
        }

        [TestCaseSource(nameof(Int64TestValues))]
        public void Test_Int64CounterList(long[] values)
        {
            SmokeTest(_listsObject.Int64CounterList, values.ToInteger());
        }

        [TestCaseSource(nameof(Int64TestValues))]
        public void Test_Int64List(long[] values)
        {
            SmokeTest(_listsObject.Int64List, values);
        }

        [TestCaseSource(nameof(DateTestValues))]
        public void Test_DateTimeOffsetList(DateTimeOffset[] values)
        {
            SmokeTest(_listsObject.DateTimeOffsetList, values);
        }

        [TestCaseSource(nameof(NullableBooleanTestValues))]
        public void Test_NullableBooleanList(bool?[] values)
        {
            SmokeTest(_listsObject.NullableBooleanList, values);
        }

        [TestCaseSource(nameof(NullableByteTestValues))]
        public void Test_NullableByteCounterList(byte?[] values)
        {
            SmokeTest(_listsObject.NullableByteCounterList, values.ToInteger());
        }

        [TestCaseSource(nameof(NullableByteTestValues))]
        public void Test_NullableByteList(byte?[] values)
        {
            SmokeTest(_listsObject.NullableByteList, values);
        }

        [TestCaseSource(nameof(NullableCharTestValues))]
        public void Test_NullableCharList(char?[] values)
        {
            SmokeTest(_listsObject.NullableCharList, values);
        }

        [TestCaseSource(nameof(NullableDoubleTestValues))]
        public void Test_NullableDoubleList(double?[] values)
        {
            SmokeTest(_listsObject.NullableDoubleList, values);
        }

        [TestCaseSource(nameof(NullableInt16TestValues))]
        public void Test_NullableInt16CounterList(short?[] values)
        {
            SmokeTest(_listsObject.NullableInt16CounterList, values.ToInteger());
        }

        [TestCaseSource(nameof(NullableInt16TestValues))]
        public void Test_NullableInt16List(short?[] values)
        {
            SmokeTest(_listsObject.NullableInt16List, values);
        }

        [TestCaseSource(nameof(NullableInt32TestValues))]
        public void Test_NullableInt32CounterList(int?[] values)
        {
            SmokeTest(_listsObject.NullableInt32CounterList, values.ToInteger());
        }

        [TestCaseSource(nameof(NullableInt32TestValues))]
        public void Test_NullableInt32List(int?[] values)
        {
            SmokeTest(_listsObject.NullableInt32List, values);
        }

        [TestCaseSource(nameof(NullableInt64TestValues))]
        public void Test_NullableInt64CounterList(long?[] values)
        {
            SmokeTest(_listsObject.NullableInt64CounterList, values.ToInteger());
        }

        [TestCaseSource(nameof(NullableInt64TestValues))]
        public void Test_NullableInt64List(long?[] values)
        {
            SmokeTest(_listsObject.NullableInt64List, values);
        }

        [TestCaseSource(nameof(NullableDateTestValues))]
        public void Test_NullableDateTimeOffsetList(DateTimeOffset?[] values)
        {
            SmokeTest(_listsObject.NullableDateTimeOffsetList, values);
        }

        [TestCaseSource(nameof(StringTestValues))]
        public void Test_StringList(string[] values)
        {
            SmokeTest(_listsObject.StringList, values);
        }

        [TestCaseSource(nameof(ByteArrayTestValues))]
        public void Test_ByteArrayList(byte[][] values)
        {
            SmokeTest(_listsObject.ByteArrayList, values);
        }

        private void SmokeTest<T>(IList<T> items, T[] toAdd)
        {
            if (toAdd == null)
            {
                toAdd = new T[0];
            }

            // Test add
            _realm.Write(() =>
            {
                foreach (var item in toAdd)
                {
                    items.Add(item);
                }
            });

            // Test iterating
            var iterator = 0;
            foreach (var item in items)
            {
                Assert.That(item, Is.EqualTo(toAdd[iterator++]));
            }

            // Test access by index
            for (var i = 0; i < items.Count; i++)
            {
                Assert.That(items[i], Is.EqualTo(toAdd[i]));
            }

            // Test indexOf
            foreach (var item in toAdd)
            {
                Assert.That(items.IndexOf(item), Is.EqualTo(Array.IndexOf(toAdd, item)));
            }

            var reference = ThreadSafeReference.Create(items);
            AsyncContext.Run(async () =>
            {
                await Task.Run(() =>
                {
                    using (var realm = Realm.GetInstance(_realm.Config))
                    {
                        var backgroundList = realm.ResolveReference(reference);
                        for (var i = 0; i < backgroundList.Count; i++)
                        {
                            Assert.That(backgroundList[i], Is.EqualTo(toAdd[i]));
                        }
                    }
                });
            });

            if (toAdd.Any())
            {
                // Test insert
                var toInsert = toAdd[_random.Next(0, toAdd.Length)];
                _realm.Write(() =>
                {
                    items.Insert(0, toInsert);
                    items.Insert(items.Count, toInsert);
                });

                Assert.That(items.First(), Is.EqualTo(toInsert));
                Assert.That(items.Last(), Is.EqualTo(toInsert));

                // Test remove
                _realm.Write(() =>
                {
                    items.Remove(toInsert);
                    items.RemoveAt(items.Count - 1);
                });

                CollectionAssert.AreEqual(items, toAdd);

                // Test move
                var from = TestHelpers.Random.Next(0, items.Count);
                var to = TestHelpers.Random.Next(0, items.Count);

                _realm.Write(() =>
                {
                    items.Move(from, to);
                });

                Assert.That(items[to], Is.EqualTo(toAdd[from]));
            }

            // Test Clear
            _realm.Write(() =>
            {
                items.Clear();
            });

            Assert.That(items, Is.Empty);
        }

        #endregion
    }
}
