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
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class MigrationTests : RealmInstanceTest
    {
        private const string FileToMigrate = "ForMigrationsToCopyAndMigrate.realm";

        [Test]
        public void TriggerMigrationBySchemaVersion()
        {
            var config = (RealmConfiguration)RealmConfiguration.DefaultConfiguration;

            using (var realm = GetRealm())
            {
                // new database doesn't push back a version number
                Assert.That(realm.Config.SchemaVersion, Is.EqualTo(0));
            }

            var config2 = config.ConfigWithPath(config.DatabasePath);
            config2.SchemaVersion = 99;

            // same path, different version, should auto-migrate quietly
            using var realm2 = GetRealm(config2);
            Assert.That(realm2.Config.SchemaVersion, Is.EqualTo(99));
        }

        [Test]
        public void TriggerMigrationBySchemaEditing()
        {
            // NOTE to regnerate the bundled database go edit the schema in Person.cs and comment/uncomment ExtraToTriggerMigration
            // running in between and saving a copy with the added field
            // this should never be needed as this test just needs the Realm to need migrating

            // Because Realms opened during migration are not immediately disposed of, they can't be deleted.
            // To circumvent that, we're leaking realm files.
            // See https://github.com/realm/realm-dotnet/issues/1357
            var path = TestHelpers.CopyBundledFileToDocuments(FileToMigrate, Path.Combine(InteropConfig.DefaultStorageFolder, Guid.NewGuid().ToString()));

            var triggersSchemaFieldValue = string.Empty;

            var configuration = new RealmConfiguration(path)
            {
                SchemaVersion = 100,
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    Assert.That(oldSchemaVersion, Is.EqualTo(99));

                    var oldPeople = migration.OldRealm.DynamicApi.All("Person");
                    var newPeople = migration.NewRealm.All<Person>();

                    Assert.That(newPeople.Count(), Is.EqualTo(oldPeople.Count()));

                    for (var i = 0; i < newPeople.Count(); i++)
                    {
                        var oldPerson = oldPeople.ElementAt(i);
                        var newPerson = newPeople.ElementAt(i);

                        Assert.That(newPerson.LastName, Is.Not.EqualTo(oldPerson.TriggersSchema));
                        newPerson.LastName = triggersSchemaFieldValue = oldPerson.TriggersSchema;
                    }
                }
            };

            var realm = GetRealm(configuration);
            var person = realm.All<Person>().Single();
            Assert.That(person.LastName, Is.EqualTo(triggersSchemaFieldValue));
        }

        [Test]
        public void ExceptionInMigrationCallback()
        {
            // Because Realms opened during migration are not immediately disposed of, they can't be deleted.
            // To circumvent that, we're leaking realm files.
            // See https://github.com/realm/realm-dotnet/issues/1357
            var path = TestHelpers.CopyBundledFileToDocuments(FileToMigrate, Path.Combine(InteropConfig.DefaultStorageFolder, Guid.NewGuid().ToString()));

            var dummyException = new Exception();

            var configuration = new RealmConfiguration(path)
            {
                SchemaVersion = 100,
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    throw dummyException;
                }
            };

            var ex = Assert.Throws<AggregateException>(() => GetRealm(configuration).Dispose());
            Assert.That(ex.Flatten().InnerException, Is.SameAs(dummyException));
        }

        [Test]
        public void MigrationTriggersDelete()
        {
            var path = RealmConfiguration.DefaultConfiguration.DatabasePath;

            var oldSchema = new Schema.RealmSchema.Builder();
            {
                var person = new Schema.ObjectSchema.Builder("Person", isEmbedded: false);
                person.Add(new Schema.Property { Name = "Name", Type = Schema.PropertyType.String });
                oldSchema.Add(person.Build());
            }

            using (var realm = Realm.GetInstance(new RealmConfiguration(path) { IsDynamic = true }, oldSchema.Build()))
            {
                realm.Write(() =>
                {
                    dynamic person = realm.DynamicApi.CreateObject("Person", null);
                    person.Name = "Foo";
                });
            }

            var newSchema = new Schema.RealmSchema.Builder();
            {
                var person = new Schema.ObjectSchema.Builder("Person", isEmbedded: false);
                person.Add(new Schema.Property { Name = "Name", Type = Schema.PropertyType.Int });
                newSchema.Add(person.Build());
            }

            using (var realm = Realm.GetInstance(new RealmConfiguration(path) { IsDynamic = true, ShouldDeleteIfMigrationNeeded = true }, newSchema.Build()))
            {
                Assert.That(realm.DynamicApi.All("Person"), Is.Empty);
            }
        }

        [Test]
        public void EmbeddedObjectWithOneBacklinkPerObject()
        {
            var path = RealmConfiguration.DefaultConfiguration.DatabasePath;

            var oldSchema = new Schema.RealmSchema.Builder();
            {
                var person = new Schema.ObjectSchema.Builder("Person", isEmbedded: false);
                person.Add(new Schema.Property { Name = "Name", Type = Schema.PropertyType.String });
                person.Add(new Schema.Property { Name = "Dog", Type = Schema.PropertyType.Object | Schema.PropertyType.Nullable, ObjectType = "Dog" });
                oldSchema.Add(person.Build());

                var dog = new Schema.ObjectSchema.Builder("Dog", isEmbedded: false);
                dog.Add(new Schema.Property { Name = "Name", Type = Schema.PropertyType.String });
                oldSchema.Add(dog.Build());
            }

            using (var realm = Realm.GetInstance(new RealmConfiguration(path) { IsDynamic = true }, oldSchema.Build()))
            {
                realm.Write(() =>
                {
                    dynamic scoobyDoo = realm.DynamicApi.CreateObject("Dog", null);
                    scoobyDoo.Name = "Scooby-Doo";

                    dynamic shaggy = realm.DynamicApi.CreateObject("Person", null);
                    shaggy.Name = "Shaggy Rogers";
                    shaggy.Dog = scoobyDoo;
                });
            }

            var newSchema = new Schema.RealmSchema.Builder();
            {
                var person = new Schema.ObjectSchema.Builder("Person", isEmbedded: false);
                person.Add(new Schema.Property { Name = "Name", Type = Schema.PropertyType.String });
                person.Add(new Schema.Property
                {
                    Name = "Dog",
                    Type = Schema.PropertyType.Object | Schema.PropertyType.Nullable,
                    ObjectType = "Dog"
                });
                newSchema.Add(person.Build());

                var dog = new Schema.ObjectSchema.Builder("Dog", isEmbedded: true);
                dog.Add(new Schema.Property { Name = "Name", Type = Schema.PropertyType.String });
                newSchema.Add(dog.Build());
            }

            // All we need to do to make the change to embedded work IF every embedded object already
            // has one and only one backlink is to increase the schema version.
            var configuration = new RealmConfiguration(path)
            {
                SchemaVersion = 42,
                IsDynamic = true
            };

            using (var realm = Realm.GetInstance(configuration, newSchema.Build()))
            {
                Assert.AreEqual(realm.DynamicApi.All("Person").Count(), 1);
                Assert.AreEqual(realm.DynamicApi.All("Person").First().Name, "Shaggy Rogers");
                Assert.NotNull(realm.DynamicApi.All("Person").First().Dog);
                Assert.AreEqual(realm.DynamicApi.All("Person").First().Dog.Name, "Scooby-Doo");
            }
        }

        [Test]
        public void EmbeddedObjectBecomingOrphaned()
        {
            var path = RealmConfiguration.DefaultConfiguration.DatabasePath;

            var oldSchema = new Schema.RealmSchema.Builder();
            {
                var person = new Schema.ObjectSchema.Builder("Person", isEmbedded: false);
                person.Add(new Schema.Property { Name = "Name", Type = Schema.PropertyType.String });
                person.Add(new Schema.Property
                {
                    Name = "Dog",
                    Type = Schema.PropertyType.Object | Schema.PropertyType.Nullable,
                    ObjectType = "Dog"
                });
                oldSchema.Add(person.Build());

                var dog = new Schema.ObjectSchema.Builder("Dog", isEmbedded: false);
                dog.Add(new Schema.Property { Name = "Name", Type = Schema.PropertyType.String });
                oldSchema.Add(dog.Build());
            }

            using (var realm = Realm.GetInstance(new RealmConfiguration(path) { IsDynamic = true }, oldSchema.Build()))
            {
                realm.Write(() =>
                {
                    dynamic scoobyDoo = realm.DynamicApi.CreateObject("Dog", null);
                    scoobyDoo.Name = "Scooby-Doo";

                    dynamic shaggy = realm.DynamicApi.CreateObject("Person", null);
                    shaggy.Name = "Shaggy Rogers";
                });
            }

            var newSchema = new Schema.RealmSchema.Builder();
            {
                var person = new Schema.ObjectSchema.Builder("Person", isEmbedded: false);
                person.Add(new Schema.Property { Name = "Name", Type = Schema.PropertyType.String });
                person.Add(new Schema.Property { Name = "Dog", Type = Schema.PropertyType.Object | Schema.PropertyType.Nullable, ObjectType = "Dog" });
                newSchema.Add(person.Build());

                var dog = new Schema.ObjectSchema.Builder("Dog", isEmbedded: true);
                dog.Add(new Schema.Property { Name = "Name", Type = Schema.PropertyType.String });
                newSchema.Add(dog.Build());
            }

            var configuration = new RealmConfiguration(path)
            {
                SchemaVersion = 42,
                IsDynamic = true,
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    Assert.That(oldSchemaVersion, Is.EqualTo(0));

                    var oldPeople = migration.OldRealm.DynamicApi.All("Person");
                    var newPeople = migration.NewRealm.DynamicApi.All("Person");
                    Assert.That(newPeople.Count(), Is.EqualTo(oldPeople.Count()));

                    var oldDogs = migration.OldRealm.DynamicApi.All("Dog");
                    var newDogs = migration.NewRealm.DynamicApi.All("Dog"); // Throws
                    Assert.That(newDogs.Count(), Is.EqualTo(oldDogs.Count()));
                }
            };

            using (var realm = Realm.GetInstance(configuration, newSchema.Build()))
            {
                Assert.AreEqual(realm.DynamicApi.All("Person").Count(), 1);
                Assert.AreEqual(realm.DynamicApi.All("Person").First().Name, "Shaggy Rogers");
            }
        }

        [Test]
        public void EmbeddedObjectWithMultipleBacklinks()
        {
            var path = RealmConfiguration.DefaultConfiguration.DatabasePath;

            var oldSchema = new Schema.RealmSchema.Builder();
            {
                var person = new Schema.ObjectSchema.Builder("Person", isEmbedded: false);
                person.Add(new Schema.Property { Name = "Name", Type = Schema.PropertyType.String });
                person.Add(new Schema.Property { Name = "Dog", Type = Schema.PropertyType.Object | Schema.PropertyType.Nullable, ObjectType = "Dog" });
                oldSchema.Add(person.Build());

                var dog = new Schema.ObjectSchema.Builder("Dog", isEmbedded: false);
                dog.Add(new Schema.Property { Name = "Name", Type = Schema.PropertyType.String });
                oldSchema.Add(dog.Build());
            }

            using (var realm = Realm.GetInstance(new RealmConfiguration(path) { IsDynamic = true }, oldSchema.Build()))
            {
                realm.Write(() =>
                {
                    dynamic scoobyDoo = realm.DynamicApi.CreateObject("Dog", null);
                    scoobyDoo.Name = "Scooby-Doo";

                    dynamic shaggy = realm.DynamicApi.CreateObject("Person", null);
                    shaggy.Name = "Shaggy Rogers";
                    shaggy.Dog = scoobyDoo;

                    dynamic charlie = realm.DynamicApi.CreateObject("Person", null);
                    charlie.Name = "Charlie Brown";
                    charlie.Dog = scoobyDoo;
                });
            }

            var newSchema = new Schema.RealmSchema.Builder();
            {
                var person = new Schema.ObjectSchema.Builder("Person", isEmbedded: false);
                person.Add(new Schema.Property { Name = "Name", Type = Schema.PropertyType.String });
                person.Add(new Schema.Property { Name = "Dog", Type = Schema.PropertyType.Object | Schema.PropertyType.Nullable, ObjectType = "Dog" });
                newSchema.Add(person.Build());

                var dog = new Schema.ObjectSchema.Builder("Dog", isEmbedded: true);
                dog.Add(new Schema.Property { Name = "Name", Type = Schema.PropertyType.String });
                newSchema.Add(dog.Build());
            }

            var configuration = new RealmConfiguration(path)
            {
                SchemaVersion = 42,
                IsDynamic = true,
            };

            using (var realm = Realm.GetInstance(configuration, newSchema.Build()))
            {
                Assert.AreEqual(realm.DynamicApi.All("Person").Count(), 1);
                Assert.AreEqual(realm.DynamicApi.All("Person").First().Name, "Shaggy Rogers");
                Assert.NotNull(realm.DynamicApi.All("Person").First().Dog);
                Assert.AreEqual(realm.DynamicApi.All("Person").First().Dog.Name, "Scooby-Doo");
            }
        }
    }
}
