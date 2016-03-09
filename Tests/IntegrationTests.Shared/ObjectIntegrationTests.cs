/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using NUnit.Framework;
using Realms;

namespace IntegrationTests
{
    [TestFixture]
    public class ObjectIntegrationTests : PeopleTestsBase
    {
        [Test, Explicit("Manual test for debugging")]
        public void SimpleTest()
        {
            MakeThreePeople ();
            var allPeople = _realm.All<Person>().Count();
            Debug.WriteLine($"There are {allPeople} in total");

            var interestingPeople = from p in _realm.All<Person>() where p.IsInteresting == true select p;

            Debug.WriteLine("Interesting people include:");
            foreach (var p in interestingPeople)
                Debug.WriteLine(" - " + p.FullName + " (" + p.Email + ")");

            var johns = from p in _realm.All<Person>() where p.FirstName == "John" select p;
            Debug.WriteLine("People named John:");
            foreach (var p in johns)
                Debug.WriteLine(" - " + p.FullName + " (" + p.Email + ")");
        }

        [Test]
        public void CreateObjectTest()
        {
            // Arrange and act
            _realm.Write(() => _realm.CreateObject<Person>());

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
            using (var transaction = _realm.BeginWrite())
            {
                p1.Score = 99.0f;
                Assert.That(p2.Score, Is.EqualTo(99.0f));  // value propagates despite transaction not finished
                Assert.That(p1.Equals(p2));  // identity-based comparison holds
                transaction.Commit(); 
            }

            // Assert
            Assert.That(p2.Score, Is.EqualTo(99.0f));  // value still holds after transaction finished
            Assert.That(p1.Equals(p2));  // identity-based comparison holds
        }

        [Test]
        public void SetAndGetPropertyTest()
        {
            // Arrange
            using (var transaction = _realm.BeginWrite())
            {
                Person p = _realm.CreateObject<Person>();

                // Act
                p.FirstName = "John";
                p.IsInteresting = true;
                p.Score = -0.9907f;
                p.Latitude = 51.508530;
                p.Longitude = 0.076132;
                transaction.Commit();
            }
            var allPeople = _realm.All<Person>().ToList();
            Person p2 = allPeople[0];  // pull it back out of the database otherwise can't tell if just a dumb property
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
            Person p;
            using (var transaction = _realm.BeginWrite())
            {
                p = _realm.CreateObject<Person>();

                // Act
                p.Email = "John@a.com";

                transaction.Commit();
            }
            var receivedEmail = p.Email;

            // Assert
            Assert.That(receivedEmail, Is.EqualTo("John@a.com"));
        }

        [Test]
        public void CreateObjectOutsideTransactionShouldFail()
        {
            // Arrange, act and assert
            Assert.Throws<RealmOutsideTransactionException>(() => _realm.CreateObject<Person>());
        }

        [Test]
        public void ManageOutsideTransactionShouldFail()
        {
            var obj = new Person();
            Assert.Throws<RealmOutsideTransactionException>(() => _realm.Manage(obj));
        }

        [Test]
        public void ManageNullObjectShouldFail()
        {
            Assert.Throws<ArgumentNullException>(() => _realm.Manage(null as Person));
        }

        [Test]
        public void ManageAnObjectFromAnotherRealmShouldFail()
        {
            Person p;
            using (var transaction = _realm.BeginWrite())
            {
                p = _realm.CreateObject<Person>();
                transaction.Commit();
            }

            using (var otherRealm = Realm.GetInstance(Path.GetTempFileName()))
            {
                Assert.Throws<RealmObjectManagedByAnotherRealmException>(() => otherRealm.Manage(p));
            }
        }

        [Test]
        public void ManageAnObjectToRealmItAlreadyBelongsToShouldFail()
        {
            Person p;
            using (var transaction = _realm.BeginWrite())
            {
                p = _realm.CreateObject<Person>();
                transaction.Commit();
            }

            Assert.Throws<RealmObjectAlreadyManagedByRealmException>(() => _realm.Manage(p));
        }

        [Test]
        public void SetPropertyOutsideTransactionShouldFail()
        {
            // Arrange
            Person p;
            using (var transaction = _realm.BeginWrite())
            {
                p = _realm.CreateObject<Person>();
                transaction.Commit();
            }

            // Act and assert
            Assert.Throws<RealmOutsideTransactionException>(() => p.FirstName = "John");
        }


