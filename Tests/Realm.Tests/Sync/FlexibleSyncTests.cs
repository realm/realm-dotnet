////////////////////////////////////////////////////////////////////////////
//
// Copyright 2021 Realm Inc.
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
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MongoDB.Bson;
using NUnit.Framework;
using Realms.Exceptions;
using Realms.Exceptions.Sync;
using Realms.Sync;
using Realms.Sync.Exceptions;

namespace Realms.Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class FlexibleSyncTests : SyncTestBase
    {
        [Test]
        public void Realm_Subscriptions_WhenLocalRealm_ReturnsNull()
        {
            var realm = GetRealm();

            Assert.That(realm.Subscriptions, Is.Null);
        }

        [Test]
        public void Realm_Subscriptions_WhenPBS_ReturnsNull()
        {
            var config = GetFakeConfig();
            var realm = GetRealm(config);

            Assert.That(realm.Subscriptions, Is.Null);
        }

        [Test]
        public void Realm_Subscriptions_WhenFLX_ReturnsSubscriptions()
        {
            var realm = GetFakeFLXRealm();

            Assert.That(realm.Subscriptions, Is.Not.Null);
            Assert.That(realm.Subscriptions.Version, Is.Zero);
            Assert.That(realm.Subscriptions.Count, Is.Zero);
            Assert.That(realm.Subscriptions.Error, Is.Null);
            Assert.That(realm.Subscriptions.State, Is.EqualTo(SubscriptionSetState.Pending));
        }

        [Test]
        public void SubscriptionSet_GetsGarbageCollected()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var realm = GetFakeFLXRealm();

                await TestHelpers.EnsureObjectsAreCollected(() =>
                {
                    var subs = realm.Subscriptions;
                    return new object[] { subs };
                });

                // This tests things via reflection because we don't want to expose private members, even internally.
                var subsRefs = typeof(Realm).GetField("_subscriptionRef", BindingFlags.NonPublic | BindingFlags.Instance);
                var weakSubs = (WeakReference<SubscriptionSet>)subsRefs.GetValue(realm);

                Assert.That(weakSubs, Is.Not.Null);
                Assert.That(weakSubs.TryGetTarget(out _), Is.False);
            });
        }

        [Test]
        public void Realm_Subscriptions_WhenSameVersion_ReturnsExistingReference()
        {
            var realm = GetFakeFLXRealm();

            var subs1 = realm.Subscriptions;
            var subs2 = realm.Subscriptions;

            Assert.That(subs1, Is.EqualTo(subs2));
            Assert.That(ReferenceEquals(subs1, subs2));
        }

        [Test]
        public void Realm_Subscriptions_WhenVersionIsGCed_CreatesANewOne()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var realm = GetFakeFLXRealm();

                await TestHelpers.EnsureObjectsAreCollected(() =>
                {
                    var subs = realm.Subscriptions;
                    return new object[] { subs };
                });

                // This tests things via reflection because we don't want to expose private members, even internally.
                var subsRef = typeof(Realm).GetField("_subscriptionRef", BindingFlags.NonPublic | BindingFlags.Instance);
                var weakSubs = (WeakReference<SubscriptionSet>)subsRef.GetValue(realm);

                Assert.That(weakSubs, Is.Not.Null);
                Assert.That(weakSubs.TryGetTarget(out _), Is.False);

                // The old one was gc-ed, so we should get a new one here
                var subsAgain = realm.Subscriptions;

                weakSubs = (WeakReference<SubscriptionSet>)subsRef.GetValue(realm);

                Assert.That(weakSubs, Is.Not.Null);
                Assert.That(weakSubs.TryGetTarget(out var subsFromList), Is.True);

                Assert.That(ReferenceEquals(subsAgain, subsFromList));
            });
        }

        [Test]
        public void SubscriptionSet_Add_WithoutUpdate_Throws()
        {
            var realm = GetFakeFLXRealm();

            var query = realm.All<SyncAllTypesObject>();

            Assert.Throws<InvalidOperationException>(() => realm.Subscriptions.Add(query));
        }

        [Test]
        public void SubscriptionSet_Update_WhenEmpty_Succeeds()
        {
            var realm = GetFakeFLXRealm();

            realm.Subscriptions.Update(() =>
            {
                // An empty update
            });

            Assert.That(realm.Subscriptions.Version, Is.EqualTo(1));
            Assert.That(realm.Subscriptions.Count, Is.Zero);
            Assert.That(realm.Subscriptions.Error, Is.Null);
            Assert.That(realm.Subscriptions.State, Is.EqualTo(SubscriptionSetState.Pending));
        }

        [Test]
        public void SubscriptionSet_Update_UpdatesItself()
        {
            var realm = GetFakeFLXRealm();
            var query = realm.All<SyncAllTypesObject>();

            var subs = realm.Subscriptions;
            subs.Update(() =>
            {
                subs.Add(query);
            });

            Assert.That(subs.Version, Is.EqualTo(1));
            Assert.That(subs.Count, Is.EqualTo(1));
            Assert.That(subs.Error, Is.Null);
            Assert.That(subs.State, Is.EqualTo(SubscriptionSetState.Pending));
            AssertSubscriptionDetails(subs[0], nameof(SyncAllTypesObject));

            var foundSub = subs.Find(query);
            Assert.That(foundSub, Is.Not.Null);
        }

        [Test]
        public void SubscriptionSet_Add_AddsSubscription()
        {
            var realm = GetFakeFLXRealm();
            var query = realm.All<SyncAllTypesObject>();

            realm.Subscriptions.Update(() =>
            {
                var sub = realm.Subscriptions.Add(query);

                AssertSubscriptionDetails(sub, nameof(SyncAllTypesObject));
                Assert.That(realm.Subscriptions.Count, Is.EqualTo(1));
            });

            Assert.That(realm.Subscriptions.Version, Is.EqualTo(1));
            Assert.That(realm.Subscriptions.Count, Is.EqualTo(1));
            Assert.That(realm.Subscriptions.Error, Is.Null);
            Assert.That(realm.Subscriptions.State, Is.EqualTo(SubscriptionSetState.Pending));

            AssertSubscriptionDetails(realm.Subscriptions[0], nameof(SyncAllTypesObject));
        }

        [Test]
        public void SubscriptionSet_Add_ComplexQuery_AddsSubscription()
        {
            var realm = GetFakeFLXRealm();
            var query = realm.All<SyncAllTypesObject>().Where(o => o.StringProperty.StartsWith("foo") && (o.BooleanProperty || o.DoubleProperty > 0.5));
            var expectedQueryString = "StringProperty BEGINSWITH \"foo\" and (BooleanProperty == true or DoubleProperty > 0.5)";

            realm.Subscriptions.Update(() =>
            {
                var sub = realm.Subscriptions.Add(query);

                AssertSubscriptionDetails(sub, nameof(SyncAllTypesObject), expectedQueryString);
                Assert.That(realm.Subscriptions.Count, Is.EqualTo(1));
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(1));
            Assert.That(realm.Subscriptions.Error, Is.Null);

            AssertSubscriptionDetails(realm.Subscriptions[0], nameof(SyncAllTypesObject), expectedQueryString);
        }

        [Test]
        public void SubscriptionSet_AddTwice_Deduplicates()
        {
            var realm = GetFakeFLXRealm();
            var query = realm.All<SyncAllTypesObject>();

            realm.Subscriptions.Update(() =>
            {
                var sub1 = realm.Subscriptions.Add(query);
                var sub2 = realm.Subscriptions.Add(query);

                Assert.That(realm.Subscriptions.Count, Is.EqualTo(1));
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(1));
            Assert.That(realm.Subscriptions.Error, Is.Null);
        }

        [Test]
        public void SubscriptionSet_AddTwice_DifferentNames_Duplicates()
        {
            var realm = GetFakeFLXRealm();
            var query = realm.All<SyncAllTypesObject>();

            realm.Subscriptions.Update(() =>
            {
                var sub1 = realm.Subscriptions.Add(query, new SubscriptionOptions { Name = "a" });
                var sub2 = realm.Subscriptions.Add(query, new SubscriptionOptions { Name = "b" });

                Assert.That(realm.Subscriptions.Count, Is.EqualTo(2));

                Assert.That(sub1.Name, Is.EqualTo("a"));
                Assert.That(sub2.Name, Is.EqualTo("b"));
                Assert.That(sub1.Query, Is.EqualTo(sub2.Query));
                Assert.That(sub1.ObjectType, Is.EqualTo(sub2.ObjectType));
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(2));
            Assert.That(realm.Subscriptions.Error, Is.Null);
        }

        [Test]
        public void SubscriptionSet_AddTwice_NamedAndUnnamed_Duplicates()
        {
            var realm = GetFakeFLXRealm();
            var query = realm.All<SyncAllTypesObject>();

            realm.Subscriptions.Update(() =>
            {
                var sub1 = realm.Subscriptions.Add(query);
                var sub2 = realm.Subscriptions.Add(query, new SubscriptionOptions { Name = "b" });

                Assert.That(realm.Subscriptions.Count, Is.EqualTo(2));

                Assert.That(sub1.Name, Is.Null);
                Assert.That(sub2.Name, Is.EqualTo("b"));
                Assert.That(sub1.Query, Is.EqualTo(sub2.Query));
                Assert.That(sub1.ObjectType, Is.EqualTo(sub2.ObjectType));
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(2));
            Assert.That(realm.Subscriptions.Error, Is.Null);
        }

        [Test]
        public void SubscriptionSet_AddSameName_NoUpdate_WhenIdentical_DoesntThrow()
        {
            var realm = GetFakeFLXRealm();
            var query = realm.All<SyncAllTypesObject>();

            realm.Subscriptions.Update(() =>
            {
                var sub1 = realm.Subscriptions.Add(query, new SubscriptionOptions { Name = "a" });
                var sub2 = realm.Subscriptions.Add(query, new SubscriptionOptions { Name = "a", UpdateExisting = false });

                Assert.That(realm.Subscriptions.Count, Is.EqualTo(1));

                AssertSubscriptionDetails(sub1, nameof(SyncAllTypesObject), name: "a");
                AssertSubscriptionDetails(sub2, nameof(SyncAllTypesObject), name: "a", expectUpdateOnly: true);
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(1));
            Assert.That(realm.Subscriptions.Error, Is.Null);
        }

        [Test]
        public void SubscriptionSet_AddSameName_NoUpdate_WhenDifferentQuery_Throws()
        {
            var realm = GetFakeFLXRealm();
            var query = realm.All<SyncAllTypesObject>();

            realm.Subscriptions.Update(() =>
            {
                var sub1 = realm.Subscriptions.Add(query, new SubscriptionOptions { Name = "a" });
                Assert.Throws<ArgumentException>(() => realm.Subscriptions.Add(query.Where(a => a.BooleanProperty), new SubscriptionOptions { Name = "a", UpdateExisting = false }));

                Assert.That(realm.Subscriptions.Count, Is.EqualTo(1));

                AssertSubscriptionDetails(sub1, nameof(SyncAllTypesObject), name: "a");
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(1));
            Assert.That(realm.Subscriptions.Error, Is.Null);
        }

        [Test]
        public void SubscriptionSet_AddSameName_NoUpdate_WhenDifferentType_Throws()
        {
            var realm = GetFakeFLXRealm();
            var query1 = realm.All<SyncAllTypesObject>();
            var query2 = realm.All<SyncCollectionsObject>();

            realm.Subscriptions.Update(() =>
            {
                var sub1 = realm.Subscriptions.Add(query1, new SubscriptionOptions { Name = "a" });
                Assert.Throws<ArgumentException>(() => realm.Subscriptions.Add(query2, new SubscriptionOptions { Name = "a", UpdateExisting = false }));

                Assert.That(realm.Subscriptions.Count, Is.EqualTo(1));

                AssertSubscriptionDetails(sub1, nameof(SyncAllTypesObject), name: "a");
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(1));
            Assert.That(realm.Subscriptions.Error, Is.Null);
        }

        [Test]
        public void SubscriptionSet_AddSameName_UpdateExisting_Updates()
        {
            var realm = GetFakeFLXRealm();
            var query1 = realm.All<SyncAllTypesObject>();
            var query2 = query1.Where(a => a.StringProperty.StartsWith("foo"));

            realm.Subscriptions.Update(() =>
            {
                var sub1 = realm.Subscriptions.Add(query1, new SubscriptionOptions { Name = "a" });
                var sub2 = realm.Subscriptions.Add(query2, new SubscriptionOptions { Name = "a" });

                Assert.That(realm.Subscriptions.Count, Is.EqualTo(1));

                AssertSubscriptionDetails(realm.Subscriptions[0], nameof(SyncAllTypesObject), "StringProperty BEGINSWITH \"foo\"", "a", expectUpdateOnly: true);
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(1));
            Assert.That(realm.Subscriptions.Error, Is.Null);
        }

        [Test]
        public void SubscriptionSet_AddSameName_DifferentType_UpdateExisting_Updates()
        {
            var realm = GetFakeFLXRealm();
            var query1 = realm.All<SyncAllTypesObject>();
            var query2 = realm.All<SyncCollectionsObject>();

            realm.Subscriptions.Update(() =>
            {
                var sub1 = realm.Subscriptions.Add(query1, new SubscriptionOptions { Name = "a" });
                var sub2 = realm.Subscriptions.Add(query2, new SubscriptionOptions { Name = "a" });

                Assert.That(realm.Subscriptions.Count, Is.EqualTo(1));

                AssertSubscriptionDetails(realm.Subscriptions[0], nameof(SyncCollectionsObject), name: "a", expectUpdateOnly: true);
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(1));
            Assert.That(realm.Subscriptions.Error, Is.Null);
        }

        [Test]
        public void SubscriptionSet_AddSameQuery_DifferentClasses_AddsBoth()
        {
            var realm = GetFakeFLXRealm();
            var query1 = realm.All<SyncAllTypesObject>();
            var query2 = realm.All<SyncCollectionsObject>();

            realm.Subscriptions.Update(() =>
            {
                var sub1 = realm.Subscriptions.Add(query1);
                var sub2 = realm.Subscriptions.Add(query2);

                Assert.That(realm.Subscriptions.Count, Is.EqualTo(2));

                AssertSubscriptionDetails(sub1, nameof(SyncAllTypesObject));
                AssertSubscriptionDetails(sub2, nameof(SyncCollectionsObject));
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(2));
            Assert.That(realm.Subscriptions.Error, Is.Null);
        }

        [Test]
        public void SubscriptionSet_Iteration()
        {
            var realm = GetFakeFLXRealm();
            var query1 = realm.All<SyncAllTypesObject>();
            var query2 = realm.All<SyncCollectionsObject>();

            realm.Subscriptions.Update(() =>
            {
                realm.Subscriptions.Add(query1, new SubscriptionOptions { Name = "a" });
                realm.Subscriptions.Add(query2);
            });

            var index = 0;
            foreach (var sub in realm.Subscriptions)
            {
                switch (index++)
                {
                    case 0:
                        AssertSubscriptionDetails(sub, nameof(SyncAllTypesObject), name: "a");
                        break;
                    case 1:
                        AssertSubscriptionDetails(sub, nameof(SyncCollectionsObject));
                        break;
                    default:
                        throw new Exception("Expected only 2 items");
                }
            }
        }

        [Test]
        public void SubscriptionSet_Indexer()
        {
            var realm = GetFakeFLXRealm();
            var query1 = realm.All<SyncAllTypesObject>();
            var query2 = realm.All<SyncCollectionsObject>();

            realm.Subscriptions.Update(() =>
            {
                realm.Subscriptions.Add(query1, new SubscriptionOptions { Name = "a" });
                realm.Subscriptions.Add(query2);
            });

            AssertSubscriptionDetails(realm.Subscriptions[0], nameof(SyncAllTypesObject), name: "a");
            AssertSubscriptionDetails(realm.Subscriptions[1], nameof(SyncCollectionsObject));

            Assert.Throws<ArgumentOutOfRangeException>(() => _ = realm.Subscriptions[-1]);
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = realm.Subscriptions[2]);
        }

        [Test]
        public void SubscriptionSet_Update_IncrementsVersion()
        {
            var realm = GetFakeFLXRealm();

            for (var i = 0; i < 10; i++)
            {
                Assert.That(realm.Subscriptions.Version, Is.EqualTo(i));
                realm.Subscriptions.Update(() => { });
            }
        }

        [Test]
        public void SubscriptionSet_FindByName_Finds()
        {
            var realm = GetFakeFLXRealm();

            var query1 = realm.All<SyncAllTypesObject>();
            var query2 = realm.All<SyncCollectionsObject>();

            realm.Subscriptions.Update(() =>
            {
                realm.Subscriptions.Add(query1, new SubscriptionOptions { Name = "a" });
                realm.Subscriptions.Add(query2, new SubscriptionOptions { Name = "b" });
            });

            var sub1 = realm.Subscriptions.Find("a");
            AssertSubscriptionDetails(sub1, nameof(SyncAllTypesObject), name: "a");

            var sub2 = realm.Subscriptions.Find("b");
            AssertSubscriptionDetails(sub2, nameof(SyncCollectionsObject), name: "b");

            Assert.That(sub1.CreatedAt, Is.LessThan(sub2.CreatedAt));
        }

        [Test]
        public void SubscriptionSet_FindByName_ReturnsNullWhenMissing()
        {
            var realm = GetFakeFLXRealm();

            Assert.That(realm.Subscriptions.Find("nonexistent"), Is.Null);

            realm.Subscriptions.Update(() =>
            {
                realm.Subscriptions.Add(realm.All<SyncAllTypesObject>(), new SubscriptionOptions { Name = "a" });
            });

            Assert.That(realm.Subscriptions.Find("a"), Is.Not.Null);
            Assert.That(realm.Subscriptions.Find("A"), Is.Null);
        }

        [Test]
        public void SubscriptionSet_FindByQuery_Finds()
        {
            var realm = GetFakeFLXRealm();

            var query1 = realm.All<SyncAllTypesObject>();
            var query2 = realm.All<SyncCollectionsObject>();

            realm.Subscriptions.Update(() =>
            {
                realm.Subscriptions.Add(query1, new SubscriptionOptions { Name = "a" });
                realm.Subscriptions.Add(query2);
            });

            var sub1 = realm.Subscriptions.Find(query1);
            AssertSubscriptionDetails(sub1, nameof(SyncAllTypesObject), name: "a");

            var sub2 = realm.Subscriptions.Find(query2);
            AssertSubscriptionDetails(sub2, nameof(SyncCollectionsObject));

            Assert.That(sub1.CreatedAt, Is.LessThan(sub2.CreatedAt));
        }

        [Test]
        public void SubscriptionSet_FindByQuery_ReturnsNullWhenMissing()
        {
            var realm = GetFakeFLXRealm();

            var existingQuery = realm.All<SyncAllTypesObject>();
            var nonExistingQuery = realm.All<SyncCollectionsObject>();

            Assert.That(realm.Subscriptions.Find(existingQuery), Is.Null);
            Assert.That(realm.Subscriptions.Find(nonExistingQuery), Is.Null);

            realm.Subscriptions.Update(() =>
            {
                realm.Subscriptions.Add(existingQuery);
            });

            Assert.That(realm.Subscriptions.Find(existingQuery), Is.Not.Null);
            Assert.That(realm.Subscriptions.Find(nonExistingQuery), Is.Null);
        }

        [Test]
        public void SubscriptionSet_Remove_ByName()
        {
            var realm = GetFakeFLXRealm();

            var query1 = realm.All<SyncAllTypesObject>();
            var query2 = realm.All<SyncCollectionsObject>();

            realm.Subscriptions.Update(() =>
            {
                realm.Subscriptions.Add(query1, new SubscriptionOptions { Name = "a" });
                realm.Subscriptions.Add(query2, new SubscriptionOptions { Name = "b" });
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(2));

            realm.Subscriptions.Update(() =>
            {
                Assert.That(realm.Subscriptions.Remove("a"), Is.True);
                Assert.That(realm.Subscriptions.Remove("a"), Is.False);
                Assert.That(realm.Subscriptions.Remove("c"), Is.False);
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(1));
            Assert.That(realm.Subscriptions[0].Name, Is.EqualTo("b"));

            realm.Subscriptions.Update(() =>
            {
                Assert.That(realm.Subscriptions.Remove("a"), Is.False);
                Assert.That(realm.Subscriptions.Remove("b"), Is.True);
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(0));
        }

        [Test]
        public void SubscriptionSet_Remove_ByName_OutsideUpdate_Throws()
        {
            var realm = GetFakeFLXRealm();

            Assert.Throws<InvalidOperationException>(() => realm.Subscriptions.Remove("foo"));
        }

        [Test]
        public void SubscriptionSet_Remove_Subscription()
        {
            var realm = GetFakeFLXRealm();

            var query1 = realm.All<SyncAllTypesObject>();
            var query2 = realm.All<SyncCollectionsObject>();

            realm.Subscriptions.Update(() =>
            {
                realm.Subscriptions.Add(query1, new SubscriptionOptions { Name = "a" });
                realm.Subscriptions.Add(query2, new SubscriptionOptions { Name = "b" });
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(2));
            var sub = realm.Subscriptions[0];
            var nonExistent = new Subscription
            {
                Id = ObjectId.GenerateNewId()
            };

            realm.Subscriptions.Update(() =>
            {
                Assert.That(realm.Subscriptions.Remove(sub), Is.True);
                Assert.That(realm.Subscriptions.Remove(sub), Is.False);
                Assert.That(realm.Subscriptions.Remove(nonExistent), Is.False);
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(1));
            Assert.That(realm.Subscriptions[0].Id != sub.Id);

            realm.Subscriptions.Update(() =>
            {
                var sub2 = realm.Subscriptions[0];
                Assert.That(realm.Subscriptions.Remove(sub), Is.False);
                Assert.That(realm.Subscriptions.Remove(sub2), Is.True);
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(0));
        }

        [Test]
        public void SubscriptionSet_Remove_Subscription_OutsideUpdate_Throws()
        {
            var realm = GetFakeFLXRealm();
            var sub = new Subscription
            {
                Id = ObjectId.GenerateNewId()
            };

            Assert.Throws<InvalidOperationException>(() => realm.Subscriptions.Remove(sub));
        }

        [Test]
        public void SubscriptionSet_Remove_ByQuery_RemoveNamed()
        {
            var realm = GetFakeFLXRealm();

            var query1 = realm.All<SyncAllTypesObject>();
            var query2 = realm.All<SyncAllTypesObject>().Where(s => s.BooleanProperty);

            realm.Subscriptions.Update(() =>
            {
                realm.Subscriptions.Add(query1);
                realm.Subscriptions.Add(query1, new SubscriptionOptions { Name = "a" });
                realm.Subscriptions.Add(query1, new SubscriptionOptions { Name = "b" });

                realm.Subscriptions.Add(query2);
                realm.Subscriptions.Add(query2, new SubscriptionOptions { Name = "c" });
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(5));

            realm.Subscriptions.Update(() =>
            {
                var removed = realm.Subscriptions.Remove(query1, removeNamed: true);
                Assert.That(removed, Is.EqualTo(3));
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(2));
            Assert.That(realm.Subscriptions[0].Name, Is.Null);
            Assert.That(realm.Subscriptions[1].Name, Is.EqualTo("c"));

            realm.Subscriptions.Update(() =>
            {
                var removed1 = realm.Subscriptions.Remove(query1, removeNamed: true);
                Assert.That(removed1, Is.EqualTo(0));

                var removed2 = realm.Subscriptions.Remove(query2, removeNamed: true);
                Assert.That(removed2, Is.EqualTo(2));
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(0));
        }

        [Test]
        public void SubscriptionSet_Remove_ByQuery_RemoveNamed_False()
        {
            var realm = GetFakeFLXRealm();

            var query1 = realm.All<SyncAllTypesObject>();
            var query2 = realm.All<SyncAllTypesObject>().Where(s => s.BooleanProperty);

            realm.Subscriptions.Update(() =>
            {
                realm.Subscriptions.Add(query1);
                realm.Subscriptions.Add(query1, new SubscriptionOptions { Name = "a" });
                realm.Subscriptions.Add(query1, new SubscriptionOptions { Name = "b" });

                realm.Subscriptions.Add(query2);
                realm.Subscriptions.Add(query2, new SubscriptionOptions { Name = "c" });
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(5));

            realm.Subscriptions.Update(() =>
            {
                var removed = realm.Subscriptions.Remove(query1, removeNamed: false);
                Assert.That(removed, Is.EqualTo(1));
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(4));
            Assert.That(realm.Subscriptions[0].Name, Is.EqualTo("a"));
            Assert.That(realm.Subscriptions[1].Name, Is.EqualTo("b"));
            Assert.That(realm.Subscriptions[2].Name, Is.Null);
            Assert.That(realm.Subscriptions[3].Name, Is.EqualTo("c"));

            realm.Subscriptions.Update(() =>
            {
                var removed1 = realm.Subscriptions.Remove(query1, removeNamed: false);
                Assert.That(removed1, Is.EqualTo(0));

                var removed2 = realm.Subscriptions.Remove(query2, removeNamed: false);
                Assert.That(removed2, Is.EqualTo(1));
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(3));
            Assert.That(realm.Subscriptions[0].Name, Is.EqualTo("a"));
            Assert.That(realm.Subscriptions[1].Name, Is.EqualTo("b"));
            Assert.That(realm.Subscriptions[2].Name, Is.EqualTo("c"));
        }

        [Test]
        public void SubscriptionSet_Remove_ByQuery_OutsideUpdate_Throws([Values(true, false)] bool removeNamed)
        {
            var realm = GetFakeFLXRealm();
            var query = realm.All<SyncAllTypesObject>();

            Assert.Throws<InvalidOperationException>(() => realm.Subscriptions.Remove(query, removeNamed: removeNamed));
        }

        [Test]
        public void SubscriptionSet_RemoveByType_RemoveNamed()
        {
            var realm = GetFakeFLXRealm();

            var query1 = realm.All<SyncAllTypesObject>();
            var query2 = realm.All<SyncCollectionsObject>();

            realm.Subscriptions.Update(() =>
            {
                realm.Subscriptions.Add(query1);
                realm.Subscriptions.Add(query1, new SubscriptionOptions { Name = "a" });
                realm.Subscriptions.Add(query1, new SubscriptionOptions { Name = "b" });

                realm.Subscriptions.Add(query2);
                realm.Subscriptions.Add(query2, new SubscriptionOptions { Name = "c" });
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(5));

            realm.Subscriptions.Update(() =>
            {
                var removed = realm.Subscriptions.RemoveAll(nameof(SyncAllTypesObject), removeNamed: true);
                Assert.That(removed, Is.EqualTo(3));
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(2));
        }

        [Test]
        public void SubscriptionSet_RemoveByType_RemoveNamed_False()
        {
            var realm = GetFakeFLXRealm();

            var query1 = realm.All<SyncAllTypesObject>();
            var query2 = realm.All<SyncCollectionsObject>();

            realm.Subscriptions.Update(() =>
            {
                realm.Subscriptions.Add(query1);
                realm.Subscriptions.Add(query1, new SubscriptionOptions { Name = "a" });
                realm.Subscriptions.Add(query1, new SubscriptionOptions { Name = "b" });

                realm.Subscriptions.Add(query2);
                realm.Subscriptions.Add(query2, new SubscriptionOptions { Name = "c" });
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(5));

            realm.Subscriptions.Update(() =>
            {
                var removed = realm.Subscriptions.RemoveAll(nameof(SyncAllTypesObject), removeNamed: false);
                Assert.That(removed, Is.EqualTo(1));
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(4));
        }

        [Test]
        public void SubscriptionSet_RemoveByType_Generic_RemoveNamed()
        {
            var realm = GetFakeFLXRealm();

            var query1 = realm.All<SyncAllTypesObject>();
            var query2 = realm.All<SyncCollectionsObject>();

            realm.Subscriptions.Update(() =>
            {
                realm.Subscriptions.Add(query1);
                realm.Subscriptions.Add(query1, new SubscriptionOptions { Name = "a" });
                realm.Subscriptions.Add(query1, new SubscriptionOptions { Name = "b" });

                realm.Subscriptions.Add(query2);
                realm.Subscriptions.Add(query2, new SubscriptionOptions { Name = "c" });
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(5));

            realm.Subscriptions.Update(() =>
            {
                var removed = realm.Subscriptions.RemoveAll<SyncAllTypesObject>(removeNamed: true);
                Assert.That(removed, Is.EqualTo(3));
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(2));
        }

        [Test]
        public void SubscriptionSet_RemoveByType_Generic_RemoveNamed_False()
        {
            var realm = GetFakeFLXRealm();

            var query1 = realm.All<SyncAllTypesObject>();
            var query2 = realm.All<SyncCollectionsObject>();

            realm.Subscriptions.Update(() =>
            {
                realm.Subscriptions.Add(query1);
                realm.Subscriptions.Add(query1, new SubscriptionOptions { Name = "a" });
                realm.Subscriptions.Add(query1, new SubscriptionOptions { Name = "b" });

                realm.Subscriptions.Add(query2);
                realm.Subscriptions.Add(query2, new SubscriptionOptions { Name = "c" });
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(5));

            realm.Subscriptions.Update(() =>
            {
                var removed = realm.Subscriptions.RemoveAll<SyncAllTypesObject>(removeNamed: false);
                Assert.That(removed, Is.EqualTo(1));
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(4));
        }

        [Test]
        public void SubscriptionSet_RemoveAll_RemoveNamed()
        {
            var realm = GetFakeFLXRealm();

            var query1 = realm.All<SyncAllTypesObject>();
            var query2 = realm.All<SyncAllTypesObject>().Where(s => s.BooleanProperty);

            realm.Subscriptions.Update(() =>
            {
                realm.Subscriptions.Add(query1);
                realm.Subscriptions.Add(query1, new SubscriptionOptions { Name = "a" });
                realm.Subscriptions.Add(query1, new SubscriptionOptions { Name = "b" });

                realm.Subscriptions.Add(query2);
                realm.Subscriptions.Add(query2, new SubscriptionOptions { Name = "c" });
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(5));

            realm.Subscriptions.Update(() =>
            {
                var removed = realm.Subscriptions.RemoveAll(removeNamed: true);
                Assert.That(removed, Is.EqualTo(5));
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(0));
        }

        [Test]
        public void SubscriptionSet_RemoveAll_RemoveNamed_False()
        {
            var realm = GetFakeFLXRealm();

            var query1 = realm.All<SyncAllTypesObject>();
            var query2 = realm.All<SyncAllTypesObject>().Where(s => s.BooleanProperty);

            realm.Subscriptions.Update(() =>
            {
                realm.Subscriptions.Add(query1);
                realm.Subscriptions.Add(query1, new SubscriptionOptions { Name = "a" });
                realm.Subscriptions.Add(query1, new SubscriptionOptions { Name = "b" });

                realm.Subscriptions.Add(query2);
                realm.Subscriptions.Add(query2, new SubscriptionOptions { Name = "c" });
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(5));

            realm.Subscriptions.Update(() =>
            {
                var removed = realm.Subscriptions.RemoveAll(removeNamed: false);
                Assert.That(removed, Is.EqualTo(2));
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(3));
            Assert.That(realm.Subscriptions[0].Name, Is.EqualTo("a"));
            Assert.That(realm.Subscriptions[1].Name, Is.EqualTo("b"));
            Assert.That(realm.Subscriptions[2].Name, Is.EqualTo("c"));
        }

        [Test]
        public void SubscriptionSet_RemoveAll_OutsideUpdate_Throws([Values(true, false)] bool removeNamed)
        {
            var realm = GetFakeFLXRealm();

            Assert.Throws<InvalidOperationException>(() => realm.Subscriptions.RemoveAll(removeNamed: removeNamed));
        }

        [Test]
        public void SubscriptionSet_WhenParentRealmIsClosed_GetsClosed()
        {
            var realm = GetFakeFLXRealm();
            var subs = realm.Subscriptions;

            realm.Dispose();

            Assert.That(DeleteRealmWithRetries(realm), Is.True);

            Assert.Throws<RealmClosedException>(() => _ = subs.Count);
        }

        [Test]
        public void SubscriptionSet_WhenSupersededParentRealmIsClosed_GetsClosed()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var realm = GetFakeFLXRealm();
                var subs = realm.Subscriptions;

                await Task.Run(() =>
                {
                    using var bgRealm = GetRealm(realm.Config);
                    bgRealm.Subscriptions.Update(() =>
                    {
                        bgRealm.Subscriptions.Add(bgRealm.All<SyncAllTypesObject>());
                    });
                });

                var updatedSubs = realm.Subscriptions;
                Assert.That(subs, Is.Not.SameAs(updatedSubs));

                realm.Dispose();

                var handleField = typeof(SubscriptionSet).GetField("_handle", BindingFlags.NonPublic | BindingFlags.Instance);
                var subsHandle = (SubscriptionSetHandle)handleField.GetValue(subs);
                var updatedSubsHandle = (SubscriptionSetHandle)handleField.GetValue(updatedSubs);
                Assert.That(subsHandle.IsClosed);
                Assert.That(updatedSubsHandle.IsClosed);

                Assert.That(DeleteRealmWithRetries(realm), Is.True);

                Assert.Throws<RealmClosedException>(() => _ = subs.Count);
                Assert.Throws<RealmClosedException>(() => _ = updatedSubs.Count);
            });
        }

        [Test]
        public void SubscriptionSet_Update_WhenActionThrows_RollsbackTransaction()
        {
            var realm = GetFakeFLXRealm();
            var query = realm.All<SyncAllTypesObject>();

            var subs = realm.Subscriptions;
            subs.Update(() =>
            {
                subs.Add(query);
            });

            Assert.That(subs.Count, Is.EqualTo(1));

            var ex = Assert.Throws<Exception>(() => subs.Update(() =>
            {
                subs.Add(realm.All<SyncCollectionsObject>());
                Assert.That(subs.Count, Is.EqualTo(2));

                // Now we do something that throws an error.
                throw new Exception("Oh no!");
            }));

            Assert.That(ex.Message, Is.EqualTo("Oh no!"));
            Assert.That(subs.Count, Is.EqualTo(1));

            Assert.That(subs.Find(query), Is.Not.Null);
        }

        [Test]
        public void SubscriptionSet_Update_WhenTransactionIsInProgress_Throws()
        {
            var realm = GetFakeFLXRealm();
            var query = realm.All<SyncAllTypesObject>();

            var subs = realm.Subscriptions;
            var ex = Assert.Throws<InvalidOperationException>(() => subs.Update(() =>
            {
                subs.Update(() => { });
            }));

            Assert.That(ex.Message, Does.Contain("already being updated"));
        }

        [Test]
        public void Realm_Subscriptions_WhenDisposed_Throws()
        {
            var realm = GetFakeFLXRealm();
            realm.Dispose();

            Assert.Throws<ObjectDisposedException>(() => _ = realm.Subscriptions);
        }

        [Test]
        public void SubscriptionSet_Enumerator()
        {
            var realm = GetFakeFLXRealm();

            var query = realm.All<SyncAllTypesObject>();

            realm.Subscriptions.Update(() =>
            {
                realm.Subscriptions.Add(query);
                realm.Subscriptions.Add(query, new SubscriptionOptions { Name = "a" });
                realm.Subscriptions.Add(query, new SubscriptionOptions { Name = "b" });
            });

            var index = 0;

            IEnumerable enumerableSubs = realm.Subscriptions;
            foreach (Subscription sub in enumerableSubs)
            {
                Assert.That(sub, Is.Not.Null);
                Assert.That(sub.Id, Is.EqualTo(realm.Subscriptions[index++].Id));
            }
        }

        [Test]
        public void SubscriptionSet_Enumerator_DoubleDispose_Throws()
        {
            var realm = GetFakeFLXRealm();
            var enumerator = realm.Subscriptions.GetEnumerator();

            enumerator.Dispose();

            Assert.Throws<ObjectDisposedException>(() => enumerator.Dispose());
        }

        [Test]
        public void SubscriptionSet_Enumerator_Reset()
        {
            var realm = GetFakeFLXRealm();

            var query = realm.All<SyncAllTypesObject>();

            realm.Subscriptions.Update(() =>
            {
                realm.Subscriptions.Add(query);
                realm.Subscriptions.Add(query, new SubscriptionOptions { Name = "a" });
                realm.Subscriptions.Add(query, new SubscriptionOptions { Name = "b" });
            });

            using var enumerator = realm.Subscriptions.GetEnumerator();

            var index = 0;
            while (enumerator.MoveNext())
            {
                Assert.That(enumerator.Current.Id, Is.EqualTo(realm.Subscriptions[index++].Id));
            }

            // Ensure we can reset the enumerator and iterate over it again.
            enumerator.Reset();

            index = 0;
            while (enumerator.MoveNext())
            {
                Assert.That(enumerator.Current.Id, Is.EqualTo(realm.Subscriptions[index++].Id));
            }
        }

        [Test]
        public void Integration_WaitForSynchronization_EmptyUpdate()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var realm = await GetFLXIntegrationRealmAsync();

                realm.Subscriptions.Update(() =>
                {
                });

                await realm.Subscriptions.WaitForSynchronizationAsync();

                Assert.That(realm.Subscriptions.State, Is.EqualTo(SubscriptionSetState.Complete));
            });
        }

        // TODO: enable this when https://github.com/realm/realm-core/issues/5208 is fixed
        [Test, Ignore("Failing, reenable when https://github.com/realm/realm-core/issues/5208 is fixed")]
        public void Integration_CloseRealmBeforeWaitCompletes()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var testGuid = Guid.NewGuid();

                await AddSomeData(testGuid);

                Task waitTask = null;
                using (var realm = await GetFLXIntegrationRealmAsync())
                {
                    realm.Subscriptions.Update(() =>
                    {
                        var query = realm.All<SyncAllTypesObject>().Where(o => o.GuidProperty == testGuid);

                        realm.Subscriptions.Add(query);
                    });

                    waitTask = WaitForSubscriptionsAsync(realm);
                }

                await waitTask;
            });
        }

        [Test]
        public void Integration_SubscriptionSet_AddRemove()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var testGuid = Guid.NewGuid();

                await AddSomeData(testGuid);

                var realm = await GetFLXIntegrationRealmAsync();

                Assert.That(realm.Subscriptions.State, Is.EqualTo(SubscriptionSetState.Complete));

                var query = realm.All<SyncAllTypesObject>().Where(o => o.DoubleProperty > 2 && o.GuidProperty == testGuid);

                await UpdateAndWaitForSubscription(query);

                Assert.That(query.Count(), Is.EqualTo(1));
                Assert.That(query.Single().DoubleProperty, Is.EqualTo(2.5));

                var query2 = realm.All<SyncAllTypesObject>().Where(o => o.DoubleProperty < 2 && o.GuidProperty == testGuid);
                await UpdateAndWaitForSubscription(query2);

                Assert.That(realm.All<SyncAllTypesObject>().Count(), Is.EqualTo(2));

                await UpdateAndWaitForSubscription(query, shouldAdd: false);

                Assert.That(realm.All<SyncAllTypesObject>().Count(), Is.EqualTo(1));
                Assert.That(query.Count(), Is.EqualTo(0));
                Assert.That(query2.Count(), Is.EqualTo(1));

                await UpdateAndWaitForSubscription(query2, shouldAdd: false);

                Assert.That(realm.All<SyncAllTypesObject>().Count(), Is.EqualTo(0));
            });
        }

        [Test]
        public void Integration_SubscriptionSet_MoveObjectOutsideView()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var testGuid = Guid.NewGuid();

                await AddSomeData(testGuid);

                var realm = await GetFLXIntegrationRealmAsync();

                Assert.That(realm.Subscriptions.State, Is.EqualTo(SubscriptionSetState.Complete));

                var query = realm.All<SyncAllTypesObject>().Where(o => o.DoubleProperty > 2 && o.GuidProperty == testGuid);

                await UpdateAndWaitForSubscription(query);

                Assert.That(query.Count(), Is.EqualTo(1));
                Assert.That(query.Single().DoubleProperty, Is.EqualTo(2.5));

                realm.Write(() =>
                {
                    query.Single().DoubleProperty = 1.99;
                });

                await TestHelpers.WaitForConditionAsync(() => !query.Any());
            });
        }

        [Test]
        public void Integration_SubscriptionSet_MoveObjectInsideView()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var testGuid = Guid.NewGuid();

                var writerRealm = await AddSomeData(testGuid);

                var realm = await GetFLXIntegrationRealmAsync();

                Assert.That(realm.Subscriptions.State, Is.EqualTo(SubscriptionSetState.Complete));

                var query = realm.All<SyncAllTypesObject>().Where(o => o.DoubleProperty > 2 && o.GuidProperty == testGuid);

                await UpdateAndWaitForSubscription(query);

                Assert.That(query.Count(), Is.EqualTo(1));
                Assert.That(query.Single().DoubleProperty, Is.EqualTo(2.5));

                // Add a new object
                writerRealm.Write(() =>
                {
                    writerRealm.Add(new SyncAllTypesObject
                    {
                        DoubleProperty = 9.9,
                        GuidProperty = testGuid
                    });
                });

                await TestHelpers.WaitForConditionAsync(() => query.Count() == 2);

                Assert.That(query.AsEnumerable().Select(o => o.DoubleProperty), Is.EquivalentTo(new[] { 2.5, 9.9 }));

                // Update an existing object
                writerRealm.Write(() =>
                {
                    writerRealm.All<SyncAllTypesObject>().Single(o => o.DoubleProperty == 1.5).DoubleProperty = 11;
                });

                await TestHelpers.WaitForConditionAsync(() => query.Count() == 3);

                Assert.That(query.AsEnumerable().Select(o => o.DoubleProperty), Is.EquivalentTo(new[] { 2.5, 9.9, 11 }));

                writerRealm.Write(() =>
                {
                    writerRealm.All<SyncAllTypesObject>().Single(o => o.DoubleProperty == 11).DoubleProperty = 0;
                });

                await TestHelpers.WaitForConditionAsync(() => query.Count() == 2);

                Assert.That(query.AsEnumerable().Select(o => o.DoubleProperty), Is.EquivalentTo(new[] { 2.5, 9.9 }));
            });
        }

        [Test]
        public void Integration_SubscriptionWithLinks()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var testGuid = Guid.NewGuid();

                var realm1 = await GetFLXIntegrationRealmAsync();
                realm1.Subscriptions.Update(() =>
                {
                    realm1.Subscriptions.Add(realm1.All<SyncAllTypesObject>().Where(o => o.GuidProperty == testGuid));
                    realm1.Subscriptions.Add(realm1.All<IntPropertyObject>().Where(o => o.GuidProperty == testGuid));
                });

                realm1.Write(() =>
                {
                    realm1.Add(getAtoWithLink(1));
                    realm1.Add(getAtoWithLink(2));
                    realm1.Add(getAtoWithLink(3));
                });

                await WaitForUploadAsync(realm1);

                var realm2 = await GetFLXIntegrationRealmAsync();
                realm2.Subscriptions.Update(() =>
                {
                    realm2.Subscriptions.Add(realm2.All<SyncAllTypesObject>().Where(o => o.GuidProperty == testGuid && o.Int64Property >= 2));
                });

                await WaitForSubscriptionsAsync(realm2);

                var atos = realm2.All<SyncAllTypesObject>().OrderBy(o => o.Int64Property);
                CollectionAssert.AreEqual(new[] { 2, 3 }, atos.AsEnumerable().Select(o => o.Int64Property));

                // We didn't subscribe to IntPropertyObject, so even though SyncAllTypesObject links to those, we should not see any.
                Assert.That(realm2.All<IntPropertyObject>().Count(), Is.EqualTo(0));
                Assert.That(atos.AsEnumerable().All(o => o.ObjectProperty == null));

                realm2.Subscriptions.Update(() =>
                {
                    realm2.Subscriptions.Add(realm2.All<IntPropertyObject>().Where(o => o.GuidProperty == testGuid && o.Int <= 2));
                });

                await WaitForSubscriptionsAsync(realm2);

                CollectionAssert.AreEqual(new[] { 2, 3 }, atos.AsEnumerable().Select(o => o.Int64Property));

                // Now we should have subscribed and should see 2 objects
                var intObjects = realm2.All<IntPropertyObject>().OrderBy(o => o.Int);
                CollectionAssert.AreEqual(new[] { 1, 2 }, intObjects.AsEnumerable().Select(o => o.Int));

                // We subscribed to ato 2 and 3, but intObject 1 and 2. So 2 should point to 2, 3 to null
                Assert.That(atos.ElementAt(0).ObjectProperty.Int, Is.EqualTo(2));
                Assert.That(atos.ElementAt(1).ObjectProperty, Is.Null);

                realm1.Write(() =>
                {
                    // ato 3 points to intObject 3, but that's outside of the view. We'll update intObject
                    // to -1 to bring it into view.
                    realm1.All<IntPropertyObject>().Single(o => o.Int == 3).Int = -1;
                });

                // intObject3 should sync down so we'll end up with 3 intObjects
                await TestHelpers.WaitForConditionAsync(() => intObjects.Count() == 3);

                Assert.That(atos.ElementAt(1).ObjectProperty.Int, Is.EqualTo(-1));

                SyncAllTypesObject getAtoWithLink(int value) => new()
                {
                    Int64Property = value,
                    GuidProperty = testGuid,
                    ObjectProperty = GetIntPropertyObject(value, testGuid)
                };
            });
        }

        [Test]
        public void Integration_SubscriptionWithEmbeddedObjects()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var testGuid = Guid.NewGuid();

                var realm1 = await GetFLXIntegrationRealmAsync();
                realm1.Subscriptions.Update(() =>
                {
                    realm1.Subscriptions.Add(realm1.All<SyncAllTypesObject>().Where(o => o.GuidProperty == testGuid));
                    realm1.Subscriptions.Add(realm1.All<IntPropertyObject>().Where(o => o.GuidProperty == testGuid));
                });

                realm1.Write(() =>
                {
                    realm1.Add(getAtoWithEmbedded(1));
                    realm1.Add(getAtoWithEmbedded(2));
                    realm1.Add(getAtoWithEmbedded(3));
                });

                await WaitForUploadAsync(realm1);

                var realm2 = await GetFLXIntegrationRealmAsync();
                realm2.Subscriptions.Update(() =>
                {
                    realm2.Subscriptions.Add(realm2.All<SyncAllTypesObject>().Where(o => o.GuidProperty == testGuid && o.Int64Property >= 2));
                });

                await WaitForSubscriptionsAsync(realm2);

                var atos = realm2.All<SyncAllTypesObject>().OrderBy(o => o.Int64Property);

                Assert.That(atos.Count(), Is.EqualTo(2));
                Assert.That(atos.ElementAt(0).Int64Property, Is.EqualTo(2));
                Assert.That(atos.ElementAt(0).EmbeddedObjectProperty.Int, Is.EqualTo(2));

                Assert.That(atos.ElementAt(1).Int64Property, Is.EqualTo(3));
                Assert.That(atos.ElementAt(1).EmbeddedObjectProperty.Int, Is.EqualTo(3));

                realm1.Write(() =>
                {
                    realm1.All<SyncAllTypesObject>().Single(o => o.Int64Property == 3).Int64Property = 1;
                });

                await TestHelpers.WaitForConditionAsync(() => atos.Count() == 1);

                Assert.That(atos.Single().Int64Property, Is.EqualTo(2));
                Assert.That(atos.Single().EmbeddedObjectProperty.Int, Is.EqualTo(2));

                SyncAllTypesObject getAtoWithEmbedded(int value) => new()
                {
                    Int64Property = value,
                    GuidProperty = testGuid,
                    EmbeddedObjectProperty = new EmbeddedIntPropertyObject
                    {
                        Int = value
                    }
                };
            });
        }

        [Test]
        public void Integration_SubscritpionWithCollections()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var testGuid = Guid.NewGuid();

                var realm1 = await GetFLXIntegrationRealmAsync();
                realm1.Subscriptions.Update(() =>
                {
                    realm1.Subscriptions.Add(realm1.All<SyncCollectionsObject>().Where(o => o.GuidProperty == testGuid));
                    realm1.Subscriptions.Add(realm1.All<IntPropertyObject>().Where(o => o.GuidProperty == testGuid));
                });

                var ito1 = GetIntPropertyObject(1, testGuid);
                var ito2 = GetIntPropertyObject(2, testGuid);
                var ito3 = GetIntPropertyObject(3, testGuid);

                var colObj1 = realm1.Write(() =>
                {
                    var collection = realm1.Add(new SyncCollectionsObject
                    {
                        GuidProperty = testGuid,
                    });

                    collection.ObjectList.Add(ito1);
                    collection.ObjectList.Add(ito2);
                    collection.ObjectList.Add(ito3);

                    // Add 1 and 2 again
                    collection.ObjectList.Add(ito1);
                    collection.ObjectList.Add(ito2);

                    collection.ObjectSet.Add(ito1);
                    collection.ObjectSet.Add(ito2);
                    collection.ObjectSet.Add(ito3);

                    collection.ObjectDict.Add("1", ito1);
                    collection.ObjectDict.Add("2", ito2);
                    collection.ObjectDict.Add("3", ito3);
                    collection.ObjectDict.Add("1_again", ito1);
                    collection.ObjectDict.Add("3_again", ito3);

                    return collection;
                });

                await WaitForUploadAsync(realm1);

                var realm2 = await GetFLXIntegrationRealmAsync();

                realm2.Subscriptions.Update(() =>
                {
                    realm2.Subscriptions.Add(realm2.All<SyncCollectionsObject>().Where(o => o.GuidProperty == testGuid));
                    realm2.Subscriptions.Add(realm2.All<IntPropertyObject>().Where(o => o.GuidProperty == testGuid && o.Int >= 2));
                });

                await WaitForSubscriptionsAsync(realm2);

                Assert.That(realm2.All<SyncCollectionsObject>().Count(), Is.EqualTo(1));
                Assert.That(realm2.All<IntPropertyObject>().Count(), Is.EqualTo(2));

                var colObj2 = realm2.All<SyncCollectionsObject>().Single();

                // We should have 2,3,2 because the 1s should be filtered
                CollectionAssert.AreEqual(new[] { 2, 3, 2 }, colObj2.ObjectList.Select(o => o.Int));
                Assert.That(colObj2.ObjectList[0], Is.EqualTo(colObj2.ObjectList[2]));

                Assert.That(colObj2.ObjectSet.Select(o => o.Int), Is.EquivalentTo(new[] { 2, 3 }));

                Assert.That(colObj2.ObjectDict.Count, Is.EqualTo(5));
                Assert.That(colObj2.ObjectDict["1"], Is.Null);
                Assert.That(colObj2.ObjectDict["2"].Int, Is.EqualTo(2));
                Assert.That(colObj2.ObjectDict["3"].Int, Is.EqualTo(3));
                Assert.That(colObj2.ObjectDict["1_again"], Is.Null);
                Assert.That(colObj2.ObjectDict["3_again"].Int, Is.EqualTo(3));
                Assert.That(colObj2.ObjectDict["3_again"], Is.EqualTo(colObj2.ObjectDict["3"]));

                // Add 4th element from the second client
                realm2.Write(() =>
                {
                    var ito4 = realm2.Add(GetIntPropertyObject(4, testGuid));
                    colObj2.ObjectList.Add(ito4);
                    colObj2.ObjectSet.Add(ito4);
                    colObj2.ObjectDict.Add("4", ito4);
                });

                await TestHelpers.WaitForConditionAsync(() => realm1.All<IntPropertyObject>().Count() == 4);

                Assert.That(colObj1.ObjectList.Count, Is.EqualTo(6));
                CollectionAssert.AreEqual(new[] { 1, 2, 3, 1, 2, 4 }, colObj1.ObjectList.Select(o => o.Int));

                Assert.That(colObj1.ObjectSet.Count, Is.EqualTo(4));
                Assert.That(colObj1.ObjectSet.Select(o => o.Int), Is.EquivalentTo(new[] { 1, 2, 3, 4 }));

                Assert.That(colObj1.ObjectDict.Count, Is.EqualTo(6));
                Assert.That(colObj1.ObjectDict["4"].Int, Is.EqualTo(4));

                // Move an element out of view from client1
                realm1.Write(() =>
                {
                    ito3.Int = -1;
                });

                await TestHelpers.WaitForConditionAsync(() => realm2.All<IntPropertyObject>().Count() == 2);

                // We should have 2, 2, 4 because 1 and 3 should be filtered
                CollectionAssert.AreEqual(new[] { 2, 2, 4 }, colObj2.ObjectList.Select(o => o.Int));
                Assert.That(colObj2.ObjectList[0], Is.EqualTo(colObj2.ObjectList[1]));

                Assert.That(colObj2.ObjectSet.Count, Is.EqualTo(2));
                Assert.That(colObj2.ObjectSet.Select(o => o.Int), Is.EquivalentTo(new[] { 2, 4 }));

                Assert.That(colObj2.ObjectDict.Count, Is.EqualTo(6));
                Assert.That(colObj2.ObjectDict["1"], Is.Null);
                Assert.That(colObj2.ObjectDict["2"].Int, Is.EqualTo(2));
                Assert.That(colObj2.ObjectDict["3"], Is.Null);
                Assert.That(colObj2.ObjectDict["1_again"], Is.Null);
                Assert.That(colObj2.ObjectDict["3_again"], Is.Null);
                Assert.That(colObj2.ObjectDict["4"].Int, Is.EqualTo(4));

                // Move an element into view from client1
                realm1.Write(() =>
                {
                    ito1.Int = 100;
                });

                await TestHelpers.WaitForConditionAsync(() => realm2.All<IntPropertyObject>().Count() == 3);

                // We should have 100, 2, 100, 2, 4 because 3 should be filtered
                CollectionAssert.AreEqual(new[] { 100, 2, 100, 2, 4 }, colObj2.ObjectList.Select(o => o.Int));
                Assert.That(colObj2.ObjectList[0], Is.EqualTo(colObj2.ObjectList[2]));
                Assert.That(colObj2.ObjectList[1], Is.EqualTo(colObj2.ObjectList[3]));

                Assert.That(colObj2.ObjectSet.Count, Is.EqualTo(3));
                Assert.That(colObj2.ObjectSet.Select(o => o.Int), Is.EquivalentTo(new[] { 100, 2, 4 }));

                Assert.That(colObj2.ObjectDict.Count, Is.EqualTo(6));
                Assert.That(colObj2.ObjectDict["1"].Int, Is.EqualTo(100));
                Assert.That(colObj2.ObjectDict["2"].Int, Is.EqualTo(2));
                Assert.That(colObj2.ObjectDict["3"], Is.Null);
                Assert.That(colObj2.ObjectDict["1_again"].Int, Is.EqualTo(100));
                Assert.That(colObj2.ObjectDict["3_again"], Is.Null);
                Assert.That(colObj2.ObjectDict["4"].Int, Is.EqualTo(4));
                Assert.That(colObj2.ObjectDict["1_again"], Is.EqualTo(colObj2.ObjectDict["1"]));
            });
        }

        [Test]
        public void Integration_RealmRemoveAllWithSubscriptions()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var testGuid = Guid.NewGuid();

                var realm1 = await GetFLXIntegrationRealmAsync();
                realm1.Subscriptions.Update(() =>
                {
                    realm1.Subscriptions.Add(realm1.All<SyncCollectionsObject>().Where(o => o.GuidProperty == testGuid));
                    realm1.Subscriptions.Add(realm1.All<IntPropertyObject>().Where(o => o.GuidProperty == testGuid));
                });

                // Get int property objects in random order just to make sure removal is value-related rather than chronological.
                var ito2 = GetIntPropertyObject(2, testGuid);
                var ito1 = GetIntPropertyObject(1, testGuid);
                var ito3 = GetIntPropertyObject(3, testGuid);

                var colObj1 = realm1.Write(() =>
                {
                    var collection = realm1.Add(new SyncCollectionsObject
                    {
                        GuidProperty = testGuid,
                    });

                    collection.ObjectList.Add(ito1);
                    collection.ObjectList.Add(ito2);
                    collection.ObjectList.Add(ito3);

                    // Add 1 and 2 again
                    collection.ObjectList.Add(ito1);
                    collection.ObjectList.Add(ito2);

                    collection.ObjectSet.Add(ito1);
                    collection.ObjectSet.Add(ito2);
                    collection.ObjectSet.Add(ito3);

                    collection.ObjectDict.Add("1", ito1);
                    collection.ObjectDict.Add("2", ito2);
                    collection.ObjectDict.Add("3", ito3);
                    collection.ObjectDict.Add("1_again", ito1);
                    collection.ObjectDict.Add("3_again", ito3);
                    return collection;
                });

                Assert.That(colObj1.ObjectList.Count, Is.EqualTo(5));
                Assert.That(colObj1.ObjectSet.Count, Is.EqualTo(3));
                Assert.That(colObj1.ObjectDict.Count, Is.EqualTo(5));
                Assert.That(realm1.All<IntPropertyObject>().Count(), Is.EqualTo(3));

                await WaitForUploadAsync(realm1);

                var realm2 = await GetFLXIntegrationRealmAsync();

                realm2.Subscriptions.Update(() =>
                {
                    realm2.Subscriptions.Add(realm2.All<IntPropertyObject>().Where(o => o.GuidProperty == testGuid && o.Int >= 2));
                });

                await WaitForSubscriptionsAsync(realm2);

                // No collections synced
                Assert.That(realm2.All<SyncCollectionsObject>().Count(), Is.EqualTo(0));

                // Only objects >= 2 synced
                Assert.That(realm2.All<IntPropertyObject>().Count(), Is.EqualTo(2));

                realm2.Write(() =>
                {
                    realm2.RemoveAll();
                });

                await TestHelpers.WaitForConditionAsync(() => realm1.All<IntPropertyObject>().Count() == 1);

                Assert.That(colObj1.ObjectList.Count, Is.EqualTo(2));
                Assert.That(colObj1.ObjectSet.Count, Is.EqualTo(1));
                Assert.That(colObj1.ObjectSet.Select(o => o.Int), Is.EquivalentTo(new[] { 1 }));

                Assert.That(colObj1.ObjectDict.Count, Is.EqualTo(5));
                Assert.That(colObj1.ObjectDict["1"].Int, Is.EqualTo(1));
                Assert.That(colObj1.ObjectDict["1_again"].Int, Is.EqualTo(1));
                Assert.That(colObj1.ObjectDict["2"], Is.Null);
                Assert.That(colObj1.ObjectDict["3"], Is.Null);
                Assert.That(colObj1.ObjectDict["3_again"], Is.Null);
            });
        }

        [Test]
        public void Integration_SubscriptionSet_WaitForSynchronization_CanBeCalledMultipleTimes()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var testGuid = Guid.NewGuid();
                var realm = await GetFLXIntegrationRealmAsync();

                realm.Subscriptions.Update(() =>
                {
                    realm.Subscriptions.Add(realm.All<SyncAllTypesObject>().Where(o => o.GuidProperty == testGuid));
                });

                Assert.That(realm.Subscriptions.State, Is.EqualTo(SubscriptionSetState.Pending));
                await realm.Subscriptions.WaitForSynchronizationAsync();

                Assert.That(realm.Subscriptions.State, Is.EqualTo(SubscriptionSetState.Complete));

                // Call WaitForSynchronizationAsync again
                await realm.Subscriptions.WaitForSynchronizationAsync();

                Assert.That(realm.Subscriptions.State, Is.EqualTo(SubscriptionSetState.Complete));
            });
        }

        [Test]
        public void Integration_CreateObjectNotMatchingSubscriptions_ShouldError()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var errorTcs = new TaskCompletionSource<SessionException>();
                var testGuid = Guid.NewGuid();
                var config = await GetFLXIntegrationConfigAsync();

                config.OnSessionError = (session, error) =>
                {
                    errorTcs.TrySetResult(error);
                };

                var realm = await GetRealmAsync(config);

                realm.Subscriptions.Update(() =>
                {
                    realm.Subscriptions.Add(realm.All<SyncAllTypesObject>().Where(o => o.GuidProperty == testGuid));
                });

                realm.Write(() =>
                {
                    realm.Add(new SyncAllTypesObject());
                });

                var sessionError = await errorTcs.Task;
                Assert.That(sessionError.ErrorCode, Is.EqualTo(ErrorCode.CompensatingWrite));
            });
        }

        [Test]
        public void Integration_UpdateObjectNotMatchingSubscriptions_ShouldError()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var errorTcs = new TaskCompletionSource<SessionException>();
                var testGuid = Guid.NewGuid();
                var config = await GetFLXIntegrationConfigAsync();
                config.OnSessionError = (session, error) =>
                {
                    errorTcs.TrySetResult(error);
                };

                var realm = await GetRealmAsync(config);

                realm.Subscriptions.Update(() =>
                {
                    realm.Subscriptions.Add(realm.All<SyncAllTypesObject>().Where(o => o.GuidProperty == testGuid));
                });

                var ato = realm.Write(() =>
                {
                    return realm.Add(new SyncAllTypesObject
                    {
                        GuidProperty = testGuid,
                    });
                });

                await WaitForUploadAsync(realm);

                realm.Write(() =>
                {
                    ato.GuidProperty = Guid.NewGuid();
                });

                realm.Write(() =>
                {
                    ato.Int32Property = 15;
                });

                var sessionError = await errorTcs.Task;
                Assert.That(sessionError.ErrorCode, Is.EqualTo(ErrorCode.CompensatingWrite));
            });
        }

        [Test]
        public void Integration_SubscriptionOnUnqueryableField_ShouldError()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var realm = await GetFLXIntegrationRealmAsync();

                realm.Subscriptions.Update(() =>
                {
                    realm.Subscriptions.Add(realm.All<SyncAllTypesObject>().Where(o => o.StringProperty == "foo"));
                });

                try
                {
                    await WaitForSubscriptionsAsync(realm);
                    Assert.Fail("Expected an error to be thrown.");
                }
                catch (SubscriptionException ex)
                {
                    Assert.That(ex.Message, Does.Contain(nameof(SyncAllTypesObject.StringProperty)).And.Contains(nameof(SyncAllTypesObject)));
                }

                Assert.That(realm.Subscriptions.State, Is.EqualTo(SubscriptionSetState.Error));
                Assert.That(realm.Subscriptions.Error.Message, Does.Contain(nameof(SyncAllTypesObject.StringProperty)).And.Contains(nameof(SyncAllTypesObject)));
            });
        }

        [Test]
        public void Integration_AfterAnError_CanRecover()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var realm = await GetFLXIntegrationRealmAsync();

                realm.Subscriptions.Update(() =>
                {
                    realm.Subscriptions.Add(realm.All<SyncAllTypesObject>().Where(o => o.StringProperty == "foo"));
                });

                try
                {
                    await realm.Subscriptions.WaitForSynchronizationAsync();
                    Assert.Fail("Expected an error to be thrown.");
                }
                catch (SubscriptionException ex)
                {
                    Assert.That(ex.Message, Does.Contain(nameof(SyncAllTypesObject.StringProperty)).And.Contains(nameof(SyncAllTypesObject)));
                }

                Assert.That(realm.Subscriptions.State, Is.EqualTo(SubscriptionSetState.Error));
                Assert.That(realm.Subscriptions.Error.Message, Does.Contain(nameof(SyncAllTypesObject.StringProperty)).And.Contains(nameof(SyncAllTypesObject)));

                var testGuid = Guid.NewGuid();

                realm.Subscriptions.Update(() =>
                {
                    realm.Subscriptions.RemoveAll(removeNamed: true);
                    realm.Subscriptions.Add(realm.All<SyncAllTypesObject>().Where(o => o.GuidProperty == testGuid));
                });

                Assert.That(realm.Subscriptions.State, Is.EqualTo(SubscriptionSetState.Pending));

                await realm.Subscriptions.WaitForSynchronizationAsync();

                Assert.That(realm.Subscriptions.State, Is.EqualTo(SubscriptionSetState.Complete));
                Assert.That(realm.Subscriptions.Error, Is.Null);
            });
        }

        [Test]
        public void Integration_UpdatingSubscription_SupersedesPreviousOnes()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var realm = await GetFLXIntegrationRealmAsync();

                var testGuid = Guid.NewGuid();

                realm.Subscriptions.Update(() =>
                {
                    realm.Subscriptions.Add(realm.All<SyncAllTypesObject>().Where(o => o.GuidProperty == testGuid));
                });

                var version = realm.Subscriptions.Version;

                // Capture subs here as we expect the collection to be superseded by the background update
                var subs = realm.Subscriptions;

                await Task.Run(() =>
                {
                    using var bgRealm = GetRealm(realm.Config);

                    Assert.That(bgRealm.Subscriptions.Version, Is.EqualTo(version));

                    bgRealm.Subscriptions.Update(() =>
                    {
                        bgRealm.Subscriptions.Add(bgRealm.All<SyncCollectionsObject>().Where(o => o.GuidProperty == testGuid));
                    });

                    Assert.That(bgRealm.Subscriptions.Version, Is.EqualTo(version + 1));
                });

                Assert.That(subs.Version, Is.EqualTo(version));
                Assert.That(realm.Subscriptions.Version, Is.EqualTo(version + 1));
                Assert.That(ReferenceEquals(subs, realm.Subscriptions), Is.False);

                await realm.Subscriptions.WaitForSynchronizationAsync();
                await subs.WaitForSynchronizationAsync();

                Assert.That(subs.State, Is.EqualTo(SubscriptionSetState.Superseded));
                Assert.That(subs.Version, Is.EqualTo(version));
                Assert.That(realm.Subscriptions.State, Is.EqualTo(SubscriptionSetState.Complete));
            });
        }

        [Test]
        public void Integration_InitialSubscriptions_Unnamed([Values(true, false)] bool openAsync)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var testGuid = Guid.NewGuid();

                await AddSomeData(testGuid);

                var config = await GetFLXIntegrationConfigAsync();
                config.PopulateInitialSubscriptions = (r) =>
                {
                    var query = r.All<SyncAllTypesObject>().Where(o => o.GuidProperty == testGuid);

                    r.Subscriptions.Add(query);
                };

                using var realm = openAsync ? await GetRealmAsync(config) : GetRealm(config);

                Assert.That(realm.Subscriptions.Count, Is.EqualTo(1));

                if (openAsync)
                {
                    Assert.That(realm.Subscriptions.State, Is.EqualTo(SubscriptionSetState.Complete));
                }
                else
                {
                    Assert.That(realm.Subscriptions.State, Is.EqualTo(SubscriptionSetState.Pending));
                    await WaitForSubscriptionsAsync(realm);
                }

                var query = realm.All<SyncAllTypesObject>().ToArray().Select(o => o.DoubleProperty);
                Assert.That(query.Count(), Is.EqualTo(2));
                Assert.That(query, Is.EquivalentTo(new[] { 1.5, 2.5 }));
            });
        }

        [Test]
        public void Integration_InitialSubscriptions_Named([Values(true, false)] bool openAsync)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var testGuid = Guid.NewGuid();

                await AddSomeData(testGuid);

                var config = await GetFLXIntegrationConfigAsync();
                config.PopulateInitialSubscriptions = (r) =>
                {
                    var query = r.All<SyncAllTypesObject>().Where(o => o.GuidProperty == testGuid && o.DoubleProperty > 2);

                    r.Subscriptions.Add(query, new() { Name = "initial" });
                };

                using var realm = openAsync ? await GetRealmAsync(config) : GetRealm(config);

                Assert.That(realm.Subscriptions.Count, Is.EqualTo(1));
                Assert.That(realm.Subscriptions[0].Name, Is.EqualTo("initial"));

                if (openAsync)
                {
                    Assert.That(realm.Subscriptions.State, Is.EqualTo(SubscriptionSetState.Complete));
                }
                else
                {
                    Assert.That(realm.Subscriptions.State, Is.EqualTo(SubscriptionSetState.Pending));
                    await WaitForSubscriptionsAsync(realm);
                }

                var query = realm.All<SyncAllTypesObject>().ToArray().Select(o => o.DoubleProperty);
                Assert.That(query.Count(), Is.EqualTo(1));
                Assert.That(query, Is.EquivalentTo(new[] { 2.5 }));
            });
        }

        [Test]
        public void Integration_InitialSubscriptions_RunsOnlyOnce([Values(true, false)] bool openAsync)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var invocations = 0;
                var config = await GetFLXIntegrationConfigAsync();
                config.PopulateInitialSubscriptions = (r) =>
                {
                    invocations++;
                };

                for (var i = 0; i < 2; i++)
                {
                    using var realm = openAsync ? await GetRealmAsync(config) : GetRealm(config);
                    Assert.That(invocations, Is.EqualTo(1));
                }
            });
        }

        [Test]
        public void Integration_InitialSubscriptions_PropagatesErrors([Values(true, false)] bool openAsync)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var testGuid = Guid.NewGuid();

                var dummyException = new Exception("Very careless");
                var config = await GetFLXIntegrationConfigAsync();
                config.PopulateInitialSubscriptions = (r) =>
                {
                    var query = r.All<SyncAllTypesObject>().Where(o => o.GuidProperty == testGuid);

                    r.Subscriptions.Add(query);

                    throw dummyException;
                };

                try
                {
                    using var shouldFailRealm = openAsync ? await GetRealmAsync(config) : GetRealm(config);
                    Assert.Fail("Expected an exception to occur");
                }
                catch (AggregateException ex)
                {
                    Assert.That(ex.Flatten().InnerException, Is.SameAs(dummyException));
                }

                // We failed the first time, we should not see PopulateInitialSubscriptions get invoked the second time
                using var realm = openAsync ? await GetRealmAsync(config) : GetRealm(config);
                Assert.That(realm.Subscriptions.Count, Is.Zero);
            });
        }

        private async Task<Realm> AddSomeData(Guid testGuid)
        {
            var writerRealm = await GetFLXIntegrationRealmAsync();
            writerRealm.Subscriptions.Update(() =>
            {
                writerRealm.Subscriptions.Add(writerRealm.All<SyncAllTypesObject>().Where(o => o.GuidProperty == testGuid));
            });

            writerRealm.Write(() =>
            {
                writerRealm.Add(new SyncAllTypesObject
                {
                    DoubleProperty = 1.5,
                    GuidProperty = testGuid,
                });

                writerRealm.Add(new SyncAllTypesObject
                {
                    DoubleProperty = 2.5,
                    GuidProperty = testGuid,
                });
            });

            await WaitForUploadAsync(writerRealm);

            return writerRealm;
        }

        private Realm GetFakeFLXRealm() => GetRealm(GetFakeFLXConfig());

        private static void AssertSubscriptionDetails(Subscription sub, string type, string query = "TRUEPREDICATE", string name = null, bool expectUpdateOnly = false)
        {
            Assert.That(sub.Id, Is.Not.EqualTo(ObjectId.Empty));
            Assert.That(sub.Name, Is.EqualTo(name).IgnoreCase);
            Assert.That(sub.Query, Is.EqualTo(query).IgnoreCase);
            Assert.That(sub.ObjectType, Is.EqualTo(type));
            Assert.That(sub.UpdatedAt, Is.EqualTo(DateTimeOffset.UtcNow).Within(TimeSpan.FromSeconds(1)));

            if (expectUpdateOnly)
            {
                Assert.That(sub.CreatedAt, Is.LessThan(sub.UpdatedAt));
            }
            else
            {
                Assert.That(sub.CreatedAt, Is.EqualTo(sub.UpdatedAt));
            }
        }

        private static async Task UpdateAndWaitForSubscription<T>(IQueryable<T> query, bool shouldAdd = true)
            where T : IRealmObject
        {
            var realm = ((RealmResults<T>)query).Realm;

            realm.Subscriptions.Update(() =>
            {
                if (shouldAdd)
                {
                    realm.Subscriptions.Add(query);
                }
                else
                {
                    realm.Subscriptions.Remove(query);
                }
            });

            Assert.That(realm.Subscriptions.State, Is.EqualTo(SubscriptionSetState.Pending));

            await WaitForSubscriptionsAsync(realm);

            Assert.That(realm.Subscriptions.State, Is.EqualTo(SubscriptionSetState.Complete));
        }

        private static IntPropertyObject GetIntPropertyObject(int value, Guid guid) => new()
        {
            Int = value,
            GuidProperty = guid
        };
    }
}
