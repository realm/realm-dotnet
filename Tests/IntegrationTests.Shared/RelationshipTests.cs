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
 
using NUnit.Framework;
using Realms;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

// NOTE some of the following data comes from Tim's data used in the Browser screenshot in the Mac app store
// unlike the Cocoa definitions, we use Pascal casing for properties
namespace IntegrationTests.Shared
{
    [TestFixture]
    public class RelationshipTests
    {
        class Dog : RealmObject
        {
            public string Name { get; set; }
            public string Color { get; set; }
            public bool Vaccinated { get; set; }
            //Owner Owner { get; set; }  will uncomment when verifying that we have back-links from ToMany relationships
        }

        class Owner : RealmObject
        {
            public string Name { get; set; }
            public Dog TopDog { get; set; }
            public IList<Dog> Dogs { get; } 
        }

        protected Realm realm;

        [SetUp]
        public void Setup()
        {
            Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);
            realm = Realm.GetInstance();

            // we don't keep any variables pointing to these as they are all added to Realm
            using (var trans = realm.BeginWrite())
            {
                /* syntax we want back needs ability for constructor to auto-bind to active write transaction
                new Owner {Name = "Tim", Dogs = new IList<Dog> {
                    new Dog {Name = "Bilbo Fleabaggins"},
                    new Dog {Name = "Earl Yippington III" }
                    } };
                    */
                Owner o1 = realm.CreateObject<Owner> ();
                o1.Name = "Tim";

                Dog d1 = realm.CreateObject<Dog> ();
                d1.Name = "Bilbo Fleabaggins";
                d1.Color = "Black";
                o1.TopDog = d1;  // set a one-one relationship
                o1.Dogs.Add (d1);

                Dog d2 = realm.CreateObject<Dog> ();
                d2.Name = "Earl Yippington III";
                d2.Color = "White";
                o1.Dogs.Add (d2);

                // lonely people and dogs
                Owner o2 = realm.CreateObject<Owner> ();
                o2.Name = "Dani";  // the dog-less

                Dog d3 = realm.CreateObject<Dog> ();  // will remain unassigned
                d3.Name = "Maggie Mongrel";
                d3.Color = "Grey";

                /*
                These would work if we can preserve init through weaving, like:
                public IList<Dog> Dogs { get; set; } = new IList<Dog>();

                new Owner {Name = "JP", Dogs = { new Dog { Name = "Deputy Dawg", Vaccinated=false } } };
                new Owner {Name = "Arwa", Dogs = { new Dog { Name = "Hairy Pawter", Color = "Black" } } };
                new Owner {Name = "Joe", Dogs = { new Dog { Name = "Jabba the Mutt", Vaccinated = false } } };
                new Owner {Name = "Alex", Dogs = { new Dog { Name = "Hairy Pawter", Color = "Black" } } };
                new Owner {Name = "Michael", Dogs = { new Dog { Name = "Nerf Herder", Color="Red" } } };
                new Owner {Name = "Adam", Dogs = { new Dog { Name = "Defense Secretary Waggles" } } };
                new Owner {Name = "Samuel", Dogs = { new Dog { Name = "Salacious B. Crumb", Color="Tan" } } };
                new Owner {Name = "Kristen"}; // Kristen's dog was abducted by Tim so she doesn't have any
                new Owner {Name = "Emily", Dogs = { new Dog { Name = "Pickles McPorkchop" } } };
                new Owner {Name = "Katsumi", Dogs = { new Dog { Name = "Sir Yaps-a-lot", Vaccinated = false } } };
                new Owner {Name = "Morgan", Dogs = { new Dog { Name = "Rudy Loosebooty" } } };
                */
                // to show you can assign later, create the Owner then assign their Dog

                    /* syntax for later
                var b = new Owner {Name = "Bjarne"};  
                b.Dogs = new IList<Dog> { new Dog { Name = "Madame Barklouder", Vaccinated = false, Color = "White" }};
                */
                trans.Commit ();
            }
        }

        [TearDown]
        public void TearDown()
        {
            realm.Close();
            Realm.DeleteRealm(realm.Config);
        }

        [Test]
        public void TimHasATopDog()
        {
            var tim = realm.All<Owner>().First( p => p.Name == "Tim");
            Assert.That(tim.TopDog.Name, Is.EqualTo( "Bilbo Fleabaggins"));
        }


