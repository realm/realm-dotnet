using NUnit.Framework;
using System.Linq;

namespace Tests
{
    [TestFixture]
    public class MockQuerySingleTableComboTests : MockQueryTestsBase
    {
        [Test]
        public void TestSimpleAnd()
        {
            // Arrange
            var query = testEntities.Where(te => te.NameStr == "Peter" && te.IntNum == 2);

            // Act
            var res = query.ToList();

            // Assert
            var queryCalls = providerLog.Where((msg) => msg.StartsWith("AddQuery")).ToArray();
            string[] expected = {
                "AddQueryGroupBegin",
                    "AddQueryEqual(col=NameStr, val=Peter)",
                "AddQueryAnd",
                    "AddQueryEqual(col=IntNum, val=2)",
                "AddQueryGroupEnd",
            };
            Assert.AreEqual(expected, queryCalls);
        }

        [Test]
        public void TestThreeAnds()
        {
            // Arrange
            var query = testEntities.Where(te => te.NameStr == "Peter" && te.IntNum >= 2 && te.IntNum <= 99);

            // Act
            var res = query.ToList();

            // Assert
            var queryCalls = providerLog.Where((msg) => msg.StartsWith("AddQuery")).ToArray();
            string[] expected = {
                "AddQueryGroupBegin",
                    "AddQueryGroupBegin",
                        "AddQueryEqual(col=NameStr, val=Peter)",
                    "AddQueryAnd",
                        "AddQueryGreaterThanOrEqual(col=IntNum, val=2)",
                    "AddQueryGroupEnd",
                "AddQueryAnd",
                    "AddQueryLessThanOrEqual(col=IntNum, val=99)",
                "AddQueryGroupEnd",
            };
            Assert.AreEqual(expected, queryCalls);
        }

        [Test]
        public void TestSimpleOr()
        {
            // Arrange
            var query = testEntities.Where(te => te.NameStr == "Peter" || te.IntNum == 2);

            // Act
            var res = query.ToList();

            // Assert
            var queryCalls = providerLog.Where((msg) => msg.StartsWith("AddQuery")).ToArray();
            string[] expected = {
                "AddQueryGroupBegin",
                    "AddQueryEqual(col=NameStr, val=Peter)",
                "AddQueryOr",
                    "AddQueryEqual(col=IntNum, val=2)",
                "AddQueryGroupEnd",
            };
            Assert.AreEqual(expected, queryCalls);
        }

        [Test]
        public void TestThreeOrs()
        {
            // Arrange
            var query = testEntities.Where(te => te.NameStr == "Peter" || te.IntNum == 2 || te.IntNum == 4);

            // Act
            var res = query.ToList();

            // Assert
            var queryCalls = providerLog.Where((msg) => msg.StartsWith("AddQuery")).ToArray();
            string[] expected = {
                "AddQueryGroupBegin",
                    "AddQueryGroupBegin",
                        "AddQueryEqual(col=NameStr, val=Peter)",
                    "AddQueryOr",
                        "AddQueryEqual(col=IntNum, val=2)",
                    "AddQueryGroupEnd",
                "AddQueryOr",
                    "AddQueryEqual(col=IntNum, val=4)",
                "AddQueryGroupEnd",
            };
            Assert.AreEqual(expected, queryCalls);
        }

        // should be identical expression to TestNested
        [Test]
        public void TestPrecedence()
        {
            // Arrange
            var query = testEntities.Where(te => te.NameStr == "Peter" && te.IntNum == 2 || te.IntNum > 3);

            // Act
            var res = query.ToList();

            // Assert
            var queryCalls = providerLog.Where((msg) => msg.StartsWith("AddQuery")).ToArray();
            string[] expected = {
                "AddQueryGroupBegin",
                    "AddQueryGroupBegin",
                        "AddQueryEqual(col=NameStr, val=Peter)",
                    "AddQueryAnd",
                        "AddQueryEqual(col=IntNum, val=2)",
                    "AddQueryGroupEnd",
                "AddQueryOr",
                    "AddQueryGreaterThan(col=IntNum, val=3)",
                "AddQueryGroupEnd",
            };
            Assert.AreEqual(expected, queryCalls);
        }

        // adding arbirary parens to make it clear but this is actually how precedence would work
        [Test]
        public void TestNested()
        {
            // Arrange
            var query = testEntities.Where(te => (te.NameStr == "Peter" && te.IntNum == 2) || te.IntNum > 3);

            // Act
            var res = query.ToList();

            // Assert
            var queryCalls = providerLog.Where((msg) => msg.StartsWith("AddQuery")).ToArray();
            string[] expected = {
                "AddQueryGroupBegin",
                    "AddQueryGroupBegin",
                        "AddQueryEqual(col=NameStr, val=Peter)",
                    "AddQueryAnd",
                        "AddQueryEqual(col=IntNum, val=2)",
                    "AddQueryGroupEnd",
                "AddQueryOr",
                    "AddQueryGreaterThan(col=IntNum, val=3)",
                "AddQueryGroupEnd",
            };
            Assert.AreEqual(expected, queryCalls);
        }


        [Test]
        public void TestPrecedenceAtTail()
        {
            // Arrange
            var query = testEntities.Where(te => te.IntNum == 2 || te.IntNum > 3 && te.NameStr == "Peter");

            // Act
            var res = query.ToList();

            // Assert
            var queryCalls = providerLog.Where((msg) => msg.StartsWith("AddQuery")).ToArray();
            string[] expected = {
                "AddQueryGroupBegin",
                    "AddQueryEqual(col=IntNum, val=2)",
                "AddQueryOr",
                    "AddQueryGroupBegin",
                        "AddQueryGreaterThan(col=IntNum, val=3)",
                    "AddQueryAnd",
                        "AddQueryEqual(col=NameStr, val=Peter)",
                    "AddQueryGroupEnd",
                "AddQueryGroupEnd"
            };
            Assert.AreEqual(expected, queryCalls);
        }


        [Test]
        public void TestBracketedPrecedence()
        {
            // Arrange
            var query = testEntities.Where(te => (te.IntNum == 2 || te.IntNum > 3) && te.NameStr == "Peter");

            // Act
            var res = query.ToList();

            // Assert
            var queryCalls = providerLog.Where((msg) => msg.StartsWith("AddQuery")).ToArray();
            string[] expected = {
                "AddQueryGroupBegin",
                    "AddQueryGroupBegin",
                        "AddQueryEqual(col=IntNum, val=2)",
                    "AddQueryOr",
                        "AddQueryGreaterThan(col=IntNum, val=3)",
                    "AddQueryGroupEnd",
                "AddQueryAnd",
                    "AddQueryEqual(col=NameStr, val=Peter)",
                "AddQueryGroupEnd",
            };
            Assert.AreEqual(expected, queryCalls);
        }

    }
} 