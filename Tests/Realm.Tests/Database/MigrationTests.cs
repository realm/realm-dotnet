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
using System.Collections.Generic;
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

                    var oldPeople = (IQueryable<RealmObject>)migration.OldRealm.DynamicApi.All("Person");
                    var newPeople = migration.NewRealm.All<Person>();

                    Assert.That(newPeople.Count(), Is.EqualTo(oldPeople.Count()));

                    for (var i = 0; i < newPeople.Count(); i++)
                    {
                        var oldPerson = oldPeople.ElementAt(i);
                        var newPerson = newPeople.ElementAt(i);

                        Assert.That(newPerson.LastName, Is.Not.EqualTo(oldPerson.DynamicApi.Get<string>("TriggersSchema")));
                        newPerson.LastName = triggersSchemaFieldValue = oldPerson.DynamicApi.Get<string>("TriggersSchema");

                        if (!TestHelpers.IsUnity)
                        {
                            // Ensure we can still use the dynamic API during migrations
                            dynamic dynamicOldPerson = oldPeople.ElementAt(i);
                            Assert.That(dynamicOldPerson.TriggersSchema, Is.EqualTo(oldPerson.DynamicApi.Get<string>("TriggersSchema")));
                        }
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
        public void MigrationRenameProperty()
        {
            var path = TestHelpers.CopyBundledFileToDocuments(FileToMigrate, Path.Combine(InteropConfig.DefaultStorageFolder, Guid.NewGuid().ToString()));

            var oldValues = new List<string>();

            var configuration = new RealmConfiguration(path)
            {
                SchemaVersion = 100,
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    var oldPeople = (IQueryable<RealmObject>)migration.OldRealm.DynamicApi.All("Person");
                    var newPeople = migration.NewRealm.All<Person>();

                    for (var i = 0; i < newPeople.Count(); i++)
                    {
                        var oldPerson = oldPeople.ElementAt(i);
                        var newPerson = newPeople.ElementAt(i);

                        var oldValue = oldPerson.DynamicApi.Get<string>("TriggersSchema");
                        oldValues.Add(oldValue);
                        Assert.That(newPerson.OptionalAddress, Is.Not.EqualTo(oldValue));
                    }

                    migration.RenameProperty(nameof(Person), "TriggersSchema", nameof(Person.OptionalAddress));
                }
            };

            using var realm = GetRealm(configuration);
            var newPeople = realm.All<Person>();

            // We cannod do this check in the migration block because we cannot access the renamed property after RenameProperty is called.
            for (var i = 0; i < newPeople.Count(); i++)
            {
                var newPerson = newPeople.ElementAt(i);
                Assert.That(newPerson.OptionalAddress, Is.EqualTo(oldValues[i]));
            }
        }

        [Test]
        public void MigrationRemoveTypeInSchema()
        {
            var oldRealmConfig = new RealmConfiguration()
            {
                SchemaVersion = 0,
                ObjectClasses = new[] { typeof(Dog), typeof(Owner), typeof(Person) },
            };

            using (var oldRealm = GetRealm(oldRealmConfig))
            {
            }

            var newRealmConfig = new RealmConfiguration()
            {
                SchemaVersion = 1,
                ObjectClasses = new[] { typeof(PrimaryKeyObjectIdObject), typeof(Person) },
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    migration.RemoveType(nameof(Person));
                }
            };

            var ex = Assert.Throws<AggregateException>(() => GetRealm(newRealmConfig));
            Assert.That(ex.Flatten().InnerException.Message, Does.Contain("Attempted to remove a type present in the current schema"));
        }

        [Test]
        public void MigrationRemoveTypeNotInSchema()
        {
            var oldRealmConfig = new RealmConfiguration()
            {
                SchemaVersion = 0,
                ObjectClasses = new[] { typeof(Dog), typeof(Owner), typeof(Person) },
            };

            using (var oldRealm = GetRealm(oldRealmConfig))
            {
                oldRealm.Write(() =>
                {
                    oldRealm.Add(new Person { FirstName = "Maria" });
                });
            }

            bool migrationCallbackCalled = false;
            var newRealmConfig = new RealmConfiguration()
            {
                SchemaVersion = 1,
                ObjectClasses = new[] { typeof(Dog), typeof(Owner) },
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    migrationCallbackCalled = true;

                    var migrationResultNotInSchema = migration.RemoveType("NotInSchemaType");
                    Assert.That(migrationResultNotInSchema, Is.False);

                    var migrationResult = migration.RemoveType(nameof(Person));
                    Assert.That(migrationResult, Is.True);

                    // Removed type in oldRealm is still available for the duration of the migration
                    var oldPeople = (IQueryable<RealmObject>)migration.OldRealm.DynamicApi.All("Person");
                    var oldPerson = oldPeople.First();

                    Assert.That(oldPeople.Count(), Is.EqualTo(1));
                    Assert.That(oldPerson.DynamicApi.Get<string>("FirstName"), Is.EqualTo("Maria"));
                }
            };

            using (var newRealm = GetRealm(newRealmConfig))
            {
                Assert.That(migrationCallbackCalled, Is.True);

                // This just means that "Person" is not in the schema that we pass in the config, but we're not sure it has been removed.
                var ex = Assert.Throws<ArgumentException>(() => newRealm.DynamicApi.All("Person"));
                Assert.That(ex.Message, Does.Contain("The class Person is not in the limited set of classes for this realm"));
            }

            var newRealmDynamicConfig = new RealmConfiguration()
            {
                SchemaVersion = 1,
                IsDynamic = true,
            };

            using (var newRealmDynamic = GetRealm(newRealmDynamicConfig))
            {
                // This means that "Person" is not in the schema anymore, as we retrieve it directly from core.
                var ex = Assert.Throws<ArgumentException>(() => newRealmDynamic.DynamicApi.All("Person"));
                Assert.That(ex.Message, Does.Contain("The class Person is not in the limited set of classes for this realm"));
            }
        }
    }
}
