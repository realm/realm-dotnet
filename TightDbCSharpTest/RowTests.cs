using NUnit.Framework;
using System;
using TightDbCSharp;

namespace TightDbCSharpTest
{
    [TestFixture]
    public class RowTests
    {

        [Test]
        public static void TestSubtableIntIndex()
        {
            using (var table = new Table())
            {
                var subpath=table.AddSubTableColumn("sub");
                table.AddIntColumn(subpath, "subint");
                table.AddEmptyRow(1);
                var sub = table[0].GetSubTable(0);//getting the subtable via a row object
                sub.AddEmptyRow(1);
                sub.SetLong(0, 0, 42);
                Assert.AreEqual(42, sub.GetLong(0,0));
            }
        }

        [Test]
        public static void TableRowMixedValues()
        {
            using (var table = new Table(new MixedField("subinmixed")))
            {
                table.AddEmptyRow(1);
                table.SetMixedSubTable(0, 0,
                    new Table(new StringField("Name"), new IntField("Cases"))
                    {
                        {"Firstname", 42},
                        {"Secondname", 43}
                    })
                    ;
                using (var  sub = table.GetMixedSubTable(0,0))
                {
                    Assert.AreEqual(42,sub.GetLong(1,0));
                    Assert.AreEqual(43, sub.GetLong(1, 1));
                    Assert.AreEqual("Firstname", sub.GetString(0, 0));
                    Assert.AreEqual("Secondname", sub.GetString(0, 1));
                }

                using (var sub = table[0].GetMixedTable(0))
                {
                    Assert.AreEqual(42, sub.GetLong(1, 0));
                    Assert.AreEqual(43, sub.GetLong(1, 1));
                    Assert.AreEqual("Firstname", sub.GetString(0, 0));
                    Assert.AreEqual("Secondname", sub.GetString(0, 1));                    
                }

                using (var sub = table[0].GetMixedTable("subinmixed"))
                {
                    Assert.AreEqual(42, sub.GetLong(1, 0));
                    Assert.AreEqual(43, sub.GetLong(1, 1));
                    Assert.AreEqual("Firstname", sub.GetString(0, 0));
                    Assert.AreEqual("Secondname", sub.GetString(0, 1));
                }

            }

        }



        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TestSubtableStringIndex()
        {
            using (var table = new Table())
            {
                var subpath = table.AddSubTableColumn("sub");
                table.AddIntColumn(subpath, "subint");
                table.AddEmptyRow(1);
                var sub = table[0].GetSubTable("subint");//getting the subtable via a row object
                sub.AddEmptyRow(1);
                sub.SetLong(0, 0, 42);
                Assert.AreEqual(42, sub.GetLong(0, 0));
            }
        }


        //if the getcolumnindex -1 returned is somehow intepreted as unsigned, this test will catch it and fail
        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void GetColumnIndexNotFoundBug32Bit()
        {
            using (var table = new Table())
            {
                table.AddSubTableColumn("sub");
                var columnIndex = table.GetColumnIndex("subint");
                Assert.AreEqual(-1,columnIndex);
            }
        }

        //if the getcolumnindex -1 returned is somehow intepreted as unsigned, this test will catch it and fail
        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void GetColumnIndexNotFoundBug32Bityped()
        {
            using (var table = new Table())
            {
                table.AddSubTableColumn("sub");                
                long columnIndex = table.GetColumnIndex("subint");//hint is okay long should not be changed to var
                Assert.AreEqual(-1, columnIndex);
            }
        }



