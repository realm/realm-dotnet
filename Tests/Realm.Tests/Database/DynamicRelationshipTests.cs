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
    [Preserve(AllMembers = true)]
    public class DynamicRelationshipTests : RealmInstanceTest
    {
        private const string BilbosName = "Bilbo Fleabaggins";
        private const string EarlsName = "Earl Yippington III";
        private const string PipilottasName = "Pipilotta";
        private const string MaggiesName = "Maggie Mongrel";

        private void RunTestInAllModes(Action<Realm> test)
        {
            foreach (var isDynamic in new[] { true, false })
            {
                var config = new RealmConfiguration(Guid.NewGuid().ToString())
                {
                    Schema = new[] { typeof(DynamicOwner), typeof(DynamicDog) },
                    IsDynamic = isDynamic
                };

                using var realm = GetRealm(config);
                realm.Write(() =>
                {
                    var ownerTim = (IRealmObject)(object)realm.DynamicApi.CreateObject(nameof(DynamicOwner), null);
                    ownerTim.DynamicApi.Set(nameof(DynamicOwner.Name), "Tim");

                    var dogBilbo = (IRealmObject)(object)realm.DynamicApi.CreateObject(nameof(DynamicDog), null);
                    dogBilbo.DynamicApi.Set(nameof(DynamicDog.Name), BilbosName);
                    dogBilbo.DynamicApi.Set(nameof(DynamicDog.Color), "Black");

                    ownerTim.DynamicApi.Set(nameof(DynamicOwner.TopDog), RealmValue.Object(dogBilbo));  // set a one-one relationship
                    GetDogs(ownerTim).Add(dogBilbo);
                    GetDogsDict(ownerTim).Add(BilbosName, dogBilbo);
                    GetTagsDict(ownerTim).Add(BilbosName, "great");
                    ownerTim.DynamicApi.GetSet<IRealmObject>(nameof(DynamicOwner.DogsSet)).Add(dogBilbo);
                    ownerTim.DynamicApi.GetSet<string>(nameof(DynamicOwner.TagsSet)).Add("responsible");

                    var dogEarl = (IRealmObject)(object)realm.DynamicApi.CreateObject(nameof(DynamicDog), null);
                    dogEarl.DynamicApi.Set(nameof(DynamicDog.Name), EarlsName);
                    dogEarl.DynamicApi.Set(nameof(DynamicDog.Color), "White");

                    GetDogs(ownerTim).Add(dogEarl);
                    GetDogsDict(ownerTim).Add(EarlsName, dogEarl);
                    GetTagsDict(ownerTim).Add(EarlsName, "playful");
                    ownerTim.DynamicApi.GetSet<IRealmObject>(nameof(DynamicOwner.DogsSet)).Add(dogEarl);
                    ownerTim.DynamicApi.GetSet<string>(nameof(DynamicOwner.TagsSet)).Add("coffee lover");

                    // lonely people and dogs
                    var ownerDani = (IRealmObject)(object)realm.DynamicApi.CreateObject(nameof(DynamicOwner), null);
                    ownerDani.DynamicApi.Set(nameof(DynamicOwner.Name), "Dani");  // the dog-less

                    var dogMaggie = (IRealmObject)(object)realm.DynamicApi.CreateObject(nameof(DynamicDog), null);  // will remain unassigned
                    dogMaggie.DynamicApi.Set(nameof(DynamicDog.Name), MaggiesName);
                    dogMaggie.DynamicApi.Set(nameof(DynamicDog.Color), "Grey");
                });

                test(realm);
            }
        }

        private void RunDynamicTestInAllModes(Action<Realm> test)
        {
            TestHelpers.IgnoreOnUnity();

            RunTestInAllModes(test);
        }

        private static IRealmObject FindOwner(Realm realm, string name = "Tim")
            => ((IQueryable<IRealmObject>)realm.DynamicApi.All(nameof(DynamicOwner))).ToArray().Single(o => o.DynamicApi.Get<string>(nameof(DynamicOwner.Name)) == name);

        private static IRealmObject FindDog(Realm realm, string name = MaggiesName)
            => ((IQueryable<IRealmObject>)realm.DynamicApi.All(nameof(DynamicDog))).ToArray().Single(o => o.DynamicApi.Get<string>(nameof(DynamicDog.Name)) == name);

        private static IList<IRealmObject> GetDogs(IRealmObject owner)
            => owner.DynamicApi.GetList<IRealmObject>(nameof(DynamicOwner.Dogs));

        private static IDictionary<string, IRealmObject> GetDogsDict(IRealmObject owner)
            => owner.DynamicApi.GetDictionary<IRealmObject>(nameof(DynamicOwner.DogsDictionary));

        private static ISet<IRealmObject> GetDogsSet(IRealmObject owner)
            => owner.DynamicApi.GetSet<IRealmObject>(nameof(DynamicOwner.DogsSet));

        private static IList<string> GetTags(IRealmObject owner)
            => owner.DynamicApi.GetList<string>(nameof(DynamicOwner.Tags));

        private static IDictionary<string, string> GetTagsDict(IRealmObject owner)
            => owner.DynamicApi.GetDictionary<string>(nameof(DynamicOwner.TagsDictionary));

        private static ISet<string> GetTagsSet(IRealmObject owner)
            => owner.DynamicApi.GetSet<string>(nameof(DynamicOwner.TagsSet));

        [Test]
        public void TimHasATopDog()
        {
            RunTestInAllModes(realm =>
            {
                var tim = FindOwner(realm);
                Assert.That(tim.DynamicApi.Get<IRealmObject>(nameof(DynamicOwner.TopDog)).DynamicApi.Get<string>(nameof(DynamicDog.Name)), Is.EqualTo(BilbosName));
            });
        }

        [Test]
        public void TimHasATopDog_Dynamic()
        {
            RunDynamicTestInAllModes(realm =>
            {
                dynamic tim = FindOwner(realm);
                Assert.That(tim.TopDog.Name, Is.EqualTo(BilbosName));
            });
        }

        [Test]
        public void TimHasTwoIterableDogs()
        {
            RunTestInAllModes(realm =>
            {
                var tim = FindOwner(realm);
                var dogNames = new List<string>();

                //// using foreach here is deliberately testing that syntax
                foreach (var dog in GetDogs(tim))
                {
                    dogNames.Add(dog.DynamicApi.Get<string>(nameof(DynamicDog.Name)));
                }

                Assert.That(dogNames, Is.EquivalentTo(new[] { BilbosName, EarlsName }));
            });
        }

        [Test]
        public void TimHasTwoIterableDogs_Dynamic()
        {
            RunDynamicTestInAllModes(realm =>
            {
                dynamic tim = FindOwner(realm);
                var dogNames = new List<string>();

                //// using foreach here is deliberately testing that syntax
                foreach (var dog in tim.Dogs)
                {
                    dogNames.Add(dog.Name);
                }

                Assert.That(dogNames, Is.EquivalentTo(new[] { BilbosName, EarlsName }));
            });
        }

        [Test]
        public void TimHasTwoIterableDogsListed()
        {
            RunTestInAllModes(realm =>
            {
                var tim = FindOwner(realm);
                var dogNames = new List<string>();
                var dogList = GetDogs(tim).ToList();  // this used to crash - issue 299
                foreach (var dog in dogList)
                {
                    dogNames.Add(dog.DynamicApi.Get<string>(nameof(DynamicDog.Name)));
                }

                Assert.That(dogNames, Is.EquivalentTo(new[] { BilbosName, EarlsName }));
            });
        }

        /// <summary>
        /// Check if ToList can be invoked on a related RealmResults.
        /// </summary>
        [Test]
        public void TimHasTwoIterableDogsListed_Dynamic()
        {
            RunDynamicTestInAllModes(realm =>
            {
                dynamic tim = FindOwner(realm);
                var dogNames = new List<string>();
                var dogList = Enumerable.ToList<dynamic>(tim.Dogs);  // this used to crash - issue 299
                foreach (var dog in dogList)
                {
                    dogNames.Add(dog.Name);
                }

                Assert.That(dogNames, Is.EquivalentTo(new[] { BilbosName, EarlsName }));
            });
        }

        [Test]
        public void TimRetiredHisTopDog()
        {
            RunTestInAllModes(realm =>
            {
                var tim = FindOwner(realm);

                realm.Write(() =>
                {
                    tim.DynamicApi.Set(nameof(DynamicOwner.TopDog), RealmValue.Null);
                });

                var tim2 = FindOwner(realm);
                Assert.That(tim2.DynamicApi.Get<IRealmObject>(nameof(DynamicOwner.TopDog)), Is.Null);  // the dog departure was saved
            });
        }

        [Test]
        public void TimRetiredHisTopDog_Dynamic()
        {
            RunDynamicTestInAllModes(realm =>
            {
                dynamic tim = FindOwner(realm);
                realm.Write(() =>
                {
                    tim.TopDog = null;
                });

                dynamic tim2 = FindOwner(realm);
                Assert.That(tim2.TopDog, Is.Null);  // the dog departure was saved
            });
        }

        [Test]
        public void TimAddsADogLater()
        {
            RunTestInAllModes(realm =>
            {
                var tim = FindOwner(realm);
                Assert.That(GetDogs(tim).Count, Is.EqualTo(2));

                realm.Write(() =>
                {
                    var maggie = FindDog(realm);
                    GetDogs(tim).Add(maggie);
                });

                var tim2 = FindOwner(realm);
                Assert.That(GetDogs(tim2).Count, Is.EqualTo(3));
                Assert.That(GetDogs(tim2)[2].DynamicApi.Get<string>(nameof(DynamicDog.Name)), Is.EqualTo(MaggiesName));
            });
        }

        [Test]
        public void TimAddsADogLater_Dynamic()
        {
            RunDynamicTestInAllModes(realm =>
            {
                dynamic tim = FindOwner(realm);
                Assert.That(tim.Dogs.Count, Is.EqualTo(2));

                realm.Write(() =>
                {
                    dynamic maggie = FindDog(realm);
                    tim.Dogs.Add(maggie);
                });

                dynamic tim2 = FindOwner(realm);
                Assert.That(tim2.Dogs.Count, Is.EqualTo(3));
                Assert.That(tim2.Dogs[2].Name, Is.EqualTo(MaggiesName));
            });
        }

        [Test]
        public void TimAddsADogByInsert()
        {
            RunTestInAllModes(realm =>
            {
                var tim = FindOwner(realm);
                Assert.That(GetDogs(tim).Count, Is.EqualTo(2));

                realm.Write(() =>
                {
                    var maggie = FindDog(realm);
                    GetDogs(tim).Insert(1, maggie);
                });

                var tim2 = FindOwner(realm);
                Assert.That(GetDogs(tim2).Count, Is.EqualTo(3));
                Assert.That(GetDogs(tim2)[1].DynamicApi.Get<string>(nameof(DynamicDog.Name)), Is.EqualTo(MaggiesName));
                Assert.That(GetDogs(tim2)[2].DynamicApi.Get<string>(nameof(DynamicDog.Name)), Is.EqualTo(EarlsName));
            });
        }

        [Test]
        public void TimAddsADogByInsert_Dynamic()
        {
            RunDynamicTestInAllModes(realm =>
            {
                dynamic tim = FindOwner(realm);
                Assert.That(tim.Dogs.Count, Is.EqualTo(2));

                realm.Write(() =>
                {
                    dynamic maggie = FindDog(realm);
                    tim.Dogs.Insert(1, maggie);
                });

                dynamic tim2 = FindOwner(realm);
                Assert.That(tim2.Dogs.Count, Is.EqualTo(3));
                Assert.That(tim2.Dogs[1].Name, Is.EqualTo(MaggiesName));
                Assert.That(tim2.Dogs[2].Name, Is.EqualTo(EarlsName));
            });
        }

        [Test]
        public void TimLosesHisDogsByOrder()
        {
            RunTestInAllModes(realm =>
            {
                var tim = FindOwner(realm);
                var timsDogs = GetDogs(tim);
                Assert.That(timsDogs.Count, Is.EqualTo(2));

                realm.Write(() =>
                {
                    timsDogs.RemoveAt(0);
                });

                var tim2 = FindOwner(realm);
                Assert.That(timsDogs.Count, Is.EqualTo(1));
                Assert.That(GetDogs(tim2).Count, Is.EqualTo(1));
                Assert.That(GetDogs(tim2)[0].DynamicApi.Get<string>(nameof(DynamicDog.Name)), Is.EqualTo(EarlsName));

                realm.Write(() =>
                {
                    timsDogs.RemoveAt(0);
                });

                var tim3 = FindOwner(realm);
                Assert.That(timsDogs.Count, Is.EqualTo(0));
                Assert.That(GetDogs(tim2).Count, Is.EqualTo(0));
                Assert.That(GetDogs(tim3).Count, Is.EqualTo(0)); // reloaded object has same empty related set
            });
        }

        [Test]
        public void TimLosesHisDogsByOrder_Dynamic()
        {
            RunDynamicTestInAllModes(realm =>
            {
                dynamic tim = FindOwner(realm);
                Assert.That(tim.Dogs.Count, Is.EqualTo(2));

                realm.Write(() =>
                {
                    tim.Dogs.RemoveAt(0);
                });

                dynamic tim2 = FindOwner(realm);
                Assert.That(tim2.Dogs.Count, Is.EqualTo(1));
                Assert.That(tim2.Dogs[0].Name, Is.EqualTo(EarlsName));

                realm.Write(() =>
                {
                    tim.Dogs.RemoveAt(0);
                });

                dynamic tim3 = FindOwner(realm);
                Assert.That(tim2.Dogs.Count, Is.EqualTo(0));
                Assert.That(tim3.Dogs.Count, Is.EqualTo(0)); // reloaded object has same empty related set
            });
        }

        [Test]
        public void TimLosesHisDogsInOneClear()
        {
            RunTestInAllModes(realm =>
            {
                var tim = FindOwner(realm);
                var timsDogs = GetDogs(tim);
                Assert.That(timsDogs.Count, Is.EqualTo(2));
                realm.Write(() =>
                {
                    timsDogs.Clear();
                });

                var tim2 = FindOwner(realm);
                Assert.That(timsDogs.Count, Is.EqualTo(0));
                Assert.That(GetDogs(tim2).Count, Is.EqualTo(0));
            });
        }

        [Test]
        public void TimLosesHisDogsInOneClear_Dynamic()
        {
            RunDynamicTestInAllModes(realm =>
            {
                dynamic tim = FindOwner(realm);
                Assert.That(tim.Dogs.Count, Is.EqualTo(2));
                realm.Write(() =>
                {
                    tim.Dogs.Clear();
                });

                dynamic tim2 = FindOwner(realm);
                Assert.That(tim2.Dogs.Count, Is.EqualTo(0));
            });
        }

        [Test]
        public void TimLosesBilbo()
        {
            RunTestInAllModes(realm =>
            {
                var bilbo = FindDog(realm, BilbosName);
                var tim = FindOwner(realm);
                var timsDogs = GetDogs(tim);
                Assert.That(timsDogs.Count, Is.EqualTo(2));

                realm.Write(() =>
                {
                    timsDogs.Remove(bilbo);
                });

                var tim2 = FindOwner(realm);
                Assert.That(timsDogs.Count, Is.EqualTo(1));
                Assert.That(timsDogs[0].DynamicApi.Get<string>(nameof(DynamicDog.Name)), Is.EqualTo(EarlsName));
                Assert.That(GetDogs(tim2).Count, Is.EqualTo(1));
                Assert.That(GetDogs(tim2)[0].DynamicApi.Get<string>(nameof(DynamicDog.Name)), Is.EqualTo(EarlsName));
            });
        }

        [Test]
        public void TimLosesBilbo_Dynamic()
        {
            RunDynamicTestInAllModes(realm =>
            {
                dynamic bilbo = FindDog(realm, BilbosName);
                dynamic tim = FindOwner(realm);
                Assert.That(tim.Dogs.Count, Is.EqualTo(2));

                realm.Write(() =>
                {
                    tim.Dogs.Remove(bilbo);
                });

                dynamic tim2 = FindOwner(realm);
                Assert.That(tim2.Dogs.Count, Is.EqualTo(1));
                Assert.That(tim2.Dogs[0].Name, Is.EqualTo(EarlsName));
            });
        }

        [Test]
        public void DaniHasNoTopDog()
        {
            RunTestInAllModes(realm =>
            {
                var dani = FindOwner(realm, "Dani");
                Assert.That(dani.DynamicApi.Get<IRealmObject>(nameof(DynamicOwner.TopDog)), Is.Null);
            });
        }

        [Test]
        public void DaniHasNoTopDog_Dynamic()
        {
            RunDynamicTestInAllModes(realm =>
            {
                dynamic dani = FindOwner(realm, "Dani");
                Assert.That(dani.TopDog, Is.Null);
            });
        }

        [Test]
        public void DaniHasNoDogs()
        {
            RunTestInAllModes(realm =>
            {
                var dani = FindOwner(realm, "Dani");
                Assert.That(GetDogs(dani).Count, Is.EqualTo(0));
                var dogsIterated = 0;
                foreach (var d in GetDogs(dani))
                {
                    dogsIterated++;
                }

                Assert.That(dogsIterated, Is.EqualTo(0));
            });
        }

        [Test]
        public void DaniHasNoDogs_Dynamic()
        {
            RunDynamicTestInAllModes(realm =>
            {
                dynamic dani = FindOwner(realm, "Dani");
                Assert.That(dani.Dogs.Count, Is.EqualTo(0));  // ToMany relationships always return a RealmList
                var dogsIterated = 0;
                foreach (var d in dani.Dogs)
                {
                    dogsIterated++;
                }

                Assert.That(dogsIterated, Is.EqualTo(0));
            });
        }

        [Test]
        public void TestExceptionsFromEmptyListOutOfRange()
        {
            RunTestInAllModes(realm =>
            {
                var dani = FindOwner(realm, "Dani");
                Assert.Throws<ArgumentOutOfRangeException>(() => GetDogs(dani).RemoveAt(0));

                var bilbo = FindDog(realm, BilbosName);
                Assert.Throws<ArgumentOutOfRangeException>(() => GetDogs(dani).Insert(-1, bilbo));
                Assert.Throws<ArgumentOutOfRangeException>(() => GetDogs(dani).Insert(1, bilbo));
                Assert.Throws<ArgumentOutOfRangeException>(() => _ = GetDogs(dani)[0]);
            });
        }

        [Test]
        public void TestExceptionsFromEmptyListOutOfRange_Dynamic()
        {
            RunDynamicTestInAllModes(realm =>
            {
                dynamic dani = FindOwner(realm, "Dani");
                Assert.Throws<ArgumentOutOfRangeException>(() => dani.Dogs.RemoveAt(0));

                dynamic bilbo = FindDog(realm, BilbosName);
                Assert.Throws<ArgumentOutOfRangeException>(() => dani.Dogs.Insert(-1, bilbo));
                Assert.Throws<ArgumentOutOfRangeException>(() => dani.Dogs.Insert(1, bilbo));
                Assert.Throws<ArgumentOutOfRangeException>(() => _ = dani.Dogs[0]);
            });
        }

        [Test]
        public void TestExceptionsFromIteratingEmptyList()
        {
            RunTestInAllModes(realm =>
            {
                var dani = FindOwner(realm, "Dani");
                var iter = GetDogs(dani).GetEnumerator();
                Assert.IsNotNull(iter);

                var movedOnToFirstItem = iter.MoveNext();
                Assert.That(movedOnToFirstItem, Is.False);

                Assert.Throws<ArgumentOutOfRangeException>(() => _ = iter.Current);
            });
        }

        [Test]
        public void TestExceptionsFromIteratingEmptyList_Dynamic()
        {
            RunDynamicTestInAllModes(realm =>
            {
                dynamic dani = FindOwner(realm, "Dani");
                var iter = dani.Dogs.GetEnumerator();
                Assert.IsNotNull(iter);

                var movedOnToFirstItem = iter.MoveNext();
                Assert.That(movedOnToFirstItem, Is.False);

                Assert.Throws<ArgumentOutOfRangeException>(() => _ = iter.Current);
            });
        }

        [Test]
        public void TestExceptionsFromTimsDogsOutOfRange()
        {
            RunTestInAllModes(realm =>
            {
                var tim = FindOwner(realm);
                var timsDogs = GetDogs(tim);
                Assert.Throws<ArgumentOutOfRangeException>(() => timsDogs.RemoveAt(4));
                var bilbo = FindDog(realm, BilbosName);

                Assert.Throws<ArgumentOutOfRangeException>(() => timsDogs.Insert(-1, bilbo));
                Assert.Throws<ArgumentOutOfRangeException>(() => timsDogs.Insert(3, bilbo));
                Assert.Throws<ArgumentOutOfRangeException>(() => _ = timsDogs[99]);
            });
        }

        [Test]
        public void TestExceptionsFromTimsDogsOutOfRange_Dynamic()
        {
            RunDynamicTestInAllModes(realm =>
            {
                dynamic tim = FindOwner(realm);
                Assert.Throws<ArgumentOutOfRangeException>(() => tim.Dogs.RemoveAt(4));
                dynamic bilbo = FindDog(realm, BilbosName);

                Assert.Throws<ArgumentOutOfRangeException>(() => tim.Dogs.Insert(-1, bilbo));
                Assert.Throws<ArgumentOutOfRangeException>(() => tim.Dogs.Insert(3, bilbo));
                Assert.Throws<ArgumentOutOfRangeException>(() => _ = tim.Dogs[99]);
            });
        }

        [Test]
        public void Backlinks()
        {
            RunTestInAllModes(realm =>
            {
                var tim = FindOwner(realm);
                foreach (var dog in GetDogs(tim))
                {
                    var owners = dog.DynamicApi.GetBacklinks(nameof(DynamicDog.Owners));
                    Assert.That(owners, Is.EquivalentTo(new[] { tim }));
                }

                var dani = FindOwner(realm, "Dani");
                var maggie = FindDog(realm);
                var maggiesOwners = maggie.DynamicApi.GetBacklinks(nameof(DynamicDog.Owners));
                Assert.That(maggiesOwners, Is.Empty);

                realm.Write(() =>
                {
                    GetDogs(dani).Add(maggie);
                });

                Assert.That(maggiesOwners, Is.EquivalentTo(new[] { dani }));
            });
        }

        [Test]
        public void Backlinks_Dynamic()
        {
            RunDynamicTestInAllModes(realm =>
            {
                dynamic tim = FindOwner(realm);
                foreach (var dog in tim.Dogs)
                {
                    Assert.That(dog.Owners, Is.EquivalentTo(new[] { tim }));
                }

                dynamic dani = FindOwner(realm, "Dani");
                dynamic maggie = FindDog(realm);
                Assert.That(maggie.Owners, Is.Empty);

                realm.Write(() =>
                {
                    dani.Dogs.Add(maggie);
                });

                Assert.That(maggie.Owners, Is.EquivalentTo(new[] { dani }));
            });
        }

        [Test]
        public void DynamicBacklinks()
        {
            RunTestInAllModes(realm =>
            {
                var tim = FindOwner(realm);
                var topOwners = tim.DynamicApi.Get<IRealmObject>(nameof(DynamicOwner.TopDog)).DynamicApi.GetBacklinksFromType(nameof(DynamicOwner), nameof(DynamicOwner.TopDog));

                Assert.That(topOwners, Is.EquivalentTo(new[] { tim }));

                var dani = FindOwner(realm, "Dani");
                var maggie = FindDog(realm);

                Assert.That(maggie.DynamicApi.GetBacklinksFromType(nameof(DynamicOwner), nameof(DynamicOwner.TopDog)), Is.Empty);

                realm.Write(() =>
                {
                    dani.DynamicApi.Set(nameof(DynamicOwner.TopDog), RealmValue.Object(maggie));
                });

                Assert.That(maggie.DynamicApi.GetBacklinksFromType(nameof(DynamicOwner), nameof(DynamicOwner.TopDog)), Is.EquivalentTo(new[] { dani }));
            });
        }

        [Test]
        public void PrimitiveList()
        {
            RunTestInAllModes(realm =>
            {
                var tim = FindOwner(realm);

                var timsTags = GetTags(tim);
                Assert.That(timsTags.Count, Is.EqualTo(0));

                realm.Write(() =>
                {
                    timsTags.Add("First");
                });

                Assert.That(timsTags.Count, Is.EqualTo(1));
                Assert.That(timsTags[0], Is.EqualTo("First"));
                Assert.That(timsTags.First(), Is.EqualTo("First"));

                realm.Write(() =>
                {
                    timsTags.Clear();
                });

                Assert.That(timsTags, Is.Empty);
            });
        }

        [Test]
        public void PrimitiveList_Dynamic()
        {
            RunDynamicTestInAllModes(realm =>
            {
                dynamic tim = FindOwner(realm);

                Assert.That(tim.Tags.Count, Is.EqualTo(0));

                realm.Write(() =>
                {
                    tim.Tags.Add("First");
                });

                Assert.That(tim.Tags.Count, Is.EqualTo(1));
                Assert.That(tim.Tags[0], Is.EqualTo("First"));
                Assert.That(((IEnumerable<dynamic>)tim.Tags).First(), Is.EqualTo("First"));

                realm.Write(() =>
                {
                    tim.Tags.Clear();
                });

                Assert.That(tim.Tags, Is.Empty);
            });
        }

        [Test]
        public void Dictionary_TryGetValue()
        {
            RunTestInAllModes(realm =>
            {
                var tim = FindOwner(realm);

                var timsDogs = GetDogsDict(tim);
                var hasBilbo = timsDogs.TryGetValue(BilbosName, out var bilbo);
                Assert.That(hasBilbo, Is.True);
                Assert.That(bilbo, Is.Not.Null);

                var hasPipi = timsDogs.TryGetValue(PipilottasName, out var pipi);
                Assert.That(hasPipi, Is.False);
                Assert.That(pipi, Is.Null);

                var timsTags = GetTagsDict(tim);
                var hasBilbosTag = timsTags.TryGetValue(BilbosName, out var bilbosTag);
                Assert.That(hasBilbosTag, Is.True);
                Assert.That(bilbosTag, Is.EqualTo("great"));

                var hasPipisTag = timsTags.TryGetValue(PipilottasName, out var pipisTag);
                Assert.That(hasPipisTag, Is.False);
                Assert.That(pipisTag, Is.Null);
            });
        }

        [Test]
        public void Dictionary_TryGetValue_Dynamic()
        {
            TestHelpers.IgnoreOnAOT("byref delegate is not implemented in the dynamic runtime on Mono AOT.");

            RunDynamicTestInAllModes(realm =>
            {
                dynamic tim = FindOwner(realm);

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
            });
        }

        [Test]
        public void Dictionary_Count()
        {
            RunTestInAllModes(realm =>
            {
                var tim = FindOwner(realm);

                Assert.That(GetDogsDict(tim).Count, Is.EqualTo(2));
                Assert.That(GetTagsDict(tim).Count, Is.EqualTo(2));
            });
        }

        [Test]
        public void Dictionary_Count_Dynamic()
        {
            RunDynamicTestInAllModes(realm =>
            {
                dynamic tim = FindOwner(realm);

                Assert.That(tim.DogsDictionary.Count, Is.EqualTo(2));
                Assert.That(tim.TagsDictionary.Count, Is.EqualTo(2));
            });
        }

        [Test]
        public void Dictionary_Get()
        {
            RunTestInAllModes(realm =>
            {
                var tim = FindOwner(realm);

                var bilbo = GetDogsDict(tim)[BilbosName];
                Assert.That(bilbo, Is.Not.Null);
                Assert.That(bilbo.DynamicApi.Get<string>(nameof(DynamicDog.Name)), Is.EqualTo(BilbosName));

                var bilbosTag = GetTagsDict(tim)[BilbosName];
                Assert.That(bilbosTag, Is.EqualTo("great"));

                Assert.Throws<KeyNotFoundException>(() => _ = GetDogsDict(tim)[PipilottasName]);
                Assert.Throws<KeyNotFoundException>(() => _ = GetTagsDict(tim)[PipilottasName]);
            });
        }

        [Test]
        public void Dictionary_Get_Dynamic()
        {
            TestHelpers.IgnoreOnAOT("Indexing dynamic dictionaries is not supported on AOT platforms.");

            RunDynamicTestInAllModes(realm =>
            {
                dynamic tim = FindOwner(realm);

                var bilbo = tim.DogsDictionary[BilbosName];
                Assert.That(bilbo, Is.Not.Null);
                Assert.That(bilbo.Name, Is.EqualTo(BilbosName));

                var bilbosTag = tim.TagsDictionary[BilbosName];
                Assert.That(bilbosTag, Is.EqualTo("great"));

                Assert.Throws<KeyNotFoundException>(() => _ = tim.DogsDictionary[PipilottasName]);
                Assert.Throws<KeyNotFoundException>(() => _ = tim.TagsDictionary[PipilottasName]);
            });
        }

        [Test]
        public void Dictionary_Set()
        {
            RunTestInAllModes(realm =>
            {
                var tim = FindOwner(realm);

                realm.Write(() =>
                {
                    var pipi = (IRealmObject)(object)realm.DynamicApi.CreateObject(nameof(DynamicDog), null);
                    pipi.DynamicApi.Set(nameof(DynamicDog.Name), PipilottasName);
                    pipi.DynamicApi.Set(nameof(DynamicDog.Color), "Orange");

                    GetDogsDict(tim)[PipilottasName] = pipi;
                    GetTagsDict(tim)[PipilottasName] = "cheerful";
                });

                Assert.That(GetDogsDict(tim).Count, Is.EqualTo(3));
                Assert.That(GetDogsDict(tim)[PipilottasName].DynamicApi.Get<string>(nameof(DynamicDog.Name)), Is.EqualTo(PipilottasName));

                Assert.That(GetTagsDict(tim).Count, Is.EqualTo(3));
                Assert.That(GetTagsDict(tim)[PipilottasName], Is.EqualTo("cheerful"));

                realm.Write(() =>
                {
                    GetDogsDict(tim)[PipilottasName] = null;
                    GetTagsDict(tim)[PipilottasName] = null;
                });

                Assert.That(GetDogsDict(tim).Count, Is.EqualTo(3));
                Assert.That(GetDogsDict(tim)[PipilottasName], Is.Null);

                Assert.That(GetTagsDict(tim).Count, Is.EqualTo(3));
                Assert.That(GetTagsDict(tim)[PipilottasName], Is.Null);
            });
        }

        [Test]
        public void Dictionary_Set_Dynamic()
        {
            TestHelpers.IgnoreOnAOT("Indexing dynamic dictionaries is not supported on AOT platforms.");

            RunDynamicTestInAllModes(realm =>
            {
                dynamic tim = FindOwner(realm);

                realm.Write(() =>
                {
                    var pipi = realm.DynamicApi.CreateObject(nameof(DynamicDog), null);
                    pipi.Name = PipilottasName;
                    pipi.Color = "Orange";

                    tim.DogsDictionary[pipi.Name] = pipi;
                    tim.TagsDictionary[pipi.Name] = "cheerful";
                });

                Assert.That(tim.DogsDictionary.Count, Is.EqualTo(3));
                Assert.That(tim.DogsDictionary[PipilottasName].Name, Is.EqualTo(PipilottasName));

                Assert.That(tim.TagsDictionary.Count, Is.EqualTo(3));
                Assert.That(tim.TagsDictionary[PipilottasName], Is.EqualTo("cheerful"));

                realm.Write(() =>
                {
                    tim.DogsDictionary[PipilottasName] = null;
                    tim.TagsDictionary[PipilottasName] = null;
                });

                Assert.That(tim.DogsDictionary.Count, Is.EqualTo(3));
                Assert.That(tim.DogsDictionary[PipilottasName], Is.Null);

                Assert.That(tim.TagsDictionary.Count, Is.EqualTo(3));
                Assert.That(tim.TagsDictionary[PipilottasName], Is.Null);
            });
        }

        [Test]
        public void Dictionary_Add()
        {
            RunTestInAllModes(realm =>
            {
                var tim = FindOwner(realm);

                realm.Write(() =>
                {
                    var pipi = (IRealmObject)(object)realm.DynamicApi.CreateObject(nameof(DynamicDog), null);
                    pipi.DynamicApi.Set(nameof(DynamicDog.Name), PipilottasName);
                    pipi.DynamicApi.Set(nameof(DynamicDog.Color), "Orange");

                    GetDogsDict(tim).Add(PipilottasName, pipi);
                    GetTagsDict(tim).Add(PipilottasName, "cheerful");
                });

                Assert.That(GetDogsDict(tim).Count, Is.EqualTo(3));
                Assert.That(GetDogsDict(tim)[PipilottasName].DynamicApi.Get<string>(nameof(DynamicDog.Name)), Is.EqualTo(PipilottasName));

                Assert.That(GetTagsDict(tim).Count, Is.EqualTo(3));
                Assert.That(GetTagsDict(tim)[PipilottasName], Is.EqualTo("cheerful"));

                Assert.Throws<ArgumentException>(() =>
                {
                    realm.Write(() =>
                    {
                        GetDogsDict(tim).Add(PipilottasName, null);
                    });
                }, $"An item with the key 'Pipilotta' has already been added.");

                Assert.Throws<ArgumentException>(() =>
                {
                    realm.Write(() =>
                    {
                        GetTagsDict(tim).Add(PipilottasName, null);
                    });
                }, $"An item with the key 'Pipilotta' has already been added.");

                Assert.That(GetDogsDict(tim).Count, Is.EqualTo(3));
                Assert.That(GetDogsDict(tim)[PipilottasName], Is.Not.Null);

                Assert.That(GetTagsDict(tim).Count, Is.EqualTo(3));
                Assert.That(GetTagsDict(tim)[PipilottasName], Is.EqualTo("cheerful"));

                realm.Write(() =>
                {
                    GetDogsDict(tim).Add("Void", null);
                    GetTagsDict(tim).Add("Void", null);
                });

                Assert.That(GetDogsDict(tim).Count, Is.EqualTo(4));
                Assert.That(GetDogsDict(tim)["Void"], Is.Null);

                Assert.That(GetTagsDict(tim).Count, Is.EqualTo(4));
                Assert.That(GetTagsDict(tim)["Void"], Is.Null);
            });
        }

        [Test]
        public void Dictionary_Add_Dynamic()
        {
            TestHelpers.IgnoreOnAOT("Indexing dynamic dictionaries is not supported on AOT platforms.");

            RunDynamicTestInAllModes(realm =>
            {
                dynamic tim = FindOwner(realm);

                realm.Write(() =>
                {
                    var pipi = realm.DynamicApi.CreateObject(nameof(DynamicDog), null);
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
                    realm.Write(() =>
                    {
                        tim.DogsDictionary.Add(PipilottasName, null);
                    });
                }, $"An item with the key 'Pipilotta' has already been added.");

                Assert.Throws<ArgumentException>(() =>
                {
                    realm.Write(() =>
                    {
                        tim.TagsDictionary.Add(PipilottasName, null);
                    });
                }, $"An item with the key 'Pipilotta' has already been added.");

                Assert.That(tim.DogsDictionary.Count, Is.EqualTo(3));
                Assert.That(tim.DogsDictionary[PipilottasName], Is.Not.Null);

                Assert.That(tim.TagsDictionary.Count, Is.EqualTo(3));
                Assert.That(tim.TagsDictionary[PipilottasName], Is.EqualTo("cheerful"));

                realm.Write(() =>
                {
                    tim.DogsDictionary.Add("Void", null);
                    tim.TagsDictionary.Add("Void", null);
                });

                Assert.That(tim.DogsDictionary.Count, Is.EqualTo(4));
                Assert.That(tim.DogsDictionary["Void"], Is.Null);

                Assert.That(tim.TagsDictionary.Count, Is.EqualTo(4));
                Assert.That(tim.TagsDictionary["Void"], Is.Null);
            });
        }

        [Test]
        public void Dictionary_Keys()
        {
            RunTestInAllModes(realm =>
            {
                var tim = FindOwner(realm);
                Assert.That(GetDogsDict(tim).Keys, Is.EquivalentTo(new[] { BilbosName, EarlsName }));
                Assert.That(GetTagsDict(tim).Keys, Is.EquivalentTo(new[] { BilbosName, EarlsName }));
            });
        }

        [Test]
        public void Dictionary_Keys_Dynamic()
        {
            RunDynamicTestInAllModes(realm =>
            {
                dynamic tim = FindOwner(realm);
                Assert.That(tim.DogsDictionary.Keys, Is.EquivalentTo(new[] { BilbosName, EarlsName }));
                Assert.That(tim.TagsDictionary.Keys, Is.EquivalentTo(new[] { BilbosName, EarlsName }));
            });
        }

        [Test]
        public void Dictionary_Values()
        {
            RunTestInAllModes(realm =>
            {
                var tim = FindOwner(realm);

                var dogNames = new List<string>();

                foreach (var dog in GetDogsDict(tim).Values)
                {
                    dogNames.Add(dog.DynamicApi.Get<string>(nameof(DynamicDog.Name)));
                }

                Assert.That(dogNames, Is.EquivalentTo(new[] { BilbosName, EarlsName }));
                Assert.That(GetTagsDict(tim).Values, Is.EquivalentTo(new[] { "great", "playful" }));
            });
        }

        [Test]
        public void Dictionary_Values_Dynamic()
        {
            RunDynamicTestInAllModes(realm =>
            {
                dynamic tim = FindOwner(realm);

                var dogNames = new List<string>();

                foreach (var dog in tim.DogsDictionary.Values)
                {
                    dogNames.Add(dog.Name);
                }

                Assert.That(dogNames, Is.EquivalentTo(new[] { BilbosName, EarlsName }));
                Assert.That(tim.TagsDictionary.Values, Is.EquivalentTo(new[] { "great", "playful" }));
            });
        }

        [Test]
        public void Dictionary_Iteration()
        {
            RunTestInAllModes(realm =>
            {
                var tim = FindOwner(realm);
                var dogNames = new List<string>();
                var dogColors = new List<string>();

                foreach (var kvp in GetDogsDict(tim))
                {
                    dogNames.Add(kvp.Key);
                    dogColors.Add(kvp.Value.DynamicApi.Get<string>(nameof(DynamicDog.Color)));
                }

                Assert.That(dogNames, Is.EquivalentTo(new[] { BilbosName, EarlsName }));
                Assert.That(dogColors, Is.EquivalentTo(new[] { "Black", "White" }));

                var tags = new List<string>();
                dogNames.Clear();

                foreach (var kvp in GetTagsDict(tim))
                {
                    dogNames.Add(kvp.Key);
                    tags.Add(kvp.Value);
                }

                Assert.That(dogNames, Is.EquivalentTo(new[] { BilbosName, EarlsName }));
                Assert.That(tags, Is.EquivalentTo(new[] { "great", "playful" }));
            });
        }

        [Test]
        public void Dictionary_Iteration_Dynamic()
        {
            RunDynamicTestInAllModes(realm =>
            {
                dynamic tim = FindOwner(realm);
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
            });
        }

        [Test]
        public void Dictionary_Remove()
        {
            RunTestInAllModes(realm =>
            {
                var tim = FindOwner(realm);

                Assert.That(GetDogsDict(tim).Count, Is.EqualTo(2));

                realm.Write(() =>
                {
                    GetDogsDict(tim).Remove(BilbosName);
                    GetTagsDict(tim).Remove(BilbosName);
                });

                Assert.That(GetDogsDict(tim).Count, Is.EqualTo(1));
                Assert.That(GetTagsDict(tim).Count, Is.EqualTo(1));
            });
        }

        [Test]
        public void Dictionary_Remove_Dynamic()
        {
            RunDynamicTestInAllModes(realm =>
            {
                dynamic tim = FindOwner(realm);

                Assert.That(tim.DogsDictionary.Count, Is.EqualTo(2));

                realm.Write(() =>
                {
                    tim.DogsDictionary.Remove(BilbosName);
                    tim.TagsDictionary.Remove(BilbosName);
                });

                Assert.That(tim.DogsDictionary.Count, Is.EqualTo(1));
                Assert.That(tim.TagsDictionary.Count, Is.EqualTo(1));
            });
        }

        [Test]
        public void Set_Count()
        {
            RunTestInAllModes(realm =>
            {
                var tim = FindOwner(realm);

                Assert.That(GetDogsSet(tim).Count, Is.EqualTo(2));
                Assert.That(GetTagsSet(tim).Count, Is.EqualTo(2));
            });
        }

        [Test]
        public void Set_Count_Dynamic()
        {
            RunDynamicTestInAllModes(realm =>
            {
                dynamic tim = FindOwner(realm);

                Assert.That(tim.DogsSet.Count, Is.EqualTo(2));
                Assert.That(tim.TagsSet.Count, Is.EqualTo(2));
            });
        }

        [Test]
        public void Set_Add()
        {
            RunTestInAllModes(realm =>
            {
                var tim = FindOwner(realm);

                var pipi = realm.Write(() =>
                {
                    var innerPipi = (IRealmObject)(object)realm.DynamicApi.CreateObject(nameof(DynamicDog), null);
                    innerPipi.DynamicApi.Set(nameof(DynamicDog.Name), PipilottasName);
                    innerPipi.DynamicApi.Set(nameof(DynamicDog.Color), "Orange");

                    Assert.That(GetDogsSet(tim).Add(innerPipi), Is.True);
                    Assert.That(GetTagsSet(tim).Add("cheerful"), Is.True);

                    return innerPipi;
                });

                Assert.That(GetDogsSet(tim).Count, Is.EqualTo(3));
                Assert.That(GetDogsSet(tim).Any(d => d.DynamicApi.Get<string>(nameof(DynamicDog.Name)) == PipilottasName));
                Assert.That(GetDogsSet(tim).Contains(pipi));

                Assert.That(GetTagsSet(tim).Count, Is.EqualTo(3));
                Assert.That(GetTagsSet(tim).Any(t => t == "cheerful"));
                Assert.That(GetTagsSet(tim).Contains("cheerful"));

                realm.Write(() =>
                {
                    // These are already added to the sets
                    Assert.That(GetDogsSet(tim).Add(pipi), Is.False);
                    Assert.That(GetTagsSet(tim).Add("cheerful"), Is.False);
                });

                Assert.That(GetDogsSet(tim).Count, Is.EqualTo(3));
                Assert.That(GetDogsSet(tim).Any(d => d.DynamicApi.Get<string>(nameof(DynamicDog.Name)) == PipilottasName));
                Assert.That(GetDogsSet(tim).Contains(pipi));

                Assert.That(GetTagsSet(tim).Count, Is.EqualTo(3));
                Assert.That(GetTagsSet(tim).Any(t => t == "cheerful"));
                Assert.That(GetTagsSet(tim).Contains("cheerful"));
            });
        }

        [Test]
        public void Set_Add_Dynamic()
        {
            RunDynamicTestInAllModes(realm =>
            {
                dynamic tim = FindOwner(realm);

                var pipi = realm.Write(() =>
                {
                    var innerPipi = realm.DynamicApi.CreateObject(nameof(DynamicDog), null);
                    innerPipi.Name = PipilottasName;
                    innerPipi.Color = "Orange";

                    Assert.That(tim.DogsSet.Add(innerPipi), Is.True);
                    Assert.That(tim.TagsSet.Add("cheerful"), Is.True);

                    return innerPipi;
                });

                Assert.That(tim.DogsSet.Count, Is.EqualTo(3));
                Assert.That(((IEnumerable<dynamic>)tim.DogsSet).Any(d => d.Name == PipilottasName));
                Assert.That(tim.DogsSet.Contains(pipi));

                Assert.That(tim.TagsSet.Count, Is.EqualTo(3));
                Assert.That(((IEnumerable<dynamic>)tim.TagsSet).Any(t => t == "cheerful"));
                Assert.That(tim.TagsSet.Contains("cheerful"));

                realm.Write(() =>
                {
                    // These are already added to the sets
                    Assert.That(tim.DogsSet.Add(pipi), Is.False);
                    Assert.That(tim.TagsSet.Add("cheerful"), Is.False);
                });

                Assert.That(tim.DogsSet.Count, Is.EqualTo(3));
                Assert.That(((IEnumerable<dynamic>)tim.DogsSet).Any(d => d.Name == PipilottasName));
                Assert.That(tim.DogsSet.Contains(pipi));

                Assert.That(tim.TagsSet.Count, Is.EqualTo(3));
                Assert.That(((IEnumerable<dynamic>)tim.TagsSet).Any(t => t == "cheerful"));
                Assert.That(tim.TagsSet.Contains("cheerful"));
            });
        }

        [Test]
        public void Set_Iteration()
        {
            RunTestInAllModes(realm =>
            {
                var tim = FindOwner(realm);
                var dogNames = new List<string>();
                var dogColors = new List<string>();

                foreach (var dog in GetDogsSet(tim))
                {
                    dogNames.Add(dog.DynamicApi.Get<string>(nameof(DynamicDog.Name)));
                    dogColors.Add(dog.DynamicApi.Get<string>(nameof(DynamicDog.Color)));
                }

                Assert.That(dogNames, Is.EquivalentTo(new[] { BilbosName, EarlsName }));
                Assert.That(dogColors, Is.EquivalentTo(new[] { "Black", "White" }));

                var tags = new List<string>();

                foreach (var tag in GetTagsSet(tim))
                {
                    tags.Add(tag);
                }

                Assert.That(tags, Is.EquivalentTo(new[] { "coffee lover", "responsible" }));
            });
        }

        [Test]
        public void Set_Iteration_Dynamic()
        {
            RunDynamicTestInAllModes(realm =>
            {
                dynamic tim = FindOwner(realm);
                var dogNames = new List<string>();
                var dogColors = new List<string>();

                foreach (var dog in tim.DogsSet)
                {
                    dogNames.Add(dog.Name);
                    dogColors.Add(dog.Color);
                }

                Assert.That(dogNames, Is.EquivalentTo(new[] { BilbosName, EarlsName }));
                Assert.That(dogColors, Is.EquivalentTo(new[] { "Black", "White" }));

                var tags = new List<string>();

                foreach (var tag in tim.TagsSet)
                {
                    tags.Add(tag);
                }

                Assert.That(tags, Is.EquivalentTo(new[] { "coffee lover", "responsible" }));
            });
        }

        [Test]
        public void Set_Remove()
        {
            RunTestInAllModes(realm =>
            {
                var tim = FindOwner(realm);

                Assert.That(GetDogsSet(tim).Count, Is.EqualTo(2));

                var bilbo = FindDog(realm, BilbosName);
                realm.Write(() =>
                {
                    GetDogsSet(tim).Remove(bilbo);
                    GetTagsSet(tim).Remove("responsible");
                });

                Assert.That(GetDogsSet(tim).Count, Is.EqualTo(1));
                Assert.That(GetDogsSet(tim).Contains(bilbo), Is.False);

                Assert.That(GetTagsSet(tim).Count, Is.EqualTo(1));
                Assert.That(GetTagsSet(tim).Contains("responsible"), Is.False);
            });
        }

        [Test]
        public void Set_Remove_Dynamic()
        {
            RunDynamicTestInAllModes(realm =>
            {
                dynamic tim = FindOwner(realm);

                Assert.That(tim.DogsSet.Count, Is.EqualTo(2));

                dynamic bilbo = FindDog(realm, BilbosName);
                realm.Write(() =>
                {
                    tim.DogsSet.Remove(bilbo);
                    tim.TagsSet.Remove("responsible");
                });

                Assert.That(tim.DogsSet.Count, Is.EqualTo(1));
                Assert.That(tim.DogsSet.Contains(bilbo), Is.False);

                Assert.That(tim.TagsSet.Count, Is.EqualTo(1));
                Assert.That(tim.TagsSet.Contains("responsible"), Is.False);
            });
        }

        // Suggested https://stackoverflow.com/a/10021436/1649102
        private static TValue TryGetValue<TKey, TValue>(IDictionary<TKey, TValue> dict, TKey value, out bool found)
        {
            TValue result;
            found = dict.TryGetValue(value, out result);
            return result;
        }

        private static bool Any(dynamic collection, Func<dynamic, bool> predicate) => ((IEnumerable<dynamic>)collection).Any(predicate);
    }

    public partial class DynamicDog : TestRealmObject
    {
        public string Name { get; set; }

        public string Color { get; set; }

        public bool Vaccinated { get; set; }

        [Backlink(nameof(DynamicOwner.Dogs))]
        public IQueryable<DynamicOwner> Owners { get; }
    }

    public partial class DynamicOwner : TestRealmObject
    {
        public string Name { get; set; }

        public DynamicDog TopDog { get; set; }

        public IList<DynamicDog> Dogs { get; }

        public IList<string> Tags { get; }

        public IDictionary<string, DynamicDog> DogsDictionary { get; }

        public IDictionary<string, string> TagsDictionary { get; }

        public ISet<DynamicDog> DogsSet { get; }

        public ISet<string> TagsSet { get; }
    }
}
