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

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Realms;

namespace Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class RealmObjectTests : RealmInstanceTest
    {
        [Test]
        public void Realm_Add_InvokesOnManaged()
        {
            var obj = new OnManagedTestClass
            {
                Id = 1
            };

            Assert.That(obj.OnManagedCalled, Is.EqualTo(0));

            _realm.Write(() =>
            {
                _realm.Add(obj);
            });

            Assert.That(obj.OnManagedCalled, Is.EqualTo(1));

            var updatedObj = new OnManagedTestClass
            {
                Id = 1
            };

            Assert.That(updatedObj.OnManagedCalled, Is.EqualTo(0));

            _realm.Write(() =>
            {
                _realm.Add(updatedObj, update: true);
            });

            Assert.That(updatedObj.OnManagedCalled, Is.EqualTo(1));
        }

        [Test]
        public void RealmObject_SetRelatedObject_InvokesOnManaged()
        {
            var first = new OnManagedTestClass();

            _realm.Write(() =>
            {
                _realm.Add(first);
            });

            var second = new OnManagedTestClass();

            Assert.That(second.OnManagedCalled, Is.EqualTo(0));

            _realm.Write(() =>
            {
                first.RelatedObject = second;
            });

            Assert.That(second.OnManagedCalled, Is.EqualTo(1));
            Assert.That(first.RelatedObject.OnManagedCalled, Is.EqualTo(1));
        }

        [Test]
        public void RealmObject_AddToRelatedList_InvokesOnManaged()
        {
            var first = new OnManagedTestClass();

            _realm.Write(() =>
            {
                _realm.Add(first);
            });

            var second = new OnManagedTestClass();

            Assert.That(second.OnManagedCalled, Is.EqualTo(0));

            _realm.Write(() =>
            {
                first.RelatedCollection.Add(second);
            });

            Assert.That(second.OnManagedCalled, Is.EqualTo(1));
            Assert.That(first.RelatedCollection[0].OnManagedCalled, Is.EqualTo(1));
        }

        [Test]
        public void Realm_Find_InvokesOnManaged()
        {
            _realm.Write(() =>
            {
                _realm.Add(new OnManagedTestClass
                {
                    Id = 1
                });
            });

            var obj = _realm.Find<OnManagedTestClass>(1);
            Assert.That(obj.OnManagedCalled, Is.EqualTo(1));
        }

        [Test]
        public void RealmResults_InvokesOnManaged()
        {
            _realm.Write(() =>
            {
                _realm.Add(new OnManagedTestClass
                {
                    Id = 1
                });

                _realm.Add(new OnManagedTestClass
                {
                    Id = 2
                });
            });

            foreach (var obj in _realm.All<OnManagedTestClass>())
            {
                Assert.That(obj.OnManagedCalled, Is.EqualTo(1));
            }

            var elementAt = _realm.All<OnManagedTestClass>().ElementAt(0);

            Assert.That(elementAt.OnManagedCalled, Is.EqualTo(1));

            var first = _realm.All<OnManagedTestClass>().First(o => o.Id == 2);

            Assert.That(first.OnManagedCalled, Is.EqualTo(1));
        }

        private class OnManagedTestClass : RealmObject
        {
            [PrimaryKey]
            public int Id { get; set; }

            public OnManagedTestClass RelatedObject { get; set; }

            public IList<OnManagedTestClass> RelatedCollection { get; }

            [Ignored]
            public int OnManagedCalled { get; private set; }

            protected override void OnManaged()
            {
                OnManagedCalled++;
            }
        }
    }
}
