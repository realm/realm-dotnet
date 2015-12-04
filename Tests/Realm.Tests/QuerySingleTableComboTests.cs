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
    public class QuerySingleTableComboTests : QueryTestsBase
    {
        [Test]
        public void SetupCreatedFourRows()
        {
            // Assert
            Assert.AreEqual(4, testEntities.Count());
        }

#if USING_REALM_CORE
        [Test]
        public void AllShouldReturnQueryable()
        {
            // Arrange
            var query = realm.All<TestEntity>();

            // Act
            var res = query.ToList();

            // Assert
            Assert.That(res != null);
        }
#endif

        #region Simple And
        [Test]
        public void TestSimpleAndTwoFieldsMatches()
        {
            // Arrange
            var query = testEntities.Where(te => te.NameStr == "Peter" && te.IntNum == 2);

            // Act
            var res = query.ToList();

            // Assert
            Assert.AreEqual(1, res.Count());
            Assert.AreEqual("Peter", res[0].NameStr);
        }

        [Test]
        public void TestSimpleAndTwoFieldsFails()
        {
            // Arrange
            var query = testEntities.Where(te => te.NameStr == "Peter" && te.IntNum == 1);

            // Act
            var res = query.ToList();

            // Assert
            Assert.AreEqual(0, res.Count());
        }

        #endregion  // Simple And


        #region Simple Or
        [Test]
        public void TestSimpleOrTwoFieldsMatchesOne()
        {
            // Arrange
            var query = testEntities.Where(te => te.NameStr == "Peter" || te.IntNum == 2);

            // Act
            var res = query.ToList();

            // Assert
            Assert.AreEqual(1, res.Count());
            Assert.AreEqual("Peter", res[0].NameStr);
        }

        [Test]
        public void TestSimpleOrTwoFieldsMatchesTwo()
        {
            // Arrange
            var query = testEntities.Where(te => te.NameStr == "Peter" || te.IntNum == 4);

            // Act
            var res = query.ToList();

            // Assert
            Assert.AreEqual(2, res.Count());
            Assert.AreEqual("Peter", res[0].NameStr);
            Assert.AreEqual("Xanh Li", res[1].NameStr);
        }

        [Test]
        public void TestSimpleOrTwoFieldsFails()
        {
            // Arrange
            var query = testEntities.Where(te => te.NameStr == "Forgotten" || te.IntNum == 99);

            // Act
            var res = query.ToList();

            // Assert
            Assert.AreEqual(0, res.Count());
        }

        #endregion  // Simple Or

        [Test]
        public void TestPrecedence()
        {
            // Arrange
            var query = testEntities.Where(te => te.IntNum > 3 || te.NameStr == "Peter" && te.IntNum > 0 );

            // Act
            var res = query.ToList();

            // Assert
            Assert.AreEqual(2, res.Count());
            Assert.AreEqual("Peter", res[0].NameStr);
            Assert.AreEqual("Xanh Li", res[1].NameStr);
        }

        [Test]
        public void TestNested()
        {
            // Arrange
            var query = testEntities.Where(te => te.NameStr == "Peter" && (te.IntNum == 2 || te.IntNum > 3));

            // Act
            var res = query.ToList();

            // Assert
            Assert.AreEqual(1, res.Count());
            Assert.AreEqual("Peter", res[0].NameStr);
        }
    }
}