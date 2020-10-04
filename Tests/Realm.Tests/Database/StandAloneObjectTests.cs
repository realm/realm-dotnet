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

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class StandAloneObjectTests : RealmTest
    {
        private Person _person;

        protected override void CustomSetUp()
        {
            _person = new Person();
            base.CustomSetUp();
        }

        [Test]
        public void PropertyGet()
        {
            string firstName = null;
            Assert.DoesNotThrow(() => firstName = _person.FirstName);
            Assert.That(string.IsNullOrEmpty(firstName));
        }

        [Test]
        public void PropertySet()
        {
            const string Name = "John";
            Assert.DoesNotThrow(() => _person.FirstName = Name);
            Assert.AreEqual(Name, _person.FirstName);
        }

        [Test]
        public void AddToRealm()
        {
            _person.FirstName = "Arthur";
            _person.LastName = "Dent";
            _person.IsInteresting = true;

            using var realm = GetRealm();
            realm.Write(() =>
            {
                realm.Add(_person);
            });

            Assert.That(_person.IsManaged);

            var p = realm.All<Person>().Single();
            Assert.That(p.FirstName, Is.EqualTo("Arthur"));
            Assert.That(p.LastName, Is.EqualTo("Dent"));
            Assert.That(p.IsInteresting);
        }

        [Test]
        public void RealmObject_WhenStandalone_ShouldHaveDefaultEqualsImplementation()
        {
            var otherPerson = new Person();

            Assert.DoesNotThrow(() => _person.Equals(otherPerson));
        }

        [Test]
        public void RealmObject_WhenManaged_ShouldNotThrow()
        {
            // This is a test to ensure that our weaver is generating valid IL regardless of property configurations
            using var realm = GetRealm();
            Assert.DoesNotThrow(() => realm.Write(() =>
            {
                realm.Add(new NoListProperties());
            }), $"{nameof(NoListProperties)} add failed.");

            Assert.DoesNotThrow(() => realm.Write(() =>
            {
                realm.Add(new OnlyListProperties());
            }), $"{nameof(OnlyListProperties)} add failed.");

            Assert.DoesNotThrow(() => realm.Write(() =>
            {
                realm.Add(new MixedProperties1());
            }), $"{nameof(MixedProperties1)} add failed.");

            Assert.DoesNotThrow(() => realm.Write(() =>
            {
                realm.Add(new MixedProperties2());
            }), $"{nameof(MixedProperties2)} add failed.");

            Assert.DoesNotThrow(() => realm.Write(() =>
            {
                realm.Add(new OneNonListProperty());
            }), $"{nameof(OneNonListProperty)} add failed.");

            Assert.DoesNotThrow(() => realm.Write(() =>
            {
                realm.Add(new OneListProperty());
            }), $"{nameof(OneListProperty)} add failed.");

            Assert.DoesNotThrow(() => realm.Write(() =>
            {
                realm.Add(new AllTypesObject { RequiredStringProperty = string.Empty });
            }), $"{nameof(AllTypesObject)} add failed.");
        }

        public class NoListProperties : RealmObject
        {
            public string Name { get; set; }

            public int Age { get; set; }
        }

        public class OnlyListProperties : RealmObject
        {
            public IList<Person> Friends { get; }

            public IList<Person> Enemies { get; }
        }

        public class MixedProperties1 : RealmObject
        {
            public string Name { get; set; }

            public IList<Person> Friends { get; }

            public int Age { get; set; }

            public IList<Person> Enemies { get; }
        }

        public class MixedProperties2 : RealmObject
        {
            public IList<Person> Friends { get; }

            public int Age { get; set; }

            public IList<Person> Enemies { get; }

            public string Name { get; set; }
        }

        public class OneNonListProperty : RealmObject
        {
            public string Name { get; set; }
        }

        public class OneListProperty : RealmObject
        {
            public IList<Person> People { get; }
        }
    }
}