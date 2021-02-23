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

namespace Realms.Tests.Database
{
    [Preserve(AllMembers = true)]
    public enum DynamicTestObjectType
    {
        RealmObject,
        DynamicRealmObject
    }

    [TestFixture(DynamicTestObjectType.RealmObject)]
    [TestFixture(DynamicTestObjectType.DynamicRealmObject)]
    [Preserve(AllMembers = true)]
    public class DynamicRelationshipTests : RealmInstanceTest
    {
        private const string BilbosName = "Bilbo Fleabaggins";
        private const string EarlsName = "Earl Yippington III";
        private const string PipilottasName = "Pipilotta";

        private class DynamicDog : RealmObject
        {
            public string Name { get; set; }

            public string Color { get; set; }

            public bool Vaccinated { get; set; }

            [Backlink(nameof(DynamicOwner.Dogs))]
            public IQueryable<DynamicOwner> Owners { get; }
        }

        private class DynamicOwner : RealmObject
        {
            public string Name { get; set; }

            public DynamicDog TopDog { get; set; }

            public IList<DynamicDog> Dogs { get; }

            public IList<string> Tags { get; }

            public IDictionary<string, DynamicDog> DogsDictionary { get; }

            public IDictionary<string, string> TagsDictionary { get; }
        }

        private readonly DynamicTestObjectType _mode;

        public DynamicRelationshipTests(DynamicTestObjectType mode)
        {
            _mode = mode;
        }

        protected override RealmConfiguration CreateConfiguration(string path)
        {
            return new RealmConfiguration(path)
            {
                ObjectClasses = new[] { typeof(DynamicOwner), typeof(DynamicDog) },
                IsDynamic = _mode == DynamicTestObjectType.DynamicRealmObject
            };
        }

        protected override void CustomSetUp()
        {
            base.CustomSetUp();

            _realm.Write(() =>
            {
                var o1 = _realm.DynamicApi.CreateObject("DynamicOwner", null);
                o1.Name = "Tim";

                var d1 = _realm.DynamicApi.CreateObject("DynamicDog", null);
                d1.Name = BilbosName;
                d1.Color = "Black";
                o1.TopDog = d1;  // set a one-one relationship
                o1.Dogs.Add(d1);
                o1.DogsDictionary.Add(d1.Name, d1);
                o1.TagsDictionary.Add(d1.Name, "great");

                var d2 = _realm.DynamicApi.CreateObject("DynamicDog", null);
                d2.Name = EarlsName;
                d2.Color = "White";
                o1.Dogs.Add(d2);
                o1.DogsDictionary.Add(d2.Name, d2);
                o1.TagsDictionary.Add(d2.Name, "playful");

                // lonely people and dogs
                var o2 = _realm.DynamicApi.CreateObject("DynamicOwner", null);
                o2.Name = "Dani";  // the dog-less

                var d3 = _realm.DynamicApi.CreateObject("DynamicDog", null);  // will remain unassigned
                d3.Name = "Maggie Mongrel";
                d3.Color = "Grey";
            });
        }

        [Test]
        public void TimHasATopDog()
        {
            var tim = _realm.DynamicApi.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");
            Assert.That(tim.TopDog.Name, Is.EqualTo(BilbosName));
        }

        [Test]
        public void TimHasTwoIterableDogs()
        {
            var tim = _realm.DynamicApi.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");
            var dogNames = new List<string>();

            //// using foreach here is deliberately testing that syntax
            foreach (var dog in tim.Dogs)
            {
                dogNames.Add(dog.Name);
            }

            Assert.That(dogNames, Is.EquivalentTo(new[] { BilbosName, EarlsName }));
        }

        /// <summary>
        /// Check if ToList can be invoked on a related RealmResults.
        /// </summary>
        [Test]
        public void TimHasTwoIterableDogsListed()
        {
            var tim = _realm.DynamicApi.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");
            var dogNames = new List<string>();
            var dogList = Enumerable.ToList<dynamic>(tim.Dogs);  // this used to crash - issue 299
            foreach (var dog in dogList)
            {
                dogNames.Add(dog.Name);
            }

            Assert.That(dogNames, Is.EquivalentTo(new[] { BilbosName, EarlsName }));
        }

