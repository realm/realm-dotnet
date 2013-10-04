using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TightDbCSharp;
using TightDbCSharp.Extensions;
using System.Reflection;


namespace TightDbCSharpTest
{
    /// <summary>
    /// Tests public TableView methods
    /// </summary>
    [TestFixture]
    public static class TableViewTests
    {
        /// <summary>
        /// simple call just to get a tableview (and have it deallocated when it exits scope)
        /// </summary>
        [Test]        
        public static void TableViewCreation()
        {
            using (var t = new Table("intfield".Int()))
            {
                t.AddEmptyRow(1);
                t.SetLong(0, 0, 42);
                using (TableView tv = t.FindAllInt(0, 42))
                {
                    Console.WriteLine(tv.Handle);
                }
            }
        }

        //returns a table with row 0 having ints 0 to 999 ascending
        //row 1 having ints 0 to 99 ascendig (10 of each)
        //row 2 having ints 0 to 9 asceding (100 of each)     
        internal static Table TableWithMultipleIntegers()
        {
            Table returnTable;
            Table t = null;
            try
            {
                t = new Table(new IntColumn("intcolumn0"), new IntColumn("intcolumn1"), new IntColumn("intcolumn2"));
                //t = new Table(new { intcolumn0 = DataType.Binary, intcolumn1 = DataType.Bool, intcolumn2 = DataType.Double });
                for (int n = 0; n < 1000; n++)
                {
                    long col0 = n;
                    long col1 = n / 10;
                    long col2 = n / 100;
                    t.AddEmptyRow(1);
                    t.SetLong(0, n, col0);
                    t.SetLong(1, n, col1);
                    t.SetLong(2, n, col2);
                }

                returnTable = t;
                t = null;

            }
            finally
            {
                if (t != null)
                {
                    t.Dispose();
                    returnTable = null;
                }
            }

            return returnTable;
        }

        //returns a table with string columns up to index columnIndex, which is an integer column, and then a string column after that
        //table is populated with null strings and in the int column row 0 has value 0*2, row 1 has value 1*2 , row n has value n*2
        //following the pattern for returning a disposable object http://msdn.microsoft.com/query/dev11.query?appId=Dev11IDEF1&l=EN-US&k=k(CA2000);k(TargetFrameworkMoniker-.NETFramework,Version%3Dv4.5);k(DevLang-csharp)&rd=true 
        private static Table GetTableWithNIntegersInColumn(long columnIndex, long numberOfIntegers)
        {
            Table returnTable;
            Table t = null;
            try
            {
                t = new Table();
                for (int n = 0; n < columnIndex; n++)
                {
                    t.AddStringColumn("StringColumn" + n);
                }
                t.AddIntColumn( "IntColumn");
                t.AddStringColumn( "StringcolumnLast");

                for (int n = 0; n < numberOfIntegers; n++)
                {
                    t.AddEmptyRow(1);
                    t.SetLong(columnIndex, n, n * 2);
                }

                //if exceptions are raised above this point, returnTable will be null, and t is with errors
                //finally should in that situation dispose of t gracefully and return null
                returnTable = t;
                t = null; //thus, when we are in the finally part, if t is not null some error has cause us to except out before returntable was 
                //created successfully. so dispose of the broken table
            }

            finally
            {
                if (t != null)
                {
                    t.Dispose();
                    returnTable = null;
                }
            }

            return returnTable;
        }

        /// <summary>
        /// create table view then search for all ints and return nothing as there is no matches
        /// </summary>
        [Test]        
        public static void TableViewNoResult()
        {
            const long column = 3;
            using (var t = GetTableWithNIntegersInColumn(column, 100))
            {
                TableView tv = t.FindAllInt(column, 1001);
                Assert.AreEqual(0, tv.Size);
            }
        }

        /// <summary>
        /// Test TableView.add(mixed) called with a string
        /// </summary>
        [Test]
        public static void TableViewMixedString()
        {
            //this first part just populates a table with two mixed string fields
            using (var t = new Table(new MixedColumn("StringField"),new IntColumn("intfield")))
            {
                const string setWithAdd = "SetWithAdd";
                const string setWithSetMixed = "SetWithSetMixed";
                const string setWithViewSetMixed = "SetWithViewSetMixed";
                const string notInView = "notInView";
                t.Add(setWithAdd,42);
                DataType dtRow0 = t.GetMixedType(0, 0);
                Assert.AreEqual(DataType.String, dtRow0);//mixed from empty rows added are int as a default
                String row0 = t.GetMixedString(0, 0);
                Assert.AreEqual(setWithAdd, row0);

                t.AddEmptyRow(1);
                t.SetMixedString(0, 1, setWithSetMixed);
                t.SetLong(1,1,42);
                DataType dtRow1 = t.GetMixedType(0, 1);
                Assert.AreEqual(DataType.String, dtRow1);
                String row1 = t.GetMixedString(0, 1);
                Assert.AreEqual(setWithSetMixed, row1);

                t.Add(notInView, 43);
                t.Add(notInView, 44);
                //create tableview with all the '42' rows, and do mixed string operations on the tableview
                using (var view = t.FindAllInt(1, 42))
                {                   
                    view.SetMixedString(0, 1, setWithViewSetMixed);                    
                    DataType dtvRow1 = view.GetMixedType(0, 1);
                    Assert.AreEqual(DataType.String, dtvRow1);
                    String viewRow1 = view.GetMixedString(0, 1);
                    Assert.AreEqual(setWithViewSetMixed, viewRow1);
                }
            }
        }


