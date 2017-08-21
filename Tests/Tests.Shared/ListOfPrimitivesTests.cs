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

        public static object[] BooleanTestValues =
        {
            new object[] { null },
            new object[] { new[] { true } },
            new object[] { new[] { true, true, false } },
        };

        [TestCaseSource(nameof(BooleanTestValues))]
        public void Test_BooleanList(bool[] values)
        {
            SmokeTest(_listsObject.BooleanList, values);
        }

        public static object[] ByteTestValues =
        {
            new object[] { null },
            new object[] { new byte[] { 0 } },
            new object[] { new byte[] { byte.MinValue, byte.MaxValue, 0 } },
            new object[] { new byte[] { 1, 2, 3 } },
        };

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

        public static object[] CharTestValues =
        {
            new object[] { null },
            new object[] { new[] { 'a' } },
            new object[] { new[] { char.MinValue, char.MaxValue } },
            new object[] { new[] { 'a', 'b', 'c', 'b' } }
        };

        [TestCaseSource(nameof(CharTestValues))]
        public void Test_CharList(char[] values)
        {
            SmokeTest(_listsObject.CharList, values);
        }

        public static object[] DoubleTestValues =
        {
            new object[] { null },
            new object[] { new[] { 1.4 } },
            new object[] { new[] { double.MinValue, double.MaxValue, 0 } },
            new object[] { new[] { -1, 3.4, 5.3, 9 } }
        };

        [TestCaseSource(nameof(DoubleTestValues))]
        public void Test_DoubleList(double[] values)
        {
            SmokeTest(_listsObject.DoubleList, values);
        }

        public static object[] Int16TestValues =
        {
            new object[] { null },
            new object[] { new short[] { 1 } },
            new object[] { new short[] { short.MaxValue, short.MinValue, 0 } },
            new object[] { new short[] { 3, -1, 45 } },
        };

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

        public static object[] Int32TestValues =
        {
            new object[] { null },
            new object[] { new[] { 1 } },
            new object[] { new[] { int.MaxValue, int.MinValue, 0 } },
            new object[] { new[] { -5, 3, 9, 350 } },
        };

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

        public static object[] Int64TestValues =
        {
            new object[] { null },
            new object[] { new[] { 1L } },
            new object[] { new[] { long.MaxValue, long.MinValue, 0 } },
            new object[] { new[] { 4, -39, 81L, -69324 } },
        };

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

        public static object[] DateTestValues =
        {
            new object[] { null },
            new object[] { new[] { DateTimeOffset.UtcNow.AddDays(-4) } },
            new object[] { new[] { DateTimeOffset.MinValue, DateTimeOffset.MaxValue, DateTimeOffset.UtcNow } },
            new object[] { new[] { DateTimeOffset.UtcNow.AddDays(5), DateTimeOffset.UtcNow.AddDays(-39), DateTimeOffset.UtcNow.AddDays(81), DateTimeOffset.UtcNow.AddDays(-69324) } },
        };

        [TestCaseSource(nameof(DateTestValues))]
        public void Test_DateTimeOffsetList(DateTimeOffset[] values)
        {
            SmokeTest(_listsObject.DateTimeOffsetList, values);
        }

        public static object[] NullableBooleanTestValues =
        {
            new object[] { null },
            new object[] { new bool?[] { true } },
            new object[] { new bool?[] { null } },
            new object[] { new bool?[] { true, true, null, false } },
        };

        [TestCaseSource(nameof(NullableBooleanTestValues))]
        public void Test_NullableBooleanList(bool?[] values)
        {
            SmokeTest(_listsObject.NullableBooleanList, values);
        }

        public static object[] NullableByteTestValues =
        {
            new object[] { null },
            new object[] { new byte?[] { 0 } },
            new object[] { new byte?[] { null } },
            new object[] { new byte?[] { byte.MinValue, byte.MaxValue, 0 } },
            new object[] { new byte?[] { 1, 2, 3, null } },
        };

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

        public static object[] NullableCharTestValues =
        {
            new object[] { null },
            new object[] { new char?[] { 'a' } },
            new object[] { new char?[] { null } },
            new object[] { new char?[] { char.MinValue, char.MaxValue } },
            new object[] { new char?[] { 'a', 'b', 'c', 'b', null } }
        };

        [TestCaseSource(nameof(NullableCharTestValues))]
        public void Test_NullableCharList(char?[] values)
        {
            SmokeTest(_listsObject.NullableCharList, values);
        }

        public static object[] NullableDoubleTestValues =
        {
            new object[] { null },
            new object[] { new double?[] { 1.4 } },
            new object[] { new double?[] { null } },
            new object[] { new double?[] { double.MinValue, double.MaxValue, 0 } },
            new object[] { new double?[] { -1, 3.4, null, 5.3, 9 } }
        };

        [TestCaseSource(nameof(NullableDoubleTestValues))]
        public void Test_NullableDoubleList(double?[] values)
        {
            SmokeTest(_listsObject.NullableDoubleList, values);
        }

        public static object[] NullableInt16TestValues =
        {
            new object[] { null },
            new object[] { new short?[] { 1 } },
            new object[] { new short?[] { null } },
            new object[] { new short?[] { short.MaxValue, short.MinValue, 0 } },
            new object[] { new short?[] { 3, -1, null, 45, null } },
        };

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

        public static object[] NullableInt32TestValues =
        {
            new object[] { null },
            new object[] { new int?[] { 1 } },
            new object[] { new int?[] { null } },
            new object[] { new int?[] { int.MaxValue, int.MinValue, 0 } },
            new object[] { new int?[] { -5, 3, 9, null, 350 } },
        };

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

        public static object[] NullableInt64TestValues =
        {
            new object[] { null },
            new object[] { new long?[] { 1 } },
            new object[] { new long?[] { null } },
            new object[] { new long?[] { long.MaxValue, long.MinValue, 0 } },
            new object[] { new long?[] { 4, -39, 81, null, -69324 } },
        };

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

        public static object[] NullableDateTestValues =
        {
            new object[] { null },
            new object[] { new DateTimeOffset?[] { DateTimeOffset.UtcNow.AddDays(-4) } },
            new object[] { new DateTimeOffset?[] { null } },
            new object[] { new DateTimeOffset?[] { DateTimeOffset.MinValue, DateTimeOffset.MaxValue, DateTimeOffset.UtcNow } },
            new object[] { new DateTimeOffset?[] { DateTimeOffset.UtcNow.AddDays(5), null, DateTimeOffset.UtcNow.AddDays(-39), DateTimeOffset.UtcNow.AddDays(81), DateTimeOffset.UtcNow.AddDays(-69324) } },
        };

        [TestCaseSource(nameof(NullableDateTestValues))]
        public void Test_NullableDateTimeOffsetList(DateTimeOffset?[] values)
        {
            SmokeTest(_listsObject.NullableDateTimeOffsetList, values);
        }

        public static object[] StringTestValues =
        {
            new object[] { null },
            new object[] { new string[] { string.Empty } },
            new object[] { new string[] { null } },
            new object[] { new string[] { " " } },
            new object[] { new string[] { "abc", "cdf", "az" } },
            new object[] { new string[] { "a", null, "foo", "bar", null } },
        };

        [TestCaseSource(nameof(StringTestValues))]
        public void Test_StringList(string[] values)
        {
            SmokeTest(_listsObject.StringList, values);
        }

        public static object[] ByteArrayTestValues =
                {
            new object[] { null },
            new object[] { new byte[][] { new byte[0] } },
            new object[] { new byte[][] { null } },
            new object[] { new byte[][] { new byte[] { 0 } } },
            new object[] { new byte[][] { new byte[] { 0, byte.MinValue, byte.MaxValue } } },
            new object[] { new byte[][] { TestHelpers.GetBytes(3), TestHelpers.GetBytes(5), TestHelpers.GetBytes(7) } },
            new object[] { new byte[][] { TestHelpers.GetBytes(1), null, TestHelpers.GetBytes(3), TestHelpers.GetBytes(3), null } },
        };

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
            }
        }
    }
}
