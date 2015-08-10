using NUnit.Framework;
using RealmNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InteropShared;

namespace Tests
{
    [TestFixture]
    public class MockQuerySingleTableTests : MockQueryTestsBase
    {
        [Test]
        public void SetupCreatedFourRows()
        {
            // Assert
            Assert.AreEqual(4, providerLog.Count((msg) => msg.StartsWith("AddEmptyRow"))); 
        }

        [Test]
        public void AllShouldReturnQueryable()
        {
            // Arrange
            var query = realm.All<TestEntity>();

            // Act
            var res = query.ToList();

            // Assert
            Assert.That(query !=null && query is IQueryable);  // Resharper says latter is always true by compilation but worth making the point
            Assert.That(res != null);
            Assert.AreEqual(1, providerLog.Count((msg) => msg.StartsWith("ExecuteQuery")));
        }

        [Test]
        public void TestWhereQueryWithEqualToString()
        {
            // Arrange
            var query = realm.All<TestEntity>().Where(te => te.NameStr == "Peter");

            // Act
            var res = query.ToList();

            // Assert
            Assert.That(res != null);
            Assert.AreEqual(1, providerLog.Count((msg) => msg.StartsWith("QueryEqual")));
        }
    }
} 