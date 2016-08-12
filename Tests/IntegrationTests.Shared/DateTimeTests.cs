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

using NUnit.Framework;
using Realms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IntegrationTests.Shared
{
    [TestFixture, Preserve(AllMembers = true)]
    public class DateTimeTests
    {
        //TODO: this is ripe for refactoring across test fixture classes

        protected Realm _realm;

        [SetUp]
        public void Setup()
        {
            Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);
            _realm = Realm.GetInstance();
        }

        [TearDown]
        public void TearDown()
        {
            _realm.Close();
            Realm.DeleteRealm(_realm.Config);
        }


        [Test]
        public void SetAndGetPropertyTest(
            [Values(0, 11, 23)] Int32 hour,
            [Values(0, 6, 30, 59)] Int32 mins,
            [Values(0, 6, 30, 59)] Int32 secs,
            [Values(0, 1, 999)] Int32 ms)
        {
            var turingsBirthday = new DateTimeOffset(1912, 6, 23, hour, mins, secs, ms, TimeSpan.Zero);

            Person turing;
            using (var transaction = _realm.BeginWrite())
            {
                turing = _realm.CreateObject<Person>();
                turing.FirstName = "Alan";
                turing.LastName = "Turing";
                turing.Birthday = turingsBirthday;
                transaction.Commit();
            }

            // perform a db fetch
            var turingAgain = _realm.All<Person>().First();

            Assert.That(turingAgain.Birthday, Is.EqualTo(turingsBirthday));
        }


        [Test]
        public void SortingFinelyDifferentDateTimes()
        {
            using (var transaction = _realm.BeginWrite()) {
                foreach (var ms in new List<Int32> { 10, 999, 998, 42 }) {
                    var birthday = new DateTimeOffset(1912, 6, 23, 23, 59, 59, ms, TimeSpan.Zero);
                    foreach (var addMs in new List<double> { -2000.0, 1.0, -1.0, 1000.0, 100.0 }) {
                        Person turing = _realm.CreateObject<Person>();
                        turing.Birthday = birthday.AddMilliseconds(addMs);
                    }
                }
                transaction.Commit();
            }

            // Assert
            var sortedTurings = _realm.All<Person>().OrderBy(p => p.Birthday);
            DateTimeOffset prevB = new DateTimeOffset();
            foreach (var t in sortedTurings) {
                Assert.That(t.Birthday, Is.GreaterThan(prevB));
                prevB = t.Birthday;
            }
        }


        [Test]
        public void FindingByMilliseconds()
        {
            var birthday = new DateTimeOffset(1912, 6, 23, 23, 59, 59, 0, TimeSpan.Zero);
            using (var transaction = _realm.BeginWrite()) {
                foreach (var addMs in new List<double> { 0.0, 1.0, -1.0 }) {
                    Person turing = _realm.CreateObject<Person>();
                    turing.Birthday = birthday.AddMilliseconds(addMs);
                }
                transaction.Commit();
            }

            // Assert
            Assert.That(_realm.All<Person>().Count(p => p.Birthday < birthday), Is.EqualTo(1));
            Assert.That(_realm.All<Person>().Count(p => p.Birthday == birthday), Is.EqualTo(1));
            Assert.That(_realm.All<Person>().Count(p => p.Birthday >= birthday), Is.EqualTo(2));
            Assert.That(_realm.All<Person>().Count(p => p.Birthday > birthday), Is.EqualTo(1));
        }

        // Issue #294: At one point, simply having an object with an indexed DateTimeOffset property
        // would cause a migration error when instantiating the database. This class and the test
        // below verifies that this issue hasn't snuck back in.
        [Preserve(AllMembers = true)]
        public class IndexedDateTimeOffsetObject : RealmObject
        {
            [Indexed]
            public DateTimeOffset DateTimeOffset { get; set; }
        }

        [Test]
        public void IndexedDateTimeOffsetTest()
        {
            // Arrange
            var config = new RealmConfiguration() {ObjectClasses = new[] {typeof (IndexedDateTimeOffsetObject)}};

            // Act and "assert" that no exception is thrown here
            using (Realm.GetInstance(config)) { }
        }

        [Test]
        public void DateTimeOffsetShouldStoreFullPrecision()
        {
            // Arrange
            const long ticks = 636059331339132912;
            var p = new Person { Birthday = new DateTimeOffset(ticks, TimeSpan.Zero) };

            // Act
            _realm.Write(() => { _realm.Manage(p); });

            // Assert
            Assert.That(p.Birthday.Ticks, Is.EqualTo(ticks));
        }
    }
}