        /// <summary>
        /// Test that setting a binary with NULL works
        /// Tests that the binary is read back successfully using most access methods
        /// </summary>
        [Test]
        public static void TableViewSetGetEmptyBinary()
        {
            using (var table = new Table(new BinaryColumn("bin"),new IntColumn("int")))
            {
                table.AddEmptyRow(1);//empty row
                table.AddEmptyRow(1);
                table.SetLong(1,1,42);//empty binary set by addemptyrow but int = 42                
                table.Add(null, 42);  //empty binary set with null
                Array binaryData3 = new Byte[] { };
                table.Add(binaryData3,42);//empty binary set with an empty byte array

                using (var tableView = table.FindAllInt(1, 42))
                {
                    //reading back a binarydata that was added with addempty row
                    Array binaryData = tableView.GetBinary(0, 0);
                    Assert.AreEqual(0, binaryData.Length);

                    //setting null, getting an empty binary data back
                    Array binaryData2 = tableView.GetBinary(0, 1);
                    Assert.AreEqual(0, binaryData2.Length);

                    //setting null, getting an empty binary data back
                    tableView.SetBinary(0, 1, null);
                    Array binaryData5 = tableView.GetBinary("bin", 1);
                    Assert.AreEqual(0, binaryData5.Length);

                    //setting empty binary data, and getting that back again
                    Array binaryData4 = tableView.GetBinary(0, 2);
                    Assert.AreEqual(0, binaryData4.Length);                    
                }
                //accessing through a row cursor
                using (var tableView = table.FindAllInt(1, 42))
                {
                    //reading back a binarydata that was added with addempty row
                    Array binaryData = tableView[0].GetBinary(0);
                    Assert.AreEqual(0, binaryData.Length);

                    //setting null, getting an empty binary data back
                    Array binaryData2 = tableView[1].GetBinary(0);
                    Assert.AreEqual(0, binaryData2.Length);

                    //setting null, getting an empty binary data back
                    tableView.SetBinary(0, 1, null);
                    Array binaryData5 = tableView[1].GetBinary(0);
                    Assert.AreEqual(0, binaryData5.Length);

                    //setting empty binary data, and getting that back again
                    Array binaryData4 = tableView[2].GetBinary(0);
                    Assert.AreEqual(0, binaryData4.Length);
                }                

            
            }
            //the 3 above for table are to be found in TableTests
        }


        /// <summary>
        /// Test TableView.FindFirstBinary
        /// </summary>
        [Test]        
        public static void TableViewFindFirstBinary()
        {
            using (var table = new Table("radio".Binary(), "int".Int()))
            {
                byte[] testArray1 = { 01, 12, 36, 22 };
                byte[] testArray2 = { 02, 12, 36, 22 };
                byte[] testArray3 = { 03, 12, 36, 22 };
                byte[] testArray4 = { 04, 12, 36, 22 };
                table.Add(testArray1, 1);
                table.Add(testArray1, 2);
                table.Add(testArray2, 1);
                table.Add(testArray2, 2);
                table.Add(testArray3, 1);
                table.Add(testArray3, 2);
                table.Add(testArray4, 1);
                table.Add(testArray4, 2);

                byte[] arrayToFind = { 02, 12, 36, 22 };
                using (var view = table.FindAllInt(1, 1))
                {
                    Assert.AreEqual(4, view.Size);
                    {
                        var rowIndex = view.FindFirstBinary(0, arrayToFind);
                        Assert.AreEqual(1, rowIndex);
                    }
                    {
                        var rowIndex = view.FindFirstBinary("radio", arrayToFind);
                        Assert.AreEqual(1, rowIndex);
                    }
                }
            }
        }



        /// <summary>
        /// Test table.findallint
        /// </summary>
        [Test]        
        public static void TableViewWithOneRow()
        {
            const long column = 3;
            using (var t = GetTableWithNIntegersInColumn(column, 100))
            {
                TableView tv = t.FindAllInt(column, 42);
                Assert.AreEqual(1, tv.Size);
            }
        }