        [Test]
        public void RemoveSucceedsTest()
        {
            // Arrange
            Person p1, p2, p3;
            using (var transaction = _realm.BeginWrite())
            {
                //p1 = new Person { FirstName = "A" };
                //p2 = new Person { FirstName = "B" };
                //p3 = new Person { FirstName = "C" };
                p1 = _realm.CreateObject<Person>(); p1.FirstName = "A";
                p2 = _realm.CreateObject<Person>(); p2.FirstName = "B";
                p3 = _realm.CreateObject<Person>(); p3.FirstName = "C";
                transaction.Commit();
            }

            // Act
            using (var transaction = _realm.BeginWrite())
            {
                _realm.Remove(p2);
                transaction.Commit();
            }

            // Assert
            //Assert.That(!p2.InRealm);

            var allPeople = _realm.All<Person>().ToList();

            Assert.That(allPeople, Is.EquivalentTo(new List<Person> { p1, p3 }));
        }


        [Test]
        public void RemoveOutsideTransactionShouldFail()
        {
            // Arrange
            Person p;
            using (var transaction = _realm.BeginWrite())
            {
                p = _realm.CreateObject<Person>();
                transaction.Commit();
            }

            // Act and assert
            Assert.Throws<RealmOutsideTransactionException>(() => _realm.Remove(p) );
        }

        [Test]
        public void NonAutomaticPropertiesShouldNotBeWoven()
        {
            Assert.That(typeof(Person).GetProperty("Nickname").GetCustomAttributes(typeof(WovenPropertyAttribute), false), Is.Empty);
        }

        [Test]
        public void NonAutomaticPropertiesShouldBeIgnored()
        {
            using (var trans = _realm.BeginWrite())
            {
                var p = _realm.CreateObject<Person>();
                p.FirstName = "Vincent";
                p.LastName = "Adultman";
                p.Nickname = "Vinnie";
                trans.Commit();
            }

            var vinnie = _realm.All<Person>().ToList().Single();
            Assert.That(vinnie.FullName, Is.EqualTo("Vincent Adultman"));
            Assert.That(string.IsNullOrEmpty(vinnie.Nickname));
        }


        [Test]
        public void CanSimplyCountAll()
        {
            MakeThreePeople();
            // note older samples will often use ToList just to get a count, with expressions such as
            // Assert.That(_realm.All<Person>().ToList().Count(), Is.EqualTo(3));
            //var folks = _realm.All<Person>().ToList();
            Assert.That(_realm.All<Person>().Count(), Is.EqualTo(3));
        }

    }

    [TestFixture]
    public class RealmMigrationTests
    {
        [Test]
        public void TriggerMigrationBySchemaVersion()
        {
            // Arrange
            var config1 = new RealmConfiguration("ChangingVersion.realm");
            Realm.DeleteRealm(config1);  // ensure start clean
            var realm1 = Realm.GetInstance(config1);
            // new database doesn't push back a version number
            Assert.That(config1.SchemaVersion, Is.EqualTo(RealmConfiguration.NotVersioned));
            realm1.Close();

            // Act
            var config2 = config1.ConfigWithPath("ChangingVersion.realm");
            config2.SchemaVersion = 99;
            Realm realm2 = null;  // should be updated by DoesNotThrow

            // Assert
            Assert.DoesNotThrow( () => realm2 = Realm.GetInstance(config2) ); // same path, different version, should auto-migrate quietly
            Assert.That(realm2.Config.SchemaVersion, Is.EqualTo(99));
            realm2.Close();

        }

        [Test]
        public void TriggerMigrationBySchemaEditing()
        {
            
            // NOTE to regnerate the bundled database go edit the schema in Person.cs and comment/uncomment ExtraToTriggerMigration
            // running in between and saving a copy with the added field
            // this should never be needed as this test just needs the Realm to need migrating
            TestHelpers.CopyBundledDatabaseToDocuments(
                "ForMigrationsToCopyAndMigrate.realm", "NeedsMigrating.realm");

            // Assert
            Realm realm1 = null;
            Assert.Throws<RealmMigrationNeededException>( () => realm1 = Realm.GetInstance("NeedsMigrating.realm") );
        }

        [Test]
        public void MigrationTriggersDelete()
        {
            // Arrange
            var config = new RealmConfiguration("MigrateWWillRecreate.realm", true);
            Realm.DeleteRealm(config);
            Assert.False(File.Exists(config.DatabasePath));

            TestHelpers.CopyBundledDatabaseToDocuments(
                "ForMigrationsToCopyAndMigrate.realm", "MigrateWWillRecreate.realm");

            // Act - should cope by deleting and silently recreating
            var realm = Realm.GetInstance(config);

            // Assert
            Assert.That(File.Exists(config.DatabasePath));
        }
    }
}