        [Test]
        public void TimHasTwoIterableDogs()
        {
            var tim = realm.All<Owner>().First( p => p.Name == "Tim");
            var dogNames = new List<string>();
            foreach (var dog in tim.Dogs)  // using foreach here is deliberately testing that syntax
            {
                dogNames.Add(dog.Name);
            }
            Assert.That(dogNames, Is.EquivalentTo( new List<String> {"Bilbo Fleabaggins", "Earl Yippington III"}));
        }


        /// <summary>
        /// Check if ToList can be invoked on a related RealmResults
        /// </summary>
        [Test]
        public void TimHasTwoIterableDogsListed()
        {
            var tim = realm.All<Owner>().First( p => p.Name == "Tim");
            var dogNames = new List<string>();
            var dogList = tim.Dogs.ToList();  // this used to crash - issue 299
            foreach (var dog in dogList)
            {
                dogNames.Add(dog.Name);
            }
            Assert.That(dogNames, Is.EquivalentTo( new List<String> {"Bilbo Fleabaggins", "Earl Yippington III"}));
        }


        [Test]
        public void TimsIterableDogsThrowExceptions()
        {
            var tim = realm.All<Owner>().First( p => p.Name == "Tim");
            Assert.Throws<ArgumentNullException> (() => tim.Dogs.CopyTo (null, 0));
            Dog[] copiedDogs = new Dog[2];
            Assert.Throws<ArgumentOutOfRangeException> (() => tim.Dogs.CopyTo (copiedDogs, -1));
            Assert.Throws<ArgumentException> (() => tim.Dogs.CopyTo (copiedDogs, 1));  // insuffiient room
        }




        [Test]
        public void TimRetiredHisTopDog()
        {
            var tim = realm.All<Owner>().First( p => p.Name == "Tim");
            using (var trans = realm.BeginWrite()) {
                tim.TopDog = null;
                trans.Commit ();
            }                
            var tim2 = realm.All<Owner>().First( p => p.Name == "Tim");
            Assert.That(tim2.TopDog, Is.Null);  // the dog departure was saved
        }


        [Test]
        public void TimAddsADogLater()
        {
            var tim = realm.All<Owner>().First( p => p.Name == "Tim");
            Assert.That(tim.Dogs.Count(), Is.EqualTo(2));  
            using (var trans = realm.BeginWrite()) {
                var dog3 = realm.All<Dog>().Where( p => p.Name == "Maggie Mongrel").ToList().First();
                tim.Dogs.Add (dog3);
                trans.Commit ();
            }
            var tim2 = realm.All<Owner>().First( p => p.Name == "Tim");
            Assert.That(tim2.Dogs.Count(), Is.EqualTo(3));  
            Assert.That(tim2.Dogs[2].Name, Is.EqualTo("Maggie Mongrel")); 
        }


        [Test]
        public void TimAddsADogByInsert()
        {
            var tim = realm.All<Owner>().Single( p => p.Name == "Tim");  // use Single for a change
            Assert.That(tim.Dogs.Count(), Is.EqualTo(2));  
            using (var trans = realm.BeginWrite()) {
                var dog3 = realm.All<Dog>().Where( p => p.Name == "Maggie Mongrel").ToList().First();
                tim.Dogs.Insert (1, dog3);
                trans.Commit ();
            }
            var tim2 = realm.All<Owner>().Single( p => p.Name == "Tim");
            Assert.That(tim2.Dogs.Count(), Is.EqualTo(3));  
            Assert.That(tim2.Dogs[1].Name, Is.EqualTo("Maggie Mongrel")); 
            Assert.That(tim2.Dogs[2].Name, Is.EqualTo("Earl Yippington III")); 
        }


        [Test]
        public void TimLosesHisDogsByOrder()
        {
            var tim = realm.All<Owner>().Single( p => p.Name == "Tim");
            Assert.That(tim.Dogs.Count(), Is.EqualTo(2));  
            using (var trans = realm.BeginWrite()) {
                tim.Dogs.RemoveAt(0);
                trans.Commit ();
            }                
            var tim2 = realm.All<Owner>().Single( p => p.Name == "Tim");
            Assert.That(tim2.Dogs.Count(), Is.EqualTo(1)); 
            Assert.That(tim2.Dogs[0].Name, Is.EqualTo("Earl Yippington III")); 
            using (var trans = realm.BeginWrite()) {
                tim.Dogs.RemoveAt(0);
                trans.Commit ();
            }                
            var tim3 = realm.All<Owner>().Single( p => p.Name == "Tim");
            Assert.That(tim2.Dogs.Count(), Is.EqualTo(0)); 
            Assert.That(tim3.Dogs.Count(), Is.EqualTo(0)); // reloaded object has same empty related set
        }


