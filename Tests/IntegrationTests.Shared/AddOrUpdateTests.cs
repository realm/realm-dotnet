﻿////////////////////////////////////////////////////////////////////////////
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
using System.Collections.Generic;
using NUnit.Framework;
using Realms;

namespace IntegrationTests.Shared
{
    [TestFixture, Preserve(AllMembers = true)]
    public class AddOrUpdateTests
    {
        protected Realm _realm;

        [SetUp]
        public void Setup()
        {
            Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);
            _realm = Realm.GetInstance();
        }

        [TearDown]
        public void TearDown()
        {
            _realm.Close();
            Realm.DeleteRealm(_realm.Config);
        }

        [Test]
        public void AddOrUpdate_WhenDoesntExist_ShouldAdd()
        {
            var standalone = new PrimaryKeyObject
            {
                Id = 1,
                StringValue = "bla"
            };

            _realm.Write(() =>
            {
                _realm.Manage(standalone, update: true);
            });

            Assert.IsTrue(standalone.IsManaged);

            var queried = _realm.ObjectForPrimaryKey<PrimaryKeyObject>(1);
            Assert.AreEqual(standalone, queried);
        }

        [Test]
        public void AddOrUpdate_WhenExists_ShouldUpdate()
        {
            var first = new PrimaryKeyObject
            {
                Id = 1,
                StringValue = "first"
            };

            _realm.Write(() =>
            {
                _realm.Manage(first, update: true);
            });

            var second = new PrimaryKeyObject
            {
                Id = 1,
                StringValue = "second"
            };

            _realm.Write(() =>
            {
                _realm.Manage(second, update: true);
            });

            var queried = _realm.ObjectForPrimaryKey<PrimaryKeyObject>(1);
            Assert.AreEqual("second", queried.StringValue);
            Assert.AreEqual("second", first.StringValue);
            Assert.AreEqual("second", second.StringValue);
        }

        [Test]
        public void AddOrUpdate_WhenNoPrimaryKey_ShouldAdd()
        {
            var noPK = new NonPrimaryKeyObject
            {
                StringValue = "123"
            };

            _realm.Write(() =>
            {
                _realm.Manage(noPK, update: true);
            });

            var noPK2 = new NonPrimaryKeyObject
            {
                StringValue = "123"
            };

            _realm.Write(() =>
            {
                _realm.Manage(noPK2, update: true);
            });

            Assert.AreNotEqual(noPK, noPK2);
            Assert.AreEqual(2, _realm.All<NonPrimaryKeyObject>().Count());
        }

        [Test]
        public void AddOrUpdate_WhenParentAndChildDontExist_ShouldAddBoth()
        {
            var first = new PrimaryKeyWithPKRelation
            {
                Id = 1,
                StringValue = "parent",
                OtherObject = new PrimaryKeyObject
                {
                    Id = 1,
                    StringValue = "child"
                }
            };

            _realm.Write(() =>
            {
                _realm.Manage(first, update: true);
            });

            var queried = _realm.ObjectForPrimaryKey<PrimaryKeyWithPKRelation>(1);
            Assert.IsNotNull(queried.OtherObject);
            Assert.AreEqual(queried.StringValue, "parent");
            Assert.AreEqual(queried.OtherObject.StringValue, "child");
        }

        [Test]
        public void AddOrUpdate_WhenParentExistsChildDoesnt_ShouldAddChild()
        {
            var first = new PrimaryKeyWithPKRelation
            {
                Id = 1,
                StringValue = "parent",
                OtherObject = new PrimaryKeyObject
                {
                    Id = 1,
                    StringValue = "child"
                }
            };

            _realm.Write(() =>
            {
                _realm.Manage(first, update: true);
            });

            var second = new PrimaryKeyWithPKRelation
            {
                Id = 1,
                StringValue = "parent",
                OtherObject = new PrimaryKeyObject
                {
                    Id = 2,
                    StringValue = "child2"
                }
            };

            _realm.Write(() =>
            {
                _realm.Manage(second, update: true);
            });

            var queried = _realm.ObjectForPrimaryKey<PrimaryKeyWithPKRelation>(1);
            Assert.IsNotNull(queried.OtherObject);
            Assert.AreEqual("parent", queried.StringValue);
            Assert.AreEqual("child2", queried.OtherObject.StringValue);

            var child1 = _realm.ObjectForPrimaryKey<PrimaryKeyObject>(1);
            Assert.AreEqual("child", child1.StringValue);

            var child2 = _realm.ObjectForPrimaryKey<PrimaryKeyObject>(2);
            Assert.AreEqual("child2", child2.StringValue);
        }