        /// <summary>
        /// Test findallint with several hits
        /// </summary>
        [Test]        
        public static void TableViewWithManyRows()
        {
            using (var t = TableWithMultipleIntegers())
            {
                {
                    TableView tv = t.FindAllInt(1, 5);
                    Assert.AreEqual(10, tv.Size);
                }
                {
                    TableView tv = t.FindAllInt("intcolumn1", 5);
                    Assert.AreEqual(10, tv.Size);
                }

            }
        }


        /// <summary>
        /// setting a long via a tableview
        /// </summary>
        [Test]
        
        public static void TableViewFindAllReadValues()
        {
            using (var t = TableWithMultipleIntegers())
            {
                TableView tv = t.FindAllInt(2, 9);
                Assert.AreEqual(100, tv.Size);
                Assert.AreEqual(900, tv.GetLong(0, 0));
                Assert.AreEqual(999, tv.GetLong(0, 99));
                tv.SetLong(0, 1, 42);
                tv.SetLong(1, 0, 13);
                Assert.AreEqual(42, t.GetLong(0, 901));
                Assert.AreEqual(13, t.GetLong(1, 900));
            }
        }

        /// <summary>
        /// Test that structure of table created is correct when taken out from a tableview
        /// </summary>
        [Test]
        public static void TableViewDumpView()
        {
            string actualres;

            using (var t = TableWithMultipleIntegers())
            {
                TableView tv = t.FindAllInt(1, 10);
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name, "find 10 integers in larger table",tv);
            }

            const string expectedres = @"------------------------------------------------------
Column count: 3
Table Name  : find 10 integers in larger table
------------------------------------------------------
 0        Int  intcolumn0          
 1        Int  intcolumn1          
 2        Int  intcolumn2          
------------------------------------------------------

Table Data Dump. Rows:10
------------------------------------------------------
{ //Start row 0
intcolumn0:100,//column 0
intcolumn1:10,//column 1
intcolumn2:1//column 2
} //End row 0
{ //Start row 1
intcolumn0:101,//column 0
intcolumn1:10,//column 1
intcolumn2:1//column 2
} //End row 1
{ //Start row 2
intcolumn0:102,//column 0
intcolumn1:10,//column 1
intcolumn2:1//column 2
} //End row 2
{ //Start row 3
intcolumn0:103,//column 0
intcolumn1:10,//column 1
intcolumn2:1//column 2
} //End row 3
{ //Start row 4
intcolumn0:104,//column 0
intcolumn1:10,//column 1
intcolumn2:1//column 2
} //End row 4
{ //Start row 5
intcolumn0:105,//column 0
intcolumn1:10,//column 1
intcolumn2:1//column 2
} //End row 5
{ //Start row 6
intcolumn0:106,//column 0
intcolumn1:10,//column 1
intcolumn2:1//column 2
} //End row 6
{ //Start row 7
intcolumn0:107,//column 0
intcolumn1:10,//column 1
intcolumn2:1//column 2
} //End row 7
{ //Start row 8
intcolumn0:108,//column 0
intcolumn1:10,//column 1
intcolumn2:1//column 2
} //End row 8
{ //Start row 9
intcolumn0:109,//column 0
intcolumn1:10,//column 1
intcolumn2:1//column 2
} //End row 9
------------------------------------------------------
";
            TestHelper.Cmp(expectedres, actualres);
        }


        /// <summary>
        /// Check that setting a double in a mixed field in a tableview works
        /// </summary>
        [Test]
        public static void TableViewAndTableTestMixedDouble()
        {
            const double testDouble = 12.2;
            using (var t = new Table(new MixedColumn("MixedField"), "stringfield".String()))
            {
                //get and set of a double in a mixed field (test type and value)
                t.AddEmptyRow(1);
                t.SetMixedDouble(0, 0, testDouble);
                t.SetString("stringfield", 0, "testdata"); //used for creation of tableview in next test
                DataType dt = t.GetMixedType(0, 0);
                Assert.AreEqual(DataType.Double, dt);
                double fromDb = t.GetMixedDouble(0, 0);
                Assert.AreEqual(testDouble, fromDb);

                const double testDouble2 = -12.2;
                //get and set of a double in a mixed in a tableview (test type and value).
                t.SetIndex(1);
                using (TableView tv = t.Distinct("stringfield"))
                {
                    Assert.AreEqual(1, tv.Size);
                    tv.SetMixedDouble(0, 0, testDouble2);
                    dt = tv.GetMixedType(0, 0);
                    Assert.AreEqual(DataType.Double, dt);
                    fromDb = tv.GetMixedDouble(0, 0);
                    Assert.AreEqual(testDouble2, fromDb);
                }
            }
        }

