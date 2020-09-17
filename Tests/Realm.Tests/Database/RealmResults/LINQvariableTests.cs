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
using MongoDB.Bson;
using NUnit.Framework;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class LINQvariableTests : PeopleTestsBase
    {
        protected override void CustomSetUp()
        {
            base.CustomSetUp();
            MakeThreePeople();
        }

        [TestCase("Peter", 1)]
        [TestCase("Zach", 0)]
        [TestCase("John", 2)]
        public void FirstNamesEqual(string matchName, int expectFound)
        {
            var c0 = _realm.All<Person>().Count(p => p.FirstName == matchName);
            Assert.That(c0, Is.EqualTo(expectFound));

            var c1 = _realm.All<Person>().Count(p => p.FirstName.StartsWith(matchName));
            Assert.That(c1, Is.EqualTo(expectFound));

            var c2 = _realm.All<Person>().Count(p => p.FirstName.Contains(matchName));
            Assert.That(c2, Is.EqualTo(expectFound));

            var c3 = _realm.All<Person>().Count(p => p.FirstName.EndsWith(matchName));
            Assert.That(c3, Is.EqualTo(expectFound));
        }

        [TestCase("P", 1)]
        [TestCase("Z", 0)]
        [TestCase("J", 2)]
        public void SingleLetterStartSearch(string matchName, int expectFound)
        {
            var c1 = _realm.All<Person>().Count(p => p.FirstName.StartsWith(matchName));
            Assert.That(c1, Is.EqualTo(expectFound));

            var c2 = _realm.All<Person>().Count(p => p.FirstName.Contains(matchName));
            Assert.That(c2, Is.EqualTo(expectFound));
        }

        [TestCase("r", 1)]
        [TestCase("z", 0)]
        [TestCase("n", 2)]
        public void SingleLetterEndSearch(string matchName, int expectFound)
        {
            var c2 = _realm.All<Person>().Count(p => p.FirstName.Contains(matchName));
            Assert.That(c2, Is.EqualTo(expectFound));

            var c3 = _realm.All<Person>().Count(p => p.FirstName.EndsWith(matchName));
            Assert.That(c3, Is.EqualTo(expectFound));
        }

        [TestCase(0.0f, 200.0f, 2)]
        [TestCase(0.0f, 90.0f, 1)]
        [TestCase(0.0f, 30.0f, 0)]
        [TestCase(-1.0f, 0.0f, 1)]
        public void ScoreWithinRange(float minScore, float maxScore, int expectFound)
        {
            var c0 = _realm.All<Person>().Count(p => p.Score > minScore && p.Score <= maxScore);
            Assert.That(c0, Is.EqualTo(expectFound));
        }

        [TestCaseSource(nameof(DecimalTestData))]
        public void DecimalTests(decimal value, int expectGreater, int expectGreaterEqual, int expectLess, int expectLessEqual)
        {
            SeedDecimalData();

            var greater = _realm.All<DecimalsObject>().Count(d => d.DecimalValue > value);
            Assert.That(greater, Is.EqualTo(expectGreater));

            var greaterEqual = _realm.All<DecimalsObject>().Count(d => d.DecimalValue >= value);
            Assert.That(greaterEqual, Is.EqualTo(expectGreaterEqual));

            var less = _realm.All<DecimalsObject>().Count(d => d.DecimalValue < value);
            Assert.That(less, Is.EqualTo(expectLess));

            var lessEqual = _realm.All<DecimalsObject>().Count(d => d.DecimalValue <= value);
            Assert.That(lessEqual, Is.EqualTo(expectLessEqual));
        }

        [TestCaseSource(nameof(Decimal128TestData))]
        public void Decimal128Tests(Decimal128 value, int expectGreater, int expectGreaterEqual, int expectLess, int expectLessEqual)
        {
            SeedDecimal128Data();

            var greater = _realm.All<DecimalsObject>().Count(d => d.Decimal128Value > value);
            Assert.That(greater, Is.EqualTo(expectGreater));

            var greaterEqual = _realm.All<DecimalsObject>().Count(d => d.Decimal128Value >= value);
            Assert.That(greaterEqual, Is.EqualTo(expectGreaterEqual));

            var less = _realm.All<DecimalsObject>().Count(d => d.Decimal128Value < value);
            Assert.That(less, Is.EqualTo(expectLess));

            var lessEqual = _realm.All<DecimalsObject>().Count(d => d.Decimal128Value <= value);
            Assert.That(lessEqual, Is.EqualTo(expectLessEqual));
        }

        // The following test cases exercise both Convert and Member RHS expressions
        [TestCase("Peter", 1)]
        [TestCase("Zach", 0)]
        [TestCase("John", 2)]
        public void FirstNamesEqual_TestCaseData(string matchName, int expectFound)
        {
            var data = new TestCaseData(matchName, expectFound);

            var c0 = _realm.All<Person>().Count(p => p.FirstName == (string)data.Arguments[0]);
            Assert.That(c0, Is.EqualTo(data.Arguments[1]));

            var c1 = _realm.All<Person>().Count(p => p.FirstName.StartsWith((string)data.Arguments[0]));
            Assert.That(c1, Is.EqualTo(data.Arguments[1]));

            var c2 = _realm.All<Person>().Count(p => p.FirstName.Contains((string)data.Arguments[0]));
            Assert.That(c2, Is.EqualTo(data.Arguments[1]));

            var c3 = _realm.All<Person>().Count(p => p.FirstName.EndsWith((string)data.Arguments[0]));
            Assert.That(c3, Is.EqualTo(data.Arguments[1]));
        }

        [TestCase("P", 1)]
        [TestCase("Z", 0)]
        [TestCase("J", 2)]
        public void SingleLetterStartSearch_TestCaseData(string matchName, int expectFound)
        {
            var data = new TestCaseData(matchName, expectFound);

            var c1 = _realm.All<Person>().Count(p => p.FirstName.StartsWith((string)data.Arguments[0]));
            Assert.That(c1, Is.EqualTo(data.Arguments[1]));

            var c2 = _realm.All<Person>().Count(p => p.FirstName.Contains((string)data.Arguments[0]));
            Assert.That(c2, Is.EqualTo(data.Arguments[1]));
        }

        [TestCase("r", 1)]
        [TestCase("z", 0)]
        [TestCase("n", 2)]
        public void SingleLetterEndSearch_TestCaseData(string matchName, int expectFound)
        {
            var data = new TestCaseData(matchName, expectFound);

            var c2 = _realm.All<Person>().Count(p => p.FirstName.Contains((string)data.Arguments[0]));
            Assert.That(c2, Is.EqualTo(data.Arguments[1]));

            var c3 = _realm.All<Person>().Count(p => p.FirstName.EndsWith((string)data.Arguments[0]));
            Assert.That(c3, Is.EqualTo(data.Arguments[1]));
        }

        [TestCase(0.0f, 200.0f, 2)]
        [TestCase(0.0f, 90.0f, 1)]
        [TestCase(0.0f, 30.0f, 0)]
        [TestCase(-1.0f, 0.0f, 1)]
        public void ScoreWithinRange_TestCaseData(float minScore, float maxScore, int expectFound)
        {
            var data = new TestCaseData(minScore, maxScore, expectFound);

            var c0 = _realm.All<Person>().Count(p => p.Score > (float)data.Arguments[0] && p.Score <= (float)data.Arguments[1]);
            Assert.That(c0, Is.EqualTo(data.Arguments[2]));
        }

        public static object[] DecimalTestData =
        {
            new object[] { decimal.MinValue, 4, 5, 0, 1 },
            new object[] { 3.5438693468936346437634743733M, 3, 3, 2, 2 },
            new object[] { 3.5438693468936346437634743734M, 2, 3, 2, 3 },
            new object[] { 3.5438693468936346437634743735M, 2, 2, 3, 3 },
            new object[] { decimal.MaxValue, 0, 1, 4, 5 },
        };

        public static object[] Decimal128TestData =
        {
            new object[] { Decimal128.MinValue, 6, 7, 0, 1 },
            new object[] { new Decimal128(decimal.MinValue), 5, 6, 1, 2 },
            new object[] { new Decimal128(3.5438693468936346437634743733M), 4, 4, 3, 3 },
            new object[] { new Decimal128(3.5438693468936346437634743734M), 3, 4, 3, 4 },
            new object[] { new Decimal128(3.5438693468936346437634743735M), 3, 3, 4, 4 },
            new object[] { new Decimal128(decimal.MaxValue), 1, 2, 5, 6 },
            new object[] { Decimal128.MaxValue, 0, 1, 6, 7 },
        };

        private void SeedDecimalData()
        {
            var decimalSeedData = new decimal[] { decimal.MinValue, 0, 3.5438693468936346437634743734M, 45, decimal.MaxValue };

            _realm.Write(() =>
            {
                foreach (var d in decimalSeedData)
                {
                    _realm.Add(new DecimalsObject { DecimalValue = d });
                }
            });
        }

        private void SeedDecimal128Data()
        {
            var decimal128SeedData = new Decimal128[] { Decimal128.MinValue, decimal.MinValue, 0, 3.5438693468936346437634743734M, 45, decimal.MaxValue, Decimal128.MaxValue };

            _realm.Write(() =>
            {
                foreach (var d in decimal128SeedData)
                {
                    _realm.Add(new DecimalsObject { Decimal128Value = d });
                }
            });
        }
    }
}