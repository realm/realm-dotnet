/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using NUnit.Framework;
using Realm;
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
    public class QuerySingleTableTests : QueryTestsBase
    {
        [Test]
        public void SetupCreatedFourRows()
        {
            // Assert
            Assert.AreEqual(4, testEntities.Count()); 
        }

#if USING_REALM_BACKEND        
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
        }
#endif

        #region String Comparisons
        [Test]
        public void TestWhereQueryWithEqualToString()
        {
            // Arrange
            var query = testEntities.Where(te => te.NameStr == "Peter");

            // Act
            var res = query.ToList();

            // Assert
            Assert.AreEqual(1, res.Count());
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
            Assert.AreEqual(3, res.Count());
        }
 #endregion  // String Comparisons


#region Bool Comparisons
        [Test]
        public void TestWhereQueryWithEqualToBool()
        {
            // Arrange
            var query = testEntities.Where(te => te.IsCool);

            // Act
            var res = query.ToList();

            // Assert
            Assert.AreEqual(1, res.Count());
        }

        [Test]
        public void TestWhereQueryWithNotEqualToBool()
        {
            // Arrange
            var query = testEntities.Where(te => te.IsCool != true);

            // Act
            var res = query.ToList();

            // Assert
            Assert.AreEqual(3, res.Count());
        }
#endregion  // Bool Comparisons


#region Int Comparisons
        [Test]
        public void TestWhereQueryWithEqualToInt()
        {
            // Arrange
            var query = testEntities.Where(te => te.IntNum == 1);

            // Act
            var res = query.ToList();

            // Assert
            Assert.AreEqual(1, res.Count());
        }

        [Test]
        public void TestWhereQueryWithNotEqualToInt()
        {
            // Arrange
            var query = testEntities.Where(te => te.IntNum != 1);

            // Act
            var res = query.ToList();

            // Assert
            Assert.AreEqual(3, res.Count());
        }

        [Test]
        public void TestWhereQueryWithLessThanInt()
        {
            // Arrange
            var query = testEntities.Where(te => te.IntNum < 2);

            // Act
            var res = query.ToList();

            // Assert
            Assert.AreEqual(1, res.Count());
        }

        [Test]
        public void TestWhereQueryWithLessThanOrEqualInt()
        {
            // Arrange
            var query = testEntities.Where(te => te.IntNum <= 2);

            // Act
            var res = query.ToList();

            // Assert
            Assert.AreEqual(2, res.Count());
        }

        [Test]
        public void TestWhereQueryWithGreaterThanInt()
        {
            // Arrange
            var query = testEntities.Where(te => te.IntNum > 2);

            // Act
            var res = query.ToList();

            // Assert
            Assert.AreEqual(2, res.Count());
        }

        [Test]
        public void TestWhereQueryWithGreaterThanOrEqualInt()
        {
            // Arrange
            var query = testEntities.Where(te => te.IntNum >= 2);

            // Act
            var res = query.ToList();

            // Assert
            Assert.AreEqual(3, res.Count());
        }
#endregion  // Int Comparisons


        /*
        Not until have setting float and double issue #67, sep issue #68 for implementing these
#region Float Comparisons
        [Test]
        public void TestWhereQueryWithEqualToFloat()
        {
            // Arrange
            var query = testEntities.Where(te => te.FloatNum == 0.99);

            // Act
            var res = query.ToList();

            // Assert
            Assert.AreEqual(1, res.Count());
        }

        [Test]
        public void TestWhereQueryWithNotEqualToFloat()
        {
            // Arrange
            var query = testEntities.Where(te => te.FloatNum != 0.0001);

            // Act
            var res = query.ToList();

            // Assert
            Assert.AreEqual(4, res.Count());
        }

        [Test]
        public void TestWhereQueryWithLessThanFloat()
        {
            // Arrange
            var query = testEntities.Where(te => te.FloatNum < 4.2);

            // Act
            var res = query.ToList();

            // Assert
            Assert.AreEqual(3, res.Count());
        }

        [Test]
        public void TestWhereQueryWithLessThanOrEqualFloat()
        {
            // Arrange
            var query = testEntities.Where(te => te.FloatNum <= 4.2);

            // Act
            var res = query.ToList();

            // Assert
            Assert.AreEqual(3, res.Count());
        }

        [Test]
        public void TestWhereQueryWithGreaterThanFloat()
        {
            // Arrange
            var query = testEntities.Where(te => te.FloatNum > 42.999);

            // Act
            var res = query.ToList();

            // Assert
            Assert.AreEqual(1, res.Count());
        }

        [Test]
        public void TestWhereQueryWithGreaterThanOrEqualFloat()
        {
            // Arrange
            var query = testEntities.Where(te => te.FloatNum >= 1.0e-5);

            // Act
            var res = query.ToList();

            // Assert
            Assert.AreEqual(3, res.Count());
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
            Assert.AreEqual(1, res.Count());
        }

        [Test]
        public void TestWhereQueryWithNotEqualToDouble()
        {
            // Arrange
            var query = testEntities.Where(te => te.DoubleNum != 0.0001);

            // Act
            var res = query.ToList();

            // Assert
            Assert.AreEqual(4, res.Count());
        }

        [Test]
        public void TestWhereQueryWithLessThanDouble()
        {
            // Arrange
            var query = testEntities.Where(te => te.DoubleNum < 4.2);

            // Act
            var res = query.ToList();

            // Assert
            Assert.AreEqual(3, res.Count());
        }

        [Test]
        public void TestWhereQueryWithLessThanOrEqualDouble()
        {
            // Arrange
            var query = testEntities.Where(te => te.DoubleNum <= 4.2);

            // Act
            var res = query.ToList();

            // Assert
            Assert.AreEqual(3, res.Count());
        }

        [Test]
        public void TestWhereQueryWithGreaterThanDouble()
        {
            // Arrange
            var query = testEntities.Where(te => te.DoubleNum > 42.999);

            // Act
            var res = query.ToList();

            // Assert
            Assert.AreEqual(1, res.Count());
        }

        [Test]
        public void TestWhereQueryWithGreaterThanOrEqualDouble()
        {
            // Arrange
            var query = testEntities.Where(te => te.DoubleNum >= 1.0e-5);

            // Act
            var res = query.ToList();

            // Assert
            Assert.AreEqual(3, res.Count());
        }
#endregion  // Double Comparisons
    */

#region DateTime Comparisons
        // TODO full range
#endregion // DateTime Comparisons

#region Binary Comparisons
        // TODO == and !=
#endregion // Binary Comparisons
    }
} 