        [Test]
        public void TimRetiredHisTopDog()
        {
            var tim = _realm.DynamicApi.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");
            using (var trans = _realm.BeginWrite())
            {
                tim.TopDog = null;
                trans.Commit();
            }

            var tim2 = _realm.DynamicApi.All("DynamicOwner").ToArray().First(p => p.Name == "Tim");
            Assert.That(tim2.TopDog, Is.Null);  // the dog departure was saved
        }

        [Test]
        public void TimAddsADogLater()
        {
            var tim = _realm.DynamicApi.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");
            Assert.That(tim.Dogs.Count, Is.EqualTo(2));
            using (var trans = _realm.BeginWrite())
            {
                var dog3 = _realm.DynamicApi.All("DynamicDog").ToArray().First(p => p.Name == "Maggie Mongrel");
                tim.Dogs.Add(dog3);
                trans.Commit();
            }

            var tim2 = _realm.DynamicApi.All("DynamicOwner").ToArray().First(p => p.Name == "Tim");
            Assert.That(tim2.Dogs.Count, Is.EqualTo(3));
            Assert.That(tim2.Dogs[2].Name, Is.EqualTo("Maggie Mongrel"));
        }

        [Test]
        public void TimAddsADogByInsert()
        {
            var tim = _realm.DynamicApi.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");  // use Single for a change
            Assert.That(tim.Dogs.Count, Is.EqualTo(2));
            using (var trans = _realm.BeginWrite())
            {
                var dog3 = _realm.DynamicApi.All("DynamicDog").ToArray().First(p => p.Name == "Maggie Mongrel");
                tim.Dogs.Insert(1, dog3);
                trans.Commit();
            }

            var tim2 = _realm.DynamicApi.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");
            Assert.That(tim2.Dogs.Count, Is.EqualTo(3));
            Assert.That(tim2.Dogs[1].Name, Is.EqualTo("Maggie Mongrel"));
            Assert.That(tim2.Dogs[2].Name, Is.EqualTo(EarlsName));
        }

        [Test]
        public void TimLosesHisDogsByOrder()
        {
            var tim = _realm.DynamicApi.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");
            Assert.That(tim.Dogs.Count, Is.EqualTo(2));
            using (var trans = _realm.BeginWrite())
            {
                tim.Dogs.RemoveAt(0);
                trans.Commit();
            }

            var tim2 = _realm.DynamicApi.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");
            Assert.That(tim2.Dogs.Count, Is.EqualTo(1));
            Assert.That(tim2.Dogs[0].Name, Is.EqualTo(EarlsName));
            using (var trans = _realm.BeginWrite())
            {
                tim.Dogs.RemoveAt(0);
                trans.Commit();
            }

            var tim3 = _realm.DynamicApi.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");
            Assert.That(tim2.Dogs.Count, Is.EqualTo(0));
            Assert.That(tim3.Dogs.Count, Is.EqualTo(0)); // reloaded object has same empty related set
        }

        [Test]
        public void TimLosesHisDogsInOneClear()
        {
            var tim = _realm.DynamicApi.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");
            Assert.That(tim.Dogs.Count, Is.EqualTo(2));
            using (var trans = _realm.BeginWrite())
            {
                tim.Dogs.Clear();
                trans.Commit();
            }

            var tim2 = _realm.DynamicApi.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");
            Assert.That(tim2.Dogs.Count, Is.EqualTo(0));
        }

        [Test]
        public void TimLosesBilbo()
        {
            var bilbo = _realm.DynamicApi.All("DynamicDog").ToArray().Single(p => p.Name == BilbosName);
            var tim = _realm.DynamicApi.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");
            Assert.That(tim.Dogs.Count, Is.EqualTo(2));
            using (var trans = _realm.BeginWrite())
            {
                tim.Dogs.Remove(bilbo);
                trans.Commit();
            }

            var tim2 = _realm.DynamicApi.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");
            Assert.That(tim2.Dogs.Count, Is.EqualTo(1));
            Assert.That(tim2.Dogs[0].Name, Is.EqualTo(EarlsName));
        }

        [Test]
        public void DaniHasNoTopDog()
        {
            var dani = _realm.DynamicApi.All("DynamicOwner").ToArray().Single(p => p.Name == "Dani");
            Assert.That(dani.TopDog, Is.Null);
        }

