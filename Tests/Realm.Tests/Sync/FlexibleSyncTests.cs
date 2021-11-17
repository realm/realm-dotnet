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
            var config = GetFakeFLXConfig();
            var realm = GetRealm(config);
            Assert.That(realm.Subscriptions, Is.Not.Null);
            Assert.That(realm.Subscriptions.Version, Is.Zero);
        }

        [Test]
        public void SubscriptionSet_GetsGarbageCollected()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var config = GetFakeFLXConfig();
                var realm = GetRealm(config);

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
            var config = GetFakeFLXConfig();
            var realm = GetRealm(config);

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
                var config = GetFakeFLXConfig();
                var realm = GetRealm(config);

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
    }
}
