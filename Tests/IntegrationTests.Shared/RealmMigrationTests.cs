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
using NUnit.Framework;
using Realms;


namespace IntegrationTests
{
    [TestFixture, Preserve(AllMembers = true)]
    public class RealmMigrationTests
    {
        [Test]
        public void TriggerMigrationBySchemaVersion()
        {
            // Arrange
            var config1 = new RealmConfiguration("ChangingVersion.realm");
            Realm.DeleteRealm(config1);  // ensure start clean
            var realm1 = Realm.GetInstance(config1);
            Assert.That(config1.SchemaVersion, Is.EqualTo(0));
            realm1.Close();

            // Act
            var config2 = config1.ConfigWithPath("ChangingVersion.realm");
            config2.SchemaVersion = 99;
            Realm realm2 = null;  // should be updated by DoesNotThrow

            // Assert
            Assert.DoesNotThrow(() => realm2 = Realm.GetInstance(config2)); // same path, different version, should auto-migrate quietly
            Assert.That(realm2.Config.SchemaVersion, Is.EqualTo(99));
            realm2.Close();

        }

        [Test]
        [Ignore("this tests for a condition that's only available under manual migrations, which we lack")]
        public void TriggerMigrationBySchemaEditing()
        {

            // NOTE to regnerate the bundled database go edit the schema in Person.cs and comment/uncomment ExtraToTriggerMigration
            // running in between and saving a copy with the added field
            // this should never be needed as this test just needs the Realm to need migrating
            TestHelpers.CopyBundledDatabaseToDocuments(
                "ForMigrationsToCopyAndMigrate.realm", "NeedsMigrating.realm");

            // Assert
            Realm realm1 = null;
            Assert.Throws<RealmMigrationNeededException>(() => realm1 = Realm.GetInstance("NeedsMigrating.realm"));
        }

        [Test]
        [Ignore("Disable until issue 507 fixed - RealmMigrationNeededException no longer thrown")]
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

