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
            TestHelpers.IgnoreOnWindows("GC blocks on Windows");

            // Arrange
            var realm = GetWeakRealm();
            Person person = null;
            ((Realm)realm.Target).Write(() =>
            {
                person = ((Realm)realm.Target).Add(new Person());
            });

            // Act
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Assert
            Assert.That(realm.IsAlive);
            Assert.That(((Realm)realm.Target).IsClosed, Is.False);
            Assert.That(person.IsValid);

            // TearDown
            ((Realm)realm.Target).Dispose();
        }

        [Test]
        public void FinalizedRealmsShouldNotInvalidateSiblingRealms()
        {
            TestHelpers.IgnoreOnWindows("GC blocks on Windows");

            // Arrange
            using var realm = Realm.GetInstance(RealmConfiguration.DefaultConfiguration.DatabasePath);
            var realmThatWillBeFinalized = GetWeakRealm();
            Person person = null;
            realm.Write(() =>
            {
                person = realm.Add(new Person());
            });

            // Act
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Assert
            Assert.That(realmThatWillBeFinalized.IsAlive, Is.False);
            Assert.That(person.IsValid);
        }

        [Test]
        public void TransactionShouldHoldStrongReferenceToRealm()
        {
            TestHelpers.IgnoreOnWindows("GC blocks on Windows");

            TestHelpers.RunAsyncTest(async () =>
            {
                var realm = GetWeakRealm();
                var transaction = CreateTransaction();

                await System.Threading.Tasks.Task.Yield();

                GC.Collect();
                GC.WaitForPendingFinalizers();

                Assert.DoesNotThrow(transaction.Dispose);
                Assert.That(realm.IsAlive);

                // TearDown
                ((Realm)realm.Target).Dispose();

                Transaction CreateTransaction() => ((Realm)realm.Target).BeginWrite();
            });
        }
    }
}
