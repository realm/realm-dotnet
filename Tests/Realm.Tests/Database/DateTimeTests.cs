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
using System.Linq;
using NUnit.Framework;
#if TEST_WEAVER
using TestAsymmetricObject = Realms.AsymmetricObject;
using TestEmbeddedObject = Realms.EmbeddedObject;
using TestRealmObject = Realms.RealmObject;
#else
using TestAsymmetricObject = Realms.IAsymmetricObject;
using TestEmbeddedObject = Realms.IEmbeddedObject;
using TestRealmObject = Realms.IRealmObject;
#endif

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class DateTimeTests : RealmInstanceTest
    {
        [Test]
        public void SetAndGetPropertyTest(
            [Values(0, 11, 23)] int hour,
            [Values(0, 6, 30, 59)] int mins,
            [Values(0, 6, 30, 59)] int secs,
            [Values(0, 1, 999)] int ms)
        {
            var turingsBirthday = new DateTimeOffset(1912, 6, 23, hour, mins, secs, ms, TimeSpan.Zero);

            _realm.Write(() =>
            {
                _realm.Add(new Person
                {
                    FirstName = "Alan",
                    LastName = "Turing",
                    Birthday = turingsBirthday
                });
            });

            // perform a db fetch
            var turingAgain = _realm.All<Person>().First();

            Assert.That(turingAgain.Birthday, Is.EqualTo(turingsBirthday));
        }

        [Test]
        public void SortingFinelyDifferentDateTimes()
        {
            using (var transaction = _realm.BeginWrite())
            {
                foreach (var ms in new int[] { 10, 999, 998, 42 })
                {
                    var birthday = new DateTimeOffset(1912, 6, 23, 23, 59, 59, ms, TimeSpan.Zero);
                    foreach (var addMs in new double[] { -2000.0, 1.0, -1.0, 1000.0, 100.0 })
                    {
                        _realm.Add(new Person { Birthday = birthday.AddMilliseconds(addMs) });
                    }
                }

                transaction.Commit();
            }

            // Assert
            var sortedTurings = _realm.All<Person>().OrderBy(p => p.Birthday);
            var prevB = default(DateTimeOffset);
            foreach (var t in sortedTurings)
            {
                Assert.That(t.Birthday, Is.GreaterThan(prevB));
                prevB = t.Birthday;
            }
        }

        [Test]
        public void FindingByMilliseconds()
        {
            var birthday = new DateTimeOffset(1912, 6, 23, 23, 59, 59, 0, TimeSpan.Zero);
            using (var transaction = _realm.BeginWrite())
            {
                foreach (var addMs in new double[] { 0.0, 1.0, -1.0 })
                {
                    _realm.Add(new Person { Birthday = birthday.AddMilliseconds(addMs) });
                }

                transaction.Commit();
            }

            // Assert
            Assert.That(_realm.All<Person>().Count(p => p.Birthday < birthday), Is.EqualTo(1));
            Assert.That(_realm.All<Person>().Count(p => p.Birthday == birthday), Is.EqualTo(1));
            Assert.That(_realm.All<Person>().Count(p => p.Birthday >= birthday), Is.EqualTo(2));
            Assert.That(_realm.All<Person>().Count(p => p.Birthday > birthday), Is.EqualTo(1));
        }

        [Test]
        public void IndexedDateTimeOffsetTest()
        {
            // Arrange
            var config = new RealmConfiguration(Guid.NewGuid().ToString())
            {
                Schema = new[] { typeof(IndexedDateTimeOffsetObject) }
            };

            // Act and "assert" that no exception is thrown here
            using (GetRealm(config))
            {
            }
        }

        [Test]
        public void DateTimeOffsetShouldStoreFullPrecision()
        {
            // Arrange
            const long Ticks = 636059331339132912;
            var p = new Person { Birthday = new DateTimeOffset(Ticks, TimeSpan.Zero) };

            // Act
            _realm.Write(() => { _realm.Add(p); });

            // Assert
            Assert.That(p.Birthday.Ticks, Is.EqualTo(Ticks));
        }
    }

    // Issue #294: At one point, simply having an object with an indexed DateTimeOffset property
    // would cause a migration error when instantiating the database. This class and the test
    // below verifies that this issue hasn't snuck back in.
    public partial class IndexedDateTimeOffsetObject : TestRealmObject
    {
        [Indexed]
        public DateTimeOffset DateTimeOffset { get; set; }
    }
}
