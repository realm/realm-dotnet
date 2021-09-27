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
using Realms.Exceptions;
using Realms.Schema;

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

            var oldConfig = new RealmConfiguration(path)
            {
                IsDynamic = true,
                Schema = new RealmSchema.Builder
                {
                    new ObjectSchema.Builder("Person", isEmbedded: false)
                    {
                        Property.FromType<string>("Name")
                    }
                }
            };

            using (var realm = GetRealm(oldConfig))
            {
                realm.Write(() =>
                {
                    var person = (RealmObject)(object)realm.DynamicApi.CreateObject("Person", null);
                    person.DynamicApi.Set("Name", "Foo");
                });
            }

            var newConfig = new RealmConfiguration
            {
                IsDynamic = true,
                ShouldDeleteIfMigrationNeeded = true,
                Schema = new RealmSchema.Builder
                {
                    new ObjectSchema.Builder("Person", isEmbedded: false)
                    {
                        Property.FromType<int>("Name")
                    }
                }
            };

            using (var realm = GetRealm(newConfig))
            {
                Assert.That(realm.DynamicApi.All("Person"), Is.Empty);

                realm.Write(() =>
                {
                    var person = (RealmObject)(object)realm.DynamicApi.CreateObject("Person", null);
                    person.DynamicApi.Set("Name", 123);
                });
            }
        }

        [Test]
        public void MigrationRenameProperty()
        {
            var path = TestHelpers.CopyBundledFileToDocuments(FileToMigrate, Path.Combine(InteropConfig.DefaultStorageFolder, Guid.NewGuid().ToString()));

            var oldPropertyValues = new List<string>();

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
                        oldPropertyValues.Add(oldValue);
                        Assert.That(newPerson.OptionalAddress, Is.Not.EqualTo(oldValue));
                    }

                    migration.RenameProperty(nameof(Person), "TriggersSchema", nameof(Person.OptionalAddress));
                }
            };

            using var realm = GetRealm(configuration);
            var newPeople = realm.All<Person>();

            // We cannod do this check in the migration block because we cannot access the renamed property after RenameProperty is called.
            var newPropertyValues = newPeople.ToArray().Select(p => p.OptionalAddress);
            Assert.That(newPropertyValues, Is.EquivalentTo(oldPropertyValues));
        }

        [Test]
        public void MigrationRenamPropertyErrors()
        {
            var path = TestHelpers.CopyBundledFileToDocuments(FileToMigrate, Path.Combine(InteropConfig.DefaultStorageFolder, Guid.NewGuid().ToString()));

            var oldPropertyValues = new List<string>();

            var configuration = new RealmConfiguration(path)
            {
                SchemaVersion = 100,
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    migration.RenameProperty(nameof(Person), "TriggersSchema", "PropertyNotInNewSchema");
                }
            };

            var ex = Assert.Throws<RealmException>(() => GetRealm(configuration));
            Assert.That(ex.Message, Does.Contain("Renamed property 'Person.PropertyNotInNewSchema' does not exist"));

            configuration = new RealmConfiguration(path)
            {
                SchemaVersion = 100,
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    migration.RenameProperty(nameof(Person), "PropertyNotInOldSchema", nameof(Person.OptionalAddress));
                }
            };

            var ex2 = Assert.Throws<AggregateException>(() => GetRealm(configuration));
            Assert.That(ex2.Flatten().InnerException.Message, Does.Contain("Cannot rename property 'Person.PropertyNotInOldSchema' because it does not exist"));

            configuration = new RealmConfiguration(path)
            {
                SchemaVersion = 100,
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    migration.RenameProperty("NonExistingType", "TriggersSchema", nameof(Person.OptionalAddress));
                }
            };

            ex2 = Assert.Throws<AggregateException>(() => GetRealm(configuration));
            Assert.That(ex2.Flatten().InnerException.Message, Does.Contain("Cannot rename properties for type 'NonExistingType' because it does not exist"));

            configuration = new RealmConfiguration(path)
            {
                SchemaVersion = 100,
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    migration.RenameProperty(nameof(Person), "TriggersSchema", nameof(Person.Birthday));
                }
            };

            ex2 = Assert.Throws<AggregateException>(() => GetRealm(configuration));
            Assert.That(ex2.Flatten().InnerException.Message, Does.Contain("Cannot rename property 'Person.TriggersSchema' to 'Birthday' because it would change from type 'string' to 'date'."));

            configuration = new RealmConfiguration(path)
            {
                SchemaVersion = 100,
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    migration.RenameProperty(nameof(Person), nameof(Person.Latitude), nameof(Person.Longitude));
                }
            };

            ex2 = Assert.Throws<AggregateException>(() => GetRealm(configuration));
            Assert.That(ex2.Flatten().InnerException.Message, Does.Contain("Cannot rename property 'Person.Latitude' to 'Longitude' because the source property still exists."));
        }

        [TestCase(null, "TriggersSchema", "OptionalAddress")]
        [TestCase("", "TriggersSchema", "OptionalAddress")]
        [TestCase("Person", null, "OptionalAddress")]
        [TestCase("Person", "", "OptionalAddress")]
        [TestCase("Person", "TriggersSchema", null)]
        [TestCase("Person", "TriggersSchema", "")]
        public void MigrationRenamePropertyInvalidArguments(string typeName, string oldName, string newName)
        {
            var path = TestHelpers.CopyBundledFileToDocuments(FileToMigrate, Path.Combine(InteropConfig.DefaultStorageFolder, Guid.NewGuid().ToString()));

            var configuration = new RealmConfiguration(path)
            {
                SchemaVersion = 100,
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    Assert.Throws<ArgumentNullException>(() => migration.RenameProperty(typeName, oldName, newName));
                }
            };

            using var realm = GetRealm(configuration);
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
            Assert.That(ex.Flatten().InnerException.Message, Does.Contain("Attempted to remove type 'Person', that is present in the current schema"));
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

            var migrationCallbackCalled = false;
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
                Assert.That(newRealm.Schema.Find("Person"), Is.Null);
            }

            var newRealmDynamicConfig = new RealmConfiguration()
            {
                SchemaVersion = 1,
                IsDynamic = true,
            };

            using (var newRealmDynamic = GetRealm(newRealmDynamicConfig))
            {
                // This means that "Person" is not in the schema anymore, as we retrieve it directly from core.
                Assert.That(newRealmDynamic.Schema.Find("Person"), Is.Null);
            }
        }

        [TestCase("")]
        [TestCase(null)]
        public void MigrationRemoveTypeInvalidArguments(string typeName)
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
                    Assert.Throws<ArgumentNullException>(() => migration.RemoveType(typeName));
                }
            };

            using var realm = GetRealm(newRealmConfig);
        }
    }
}