        [Test]
        public void TimLosesHisDogsInOneClear()
        {
            var tim = realm.All<Owner>().Single(p => p.Name == "Tim");
            Assert.That(tim.Dogs.Count(), Is.EqualTo(2));
            using (var trans = realm.BeginWrite()) {
                tim.Dogs.Clear();
                trans.Commit();
            }
            var tim2 = realm.All<Owner>().Single(p => p.Name == "Tim");
            Assert.That(tim2.Dogs.Count(), Is.EqualTo(0));
        }


        [Test]
        public void TimLosesBilbo()
        {
            var bilbo = realm.All<Dog> ().First (p => p.Name == "Bilbo Fleabaggins");
            var tim = realm.All<Owner>().Single( p => p.Name == "Tim");
            Assert.That(tim.Dogs.Count(), Is.EqualTo(2));  
            using (var trans = realm.BeginWrite()) {
                tim.Dogs.Remove(bilbo);
                trans.Commit ();
            }                
            var tim2 = realm.All<Owner>().Single( p => p.Name == "Tim");
            Assert.That(tim2.Dogs.Count(), Is.EqualTo(1)); 
            Assert.That(tim2.Dogs[0].Name, Is.EqualTo("Earl Yippington III")); 
        }


        [Test]
        public void DaniHasNoTopDog()
        {
            var dani = realm.All<Owner>().Where( p => p.Name == "Dani").First();
            Assert.That(dani.TopDog, Is.Null);
        }


        [Test]
        public void DaniHasNoDogs()
        {
            var dani = realm.All<Owner>().Where( p => p.Name == "Dani").Single();
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
            Assert.Throws<IndexOutOfRangeException>(() => dani.Dogs.RemoveAt(0));
            var bilbo = realm.All<Dog>().Single(p => p.Name == "Bilbo Fleabaggins");
            Dog scratch;  // for assignment in following getters
            Assert.Throws<IndexOutOfRangeException>(() => dani.Dogs.Insert(-1, bilbo));
            Assert.Throws<IndexOutOfRangeException>(() => dani.Dogs.Insert(0, bilbo));
            Assert.Throws<IndexOutOfRangeException>(() => scratch = dani.Dogs[0]);
        }


        [Test]
        public void TestExceptionsFromIteratingEmptyList()
        {
            var dani = realm.All<Owner>().Where(p => p.Name == "Dani").Single();
            var iter =  dani.Dogs.GetEnumerator();
            Assert.IsNotNull(iter);
            var movedOnToFirstItem = iter.MoveNext();
            Assert.That(movedOnToFirstItem, Is.False);
            Dog currentDog;
            Assert.Throws<IndexOutOfRangeException>(() => currentDog = iter.Current );
        }


        [Test]
        public void TestExceptionsFromTimsDogsOutOfRange()
        {
            var tim = realm.All<Owner>().Single( p => p.Name == "Tim");
            Assert.Throws<IndexOutOfRangeException>( () => tim.Dogs.RemoveAt(4) );
            var bilbo = realm.All<Dog>().Single( p => p.Name == "Bilbo Fleabaggins");
            Dog scratch;  // for assignment in following getters
            Assert.Throws<IndexOutOfRangeException>( () => tim.Dogs.Insert(-1, bilbo) );
            Assert.Throws<IndexOutOfRangeException>( () => tim.Dogs.Insert(3, bilbo) );
            Assert.Throws<IndexOutOfRangeException>( () => scratch = tim.Dogs[99] );
        }

        [Test]
        public void TestSettingStandAloneObjectToRelationship()
        {
            var owner = realm.All<Owner>().ToList().First();
            var dog = new Dog { Name = "Astro" };
            
            using (var trans = realm.BeginWrite())
            {
                owner.TopDog = dog;
                trans.Commit();
            }

            var dogAgain = realm.All<Dog>().Where(d => d.Name == "Astro").ToList().SingleOrDefault();
            Assert.That(dogAgain, Is.Not.Null);
            Assert.That(dog.IsManaged);
        }

