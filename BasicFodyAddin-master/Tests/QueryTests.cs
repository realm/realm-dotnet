using NUnit.Framework;
using RealmIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            stubCoreProvider.AddBulk("TestEntity", new[]
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

            // Act
            var testEntities = realm.All<TestEntity>().Where(t => t.Str == "John").ToList();

            // Assert
            Assert.That(testEntities.Count(), Is.EqualTo(2));
            Assert.That(testEntities.First().Str, Is.EqualTo("John"));
        }
    }
} 