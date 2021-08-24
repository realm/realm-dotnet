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
    public class AAAATests : PeopleTestsBase
    {
        protected override void CustomSetUp()
        {
            base.CustomSetUp();
            MakeThreePeople();
        }

        private static string GetDebugView(Expression exp)
        {
            if (exp == null)
            {
                return null;
            }

            var propertyInfo = typeof(Expression).GetProperty("DebugView", BindingFlags.Instance | BindingFlags.NonPublic);
            return propertyInfo.GetValue(exp) as string;
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

        // End of new tests

        //[Test]
        //public void Ordering()
        //{
        //    var query = _realm.All<Person>()
        //        .Where(p => p.FirstName.StartsWith("abc") && p.IsInteresting)
        //        .Where(p => p.Birthday < System.DateTimeOffset.UtcNow)
        //        .OrderBy(p => p.FirstName)
        //        .ThenByDescending(p => p.Birthday);

        //    _ = query.ToArray();
        //}

        //[Test]
        //public void DictTest()
        //{
        //    var query = _realm.All<CollectionsObject>().Where(a => a.BooleanDict.Any(kvp => kvp.Key.StartsWith("abc")));
        //    var debugView = GetDebugView(query.Expression);
        //    Console.WriteLine(debugView);
        //    _ = query.ToArray();
        //}

        //[Test]
        //public void ListTest()
        //{
        //    var query = _realm.All<CollectionsObject>().Where(a => a.BooleanList.Count > 5);
        //    _ = query.ToArray();
        //}

        //[Test]
        //public void Iteration()
        //{
        //    _realm.Write(() =>
        //    {
        //        for (var i = 0; i < 10; i++)
        //        {
        //            _realm.Add(new IntPropertyObject
        //            {
        //                Int = i
        //            });
        //        }
        //    });

        //    var query = _realm.All<IntPropertyObject>().Where(a => a.Int > 5);
        //    foreach (var item in query)
        //    {
        //        System.Console.WriteLine(item.Int);
        //    }

        //    for (var i = 0; i < query.Count(); i++)
        //    {
        //        System.Console.WriteLine(query.ElementAt(i).Int);
        //    }
        //}
    }
}
