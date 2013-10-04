using NUnit.Framework;
using System;
using TightDbCSharp;

namespace TightDbCSharpTest
{
    /// <summary>
    /// Tests that test the Row class
    /// </summary>
    [TestFixture]
    public static class RowTests
    {



        /// <summary>
        /// test get subtable and setting and getting int in that subtable (using a row as a starting point)
        /// </summary>
        [Test]
        public static void TestSubTableIntIndex()
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




        /// <summary>
        /// test setting and getting longs and strings in a subtable mixed via row
        /// </summary>
        [Test]
        public static void TableRowMixedValues()
        {
            using (var table = new Table(new MixedColumn("subinmixed")))
            {
                table.AddEmptyRow(1);
                using (var sub1 = new Table(new StringColumn("Name"), new IntColumn("Cases")))
                {
                    sub1.Add("Firstname", 42);
                    sub1.Add("Secondname", 43);                                
                    table.SetMixedSubTable(0, 0, sub1);
                }
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


        /// <summary>
        /// test that tablerow is not validated when legally changing row field values
        /// </summary>
        [Test]
        public static void TestNoInvalidationWhileChangingRow()
        {
            using (var table = new Table(new IntColumn("int")))
            {
                table.AddEmptyRow(1);
                Row row = table[0];
                row.SetInt(0,42);                
                row.SetInt(0,45);//this should be okay. table version changes only if inserting or adding rows or columns
                Assert.AreEqual(45,row.GetLong(0));
                table.SetInt(0,0,33);
                Assert.AreEqual(33,row.GetLong(0));//also okay. table version should only change if indexes are void, inserting and adding rows or columns
            }            
        }

        /// <summary>
        /// test that tablerow is invalidated if table changes row count
        /// </summary>
        [Test]
        [ExpectedException("System.InvalidOperationException")]
        public static void TestInvalidationWhileChangingRow()
        {
            using (var table = new Table(new IntColumn("int")))
            {
                table.AddEmptyRow(1);
                Row row = table[0];
                row.SetInt(0, 42);
                row.SetInt(0, 45);//this should be okay. table version changes only if inserting or adding rows or columns
                Assert.AreEqual(45, row.GetLong(0));
                table.SetInt(0, 0, 33);
                Assert.AreEqual(33, row.GetLong(0));//also okay. table version should only change if indexes are void, inserting and adding rows or columns
                table.AddEmptyRow(1);
                var valuenow = row.GetLong(0);//the row.getlong should throw
                Assert.AreEqual(valuenow,valuenow);//in case no throw, dont throw an asssert-need the expectedexception to trigger
            }
        }


        /// <summary>
        /// test getting a subtablie via its name using a tablerow
        /// </summary>
        [Test]
        
        public static void TestSubTableStringIndex()
        {
            using (var table = new Table())
            {
                var subpath = table.AddSubTableColumn("sub");
                table.AddIntColumn(subpath, "subint");
                table.AddEmptyRow(1);
                var sub = table[0].GetSubTable("sub");//getting the subtable via a row object
                sub.AddEmptyRow(1);
                sub.SetLong(0, 0, 42);
                Assert.AreEqual(42, sub.GetLong(0, 0));
            }
        }


        //
        /// <summary>
        /// Interop test
        /// if the getcolumnindex -1 returned is somehow intepreted as unsigned, this test will catch it and fail
        /// The test ought to fail with System.ArgumentOutOfRangeException
        /// </summary>
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

        
        /// <summary>
        /// Interop test
        /// if the getcolumnindex -1 returned is somehow intepreted as unsigned, this test will catch it and fail
        /// The test ought to fail with System.ArgumentOutOfRangeException
        /// </summary>
        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void GetColumnIndexNotFoundBug32BitTyped()
        {
            using (var table = new Table())
            {
                table.AddSubTableColumn("sub");                
                long columnIndex = table.GetColumnIndex("subint");//hint is okay long should not be changed to var
                Assert.AreEqual(-1, columnIndex);
            }
        }



        /// <summary>
        /// Tests setting and gettting all DataType types
        /// </summary>
        [Test]
        public static void TestSetAndGet()
        {
            using (var t = new Table(
                new IntColumn("Count"),
                new BoolColumn("Valid"),
                new StringColumn("Name"),
                new BinaryColumn("BLOB"),
                new MixedColumn("HtmlPage"),
                new DateColumn("FirstSeen"),
                new FloatColumn("float"),
                new DoubleColumn("double")
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

        /// <summary>
        /// test remove method on tablerow
        /// </summary>
        [Test]
        public static void TestRowDelete()
        {
            Table table;
            using (table = new Table(new StringColumn("test")) )
            {
                table.Add("Hans");
                table.Add("Grethe");
                TableRow tr = table[0];
                Assert.AreEqual(2,table.Size);
                tr.Remove();
                Assert.AreEqual(1, table.Size);
                Assert.AreEqual("Grethe",table.GetString(0,0));
            }
        }


        //test if a row object gets disabled when it changes its table in an invalidating way
        /// <summary>
        /// test that tablerow is invalidated by remove from inside tablerow
        /// </summary>
        [Test]
        [ExpectedException("System.InvalidOperationException")]//because the table row shouldve been invalidated after it was removed
        public static void TestRowDeleteInvalidated()
        {
            using (var table = new Table(new StringColumn("test")) )
            {
                table.AddMany(new[] {"Hans", "Grethe"});
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

        
        /// <summary>
        /// test if a row object gets disabled when the user changes its table in an invalidating way, not going through the rowobject
        /// </summary>
        [Test]
        [ExpectedException("System.InvalidOperationException")]//because the table row shouldve been invalidated after it was removed
        public static void TestRowDeleteInvalidatedThroughTable()
        {
            using (var table = new Table(new StringColumn("test")) )
            {
                table.AddMany(new[] {"Hans", "Grethe"});
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


        /// <summary>
        /// test if a row object gets disabled when the user changes its table in an invalidating way, trough a copy of the table
        /// taken out from a group
        /// this will liekly only work correctly if we ensure that table wrappers are reused when user requests the sam table
        /// test if a row object gets disabled when the user changes its table in an invalidating way, not going through the rowobject
        /// </summary>
        [Test]
        [ExpectedException("System.InvalidOperationException")]
        //because the table row shouldve been invalidated after it was removed
        public static void TestRowDeleteInvalidatedThroughGroup()
        {
            using (var group = new Group())
            using (var table = group.CreateTable("T1", new StringColumn("test")))
            using (var table2 = group.GetTable("T1"))

            {
                table.Add("Hans");
                table.Add("Grethe");
                TableRow tr = table[0];
                Assert.AreEqual(2, table.Size);
                Assert.AreEqual(true, tr.IsValid());
                table2.Remove(0);
                Assert.AreEqual(1, table.Size);
                Assert.AreEqual(true, tr.OwnerTable.IsValid());
                Assert.AreEqual(false, tr.IsValid());//unit test fails here bc we have two table C# wrappers
                var grethe = tr.GetString(0); //this should fail bc accessing row after delete or insert is illegal
                Assert.AreEqual("Grethe", grethe); //this should never run
            }
        }

        /// <summary>
        /// test that string indexing fields in a tablerow fails when the string is not a column name
        /// </summary>
        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TestIndexerWrongStringIndex()
        {
            using (var t = new Table(new IntColumn("A")))
            {
                t.Add(42);
                TableRow tr = t[0];
                var tester = (long) tr["NoRow"];//exception should be thrown here
                Assert.AreEqual(0,tester);//use tester
                Assert.Fail("Accessing table row with a bad column string index should have thrown an exception");
            }            
        }


        /// <summary>
        /// Test indexer on various DataType types
        /// </summary>
        [Test]
        public static void TestIndexer()
        {
            using (var t = new Table(
                new ColumnSpec("Count", DataType.Int),
                new ColumnSpec("Valid", DataType.Bool),
                new ColumnSpec("Name", DataType.String),
                new ColumnSpec("BLOB", DataType.Binary),
                new ColumnSpec("HtmlPage", DataType.Mixed),
                new ColumnSpec("FirstSeen", DataType.Date),
                new ColumnSpec("float", DataType.Float),
                new ColumnSpec("double", DataType.Double)
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
                tr["Valid"] = false;
                Assert.AreEqual(false,tr["Valid"]);
            }
        }
    }
}

