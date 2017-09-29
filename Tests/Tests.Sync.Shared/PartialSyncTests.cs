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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;
using Realms;
using Realms.Sync;

namespace Tests.Sync
{
#if !ROS_SETUP
    [NUnit.Framework.Explicit]
#endif
    [TestFixture, Preserve(AllMembers = true)]
    public class PartialSyncTests : SyncTestBase
    {
        [Test]
        public void SubscribeForObjects_ReturnsExpectedQuery()
        {
            AsyncContext.Run(async () =>
            {
                using (var realm = await GetPartialRealm())
                {
                    await realm.GetSession().WaitForDownloadAsync();

                    Assert.That(realm.All<Dog>().Count(), Is.EqualTo(0));
                    Assert.That(realm.All<Owner>().Count(), Is.EqualTo(0));

                    var owners = await realm.SubscribeForObjects<Owner>("Age < 5");

                    Assert.That(owners.Count(), Is.EqualTo(5));

                    foreach (var owner in owners)
                    {
                        Assert.That(owner.Age < 5);
                        Assert.That(owner.TopDog, Is.Not.Null);
                        Assert.That(owner.TopDog.Vaccinated, Is.EqualTo(owner.Age % 2 == 0));
                    }
                }
            });
        }

        [Test]
        public void SubscribeForObjects_SynchronizesOnlyMatchingObjects()
        {
            AsyncContext.Run(async () =>
            {
                using (var realm = await GetPartialRealm())
                {
                    await realm.GetSession().WaitForDownloadAsync();

                    Assert.That(realm.All<Dog>().Count(), Is.EqualTo(0));
                    Assert.That(realm.All<Owner>().Count(), Is.EqualTo(0));

                    await realm.SubscribeForObjects<Owner>("Age < 5");

                    var queriedOwners = realm.All<Owner>().Where(o => o.Age < 5);

                    Assert.That(queriedOwners.Count(), Is.EqualTo(5));

                    foreach (var owner in queriedOwners)
                    {
                        Assert.That(owner.Age < 5);
                        Assert.That(owner.TopDog, Is.Not.Null);
                        Assert.That(owner.TopDog.Vaccinated, Is.EqualTo(owner.Age % 2 == 0));
                    }
                }
            });
        }

        [Test]
        public void SubscribeForObjects_WhenTwoQueriesOverlap_SynchronizesTheUnion()
        {
            AsyncContext.Run(async () =>
            {
                using (var realm = await GetPartialRealm())
                {
                    await realm.GetSession().WaitForDownloadAsync();

                    Assert.That(realm.All<Dog>().Count(), Is.EqualTo(0));
                    Assert.That(realm.All<Owner>().Count(), Is.EqualTo(0));

                    var youngerThan3 = await realm.SubscribeForObjects<Owner>("Age < 3");
                    var range1to6 = await realm.SubscribeForObjects<Owner>("Age > 1 AND Age < 6");

                    Assert.That(youngerThan3.Count(), Is.EqualTo(3));
                    Assert.That(range1to6.Count(), Is.EqualTo(4));

                    Assert.That(youngerThan3.All(o => o.Age < 3));
                    Assert.That(range1to6.All(o => o.Age > 1 && o.Age < 6));

                    var allInRealm = realm.All<Owner>();

                    Assert.That(allInRealm.Count(), Is.EqualTo(6));
                    Assert.That(allInRealm.All(o => o.Age < 5));
                }
            });
        }

        [Test]
        public void SubscribeForObjects_WhenTwoQueriesDontOverlap_SynchronizesTheUnion()
        {
            AsyncContext.Run(async () =>
            {
                using (var realm = await GetPartialRealm())
                {
                    await realm.GetSession().WaitForDownloadAsync();

                    Assert.That(realm.All<Dog>().Count(), Is.EqualTo(0));
                    Assert.That(realm.All<Owner>().Count(), Is.EqualTo(0));

                    var youngerThan3 = await realm.SubscribeForObjects<Owner>("Age < 3");
                    var olderThan6 = await realm.SubscribeForObjects<Owner>("Age > 6");

                    Assert.That(youngerThan3.Count(), Is.EqualTo(3));
                    Assert.That(olderThan6.Count(), Is.EqualTo(3));

                    Assert.That(youngerThan3.All(o => o.Age < 3));
                    Assert.That(olderThan6.All(o => o.Age > 6));

                    var allInRealm = realm.All<Owner>();

                    Assert.That(allInRealm.Count(), Is.EqualTo(6));
                    Assert.That(allInRealm.All(o => o.Age < 3 || o.Age > 6));
                }
            });
        }

        private async Task<Realm> GetPartialRealm(Action<SyncConfiguration> setupConfig = null, [CallerMemberName] string realmPath = null)
        {
            var user = await SyncTestHelpers.GetUserAsync();
            var config = new SyncConfiguration(user, SyncTestHelpers.RealmUri(realmPath));

            setupConfig?.Invoke(config);

            using (var original = GetRealm(config))
            {
                original.Write(() =>
                {
                    for (var i = 0; i < 10; i++)
                    {
                        original.Add(new Owner
                        {
                            Name = "Owner #" + i,
                            Age = i,
                            TopDog = new Dog
                            {
                                Name = "Dog #" + i,
                                Vaccinated = i % 2 == 0,
                                Color = $"#{i}{i}AAAAAA"
                            }
                        });
                    }
                });

                await original.GetSession().WaitForUploadAsync();
            }

            config.IsPartial = true;
            return GetRealm(config);
        }
    }
}
