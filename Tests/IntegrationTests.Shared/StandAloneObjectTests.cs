﻿////////////////////////////////////////////////////////////////////////////
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

using NUnit.Framework;
using Realms;
using System.Collections.Generic;
using System.Linq;
using System;

namespace IntegrationTests.Shared
{
    [TestFixture, Preserve(AllMembers = true)]
    public class StandAloneObjectTests
    {
        private Person _person;

        [SetUp]
        public void SetUp()
        {
            _person = new Person();
            Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);
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
            const string name = "John";
            Assert.DoesNotThrow(() => _person.FirstName = name);
            Assert.AreEqual(name, _person.FirstName);
        }

        [Test]
        public void AddToRealm()
        {
            _person.FirstName = "Arthur";
            _person.LastName = "Dent";
            _person.IsInteresting = true;

            using (var realm = Realm.GetInstance())
            {
                using (var transaction = realm.BeginWrite())
                {
                    realm.Manage(_person);
                    transaction.Commit();
                }

                Assert.That(_person.IsManaged);

                var p = realm.All<Person>().Single();
                Assert.That(p.FirstName, Is.EqualTo("Arthur"));
                Assert.That(p.LastName, Is.EqualTo("Dent"));
                Assert.That(p.IsInteresting);
            }
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

            using (var realm = Realm.GetInstance())
            {
                Assert.DoesNotThrow(() => realm.Write(() =>
                {
                    realm.Manage(new NoListProperties());
                }), $"{nameof(NoListProperties)} manage failed.");

                Assert.DoesNotThrow(() => realm.Write(() =>
                {
                    realm.Manage(new OnlyListProperties());
                }), $"{nameof(OnlyListProperties)} manage failed.");

                Assert.DoesNotThrow(() => realm.Write(() =>
                {
                    realm.Manage(new MixedProperties1());
                }), $"{nameof(MixedProperties1)} manage failed.");

                Assert.DoesNotThrow(() => realm.Write(() =>
                {
                    realm.Manage(new MixedProperties2());
                }), $"{nameof(MixedProperties2)} manage failed.");

                Assert.DoesNotThrow(() => realm.Write(() =>
                {
                    realm.Manage(new OneNonListProperty());
                }), $"{nameof(OneNonListProperty)} manage failed.");

                Assert.DoesNotThrow(() => realm.Write(() =>
                {
                    realm.Manage(new OneListProperty());
                }), $"{nameof(OneListProperty)} manage failed.");

                Assert.DoesNotThrow(() => realm.Write(() =>
                {
                    realm.Manage(new AllPropsClass());
                }), $"{nameof(AllPropsClass)} manage failed.");
            }
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

        public class AllPropsClass : RealmObject
        {
            public string String { get; set; }
            public char Char { get; set; }
            public byte Byte { get; set; }
            public Int16 Int16 { get; set; }
            public Int32 Int32 { get; set; }
            public Int64 Int64 { get; set; }
            public Single Single { get; set; }
            public Double Double { get; set; }
            public DateTimeOffset DateTimeOffset { get; set; }
            public Boolean Boolean { get; set; }
            public Byte[] ByteArray { get; set; }
            public char? NullableChar { get; set; }
            public byte? NullableByte { get; set; }
            public Int16? NullableInt16 { get; set; }
            public Int32? NullableInt32 { get; set; }
            public Int64? NullableInt64 { get; set; }
            public Single? NullableSingle { get; set; }
            public Double? NullableDouble { get; set; }
            public DateTimeOffset? NullableDateTimeOffset { get; set; }
            public Boolean? NullableBoolean { get; set; }
        }
    }
}
