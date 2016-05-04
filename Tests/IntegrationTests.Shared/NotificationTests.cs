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

#if ENABLE_INTERNAL_NON_PCL_TESTS
using System;
using System.Linq;
using NUnit.Framework;
using Realms;
using System.Threading;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace IntegrationTests.Shared
{
    [TestFixture]
    public class NotificationTests
    {
        private string _databasePath;
        private Realm _realm;

        [SetUp]
        public void Setup()
        {
            _databasePath = Path.GetTempFileName();
            _realm = Realm.GetInstance(_databasePath);
        }

        [TearDown]
        public void TearDown()
        {
            _realm.Close();
            Realm.DeleteRealm(_realm.Config);
        }

        [Test]
        public void ShouldTriggerRealmChangedEvent() 
        {
            // Arrange
            var wasNotified = false;
            _realm.RealmChanged += (sender, e) => { wasNotified = true; };

            // Act
            _realm.Write(() => _realm.CreateObject<Person>());

            // Assert
            Assert.That(wasNotified, "RealmChanged notification was not triggered");
        }


        [Test]
        public void ResultsShouldSendNotifications()
        {
            var query = _realm.All<Person>();
            RealmResults<Person>.ChangeSet changes = null;
            RealmResults<Person>.NotificationCallback cb = (s, c, e) => changes = c;

            using (query.SubscribeForNotifications(cb))
            {
                _realm.Write(() => _realm.CreateObject<Person>());

                TestHelpers.RunEventLoop(TimeSpan.FromMilliseconds(100));
                Assert.That(changes?.InsertedIndices, Is.EquivalentTo(new int[] { 0 }));
            }
        }
    }
}

#endif  // #if ENABLE_INTERNAL_NON_PCL_TESTS
