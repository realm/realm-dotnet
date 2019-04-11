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

namespace Realms.Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class QueryBasedSyncTests : SyncTestBase
    {
        [TestCase(true)]
        [TestCase(false)]
        public void SubscribeForObjects_ReturnsExpectedQuery(bool openAsync)
        {
            AsyncContext.Run(async () =>
            {
                using (var realm = await GetQueryBasedRealm(openAsync))
                {
                    var query = realm.All<ObjectA>().Where(o => o.IntValue < 5);
                    var subscription = query.Subscribe();
                    Assert.That(subscription.State, Is.EqualTo(SubscriptionState.Creating));

                    await subscription.WaitForSynchronizationAsync().Timeout(2000);

                    Assert.That(subscription.State, Is.EqualTo(SubscriptionState.Complete));
                    Assert.That(subscription.Results.Count(), Is.EqualTo(5));

                    foreach (var a in subscription.Results)
                    {
                        Assert.That(a.IntValue < 5);
                        Assert.That(a.B, Is.Not.Null);
                        Assert.That(a.B.BoolValue, Is.EqualTo(a.IntValue % 2 == 0));
                    }
                }
            });
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SubscribeForObjects_SynchronizesOnlyMatchingObjects(bool openAsync)
        {
            AsyncContext.Run(async () =>
            {
                using (var realm = await GetQueryBasedRealm(openAsync))
                {
                    var query = realm.All<ObjectA>().Where(o => o.IntValue < 5);
                    var subscription = query.Subscribe();
                    await subscription.WaitForSynchronizationAsync().Timeout(2000);

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

        [TestCase(true)]
        [TestCase(false)]
        public void SubscribeForObjects_WhenTwoQueriesOverlap_SynchronizesTheUnion(bool openAsync)
        {
            AsyncContext.Run(async () =>
            {
                using (var realm = await GetQueryBasedRealm(openAsync))
                {
                    var youngerThan3Subscription = realm.All<ObjectA>().Where(o => o.IntValue < 3).Subscribe();
                    var range1to6Subscription = realm.All<ObjectA>().Where(o => o.IntValue > 1 && o.IntValue < 6).Subscribe();

                    await youngerThan3Subscription.WaitForSynchronizationAsync().Timeout(2000);
                    await range1to6Subscription.WaitForSynchronizationAsync().Timeout(2000);

                    var youngerThan3 = youngerThan3Subscription.Results;
                    var range1to6 = range1to6Subscription.Results;

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

        [TestCase(true)]
        [TestCase(false)]
        public void SubscribeForObjects_WhenTwoQueriesDontOverlap_SynchronizesTheUnion(bool openAsync)
        {
            AsyncContext.Run(async () =>
            {
                using (var realm = await GetQueryBasedRealm(openAsync))
                {
                    var youngerThan3Subscription = realm.All<ObjectA>().Where(o => o.IntValue < 3).Subscribe();
                    var olderThan6Subscription = realm.All<ObjectA>().Where(o => o.IntValue > 6).Subscribe();

                    await youngerThan3Subscription.WaitForSynchronizationAsync().Timeout(2000);
                    await olderThan6Subscription.WaitForSynchronizationAsync().Timeout(2000);

                    var youngerThan3 = youngerThan3Subscription.Results;
                    var olderThan6 = olderThan6Subscription.Results;

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

        [TestCase(true)]
        [TestCase(false)]
        public void NamedSubscription_CanResubscribe(bool openAsync)
        {
            AsyncContext.Run(async () =>
            {
                using (var realm = await GetQueryBasedRealm(openAsync))
                {
                    var query = realm.All<ObjectA>().Where(o => o.IntValue < 5);
                    var subscription = query.Subscribe(name: "less than 5");

                    await subscription.WaitForSynchronizationAsync().Timeout(2000);

                    Assert.That(subscription.Results.Count(), Is.EqualTo(5));

                    var subscription2 = realm.All<ObjectA>().Where(o => o.IntValue < 5).Subscribe(name: "less than 5");
                    await subscription2.WaitForSynchronizationAsync().Timeout(2000);

                    Assert.That(subscription.Results.Count(), Is.EqualTo(5));
                    Assert.That(subscription2.Results.Count(), Is.EqualTo(5));
                }
            });
        }

        [TestCase(true)]
        [TestCase(false)]
        public void NamedSubscription_CannotChangeQuery(bool openAsync)
        {
            AsyncContext.Run(async () =>
            {
                using (var realm = await GetQueryBasedRealm(openAsync))
                {
                    var query = realm.All<ObjectA>().Where(o => o.IntValue < 5);
                    var subscription = query.Subscribe(name: "foo");

                    await subscription.WaitForSynchronizationAsync().Timeout(2000);

                    Assert.That(subscription.Results.Count(), Is.EqualTo(5));

                    var subscription2 = realm.All<ObjectA>().Where(o => o.IntValue > 5).Subscribe(name: "foo");
                    try
                    {
                        await subscription2.WaitForSynchronizationAsync().Timeout(5000);
                        Assert.Fail("Expected to fail.");
                    }
                    catch (RealmException ex)
                    {
                        Assert.That(subscription2.State, Is.EqualTo(SubscriptionState.Error));
                        Assert.That(subscription2.Error, Is.Not.Null);
                        Assert.That(subscription2.Error.Message, Does.Contain("An existing subscription exists with the same name"));
                        Assert.That(ex, Is.EqualTo(subscription2.Error));
                    }
                }
            });
        }

        [TestCase(true)]
        [TestCase(false)]
        public void UnnamedSubscription_CanUnsubscribe(bool openAsync)
        {
            AsyncContext.Run(async () =>
            {
                using (var realm = await GetQueryBasedRealm(openAsync))
                {
                    var query = realm.All<ObjectA>().Where(o => o.IntValue < 5);
                    var subscription = query.Subscribe();

                    await subscription.WaitForSynchronizationAsync().Timeout(2000);

                    Assert.That(subscription.Results.Count(), Is.EqualTo(5));

                    await subscription.UnsubscribeAsync();

                    Assert.That(subscription.State, Is.EqualTo(SubscriptionState.Invalidated));

                    await TestHelpers.WaitForConditionAsync(() => query.Count() == 0);
                    Assert.That(query.Count(), Is.EqualTo(0));
                    Assert.That(subscription.Results.Count(), Is.EqualTo(0));
                }
            });
        }

        [TestCase(true)]
        [TestCase(false)]
        public void NamedSubscription_CanUnsubscribe(bool openAsync)
        {
            AsyncContext.Run(async () =>
            {
                using (var realm = await GetQueryBasedRealm(openAsync))
                {
                    var query = realm.All<ObjectA>().Where(o => o.IntValue < 5);
                    var subscription = query.Subscribe(name: "query");

                    await subscription.WaitForSynchronizationAsync().Timeout(2000);

                    Assert.That(subscription.Results.Count(), Is.EqualTo(5));

                    await subscription.UnsubscribeAsync();

                    Assert.That(subscription.State, Is.EqualTo(SubscriptionState.Invalidated));

                    await TestHelpers.WaitForConditionAsync(() => query.Count() == 0);
                    Assert.That(query.Count(), Is.EqualTo(0));
                    Assert.That(subscription.Results.Count(), Is.EqualTo(0));
                }
            });
        }

        [TestCase(true)]
        [TestCase(false)]
        public void NamedSubscription_CanUnsubscribeByName(bool openAsync)
        {
            AsyncContext.Run(async () =>
            {
                using (var realm = await GetQueryBasedRealm(openAsync))
                {
                    var query = realm.All<ObjectA>().Where(o => o.IntValue < 5);
                    var subscription = query.Subscribe(name: "query");

                    await subscription.WaitForSynchronizationAsync().Timeout(2000);

                    Assert.That(subscription.Results.Count(), Is.EqualTo(5));

                    await realm.UnsubscribeAsync("query");

                    await TestHelpers.WaitForConditionAsync(() => query.Count() == 0);
                    Assert.That(subscription.Results.Count(), Is.EqualTo(0));
                }
            });
        }

        // https://github.com/realm/realm-dotnet/issues/1716
        [TestCase(true)]
        [TestCase(false)]
        public void Subcribe_WaitForSynchronization_Multiple(bool openAsync)
        {
            AsyncContext.Run(async () =>
            {
                using (var realm = await GetQueryBasedRealm(openAsync))
                {
                    var subscription = realm.All<ObjectA>().Where(f => f.IntValue > 0).Subscribe();
                    await subscription.WaitForSynchronizationAsync().Timeout(5000);

                    var subscription2 = realm.All<ObjectA>().Where(f => f.IntValue > 0).Subscribe();
                    await subscription2.WaitForSynchronizationAsync().Timeout(5000);
                }
            });
        }

        [TestCase(true)]
        [TestCase(false)]
        public void WaitForSynchronization_OnBackgroundThread_Throws(bool openAsync)
        {
            AsyncContext.Run(async () =>
            {
                using (var realm = await GetQueryBasedRealm(openAsync))
                {
                    var config = realm.Config;
                    await Task.Run(() =>
                    {
                        using (var bgRealm = GetRealm(config))
                        {
                            var sub = bgRealm.All<ObjectA>().Subscribe();
                            Assert.Throws<NotSupportedException>(() => sub.WaitForSynchronizationAsync());
                        }
                    }).Timeout(2000);
                }
            });
        }

        private async Task<Realm> GetQueryBasedRealm(bool openAsync, [CallerMemberName] string realmPath = null)
        {
            SyncTestHelpers.RequiresRos();

            var user = await SyncTestHelpers.GetUserAsync();
            var config = new QueryBasedSyncConfiguration(SyncTestHelpers.RealmUri($"~/{realmPath}_{openAsync}"), user, Guid.NewGuid().ToString())
            {
                ObjectClasses = new[] { typeof(ObjectA), typeof(ObjectB) }
            };

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

            config = new QueryBasedSyncConfiguration(config.ServerUri, config.User, config.DatabasePath + "_partial")
            {
                ObjectClasses = config.ObjectClasses
            };

            Realm result;

            // We test both `GetInstance` and `GetInstanceAsync` to guard against regressions:
            // https://github.com/realm/realm-dotnet/issues/1814
            if (openAsync)
            {
                result = await GetRealmAsync(config).Timeout(5000);
            }
            else
            {
                result = GetRealm(config);
                await GetSession(result).WaitForDownloadAsync();
            }

            Assert.That(result.All<ObjectB>().Count(), Is.EqualTo(0));
            Assert.That(result.All<ObjectA>().Count(), Is.EqualTo(0));

            return result;
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
