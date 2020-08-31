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
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Realms.Exceptions;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class AddOrUpdateTests : RealmInstanceTest
    {
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

        // confirm when we do this in a single Write that child objects correctly owned
        [Test]
        public void AddOrUpdate_WhenListHasPK_ShouldUpdateListItemsSingleWrite()
        {
            PrimaryKeyWithPKList first = null;
            _realm.Write(() =>
            {
                for (var i = 0; i < 10; i++)
                {
                    first = new PrimaryKeyWithPKList
                    {
                        Id = 1,
                        StringValue = "first"
                    };

                    first.ListValue.Add(new PrimaryKeyObject
                    {
                        Id = 1,
                        StringValue = "child"
                    });

                    first.ListValue.Add(new PrimaryKeyObject
                    {
                        Id = 2,
                        StringValue = "secondChild"
                    });

                    _realm.Add(first, update: true);
                }
            });

            Assert.That(first.ListValue.Count, Is.EqualTo(2));  // did the Add keep adding dups?
            Assert.That(_realm.Find<PrimaryKeyWithPKList>(1).ListValue.Count, Is.EqualTo(2));
            Assert.That(_realm.Find<PrimaryKeyObject>(1).StringValue, Is.EqualTo("child"));
            Assert.That(_realm.Find<PrimaryKeyObject>(2).StringValue, Is.EqualTo("secondChild"));
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

        [Test]
        public void CyclicRelations_ShouldWork()
        {
            var parent = new Parent
            {
                Id = 1,
                Name = "Peter",
            };

            parent.Child = new Child
            {
                Id = 1,
                Name = "Kate",
                Parent = parent,
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
        public void AddOrUpdate_WhenPKIsIntAndNull_ShouldUpdate()
        {
            var first = new NullablePrimaryKeyObject
            {
                StringValue = "first"
            };

            _realm.Write(() => _realm.Add(first, update: true));

            var second = new NullablePrimaryKeyObject
            {
                StringValue = "second"
            };

            _realm.Write(() => _realm.Add(second, update: true));

            Assert.That(first.StringValue, Is.EqualTo("second"));
            Assert.That(second.StringValue, Is.EqualTo("second"));
            Assert.That(_realm.All<NullablePrimaryKeyObject>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void AddOrUpdate_WhenPKIsStringAndNull_ShouldUpdate()
        {
            var first = new PrimaryKeyStringObject
            {
                Value = "first"
            };

            _realm.Write(() => _realm.Add(first, update: true));

            var second = new PrimaryKeyStringObject
            {
                Value = "second"
            };

            _realm.Write(() => _realm.Add(second, update: true));

            Assert.That(first.Value, Is.EqualTo("second"));
            Assert.That(second.Value, Is.EqualTo("second"));
            Assert.That(_realm.All<PrimaryKeyStringObject>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void Add_ShouldReturnPassedInObject()
        {
            var first = new Person
            {
                FirstName = "Peter"
            };

            Person added = null;
            _realm.Write(() =>
            {
                added = _realm.Add(first, update: false);
            });

            Assert.That(added, Is.SameAs(first));
        }

        [Test]
        public void AddOrUpdate_ShouldReturnPassedInObject()
        {
            var first = new PrimaryKeyObject
            {
                Id = 1,
                StringValue = "1"
            };

            PrimaryKeyObject firstAdded = null;
            _realm.Write(() =>
            {
                firstAdded = _realm.Add(first, update: true);
            });

            Assert.That(firstAdded, Is.SameAs(first));

            var second = new PrimaryKeyObject
            {
                Id = 1,
                StringValue = "2"
            };

            PrimaryKeyObject secondAdded = null;
            _realm.Write(() =>
            {
                secondAdded = _realm.Add(second, update: true);
            });

            Assert.That(secondAdded, Is.SameAs(second));
            Assert.That(first.StringValue, Is.EqualTo("2"));
            Assert.That(firstAdded.StringValue, Is.EqualTo("2"));
            Assert.That(second.StringValue, Is.EqualTo("2"));
            Assert.That(secondAdded.StringValue, Is.EqualTo("2"));
        }

        [Test]
        public void Add_ShouldReturnManaged()
        {
            Person person = null;
            _realm.Write(() =>
            {
                person = _realm.Add(new Person
                {
                    FirstName = "Peter"
                });
            });

            Assert.That(person.IsManaged);
            Assert.That(person.FirstName == "Peter");
        }

        [Test]
        public void AddOrUpdate_ShouldReturnManaged()
        {
            PrimaryKeyObject item = null;
            _realm.Write(() =>
            {
                item = _realm.Add(new PrimaryKeyObject
                {
                    Id = 1,
                    StringValue = "1"
                }, update: true);
            });

            Assert.That(item.IsManaged);
            Assert.That(item.StringValue == "1");
        }

        [TestCase(typeof(PrimaryKeyCharObject))]
        [TestCase(typeof(PrimaryKeyByteObject))]
        [TestCase(typeof(PrimaryKeyInt16Object))]
        [TestCase(typeof(PrimaryKeyInt32Object))]
        [TestCase(typeof(PrimaryKeyInt64Object))]
        [TestCase(typeof(PrimaryKeyNullableCharObject))]
        [TestCase(typeof(PrimaryKeyNullableByteObject))]
        [TestCase(typeof(PrimaryKeyNullableInt16Object))]
        [TestCase(typeof(PrimaryKeyNullableInt32Object))]
        [TestCase(typeof(PrimaryKeyNullableInt64Object))]
        [TestCase(typeof(PrimaryKeyStringObject))]
        public void Add_WhenPKIsDefaultAndDuplicate_ShouldThrow(Type type)
        {
            Assert.That(() =>
            {
                _realm.Write(() =>
                {
                    _realm.Add((RealmObject)Activator.CreateInstance(type));
                    _realm.Add((RealmObject)Activator.CreateInstance(type));
                });
            }, Throws.TypeOf<RealmDuplicatePrimaryKeyValueException>());
        }

        [Test]
        public void Add_WhenPKIsNotDefaultButDuplicate_ShouldThrow()
        {
            Assert.That(() =>
            {
                _realm.Write(() =>
                {
                    _realm.Add(new PrimaryKeyStringObject { StringProperty = "1" });
                    _realm.Add(new PrimaryKeyStringObject { StringProperty = "1" });
                });
            }, Throws.TypeOf<RealmDuplicatePrimaryKeyValueException>());
        }

        [Test]
        public void Add_WhenRequiredPropertyIsNotSet_ShouldThrow()
        {
            Assert.That(() =>
            {
                _realm.Write(() =>
                {
                    _realm.Add(new RequiredStringObject());
                });
            }, Throws.TypeOf<RealmException>());
        }

        [Test]
        public void AddOrUpdate_WhenObjectHasNonEmptyList_ShouldNotThrow()
        {
            /* This test verifies that updating an object that has PK and non-empty list will not throw an exception.
             * The reason it *could* throw is a limitation on core in table.cpp: Table::check_lists_are_empty.
             * The comment states:
             *     FIXME: Due to a limitation in Sync, it is not legal to change the primary
             *     key of a row that contains lists (including linklists) after those lists
             *     have been populated. This limitation may be lifted in the future, but for
             *     now it is necessary to ensure that all lists are empty before setting a
             *     primary key (by way of set_int_unique() or set_string_unique() or set_null_unique()).
             *
             * So if we set the Primary Key unnecessarily in the .NET binding, we could trigger that case. */

            var first = new PrimaryKeyWithPKList
            {
                Id = 42,
                StringValue = "value1"
            };

            first.ListValue.Add(new PrimaryKeyObject
            {
                Id = 1
            });

            _realm.Write(() => _realm.Add(first));

            var second = new PrimaryKeyWithPKList
            {
                Id = 42,
                StringValue = "value2"
            };

            second.ListValue.Add(new PrimaryKeyObject
            {
                Id = 1
            });

            Assert.That(() =>
            {
                _realm.Write(() => _realm.Add(second, update: true));
            }, Throws.Nothing);
        }

        [Test]
        public void AddOrUpdate_WhenListIsNull_ShouldNotThrow()
        {
            var first = new PrimaryKeyWithPKList
            {
                Id = 42
            };

            Assert.That(() =>
            {
                _realm.Write(() => _realm.Add(first, update: true));
            }, Throws.Nothing);
        }

        [Test]
        public void AddOrUpdate_WhenNewListIsNull_ShouldNotThrow()
        {
            var first = new PrimaryKeyWithPKList
            {
                Id = 1
            };

            first.ListValue.Add(new PrimaryKeyObject
            {
                Id = 1
            });

            _realm.Write(() => _realm.Add(first));

            var second = new PrimaryKeyWithPKList
            {
                Id = 1
            };

            // second.listValue is null, because the getter is never invoked.
            _realm.Write(() => _realm.Add(second, update: true));

            Assert.That(first.ListValue, Is.EquivalentTo(second.ListValue));

            // Verify that the original list was cleared
            Assert.That(first.ListValue.Count, Is.EqualTo(0));

            // Verify that clearing the list hasn't deleted the item from the Realm.
            Assert.That(_realm.All<PrimaryKeyObject>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void AddOrUpdate_WhenListObjectsHavePK_ShouldOverwriteList()
        {
            // Original object - has 2 elements in the list
            var first = new PrimaryKeyWithPKList
            {
                Id = 1
            };

            first.ListValue.Add(new PrimaryKeyObject
            {
                Id = 1
            });

            first.ListValue.Add(new PrimaryKeyObject
            {
                Id = 2
            });

            _realm.Write(() => _realm.Add(first));

            // Object to update with - has 1 element in the list
            var second = new PrimaryKeyWithPKList
            {
                Id = 1
            };

            second.ListValue.Add(new PrimaryKeyObject
            {
                Id = 3
            });

            _realm.Write(() => _realm.Add(second, update: true));

            Assert.That(first.ListValue, Is.EquivalentTo(second.ListValue));

            // Verify that the original list was replaced with the new one.
            Assert.That(first.ListValue.Count, Is.EqualTo(1));

            // Verify that the list's sole element has the correct Id.
            Assert.That(first.ListValue[0].Id, Is.EqualTo(3));

            // Verify that overwriting the list hasn't deleted any elements from the Realm.
            Assert.That(_realm.All<PrimaryKeyObject>().Count(), Is.EqualTo(3));
        }

        [Test]
        public void AddOrUpdate_WhenListObjectsDontHavePK_ShouldOverwriteList()
        {
            var first = new PrimaryKeyWithNoPKList
            {
                Id = 1
            };

            first.ListValue.Add(new NonPrimaryKeyObject
            {
                StringValue = "1"
            });

            _realm.Write(() => _realm.Add(first));

            var second = new PrimaryKeyWithNoPKList
            {
                Id = 1
            };

            second.ListValue.Add(new NonPrimaryKeyObject
            {
                StringValue = "2"
            });

            _realm.Write(() => _realm.Add(second, update: true));

            Assert.That(first.ListValue, Is.EquivalentTo(second.ListValue));

            // Verify that the original list was replaced with the new one and not merged with it.
            Assert.That(first.ListValue.Count, Is.EqualTo(1));

            // Verify that the list's sole element has the correct String value.
            Assert.That(first.ListValue[0].StringValue, Is.EqualTo("2"));

            // Verify that overwriting the list hasn't deleted any elements from the Realm.
            Assert.That(_realm.All<NonPrimaryKeyObject>().Count(), Is.EqualTo(2));
        }

        [Test]
        public void AddOrUpdate_WhenObjectIsRemapped_ShouldWork()
        {
            var first = new RemappedTypeObject
            {
                Id = 1
            };

            var second = new RemappedTypeObject
            {
                Id = 2,
            };

            first.MappedLink = second;
            first.MappedList.Add(second);

            _realm.Write(() =>
            {
                _realm.Add(first, update: true);
            });

            Assert.That(first.IsManaged);
            Assert.That(second.IsManaged);
            Assert.That(first.MappedLink, Is.EqualTo(second));
            Assert.That(first.MappedList, Does.Contain(second));
            Assert.That(second.MappedBacklink, Does.Contain(first));

            _realm.Write(() =>
            {
                _realm.Add(new RemappedTypeObject
                {
                    Id = 1,
                    MappedLink = new RemappedTypeObject
                    {
                        Id = 2,
                        StringValue = "Updated"
                    }
                }, update: true);
            });

            Assert.That(first.MappedLink.StringValue, Is.EqualTo("Updated"));
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