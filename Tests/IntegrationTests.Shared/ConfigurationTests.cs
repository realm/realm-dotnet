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
            var config1 = new RealmConfiguration("fred.realm");
            var config2 = new RealmConfiguration("barney.realm");

            // Assert
            Assert.That(config1.GetHashCode(), Is.Not.EqualTo(0));  
            Assert.That(config1.GetHashCode(), Is.Not.EqualTo(config2.GetHashCode())); 
        }
    }
}
