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
using MongoDB.Bson;
using NUnit.Framework;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class AARealmValueTests : RealmInstanceTest //TODO for testing
    {
        //TODO FP Most probably we can make the next function generic by using TestCases

        /* What to test:
         *  - Null value can be retrieved (RealmValue can contain null)
         * - A realmValue can be set and retrieved with the same type from the db
         * - A realm value can change type and it can be retrieved
         * - Need to add RealmValue to the test classes with all Types
         * 
         * Suggestions from Nikola
         * That looks fine to me - I think you might also want to cover the explicit cast cases as well as some error cases - e.g. trying to cast the value to string
            Some other scenarios you may want to try is:
            - Once an object is managed, that we can do obj.RealmValue.AsInt32RealmInteger().Increment() and validate that this actually increments the underlying value.
            - Assigning a value to 5, then to “abc” works.
            - Taking a reference to the Realm value does not change after replacing it with something else. I.e. var val1 = obj.RealmValue; then obj.RealmValue = "something else" and then verify that val1 is different from obj.RealmValue and that val1 still has its original data.
            - Ensure that notifications work - i.e. setting a realm value to something else will raise PropertyChanged notifications.
            - Test dynamic api,
            - List<RealmValue> should work, but not RealmValue = List<abc>
         * 
         * Later:
         * - Queries
         * - List of realm values work as expected
         * 
         *  rv == Realm.Null
         *  rv.IsNull() ?
         * 
         * 
         * This is what Nikola wanted, but it seems we can't really set the struct to null
         *  foo.rv = null // works
         *  foo.rv == null => return true
         *  foo.rv is null => return false -- this what means that it cannot be null (but contain Null)
         *  foo.rv = null same as foo.rv = RealmValue.Null()
         */


        [Test]
        public void RealmValue_WhenUnmanaged_CharTests()
        {
            RunNumericTests((char)10, 10);
        }

        [Test]
        public void RealmValue_WhenUnmanaged_ByteTests()
        {
            RunNumericTests((byte)10, 10);
        }

        [Test]
        public void RealmValue_WhenUnmanaged_IntTests()
        {
            RunNumericTests(10, 10);
        }

        [Test]
        public void RealmValue_WhenUnmanaged_ShortTests()
        {
            RunNumericTests((short)10, 10);
        }

        [Test]
        public void RealmValue_WhenUnmanaged_LongTests()
        {
            RunNumericTests(10L, 10);
        }

        public static void RunNumericTests(RealmValue rv, long value)
        {
            Assert.That(rv == value);
            Assert.That(rv.Type, Is.EqualTo(RealmValueType.Int));

            // 8 - byte
            Assert.That((byte)rv == value);
            Assert.That(rv.As<byte>() == value);
            Assert.That((byte?)rv == value);
            Assert.That(rv.As<byte?>() == value);
            Assert.That(rv.AsByte() == value);
            Assert.That(rv.AsNullableByte() == value);
            Assert.That(rv.AsByteRealmInteger() == value);
            Assert.That(rv.AsNullableByteRealmInteger() == value);

            // 16 - short
            Assert.That((short)rv == value);
            Assert.That(rv.As<short>() == value);
            Assert.That((short?)rv == value);
            Assert.That(rv.As<short?>() == value);
            Assert.That(rv.AsInt16() == value);
            Assert.That(rv.AsNullableInt16() == value);
            Assert.That(rv.AsInt16RealmInteger() == value);
            Assert.That(rv.AsNullableInt16RealmInteger() == value);

            // 32 - int
            Assert.That((int)rv == value);
            Assert.That(rv.As<int>() == value);
            Assert.That((int?)rv == value);
            Assert.That(rv.As<int?>() == value);
            Assert.That(rv.AsInt32() == value);
            Assert.That(rv.AsNullableInt32() == value);
            Assert.That(rv.AsInt32RealmInteger() == value);
            Assert.That(rv.AsNullableInt32RealmInteger() == value);

            // 64 - long
            Assert.That((long)rv == value);
            Assert.That(rv.As<long>() == value);
            Assert.That((long?)rv == value);
            Assert.That(rv.As<long?>() == value);
            Assert.That(rv.AsInt64() == value);
            Assert.That(rv.AsNullableInt64() == value);
            Assert.That(rv.AsInt64RealmInteger() == value);
            Assert.That(rv.AsNullableInt64RealmInteger() == value);
        }

        [Test]
        public void RealmValue_WhenUnmanaged_FloatTests()
        {
            float value = 10F;
            RealmValue rv = value;

            Assert.That(rv == value);
            Assert.That(rv.Type, Is.EqualTo(RealmValueType.Float));

            Assert.That((float)rv == value);
            Assert.That(rv.As<float>() == value);
            Assert.That((float?)rv == value);
            Assert.That(rv.As<float?>() == value);
            Assert.That(rv.AsFloat() == value);
            Assert.That(rv.AsNullableFloat() == value);
        }

        [Test]
        public void RealmValue_WhenUnmanaged_DoubleTests()
        {
            double value = 10;
            RealmValue rv = value;

            Assert.That(rv == value);
            Assert.That(rv.Type, Is.EqualTo(RealmValueType.Double));

            Assert.That((double)rv == value);
            Assert.That(rv.As<double>() == value);
            Assert.That((double?)rv == value);
            Assert.That(rv.As<double?>() == value);
            Assert.That(rv.AsDouble() == value);
            Assert.That(rv.AsNullableDouble() == value);
        }

        [Test]
        public void RealmValue_WhenUnmanaged_Decimal128Tests()
        {
            Decimal128 value = 10;
            RealmValue rv = value;

            Assert.That(rv == value);
            Assert.That(rv.Type, Is.EqualTo(RealmValueType.Decimal128));

            Assert.That((Decimal128)rv == value);
            Assert.That(rv.As<Decimal128>() == value);
            Assert.That((Decimal128?)rv == value);
            Assert.That(rv.As<Decimal128?>() == value);
            Assert.That(rv.AsDecimal128() == value);
            Assert.That(rv.AsNullableDecimal128() == value);
        }

        [Test]
        public void RealmValue_WhenUnmanaged_DecimalTests()
        {
            decimal value = 10;
            RealmValue rv = value;

            Assert.That(rv == value);
            Assert.That(rv.Type, Is.EqualTo(RealmValueType.Decimal128));

            Assert.That((decimal)rv == value);
            Assert.That(rv.As<decimal>() == value);
            Assert.That((decimal?)rv == value);
            Assert.That(rv.As<decimal?>() == value);
            Assert.That(rv.AsDecimal() == value);
            Assert.That(rv.AsNullableDecimal() == value);
        }

        [Test]
        public void RealmValue_WhenUnmanaged_DateTests()
        {
            DateTimeOffset value = DateTimeOffset.Now;
            RealmValue rv = value;

            Assert.That(rv == value);
            Assert.That(rv.Type, Is.EqualTo(RealmValueType.Date));

            Assert.That((DateTimeOffset)rv == value);
            Assert.That(rv.As<DateTimeOffset>() == value);
            Assert.That((DateTimeOffset?)rv == value);
            Assert.That(rv.As<DateTimeOffset?>() == value);
            Assert.That(rv.AsDate() == value);
            Assert.That(rv.AsNullableDate() == value);
        }

        [Test]
        public void RealmValue_WhenUnmanaged_ObjectIdTests()
        {
            ObjectId value = ObjectId.GenerateNewId();
            RealmValue rv = value;

            Assert.That(rv == value);
            Assert.That(rv.Type, Is.EqualTo(RealmValueType.ObjectId));

            Assert.That((ObjectId)rv == value);
            Assert.That(rv.As<ObjectId>() == value);
            Assert.That((ObjectId?)rv == value);
            Assert.That(rv.As<ObjectId?>() == value);
            Assert.That(rv.AsObjectId() == value);
            Assert.That(rv.AsNullableObjectId() == value);
        }

        [Test]
        public void RealmValue_WhenUnmanaged_GuidTests()
        {
            Guid value = Guid.NewGuid();
            RealmValue rv = value;

            Assert.That(rv == value);
            Assert.That(rv.Type, Is.EqualTo(RealmValueType.Guid));

            Assert.That((Guid)rv == value);
            Assert.That(rv.As<Guid>() == value);
            Assert.That((Guid?)rv == value);
            Assert.That(rv.As<Guid?>() == value);
            Assert.That(rv.AsGuid() == value);
            Assert.That(rv.AsNullableGuid() == value);
        }

        [Test]
        public void RealmValue_WhenUnmanaged_BoolTests()
        {
            bool value = true;
            RealmValue rv = value;

            Assert.That(rv == value);
            Assert.That(rv.Type, Is.EqualTo(RealmValueType.Bool));

            Assert.That((bool)rv == value);
            Assert.That(rv.As<bool>() == value);
            Assert.That((bool?)rv == value);
            Assert.That(rv.As<bool?>() == value);
            Assert.That(rv.AsBool() == value);
            Assert.That(rv.AsNullableBool() == value);
        }

        [Test]
        public void RealmValue_WhenUnmanaged_StringTests()
        {
            string value = "abc";
            RealmValue rv = value;

            Assert.That(rv == value);
            Assert.That(rv.Type, Is.EqualTo(RealmValueType.String));

            Assert.That((string)rv == value);
            Assert.That(rv.As<string>() == value);
            Assert.That(rv.AsString() == value);
        }

        [Test]
        public void RealmValue_WhenUnmanaged_DataTests()
        {
            byte[] value = new byte[] { 0, 1, 2 };
            RealmValue rv = value;

            Assert.That(rv == value);
            Assert.That(rv.Type, Is.EqualTo(RealmValueType.Data));

            Assert.That((byte[])rv == value);
            Assert.That(rv.As<byte[]>() == value);
            Assert.That(rv.AsData() == value);
        }

        [Test]
        public void RealmValue_WhenUnmanaged_ObjectTests()
        {
            Dog value = new Dog { Name = "Fido", Color = "Brown" };
            RealmValue rv = value;

            Assert.That(rv == value);
            Assert.That(rv.Type, Is.EqualTo(RealmValueType.Object));

            Assert.That((RealmObjectBase)rv == value);
            Assert.That(rv.As<RealmObjectBase>() == value);
            Assert.That(rv.AsRealmObject() == value);
        }

        [Test]
        public void RealmValue_AsNullable_ReturnsNull()
        {
            RealmValue rv = RealmValue.Null;

            Assert.That(rv.AsNullableBool() == null);
            Assert.That(rv.AsNullableChar() == null);
            Assert.That(rv.AsNullableDate() == null);
            Assert.That(rv.AsNullableDecimal() == null);
            Assert.That(rv.AsNullableDecimal128() == null);
            Assert.That(rv.AsNullableDouble() == null);
            Assert.That(rv.AsNullableFloat() == null);
            Assert.That(rv.AsNullableGuid() == null);
            Assert.That(rv.AsNullableObjectId() == null);
            Assert.That(rv.AsNullableByte() == null);
            Assert.That(rv.AsNullableByteRealmInteger() == null);
            Assert.That(rv.AsNullableInt16() == null);
            Assert.That(rv.AsNullableInt16RealmInteger() == null);
            Assert.That(rv.AsNullableInt32() == null);
            Assert.That(rv.AsNullableInt32RealmInteger() == null);
            Assert.That(rv.AsNullableInt64() == null);
            Assert.That(rv.AsNullableInt64RealmInteger() == null);

            Assert.That((bool?)rv == null);
            Assert.That((DateTimeOffset?)rv == null);
            Assert.That((decimal?)rv == null);
            Assert.That((Decimal128?)rv == null);
            Assert.That((double?)rv == null);
            Assert.That((float?)rv == null);
            Assert.That((Guid?)rv == null);
            Assert.That((ObjectId?)rv == null);
            Assert.That((byte?)rv == null);
            Assert.That((RealmInteger<byte>?)rv == null);
            Assert.That((short?)rv == null);
            Assert.That((RealmInteger<short>?)rv == null);
            Assert.That((int?)rv == null);
            Assert.That((RealmInteger<int>?)rv == null);
            Assert.That((long?)rv == null);
            Assert.That((RealmInteger<long>?)rv == null);
        }

        [Test]
        public void RealmValue_WhenManaged_IntTest()
        {
            int value = 10;
            var rvo = new RealmValueObject { Id = 1, RealmValueProperty = value };
            var retrievedObject = PersistAndFind(rvo);

            Assert.That(retrievedObject.RealmValueProperty == value);
        }

        [Test]
        public void RealmValue_WhenManaged_ShortTest()
        {
            short value = 10;
            var rvo = new RealmValueObject { Id = 1, RealmValueProperty = value };
            var retrievedObject = PersistAndFind(rvo);

            Assert.That(retrievedObject.RealmValueProperty == value);
        }

        [Test]
        public void RealmValue_WhenManaged_LongTest()
        {
            long value = 10;
            var rvo = new RealmValueObject { Id = 1, RealmValueProperty = value };
            var retrievedObject = PersistAndFind(rvo);

            Assert.That(retrievedObject.RealmValueProperty == value);
        }

        [Test]
        public void RealmValue_WhenManaged_CharTest()
        {
            char value = 'a';
            var rvo = new RealmValueObject { Id = 1, RealmValueProperty = value };
            var retrievedObject = PersistAndFind(rvo);

            Assert.That(retrievedObject.RealmValueProperty == value);
        }

        [Test]
        public void RealmValue_WhenManaged_ByteTest()
        {
            byte value = 10;
            var rvo = new RealmValueObject { Id = 1, RealmValueProperty = value };
            var retrievedObject = PersistAndFind(rvo);

            Assert.That(retrievedObject.RealmValueProperty == value);
        }

        [Test]
        public void RealmValue_WhenManaged_BoolTest()
        {
            bool value = true;
            var rvo = new RealmValueObject { Id = 1, RealmValueProperty = value };
            var retrievedObject = PersistAndFind(rvo);

            Assert.That(retrievedObject.RealmValueProperty == value);
        }

        [Test]
        public void RealmValue_WhenManaged_FloatTest()
        {
            float value = 10;
            var rvo = new RealmValueObject { Id = 1, RealmValueProperty = value };
            var retrievedObject = PersistAndFind(rvo);

            Assert.That(retrievedObject.RealmValueProperty == value);
        }

        [Test]
        public void RealmValue_WhenManaged_DoubleTest()
        {
            double value = 10;
            var rvo = new RealmValueObject { Id = 1, RealmValueProperty = value };
            var retrievedObject = PersistAndFind(rvo);

            Assert.That(retrievedObject.RealmValueProperty == value);
        }

        [Test]
        public void RealmValue_WhenManaged_Decimal128Test()
        {
            Decimal128 value = 10;
            var rvo = new RealmValueObject { Id = 1, RealmValueProperty = value };
            var retrievedObject = PersistAndFind(rvo);

            Assert.That(retrievedObject.RealmValueProperty == value);
        }

        [Test]
        public void RealmValue_WhenManaged_DecimalTest()
        {
            decimal value = 10;
            var rvo = new RealmValueObject { Id = 1, RealmValueProperty = value };
            var retrievedObject = PersistAndFind(rvo);

            Assert.That(retrievedObject.RealmValueProperty == value);
        }

        [Test]
        public void RealmValue_WhenManaged_GuidTest()
        {
            Guid value = Guid.NewGuid();
            var rvo = new RealmValueObject { Id = 1, RealmValueProperty = value };
            var retrievedObject = PersistAndFind(rvo);

            Assert.That(retrievedObject.RealmValueProperty == value);
        }

        [Test]
        public void RealmValue_WhenManaged_ObjectIdTest()
        {
            ObjectId value = ObjectId.GenerateNewId();
            var rvo = new RealmValueObject { Id = 1, RealmValueProperty = value };
            var retrievedObject = PersistAndFind(rvo);

            Assert.That(retrievedObject.RealmValueProperty == value);
        }

        [Test]
        public void RealmValue_WhenManaged_DateTest()
        {
            DateTimeOffset value = DateTimeOffset.Now;
            var rvo = new RealmValueObject { Id = 1, RealmValueProperty = value };
            var retrievedObject = PersistAndFind(rvo);

            Assert.That(retrievedObject.RealmValueProperty == value);
        }

        public class RO : RealmObject
        {
            [PrimaryKey]
            public int Id { get; set; }

            public RLink Link { get; set; }
        }

        public class RLink : RealmObject
        {
            public string Name { get; set; }
        }

        [Test]
        public void AAAREst()
        {
            var rlink = new RLink { Name = "lucio" };
            var ro = new RO { Id = 1, Link = rlink };

            _realm.Write(() =>
            {
                _realm.Add(ro);
            });

            var retrieved = _realm.Find<RO>(1);

            var again = retrieved.Link;

            Assert.That(again.Name == "lucio");

        }

        [Test]
        public void RealmValue_WhenManaged_StringTest()
        {
            string value = "abc";
            var rvo = new RealmValueObject { Id = 1, RealmValueProperty = value };
            var retrievedObject = PersistAndFind(rvo);

            Assert.That(retrievedObject.RealmValueProperty == value);
        }

        [Test]
        public void RealmValue_WhenManaged_DataTest()
        {
            byte[] value = new byte[] { 0, 1, 2 };
            var rvo = new RealmValueObject { Id = 1, RealmValueProperty = value };
            var retrievedObject = PersistAndFind(rvo);

            Assert.That(retrievedObject.RealmValueProperty.AsData(), Is.EqualTo(value));
        }

        [Test]
        public void RealmValue_WhenManaged_ObjectTest()
        {
            Dog value = new Dog { Name = "Fido", Color = "Brown" };

            var rvo = new RealmValueObject { Id = 1, RealmValueProperty = value };
            var retrievedObject = PersistAndFind(rvo);
            var retrievedRv = (Dog)retrievedObject.RealmValueProperty;

            Assert.That(retrievedRv.Name == value.Name);
            Assert.That(retrievedRv.Color == value.Color);
        }

        [Test]
        public void RealmValue_WithRealmInteger_Increments()
        {
            int value = 10;
            var rvo = new RealmValueObject { Id = 1, RealmValueProperty = value };
            var retrievedObject = PersistAndFind(rvo);

            Assert.That(retrievedObject.RealmValueProperty.AsInt32() == 10);

            _realm.Write(() =>
            {
                retrievedObject.RealmValueProperty.AsInt32RealmInteger().Increment();
            });

            Assert.That(retrievedObject.RealmValueProperty.AsInt32() == 11);

            _realm.Write(() =>
            {
                retrievedObject.RealmValueProperty.AsInt32RealmInteger().Decrement();
            });

            Assert.That(retrievedObject.RealmValueProperty.AsInt32() == 10);
        }

        private RealmValueObject PersistAndFind(RealmValueObject rvo)
        {
            _realm.Write(() =>
            {
                _realm.Add(rvo);
            });

            return _realm.Find<RealmValueObject>(1);
        }

        private class RealmValueObject : RealmObject
        {
            [PrimaryKey]
            public int Id { get; set; }

            public RealmValue RealmValueProperty { get; set; }
        }
    }
}
