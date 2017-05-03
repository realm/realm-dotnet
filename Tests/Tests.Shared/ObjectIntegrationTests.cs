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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Realms;
using Realms.Exceptions;

using ExplicitAttribute = NUnit.Framework.ExplicitAttribute;

namespace Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class ObjectIntegrationTests : PeopleTestsBase
    {
        [Test, Explicit("Manual test for debugging")]
        public void SimpleTest()
        {
            MakeThreePeople();
            var allPeople = _realm.All<Person>().Count();
            Debug.WriteLine($"There are {allPeople} in total");

            var interestingPeople = from p in _realm.All<Person>() where p.IsInteresting == true select p;

            Debug.WriteLine("Interesting people include:");
            foreach (var p in interestingPeople)
            {
                Debug.WriteLine(" - " + p.FullName + " (" + p.Email + ")");
            }

            var johns = from p in _realm.All<Person>() where p.FirstName == "John" select p;
            Debug.WriteLine("People named John:");
            foreach (var p in johns)
            {
                Debug.WriteLine(" - " + p.FullName + " (" + p.Email + ")");
            }
        }

        // Test added to ensure there were no side-effects immedately after a Rollback
        [Test]
        public void CreateObjectAfterRollbackTest()
        {
            using (var transaction = _realm.BeginWrite())
            {
                _realm.Add(new Person { FirstName = "No", LastName = "Body" });
                transaction.Rollback();
            }

            // one user bug report in 2016 led to a suspicion that a Rollback might leave a Realm in an unwritable state
            _realm.Write(() => _realm.Add(new Person { FirstName = "Some", LastName = "Body" }));

            // Assert
            Assert.That(_realm.All<Person>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void ReadAndWriteEqualityTest()
        {
            // Arrange
            MakeThreePeople();
            var p1 = _realm.All<Person>().First(p => p.Score >= 100);
            var p2 = _realm.All<Person>().First(p => p.Score >= 100);
            Assert.That(p1.Equals(p2));

            // Act
            _realm.Write(() =>
            {
                p1.Score = 99.0f;
                Assert.That(p2.Score, Is.EqualTo(99.0f)); // value propagates despite transaction not finished
                Assert.That(p1.Equals(p2)); // identity-based comparison holds
            });

            // Assert
            Assert.That(p2.Score, Is.EqualTo(99.0f));  // value still holds after transaction finished
            Assert.That(p1.Equals(p2));  // identity-based comparison holds
        }

        [Test]
        public void SetAndGetPropertyTest()
        {
            // Arrange
            _realm.Write(() =>
            {
                var p = _realm.Add(new Person());

                // Act
                p.FirstName = "John";
                p.IsInteresting = true;
                p.Score = -0.9907f;
                p.Latitude = 51.508530;
                p.Longitude = 0.076132;
            });

            var allPeople = _realm.All<Person>().ToList();
            var p2 = allPeople[0];  // pull it back out of the database otherwise can't tell if just a dumb property
            var receivedFirstName = p2.FirstName;
            var receivedIsInteresting = p2.IsInteresting;
            var receivedScore = p2.Score;
            var receivedLatitude = p2.Latitude;

            // Assert
            Assert.That(receivedFirstName, Is.EqualTo("John"));
            Assert.That(receivedIsInteresting, Is.True);
            Assert.That(receivedScore, Is.EqualTo(-0.9907f));
            Assert.That(receivedLatitude, Is.EqualTo(51.508530));
        }

        [Test]
        public void SetRemappedPropertyTest()
        {
            // Arrange
            Person p = null;
            _realm.Write(() =>
            {
                p = _realm.Add(new Person());

                // Act
                p.Email = "John@a.com";
            });

            var receivedEmail = p.Email;

            // Assert
            Assert.That(receivedEmail, Is.EqualTo("John@a.com"));
        }

        [Test]
        public void CreateObjectOutsideTransactionShouldFail()
        {
            // Arrange, act and assert
            Assert.That(() => _realm.Add(new Person()), Throws.TypeOf<RealmInvalidTransactionException>());
        }

        [Test]
        public void AddOutsideTransactionShouldFail()
        {
            var obj = new Person();
            Assert.That(() => _realm.Add(obj), Throws.TypeOf<RealmInvalidTransactionException>());
        }

        [Test]
        public void AddNullObjectShouldFail()
        {
            Assert.That(() => _realm.Add(null as Person), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void AddAnObjectFromAnotherRealmShouldFail()
        {
            Person p = null;
            _realm.Write(() => p = _realm.Add(new Person()));

            var secondaryConfig = new RealmConfiguration("AddAnObjectFromAnotherRealmShouldFail");
            Realm.DeleteRealm(secondaryConfig);
            using (var otherRealm = Realm.GetInstance(secondaryConfig))
            {
                Assert.That(() => otherRealm.Add(p), Throws.TypeOf<RealmObjectManagedByAnotherRealmException>());
            }
        }

        [Test]
        public void SetPropertyOutsideTransactionShouldFail()
        {
            // Arrange
            Person p = null;
            _realm.Write(() => p = _realm.Add(new Person()));

            // Act and assert
            Assert.That(() => p.FirstName = "John", Throws.TypeOf<RealmInvalidTransactionException>());
        }

        [Test]
        public void NonAutomaticPropertiesShouldNotBeWoven()
        {
            Assert.That(typeof(Person).GetProperty("Nickname").GetCustomAttributes(typeof(WovenPropertyAttribute), false), Is.Empty);
        }

        [Test]
        public void NonAutomaticPropertiesShouldBeIgnored()
        {
            _realm.Write(() =>
            {
                _realm.Add(new Person
                {
                    FirstName = "Vincent",
                    LastName = "Adultman",
                    Nickname = "Vinnie"
                });
            });

            var vinnie = _realm.All<Person>().ToList().Single();
            Assert.That(vinnie.FullName, Is.EqualTo("Vincent Adultman"));
            Assert.That(string.IsNullOrEmpty(vinnie.Nickname));
        }

        [Test]
        public void CanSimplyCountAll()
        {
            MakeThreePeople();
            Assert.That(_realm.All<Person>().Count(), Is.EqualTo(3));
        }

        [Test]
        public void IteratePeople()
        {
            MakeThreePeople();

            // primarily just testing we iterate through all the people in the realm
            int iterCount = 0;
            var emails = new[] { "john@smith.com", "john@doe.com", "peter@jameson.net" };
            foreach (var p in _realm.All<Person>())
            {
                Assert.That(p.Email, Is.EqualTo(emails[iterCount]));
                iterCount++;
            }

            Assert.That(iterCount, Is.EqualTo(3));
        }

        [Test]
        public void IsValidReturnsTrueWhenObjectIsAttached()
        {
            _realm.Write(() =>
            {
                var p1 = _realm.Add(new Person());
                Assert.That(p1.IsValid);

                _realm.Remove(p1);
                Assert.That(p1.IsValid, Is.False);
            });

            // IsValid should always return true for standalone objects
            var p2 = new Person();
            Assert.That(p2.IsValid);
        }
    }
}