        [Test]
        public void AddOrUpdate_WhenParentAndChildExist_ShouldUpdateBoth()
        {
            var first = new PrimaryKeyWithPKRelation
            {
                Id = 1,
                StringValue = "parent",
                OtherObject = new PrimaryKeyObject
                {
                    Id = 1,
                    StringValue = "child"
                }
            };

            _realm.Write(() =>
            {
                _realm.Manage(first, update: true);
            });

            var second = new PrimaryKeyWithPKRelation
            {
                Id = 1,
                StringValue = "parent2",
                OtherObject = new PrimaryKeyObject
                {
                    Id = 1,
                    StringValue = "child2"
                }
            };

            _realm.Write(() =>
            {
                _realm.Manage(second, update: true);
            });

            var queried = _realm.ObjectForPrimaryKey<PrimaryKeyWithPKRelation>(1);
            Assert.IsNotNull(queried.OtherObject);
            Assert.AreEqual("parent2", queried.StringValue);
            Assert.AreEqual("child2", queried.OtherObject.StringValue);

            var child1 = _realm.ObjectForPrimaryKey<PrimaryKeyObject>(1);
            Assert.AreEqual("child2", child1.StringValue);
        }

        [Test]
        public void AddOrUpdate_WhenChildHasNoPK_ShouldAddChild()
        {
            var first = new PrimaryKeyWithNonPKRelation
            {
                Id = 1,
                StringValue = "parent",
                OtherObject = new NonPrimaryKeyObject
                {
                    StringValue = "child"
                }
            };

            _realm.Write(() =>
            {
                _realm.Manage(first, update: true);
            });

            var second = new PrimaryKeyWithNonPKRelation
            {
                Id = 2,
                StringValue = "parent2",
                OtherObject = new NonPrimaryKeyObject
                {
                    StringValue = "child"
                }
            };

            _realm.Write(() =>
            {
                _realm.Manage(second, update: true);
            });

            Assert.AreNotEqual(first.OtherObject, second.OtherObject);
            Assert.AreEqual(2, _realm.All<NonPrimaryKeyObject>().Count());
        }

        [Test]
        public void AddOrUpdate_WhenChildHasPK_ShouldUpdateChild()
        {
            var first = new PrimaryKeyWithPKRelation
            {
                Id = 1,
                StringValue = "parent",
                OtherObject = new PrimaryKeyObject
                {
                    Id = 1,
                    StringValue = "child"
                }
            };

            _realm.Write(() =>
            {
                _realm.Manage(first, update: true);
            });

            var second = new PrimaryKeyWithPKRelation
            {
                Id = 2,
                StringValue = "parent2",
                OtherObject = new PrimaryKeyObject
                {
                    Id = 1,
                    StringValue = "child2"
                }
            };

            _realm.Write(() =>
            {
                _realm.Manage(second, update: true);
            });

            Assert.AreEqual(first.OtherObject, second.OtherObject);
            Assert.AreEqual(1, _realm.All<PrimaryKeyObject>().Count());
            Assert.AreEqual("child2", _realm.ObjectForPrimaryKey<PrimaryKeyObject>(1).StringValue);
        }

        [Test]
        public void AddOrUpdate_WhenListHasPK_ShouldUpdateListItems()
        {
            var first = new PrimaryKeyWithPKList
            {
                Id = 1,
                StringValue = "first"
            };

            first.ListValue.Add(new PrimaryKeyObject
            {
                Id = 1,
                StringValue = "child"
            });

            _realm.Write(() =>
            {
                _realm.Manage(first, update: true);
            });

            var second = new PrimaryKeyWithPKList
            {
                Id = 2,
                StringValue = "second"
            };

            second.ListValue.Add(new PrimaryKeyObject
            {
                Id = 1,
                StringValue = "secondChild"
            });

            _realm.Write(() =>
            {
                _realm.Manage(second, update: true);
            });

            Assert.That(first.ListValue, Is.EqualTo(second.ListValue));
            Assert.AreEqual(1, _realm.All<PrimaryKeyObject>().Count());
            Assert.AreEqual("secondChild", _realm.ObjectForPrimaryKey<PrimaryKeyObject>(1).StringValue);
        }

        [Test]
        public void AddOrUpdate_WhenListHasNoPK_ShouldAddListItems()
        {
            var first = new PrimaryKeyWithNoPKList
            {
                Id = 1,
                StringValue = "first"
            };

            first.ListValue.Add(new NonPrimaryKeyObject
            {
                StringValue = "child"
            });

            _realm.Write(() =>
            {
                _realm.Manage(first, update: true);
            });

            var second = new PrimaryKeyWithNoPKList
            {
                Id = 2,
                StringValue = "second"
            };

            second.ListValue.Add(new NonPrimaryKeyObject
            {
                StringValue = "secondChild"
            });

            _realm.Write(() =>
            {
                _realm.Manage(second, update: true);
            });

            Assert.That(first.ListValue, Is.Not.EqualTo(second.ListValue));
            Assert.AreEqual(2, _realm.All<NonPrimaryKeyObject>().Count());
        }

