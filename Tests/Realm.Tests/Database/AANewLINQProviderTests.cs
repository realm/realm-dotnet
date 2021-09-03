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
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class AANewLINQProviderTests : PeopleTestsBase
    {
        protected override void CustomSetUp()
        {
            base.CustomSetUp();
            MakeThreePeople();
        }

        [Test]
        public void WhereNullEqTest()
        {
            var addressEqNull = _realm.All<Person>().Where(p => p.OptionalAddress == null).ToList();
            Assert.That(addressEqNull.Count, Is.EqualTo(1));
            Assert.That(addressEqNull[0].FullName, Is.EqualTo("Peter Jameson"));
        }

        [Test]
        public void WhereBooleanEqTest()
        {
            var q1 = _realm.All<Person>().Where(p => p.IsInteresting).ToList();

            Assert.That(q1.Count, Is.EqualTo(2));
            Assert.That(q1[0].FullName, Is.EqualTo("John Smith"));
            Assert.That(q1[1].FullName, Is.EqualTo("Peter Jameson"));
        }

        [Test]
        public void WhereBooleanEqTestWithConstant()
        {
            var q1 = _realm.All<Person>().Where(p => p.IsInteresting == true).ToList();
            Assert.That(q1.Count, Is.EqualTo(2));
            Assert.That(q1[0].FullName, Is.EqualTo("John Smith"));
            Assert.That(q1[1].FullName, Is.EqualTo("Peter Jameson"));
        }

        [Test]
        public void WhereBooleanNeqTest()
        {
            var isInterestingFalse = _realm.All<Person>().Where(p => !p.IsInteresting).ToList();
            Assert.That(isInterestingFalse.Count, Is.EqualTo(1));
            Assert.That(isInterestingFalse[0].FullName, Is.EqualTo("John Doe"));
        }

        [Test]
        public void WhereDoubleEqualTest()
        {
            var latitudeEq = _realm.All<Person>().Where(p => p.Latitude == 51.508530).ToList();
            Assert.That(latitudeEq.Count, Is.EqualTo(1));
            Assert.That(latitudeEq[0].FullName, Is.EqualTo("John Smith"));
        }

        [Test]
        public void WhereFloatEqualTest()
        {
            var scoreEq = _realm.All<Person>().Where(p => p.Score == 100.0f).ToList();
            Assert.That(scoreEq.Count, Is.EqualTo(1));
            Assert.That(scoreEq[0].FullName, Is.EqualTo("John Doe"));
        }

        [Test]
        public void WhereFloatNotEqualTest()
        {
            var scoreNeq = _realm.All<Person>().Where(p => p.Score != 100).ToList();
            Assert.That(scoreNeq.Count, Is.EqualTo(2));
            Assert.That(scoreNeq[0].FullName, Is.EqualTo("John Smith"));
            Assert.That(scoreNeq[1].FullName, Is.EqualTo("Peter Jameson"));
        }

        [Test]
        public void WhereFloatGtAndLtThanTest()
        {
            var scoreGtAndLt = _realm.All<Person>().Where(p => p.Score > 40
            && p.Score < 48).ToList();
            Assert.That(scoreGtAndLt.Count, Is.EqualTo(1));
            Assert.That(scoreGtAndLt[0].FullName, Is.EqualTo("Peter Jameson"));
        }

        [Test]
        public void WhereFloatGteAndLteThanTest()
        {
            var scoreGteAndLte = _realm.All<Person>()
                .Where(p => p.Score >= 100.0f && p.Score <= 100.0f).ToList();
            Assert.That(scoreGteAndLte.Count, Is.EqualTo(1));
            Assert.That(scoreGteAndLte[0].FullName, Is.EqualTo("John Doe"));
        }

        [Test]
        public void WhereStringEqualityTest()
        {
            var lastNameEq = _realm.All<Person>().Where(p => p.LastName == "Doe").ToList();
            Assert.That(lastNameEq.Count, Is.EqualTo(1));
            Assert.That(lastNameEq[0].FullName, Is.EqualTo("John Doe"));
        }

        [Test]
        public void WhereNegationTest()
        {
            var scoreNeq = _realm.All<Person>().Where(p => !(p.Score == -0.9907f)).ToList();
            Assert.That(scoreNeq.Count, Is.EqualTo(2));
            Assert.That(scoreNeq[0].FullName, Is.EqualTo("John Doe"));
            Assert.That(scoreNeq[1].FullName, Is.EqualTo("Peter Jameson"));
        }

        [Test]
        public void WhereFloatEqualityReversedOrderTest()
        {
            var q1 = _realm.All<Person>().Where(p => p.Score > 40).ToList();
            var q2 = _realm.All<Person>().Where(p => 40 <= p.Score).ToList();

            Assert.That(q1.Count, Is.EqualTo(2));
            Assert.That(q1[0].FullName, Is.EqualTo("John Doe"));
            Assert.That(q1[1].FullName, Is.EqualTo("Peter Jameson"));

            Assert.That(q2.Count, Is.EqualTo(2));
            Assert.That(q2[0].FullName, Is.EqualTo("John Doe"));
            Assert.That(q2[1].FullName, Is.EqualTo("Peter Jameson"));

        }

        [Test]
        public void WhereBooleanAndTest()
        {
            var scoreEqAndLastNameEq = _realm.All<Person>()
                .Where(p => p.Score == 100 && p.LastName == "Doe").ToList();
            Assert.That(scoreEqAndLastNameEq.Count, Is.EqualTo(1));
            Assert.That(scoreEqAndLastNameEq[0].FullName, Is.EqualTo("John Doe"));
        }

        [Test]
        public void WhereBooleanOrTest()
        {
            var firstNameEqOrScoreEq = _realm.All<Person>()
                .Where(p => p.FirstName == "NonExistant" || p.Score == 42.42f).ToList();
            Assert.That(firstNameEqOrScoreEq.Count, Is.EqualTo(1));
            Assert.That(firstNameEqOrScoreEq[0].FullName, Is.EqualTo("Peter Jameson"));
        }

        [Test]
        public void StringStartsWithTest()
        {
            var firstNameStartsWith = _realm.All<Person>().Where(p => p.FirstName.StartsWith("Pet")).ToList();
            Assert.That(firstNameStartsWith.Count, Is.EqualTo(1));
            Assert.That(firstNameStartsWith[0].FullName, Is.EqualTo("Peter Jameson"));
        }

        [Test]
        public void StringStartsWithCaseTest()
        {
            var firstNameStartsWith = _realm.All<Person>().Where(p => p.FirstName.StartsWith("pet",  StringComparison.OrdinalIgnoreCase)).ToList();
            Assert.That(firstNameStartsWith.Count, Is.EqualTo(1));
            Assert.That(firstNameStartsWith[0].FullName, Is.EqualTo("Peter Jameson"));
        }

        [Test]
        public void StringEndsWithTest()
        {
            var firstNameEndsWith = _realm.All<Person>().Where(p => p.FirstName.EndsWith("ter")).ToList();
            Assert.That(firstNameEndsWith.Count, Is.EqualTo(1));
            Assert.That(firstNameEndsWith[0].FullName, Is.EqualTo("Peter Jameson"));
        }

        [Test]
        public void StringEndsWithCaseTest()
        {
            var q1 = _realm.All<Person>().Where(p => p.FirstName.EndsWith("Ter", StringComparison.OrdinalIgnoreCase)).ToList();
            Assert.That(q1.Count, Is.EqualTo(1));
            Assert.That(q1[0].FullName, Is.EqualTo("Peter Jameson"));

            var q2 = _realm.All<Person>().Where(p => p.FirstName.EndsWith("Ter", StringComparison.Ordinal)).ToList();
            Assert.That(q2.Count, Is.EqualTo(0));
        }

        [Test]
        public void StringContainsTest()
        {
            var firstNameContains = _realm.All<Person>().Where(p => p.FirstName.Contains("ete")).ToList();
            Assert.That(firstNameContains.Count, Is.EqualTo(1));
            Assert.That(firstNameContains[0].FullName, Is.EqualTo("Peter Jameson"));
        }

        [Test]
        public void StringContainsIgnoreCaseTest()
        {
            var q1 = _realm.All<Person>().Where(p => p.FirstName.Contains("Ete", StringComparison.OrdinalIgnoreCase)).ToList();
            Assert.That(q1.Count, Is.EqualTo(1));
            Assert.That(q1[0].FullName, Is.EqualTo("Peter Jameson"));

            var q2 = _realm.All<Person>().Where(p => p.FirstName.Contains("Ete", StringComparison.Ordinal)).ToList();
            Assert.That(q2.Count, Is.EqualTo(0));
        }

        [Test]
        public void StringLikeTest()
        {
            var q1 = _realm.All<Person>().Where(p => p.FirstName.Like("p?t??", false)).ToList();
            Assert.That(q1.Count, Is.EqualTo(1));
            Assert.That(q1[0].FullName, Is.EqualTo("Peter Jameson"));

            var q2 = _realm.All<Person>().Where(p => p.FirstName.Like("p?t??", true)).ToList();
            Assert.That(q2.Count, Is.EqualTo(0));
        }

        [Test]
        public void SearchComparingConstants()
        {
            // Verify that constants in LINQ work
            var equality = _realm.All<Person>().Where(p => p.Salary == Constants.SixtyThousandConstant).ToArray();
            Assert.That(equality.Length, Is.EqualTo(1));
            Assert.That(equality[0].FullName, Is.EqualTo("John Doe"));
        }

        [Test]
        public void SearchComparingStaticFields()
        {
            // Verify that static field in LINQ work
            var equality = _realm.All<Person>().Where(p => p.Salary == Constants.SixtyThousandField).ToArray();
            Assert.That(equality.Length, Is.EqualTo(1));
            Assert.That(equality[0].FullName, Is.EqualTo("John Doe"));
        }

        [Test]
        public void SearchComparingStaticProperties()
        {
            // Verify that static properties in LINQ work
            var equality = _realm.All<Person>().Where(p => p.Salary == Constants.SixtyThousandProperty).ToArray();
            Assert.That(equality.Length, Is.EqualTo(1));
            Assert.That(equality[0].FullName, Is.EqualTo("John Doe"));
        }

        [Test]
        public void SearchComparingInstanceFields()
        {
            var constants = new InstanceConstants();

            // Verify that instance fields in LINQ work
            var equality = _realm.All<Person>().Where(p => p.Salary == constants.SixtyThousandField).ToArray();
            Assert.That(equality.Length, Is.EqualTo(1));
            Assert.That(equality[0].FullName, Is.EqualTo("John Doe"));
        }

        [Test]
        public void SearchComparingInstanceProperties()
        {
            var constants = new InstanceConstants();

            // Verify that instance properties in LINQ work
            var equality = _realm.All<Person>().Where(p => p.Salary == constants.SixtyThousandProperty).ToArray();
            Assert.That(equality.Length, Is.EqualTo(1));
            Assert.That(equality[0].FullName, Is.EqualTo("John Doe"));
        }

        [Test]
        public void NestedPropertyComparison()
        {
            var equality = _realm.All<Person>().Where(p => p.Pet.Name == "Dido").ToArray();
            Assert.That(equality.Length, Is.EqualTo(1));
            Assert.That(equality[0].FullName, Is.EqualTo("John Smith"));
        }

        private static class Constants
        {
            public const long SixtyThousandConstant = 60000;

            public static readonly long SixtyThousandField = 60000;

            public static long SixtyThousandProperty { get; } = 60000;
        }

        private class NestedConstants
        {
            public InstanceConstants InstanceConstants { get; } = new InstanceConstants();
        }

        private class InstanceConstants
        {
            public readonly long SixtyThousandField = 60000;

            public long SixtyThousandProperty { get; } = 60000;
        }

    }
}
