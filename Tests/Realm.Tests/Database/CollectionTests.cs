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

#if TEST_WEAVER
using TestRealmObject = Realms.RealmObject;
#else
using TestRealmObject = Realms.IRealmObject;
#endif
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using MongoDB.Bson;
using NUnit.Framework;
using Realms.Exceptions;

namespace Realms.Tests.Database;

[TestFixture, Preserve(AllMembers = true)]
public class CollectionTests : RealmInstanceTest
{
    private const string AnimalFarm = "Animal Farm";
    private const string LordOfTheRings = "The Lord of the Rings";
    private const string LordOfTheFlies = "Lord of the Flies";
    private const string WheelOfTime = "The Wheel of Time";
    private const string Silmarillion = "The Silmarillion";

    [Test]
    public void ListInsert_WhenIndexIsNegative_ShouldThrow()
    {
        var container = new ContainerObject();
        _realm.Write(() => _realm.Add(container));

        Assert.That(() =>
        {
            _realm.Write(() => container.Items.Insert(-1, new IntPropertyObject()));
        }, Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void ListInsert_WhenIndexIsMoreThanCount_ShouldThrow()
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
    public void ListAdd_WhenValueIsNull_ShouldThrow()
    {
        var container = new ContainerObject();
        _realm.Write(() => _realm.Add(container));

        _realm.Write(() =>
        {
            Assert.That(() => container.Items.Insert(0, null!), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => container.Items.Add(null!), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => container.Items[0] = null!, Throws.TypeOf<ArgumentNullException>());
        });
    }

    [Test]
    public void ListOfEmbeddedAdd_WhenValueIsNull_ShouldThrow()
    {
        var container = _realm.Write(() => _realm.Add(new CollectionsObject()));

        _realm.Write(() =>
        {
            Assert.That(() => container.EmbeddedObjectList.Insert(0, null!), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => container.EmbeddedObjectList.Add(null!), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => container.EmbeddedObjectList[0] = null!, Throws.TypeOf<ArgumentNullException>());
        });
    }

    [Test]
    public void SetAdd_WhenValueIsNull_ShouldThrow()
    {
        var container = _realm.Write(() => _realm.Add(new CollectionsObject()));

        _realm.Write(() =>
        {
            Assert.That(() => container.ObjectSet.Add(null!), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => container.ObjectSet.UnionWith(new IntPropertyObject[] { null! }), Throws.TypeOf<ArgumentNullException>());
        });
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
            _ =>
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

        foreach (var dummy in collection)
        {
            counter++;
        }

        Assert.That(counter, Is.EqualTo(1));

        _realm.Write(() => _realm.Remove(container));

        Assert.That(collection.IsValid, Is.False);

        counter = 0;

        foreach (var dummy in collection)
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
        using var token = oldDogs.SubscribeForNotifications((_, changes) =>
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
            Throws.TypeOf<ArgumentException>().And.Message.Contains("Invalid predicate"));
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
            Throws.TypeOf<ArgumentException>().And.Message.Contains("Request for argument at index 0 but no arguments are provided"));
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
        using var token = oldDogs.SubscribeForNotifications((_, changes) =>
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
            Throws.TypeOf<ArgumentException>().And.Message.Contains("Invalid predicate"));
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
            Throws.TypeOf<ArgumentException>().And.Message.Contains("Request for argument at index 0 but no arguments are provided"));
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
        using var token = container.Items.SubscribeForNotifications((_, changes) =>
        {
            if (changes != null)
            {
                notifications.Add(changes);
            }
        });

        for (var i = 0; i < 5; i++)
        {
            _realm.Write(() => container.Items[i] = new IntPropertyObject { Int = i + 5 });
            _realm.Refresh();

            Assert.That(notifications.Count, Is.EqualTo(i + 1));
            Assert.That(notifications[i].ModifiedIndices, Is.EquivalentTo(new[] { i }));
            Assert.That(container.Items[i].Int, Is.EqualTo(i + 5));
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

    public readonly struct StringQueryNumericData
    {
        public readonly string PropertyName;
        public readonly bool ExpectedMatch;
        public readonly RealmValue ValueToAddToRealm;
        public readonly RealmValue ValueToQueryFor;

        public StringQueryNumericData(string propertyName, RealmValue valueToAddToDB, RealmValue valueToQueryFor, bool expectedMatch)
        {
            PropertyName = propertyName;
            ValueToAddToRealm = valueToAddToDB;
            ValueToQueryFor = valueToQueryFor;
            ExpectedMatch = expectedMatch;
        }

        public override string ToString() => $"{PropertyName}: '{ValueToAddToRealm}' should{(ExpectedMatch ? string.Empty : " NOT")} match '{ValueToQueryFor}': {ExpectedMatch}";
    }

    public readonly struct StringQueryTestData
    {
        public readonly string PropertyName;
        public readonly RealmValue MatchingValue;
        public readonly RealmValue NonMatchingValue;

        public StringQueryTestData(string propertyName, RealmValue matchingValue, RealmValue nonMatchingValue)
        {
            PropertyName = propertyName;
            MatchingValue = matchingValue;
            NonMatchingValue = nonMatchingValue;
        }

        public override string ToString() => $"{PropertyName}, match: '{MatchingValue}' non-match: '{NonMatchingValue}'";
    }

    private static object? BoxValue(RealmValue val, Type targetType)
    {
        var boxed = val.AsAny();
        if (boxed != null && targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(RealmInteger<>))
        {
            var wrappedType = targetType.GetGenericArguments().Single();
            boxed = Convert.ChangeType(boxed, wrappedType);
            return Activator.CreateInstance(targetType, BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { boxed }, null)!;
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
        var propInfo = typeof(AllTypesObject).GetProperty(data.PropertyName)!;
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
        var propInfo = typeof(AllTypesObject).GetProperty(data.PropertyName)!;
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
        var propInfoObjWithEmbedded = typeof(ObjectWithEmbeddedProperties).GetProperty("AllTypesObject")!;
        var propInfoEmbeddedAllTypes = typeof(EmbeddedAllTypesObject).GetProperty(data.PropertyName)!;

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
        var propInfo = typeof(AllTypesObject).GetProperty(data.PropertyName)!;
        var boxedMatch = BoxValue(data.MatchingValue, propInfo.PropertyType);

        _realm.Write(() =>
        {
            var matchingObj = _realm.Add(new AllTypesObject { RequiredStringProperty = string.Empty });
            propInfo.SetValue(matchingObj, boxedMatch);
        });

        var ex = Assert.Throws<ArgumentException>(() => _realm.All<AllTypesObject>().Filter($"{data.PropertyName} = $0", data.NonMatchingValue))!;
        Assert.That(ex.Message, Does.Contain($"Unsupported comparison"));
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

        QueryArgument[] argumentsArray = null!;
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
        Assert.That(objects.ToArray().All(o => o.B!.C!.Int < 5));
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

        void CallbackHandler(IRealmCollection<A> sender, ChangeSet? changes)
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
        Assert.That(objects.ToArray().All(o => o.Value && o.B!.C!.Int < 6));
    }

    [Test]
    public void Results_GetFiltered_AfterLINQ()
    {
        PopulateAObjects();

        var objects = _realm.All<A>().Where(o => o.Value);
        Assert.That(objects.Count(), Is.EqualTo(5));

        objects = objects.Filter("B.C.Int < 6");
        Assert.That(objects.Count(), Is.EqualTo(3));
        Assert.That(objects.ToArray().All(o => o.Value && o.B!.C!.Int < 6));
    }

    [Test]
    public void Results_GetFiltered_Sorted()
    {
        PopulateAObjects();
        var objects = _realm.All<A>().Filter("TRUEPREDICATE SORT(Value ASC, B.C.Int DESC)").AsRealmCollection();

        var expectedOrder = new[] { 9, 7, 5, 3, 1, 8, 6, 4, 2, 0 };
        for (var i = 0; i < 10; i++)
        {
            var obj = objects[i]!;
            Assert.That(obj.Value, Is.EqualTo(i >= 5));

            Assert.That(obj.B!.C!.Int, Is.EqualTo(expectedOrder[i]));
        }
    }

    [Test]
    public void Results_GetFiltered_Distinct()
    {
        PopulateAObjects();
        var objects = _realm.All<A>().Filter("TRUEPREDICATE SORT(Value ASC) DISTINCT(Value)").AsRealmCollection();

        Assert.That(objects.Count, Is.EqualTo(2));
        Assert.That(objects[0]!.Value, Is.False);
        Assert.That(objects[1]!.Value, Is.True);
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
            Throws.TypeOf<ArgumentException>().And.Message.Contains("'A' has no property 'Foo'"));
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
        Assert.That(obj.Realm!.IsFrozen, Is.False);

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
                var owner = _realm.Write(() => _realm.Add(new Owner
                {
                    ListOfDogs = { new Dog { Name = "Lasse" } }
                }));

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

        var query = _realm.All<Dog>().Where(d => d.Name!.StartsWith("B"));
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
                return new object[] { frozenQuery };
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

        var frozenQuery = Freeze(_realm.All<Dog>().Where(d => d.Name!.StartsWith("R")));
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

        var obj1 = _realm.Write(() => _realm.Add(new CollectionsObject()));

        var obj2 = realm2.Write(() => realm2.Add(new CollectionsObject()));

        _realm.Write(() =>
        {
            obj1.ObjectList.Add(item);
            obj1.EmbeddedObjectList.Add(embeddedItem);
        });

        Assert.That(() => realm2.Write(() => obj2.ObjectList.Add(item)), Throws.TypeOf<RealmException>().And.Message.Contains("object that is already in another realm"));
        Assert.That(() => realm2.Write(() => obj2.EmbeddedObjectList.Add(embeddedItem)), Throws.TypeOf<RealmException>().And.Message.Contains("embedded object that is already managed"));
    }

    [Test]
    public void Results_Filter_WithRemappedProperty()
    {
        _realm.Write(() =>
        {
            _realm.Add(new RemappedTypeObject
            {
                Id = 0,
                StringValue = "abc"
            });

            _realm.Add(new RemappedTypeObject
            {
                Id = 1,
                StringValue = "cde"
            });
        });

        // Id is mapped to _id - we validate that the query works both with Id and _id
        foreach (var columnName in new[] { "Id", "_id" })
        {
            var objects = _realm.All<RemappedTypeObject>().Filter($"{columnName} > 0");

            Assert.That(objects.Count(), Is.EqualTo(1));
            Assert.That(objects.Single().StringValue, Is.EqualTo("cde"));
        }
    }

    [Test]
    public void List_Filter_WithRemappedProperty()
    {
        var obj = _realm.Write(() => _realm.Add(new SyncCollectionsObject
        {
            ObjectList =
            {
                new() { Int = 5 },
                new() { Int = 10 },
            }
        }));

        var list = obj.ObjectList;

        // Id is mapped to _id - we validate that the query works both with Id and _id
        foreach (var columnName in new[] { "Id", "_id" })
        {
            var objects = list.Filter($"{columnName} = $0", list[0].Id);

            Assert.That(objects.Count(), Is.EqualTo(1));
            Assert.That(objects.Single().Int, Is.EqualTo(list[0].Int));
        }
    }

    [Test]
    public void Set_Filter_WithRemappedProperty()
    {
        var obj = _realm.Write(() => _realm.Add(new SyncCollectionsObject
        {
            ObjectSet =
            {
                new() { Int = 5 },
                new() { Int = 10 },
            }
        }));

        var set = obj.ObjectSet;

        // Id is mapped to _id - we validate that the query works both with Id and _id
        foreach (var columnName in new[] { "Id", "_id" })
        {
            var toMatch = set.First();
            var objects = set.Filter($"{columnName} = $0", toMatch.Id);

            Assert.That(objects.Count(), Is.EqualTo(1));
            Assert.That(objects.Single().Int, Is.EqualTo(toMatch.Int));
        }
    }

    [Test]
    public void Dict_Filter_WithRemappedProperty()
    {
        var obj = _realm.Write(() => _realm.Add(new SyncCollectionsObject
        {
            ObjectDict =
            {
                ["a"] = new() { Int = 5 },
                ["b"] = new() { Int = 10 },
            }
        }));

        var dict = obj.ObjectDict;

        // Id is mapped to _id - we validate that the query works both with Id and _id
        foreach (var columnName in new[] { "Id", "_id" })
        {
            var objects = dict.Filter($"{columnName} = $0", dict["a"]!.Id);

            Assert.That(objects.Count(), Is.EqualTo(1));
            Assert.That(objects.Single().Int, Is.EqualTo(dict["a"]!.Int));
        }
    }

    public readonly struct FtsTestData
    {
        public readonly string Query;
        public readonly string[] ExpectedResults;

        public FtsTestData(string query, params string[] expectedResults)
        {
            Query = query;
            ExpectedResults = expectedResults;
        }

        public override string ToString() => Query;
    }

    private static readonly object[] FtsTestCases =
    {
        new object[] { new FtsTestData("lord of the", LordOfTheFlies, LordOfTheRings, WheelOfTime, Silmarillion) },
        new object[] { new FtsTestData("fantasy novel", LordOfTheRings, WheelOfTime) },
        new object[] { new FtsTestData("popular english", LordOfTheFlies, Silmarillion) },
        new object[] { new FtsTestData("amazing awesome stuff") },
        new object[] { new FtsTestData("fantasy -novel", Silmarillion) },
    };

    [TestCaseSource(nameof(FtsTestCases))]
    public void Fts_Filter_SimpleTerm(FtsTestData testData)
    {
        PopulateFtsData();

        var summaryMatches = _realm.All<ObjectWithFtsIndex>().Filter($"{nameof(ObjectWithFtsIndex.Summary)} TEXT $0", testData.Query).ToArray().Select(o => o.Title);
        Assert.That(summaryMatches, Is.EquivalentTo(testData.ExpectedResults));

        var nullableSummaryMatches = _realm.All<ObjectWithFtsIndex>().Filter($"{nameof(ObjectWithFtsIndex.NullableSummary)} TEXT $0", testData.Query).ToArray().Select(o => o.Title);
        Assert.That(nullableSummaryMatches, Is.EquivalentTo(testData.ExpectedResults));
    }

    [TestCaseSource(nameof(FtsTestCases))]
    public void Fts_Linq_SimpleTerm(FtsTestData testData)
    {
        PopulateFtsData();

        var summaryMatches = _realm.All<ObjectWithFtsIndex>().Where(o => QueryMethods.FullTextSearch(o.Summary, testData.Query)).ToArray().Select(o => o.Title);
        Assert.That(summaryMatches, Is.EquivalentTo(testData.ExpectedResults));

        var nullableSummaryMatches = _realm.All<ObjectWithFtsIndex>().Where(o => QueryMethods.FullTextSearch(o.NullableSummary, testData.Query)).ToArray().Select(o => o.Title);
        Assert.That(nullableSummaryMatches, Is.EquivalentTo(testData.ExpectedResults));
    }

    [Test]
    public void Fts_Linq_WhenTermsIsNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _ = _realm.All<ObjectWithFtsIndex>().Where(o => QueryMethods.FullTextSearch(o.Summary, null!)).ToArray());
        Assert.Throws<ArgumentNullException>(() => _ = _realm.All<ObjectWithFtsIndex>().Where(o => QueryMethods.FullTextSearch(o.NullableSummary, null!)).ToArray());
    }

