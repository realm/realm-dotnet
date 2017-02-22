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
using Realms;

// NOTE some of the following data comes from Tim's data used in the Browser screenshot in the Mac app store
// unlike the Cocoa definitions, we use Pascal casing for properties
namespace IntegrationTests
{
    [TestFixture, Preserve(AllMembers = true)]
    public class RelationshipTests
    {
        protected Realm realm;

        [SetUp]
        public void SetUp()
        {
            Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);
            realm = Realm.GetInstance();

            // we don't keep any variables pointing to these as they are all added to Realm
            using (var trans = realm.BeginWrite())
            {
                var o1 = realm.Add(new Owner { Name = "Tim" });

                var d1 = realm.Add(new Dog
                {
                    Name = "Bilbo Fleabaggins",
                    Color = "Black"
                });

                o1.TopDog = d1;  // set a one-one relationship
                o1.Dogs.Add(d1);

                var d2 = realm.Add(new Dog
                {
                    Name = "Earl Yippington III",
                    Color = "White"
                });

                o1.Dogs.Add(d2);

                // lonely people and dogs
                realm.Add(new Owner
                {
                    Name = "Dani" // the dog-less
                });

                realm.Add(new Dog // will remain unassigned
                {
                    Name = "Maggie Mongrel",
                    Color = "Grey"
                });

                trans.Commit();
            }
        }

        [TearDown]
        public void TearDown()
        {
            realm.Dispose();
            Realm.DeleteRealm(realm.Config);
        }

        [Test]
        public void TimHasATopDog()
        {
            var tim = realm.All<Owner>().First(p => p.Name == "Tim");
            Assert.That(tim.TopDog.Name, Is.EqualTo("Bilbo Fleabaggins"));
        }

        [Test]
        public void TimHasTwoIterableDogs()
        {
            var tim = realm.All<Owner>().First(p => p.Name == "Tim");
            var dogNames = new List<string>();
            foreach (var dog in tim.Dogs)  // using foreach here is deliberately testing that syntax
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
            var tim = realm.All<Owner>().First(p => p.Name == "Tim");
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
            var tim = realm.All<Owner>().First(p => p.Name == "Tim");
            Assert.That(() => tim.Dogs.CopyTo(null, 0), Throws.TypeOf<ArgumentNullException>());
            var copiedDogs = new Dog[2];
            Assert.That(() => tim.Dogs.CopyTo(copiedDogs, -1), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => tim.Dogs.CopyTo(copiedDogs, 1), Throws.TypeOf<ArgumentException>()); // insuffiient room
        }

        [Test]
        public void TimRetiredHisTopDog()
        {
            var tim = realm.All<Owner>().First(p => p.Name == "Tim");
            using (var trans = realm.BeginWrite())
            {
                tim.TopDog = null;
                trans.Commit();
            }

            var tim2 = realm.All<Owner>().First(p => p.Name == "Tim");
            Assert.That(tim2.TopDog, Is.Null);  // the dog departure was saved
        }

        [Test]
        public void TimAddsADogLater()
        {
            var tim = realm.All<Owner>().First(p => p.Name == "Tim");
            Assert.That(tim.Dogs.Count(), Is.EqualTo(2));
            using (var trans = realm.BeginWrite())
            {
                var dog3 = realm.All<Dog>().Where(p => p.Name == "Maggie Mongrel").First();
                tim.Dogs.Add(dog3);
                trans.Commit();
            }

            var tim2 = realm.All<Owner>().First(p => p.Name == "Tim");
            Assert.That(tim2.Dogs.Count(), Is.EqualTo(3));
            Assert.That(tim2.Dogs[2].Name, Is.EqualTo("Maggie Mongrel"));
        }

        [Test]
        public void TimAddsADogByInsert()
        {
            var tim = realm.All<Owner>().Single(p => p.Name == "Tim");  // use Single for a change
            Assert.That(tim.Dogs.Count(), Is.EqualTo(2));
            using (var trans = realm.BeginWrite())
            {
                var dog3 = realm.All<Dog>().First(p => p.Name == "Maggie Mongrel");
                tim.Dogs.Insert(1, dog3);
                trans.Commit();
            }

            var tim2 = realm.All<Owner>().Single(p => p.Name == "Tim");
            Assert.That(tim2.Dogs.Count(), Is.EqualTo(3));
            Assert.That(tim2.Dogs[1].Name, Is.EqualTo("Maggie Mongrel"));
            Assert.That(tim2.Dogs[2].Name, Is.EqualTo("Earl Yippington III"));
        }

