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

// NOTE some of the following data comes from Tim's data used in the Browser screenshot in the Mac app store
// unlike the Cocoa definitions, we use Pascal casing for properties
namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class RelationshipTests : RealmInstanceTest
    {
        protected override void CustomSetUp()
        {
            base.CustomSetUp();
            _realm.Write(() =>
            {
                var o1 = _realm.Add(new Owner { Name = "Tim" });

                var d1 = _realm.Add(new Dog
                {
                    Name = "Bilbo Fleabaggins",
                    Color = "Black"
                });

                o1.TopDog = d1;  // set a one-one relationship
                o1.Dogs.Add(d1);

                var d2 = _realm.Add(new Dog
                {
                    Name = "Earl Yippington III",
                    Color = "White"
                });

                o1.Dogs.Add(d2);

                // lonely people and dogs
                _realm.Add(new Owner
                {
                    Name = "Dani" // the dog-less
                });

                _realm.Add(new Dog // will remain unassigned
                {
                    Name = "Maggie Mongrel",
                    Color = "Grey"
                });
            });
        }

        [Test]
        public void TimHasATopDog()
        {
            var tim = _realm.All<Owner>().First(p => p.Name == "Tim");
            Assert.That(tim.TopDog.Name, Is.EqualTo("Bilbo Fleabaggins"));
        }

        [Test]
        public void TimHasTwoIterableDogs()
        {
            var tim = _realm.All<Owner>().First(p => p.Name == "Tim");
            var dogNames = new List<string>();

            // using foreach here is deliberately testing that syntax
            foreach (var dog in tim.Dogs)
            {
                dogNames.Add(dog.Name);
            }

            Assert.That(dogNames, Is.EquivalentTo(new List<string> { "Bilbo Fleabaggins", "Earl Yippington III" }));
        }

        /// <summary>
        /// Check if ToList can be invoked on a related RealmResults.
        /// </summary>
        [Test]
        public void TimHasTwoIterableDogsListed()
        {
            var tim = _realm.All<Owner>().First(p => p.Name == "Tim");
            var dogNames = new List<string>();
            var dogList = tim.Dogs.ToList();  // this used to crash - issue 299
            foreach (var dog in dogList)
            {
                dogNames.Add(dog.Name);
            }

            Assert.That(dogNames, Is.EquivalentTo(new List<string> { "Bilbo Fleabaggins", "Earl Yippington III" }));
        }

        [Test]
        public void TimsIterableDogsThrowExceptions()
        {
            var tim = _realm.All<Owner>().First(p => p.Name == "Tim");
            Assert.That(() => tim.Dogs.CopyTo(null, 0), Throws.TypeOf<ArgumentNullException>());
            var copiedDogs = new Dog[2];
            Assert.That(() => tim.Dogs.CopyTo(copiedDogs, -1), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => tim.Dogs.CopyTo(copiedDogs, 1), Throws.TypeOf<ArgumentException>()); // insuffiient room
        }

        [Test]
        public void TimRetiredHisTopDog()
        {
            var tim = _realm.All<Owner>().First(p => p.Name == "Tim");
            using (var trans = _realm.BeginWrite())
            {
                tim.TopDog = null;
                trans.Commit();
            }

            var tim2 = _realm.All<Owner>().First(p => p.Name == "Tim");
            Assert.That(tim2.TopDog, Is.Null);  // the dog departure was saved
        }

        [Test]
        public void TimAddsADogLater()
        {
            var tim = _realm.All<Owner>().First(p => p.Name == "Tim");
            Assert.That(tim.Dogs.Count, Is.EqualTo(2));
            using (var trans = _realm.BeginWrite())
            {
                var dog3 = _realm.All<Dog>().Where(p => p.Name == "Maggie Mongrel").First();
                tim.Dogs.Add(dog3);
                trans.Commit();
            }

            var tim2 = _realm.All<Owner>().First(p => p.Name == "Tim");
            Assert.That(tim2.Dogs.Count, Is.EqualTo(3));
            Assert.That(tim2.Dogs[2].Name, Is.EqualTo("Maggie Mongrel"));
        }

        [Test]
        public void TimAddsADogByInsert()
        {
            var tim = _realm.All<Owner>().Single(p => p.Name == "Tim");  // use Single for a change
            Assert.That(tim.Dogs.Count, Is.EqualTo(2));
            using (var trans = _realm.BeginWrite())
            {
                var dog3 = _realm.All<Dog>().First(p => p.Name == "Maggie Mongrel");
                tim.Dogs.Insert(1, dog3);
                trans.Commit();
            }

            var tim2 = _realm.All<Owner>().Single(p => p.Name == "Tim");
            Assert.That(tim2.Dogs.Count, Is.EqualTo(3));
            Assert.That(tim2.Dogs[1].Name, Is.EqualTo("Maggie Mongrel"));
            Assert.That(tim2.Dogs[2].Name, Is.EqualTo("Earl Yippington III"));
        }

        [Test]
        public void TimLosesHisDogsByOrder()
        {
            var tim = _realm.All<Owner>().Single(p => p.Name == "Tim");
            Assert.That(tim.Dogs.Count, Is.EqualTo(2));
            using (var trans = _realm.BeginWrite())
            {
                tim.Dogs.RemoveAt(0);
                trans.Commit();
            }

            var tim2 = _realm.All<Owner>().Single(p => p.Name == "Tim");
            Assert.That(tim2.Dogs.Count, Is.EqualTo(1));
            Assert.That(tim2.Dogs[0].Name, Is.EqualTo("Earl Yippington III"));
            using (var trans = _realm.BeginWrite())
            {
                tim.Dogs.RemoveAt(0);
                trans.Commit();
            }

            var tim3 = _realm.All<Owner>().Single(p => p.Name == "Tim");
            Assert.That(tim2.Dogs.Count, Is.EqualTo(0));
            Assert.That(tim3.Dogs.Count, Is.EqualTo(0)); // reloaded object has same empty related set
        }

        [Test]
        public void TimLosesHisDogsInOneClear()
        {
            var tim = _realm.All<Owner>().Single(p => p.Name == "Tim");
            Assert.That(tim.Dogs.Count, Is.EqualTo(2));
            using (var trans = _realm.BeginWrite())
            {
                tim.Dogs.Clear();
                trans.Commit();
            }

            var tim2 = _realm.All<Owner>().Single(p => p.Name == "Tim");
            Assert.That(tim2.Dogs.Count, Is.EqualTo(0));
        }

        [Test]
        public void TimLosesBilbo()
        {
            var bilbo = _realm.All<Dog>().First(p => p.Name == "Bilbo Fleabaggins");
            var tim = _realm.All<Owner>().Single(p => p.Name == "Tim");
            Assert.That(tim.Dogs.Count, Is.EqualTo(2));
            using (var trans = _realm.BeginWrite())
            {
                tim.Dogs.Remove(bilbo);
                trans.Commit();
            }

            var tim2 = _realm.All<Owner>().Single(p => p.Name == "Tim");
            Assert.That(tim2.Dogs.Count, Is.EqualTo(1));
            Assert.That(tim2.Dogs[0].Name, Is.EqualTo("Earl Yippington III"));
        }

        [Test]
        public void DaniHasNoTopDog()
        {
            var dani = _realm.All<Owner>().Where(p => p.Name == "Dani").First();
            Assert.That(dani.TopDog, Is.Null);
        }

        [Test]
        public void DaniHasNoDogs()
        {
            var dani = _realm.All<Owner>().Where(p => p.Name == "Dani").Single();
            Assert.That(dani.Dogs.Count, Is.EqualTo(0));  // ToMany relationships always return a _realmList
            var dogsIterated = 0;
            foreach (var d in dani.Dogs)
            {
                dogsIterated++;
            }

            Assert.That(dogsIterated, Is.EqualTo(0));
        }

        [Test]
        public void TestExceptionsFromEmptyListOutOfRange()
        {
            var dani = _realm.All<Owner>().Where(p => p.Name == "Dani").First();
            Assert.That(() => dani.Dogs.RemoveAt(0), Throws.TypeOf<ArgumentOutOfRangeException>());
            var bilbo = _realm.All<Dog>().Single(p => p.Name == "Bilbo Fleabaggins");
            Dog scratch;  // for assignment in following getters
            Assert.That(() => dani.Dogs.Insert(-1, bilbo), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => dani.Dogs.Insert(1, bilbo), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => scratch = dani.Dogs[0], Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void TestExceptionsFromIteratingEmptyList()
        {
            var dani = _realm.All<Owner>().Where(p => p.Name == "Dani").Single();
            var iter = dani.Dogs.GetEnumerator();
            Assert.IsNotNull(iter);
            var movedOnToFirstItem = iter.MoveNext();
            Assert.That(movedOnToFirstItem, Is.False);
            Dog currentDog;
            Assert.That(() => currentDog = iter.Current, Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void TestExceptionsFromTimsDogsOutOfRange()
        {
            var tim = _realm.All<Owner>().Single(p => p.Name == "Tim");
            Assert.That(() => tim.Dogs.RemoveAt(4), Throws.TypeOf<ArgumentOutOfRangeException>());
            var bilbo = _realm.All<Dog>().Single(p => p.Name == "Bilbo Fleabaggins");
            Dog scratch;  // for assignment in following getters
            Assert.That(() => tim.Dogs.Insert(-1, bilbo), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => tim.Dogs.Insert(3, bilbo), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => scratch = tim.Dogs[99], Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void TestSettingStandAloneObjectToRelationship()
        {
            var owner = _realm.All<Owner>().First();
            var dog = new Dog { Name = "Astro" };

            using (var trans = _realm.BeginWrite())
            {
                owner.TopDog = dog;
                trans.Commit();
            }

            var dogAgain = _realm.All<Dog>().Single(d => d.Name == "Astro");
            Assert.That(dogAgain, Is.Not.Null);
            Assert.That(dog.IsManaged);
        }

        [Test]
        public void TestAddingStandAloneObjectToToManyRelationship()
        {
            var owner = _realm.All<Owner>().First();
            var dog = new Dog { Name = "Astro" };

            using (var trans = _realm.BeginWrite())
            {
                owner.Dogs.Add(dog);
                trans.Commit();
            }

            var dogAgain = _realm.All<Dog>().Single(d => d.Name == "Astro");
            Assert.That(dogAgain, Is.Not.Null);
            Assert.That(dog.IsManaged);
        }

        [Test]
        public void TestManagingStandaloneTwoLevelRelationship()
        {
            var person = new Person
            {
                FullName = "Jack Thorne",
                Friends = // see NoteOnListInit above
                {
                    new Person { FullName = "Christian Molehound" },
                    new Person { FullName = "Frederick Van Whatnot" }
                }
            };

            Assert.That(person.Friends is List<Person>);

            using (var trans = _realm.BeginWrite())
            {
                _realm.Add(person);
                trans.Commit();
            }

            Assert.That(person.Friends is RealmList<Person>);
            Assert.That(_realm.All<Person>().ToList().Select(p => p.FirstName),
                        Is.EquivalentTo(new[] { "Jack", "Christian", "Frederick" }));
        }

        [Test]
        public void TestManagingStandaloneThreeLevelRelationship()
        {
            var sally = new Person
            {
                FullName = "Sally",
                Friends = // see NoteOnListInit above
                {
                    new Person { FullName = "Alice" },
                    new Person
                    {
                        FullName = "Joan",
                        Friends = // see NoteOnListInit above
                        {
                            new Person
                            {
                                FullName = "Krystal",
                                Friends = { new Person { FullName = "Sally" } } // Manages a second Sally
                            }
                        }
                    }
                }
            };

            using (var trans = _realm.BeginWrite())
            {
                _realm.Add(sally); // top person Manages entire tree
                trans.Commit();
            }

            Assert.That(_realm.All<Person>().ToList().Select(p => p.FirstName),
                        Is.EquivalentTo(new[] { "Alice", "Joan", "Krystal", "Sally", "Sally" }));
        }

        [Test]
        public void TestCircularRelationshipsFromStandaloneTwoStage()
        {
            var sally = new Person
            {
                FullName = "Sally",
                Friends =
                {
                    new Person { FullName = "Alice" },
                    new Person { FullName = "Joan" }
                }
            };
            var joanFriend = new Person
            {
                FullName = "Krystal",
                Friends = { sally }
            };

            using (var trans = _realm.BeginWrite())
            {
                _realm.Add(sally); // top person Manages entire tree
                sally.Friends[1].Friends.Add(joanFriend);

                trans.Commit();
            }

            Assert.That(_realm.All<Person>().ToList().Select(p => p.FirstName),
                        Is.EquivalentTo(new[] { "Alice", "Joan", "Krystal", "Sally" }));
        }

        [Test]
        public void Backlinks_SanityCheck()
        {
            var tim = _realm.All<Owner>().Single(o => o.Name == "Tim");
            foreach (var dog in tim.Dogs)
            {
                Assert.That(dog.Owners, Is.EquivalentTo(new[] { tim }));
            }

            var dani = _realm.All<Owner>().Single(o => o.Name == "Dani");
            var maggie = _realm.All<Dog>().Single(d => d.Name == "Maggie Mongrel");
            Assert.That(maggie.Owners, Is.Empty);

            _realm.Write(() => dani.Dogs.Add(maggie));
            Assert.That(maggie.Owners, Is.EquivalentTo(new[] { dani }));
        }

        [Test]
        public void Backlinks_WhenTargetsDifferentClass_ShouldReturnOnlyRelatedData()
        {
            var john = new Owner
            {
                Name = "John"
            };

            var doggy = new Dog
            {
                Name = "Doggy"
            };

            _realm.Write(() =>
            {
                _realm.Add(doggy);
                _realm.Add(john);
            });

            // We check both Count() and FirstOrDefault() due to a bug we had with queries
            Assert.That(doggy.Owners.Count(), Is.EqualTo(0));
            Assert.That(doggy.Owners.FirstOrDefault(), Is.Null);

            var sally = new Owner
            {
                Name = "Sally"
            };

            _realm.Write(() =>
            {
                _realm.Add(sally);

                john.Dogs.Add(doggy);
            });

            Assert.That(doggy.Owners.Count(), Is.EqualTo(1));
            Assert.That(doggy.Owners.Single(), Is.EqualTo(john));

            _realm.Write(() =>
            {
                sally.Dogs.Add(doggy);
            });

            Assert.That(doggy.Owners.Count(), Is.EqualTo(2));

            var alphabeticalOwners = doggy.Owners.OrderBy(o => o.Name);
            var reverseAlphabeticalOwners = doggy.Owners.OrderByDescending(o => o.Name);

            Assert.That(alphabeticalOwners.ElementAt(0), Is.EqualTo(john));
            Assert.That(reverseAlphabeticalOwners.ElementAt(1), Is.EqualTo(john));

            Assert.That(reverseAlphabeticalOwners.ElementAt(0), Is.EqualTo(sally));
            Assert.That(alphabeticalOwners.ElementAt(1), Is.EqualTo(sally));
        }

        [Test]
        public void Backlinks_WhenTargetsSameClass_ShouldReturnOnlyRelatedData()
        {
            var child1 = new RecursiveBacklinksObject
            {
                Id = 1
            };

            var parent = new RecursiveBacklinksObject
            {
                Id = 100
            };

            _realm.Write(() =>
            {
                _realm.Add(child1);
                _realm.Add(parent);
            });

            Assert.That(parent.Children.Count(), Is.EqualTo(0));
            Assert.That(parent.Children.FirstOrDefault(), Is.Null);

            var child2 = new RecursiveBacklinksObject
            {
                Id = 2
            };

            _realm.Write(() =>
            {
                _realm.Add(child2);

                child1.Parent = parent;
            });

            Assert.That(parent.Children.Count(), Is.EqualTo(1));
            Assert.That(parent.Children.Single(), Is.EqualTo(child1));

            _realm.Write(() =>
            {
                child2.Parent = parent;
            });

            var orderedChildren = parent.Children.OrderBy(o => o.Id);
            var reverseOrderedChildren = parent.Children.OrderByDescending(o => o.Id);

            Assert.That(orderedChildren.ElementAt(0), Is.EqualTo(child1));
            Assert.That(reverseOrderedChildren.ElementAt(1), Is.EqualTo(child1));

            Assert.That(reverseOrderedChildren.ElementAt(0), Is.EqualTo(child2));
            Assert.That(orderedChildren.ElementAt(1), Is.EqualTo(child2));
        }

        [Test]
        public void GetBacklinkCount_WhenUnmanaged_ReturnsZero()
        {
            var owner = new Owner
            {
                TopDog = new Dog(),
            };

            owner.Dogs.Add(new Dog());
            Assert.That(owner.BacklinksCount, Is.EqualTo(0));
            Assert.That(owner.TopDog.BacklinksCount, Is.EqualTo(0));
            Assert.That(owner.Dogs[0].BacklinksCount, Is.EqualTo(0));
        }

        [Test]
        public void GetBacklinkCount_WhenReferredByList()
        {
            var doggo = new Dog();
            _realm.Write(() => _realm.Add(doggo));

            // Nobody refers to doggo yet
            Assert.That(doggo.BacklinksCount, Is.EqualTo(0));

            var firstOwner = new Owner();
            firstOwner.Dogs.Add(doggo);
            _realm.Write(() => _realm.Add(firstOwner));

            // Just firstOwner
            Assert.That(doggo.BacklinksCount, Is.EqualTo(1));

            var secondOwner = new Owner();
            secondOwner.Dogs.Add(doggo);
            _realm.Write(() => _realm.Add(secondOwner));

            // firstOwner and secondOwner
            Assert.That(doggo.BacklinksCount, Is.EqualTo(2));

            _realm.Write(() => _realm.Remove(secondOwner));

            // Just firstOwner
            Assert.That(doggo.BacklinksCount, Is.EqualTo(1));

            _realm.Write(() => firstOwner.Dogs.Remove(doggo));

            // Nobody refers to doggo anymore
            Assert.That(doggo.BacklinksCount, Is.EqualTo(0));
        }

        [Test]
        public void GetBacklinkCount_WhenReferredByObject()
        {
            var doggo = new Dog();
            _realm.Write(() => _realm.Add(doggo));

            // Nobody refers to doggo yet
            Assert.That(doggo.BacklinksCount, Is.EqualTo(0));

            var firstOwner = new Owner
            {
                TopDog = doggo
            };
            _realm.Write(() => _realm.Add(firstOwner));

            // Just firstOwner
            Assert.That(doggo.BacklinksCount, Is.EqualTo(1));

            var secondOwner = new Owner
            {
                TopDog = doggo
            };
            _realm.Write(() => _realm.Add(secondOwner));

            // firstOwner and secondOwner
            Assert.That(doggo.BacklinksCount, Is.EqualTo(2));

            _realm.Write(() => _realm.Remove(secondOwner));

            // Just firstOwner
            Assert.That(doggo.BacklinksCount, Is.EqualTo(1));

            _realm.Write(() => firstOwner.TopDog = null);

            // Nobody refers to doggo anymore
            Assert.That(doggo.BacklinksCount, Is.EqualTo(0));
        }

        [Test]
        public void GetBacklinkCount_WhenReferredByObjectAndList()
        {
            var doggo = new Dog();
            _realm.Write(() => _realm.Add(doggo));

            // Nobody refers to doggo yet
            Assert.That(doggo.BacklinksCount, Is.EqualTo(0));

            var firstOwner = new Owner
            {
                TopDog = doggo
            };
            firstOwner.Dogs.Add(doggo);

            _realm.Write(() => _realm.Add(firstOwner));

            // FirstOwner via TopDog and Dogs
            Assert.That(doggo.BacklinksCount, Is.EqualTo(2));

            var secondOwner = new Owner
            {
                TopDog = doggo
            };
            _realm.Write(() => _realm.Add(secondOwner));

            // firstOwner via TopDog and Dogs and secondOwner via TopDog
            Assert.That(doggo.BacklinksCount, Is.EqualTo(3));

            _realm.Write(() => secondOwner.Dogs.Add(doggo));

            // firstOwner via TopDog and Dogs and secondOwner via TopDog and Dogs
            Assert.That(doggo.BacklinksCount, Is.EqualTo(4));

            _realm.Write(() => _realm.Remove(firstOwner));

            // secondOwner via TopDog and Dogs
            Assert.That(doggo.BacklinksCount, Is.EqualTo(2));

            _realm.Write(() => secondOwner.TopDog = null);

            // secondOwner via Dogs
            Assert.That(doggo.BacklinksCount, Is.EqualTo(1));

            _realm.Write(() => secondOwner.Dogs.Clear());

            // Nobody refers to doggo anymore
            Assert.That(doggo.BacklinksCount, Is.EqualTo(0));
        }

        [Test]
        public void GetBacklinkCount_WhenReferredByDifferentObjectTypes()
        {
            var doggo = new Dog();
            _realm.Write(() => _realm.Add(doggo));

            // Nobody refers to doggo yet
            Assert.That(doggo.BacklinksCount, Is.EqualTo(0));

            var owner = new Owner
            {
                TopDog = doggo
            };
            owner.Dogs.Add(doggo);

            _realm.Write(() => _realm.Add(owner));

            // owner via TopDog and Dogs
            Assert.That(doggo.BacklinksCount, Is.EqualTo(2));

            var walker = new Walker
            {
                TopDog = doggo
            };
            _realm.Write(() => _realm.Add(walker));

            // owner via TopDog and Dogs and walker via TopDog
            Assert.That(doggo.BacklinksCount, Is.EqualTo(3));

            _realm.Write(() => walker.Dogs.Add(doggo));

            // owner via TopDog and Dogs and walker via TopDog and Dogs
            Assert.That(doggo.BacklinksCount, Is.EqualTo(4));

            _realm.Write(() => _realm.Remove(owner));

            // walker via TopDog and Dogs
            Assert.That(doggo.BacklinksCount, Is.EqualTo(2));

            _realm.Write(() => walker.TopDog = null);

            // walker via Dogs
            Assert.That(doggo.BacklinksCount, Is.EqualTo(1));

            _realm.Write(() => walker.Dogs.Clear());

            // Nobody refers to doggo anymore
            Assert.That(doggo.BacklinksCount, Is.EqualTo(0));
        }

        #region DeleteRelated

        // from http://stackoverflow.com/questions/37819634/best-method-to-remove-managed-child-lists-one-to-many-parent-child-relationsh
        // shows a workaround for our lack of cascading delete
        public class Product : RealmObject
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public string Date { get; set; }

            public IList<Report> Reports { get; } // child objects
        }

        public class Report : RealmObject
        {
            public int Id { get; set; }

            public string Ref { get; set; }

            public string Date { get; set; }

            public Product Parent { get; set; } // Parent object reference
        }

        [Test]
        public void TestDeleteChildren()
        {
            // Arrange - setup some hierarchies
            _realm.Write(() =>
            {
                for (var pid = 1; pid <= 4; ++pid)
                {
                    var p = _realm.Add(new Product
                    {
                        Id = pid,
                        Name = $"Product {pid}"
                    });

                    for (var rid = 1; rid <= 5; ++rid)
                    {
                        var r = _realm.Add(new Report
                        {
                            Id = rid + (pid * 1000),
                            Ref = $"Report {pid}:{rid}"
                        });

                        p.Reports.Add(r);
                    }
                }
            });

            var delId = 1;
            var delP = _realm.All<Product>().First(p => p.Id == delId);
            Assert.IsNotNull(delP);
            Assert.That(delP.Reports.Count, Is.EqualTo(5));

            _realm.Write(() =>
            {
                // use ToList to get static list so can remove items
                foreach (var r in delP.Reports.ToList())
                {
                    _realm.Remove(r); // removes from the _realm, and updates delP.Reports so can't just iterate that
                }
            });

            Assert.That(delP.Reports.Count, Is.EqualTo(0));
        }

        #endregion
    }
}