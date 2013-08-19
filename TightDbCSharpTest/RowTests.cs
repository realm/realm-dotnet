using NUnit.Framework;
using System;
using TightDbCSharp;

namespace TightDbCSharpTest
{
    [TestFixture]
    public class RowTests
    {
        [Test]
        public static void TestSetAndGet()
        {
            using (var t = new Table(
                new Field("Count", DataType.Int),
                new Field("Valid", DataType.Bool),
                new Field("Name", DataType.String),
                new Field("BLOB", DataType.Binary),
                new Field("HtmlPage", DataType.Mixed),
                new Field("FirstSeen", DataType.Date),
                new Field("float", DataType.Float),
                new Field("double", DataType.Double)
                ))
            {
                t.Add(1, true, "Hans", new byte[] { 0, 1, 2, 3, 4, 5 }, "MixedStr", new DateTime(1980, 1, 2, 0, 0, 0, DateTimeKind.Utc), 3.14f, 3.14 * 12);
                TableRow tr = t[0];
                Assert.AreEqual(1,tr.GetLong(0));
                Assert.AreEqual(true, tr.GetBoolean(1));
                Assert.AreEqual("Hans", tr.GetString(2));
                Assert.AreEqual(new []{0,1,2,3,4,5}, tr.GetBinary(3));
                Assert.AreEqual("MixedStr",tr.GetMixedString(4));
                Assert.AreEqual(new DateTime(1980, 1, 2, 0, 0, 0, DateTimeKind.Utc), tr.GetDateTime(5));
                Assert.AreEqual(3.14f, tr.GetFloat(6));
                Assert.AreEqual(3.14*12, tr.GetDouble(7));
            }
        }

        [Test]
        public static void TestIndexer()
        {
            using (var t = new Table(
                new Field("Count", DataType.Int),
                new Field("Valid", DataType.Bool),
                new Field("Name", DataType.String),
                new Field("BLOB", DataType.Binary),
                new Field("HtmlPage", DataType.Mixed),
                new Field("FirstSeen", DataType.Date),
                new Field("float", DataType.Float),
                new Field("double", DataType.Double)
                ))
            {
                t.Add(1, true, "Hans", new byte[] { 0, 1, 2, 3, 4, 5 }, "MixedStr", new DateTime(1980, 1, 2,0,0,0,DateTimeKind.Utc), 3.14f, 3.14 * 12);
                var tr = t[0];
                Assert.AreEqual(1, tr[0]);
                Assert.AreEqual(true, tr[1]);
                Assert.AreEqual("Hans", tr[2]);
                Assert.AreEqual(new[] { 0, 1, 2, 3, 4, 5 },tr[3]);
                Assert.AreEqual("MixedStr", tr[4]);
                DateTime returnedDateTime = (DateTime)tr[5] ;
                Assert.AreEqual(new DateTime(1980, 1, 2,0,0,0,DateTimeKind.Utc), returnedDateTime);
                Assert.AreEqual(3.14f, tr[6]);
                Assert.AreEqual(3.14 * 12, tr[7]);
            }
        }
    }
}

