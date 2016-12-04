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
using NUnit.Framework;
using Realms;
using Realms.Sync;

namespace Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class SyncConfigurationTests
    {
        private static readonly Uri serverUri = new Uri("http://example.com");

        [Test]
        public void ConfigurationsAreEquals()
        {
            // Arrange
            var config1 = new SyncConfiguration(new DummyUser(), serverUri, "fred.realm");
            var config2 = new SyncConfiguration(new DummyUser(), serverUri, "fred.realm");

            // Assert
            Assert.That(config1, Is.EqualTo(config2));

            var base1 = (RealmConfigurationBase)config1;
            Assert.That(base1, Is.EqualTo(config2));
            Assert.That(config2, Is.EqualTo(base1));
        }

        [Test]
        public void ConfigurationsAreDifferent()
        {
            // Arrange
            var config1 = new SyncConfiguration(new DummyUser(), serverUri, "fred.realm");
            var config2 = new SyncConfiguration(new DummyUser(), serverUri, "barney.realm");
            var config1b = new SyncConfiguration(new DummyUser("peter"), new Uri("http://example.com"), "fred.realm");

            // Assert
            Assert.That(config1, Is.Not.EqualTo(config2));
            Assert.That(config1, Is.Not.EqualTo(config1b));
        }

        [Test]
        public void ConfigurationsHaveDifferentHashes()
        {
            // Arrange
            var config1 = new SyncConfiguration(new DummyUser(), serverUri, "ConfigurationsHaveDifferentHashes1.realm");
            var config2 = new SyncConfiguration(new DummyUser(), serverUri, "ConfigurationsHaveDifferentHashes2.realm");

            // Assert
            Assert.That(config1.GetHashCode(), Is.Not.EqualTo(0));
            Assert.That(config1.GetHashCode(), Is.Not.EqualTo(config2.GetHashCode()));
        }

        [Test]
        public void ConfigurationsHaveTheSameHashes()
        {
            // Arrange
            var config1 = new SyncConfiguration(new DummyUser(), serverUri, "ConfigurationsHaveTheSameHashes.realm");
            var config2 = new SyncConfiguration(new DummyUser(), serverUri, "ConfigurationsHaveTheSameHashes.realm");

            // Assert
            Assert.That(config1.GetHashCode(), Is.Not.EqualTo(0));
            Assert.That(config1.GetHashCode(), Is.EqualTo(config2.GetHashCode()));
        }

        [Test]
        public void SyncConfigurationAndRealmConfigurationAreDifferent()
        {
            // Arrange
            var syncConfig = new SyncConfiguration(new DummyUser(null), serverUri, "some.realm");
            var realmConfig = new RealmConfiguration("some.realm");

            // Assert
            Assert.That(syncConfig, Is.Not.EqualTo(realmConfig));
            Assert.That(syncConfig.GetHashCode(), Is.Not.EqualTo(realmConfig.GetHashCode()));
            Assert.That((RealmConfigurationBase)syncConfig, Is.Not.EqualTo((RealmConfigurationBase)realmConfig));
        }

        private class DummyUser : User
        {
            public override Uri ServerUri => serverUri;

            public override string Identity { get; }

            public DummyUser(string identity = "user") : base(null)
            {
                Identity = identity;
            }
        }
    }
}
