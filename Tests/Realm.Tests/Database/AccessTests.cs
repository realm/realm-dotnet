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
using MongoDB.Bson;
using NUnit.Framework;
using Realms.Exceptions;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class AccessTests : RealmInstanceTest
    {
        [TestCaseSource(nameof(SetAndGetValueCases))]
        public void SetAndGetValue(string propertyName, object propertyValue)
        {
            AllTypesObject ato = null;
            _realm.Write(() =>
            {
                ato = _realm.Add(new AllTypesObject { RequiredStringProperty = string.Empty });

                TestHelpers.SetPropertyValue(ato, propertyName, propertyValue);
            });

            Assert.That(TestHelpers.GetPropertyValue(ato, propertyName), Is.EqualTo(propertyValue));
        }

        public static object[] SetAndGetValueCases =
        {
            new object[] { "CharProperty", '0' },
            new object[] { "ByteProperty", (byte)100 },
            new object[] { "Int16Property", (short)100 },
            new object[] { "Int32Property", 100 },
            new object[] { "Int64Property", 100L },
            new object[] { "SingleProperty", 123.123f },
            new object[] { "DoubleProperty", 123.123 },
            new object[] { "BooleanProperty", true },
            new object[] { "ByteArrayProperty", new byte[] { 0xde, 0xad, 0xbe, 0xef } },
            new object[] { "ByteArrayProperty", Array.Empty<byte>() },
            new object[] { "StringProperty", "hello" },
            new object[] { "DateTimeOffsetProperty", new DateTimeOffset(1956, 6, 1, 0, 0, 0, TimeSpan.Zero) },
            new object[] { "DecimalProperty", 123.456M },
            new object[] { "Decimal128Property", new Decimal128(564.42343424323) },
        };

        [TestCaseSource(nameof(SetAndReplaceWithNullCases))]
        public void SetValueAndReplaceWithNull(string propertyName, object propertyValue)
        {
            AllTypesObject ato = null;
            _realm.Write(() =>
            {
                ato = _realm.Add(new AllTypesObject { RequiredStringProperty = string.Empty });

                TestHelpers.SetPropertyValue(ato, propertyName, propertyValue);
            });

            Assert.That(TestHelpers.GetPropertyValue(ato, propertyName), Is.EqualTo(propertyValue));

            _realm.Write(() =>
            {
                TestHelpers.SetPropertyValue(ato, propertyName, null);
            });

            Assert.That(TestHelpers.GetPropertyValue(ato, propertyName), Is.EqualTo(null));
        }

        public static object[] SetAndReplaceWithNullCases =
        {
            new object[] { "NullableCharProperty", '0' },
            new object[] { "NullableByteProperty", (byte)100 },
            new object[] { "NullableInt16Property", (short)100 },
            new object[] { "NullableInt32Property", 100 },
            new object[] { "NullableInt64Property", 100L },
            new object[] { "NullableSingleProperty", 123.123f },
            new object[] { "NullableDoubleProperty", 123.123 },
            new object[] { "NullableBooleanProperty", true },
            new object[] { "NullableDecimalProperty", 123.456M },
            new object[] { "NullableDecimal128Property", new Decimal128(123.456) },
            new object[] { "ByteArrayProperty", new byte[] { 0xde, 0xad, 0xbe, 0xef } },
            new object[] { "ByteArrayProperty", Array.Empty<byte>() },
            new object[] { "StringProperty", "hello" },
            new object[] { "StringProperty", string.Empty },
            new object[] { "NullableDateTimeOffsetProperty", new DateTimeOffset(1956, 6, 1, 0, 0, 0, TimeSpan.Zero) }
        };

        [Test]
        public void AccessingRemovedObjectShouldThrow()
        {
            // Arrange
            Person p1 = null;
            _realm.Write(() =>
            {
                p1 = _realm.Add(new Person());

                // Create another object to ensure there is a row in the db after deleting p1.
                _realm.Add(new Person());

                _realm.Remove(p1);
            });

            // Act and assert
            Assert.Throws<RealmInvalidObjectException>(() =>
            {
                var illegalAccess = p1.FirstName;
            });
        }

        [Test]
        public void AccessingObjectInClosedRealmShouldThrow()
        {
            // Arrange
            Person p1 = null;
            _realm.Write(() => p1 = _realm.Add(new Person()));
            _realm.Dispose();

            // Act and assert
            Assert.Throws<RealmClosedException>(() =>
            {
                var illegalAccess = p1.FirstName;
            });
        }

        [Test]
        public void RealmObjectProperties_WhenNotSet_ShouldHaveDefaultValues()
        {
            var obj = new AllTypesObject
            {
                RequiredStringProperty = string.Empty
            };

            _realm.Write(() => _realm.Add(obj));

            Assert.That(obj.ByteArrayProperty, Is.EqualTo(default(byte[])));
            Assert.That(obj.StringProperty, Is.EqualTo(default(string)));
            Assert.That(obj.BooleanProperty, Is.EqualTo(default(bool)));
            Assert.That(obj.ByteProperty, Is.EqualTo(default(byte)));
            Assert.That(obj.CharProperty, Is.EqualTo(default(char)));
            Assert.That(obj.DateTimeOffsetProperty, Is.EqualTo(default(DateTimeOffset)));
            Assert.That(obj.SingleProperty, Is.EqualTo(default(float)));
            Assert.That(obj.DoubleProperty, Is.EqualTo(default(double)));
            Assert.That(obj.Int16Property, Is.EqualTo(default(short)));
            Assert.That(obj.Int32Property, Is.EqualTo(default(int)));
            Assert.That(obj.Int64Property, Is.EqualTo(default(long)));
            Assert.That(obj.DecimalProperty, Is.EqualTo(default(decimal)));
            Assert.That(obj.Decimal128Property, Is.EqualTo(default(Decimal128)));
            Assert.That(obj.NullableBooleanProperty, Is.EqualTo(default(bool?)));
            Assert.That(obj.NullableByteProperty, Is.EqualTo(default(byte?)));
            Assert.That(obj.NullableCharProperty, Is.EqualTo(default(char?)));
            Assert.That(obj.NullableDateTimeOffsetProperty, Is.EqualTo(default(DateTimeOffset?)));
            Assert.That(obj.NullableSingleProperty, Is.EqualTo(default(float?)));
            Assert.That(obj.NullableDoubleProperty, Is.EqualTo(default(double?)));
            Assert.That(obj.NullableInt16Property, Is.EqualTo(default(short?)));
            Assert.That(obj.NullableInt32Property, Is.EqualTo(default(int?)));
            Assert.That(obj.NullableInt64Property, Is.EqualTo(default(long?)));
            Assert.That(obj.NullableDecimalProperty, Is.EqualTo(default(decimal?)));
            Assert.That(obj.NullableDecimal128Property, Is.EqualTo(default(Decimal128?)));
        }
    }
}
