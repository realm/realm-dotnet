////////////////////////////////////////////////////////////////////////////
//
// Copyright 2017 Realm Inc.
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
using NUnit.Framework;
using Realms;
using Realms.Helpers;

namespace Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class RealmIntegerTests : RealmInstanceTest
    {
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(255)]
        [TestCase(-1)]
        [TestCase(-100)]
        public void RealmInteger_HasDefaultValue(int defaultValue)
        {
            var byteValue = (byte)Math.Max(defaultValue, 0);

            var counter = new CounterObject
            {
                ByteProperty = byteValue,
                Int16Property = (short)defaultValue,
                Int32Property = defaultValue,
                Int64Property = defaultValue,
            };

            Assert.That(counter.ByteProperty == byteValue);
            Assert.That(counter.Int16Property == defaultValue);
            Assert.That(counter.Int32Property == defaultValue);
            Assert.That(counter.Int64Property == defaultValue);

            _realm.Write(() =>
            {
                _realm.Add(counter);
            });

            Assert.That(counter.ByteProperty == byteValue);
            Assert.That(counter.Int16Property == defaultValue);
            Assert.That(counter.Int32Property == defaultValue);
            Assert.That(counter.Int64Property == defaultValue);
        }

        [TestCase(null)]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(255)]
        [TestCase(-1)]
        [TestCase(-100)]
        public void NullableRealmInteger_HasDefaultValue(int? defaultValue)
        {
            var byteValue = defaultValue.HasValue ? (byte?)Math.Max(defaultValue.Value, 0) : null;

            var counter = new CounterObject
            {
                NullableByteProperty = byteValue,
                NullableInt16Property = (short?)defaultValue,
                NullableInt32Property = defaultValue,
                NullableInt64Property = defaultValue,
            };

            Assert.That(counter.NullableByteProperty == byteValue);
            Assert.That(counter.NullableInt16Property == defaultValue);
            Assert.That(counter.NullableInt32Property == defaultValue);
            Assert.That(counter.NullableInt64Property == defaultValue);

            _realm.Write(() =>
            {
                _realm.Add(counter);
            });

            Assert.That(counter.NullableByteProperty == byteValue);
            Assert.That(counter.NullableInt16Property == defaultValue);
            Assert.That(counter.NullableInt32Property == defaultValue);
            Assert.That(counter.NullableInt64Property == defaultValue);
        }

        [TestCaseSource(nameof(ByteIncrementTestCases))]
        public void RealmInteger_WhenByte_IncrementTests(byte original, byte value, bool managed)
        {
            var counter = new CounterObject
            {
                ByteProperty = original,
            };

            var sum = (byte)(original + value);
            if (managed)
            {
                _realm.Write(() => _realm.Add(counter));
                _realm.Write(() => counter.ByteProperty.Increment(value));
                Assert.That((byte)counter.ByteProperty, Is.EqualTo(sum));
            }
            else
            {
                var result = counter.ByteProperty.Increment(value);
                Assert.That((byte)result, Is.EqualTo(sum));
                Assert.That((byte)counter.ByteProperty, Is.EqualTo(original));
            }
        }

        [TestCaseSource(nameof(ShortIncrementTestCases))]
        public void RealmInteger_WhenShort_IncrementTests(short original, short value, bool managed)
        {
            var counter = new CounterObject
            {
                Int16Property = original,
            };

            var sum = (short)(original + value);
            if (managed)
            {
                _realm.Write(() => _realm.Add(counter));
                _realm.Write(() => counter.Int16Property.Increment(value));
                Assert.That((short)counter.Int16Property, Is.EqualTo(sum));
            }
            else
            {
                var result = counter.Int16Property.Increment(value);
                Assert.That((short)result, Is.EqualTo(sum));
                Assert.That((short)counter.Int16Property, Is.EqualTo(original));
            }
        }

        [TestCaseSource(nameof(IntIncrementTestCases))]
        public void RealmInteger_WhenInt_IncrementTests(int original, int value, bool managed)
        {
            var counter = new CounterObject
            {
                Int32Property = original,
            };

            var sum = original + value;
            if (managed)
            {
                _realm.Write(() => _realm.Add(counter));
                _realm.Write(() => counter.Int32Property.Increment(value));
                Assert.That((int)counter.Int32Property, Is.EqualTo(sum));
            }
            else
            {
                var result = counter.Int32Property.Increment(value);
                Assert.That((int)result, Is.EqualTo(sum));
                Assert.That((int)counter.Int32Property, Is.EqualTo(original));
            }
        }

        [TestCaseSource(nameof(LongIncrementTestCases))]
        public void RealmInteger_WhenLong_IncrementTests(long original, long value, bool managed)
        {
            var counter = new CounterObject
            {
                Int64Property = original,
            };

            var sum = original + value;
            if (managed)
            {
                _realm.Write(() => _realm.Add(counter));
                _realm.Write(() => counter.Int64Property.Increment(value));
                Assert.That((long)counter.Int64Property, Is.EqualTo(sum));
            }
            else
            {
                var result = counter.Int64Property.Increment(value);
                Assert.That((long)result, Is.EqualTo(sum));
                Assert.That((long)counter.Int64Property, Is.EqualTo(original));
            }
        }

        private static IEnumerable<object> ByteIncrementTestCases()
        {
            var values = new byte[] { 0, 1, byte.MaxValue };
            foreach (var original in values)
            {
                foreach (var value in values)
                {
                    for (var i = 0; i < 2; i++)
                    {
                        yield return new object[] { original, value, i == 0 };
                    }
                }
            }
        }

        private static IEnumerable<object> ShortIncrementTestCases()
        {
            var values = new short[] { 0, 1, -1, short.MaxValue, short.MinValue };
            foreach (var original in values)
            {
                foreach (var value in values)
                {
                    for (var i = 0; i < 2; i++)
                    {
                        yield return new object[] { original, value, i == 0 };
                    }
                }
            }
        }

        private static IEnumerable<object> IntIncrementTestCases()
        {
            var values = new int[] { 0, 1, -1, int.MaxValue, int.MinValue };
            foreach (var original in values)
            {
                foreach (var value in values)
                {
                    for (var i = 0; i < 2; i++)
                    {
                        yield return new object[] { original, value, i == 0 };
                    }
                }
            }
        }

        private static IEnumerable<object> LongIncrementTestCases()
        {
            var values = new long[] { 0, 1, -1, long.MaxValue, long.MinValue };
            foreach (var original in values)
            {
                foreach (var value in values)
                {
                    for (var i = 0; i < 2; i++)
                    {
                        yield return new object[] { original, value, i == 0 };
                    }
                }
            }
        }
    }
}
