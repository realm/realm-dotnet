using System.Collections.Generic;
using NUnit.Framework;
using System;
using TightDbCSharp;
using TightDbCSharp.Extensions;

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
        [ExpectedException("System.ArgumentException")]
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
        [ExpectedException("System.ArgumentException")]
        public static void GetColumnIndexNotFoundBug32BitTyped()
        {
            using (var table = new Table())
            {
                table.AddSubTableColumn("sub");                
                long columnIndex = table.GetColumnIndex("subint");//hint is okay long should not be changed to var
                Assert.AreEqual(-1, columnIndex);
            }
        }

        //helper method to get us a null reference where the compiler don't figure it out
        //0 as parameter will return a null
        //anything else as parameter will return an empty IEnumerable<object> that has no items to enumerate
        private static IEnumerable<object> GetNullEnum(int dummy)
        {
            return (dummy*-1) != (dummy*1) ? new List<object>() : null;//0*-1==0*+1 all other values are !=
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
                new DoubleColumn("double"),
                new SubTableColumn("SubObject", new IntColumn("Subint")), //used to test calls with Ienumerable<object>
                new SubTableColumn("SubTable", new IntColumn("Subint")),//used to test calls with Table    
                new SubTableColumn("SubNullValue", new IntColumn("Subint")), //used to test calls with an object that is null
                new SubTableColumn("SubNullConstant", new IntColumn("Subint")) //used to test calls with null as parameter
                ))
            using(var subTableTemplate = new Table(new IntColumn("Subint")))
            {                                
                subTableTemplate.AddMany(new[] { 8, 9, 10, 11 });                    
                t.Add(1, true, "Hans", new byte[] {0, 1, 2, 3, 4, 5}, "MixedStr",
                    new DateTime(1980, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                    3.14f, 3.14*12,
                    new object[] { 0, 1, 2, 3 },//could be interpreted as four rows of long now that we know the table schema is one long per row                    
                    subTableTemplate,
                    GetNullEnum(0),
                    null
                    );
                var tr = t[0];

                Assert.AreEqual(3, tr.GetColumnIndex("BLOB"));
                Assert.AreEqual(1, tr.GetLong(0));
                Assert.AreEqual(true, tr.GetBoolean(1));
                Assert.AreEqual("Hans", tr.GetString(2));
                Assert.AreEqual(new byte[] {0, 1, 2, 3, 4, 5}, tr.GetBinary(3));
                Assert.AreEqual("MixedStr", tr.GetMixedString(4));
                Assert.AreEqual("MixedStr", tr.GetMixed(4)); //hit this method too
                Assert.AreEqual(new DateTime(1980, 1, 2, 0, 0, 0, DateTimeKind.Utc), tr.GetDateTime(5));
                Assert.AreEqual(3.14f, tr.GetFloat(6));
                Assert.AreEqual(3.14*12, tr.GetDouble(7));
                using (var sub = tr.GetSubTable("SubObject"))
                {
                    Assert.AreEqual(4, sub.Size);
                    Assert.AreEqual(3, sub.GetLong(0, 3));
                }
                using (var sub = tr.GetSubTable("SubTable"))
                {
                    Assert.AreEqual(4, sub.Size);
                    Assert.AreEqual(11, sub.GetLong(0, 3));
                }
                using (var sub = tr.GetSubTable("SubNullValue"))
                {
                    Assert.AreEqual(0, sub.Size);
                }
                using (var sub = tr.GetSubTable("SubNullConstant"))
                {
                    Assert.AreEqual(0, sub.Size);
                }



                t.Remove(0);
                t.AddEmptyRow(2);
                tr = t[1];
                tr[0] = 1;
                tr[1] = true;
                tr[2] = "Hans";
                tr[3] = new byte[] {0, 1, 2, 3, 4, 5};
                tr[4] = "MixedStr";
                tr[5] = new DateTime(1980, 1, 2, 0, 0, 0, DateTimeKind.Utc);
                tr[6] = 3.14f;
                tr[7] = 3.14*12;
                tr[8] = new object[] {4, 3, 2, 1};
                tr[9] = subTableTemplate;
                tr[10] = GetNullEnum(0);
                tr[10] = null;//set as null as a constant
           
            Assert.AreEqual(1, tr.GetLong(0));
                Assert.AreEqual(true, tr.GetBoolean(1));
                Assert.AreEqual("Hans", tr.GetString(2));
                Assert.AreEqual(new byte[] { 0, 1, 2, 3, 4, 5 }, tr.GetBinary(3));
                Assert.AreEqual("MixedStr", tr.GetMixedString(4));
                Assert.AreEqual(new DateTime(1980, 1, 2, 0, 0, 0, DateTimeKind.Utc), tr.GetDateTime(5));
                Assert.AreEqual(3.14f, tr.GetFloat(6));
                Assert.AreEqual(3.14 * 12, tr.GetDouble(7));
                using (var sub = tr.GetSubTable("SubObject"))
                {
                    Assert.AreEqual(4, sub.Size);
                    Assert.AreEqual(1, sub.GetLong(0, 3));
                }
                using (var sub = tr.GetSubTable("SubTable"))
                {
                    Assert.AreEqual(4, sub.Size);
                    Assert.AreEqual(11, sub.GetLong(0, 3));
                }
                using (var sub = tr.GetSubTable("SubNullValue"))
                {
                    Assert.AreEqual(0, sub.Size);
                }
                using (var sub = tr.GetSubTable("SubNullConstant"))
                {
                    Assert.AreEqual(0, sub.Size);
                }

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
                tr.SetSubTable(8,new object[] { 1, 2, 3, 4 });
                tr.SetSubTable(9,subTableTemplate);
                tr.SetSubTable("SubNullValue",GetNullEnum(0));
#if VER40PLUS                
                tr.SetSubTable("SubNullConstant", null);//this is only possible in .net 40 and 45 -in .35 user must use tr.ClearSubTable(column)
#endif
                
                Assert.AreEqual(1, tr.GetLong(0));
                Assert.AreEqual(true, tr.GetBoolean(1));
                Assert.AreEqual("Hans", tr.GetString(2));
                Assert.AreEqual(new byte[] { 0, 1, 2, 3, 4, 5 }, tr.GetBinary(3));
                Assert.AreEqual("MixedStr", tr.GetMixedString(4));
                Assert.AreEqual(new DateTime(1980, 1, 2, 0, 0, 0, DateTimeKind.Utc), tr.GetDateTime(5));
                Assert.AreEqual(3.14f, tr.GetFloat(6));
                Assert.AreEqual(3.14 * 12, tr.GetDouble(7));
                using (var sub = tr.GetSubTable(8))
                {
                    Assert.AreEqual(4, sub.Size);
                    Assert.AreEqual(4, sub.GetLong(0, 3));
                }
                using (var sub = tr.GetSubTable("SubTable"))
                {
                    Assert.AreEqual(4, sub.Size);
                    Assert.AreEqual(11, sub.GetLong(0, 3));
                }
                using (var sub = tr.GetSubTable("SubNullValue"))
                {
                    Assert.AreEqual(0, sub.Size);
                }
                using (var sub = tr.GetSubTable("SubNullConstant"))
                {
                    Assert.AreEqual(0, sub.Size);
                }


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
                tr.SetSubTable("SubObject", new object[] { 4, 3, 2, 1 });
                tr.SetSubTable("SubTable", subTableTemplate);
                tr.SetSubTable("SubNullValue", GetNullEnum(0));//in this case the compiler do not know p2 is null
#if V40PLUS
                tr.SetSubTable("SubNullConstant",null);//specifying null directly is only legal i .net40 plus. 
#endif

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
                using (var sub = tr.GetSubTable(8))
                {
                    Assert.AreEqual(4, sub.Size);
                    Assert.AreEqual(1, sub.GetLong(0, 3));
                }
                using (var sub = tr.GetSubTable("SubTable"))
                {
                    Assert.AreEqual(4, sub.Size);
                    Assert.AreEqual(11, sub.GetLong(0, 3));
                }
                using (var sub = tr.GetSubTable("SubNullValue"))
                {
                    Assert.AreEqual(0, sub.Size);
                }
                using (var sub = tr.GetSubTable("SubNullConstant"))
                {
                    Assert.AreEqual(0, sub.Size);
                }
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
        /// Call clearsubtable and inspect that the subtable is in fact cleared (zero rows)
        /// </summary>
        [Test]
        public static void TestClearSubtable()
        {
            const int  testvalue =  422;
            using (var table = new Table("sub".Table("intfield".Int())))
            {
                using (var sub = new Table("intfield".Int())){
                    sub.Add(testvalue);
                    table.Add(sub);
                    foreach (var row in table)
                    {
                        using (var sub2 = row.GetSubTable(0))
                        {
                            Assert.AreEqual(testvalue, sub2.GetLong(0,0));
                            Assert.AreEqual(1,sub2.Size);
                            row.ClearSubTable(0);//this will render sub2 invalid as it has been changed from the outside

                            using (var sub3 = row.GetSubTable(0))
                            {
                                Assert.AreEqual(0, sub3.Size);
                                Assert.AreEqual(1, sub3.ColumnCount);
                            }
                        }
                    }

                    table.Remove(0);//remove the row with the emptied sub
                    table.Add(sub);//add a sub with a subfield integer with 12
                    foreach (var row in table)
                    {
                        using (var sub2 = row.GetSubTable(0))
                        {
                            Assert.AreEqual(testvalue, sub2.GetLong(0,0));
                            Assert.AreEqual(1, sub2.Size);
                            row.ClearSubTable("sub");//this will render sub2 invalid as it has been changed from the outside

                            using (var sub3 = row.GetSubTable(0))
                            {
                                Assert.AreEqual(0, sub3.Size);
                                Assert.AreEqual(1, sub3.ColumnCount);
                            }
                        }
                    }

                }
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
        [ExpectedException("System.ArgumentException")]
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

