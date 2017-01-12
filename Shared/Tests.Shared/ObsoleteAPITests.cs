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

using System.Linq;
using NUnit.Framework;
using Realms;

namespace IntegrationTests
{
    /// <summary>
    /// These tests validate we don't accidentally break obsoleted API.
    /// Only test API that are more than just proxies.
    /// </summary>
    [TestFixture, Preserve(AllMembers = true)]
    public class ObsoleteAPITests : RealmInstanceTest
    {
        [Test]
        public void CreateObjectTest()
        {
            // Arrange and act
            _realm.Write(() => _realm.CreateObject<Person>());

            // Assert
            var people = _realm.All<Person>();
            Assert.That(people.Count(), Is.EqualTo(1));

            var person = people.Single();

            _realm.Write(() => person.FirstName = "John");

            Assert.That(person.FirstName, Is.EqualTo("John"));
        }
    }
}