        [Test]
        public void DaniHasNoDogs()
        {
            var dani = _realm.DynamicApi.All("DynamicOwner").ToArray().Single(p => p.Name == "Dani");
            Assert.That(dani.Dogs.Count, Is.EqualTo(0));  // ToMany relationships always return a RealmList
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
            var dani = _realm.DynamicApi.All("DynamicOwner").ToArray().Single(p => p.Name == "Dani");
            Assert.Throws<ArgumentOutOfRangeException>(() => dani.Dogs.RemoveAt(0));
            var bilbo = _realm.DynamicApi.All("DynamicDog").ToArray().Single(p => p.Name == BilbosName);
            dynamic scratch;  // for assignment in following getters
            Assert.Throws<ArgumentOutOfRangeException>(() => dani.Dogs.Insert(-1, bilbo));
            Assert.Throws<ArgumentOutOfRangeException>(() => dani.Dogs.Insert(1, bilbo));
            Assert.Throws<ArgumentOutOfRangeException>(() => scratch = dani.Dogs[0]);
        }

        [Test]
        public void TestExceptionsFromIteratingEmptyList()
        {
            var dani = _realm.DynamicApi.All("DynamicOwner").ToArray().Single(p => p.Name == "Dani");
            var iter = dani.Dogs.GetEnumerator();
            Assert.IsNotNull(iter);
            var movedOnToFirstItem = iter.MoveNext();
            Assert.That(movedOnToFirstItem, Is.False);
            dynamic currentDog;
            Assert.Throws<ArgumentOutOfRangeException>(() => currentDog = iter.Current);
        }

        [Test]
        public void TestExceptionsFromTimsDogsOutOfRange()
        {
            var tim = _realm.DynamicApi.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");
            Assert.Throws<ArgumentOutOfRangeException>(() => tim.Dogs.RemoveAt(4));
            var bilbo = _realm.DynamicApi.All("DynamicDog").ToArray().Single(p => p.Name == BilbosName);
            dynamic scratch;  // for assignment in following getters
            Assert.Throws<ArgumentOutOfRangeException>(() => tim.Dogs.Insert(-1, bilbo));
            Assert.Throws<ArgumentOutOfRangeException>(() => tim.Dogs.Insert(3, bilbo));
            Assert.Throws<ArgumentOutOfRangeException>(() => scratch = tim.Dogs[99]);
        }

        [Test]
        public void Backlinks()
        {
            var tim = _realm.DynamicApi.All("DynamicOwner").ToArray().Single(o => o.Name == "Tim");
            foreach (var dog in tim.Dogs)
            {
                Assert.That(dog.Owners, Is.EquivalentTo(new[] { tim }));
            }

            var dani = _realm.DynamicApi.All("DynamicOwner").ToArray().Single(o => o.Name == "Dani");
            var maggie = _realm.DynamicApi.All("DynamicDog").ToArray().Single(d => d.Name == "Maggie Mongrel");
            Assert.That(maggie.Owners, Is.Empty);

            _realm.Write(() =>
            {
                dani.Dogs.Add(maggie);
            });

            Assert.That(maggie.Owners, Is.EquivalentTo(new[] { dani }));
        }

        [Test]
        public void DynamicBacklinks()
        {
            var tim = _realm.DynamicApi.All("DynamicOwner").ToArray().Single(o => o.Name == "Tim");
            var topOwners = tim.TopDog.GetBacklinks("DynamicOwner", "TopDog");

            Assert.That(topOwners, Is.EquivalentTo(new[] { tim }));

            var dani = _realm.DynamicApi.All("DynamicOwner").ToArray().Single(o => o.Name == "Dani");
            var maggie = _realm.DynamicApi.All("DynamicDog").ToArray().Single(d => d.Name == "Maggie Mongrel");
            Assert.That(maggie.GetBacklinks("DynamicOwner", "TopDog"), Is.Empty);

            _realm.Write(() =>
            {
                dani.TopDog = maggie;
            });

            Assert.That(maggie.GetBacklinks("DynamicOwner", "TopDog"), Is.EquivalentTo(new[] { dani }));
        }

        [Test]
        public void PrimitiveList()
        {
            var tim = _realm.DynamicApi.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");

            Assert.That(tim.Tags.Count, Is.EqualTo(0));

            _realm.Write(() =>
            {
                tim.Tags.Add("First");
            });

            Assert.That(tim.Tags.Count, Is.EqualTo(1));
            Assert.That(tim.Tags[0], Is.EqualTo("First"));
            Assert.That(((IEnumerable<dynamic>)tim.Tags).First(), Is.EqualTo("First"));

            _realm.Write(() =>
            {
                tim.Tags.Clear();
            });

            Assert.That(tim.Tags, Is.Empty);
        }

