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
using NUnit.Framework.Constraints;
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
            SyncTestHelpers.RunRosTestAsync(async () =>
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
            SyncTestHelpers.RunRosTestAsync(async () =>
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
            SyncTestHelpers.RunRosTestAsync(async () =>
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
            SyncTestHelpers.RunRosTestAsync(async () =>
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

        [TestCase(true, true)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(false, false)]
        public void Subscription_GetAll_FindsSubscription(bool openAsync, bool isNamed)
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                using (var realm = await GetQueryBasedRealm(openAsync))
                {
                    var name = isNamed ? "some subscription" : null;
                    var query = realm.All<ObjectA>().Where(o => o.IntValue < 5);
                    var now = DateTimeOffset.UtcNow;
                    var subscription = query.Subscribe(new SubscriptionOptions { Name = name });

                    await subscription.WaitForSynchronizationAsync().Timeout(2000);

                    var sub = realm.GetAllSubscriptions().FirstOrDefault(s => s.ObjectType == nameof(ObjectA));

                    Assert.That(sub, Is.Not.Null);
                    Assert.That(sub.Error, Is.Null);
                    Assert.That(sub.Name, isNamed ? (IResolveConstraint)Is.EqualTo(name) : Is.Not.Null);
                    Assert.That(sub.ObjectType, Is.EqualTo(nameof(ObjectA)));
                    Assert.That(sub.CreatedAt, Is.InRange(now, now.AddSeconds(2)));
                    Assert.That(sub.CreatedAt, Is.EqualTo(sub.UpdatedAt));
                    Assert.That(sub.ExpiresAt, Is.Null);
                    Assert.That(sub.TimeToLive, Is.Null);
                }
            });
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Subscribe_WhenTtlSet_ExpiresSubscription(bool openAsync)
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                RealmConfigurationBase config;
                using (var realm = await GetQueryBasedRealm(openAsync))
                {
                    config = realm.Config;
                    var ttl = TimeSpan.FromMilliseconds(500);
                    var query = realm.All<ObjectA>().Where(o => o.IntValue < 5);
                    var subscription = query.Subscribe(new SubscriptionOptions { TimeToLive = ttl });

                    await subscription.WaitForSynchronizationAsync().Timeout(2000);

                    var sub = realm.GetAllSubscriptions().FirstOrDefault(s => s.ObjectType == nameof(ObjectA));

                    Assert.That(sub, Is.Not.Null);
                    Assert.That(sub.ExpiresAt, Is.EqualTo(sub.CreatedAt.Add(ttl)));
                    Assert.That(sub.TimeToLive, Is.EqualTo(ttl));
                }
            });
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Subscribe_UpdatesQuery(bool openAsync)
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                using (var realm = await GetQueryBasedRealm(openAsync))
                {
                    var subscription = realm.All<ObjectA>()
                                            .Where(o => o.IntValue < 5)
                                            .Subscribe(new SubscriptionOptions { Name = "foo", ShouldUpdate = true });

                    await subscription.WaitForSynchronizationAsync().Timeout(2000);

                    Assert.That(subscription.Results.Count(), Is.EqualTo(5));

                    var namedSub = realm.GetAllSubscriptions().Single(s => s.ObjectType == nameof(ObjectA));
                    var originalQuery = namedSub.Query;
                    var originalUpdatedAt = namedSub.UpdatedAt;
                    var originalCreatedAt = namedSub.CreatedAt;

                    Assert.That(originalCreatedAt, Is.EqualTo(originalUpdatedAt));

                    var updatedSub = realm.All<ObjectA>()
                                          .Where(o => o.IntValue < 3)
                                          .Subscribe(new SubscriptionOptions { Name = "foo", ShouldUpdate = true });
                                          
                    await updatedSub.WaitForSynchronizationAsync().Timeout(2000);
                    Assert.That(subscription.Results.Count(), Is.EqualTo(3));

                    // NamedSub is a Realm object so it should have updated itself.
                    Assert.That(originalQuery, Is.Not.EqualTo(namedSub.Query));
                    Assert.That(originalCreatedAt, Is.EqualTo(namedSub.CreatedAt));
                    Assert.That(originalUpdatedAt, Is.LessThan(namedSub.UpdatedAt));
                }
            });
        }

        [TestCase(true, true)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(false, false)]
        public void Subscribe_UpdatesTtl(bool openAsync, bool changeTtlValue)
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                using (var realm = await GetQueryBasedRealm(openAsync))
                {
                    var originalTtl = TimeSpan.FromSeconds(1);
                    var subscription = realm.All<ObjectA>()
                                            .Where(o => o.IntValue < 5)
                                            .Subscribe(new SubscriptionOptions { Name = "foo", ShouldUpdate = true, TimeToLive = originalTtl });

                    await subscription.WaitForSynchronizationAsync().Timeout(2000);

                    var namedSub = realm.GetAllSubscriptions().Single(s => s.ObjectType == nameof(ObjectA));
                    var originalUpdatedAt = namedSub.UpdatedAt;
                    var originalCreatedAt = namedSub.CreatedAt;

                    Assert.That(originalCreatedAt, Is.EqualTo(originalUpdatedAt));
                    Assert.That(namedSub.TimeToLive, Is.EqualTo(originalTtl));
                    Assert.That(namedSub.ExpiresAt, Is.EqualTo(namedSub.UpdatedAt.Add(originalTtl)));

                    var updatedTtl = changeTtlValue ? TimeSpan.FromSeconds(2) : originalTtl;
                    var updatedSub = realm.All<ObjectA>()
                                          .Where(o => o.IntValue < 5)
                                          .Subscribe(new SubscriptionOptions { Name = "foo", ShouldUpdate = true, TimeToLive = updatedTtl });

                    await updatedSub.WaitForSynchronizationAsync().Timeout(2000);

                    // NamedSub is a Realm object so it should have updated itself.
                    Assert.That(originalCreatedAt, Is.EqualTo(namedSub.CreatedAt));
                    Assert.That(originalUpdatedAt, Is.LessThan(namedSub.UpdatedAt));
                    Assert.That(namedSub.TimeToLive, Is.EqualTo(updatedTtl));
                    Assert.That(namedSub.ExpiresAt, Is.EqualTo(namedSub.UpdatedAt.Add(updatedTtl)));
                }
            });
        }

        [TestCase(true)]
        [TestCase(false)]
        public void NamedSubscription_CanResubscribe(bool openAsync)
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                using (var realm = await GetQueryBasedRealm(openAsync))
                {
                    var query = realm.All<ObjectA>().Where(o => o.IntValue < 5);
                    var subscription = query.Subscribe(new SubscriptionOptions { Name = "less than 5" });

                    await subscription.WaitForSynchronizationAsync().Timeout(2000);

                    Assert.That(subscription.Results.Count(), Is.EqualTo(5));

                    var subscription2 = realm.All<ObjectA>().Where(o => o.IntValue < 5).Subscribe(new SubscriptionOptions { Name = "less than 5" });
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
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                using (var realm = await GetQueryBasedRealm(openAsync))
                {
                    var query = realm.All<ObjectA>().Where(o => o.IntValue < 5);
                    var subscription = query.Subscribe(new SubscriptionOptions { Name = "foo" });

                    await subscription.WaitForSynchronizationAsync().Timeout(2000);

                    Assert.That(subscription.Results.Count(), Is.EqualTo(5));

                    var subscription2 = realm.All<ObjectA>()
                                             .Where(o => o.IntValue > 5)
                                             .Subscribe(new SubscriptionOptions { Name = "foo" });
                    try
                    {
                        await subscription2.WaitForSynchronizationAsync().Timeout(5000);
                        Assert.Fail("Expected to fail.");
                    }
                    catch (RealmException ex)
                    {
                        Assert.That(subscription2.State, Is.EqualTo(SubscriptionState.Error));
                        Assert.That(subscription2.Error, Is.Not.Null);
                        Assert.That(subscription2.Error.Message, Does.Contain("An existing subscription exists with the name"));
                        Assert.That(ex, Is.EqualTo(subscription2.Error));
                    }
                }
            });
        }

        [TestCase(true)]
        [TestCase(false)]
        public void UnnamedSubscription_CanUnsubscribe(bool openAsync)
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
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
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                using (var realm = await GetQueryBasedRealm(openAsync))
                {
                    var query = realm.All<ObjectA>().Where(o => o.IntValue < 5);
                    var subscription = query.Subscribe(new SubscriptionOptions { Name = "query" });

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
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                using (var realm = await GetQueryBasedRealm(openAsync))
                {
                    var query = realm.All<ObjectA>().Where(o => o.IntValue < 5);
                    var subscription = query.Subscribe(new SubscriptionOptions { Name = "query" });

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
            SyncTestHelpers.RunRosTestAsync(async () =>
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
            SyncTestHelpers.RunRosTestAsync(async () =>
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

        [TestCase(true)]
        [TestCase(false)]
        public void Subscribe_WithInclusions(bool openAsync)
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                using (var realm = await GetQueryBasedRealm(openAsync))
                {
                    var subscription = realm.All<ObjectB>()
                                            .Where(b => b.BoolValue)
                                            .Subscribe(includedBacklinks: b => b.As);

                    await subscription.WaitForSynchronizationAsync().Timeout(2000);

                    Assert.That(realm.All<ObjectB>().Count(), Is.EqualTo(5));
                    Assert.That(realm.All<ObjectA>().Count(), Is.EqualTo(5));
                    Assert.That(realm.All<ObjectA>().ToArray().All(a => a.IntValue % 2 == 0));
                }
            });
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Subscribe_WithInclusions_ExtraHop(bool openAsync)
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                using (var realm = await GetQueryBasedRealm(openAsync))
                {
                    var subscription = realm.All<ObjectC>()
                                            .Filter("B.BoolValue == true")
                                            .Subscribe(includedBacklinks: c => c.B.As);

                    await subscription.WaitForSynchronizationAsync().Timeout(2000);

                    Assert.That(realm.All<ObjectC>().Count(), Is.EqualTo(5));
                    Assert.That(realm.All<ObjectB>().Count(), Is.EqualTo(5));
                    Assert.That(realm.All<ObjectA>().Count(), Is.EqualTo(5));
                    Assert.That(realm.All<ObjectA>().ToArray().All(a => a.IntValue % 2 == 0));
                }
            });
        }

        private async Task<Realm> GetQueryBasedRealm(bool openAsync, [CallerMemberName] string realmPath = null)
        {
            var user = await SyncTestHelpers.GetUserAsync();
            var config = new QueryBasedSyncConfiguration(SyncTestHelpers.RealmUri($"~/{realmPath}_{openAsync}"), user, Guid.NewGuid().ToString())
            {
                ObjectClasses = new[] { typeof(ObjectA), typeof(ObjectB), typeof(ObjectC) }
            };

            using (var original = GetRealm(config))
            {
                original.Write(() =>
                {
                    for (var i = 0; i < 10; i++)
                    {
                        var a = original.Add(new ObjectA
                        {
                            StringValue = "A #" + i,
                            IntValue = i,
                            B = new ObjectB
                            {
                                StringValue = "B #" + i,
                                BoolValue = i % 2 == 0,
                            }
                        });

                        original.Add(new ObjectC
                        {
                            IntValue = i,
                            B = a.B
                        });
                    }
                });

                await SyncTestHelpers.WaitForUploadAsync(original);
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

            // We test both `GetInstance` and `GetInstanceAsync` to guard against regressions:
            // https://github.com/realm/realm-dotnet/issues/1814
            var result = await GetRealmAsync(config, openAsync).Timeout(5000);

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

            [Backlink(nameof(ObjectA.B))]
            public IQueryable<ObjectA> As { get; }
        }

        public class ObjectC : RealmObject
        {
            public int IntValue { get; set; }

            public ObjectB B { get; set; }
        }
    }
}
