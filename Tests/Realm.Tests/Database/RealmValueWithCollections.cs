////////////////////////////////////////////////////////////////////////////
//
// Copyright 2023 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
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
using MongoDB.Bson;
using NUnit.Framework;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    internal class RealmValueWithCollections : RealmInstanceTest
    {
        private RealmValueObject PersistAndFind(RealmValue rv)
        {
            _realm.Write(() =>
            {
                _realm.Add(new RealmValueObject { RealmValueProperty = rv });
            });

            return _realm.All<RealmValueObject>().First();
        }

        [Test]
        public void TestA([Values(true, false)] bool isManaged)
        {
            var originalList = new List<RealmValue>
            {
                RealmValue.Null,
                1,
                true,
                "string",
                new byte[] { 0, 1, 2 },
                new DateTimeOffset(1234, 5, 6, 7, 8, 9, TimeSpan.Zero),
                1f,
                2d,
                3m,
                new ObjectId("5f63e882536de46d71877979"),
                Guid.Parse("3809d6d9-7618-4b3d-8044-2aa35fd02f31"),
                new InternalObject { IntProperty = 10, StringProperty = "brown" },
            };

            RealmValue rv = originalList;

            if (isManaged)
            {
                rv = PersistAndFind(rv).RealmValueProperty;
            }

            Assert.That(rv.Type, Is.EqualTo(RealmValueType.List));
            Assert.That(rv != RealmValue.Null);

            var retrievedList = rv.AsList();

            Assert.That(retrievedList, Is.EquivalentTo(originalList));
        }

        /* To test:
         *  - everything works both managed and unmanaged
         *  - explicit/implicit conversion work
         *  - works with objects
         *  - works with lists inside lists
         *  - Can change type
         *  - Can add/replace at index
         *  - Can delete elements
         *  
         *  
         *  DONE:
         *  - 
         *  
         *  - Doesn't cause issues with queries
         *  - Dynamic ?
         *  - sets can't contain other collections
         * 
         */

        [Test]
        public void Test1()
        {
            var rvo = new RealmValueObject();

            rvo.RealmValueProperty = new List<RealmValue> { 1, "two", 3 };

            _realm.Write(() =>
            {
                _realm.Add(rvo);
            });

            var savedValue = rvo.RealmValueProperty;
            var list = savedValue.AsList();

            Assert.That(list.Count(), Is.EqualTo(3));

            var firstVal = list[0].AsInt16();
            var secondVal = list[1].AsString();
            var thirdVal = list[2].AsInt16();

            Assert.That(firstVal, Is.EqualTo(1));
            Assert.That(secondVal, Is.EqualTo("two"));
            Assert.That(thirdVal, Is.EqualTo(3));
        }

        [Test]
        public void Test2()
        {
            var rvo = new RealmValueObject();

            rvo.RealmValueProperty = new List<RealmValue> { 1, "two", new List<RealmValue> { 0, 15 } };

            _realm.Write(() =>
            {
                _realm.Add(rvo);
            });

            var savedValue = rvo.RealmValueProperty;
            var list = savedValue.AsList();

            Assert.That(list.Count(), Is.EqualTo(3));

            var thirdVal = list[2].AsList();

            var firstEl = thirdVal[0].AsInt16();
            var secondEl = thirdVal[1].AsInt16();

            Assert.That(firstEl, Is.EqualTo(0));
            Assert.That(secondEl, Is.EqualTo(15));
        }
    }
}
