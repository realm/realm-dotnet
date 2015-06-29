using NUnit.Framework;
using RealmNet;
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

        private CoreProviderStub _coreProviderStub;
        private Realm realm;

        [SetUp]
        public void Setup()
        {
            _coreProviderStub = new CoreProviderStub();
            realm = new Realm(_coreProviderStub);
        }

        private void PrepareForQueries()
        {
            _coreProviderStub.AddBulk("TestEntity", new dynamic[]
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
            Assert.That(_coreProviderStub.Queries.Count, Is.EqualTo(1));
            Assert.That(_coreProviderStub.Queries[0].TableName, Is.EqualTo("TestEntity"));
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
            Assert.That(_coreProviderStub.Queries.Count, Is.EqualTo(1));
            Assert.That(_coreProviderStub.Queries[0].TableName, Is.EqualTo("TestEntity"));
            Assert.That(_coreProviderStub.Queries[0].Sequence.Count, Is.EqualTo(1));
            Assert.That(_coreProviderStub.Queries[0].Sequence[0].Name, Is.EqualTo("Equal"));
            Assert.That(_coreProviderStub.Queries[0].Sequence[0].Field, Is.EqualTo("Str"));
            Assert.That(_coreProviderStub.Queries[0].Sequence[0].Value, Is.EqualTo("Peter"));
        }
    }
} 