        [Test]
        public static void TestSetAndGet()
        {
            using (var t = new Table(
                new IntField("Count"),
                new BoolField("Valid"),
                new StringField("Name"),
                new BinaryField("BLOB"),
                new MixedField("HtmlPage"),
                new DateField("FirstSeen"),
                new FloatField("float"),
                new DoubleField("double")
                ))
            {               
                t.Add(1, true, "Hans", new byte[] { 0, 1, 2, 3, 4, 5 }, "MixedStr", new DateTime(1980, 1, 2, 0, 0, 0, DateTimeKind.Utc), 3.14f, 3.14 * 12);
                TableRow tr = t[0];

                Assert.AreEqual(3,tr.GetColumnIndex("BLOB"));
                Assert.AreEqual(1,tr.GetLong(0));
                Assert.AreEqual(true, tr.GetBoolean(1));
                Assert.AreEqual("Hans", tr.GetString(2));
                Assert.AreEqual(new byte[]{0,1,2,3,4,5}, tr.GetBinary(3));
                Assert.AreEqual("MixedStr",tr.GetMixedString(4));
                Assert.AreEqual("MixedStr", tr.GetMixed(4));//hit this method too
                Assert.AreEqual(new DateTime(1980, 1, 2, 0, 0, 0, DateTimeKind.Utc), tr.GetDateTime(5));
                Assert.AreEqual(3.14f, tr.GetFloat(6));
                Assert.AreEqual(3.14*12, tr.GetDouble(7));

                t.Remove(0);
                t.AddEmptyRow(2);
                tr = t[1];
                tr[0] = 1;
                tr[1] = true;
                tr[2] = "Hans";
                tr[3] = new byte[] {0, 1, 2, 3, 4, 5};
                tr[4] = "MixedStr";
                tr[5] = new DateTime(1980, 1, 2, 0, 0, 0, DateTimeKind.Utc);
                tr[6] =  3.14f;
                tr[7] = 3.14 * 12;
                Assert.AreEqual(1, tr.GetLong(0));
                Assert.AreEqual(true, tr.GetBoolean(1));
                Assert.AreEqual("Hans", tr.GetString(2));
                Assert.AreEqual(new byte[] { 0, 1, 2, 3, 4, 5 }, tr.GetBinary(3));
                Assert.AreEqual("MixedStr", tr.GetMixedString(4));
                Assert.AreEqual(new DateTime(1980, 1, 2, 0, 0, 0, DateTimeKind.Utc), tr.GetDateTime(5));
                Assert.AreEqual(3.14f, tr.GetFloat(6));
                Assert.AreEqual(3.14 * 12, tr.GetDouble(7));

                var rowNo = t.AddEmptyRow(1);
                tr = t[rowNo];
                tr.SetLong(0,1);
                tr.SetBoolean(1,true);
                tr.SetString(2,"Hans");
                tr.SetBinary(3,new byte[] { 0, 1, 2, 3, 4, 5 });
                tr.SetMixedString(4,"MixedStr");               
                tr.SetDateTime(5, new DateTime(1980, 1, 2, 0, 0, 0, DateTimeKind.Utc));
                tr.SetFloat(6,3.14f);
                tr.SetDouble(7,3.14 * 12);


                Assert.AreEqual(1, tr.GetLong(0));
                Assert.AreEqual(true, tr.GetBoolean(1));
                Assert.AreEqual("Hans", tr.GetString(2));
                Assert.AreEqual(new byte[] { 0, 1, 2, 3, 4, 5 }, tr.GetBinary(3));
                Assert.AreEqual("MixedStr", tr.GetMixedString(4));
                Assert.AreEqual(new DateTime(1980, 1, 2, 0, 0, 0, DateTimeKind.Utc), tr.GetDateTime(5));
                Assert.AreEqual(3.14f, tr.GetFloat(6));
                Assert.AreEqual(3.14 * 12, tr.GetDouble(7));


                rowNo = t.AddEmptyRow(1);
                tr = t[rowNo];
                tr.SetLong("Count", 1);
                tr.SetBoolean("Valid", true);
                tr.SetString("Name", "Hans");
                tr.SetBinary("BLOB", new byte[] { 0, 1, 2, 3, 4, 5 });
                tr.SetMixedString("HtmlPage", "MixedStr");
                
                tr.SetDateTime("FirstSeen", new DateTime(1980, 1, 2, 0, 0, 0, DateTimeKind.Utc));
                tr.SetFloat("float", 3.14f);
                tr.SetDouble("double", 3.14 * 12);


                Assert.AreEqual(1, tr.GetLong(0));
                Assert.AreEqual(true, tr.GetBoolean(1));
                Assert.AreEqual(true, tr.GetBoolean("Valid"));
                Assert.AreEqual("Hans", tr.GetString(2));
                Assert.AreEqual("Hans", tr.GetString("Name"));
                Assert.AreEqual(new byte[] { 0, 1, 2, 3, 4, 5 }, tr.GetBinary(3));
                Assert.AreEqual(new byte[] { 0, 1, 2, 3, 4, 5 }, tr.GetBinary("BLOB"));
                Assert.AreEqual("MixedStr", tr.GetMixedString(4));
                Assert.AreEqual(new DateTime(1980, 1, 2, 0, 0, 0, DateTimeKind.Utc), tr.GetDateTime(5));
                Assert.AreEqual(3.14f, tr.GetFloat(6));
                Assert.AreEqual(3.14 * 12, tr.GetDouble(7));        
            }
        }

