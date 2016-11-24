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

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Realms;

namespace IntegrationTests.Shared
{
    [TestFixture, Preserve(AllMembers = true)]
    public class AddOrUpdateTests
    {
        protected Realm _realm;

        [SetUp]
        public void SetUp()
        {
            Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);
            _realm = Realm.GetInstance();
        }

        [TearDown]
        public void TearDown()
        {
            _realm.Dispose();
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
                _realm.Add(standalone, update: true);
            });

            Assert.That(standalone.IsManaged, Is.True);

            var queried = _realm.Find<PrimaryKeyObject>(1);
            Assert.That(queried, Is.EqualTo(standalone));
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
                _realm.Add(first, update: true);
            });

            var second = new PrimaryKeyObject
            {
                Id = 1,
                StringValue = "second"
            };

            _realm.Write(() =>
            {
                _realm.Add(second, update: true);
            });

            var queried = _realm.Find<PrimaryKeyObject>(1);
            Assert.That(queried.StringValue, Is.EqualTo("second"));
            Assert.That(first.StringValue, Is.EqualTo("second"));
            Assert.That(second.StringValue, Is.EqualTo("second"));
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
                _realm.Add(noPK, update: true);
            });

            var noPK2 = new NonPrimaryKeyObject
            {
                StringValue = "123"
            };

            _realm.Write(() =>
            {
                _realm.Add(noPK2, update: true);
            });

            Assert.That(noPK2, Is.Not.EqualTo(noPK));
            Assert.That(_realm.All<NonPrimaryKeyObject>().Count(), Is.EqualTo(2));
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
                _realm.Add(first, update: true);
            });

            var queried = _realm.Find<PrimaryKeyWithPKRelation>(1);
            Assert.That(queried.OtherObject, Is.Not.Null);
            Assert.That(queried.StringValue, Is.EqualTo("parent"));
            Assert.That(queried.OtherObject.StringValue, Is.EqualTo("child"));
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
                _realm.Add(first, update: true);
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
                _realm.Add(second, update: true);
            });

            var queried = _realm.Find<PrimaryKeyWithPKRelation>(1);
            Assert.That(queried.OtherObject, Is.Not.Null);
            Assert.That(queried.StringValue, Is.EqualTo("parent"));
            Assert.That(queried.OtherObject.StringValue, Is.EqualTo("child2"));

            var child1 = _realm.Find<PrimaryKeyObject>(1);
            Assert.That(child1.StringValue, Is.EqualTo("child"));

            var child2 = _realm.Find<PrimaryKeyObject>(2);
            Assert.That(child2.StringValue, Is.EqualTo("child2"));
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
                _realm.Add(first, update: true);
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
                _realm.Add(second, update: true);
            });

            var queried = _realm.Find<PrimaryKeyWithPKRelation>(1);
            Assert.That(queried.OtherObject, Is.Not.Null);
            Assert.That(queried.StringValue, Is.EqualTo("parent2"));
            Assert.That(queried.OtherObject.StringValue, Is.EqualTo("child2"));

            var child1 = _realm.Find<PrimaryKeyObject>(1);
            Assert.That(child1.StringValue, Is.EqualTo("child2"));
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
                _realm.Add(first, update: true);
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
                _realm.Add(second, update: true);
            });

            Assert.That(second.OtherObject, Is.Not.EqualTo(first.OtherObject));
            Assert.That(_realm.All<NonPrimaryKeyObject>().Count(), Is.EqualTo(2));
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
                _realm.Add(first, update: true);
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
                _realm.Add(second, update: true);
            });

            Assert.That(second.OtherObject, Is.EqualTo(first.OtherObject));
            Assert.That(_realm.All<PrimaryKeyObject>().Count(), Is.EqualTo(1));
            Assert.That(_realm.Find<PrimaryKeyObject>(1).StringValue, Is.EqualTo("child2"));
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
                _realm.Add(first, update: true);
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
                _realm.Add(second, update: true);
            });

            Assert.That(first.ListValue, Is.EqualTo(second.ListValue));
            Assert.That(_realm.All<PrimaryKeyObject>().Count(), Is.EqualTo(1));
            Assert.That(_realm.Find<PrimaryKeyObject>(1).StringValue, Is.EqualTo("secondChild"));
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
                _realm.Add(first, update: true);
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
                _realm.Add(second, update: true);
            });

            Assert.That(first.ListValue, Is.Not.EqualTo(second.ListValue));
            Assert.That(_realm.All<NonPrimaryKeyObject>().Count(), Is.EqualTo(2));
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
                _realm.Add(first, update: true);
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
                _realm.Add(updatedFirst, update: true);
            });

            Assert.That(_realm.All<PrimaryKeyObject>().Count(), Is.EqualTo(2));
            Assert.That(_realm.Find<PrimaryKeyObject>(1).StringValue, Is.EqualTo("updated child"));
            Assert.That(_realm.Find<PrimaryKeyObject>(2).StringValue, Is.EqualTo("new child"));
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
                _realm.Add(first, update: true);
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
                _realm.Add(second, update: true);
            });

            Assert.That(_realm.All<NonPrimaryKeyWithPKRelation>().Count(), Is.EqualTo(2));
            Assert.That(_realm.All<PrimaryKeyObject>().Count(), Is.EqualTo(1));
            Assert.That(_realm.Find<PrimaryKeyObject>(1).StringValue, Is.EqualTo("updated child"));
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
                _realm.Add(first, update: true);
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
                _realm.Add(second, update: true);
            });

            Assert.That(_realm.All<NonPrimaryKeyWithNonPKRelation>().Count(), Is.EqualTo(2));
            Assert.That(_realm.All<NonPrimaryKeyObject>().Count(), Is.EqualTo(2));
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
                _realm.Add(first, update: true);
            });

            var updatedFirst = new PrimaryKeyWithPKRelation
            {
                Id = 1,
                StringValue = "no child"
            };

            _realm.Write(() =>
            {
                _realm.Add(updatedFirst, update: true);
            });

            Assert.That(first.OtherObject, Is.Null);
            Assert.That(updatedFirst.OtherObject, Is.Null);
            Assert.That(_realm.Find<PrimaryKeyWithPKRelation>(1).OtherObject, Is.Null);
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
                _realm.Add(parent, update: true);
            });

            var persistedParent = _realm.Find<Parent>(1);
            var persistedChild = _realm.Find<Child>(1);

            Assert.That(persistedParent.Name, Is.EqualTo("Peter"));
            Assert.That(persistedChild.Name, Is.EqualTo("Kate"));
            Assert.That(persistedParent.Child, Is.Not.Null);
            Assert.That(persistedChild.Parent, Is.Not.Null);
            Assert.That(persistedChild.Parent.Name, Is.EqualTo("Peter"));
            Assert.That(persistedParent.Child.Name, Is.EqualTo("Kate"));
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
                _realm.Add(parent, update: true);
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
                _realm.Add(updatedParent, update: true);
            });

            var persistedParent = _realm.Find<PrimaryKeyWithNonPKChildWithPKGrandChild>(1);
            Assert.That(persistedParent.StringValue, Is.EqualTo("updated parent"));
            Assert.That(persistedParent.NonPKChild, Is.Not.Null);
            Assert.That(persistedParent.NonPKChild.StringValue, Is.EqualTo("new child"));
            Assert.That(persistedParent.NonPKChild.OtherObject, Is.Not.Null);
            Assert.That(persistedParent.NonPKChild.OtherObject.StringValue, Is.EqualTo("updated grandchild"));

            Assert.That(_realm.All<PrimaryKeyWithNonPKChildWithPKGrandChild>().Count(), Is.EqualTo(1));
            Assert.That(_realm.All<NonPrimaryKeyWithPKRelation>().Count(), Is.EqualTo(2));
            Assert.That(_realm.All<PrimaryKeyObject>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void AddOrUpdate_WhenPKIsNull_ShouldUpdate()
        {
            var first = new NullablePrimaryKeyObject
            {
                StringValue = "first"
            };

            _realm.Write(() =>
            {
                _realm.Add(first, update: true);
            });

            var second = new NullablePrimaryKeyObject
            {
                StringValue = "second"
            };

            _realm.Write(() =>
            {
                _realm.Add(second, update: true);
            });

            Assert.That(first.StringValue, Is.EqualTo("second"));
            Assert.That(second.StringValue, Is.EqualTo("second"));
            Assert.That(_realm.All<NullablePrimaryKeyObject>().Count(), Is.EqualTo(1));
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

        private class NullablePrimaryKeyObject : RealmObject
        {
            [PrimaryKey]
            public long? Id { get; set; }

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