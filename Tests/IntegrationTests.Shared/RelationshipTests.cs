/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using NUnit.Framework;
using RealmNet;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

// NOTE some of the following data comes from Tim's data used in the Browser screenshot in the Mac app store
// unlike the Cocoa definitions, we use Pascal casing for properties
namespace Tests
{
    [TestFixture]
    public class RelationshipTests
    {
        class Dog : RealmObject
        {
            public string Name { get; set; }
            public string Color { get; set; } = "Brown";
            public bool Vaccinated { get; set; } = true;
            // here to create Fody error feedback until we support in https://github.com/realm/realm-dotnet/issues/36
            // public DateTime born { get; set; }
            //Owner Owner { get; set; }  will uncomment when verifying that we have back-links from ToMany relationships
        }

        class Owner : RealmObject
        {
            public string Name { get; set; }
            public Dog TopDog { get; set; }
            public RealmList<Dog> Dogs { get; set; } // TODO allow this if we can preserve init through weaving = new RealmList<Dog>();
        }

        protected Realm realm;

        [SetUp]
        public void Setup()
        {

            realm = Realm.GetInstance(Path.GetTempFileName());

            // we don't keep any variables pointing to these as they are all added to Realm
            using (var trans = realm.BeginWrite())
            {
                /* syntax we want back needs ability for constructor to auto-bind to active write transaction
                new Owner {Name = "Tim", Dogs = new RealmList<Dog> {
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
                //o1.Dogs.Add (d2);

                // lonely people and dogs
                Owner o2 = realm.CreateObject<Owner> ();
                o2.Name = "Dani";  // the dog-less

                Dog d3 = realm.CreateObject<Dog> ();  // will remain unassigned
                d3.Name = "Maggie Mongrel";
                d3.Color = "Grey";

                /*
                These would work if we can preserve init through weaving, like:
                public RealmList<Dog> Dogs { get; set; } = new RealmList<Dog>();

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
                b.Dogs = new RealmList<Dog> { new Dog { Name = "Madame Barklouder", Vaccinated = false, Color = "White" }};
                */
                trans.Commit ();
            }
        }


        [Test]
        public void TimHasATopDog()
        {
            var tim = realm.All<Owner>().Where( p => p.Name == "Tim").ToList().First();
            Assert.That(tim.TopDog.Name, Is.EqualTo( "Bilbo Fleabaggins"));
        }


        [Test]
        public void TimRetiredHisTopDog()
        {
            var tim = realm.All<Owner>().Where( p => p.Name == "Tim").ToList().First();
            using (var trans = realm.BeginWrite()) {
                tim.TopDog = null;
                trans.Commit ();
            }                
            var tim2 = realm.All<Owner>().Where( p => p.Name == "Tim").ToList().First();
            Assert.That(tim2.TopDog, Is.Null);  // the dog departure was saved
        }


        [Test]
        public void DaniHasNoTopDog()
        {
            var dani = realm.All<Owner>().Where( p => p.Name == "Dani").ToList().First();
            Assert.That(dani.TopDog, Is.Null);
        }


        [Test]
        public void DaniHasNoDogs()
        {
            var dani = realm.All<Owner>().Where( p => p.Name == "Dani").ToList().First();
            Assert.That(dani.Dogs.Count(), Is.EqualTo(0));  // ToMany relationships always return a RealmList
        }

    }
} 