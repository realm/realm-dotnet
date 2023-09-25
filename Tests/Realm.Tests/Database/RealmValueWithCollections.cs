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
        public void List_WhenRetrieved_WorksWithAllTypes([Values(true, false)] bool isManaged)
        {
            var innerList1 = new List<RealmValue> { "inner1" };
            var innerList2 = new List<RealmValue> { "inner2", innerList1 };

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
                innerList2,
            };

            RealmValue rv = originalList;

            if (isManaged)
            {
                rv = PersistAndFind(rv).RealmValueProperty;
            }

            Assert.That(rv.Type, Is.EqualTo(RealmValueType.List));
            Assert.That(rv != RealmValue.Null);

            Assert.That(rv.AsList(), Is.EqualTo(originalList));
            Assert.That(rv == originalList);
            Assert.That(rv.Equals(originalList));
        }

        [Test]
        public void List_WhenSetBeforeBeingManaged_WorksAsIntended()
        {
            var originalList = new List<RealmValue> { 1, "string", true };

            RealmValue rv = originalList;

            var rvo = new RealmValueObject { RealmValueProperty = rv };

            _realm.Write(() =>
            {
                _realm.Add(rvo);
            });

            rv = rvo.RealmValueProperty;

            Assert.That(rv.Type, Is.EqualTo(RealmValueType.List));
            Assert.That(rv != RealmValue.Null);

            Assert.That(rv.AsList(), Is.EqualTo(originalList));
        }

        [Test]
        public void List_WithConstructorMethodOrOperator_WorksTheSame([Values(true, false)] bool isManaged)
        {
            var originalList = new List<RealmValue> { 1, "string", true };

            RealmValue rvOperator = originalList;
            RealmValue rvConstructor = RealmValue.List(originalList);

            if (isManaged)
            {
                rvOperator = PersistAndFind(rvOperator).RealmValueProperty;
                rvConstructor = PersistAndFind(rvConstructor).RealmValueProperty;
            }

            Assert.That(rvOperator.AsList(), Is.EqualTo(originalList));
            Assert.That(rvConstructor.AsList(), Is.EqualTo(originalList));
        }

        [Test]
        public void List_WhenManaged_IsNotSameReferenceAsOriginalList()
        {
            var originalList = new List<RealmValue> { 1, "string", true };

            RealmValue rv = originalList;
            rv = PersistAndFind(rv).RealmValueProperty;
            var retrievedList = rv.AsList();

            originalList.RemoveAt(1);
            Assert.That(ReferenceEquals(originalList, retrievedList), Is.False);
        }

        [Test]
        public void ListInsideMixed_WhenUnmanaged_IsSameReferenceAsOriginalList()
        {
            var originalList = new List<RealmValue> { 1, "string", true };

            RealmValue rv = originalList;
            var retrievedList = rv.AsList();

            originalList.RemoveAt(1);
            Assert.That(ReferenceEquals(originalList, retrievedList), Is.True);
        }

        [Test]
        public void List_AfterCreation_CanBeAssigned([Values(true, false)] bool isManaged)
        {
            var stringVal = "Mario";
            var rvo = new RealmValueObject { RealmValueProperty = stringVal };

            if (isManaged)
            {
                _realm.Write(() =>
                {
                    _realm.Add(rvo);
                });
            }

            Assert.That(rvo.RealmValueProperty == stringVal);

            var listVal = new List<RealmValue> { 1, "string", true };

            _realm.Write(() =>
            {
                rvo.RealmValueProperty = listVal;
            });

            Assert.That(rvo.RealmValueProperty.AsList(), Is.EqualTo(listVal));

            var newStringVal = "Luigi";

            _realm.Write(() =>
            {
                rvo.RealmValueProperty = newStringVal;
            });

            Assert.That(rvo.RealmValueProperty == newStringVal);
        }

        [Test]
        public void List_WhenManaged_CanBeModified()
        {
            var listVal = new List<RealmValue> { 1, "string", true };

            var rvo = _realm.Write(() =>
            {
                return _realm.Add(new RealmValueObject { RealmValueProperty = listVal });
            });

            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsList()[1] = "Mario";
                listVal[1] = "Mario"; // To keep both list updated
            });

            Assert.That(rvo.RealmValueProperty.AsList(), Is.EqualTo(listVal));

            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsList().RemoveAt(2);
                listVal.RemoveAt(2);
            });

            Assert.That(rvo.RealmValueProperty.AsList(), Is.EqualTo(listVal));

            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsList().Add("newVal");
                listVal.Add("newVal");
            });

            Assert.That(rvo.RealmValueProperty.AsList(), Is.EqualTo(listVal));
        }

        [Test]
        public void List_AddSetInsertList_WorksAsIntended()
        {
            var listVal = new List<RealmValue> { 1, "string", true };

            var rvo = _realm.Write(() =>
            {
                return _realm.Add(new RealmValueObject { RealmValueProperty = listVal });
            });

            var innerList1 = new List<RealmValue> { "inner", 23, false };

            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsList()[1] = innerList1;
                listVal[1] = innerList1;
            });

            Assert.That(rvo.RealmValueProperty.AsList(), Is.EqualTo(listVal));

            var innerList2 = new List<RealmValue> { "inner2", 23, false };

            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsList().Insert(1, innerList2);
                listVal.Insert(1, innerList2);
            });

            Assert.That(rvo.RealmValueProperty.AsList(), Is.EqualTo(listVal));

            var innerList3 = new List<RealmValue> { "inner3", 23, false };

            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsList().Add(innerList3);
                listVal.Add(innerList3);
            });

            Assert.That(rvo.RealmValueProperty.AsList(), Is.EqualTo(listVal));
        }

        [Test]
        public void List_WhenManaged_WorksWithDynamic()
        {
            var originalList = new List<RealmValue> { 1, "string", true };

            var rvo = _realm.Write(() =>
            {
                return _realm.Add(new RealmValueObject());
            });

            _realm.Write(() =>
            {
                rvo.DynamicApi.Set(nameof(RealmValueObject.RealmValueProperty), originalList);
            });

            var rvp = rvo.DynamicApi.Get<RealmValue>(nameof(RealmValueObject.RealmValueProperty));

            Assert.That(rvp.AsList(), Is.EqualTo(originalList));
        }

        [Test]
        public void List_WhenManaged_WorksWithNotifications()
        {
            var originalList = new List<RealmValue> { 1, "string", true };

            var rvo = _realm.Write(() =>
            {
                return _realm.Add(new RealmValueObject { RealmValueProperty = originalList });
            });

            var callbacks = new List<ChangeSet>();
            using var token = rvo.RealmValueProperty.AsList().SubscribeForNotifications((collection, changes) =>
            {
                if (changes != null)
                {
                    callbacks.Add(changes);
                }
            });

            _realm.Write(() =>
            {
                rvo.RealmValueProperty.AsList()[2] = "mario";
            });

            _realm.Refresh();

            Assert.That(callbacks.Count, Is.EqualTo(1));
        }
    }
}
