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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NUnit.Framework;
using Realms;

namespace IntegrationTests
{
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    internal class Cities : RealmObject
    {
        public string Name { get; set; }
    }

    [TestFixture, Preserve(AllMembers = true)]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    internal class SortingTests : PeopleTestsBase
    {
        // see comment on base method why this isn't decorated with [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            MakeThreePeople();
            _realm.Write(() =>
            {
                // add an entry like John Doe but interesting
                _realm.Add(new Person
                {
                    FullName = "John Jamez",
                    IsInteresting = true,
                    Email = "john@doe.com",
                    Score = 100,
                    Latitude = 40.7637286,
                    Longitude = -73.9748113
                });
            });
        }

        [Test]
        public void AllSortOneLevel()
        {
            var s0 = _realm.All<Person>().OrderBy(p => p.Score).ToList().Select(p => p.Score);
            Assert.That(s0, Is.EqualTo(new[] { -0.9907f, 42.42f, 100.0f, 100.0f }));

            var s1 = _realm.All<Person>().OrderByDescending(p => p.Latitude).ToList().Select(p => p.Latitude);
            Assert.That(s1, Is.EqualTo(new[] { 51.508530, 40.7637286, 40.7637286, 37.7798657 }));
        }

        [Test]
        public void AllSortUpUp()
        {
            var sortAA = _realm.All<Person>()
                .OrderBy(p => p.FirstName)
                .ThenBy(p => p.Latitude).ToList();
            var sortAAname = sortAA.Select(p => p.FirstName);
            Assert.That(sortAAname, Is.EqualTo(new[] { "John", "John", "John", "Peter" }));
            var sortAAlat = sortAA.Select(p => p.Latitude);
            Assert.That(sortAAlat, Is.EqualTo(new[] { 40.7637286, 40.7637286, 51.508530, 37.7798657 }));
        }

        [Test]
        public void AllSortDownUp()
        {
            var sortDA = _realm.All<Person>()
                .OrderByDescending(p => p.FirstName)
                .ThenBy(p => p.Latitude).ToList();
            var sortDAname = sortDA.Select(p => p.FirstName);
            Assert.That(sortDAname, Is.EqualTo(new[] { "Peter", "John", "John", "John" }));
            var sortDAlat = sortDA.Select(p => p.Latitude);
            Assert.That(sortDAlat, Is.EqualTo(new[] { 37.7798657, 40.7637286, 40.7637286, 51.508530 }));
        }

        [Test]
        public void AllSortUpDown()
        {
            var sortAD = _realm.All<Person>()
                .OrderBy(p => p.FirstName)
                .ThenByDescending(p => p.Latitude).ToList();
            var sortADname = sortAD.Select(p => p.FirstName);
            Assert.That(sortADname, Is.EqualTo(new[] { "John", "John", "John", "Peter" }));
            var sortADlat = sortAD.Select(p => p.Latitude);
            Assert.That(sortADlat, Is.EqualTo(new[] { 51.508530, 40.7637286, 40.7637286, 37.7798657 }));
        }

        [Test]
        public void AllSortDownDown()
        {
            var sortDD = _realm.All<Person>()
                .OrderByDescending(p => p.FirstName)
                .ThenByDescending(p => p.Latitude).ToList();
            var sortDDname = sortDD.Select(p => p.FirstName);
            Assert.That(sortDDname, Is.EqualTo(new[] { "Peter", "John", "John", "John" }));
            var sortDDlat = sortDD.Select(p => p.Latitude);
            Assert.That(sortDDlat, Is.EqualTo(new[] { 37.7798657, 51.508530, 40.7637286, 40.7637286 }));
        }

        [Test]
        public void QuerySortOneLevelNumbers()
        {
            // use OrderByDescending because standard sample numbers happen to be created ascending
            var s0gen = _realm.All<Person>().Where(p => p.IsInteresting).OrderByDescending(p => p.Score).ToList();
            var s0 = s0gen.Select(p => p.Score);
            Assert.That(s0, Is.EqualTo(new[] { 100.0f, 42.42f, -0.9907f }));

            var s1 = _realm.All<Person>().Where(p => p.IsInteresting).OrderBy(p => p.Latitude).ToList().Select(p => p.Latitude);
            Assert.That(s1, Is.EqualTo(new[] { 37.7798657, 40.7637286, 51.508530 }));
        }

        [Test]
        public void QuerySortOneLevelStrings()
        {
            var s0 = _realm.All<Person>().Where(p => p.IsInteresting).OrderBy(p => p.LastName).ToList().Select(p => p.LastName).ToList();
            Assert.That(s0, Is.EqualTo(new[] { "Jameson", "Jamez", "Smith" }));
        }

        [Test]
        public void QuerySortUpUp()
        {
            var sortAA = _realm.All<Person>().Where(p => p.IsInteresting)
                .OrderBy(p => p.FirstName)
                .ThenBy(p => p.Latitude).ToList();
            var sortAAname = sortAA.Select(p => p.FirstName);
            Assert.That(sortAAname, Is.EqualTo(new[] { "John", "John", "Peter" }));
            var sortAAlat = sortAA.Select(p => p.Latitude);
            Assert.That(sortAAlat, Is.EqualTo(new[] { 40.7637286, 51.508530, 37.7798657 }));
        }

