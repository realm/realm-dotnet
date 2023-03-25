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
using NUnit.Framework;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class LifetimeTests : RealmTest
    {
        [Test]
        public void RealmObjectsShouldKeepRealmAlive()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                await TestHelpers.EnsurePreserverKeepsObjectAlive(() =>
                {
                    var realm = Realm.GetInstance();
                    var person = realm.Write(() =>
                    {
                        return realm.Add(new Person());
                    });
                    var realmReference = new WeakReference(realm);

                    return (person, realmReference);
                }, x =>
                {
                    Assert.That(x.Reference.IsAlive);
                    Assert.That(((Realm)x.Reference.Target!).IsClosed, Is.False);
                    Assert.That(x.Preserver.IsValid);
                });
            });
        }

        [Test]
        public void FinalizedRealmsShouldNotInvalidateSiblingRealms()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                using var realm = Realm.GetInstance(RealmConfiguration.DefaultConfiguration.DatabasePath);

                Person? person = null;
                await TestHelpers.EnsureObjectsAreCollected(() =>
                {
                    var secondRealm = Realm.GetInstance(realm.Config);

                    // Create a person in the first Realm instance and let the second one get garbage collected.
                    // We expect this not to close the first one or invalidate the person instance.
                    realm.Write(() =>
                    {
                        person = realm.Add(new Person());
                    });

                    return new[] { secondRealm };
                });

                // Assert
                Assert.That(person?.IsValid, Is.True);
            });
        }

        [Test]
        public void TransactionShouldHoldStrongReferenceToRealm()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                await TestHelpers.EnsurePreserverKeepsObjectAlive(() =>
                {
                    var realm = Realm.GetInstance();
                    var transaction = realm.BeginWrite();
                    var realmReference = new WeakReference(realm);

                    return (transaction, realmReference);
                });
            });
        }
    }
}
