﻿using NUnit.Framework;
using RealmNet;
using RealmNet.Interop;
using System.Collections.Generic;
using System.Linq;
using InteropShared;
using System.Diagnostics;

// NOTE some of the following data comes from Tim's data used in the Browser screenshot in the Mac app store
// unlike the Cocoa definitions, we use Pascal casing for properties
namespace Tests
{
    [TestFixture]
    public class ListTests
    {
        class Dog : RealmObject
        {
            public string Name { get; set; }
            public string Color { get; set; } = "Brown";
            public bool Vaccinated { get; set; } = true;
           // TODO Owner owner { get; set; }
        }

        class Owner : RealmObject
        {
            public string Name { get; set; }
            public IList<Dog> Dogs { get; set; }
        }

        protected Realm realm;

        [SetUp]
        public void Setup()
        {
#if USING_REALM_CORE
            Realm.ActiveCoreProvider = new CoreProvider();
#else
            // use a mock - supports get and set operations but not searching
            Realm.ActiveCoreProvider = new MockCoreProvider();
#endif
            realm = Realm.GetInstance(System.IO.Path.GetTempFileName());

            // we don't keep any variables pointing to these as they are all added to Realm
            using (var writeWith = realm.BeginWrite())
            {
                new Owner {Name = "Tim", Dogs = {
                    new Dog {Name = "Bilbo Fleabaggins"},
                    new Dog {Name = "Earl Yippington III" }
                    } };
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
                // to show you can assign later, create the Owner then assign their Dog
                var b = new Owner {Name = "Bjarne"};  
                b.Dogs = new List<Dog> { new Dog { Name = "Madame Barklouder", Vaccinated = false, Color = "White" }};
            }
        }


        [Test]
        public void ListAllOwners()
        {
            // Arrange
            var owners = realm.All<Owner>();
            foreach (var o in owners)
            {
                Debug.WriteLine(o.Name);
            }
        }


        [Test]
        public void ListAllDogs()
        {
            // Arrange
            var furryBosses = realm.All<Dog>();
            foreach (var dog in furryBosses)
            {
                Debug.WriteLine($"{dog.Name} is {dog.Color}");
            }
        }

    }
} 