        [Test]
        public void Dictionary_TryGetValue()
        {
            TestHelpers.IgnoreOnAOT("byref delegate is not implemented in the dynamic runtime on Mono AOT.");

            var tim = _realm.DynamicApi.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");

            var bilbo = TryGetValue(tim.DogsDictionary, BilbosName, out bool hasBilbo);
            Assert.That(hasBilbo, Is.True);
            Assert.That(bilbo, Is.Not.Null);

            var pipi = TryGetValue(tim.DogsDictionary, PipilottasName, out bool hasPipi);
            Assert.That(hasPipi, Is.False);
            Assert.That(pipi, Is.Null);

            var bilbosTag = TryGetValue(tim.TagsDictionary, BilbosName, out bool hasBilbosTag);
            Assert.That(hasBilbosTag, Is.True);
            Assert.That(bilbosTag, Is.EqualTo("great"));

            var pipisTag = TryGetValue(tim.TagsDictionary, PipilottasName, out bool hasPipisTag);
            Assert.That(hasPipisTag, Is.False);
            Assert.That(pipisTag, Is.Null);
        }

        [Test]
        public void Dictionary_Count()
        {
            var tim = _realm.DynamicApi.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");

            Assert.That(tim.DogsDictionary.Count, Is.EqualTo(2));
            Assert.That(tim.TagsDictionary.Count, Is.EqualTo(2));
        }

        [Test]
        public void Dictionary_Get()
        {
            TestHelpers.IgnoreOnAOT("Indexing dynamic dictionaries is not supported on AOT platforms.");

            var tim = _realm.DynamicApi.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");

            var bilbo = tim.DogsDictionary[BilbosName];
            Assert.That(bilbo, Is.Not.Null);
            Assert.That(bilbo.Name, Is.EqualTo(BilbosName));

            var bilbosTag = tim.TagsDictionary[BilbosName];
            Assert.That(bilbosTag, Is.EqualTo("great"));

            Assert.Throws<KeyNotFoundException>(() => _ = tim.DogsDictionary[PipilottasName]);
            Assert.Throws<KeyNotFoundException>(() => _ = tim.TagsDictionary[PipilottasName]);
        }

        [Test]
        public void Dictionary_Set()
        {
            TestHelpers.IgnoreOnAOT("Indexing dynamic dictionaries is not supported on AOT platforms.");

            var tim = _realm.DynamicApi.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");

            _realm.Write(() =>
            {
                var pipi = _realm.DynamicApi.CreateObject("DynamicDog", null);
                pipi.Name = PipilottasName;
                pipi.Color = "Orange";

                tim.DogsDictionary[pipi.Name] = pipi;
                tim.TagsDictionary[pipi.Name] = "cheerful";
            });

            Assert.That(tim.DogsDictionary.Count, Is.EqualTo(3));
            Assert.That(tim.DogsDictionary[PipilottasName].Name, Is.EqualTo(PipilottasName));

            Assert.That(tim.TagsDictionary.Count, Is.EqualTo(3));
            Assert.That(tim.TagsDictionary[PipilottasName], Is.EqualTo("cheerful"));

            _realm.Write(() =>
            {
                tim.DogsDictionary[PipilottasName] = null;
                tim.TagsDictionary[PipilottasName] = null;
            });

            Assert.That(tim.DogsDictionary.Count, Is.EqualTo(3));
            Assert.That(tim.DogsDictionary[PipilottasName], Is.Null);

            Assert.That(tim.TagsDictionary.Count, Is.EqualTo(3));
            Assert.That(tim.TagsDictionary[PipilottasName], Is.Null);
        }