        /// <summary>
        /// set float in a mixed field in tableview and table
        /// </summary>
        [Test]
        public static void TableViewAndTableTestMixedFloat()
        {
            //performance test
            for (int n = 0; n < 10000; n++)
            {
                const float testFloat = -12.2f;
                using (var t = new Table(new MixedColumn("MixedField"), "stringfield".String()))
                {
                    //get and set of a double in a mixed field (test type and value)
                    t.AddEmptyRow(1);
                    t.SetMixedFloat(0, 0, testFloat);
                    t.SetString("stringfield", 0, "testdata"); //used for creation of tableview in next test
                    DataType dt = t.GetMixedType(0, 0);
                    Assert.AreEqual(DataType.Float, dt);
                    float fromDb = t.GetMixedFloat(0, 0);
                    Assert.AreEqual(testFloat, fromDb);

                    const float testFloat2 = -12.2f;
                    //get and set of a double in a mixed in a tableview (test type and value).
                    t.SetIndex(1);
                    using (TableView tv = t.Distinct("stringfield"))
                    {
                        Assert.AreEqual(1, tv.Size);
                        tv.SetMixedFloat(0, 0, testFloat2);
                        dt = tv.GetMixedType(0, 0);
                        Assert.AreEqual(DataType.Float, dt);
                        fromDb = tv.GetMixedFloat(0, 0);
                        Assert.AreEqual(testFloat2, fromDb);
                    }
                }
            }
        }



        /// <summary>
        /// set double in a mixed field table and tableview
        /// </summary>
        [Test]
        public static void TableViewAndTableTestDouble()
        {
            const string fn0 = "stringfield";
            const string fn1 = "doublefield1";
            const string fn2 = "doublefield2";

            using (var t = new Table(fn0.String(), fn1.Double(), fn2.Double()))
            {
                //first test the table gets the doubles right
                const double testdouble = -42.3;
                const double testdouble2 = 42.5;
                t.AddEmptyRow(1);
                t.SetString(fn0, 0, "teststring");
                t[0].SetDouble(fn1, testdouble);
                t.SetDouble(2, 0, testdouble2);
                Assert.AreEqual(testdouble, t.GetDouble(fn1, 0));
                Assert.AreEqual(testdouble, t.GetDouble(1, 0));
                Assert.AreEqual(testdouble2, t[0].GetDouble(fn2));
                Assert.AreEqual(testdouble2, t.GetDouble(2, 0));

                //then try once again, but with a tableview
                t.SetIndex(0);
                using (
                    TableView tv = t.Distinct(0))
                {
                    tv.SetString(fn0, 0, "teststring");
                    tv[0].SetDouble(fn1, testdouble);
                    tv.SetDouble(fn2, 0, testdouble2);
                    Assert.AreEqual(testdouble, tv.GetDouble(fn1, 0));
                    Assert.AreEqual(testdouble, tv.GetDouble(1, 0));
                    Assert.AreEqual(testdouble2, tv[0].GetDouble(fn2));
                    Assert.AreEqual(testdouble2, tv[0].GetDouble(2));
                }
            }
        }



        /// <summary>
        /// set get float in tableview and table
        /// </summary>
        [Test]
        public static void TableViewAndTableTestFloat()
        {
            const string fn0 = "stringfield";
            const string fn1 = "floatfield1";
            const string fn2 = "floatfield2";

            using (var t = new Table(fn0.String(), fn1.Float(), fn2.Float()))
            {
                //first test the table gets the doubles right
                const float testfloat = -42.3f;
                const float testfloat2 = 42.5f;
                t.AddEmptyRow(1);
                t.SetString(fn0, 0, "teststring");
                t[0].SetFloat(fn1, testfloat);
                t.SetFloat(2, 0, testfloat2);
                Assert.AreEqual(testfloat, t.GetFloat(fn1, 0));
                Assert.AreEqual(testfloat, t.GetFloat(1, 0));
                Assert.AreEqual(testfloat2, t[0].GetFloat(fn2));
                Assert.AreEqual(testfloat2, t.GetFloat(2, 0));

                //then try once again, but with a tableview
                t.SetIndex(0);
                using (TableView tv = t.Distinct(fn0))
                {
                    Assert.AreEqual(1, tv.Size);
                    tv.SetString(fn0, 0, "teststring");
                    tv.SetFloat(2, 0, testfloat2);
                    tv[0].SetFloat(fn1, testfloat);
                    Assert.AreEqual(testfloat, tv.GetFloat(fn1, 0));
                    Assert.AreEqual(testfloat, tv.GetFloat(1, 0));
                    Assert.AreEqual(testfloat2, tv[0].GetFloat(fn2));
                    Assert.AreEqual(testfloat2, tv[0].GetFloat(2));
                }
            }
        }


