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

using System.Linq;
using System.IO;
using NUnit.Framework;
using Realms;
using System;
using System.Diagnostics;

namespace IntegrationTests
{
    [TestFixture, Preserve(AllMembers = true)]
    public class MigrationTests
    {
        [Test]
        public void TriggerMigrationBySchemaVersion()
        {
            // Arrange
            var config1 = new RealmConfiguration("ChangingVersion.realm");
            Realm.DeleteRealm(config1);  // ensure start clean
            var realm1 = Realm.GetInstance(config1);
            // new database doesn't push back a version number
            Assert.That(config1.SchemaVersion, Is.EqualTo(0));
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

            var triggersSchemaFieldValue = string.Empty;

            var configuration = new RealmConfiguration("NeedsMigrating.realm");
            configuration.SchemaVersion = 100;
            configuration.MigrationCallback = (migration, oldSchemaVersion) =>
            {
                Assert.That(oldSchemaVersion, Is.EqualTo(99));

                var oldPeople = migration.OldRealm.All("Person");
                var newPeople = migration.NewRealm.All<Person>();

                Assert.That(newPeople.Count(), Is.EqualTo(oldPeople.Count()));

                for (var i = 0; i < newPeople.Count(); i++)
                {
                    var oldPerson = oldPeople.ElementAt(i);
                    var newPerson = newPeople.ElementAt(i);

                    Assert.That(newPerson.LastName, Is.Not.EqualTo(oldPerson.TriggersSchema));
                    newPerson.LastName = triggersSchemaFieldValue = oldPerson.TriggersSchema;
                }
            };

            using (var realm = Realm.GetInstance(configuration))
            {
                var person = realm.All<Person>().Single();
                Assert.That(person.LastName, Is.EqualTo(triggersSchemaFieldValue));
            }
        }

        [Test]
        public void ExceptionInMigrationCallback()
        {
            TestHelpers.CopyBundledDatabaseToDocuments(
                "ForMigrationsToCopyAndMigrate.realm", "NeedsMigrating.realm");

            var dummyException = new Exception();

            var configuration = new RealmConfiguration("NeedsMigrating.realm") { SchemaVersion = 100 };
            configuration.MigrationCallback = (migration, oldSchemaVersion) =>
            {
                throw dummyException;
            };

            var ex = Assert.Throws<AggregateException>(() => Realm.GetInstance(configuration).Close());
            Assert.That(ex.Flatten().InnerException, Is.SameAs(dummyException));
        }

        [Test]
#if WINDOWS
        [Ignore("Automatic deletion doesn't work on Windows at the moment")]
#endif
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

    #region MigrationExperiment
    /*

    namespace Version0
    {
        public class Person : RealmObject
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public int Age { get; set; }
        }
    }

    namespace Version1
    {
        public class Person : RealmObject
        {
            public string FullName { get; set; }
            public int Age { get; set; }
        }
    }

    namespace Version2
    {
        public class Person : RealmObject
        {
            public string FullName { get; set; }
            public DateTimeOffset Birthday { get; set; }
        }
    }

    public class MigrationExperiment
    {
        public void PrintObject(object o)
        {
            Debug.WriteLine(string.Join(", ", o.GetType().GetProperties().Select(p => p.Name + " = " + p.GetValue(o))));
        }

        public void Version0Test()
        {
            var config = new RealmConfiguration("MigrationExperiment") { ObjectClasses = new[] { typeof(Version0.Person) } };
            Realm.DeleteRealm(config);
            var realm = Realm.GetInstance(config);

            realm.Write(() =>
            {
                var p1 = realm.CreateObject<Version0.Person>();
                p1.FirstName = "John";
                p1.LastName = "Peterson";
                p1.Age = 64;

                var p2 = realm.CreateObject<Version0.Person>();
                p2.FirstName = "Peter";
                p2.LastName = "Johnson";
                p2.Age = 28;
            });

            realm.All<Version0.Person>().ToList().ForEach(PrintObject);
        }

        public void Version1Test()
        {
            var config = new RealmConfiguration("MigrationExperiment")
            {
                SchemaVersion = 1,
                ObjectClasses = new[] { typeof(Version1.Person) },
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    var newPeople = migration.NewRealm.All<Version1.Person>();

                    // Use the dynamic api for oldPeople so we can access
                    // .FirstName and .LastName even though they no longer
                    // exist in the class definition.
                    var oldPeople = migration.OldRealm.All("Person");

                    for (var i = 0; i < newPeople.Count(); i++)
                    {
                        var oldPerson = oldPeople.ElementAt(i);
                        var newPerson = newPeople.ElementAt(i);

                        newPerson.FullName = oldPerson.FirstName + " " + oldPerson.LastName;
                    }
                }
            };
            var realm = Realm.GetInstance(config);
            realm.All<Version1.Person>().ToList().ForEach(PrintObject);
        }

        public void Version2Test()
        {
            var config = new RealmConfiguration("MigrationExperiment")
            {
                SchemaVersion = 2,
                ObjectClasses = new[] { typeof(Version2.Person) },
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    var newPeople = migration.NewRealm.All<Version2.Person>();
                    var oldPeople = migration.OldRealm.All("Person");

                    for (var i = 0; i < newPeople.Count(); i++)
                    {
                        var oldPerson = oldPeople.ElementAt(i);
                        var newPerson = newPeople.ElementAt(i);

                        // Migrate Person from version 0 to 1: replace FirstName and LastName with FullName
                        if (oldSchemaVersion < 1)
                        {
                            newPerson.FullName = oldPerson.FirstName + " " + oldPerson.LastName;
                        }

                        // Migrate Person from version 1 to 2: replace Age with Birthday
                        if (oldSchemaVersion < 2)
                        {
                            newPerson.Birthday = DateTimeOffset.Now.AddYears(-(int)oldPerson.Age);
                        }
                    }
                }
            };
            var realm = Realm.GetInstance(config);
            realm.All<Version2.Person>().ToList().ForEach(PrintObject);
        }
    }

    */
    #endregion
}
