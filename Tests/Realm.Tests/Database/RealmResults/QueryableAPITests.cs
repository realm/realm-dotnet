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
using NUnit.Framework;
using Realms;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class QueryableAPITests : PeopleTestsBase
    {
        [Test]
        public void ProviderCreateQuery_WhenGeneric_ReturnsResults()
        {
            MakeThreePeople();

            var query = _realm.All<Person>();
            Expression<Func<Person, bool>> whereClause = p => p.IsInteresting;

            var filterExpression = Expression.Call(typeof(Queryable), "Where", new Type[] { query.ElementType }, query.Expression, whereClause);
            var result = query.Provider.CreateQuery<Person>(filterExpression).ToArray();

            Assert.That(result.Length, Is.EqualTo(2));
            Assert.That(result.All(r => r.IsInteresting), "All items in the result are interesting");
        }

        [Test]
        public void ProviderCreateQuery_WhenNonGeneric_ReturnsResults()
        {
            MakeThreePeople();

            var query = _realm.All<Person>();
            Expression<Func<Person, bool>> whereClause = p => p.IsInteresting;

            var filterExpression = Expression.Call(typeof(Queryable), "Where", new Type[] { query.ElementType }, query.Expression, whereClause);
            var result = query.Provider.CreateQuery(filterExpression);

            var count = 0;
            foreach (Person person in result)
            {
                Assert.That(person.IsInteresting, "Person is interesting");
                count++;
            }

            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        public void ProviderExecute_WhenGeneric_ReturnsResult()
        {
            MakeThreePeople();

            var query = _realm.All<Person>();
            Expression<Func<Person, bool>> whereClause = p => p.IsInteresting;

            var filterExpression = Expression.Call(typeof(Queryable), "FirstOrDefault", new Type[] { query.ElementType }, query.Expression, whereClause);
            var result = query.Provider.Execute<Person>(filterExpression);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsInteresting);
        }

        [Test]
        public void ProviderExecute_WhenNonGeneric_ReturnsResult()
        {
            MakeThreePeople();

            var query = _realm.All<Person>();
            Expression<Func<Person, bool>> whereClause = p => p.IsInteresting;

            var filterExpression = Expression.Call(typeof(Queryable), "FirstOrDefault", new Type[] { query.ElementType }, query.Expression, whereClause);
            var result = query.Provider.Execute(filterExpression);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<Person>());
            Assert.That(((Person)result).IsInteresting);
        }

        [Test]
        public void ProviderExecute_WhenGenericAndNoMatch_ReturnsNull()
        {
            MakeThreePeople();

            var query = _realm.All<Person>();
            Expression<Func<Person, bool>> whereClause = p => p.FirstName == "non-existent";

            var filterExpression = Expression.Call(typeof(Queryable), "FirstOrDefault", new Type[] { query.ElementType }, query.Expression, whereClause);
            var result = query.Provider.Execute<Person>(filterExpression);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void ProviderExecute_WhenNonGenericAndNoMatch_ReturnsNull()
        {
            MakeThreePeople();

            var query = _realm.All<Person>();
            Expression<Func<Person, bool>> whereClause = p => p.FirstName == "non-existent";

            var filterExpression = Expression.Call(typeof(Queryable), "FirstOrDefault", new Type[] { query.ElementType }, query.Expression, whereClause);
            var result = query.Provider.Execute(filterExpression);

            Assert.That(result, Is.Null);
        }
    }
}
