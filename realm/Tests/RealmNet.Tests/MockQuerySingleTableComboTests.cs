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
                "AddQueryGroupEnd",
            "AddQueryAnd",
                "AddQueryGroupBegin",
                    "AddQueryEqual(col=IntNum, val=2)",
                "AddQueryGroupEnd"
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
                "AddQueryGroupEnd",
            "AddQueryOr",
                "AddQueryGroupBegin",
                    "AddQueryEqual(col=IntNum, val=2)",
                "AddQueryGroupEnd"
            };
            Assert.AreEqual(expected, queryCalls);
        }

        [Test]
        public void TestNested()
        {
            // Arrange
            var query = testEntities.Where(te =>(te.NameStr == "Peter" && te.IntNum == 2) || te.IntNum > 3);

            // Act
            var res = query.ToList();

            // Assert
            var queryCalls = providerLog.Where((msg) => msg.StartsWith("AddQuery")).ToArray();
            string[] expected = {
                "AddQueryGroupBegin",
                    "AddQueryGroupBegin",
                        "AddQueryEqual(col=NameStr, val=Peter)",
                    "AddQueryGroupEnd",
                "AddQueryAnd",
                    "AddQueryGroupBegin",
                        "AddQueryEqual(col=IntNum, val=2)",
                    "AddQueryGroupEnd",
                "AddQueryGroupEnd",
            "AddQueryOr",
                "AddQueryGroupBegin",
                    "AddQueryGreaterThan(col=IntNum, val=3)",
                "AddQueryGroupEnd"
            };
            Assert.AreEqual(expected, queryCalls);
        }

    }
} 