    [Test]
    public void Fts_OnNonIndexedProperty_Throws()
    {
        var ex = Assert.Throws<RealmException>(() => _ = _realm.All<ObjectWithFtsIndex>().Where(o => QueryMethods.FullTextSearch(o.Title, "value")).ToArray());
        Assert.That(ex!.Message, Does.Contain("Column has no fulltext index"));

        ex = Assert.Throws<RealmException>(() => _ = _realm.All<ObjectWithFtsIndex>().Filter($"{nameof(ObjectWithFtsIndex.Title)} TEXT $0", "value").ToArray());
        Assert.That(ex!.Message, Does.Contain("Column has no fulltext index"));
    }

    [Test]
    public void QueryArgument_ToString()
    {
        QueryArgument charArg = 'A';
        Assert.That(charArg.ToString(), Is.EqualTo(((int)'A').ToString()));

        QueryArgument boolArg = true;
        Assert.That(boolArg.ToString(), Is.EqualTo("True"));

        QueryArgument byteArg = (byte)5;
        Assert.That(byteArg.ToString(), Is.EqualTo("5"));

        QueryArgument shortArg = (short)10;
        Assert.That(shortArg.ToString(), Is.EqualTo("10"));

        QueryArgument intArg = -20;
        Assert.That(intArg.ToString(), Is.EqualTo("-20"));

        QueryArgument longArg = 9999999999999999;
        Assert.That(longArg.ToString(), Is.EqualTo("9999999999999999"));

        QueryArgument floatArg = 1.234f;
        Assert.That(floatArg.ToString(), Is.EqualTo(1.234f.ToString()));

        QueryArgument doubleArg = 4.5;
        Assert.That(doubleArg.ToString(), Is.EqualTo(4.5.ToString()));

        var date = new DateTimeOffset(2023, 10, 5, 2, 3, 4, TimeSpan.Zero);
        QueryArgument dateArg = date;
        Assert.That(dateArg.ToString(), Is.EqualTo(date.ToString()));

        QueryArgument decimalArg = 1.23456789123456789M;
        Assert.That(decimalArg.ToString(), Is.EqualTo(1.23456789123456789M.ToString(CultureInfo.InvariantCulture)));

        QueryArgument decimal128Arg = (Decimal128)10.20M;
        Assert.That(decimal128Arg.ToString(), Is.EqualTo(10.20M.ToString(CultureInfo.InvariantCulture)));

        QueryArgument objectIdArg = new ObjectId("507f1f77bcf86cd799439011");
        Assert.That(objectIdArg.ToString(), Is.EqualTo("507f1f77bcf86cd799439011"));

        QueryArgument guidArg = new Guid("E30AB544-67B5-42E2-80F7-C2D2E8E01B22");
        Assert.That(guidArg.ToString(), Is.EqualTo("e30ab544-67b5-42e2-80f7-c2d2e8e01b22"));

        QueryArgument nullableCharArg = (char?)'A';
        Assert.That(nullableCharArg.ToString(), Is.EqualTo(((int)'A').ToString()));

        QueryArgument nullableBoolArg = (bool?)true;
        Assert.That(nullableBoolArg.ToString(), Is.EqualTo("True"));

        QueryArgument nullableByteArg = (byte?)5;
        Assert.That(nullableByteArg.ToString(), Is.EqualTo("5"));

        QueryArgument nullableShortArg = (short?)10;
        Assert.That(nullableShortArg.ToString(), Is.EqualTo("10"));

        QueryArgument nullableIntArg = (int?)-20;
        Assert.That(nullableIntArg.ToString(), Is.EqualTo("-20"));

        QueryArgument nullableLongArg = (long?)9999999999999999;
        Assert.That(nullableLongArg.ToString(), Is.EqualTo("9999999999999999"));

        QueryArgument nullableFloatArg = (float?)1.234f;
        Assert.That(nullableFloatArg.ToString(), Is.EqualTo(1.234f.ToString()));

        QueryArgument nullableDoubleArg = (double?)4.5;
        Assert.That(nullableDoubleArg.ToString(), Is.EqualTo(4.5.ToString()));

        QueryArgument nullableDateArg = (DateTimeOffset?)date;
        Assert.That(nullableDateArg.ToString(), Is.EqualTo(date.ToString()));

        QueryArgument nullableDecimalArg = (decimal?)1.23456789123456789M;
        Assert.That(nullableDecimalArg.ToString(), Is.EqualTo(1.23456789123456789M.ToString(CultureInfo.InvariantCulture)));

        QueryArgument nullableDecimal128Arg = (Decimal128?)10.20M;
        Assert.That(nullableDecimal128Arg.ToString(), Is.EqualTo(10.20M.ToString(CultureInfo.InvariantCulture)));

        QueryArgument nullableObjectIdArg = (ObjectId?)new ObjectId("507f1f77bcf86cd799439011");
        Assert.That(nullableObjectIdArg.ToString(), Is.EqualTo("507f1f77bcf86cd799439011"));

        QueryArgument nullableGuidArg = (Guid?)new Guid("E30AB544-67B5-42E2-80F7-C2D2E8E01B22");
        Assert.That(nullableGuidArg.ToString(), Is.EqualTo("e30ab544-67b5-42e2-80f7-c2d2e8e01b22"));

        RealmInteger<byte> byteInteger = 5;
        QueryArgument byteIntegerArg = byteInteger;
        Assert.That(byteIntegerArg.ToString(), Is.EqualTo("5"));

        QueryArgument nullableByteIntegerArg = (RealmInteger<byte>?)byteInteger;
        Assert.That(nullableByteIntegerArg.ToString(), Is.EqualTo("5"));

        RealmInteger<short> shortInteger = 5;
        QueryArgument shortIntegerArg = shortInteger;
        Assert.That(shortIntegerArg.ToString(), Is.EqualTo("5"));

        QueryArgument nullableShortIntegerArg = (RealmInteger<short>?)shortInteger;
        Assert.That(nullableShortIntegerArg.ToString(), Is.EqualTo("5"));

        RealmInteger<int> intInteger = 5;
        QueryArgument intIntegerArg = intInteger;
        Assert.That(intIntegerArg.ToString(), Is.EqualTo("5"));

        QueryArgument nullableIntIntegerArg = (RealmInteger<int>?)intInteger;
        Assert.That(nullableIntIntegerArg.ToString(), Is.EqualTo("5"));

        RealmInteger<long> longInteger = 5;
        QueryArgument longIntegerArg = longInteger;
        Assert.That(longIntegerArg.ToString(), Is.EqualTo("5"));

        QueryArgument nullableLongIntegerArg = (RealmInteger<long>?)longInteger;
        Assert.That(nullableLongIntegerArg.ToString(), Is.EqualTo("5"));

        QueryArgument stringArg = "abc";
        Assert.That(stringArg.ToString(), Is.EqualTo("abc"));

        QueryArgument byteArrayArg = new byte[] { 1, 2, 3 };
        Assert.That(byteArrayArg.ToString(), Is.EqualTo("System.Byte[]"));

        QueryArgument objArg = (RealmObjectBase?)null;
        Assert.That(objArg.ToString(), Is.EqualTo("<null>"));

        Assert.That(stringArg.ToNative().Value.ToString(), Does.Contain("primitive String"));
    }

