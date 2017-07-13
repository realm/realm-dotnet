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

using System.Linq;
using NUnit.Framework;
using Realms;

namespace Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class RealmIntegerQueryTests : RealmInstanceTest
    {
        [Test]
        public void RealmInteger_OfByte_Tests()
        {
            AddCounterObjects(out var zeros, out var ones);

            byte dotnetZero = 0;
            RealmInteger<byte> realmZero = 0;
            byte dotnetOne = 1;
            RealmInteger<byte> realmOne = 1;

            var equality = _realm.All<CounterObject>().Where(o => o.ByteProperty == 0).ToArray();
            var dotnetEquality = _realm.All<CounterObject>().Where(o => o.ByteProperty == dotnetZero).ToArray();
            var realmEquality = _realm.All<CounterObject>().Where(o => o.ByteProperty == realmZero).ToArray();

            Assert.That(equality, Is.EquivalentTo(new[] { zeros }));
            Assert.That(dotnetEquality, Is.EquivalentTo(new[] { zeros }));
            Assert.That(realmEquality, Is.EquivalentTo(new[] { zeros }));

            var inequality = _realm.All<CounterObject>().Where(o => o.ByteProperty != 0).ToArray();
            var dotnetInequality = _realm.All<CounterObject>().Where(o => o.ByteProperty != dotnetZero).ToArray();
            var realmInequality = _realm.All<CounterObject>().Where(o => o.ByteProperty != realmZero).ToArray();

            Assert.That(inequality, Is.EquivalentTo(new[] { ones }));
            Assert.That(dotnetInequality, Is.EquivalentTo(new[] { ones }));
            Assert.That(realmInequality, Is.EquivalentTo(new[] { ones }));

            var lessThan = _realm.All<CounterObject>().Where(o => o.ByteProperty < 1).ToArray();
            var dotnetLessThan = _realm.All<CounterObject>().Where(o => o.ByteProperty < dotnetOne).ToArray();
            var realmLessThan = _realm.All<CounterObject>().Where(o => o.ByteProperty < realmOne).ToArray();

            Assert.That(lessThan, Is.EquivalentTo(new[] { zeros }));
            Assert.That(dotnetLessThan, Is.EquivalentTo(new[] { zeros }));
            Assert.That(realmLessThan, Is.EquivalentTo(new[] { zeros }));

            var lessThanEqual = _realm.All<CounterObject>().Where(o => o.ByteProperty <= 1).ToArray();
            var dotnetLessThanEqual = _realm.All<CounterObject>().Where(o => o.ByteProperty <= dotnetOne).ToArray();
            var realmLessThanEqual = _realm.All<CounterObject>().Where(o => o.ByteProperty <= realmOne).ToArray();

            Assert.That(lessThanEqual, Is.EquivalentTo(new[] { zeros, ones }));
            Assert.That(dotnetLessThanEqual, Is.EquivalentTo(new[] { zeros, ones }));
            Assert.That(realmLessThanEqual, Is.EquivalentTo(new[] { zeros, ones }));

            var greaterThan = _realm.All<CounterObject>().Where(o => o.ByteProperty > 0).ToArray();
            var dotnetGreaterThan = _realm.All<CounterObject>().Where(o => o.ByteProperty > dotnetZero).ToArray();
            var realmGreaterThan = _realm.All<CounterObject>().Where(o => o.ByteProperty > realmZero).ToArray();

            Assert.That(greaterThan, Is.EquivalentTo(new[] { ones }));
            Assert.That(dotnetGreaterThan, Is.EquivalentTo(new[] { ones }));
            Assert.That(realmGreaterThan, Is.EquivalentTo(new[] { ones }));

            var greaterThanEqual = _realm.All<CounterObject>().Where(o => o.ByteProperty >= 0).ToArray();
            var dotnetGreaterThanEqual = _realm.All<CounterObject>().Where(o => o.ByteProperty >= dotnetZero).ToArray();
            var realmGreaterThanEqual = _realm.All<CounterObject>().Where(o => o.ByteProperty >= realmZero).ToArray();

            Assert.That(greaterThanEqual, Is.EquivalentTo(new[] { zeros, ones }));
            Assert.That(dotnetGreaterThanEqual, Is.EquivalentTo(new[] { zeros, ones }));
            Assert.That(realmGreaterThanEqual, Is.EquivalentTo(new[] { zeros, ones }));
        }

        [Test]
        public void RealmInteger_OfInt16_Tests()
        {
            AddCounterObjects(out var zeros, out var ones);

            short dotnetZero = 0;
            RealmInteger<short> realmZero = 0;
            short dotnetOne = 1;
            RealmInteger<short> realmOne = 1;

            var equality = _realm.All<CounterObject>().Where(o => o.Int16Property == 0).ToArray();
            var dotnetEquality = _realm.All<CounterObject>().Where(o => o.Int16Property == dotnetZero).ToArray();
            var realmEquality = _realm.All<CounterObject>().Where(o => o.Int16Property == realmZero).ToArray();

            Assert.That(equality, Is.EquivalentTo(new[] { zeros }));
            Assert.That(dotnetEquality, Is.EquivalentTo(new[] { zeros }));
            Assert.That(realmEquality, Is.EquivalentTo(new[] { zeros }));

            var inequality = _realm.All<CounterObject>().Where(o => o.Int16Property != 0).ToArray();
            var dotnetInequality = _realm.All<CounterObject>().Where(o => o.Int16Property != dotnetZero).ToArray();
            var realmInequality = _realm.All<CounterObject>().Where(o => o.Int16Property != realmZero).ToArray();

            Assert.That(inequality, Is.EquivalentTo(new[] { ones }));
            Assert.That(dotnetInequality, Is.EquivalentTo(new[] { ones }));
            Assert.That(realmInequality, Is.EquivalentTo(new[] { ones }));

            var lessThan = _realm.All<CounterObject>().Where(o => o.Int16Property < 1).ToArray();
            var dotnetLessThan = _realm.All<CounterObject>().Where(o => o.Int16Property < dotnetOne).ToArray();
            var realmLessThan = _realm.All<CounterObject>().Where(o => o.Int16Property < realmOne).ToArray();

            Assert.That(lessThan, Is.EquivalentTo(new[] { zeros }));
            Assert.That(dotnetLessThan, Is.EquivalentTo(new[] { zeros }));
            Assert.That(realmLessThan, Is.EquivalentTo(new[] { zeros }));

            var lessThanEqual = _realm.All<CounterObject>().Where(o => o.Int16Property <= 1).ToArray();
            var dotnetLessThanEqual = _realm.All<CounterObject>().Where(o => o.Int16Property <= dotnetOne).ToArray();
            var realmLessThanEqual = _realm.All<CounterObject>().Where(o => o.Int16Property <= realmOne).ToArray();

            Assert.That(lessThanEqual, Is.EquivalentTo(new[] { zeros, ones }));
            Assert.That(dotnetLessThanEqual, Is.EquivalentTo(new[] { zeros, ones }));
            Assert.That(realmLessThanEqual, Is.EquivalentTo(new[] { zeros, ones }));

            var greaterThan = _realm.All<CounterObject>().Where(o => o.Int16Property > 0).ToArray();
            var dotnetGreaterThan = _realm.All<CounterObject>().Where(o => o.Int16Property > dotnetZero).ToArray();
            var realmGreaterThan = _realm.All<CounterObject>().Where(o => o.Int16Property > realmZero).ToArray();

            Assert.That(greaterThan, Is.EquivalentTo(new[] { ones }));
            Assert.That(dotnetGreaterThan, Is.EquivalentTo(new[] { ones }));
            Assert.That(realmGreaterThan, Is.EquivalentTo(new[] { ones }));

            var greaterThanEqual = _realm.All<CounterObject>().Where(o => o.Int16Property >= 0).ToArray();
            var dotnetGreaterThanEqual = _realm.All<CounterObject>().Where(o => o.Int16Property >= dotnetZero).ToArray();
            var realmGreaterThanEqual = _realm.All<CounterObject>().Where(o => o.Int16Property >= realmZero).ToArray();

            Assert.That(greaterThanEqual, Is.EquivalentTo(new[] { zeros, ones }));
            Assert.That(dotnetGreaterThanEqual, Is.EquivalentTo(new[] { zeros, ones }));
            Assert.That(realmGreaterThanEqual, Is.EquivalentTo(new[] { zeros, ones }));
        }

        [Test]
        public void RealmInteger_OfInt32_Tests()
        {
            AddCounterObjects(out var zeros, out var ones);

            var dotnetZero = 0;
            RealmInteger<int> realmZero = 0;
            var dotnetOne = 1;
            RealmInteger<int> realmOne = 1;

            var equality = _realm.All<CounterObject>().Where(o => o.Int32Property == 0).ToArray();
            var dotnetEquality = _realm.All<CounterObject>().Where(o => o.Int32Property == dotnetZero).ToArray();
            var realmEquality = _realm.All<CounterObject>().Where(o => o.Int32Property == realmZero).ToArray();

            Assert.That(equality, Is.EquivalentTo(new[] { zeros }));
            Assert.That(dotnetEquality, Is.EquivalentTo(new[] { zeros }));
            Assert.That(realmEquality, Is.EquivalentTo(new[] { zeros }));

            var inequality = _realm.All<CounterObject>().Where(o => o.Int32Property != 0).ToArray();
            var dotnetInequality = _realm.All<CounterObject>().Where(o => o.Int32Property != dotnetZero).ToArray();
            var realmInequality = _realm.All<CounterObject>().Where(o => o.Int32Property != realmZero).ToArray();

            Assert.That(inequality, Is.EquivalentTo(new[] { ones }));
            Assert.That(dotnetInequality, Is.EquivalentTo(new[] { ones }));
            Assert.That(realmInequality, Is.EquivalentTo(new[] { ones }));

            var lessThan = _realm.All<CounterObject>().Where(o => o.Int32Property < 1).ToArray();
            var dotnetLessThan = _realm.All<CounterObject>().Where(o => o.Int32Property < dotnetOne).ToArray();
            var realmLessThan = _realm.All<CounterObject>().Where(o => o.Int32Property < realmOne).ToArray();

            Assert.That(lessThan, Is.EquivalentTo(new[] { zeros }));
            Assert.That(dotnetLessThan, Is.EquivalentTo(new[] { zeros }));
            Assert.That(realmLessThan, Is.EquivalentTo(new[] { zeros }));

            var lessThanEqual = _realm.All<CounterObject>().Where(o => o.Int32Property <= 1).ToArray();
            var dotnetLessThanEqual = _realm.All<CounterObject>().Where(o => o.Int32Property <= dotnetOne).ToArray();
            var realmLessThanEqual = _realm.All<CounterObject>().Where(o => o.Int32Property <= realmOne).ToArray();

            Assert.That(lessThanEqual, Is.EquivalentTo(new[] { zeros, ones }));
            Assert.That(dotnetLessThanEqual, Is.EquivalentTo(new[] { zeros, ones }));
            Assert.That(realmLessThanEqual, Is.EquivalentTo(new[] { zeros, ones }));

            var greaterThan = _realm.All<CounterObject>().Where(o => o.Int32Property > 0).ToArray();
            var dotnetGreaterThan = _realm.All<CounterObject>().Where(o => o.Int32Property > dotnetZero).ToArray();
            var realmGreaterThan = _realm.All<CounterObject>().Where(o => o.Int32Property > realmZero).ToArray();

            Assert.That(greaterThan, Is.EquivalentTo(new[] { ones }));
            Assert.That(dotnetGreaterThan, Is.EquivalentTo(new[] { ones }));
            Assert.That(realmGreaterThan, Is.EquivalentTo(new[] { ones }));

            var greaterThanEqual = _realm.All<CounterObject>().Where(o => o.Int32Property >= 0).ToArray();
            var dotnetGreaterThanEqual = _realm.All<CounterObject>().Where(o => o.Int32Property >= dotnetZero).ToArray();
            var realmGreaterThanEqual = _realm.All<CounterObject>().Where(o => o.Int32Property >= realmZero).ToArray();

            Assert.That(greaterThanEqual, Is.EquivalentTo(new[] { zeros, ones }));
            Assert.That(dotnetGreaterThanEqual, Is.EquivalentTo(new[] { zeros, ones }));
            Assert.That(realmGreaterThanEqual, Is.EquivalentTo(new[] { zeros, ones }));
        }

        [Test]
        public void RealmInteger_OfInt64_Tests()
        {
            AddCounterObjects(out var zeros, out var ones);

            long dotnetZero = 0;
            RealmInteger<long> realmZero = 0;
            long dotnetOne = 1;
            RealmInteger<long> realmOne = 1;

            var equality = _realm.All<CounterObject>().Where(o => o.Int64Property == 0).ToArray();
            var dotnetEquality = _realm.All<CounterObject>().Where(o => o.Int64Property == dotnetZero).ToArray();
            var realmEquality = _realm.All<CounterObject>().Where(o => o.Int64Property == realmZero).ToArray();

            Assert.That(equality, Is.EquivalentTo(new[] { zeros }));
            Assert.That(dotnetEquality, Is.EquivalentTo(new[] { zeros }));
            Assert.That(realmEquality, Is.EquivalentTo(new[] { zeros }));

            var inequality = _realm.All<CounterObject>().Where(o => o.Int64Property != 0).ToArray();
            var dotnetInequality = _realm.All<CounterObject>().Where(o => o.Int64Property != dotnetZero).ToArray();
            var realmInequality = _realm.All<CounterObject>().Where(o => o.Int64Property != realmZero).ToArray();

            Assert.That(inequality, Is.EquivalentTo(new[] { ones }));
            Assert.That(dotnetInequality, Is.EquivalentTo(new[] { ones }));
            Assert.That(realmInequality, Is.EquivalentTo(new[] { ones }));

            var lessThan = _realm.All<CounterObject>().Where(o => o.Int64Property < 1).ToArray();
            var dotnetLessThan = _realm.All<CounterObject>().Where(o => o.Int64Property < dotnetOne).ToArray();
            var realmLessThan = _realm.All<CounterObject>().Where(o => o.Int64Property < realmOne).ToArray();

            Assert.That(lessThan, Is.EquivalentTo(new[] { zeros }));
            Assert.That(dotnetLessThan, Is.EquivalentTo(new[] { zeros }));
            Assert.That(realmLessThan, Is.EquivalentTo(new[] { zeros }));

            var lessThanEqual = _realm.All<CounterObject>().Where(o => o.Int64Property <= 1).ToArray();
            var dotnetLessThanEqual = _realm.All<CounterObject>().Where(o => o.Int64Property <= dotnetOne).ToArray();
            var realmLessThanEqual = _realm.All<CounterObject>().Where(o => o.Int64Property <= realmOne).ToArray();

            Assert.That(lessThanEqual, Is.EquivalentTo(new[] { zeros, ones }));
            Assert.That(dotnetLessThanEqual, Is.EquivalentTo(new[] { zeros, ones }));
            Assert.That(realmLessThanEqual, Is.EquivalentTo(new[] { zeros, ones }));

            var greaterThan = _realm.All<CounterObject>().Where(o => o.Int64Property > 0).ToArray();
            var dotnetGreaterThan = _realm.All<CounterObject>().Where(o => o.Int64Property > dotnetZero).ToArray();
            var realmGreaterThan = _realm.All<CounterObject>().Where(o => o.Int64Property > realmZero).ToArray();

            Assert.That(greaterThan, Is.EquivalentTo(new[] { ones }));
            Assert.That(dotnetGreaterThan, Is.EquivalentTo(new[] { ones }));
            Assert.That(realmGreaterThan, Is.EquivalentTo(new[] { ones }));

            var greaterThanEqual = _realm.All<CounterObject>().Where(o => o.Int64Property >= 0).ToArray();
            var dotnetGreaterThanEqual = _realm.All<CounterObject>().Where(o => o.Int64Property >= dotnetZero).ToArray();
            var realmGreaterThanEqual = _realm.All<CounterObject>().Where(o => o.Int64Property >= realmZero).ToArray();

            Assert.That(greaterThanEqual, Is.EquivalentTo(new[] { zeros, ones }));
            Assert.That(dotnetGreaterThanEqual, Is.EquivalentTo(new[] { zeros, ones }));
            Assert.That(realmGreaterThanEqual, Is.EquivalentTo(new[] { zeros, ones }));
        }

        [Test]
        public void RealmInteger_OfNullableByte_Tests()
        {
            AddCounterObjects(out var zeros, out var ones);

            byte? dotnetNull = null;
            RealmInteger<byte>? realmNull = null;
            byte? dotnetZero = 0;
            RealmInteger<byte>? realmZero = 0;
            byte? dotnetOne = 1;
            RealmInteger<byte>? realmOne = 1;
            byte? dotnetTwo = 2;
            RealmInteger<byte>? realmTwo = 2;

            var equality = _realm.All<CounterObject>().Where(o => o.NullableByteProperty == null).ToArray();
            var dotnetEquality = _realm.All<CounterObject>().Where(o => o.NullableByteProperty == dotnetNull).ToArray();
            var realmEquality = _realm.All<CounterObject>().Where(o => o.NullableByteProperty == realmNull).ToArray();

            Assert.That(equality, Is.EquivalentTo(new[] { zeros }));
            Assert.That(dotnetEquality, Is.EquivalentTo(new[] { zeros }));
            Assert.That(realmEquality, Is.EquivalentTo(new[] { zeros }));

            var inequality = _realm.All<CounterObject>().Where(o => o.NullableByteProperty != null).ToArray();
            var dotnetInequality = _realm.All<CounterObject>().Where(o => o.NullableByteProperty != dotnetNull).ToArray();
            var realmInequality = _realm.All<CounterObject>().Where(o => o.NullableByteProperty != realmNull).ToArray();

            Assert.That(inequality, Is.EquivalentTo(new[] { ones }));
            Assert.That(dotnetInequality, Is.EquivalentTo(new[] { ones }));
            Assert.That(realmInequality, Is.EquivalentTo(new[] { ones }));

            // In Realm Query language, null is not < 1
            var lessThan = _realm.All<CounterObject>().Where(o => o.NullableByteProperty < 2).ToArray();
            var dotnetLessThan = _realm.All<CounterObject>().Where(o => o.NullableByteProperty < dotnetTwo).ToArray();
            var realmLessThan = _realm.All<CounterObject>().Where(o => o.NullableByteProperty < realmTwo).ToArray();

            Assert.That(lessThan, Is.EquivalentTo(new[] { ones }));
            Assert.That(dotnetLessThan, Is.EquivalentTo(new[] { ones }));
            Assert.That(realmLessThan, Is.EquivalentTo(new[] { ones }));

            var lessThanEqual = _realm.All<CounterObject>().Where(o => o.NullableByteProperty <= 1).ToArray();
            var dotnetLessThanEqual = _realm.All<CounterObject>().Where(o => o.NullableByteProperty <= dotnetOne).ToArray();
            var realmLessThanEqual = _realm.All<CounterObject>().Where(o => o.NullableByteProperty <= realmOne).ToArray();

            Assert.That(lessThanEqual, Is.EquivalentTo(new[] { ones }));
            Assert.That(dotnetLessThanEqual, Is.EquivalentTo(new[] { ones }));
            Assert.That(realmLessThanEqual, Is.EquivalentTo(new[] { ones }));

            var greaterThan = _realm.All<CounterObject>().Where(o => o.NullableByteProperty > 0).ToArray();
            var dotnetGreaterThan = _realm.All<CounterObject>().Where(o => o.NullableByteProperty > dotnetZero).ToArray();
            var realmGreaterThan = _realm.All<CounterObject>().Where(o => o.NullableByteProperty > realmZero).ToArray();

            Assert.That(greaterThan, Is.EquivalentTo(new[] { ones }));
            Assert.That(dotnetGreaterThan, Is.EquivalentTo(new[] { ones }));
            Assert.That(realmGreaterThan, Is.EquivalentTo(new[] { ones }));

            var greaterThanEqual = _realm.All<CounterObject>().Where(o => o.NullableByteProperty >= 1).ToArray();
            var dotnetGreaterThanEqual = _realm.All<CounterObject>().Where(o => o.NullableByteProperty >= dotnetOne).ToArray();
            var realmGreaterThanEqual = _realm.All<CounterObject>().Where(o => o.NullableByteProperty >= realmOne).ToArray();

            Assert.That(greaterThanEqual, Is.EquivalentTo(new[] { ones }));
            Assert.That(dotnetGreaterThanEqual, Is.EquivalentTo(new[] { ones }));
            Assert.That(realmGreaterThanEqual, Is.EquivalentTo(new[] { ones }));
        }

        [Test]
        public void RealmInteger_OfNullableInt16_Tests()
        {
            AddCounterObjects(out var zeros, out var ones);

            short? dotnetNull = null;
            RealmInteger<short>? realmNull = null;
            short? dotnetZero = 0;
            RealmInteger<short>? realmZero = 0;
            short? dotnetOne = 1;
            RealmInteger<short>? realmOne = 1;
            short? dotnetTwo = 2;
            RealmInteger<short>? realmTwo = 2;

            var equality = _realm.All<CounterObject>().Where(o => o.NullableInt16Property == null).ToArray();
            var dotnetEquality = _realm.All<CounterObject>().Where(o => o.NullableInt16Property == dotnetNull).ToArray();
            var realmEquality = _realm.All<CounterObject>().Where(o => o.NullableInt16Property == realmNull).ToArray();

            Assert.That(equality, Is.EquivalentTo(new[] { zeros }));
            Assert.That(dotnetEquality, Is.EquivalentTo(new[] { zeros }));
            Assert.That(realmEquality, Is.EquivalentTo(new[] { zeros }));

            var inequality = _realm.All<CounterObject>().Where(o => o.NullableInt16Property != null).ToArray();
            var dotnetInequality = _realm.All<CounterObject>().Where(o => o.NullableInt16Property != dotnetNull).ToArray();
            var realmInequality = _realm.All<CounterObject>().Where(o => o.NullableInt16Property != realmNull).ToArray();

            Assert.That(inequality, Is.EquivalentTo(new[] { ones }));
            Assert.That(dotnetInequality, Is.EquivalentTo(new[] { ones }));
            Assert.That(realmInequality, Is.EquivalentTo(new[] { ones }));

            // In Realm Query language, null is not < 1
            var lessThan = _realm.All<CounterObject>().Where(o => o.NullableInt16Property < 2).ToArray();
            var dotnetLessThan = _realm.All<CounterObject>().Where(o => o.NullableInt16Property < dotnetTwo).ToArray();
            var realmLessThan = _realm.All<CounterObject>().Where(o => o.NullableInt16Property < realmTwo).ToArray();

            Assert.That(lessThan, Is.EquivalentTo(new[] { ones }));
            Assert.That(dotnetLessThan, Is.EquivalentTo(new[] { ones }));
            Assert.That(realmLessThan, Is.EquivalentTo(new[] { ones }));

            var lessThanEqual = _realm.All<CounterObject>().Where(o => o.NullableInt16Property <= 1).ToArray();
            var dotnetLessThanEqual = _realm.All<CounterObject>().Where(o => o.NullableInt16Property <= dotnetOne).ToArray();
            var realmLessThanEqual = _realm.All<CounterObject>().Where(o => o.NullableInt16Property <= realmOne).ToArray();

            Assert.That(lessThanEqual, Is.EquivalentTo(new[] { ones }));
            Assert.That(dotnetLessThanEqual, Is.EquivalentTo(new[] { ones }));
            Assert.That(realmLessThanEqual, Is.EquivalentTo(new[] { ones }));

            var greaterThan = _realm.All<CounterObject>().Where(o => o.NullableInt16Property > 0).ToArray();
            var dotnetGreaterThan = _realm.All<CounterObject>().Where(o => o.NullableInt16Property > dotnetZero).ToArray();
            var realmGreaterThan = _realm.All<CounterObject>().Where(o => o.NullableInt16Property > realmZero).ToArray();

            Assert.That(greaterThan, Is.EquivalentTo(new[] { ones }));
            Assert.That(dotnetGreaterThan, Is.EquivalentTo(new[] { ones }));
            Assert.That(realmGreaterThan, Is.EquivalentTo(new[] { ones }));

            var greaterThanEqual = _realm.All<CounterObject>().Where(o => o.NullableInt16Property >= 1).ToArray();
            var dotnetGreaterThanEqual = _realm.All<CounterObject>().Where(o => o.NullableInt16Property >= dotnetOne).ToArray();
            var realmGreaterThanEqual = _realm.All<CounterObject>().Where(o => o.NullableInt16Property >= realmOne).ToArray();

            Assert.That(greaterThanEqual, Is.EquivalentTo(new[] { ones }));
            Assert.That(dotnetGreaterThanEqual, Is.EquivalentTo(new[] { ones }));
            Assert.That(realmGreaterThanEqual, Is.EquivalentTo(new[] { ones }));
        }

        [Test]
        public void RealmInteger_OfNullableInt32_Tests()
        {
            AddCounterObjects(out var zeros, out var ones);

            int? dotnetNull = null;
            RealmInteger<int>? realmNull = null;
            int? dotnetZero = 0;
            RealmInteger<int>? realmZero = 0;
            int? dotnetOne = 1;
            RealmInteger<int>? realmOne = 1;
            int? dotnetTwo = 2;
            RealmInteger<int>? realmTwo = 2;

            var equality = _realm.All<CounterObject>().Where(o => o.NullableInt32Property == null).ToArray();
            var dotnetEquality = _realm.All<CounterObject>().Where(o => o.NullableInt32Property == dotnetNull).ToArray();
            var realmEquality = _realm.All<CounterObject>().Where(o => o.NullableInt32Property == realmNull).ToArray();

            Assert.That(equality, Is.EquivalentTo(new[] { zeros }));
            Assert.That(dotnetEquality, Is.EquivalentTo(new[] { zeros }));
            Assert.That(realmEquality, Is.EquivalentTo(new[] { zeros }));

            var inequality = _realm.All<CounterObject>().Where(o => o.NullableInt32Property != null).ToArray();
            var dotnetInequality = _realm.All<CounterObject>().Where(o => o.NullableInt32Property != dotnetNull).ToArray();
            var realmInequality = _realm.All<CounterObject>().Where(o => o.NullableInt32Property != realmNull).ToArray();

            Assert.That(inequality, Is.EquivalentTo(new[] { ones }));
            Assert.That(dotnetInequality, Is.EquivalentTo(new[] { ones }));
            Assert.That(realmInequality, Is.EquivalentTo(new[] { ones }));

            // In Realm Query language, null is not < 1
            var lessThan = _realm.All<CounterObject>().Where(o => o.NullableInt32Property < 2).ToArray();
            var dotnetLessThan = _realm.All<CounterObject>().Where(o => o.NullableInt32Property < dotnetTwo).ToArray();
            var realmLessThan = _realm.All<CounterObject>().Where(o => o.NullableInt32Property < realmTwo).ToArray();

            Assert.That(lessThan, Is.EquivalentTo(new[] { ones }));
            Assert.That(dotnetLessThan, Is.EquivalentTo(new[] { ones }));
            Assert.That(realmLessThan, Is.EquivalentTo(new[] { ones }));

            var lessThanEqual = _realm.All<CounterObject>().Where(o => o.NullableInt32Property <= 1).ToArray();
            var dotnetLessThanEqual = _realm.All<CounterObject>().Where(o => o.NullableInt32Property <= dotnetOne).ToArray();
            var realmLessThanEqual = _realm.All<CounterObject>().Where(o => o.NullableInt32Property <= realmOne).ToArray();

            Assert.That(lessThanEqual, Is.EquivalentTo(new[] { ones }));
            Assert.That(dotnetLessThanEqual, Is.EquivalentTo(new[] { ones }));
            Assert.That(realmLessThanEqual, Is.EquivalentTo(new[] { ones }));

            var greaterThan = _realm.All<CounterObject>().Where(o => o.NullableInt32Property > 0).ToArray();
            var dotnetGreaterThan = _realm.All<CounterObject>().Where(o => o.NullableInt32Property > dotnetZero).ToArray();
            var realmGreaterThan = _realm.All<CounterObject>().Where(o => o.NullableInt32Property > realmZero).ToArray();

            Assert.That(greaterThan, Is.EquivalentTo(new[] { ones }));
            Assert.That(dotnetGreaterThan, Is.EquivalentTo(new[] { ones }));
            Assert.That(realmGreaterThan, Is.EquivalentTo(new[] { ones }));

            var greaterThanEqual = _realm.All<CounterObject>().Where(o => o.NullableInt32Property >= 1).ToArray();
            var dotnetGreaterThanEqual = _realm.All<CounterObject>().Where(o => o.NullableInt32Property >= dotnetOne).ToArray();
            var realmGreaterThanEqual = _realm.All<CounterObject>().Where(o => o.NullableInt32Property >= realmOne).ToArray();

            Assert.That(greaterThanEqual, Is.EquivalentTo(new[] { ones }));
            Assert.That(dotnetGreaterThanEqual, Is.EquivalentTo(new[] { ones }));
            Assert.That(realmGreaterThanEqual, Is.EquivalentTo(new[] { ones }));
        }

        [Test]
        public void RealmInteger_OfNullableInt64_Tests()
        {
            AddCounterObjects(out var zeros, out var ones);

            long? dotnetNull = null;
            RealmInteger<long>? realmNull = null;
            long? dotnetZero = 0;
            RealmInteger<long>? realmZero = 0;
            long? dotnetOne = 1;
            RealmInteger<long>? realmOne = 1;
            long? dotnetTwo = 2;
            RealmInteger<long>? realmTwo = 2;

            var equality = _realm.All<CounterObject>().Where(o => o.NullableInt64Property == null).ToArray();
            var dotnetEquality = _realm.All<CounterObject>().Where(o => o.NullableInt64Property == dotnetNull).ToArray();
            var realmEquality = _realm.All<CounterObject>().Where(o => o.NullableInt64Property == realmNull).ToArray();

            Assert.That(equality, Is.EquivalentTo(new[] { zeros }));
            Assert.That(dotnetEquality, Is.EquivalentTo(new[] { zeros }));
            Assert.That(realmEquality, Is.EquivalentTo(new[] { zeros }));

            var inequality = _realm.All<CounterObject>().Where(o => o.NullableInt64Property != null).ToArray();
            var dotnetInequality = _realm.All<CounterObject>().Where(o => o.NullableInt64Property != dotnetNull).ToArray();
            var realmInequality = _realm.All<CounterObject>().Where(o => o.NullableInt64Property != realmNull).ToArray();

            Assert.That(inequality, Is.EquivalentTo(new[] { ones }));
            Assert.That(dotnetInequality, Is.EquivalentTo(new[] { ones }));
            Assert.That(realmInequality, Is.EquivalentTo(new[] { ones }));

            // In Realm Query language, null is not < 1
            var lessThan = _realm.All<CounterObject>().Where(o => o.NullableInt64Property < 2).ToArray();
            var dotnetLessThan = _realm.All<CounterObject>().Where(o => o.NullableInt64Property < dotnetTwo).ToArray();
            var realmLessThan = _realm.All<CounterObject>().Where(o => o.NullableInt64Property < realmTwo).ToArray();

            Assert.That(lessThan, Is.EquivalentTo(new[] { ones }));
            Assert.That(dotnetLessThan, Is.EquivalentTo(new[] { ones }));
            Assert.That(realmLessThan, Is.EquivalentTo(new[] { ones }));

            var lessThanEqual = _realm.All<CounterObject>().Where(o => o.NullableInt64Property <= 1).ToArray();
            var dotnetLessThanEqual = _realm.All<CounterObject>().Where(o => o.NullableInt64Property <= dotnetOne).ToArray();
            var realmLessThanEqual = _realm.All<CounterObject>().Where(o => o.NullableInt64Property <= realmOne).ToArray();

            Assert.That(lessThanEqual, Is.EquivalentTo(new[] { ones }));
            Assert.That(dotnetLessThanEqual, Is.EquivalentTo(new[] { ones }));
            Assert.That(realmLessThanEqual, Is.EquivalentTo(new[] { ones }));

            var greaterThan = _realm.All<CounterObject>().Where(o => o.NullableInt64Property > 0).ToArray();
            var dotnetGreaterThan = _realm.All<CounterObject>().Where(o => o.NullableInt64Property > dotnetZero).ToArray();
            var realmGreaterThan = _realm.All<CounterObject>().Where(o => o.NullableInt64Property > realmZero).ToArray();

            Assert.That(greaterThan, Is.EquivalentTo(new[] { ones }));
            Assert.That(dotnetGreaterThan, Is.EquivalentTo(new[] { ones }));
            Assert.That(realmGreaterThan, Is.EquivalentTo(new[] { ones }));

            var greaterThanEqual = _realm.All<CounterObject>().Where(o => o.NullableInt64Property >= 1).ToArray();
            var dotnetGreaterThanEqual = _realm.All<CounterObject>().Where(o => o.NullableInt64Property >= dotnetOne).ToArray();
            var realmGreaterThanEqual = _realm.All<CounterObject>().Where(o => o.NullableInt64Property >= realmOne).ToArray();

            Assert.That(greaterThanEqual, Is.EquivalentTo(new[] { ones }));
            Assert.That(dotnetGreaterThanEqual, Is.EquivalentTo(new[] { ones }));
            Assert.That(realmGreaterThanEqual, Is.EquivalentTo(new[] { ones }));
        }

        private void AddCounterObjects(out CounterObject zeros, out CounterObject ones)
        {
            var counter0 = new CounterObject
            {
                Id = 0
            };

            var counter1 = new CounterObject
            {
                Id = 1,
                ByteProperty = 1,
                Int16Property = 1,
                Int32Property = 1,
                Int64Property = 1,
                NullableByteProperty = 1,
                NullableInt16Property = 1,
                NullableInt32Property = 1,
                NullableInt64Property = 1
            };

            _realm.Write(() =>
            {
                _realm.Add(counter0);
                _realm.Add(counter1);
            });

            zeros = counter0;
            ones = counter1;
        }
    }
}
