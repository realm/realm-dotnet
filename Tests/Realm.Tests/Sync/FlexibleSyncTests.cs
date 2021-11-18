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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Realms.Sync;

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
                var subsRefs = typeof(Realm).GetField("_subscriptionRefs", BindingFlags.NonPublic | BindingFlags.Instance);
                var subs = (List<WeakReference<SubscriptionSet>>)subsRefs.GetValue(realm);

                Assert.That(subs.Count, Is.EqualTo(1));
                Assert.That(subs[0].TryGetTarget(out _), Is.False);
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
                var subsRefs = typeof(Realm).GetField("_subscriptionRefs", BindingFlags.NonPublic | BindingFlags.Instance);
                var subs = (List<WeakReference<SubscriptionSet>>)subsRefs.GetValue(realm);

                Assert.That(subs.Count, Is.EqualTo(1));
                Assert.That(subs[0].TryGetTarget(out _), Is.False);

                // The old one was gc-ed, so we should get a new one here
                var subsAgain = realm.Subscriptions;

                Assert.That(subs.Count, Is.EqualTo(2));
                Assert.That(subs[1].TryGetTarget(out var subsFromList), Is.True);

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
            var query = realm.All<SyncAllTypesObject>().Where(o => o.StringProperty.StartsWith("foo") && (o.BooleanProperty || o.FloatProperty > 3.2f));
            var expectedQueryString = "StringProperty BEGINSWITH \"foo\" and (BooleanProperty == true or FloatProperty > 3.2)";

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
            Assert.That(realm.Subscriptions[0].Name, Is.EqualTo("c"));
            Assert.That(realm.Subscriptions[1].Name, Is.Null);

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
            Assert.That(realm.Subscriptions[2].Name, Is.EqualTo("c"));
            Assert.That(realm.Subscriptions[3].Name, Is.Null);

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

            Assert.Throws<ObjectDisposedException>(() => _ = subs.Count);
        }

        [Test]
        public void Realm_Subscriptions_WhenDisposed_Throws()
        {
            var realm = GetFakeFLXRealm();
            realm.Dispose();

            Assert.Throws<ObjectDisposedException>(() => _ = realm.Subscriptions);
        }

        private Realm GetFakeFLXRealm() => GetRealm(GetFakeFLXConfig());

        private static void AssertSubscriptionDetails(Subscription sub, string type, string query = "TRUEPREDICATE", string name = null, bool expectUpdateOnly = false)
        {
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
    }
}
