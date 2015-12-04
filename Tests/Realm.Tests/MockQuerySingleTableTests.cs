using NUnit.Framework;
using Realms;
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
        public void AllShouldReturnQueryable()
        {
            // Arrange
            var query = realm.All<TestEntity>();

            // Act
            var res = query.ToList();

            // Assert
            Assert.That(query !=null);
            Assert.That(res != null);
            Assert.AreEqual(1, providerLog.Count((msg) => msg.StartsWith("ExecuteQuery")));
        }

        #region String Comparisons
        [Test]
        public void TestWhereQueryWithEqualToString()
        {
            // Arrange
            var query = testEntities.Where(te => te.NameStr == "Peter");

            // Act
            var res = query.ToList();

            // Assert
            Assert.That(res != null);
            Assert.AreEqual(1, providerLog.Count((msg) => msg.StartsWith("AddQueryEqual")));
        }

        [Test]
        public void TestWhereQueryWithNotEqualToString()
        {
            // Arrange
            var query = testEntities.Where(te => te.NameStr != "Peter");

            // Act
            var res = query.ToList();

            // Assert
            Assert.That(res != null);
            Assert.AreEqual(1, providerLog.Count((msg) => msg.StartsWith("AddQueryNotEqual")));
        }
        #endregion


        #region Bool Comparisons
        [Test]
        public void TestWhereQueryWithEqualToBool()
        {
            // Arrange
            var query = testEntities.Where(te => te.IsCool == false);

            // Act
            var res = query.ToList();

            // Assert
            Assert.That(res != null);
            Assert.AreEqual(1, providerLog.Count((msg) => msg.StartsWith("AddQueryEqual")));
        }

        [Test]
        public void TestWhereQueryWithNotEqualToBool()
        {
            // Arrange
            var query = testEntities.Where(te => te.IsCool != false);

            // Act
            var res = query.ToList();

            // Assert
            Assert.That(res != null);
            Assert.AreEqual(1, providerLog.Count((msg) => msg.StartsWith("AddQueryNotEqual")));
        }
        #endregion


        #region Int Comparisons
        [Test]
        public void TestWhereQueryWithEqualToInt()
        {
            // Arrange
            var query = testEntities.Where(te => te.IntNum == 42);

            // Act
            var res = query.ToList();

            // Assert
            Assert.That(res != null);
            Assert.AreEqual(1, providerLog.Count((msg) => msg.StartsWith("AddQueryEqual")));
        }

        [Test]
        public void TestWhereQueryWithNotEqualToInt()
        {
            // Arrange
            var query = testEntities.Where(te => te.IntNum != 42);

            // Act
            var res = query.ToList();

            // Assert
            Assert.That(res != null);
            Assert.AreEqual(1, providerLog.Count((msg) => msg.StartsWith("AddQueryNotEqual")));
        }

        [Test]
        public void TestWhereQueryWithLessThanInt()
        {
            // Arrange
            var query = testEntities.Where(te => te.IntNum < 42);

            // Act
            var res = query.ToList();

            // Assert
            Assert.That(res != null);
            Assert.AreEqual(1, providerLog.Count((msg) => msg.StartsWith("AddQueryLessThan")));
        }

        [Test]
        public void TestWhereQueryWithLessThanOrEqualInt()
        {
            // Arrange
            var query = testEntities.Where(te => te.IntNum <= 42);

            // Act
            var res = query.ToList();

            // Assert
            Assert.That(res != null);
            Assert.AreEqual(1, providerLog.Count((msg) => msg.StartsWith("AddQueryLessThanOrEqual")));
        }

        [Test]
        public void TestWhereQueryWithGreaterThanInt()
        {
            // Arrange
            var query = testEntities.Where(te => te.IntNum > 42);

            // Act
            var res = query.ToList();

            // Assert
            Assert.That(res != null);
            Assert.AreEqual(1, providerLog.Count((msg) => msg.StartsWith("AddQueryGreaterThan")));
        }

        [Test]
        public void TestWhereQueryWithGreaterThanOrEqualInt()
        {
            // Arrange
            var query = testEntities.Where(te => te.IntNum >= 42);

            // Act
            var res = query.ToList();

            // Assert
            Assert.That(res != null);
            Assert.AreEqual(1, providerLog.Count((msg) => msg.StartsWith("AddQueryGreaterThanOrEqual")));
        }
        #endregion  // Int Comparisons


        #region Float Comparisons
        [Test]
        public void TestWhereQueryWithEqualToFloat()
        {
            // Arrange
            var query = testEntities.Where(te => te.FloatNum == 0.99);

            // Act
            var res = query.ToList();

            // Assert
            Assert.That(res != null);
            Assert.AreEqual(1, providerLog.Count((msg) => msg.StartsWith("AddQueryEqual")));
        }

        [Test]
        public void TestWhereQueryWithNotEqualToFloat()
        {
            // Arrange
            var query = testEntities.Where(te => te.FloatNum != 0.0001);

            // Act
            var res = query.ToList();

            // Assert
            Assert.That(res != null);
            Assert.AreEqual(1, providerLog.Count((msg) => msg.StartsWith("AddQueryNotEqual")));
        }

        [Test]
        public void TestWhereQueryWithLessThanFloat()
        {
            // Arrange
            var query = testEntities.Where(te => te.FloatNum < 4.2);

            // Act
            var res = query.ToList();

            // Assert
            Assert.That(res != null);
            Assert.AreEqual(1, providerLog.Count((msg) => msg.StartsWith("AddQueryLessThan")));
        }

        [Test]
        public void TestWhereQueryWithLessThanOrEqualFloat()
        {
            // Arrange
            var query = testEntities.Where(te => te.FloatNum <= 4.2);

            // Act
            var res = query.ToList();

            // Assert
            Assert.That(res != null);
            Assert.AreEqual(1, providerLog.Count((msg) => msg.StartsWith("AddQueryLessThanOrEqual")));
        }

        [Test]
        public void TestWhereQueryWithGreaterThanFloat()
        {
            // Arrange
            var query = testEntities.Where(te => te.FloatNum > 42.999);

            // Act
            var res = query.ToList();

            // Assert
            Assert.That(res != null);
            Assert.AreEqual(1, providerLog.Count((msg) => msg.StartsWith("AddQueryGreaterThan")));
        }

        [Test]
        public void TestWhereQueryWithGreaterThanOrEqualFloat()
        {
            // Arrange
            var query = testEntities.Where(te => te.FloatNum >= 1.0e-5);

            // Act
            var res = query.ToList();

            // Assert
            Assert.That(res != null);
            Assert.AreEqual(1, providerLog.Count((msg) => msg.StartsWith("AddQueryGreaterThanOrEqual")));
        }
        #endregion  // Float Comparisons


        #region Double Comparisons
        [Test]
        public void TestWhereQueryWithEqualToDouble()
        {
            // Arrange
            var query = testEntities.Where(te => te.DoubleNum == 0.99);

            // Act
            var res = query.ToList();

            // Assert
            Assert.That(res != null);
            Assert.AreEqual(1, providerLog.Count((msg) => msg.StartsWith("AddQueryEqual")));
        }

        [Test]
        public void TestWhereQueryWithNotEqualToDouble()
        {
            // Arrange
            var query = testEntities.Where(te => te.DoubleNum != 0.0001);

            // Act
            var res = query.ToList();

            // Assert
            Assert.That(res != null);
            Assert.AreEqual(1, providerLog.Count((msg) => msg.StartsWith("AddQueryNotEqual")));
        }

        [Test]
        public void TestWhereQueryWithLessThanDouble()
        {
            // Arrange
            var query = testEntities.Where(te => te.DoubleNum < 4.2);

            // Act
            var res = query.ToList();

            // Assert
            Assert.That(res != null);
            Assert.AreEqual(1, providerLog.Count((msg) => msg.StartsWith("AddQueryLessThan")));
        }

        [Test]
        public void TestWhereQueryWithLessThanOrEqualDouble()
        {
            // Arrange
            var query = testEntities.Where(te => te.DoubleNum <= 4.2);

            // Act
            var res = query.ToList();

            // Assert
            Assert.That(res != null);
            Assert.AreEqual(1, providerLog.Count((msg) => msg.StartsWith("AddQueryLessThanOrEqual")));
        }

        [Test]
        public void TestWhereQueryWithGreaterThanDouble()
        {
            // Arrange
            var query = testEntities.Where(te => te.DoubleNum > 42.999);

            // Act
            var res = query.ToList();

            // Assert
            Assert.That(res != null);
            Assert.AreEqual(1, providerLog.Count((msg) => msg.StartsWith("AddQueryGreaterThan")));
        }

        [Test]
        public void TestWhereQueryWithGreaterThanOrEqualDouble()
        {
            // Arrange
            var query = testEntities.Where(te => te.DoubleNum >= 1.0e-5);

            // Act
            var res = query.ToList();

            // Assert
            Assert.That(res != null);
            Assert.AreEqual(1, providerLog.Count((msg) => msg.StartsWith("AddQueryGreaterThanOrEqual")));
        }
        #endregion  // Double Comparisons


    }
} 