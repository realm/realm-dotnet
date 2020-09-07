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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Realms.Exceptions;

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

            using (var readonlyRealm = Realm.GetInstance(config))
            {
                var readonlyContainer = readonlyRealm.All<ContainerObject>().Single();
                Assert.That(readonlyContainer.Items.IsReadOnly);
            }
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

            Assert.That(() => owner.Dogs.AsRealmCollection()[-1], Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => owner.Dogs.AsRealmCollection()[0], Throws.TypeOf<ArgumentOutOfRangeException>());
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
            Assert.That(items, Is.EqualTo(Enumerable.Range(2, 10)));
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
            Assert.That(items, Is.EqualTo(Enumerable.Range(0, 10)));
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
            var token = container.Items.SubscribeForNotifications((sender, changes, error) =>
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
                        Dogs =
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

            var owners = _realm.All<Owner>().Filter("Dogs.Vaccinated == true");
            Assert.That(owners.Count(), Is.EqualTo(4));
            Assert.That(owners.ToArray().All(o => o.Dogs.Any(d => d.Vaccinated)));
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
                        Dogs = { dog }
                    });

                    _realm.Add(new Owner
                    {
                        Name = $"Person {((2 * i) + 1) % 5}",
                        Dogs = { dog }
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
                        Dogs = { dog }
                    });

                    _realm.Add(new Owner
                    {
                        Name = $"Person {((2 * i) + 1) % 5}",
                        Dogs = { dog }
                    });
                }
            });

            var dogs = _realm.All<Dog>().Filter("@links.Owner.Dogs.Name == 'Person 0'");
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

            void CallbackHandler(IRealmCollection<A> sender, ChangeSet changes, Exception error)
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
        public void Results_GetFiltered_WhenPredicateIsInvalid_Throws()
        {
            Assert.That(
                () => _realm.All<A>().Filter("Foo == 5"),
                Throws.TypeOf<RealmException>().And.Message.Contains("No property 'Foo' on object of type 'A'"));
        }

        [Test]
        public void List_IndexOf_WhenObjectBelongsToADifferentRealm_ShouldThrow()
        {
            var config = new RealmConfiguration(Path.GetTempFileName());
            try
            {
                var owner = new Owner();
                _realm.Write(() =>
                {
                    _realm.Add(owner);
                });

                using (var otherRealm = Realm.GetInstance(config))
                {
                    var otherRealmDog = new Dog();
                    otherRealm.Write(() =>
                    {
                        otherRealm.Add(otherRealmDog);
                    });

                    Assert.That(() => owner.Dogs.IndexOf(otherRealmDog), Throws.InstanceOf<RealmObjectManagedByAnotherRealmException>());
                }
            }
            finally
            {
                Realm.DeleteRealm(config);
            }
        }

        [Test]
        public void List_Freeze_ReturnsAFrozenCopy()
        {
            var obj = new Owner
            {
                Name = "Peter",
                Dogs =
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

            var frozenDogs = Freeze(obj.Dogs);

            Assert.That(obj.IsManaged);
            Assert.That(obj.IsValid);
            Assert.That(obj.IsFrozen, Is.False);
            Assert.That(obj.Dogs.AsRealmCollection().IsFrozen, Is.False);
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
                WeakReference listRef = null;
                new Action(() =>
                {
                    var owner = new Owner
                    {
                        Dogs = { new Dog { Name = "Lasse" } }
                    };
                    _realm.Write(() =>
                    {
                        _realm.Add(owner);
                    });

                    listRef = new WeakReference(owner.Dogs.Freeze());
                })();

                while (listRef.IsAlive)
                {
                    await Task.Yield();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }

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
                WeakReference queryRef = null;
                new Action(() =>
                {
                    _realm.Write(() =>
                    {
                        _realm.Add(new Dog { Name = "Lasse" });
                    });

                    queryRef = new WeakReference(_realm.All<Dog>());
                })();

                while (queryRef.IsAlive)
                {
                    await Task.Yield();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }

                // This will throw on Windows if the Realm object wasn't really GC-ed and its Realm - closed
                Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);
            });
        }

        [Test]
        public void List_Freeze_WhenUnmanaged_Throws()
        {
            var list = new List<Owner>();
            Assert.Throws<RealmException>(() => list.Freeze(), "Unmanaged lists cannot be frozen.");
        }

        [Test]
        public void Query_Freeze_WhenUnmanaged_Throws()
        {
            var query = new List<Owner>().AsQueryable();
            Assert.Throws<RealmException>(() => query.Freeze(), "Unmanaged queries cannot be frozen.");
        }

        [Test]
        public void List_Freeze_WhenFrozen_ReturnsSameInstance()
        {
            var obj = new Owner();
            _realm.Write(() =>
            {
                _realm.Add(obj);
            });

            var frozenList = Freeze(obj.Dogs);
            Assert.That(ReferenceEquals(frozenList, obj.Dogs), Is.False);

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
                Dogs =
                {
                    new Dog { Name = "Rex" },
                    new Dog { Name = "Luthor" }
                }
            };

            _realm.Write(() =>
            {
                _realm.Add(obj);
            });

            var frozenList = obj.Dogs.Freeze();
            Assert.That(frozenList.Count, Is.EqualTo(2));

            _realm.Write(() =>
            {
                obj.Dogs[0].Name = "Lex";
                obj.Dogs.Insert(1, new Dog());
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

            var frozenQuery = _realm.All<Dog>().OrderBy(d => d.Name).Freeze();
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

            var frozenQuery = _realm.All<Dog>().Filter("TRUEPREDICATE SORT(Name ASC)").Freeze();
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

            var frozenQuery = _realm.All<Dog>().Where(d => d.Name.StartsWith("R")).Freeze();
            Assert.That(frozenQuery.Count(), Is.EqualTo(2));

            _realm.Write(() =>
            {
                _realm.Add(new Dog { Name = "Randy" });
            });

            Assert.That(frozenQuery.Count(), Is.EqualTo(2));
            Assert.That(frozenQuery.ToArray().FirstOrDefault(d => d.Name == "Randy"), Is.Null);
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

        private class A : RealmObject
        {
            public bool Value { get; set; }

            public B B { get; set; }
        }

        private class B : RealmObject
        {
            public IntPropertyObject C { get; set; }
        }
    }
}
