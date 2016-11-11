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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Realms;

namespace IntegrationTests.Shared
{
    [TestFixture, Preserve(AllMembers = true)]
    public class PropertyChangedTests
    {
        private string _databasePath;
        private Realm _realm;

        [SetUp]
        public void SetUp()
        {
            _databasePath = Path.GetTempFileName();
            _realm = Realm.GetInstance(_databasePath);
        }

        [TearDown]
        public void TearDown()
        {
            _realm.Dispose();
            Realm.DeleteRealm(_realm.Config);
        }

        [Test]
        public void ShouldTriggerObjectPropertyChangedEvent()
        {
            string notifiedPropertyName = null;
            var person = new Person();
            _realm.Write(() =>
            {
                _realm.Add(person);
            });

            var handler = new PropertyChangedEventHandler((sender, e) => 
            {
                notifiedPropertyName = e.PropertyName;
            });

            person.PropertyChanged += handler;

            Task.Run(() =>
            {
                var realm = Realm.GetInstance(_databasePath);
                var p = realm.All<Person>().First();
                realm.Write(() =>
                {
                    p.FirstName = "Peter";
                });
            }).Wait();

            TestHelpers.RunEventLoop(TimeSpan.FromMilliseconds(1));

            Assert.That(notifiedPropertyName, Is.EqualTo(nameof(Person.FirstName)));
        }
    }
}