        /// <summary>
        /// set and get float in table and tableview
        /// </summary>
        [Test]
        public static void TableViewAndTableTestFloatSimple()
        {
            const string fn0 = "stringfield";
            const string fn1 = "floatfield1";

            using (var t = new Table(fn0.String(), fn1.Float()))
            {
                //first test the table gets the doubles right
                const float testfloat = -42.3f;
                t.AddEmptyRow(1);
                t.SetString(fn0, 0, "teststring");
                t.SetFloat(fn1, 0, testfloat);

                Assert.AreEqual(testfloat, t.GetFloat(fn1, 0));
                Assert.AreEqual(testfloat, t.GetFloat(1, 0));

                //then try once again, but with a tableview
                t.SetIndex(0);
                using (
                    TableView tv = t.Distinct(fn0))
                {
                    Assert.AreEqual(1, tv.Size);
                    tv.SetString(fn0, 0, "teststring!!");
                    tv.SetFloat(fn1, 0, testfloat);
                    Assert.AreEqual(testfloat, tv.GetFloat(fn1, 0));
                    Assert.AreEqual(testfloat, tv.GetFloat(1, 0));
                    Assert.AreEqual(1, t.Size);
                }
            }
        }

        
        /// <summary>
        /// Set a subtable in a tableview as a Table
        /// </summary>
        [Test]
        public static void TableViewSetSubTable()
        {
            using (var t = new Table(
                "do'h".Int(),
                "sub".SubTable(
                    "substringfield1".String(),
                    "substringfield2".String()
                    ),
                "mazda".Int()
                )
                )
            {
                using (var sub = new Table(
                    "substringfield1".String(),
                    "substringfield2".String()
                    )
                    )
                {
                    const string string00 = "stringvalueC0R0";
                    sub.Add(string00, "stringvalue2R0");
                    sub.Add("stringvalue1R1", "stringvalue2R1");
                    t.Add(42, sub, 43); //a row with the subtable in it
                    using (TableView view = t.FindAllInt(0, 42))
                    using (Table subreturned = view.GetSubTable(1, 0)) //testing getsubtable)
                    {
                        //tableview now has the one row with the subtable

                        Assert.AreEqual(string00, subreturned.GetString(0, 0));

                        const string changedString = "Changed";
                        sub.SetString(1, 1, changedString);
                        view.SetSubTable(1, 0, sub);

                        using (Table subreturnedchanged = view.GetSubTable(1, 0))
                        {                    
                            Assert.AreEqual(changedString, subreturnedchanged.GetString(1, 1));
                            //now, try to set the subtable values via a tableview
                        }
                    }
                }
            }
        }



        
        /// <summary>
        /// ensure that TableView ClearSubtable clears the subtable
        /// </summary>
        [Test]
        public static void TableViewClearSubTable()
        {
            using (var t = new Table(
                "do'h".Int(),
                "sub".SubTable(
                    "substringfield1".String(),
                    "substringfield2".String()
                    ),
                "mazda".Int()
                )
                )
            {
                using (var sub = new Table(
                    "substringfield1".String(),
                    "substringfield2".String()
                    )
                    )
                {
                    const string string00 = "stringvalueC0R0";
                    sub.Add(string00, "stringvalue2R0");
                    sub.Add("stringvalue1R1", "stringvalue2R1");
                    t.Add(42, sub, 43); //a row with the subtable in it
                    using (TableView view = t.FindAllInt(0, 42))
                    {
                        //tableview now has the one row with the subtable
                        Table subreturned = view.GetSubTable(1, 0); //testing getsubtable
                        Assert.AreEqual(string00, subreturned.GetString(0, 0));

                        const string changedString = "Changed";
                        sub.SetString(1, 1, changedString);
                        view.SetSubTable("sub", 0, sub);
                        using (Table subreturnedchanged = view.GetSubTable(1, 0))
                        {
                            Assert.AreEqual(changedString, subreturnedchanged.GetString(1, 1));

                            view.ClearSubTable(1, 0);

                            using (Table clearedSubReturned = view.GetSubTable(1, 0))
                            {
                                Assert.AreEqual(0, clearedSubReturned.Size);
                                //now, try to set the subtable values via a tableview
                            }
                        }
                    }
                }
            }
        }




        /// <summary>
        /// set binary in a mixed field in tableview and table
        /// </summary>
        [Test]
        public static void TableViewSetMixedBinary()
        {
            using (var table = new Table("matadormix".Mixed(),"int".Int()))
            {
                byte[] testArray = { 01, 12, 36, 22 };
                table.AddEmptyRow(1);
                table.AddEmptyRow(1);
                table.SetMixedBinary(0, 1, testArray);
                table.SetLong(1,1,42);

                TableView tableView = table.FindAllInt(1, 42);

                byte[] testReturned = tableView.GetMixedBinary(0, 0);
                Assert.AreEqual(4, testReturned.Length);
                Assert.AreEqual(1, testReturned[0]);
                Assert.AreEqual(12, testReturned[1]);
                Assert.AreEqual(36, testReturned[2]);
                Assert.AreEqual(22, testReturned[3]);
            }
        }



