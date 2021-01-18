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
         *  - Retrieve value with asInt, asString and  so on
         * - A realmValue can be set and retrieved with the same type from the db
         * - A realm value can change type and it can be retrieved
         * - Int, bool, char and others are memorised as 64bit, so we can see if we can cast it to narrower types
         * - Need to add RealmValue to the test classes with all Types
         * - When testing with Null, you need to check it can be converted with all the .AsNullable....
         * 
         * 
         * Suggestions from Nikola
         * That looks fine to me - I think you might also want to cover the explicit cast cases as well as some error cases - e.g. trying to cast the value to string
            Some other scenarios you may want to try is:
            - Once an object is managed, that we can do obj.RealmValue.AsInt32RealmInteger().Increment() and validate that this actually increments the underlying value.
            - Assigning a value to 5, then to “abc” works.
            - Taking a reference to the Realm value does not change after replacing it with something else. I.e. var val1 = obj.RealmValue; then obj.RealmValue = "something else" and then verify that val1 is different from obj.RealmValue and that val1 still has its original data.
            - Ensure that notifications work - i.e. setting a realm value to something else will raise PropertyChanged notifications.
            You can also consider structuring your tests similar to the way we’re doing List/Set tests - where we have a generic helper method that runs the actual tests and the tests are simply calling it - that will likely save you a ton of identical code.
         * 
         * 
         * Nicola è D'accordo sul fatto che non si può fare l'ultimo punto (generalizzazione)
         * Yeah, I see your point - a few of the tests could be rewritten as generic if we pass the to/from conversion functions, but the majority, which does AsInt, AsRealmInteger and so on will probably not work nicely. Perhaps you can at least unify the numeric tests
            private void RunNumericTest(RealmValue initialValue, long expectedValue)
            {
                Assert.That((long)initialValue, Is.EqualTo(expectedValue));
                Assert.That((int)initialValue, Is.EqualTo((int)expectedValue));
                // ...
            }
            [Test]
            public void RealmValue_WhenUnmanaged_IntTests()
            {
                RunUnmanagedTest(10, 10);
            }
            [Test]
            public void RealmValue_WhenUnmanaged_ShortTests()
            {
                RunUnmanagedTest((short)5, 5);
            }
         * 
         * 
         * Later:
         * - Queries
         * - List of realm values work as expected
         * 
         */

        [Test]
        public void RealmValue_IntA()
        {
            int value = 10;
            RealmValue rv = value;

            Assert.That(rv == value);
            Assert.That((int)rv, Is.EqualTo(value));
            Assert.That(rv.Type, Is.EqualTo(RealmValueType.Int));
            Assert.That(rv.AsInt32(), Is.EqualTo(value));
            Assert.That(rv.AsNullableInt32(), Is.EqualTo(value));
            Assert.That(rv.AsInt32RealmInteger(), Is.EqualTo(value));
            Assert.That(rv.AsNullableInt32RealmInteger(), Is.EqualTo(value));
            Assert.That(rv.AsInt16(), Is.EqualTo(value));
            // ... As for 32
            Assert.That(rv.AsInt64(), Is.EqualTo(value));
            // ... As for 32
            Assert.That(rv.AsChar(), Is.EqualTo(value));
        }

        public void RealmValue_Int<T>(T value)
        {
            RealmValue rv = value;

            Assert.That(rv == value);
            Assert.That((int)rv, Is.EqualTo(value));
            Assert.That(rv.Type, Is.EqualTo(RealmValueType.Int));
            Assert.That(rv.AsInt32(), Is.EqualTo(value));
            Assert.That(rv.AsNullableInt32(), Is.EqualTo(value));
            Assert.That(rv.AsInt32RealmInteger(), Is.EqualTo(value));
            Assert.That(rv.AsNullableInt32RealmInteger(), Is.EqualTo(value));
            Assert.That(rv.AsInt16(), Is.EqualTo(value));
            // ... As for 32
            Assert.That(rv.AsInt64(), Is.EqualTo(value));
            // ... As for 32
            Assert.That(rv.AsChar(), Is.EqualTo(value));
        }

        [Test]
        public void RealmValue_IntB()
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
