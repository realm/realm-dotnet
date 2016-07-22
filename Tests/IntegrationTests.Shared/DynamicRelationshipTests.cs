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
#if ENABLE_INTERNAL_NON_PCL_TESTS    
    [Preserve(AllMembers = true)]
    public enum DynamicTestObjectType
    {
        RealmObject,
        DynamicRealmObject
    }

    [TestFixture(DynamicTestObjectType.RealmObject)]
    [TestFixture(DynamicTestObjectType.DynamicRealmObject)]
    [Preserve(AllMembers = true)]
    public class DynamicRelationshipTests
    {
        [Preserve(AllMembers = true)]
        class DynamicDog : RealmObject
        {
            public string Name { get; set; }
            public string Color { get; set; }
            public bool Vaccinated { get; set; }
            //Owner Owner { get; set; }  will uncomment when verifying that we have back-links from ToMany relationships
        }

        [Preserve(AllMembers = true)]
        class DynamicOwner : RealmObject
        {
            public string Name { get; set; }
            public DynamicDog TopDog { get; set; }
            public RealmList<DynamicDog> Dogs { get; }
        }

        private RealmSchema _schema;
        private Realm _realm;

        public DynamicRelationshipTests(DynamicTestObjectType mode)
        {
            _schema = RealmSchema.CreateSchemaForClasses(new[] { typeof(DynamicOwner), typeof(DynamicDog) });
            if (mode == DynamicTestObjectType.DynamicRealmObject)
                _schema = _schema.DynamicClone();
        }

        [SetUp]
        public void Setup()
        {
            Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);
            _realm = Realm.GetInstance(RealmConfiguration.DefaultConfiguration, _schema);

            using (var trans = _realm.BeginWrite())
            {
                var o1 = _realm.CreateObject("DynamicOwner");
                o1.Name = "Tim";

                var d1 = _realm.CreateObject("DynamicDog");
                d1.Name = "Bilbo Fleabaggins";
                d1.Color = "Black";
                o1.TopDog = d1;  // set a one-one relationship
                o1.Dogs.Add(d1);

                var d2 = _realm.CreateObject("DynamicDog");
                d2.Name = "Earl Yippington III";
                d2.Color = "White";
                o1.Dogs.Add(d2);

                // lonely people and dogs
                var o2 = _realm.CreateObject("DynamicOwner");
                o2.Name = "Dani";  // the dog-less

                var d3 = _realm.CreateObject("DynamicDog");  // will remain unassigned
                d3.Name = "Maggie Mongrel";
                d3.Color = "Grey";

                trans.Commit();
            }
        }

        [TearDown]
        public void TearDown()
        {
            _realm.Close();
            Realm.DeleteRealm(_realm.Config);
        }

        [Test]
        public void TimHasATopDog()
        {
            var tim = _realm.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");
            Assert.That(tim.TopDog.Name, Is.EqualTo("Bilbo Fleabaggins"));
        }

        [Test]
        public void TimHasTwoIterableDogs()
        {
            var tim = _realm.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");
            var dogNames = new List<string>();
            foreach (var dog in tim.Dogs)  // using foreach here is deliberately testing that syntax
            {
                dogNames.Add(dog.Name);
            }
            Assert.That(dogNames, Is.EquivalentTo(new[] { "Bilbo Fleabaggins", "Earl Yippington III" }));
        }

        /// <summary>
        /// Check if ToList can be invoked on a related RealmResults
        /// </summary>
        [Test]
        public void TimHasTwoIterableDogsListed()
        {
            var tim = _realm.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");
            var dogNames = new List<string>();
            var dogList = Enumerable.ToList<dynamic>(tim.Dogs);  // this used to crash - issue 299
            foreach (var dog in dogList)
            {
                dogNames.Add(dog.Name);
            }
            Assert.That(dogNames, Is.EquivalentTo(new[] { "Bilbo Fleabaggins", "Earl Yippington III" }));
        }

        [Test]
        public void TimRetiredHisTopDog()
        {
            var tim = _realm.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");
            using (var trans = _realm.BeginWrite())
            {
                tim.TopDog = null;
                trans.Commit();
            }
            var tim2 = _realm.All("DynamicOwner").ToArray().First(p => p.Name == "Tim");
            Assert.That(tim2.TopDog, Is.Null);  // the dog departure was saved
        }

        [Test]
        public void TimAddsADogLater()
        {
            var tim = _realm.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");
            Assert.That(tim.Dogs.Count, Is.EqualTo(2));
            using (var trans = _realm.BeginWrite())
            {
                var dog3 = _realm.All("DynamicDog").ToArray().First(p => p.Name == "Maggie Mongrel");
                tim.Dogs.Add(dog3);
                trans.Commit();
            }
            var tim2 = _realm.All("DynamicOwner").ToArray().First(p => p.Name == "Tim");
            Assert.That(tim2.Dogs.Count, Is.EqualTo(3));
            Assert.That(tim2.Dogs[2].Name, Is.EqualTo("Maggie Mongrel"));
        }

        [Test]
        public void TimAddsADogByInsert()
        {
            var tim = _realm.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");  // use Single for a change
            Assert.That(tim.Dogs.Count, Is.EqualTo(2));
            using (var trans = _realm.BeginWrite())
            {
                var dog3 = _realm.All("DynamicDog").ToArray().First(p => p.Name == "Maggie Mongrel");
                tim.Dogs.Insert(1, dog3);
                trans.Commit();
            }
            var tim2 = _realm.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");
            Assert.That(tim2.Dogs.Count, Is.EqualTo(3));
            Assert.That(tim2.Dogs[1].Name, Is.EqualTo("Maggie Mongrel"));
            Assert.That(tim2.Dogs[2].Name, Is.EqualTo("Earl Yippington III"));
        }

        [Test]
        public void TimLosesHisDogsByOrder()
        {
            var tim = _realm.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");
            Assert.That(tim.Dogs.Count, Is.EqualTo(2));
            using (var trans = _realm.BeginWrite())
            {
                tim.Dogs.RemoveAt(0);
                trans.Commit();
            }
            var tim2 = _realm.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");
            Assert.That(tim2.Dogs.Count, Is.EqualTo(1));
            Assert.That(tim2.Dogs[0].Name, Is.EqualTo("Earl Yippington III"));
            using (var trans = _realm.BeginWrite())
            {
                tim.Dogs.RemoveAt(0);
                trans.Commit();
            }
            var tim3 = _realm.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");
            Assert.That(tim2.Dogs.Count, Is.EqualTo(0));
            Assert.That(tim3.Dogs.Count, Is.EqualTo(0)); // reloaded object has same empty related set
        }

        [Test]
        public void TimLosesHisDogsInOneClear()
        {
            var tim = _realm.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");
            Assert.That(tim.Dogs.Count, Is.EqualTo(2));
            using (var trans = _realm.BeginWrite())
            {
                tim.Dogs.Clear();
                trans.Commit();
            }
            var tim2 = _realm.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");
            Assert.That(tim2.Dogs.Count, Is.EqualTo(0));
        }

        [Test]
        public void TimLosesBilbo()
        {
            var bilbo = _realm.All("DynamicDog").ToArray().Single(p => p.Name == "Bilbo Fleabaggins");
            var tim = _realm.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");
            Assert.That(tim.Dogs.Count, Is.EqualTo(2));
            using (var trans = _realm.BeginWrite())
            {
                tim.Dogs.Remove(bilbo);
                trans.Commit();
            }
            var tim2 = _realm.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");
            Assert.That(tim2.Dogs.Count, Is.EqualTo(1));
            Assert.That(tim2.Dogs[0].Name, Is.EqualTo("Earl Yippington III"));
        }

        [Test]
        public void DaniHasNoTopDog()
        {
            var dani = _realm.All("DynamicOwner").ToArray().Single(p => p.Name == "Dani");
            Assert.That(dani.TopDog, Is.Null);
        }

        [Test]
        public void DaniHasNoDogs()
        {
            var dani = _realm.All("DynamicOwner").ToArray().Single(p => p.Name == "Dani");
            Assert.That(dani.Dogs.Count, Is.EqualTo(0));  // ToMany relationships always return a RealmList
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
            var dani = _realm.All("DynamicOwner").ToArray().Single(p => p.Name == "Dani");
            Assert.Throws<IndexOutOfRangeException>(() => dani.Dogs.RemoveAt(0));
            var bilbo = _realm.All("DynamicDog").ToArray().Single(p => p.Name == "Bilbo Fleabaggins");
            dynamic scratch;  // for assignment in following getters
            Assert.Throws<IndexOutOfRangeException>(() => dani.Dogs.Insert(-1, bilbo));
            Assert.Throws<IndexOutOfRangeException>(() => dani.Dogs.Insert(0, bilbo));
            Assert.Throws<IndexOutOfRangeException>(() => scratch = dani.Dogs[0]);
        }

        [Test]
        public void TestExceptionsFromIteratingEmptyList()
        {
            var dani = _realm.All("DynamicOwner").ToArray().Single(p => p.Name == "Dani");
            var iter = dani.Dogs.GetEnumerator();
            Assert.IsNotNull(iter);
            var movedOnToFirstItem = iter.MoveNext();
            Assert.That(movedOnToFirstItem, Is.False);
            dynamic currentDog;
            Assert.Throws<IndexOutOfRangeException>(() => currentDog = iter.Current);
        }

        [Test]
        public void TestExceptionsFromTimsDogsOutOfRange()
        {
            var tim = _realm.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");
            Assert.Throws<IndexOutOfRangeException>(() => tim.Dogs.RemoveAt(4));
            var bilbo = _realm.All("DynamicDog").ToArray().Single(p => p.Name == "Bilbo Fleabaggins");
            dynamic scratch;  // for assignment in following getters
            Assert.Throws<IndexOutOfRangeException>(() => tim.Dogs.Insert(-1, bilbo));
            Assert.Throws<IndexOutOfRangeException>(() => tim.Dogs.Insert(3, bilbo));
            Assert.Throws<IndexOutOfRangeException>(() => scratch = tim.Dogs[99]);
        }
    }
#endif
}