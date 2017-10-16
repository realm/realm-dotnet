////////////////////////////////////////////////////////////////////////////
//
// Copyright 2017 Realm Inc.
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
using Realms.Exceptions;
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
                    await GetSession(realm).WaitForDownloadAsync();

                    Assert.That(realm.All<ObjectA>().Count(), Is.EqualTo(0));
                    Assert.That(realm.All<ObjectB>().Count(), Is.EqualTo(0));

                    var objectAs = await realm.SubscribeToObjectsAsync<ObjectA>("IntValue < 5").Timeout(2000);

                    Assert.That(objectAs.Count(), Is.EqualTo(5));

                    foreach (var a in objectAs)
                    {
                        Assert.That(a.IntValue < 5);
                        Assert.That(a.B, Is.Not.Null);
                        Assert.That(a.B.BoolValue, Is.EqualTo(a.IntValue % 2 == 0));
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
                    await GetSession(realm).WaitForDownloadAsync();

                    Assert.That(realm.All<ObjectB>().Count(), Is.EqualTo(0));
                    Assert.That(realm.All<ObjectA>().Count(), Is.EqualTo(0));

                    await realm.SubscribeToObjectsAsync<ObjectA>("IntValue < 5").Timeout(2000);

                    var queriedObjectAs = realm.All<ObjectA>().Where(o => o.IntValue < 5);

                    Assert.That(queriedObjectAs.Count(), Is.EqualTo(5));

                    foreach (var a in queriedObjectAs)
                    {
                        Assert.That(a.IntValue < 5);
                        Assert.That(a.B, Is.Not.Null);
                        Assert.That(a.B.BoolValue, Is.EqualTo(a.IntValue % 2 == 0));
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
                    await GetSession(realm).WaitForDownloadAsync();

                    Assert.That(realm.All<ObjectB>().Count(), Is.EqualTo(0));
                    Assert.That(realm.All<ObjectA>().Count(), Is.EqualTo(0));

                    var youngerThan3 = await realm.SubscribeToObjectsAsync<ObjectA>("IntValue < 3").Timeout(2000);
                    var range1to6 = await realm.SubscribeToObjectsAsync<ObjectA>("IntValue > 1 AND IntValue < 6").Timeout(2000);

                    Assert.That(youngerThan3.Count(), Is.EqualTo(3));
                    Assert.That(range1to6.Count(), Is.EqualTo(4));

                    Assert.That(youngerThan3.ToArray().All(o => o.IntValue < 3));
                    Assert.That(range1to6.ToArray().All(o => o.IntValue > 1 && o.IntValue < 6));

                    var allInRealm = realm.All<ObjectA>();

                    Assert.That(allInRealm.Count(), Is.EqualTo(6));
                    Assert.That(allInRealm.ToArray().All(o => o.IntValue < 6));
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
                    await GetSession(realm).WaitForDownloadAsync();

                    Assert.That(realm.All<ObjectB>().Count(), Is.EqualTo(0));
                    Assert.That(realm.All<ObjectA>().Count(), Is.EqualTo(0));

                    var youngerThan3 = await realm.SubscribeToObjectsAsync<ObjectA>("IntValue < 3").Timeout(2000);
                    var olderThan6 = await realm.SubscribeToObjectsAsync<ObjectA>("IntValue > 6").Timeout(2000);

                    Assert.That(youngerThan3.Count(), Is.EqualTo(3));
                    Assert.That(olderThan6.Count(), Is.EqualTo(3));

                    Assert.That(youngerThan3.ToArray().All(o => o.IntValue < 3));
                    Assert.That(olderThan6.ToArray().All(o => o.IntValue > 6));

                    var allInRealm = realm.All<ObjectA>();

                    Assert.That(allInRealm.Count(), Is.EqualTo(6));
                    Assert.That(allInRealm.ToArray().All(o => o.IntValue < 3 || o.IntValue > 6));
                }
            });
        }

        [Test]
        public void SubscribeForObjects_WhenQueryIsInvalid_Throws()
        {
            AsyncContext.Run(async () =>
            {
                using (var realm = await GetPartialRealm())
                {
                    await GetSession(realm).WaitForDownloadAsync();

                    try
                    {
                        var objectAs = await realm.SubscribeToObjectsAsync<ObjectA>("foo = bar").Timeout(2000);
                        Assert.Fail("Expected an exception to be thrown.");
                    }
                    catch (RealmException ex)
                    {
                        Assert.That(ex.Message, Contains.Substring("QueryParser"));
                    }
                }
            });
        }

        private async Task<Realm> GetPartialRealm(Action<SyncConfiguration> setupConfig = null, [CallerMemberName] string realmPath = null)
        {
            var user = await SyncTestHelpers.GetUserAsync();
            var config = new SyncConfiguration(user, SyncTestHelpers.RealmUri($"~/{realmPath}"), Guid.NewGuid().ToString())
            {
                ObjectClasses = new[] { typeof(ObjectA), typeof(ObjectB) }
            };

            setupConfig?.Invoke(config);

            using (var original = GetRealm(config))
            {
                original.Write(() =>
                {
                    for (var i = 0; i < 10; i++)
                    {
                        original.Add(new ObjectA
                        {
                            StringValue = "A #" + i,
                            IntValue = i,
                            B = new ObjectB
                            {
                                StringValue = "B #" + i,
                                BoolValue = i % 2 == 0,
                            }
                        });
                    }
                });

                await GetSession(original).WaitForUploadAsync();
            }

            try
            {
                Realm.DeleteRealm(config);
            }
            catch
            {
            }

            config = new SyncConfiguration(config.User, config.ServerUri, config.DatabasePath + "_partial")
            {
                ObjectClasses = config.ObjectClasses,
                IsPartial = true
            };

            return GetRealm(config);
        }

        public class ObjectA : RealmObject
        {
            public string StringValue { get; set; }

            public int IntValue { get; set; }

            public ObjectB B { get; set; }
        }

        public class ObjectB : RealmObject
        {
            public string StringValue { get; set; }

            public bool BoolValue { get; set; }
        }
    }
}