        [Test]
        public void AddOrUpdate_WhenListHasPK_ShouldAddNewAndUpdateOldItems()
        {
            var first = new PrimaryKeyWithPKList
            {
                Id = 1,
                StringValue = "first"
            };

            first.ListValue.Add(new PrimaryKeyObject
            {
                Id = 1,
                StringValue = "child"
            });

            _realm.Write(() =>
            {
                _realm.Manage(first, update: true);
            });

            var updatedFirst = new PrimaryKeyWithPKList
            {
                Id = 1,
                StringValue = "updated first"
            };

            updatedFirst.ListValue.Add(new PrimaryKeyObject
            {
                Id = 1,
                StringValue = "updated child"
            });

            updatedFirst.ListValue.Add(new PrimaryKeyObject
            {
                Id = 2,
                StringValue = "new child"
            });

            _realm.Write(() =>
            {
                _realm.Manage(updatedFirst, update: true);
            });

            Assert.AreEqual(2, _realm.All<PrimaryKeyObject>().Count());
            Assert.AreEqual("updated child", _realm.ObjectForPrimaryKey<PrimaryKeyObject>(1).StringValue);
            Assert.AreEqual("new child", _realm.ObjectForPrimaryKey<PrimaryKeyObject>(2).StringValue);
        }

        [Test]
        public void AddOrUpdate_WhenParentHasNoPKChildHasPK_ShouldAddParentUpdateChild()
        {
            var first = new NonPrimaryKeyWithPKRelation
            {
                StringValue = "first parent",
                OtherObject = new PrimaryKeyObject
                {
                    Id = 1,
                    StringValue = "child"
                }
            };

            _realm.Write(() =>
            {
                _realm.Manage(first, update: true);
            });

            var second = new NonPrimaryKeyWithPKRelation
            {
                StringValue = "second parent",
                OtherObject = new PrimaryKeyObject
                {
                    Id = 1,
                    StringValue = "updated child"
                }
            };

            _realm.Write(() =>
            {
                _realm.Manage(second, update: true);
            });

            Assert.AreEqual(2, _realm.All<NonPrimaryKeyWithPKRelation>().Count());
            Assert.AreEqual(1, _realm.All<PrimaryKeyObject>().Count());
            Assert.AreEqual("updated child", _realm.ObjectForPrimaryKey<PrimaryKeyObject>(1).StringValue);
        }

        [Test]
        public void AddOrUpdate_WhenParentHasNoPKChildHasNoPK_ShouldAddBoth()
        {
            var first = new NonPrimaryKeyWithNonPKRelation
            {
                StringValue = "first parent",
                OtherObject = new NonPrimaryKeyObject
                {
                    StringValue = "child"
                }
            };

            _realm.Write(() =>
            {
                _realm.Manage(first, update: true);
            });

            var second = new NonPrimaryKeyWithNonPKRelation
            {
                StringValue = "second parent",
                OtherObject = new NonPrimaryKeyObject
                {
                    StringValue = "second child"
                }
            };

            _realm.Write(() =>
            {
                _realm.Manage(second, update: true);
            });

            Assert.AreEqual(2, _realm.All<NonPrimaryKeyWithNonPKRelation>().Count());
            Assert.AreEqual(2, _realm.All<NonPrimaryKeyObject>().Count());
        }

        [Test]
        public void AddOrUpdate_WhenRelationIsNull_ShouldClearLink()
        {
            var first = new PrimaryKeyWithPKRelation
            {
                Id = 1,
                StringValue = "has child",
                OtherObject = new PrimaryKeyObject
                {
                    Id = 1,
                    StringValue = "child"
                }
            };

            _realm.Write(() =>
            {
                _realm.Manage(first, update: true);
            });

            var updatedFirst = new PrimaryKeyWithPKRelation
            {
                Id = 1,
                StringValue = "no child"
            };

            _realm.Write(() =>
            {
                _realm.Manage(updatedFirst, update: true);
            });

            Assert.IsNull(first.OtherObject);
            Assert.IsNull(updatedFirst.OtherObject);
            Assert.IsNull(_realm.ObjectForPrimaryKey<PrimaryKeyWithPKRelation>(1).OtherObject);
        }