        /// <summary>
        /// ensure iterators iterate first row, last row and the one(s) in between
        /// </summary>
        [Test]
        public static void TableViewIterationTest()
        {
            using
                (
                var t = new Table("stringfield".String())
                )
            {
                t.AddEmptyRow(3);
                t.SetString(0, 0, "firstrow");
                t.SetString(0, 0, "secondrow");
                t.SetString(0, 0, "thirdrow");
                long n = 0;
                foreach (var row in t.Where().FindAll()) //loop through a tableview should get os Row classes
                {                    
                    Assert.AreEqual(n,row.RowIndex);//
                    n++;
                }
                Assert.AreEqual(n,t.Size);
            }
        }


        /// <summary>
        /// test that stacked tableviews return the correct rows
        /// also verify that enumerating a tableview returns Row objects where the rowix matches the tableview rownumbers (not the table rownumbers)
        /// </summary>
        [Test]
        public static void TableViewStackedViewsSimplified()
        {
            using (var table = new Table(new IntColumn("i1"), new IntColumn("i2"), new StringColumn("S1")))
            {
                table.Add(1, 2, "C");
                table.Add(2, 2, "I");
                using (var view = table.FindAllInt(0, 2))
                {
                    Assert.AreEqual(1,view.Size);
                    using (var view2 = view.FindAllInt(1, 2))
                    {
                        Assert.AreEqual("I", view2.GetString(2, 0));//fails here bc of a core bug
                    }
                }
            }
        }


        /// <summary>
        /// test that stacked tableviews return the correct rows
        /// also verify that enumerating a tableview returns Row objects where the rowix matches the tableview rownumbers (not the table rownumbers)
        /// </summary>
        [Test]
        public static void TableViewStackedViews()
        {
            using (var table = new Table(new IntColumn("i1"), new IntColumn("i2"), new StringColumn("S1")))
            {
                table.Add(1, 1, "A");
                table.Add(1, 1, "B");
                table.Add(1, 2, "C");
                table.Add(1, 2, "D");
                table.Add(1, 3, "E");
                table.Add(1, 3, "F");
                table.Add(2, 1, "G");
                table.Add(2, 1, "H");
                table.Add(2, 2, "I");
                table.Add(2, 2, "J");
                table.Add(2, 3, "K");
                table.Add(2, 3, "L");
                table.Add(3, 1, "M");
                table.Add(3, 1, "N");
                table.Add(3, 2, "O");
                table.Add(3, 2, "P");
                table.Add(3, 3, "Q");
                table.Add(3, 3, "R");
                using (var view = table.FindAllInt(0, 2))
                {
                    Assert.AreEqual(6,view.Size);//assert that the view returned the right records
                    Assert.AreEqual("G",view.GetString(2,0));
                    //check that the row indicies in the row cursor are relative to the view rows, not the table rows
                    var rno = 0;
                    foreach (var row in view)
                    {
                        Assert.AreEqual(rno,row.RowIndex);
                        Assert.AreEqual(2,row.GetLong("i1"));
                        rno++;
                    }
                    using (var view2 =  view.FindAllInt(1,2))
                    {
                        Assert.AreEqual(2, view2.Size);//assert that the view returned the right records
                        Assert.AreEqual("I", view2.GetString(2,0));
                        //check that the row indicies in the row cursor are relative to the view rows, not the table rows
                        rno = 0;
                        foreach (var row in view2)
                        {
                            Assert.AreEqual(rno, row.RowIndex);
                            Assert.AreEqual(2, row.GetLong("i2"));
                            rno++;
                        }                        
                    }
                }
            }
        }

        
        /// <summary>
        /// Part of test of iterator invalidation when a table used in a tableview gets rows added or deleted
        /// this iteration should work fine - no rows are shuffled around
        /// </summary>
        [Test]
        public static void TableViewIteratorInvalidation1Legal()
        {
            using (var table = new Table("intfield".Int(),"findfield".Int()))
            {                
                const int rows = 300;
                table.AddEmptyRow(rows);
                for (var f = 0; f < rows; f++)
                {
                    table.Set(f,f,0);//first field is 0 to rows, second field is always zero
                }
                
                using (var tableview=table.FindAllInt(1,0))//select all rows
                {
                    var cnt = tableview.Aggregate<Row, long>(0, (current, row) => current + row.GetLong(0));//using linq, which uses iterators
                    Assert.AreEqual(rows - 1, table.Last().GetLong(0));
                    Assert.AreEqual((rows/2)*(rows-1),cnt);
                }
            }
        }



        
        /// <summary>
        /// this iteration should fail.  a row is removed in the loop, earlier than where the iterator is
        /// this iteration should work fine - no rows are shuffled around
        /// 
        /// </summary>
        [Test]
        [ExpectedException("System.InvalidOperationException")]
        public static void TableViewIteratorInvalidation2TableAdd()
        {
            using (var table = new Table("intfield".Int(), "findfield".Int()))
            {
                const int rows = 300;
                //don't change rows to below 3, the rest of the code will not work well
                table.AddEmptyRow(rows);
                for (var f = 0; f < rows; f++)
                {
                    table.Set(f, f, 0);//first field is 0 to rows, second field is always zero
                }

                using (var tableview = table.FindAllInt(1, 0))//select all rows
                {
                    long cnt = 0;
                    foreach (var row in tableview){
                        cnt = cnt + row.GetLong(0);
                        if (row.RowIndex > rows/2)
                        {
                            table.Remove(row.RowIndex-2);//do not set rows to a very low number,or this will fail                             
                        }
                    }
                    Assert.AreEqual(rows - 1, table.Last().GetLong(0));//should never get this far
                }
            }
        }

        
        /// <summary>
        /// modification through a tableview is okay
        /// </summary>
        [Test]
        public static void TableViewIsValidLegal()
        {
            using (var table = new Table(new IntColumn("test")))
            {
                table.AddMany(new[]{1,2,2,3,2,4});
                using (var tableview = table.FindAllInt(0, 2))
                {
                    Assert.AreEqual(3, tableview.Size);
                    tableview.Remove(1);
                    Assert.AreEqual(2, tableview.Size);
                }
            }
        }


