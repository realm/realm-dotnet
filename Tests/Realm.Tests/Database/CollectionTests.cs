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
using System.Reflection;
using MongoDB.Bson;
using NUnit.Framework;
using Realms.Exceptions;
#if TEST_WEAVER
using TestAsymmetricObject = Realms.AsymmetricObject;
using TestEmbeddedObject = Realms.EmbeddedObject;
using TestRealmObject = Realms.RealmObject;
#else
using TestAsymmetricObject = Realms.IAsymmetricObject;
using TestEmbeddedObject = Realms.IEmbeddedObject;
using TestRealmObject = Realms.IRealmObject;
#endif

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class CollectionTests : RealmInstanceTest
    {
        [Test]
        public void Insert_WhenIndexIsNegative_ShouldThrow()
        {
            var container = new ContainerObject();
            _realm.Write(() => _realm.Add(container));

            Assert.That(() =>
            {
                _realm.Write(() => container.Items.Insert(-1, new IntPropertyObject()));
            }, Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Insert_WhenIndexIsMoreThanCount_ShouldThrow()
        {
            var container = new ContainerObject();
            _realm.Write(() => _realm.Add(container));

            Assert.That(() =>
            {
                _realm.Write(() => container.Items.Insert(1, new IntPropertyObject()));
            }, Throws.TypeOf<ArgumentOutOfRangeException>());

            _realm.Write(() => container.Items.Add(new IntPropertyObject()));
            Assert.That(() =>
            {
                _realm.Write(() => container.Items.Insert(2, new IntPropertyObject()));
            }, Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Insert_WhenIndexIsEqualToCount_ShouldWork()
        {
            var container = new ContainerObject();
            _realm.Write(() => _realm.Add(container));

            var toInsert1 = new IntPropertyObject();
            _realm.Write(() => container.Items.Insert(0, toInsert1));
            Assert.That(container.Items.Count, Is.EqualTo(1));
            Assert.That(container.Items[0], Is.EqualTo(toInsert1));

            var toInsert2 = new IntPropertyObject();
            _realm.Write(() => container.Items.Insert(1, toInsert2));
            Assert.That(container.Items.Count, Is.EqualTo(2));
            Assert.That(container.Items[1], Is.EqualTo(toInsert2));
        }

        [TestCase(0, 3, "12304")]
        [TestCase(0, 4, "12340")]
        [TestCase(4, 0, "40123")]
        [TestCase(2, 4, "01342")]
        [TestCase(3, 1, "03124")]
        [TestCase(4, 4, "01234")]
        [TestCase(1, 1, "01234")]
        public void Move_WhenUnmanaged_ShouldMoveTheItem(int from, int to, string expected)
        {
            var list = new List<IntPropertyObject>();

            for (var i = 0; i < 5; i++)
            {
                list.Add(new IntPropertyObject { Int = i });
            }

            var item = list[from];
            list.Move(item, to);
            Assert.That(string.Join(string.Empty, list.Select(i => i.Int)), Is.EqualTo(expected));
        }

        [TestCase(0, -1)]
        [TestCase(4, 5)]
        [TestCase(1, 5)]
        [TestCase(3, -3)]
        public void Move_WhenUnmanagedAndDestinationIndexIsInvalid_ShouldThrow(int from, int to)
        {
            var list = new List<IntPropertyObject>();

            for (var i = 0; i < 5; i++)
            {
                list.Add(new IntPropertyObject { Int = i });
            }

            var item = list[from];
            Assert.That(() => list.Move(item, to), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [TestCase(0, 3, "12304")]
        [TestCase(0, 4, "12340")]
        [TestCase(4, 0, "40123")]
        [TestCase(2, 4, "01342")]
        [TestCase(3, 1, "03124")]
        [TestCase(4, 4, "01234")]
        [TestCase(1, 1, "01234")]
        public void Move_WhenManaged_ShouldMoveTheItem(int from, int to, string expected)
        {
            var container = GetPopulatedManagedContainerObject();

            _realm.Write(() =>
            {
                var item = container.Items[from];
                container.Items.Move(item, to);
            });

            Assert.That(string.Join(string.Empty, container.Items.Select(i => i.Int)), Is.EqualTo(expected));
        }

        [TestCase(0, -1)]
        [TestCase(4, 5)]
        [TestCase(1, 5)]
        [TestCase(3, -3)]
        public void Move_WhenManagedAndDestinationIndexIsInvalid_ShouldThrow(int from, int to)
        {
            var container = GetPopulatedManagedContainerObject();

            _realm.Write(() =>
            {
                var item = container.Items[from];
                Assert.That(() => container.Items.Move(item, to), Throws.TypeOf<ArgumentOutOfRangeException>());
            });
        }

        [Test]
        public void IList_IsReadOnly_WhenRealmIsReadOnly_ShouldBeTrue()
        {
            var writeableContainer = GetPopulatedManagedContainerObject();
            Assert.That(writeableContainer.Items.IsReadOnly, Is.False);

            _realm.Dispose();

            var config = _configuration.ConfigWithPath(_configuration.DatabasePath);
            config.IsReadOnly = true;

            using var readonlyRealm = GetRealm(config);
            var readonlyContainer = readonlyRealm.All<ContainerObject>().Single();
            Assert.That(readonlyContainer.Items.IsReadOnly);
        }

        [Test]
        public void Results_GetAtIndex_WhenIndexIsOutOfRange_ShouldThrow()
        {
            Assert.That(() => _realm.All<IntPropertyObject>().AsRealmCollection()[-1], Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => _realm.All<IntPropertyObject>().AsRealmCollection()[0], Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void List_GetAtIndex_WhenIndexIsOutOfRange_ShouldThrow()
        {
            var owner = new Owner();
            _realm.Write(() => _realm.Add(owner));

            Assert.That(() => owner.ListOfDogs.AsRealmCollection()[-1], Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => owner.ListOfDogs.AsRealmCollection()[0], Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void List_WhenRealmIsClosed_ShouldBeInvalid()
        {
            var container = GetPopulatedManagedContainerObject();

            Assert.That(container.Items.AsRealmCollection().IsValid);

            _realm.Dispose();

            Assert.That(container.Items.AsRealmCollection().IsValid, Is.False);
        }

        [Test]
        public void List_WhenParentIsDeleted_ShouldBeInvalid()
        {
            var container = GetPopulatedManagedContainerObject();

            Assert.That(container.Items.AsRealmCollection().IsValid);

            _realm.Write(() => _realm.Remove(container));

            Assert.That(container.Items.AsRealmCollection().IsValid, Is.False);
        }

        [Test]
        public void Results_WhenRealmIsClosed_ShouldBeInvalid()
        {
            var items = _realm.All<IntPropertyObject>();

            Assert.That(items.AsRealmCollection().IsValid);

            _realm.Dispose();

            Assert.That(items.AsRealmCollection().IsValid, Is.False);
        }

        [Test]
        public void Results_WhenEnumerating_ShouldBeStable()
        {
            TestStableIteration(
                i => _realm.Add(new IntPropertyObject { Int = i }),
                _realm.All<IntPropertyObject>,
                item => item.Int += 2);

            var items = _realm.All<IntPropertyObject>().ToArray().Select(i => i.Int);
            Assert.That(items, Is.EquivalentTo(Enumerable.Range(2, 10)));
        }

        [Test]
        public void ObjectList_WhenEnumeratingAndRemovingFromList_ShouldBeStable()
        {
            var container = new ContainerObject();
            _realm.Write(() => _realm.Add(container));

            TestStableIteration(
                i => container.Items.Add(new IntPropertyObject { Int = i }),
                () => container.Items,
                item => container.Items.Remove(item));

            Assert.That(container.Items, Is.Empty);

            var items = _realm.All<IntPropertyObject>().ToArray().Select(i => i.Int);
            Assert.That(items, Is.EquivalentTo(Enumerable.Range(0, 10)));
        }

        [Test]
        public void ObjectList_WhenEnumeratingFrozenList_ShouldBeStable()
        {
            var container = new ContainerObject();
            _realm.Write(() =>
            {
                _realm.Add(container);
            });

            var j = 10;
            var frozenList = container.Items.Freeze();

            TestStableIteration(
                i => container.Items.Add(new IntPropertyObject { Int = i }),
                () => frozenList = container.Items.Freeze(),
                item =>
                {
                    // Just remove last from container since item from
                    // frozen version is not equivalent.
                    j -= 1;
                    container.Items.RemoveAt(j);
                });

            Assert.That(container.Items, Is.Empty);

            var items = _realm.All<IntPropertyObject>().ToArray().Select(i => i.Int);
            var frozenItems = frozenList.ToArray().Select(i => i.Int);
            Assert.That(frozenItems, Is.EquivalentTo(Enumerable.Range(0, 10)));
            Assert.That(items, Is.EquivalentTo(Enumerable.Range(0, 10)));
        }

        [Test]
        public void ObjectList_WhenEnumeratingAndRemovingFromRealm_ShouldBeStable()
        {
            var container = new ContainerObject();
            _realm.Write(() => _realm.Add(container));

            TestStableIteration(
                i => container.Items.Add(new IntPropertyObject { Int = i }),
                () => container.Items,
                _realm.Remove);

            Assert.That(container.Items, Is.Empty);
            Assert.That(_realm.All<IntPropertyObject>(), Is.Empty);
        }

        [Test, Ignore("Snapshotting primitive lists does not work with Core 6. We should probably freeze the list when that is implemented.")]
        public void PrimitiveList_WhenEnumerating_ShouldBeStable()
        {
            var container = new ListsObject();
            _realm.Write(() => _realm.Add(container));

            TestStableIteration(
                container.Int32List.Add,
                () => container.Int32List,
                i => container.Int32List.Remove(i));

            Assert.That(container.Int32List, Is.Empty);
        }

        [Test]
        public void Collection_GetEnumerator_WhenCollectionDeleted_ReturnsEmpty()
        {
            var container = new ContainerObject
            {
                Items =
                {
                    new IntPropertyObject()
                }
            };

            _realm.Write(() => _realm.Add(container));

            var collection = (RealmList<IntPropertyObject>)container.Items;

            Assert.That(collection.IsValid);

            var counter = 0;
            foreach (var value in collection)
            {
                counter++;
            }

            Assert.That(counter, Is.EqualTo(1));

            _realm.Write(() => _realm.Remove(container));

            Assert.That(collection.IsValid, Is.False);

            counter = 0;
            foreach (var value in collection)
            {
                counter++;
            }

            Assert.That(counter, Is.EqualTo(0));
        }

        [Test]
        public void ListAsRealmQueryable_RaisesNotifications()
        {
            var joe = new Owner
            {
                Name = "Joe"
            };

            _realm.Write(() => _realm.Add(joe));

            var oldDogs = joe.ListOfDogs.AsRealmQueryable().Where(d => d.Age >= 5);

            var changeSets = new List<ChangeSet>();
            var token = oldDogs.SubscribeForNotifications((sender, changes) =>
            {
                if (changes != null)
                {
                    changeSets.Add(changes);
                }
            });

            for (var i = 0; i < 10; i++)
            {
                _realm.Write(() => joe.ListOfDogs.Add(new Dog { Age = i }));
                _realm.Refresh();

                if (i >= 5)
                {
                    Assert.That(changeSets.Count, Is.EqualTo(i - 4));

                    var changeSet = changeSets.Last();
                    Assert.That(changeSet.InsertedIndices.Length, Is.EqualTo(1));
                    Assert.That(changeSet.DeletedIndices, Is.Empty);
                    Assert.That(changeSet.ModifiedIndices, Is.Empty);

                    Assert.That(oldDogs.ElementAt(changeSet.InsertedIndices[0]).Age, Is.EqualTo(i));
                }
            }
        }

        [Test]
        public void ListAsRealmQueryable_WhenNotRealmList_Throws()
        {
            var list = new List<Dog>();

            Assert.That(
                () => list.AsRealmQueryable(),
                Throws.TypeOf<ArgumentException>().And.Message.Contains("list must be a Realm List property"));
        }

        [Test]
        public void ListFilter_ReturnsCorrectElementAtResult()
        {
            var joe = new Owner
            {
                Name = "Joe"
            };

            _realm.Write(() => _realm.Add(joe));

            var oldDogs = joe.ListOfDogs.Filter("Age >= 5");

            _realm.Write(() =>
            {
                for (var i = 0; i < 10; i++)
                {
                    joe.ListOfDogs.Add(new Dog { Age = i });
                }
            });

            for (var i = 0; i < 5; i++)
            {
                var dog = oldDogs.ElementAt(i);
                Assert.That(dog.Age, Is.EqualTo(i + 5));
            }
        }

        [Test]
        public void ListFilter_PassesArgumentsCorrectly()
        {
            var joe = new Owner
            {
                Name = "Joe"
            };

            _realm.Write(() => _realm.Add(joe));

            var fiDogs = joe.ListOfDogs.Filter("Name BEGINSWITH[c] $0", "Fi");

            _realm.Write(() =>
            {
                joe.ListOfDogs.Add(new Dog { Name = "Rick" });
                joe.ListOfDogs.Add(new Dog { Name = "Fido" });
                joe.ListOfDogs.Add(new Dog { Name = "Fester" });
                joe.ListOfDogs.Add(new Dog { Name = "Fifi" });
                joe.ListOfDogs.Add(new Dog { Name = "Bango" });
            });

            Assert.That(fiDogs.Count(), Is.EqualTo(2));
            Assert.That(fiDogs.ToArray().Select(d => d.Name), Is.EquivalentTo(new[] { "Fido", "Fifi" }));
        }

        [Test]
        public void ListFilter_CanSortResults()
        {
            var joe = new Owner
            {
                Name = "Joe"
            };

            _realm.Write(() => _realm.Add(joe));

            var fiDogs = joe.ListOfDogs.Filter("Name BEGINSWITH[c] $0 SORT(Name desc)", "Fi");

            _realm.Write(() =>
            {
                joe.ListOfDogs.Add(new Dog { Name = "Rick" });
                joe.ListOfDogs.Add(new Dog { Name = "Fido" });
                joe.ListOfDogs.Add(new Dog { Name = "Fifi" });
                joe.ListOfDogs.Add(new Dog { Name = "Bango" });
            });

            Assert.That(fiDogs.Count(), Is.EqualTo(2));
            Assert.That(fiDogs.ElementAt(0).Name, Is.EqualTo("Fifi"));
            Assert.That(fiDogs.ElementAt(1).Name, Is.EqualTo("Fido"));
        }

        [Test]
        public void ListFilter_CanBeFilteredWithLinq()
        {
            var joe = new Owner
            {
                Name = "Joe"
            };

            _realm.Write(() => _realm.Add(joe));

            var rDogs = joe.ListOfDogs.Filter("Name BEGINSWITH[c] $0 SORT(Name desc)", "R");

            _realm.Write(() =>
            {
                joe.ListOfDogs.Add(new Dog { Name = "Fifi", Vaccinated = false, Age = 7 });
                joe.ListOfDogs.Add(new Dog { Name = "Rick", Vaccinated = true, Age = 9 });
                joe.ListOfDogs.Add(new Dog { Name = "Robert", Vaccinated = true, Age = 3 });
                joe.ListOfDogs.Add(new Dog { Name = "Roxy", Vaccinated = false, Age = 12 });
                joe.ListOfDogs.Add(new Dog { Name = "Rory", Vaccinated = true, Age = 5 });
                joe.ListOfDogs.Add(new Dog { Name = "Bango", Vaccinated = true, Age = 1 });
            });

            Assert.That(rDogs.Count(), Is.EqualTo(4));

            rDogs = rDogs.Where(d => d.Vaccinated).OrderBy(d => d.Age);

            Assert.That(rDogs.Count(), Is.EqualTo(3));
            Assert.That(rDogs.ElementAt(0).Name, Is.EqualTo("Robert"));
            Assert.That(rDogs.ElementAt(1).Name, Is.EqualTo("Rory"));
            Assert.That(rDogs.ElementAt(2).Name, Is.EqualTo("Rick"));
        }

        [Test]
        public void ListFilter_CanBeFilteredWithStringPredicate()
        {
            var joe = new Owner
            {
                Name = "Joe"
            };

            _realm.Write(() => _realm.Add(joe));

            var rDogs = joe.ListOfDogs.Filter("Name BEGINSWITH[c] $0 SORT(Name desc)", "R");

            _realm.Write(() =>
            {
                joe.ListOfDogs.Add(new Dog { Name = "Fifi", Vaccinated = false, Age = 7 });
                joe.ListOfDogs.Add(new Dog { Name = "Rick", Vaccinated = true, Age = 9 });
                joe.ListOfDogs.Add(new Dog { Name = "Robert", Vaccinated = true, Age = 3 });
                joe.ListOfDogs.Add(new Dog { Name = "Roxy", Vaccinated = false, Age = 12 });
                joe.ListOfDogs.Add(new Dog { Name = "Rory", Vaccinated = true, Age = 5 });
                joe.ListOfDogs.Add(new Dog { Name = "Bango", Vaccinated = true, Age = 1 });
            });

            Assert.That(rDogs.Count(), Is.EqualTo(4));

            rDogs = rDogs.Filter("Vaccinated = true SORT(Age asc)");

            Assert.That(rDogs.Count(), Is.EqualTo(3));
            Assert.That(rDogs.ElementAt(0).Name, Is.EqualTo("Robert"));
            Assert.That(rDogs.ElementAt(1).Name, Is.EqualTo("Rory"));
            Assert.That(rDogs.ElementAt(2).Name, Is.EqualTo("Rick"));
        }

        [Test]
        public void ListFilter_WhenNotRealmList_Throws()
        {
            var list = new List<Dog>();

            Assert.That(
                () => list.Filter(string.Empty),
                Throws.TypeOf<ArgumentException>().And.Message.Contains("list must be a Realm List property"));
        }

        [Test]
        public void ListFilter_InvalidPredicate_Throws()
        {
            var joe = new Owner
            {
                Name = "Joe"
            };

            _realm.Write(() => _realm.Add(joe));

            Assert.That(
                () => joe.ListOfDogs.Filter(string.Empty),
                Throws.TypeOf<RealmException>().And.Message.Contains("Invalid predicate"));
        }

        [Test]
        public void ListFilter_NoArguments_Throws()
        {
            var joe = new Owner
            {
                Name = "Joe"
            };

            _realm.Write(() => _realm.Add(joe));

            Assert.That(
                () => joe.ListOfDogs.Filter("Name = $0"),
                Throws.TypeOf<RealmException>().And.Message.Contains("no arguments are provided"));
        }

        [Test]
        public void Set_WhenIndexIsNegative_ShouldThrow()
        {
            var container = new ContainerObject();
            _realm.Write(() => _realm.Add(container));

            Assert.That(() =>
            {
                _realm.Write(() => container.Items[-1] = new IntPropertyObject());
            }, Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Set_WhenIndexIsMoreThanCount_ShouldThrow()
        {
            var container = new ContainerObject();
            _realm.Write(() => _realm.Add(container));

            Assert.That(() =>
            {
                _realm.Write(() => container.Items[1] = new IntPropertyObject());
            }, Throws.TypeOf<ArgumentOutOfRangeException>());

            _realm.Write(() => container.Items.Add(new IntPropertyObject()));
            Assert.That(() =>
            {
                _realm.Write(() => container.Items[2] = new IntPropertyObject());
            }, Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Set_WhenObjectIsManaged_ShouldWork()
        {
            var container = new ContainerObject();
            var objectToSet = new IntPropertyObject
            {
                Int = 42
            };

            _realm.Write(() =>
            {
                _realm.Add(container);
                container.Items.Add(new IntPropertyObject
                {
                    Int = -1
                });

                _realm.Add(objectToSet);
            });

            _realm.Write(() => container.Items[0] = objectToSet);

            Assert.That(container.Items.Count, Is.EqualTo(1));
            Assert.That(container.Items[0], Is.EqualTo(objectToSet));
            Assert.That(container.Items[0].Int, Is.EqualTo(42));
        }

        [Test]
        public void Set_WhenObjectIsUnmanaged_ShouldAddToRealm()
        {
            var container = new ContainerObject();
            var objectToSet = new IntPropertyObject
            {
                Int = 42
            };

            _realm.Write(() =>
            {
                _realm.Add(container);
                container.Items.Add(new IntPropertyObject
                {
                    Int = -1
                });
            });

            _realm.Write(() => container.Items[0] = objectToSet);

            Assert.That(objectToSet.IsManaged);
            Assert.That(container.Items.Count, Is.EqualTo(1));
            Assert.That(container.Items[0], Is.EqualTo(objectToSet));
            Assert.That(container.Items[0].Int, Is.EqualTo(42));
        }

        [Test]
        public void SetAsRealmQueryable_RaisesNotifications()
        {
            var joe = new Owner
            {
                Name = "Joe"
            };

            _realm.Write(() => _realm.Add(joe));

            var oldDogs = joe.SetOfDogs.AsRealmQueryable().Where(d => d.Age >= 5);

            var changeSets = new List<ChangeSet>();
            var token = oldDogs.SubscribeForNotifications((sender, changes) =>
            {
                if (changes != null)
                {
                    changeSets.Add(changes);
                }
            });

            for (var i = 0; i < 10; i++)
            {
                _realm.Write(() => joe.SetOfDogs.Add(new Dog { Age = i }));
                _realm.Refresh();

                if (i >= 5)
                {
                    Assert.That(changeSets.Count, Is.EqualTo(i - 4));

                    var changeSet = changeSets.Last();
                    Assert.That(changeSet.InsertedIndices.Length, Is.EqualTo(1));
                    Assert.That(changeSet.DeletedIndices, Is.Empty);
                    Assert.That(changeSet.ModifiedIndices, Is.Empty);

                    Assert.That(oldDogs.ElementAt(changeSet.InsertedIndices[0]).Age, Is.EqualTo(i));
                }
            }
        }

        [Test]
        public void SetAsRealmQueryable_WhenNotRealmSet_Throws()
        {
            var set = new HashSet<Dog>();

            Assert.That(
                () => set.AsRealmQueryable(),
                Throws.TypeOf<ArgumentException>().And.Message.Contains("set must be a Realm Set property"));
        }

        [Test]
        public void SetFilter_ReturnsCorrectElementAtResult()
        {
            var joe = new Owner
            {
                Name = "Joe"
            };

            _realm.Write(() => _realm.Add(joe));

            var oldDogs = joe.SetOfDogs.Filter("Age >= 5");

            _realm.Write(() =>
            {
                for (var i = 0; i < 10; i++)
                {
                    joe.SetOfDogs.Add(new Dog { Age = i });
                }
            });

            for (var i = 0; i < 5; i++)
            {
                var dog = oldDogs.ElementAt(i);
                Assert.That(dog.Age, Is.EqualTo(i + 5));
            }
        }

        [Test]
        public void SetFilter_PassesArgumentsCorrectly()
        {
            var joe = new Owner
            {
                Name = "Joe"
            };

            _realm.Write(() => _realm.Add(joe));

            var fiDogs = joe.SetOfDogs.Filter("Name BEGINSWITH[c] $0", "Fi");

            _realm.Write(() =>
            {
                joe.SetOfDogs.Add(new Dog { Name = "Rick" });
                joe.SetOfDogs.Add(new Dog { Name = "Fido" });
                joe.SetOfDogs.Add(new Dog { Name = "Fester" });
                joe.SetOfDogs.Add(new Dog { Name = "Fifi" });
                joe.SetOfDogs.Add(new Dog { Name = "Bango" });
            });

            Assert.That(fiDogs.Count(), Is.EqualTo(2));
            Assert.That(fiDogs.ToArray().Select(d => d.Name), Is.EquivalentTo(new[] { "Fido", "Fifi" }));
        }

        [Test]
        public void SetFilter_CanSortResults()
        {
            var joe = new Owner
            {
                Name = "Joe"
            };

            _realm.Write(() => _realm.Add(joe));

            var fiDogs = joe.SetOfDogs.Filter("Name BEGINSWITH[c] $0 SORT(Name desc)", "Fi");

            _realm.Write(() =>
            {
                joe.SetOfDogs.Add(new Dog { Name = "Rick" });
                joe.SetOfDogs.Add(new Dog { Name = "Fido" });
                joe.SetOfDogs.Add(new Dog { Name = "Fifi" });
                joe.SetOfDogs.Add(new Dog { Name = "Bango" });
            });

            Assert.That(fiDogs.Count(), Is.EqualTo(2));
            Assert.That(fiDogs.ElementAt(0).Name, Is.EqualTo("Fifi"));
            Assert.That(fiDogs.ElementAt(1).Name, Is.EqualTo("Fido"));
        }

        [Test]
        public void SetFilter_CanBeFilteredWithLinq()
        {
            var joe = new Owner
            {
                Name = "Joe"
            };

            _realm.Write(() => _realm.Add(joe));

            var rDogs = joe.SetOfDogs.Filter("Name BEGINSWITH[c] $0 SORT(Name desc)", "R");

            _realm.Write(() =>
            {
                joe.SetOfDogs.Add(new Dog { Name = "Fifi", Vaccinated = false, Age = 7 });
                joe.SetOfDogs.Add(new Dog { Name = "Rick", Vaccinated = true, Age = 9 });
                joe.SetOfDogs.Add(new Dog { Name = "Robert", Vaccinated = true, Age = 3 });
                joe.SetOfDogs.Add(new Dog { Name = "Roxy", Vaccinated = false, Age = 12 });
                joe.SetOfDogs.Add(new Dog { Name = "Rory", Vaccinated = true, Age = 5 });
                joe.SetOfDogs.Add(new Dog { Name = "Bango", Vaccinated = true, Age = 1 });
            });

            Assert.That(rDogs.Count(), Is.EqualTo(4));

            rDogs = rDogs.Where(d => d.Vaccinated).OrderBy(d => d.Age);

            Assert.That(rDogs.Count(), Is.EqualTo(3));
            Assert.That(rDogs.ElementAt(0).Name, Is.EqualTo("Robert"));
            Assert.That(rDogs.ElementAt(1).Name, Is.EqualTo("Rory"));
            Assert.That(rDogs.ElementAt(2).Name, Is.EqualTo("Rick"));
        }

        [Test]
        public void SetFilter_CanBeFilteredWithStringPredicate()
        {
            var joe = new Owner
            {
                Name = "Joe"
            };

            _realm.Write(() => _realm.Add(joe));

            var rDogs = joe.SetOfDogs.Filter("Name BEGINSWITH[c] $0 SORT(Name desc)", "R");

            _realm.Write(() =>
            {
                joe.SetOfDogs.Add(new Dog { Name = "Fifi", Vaccinated = false, Age = 7 });
                joe.SetOfDogs.Add(new Dog { Name = "Rick", Vaccinated = true, Age = 9 });
                joe.SetOfDogs.Add(new Dog { Name = "Robert", Vaccinated = true, Age = 3 });
                joe.SetOfDogs.Add(new Dog { Name = "Roxy", Vaccinated = false, Age = 12 });
                joe.SetOfDogs.Add(new Dog { Name = "Rory", Vaccinated = true, Age = 5 });
                joe.SetOfDogs.Add(new Dog { Name = "Bango", Vaccinated = true, Age = 1 });
            });

            Assert.That(rDogs.Count(), Is.EqualTo(4));

            rDogs = rDogs.Filter("Vaccinated = true SORT(Age asc)");

            Assert.That(rDogs.Count(), Is.EqualTo(3));
            Assert.That(rDogs.ElementAt(0).Name, Is.EqualTo("Robert"));
            Assert.That(rDogs.ElementAt(1).Name, Is.EqualTo("Rory"));
            Assert.That(rDogs.ElementAt(2).Name, Is.EqualTo("Rick"));
        }

        [Test]
        public void SetFilter_WhenNotRealmSet_Throws()
        {
            var set = new HashSet<Dog>();

            Assert.That(
                () => set.Filter(string.Empty),
                Throws.TypeOf<ArgumentException>().And.Message.Contains("set must be a Realm Set property"));
        }

        [Test]
        public void SetFilter_InvalidPredicate_Throws()
        {
            var joe = new Owner
            {
                Name = "Joe"
            };

            _realm.Write(() => _realm.Add(joe));

            Assert.That(
                () => joe.SetOfDogs.Filter(string.Empty),
                Throws.TypeOf<RealmException>().And.Message.Contains("Invalid predicate"));
        }

        [Test]
        public void SetFilter_NoArguments_Throws()
        {
            var joe = new Owner
            {
                Name = "Joe"
            };

            _realm.Write(() => _realm.Add(joe));

            Assert.That(
                () => joe.SetOfDogs.Filter("Name = $0"),
                Throws.TypeOf<RealmException>().And.Message.Contains("no arguments are provided"));
        }

        [Test]
        public void Set_EmitsModifiedNotifications()
        {
            var container = new ContainerObject();
            _realm.Write(() =>
            {
                _realm.Add(container);
                for (var i = 0; i < 5; i++)
                {
                    container.Items.Add(new IntPropertyObject { Int = i });
                }
            });

            var notifications = new List<ChangeSet>();
            var token = container.Items.SubscribeForNotifications((sender, changes) =>
            {
                if (changes != null)
                {
                    notifications.Add(changes);
                }
            });

            using (token)
            {
                for (var i = 0; i < 5; i++)
                {
                    _realm.Write(() => container.Items[i] = new IntPropertyObject { Int = i + 5 });
                    _realm.Refresh();

                    Assert.That(notifications.Count, Is.EqualTo(i + 1));
                    Assert.That(notifications[i].ModifiedIndices, Is.EquivalentTo(new[] { i }));
                    Assert.That(container.Items[i].Int, Is.EqualTo(i + 5));
                }
            }
        }

        [Test]
        public void Results_GetFiltered_SanityTest()
        {
            _realm.Write(() =>
            {
                for (var i = 0; i < 10; i++)
                {
                    _realm.Add(new IntPropertyObject { Int = i });
                }
            });

            var query = _realm.All<IntPropertyObject>().Filter("Int >= 5");

            Assert.That(query.Count(), Is.EqualTo(5));
            Assert.That(query.ToArray().All(i => i.Int >= 5));
        }

        public struct StringQueryNumericData
        {
            public string PropertyName;
            public RealmValue ValueToAddToRealm;
            public RealmValue ValueToQueryFor;
            public bool ExpectedMatch;

            public StringQueryNumericData(string propertyName, RealmValue valueToAddToDB, RealmValue valueToQueryFor, bool expectedMatch)
            {
                PropertyName = propertyName;
                ValueToAddToRealm = valueToAddToDB;
                ValueToQueryFor = valueToQueryFor;
                ExpectedMatch = expectedMatch;
            }

            public override string ToString() => $"{PropertyName}: '{ValueToAddToRealm}' should{(ExpectedMatch ? string.Empty : " NOT")} match '{ValueToQueryFor}': {ExpectedMatch}";
        }

        public struct StringQueryTestData
        {
            public string PropertyName;
            public RealmValue MatchingValue;
            public RealmValue NonMatchingValue;

            public StringQueryTestData(string propertyName, RealmValue matchingValue, RealmValue nonMatchingValue)
            {
                PropertyName = propertyName;
                MatchingValue = matchingValue;
                NonMatchingValue = nonMatchingValue;
            }

            public override string ToString() => $"{PropertyName}, match: '{MatchingValue}' non-match: '{NonMatchingValue}'";
        }

        private static object BoxValue(RealmValue val, Type targetType)
        {
            var boxed = val.AsAny();
            if (boxed != null && targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(RealmInteger<>))
            {
                var wrappedType = targetType.GetGenericArguments().Single();
                boxed = Convert.ChangeType(boxed, wrappedType);
                return Activator.CreateInstance(targetType, BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { boxed }, null);
            }

            if (boxed != null && boxed.GetType() != targetType)
            {
                return Convert.ChangeType(boxed, targetType);
            }

            return boxed;
        }

        public static object[] StringQuery_AllTypes =
        {
            new object[] { new StringQueryTestData(nameof(AllTypesObject.CharProperty), 'c', 'b') },
            new object[] { new StringQueryTestData(nameof(AllTypesObject.ByteProperty), 0x5, 0x4) },
            new object[] { new StringQueryTestData(nameof(AllTypesObject.Int16Property), 5, 4) },
            new object[] { new StringQueryTestData(nameof(AllTypesObject.Int32Property), 34, 42) },
            new object[] { new StringQueryTestData(nameof(AllTypesObject.Int64Property), 74L, 23L) },
            new object[] { new StringQueryTestData(nameof(AllTypesObject.SingleProperty), 3.0f, 2.0f) },
            new object[] { new StringQueryTestData(nameof(AllTypesObject.DoubleProperty), 4.0, 2.0) },
            new object[] { new StringQueryTestData(nameof(AllTypesObject.BooleanProperty), true, false) },
            new object[] { new StringQueryTestData(nameof(AllTypesObject.DateTimeOffsetProperty), new DateTimeOffset(1956, 6, 1, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2020, 8, 2, 0, 0, 0, TimeSpan.Zero)) },
            new object[] { new StringQueryTestData(nameof(AllTypesObject.DecimalProperty), 0.9999999999999999999999999999, 0.3333333333333333333333333333) },
            new object[] { new StringQueryTestData(nameof(AllTypesObject.Decimal128Property), new Decimal128(564.42343424323), new Decimal128(666.42300000003)) },
            new object[] { new StringQueryTestData(nameof(AllTypesObject.ObjectIdProperty), new ObjectId("5f64cd9f1691c361b2451d96"), new ObjectId("5ffffffffffff22222222222")) },
            new object[] { new StringQueryTestData(nameof(AllTypesObject.GuidProperty), new Guid("0f8fad5b-d9cb-469f-a165-70867728950e"), new Guid("0ffffffb-dddd-4444-aaaa-70000000000e")) },
            new object[] { new StringQueryTestData(nameof(AllTypesObject.StringProperty), "hello world", "no salute") },
            new object[] { new StringQueryTestData(nameof(AllTypesObject.ByteArrayProperty), new byte[] { 0x5, 0x4, 0x3, 0x2, 0x1 }, new byte[] { 0x1, 0x1, 0x1 }) },
            new object[] { new StringQueryTestData(nameof(AllTypesObject.ByteCounterProperty), new RealmInteger<byte>(0x6), new RealmInteger<byte>(0x8)) },
            new object[] { new StringQueryTestData(nameof(AllTypesObject.Int16CounterProperty), new RealmInteger<short>(2), new RealmInteger<short>(7)) },
            new object[] { new StringQueryTestData(nameof(AllTypesObject.Int32CounterProperty), new RealmInteger<int>(10), new RealmInteger<int>(24)) },
            new object[] { new StringQueryTestData(nameof(AllTypesObject.Int64CounterProperty), new RealmInteger<long>(5L), new RealmInteger<long>(22L)) },
        };

        public static object[] StringQuery_NumericValues =
        {
            // implicit conversion match
            new object[] { new StringQueryNumericData(nameof(AllTypesObject.CharProperty), 'a', 97, true) },
            new object[] { new StringQueryNumericData(nameof(AllTypesObject.ByteProperty), 0x6, 6, true) },
            new object[] { new StringQueryNumericData(nameof(AllTypesObject.ByteProperty), 0xf, 15.0f, true) },
            new object[] { new StringQueryNumericData(nameof(AllTypesObject.Int16Property), 55, 55.0f, true) },
            new object[] { new StringQueryNumericData(nameof(AllTypesObject.Int32Property), 66, 66.0, true) },
            new object[] { new StringQueryNumericData(nameof(AllTypesObject.Int32Property), 19, 19.0f, true) },
            new object[] { new StringQueryNumericData(nameof(AllTypesObject.Int64Property), 77L, 77, true) },
            new object[] { new StringQueryNumericData(nameof(AllTypesObject.Int64Property), 82L, 82m, true) },
            new object[] { new StringQueryNumericData(nameof(AllTypesObject.SingleProperty), 88.8f, 88.8, true) },
            new object[] { new StringQueryNumericData(nameof(AllTypesObject.SingleProperty), 49f, 49, true) },
            new object[] { new StringQueryNumericData(nameof(AllTypesObject.DoubleProperty), 106.0, 106m, true) },
            new object[] { new StringQueryNumericData(nameof(AllTypesObject.DecimalProperty), 1m, 1f, true) },
            new object[] { new StringQueryNumericData(nameof(AllTypesObject.DecimalProperty), 5m, 5.0, true) },

            // implicit conversion no match
            new object[] { new StringQueryNumericData(nameof(AllTypesObject.CharProperty), 'c', 2555, false) },
            new object[] { new StringQueryNumericData(nameof(AllTypesObject.ByteProperty), 0x5, 'g', false) },
            new object[] { new StringQueryNumericData(nameof(AllTypesObject.Int16Property), 5, 35L, false) },
            new object[] { new StringQueryNumericData(nameof(AllTypesObject.Int32Property), 34, 563.0f, false) },
            new object[] { new StringQueryNumericData(nameof(AllTypesObject.Int64Property), 74L, 7435, false) },
            new object[] { new StringQueryNumericData(nameof(AllTypesObject.SingleProperty), 3.0f, 21.0, false) },
            new object[] { new StringQueryNumericData(nameof(AllTypesObject.DoubleProperty), 4.0, 'c', false) },

            // no implicit conversion no match
            new object[] { new StringQueryNumericData(nameof(AllTypesObject.DoubleProperty), 109.9, 109.9f, false) },
        };

        public static object[] StringQuery_MismatchingTypes_ToThrow =
        {
            new object[] { new StringQueryTestData(nameof(AllTypesObject.CharProperty), 'c', "who are you") },
            new object[] { new StringQueryTestData(nameof(AllTypesObject.Int16Property), 2, "no one is here") },
            new object[] { new StringQueryTestData(nameof(AllTypesObject.Int32Property), 3, "go away") },
            new object[] { new StringQueryTestData(nameof(AllTypesObject.Int64Property), 32L, "again you?") },
            new object[] { new StringQueryTestData(nameof(AllTypesObject.SingleProperty), 4.0f, "I said go") },
            new object[] { new StringQueryTestData(nameof(AllTypesObject.DoubleProperty), 5.0, "I'm getting angry") },
            new object[] { new StringQueryTestData(nameof(AllTypesObject.ByteProperty), 0x6, "I give up") },
            new object[] { new StringQueryTestData(nameof(AllTypesObject.BooleanProperty), true, "enough") },
            new object[] { new StringQueryTestData(nameof(AllTypesObject.BooleanProperty), true, 1) },
            new object[] { new StringQueryTestData(nameof(AllTypesObject.DateTimeOffsetProperty), new DateTimeOffset(1956, 6, 1, 0, 0, 0, TimeSpan.Zero), 5) },
            new object[] { new StringQueryTestData(nameof(AllTypesObject.DecimalProperty), 7m, new byte[] { 0x1, 0x2, 0x3 }) },
            new object[] { new StringQueryTestData(nameof(AllTypesObject.Decimal128Property), new Decimal128(564.42343424323), new byte[] { 0x3, 0x2, 0x1 }) },
            new object[] { new StringQueryTestData(nameof(AllTypesObject.ObjectIdProperty), new ObjectId("5f64cd9f1691c361b2451d96"), "hello world") },
            new object[] { new StringQueryTestData(nameof(AllTypesObject.GuidProperty), new Guid("0f8fad5b-d9cb-469f-a165-70867728950e"), 'v') },
            new object[] { new StringQueryTestData(nameof(AllTypesObject.StringProperty), "hello you", 13m) },
            new object[] { new StringQueryTestData(nameof(AllTypesObject.ByteArrayProperty), new byte[] { 0x5, 0x4, 0x3, 0x2, 0x1 }, 34L) },
        };

        [TestCaseSource(nameof(StringQuery_AllTypes))]
        public void QueryFilter_WithAnyArguments_ShouldMatch(StringQueryTestData data)
        {
            var propInfo = typeof(AllTypesObject).GetProperty(data.PropertyName);
            var boxedMatch = BoxValue(data.MatchingValue, propInfo.PropertyType);
            var boxedNonMatch = BoxValue(data.NonMatchingValue, propInfo.PropertyType);

            _realm.Write(() =>
            {
                var matchingObj = _realm.Add(new AllTypesObject { RequiredStringProperty = string.Empty });
                propInfo.SetValue(matchingObj, boxedMatch);

                var nonMatchingObj = _realm.Add(new AllTypesObject { RequiredStringProperty = string.Empty });
                propInfo.SetValue(nonMatchingObj, boxedNonMatch);
            });

            var matches = _realm.All<AllTypesObject>().Filter($"{data.PropertyName} = $0", data.MatchingValue);
            var foundVal = propInfo.GetValue(matches.Single());
            Assert.AreEqual(foundVal, boxedMatch);
        }

        [TestCaseSource(nameof(StringQuery_NumericValues))]
        public void QueryFilter_WithNumericArguments(StringQueryNumericData data)
        {
            var propInfo = typeof(AllTypesObject).GetProperty(data.PropertyName);
            var boxedMatch = BoxValue(data.ValueToAddToRealm, propInfo.PropertyType);

            _realm.Write(() =>
            {
                var matchingObj = _realm.Add(new AllTypesObject { RequiredStringProperty = string.Empty });
                propInfo.SetValue(matchingObj, boxedMatch);
            });

            var matches = _realm.All<AllTypesObject>().Filter($"{data.PropertyName} = $0", data.ValueToQueryFor);
            if (data.ExpectedMatch)
            {
                Assert.AreEqual(matches.Count(), 1);
                Assert.AreEqual(propInfo.GetValue(matches.Single()), boxedMatch);
            }
            else
            {
                Assert.AreEqual(matches.Count(), 0);
            }
        }

        [TestCaseSource(nameof(StringQuery_AllTypes))]
        public void QueryFilter_WithAnyEmbeddedObjectArguments_ShouldMatch(StringQueryTestData data)
        {
            var propInfoObjWithEmbedded = typeof(ObjectWithEmbeddedProperties).GetProperty("AllTypesObject");
            var propInfoEmbeddedAllTypes = typeof(EmbeddedAllTypesObject).GetProperty(data.PropertyName);

            var boxedMatch = BoxValue(data.MatchingValue, propInfoEmbeddedAllTypes.PropertyType);
            var embeddedAllTypesMatch = new EmbeddedAllTypesObject();
            propInfoEmbeddedAllTypes.SetValue(embeddedAllTypesMatch, boxedMatch);

            var boxedNonMatch = BoxValue(data.NonMatchingValue, propInfoEmbeddedAllTypes.PropertyType);
            var embeddedAllTypesNonMatch = new EmbeddedAllTypesObject();
            propInfoEmbeddedAllTypes.SetValue(embeddedAllTypesNonMatch, boxedNonMatch);

            _realm.Write(() =>
            {
                var matchingEmbeddedObj = _realm.Add(new ObjectWithEmbeddedProperties { PrimaryKey = 0 });
                propInfoObjWithEmbedded.SetValue(matchingEmbeddedObj, embeddedAllTypesMatch);

                var nonMatchingEmbeddedObj = _realm.Add(new ObjectWithEmbeddedProperties { PrimaryKey = 1 });
                propInfoObjWithEmbedded.SetValue(nonMatchingEmbeddedObj, embeddedAllTypesNonMatch);
            });

            var matches = _realm.All<ObjectWithEmbeddedProperties>().Filter("AllTypesObject = $0", embeddedAllTypesMatch);
            var foundObj = propInfoEmbeddedAllTypes.GetValue(matches.Single().AllTypesObject);
            Assert.AreEqual(foundObj, boxedMatch);
        }

        [TestCaseSource(nameof(StringQuery_MismatchingTypes_ToThrow))]
        public void QueryFilter_WithWrongArguments_ShouldThrow(StringQueryTestData data)
        {
            var propInfo = typeof(AllTypesObject).GetProperty(data.PropertyName);
            var boxedMatch = BoxValue(data.MatchingValue, propInfo.PropertyType);

            _realm.Write(() =>
            {
                var matchingObj = _realm.Add(new AllTypesObject { RequiredStringProperty = string.Empty });
                propInfo.SetValue(matchingObj, boxedMatch);
            });

            Assert.Throws<RealmException>(() => _realm.All<AllTypesObject>().Filter($"{data.PropertyName} = $0", data.NonMatchingValue), $"Unsupported comparison between type {propInfo.PropertyType.Name} and type {data.NonMatchingValue.GetType().Name}");
        }

        [Test]
        public void QueryFilter_WithArgumentsObjectList_ShouldMatch()
        {
            Internal_QueryFilter_OnObjects(true);
        }

        [Test]
        public void QueryFilter_WithArgumentsObject_ShouldMatch()
        {
            Internal_QueryFilter_OnObjects(false);
        }

        private void Internal_QueryFilter_OnObjects(bool queryList)
        {
            var dog1 = new Dog { Name = "Fido", Color = "black", Vaccinated = true };
            var dog2 = new Dog { Name = "Pluto", Color = "white", Vaccinated = true };
            var dog3 = new Dog { Name = "Pippo", Color = "brown", Vaccinated = false };
            var dog4 = new Dog { Name = "JustDog", Color = "pink", Vaccinated = false };
            var marioOwner = new Owner { Name = "Mario", TopDog = dog1, ListOfDogs = { dog1, dog2, dog3 } };
            var luigiOwner = new Owner { Name = "Luigi", TopDog = dog4, ListOfDogs = { dog4 } };

            _realm.Write(() =>
            {
                _realm.Add(marioOwner);
                _realm.Add(luigiOwner);
            });
            IList<Dog> list = new List<Dog>();
            list.Add(dog1);
            var matches = queryList ? _realm.All<Owner>().Filter("ANY ListOfDogs.Name == $0", dog1.Name) : _realm.All<Owner>().Filter("TopDog == $0", dog1);
            Assert.AreEqual(marioOwner, matches.Single());
        }

        [Test]
        public void QueryFilter_WithArgumentsUnmanagedObjects_ShouldThrow()
        {
            _realm.Write(() =>
            {
                _realm.Add(new Owner { TopDog = new Dog { Name = "Doge", Color = "almost yellow", Vaccinated = true } });
            });

            Assert.Throws<RealmException>(() => _realm.All<Owner>().Filter("TopDog = $0", new Dog { Name = "Doge", Color = "almost yellow", Vaccinated = true }));
        }

        [Test]
        public void QueryFilter_WithNullArguments()
        {
            var nullableObjMatch = new AllTypesObject { RequiredStringProperty = "hello", NullableInt32Property = null };
            var nullableObjNoMatch = new AllTypesObject { RequiredStringProperty = "world", NullableInt32Property = 42 };
            _realm.Write(() =>
            {
                _realm.Add(nullableObjMatch);
                _realm.Add(nullableObjNoMatch);
            });

            var matches = _realm.All<AllTypesObject>().Filter("NullableInt32Property = $0", (int?)null);
            Assert.AreEqual(matches.Single(), nullableObjMatch);

            RealmValue[] argumentsArray = null;
            Assert.Throws<ArgumentNullException>(() => _realm.All<Dog>().Filter("Name = $0", argumentsArray));
        }

        [Test]
        public void QueryFilter_WithMultipleArguments_ShouldMatch()
        {
            var matchingObj = new AllTypesObject { RequiredStringProperty = "hello pp", Int32Property = 9, SingleProperty = 21.0f, CharProperty = 'p' };
            var nonMatchingObj = new AllTypesObject { RequiredStringProperty = "no salute", Int32Property = 0, SingleProperty = 1.0f, CharProperty = 'h' };
            _realm.Write(() =>
            {
                _realm.Add(matchingObj);
                _realm.Add(nonMatchingObj);
            });

            var matches = _realm.All<AllTypesObject>().Filter("Int32Property == $0 && SingleProperty == $1 && CharProperty == $2 && RequiredStringProperty == $3", 9, 21.0f, 'p', "hello pp");
            Assert.AreEqual(matches.Single(), matchingObj);
        }

        [Test]
        public void Results_GetFiltered_List()
        {
            _realm.Write(() =>
            {
                for (var i = 0; i < 10; i++)
                {
                    var owner = new Owner
                    {
                        Name = $"Owner {i}",
                        ListOfDogs =
                        {
                            new Dog
                            {
                                Name = $"Dog {2 * i}",
                                Vaccinated = (2 * i) % 5 == 0
                            },
                            new Dog
                            {
                                Name = $"Dog {(2 * i) + 1}",
                                Vaccinated = ((2 * i) + 1) % 5 == 0
                            }
                        }
                    };
                    _realm.Add(owner);
                }
            });

            var owners = _realm.All<Owner>().Filter("ListOfDogs.Vaccinated == true");
            Assert.That(owners.Count(), Is.EqualTo(4));
            Assert.That(owners.ToArray().All(o => o.ListOfDogs.Any(d => d.Vaccinated)));
        }

        [Test]
        public void Results_GetFiltered_NamedBacklink()
        {
            _realm.Write(() =>
            {
                for (var i = 0; i < 10; i++)
                {
                    var dog = new Dog
                    {
                        Name = $"Dog {i}"
                    };

                    _realm.Add(new Owner
                    {
                        Name = $"Person {(2 * i) % 5}",
                        ListOfDogs = { dog }
                    });

                    _realm.Add(new Owner
                    {
                        Name = $"Person {((2 * i) + 1) % 5}",
                        ListOfDogs = { dog }
                    });
                }
            });

            var dogs = _realm.All<Dog>().Filter("Owners.Name == 'Person 0'");
            Assert.That(dogs.Count(), Is.EqualTo(4));
            Assert.That(dogs.ToArray().All(d => d.Owners.Any(o => o.Name == "Person 0")));
        }

        [Test]
        public void Results_GetFiltered_UnnamedBacklink()
        {
            _realm.Write(() =>
            {
                for (var i = 0; i < 10; i++)
                {
                    var dog = new Dog
                    {
                        Name = $"Dog {i}"
                    };

                    _realm.Add(new Owner
                    {
                        Name = $"Person {(2 * i) % 5}",
                        ListOfDogs = { dog }
                    });

                    _realm.Add(new Owner
                    {
                        Name = $"Person {((2 * i) + 1) % 5}",
                        ListOfDogs = { dog }
                    });
                }
            });

            var dogs = _realm.All<Dog>().Filter("@links.Owner.ListOfDogs.Name == 'Person 0'");
            Assert.That(dogs.Count(), Is.EqualTo(4));
            Assert.That(dogs.ToArray().All(d => d.Owners.Any(o => o.Name == "Person 0")));
        }

        [Test]
        public void Results_GetFiltered_FollowLinks()
        {
            PopulateAObjects();

            var objects = _realm.All<A>().Filter("B.C.Int < 5");
            Assert.That(objects.Count(), Is.EqualTo(5));
            Assert.That(objects.ToArray().All(o => o.B.C.Int < 5));
        }

        [Test]
        public void Results_GetFiltered_Notifications()
        {
            PopulateAObjects(3, 4, 5, 6, 7);

            var objects = _realm.All<A>().Filter("B.C.Int < 5");
            Assert.That(objects.Count(), Is.EqualTo(2));

            var notificationsCount = 0;

            using (var token = objects.SubscribeForNotifications(CallbackHandler))
            {
                PopulateAObjects(2);
                Assert.That(notificationsCount, Is.EqualTo(1));

                PopulateAObjects(8);
                Assert.That(notificationsCount, Is.EqualTo(1));

                PopulateAObjects(1);
                Assert.That(notificationsCount, Is.EqualTo(2));

                PopulateAObjects(9);
                Assert.That(notificationsCount, Is.EqualTo(2));
            }

            Assert.That(objects.Count(), Is.EqualTo(4));

            void CallbackHandler(IRealmCollection<A> sender, ChangeSet changes)
            {
                if (changes != null)
                {
                    notificationsCount++;
                    Assert.That(changes.InsertedIndices.Length, Is.EqualTo(1));
                }
            }
        }

        [Test]
        public void Results_GetFiltered_BeforeLINQ()
        {
            PopulateAObjects();

            var objects = _realm.All<A>().Filter("B.C.Int < 6");
            Assert.That(objects.Count(), Is.EqualTo(6));

            objects = objects.Where(a => a.Value);
            Assert.That(objects.Count(), Is.EqualTo(3));
            Assert.That(objects.ToArray().All(o => o.Value && o.B.C.Int < 6));
        }

        [Test]
        public void Results_GetFiltered_AfterLINQ()
        {
            PopulateAObjects();

            var objects = _realm.All<A>().Where(o => o.Value);
            Assert.That(objects.Count(), Is.EqualTo(5));

            objects = objects.Filter("B.C.Int < 6");
            Assert.That(objects.Count(), Is.EqualTo(3));
            Assert.That(objects.ToArray().All(o => o.Value && o.B.C.Int < 6));
        }

        [Test]
        public void Results_GetFiltered_Sorted()
        {
            PopulateAObjects();
            var objects = _realm.All<A>().Filter("TRUEPREDICATE SORT(Value ASC, B.C.Int DESC)").AsRealmCollection();

            var expectedOrder = new[] { 9, 7, 5, 3, 1, 8, 6, 4, 2, 0 };
            for (var i = 0; i < 10; i++)
            {
                var obj = objects[i];
                Assert.That(obj.Value, Is.EqualTo(i >= 5));

                Assert.That(obj.B.C.Int, Is.EqualTo(expectedOrder[i]));
            }
        }

        [Test]
        public void Results_GetFiltered_Distinct()
        {
            PopulateAObjects();
            var objects = _realm.All<A>().Filter("TRUEPREDICATE SORT(Value ASC) DISTINCT(Value)").AsRealmCollection();

            Assert.That(objects.Count, Is.EqualTo(2));
            Assert.That(objects[0].Value, Is.False);
            Assert.That(objects[1].Value, Is.True);
        }

        [Test]
        public void Results_GetFiltered_Limit()
        {
            PopulateAObjects();
            var objects = _realm.All<A>().Filter("TRUEPREDICATE SORT(Value ASC) Limit(1)");

            Assert.That(objects.Count(), Is.EqualTo(1));
            Assert.That(objects.AsRealmCollection().Count, Is.EqualTo(1));
            Assert.That(objects.Single().Value, Is.False);
        }

        [Test]
        public void Results_GetFiltered_WhenPredicateIsInvalid_Throws()
        {
            Assert.That(
                () => _realm.All<A>().Filter("Foo == 5"),
                Throws.TypeOf<RealmException>().And.Message.Contains("'A' has no property 'Foo'"));
        }

        [Test]
        public void List_IndexOf_WhenObjectBelongsToADifferentRealm_ShouldThrow()
        {
            var owner = new Owner();
            _realm.Write(() =>
            {
                _realm.Add(owner);
            });

            var config = new RealmConfiguration(Guid.NewGuid().ToString());
            using var otherRealm = GetRealm(config);
            var otherRealmDog = new Dog();
            otherRealm.Write(() =>
            {
                otherRealm.Add(otherRealmDog);
            });

            Assert.That(() => owner.ListOfDogs.IndexOf(otherRealmDog), Throws.InstanceOf<RealmObjectManagedByAnotherRealmException>());
        }

        [Test]
        public void List_Freeze_ReturnsAFrozenCopy()
        {
            var obj = new Owner
            {
                Name = "Peter",
                ListOfDogs =
                {
                    new Dog { Name = "Alpha" },
                    new Dog { Name = "Beta" }
                }
            };

            _realm.Write(() =>
            {
                _realm.Add(obj);
            });

            Assert.That(obj.IsManaged);

            var frozenDogs = Freeze(obj.ListOfDogs);

            Assert.That(obj.IsManaged);
            Assert.That(obj.IsValid);
            Assert.That(obj.IsFrozen, Is.False);
            Assert.That(obj.ListOfDogs.AsRealmCollection().IsFrozen, Is.False);
            Assert.That(obj.Realm.IsFrozen, Is.False);

            Assert.That(frozenDogs.AsRealmCollection().IsFrozen);
            Assert.That(frozenDogs.AsRealmCollection().IsValid);
            Assert.That(frozenDogs.AsRealmCollection().Realm.IsFrozen);
            Assert.That(frozenDogs[0].IsFrozen);
            Assert.That(frozenDogs[0].IsValid);
            Assert.That(frozenDogs[1].IsFrozen);
            Assert.That(frozenDogs[1].IsValid);
        }

        [Test]
        public void FrozenList_GetsGarbageCollected()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                await TestHelpers.EnsureObjectsAreCollected(() =>
                {
                    var owner = _realm.Write(() =>
                    {
                        return _realm.Add(new Owner
                        {
                            ListOfDogs = { new Dog { Name = "Lasse" } }
                        });
                    });

                    var frozenList = owner.ListOfDogs.Freeze();
                    var frozenRealm = frozenList.AsRealmCollection().Realm;
                    return new object[] { frozenList, frozenRealm };
                });

                // This will throw on Windows if the Realm object wasn't really GC-ed and its Realm - closed
                Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);
            });
        }

        [Test]
        public void Query_Freeze_ReturnsAFrozenCopy()
        {
            _realm.Write(() =>
            {
                _realm.Add(new Dog { Name = "Alpha" });
                _realm.Add(new Dog { Name = "Beta" });
                _realm.Add(new Dog { Name = "Betazaurus" });
            });

            var query = _realm.All<Dog>().Where(d => d.Name.StartsWith("B"));
            var frozenQuery = Freeze(query);

            Assert.That(query.AsRealmCollection().IsValid);
            Assert.That(query.AsRealmCollection().IsFrozen, Is.False);

            Assert.That(frozenQuery.AsRealmCollection().IsFrozen);
            Assert.That(frozenQuery.AsRealmCollection().IsValid);
            Assert.That(frozenQuery.AsRealmCollection().Realm.IsFrozen);
            Assert.That(frozenQuery.First().IsFrozen);
            Assert.That(frozenQuery.First().IsValid);
            Assert.That(frozenQuery.Last().IsFrozen);
            Assert.That(frozenQuery.Last().IsValid);
        }

        [Test]
        public void FrozenQuery_GetsGarbageCollected()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                await TestHelpers.EnsureObjectsAreCollected(() =>
                {
                    _realm.Write(() =>
                    {
                        _realm.Add(new Dog { Name = "Lasse" });
                    });

                    var frozenQuery = _realm.All<Dog>().Freeze();
                    return new[] { frozenQuery };
                });

                // This will throw on Windows if the Realm object wasn't really GC-ed and its Realm - closed
                Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);
            });
        }

        [Test]
        public void List_Freeze_WhenUnmanaged_Throws()
        {
            var list = new List<Owner>();
            Assert.Throws<RealmException>(() => Freeze(list), "Unmanaged lists cannot be frozen.");
        }

        [Test]
        public void Query_Freeze_WhenUnmanaged_Throws()
        {
            var query = new List<Owner>().AsQueryable();
            Assert.Throws<RealmException>(() => Freeze(query), "Unmanaged queries cannot be frozen.");
        }

        [Test]
        public void List_Freeze_WhenFrozen_ReturnsSameInstance()
        {
            var obj = new Owner();
            _realm.Write(() =>
            {
                _realm.Add(obj);
            });

            var frozenList = Freeze(obj.ListOfDogs);
            Assert.That(ReferenceEquals(frozenList, obj.ListOfDogs), Is.False);

            var deepFrozenList = Freeze(frozenList);
            Assert.That(ReferenceEquals(frozenList, deepFrozenList));
        }

        [Test]
        public void Query_Freeze_WhenFrozen_ReturnsSameInstance()
        {
            _realm.Write(() =>
            {
                _realm.Add(new Owner());
            });

            var query = _realm.All<Owner>();

            var frozenQuery = Freeze(query);
            Assert.That(ReferenceEquals(frozenQuery, query), Is.False);

            var deepFrozenQuery = Freeze(frozenQuery);
            Assert.That(ReferenceEquals(frozenQuery, deepFrozenQuery));
        }

        [Test]
        public void FrozenList_DoesntChange()
        {
            var obj = new Owner
            {
                ListOfDogs =
                {
                    new Dog { Name = "Rex" },
                    new Dog { Name = "Luthor" }
                }
            };

            _realm.Write(() =>
            {
                _realm.Add(obj);
            });

            var frozenList = Freeze(obj.ListOfDogs);

            Assert.That(frozenList.Count, Is.EqualTo(2));

            _realm.Write(() =>
            {
                obj.ListOfDogs[0].Name = "Lex";
                obj.ListOfDogs.Insert(1, new Dog());
            });

            Assert.That(frozenList.Count, Is.EqualTo(2));
            Assert.That(frozenList[0].Name, Is.EqualTo("Rex"));
            Assert.That(frozenList[1].Name, Is.EqualTo("Luthor"));
        }

        [Test]
        public void FrozenQuery_WhenOrderedWithLINQ_DoesntChange()
        {
            _realm.Write(() =>
            {
                _realm.Add(new Dog { Name = "C" });
                _realm.Add(new Dog { Name = "A" });
                _realm.Add(new Dog { Name = "E" });
            });

            var frozenQuery = Freeze(_realm.All<Dog>().OrderBy(d => d.Name));
            Assert.That(frozenQuery.Count(), Is.EqualTo(3));
            Assert.That(frozenQuery.ElementAt(0).Name, Is.EqualTo("A"));
            Assert.That(frozenQuery.ElementAt(1).Name, Is.EqualTo("C"));
            Assert.That(frozenQuery.ElementAt(2).Name, Is.EqualTo("E"));

            _realm.Write(() =>
            {
                _realm.Add(new Dog { Name = "D" });
                _realm.Add(new Dog { Name = "B" });
            });

            Assert.That(frozenQuery.Count(), Is.EqualTo(3));
            Assert.That(frozenQuery.ElementAt(0).Name, Is.EqualTo("A"));
            Assert.That(frozenQuery.ElementAt(1).Name, Is.EqualTo("C"));
            Assert.That(frozenQuery.ElementAt(2).Name, Is.EqualTo("E"));
        }

        [Test]
        public void FrozenQuery_WhenOrderedWithString_DoesntChange()
        {
            _realm.Write(() =>
            {
                _realm.Add(new Dog { Name = "C" });
                _realm.Add(new Dog { Name = "A" });
                _realm.Add(new Dog { Name = "E" });
            });

            var frozenQuery = Freeze(_realm.All<Dog>().Filter("TRUEPREDICATE SORT(Name ASC)"));
            Assert.That(frozenQuery.Count(), Is.EqualTo(3));
            Assert.That(frozenQuery.ElementAt(0).Name, Is.EqualTo("A"));
            Assert.That(frozenQuery.ElementAt(1).Name, Is.EqualTo("C"));
            Assert.That(frozenQuery.ElementAt(2).Name, Is.EqualTo("E"));

            _realm.Write(() =>
            {
                _realm.Add(new Dog { Name = "D" });
                _realm.Add(new Dog { Name = "B" });
            });

            Assert.That(frozenQuery.Count(), Is.EqualTo(3));
            Assert.That(frozenQuery.ElementAt(0).Name, Is.EqualTo("A"));
            Assert.That(frozenQuery.ElementAt(1).Name, Is.EqualTo("C"));
            Assert.That(frozenQuery.ElementAt(2).Name, Is.EqualTo("E"));
        }

        [Test]
        public void FrozenQuery_WhenFiltered_DoesntChange()
        {
            _realm.Write(() =>
            {
                _realm.Add(new Dog { Name = "Rex" });
                _realm.Add(new Dog { Name = "Roger" });
                _realm.Add(new Dog { Name = "Lasse" });
            });

            var frozenQuery = Freeze(_realm.All<Dog>().Where(d => d.Name.StartsWith("R")));
            Assert.That(frozenQuery.Count(), Is.EqualTo(2));

            _realm.Write(() =>
            {
                _realm.Add(new Dog { Name = "Randy" });
            });

            Assert.That(frozenQuery.Count(), Is.EqualTo(2));
            Assert.That(frozenQuery.ToArray().FirstOrDefault(d => d.Name == "Randy"), Is.Null);
        }

        [Test]
        public void ObjectFromAnotherRealm_ThrowsRealmException()
        {
            var realm2 = GetRealm(CreateConfiguration(Guid.NewGuid().ToString()));

            var item = new IntPropertyObject { Int = 5 };
            var embeddedItem = new EmbeddedIntPropertyObject { Int = 10 };

            var obj1 = _realm.Write(() =>
            {
                return _realm.Add(new CollectionsObject());
            });

            var obj2 = realm2.Write(() =>
            {
                return realm2.Add(new CollectionsObject());
            });

            _realm.Write(() =>
            {
                obj1.ObjectList.Add(item);
                obj1.EmbeddedObjectList.Add(embeddedItem);
            });

            Assert.That(() => realm2.Write(() => obj2.ObjectList.Add(item)), Throws.TypeOf<RealmException>().And.Message.Contains("object that is already in another realm"));
            Assert.That(() => realm2.Write(() => obj2.EmbeddedObjectList.Add(embeddedItem)), Throws.TypeOf<RealmException>().And.Message.Contains("embedded object that is already managed"));
        }

        private void PopulateAObjects(params int[] values)
        {
            if (values.Length == 0)
            {
                values = Enumerable.Range(0, 10).ToArray();
            }

            _realm.Write(() =>
            {
                foreach (var value in values)
                {
                    _realm.Add(new A
                    {
                        Value = value % 2 == 0,
                        B = new B { C = new IntPropertyObject { Int = value } }
                    });
                }
            });
            _realm.Refresh();
        }

        private void TestStableIteration<T>(Action<int> addItem, Func<IEnumerable<T>> getItems, Action<T> modifyItem)
        {
            _realm.Write(() =>
            {
                for (var i = 0; i < 10; i++)
                {
                    addItem(i);
                }
            });

            var count = 0;
            foreach (var item in getItems())
            {
                _realm.Write(() => modifyItem(item));
                count++;
            }

            Assert.That(count, Is.EqualTo(10));
        }

        private ContainerObject GetPopulatedManagedContainerObject()
        {
            var container = new ContainerObject();

            for (var i = 0; i < 5; i++)
            {
                container.Items.Add(new IntPropertyObject { Int = i });
            }

            _realm.Write(() => _realm.Add(container));

            return container;
        }
    }

    public partial class A : TestRealmObject
    {
        public bool Value { get; set; }

        public B B { get; set; }
    }

    public partial class B : TestRealmObject
    {
        public IntPropertyObject C { get; set; }
    }
}
