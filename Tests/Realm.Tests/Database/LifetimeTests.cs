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
        // This method was extracted to ensure that the actual realm instance
        // isn't preserved in the scope of the test, even when the debugger is running.
        private static WeakReference GetWeakRealm()
        {
            return new WeakReference(Realm.GetInstance());
        }

        [Test]
        public void RealmObjectsShouldKeepRealmAlive()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                // Arrange
                var realmRef = GetWeakRealm();
                var person = ((Realm)realmRef.Target).Write(() =>
                {
                    return ((Realm)realmRef.Target).Add(new Person());
                });

                // Act
                await TestHelpers.EnsureThatReferenceIsAlive(2000, realmRef);

                // Assert
                Assert.That(realmRef.IsAlive);
                Assert.That(((Realm)realmRef.Target).IsClosed, Is.False);
                Assert.That(person.IsValid);

                // TearDown
                ((Realm)realmRef.Target).Dispose();
            });
        }

        [Test]
        public void FinalizedRealmsShouldNotInvalidateSiblingRealms()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                // Arrange
                using var realm = Realm.GetInstance(RealmConfiguration.DefaultConfiguration.DatabasePath);
                var realmThatWillBeFinalized = GetWeakRealm();
                Person person = null;
                realm.Write(() =>
                {
                    person = realm.Add(new Person());
                });

                // Act
                await TestHelpers.WaitUntilReferenceIsCollected(realmThatWillBeFinalized);

                // Assert
                Assert.That(person.IsValid);
            });
        }

        [Test]
        public void TransactionShouldHoldStrongReferenceToRealm()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var realmRef = GetWeakRealm();
                var transaction = ((Realm)realmRef.Target).BeginWrite();

                await TestHelpers.EnsureThatReferenceIsAlive(2000, realmRef);

                Assert.DoesNotThrow(transaction.Dispose);
                Assert.That(realmRef.IsAlive);

                // TearDown
                ((Realm)realmRef.Target).Dispose();
            });
        }
    }
}
