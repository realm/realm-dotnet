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
using MongoDB.Bson;
using NUnit.Framework;
using TestExplicitAttribute = NUnit.Framework.ExplicitAttribute;

namespace Realms.Tests.Database
{
    internal class Cities : RealmObject
    {
        public string Name { get; set; }
    }

    [TestFixture, Preserve(AllMembers = true)]
    internal class SortingTests : PeopleTestsBase
    {
        protected override void CustomSetUp()
        {
            base.CustomSetUp();
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
        public void OrderBy_OverAComplexExpression_Throws()
        {
            Assert.Throws<NotSupportedException>(() => _realm.All<Person>().OrderBy(p => p.Latitude > 100).ToList(),
                "If you use an expression other than simple property specifier it throws.");
        }

        [Test, TestExplicit("Blocked on https://github.com/realm/realm-core/issues/3884")]
        public void OrderBy_WhenCalledConsequently_ReplacesOrdering()
        {
            // TODO: verify ordering is replaced once https://github.com/realm/realm-core/issues/3884 is resolved
            _ = _realm.All<Person>().Where(p => p.IsInteresting)
                .OrderBy(p => p.FirstName).OrderBy(p => p.Latitude).ToList();

            _ = _realm.All<Person>().Where(p => p.IsInteresting)
                .OrderByDescending(p => p.FirstName).OrderBy(p => p.Latitude).ToList();
        }

        [Test]
        public void All_OrderBy_OverLinks()
        {
            MakeThreeLinkingObjects("B", 3, 10000, "A", 1, 5000, "C", 2, 1000);

            var level1 = _realm.All<Level1>()
                               .OrderBy(l => l.StringValue)
                               .ToArray();

            Assert.That(level1.Select(l => l.StringValue), Is.EqualTo(new[] { "A", "B", "C" }));

            var level2 = _realm.All<Level1>()
                               .OrderBy(l => l.Level2.IntValue)
                               .ToArray();

            Assert.That(level2.Select(l => l.StringValue), Is.EqualTo(new[] { "A", "C", "B" }));
            Assert.That(level2.Select(l => l.Level2.IntValue), Is.EqualTo(new[] { 1, 2, 3 }));

            var level3 = _realm.All<Level1>()
                               .OrderBy(l => l.Level2.Level3.DateValue)
                               .ToArray();

            Assert.That(level3.Select(l => l.StringValue), Is.EqualTo(new[] { "C", "A", "B" }));
            Assert.That(level3.Select(l => l.Level2.IntValue), Is.EqualTo(new[] { 2, 1, 3 }));
            Assert.That(level3.Select(l => l.Level2.Level3.DateValue), Is.EqualTo(new[] { Date(1000), Date(5000), Date(10000) }));
        }

        [Test]
        public void All_OrderByDescending_OverLinks()
        {
            MakeThreeLinkingObjects("B", 3, 10000, "A", 1, 5000, "C", 2, 1000);

            var level1 = _realm.All<Level1>()
                               .OrderByDescending(l => l.StringValue)
                               .ToArray();

            Assert.That(level1.Select(l => l.StringValue), Is.EqualTo(new[] { "C", "B", "A" }));

            var level2 = _realm.All<Level1>()
                               .OrderByDescending(l => l.Level2.IntValue)
                               .ToArray();

            Assert.That(level2.Select(l => l.StringValue), Is.EqualTo(new[] { "B", "C", "A" }));
            Assert.That(level2.Select(l => l.Level2.IntValue), Is.EqualTo(new[] { 3, 2, 1 }));

            var level3 = _realm.All<Level1>()
                               .OrderByDescending(l => l.Level2.Level3.DateValue)
                               .ToArray();

            Assert.That(level3.Select(l => l.StringValue), Is.EqualTo(new[] { "B", "A", "C" }));
            Assert.That(level3.Select(l => l.Level2.IntValue), Is.EqualTo(new[] { 3, 1, 2 }));
            Assert.That(level3.Select(l => l.Level2.Level3.DateValue), Is.EqualTo(new[] { Date(10000), Date(5000), Date(1000) }));
        }

        [Test]
        public void All_OrderByThenByDescending_OverLinks()
        {
            MakeThreeLinkingObjects("A", 2, 10000, "B", 2, 5000, "C", 1, 10000);

            var items = _realm.All<Level1>()
                              .OrderBy(o => o.Level2.IntValue)
                              .ThenByDescending(o => o.Level2.Level3.DateValue)
                              .ToArray();

            Assert.That(items.Select(l => l.StringValue), Is.EqualTo(new[] { "C", "A", "B" }));
        }

        [Test]
        public void All_OrderByDescendingThenBy_OverLinks()
        {
            MakeThreeLinkingObjects("A", 2, 10000, "B", 2, 5000, "C", 1, 10000);

            var items = _realm.All<Level1>()
                              .OrderByDescending(o => o.Level2.IntValue)
                              .ThenBy(o => o.Level2.Level3.DateValue)
                              .ToArray();

            Assert.That(items.Select(l => l.StringValue), Is.EqualTo(new[] { "B", "A", "C" }));
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

        [TestCase(true)]
        [TestCase(false)]
        public void SortsByDecimal(bool ascending)
        {
            var data = new[] { 1.23M, 9.123M, 5.323423M, -123.324M, 9.123M, decimal.MinValue, decimal.Zero, decimal.MaxValue };
            _realm.Write(() =>
            {
                foreach (var value in data)
                {
                    _realm.Add(new DecimalsObject { DecimalValue = value });
                }
            });

            if (ascending)
            {
                var sortedDecimals = _realm.All<DecimalsObject>().OrderBy(d => d.DecimalValue).ToArray().Select(d => d.DecimalValue);
                Assert.That(sortedDecimals, Is.EqualTo(data.OrderBy(d => d)));
            }
            else
            {
                var sortedDecimals = _realm.All<DecimalsObject>().OrderByDescending(d => d.DecimalValue).ToArray().Select(d => d.DecimalValue);
                Assert.That(sortedDecimals, Is.EqualTo(data.OrderByDescending(d => d)));
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SortsByDecimal128(bool ascending)
        {
            var data = new Decimal128[] { 1.23M, 1, 94, 9.123M, 5.323423M, Decimal128.MinValue, Decimal128.MaxValue, Decimal128.Zero, -123.324M, 9.123M, decimal.MinValue, decimal.Zero, decimal.MaxValue };
            _realm.Write(() =>
            {
                foreach (var value in data)
                {
                    _realm.Add(new DecimalsObject { Decimal128Value = value });
                }
            });

            if (ascending)
            {
                var sortedDecimals = _realm.All<DecimalsObject>().OrderBy(d => d.Decimal128Value).ToArray().Select(d => d.Decimal128Value);
                Assert.That(sortedDecimals, Is.EqualTo(data.OrderBy(d => d)));
            }
            else
            {
                var sortedDecimals = _realm.All<DecimalsObject>().OrderByDescending(d => d.Decimal128Value).ToArray().Select(d => d.Decimal128Value);
                Assert.That(sortedDecimals, Is.EqualTo(data.OrderByDescending(d => d)));
            }
        }

        private void MakeThreeLinkingObjects(
            string string1, int int1, long date1,
            string string2, int int2, long date2,
            string string3, int int3, long date3)
        {
            _realm.Write(() =>
            {
                _realm.Add(new Level1
                {
                    StringValue = string1,
                    Level2 = new Level2
                    {
                        IntValue = int1,
                        Level3 = new Level3
                        {
                            DateValue = Date(date1)
                        }
                    }
                });

                _realm.Add(new Level1
                {
                    StringValue = string2,
                    Level2 = new Level2
                    {
                        IntValue = int2,
                        Level3 = new Level3
                        {
                            DateValue = Date(date2)
                        }
                    }
                });

                _realm.Add(new Level1
                {
                    StringValue = string3,
                    Level2 = new Level2
                    {
                        IntValue = int3,
                        Level3 = new Level3
                        {
                            DateValue = Date(date3)
                        }
                    }
                });
            });
        }

        private static DateTimeOffset Date(long ticks)
        {
            return new DateTimeOffset(ticks, TimeSpan.Zero);
        }

        private class Level1 : RealmObject
        {
            public string StringValue { get; set; }

            public Level2 Level2 { get; set; }
        }

        private class Level2 : RealmObject
        {
            public int IntValue { get; set; }

            public Level3 Level3 { get; set; }
        }

        public class Level3 : RealmObject
        {
            public DateTimeOffset DateValue { get; set; }
        }
    }
}