    [Test]
    public void RealmResults_ICollection_UnsupportedAPI()
    {
        var results = (ICollection<Person>)_realm.All<Person>();
        Assert.That(results.IsReadOnly);

        Assert.Throws<NotSupportedException>(() => results.Add(new Person()));
        Assert.Throws<NotSupportedException>(() => results.Remove(null!));
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

    private void PopulateFtsData()
    {
        _realm.Write(() =>
        {
            _realm.Add(new ObjectWithFtsIndex("N/A", string.Empty)
            {
                NullableSummary = null
            });
            _realm.Add(new ObjectWithFtsIndex(AnimalFarm, "Animal Farm is a beast fable, in the form of a satirical allegorical novella, by George Orwell, first published in England on 17 August 1945. It tells the story of a group of farm animals who rebel against their human farmer, hoping to create a society where the animals can be equal, free, and happy. Ultimately, the rebellion is betrayed, and under the dictatorship of a pig named Napoleon, the farm ends up in a state as bad as it was before. According to Orwell, Animal Farm reflects events leading up to the Russian Revolution of 1917 and then on into the Stalinist era of the Soviet Union. Orwell, a democratic socialist, was a critic of Joseph Stalin and hostile to Moscow-directed Stalinism, an attitude that was critically shaped by his experiences during the Barcelona May Days conflicts between the POUM and Stalinist forces during the Spanish Civil War. In a letter to Yvonne Davet, Orwell described Animal Farm as a satirical tale against Stalin (\"un conte satirique contre Staline\"), and in his essay \"Why I Write\" (1946), wrote that Animal Farm was the first book in which he tried, with full consciousness of what he was doing, \"to fuse political purpose and artistic purpose into one whole\". The original title was Animal Farm: A Fairy Story, but US publishers dropped the subtitle when it was published in 1946, and only one of the translations during Orwell's lifetime, the Telugu version, kept it. Other titular variations include subtitles like \"A Satire\" and \"A Contemporary Satire\". Orwell suggested the title Union des républiques socialistes animales for the French translation, which abbreviates to URSA, the Latin word for \"bear\", a symbol of Russia. It also played on the French name of the Soviet Union, Union des républiques socialistes soviétiques. Orwell wrote the book between November 1943 and February 1944, when the United Kingdom was in its wartime alliance with the Soviet Union against Nazi Germany, and the British intelligentsia held Stalin in high esteem, a phenomenon Orwell hated. The manuscript was initially rejected by several British and American publishers, including one of Orwell's own, Victor Gollancz, which delayed its publication. It became a great commercial success when it did appear partly because international relations were transformed as the wartime alliance gave way to the Cold War. Time magazine chose the book as one of the 100 best English-language novels (1923 to 2005); it also featured at number 31 on the Modern Library List of Best 20th-Century Novels, and number 46 on the BBC's The Big Read poll. It won a Retrospective Hugo Award in 1996 and is included in the Great Books of the Western World selection."));
            _realm.Add(new ObjectWithFtsIndex(LordOfTheRings, "The Lord of the Rings is an epic high-fantasy novel by English author and scholar J. R. R. Tolkien. Set in Middle-earth, the story began as a sequel to Tolkien's 1937 children's book The Hobbit, but eventually developed into a much larger work. Written in stages between 1937 and 1949, The Lord of the Rings is one of the best-selling books ever written, with over 150 million copies sold. The title refers to the story's main antagonist, the Dark Lord Sauron, who, in an earlier age, created the One Ring to rule the other Rings of Power given to Men, Dwarves, and Elves, in his campaign to conquer all of Middle-earth. From homely beginnings in the Shire, a hobbit land reminiscent of the English countryside, the story ranges across Middle-earth, following the quest to destroy the One Ring, seen mainly through the eyes of the hobbits Frodo, Sam, Merry and Pippin. Although often called a trilogy, the work was intended by Tolkien to be one volume of a two-volume set along with The Silmarillion. For economic reasons, The Lord of the Rings was published over the course of a year from 29 July 1954 to 20 October 1955 in three volumes titled The Fellowship of the Ring, The Two Towers, and The Return of the King. The work is divided internally into six books, two per volume, with several appendices of background material. Some later editions print the entire work in a single volume, following the author's original intent. Tolkien's work, after an initially mixed reception by the literary establishment, has been the subject of extensive analysis of its themes and origins. Influences on this earlier work, and on the story of The Lord of the Rings, include philology, mythology, Christianity, earlier fantasy works, and his own experiences in the First World War."));
            _realm.Add(new ObjectWithFtsIndex(LordOfTheFlies, "Lord of the Flies is a 1954 novel by the Nobel Prize-winning British author William Golding. The plot concerns a group of British boys who are stranded on an uninhabited island and their disastrous attempts to govern themselves. Themes include the tension between groupthink and individuality, between rational and emotional reactions, and between morality and immorality. The novel, which was Golding's debut, was generally well received. It was named in the Modern Library 100 Best Novels, reaching number 41 on the editor's list, and 25 on the reader's list. In 2003, it was listed at number 70 on the BBC's The Big Read poll, and in 2005 Time magazine named it as one of the 100 best English-language novels published between 1923 and 2005, and included it in its list of the 100 Best Young-Adult Books of All Time. Popular reading in schools, especially in the English-speaking world, Lord of the Flies was ranked third in the nation's favourite books from school in a 2016 UK poll."));
            _realm.Add(new ObjectWithFtsIndex(WheelOfTime, "The Wheel of Time is a series of high fantasy novels by American author Robert Jordan, with Brandon Sanderson as a co-author for the final three novels. Originally planned as a six-book series at its debut in 1990, The Wheel of Time came to span 14 volumes, in addition to a prequel novel and two companion books. Jordan died in 2007 while working on what was planned to be the final volume in the series. He prepared extensive notes which enabled fellow fantasy author Brandon Sanderson to complete the final book, which grew into three volumes: The Gathering Storm (2009), Towers of Midnight (2010), and A Memory of Light (2013). The series draws on numerous elements of both European and Asian mythology, most notably the cyclical nature of time found in Buddhism and Hinduism; the metaphysical concepts of balance, duality, and a respect for nature found in Taoism; the Abrahamic concepts of God and Satan; and Leo Tolstoy's War and Peace. The Wheel of Time is notable for its length, detailed imaginary world, and magic system, and its large cast of characters. The eighth through fourteenth books each reached number one on the New York Times Best Seller list. After its completion, the series was nominated for a Hugo Award. As of 2021, the series has sold over 90 million copies worldwide, making it one of the best-selling epic fantasy series since The Lord of the Rings. Its popularity has spawned a collectible card game, a video game, a roleplaying game, and a soundtrack album. A TV series adaptation produced by Sony Pictures and Amazon Studios premiered in 2021.The Wheel of Time is a series of high fantasy novels by American author Robert Jordan, with Brandon Sanderson as a co-author for the final three novels. Originally planned as a six-book series at its debut in 1990, The Wheel of Time came to span 14 volumes, in addition to a prequel novel and two companion books. Jordan died in 2007 while working on what was planned to be the final volume in the series. He prepared extensive notes which enabled fellow fantasy author Brandon Sanderson to complete the final book, which grew into three volumes: The Gathering Storm (2009), Towers of Midnight (2010), and A Memory of Light (2013). The series draws on numerous elements of both European and Asian mythology, most notably the cyclical nature of time found in Buddhism and Hinduism; the metaphysical concepts of balance, duality, and a respect for nature found in Taoism; the Abrahamic concepts of God and Satan; and Leo Tolstoy's War and Peace. The Wheel of Time is notable for its length, detailed imaginary world, and magic system, and its large cast of characters. The eighth through fourteenth books each reached number one on the New York Times Best Seller list. After its completion, the series was nominated for a Hugo Award. As of 2021, the series has sold over 90 million copies worldwide, making it one of the best-selling epic fantasy series since The Lord of the Rings. Its popularity has spawned a collectible card game, a video game, a roleplaying game, and a soundtrack album. A TV series adaptation produced by Sony Pictures and Amazon Studios premiered in 2021. "));
            _realm.Add(new ObjectWithFtsIndex(Silmarillion, "The Silmarillion (Quenya: [silmaˈrilliɔn]) is a collection of myths and stories in varying styles by the English writer J. R. R. Tolkien. It was edited and published posthumously by his son Christopher Tolkien in 1977, assisted by the fantasy author Guy Gavriel Kay. It tells of Eä, a fictional universe that includes the Blessed Realm of Valinor, the once-great region of Beleriand, the sunken island of Númenor, and the continent of Middle-earth, where Tolkien's most popular works—The Hobbit and The Lord of the Rings—are set. The Silmarillion has five parts. The first, Ainulindalë, tells in mythic style of the creation of Eä, the \"world that is.\" The second part, Valaquenta, gives a description of the Valar and Maiar, supernatural powers of Eä. The next section, Quenta Silmarillion, which forms the bulk of the collection, chronicles the history of the events before and during the First Age, including the wars over three jewels, the Silmarils, that gave the book its title. The fourth part, Akallabêth, relates the history of the Downfall of Númenor and its people, which takes place in the Second Age. The final part, Of the Rings of Power and the Third Age, is a brief summary of the events of The Lord of the Rings and those that led to them. The book shows the influence of many sources, including the Finnish epic Kalevala, Greek mythology in the lost island of Atlantis (as Númenor) and the Olympian gods (in the shape of the Valar, though these also resemble the Norse Æsir)."));
        });
    }
}

public partial class A : TestRealmObject
{
    public bool Value { get; set; }

    public B? B { get; set; }
}

public partial class B : TestRealmObject
{
    public IntPropertyObject? C { get; set; }
}