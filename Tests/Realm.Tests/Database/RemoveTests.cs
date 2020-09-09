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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Realms.Exceptions;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class RemoveTests : RealmInstanceTest
    {
        [Test]
        public void RemoveSucceedsTest()
        {
            // Arrange
            Person p1 = null, p2 = null, p3 = null;
            _realm.Write(() =>
            {
                p1 = _realm.Add(new Person { FirstName = "A" });
                p2 = _realm.Add(new Person { FirstName = "B" });
                p3 = _realm.Add(new Person { FirstName = "C" });
            });

            // Act
            _realm.Write(() => _realm.Remove(p2));

            // Assert
            var allPeople = _realm.All<Person>().ToList();
            Assert.That(allPeople, Is.EquivalentTo(new List<Person> { p1, p3 }));
        }

        [Test]
        public void RemoveOutsideTransactionShouldFail()
        {
            // Arrange
            Person p = null;
            _realm.Write(() => p = _realm.Add(new Person()));

            // Act and assert
            Assert.That(() => _realm.Remove(p), Throws.TypeOf<RealmInvalidTransactionException>());
        }

        [Test]
        public void RemoveRangeCanRemoveSpecificObjects()
        {
            // Arrange
            _realm.Write(() =>
            {
                _realm.Add(new Person
                {
                    FirstName = "deletable person #1",
                    IsInteresting = false
                });

                _realm.Add(new Person
                {
                    FirstName = "person to keep",
                    IsInteresting = true
                });

                _realm.Add(new Person
                {
                    FirstName = "deletable person #2",
                    IsInteresting = false
                });
            });

            // Act
            _realm.Write(() => _realm.RemoveRange(_realm.All<Person>().Where(p => !p.IsInteresting)));

            // Assert
            Assert.That(_realm.All<Person>().ToList().Select(p => p.FirstName).ToArray(),
                Is.EqualTo(new[] { "person to keep" }));
        }

        [Test]
        public void RemoveAll_WhenGeneric_RemovesAllObjectsOfAGivenType()
        {
            // Arrange
            _realm.Write(() =>
            {
                _realm.Add(new Person());
                _realm.Add(new Person());
                _realm.Add(new Person());
            });

            Assert.That(_realm.All<Person>().Count(), Is.EqualTo(3));

            // Act
            _realm.Write(() => _realm.RemoveAll<Person>());

            // Assert
            Assert.That(_realm.All<Person>().Count(), Is.EqualTo(0));
        }

        [Test]
        public void RemoveAll_WhenDynamic_RemovesAllObjectsOfAGivenType()
        {
            // Arrange
            _realm.Write(() =>
            {
                _realm.Add(new Person());
                _realm.Add(new Person());
                _realm.Add(new Person());
            });

            Assert.That(_realm.All<Person>().Count(), Is.EqualTo(3));

            // Act
            _realm.Write(() => _realm.DynamicApi.RemoveAll(nameof(Person)));

            // Assert
            Assert.That(_realm.All<Person>().Count(), Is.EqualTo(0));
        }

        [Test]
        public void RemoveAllObjectsShouldClearTheDatabase()
        {
            // Arrange
            _realm.Write(() =>
            {
                _realm.Add(new Person());
                _realm.Add(new Person());
                _realm.Add(new AllTypesObject { RequiredStringProperty = string.Empty });

                Assert.That(_realm.All<Person>().Count(), Is.EqualTo(2));
                Assert.That(_realm.All<AllTypesObject>().Count(), Is.EqualTo(1));
            });

            // Act
            _realm.Write(() => _realm.RemoveAll());

            // Assert
            Assert.That(_realm.All<Person>().Count(), Is.EqualTo(0));
            Assert.That(_realm.All<AllTypesObject>().Count(), Is.EqualTo(0));
        }

        [Test]
        public void RemoveObject_FromAnotherRealm_ShouldThrow()
        {
            PerformWithOtherRealm(null, other =>
            {
                Person otherPerson = null;
                other.Write(() =>
                {
                    otherPerson = other.Add(new Person());
                });

                Assert.That(() => _realm.Write(() => _realm.Remove(otherPerson)),
                            Throws.TypeOf<RealmObjectManagedByAnotherRealmException>());
            });
        }

        [Test]
        public void RemoveResults_FromAnotherRealm_ShouldThrow()
        {
            PerformWithOtherRealm(null, other =>
            {
                other.Write(() => other.Add(new Person()));

                var people = other.All<Person>();

                Assert.That(() => _realm.Write(() => _realm.RemoveRange(people)),
                            Throws.TypeOf<RealmObjectManagedByAnotherRealmException>());
            });
        }

        [Test]
        public void RemoveObject_WhenStandalone_ShouldThrow()
        {
            var person = new Person();

            Assert.That(() => _realm.Write(() => _realm.Remove(person)), Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void RemoveObject_FromSameRealm_ShouldWork()
        {
            PerformWithOtherRealm(_realm.Config.DatabasePath, other =>
            {
                Person otherPerson = null;
                other.Write(() =>
                {
                    otherPerson = other.Add(new Person());
                });

                Assert.That(() => _realm.Write(() => _realm.Remove(otherPerson)), Throws.Nothing);
            });
        }

        [Test]
        public void RemoveResults_FromTheSameRealm_ShouldWork()
        {
            PerformWithOtherRealm(_realm.Config.DatabasePath, other =>
            {
                other.Write(() => other.Add(new Person()));

                var people = other.All<Person>();

                Assert.That(() => _realm.Write(() => _realm.RemoveRange(people)), Throws.Nothing);
            });
        }

        private void PerformWithOtherRealm(string path, Action<Realm> action)
        {
            Realm otherRealm;
            using (otherRealm = Realm.GetInstance(path ?? Path.GetTempFileName()))
            {
                action(otherRealm);
            }

            if (path != _realm.Config.DatabasePath)
            {
                Realm.DeleteRealm(otherRealm.Config);
            }
        }
    }
}