        /// <summary>
        /// tableview should invalidate itself if the underlying table changes number of rows
        /// </summary>
        [Test]
        [ExpectedException("System.InvalidOperationException")]
        public static void TableViewIsValidNotLegalThroughTable()
        {
            using (var table = new Table(new IntColumn("test")))
            {
                table.AddMany(new[] { 1, 2, 2, 3, 2, 4 });
                using (var tableview = table.FindAllInt(0, 2))
                {
                    Assert.AreEqual(3, tableview.Size);
                    table.Remove(1);
                    Assert.AreEqual(false, tableview.IsValid());
                    var size = tableview.Size; //this should throw
                    Assert.AreEqual(2, size); //this should not run
                }
            }
        }

        /// <summary>
        /// check that tableview gets invalidtaed if a table is changed in a object taken out form tablegroup 
        /// and  the view is from antoher instance of same table from same group
        /// </summary>
        [Test]
   [ExpectedException("System.InvalidOperationException")]
        public static void TableViewIsValidNotLegalThroughGroup()
        {
            using (var group = new  Group())
            using (var table =  group.CreateTable("T1",new IntColumn("test")))
            using (var table2 = group.GetTable("T1"))
            {
                table.AddMany(new List<long> {1, 2, 3, 4, 5, 6, 7, 8, 9, 10,2,2,2,2});//add many takes a collection of row values
                using (var tableview = table.FindAllInt(0, 2)){
                Assert.AreEqual(5, tableview.Size);
                table2.Remove(1);
                Assert.AreEqual(13,table.Size);
                Assert.AreEqual(false, tableview.IsValid());//this should return false
                long size = tableview.Size;//this should throw
                Assert.AreEqual(2, size);//this should not run
                }
            }
        }





        /// <summary>
        /// this iteration should fail.  a row is removed in the loop, earlier than where the iterator is
        /// this iteration should work fine - no rows are shuffled around
        /// </summary>
        [Test]
        [ExpectedException("System.InvalidOperationException")]
        public static void TableViewIteratorInvalidation3TableRemove()
        {
            using (var table = new Table("intfield".Int(), "findfield".Int()))
            {
                const int rows = 300;
                //don't change rows to below 3, the rest of the code will not work well
                table.AddEmptyRow(rows);
                for (var f = 0; f < rows; f++)
                {
                    table.Set(f, f, 0);//first field is 0 to rows, second field is always zero
                }

                using (var tableview = table.FindAllInt(1, 0))//select all rows
                {
                    long cnt = 0;
                    foreach (var row in tableview)
                    {
                        cnt = cnt + row.GetLong(0);
                        if (row.RowIndex > rows / 2)
                        {
                            table.Remove(row.RowIndex + 2);//do not set rows to a very low number,or this will fail                             
                        }
                    }
                    Assert.AreEqual(rows - 1, table.Last().GetLong(0));//should never get this far
                }
            }
        }




        /// <summary>
        ///this iteration should fail.  a row is removed in the loop, earlier than where the iterator is
        ///this iteration should fail.  a row is removed in the loop, earlier than where the iterator is 
        ///this iteration should work fine - no rows are shuffled around 
        /// </summary>
        [Test]
        [ExpectedException("System.InvalidOperationException")]
        public static void TableViewIteratorInvalidation4Insert()
        {
            using (var table = new Table("intfield".Int(), "findfield".Int()))
            {
                const int rows = 300;                
                //don't change rows to below 3, the rest of the code will not work well
                table.AddEmptyRow(rows);
                for (var f = 0; f < rows; f++)
                {
                    table.Set(f, f, 0);//first field is 0 to rows, second field is always zero
                }

                using (var tableview = table.FindAllInt(1, 0))//select all rows
                {
                    long cnt = 0;
                    foreach (var row in tableview)
                    {
                        cnt = cnt + row.GetLong(0);
                        if (row.RowIndex > rows / 2)
                        {
                            table.Insert(row.RowIndex - 2,42,42);//do not set rows to a very low number,or this will fail                             
                        }
                    }
                    Assert.AreEqual(rows - 1, table.Last().GetLong(0));//should never get this far
                }
            }
        }


