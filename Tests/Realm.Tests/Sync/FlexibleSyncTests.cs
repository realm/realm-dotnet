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

                AssertSubscriptionDetails(sub, nameof(SyncAllTypesObject), "TRUEPREDICATE");
                Assert.That(realm.Subscriptions.Count, Is.EqualTo(1));
            });

            Assert.That(realm.Subscriptions.Version, Is.EqualTo(1));
            Assert.That(realm.Subscriptions.Count, Is.EqualTo(1));
            Assert.That(realm.Subscriptions.Error, Is.Null);
            Assert.That(realm.Subscriptions.State, Is.EqualTo(SubscriptionSetState.Pending));

            AssertSubscriptionDetails(realm.Subscriptions[0], nameof(SyncAllTypesObject), "TRUEPREDICATE");
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

                AssertSubscriptionDetails(sub1, nameof(SyncAllTypesObject), "TRUEPREDICATE");
                AssertSubscriptionDetails(sub2, nameof(SyncCollectionsObject), "TRUEPREDICATE");
            });

            Assert.That(realm.Subscriptions.Count, Is.EqualTo(2));
            Assert.That(realm.Subscriptions.Error, Is.Null);
        }

        private Realm GetFakeFLXRealm() => GetRealm(GetFakeFLXConfig());

        private static void AssertSubscriptionDetails(Subscription sub, string type, string query, string name = null)
        {
            Assert.That(sub.Name, Is.EqualTo(name ?? query).IgnoreCase);
            Assert.That(sub.Query, Is.EqualTo(query).IgnoreCase);
            Assert.That(sub.ObjectType, Is.EqualTo(type));
        }
    }
}
