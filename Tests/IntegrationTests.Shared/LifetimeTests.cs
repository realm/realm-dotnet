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
using Realms;

namespace IntegrationTests.Shared
{
    [TestFixture, Preserve(AllMembers = true)]
    public class LifetimeTests
    {
        // This method was extracted to ensure that the actual realm instance
        // isn't preserved in the scope of the test, even when the debugger is running.
        private WeakReference GetWeakRealm()
        {
            return new WeakReference(Realm.GetInstance("LifetimeTests.realm"));
        }

        [Test]
        public void RealmObjectsShouldKeepRealmAlive()
        {
            // Arrange
            var realm = GetWeakRealm();
            Person person = null;
            ((Realm)realm.Target).Write(() => { person = ((Realm)realm.Target).CreateObject<Person>(); });

            // Act
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Assert
            Assert.That(realm.IsAlive);
            Assert.That(((Realm)realm.Target).IsClosed, Is.False);
            Assert.That(person.IsValid);
        }

        [Test]
        public void FinalizedRealmsShouldNotInvalidateSiblingRealms()
        {
            // Arrange
            var realm = Realm.GetInstance("LifetimeTests.realm");
            var realmThatWillBeFinalized = GetWeakRealm();
            Person person = null;
            realm.Write(() => { person = realm.CreateObject<Person>(); });

            // Act
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Assert
            Assert.That(realmThatWillBeFinalized.IsAlive, Is.False);
            Assert.That(person.IsValid);
        }
    }
}
