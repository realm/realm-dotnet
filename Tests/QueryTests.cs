using NUnit.Framework;
using RealmIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.TestHelpers;

namespace Tests
{
    [TestFixture]
    public class RealmObjectTests
    {
        private class TestEntity
        {
            public string Str { get; set; }
            public int Number { get; set; }
        }

        private StubCoreProvider stubCoreProvider;
        private Realm realm;

        [SetUp]
        public void Setup()
        {
            stubCoreProvider = new StubCoreProvider();
            realm = new Realm(stubCoreProvider);
        }

        private void PrepareForQueries()
        {
            stubCoreProvider.AddBulk("TestEntity", new dynamic[]
            {
                new { Str = "John", Number = 1 },
                new { Str = "Peter", Number = 2 }
            });
        }

        [Test]
        public void AllShouldReturnQueryable()
        {
            // Arrange
            PrepareForQueries();
            var query = realm.All<TestEntity>();

            // Act
            query.ToList();

            // Assert
            Assert.That(stubCoreProvider.Queries.Count, Is.EqualTo(1));
            Assert.That(stubCoreProvider.Queries[0].TableName, Is.EqualTo("TestEntity"));
        }

        [Test]
        public void TestWhereQueryWithEqualToCondition()
        {
            // Arrange
            PrepareForQueries();
            var query = realm.All<TestEntity>().Where(te => te.Str == "Peter");

            // Act
            query.ToList();

            // Assert
            Assert.That(stubCoreProvider.Queries.Count, Is.EqualTo(1));
            Assert.That(stubCoreProvider.Queries[0].TableName, Is.EqualTo("TestEntity"));
            Assert.That(stubCoreProvider.Queries[0].Sequence.Count, Is.EqualTo(1));
            Assert.That(stubCoreProvider.Queries[0].Sequence[0].Name, Is.EqualTo("Equal"));
            Assert.That(stubCoreProvider.Queries[0].Sequence[0].Field, Is.EqualTo("Str"));
            Assert.That(stubCoreProvider.Queries[0].Sequence[0].Value, Is.EqualTo("Peter"));
        }
    }
} 