using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RealmNet;
using InteropShared;

// TODO abstract out the CoreProvider
namespace Tests
{
    [TestFixture]
    public class CoreProviderTests
    {
        [Test]
        public void ShouldAddTable()
        {
            // Arrange
            var coreProvider = new MockCoreProvider();

            // Act
            coreProvider.AddTable(null, "T1");

            // Assert
            Assert.That(coreProvider.HasTable(null, "T1"));
        }

        [Test]
        public void AddColumnShouldNotThrow()
        {
            // Arrange
            var coreProvider = new MockCoreProvider();
            coreProvider.AddTable(null, "T1");

            // Act
            coreProvider.AddColumnToTable("T1", "C1", typeof (string));

            // Assert
            Assert.Pass();
        }

        [Test]
        public void InsertRowShouldNotThrow()
        {
            // Arrange
            var coreProvider = new MockCoreProvider();
            coreProvider.AddTable(null, "T1");

            // Act
            coreProvider.AddEmptyRow("T1");

            // Assert
            Assert.Pass();
        }

        [Test]
        public void SetAndGetValue()
        {
            // Arrange
            var coreProvider = new MockCoreProvider();
            coreProvider.AddTable(null,  "T1");
            coreProvider.AddColumnToTable("T1", "C1", typeof (string));
            coreProvider.AddEmptyRow("T1");
            
            // Act
            coreProvider.SetValue<string>("T1", "C1", 0, "actual value");
            var actual = coreProvider.GetValue<string>("T1", "C1", 0);

            // Assert
            Assert.That(actual, Is.EqualTo("actual value"));
        }

        [Test]
        public void CreateQueryTest()
        {
            // Arrange
            var coreProvider = new MockCoreProvider();
            coreProvider.AddTable(null, "T1");

            // Act
            var query = coreProvider.CreateQuery("T1");

            // Assert
            Assert.That(query, Is.Not.Null);
        }

        [Test]
        public void ExecuteQueryTest()
        {
            // Arrange
            var coreProvider = new MockCoreProvider();
            coreProvider.AddTable(null, "T1");
            var query = coreProvider.CreateQuery("T1");

            // Act
            var enumerable = coreProvider.ExecuteQuery(query, typeof (string));

            // Assert
            Assert.Pass();
        }


    }
}