        [Test]
        public void TestAddingStandAloneObjectToToManyRelationship()
        {
            var owner = realm.All<Owner>().ToList().First();
            var dog = new Dog { Name = "Astro" };

            using (var trans = realm.BeginWrite())
            {
                owner.Dogs.Add(dog);
                trans.Commit();
            }

            var dogAgain = realm.All<Dog>().Where(d => d.Name == "Astro").ToList().SingleOrDefault();
            Assert.That(dogAgain, Is.Not.Null);
            Assert.That(dog.IsManaged);
        }
        /*
        [Test, Description("This is the default C# behaviour for a list property")]
        public void StandaloneListsShouldBeNull()
        {
            var person = new Person();

            Assert.That(person.Friends, Is.Null);
        }
*/

        /*
         NoteOnListInit
         If you have a simple braced init like 
         Friends = { new Person {  FullName = "Sally"} }
         it requires the property is already initialised and is short-hand for a series of Add
         calls such as
         Friends.Add( new Person {  FullName = "Sally"} );

         A null property has to be initialised with a full object:
         Friends = new List<Person> { new Person {  FullName = "Sally"} }

         We have two approaches under consideration which would allow the simple braced init:
         1) automatically weave in a clause in the ctor initialising the list properties, 
            which adds overhead to creating an object if they never use the list.
         2) If the user chooses to init a list in the ctor, preserve the ctor init (which is a 
            requested feature anyway in issue 109)
        */
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
                realm.Manage(person);
                trans.Commit();
            }

            Assert.That(person.Friends is RealmList<Person>);
            Assert.That(realm.All<Person>().ToList().Select(p => p.FirstName),
                        Is.EquivalentTo(new[] { "Jack", "Christian", "Frederick" })
                       );
        
        }


        [Test]
        public void TestManagingStandaloneThreeLevelRelationship()
        {
            var sally = new Person
            {
                FullName = "Sally",
                Friends =  // see NoteOnListInit above
                {
                    new Person { FullName = "Alice" },
                    new Person
                    {
                        FullName = "Joan",
                        Friends =   // see NoteOnListInit above
                        {
                            new Person()
                            {
                                FullName = "Krystal",
                                Friends =  { new Person {  FullName = "Sally"} }  // Managees a second Sally
                            }
                        } 

                    }
                }
            };

            using (var trans = realm.BeginWrite()) {
                realm.Manage(sally);  // top person Managees entire tree
                trans.Commit();
            }

            Assert.That(realm.All<Person>().ToList().Select(p => p.FirstName),
                        Is.EquivalentTo(new[] { "Alice", "Joan", "Krystal", "Sally", "Sally" })
                       );
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
                    new Person { FullName = "Joan"  }       
                }
            };
            var joanFriend = new Person()
            {
                FullName = "Krystal",
                Friends = {sally} 
            };

            using (var trans = realm.BeginWrite()) {
                realm.Manage(sally);  // top person Managees entire tree
                sally.Friends[1].Friends.Add(joanFriend);  

                trans.Commit();
            }

            Assert.That(realm.All<Person>().ToList().Select(p => p.FirstName), 
                        Is.EquivalentTo(new [] { "Alice", "Joan", "Krystal", "Sally"})
                       );
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
        public void TestDeleteChildren ()
        {
            // Arrange - setup some hierarchies
            realm.Write (() => {
                for (var pid = 1; pid <= 4; ++pid) {
                    var p = realm.CreateObject<Product>();
                    p.Id = pid; p.Name = $"Product {pid}";
                    for (var rid = 1; rid <= 5; ++rid) {
                        var r = realm.CreateObject<Report>();
                        r.Id = rid+pid*1000; r.Ref = $"Report {pid}:{rid}";
                        p.Reports.Add(r);
                    }
                }
            });

            var delId = 1;
            var delP = realm.All<Product>().First(p => p.Id == delId);
            Assert.IsNotNull(delP);
            Assert.That(delP.Reports.Count, Is.EqualTo(5));

            realm.Write(() => {
                foreach (var r in delP.Reports.ToList())  // use ToList to get static list so can remove items
                    realm.Remove(r);  // removes from the realm, and updates delP.Reports so can't just iterate that
            });
            Assert.That(delP.Reports.Count, Is.EqualTo(0));
        }
        #endregion
    }
}