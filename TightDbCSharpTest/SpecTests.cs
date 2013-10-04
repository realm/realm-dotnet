using System;
using System.IO;
using TightDbCSharp;
using NUnit.Framework;
using TightDbCSharp.Extensions;


namespace TightDbCSharpTest
{
    [TestFixture]
    internal static class SpecTests
    {
        [Test] public static void SpecInspection()
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