        [Test, Ignore("Cyclic relations don't work yet.")]
        public void CyclicRelations_ShouldWork()
        {
            var parent = new Parent
            {
                Id = 1,
                Name = "Peter",
                Child = new Child
                {
                    Id = 1,
                    Name = "Kate",
                    Parent = new Parent
                    {
                        Id = 1
                    }
                }
            };

            _realm.Write(() =>
            {
                _realm.Manage(parent, update: true);
            });

            var persistedParent = _realm.ObjectForPrimaryKey<Parent>(1);
            var persistedChild = _realm.ObjectForPrimaryKey<Child>(1);

            Assert.AreEqual("Peter", persistedParent.Name);
            Assert.AreEqual("Kate", persistedChild.Name);
            Assert.IsNotNull(persistedParent.Child);
            Assert.IsNotNull(persistedChild.Parent);
            Assert.AreEqual("Peter", persistedChild.Parent.Name);
            Assert.AreEqual("Kate", persistedParent.Child.Name);
        }

        [Test]
        public void AddOrUpdate_WhenChildHasNoPKAndGrandchildHasPK_ShouldAddChildUpdateGrandchild()
        {
            var parent = new PrimaryKeyWithNonPKChildWithPKGrandChild
            {
                Id = 1,
                StringValue = "parent",
                NonPKChild = new NonPrimaryKeyWithPKRelation
                {
                    StringValue = "child",
                    OtherObject = new PrimaryKeyObject
                    {
                        Id = 1,
                        StringValue = "grandchild"
                    }
                }
            };

            _realm.Write(() =>
            {
                _realm.Manage(parent, update: true);
            });

            var updatedParent = new PrimaryKeyWithNonPKChildWithPKGrandChild
            {
                Id = 1,
                StringValue = "updated parent",
                NonPKChild = new NonPrimaryKeyWithPKRelation
                {
                    StringValue = "new child",
                    OtherObject = new PrimaryKeyObject
                    {
                        Id = 1,
                        StringValue = "updated grandchild"
                    }
                }
            };

            _realm.Write(() =>
            {
                _realm.Manage(updatedParent, update: true);
            });

            var persistedParent = _realm.ObjectForPrimaryKey<PrimaryKeyWithNonPKChildWithPKGrandChild>(1);
            Assert.AreEqual("updated parent", persistedParent.StringValue);
            Assert.IsNotNull(persistedParent.NonPKChild);
            Assert.AreEqual("new child", persistedParent.NonPKChild.StringValue);
            Assert.IsNotNull(persistedParent.NonPKChild.OtherObject);
            Assert.AreEqual("updated grandchild", persistedParent.NonPKChild.OtherObject.StringValue);

            Assert.AreEqual(1, _realm.All<PrimaryKeyWithNonPKChildWithPKGrandChild>().Count());
            Assert.AreEqual(2, _realm.All<NonPrimaryKeyWithPKRelation>().Count());
            Assert.AreEqual(1, _realm.All<PrimaryKeyObject>().Count());
        }

        private class Parent : RealmObject
        {
            [PrimaryKey]
            public long Id { get; set; }

            public string Name { get; set; }

            public Child Child { get; set; }
        }

        private class Child : RealmObject
        {
            [PrimaryKey]
            public long Id { get; set; }

            public string Name { get; set; }

            public Parent Parent { get; set; }
        }

        private class PrimaryKeyWithNonPKChildWithPKGrandChild : RealmObject
        {
            [PrimaryKey]
            public long Id { get; set; }

            public string StringValue { get; set; }

            public NonPrimaryKeyWithPKRelation NonPKChild { get; set; }
        }

        private class NonPrimaryKeyObject : RealmObject
        {
            public string StringValue { get; set; }
        }

        private class PrimaryKeyObject : RealmObject
        {
            [PrimaryKey]
            public long Id { get; set; }

            public string StringValue { get; set; }
        }

        private class PrimaryKeyWithPKRelation : RealmObject
        {
            [PrimaryKey]
            public long Id { get; set; }

            public string StringValue { get; set; }

            public PrimaryKeyObject OtherObject { get; set; }
        }

        private class PrimaryKeyWithNonPKRelation : RealmObject
        {
            [PrimaryKey]
            public long Id { get; set; }

            public string StringValue { get; set; }

            public NonPrimaryKeyObject OtherObject { get; set; }
        }

        private class PrimaryKeyWithPKList : RealmObject
        {
            [PrimaryKey]
            public long Id { get; set; }

            public string StringValue { get; set; }

            public IList<PrimaryKeyObject> ListValue { get; }
        }

        private class PrimaryKeyWithNoPKList : RealmObject
        {
            [PrimaryKey]
            public long Id { get; set; }

            public string StringValue { get; set; }

            public IList<NonPrimaryKeyObject> ListValue { get; }
        }

        private class NonPrimaryKeyWithPKRelation : RealmObject
        {
            public string StringValue { get; set; }

            public PrimaryKeyObject OtherObject { get; set; }
        }

        private class NonPrimaryKeyWithNonPKRelation : RealmObject
        {
            public string StringValue { get; set; }

            public NonPrimaryKeyObject OtherObject { get; set; }
        }
    }
}