        [Test]
        public void TimLosesHisDogsByOrder()
        {
            var tim = realm.All<Owner>().Single(p => p.Name == "Tim");
            Assert.That(tim.Dogs.Count(), Is.EqualTo(2));
            using (var trans = realm.BeginWrite())
            {
                tim.Dogs.RemoveAt(0);
                trans.Commit();
            }

            var tim2 = realm.All<Owner>().Single(p => p.Name == "Tim");
            Assert.That(tim2.Dogs.Count(), Is.EqualTo(1));
            Assert.That(tim2.Dogs[0].Name, Is.EqualTo("Earl Yippington III"));
            using (var trans = realm.BeginWrite())
            {
                tim.Dogs.RemoveAt(0);
                trans.Commit();
            }

            var tim3 = realm.All<Owner>().Single(p => p.Name == "Tim");
            Assert.That(tim2.Dogs.Count(), Is.EqualTo(0));
            Assert.That(tim3.Dogs.Count(), Is.EqualTo(0)); // reloaded object has same empty related set
        }

        [Test]
        public void TimLosesHisDogsInOneClear()
        {
            var tim = realm.All<Owner>().Single(p => p.Name == "Tim");
            Assert.That(tim.Dogs.Count(), Is.EqualTo(2));
            using (var trans = realm.BeginWrite())
            {
                tim.Dogs.Clear();
                trans.Commit();
            }

            var tim2 = realm.All<Owner>().Single(p => p.Name == "Tim");
            Assert.That(tim2.Dogs.Count(), Is.EqualTo(0));
        }

        [Test]
        public void TimLosesBilbo()
        {
            var bilbo = realm.All<Dog>().First(p => p.Name == "Bilbo Fleabaggins");
            var tim = realm.All<Owner>().Single(p => p.Name == "Tim");
            Assert.That(tim.Dogs.Count(), Is.EqualTo(2));
            using (var trans = realm.BeginWrite())
            {
                tim.Dogs.Remove(bilbo);
                trans.Commit();
            }

            var tim2 = realm.All<Owner>().Single(p => p.Name == "Tim");
            Assert.That(tim2.Dogs.Count(), Is.EqualTo(1));
            Assert.That(tim2.Dogs[0].Name, Is.EqualTo("Earl Yippington III"));
        }

        [Test]
        public void DaniHasNoTopDog()
        {
            var dani = realm.All<Owner>().Where(p => p.Name == "Dani").First();
            Assert.That(dani.TopDog, Is.Null);
        }

        [Test]
        public void DaniHasNoDogs()
        {
            var dani = realm.All<Owner>().Where(p => p.Name == "Dani").Single();
            Assert.That(dani.Dogs.Count(), Is.EqualTo(0));  // ToMany relationships always return a RealmList
            int dogsIterated = 0;
            foreach (var d in dani.Dogs)
            {
                dogsIterated++;
            }

            Assert.That(dogsIterated, Is.EqualTo(0));
        }

