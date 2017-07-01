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

        [TestCaseSource(nameof(IncrementTestCases))]
        public void RealmInteger_IncrementTests(int original, int value, bool managed)
        {
            var counter = new CounterObject
            {
                Int32Property = original,
            };

            if (managed)
            {
                _realm.Write(() => _realm.Add(counter));
                _realm.Write(() => counter.Int32Property.Increment(value));
            }
            else
            {
                counter.Int32Property.Increment(value);
            }

            var expected = original + (managed ? value : 0);
            Assert.That((int)counter.Int32Property, Is.EqualTo(expected));
        }

        private static IEnumerable<object> IncrementTestCases()
        {
            var values = new int[] { 0, 1, -1, 100, int.MaxValue, int.MinValue };
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
