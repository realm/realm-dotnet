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
         *  - RealmValue cannot be null
         * - A realmValue can be set and retrieved with the same type from the db
         * - A realm value can change type and it can be retrieved
         * - Need to add RealmValue to the test classes with all Types
         * - When testing with Null, you need to check it can be converted with all the .AsNullable....
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

        [Test]
        public void RealmValue_WhenUnmanaged_FloatTests()
        {
            float value = 10F;
            RealmValue rv = value;

            Assert.That(rv == value);
            Assert.That(rv.Type, Is.EqualTo(RealmValueType.Float));

            Assert.That((float)rv == value);
            Assert.That(rv.As<float>() == value);
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
        public void RealmValue_Nullable() //TODO to finish
        {
            RealmValue rv = RealmValue.Null;

            Assert.That(rv.AsNullableBool() == null);
            //All other types
        }

        public static void RunNumericTests(RealmValue rv, long value)
        {
            Assert.That(rv == value);
            Assert.That(rv.Type, Is.EqualTo(RealmValueType.Int));

            // 8 - byte
            Assert.That((byte)rv == value);
            Assert.That(rv.As<byte>() == value);
            Assert.That(rv.AsByte() == value); // TODO For now we use this style to avoid problems with Equals
            Assert.That(rv.AsNullableByte() == value);
            Assert.That(rv.AsByteRealmInteger() == value);
            Assert.That(rv.AsNullableByteRealmInteger() == value);

            // 16 - short
            Assert.That((short)rv == value);
            Assert.That(rv.As<short>() == value);
            Assert.That(rv.AsInt16() == value);
            Assert.That(rv.AsNullableInt16() == value);
            Assert.That(rv.AsInt16RealmInteger() == value);
            Assert.That(rv.AsNullableInt16RealmInteger() == value);

            // 32 - int
            Assert.That((int)rv == value);
            Assert.That(rv.As<int>() == value);
            Assert.That(rv.AsInt32() == value);
            Assert.That(rv.AsNullableInt32() == value);
            Assert.That(rv.AsInt32RealmInteger() == value);
            Assert.That(rv.AsNullableInt32RealmInteger() == value);

            // 64 - long
            Assert.That((long)rv == value);
            Assert.That(rv.As<long>() == value);
            Assert.That(rv.AsInt64() == value);
            Assert.That(rv.AsNullableInt64() == value);
            Assert.That(rv.AsInt64RealmInteger() == value);
            Assert.That(rv.AsNullableInt64RealmInteger() == value);
        }

        [Test]
        public void RealmValue_WhenManaged()
        {
            int value = 10;

            _realm.Write(() =>
            {
                _realm.Add(new RealmValueObject
                {
                    Id = 1,
                    RealmValue = value
                });
            });

            var ob = _realm.Find<RealmValueObject>(1);

            Assert.That(ob.RealmValue == value);
        }

        private class RealmValueObject : RealmObject
        {
            [PrimaryKey]
            public int Id { get; set; }

            public RealmValue RealmValue { get; set; }
        }
    }
}