        [Test]
        public static void TestRowDelete()
        {
            using (var table = new Table(new StringField("test")) {"Hans", "Grethe"})
            {
                TableRow tr = table[0];
                Assert.AreEqual(2,table.Size);
                tr.Remove();
                Assert.AreEqual(1, table.Size);
                Assert.AreEqual("Grethe",table.GetString(0,0));
            }
        }



        //test if a row object gets disabled when it changes its table in an invalidating way
        [Test]
        [ExpectedException("System.InvalidOperationException")]//because the table row shouldve been invalidated after it was removed
        public static void TestRowDeleteInvalidated()
        {
            using (var table = new Table(new StringField("test")) { "Hans", "Grethe" })
            {
                TableRow tr = table[0];
                Assert.AreEqual(2, table.Size);
                Assert.AreEqual(true,tr.IsValid());
                tr.Remove();
                Assert.AreEqual(1, table.Size);
                Assert.AreEqual(true,tr.OwnerTable.IsValid());
                Assert.AreEqual(false,tr.IsValid());
                var grethe = tr.GetString(0);//this should fail bc accessing row after delete or insert is illegal
                Assert.AreEqual("Grethe",grethe);//this should never run
            }
        }

        //test if a row object gets disabled when the user changes its table in an invalidating way, not going through the rowobject
        [Test]
        [ExpectedException("System.InvalidOperationException")]//because the table row shouldve been invalidated after it was removed
        public static void TestRowDeleteInvalidatedThrougTtable()
        {
            using (var table = new Table(new StringField("test")) { "Hans", "Grethe" })
            {
                TableRow tr = table[0];
                Assert.AreEqual(2, table.Size);
                Assert.AreEqual(true, tr.IsValid());
                table.Remove(0);
                Assert.AreEqual(1, table.Size);
                Assert.AreEqual(true, tr.OwnerTable.IsValid());
                Assert.AreEqual(false, tr.IsValid());
                var grethe = tr.GetString(0);//this should fail bc accessing row after delete or insert is illegal
                Assert.AreEqual("Grethe", grethe);//this should never run
            }
        }


        //test if a row object gets disabled when the user changes its table in an invalidating way, trough a copy of the table
        //taken out from a group
        //this will liekly only work correctly if we ensure that table wrappers are reused when user requests the sam table
        //test if a row object gets disabled when the user changes its table in an invalidating way, not going through the rowobject
        [Test]
        [ExpectedException("System.InvalidOperationException")]
        //because the table row shouldve been invalidated after it was removed
        public static void TestRowDeleteInvalidatedThrougGroup()
        {
            using (var group = new Group())
            using (var table = group.CreateTable("T1", new StringField("test")))
            using (var table2 = group.GetTable("T1"))

            {
                table.Add("Hans");
                table.Add("Grethe");
                TableRow tr = table[0];
                Assert.AreEqual(2, table.Size);
                Assert.AreEqual(true, tr.IsValid());
                {
                    table2.Remove(0);
                }
                Assert.AreEqual(1, table.Size);
                Assert.AreEqual(true, tr.OwnerTable.IsValid());
                Assert.AreEqual(false, tr.IsValid());//unit test fails here bc we have two table C# wrappers
                var grethe = tr.GetString(0); //this should fail bc accessing row after delete or insert is illegal
                Assert.AreEqual("Grethe", grethe); //this should never run
            }

        }

        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TestIndexerWrongStringIndex()
        {
            using (var t = new Table(new IntField("A")))
            {
                t.Add(42);
                TableRow tr = t[0];
                var tester = (long) tr["NoRow"];
                Assert.Fail("Accessing tablerow with a bad string index should have thrown an exception");
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
                Assert.AreEqual(8,tr.ColumnCount);
                Assert.AreEqual(1, tr[0]);
                Assert.AreEqual(true, tr["Valid"]);//todo add unit test that tests if invalid strings in row indexer are thrown
                Assert.AreEqual("Hans", tr[2]);
                Assert.AreEqual(new[] { 0, 1, 2, 3, 4, 5 },tr[3]);
                Assert.AreEqual("MixedStr", tr[4]);
                var returnedDateTime = (DateTime)tr[5] ;
                Assert.AreEqual(new DateTime(1980, 1, 2,0,0,0,DateTimeKind.Utc), returnedDateTime);
                Assert.AreEqual(3.14f, tr[6]);
                Assert.AreEqual(3.14 * 12, tr[7]);
            }
        }
    }
}

