using NUnit.Framework;
using TightDbCSharp;
using TightDbCSharp.Extensions;

namespace TightDbCSharpTest
{
    [TestFixture]
    public static class SpecTests
    {

        [Test]
        public static void SpecInspection()
        {
            using (var table = new Table("IntColumn".Int(), "sub".SubTable("int".Int())))
            {
                Assert.AreEqual(2,table.Spec.ColumnCount );
                Assert.AreEqual(DataType.Table,table.Spec.GetColumnType(1));
                Assert.AreEqual("int", table.Spec.GetSpec(1).GetColumnName(0));
            }
        }


    }
}