        [Test]
        public void TestExceptionsFromEmptyListOutOfRange()
        {
            var dani = realm.All<Owner>().Where(p => p.Name == "Dani").First();
            Assert.That(() => dani.Dogs.RemoveAt(0), Throws.TypeOf<ArgumentOutOfRangeException>());
            var bilbo = realm.All<Dog>().Single(p => p.Name == "Bilbo Fleabaggins");
            Dog scratch;  // for assignment in following getters
            Assert.That(() => dani.Dogs.Insert(-1, bilbo), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => dani.Dogs.Insert(1, bilbo), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => scratch = dani.Dogs[0], Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void TestExceptionsFromIteratingEmptyList()
        {
            var dani = realm.All<Owner>().Where(p => p.Name == "Dani").Single();
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
            var tim = realm.All<Owner>().Single(p => p.Name == "Tim");
            Assert.That(() => tim.Dogs.RemoveAt(4), Throws.TypeOf<ArgumentOutOfRangeException>());
            var bilbo = realm.All<Dog>().Single(p => p.Name == "Bilbo Fleabaggins");
            Dog scratch;  // for assignment in following getters
            Assert.That(() => tim.Dogs.Insert(-1, bilbo), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => tim.Dogs.Insert(3, bilbo), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => scratch = tim.Dogs[99], Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void TestSettingStandAloneObjectToRelationship()
        {
            var owner = realm.All<Owner>().First();
            var dog = new Dog { Name = "Astro" };

            using (var trans = realm.BeginWrite())
            {
                owner.TopDog = dog;
                trans.Commit();
            }

            var dogAgain = realm.All<Dog>().Single(d => d.Name == "Astro");
            Assert.That(dogAgain, Is.Not.Null);
            Assert.That(dog.IsManaged);
        }

        [Test]
        public void TestAddingStandAloneObjectToToManyRelationship()
        {
            var owner = realm.All<Owner>().First();
            var dog = new Dog { Name = "Astro" };

            using (var trans = realm.BeginWrite())
            {
                owner.Dogs.Add(dog);
                trans.Commit();
            }

            var dogAgain = realm.All<Dog>().Single(d => d.Name == "Astro");
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

            using (var trans = realm.BeginWrite())
            {
                realm.Add(person);
                trans.Commit();
            }

            Assert.That(person.Friends is RealmList<Person>);
            Assert.That(realm.All<Person>().ToList().Select(p => p.FirstName),
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

            using (var trans = realm.BeginWrite())
            {
                realm.Add(sally); // top person Manages entire tree
                trans.Commit();
            }

            Assert.That(realm.All<Person>().ToList().Select(p => p.FirstName),
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

            using (var trans = realm.BeginWrite())
            {
                realm.Add(sally); // top person Manages entire tree
                sally.Friends[1].Friends.Add(joanFriend);

                trans.Commit();
            }

            Assert.That(realm.All<Person>().ToList().Select(p => p.FirstName),
                        Is.EquivalentTo(new[] { "Alice", "Joan", "Krystal", "Sally" }));
        }

        [Test]
        public void Backlinks_SanityCheck()
        {
            var tim = realm.All<Owner>().Single(o => o.Name == "Tim");
            foreach (var dog in tim.Dogs)
            {
                Assert.That(dog.Owners, Is.EquivalentTo(new[] { tim })); 
            }

            var dani = realm.All<Owner>().Single(o => o.Name == "Dani");
            var maggie = realm.All<Dog>().Single(d => d.Name == "Maggie Mongrel");
            Assert.That(maggie.Owners, Is.Empty);

            realm.Write(() => dani.Dogs.Add(maggie));
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

            realm.Write(() =>
            {
                realm.Add(doggy);
                realm.Add(john);
            });

            // We check both Count() and FirstOrDefault() due to a bug we had with queries
            Assert.That(doggy.Owners.Count(), Is.EqualTo(0));
            Assert.That(doggy.Owners.FirstOrDefault(), Is.Null);

            var sally = new Owner
            {
                Name = "Sally"
            };

            realm.Write(() =>
            {
                realm.Add(sally);

                john.Dogs.Add(doggy);
            });

            Assert.That(doggy.Owners.Count(), Is.EqualTo(1));
            Assert.That(doggy.Owners.Single(), Is.EqualTo(john));

            realm.Write(() =>
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

            realm.Write(() =>
            {
                realm.Add(child1);
                realm.Add(parent);
            });

            Assert.That(parent.Children.Count(), Is.EqualTo(0));
            Assert.That(parent.Children.FirstOrDefault(), Is.Null);

            var child2 = new RecursiveBacklinksObject
            {
                Id = 2
            };

            realm.Write(() =>
            {
                realm.Add(child2);

                child1.Parent = parent;
            });

            Assert.That(parent.Children.Count(), Is.EqualTo(1));
            Assert.That(parent.Children.Single(), Is.EqualTo(child1));

            realm.Write(() =>
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
            realm.Write(() =>
            {
                for (var pid = 1; pid <= 4; ++pid)
                {
                    var p = realm.Add(new Product
                    {
                        Id = pid,
                        Name = $"Product {pid}"
                    });

                    for (var rid = 1; rid <= 5; ++rid)
                    {
                        var r = realm.Add(new Report
                        {
                            Id = rid + (pid * 1000),
                            Ref = $"Report {pid}:{rid}"
                        });

                        p.Reports.Add(r);
                    }
                }
            });

            var delId = 1;
            var delP = realm.All<Product>().First(p => p.Id == delId);
            Assert.IsNotNull(delP);
            Assert.That(delP.Reports.Count, Is.EqualTo(5));

            realm.Write(() =>
            {
                foreach (var r in delP.Reports.ToList()) // use ToList to get static list so can remove items
                {
                    realm.Remove(r); // removes from the realm, and updates delP.Reports so can't just iterate that
                }
            });

            Assert.That(delP.Reports.Count, Is.EqualTo(0));
        }

        #endregion
    }
}