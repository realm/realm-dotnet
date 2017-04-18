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
using Realms.Exceptions;

using ExplicitAttribute = NUnit.Framework.ExplicitAttribute;

namespace Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class ConfigurationTests
    {
        private static void ReliesOnEncryption()
        {
#if ENCRYPTION_DISABLED
            Assert.Ignore("This test relies on encryption which is not enabled in this build");
#endif
        }

        [Test]
        public void DefaultConfigurationShouldHaveValidPath()
        {
            // Arrange
            var config = RealmConfiguration.DefaultConfiguration;

            // Assert
            Assert.That(Path.IsPathRooted(config.DatabasePath));
        }

        [Test]
        public void CanSetConfigurationPartialPath()
        {
            // Arrange
            var config = RealmConfiguration.DefaultConfiguration.ConfigWithPath("jan" + Path.DirectorySeparatorChar + "docs" + Path.DirectorySeparatorChar);

            // Assert
            Assert.That(Path.IsPathRooted(config.DatabasePath));
            Assert.That(config.DatabasePath, Does.EndWith(Path.Combine("jan", "docs", "default.realm")));
        }

        [Test]
        public void PathIsCanonicalised()
        {
            // Arrange
            var config = RealmConfiguration.DefaultConfiguration.ConfigWithPath(Path.Combine("..", "Documents", "fred.realm"));

            // Assert
            Assert.That(Path.IsPathRooted(config.DatabasePath));
            Assert.That(config.DatabasePath, Does.EndWith(Path.Combine("Documents", "fred.realm")));
            Assert.IsFalse(config.DatabasePath.Contains(".."));  // our use of relative up and down again was normalised out
        }

        [Test]
        public void CanOverrideConfigurationFilename()
        {
            // Arrange
            var config = new RealmConfiguration();
            var config2 = config.ConfigWithPath("fred.realm");

            // Assert
            Assert.That(config2.DatabasePath, Does.EndWith("fred.realm"));
        }

        [Test]
        public void CanSetDefaultConfiguration()
        {
            // Arrange
            var config = new RealmConfiguration();
            RealmConfiguration.DefaultConfiguration = config.ConfigWithPath("fred.realm");

            // Assert
            Assert.That(RealmConfiguration.DefaultConfiguration.DatabasePath, Does.EndWith("fred.realm"));
        }

        [Test]
        public void EncryptionKeyMustBe64Bytes()
        {
            ReliesOnEncryption();

            // Arrange
            var config = new RealmConfiguration("EncryptionKeyMustBe64Bytes.realm");
            var smallKey = new byte[] { 1, 2, 3 };
            var bigKey = new byte[656];

            // Assert
            Assert.Throws<FormatException>(() => config.EncryptionKey = smallKey);
            Assert.Throws<FormatException>(() => config.EncryptionKey = bigKey);
        }

        [Test]
        public void ValidEncryptionKeyAccepted()
        {
            ReliesOnEncryption();

            // Arrange
            var config = new RealmConfiguration("ValidEncryptionKeyAcceoted.realm");
            var goldilocksKey = new byte[64];

            // Assert
            Assert.DoesNotThrow(() => config.EncryptionKey = goldilocksKey);
            Assert.DoesNotThrow(() => config.EncryptionKey = null);
        }

        [Test]
        public void UnableToOpenWithNoKey()
        {
            ReliesOnEncryption();

            // Arrange
            var config = new RealmConfiguration("UnableToOpenWithNoKey.realm");
            Realm.DeleteRealm(config);  // ensure guarded from prev tests
            var emptyKey = new byte[64];
            config.EncryptionKey = emptyKey;
            using (Realm.GetInstance(config))
            {
            }

            config.EncryptionKey = null;

            // Assert
            Assert.Throws<RealmFileAccessErrorException>(() =>
            {
                using (Realm.GetInstance(config))
                {
                }
            });
        }

        [Test]
        public void UnableToOpenWithKeyIfNotEncrypted()
        {
            ReliesOnEncryption();

            // Arrange
            var config = new RealmConfiguration("UnableToOpenWithKeyIfNotEncrypted.realm");
            Realm.DeleteRealm(config);  // ensure guarded from prev tests
            var openedWithoutKey = Realm.GetInstance(config);
            openedWithoutKey.Dispose();
            var emptyKey = new byte[64];
            config.EncryptionKey = emptyKey;

            // Assert
            Assert.Throws<RealmFileAccessErrorException>(() =>
            {
                using (Realm.GetInstance(config))
                {
                }
            });
        }

        [Test]
        public void UnableToOpenWithDifferentKey()
        {
            ReliesOnEncryption();

            // Arrange
            var config = new RealmConfiguration("UnableToOpenWithDifferentKey.realm");
            Realm.DeleteRealm(config);  // ensure guarded from prev tests
            var emptyKey = new byte[64];
            config.EncryptionKey = emptyKey;
            var openedWithKey = Realm.GetInstance(config);
            openedWithKey.Dispose();
            config.EncryptionKey[0] = 42;

            // Assert
            Assert.Throws<RealmFileAccessErrorException>(() =>
            {
                using (Realm.GetInstance(config))
                {
                }
            });
        }

        [Test]
        public void AbleToReopenEncryptedWithSameKey()
        {
            ReliesOnEncryption();

            // Arrange
            var config = new RealmConfiguration("AbleToReopenEncryptedWithSameKey.realm");
            Realm.DeleteRealm(config);  // ensure guarded from prev tests
            var answerKey = new byte[64];
            answerKey[0] = 42;
            config.EncryptionKey = answerKey;
            var openedWithKey = Realm.GetInstance(config);
            openedWithKey.Dispose();

            var config2 = new RealmConfiguration("AbleToReopenEncryptedWithSameKey.realm");
            var answerKey2 = new byte[64];
            answerKey2[0] = 42;
            config2.EncryptionKey = answerKey2;

            // Assert
            Assert.DoesNotThrow(() =>
            {
                using (Realm.GetInstance(config2))
                {
                }
            });
        }

        [Test]
        public void ReadOnlyFilesMustExist()
        {
            // Arrange
            var config = new RealmConfiguration("FileNotThere.realm")
            {
                IsReadOnly = true
            };

            // Assert
            Assert.Throws<RealmFileNotFoundException>(() =>
            {
                Realm.GetInstance(config);
            });
        }

        [Test, Explicit("Currently, a RealmMismatchedConfigException is thrown. Registered as #580")]
        public void ReadOnlyRealmsWillNotAutoMigrate()
        {
            // Arrange
            var config = new RealmConfiguration("WillBeReadonly.realm");
            Realm.DeleteRealm(config);  // ensure start clean
            config.IsReadOnly = true;
            config.SchemaVersion = 42;
            TestHelpers.CopyBundledDatabaseToDocuments(
                "ForMigrationsToCopyAndMigrate.realm", "WillBeReadonly.realm");

            // Assert
            Assert.Throws<RealmMigrationNeededException>(() =>
            {
                Realm.GetInstance(config);
            });
        }

        [Test]
        public void ReadOnlyRealmsArentWritable()
        {
            // Arrange
            var config = new RealmConfiguration("WillBeReadonly.realm");
            Realm.DeleteRealm(config);  // ensure start clean
            config.SchemaVersion = 0;  // must set version before file can be opened readOnly
            using (var openToCreate = Realm.GetInstance(config))
            {
                openToCreate.Write(() =>
                {
                    openToCreate.Add(new Person());
                });
            }

            config.IsReadOnly = true;

            using (var openedReadonly = Realm.GetInstance(config))
            {
                // Assert
                Assert.Throws<RealmInvalidTransactionException>(() =>
                {
                    openedReadonly.Write(() =>
                    {
                        openedReadonly.Add(new Person());
                    });
                });
            }
        }
    }
}