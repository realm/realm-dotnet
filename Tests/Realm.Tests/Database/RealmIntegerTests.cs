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
using NUnit.Framework;

namespace Realms.Tests.Database
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

        public static byte[] ByteValues = new[] { (byte)0, (byte)1, byte.MaxValue };

        [Test]
        public void RealmInteger_WhenByte_IncrementTests(
           [ValueSource(nameof(ByteValues))] byte original,
           [ValueSource(nameof(ByteValues))] byte value)
        {
            var managedCounter = _realm.Write(() => _realm.Add(new CounterObject { ByteProperty = original }));
            _realm.Write(() => managedCounter.ByteProperty.Increment(value));
            Assert.That((byte)managedCounter.ByteProperty, Is.EqualTo((byte)(original + value)));

            var unmanagedCounter = new CounterObject { ByteProperty = original };
            Assert.That(() => unmanagedCounter.ByteProperty.Increment(value), Throws.TypeOf<NotSupportedException>());
        }

        public static short[] ShortValues = new[] { (short)0, (short)1, (short)-1, short.MinValue, short.MaxValue };

        [Test]
        public void RealmInteger_WhenShort_IncrementTests(
           [ValueSource(nameof(ShortValues))] short original,
           [ValueSource(nameof(ShortValues))] short value)
        {
            var managedCounter = _realm.Write(() => _realm.Add(new CounterObject { Int16Property = original }));
            _realm.Write(() => managedCounter.Int16Property.Increment(value));
            Assert.That((short)managedCounter.Int16Property, Is.EqualTo((short)(original + value)));

            var unmanagedCounter = new CounterObject { Int16Property = original };
            Assert.That(() => unmanagedCounter.Int16Property.Increment(value), Throws.TypeOf<NotSupportedException>());
        }

        public static int[] IntValues = new[] { 0, 1, -1, int.MinValue, int.MaxValue };

        [Test]
        public void RealmInteger_WhenInt_IncrementTests(
           [ValueSource(nameof(IntValues))] int original,
           [ValueSource(nameof(IntValues))] int value)
        {
            var managedCounter = _realm.Write(() => _realm.Add(new CounterObject { Int32Property = original }));
            _realm.Write(() => managedCounter.Int32Property.Increment(value));
            Assert.That((int)managedCounter.Int32Property, Is.EqualTo(original + value));

            var unmanagedCounter = new CounterObject { Int32Property = original };
            Assert.That(() => unmanagedCounter.Int32Property.Increment(value), Throws.TypeOf<NotSupportedException>());
        }

        public static long[] LongValues = new[] { 0L, 1L, -1L, long.MaxValue, long.MinValue };

        [Test]
        public void RealmInteger_WhenLong_IncrementTests(
            [ValueSource(nameof(LongValues))] long original,
            [ValueSource(nameof(LongValues))] long value)
        {
            var managedCounter = _realm.Write(() => _realm.Add(new CounterObject { Int64Property = original }));
            _realm.Write(() => managedCounter.Int64Property.Increment(value));
            Assert.That((long)managedCounter.Int64Property, Is.EqualTo(original + value));

            var unmanagedCounter = new CounterObject { Int64Property = original };
            Assert.That(() => unmanagedCounter.Int64Property.Increment(value), Throws.TypeOf<NotSupportedException>());
        }
    }
}
