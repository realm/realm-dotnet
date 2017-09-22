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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using NUnit.Framework;
using Realms;
using Realms.Exceptions;

using ExplicitAttribute = NUnit.Framework.ExplicitAttribute;

namespace Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class ConfigurationTests : RealmTest
    {
        private RealmConfiguration _configuration;

        protected override bool OverrideDefaultConfig => false;

        protected override void CustomSetUp()
        {
            base.CustomSetUp();
            _configuration = new RealmConfiguration(Path.GetTempFileName());
        }

        protected override void CustomTearDown()
        {
            base.CustomTearDown();

            Realm.DeleteRealm(_configuration);
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
            Assert.That(config.DatabasePath, Does.Not.Contain(".."));  // our use of relative up and down again was normalised out
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
            // Arrange
            var config = new RealmConfiguration();
            var smallKey = new byte[] { 1, 2, 3 };
            var bigKey = new byte[656];

            // Assert
            Assert.That(() => config.EncryptionKey = smallKey, Throws.TypeOf<FormatException>());
            Assert.That(() => config.EncryptionKey = bigKey, Throws.TypeOf<FormatException>());
        }

        [Test]
        public void ValidEncryptionKeyAccepted()
        {
            // Arrange
            var config = new RealmConfiguration();

            // Assert
            Assert.That(() => config.EncryptionKey = TestHelpers.GetEncryptionKey(), Throws.Nothing);
            Assert.That(() => config.EncryptionKey = null, Throws.Nothing);
        }

        [Test]
        public void UnableToOpenWithNoKey()
        {
            // Arrange
            _configuration.EncryptionKey = TestHelpers.GetEncryptionKey();
            using (Realm.GetInstance(_configuration))
            {
            }

            _configuration.EncryptionKey = null;

            // Assert
            Assert.That(() => Realm.GetInstance(_configuration), Throws.TypeOf<RealmFileAccessErrorException>());
        }

        [Test]
        public void UnableToOpenWithKeyIfNotEncrypted()
        {
            // Arrange
            using (Realm.GetInstance(_configuration))
            {
            }

            _configuration.EncryptionKey = TestHelpers.GetEncryptionKey();

            // Assert
            Assert.That(() => Realm.GetInstance(_configuration), Throws.TypeOf<RealmFileAccessErrorException>());
        }

        [Test]
        public void UnableToOpenWithDifferentKey()
        {
            // Arrange
            _configuration.EncryptionKey = TestHelpers.GetEncryptionKey();

            using (Realm.GetInstance(_configuration))
            {
            }

            _configuration.EncryptionKey[0] = 42;

            // Assert
            Assert.That(() => Realm.GetInstance(_configuration), Throws.TypeOf<RealmFileAccessErrorException>());
        }

        [Test]
        public void AbleToReopenEncryptedWithSameKey()
        {
            // Arrange
            _configuration.EncryptionKey = TestHelpers.GetEncryptionKey(42);

            using (Realm.GetInstance(_configuration))
            {
            }

            var config2 = new RealmConfiguration(_configuration.DatabasePath)
            {
                EncryptionKey = TestHelpers.GetEncryptionKey(42)
            };

            // Assert
            Assert.That(() =>
            {
                using (Realm.GetInstance(config2))
                {
                }
            }, Throws.Nothing);
        }

        [Test]
        public void ReadOnlyFilesMustExist()
        {
            // Arrange
            _configuration.IsReadOnly = true;

            // Assert
            Assert.That(() => Realm.GetInstance(_configuration), Throws.TypeOf<RealmFileAccessErrorException>());
        }

        [Test, Explicit("Currently, a RealmMismatchedConfigException is thrown. Registered as #580")]
        public void ReadOnlyRealmsWillNotAutoMigrate()
        {
            // Arrange
            _configuration.IsReadOnly = true;
            _configuration.SchemaVersion = 42;
            TestHelpers.CopyBundledDatabaseToDocuments(
                "ForMigrationsToCopyAndMigrate.realm", Path.GetFileName(_configuration.DatabasePath));

            // Assert
            Assert.That(() => Realm.GetInstance(_configuration), Throws.TypeOf<RealmMigrationNeededException>());
        }

        [Test]
        public void ReadOnlyRealmsArentWritable()
        {
            // Arrange
            _configuration.SchemaVersion = 0;  // must set version before file can be opened readOnly
            using (var openToCreate = Realm.GetInstance(_configuration))
            {
                openToCreate.Write(() =>
                {
                    openToCreate.Add(new Person());
                });
            }

            _configuration.IsReadOnly = true;

            using (var openedReadonly = Realm.GetInstance(_configuration))
            {
                // Assert
                Assert.That(() =>
                {
                    openedReadonly.Write(() =>
                    {
                        openedReadonly.Add(new Person());
                    });
                }, Throws.TypeOf<RealmInvalidTransactionException>());
            }
        }

        [Test]
        public void DuplicateClassNames_ThrowsException()
        {
            var config = new RealmConfiguration
            {
                ObjectClasses = new[]
                {
                    typeof(Foo.DuplicateClass),
                    typeof(Bar.DuplicateClass)
                }
            };

            var constraint = Throws.TypeOf<NotSupportedException>().And
                                   .Message.Contains("Foo.DuplicateClass").And
                                   .Message.Contains("Bar.DuplicateClass");

            Assert.That(() => Realm.GetInstance(config), constraint);
        }
    }
}

namespace Foo
{
    [Explicit]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public class DuplicateClass : RealmObject
    {
        public int IntValue { get; set; }
    }
}

namespace Bar
{
    [Explicit]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public class DuplicateClass : RealmObject
    {
        public string StringValue { get; set; }
    }
}