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
    public class MigrationTests : RealmTest
    {
        private const string FileToMigrate = "ForMigrationsToCopyAndMigrate.realm";

        [Test]
        public void TriggerMigrationBySchemaVersion()
        {
            var config = (RealmConfiguration)RealmConfiguration.DefaultConfiguration;

            // Arrange
            using (var realm = Realm.GetInstance())
            {
                // new database doesn't push back a version number
                Assert.That(realm.Config.SchemaVersion, Is.EqualTo(0));
            }

            // Act
            var config2 = config.ConfigWithPath(config.DatabasePath);
            config2.SchemaVersion = 99;
            Realm realm2 = null;  // should be updated by DoesNotThrow

            try
            {
                // Assert
                Assert.That(() => realm2 = Realm.GetInstance(config2), Throws.Nothing); // same path, different version, should auto-migrate quietly
                Assert.That(realm2.Config.SchemaVersion, Is.EqualTo(99));
            }
            finally
            {
                realm2.Dispose();
            }
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
            var path = TestHelpers.CopyBundledFileToDocuments(FileToMigrate, Path.GetTempFileName());

            var triggersSchemaFieldValue = string.Empty;

            var configuration = new RealmConfiguration(path)
            {
                SchemaVersion = 100,
                MigrationCallback = (migration, oldSchemaVersion) =>
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
            // Because Realms opened during migration are not immediately disposed of, they can't be deleted.
            // To circumvent that, we're leaking realm files.
            // See https://github.com/realm/realm-dotnet/issues/1357
            var path = TestHelpers.CopyBundledFileToDocuments(FileToMigrate, Path.GetTempFileName());

            var dummyException = new Exception();

            var configuration = new RealmConfiguration(path)
            {
                SchemaVersion = 100,
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    throw dummyException;
                }
            };

            var ex = Assert.Throws<AggregateException>(() => Realm.GetInstance(configuration).Dispose());
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
                    dynamic person = realm.CreateObject("Person", null);
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
                Assert.That(realm.All("Person"), Is.Empty);
            }
        }
    }
}