        [Test]
        public void QuerySortDownUp()
        {
            var sortDA = _realm.All<Person>().Where(p => p.IsInteresting)
                .OrderByDescending(p => p.FirstName)
                .ThenBy(p => p.Latitude).ToList();
            var sortDAname = sortDA.Select(p => p.FirstName);
            Assert.That(sortDAname, Is.EqualTo(new[] { "Peter", "John", "John" }));
            var sortDAlat = sortDA.Select(p => p.Latitude);
            Assert.That(sortDAlat, Is.EqualTo(new[] { 37.7798657, 40.7637286, 51.508530 }));
        }

        [Test]
        public void QuerySortUpDown()
        {
            var sortAD = _realm.All<Person>().Where(p => p.IsInteresting)
                .OrderBy(p => p.FirstName)
                .ThenByDescending(p => p.Latitude).ToList();
            var sortADname = sortAD.Select(p => p.FirstName);
            Assert.That(sortADname, Is.EqualTo(new[] { "John", "John", "Peter" }));
            var sortADlat = sortAD.Select(p => p.Latitude);
            Assert.That(sortADlat, Is.EqualTo(new[] { 51.508530, 40.7637286, 37.7798657 }));
        }

        [Test]
        public void QuerySortDownDown()
        {
            var sortDD = _realm.All<Person>().Where(p => p.IsInteresting)
                .OrderByDescending(p => p.FirstName)
                .ThenByDescending(p => p.Latitude).ToList();
            var sortDDname = sortDD.Select(p => p.FirstName);
            Assert.That(sortDDname, Is.EqualTo(new[] { "Peter", "John", "John" }));
            var sortDDlat = sortDD.Select(p => p.Latitude);
            Assert.That(sortDDlat, Is.EqualTo(new[] { 37.7798657, 51.508530, 40.7637286 }));
        }

        [Test]
        public void SortExceptionsForInvalidSortCode()
        {
            Assert.Throws<NotSupportedException>(() => _realm.All<Person>().OrderBy(p => p.Latitude > 100).ToList(),
                "If you use an expression other than simple property specifier it throws.");

            Assert.Throws<NotSupportedException>(() => _realm.All<Person>().Where(p => p.IsInteresting)
                .OrderBy(p => p.FirstName).OrderBy(p => p.Latitude).ToList(),
                "Should catch using more than one OrderBy");

            Assert.Throws<NotSupportedException>(() => _realm.All<Person>().Where(p => p.IsInteresting)
                .OrderByDescending(p => p.FirstName).OrderBy(p => p.Latitude).ToList(),
                "Should catch using both OrderBy and OrderByDescending");
        }

        [Test]
        public void FirstIsDifferentSorted()
        {
            var highestScore = _realm.All<Person>().OrderByDescending(p => p.Score).First();
            Assert.That(highestScore.Email, Is.EqualTo("john@doe.com"));

            var sortedFirstInteresting = _realm.All<Person>().OrderByDescending(p => p.FirstName).First(p => p.IsInteresting);
            Assert.That(sortedFirstInteresting.Email, Is.EqualTo("peter@jameson.net"));

            var sortedFirst = _realm.All<Person>()
                .Where(p => p.FirstName == "John")
                .OrderBy(p => p.Latitude)
                .First();
            Assert.That(sortedFirst.Email, Is.EqualTo("john@doe.com"));
        }

        [Test]
        public void LastIsDifferentSorted()
        {
            var lowestScore = _realm.All<Person>().OrderByDescending(p => p.Score).Last();
            Assert.That(lowestScore.Email, Is.EqualTo("john@smith.com"));

            var sortedLastInteresting = _realm.All<Person>().OrderByDescending(p => p.LastName).Last(p => p.IsInteresting);
            Assert.That(sortedLastInteresting.Email, Is.EqualTo("peter@jameson.net"));

            var sortedLast = _realm.All<Person>()
                                   .Where(p => p.FirstName == "John")
                                   .OrderBy(p => p.Salary)
                                   .Last();
            Assert.That(sortedLast.Email, Is.EqualTo("john@doe.com"));
        }

        [Test]
        public void ElementAtSorted()
        {
            var lowestScore = _realm.All<Person>().OrderByDescending(p => p.Score).ElementAt(3);
            Assert.That(lowestScore.Email, Is.EqualTo("john@smith.com"));

            var sortedFirstInteresting = _realm.All<Person>().OrderByDescending(p => p.FirstName).Where(p => p.IsInteresting).ElementAt(0);
            Assert.That(sortedFirstInteresting.Email, Is.EqualTo("peter@jameson.net"));
        }

        [Test]
        public void SortsByAcceptedOrder()
        {
            _realm.Write(() =>
            {
                foreach (var city in new[] { "Santo Domingo", "Åby", "Sydney", "São Paulo", "Shanghai", "A-Place", "A Place" })
                {
                    _realm.Add(new Cities { Name = city });
                }
            });
            var sortedCities = _realm.All<Cities>().OrderBy(c => c.Name).ToList().Select(c => c.Name);
            Assert.That(sortedCities, Is.EqualTo(new[] { "A-Place", "A Place", "Santo Domingo", "São Paulo", "Shanghai", "Sydney", "Åby" }));
        }
    }
}