        [Test]
        public void Dictionary_Add()
        {
            TestHelpers.IgnoreOnAOT("Indexing dynamic dictionaries is not supported on AOT platforms.");

            var tim = _realm.DynamicApi.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");

            _realm.Write(() =>
            {
                var pipi = _realm.DynamicApi.CreateObject("DynamicDog", null);
                pipi.Name = PipilottasName;
                pipi.Color = "Orange";

                tim.DogsDictionary.Add(PipilottasName, pipi);
                tim.TagsDictionary.Add(PipilottasName, "cheerful");
            });

            Assert.That(tim.DogsDictionary.Count, Is.EqualTo(3));
            Assert.That(tim.DogsDictionary[PipilottasName].Name, Is.EqualTo(PipilottasName));

            Assert.That(tim.TagsDictionary.Count, Is.EqualTo(3));
            Assert.That(tim.TagsDictionary[PipilottasName], Is.EqualTo("cheerful"));

            Assert.Throws<ArgumentException>(() =>
            {
                _realm.Write(() =>
                {
                    tim.DogsDictionary.Add(PipilottasName, null);
                });
            }, $"An item with the key 'Pipilotta' has already been added.");

            Assert.Throws<ArgumentException>(() =>
            {
                _realm.Write(() =>
                {
                    tim.TagsDictionary.Add(PipilottasName, null);
                });
            }, $"An item with the key 'Pipilotta' has already been added.");

            Assert.That(tim.DogsDictionary.Count, Is.EqualTo(3));
            Assert.That(tim.DogsDictionary[PipilottasName], Is.Not.Null);

            Assert.That(tim.TagsDictionary.Count, Is.EqualTo(3));
            Assert.That(tim.TagsDictionary[PipilottasName], Is.EqualTo("cheerful"));

            _realm.Write(() =>
            {
                tim.DogsDictionary.Add("Void", null);
                tim.TagsDictionary.Add("Void", null);
            });

            Assert.That(tim.DogsDictionary.Count, Is.EqualTo(4));
            Assert.That(tim.DogsDictionary["Void"], Is.Null);

            Assert.That(tim.TagsDictionary.Count, Is.EqualTo(4));
            Assert.That(tim.TagsDictionary["Void"], Is.Null);
        }

        [Test]
        public void Dictionary_Keys()
        {
            var tim = _realm.DynamicApi.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");
            Assert.That(tim.DogsDictionary.Keys, Is.EquivalentTo(new[] { BilbosName, EarlsName }));
            Assert.That(tim.TagsDictionary.Keys, Is.EquivalentTo(new[] { BilbosName, EarlsName }));
        }

        [Test]
        public void Dictionary_Values()
        {
            var tim = _realm.DynamicApi.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");

            var dogNames = new List<string>();

            foreach (var dog in tim.DogsDictionary.Values)
            {
                dogNames.Add(dog.Name);
            }

            Assert.That(dogNames, Is.EquivalentTo(new[] { BilbosName, EarlsName }));
            Assert.That(tim.TagsDictionary.Values, Is.EquivalentTo(new[] { "great", "playful" }));
        }

        [Test]
        public void Dictionary_Iteration()
        {
            var tim = _realm.DynamicApi.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");
            var dogNames = new List<string>();
            var dogColors = new List<string>();

            foreach (var kvp in tim.DogsDictionary)
            {
                dogNames.Add(kvp.Key);
                dogColors.Add(kvp.Value.Color);
            }

            Assert.That(dogNames, Is.EquivalentTo(new[] { BilbosName, EarlsName }));
            Assert.That(dogColors, Is.EquivalentTo(new[] { "Black", "White" }));

            var tags = new List<string>();
            dogNames.Clear();

            foreach (var kvp in tim.TagsDictionary)
            {
                dogNames.Add(kvp.Key);
                tags.Add(kvp.Value);
            }

            Assert.That(dogNames, Is.EquivalentTo(new[] { BilbosName, EarlsName }));
            Assert.That(tags, Is.EquivalentTo(new[] { "great", "playful" }));
        }

        [Test]
        public void Dictionary_Remove()
        {
            var tim = _realm.DynamicApi.All("DynamicOwner").ToArray().Single(p => p.Name == "Tim");

            Assert.That(tim.DogsDictionary.Count, Is.EqualTo(2));

            _realm.Write(() =>
            {
                tim.DogsDictionary.Remove(BilbosName);
                tim.TagsDictionary.Remove(BilbosName);
            });

            Assert.That(tim.DogsDictionary.Count, Is.EqualTo(1));
            Assert.That(tim.TagsDictionary.Count, Is.EqualTo(1));
        }

        // Suggested https://stackoverflow.com/a/10021436/1649102
        private static TValue TryGetValue<TKey, TValue>(IDictionary<TKey, TValue> dict, TKey value, out bool found)
        {
            TValue result;
            found = dict.TryGetValue(value, out result);
            return result;
        }
    }
}