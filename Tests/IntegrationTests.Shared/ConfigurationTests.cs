/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using NUnit.Framework;
using System.Threading.Tasks;
using Realms;

namespace IntegrationTests
{
    [TestFixture]
    public class ConfigurationTests
    {
        private void ReliesOnEncryption()
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
            Assert.That(config.DatabasePath, Is.StringEnding(Path.Combine("jan", "docs", "default.realm")));
        }
        
        [Test]
        public void PathIsCanonicalised()
        {
            // Arrange
            var config = RealmConfiguration.DefaultConfiguration.ConfigWithPath(Path.Combine("..", "Documents", "fred.realm"));

            // Assert
            Assert.That(Path.IsPathRooted(config.DatabasePath));
            Assert.That(config.DatabasePath, Is.StringEnding(Path.Combine("Documents", "fred.realm")));
            Assert.IsFalse(config.DatabasePath.Contains(".."));  // our use of relative up and down again was normalised out
        }

        [Test]
        public void CanOverrideConfigurationFilename()
        {
            // Arrange
            var config = new RealmConfiguration();
            var config2 = config.ConfigWithPath ("fred.realm");

            // Assert
            Assert.That(config2.DatabasePath, Is.StringEnding("fred.realm"));
        }

        [Test]
        public void CanSetDefaultConfiguration()
        {
            // Arrange
            var config = new RealmConfiguration();
            RealmConfiguration.DefaultConfiguration = config.ConfigWithPath ("fred.realm");

            // Assert
            Assert.That(RealmConfiguration.DefaultConfiguration.DatabasePath, Is.StringEnding("fred.realm"));
        }

        [Test]
        public void ConfigurationsAreSame()
        {
            // Arrange
            var config1 = new RealmConfiguration("fred.realm");
            var config2 = new RealmConfiguration("fred.realm");

            // Assert
            Assert.That(config1, Is.EqualTo(config2));
        }

        [Test]
        public void ConfigurationsAreDifferent()
        {
            // Arrange
            var config1 = new RealmConfiguration("fred.realm");
            var config2 = new RealmConfiguration("barney.realm");
            var config1b = new RealmConfiguration("fred.realm", true);

            // Assert
            Assert.That(config1, Is.Not.EqualTo(config2));
            Assert.That(config1, Is.Not.EqualTo(config1b));
        }


        [Test]
        public void ConfigurationsHaveDifferentHashes()
        {
            // Arrange
            var config1 = new RealmConfiguration("ConfigurationsHaveDifferentHashes1.realm");
            var config2 = new RealmConfiguration("ConfigurationsHaveDifferentHashes2.realm");

            // Assert
            Assert.That(config1.GetHashCode(), Is.Not.EqualTo(0));  
            Assert.That(config1.GetHashCode(), Is.Not.EqualTo(config2.GetHashCode())); 
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
            Assert.Throws<FormatException>( () => config.EncryptionKey = smallKey);  
            Assert.Throws<FormatException>( () => config.EncryptionKey = bigKey);  
        }


        [Test]
        public void ValidEncryptionKeyAcceoted()
        {
            ReliesOnEncryption();

            // Arrange
            var config = new RealmConfiguration("ValidEncryptionKeyAcceoted.realm");
            var goldilocksKey = new byte[64];

            // Assert
            Assert.DoesNotThrow( () => config.EncryptionKey = goldilocksKey);  
            Assert.DoesNotThrow( () => config.EncryptionKey = null);  
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
            using (Realm.GetInstance(config)) { }
            config.EncryptionKey = null;

            // Assert
            Assert.Throws<RealmFileAccessErrorException>( () => { using (Realm.GetInstance(config)) {} });  
        }


        [Test]
        public void UnableToOpenWithKeyIfNotEncrypted()
        {
            ReliesOnEncryption();

            // Arrange
            var config = new RealmConfiguration("UnableToOpenWithKeyIfNotEncrypted.realm");
            Realm.DeleteRealm(config);  // ensure guarded from prev tests
            var openedWithoutKey = Realm.GetInstance(config);
            openedWithoutKey.Close();
            var emptyKey = new byte[64]; 
            config.EncryptionKey = emptyKey;  

            // Assert
            Assert.Throws<RealmFileAccessErrorException>( () => { using (Realm.GetInstance(config)) {} });  
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
            openedWithKey.Close();
            config.EncryptionKey[0] = 42;

            // Assert
            Assert.Throws<RealmFileAccessErrorException>( () => { using (Realm.GetInstance(config)) {} });  
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
            openedWithKey.Close();

            var config2 = new RealmConfiguration("AbleToReopenEncryptedWithSameKey.realm");
            var answerKey2 = new byte[64]; 
            answerKey2[0] = 42;
            config2.EncryptionKey = answerKey2;

            // Assert
            Assert.DoesNotThrow( () => { using (Realm.GetInstance(config2)) {} });  
        }
    }
}