        /// <summary>
        /// this iteration should fail.  a row is removed in the loop, earlier than where the iterator is
        /// this iteration should fail.  a row is removed in the loop, earlier than where the iterator is
        /// this iteration should work fine - no rows are shuffled around
        /// </summary>
        [Test]
        [ExpectedException("System.InvalidOperationException")]
        public static void TableViewIteratorInvalidation5Insert()
        {
            using (var table = new Table("intfield".Int(), "findfield".Int()))
            {
                const int rows = 300;
                //don't change rows to below 3, the rest of the code will not work well
                table.AddEmptyRow(rows);
                for (var f = 0; f < rows; f++)
                {
                    table.Set(f, f, 0);//first field is 0 to rows, second field is always zero
                }

                using (var tableview = table.FindAllInt(1, 0))//select all rows
                {
                    long cnt = 0;
                    foreach (var row in tableview)
                    {
                        cnt = cnt + row.GetLong(0);
                        if (row.RowIndex > rows / 2)
                        {
                            table.Insert(row.RowIndex + 2, 42, 42);//do not set rows to a very low number,or this will fail                             
                        }
                    }
                    Assert.AreEqual(rows - 1, table.Last().GetLong(0));//should never get this far
                }
            }
        }


        /// <summary>
        /// this iteration should fail.  a row is removed in the loop, earlier than where the iterator is
        /// this iteration should fail.  a row is removed in the loop, earlier than where the iterator is
        /// this iteration should work fine - no rows are shuffled around
        /// </summary>
        [Test]
        [ExpectedException("System.InvalidOperationException")]
        public static void TableViewIteratorInvalidation6AddEmptyRow()
        {
            using (var table = new Table("intfield".Int(), "findfield".Int()))
            {
                const int rows = 300;                
                //don't change rows to below 3, the rest of the code will not work well
                table.AddEmptyRow(rows);
                for (var f = 0; f < rows; f++)
                {
                    table.Set(f, f, 0);//first field is 0 to rows, second field is always zero
                }

                using (var tableview = table.FindAllInt(1, 0))//select all rows
                {
                    long cnt = 0;
                    foreach (var row in tableview)
                    {
                        cnt = cnt + row.GetLong(0);
                        if (row.RowIndex > rows / 2)
                        {
                            table.AddEmptyRow(1);
                        }
                    }
                    Assert.AreEqual(rows - 1, table.Last().GetLong(0));//should never get this far
                }
            }
        }

        /// <summary>
        /// this iteration should fail.  a row is removed in the loop, earlier than where the iterator is
        /// this iteration should fail.  a row is removed in the loop, earlier than where the iterator is
        /// this iteration should work fine - no rows are shuffled around        
        /// </summary>
        [Test]
        [ExpectedException("System.InvalidOperationException")]
        public static void TableViewIteratorInvalidation7RemoveLast()
        {
            using (var table = new Table("intfield".Int(), "findfield".Int()))
            {
                const int rows = 300;
                //don't change rows to below 3, the rest of the code will not work well
                table.AddEmptyRow(rows);
                for (var f = 0; f < rows; f++)
                {
                    table.Set(f, f, 0);//first field is 0 to rows, second field is always zero
                }

                using (var tableview = table.FindAllInt(1, 0))//select all rows
                {
                    long cnt = 0;
                    foreach (var row in tableview)
                    {
                        cnt = cnt + row.GetLong(0);
                        if (row.RowIndex > rows / 2)
                        {
                            table.RemoveLast();
                        }
                    }
                    Assert.AreEqual(rows - 1, table.Last().GetLong(0));//should never get this far
                }
            }
        }



        /// <summary>
        ///make sure tableview returns field values correctly
        /// </summary>
        [Test]

        public static void TableViewFindAllChangeValues()
        {
            using (var t = TableWithMultipleIntegers())
            {
                TableView tv = t.FindAllInt(2, 9);
                Assert.AreEqual(100, tv.Size);
                Assert.AreEqual(900, tv.GetLong(0, 0));
                Assert.AreEqual(999, tv.GetLong(0, 99));
            }
        }


        /// <summary>
        /// make sure tableview returns field values correctly
        /// </summary>
        [Test]        
        public static void TableViewLast()
        {
            using (var t = TableWithMultipleIntegers())
            {
                TableView tv = t.FindAllInt(2, 9);
                var row = tv.Last();
                Assert.AreEqual(999,row.GetLong(0));
                Assert.AreEqual(99, row.GetLong(1));
                Assert.AreEqual(9, row.GetLong(2));
            }
        }


    }
}
