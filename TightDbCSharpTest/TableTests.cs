using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using TightDbCSharp;
using TightDbCSharp.Extensions;




namespace TightDbCSharpTest
{
    /// <summary>
    /// Test Table Class So many tests they have been split up into two test fixtures, this is one of them
    /// What goes where is random
    /// </summary>
    [TestFixture]
    public static class TableTests1
    {
        /// <summary>
        /// test get column name
        /// </summary>
        [Test]
        public static void TableGetColumnName()
        {
            var testFieldNames = new List<string>
            {
                "fieldname",
                "",
                "1",
                "\\",
                "ÆØÅæøå",
                "0123456789abcdefghijklmnopqrstuvwxyz"
            };
            using (var testTable = new Table())
            {
                long n = 0;
                foreach (string str in testFieldNames)
                {
                    Assert.AreEqual(n, testTable.AddStringColumn(str));
                    Assert.AreEqual(str, testTable.GetColumnName(n++));
                }
            }
        }


        /// <summary>
        /// Test get column index
        /// </summary>
        [Test]
        public static void TableGetColumnIndex()
        {

            var testFieldNames = new List<string>
            {
                "fieldname",
                "",
                "1",
                "\\",
                "ÆØÅæøå",
                "0123456789abcdefghijklmnopqrstuvwxyz"
            };
            using (var testTable = new Table())
            {
                var n = 0;
                foreach (var str in testFieldNames)
                {
                    Assert.AreEqual(n, testTable.AddStringColumn(str));
                    Assert.AreEqual(n++, testTable.GetColumnIndex(str));
                }
            }
        }




        
        /// <summary>
        /// Right now this test uses creation of tables as a test - the column name will be set to all sorts of crazy thing, and we want them back that way
        /// </summary>
        [Test]
        public static void TableWithPerThousandSign()
        {
            String actualres;
            using (
                var notSpecifyingFields = new Table(
                    "subtable".Table()
                    )) //at this point we have created a table with no fields
            {
                notSpecifyingFields.AddStringColumn("12345‰7890");
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                    "table name is 12345 then the permille sign ISO 10646:8240 then 7890",
                    notSpecifyingFields);
            }
            const string expectedres = @"------------------------------------------------------
Column count: 2
Table Name  : table name is 12345 then the permille sign ISO 10646:8240 then 7890
------------------------------------------------------
 0      Table  subtable            
 1     String  12345‰7890          
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
        }
 

        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/system.datetime.kind.aspx    about datetime.kind
        /// http://msdn.microsoft.com/en-us/library/ms973825.aspx  old doc. with best practices from bf datetime.kind was introduced
        /// 
        /// Note that when You send just a date to tightdb, tightdb assumes it is system local time, and converts it to UTC before storing
        /// If You create your datetime with DateTimeKind.Utc then it will be saved without any modification
        /// The rules are :
        /// Tightdb always returns dates as DateTimeKind.Utc
        /// Tightdb stores dates set with DateTimeKind.Utc with no changes
        /// Tightdb changes dates set with DateTimeKind.Local to Utc before they are stored
        /// Tightdb assumes that dates where DateTimeKind.Unknown is set, are system local, and converts them to Utc
        /// This behavior ensures that dates are always stored as Utc, and that originaldate.ToUniversalTime() == originaldate back from database.ToUniversalTime()
        /// </summary>
        [Test]
        public static void DateTimeTest()
        {
            var myDateTime = new DateTime(1980, 1, 2);

            Int64 tightdbdate = Table.DebugToTightDbTime(myDateTime);
            DateTime returnedDateTime = Table.DebugToCSharpTimeUtc(tightdbdate); //1980,1,1,23:00, kind = UTC
            Assert.AreEqual(myDateTime.ToUniversalTime(), returnedDateTime.ToUniversalTime());
        }

        /// <summary>
        /// Test roundtripping a  DataTime of kind Unspecified        
        /// </summary>
        [Test]
        public static void DateTimeTestUnspecified()
        {
            var myDateTime = new DateTime(1980, 1, 2, 0, 0, 0, DateTimeKind.Unspecified);

            Int64 tightdbdate = Table.DebugToTightDbTime(myDateTime);
            DateTime returnedDateTime = Table.DebugToCSharpTimeUtc(tightdbdate); //1980,1,1,23:00, kind = UTC
            Assert.AreEqual(myDateTime.ToUniversalTime(), returnedDateTime.ToUniversalTime());
        }

        /// <summary>
        /// Test DateTimeKind.Local
        /// </summary>
        [Test]
        public static void DateTimeTestLocal()
        {
            var myDateTime = new DateTime(1980, 1, 2, 0, 0, 0, DateTimeKind.Local);


            Int64 tightdbdate = Table.DebugToTightDbTime(myDateTime);
            DateTime returnedDateTime = Table.DebugToCSharpTimeUtc(tightdbdate); //1980,1,1,23:00, kind = UTC
            Assert.AreEqual(myDateTime.ToUniversalTime(), returnedDateTime.ToUniversalTime());
        }

        /// <summary>
        /// Test DateTimeKind.Utc
        /// </summary>
        [Test]
        public static void DateTimeTestUtc()
        {
            var myDateTime = new DateTime(1980, 1, 2, 0, 0, 0, DateTimeKind.Utc);


            Int64 tightdbdate = Table.DebugToTightDbTime(myDateTime);
            DateTime returnedDateTime = Table.DebugToCSharpTimeUtc(tightdbdate);
            Assert.AreEqual(myDateTime, returnedDateTime); //1980,1,2,00:00, kind = UTC
        }





        /// <summary>
        /// Test that should fail if we use ansi somewhere in the roundtripping
        /// </summary>
        [Test]
        public static void TableWithNotAnsiCharacters()
        {
            String actualres;
            using (
                var notSpecifyingFields = new Table(
                    "subtable".Table()
                    )) //at this point we have created a table with no fields
            {
                notSpecifyingFields.AddStringColumn("123\u0300\u0301678");
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                    "column name is 123 then two non-ascii unicode chars then 678",
                    notSpecifyingFields);
            }
            const string expectedres = @"------------------------------------------------------
Column count: 2
Table Name  : column name is 123 then two non-ascii unicode chars then 678
------------------------------------------------------
 0      Table  subtable            
 1     String  123" + "\u0300\u0301" + @"678            
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
        }


        [SuppressMessage("Microsoft.Globalization",
            "CA1303:Do not pass literals as localized parameters",
            MessageId = "NUnit.Framework.Assert.AreEqual(System.Int64,System.Int64,System.String,System.Object[])"),
         SuppressMessage("Microsoft.Naming",
             "CA2204:Literals should be spelled correctly", MessageId = "InsertInt")]
        private static void CheckNumberInIntColumn(Table table, long columnNumber, long rowNumber, long testValue)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table"); //code analysis made me do this             
            }
            table.SetLong(columnNumber, rowNumber, testValue);
            var gotOut = table.GetLong(columnNumber, rowNumber);
            Assert.AreEqual(testValue, gotOut, "Table.InsertInt value mismatch sent{0} got{1}", testValue, gotOut);
        }


        //create a table of only integers, 3 columns.
        //with 42*42 in {0,0}, with long.minvalue in {1,1} and with long.minvalue+24 in {2,2}
        //the other fields have never been touched
        [SuppressMessage("Microsoft.Reliability",
            "CA2000:Dispose objects before losing scope"),
         SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        private static Table GetTableWithIntegers(bool subTable)
        {
            var t = new Table();
//            var s = t.Spec;
            t.AddIntColumn("IntColumn1");
            t.AddIntColumn("IntColumn2");
            t.AddIntColumn("IntColumn3");
            if (subTable)
            {
                var path = t.AddSubTableColumn("SubTableWithInts");
                t.AddIntColumn(path, "SubIntColumn1");
                t.AddIntColumn(path, "SubIntColumn2");
                t.AddIntColumn(path, "SubIntColumn3");
            }

            var rowindex = t.AddEmptyRow(1); //0
            long colummnIndex = 0;
            CheckNumberInIntColumn(t, colummnIndex, rowindex, 0);
            CheckNumberInIntColumn(t, colummnIndex, rowindex, -0);
            CheckNumberInIntColumn(t, colummnIndex, rowindex, 42*42);

            if (subTable)
            {
                Table sub = t.GetSubTable(3, rowindex);
                sub.AddEmptyRow(3);
                CheckNumberInIntColumn(sub, colummnIndex, rowindex, 2L);
            }

            colummnIndex = 1;
            rowindex = t.AddEmptyRow(1); //1
            CheckNumberInIntColumn(t, colummnIndex, rowindex, Int64.MaxValue);
            CheckNumberInIntColumn(t, colummnIndex, rowindex, Int64.MinValue);
            if (subTable)
            {
                Table sub = t.GetSubTable(3, rowindex);
                sub.AddEmptyRow(3);
                CheckNumberInIntColumn(sub, colummnIndex, rowindex, 2L);
            }



            colummnIndex = 2;
            rowindex = t.AddEmptyRow(1); //2
            CheckNumberInIntColumn(t, colummnIndex, rowindex, Int64.MaxValue - 42);
            CheckNumberInIntColumn(t, colummnIndex, rowindex, Int64.MinValue + 42);
            if (subTable)
            {
                Table sub = t.GetSubTable(3, rowindex);
                sub.AddEmptyRow(3);
                CheckNumberInIntColumn(sub, colummnIndex, rowindex, 2L);
            }
            return t;
        }       


        /// <summary>
        /// Test adding a new colum to a table with data in  it
        /// </summary>
        [Test]
        public static void TableAddColumnWithData()
        {
            using (var table = new Table(new IntColumn("Integers")) )
            {
                table.AddMany(new[] {1, 2, 3, 4, 5});
                table.AddStringColumn("NewColumn");//this is legal
                Assert.AreEqual(5,table.GetLong(0,4));
                Assert.AreEqual("", table.GetString(1, 4));//and we ought to have empty strings in there
            }
        }


        /// <summary>
        /// Test adding a subtable column and fields
        /// </summary>
        [Test]
        public static void TableAddColumnAndSpecTest()
        {
            using (
                var t = new Table())
            {

                t.AddIntColumn("IntColumn1");
                t.AddIntColumn("IntColumn2");
                t.AddIntColumn("IntColumn3");
                var subTbl = t.AddSubTableColumn("SubTbl");
                t.AddIntColumn(subTbl, "SubIntColumn1");
                t.AddIntColumn(subTbl, "SubIntColumn2");
                t.AddIntColumn(subTbl, "SubIntColumn3");


                t.AddEmptyRow(1);
                var sub = t.GetSubTable("SubTbl", 0);
                Assert.AreEqual(0, sub.Size); //remove compiler warning
                Assert.AreEqual("SubIntColumn3", sub.GetColumnName(2));
            }
        }


        /// <summary>
        /// Test rename column
        /// </summary>
        [Test]
        public static void TableRenameColumnTest()
        {
            using (var t = new Table()){
            
            t.AddStringColumn("Bent");
            t.RenameColumn(0,"Straight");
            Assert.AreEqual("Straight",t.GetColumnName(0));
            }
        }
       
        
        
        /// <summary>
        /// this test can safely be removed now.
        /// prior to path based,
        /// this test failed bc there is no way to check if calling updatefromspec is allowed
        /// specifically we cannot call c++ and get a column count that excludes the changes made to the spec
        /// a Workaround in Table has been made, that sets the internal property HasColumns to true when
        /// addcolumn or updatefromspec has been called. Updatefromspec will then fail if HasColumns is true
        /// more tests are being added that test for other spec operations that cannot be done on tables
        /// where columns already have been properly set and "comitted" with updatefromspec or addcolumn
        /// </summary>
        [Test]
        //[ExpectedException("System.InvalidOperationException")] //updatefromspec on a table with existing columns
        public static void TableAddColumnAndSpecTestSimple()
        {
            using (var t = new Table())
            {                
                Assert.AreEqual(0, t.AddIntColumn("IntColumn1")); //after this call, spec modifications are illegal
                t.AddSubTableColumn("SubTableWithInts");
                t.AddEmptyRow(1);
                Table sub = t.GetSubTable(1, 0);
                Assert.AreEqual(sub.Size, 0); //avoid compiler warning
            }
        }




        
        /// <summary>
        /// tests various cases of subtable array types, value and reference contents
        /// </summary>
        [Test]
        public static void TableAddIntArray()
        {
            using (var t = new Table())
            {
                t.AddIntColumn("IntColumn1");
                t.AddIntColumn("IntColumn2");
                t.AddIntColumn("IntColumn3");
                var path = t.AddSubTableColumn("SubTableWithInts");
                t.AddIntColumn(path, "SubIntColumn1");
                t.AddIntColumn(path, "SubIntColumn2");
                t.AddIntColumn(path, "SubIntColumn3");

                t.AddEmptyRow(1);
                var tr = t.Add(1, 2, 3, null);
                Assert.AreEqual(1, tr); //add should return the row being added to
                t.Add(3, 4, 5,
                    new object[] //compiler will make this an object array
                    {
                        new[] {4, 5, 6}, //compiler will make this an int array
                        new object[] {12, 13, 14} //and this is an object array with ints - both should work
                    });

                t.Add(3, 4, 5,
                    new[] //compiler will make this a pointer array!
                    {
                        new[] {14, 5, 6}, //compiler will make this an int array
                        new[] {12, 113, 14} //and this too
                    });
                Assert.AreEqual(0, t.GetSubTable("SubTableWithInts", 0).Size);

                Assert.AreEqual(1, t.GetLong("IntColumn1", 1));
                Assert.AreEqual(2, t.GetLong("IntColumn2", 1));
                Assert.AreEqual(3, t.GetLong("IntColumn3", 1));
                Assert.AreEqual(0, t.GetSubTable("SubTableWithInts", 1).Size);

                Assert.AreEqual(3, t.GetLong("IntColumn1", 2));
                Assert.AreEqual(4, t.GetLong("IntColumn2", 2));
                Assert.AreEqual(5, t.GetLong("IntColumn3", 2));
                using (var sub = t.GetSubTable("SubTableWithInts", 2))
                {
                    Assert.AreEqual(4, sub.GetLong("SubIntColumn1", 0));
                    Assert.AreEqual(5, sub.GetLong("SubIntColumn2", 0));
                    Assert.AreEqual(6, sub.GetLong("SubIntColumn3", 0));

                    Assert.AreEqual(12, sub.GetLong("SubIntColumn1", 1));
                    Assert.AreEqual(13, sub.GetLong("SubIntColumn2", 1));
                    Assert.AreEqual(14, sub.GetLong("SubIntColumn3", 1));
                }

                using (var sub = t.GetSubTable("SubTableWithInts", 3))
                {
                    Assert.AreEqual(14, sub.GetLong("SubIntColumn1", 0));
                    Assert.AreEqual(5, sub.GetLong("SubIntColumn2", 0));
                    Assert.AreEqual(6, sub.GetLong("SubIntColumn3", 0));

                    Assert.AreEqual(12, sub.GetLong("SubIntColumn1", 1));
                    Assert.AreEqual(113, sub.GetLong("SubIntColumn2", 1));
                    Assert.AreEqual(14, sub.GetLong("SubIntColumn3", 1));
                }
            }
        }


        /// <summary>
        /// Test that SetRow works
        /// </summary>
        [Test]
        public static void TableSetRowTest()
        {
            using (
                var table = new Table("intColumn".Int(), "intColumn2".Int(), "stringcolumn".String()))
            {
                table.AddEmptyRow(2);
                table[1].SetRow(12, 24, "test");
                Assert.AreEqual(12, table.GetLong(0, 1));
                Assert.AreEqual(24, table.GetLong(1, 1));
                Assert.AreEqual("test", table.GetString("stringcolumn", 1));
            }
        }

        /// <summary>
        ///utilize the collection initializer now we implement IEnumerator and Add 
        ///please note that this is not recommended as using does not work with collection initializers
        ///especially, using will not catch exceptions thrown in the calls to collection.Add
        ///see http://connect.microsoft.com/VisualStudio/feedback/details/654186/collection-initializers-called-on-collections-that-implement-idisposable-need-to-call-dispose-in-case-of-failure
        ///You will get a FXCOP CA200 error wtih this test, alerting You that the temporary variable that holds table
        ///after its constructor is called, but before the adds are finished, will not get disposed on errors        
        /// </summary>
        [Test]
        public static void TableCollectionInitializer()
        {

#if v40PLUS
            using (
                var oneliner = new Table(//FxCop Error is okay
                    new IntField("I1"),
                    new StringField("s1"),
                    new DoubleField("D1"))
                {
                    {1, "test", 12.4},
                    {4,"collection initializers are cool",42.1},
                    {17,"You can add as many rows as You want",12D},
                    {1,"as long as types and number of parametres match table structure",123.1}
                }){
#else
            using (
                var oneliner = new Table( //FxCop Error is okay
                    new IntColumn("I1"),
                    new StringColumn("s1"),
                    new DoubleColumn("D1"))
                )
            {
                oneliner.Add(1, "test", 12.4);
                oneliner.Add(4, "collection initializers are cool", 42.1);
                oneliner.Add(17, "You can add as many rows as You want", 12D);
                oneliner.Add(1, "as long as types and number of parametres match table structure", 123.1);
#endif

                foreach (var row in oneliner)
                {
                    row.SetString("s1", row.GetDouble("D1").ToString(CultureInfo.InvariantCulture));
                    //do some processing of each row
                }
            }
        }



        /// <summary>
        /// adding subtable as a constant array
        /// </summary>
        [Test]
        public static void TableAddSubTableStringArray()
        {
            using (
                var table = new Table(
                    new StringColumn("name"),
                    new IntColumn("age"),
                    new BoolColumn("hired"),
                    new SubTableColumn("phones", //sub table specification
                        new StringColumn("desc"),
                        new StringColumn("number")
                        )
                    ))
            {                                
                table.Add("Mary", 21, false, new[]
                {
                    new[] {"mobile", "232-323-3232"},
                    new[] {"work", "434-434-4343"}
                });
            }
        }

        /// <summary>
        /// test remove column in various situations
        /// </summary>
        [Test]
        public static void TableRemoveColumnTest()
        {
            using (var t = new Table())
            {
                t.AddIntColumn( "intcolumn");
                Assert.AreEqual(1, t.ColumnCount);
                t.RemoveColumn(0);//specifying by index
                Assert.AreEqual(0, t.ColumnCount);

                t.AddIntColumn( "c1");
                t.AddStringColumn("c2");
                t.AddDateColumn( "c3");

                Assert.AreEqual(DataType.Int, t.ColumnType(0));
                Assert.AreEqual(DataType.String, t.ColumnType(1));
                Assert.AreEqual(DataType.Date, t.ColumnType(2));

                t.RemoveColumn(new List<long> {1});//specifying by path
                Assert.AreEqual(DataType.Int, t.ColumnType(0));
                Assert.AreEqual(DataType.Date, t.ColumnType(1));

                var path = t.AddSubTableColumn("sub");

                Assert.AreEqual("sub",t.GetColumnName(2));
                t.AddIntColumn(path, "intsubfield1");
                t.AddIntColumn(path, "intsubfield2");                
                Assert.AreEqual("intsubfield1",t.Spec.GetSpec(path[0]).GetColumnName(0));
                
                path.Add(0);//so points to "sub" and then its column[0]
                t.RemoveColumn(path);//remove intsubfield1
                Assert.AreEqual("intsubfield2", t.Spec.GetSpec(path[0]).GetColumnName(0));
                path.RemoveAt(path.Count-1);//make path one shorter, to make it point to sub
                t.AddIntColumn(path, "intsubfield3");
                Assert.AreEqual("intsubfield3", t.Spec.GetSpec(path[0]).GetColumnName(1));
                t.RemoveColumn(path,0);
                Assert.AreEqual("intsubfield3", t.Spec.GetSpec(path[0]).GetColumnName(0));
            }
        }


        /// <summary>
        /// test table last
        /// </summary>
        [Test]
        public static void TableLast()
        {
            using (var t = new Table("intfeld".Int()))
            {
                t.AddMany(new[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10});
                Assert.AreEqual(11, t.Size);
                var n = 0;
                foreach (var tr in t)
                {
                    Assert.AreEqual(tr.RowIndex, n);
                    n++;
                }
                Assert.AreEqual(10, t.Last().RowIndex);
                Assert.AreEqual(10, t.Last().GetLong(0));
            }
        }



        /// <summary>
        /// Test that table iterator returns the correct class
        /// </summary>
        [Test]
        public static void TableIterationTest()
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
                foreach (var tableRow in t)
                {
                    var isTableRow = (typeof (TableRow) == tableRow.GetType());
                    Assert.AreEqual(true, isTableRow);
                    //assert important as Table's parent also implements an iterator that yields rows. We want TableRows when 
                    //we expicitly iterate a Table with foreach
                }
            }
        }


        private static void IterateTableOrView(TableOrView tov)
        {
            if (tov != null && !tov.IsEmpty)
                //the isempty test is just to trick ReSharper not to suggest tov be declared as Ienummerable<Row>
            {
                foreach (var row in (IEnumerable<Row>) tov)//cast neccesary bc TableOrView does not implement a generic iterator
                    //loop through a TableOrview should get os Row classes EVEN IF THE UNDERLYING IS A TABLE
                {
                    bool isRow = (row!=null);
                    Assert.AreEqual(true,isRow);
                    //we explicitly iterate a Table with foreach
                }
            }
        }



        /// <summary>
        /// Test that table and tableview findalldate works as they should
        /// </summary>
        [Test]
        public static void TableFindAllDateSuccessful()
        {
            using (var t = new Table("DateField1".Date(),"DateField2".Date(), "IntField".Int()))
            {
                var match = new DateTime(2000,01,03,17,42,42);
                var nomatch  = new DateTime(2001,01,03,17,42,42);
                t.Add(match, match,1);
                t.Add(match,nomatch, 2);
                t.Add(nomatch,match, 3);
                t.Add(nomatch, nomatch, 4);
                t.Add(match,nomatch, 5);
                t.Add(match, match, 6);
                using (var tv = t.FindAllDateTime(0, match))
                using (var tvs = t.FindAllDateTime("DateField1", match))
                {
                    Assert.AreEqual(1,tv[0].GetLong(2));
                    Assert.AreEqual(2,tv[1].GetLong(2));
                    Assert.AreEqual(5,tv[2].GetLong(2));
                    Assert.AreEqual(6, tv[3].GetLong(2));
                    Assert.AreEqual(4, tv.Size);
                    Assert.AreEqual(4, tvs.Size);

                    using (var tv2 = tv.FindAllDateTime(1, match))
                    {
                        Assert.AreEqual(1,tv2.GetLong(2,0));//first row with two matches
                        Assert.AreEqual(6, tv2.GetLong(2,1));//second row with two matches
                    }
                }
            }
        }

        /// <summary>
        /// test bad field type specification in findalldate
        /// </summary>
        [Test]
        [ExpectedException("System.ArgumentException")]
        public static void TableFindAllDateBadTypeString()
        {
            using (var t = new Table("DateField".Date(), "IntField".Int()))
            {
                var match = new DateTime(2000, 01, 03, 17, 42, 42);

                t.Add(match, 1);
                using (var tv = t.FindAllDateTime("IntField", match))//should throw
                {
                    Assert.AreEqual(1, tv[0].GetLong(1));
                }
            }
        }

        /// <summary>
        /// test bad field type specification in findalldate
        /// </summary>
        [Test]
        [ExpectedException("System.ArgumentException")]
        public static void TableFindAllDateBadType()
        {
            using (var t = new Table("DateField".Date(), "IntField".Int()))
            {
                var match = new DateTime(2000, 01, 03, 17, 42, 42);
               
                t.Add(match, 1);                
                using (var tv = t.FindAllDateTime(1, match))//should throw
                {
                    Assert.AreEqual(1, tv[0].GetLong(1));
                }
            }
        }



        /// <summary>
        /// Test FindallBool tv and table
        /// </summary>
        [Test]
        public static void TableFindAllBoolSuccessful()
        {
            using (var t = new Table("BoolField1".Bool(), "BoolField2".Bool(), "IntField".Int()))
            {
                const bool match = true;
                const bool nomatch = false;
                t.Add(match, match, 1);
                t.Add(match, nomatch, 2);
                t.Add(nomatch, match, 3);
                t.Add(nomatch, nomatch, 4);
                t.Add(match, nomatch, 5);
                t.Add(match, match, 6);
                using (var tv = t.FindAllBool(0, match))
                using (var tvs = t.FindAllBool("BoolField1", match))
                {
                    Assert.AreEqual(1, tv[0].GetLong(2));
                    Assert.AreEqual(2, tv[1].GetLong(2));
                    Assert.AreEqual(5, tv[2].GetLong(2));
                    Assert.AreEqual(6, tv[3].GetLong(2));
                    Assert.AreEqual(4, tv.Size);
                    Assert.AreEqual(4, tvs.Size);

                    using (var tv2 = tv.FindAllBool(1, match))
                    {
                        Assert.AreEqual(1, tv2.GetLong(2, 0));//first row with two matches
                        Assert.AreEqual(6, tv2.GetLong(2, 1));//second row with two matches
                    }
                }
            }
        }

        /// <summary>
        /// Test bad field specification, wrong type
        /// </summary>
        [ExpectedException("System.ArgumentException")]
        [Test]
        public static void TableFindAllBoolBadType()
        {
            using (var t = new Table("Field".Bool(), "IntField".Int()))
            {
                const bool  match =true;                
                t.Add(match, 1);
                using (var tv = t.FindAllBool(1, match))//should throw
                {
                    Assert.AreEqual(1, tv[0].GetLong(1));
                }
            }
        }


        /// <summary>
        /// Test bad field specification, wrong type
        /// </summary>

        [ExpectedException("System.ArgumentException")]
        [Test]
        public static void TableFindAllBoolBadType2()
        {
            using (var t = new Table("Field".Bool(), "IntField".Int()))
            {
                const bool match = true;
                t.Add(match, 1);
                using (var tv = t.FindAllBool("IntField", match))//should throw
                {
                    Assert.AreEqual(1, tv[0].GetLong(1));
                }
            }
        }






        /// <summary>
        /// Test FindallFloat tv and table
        /// </summary>

        [Test]
        public static void TableFindAllFloatSuccessful()
        {
            using (var t = new Table("Field1".Float(), "Field2".Float(), "IntField".Int()))
            {
                const float match = 42;
                const float nomatch = -42;
                t.Add(match, match, 1);
                t.Add(match, nomatch, 2);
                t.Add(nomatch, match, 3);
                t.Add(nomatch, nomatch, 4);
                t.Add(match, nomatch, 5);
                t.Add(match, match, 6);
                using (var tv = t.FindAllFloat(0, match))
                using (var tvs = t.FindAllFloat("Field1", match))
                {
                    Assert.AreEqual(1, tv[0].GetLong(2));
                    Assert.AreEqual(2, tv[1].GetLong(2));
                    Assert.AreEqual(5, tv[2].GetLong(2));
                    Assert.AreEqual(6, tv[3].GetLong(2));
                    Assert.AreEqual(4, tv.Size);
                    Assert.AreEqual(4, tvs.Size);

                    using (var tv2 = tv.FindAllFloat(1, match))
                    {
                        Assert.AreEqual(1, tv2.GetLong(2, 0));//first row with two matches
                        Assert.AreEqual(6, tv2.GetLong(2, 1));//second row with two matches
                    }
                }
            }
        }

        /// <summary>
        /// Test bad field specification, wrong type
        /// </summary>

        [ExpectedException("System.ArgumentException")]
        [Test]
        public static void TableFindAllFloatBadType()
        {
            using (var t = new Table("Field".Float(), "IntField".Int()))
            {
                const float match = 42;
                t.Add(match, 1);
                using (var tv = t.FindAllFloat(1, match))//should throw
                {
                    Assert.AreEqual(1, tv[0].GetLong(1));
                }
            }
        }


        /// <summary>
        /// Test bad field specification, wrong type
        /// </summary>
        [ExpectedException("System.ArgumentException")]
        [Test]
        public static void TableFindAllFloatBadType2()
        {
            using (var t = new Table("Field".Float(), "IntField".Int()))
            {
                const float match = 42;
                t.Add(match, 1);
                using (var tv = t.FindAllFloat("IntField", match))//should throw
                {
                    Assert.AreEqual(1, tv[0].GetLong(1));
                }
            }
        }









        /// <summary>
        /// Test FindallDouble tv and table
        /// </summary>

        [Test]
        public static void TableFindAllDoubleSuccessful()
        {
            using (var t = new Table("Field1".Double(), "Field2".Double(), "IntField".Int()))
            {
                const double match = 42;
                const double nomatch = -42;
                t.Add(match, match, 1);
                t.Add(match, nomatch, 2);
                t.Add(nomatch, match, 3);
                t.Add(nomatch, nomatch, 4);
                t.Add(match, nomatch, 5);
                t.Add(match, match, 6);
                using (var tv = t.FindAllDouble(0, match))
                using (var tvs = t.FindAllDouble("Field1", match))
                {
                    Assert.AreEqual(1, tv[0].GetLong(2));
                    Assert.AreEqual(2, tv[1].GetLong(2));
                    Assert.AreEqual(5, tv[2].GetLong(2));
                    Assert.AreEqual(6, tv[3].GetLong(2));
                    Assert.AreEqual(4, tv.Size);
                    Assert.AreEqual(4, tvs.Size);

                    using (var tv2 = tv.FindAllDouble(1, match))
                    {
                        Assert.AreEqual(1, tv2.GetLong(2, 0));//first row with two matches
                        Assert.AreEqual(6, tv2.GetLong(2, 1));//second row with two matches
                    }
                }
            }
        }

        /// <summary>
        /// Test bad field specification, wrong type
        /// </summary>
        [ExpectedException("System.ArgumentException")]
        [Test]
        public static void TableFindAllDoubleBadType()
        {
            using (var t = new Table("Field".Double(), "IntField".Int()))
            {
                const double match = 42;
                t.Add(match, 1);
                using (var tv = t.FindAllDouble(1, match))//should throw
                {
                    Assert.AreEqual(1, tv[0].GetDouble(1));
                }
            }
        }


        /// <summary>
        /// Test bad field specification, wrong type
        /// </summary>
        [ExpectedException("System.ArgumentException")]
        [Test]
        public static void TableFindAllDoubleBadType2()
        {
            using (var t = new Table("Field".Double(), "IntField".Int()))
            {
                const double match = 42;
                t.Add(match, 1);
                using (var tv = t.FindAllDouble("IntField", match))//should throw
                {
                    Assert.AreEqual(1, tv[0].GetDouble(1));
                }
            }
        }



        /// <summary>
        /// Test FindallInt tv and table
        /// </summary>

        [Test]
        public static void TableFindAllIntSuccessful()
        {
            using (var t = new Table("Field1".Int(), "Field2".Int(), "IntField".Int()))
            {
                const int match = 42;
                const int nomatch = -42;
                t.Add(match, match, 1);
                t.Add(match, nomatch, 2);
                t.Add(nomatch, match, 3);
                t.Add(nomatch, nomatch, 4);
                t.Add(match, nomatch, 5);
                t.Add(match, match, 6);
                using (var tv = t.FindAllInt(0, match))
                using (var tvs = t.FindAllInt("Field1", match))
                {
                    Assert.AreEqual(1, tv[0].GetLong(2));
                    Assert.AreEqual(2, tv[1].GetLong(2));
                    Assert.AreEqual(5, tv[2].GetLong(2));
                    Assert.AreEqual(6, tv[3].GetLong(2));
                    Assert.AreEqual(4, tv.Size);
                    Assert.AreEqual(4, tvs.Size);

                    using (var tv2 = tv.FindAllInt(1, match))
                    {
                        Assert.AreEqual(1, tv2.GetLong(2, 0));//first row with two matches
                        Assert.AreEqual(6, tv2.GetLong(2, 1));//second row with two matches
                    }
                }
            }
        }

        /// <summary>
        /// Test bad field specification, wrong type
        /// </summary>
        [ExpectedException("System.ArgumentException")]
        [Test]
        public static void TableFindAllIntBadType()
        {
            using (var t = new Table("Field".Int(), "SunField".Table()))
            {
                const int match = 42;
                t.Add(match,null);
                using (var tv = t.FindAllInt(1, match))//should throw
                {
                    Assert.AreEqual(1, tv[0].GetLong(1));
                }
            }
        }


        /// <summary>
        /// Test bad field specification, wrong type
        /// </summary>
        [ExpectedException("System.ArgumentException")]
        [Test]
        public static void TableFindAllIntBadType2()
        {
            using (var t = new Table("Field".Int(), "SubField".Table()))
            {
                const int match = 42;
                t.Add(match, null);
                using (var tv = t.FindAllInt("IntField", match))//should throw
                {
                    Assert.AreEqual(1, tv[0].GetLong(1));
                }
            }
        }

















        /// <summary>
        /// Test FindallString tv and table
        /// </summary>

        [Test]
        public static void TableFindAllStringSuccessful()
        {
            using (var t = new Table("Field1".String(), "Field2".String(), "IntField".Int()))
            {
                const String match = "42";
                const String nomatch = "-42";
                t.Add(match, match, 1);
                t.Add(match, nomatch, 2);
                t.Add(nomatch, match, 3);
                t.Add(nomatch, nomatch, 4);
                t.Add(match, nomatch, 5);
                t.Add(match, match, 6);
                using (var tv = t.FindAllString(0, match))
                using (var tvs = t.FindAllString("Field1", match))
                {
                    Assert.AreEqual(1, tv[0].GetLong(2));
                    Assert.AreEqual(2, tv[1].GetLong(2));
                    Assert.AreEqual(5, tv[2].GetLong(2));
                    Assert.AreEqual(6, tv[3].GetLong(2));
                    Assert.AreEqual(4, tv.Size);
                    Assert.AreEqual(4, tvs.Size);

                    using (var tv2 = tv.FindAllString(1, match))
                    {
                        Assert.AreEqual(1, tv2.GetLong(2, 0));//first row with two matches
                        Assert.AreEqual(6, tv2.GetLong(2, 1));//second row with two matches
                    }
                }
            }
        }

        /// <summary>
        /// Test bad field specification, wrong type
        /// </summary>
        [ExpectedException("System.ArgumentException")]
        [Test]
        public static void TableFindAllStringBadType()
        {
            using (var t = new Table("Field".String(), "IntField".Int()))
            {
                const String match = "42";
                t.Add(match, 1);
                using (var tv = t.FindAllString(1, match))//should throw bc column dataType is DataType.Int
                {
                    Assert.AreEqual(1, tv[0].GetLong(1));
                }
            }
        }

        /// <summary>
        /// Test bad field specification, wrong type
        /// </summary>
        [ExpectedException("System.ArgumentException")]
        [Test]
        public static void TableFindAllStringBadType2()
        {
            using (var t = new Table("Field".String(), "IntField".Int()))
            {
                const String match = "42";
                t.Add(match, 1);
                using (var tv = t.FindAllString("IntField", match))//should throw bc column dataType is DataType.Int
                {
                    Assert.AreEqual(1, tv[0].GetLong(1));
                }
            }
        }


        /// <summary>
        /// Test FindAllbinary view and table
        /// </summary>
        [Test]
        public static void TableFindAllBinarySuccessful()
        {
            using (var t = new Table("Field1".Binary(), "Field2".Binary(), "IntField".Int()))
            {
                var match = new Byte[] {42,42,42,42,66};
                var nomatch = new Byte[] { 42, 42, 43, 42, 66 };
                t.Add(match, match, 1);
                t.Add(match, nomatch, 2);
                t.Add(nomatch, match, 3);
                t.Add(nomatch, nomatch, 4);
                t.Add(match, nomatch, 5);
                t.Add(match, match, 6);
                using (var tv = t.FindAllBinary(0, match))
                using (var tvs = t.FindAllBinary("Field1", match))
                {
                    Assert.AreEqual(1, tv[0].GetLong(2));
                    Assert.AreEqual(2, tv[1].GetLong(2));
                    Assert.AreEqual(5, tv[2].GetLong(2));
                    Assert.AreEqual(6, tv[3].GetLong(2));
                    Assert.AreEqual(4, tv.Size);
                    Assert.AreEqual(4, tvs.Size);

                    using (var tv2 = tv.FindAllBinary(1, match))
                    {
                        Assert.AreEqual(1, tv2.GetLong(2, 0));//first row with two matches
                        Assert.AreEqual(6, tv2.GetLong(2, 1));//second row with two matches
                    }
                }
            }
        }

        /// <summary>
        /// Test bad field specification, wrong type
        /// </summary>
        [ExpectedException("System.ArgumentException")]
        [Test]
        public static void TableFindAllBinaryBadType()
        {
            using (var t = new Table("Field".Binary(), "IntField".Int()))
            {
                var match = new Byte []{42};
                t.Add(match, 1);
                using (var tv = t.FindAllBinary(1, match))//should throw bc column dataType is DataType.Int
                {
                    Assert.AreEqual(1, tv[0].GetLong(1));
                }
            }
        }

        /// <summary>
        /// Test bad field specification, wrong type
        /// </summary>
        [ExpectedException("System.ArgumentException")]
        [Test]
        public static void TableFindAllBinaryBadType2()
        {
            using (var t = new Table("Field".Binary(), "IntField".Int()))
            {
                var match = new Byte[] { 42 };
                t.Add(match, 1);
                using (var tv = t.FindAllBinary("IntField", match))//should throw bc column dataType is DataType.Int
                {
                    Assert.AreEqual(1, tv[0].GetLong(1));
                }
            }
        }



        /// <summary>
        /// test table and view iteration
        /// </summary>
        [Test]
        public static void TableOrViewIterationTest()
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
                IterateTableOrView(t);
                IterateTableOrView(t.Where().FindAll());
            }
        }


        //test case that exposes a table iterator that should be invalidated because the table had 
        //insert, delete or sort operations performed on it.
        //list of operations that should invalidate all iterators :

        //table.insert OK
        //table.remove OK
        //table.addemptyrow OK
        //table.clear - doesn't exist in binding
        //table.removelast

        //todo: besides checking if iterators fail, also check that using row objects fail if their table has been changed
        //some unit tests have been bmade, but expand to rows gotten from tableview, and query

        //todo: repeat this unit test for queries (the query itself should also be versioned as operations exist that change the query)
        //query can be invalidated by itself being changed, or by the underlying table being changed. Not sure if we can query a tableview

        //on the query or table object, but also modifications that might be done on the underlying table.
        //perhaps the iterator should also take a look at isattached(isvalid)

        
        /// <summary>
        /// this iteration should work fine - no rows are shuffled around
        /// </summary>
        [Test]
        public static void TableIteratorInvalidation1Legal()
        {
            using (var table = new Table("intfield".Int()))
            {
                const int rows = 300;
                table.AddEmptyRow(rows);
                var n = 0;
                foreach (var tableRow in table)
                {                    
                    tableRow.SetInt(0,n);
                    ++n;
                }
                Assert.AreEqual(rows-1,table.Last().GetLong(0));
            }
        }



        
        /// <summary>
        /// this iteration should fail.  a row is removed in the loop, earlier than where the iterator is
        /// </summary>
        [Test]
        [ExpectedException("System.InvalidOperationException")]
        public static void TableIteratorInvalidation2Delete()
        {
            using (var table = new Table("intfield".Int()))
            {
                const int rows = 300;
                table.AddEmptyRow(rows);
                var n = 0;
                foreach (var tableRow in table)
                {
                    tableRow.SetInt(0, n);
                    ++n;
                    if (n == 50)
                    {
                        table.Remove(48);
                    }
                }
                Assert.AreEqual(rows-1, table.Last().GetLong(0));//this should never execute
            }
        }


        
        /// <summary>
        /// this iteration should fail.  a row is removed in the loop, later than where the iterator is
        /// </summary>
        [Test]
        [ExpectedException("System.InvalidOperationException")]
        public static void TableIteratorInvalidation3Delete()
        {
            using (var table = new Table("intfield".Int()))
            {
                const int rows = 300;
                table.AddEmptyRow(rows);
                var n = 0;
                foreach (var tableRow in table)
                {                    
                    tableRow.SetInt("intfield", n);
                    ++n;                 
                    if (n == 50)
                    {
                        table.Remove(52);
                    }
                }
                Assert.AreEqual(rows-1, table.Last().GetLong(0));
            }
        }




        
        /// <summary>
        /// this iteration should fail.  a row is removed in the loop, earlier than where the iterator is
        /// </summary>
        [Test]
        [ExpectedException("System.InvalidOperationException")]
        public static void TableIteratorInvalidation4Insert()
        {
            using (var table = new Table("intfield".Int()))
            {
                const int rows = 300;
                table.AddEmptyRow(rows);
                var n = 0;
                foreach (var tableRow in table)
                {                    
                    tableRow.SetInt(0, n);
                    ++n;
                    if (n == 50)
                    {
                        table.Insert(48,314);
                    }
                }
                Assert.AreEqual(rows-1, table.Last().GetLong(0));
            }
        }


        
        /// <summary>
        /// this iteration should fail.  a row is inserted in the loop, later than where the iterator is
        /// </summary>
        [Test]
        [ExpectedException("System.InvalidOperationException")]
        public static void TableIteratorInvalidation5Insert()
        {
            using (var table = new Table("intfield".Int()))
            {
                const int rows = 300;
                table.AddEmptyRow(rows);
                var n = 0;
                foreach (var tableRow in table)
                {                    
                    tableRow.SetInt(0, n);
                    ++n;
                    if (n == 50)
                    {
                        table.Insert(52,314);
                    }
                }
                Assert.AreEqual(rows-1, table.Last().GetLong(0));
            }
        }


        
        /// <summary>
        /// this iteration should fail.  a row is inserted in the loop, later than where the iterator is
        /// </summary>
        [Test]
        [ExpectedException("System.InvalidOperationException")]
        public static void TableIteratorInvalidation6AddEmptyRow()
        {
            using (var table = new Table("intfield".Int()))
            {
                const int rows = 300;
                table.AddEmptyRow(rows);
                long n = 0;
                foreach (var tableRow in table)
                {                    
                    tableRow.SetLong(0, n);
                    n++;
                    if (n == 50)
                    {
                        table.AddEmptyRow(1);//renders iterator illegal
                    }
                }
                Assert.AreEqual(rows-1, table.Last().GetLong(0));
            }
        }

        
        /// <summary>
        /// this iteration should fail.  a row is inserted in the loop, later than where the iterator is
        /// </summary>
        [Test]
        [ExpectedException("System.InvalidOperationException")]
        public static void TableIteratorInvalidation7Clear()
        {
            using (var table = new Table("intfield".Int()))
            {
                const int rows = 300;
                table.AddEmptyRow(rows);
                long n = 0;
                foreach (var tableRow in table)
                {                    
                    tableRow.SetLong(0, n);
                    n++;
                    if (n == 50)
                    {
                        table.RemoveLast();//renders iterator illegal
                    }
                }
                Assert.AreEqual(rows-1, table.Last().GetLong(0));//this will fail 100<>99, but we should never get this far.
            }
        }




        /// <summary>
        /// test tableisvalid
        /// </summary>
        [Test]
        public static void TableIsValidTest()
        {
            using (var t = new Table())
            {
                Assert.AreEqual(true, t.IsValid());
                t.AddIntColumn( "do'h");
                Assert.AreEqual(true, t.IsValid());
                using (var sub = new Table())
                {
                    t.AddSubTableColumn( "sub");
                    t.Add(42, sub);
                    Assert.AreEqual(true, sub.IsValid());
                    t.Set(0, 43, null);
                    Table sub2 = t.GetSubTable(1, 0);
                    Assert.AreEqual(true, sub2.IsValid());
                    Assert.AreEqual(true, sub.IsValid());
                    t.Add(42, sub);
                    Table sub3 = t.GetSubTable(1, 1);
                    t.Set(1, 45, null);
                    Assert.AreEqual(false, sub3.IsValid());
                    t.Set(1, 45, sub);
                    Assert.AreEqual(false, sub3.IsValid());
                }
            }
        }

        /// <summary>
        /// test removelast
        /// </summary>
        [Test]
        public static void TableRemoveLast()
        {
            using (var table = new Table(new IntColumn("intcolumn")))
            {
                table.AddEmptyRow(100);
                long size = table.Size;
                Assert.AreEqual(100,size);
                table.RemoveLast();
                Assert.AreEqual(99,table.Size);//did we in fact remove a row?
                table.AddEmptyRow(1);
                Assert.AreEqual(100, table.Size);//ensure a row was added
                Assert.AreEqual(0, table.GetLong(0, 99));//newly added row sb value 0
                table.SetInt("intcolumn",99,42);//change it to value 42
                Assert.AreEqual(42,table[99].GetLong(0));//ensure it is in fact 42
                table.RemoveLast();
                Assert.AreEqual(99, table.Size);//did the row get deleted
                Assert.AreEqual(0, table[98].GetLong(0));//top row  should now be 0, the 42 should have been deleted
            }
        }

        /// <summary>
        /// Test hassharedspec
        /// </summary>
        [Test]
        public static void TableSharedSpecTest()
        {
            //create a table1 with a subtable1 column in it, with an int in it. The subtable with an int will 
            //have a shared spec, as subtable1 spec is part of the table1 and thus tableSharedSpec should return true

            using (var table1 = new Table("subtable".Table(
                "int".Int()
                )
                )
                )
            {
                Assert.AreEqual(false, table1.HasSharedSpec());
                table1.AddEmptyRow(1); //add an empty subtalbe to the first column in the table

                //table1.ClearSubTable(0, 0);//somehow i think this is not legal? similarily putting in a subtable that does not match the spec of the master table

                using (Table sub = table1.GetSubTable(0, 0))
                {
                    sub.Add(42); //add row with the int 42 in it
                    Assert.AreEqual(true, sub.HasSharedSpec());
                }
            }
        }


        
        /// <summary>
        /// Ensure that altering spec on a subtable is illegal
        /// test if we allow chaning the spec of a non-mixed subtable (should be illegal)       
        /// </summary>
        [Test]
        [ExpectedException("System.InvalidOperationException")]
        public static void TableSharedSpecChangeTest()
        {
            //create a table1 with a subtable1 column in it, with an int in it. The subtable with an int will 
            //have a shared spec, as subtable1 spec is part of the table1 and thus tableSharedSpec should return true

            using (var table1 = new Table("subtable".Table(
                "int".Int()
                )
                )
                )
            {
                Assert.AreEqual(false, table1.HasSharedSpec());
                table1.AddEmptyRow(1); //add an empty subtalbe to the first column in the table
                using (Table sub = table1.GetSubTable(0, 0))
                {
                    sub.AddIntColumn( "intcolumn");
                    //this is illegal and should throw an exception                    
                } //the control mechanism sb that the spec for sub is readonly, or that addcolumn will not work
            } //on a table that is a non-root subtable
        }


        
        /// <summary>
        /// test that we allow changing the spec of a mixed subtable (should be legal)
        /// </summary>
        [Test]
        public static void TableMixedSpecChangeTest()
        {
            //create a table1 with a subtable1 column in it, with an int in it. The subtable with an int will 
            //have a shared spec, as subtable1 spec is part of the table1 and thus tableSharedSpec should return true

            using (var table1 = new Table("mixedfield".Mixed()))
            {
                Assert.AreEqual(false, table1.HasSharedSpec());
                table1.AddEmptyRow(1);
                table1.SetMixedEmptySubTable(0, 0); //create an empty subtable in the mixed row
                using (Table sub = table1.GetMixedSubTable(0, 0))
                {
                    sub.AddIntColumn( "intcolumn");
                    //this is legal as the table in the mixed is its own root
                }
            }
        }




        /// <summary>
        ///old spec test
        ///test that we block changing the spec of a mixed subtable with updatefromspec
        ///if it already have comitted columns and if the columns were added before we read
        ///the table in from the subspec
        /// 
        /// </summary>
        [Test]
        public static void TableMixedSpecChangeAfterGet()
        {
            using (var table1 = new Table("mixedfield".Mixed()))
            {
                Assert.AreEqual(false, table1.HasSharedSpec());
                table1.AddEmptyRow(1);
                table1.SetMixedEmptySubTable(0, 0); //create an empty subtable in the mixed row
                using (Table sub = table1.GetMixedSubTable(0, 0))
                {
                    sub.AddIntColumn( "intcolumn");
                    //this is legal as the table in the mixed is its own root
                 }
                using (Table sub = table1.GetMixedSubTable(0, 0))
                {
                    Assert.AreEqual(false, sub.HasSharedSpec());
                }
            }
        }



        /// <summary>
        /// test with the newest kind of field object constructores - lasse's inherited specialized ones
        /// </summary>
        [Test]
        public static void TableSubTableSubTable()
        {
            string actualres;
            using (var t = new Table(
                "root".SubTable(
                    "s1".SubTable(
                        "s2".Table(
                            "fld".Int())))))
            {
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                    "table with subtable with subtable",
                    t);
            }
            const string expectedres = @"------------------------------------------------------
Column count: 1
Table Name  : table with subtable with subtable
------------------------------------------------------
 0      Table  root                
    0      Table  s1                  
       0      Table  s2                  
          0        Int  fld                 
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
        }




        /// <summary>
        /// Test Table creation using typed constructor helper classes
        /// </summary>
        [Test]
        public static void TypedFieldClasses()
        {
            String actualres;
            using (
                var newFieldClasses = new Table(
                    new StringColumn("F1"),
                    new IntColumn("F2"),
                    new SubTableColumn("Sub1",
                        new StringColumn("F11"),
                        new IntColumn("F12"))
                    ))
            {
                newFieldClasses.AddStringColumn("Buksestørrelse");

                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                    "table created with all types using the new field classes",
                    newFieldClasses);
            }
            const string expectedres = @"------------------------------------------------------
Column count: 4
Table Name  : table created with all types using the new field classes
------------------------------------------------------
 0     String  F1                  
 1        Int  F2                  
 2      Table  Sub1                
    0     String  F11                 
    1        Int  F12                 
 3     String  Buksestørrelse      
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
        }


        /// <summary>
        /// test that adding a subtable is working
        /// </summary>
        [Test]
        public static void TableAddSubTableAsNull()
        {
            using (var table = new Table(new SubTableColumn("sub")))
            {
                table.Add(null);
            }

            using (var table = new Table(new IntColumn("test"),new SubTableColumn("sub")))
            {
                table.Add(12,null);
            }
        }


        /// <summary>
        /// Test table clone.
        /// We once had a problem with some fieldnames
        /// </summary>
        [Test]
        public static void TableCloneLostFieldNameTest()
        {
            const string fnsub =
                "sub";
            const string fnsubsub = "subsub";
            String actualres = "";
            using (var smallTable = new Table(fnsub.Table(fnsubsub.Table())))
            {
                using (var tempSubTable = new Table(fnsubsub.Table()))
                {
                    smallTable.Add(tempSubTable);
                }
                //okay that tempsubtable is disposed here, as adding subtables is done by copying their structure and value
                Assert.AreEqual(fnsub, smallTable.GetColumnName(0));
                Assert.AreEqual(fnsubsub, smallTable.GetSubTable(0, 0).GetColumnName(0));
                Spec spec1 = smallTable.Spec;
                Assert.AreEqual(fnsub, spec1.GetColumnName(0));
                Spec spec2 = spec1.GetSpec(0);
                Assert.AreEqual(fnsubsub, spec2.GetColumnName(0));

                var clonedTable = smallTable.Clone();
                if (clonedTable != null)
                {
                    Assert.AreEqual(fnsub, clonedTable.GetColumnName(0));
                    Assert.AreEqual(fnsubsub, clonedTable.GetSubTable(0, 0).GetColumnName(0));
                    Spec spec1S = smallTable.Spec;
                    Assert.AreEqual(fnsub, spec1S.GetColumnName(0));
                    Spec spec2S = spec1S.GetSpec(0);
                    Assert.AreEqual(fnsubsub, spec2S.GetColumnName(0));


                    actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                        "tableclone subsub fieldnames test",
                        smallTable.Clone());



                }
                else
                {
                    {
                        Assert.AreEqual("clonedTable was null", "it should have contained data");
                    }
                }

            }
            const string expectedres = @"------------------------------------------------------
Column count: 1
Table Name  : tableclone subsub fieldnames test
------------------------------------------------------
 0      Table  sub                 
    0      Table  subsub              
------------------------------------------------------

Table Data Dump. Rows:1
------------------------------------------------------
{ //Start row 0
sub:[ //0 rows]//column 0
} //End row 0
------------------------------------------------------
";
            TestHelper.Cmp(expectedres, actualres);
        }



        /// <summary>
        /// Table clone test
        /// </summary>
        [Test]
        public static void TableCloneTest4()
        {
            String actualres;
            using (
                var newFieldClasses = new Table(
                    //new StringField("F1"),
                    //new IntField("F2"),
                    new SubTableColumn("Sub1" //),
                        //                      new StringField("F11"),
                        //                      new IntField("F12"))
                        )))
            {
                newFieldClasses.AddStringColumn("Buksestørrelse");

                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                    "table created with all types using the new field classes",
                    newFieldClasses.Clone());
            }
            const string expectedres = @"------------------------------------------------------
Column count: 2
Table Name  : table created with all types using the new field classes
------------------------------------------------------
 0      Table  Sub1                
 1     String  Buksestørrelse      
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
        }




        /// <summary>
        /// Table clone test
        /// </summary>
        [Test]
        public static void TableCloneTest3()
        {
            String actualres;
            using (
                var newFieldClasses = new Table(
                    //new StringField("F1"),
                    //new IntField("F2"),
                    //    new SubTableField("Sub1",
                    //                      new StringField("F11"),
                    //                      new IntField("F12"))
                    ))
            {
                newFieldClasses.AddStringColumn( "Buksestørrelse");


                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                    "table created with all types using the new field classes",
                    newFieldClasses.Clone());
            }
            const string expectedres = @"------------------------------------------------------
Column count: 1
Table Name  : table created with all types using the new field classes
------------------------------------------------------
 0     String  Buksestørrelse      
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
        }




        /// <summary>
        /// Table clone test
        /// </summary>
        [Test]
        public static void TableCloneTest2()
        {
            String actualres;
            using (
                var newFieldClasses = new Table(
                    new SubTableColumn("Sub1",
                        new StringColumn("F11"),
                        new IntColumn("F12"))
                    ))
            {
                newFieldClasses.AddStringColumn("Buksestørrelse");


                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                    "table created with all types using the new field classes",
                    newFieldClasses.Clone());
            }
            const string expectedres = @"------------------------------------------------------
Column count: 2
Table Name  : table created with all types using the new field classes
------------------------------------------------------
 0      Table  Sub1                
    0     String  F11                 
    1        Int  F12                 
 1     String  Buksestørrelse      
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
        }


        /// <summary>
        /// Table clone test
        /// </summary>
        [Test]
        public static void TableCloneTest()
        {
            String actualres;
            using (
                var newFieldClasses = new Table(
                    new StringColumn("F1"),
                    new IntColumn("F2"),
                    new SubTableColumn("Sub1",
                        new StringColumn("F11"),
                        new IntColumn("F12"))
                    ))
            {
                newFieldClasses.AddStringColumn("Buksestørrelse");


                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                    "table created with all types using the new field classes",
                    newFieldClasses.Clone());
            }
            const string expectedres = @"------------------------------------------------------
Column count: 4
Table Name  : table created with all types using the new field classes
------------------------------------------------------
 0     String  F1                  
 1        Int  F2                  
 2      Table  Sub1                
    0     String  F11                 
    1        Int  F12                 
 3     String  Buksestørrelse      
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
        }


        //illustration of field usage, usecase / unit test

        //The user can decide to create his own field types, that could then be used in several different table definitions, to ensure 
        //that certain kinds of fields used by common business logic always were of the correct type and setup
        //For example a field called itemcode that currently hold integers to denote owned item codes in a game,
        //but perhaps later should be a string field instead
        //if you have many IntegerField fields in many tables with item codes in them, you could use Itemcode instead, and then effect the change to string
        //only by changing the ineritance of the Itemcode type from IntegerField to StringField
        //thus by introducing your own class, You hide the field implementation detail from the users using this field type


        private class ItemCode : IntColumn
            //whenever ItemCode is specified in a table definition, an IntegerField is created
        {
            public ItemCode(String columnName)
                : base(columnName)
            {
            }
        }

        //because of a defense against circular field references, the subtablefield cannot be used this way, however you can make a method that returns an often
        //used subtable specification like this instead :

        //subtable field set used by our general login processing system
        private static SubTableColumn OwnedItems()
        {
            return new SubTableColumn(
                ("OwnedItems"),
                new StringColumn("Item Name"),
                new ItemCode("ItemId"),
                new IntColumn("Number Owned"),
                new BoolColumn("ItemPowerLevel"));
        }

        //game state dataset used by our general game saving system for casual games
        private static SubTableColumn GameSaveFields()
        {
            return new SubTableColumn(
                ("GameState"),
                new StringColumn("SaveDate"),
                new IntColumn("UserId"),
                new StringColumn("Users description"),
                new BinaryColumn("GameData1"),
                new StringColumn("GameData2"));
        }


        
        /// <summary>
        /// creation of table using user overridden or generated fields (ensuring same subtable structure across applications or tables)
        /// </summary>
        [Test]
        public static void UserCreatedFields()
        {
            String actualres;

            using (
                var game1 = new Table(
                    OwnedItems(),
                    new IntColumn("UserId"),
                    //some game specific stuff. All players are owned by some item, don't ask me why
                    new BinaryColumn("BoardLayout"), //game specific
                    GameSaveFields())
                )
            {
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name + "1",
                    "table created user defined types and methods", game1);
            }
            string expectedres =
                @"------------------------------------------------------
Column count: 4
Table Name  : table created user defined types and methods
------------------------------------------------------
 0      Table  OwnedItems          
    0     String  Item Name           
    1        Int  ItemId              
    2        Int  Number Owned        
    3       Bool  ItemPowerLevel      
 1        Int  UserId              
 2     Binary  BoardLayout         
 3      Table  GameState           
    0     String  SaveDate            
    1        Int  UserId              
    2     String  Users description   
    3     Binary  GameData1           
    4     String  GameData2           
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);




            using (var game2 = new Table(
                OwnedItems(),
                new ItemCode("UserId"), //game specific
                new ItemCode("UsersBestFriend"), //game specific
                new IntColumn("Game Character Type"), //game specific
                GameSaveFields()))
            {
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name + "2",
                    "table created user defined types and methods", game2);
            }
            expectedres =
                @"------------------------------------------------------
Column count: 5
Table Name  : table created user defined types and methods
------------------------------------------------------
 0      Table  OwnedItems          
    0     String  Item Name           
    1        Int  ItemId              
    2        Int  Number Owned        
    3       Bool  ItemPowerLevel      
 1        Int  UserId              
 2        Int  UsersBestFriend     
 3        Int  Game Character Type 
 4      Table  GameState           
    0     String  SaveDate            
    1        Int  UserId              
    2     String  Users description   
    3     Binary  GameData1           
    4     String  GameData2           
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);

        }


        
        /// <summary>
        /// this kind of creation call should be legal - it creates a totally empty table, then only later sets up a field        
        /// </summary>
        [Test]
        public static void SubTableNoFields()
        {
            String actualres;
            using (
                var notSpecifyingFields = new Table(
                    "subtable".Table()
                    )) //at this point we have created a table with no fields
            {
                notSpecifyingFields.AddStringColumn("Buksestørrelse");
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                    "one field Created in two steps with table add column",
                    notSpecifyingFields);
            }
            const string expectedres =
                @"------------------------------------------------------
Column count: 2
Table Name  : one field Created in two steps with table add column
------------------------------------------------------
 0      Table  subtable            
 1     String  Buksestørrelse      
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
        }


        /// <summary>
        /// very simple table create test
        /// </summary>
        [Test]
        public static void TestHandleAcquireOneField()
        {
            string actualres;
            using (var testtbl = new Table(new ColumnSpec("name", DataType.String)))
            {
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name, "NameField", testtbl);
            }
            const string expectedres =
                @"------------------------------------------------------
Column count: 1
Table Name  : NameField
------------------------------------------------------
 0     String  name                
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
        }


        /// <summary>
        /// table create with some more columns
        /// </summary>
        [Test]
        public static void TestHandleAcquireSeveralFields()
        {
            String actualres;
            using (var testtbl3 = new Table(
                "Name".TightDbString(),
                "Age".TightDbInt(),
                "count".TightDbInt(),
                "Whatever".TightDbMixed()
                ))
            {
                //long  test = testtbl3.getdllversion_CSH();
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name, "four columns, Last Mixed",
                    testtbl3);
            }
            const string expectedres =
                @"------------------------------------------------------
Column count: 4
Table Name  : four columns, Last Mixed
------------------------------------------------------
 0     String  Name                
 1        Int  Age                 
 2        Int  count               
 3      Mixed  Whatever            
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
        }

        
        /// <summary>
        /// test the alternative table dumper implementation that does not use table class
        /// also test that all the table creation string extension based helper classes work
        /// </summary>
        [Test]
        public static void TestAllFieldTypesStringExtensions()
        {
            string actualres1;
            string actualres2;
            using (var t = new Table(
                "Count".Int(),
                "Valid".TightDbBool(),
                "Name".String(),
                "BLOB".TightDbBinary(),
                "Items".SubTable(
                    "ItemCount".Int(),
                    "ItemName".String()),
                "HtmlPage".Mixed(),
                "FirstSeen".Date(),
                "Fraction".Float(),
                "QuiteLargeNumber".Double()
                ))
            {
                actualres1 = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                    "Table with all allowed types (String Extensions)", t);
                actualres2 = TestHelper.TableDumperSpec(MethodBase.GetCurrentMethod().Name,
                    "Table with all allowed types (String Extensions)", t);
            }
            const string expectedres =
                @"------------------------------------------------------
Column count: 9
Table Name  : Table with all allowed types (String Extensions)
------------------------------------------------------
 0        Int  Count               
 1       Bool  Valid               
 2     String  Name                
 3     Binary  BLOB                
 4      Table  Items               
    0        Int  ItemCount           
    1     String  ItemName            
 5      Mixed  HtmlPage            
 6       Date  FirstSeen           
 7      Float  Fraction            
 8     Double  QuiteLargeNumber    
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres1);
            TestHelper.Cmp(expectedres, actualres2);
        }



        
        /// <summary>
        /// table create using old general field helper class
        /// </summary>
        [Test]
        public static void TestAllFieldTypesFieldClass()
        {
            string actualres1;
            string actualres2;
            using (var t = new Table(
                new ColumnSpec("Count", DataType.Int),
                new ColumnSpec("Valid", DataType.Bool),
                new ColumnSpec("Name", DataType.String),
                new ColumnSpec("BLOB", DataType.Binary),
                new ColumnSpec("Items",
                    new ColumnSpec("ItemCount", DataType.Int),
                    new ColumnSpec("ItemName", DataType.String)),
                new ColumnSpec("HtmlPage", DataType.Mixed),
                new ColumnSpec("FirstSeen", DataType.Date),
                new ColumnSpec("Fraction", DataType.Float),
                new ColumnSpec("QuiteLargeNumber", DataType.Double)
                ))
            {
                actualres1 = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                    "Table with all allowed types (Field)", t);
                actualres2 = TestHelper.TableDumperSpec(MethodBase.GetCurrentMethod().Name,
                    "Table with all allowed types (Field)", t);
            }
            const string expectedres =
                @"------------------------------------------------------
Column count: 9
Table Name  : Table with all allowed types (Field)
------------------------------------------------------
 0        Int  Count               
 1       Bool  Valid               
 2     String  Name                
 3     Binary  BLOB                
 4      Table  Items               
    0        Int  ItemCount           
    1     String  ItemName            
 5      Mixed  HtmlPage            
 6       Date  FirstSeen           
 7      Float  Fraction            
 8     Double  QuiteLargeNumber    
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres1);
            TestHelper.Cmp(expectedres, actualres2);
        }

        //todo:consider if we should not remove this option. Have a hard time figuring 
        //when the user only have the type as a string. Keep for now
        /// <summary>
        /// Table create using string field helper class.
        /// (depricated)
        /// </summary>
        [Test]
        public static void TestAllFieldTypesFieldClassStrings()
        {
            string actualres1;
            string actualres2;
            using (var t = new Table(
                new ColumnSpec("Count1", "integer"),
                new ColumnSpec("Count2", "Integer"), //Any case is okay
                new ColumnSpec("Count3", "int"),
                new ColumnSpec("Count4", "INT"), //Any case is okay
                new ColumnSpec("Valid1", "boolean"),
                new ColumnSpec("Valid2", "bool"),
                new ColumnSpec("Valid3", "Boolean"),
                new ColumnSpec("Valid4", "Bool"),
                new ColumnSpec("Name1", "string"),
                new ColumnSpec("Name2", "string"),
                new ColumnSpec("Name3", "str"),
                new ColumnSpec("Name4", "Str"),
                new ColumnSpec("BLOB1", "binary"),
                new ColumnSpec("BLOB2", "Binary"),
                new ColumnSpec("BLOB3", "blob"),
                new ColumnSpec("BLOB4", "Blob"),
                new ColumnSpec("Items",
                    new ColumnSpec("ItemCount", "integer"),
                    new ColumnSpec("ItemName", "string")),
                new ColumnSpec("HtmlPage1", "mixed"),
                new ColumnSpec("HtmlPage2", "MIXED"),
                new ColumnSpec("FirstSeen1", "date"),
                new ColumnSpec("FirstSeen2", "daTe"),
                new ColumnSpec("Fraction1", "float"),
                new ColumnSpec("Fraction2", "Float"),
                new ColumnSpec("QuiteLargeNumber1", "double"),
                new ColumnSpec("QuiteLargeNumber2", "Double")
                ))
            {
                actualres1 = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                    "Table with all allowed types (Field_string)", t);
                actualres2 = TestHelper.TableDumperSpec(MethodBase.GetCurrentMethod().Name,
                    "Table with all allowed types (Field_string)", t);
            }
            const string expectedres =
                @"------------------------------------------------------
Column count: 25
Table Name  : Table with all allowed types (Field_string)
------------------------------------------------------
 0        Int  Count1              
 1        Int  Count2              
 2        Int  Count3              
 3        Int  Count4              
 4       Bool  Valid1              
 5       Bool  Valid2              
 6       Bool  Valid3              
 7       Bool  Valid4              
 8     String  Name1               
 9     String  Name2               
10     String  Name3               
11     String  Name4               
12     Binary  BLOB1               
13     Binary  BLOB2               
14     Binary  BLOB3               
15     Binary  BLOB4               
16      Table  Items               
    0        Int  ItemCount           
    1     String  ItemName            
17      Mixed  HtmlPage1           
18      Mixed  HtmlPage2           
19       Date  FirstSeen1          
20       Date  FirstSeen2          
21      Float  Fraction1           
22      Float  Fraction2           
23     Double  QuiteLargeNumber1   
24     Double  QuiteLargeNumber2   
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres1);
            TestHelper.Cmp(expectedres, actualres2);
        }



        
        /// <summary>
        /// test new subclassed table creator helper classes
        /// </summary>
        [Test]
        public static void TestAllFieldTypesTypedFields()
        {
            string actualres1;
            string actualres2;
            using (var t = new Table(
                new IntColumn("Count"),
                new BoolColumn("Valid"),
                new StringColumn("Name"),
                new BinaryColumn("BLOB"),
                new SubTableColumn("Items",
                    new IntColumn("ItemCount"),
                    new StringColumn("ItemName")),
                new MixedColumn("HtmlPage"),
                new DateColumn("FirstSeen"),
                new FloatColumn("Fraction"),
                new DoubleColumn("QuiteLargeNumber")
                ))
            {
                actualres1 = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                    "Table with all allowed types (Typed Field)", t);
                actualres2 = TestHelper.TableDumperSpec(MethodBase.GetCurrentMethod().Name,
                    "Table with all allowed types (Typed Field)", t);
            }
            const string expectedres =
                @"------------------------------------------------------
Column count: 9
Table Name  : Table with all allowed types (Typed Field)
------------------------------------------------------
 0        Int  Count               
 1       Bool  Valid               
 2     String  Name                
 3     Binary  BLOB                
 4      Table  Items               
    0        Int  ItemCount           
    1     String  ItemName            
 5      Mixed  HtmlPage            
 6       Date  FirstSeen           
 7      Float  Fraction            
 8     Double  QuiteLargeNumber    
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres1);
            TestHelper.Cmp(expectedres, actualres2);
        }

        /*
        //test the alternative table dumper implementation that does not use table class
        [Test]
        public static void TestAllFieldTypesFieldMethods()
        {
            string actualres1;
            string actualres2;
            using (var t = new Table(
                Table.intfield("Count1"),
                Table.Field<String>("blah"),
                Table.Field<float>("blah"),
                Table.Field<int>("blah"),
                Table.Field<Table>("blah"),
                
                new Field("Count3", "int"),
                new Field("Count4", "INT"), //Any case is okay
                new Field("Valid1", "boolean"),
                new Field("Valid2", "bool"),
                new Field("Valid3", "Boolean"),
                new Field("Valid4", "Bool"),
                new Field("Name1", "string"),
                new Field("Name2", "string"),
                new Field("Name3", "str"),
                new Field("Name4", "Str"),
                new Field("BLOB1", "binary"),
                new Field("BLOB2", "Binary"),
                new Field("BLOB3", "blob"),
                new Field("BLOB4", "Blob"),
                new Field("Items",
                          new Field("ItemCount", "integer"),
                          new Field("ItemName", "string")),
                new Field("HtmlPage1", "mixed"),
                new Field("HtmlPage2", "MIXED"),
                new Field("FirstSeen1", "date"),
                new Field("FirstSeen2", "daTe"),
                new Field("Fraction1", "float"),
                new Field("Fraction2", "Float"),
                new Field("QuiteLargeNumber1", "double"),
                new Field("QuiteLargeNumber2", "Double")
                ))
            {
                actualres1 = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                                                 "Table with all allowed types (Field_string)", t);
                actualres2 = TestHelper.TableDumperSpec(MethodBase.GetCurrentMethod().Name,
                                                     "Table with all allowed types (Field_string)", t);
            }
            const string expectedres =
                @"------------------------------------------------------
Column count: 25
Table Name  : Table with all allowed types (Field_string)
------------------------------------------------------
 0        Int  Count1              
 1        Int  Count2              
 2        Int  Count3              
 3        Int  Count4              
 4       Bool  Valid1              
 5       Bool  Valid2              
 6       Bool  Valid3              
 7       Bool  Valid4              
 8     String  Name1               
 9     String  Name2               
10     String  Name3               
11     String  Name4               
12     Binary  BLOB1               
13     Binary  BLOB2               
14     Binary  BLOB3               
15     Binary  BLOB4               
16      Table  Items               
    0        Int  ItemCount           
    1     String  ItemName            
17      Mixed  HtmlPage1           
18      Mixed  HtmlPage2           
19       Date  FirstSeen1          
20       Date  FirstSeen2          
21      Float  Fraction1           
22      Float  Fraction2           
23     Double  QuiteLargeNumber1   
24     Double  QuiteLargeNumber2   
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres1);
            TestHelper.Cmp(expectedres, actualres2);
        }
        */







        /// <summary>
        /// Test that mixes various types of table create helper 
        /// </summary>
        [Test]
        public static void TestMixedConstructorWithSubTables()
        {
            string actualres;
            using (
                var testtbl = new Table(
                    "Name".TightDbString(),
                    "Age".TightDbInt(),
                    new ColumnSpec("age2", DataType.Int),
                    new ColumnSpec("age3", "Int"),
                    //                new IntegerField("Age3"),
                    new ColumnSpec("comments",
                        new ColumnSpec("phone#1", DataType.String),
                        new ColumnSpec("phone#2", DataType.String),
                        new ColumnSpec("phone#3", "String"),
                        "phone#4".TightDbString()
                        ),
                    new ColumnSpec("whatever", DataType.Mixed)
                    ))
            {
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name, "six colums,sub four columns",
                    testtbl);
            }
            const string expectedres =
                @"------------------------------------------------------
Column count: 6
Table Name  : six colums,sub four columns
------------------------------------------------------
 0     String  Name                
 1        Int  Age                 
 2        Int  age2                
 3        Int  age3                
 4      Table  comments            
    0     String  phone#1             
    1     String  phone#2             
    2     String  phone#3             
    3     String  phone#4             
 5      Mixed  whatever            
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
        }




        /// <summary>
        /// scope has been thoroughly debugged and does work perfectly in all imagined cases, but the testing was done before unit tests had been created
        /// </summary>
        [Test]        
        public static void TestTableScope()
        {
            Table testTable; //bad way to code this but i need the reference after the using clause
            using (testTable = new Table())
            {

                Assert.False(testTable.IsDisposed); //do a test to see that testtbl has a valid table handle 
            }
            Assert.True(testTable.IsDisposed);
            //do a test here to see that testtbl now does not have a valid table handle
        }



        /// <summary>
        /// 
        ///while You cannot cross-link parents and subtables inside a new table() construct, you can try to do so, by deliberatly changing
        /// the subtable references in Field objects that You instantiate yourself -and then call Table.create(Yourfiled) with a 
        /// field definition that is self referencing.
        /// however, currently this is not possible as seen in the example below.
        /// the subtables cannot be changed directly, so all You can do is create new objects that has old already created objects as subtables
        /// therefore a tree structure, no recursion.
        /// 
        /// below is my best shot at someone trying to create a table with custom built cross-linked field definitions (and failing)
        /// 
        /// I did not design the Field type to be used on its own like the many examples below. However , none of these weird uses break anything
        /// 
        /// </summary>
        [Test]
        public static void TestIllegalFieldDefinitions1()
        {
            ColumnSpec f5 = "f5".Int(); //create a field reference, type does not matter
            f5 = "f5".Table(f5); //try to overwrite the field object with a new object that references itself 
            string actualres;
            using (
                var t = new Table(f5))
                //this will not crash or loop forever the subtable field does not references itself 
            {
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name, "self-referencing subtable", t);
            }
            const string expectedres =
                @"------------------------------------------------------
Column count: 1
Table Name  : self-referencing subtable
------------------------------------------------------
 0      Table  f5                  
    0        Int  f5                  
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
        }

        /// <summary>
        /// Try to create table with entangled table constructor heloper classes.This should work, it should be impossible
        /// to make the constructor hang, for instance
        /// </summary>
        [Test]
        public static void TestIllegalFieldDefinitions2()
        {
            ColumnSpec fc = "fc".Int(); //create a field reference, type does not matter
            ColumnSpec fp = "fp".Table(fc); //let fp be the parent table subtable column, fc be the sole field in a subtable

            fc = "fc".Table(fp); //then change the field type from int to subtable and reference the parent

            //You now think You have illegal field definitions in fc and fp as both are subtables and both reference the other as the sole subtable field
            //however, they are new objects that reference the old classes that they replaced.
            String actualres;
            using (
                var t2 = new Table(fc))
            {
                //should crash too
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                    "subtable that has subtable that references its parent #1", t2);
            }
            const String expectedres =
                @"------------------------------------------------------
Column count: 1
Table Name  : subtable that has subtable that references its parent #1
------------------------------------------------------
 0      Table  fc                  
    0      Table  fp                  
       0        Int  fc                  
------------------------------------------------------

";


            TestHelper.Cmp(expectedres, actualres);
        }

        /// <summary>
        /// more attempts to hang the table constructor (doesn't hang)
        /// </summary>
        [Test]
        public static void TestIllegalFieldDefinitions3()
        {
            ColumnSpec fc = "fc".Int(); //create a field reference, type does not matter
            ColumnSpec fp = "fp".Table(fc); //let fp be the parent table subtable column, fc be the sole field in a subtable
            // ReSharper disable RedundantAssignment
            fc = "fc".Table(fp); //then change the field type from int to subtable and reference the parent
            // ReSharper restore RedundantAssignment

            String actualres;
            using (
                var t3 = new Table(fp))
            {
                //should crash too
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                    "subtable that has subtable that references its parent #2", t3);
            }
            const String expectedres =
                @"------------------------------------------------------
Column count: 1
Table Name  : subtable that has subtable that references its parent #2
------------------------------------------------------
 0      Table  fp                  
    0        Int  fc                  
------------------------------------------------------

";

            TestHelper.Cmp(expectedres, actualres);

        }

        /// <summary>
        /// 
        ///super creative attemt at creating a cyclic graph of Field objects
        ///still it fails because the array being manipulated is from GetSubTableArray and thus NOT the real list inside F1 even though the actual field objects referenced from the array ARE the real objects
        ///point is - You cannot stuff field definitions down into the internal array this way
        /// </summary>
        [Test]
        public static void TestCyclicFieldDefinition1()
        {

            ColumnSpec f1 = "f10".SubTable("f11".Int(), "f12".Int());
            var subTableElements = f1.GetSubTableArray();
            subTableElements[0] = f1; //and the "f16" field in f1.f15.f16 is now replaced with f1.. recursiveness


            string actualres;
            using (var t4 = new Table(f1))
            {
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name, "cyclic field definition", t4);
            }
            const String expectedres =
                @"------------------------------------------------------
Column count: 1
Table Name  : cyclic field definition
------------------------------------------------------
 0      Table  f10                 
    0        Int  f11                 
    1        Int  f12                 
------------------------------------------------------

";

            TestHelper.Cmp(expectedres, actualres);
        }

        //dastardly creative attemt at creating a cyclic graph of Field objects
        //this creative approach succeeded in creating a stack overflow situation when the table is being created, but now it is not possible as AddSubTableFields has been made
        //internal, thus unavailable in customer assemblies.

        private class AttemptCircularColumnSpec : ColumnSpec
        {
            [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"),
             SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
                 MessageId = "fielddefinitions"),
             SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
                 MessageId = "fieldName")]
            // ReSharper disable UnusedParameter.Local
            public void Setsubtablearray(String fieldName, ColumnSpec[] fielddefinitions)
                //make the otherwise hidden addsubtablefield public
                // ReSharper restore UnusedParameter.Local
            {
                //uncommenting the line below should create a compiletime error (does now) or else this unit test wil bomb the system
                //                AddSubTableFields(this, fieldName,fielddefinitions);
            }

            [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
                MessageId = "columnName"),
             SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
                 MessageId = "subTableFieldsArray")]
            // ReSharper disable UnusedParameter.Local
            public AttemptCircularColumnSpec(string columnName, params ColumnSpec[] subTableColumnsSpecArray)
                // ReSharper restore UnusedParameter.Local
            {
                FieldType = DataType.Table;
            }
        }


        /// <summary>
        /// setsubtable test
        /// </summary>
        [Test]
        public static void TableSetSubTableAsSubTable()
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
                    t.AddEmptyRow(1);
                    t.SetSubTable(1, 0, sub);
                    Table subreturned = t.GetSubTable(1, 0);
                    Assert.AreEqual(string00, subreturned.GetString(0, 0));
                }
            }
        }

        /// <summary>
        /// Stting a subtble using array of array
        /// </summary>
        [Test]
        public static void TableSetSubTableAsEnumerable()
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
                var sub = new []//Create something IEnumerable(object)
                {
                    new[] {"R0F0", "R0F1"},
                    new[] {"R1F0", "R1F1"},
                    new[] {"R2F0", "R2F1"}
                };

                {
                    t.AddEmptyRow(1);
                    t.SetSubTable(1, 0, sub);
                    Table subreturned = t.GetSubTable(1, 0);
                    Assert.AreEqual("R2F1", subreturned.GetString(1,2));
                }
            }
        }


        /// <summary>
        /// clear subtable test
        /// </summary>
        [Test]
        public static void TableClearSubTable()
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
                    t.AddEmptyRow(1);
                    t.SetSubTable(1, 0, sub);
                    Table subreturned = t.GetSubTable(1, 0);
                    Assert.AreEqual(string00, subreturned.GetString(0, 0));
                    t.ClearSubTable(1, 0);
                    Table clearedSubReturned = t.GetSubTable(1, 0);
                    Assert.AreEqual(0, clearedSubReturned.Size);
                }
            }
        }

        /// <summary>
        /// table insert test
        /// </summary>
        [Test]
        public static void TableInsert()
        {
            using (var table = new Table("IntField".Int()))
            {
                table.Insert(0, 42); //42
                table.Insert(0, 1); //1,42
                table.Insert(0, 2); //2,1,42
                table.Insert(0, 3); //3,2,1,42
                table.Insert(0, 4); //4,3,2,1,42
                table.Insert(3, 100); //4,3,2,100,1,42
                table.Insert(3, 101); //4,3,2,101,100,1,42
                Assert.AreEqual(4, table.GetLong(0, 0));
                Assert.AreEqual(3, table.GetLong(0, 1));
                Assert.AreEqual(2, table.GetLong(0, 2));
                Assert.AreEqual(101, table.GetLong(0, 3));
                Assert.AreEqual(100, table.GetLong(0, 4));
                Assert.AreEqual(1, table.GetLong(0, 5));
                Assert.AreEqual(42, table.GetLong(0, 6));
            }
        }

        /// <summary>
        /// table insert with bad index
        /// </summary>
        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableInsertTooLowIndex()
        {
            using (var table = new Table("IntField".Int()))
            {
                table.Insert(-1, 42); //throws
            }
        }

        /// <summary>
        /// table insert too low
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableInsertTooHighIndex()
        {
            using (var table = new Table("IntField".Int()))
            {
                try
                {
                    table.Add(0);
                    table.Add(1);
                    table.Insert(2, 42); //okay - inserts after last
                }
                catch //none of the above should throw anything
                {
                    throw new InvalidOperationException("Table Insert (Size+1,n) threw an unexpected exception");
                }
                table.Insert(4, 42); //Bad - throws
            }
        }



        
        
        
        /// <summary>
        /// todo:This distinct thing only works on indexed string columns, and lacks documentation. Wait for core to catch up.
        /// until then, we have to make sure users only calls distinct on indexed string columns
        /// this unit test tests exactly that scenario
        /// </summary>
        [Test]
        public static void TableDistinct()
        {
            using (var table = new Table("IntField".Int(), "StringField".String()))
            {
                Assert.AreEqual(false, table.HasIndex(1));
                Assert.AreEqual(false, table.HasIndex(0));
                Assert.AreEqual(false, table.HasIndex("StringField"));
                Assert.AreEqual(false, table.HasIndex("IntField"));
                table.SetIndex(1);
                Assert.AreEqual(true, table.HasIndex("StringField"));
                Assert.AreEqual(false, table.HasIndex("IntField"));
                table.Add(1, "A");
                table.Add(2, "B");
                table.Add(3, "C");
                table.Add(4, "A");
                table.Add(5, "B");
                table.Add(6, "C");
                table.Add(7, "B");
                table.Add(8, "B");
                table.Add(9, "B");
                table.Add(10, "D");
                table.Add(11, "x");
                TableView tv = table.Distinct("StringField");
                Assert.AreEqual(5, tv.Size);
                Assert.AreEqual("A", tv.GetString(1, 0));
                Assert.AreEqual("B", tv.GetString(1, 1));
                Assert.AreEqual("C", tv.GetString(1, 2));
                Assert.AreEqual("D", tv.GetString(1, 3));
                Assert.AreEqual("x", tv.GetString(1, 4));
                Assert.AreEqual(1, tv.GetLong(0, 0));
                Assert.AreEqual(2, tv.GetLong(0, 1));
                Assert.AreEqual(3, tv.GetLong(0, 2));
                Assert.AreEqual(10, tv.GetLong(0, 3));
                Assert.AreEqual(11, tv.GetLong(0, 4));
            }
        }


        /// <summary>
        /// Call distinct with a not supported not indexed column ()
        /// </summary>
        [Test]
        [ExpectedException("System.ArgumentException")] //type validation
        public static void TableDistinctBadType()
        {
            using (var table = new Table("IntField".Int(), "StringField".String()))
            {
                table.SetIndex(1);
                table.Add(1, "A");
                TableView tv = table.Distinct("IntField"); //throws bc intfield must be an indexed stringfield
                Assert.AreEqual(0, tv.Size); //we should never get this far
            }
        }


        /// <summary>
        /// test that distinct errors when column has no index 
        /// </summary>
        [Test]
        [ExpectedException("System.InvalidOperationException")]
        public static void TableDistinctNoIndex()
        {
            using (var table = new Table("IntField".Int(), "StringField".String()))
            {
                table.Add(1, "A");
                TableView tv = table.Distinct("StringField"); //throws bc intfield must be an indexed stringfield
                Assert.AreEqual(0, tv.Size); //we should never get this far
            }
        }

        /// <summary>
        /// Call set index on a non-indexable field
        /// </summary>
        [Test]
        [ExpectedException("System.ArgumentException")] //must throw tyepe validation exception
        public static void TableSetIndexTypeError()
        {
            using (var table = new Table("IntField".Int(), "StringField".String()))
            {
                table.SetIndex(0); //throws bc col 0 is not a string column
            }
        }

        /// <summary>
        /// must throw because core does not support indexes in non-mixed subtables yet
        /// </summary>
        [Test]
        [ExpectedException("System.InvalidOperationException")]
        
        public static void TableSetIndexSubTableError()
        {
            using (var table = new Table("IntField".Int(), "subfield".Table("substringfield".String())))
            {
                table.AddEmptyRow(1);
                using (var sub = table.GetSubTable(1, 0))
                {
                    sub.SetIndex(0); //throws
                }
            }
        }




        /// <summary>
        /// test insertempty row
        /// </summary>
        [Test]
        public static void TableInsertEmptyRow()
        {
            using (var table = new Table("IntField".Int()))
            {
                table.Add(42);
                table.Add(43);
                table.Add(44);
                table.Add(45);
                table.InsertEmptyRow(0, 1); //0 42 43 44 45
                table.InsertEmptyRow(3, 2); //0 42 43 0 0 44 45
                table.InsertEmptyRow(7, 1); //0 42 43 0 0 44 45
                Assert.AreEqual(0, table.GetLong(0, 0));
                Assert.AreEqual(42, table.GetLong(0, 1));
                Assert.AreEqual(43, table.GetLong(0, 2));
                Assert.AreEqual(0, table.GetLong(0, 3));
                Assert.AreEqual(0, table.GetLong(0, 4));
                Assert.AreEqual(44, table.GetLong(0, 5));
                Assert.AreEqual(45, table.GetLong(0, 6));
            }
        }

        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableInsertEmptyTooLowIndex()
        {
            using (var table = new Table("IntField".Int()))
            {
                table.InsertEmptyRow(-1, 10); //throws
            }
        }

        /// <summary>
        /// test bad insert row index
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableInsertEmptyTooHighIndex()
        {
            using (var table = new Table("IntField".Int()))
            {
                try
                {
                    table.Add(0);
                    table.Add(1);
                    table.InsertEmptyRow(2, 1); //okay - inserts after last
                }
                catch //none of the above should throw anything
                {
                    throw new InvalidOperationException();
                }
                table.InsertEmptyRow(4, 1); //Bad - throws
            }
        }

        //suppose You have several places in Your table hierachy where You need a group of fields, that are specified the same way
        //for instance a group of fields that together specify a physical Address
        //You could create a method that puts this group into a column in a table structure, to make sure they are created
        //the same way everywhere they are used
        //path is then the pointer to the column that is a subtable, and where You want Your address fields and stuff
        private static void AddAddressFieldsInSubtable(Table t, List<long> path)
        {
            t.AddColumn(path, DataType.String, "Address1");
            t.AddColumn(path, DataType.String, "Address2");
            t.AddColumn(path, DataType.String, "AddressType");//some entities have more than  one address type, like, shipping, and invoice address
            t.AddColumn(path, DataType.Table, "ShippingConstraints");
            path.Add(3);//add the location of ShippingConstraints
            t.AddColumn(path, DataType.String, "ConstraintName");
            t.AddColumn(path, DataType.String, "ConstraintValue");
        }

        /// <summary>
        /// add columns in subtable using path navigation
        /// </summary>
        [Test]
        public static void TableAddSubTablePlausiblePathExample()
        {
            using (var customers = new Table())
            {
                customers.AddStringColumn("ID");
                customers.AddStringColumn("InGoodStanding"); //do we extend credit?
                customers.AddStringColumn("Contact Name");
                customers.AddStringColumn("Email Address");
                var addresspath = customers.AddSubTableColumn("Addresses");
                AddAddressFieldsInSubtable(customers, addresspath); //add address info to the Addresses subtable
            }

            using (var contractors = new Table())
            {
                contractors.AddStringColumn("Department");
                var addresspath = contractors.AddSubTableColumn("Projects");
                {
                    contractors.AddDateColumn(addresspath, "StartDate");
                    var contractorspath = contractors.AddSubTableColumn(addresspath, "ProjectContractors");
                    {
                        contractors.AddStringColumn(contractorspath, "Contactor ID");
                        contractors.AddStringColumn(contractorspath, "Contactor Type");
                        var addressespath = contractors.AddSubTableColumn(contractorspath, "Addresses");
                        AddAddressFieldsInSubtable(contractors, addressespath);
                        //add address info to the Addresses subtable                    
                    }
                }
            }
        }

        /// <summary>
        /// this should probably fail, or else create John as a field in the top table
        /// failed earlier but now it actually creates "John" at the top level
        /// </summary>
        [Test]
        //[ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableAddSubTableEmptyArray()
        {
            using (var toptable1 = new Table(new IntColumn("ID"), new StringColumn("name"), new SubTableColumn("Edges")))
            {
                toptable1.AddColumn(new List<long>() , DataType.Int, "John");//adding with no path.
            }
        }

        


        /// <summary>
        /// this should definetly fail as no column is specified - the column is not identified at the top level
        /// </summary>
        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableRenameSubTableEmptyArray1()
        {
            using (var toptable1 = new Table(new IntColumn("ID"), new StringColumn("name"), new SubTableColumn("Edges")))
            {
                toptable1.RenameColumn(new List<long>(),  "John");//adding with no path.
            }
        }

        
        /// <summary>
        /// this should definetly fail as no column is specified
        /// </summary>
        [Test]
        [ExpectedException("System.ArgumentNullException")]
        public static void TableRenameSubTableEmptyArray3()
        {
            using (var toptable1 = new Table(new IntColumn("ID"), new StringColumn("name"), new SubTableColumn("Edges")))
            {
                toptable1.RenameColumn( null,  "John");//adding with no path.
            }
        }



        
        /// <summary>
        /// this was not possible using spec (adding columns to tables with existing columns), but should be possible now
        /// </summary>
        [Test]
        public static void TableAddColumn()
        {
            using (var t = new Table("field".Int()))
            {                
                t.AddIntColumn( "intfield");                
            }
        }


        
        /// <summary>
        /// test that the various spec column adders actually add the correct column type
        /// </summary>
        [Test]
        public static void TableAddColumnTypes()
        {
            using (var table = new Table())
            {                
                Assert.AreEqual(0, table.AddBinaryColumn("binary"));
                Assert.AreEqual(1, table.AddBoolColumn("bool"));
                Assert.AreEqual(2, table.AddDateColumn("date"));
                Assert.AreEqual(3, table.AddDoubleColumn("double"));
                Assert.AreEqual(4, table.AddFloatColumn("float"));
                Assert.AreEqual(5, table.AddIntColumn("int"));
                Assert.AreEqual(6, table.AddMixedColumn("mixed"));
                Assert.AreEqual(7, table.AddStringColumn("string"));
                Assert.AreEqual(new List<long>{8}, table.AddSubTableColumn("subtable"));
                var path = new List<long>{8};
                Assert.AreEqual(0, table.AddBinaryColumn(path,"binary"));
                Assert.AreEqual(1, table.AddBoolColumn(path, "bool"));
                Assert.AreEqual(2, table.AddDateColumn(path, "date"));
                Assert.AreEqual(3, table.AddDoubleColumn(path, "double"));
                Assert.AreEqual(4, table.AddFloatColumn(path, "float"));
                Assert.AreEqual(5, table.AddIntColumn(path, "int"));
                Assert.AreEqual(6, table.AddMixedColumn(path, "mixed"));
                Assert.AreEqual(7, table.AddStringColumn(path, "string"));
                Assert.AreEqual(new List<long>{8,8}, table.AddSubTableColumn(path, "table"));
                
                Assert.AreEqual(false, table.HasSharedSpec());//shared spec is false for root tables
                Assert.AreEqual(DataType.Binary, table.ColumnType(0));
                Assert.AreEqual(DataType.Bool, table.ColumnType(1));
                Assert.AreEqual(DataType.Date, table.ColumnType(2));
                Assert.AreEqual(DataType.Double, table.ColumnType(3));
                Assert.AreEqual(DataType.Float, table.ColumnType(4));
                Assert.AreEqual(DataType.Int, table.ColumnType(5));
                Assert.AreEqual(DataType.Mixed, table.ColumnType(6));
                Assert.AreEqual(DataType.String, table.ColumnType(7));
                Assert.AreEqual(DataType.Table, table.ColumnType(8));
                table.AddEmptyRow(5);
                Table subTable = table.GetSubTable(8, 3);
                Assert.AreEqual(true, subTable.HasSharedSpec());
                Assert.AreEqual(DataType.Binary, subTable.ColumnType(0));
                Assert.AreEqual(DataType.Bool, subTable.ColumnType(1));
                Assert.AreEqual(DataType.Date, subTable.ColumnType(2));
                Assert.AreEqual(DataType.Double, subTable.ColumnType(3));
                Assert.AreEqual(DataType.Float, subTable.ColumnType(4));
                Assert.AreEqual(DataType.Int, subTable.ColumnType(5));
                Assert.AreEqual(DataType.Mixed, subTable.ColumnType(6));
                Assert.AreEqual(DataType.String, subTable.ColumnType(7));
                Assert.AreEqual(DataType.Table, subTable.ColumnType(8));
            }
        }

        /// <summary>
        /// old spec test, modified a bit
        /// Illustrate a problem if the user creates two wrappers
        /// that both wrap the same table from a group, and then changes
        /// spec i one of them, and then asks the other if spec change is legal
        /// problem is the wrapper state HasRows - it is not updated in all wrappers
        /// that wrap the same table.
        /// HOWEVER - it is pretty weird to get two table wrappers from the same group
        /// at once in the first place. That in itself should probably be illegal, even though
        /// legal spec operations will work fine if they come in from the wrappers interleaved
        /// 
        /// </summary>
        [Test]
        public static void TableTwoWrappersChangeSpec()
        {
            using (var g = new Group())
            {
                Table t1 = g.CreateTable("T");
                Table t2 = g.GetTable("T");
                Table t3 = g.GetTable("T");
                Assert.AreEqual(t1.Handle, t2.Handle);
                t2.AddIntColumn("inttie");
                t1.AddStringColumn("stringie");
                Assert.AreEqual(2, t3.ColumnCount);
            }
        }




        
        /// <summary>
        /// negative top index
        /// </summary>
        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableRenameSubTableBadIndex1()
        {
            using (var toptable1 = new Table(new IntColumn("ID"), new StringColumn("name"), new SubTableColumn("Edges")))
            {
                toptable1.RenameColumn(new List<long> {-1}, "John");//this column does not exist
            }
        }

        /// <summary>
        /// bad path value speciified
        /// </summary>
        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableRenameSubTableBadIndex2()
        {
            using (var toptable1 = new Table(new IntColumn("ID"), new StringColumn("name"), new SubTableColumn("Edges")))
            {
                toptable1.RenameColumn(new List<long> { 3 }, "John");//this column does not exist
            }
        }

        /// <summary>
        /// bad path value speciified
        /// </summary>
        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableRenameSubTableBadIndex3()
        {
            using (var toptable1 = new Table(new IntColumn("ID"), new StringColumn("name")))
            {
                var edges = toptable1.AddSubTableColumn("Edges");
                toptable1.AddIntColumn(edges, "ID");
                toptable1.AddIntColumn(edges ,"Weight");               
                toptable1.RenameColumn(new List<long>{2,-1}, "John");//this column does not exist
            }
        }


        /// <summary>
        /// bad path value speciified
        /// </summary>
        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableRenameSubTableBadIndex4()
        {
            using (var toptable1 = new Table(new IntColumn("ID"), new StringColumn("name"), new SubTableColumn("Edges")))
            {
                var edges = new List<long> {2};
                toptable1.AddColumn(edges,DataType.Int, "ID");
                toptable1.AddColumn(edges,DataType.Int, "Weight");
                edges.Add(2);//point to an nonexisting column
                toptable1.RenameColumn(edges,"John");//this column does not exist
            }
        }

        /// <summary>
        /// bad path value speciified
        /// </summary>
        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableRenameSubTableBadIndex5()
        {
            using (var toptable1 = new Table(new IntColumn("ID"), new StringColumn("name")))
            {
                var edgepath = toptable1.AddSubTableColumn("Edges");
                toptable1.AddColumn(edgepath,DataType.Int, "ID");
                var emptySub=toptable1.AddSubTableColumn(edgepath, "EmptySub");
                var illegalpath =  new List<long>(emptySub) {1};
                toptable1.RenameColumn(illegalpath,"John");//this column does not exist, emptysub does not have a field 1
            }
        }

        [Test]        
        public static void TableRemoveSubColumn()
        {
            using (var toptable1 = new Table(new IntColumn("ID"), new StringColumn("name"), new SubTableColumn("Edges")))
            {
                var path = new List<long> {2};
                toptable1.AddColumn(path ,DataType.Int, "ID");
                toptable1.AddColumn(path , DataType.Table, "EmptySub");
                Assert.AreEqual(2,toptable1.Spec.GetSpec(2).ColumnCount);//todo:get rid of spec usage when we have a get_column_count(path) in core
                path.Add(0);
                toptable1.RemoveColumn(path);
                Assert.AreEqual(1, toptable1.Spec.GetSpec(2).ColumnCount);//todo:get rid of spec usage when we have a get_column_count(path) in core
                Assert.AreEqual("EmptySub", toptable1.Spec.GetSpec(2).GetColumnName(0));//todo:get rid of spec usage when we have a get_column_count(path) in core
                Assert.AreEqual(DataType.Table, toptable1.Spec.GetSpec(2).GetColumnType(0));//todo:get rid of spec usage when we have a get_column_count(path) in core
            }
        }

        //add generic column at the top level
        [Test]
        public static void TableAddColumnTypeParameter()
        {
            using (var table = new Table())
            {
                table.AddColumn(DataType.Bool, "boolcolumn");
                Assert.AreEqual("boolcolumn",table.GetColumnName(0));
                Assert.AreEqual(DataType.Bool, table.ColumnType(0));
            }
        }

        //add generic column with a path top level
        [Test]
        public static void TableAddColumnTypeParameterPath()
        {
            using (var table = new Table())
            {
                var path = table.AddSubTableColumn("sub");
                table.AddColumn(path,DataType.Bool, "boolcolumn");
                Assert.AreEqual("boolcolumn", table.Spec.GetSpec(0).GetColumnName(0));
                Assert.AreEqual(DataType.Bool, table.Spec.GetSpec(0).GetColumnType(0));
            }
        }



        // create a nested subtable, used to store a graph where most of the work is looking at a vertex and its edges (not so much the vertices its edges point to) :
                // ID int
                // Name string
                // Edges subtable
                //       ID int //(target vertex ID)
                //       Weight double //weight for edge
                //       Cost Double  //cost for this edge, will be calculated
                //       Visited boolean //did we calculate the weight for this edge yet?
                //       Edge data       
                //          Name String  //extra data tagged on the edge
                //          Value int    //used to calculate cost of traversing edge



            [Test]
        public static void TableAddSubTableUsingPath()
        {
            String actualres;
            //test array based syntax
                using (
                    var toptable1 = new Table(new IntColumn("ID"), new StringColumn("name"), new SubTableColumn("Edges")))
                {
                    var path = new List<long> {2};//Any kind of IList will do
                    toptable1.AddIntColumn(path,  "ID");
                    toptable1.AddDoubleColumn(path, "Weight");
                    toptable1.AddDoubleColumn(path,  "Cost");
                    toptable1.AddBoolColumn(path,  "Visited");
                    path = toptable1.AddSubTableColumn(path, "Edge Data");
                    
                    toptable1.AddColumn(path, DataType.String, "Name");
                    toptable1.AddColumn(path, DataType.Int, "Value");

                    actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                        "subtable structure made with AddSubColumn path arrays", toptable1);
                }
                const string expectedres = 
@"------------------------------------------------------
Column count: 3
Table Name  : subtable structure made with AddSubColumn path arrays
------------------------------------------------------
 0        Int  ID                  
 1     String  name                
 2      Table  Edges               
    0        Int  ID                  
    1     Double  Weight              
    2     Double  Cost                
    3       Bool  Visited             
    4      Table  Edge Data           
       0     String  Name                
       1        Int  Value               
------------------------------------------------------

";
            Assert.AreEqual(expectedres, actualres);
        }

        //see test above -  perhaps this kind of notation is not really very useful, it formats pretty ugly when being autoformatted
        //It might always be easier to read and maintain if the user instantiates a List<int> object as in the parameter example
        [Test]
        public static void TableAddSubTableUsingParameters()
        {
            String actualres;
            //test array based syntax
            using (var toptable1 = new Table(new IntColumn("ID"), new StringColumn("name")))
            {
                var edges = toptable1.AddSubTableColumn("Edges");
                toptable1.AddColumn(edges,DataType.Int, "ID");
                toptable1.AddColumn(edges,DataType.Double, "Weight");
                toptable1.AddColumn(edges,DataType.Double, "Cost");
                var test = toptable1.AddColumn(edges,DataType.Bool, "Visited");
                var edgeData=toptable1.AddSubTableColumn(edges, "Edge Data");
                toptable1.AddColumn(edgeData,DataType.String, "Name");
                toptable1.AddColumn(edgeData,DataType.Int, "Value");
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                    "subtable structure made with AddSubColumn parameters", toptable1);
                Assert.AreEqual(3, test); //test should be placed in column index 3
            }
            const string expectedres =
@"------------------------------------------------------
Column count: 3
Table Name  : subtable structure made with AddSubColumn parameters
------------------------------------------------------
 0        Int  ID                  
 1     String  name                
 2      Table  Edges               
    0        Int  ID                  
    1     Double  Weight              
    2     Double  Cost                
    3       Bool  Visited             
    4      Table  Edge Data           
       0     String  Name                
       1        Int  Value               
------------------------------------------------------

";

            Assert.AreEqual(expectedres, actualres);
        }



        //see test above
        [Test]
        public static void TableRenameSubTableUsingParameters()
        {
            //test array based syntax
            using (var toptable1 = new Table(new IntColumn("ID"), new StringColumn("name")))
            {
                var edges=toptable1.AddSubTableColumn("Edges");
                var idindex=toptable1.AddIntColumn(edges, "ID");
                toptable1.AddDoubleColumn(edges, "Weight");
                toptable1.AddDoubleColumn(edges, "Cost");
                toptable1.AddBoolColumn(edges, "Visited");
                var edgeData = toptable1.AddSubTableColumn(edges,"Edge Data");
                var nameIndex = toptable1.AddStringColumn(edgeData, "Name");
                toptable1.AddIntColumn(edgeData, "Value");
                string actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name+"_before_rename",
                    "subtable structure made with AddSubColumn parameters", toptable1);
            
            const string expectedres =
@"------------------------------------------------------
Column count: 3
Table Name  : subtable structure made with AddSubColumn parameters
------------------------------------------------------
 0        Int  ID                  
 1     String  name                
 2      Table  Edges               
    0        Int  ID                  
    1     Double  Weight              
    2     Double  Cost                
    3       Bool  Visited             
    4      Table  Edge Data           
       0     String  Name                
       1        Int  Value               
------------------------------------------------------

";

            Assert.AreEqual(expectedres, actualres);


            //now, do some renaming the last number in the path is the column id for the column that should be renamed. all other path nubers must specify subtable columns
            toptable1.RenameColumn(edges, idindex,"_ID");
            toptable1.RenameColumn(edgeData,"_Edge Data");//edge data points directly to the column whose name we want to change
            toptable1.RenameColumn(edgeData,nameIndex, "_Name" );

            string actualres2 = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name + "_after_rename",
"rename columns in subtables via parameters", toptable1);

            const string expectedres2 =
@"------------------------------------------------------
Column count: 3
Table Name  : rename columns in subtables via parameters
------------------------------------------------------
 0        Int  ID                  
 1     String  name                
 2      Table  Edges               
    0        Int  _ID                 
    1     Double  Weight              
    2     Double  Cost                
    3       Bool  Visited             
    4      Table  _Edge Data          
       0     String  _Name               
       1        Int  Value               
------------------------------------------------------

";

                Assert.AreEqual(expectedres2,actualres2);
            }            
        }


        //todo(er i Asana):måske skulle addcolumn også kunne tage et field objekt og et field array - så kunne man bruge table create syntaxen til at adde kolonner
        //lav prioritet, eneste fordel vil være at man kan oprette tables med subtables i en oneliner
        [Test]
        public static void TableAddSubTableHugeTable()
        {
            using (var table = new Table())
            {
                var st0 = table.AddSubTableColumn("st0");
                {
                    table.AddIntColumn(st0, "st00");
                    table.AddIntColumn(st0, "st01");
                }
                var st1 = table.AddSubTableColumn("st1");
                {
                    table.AddIntColumn(st1, "st10");
                    table.AddIntColumn(st1, "st11");
                }
                var st2 = table.AddSubTableColumn("st2");
                {
                    table.AddIntColumn(st2, "st20");
                    var st21 = table.AddSubTableColumn(st2, "st21");
                    {
                        table.AddIntColumn(st21, "st210");
                        table.AddIntColumn(st21, "st211");
                        var st212 = table.AddSubTableColumn(st21, "st212");
                        {
                            table.AddIntColumn(st212, "st2121");
                            table.AddIntColumn(st212, "st2122");
                        }
                    }
                }
            }
        }


        [Test]
        public static void TableSetBinary()
        {
            using (var table = new Table("binaryfield".Binary()))
            {
                byte[] testArray = {42};
                table.AddEmptyRow(1);
                table.SetBinary(0, 0, testArray);

                byte[] testReturned = table.GetBinary(0, 0);
                Assert.AreEqual(1, testReturned.Length);
                Assert.AreEqual(42, testReturned[0]);
            }
        }


        [Test]
        public static void TableSetMixedBinary()
        {
            using (var table = new Table("matadormix".Mixed()))
            {
                byte[] testArray = {01, 12, 36, 22};
                table.AddEmptyRow(1);
                table.SetMixedBinary(0, 0, testArray);

                byte[] testReturned = table.GetMixedBinary(0, 0);
                Assert.AreEqual(4, testReturned.Length);
                Assert.AreEqual(1, testReturned[0]);
                Assert.AreEqual(12, testReturned[1]);
                Assert.AreEqual(36, testReturned[2]);
                Assert.AreEqual(22, testReturned[3]);
            }
        }



        [Test]
        public static void TableSetGetEmptyBinary()
        {
            using (var table = new Table(new BinaryColumn("bin")))
            {
                //reading back a binarydata that was added with addempty row
                table.AddEmptyRow(1);
                Array binaryData = table.GetBinary(0, 0);
                Assert.AreEqual(0, binaryData.Length);

                //setting null, getting an empty binary data back
                table.Add(null);
                Array binaryData2 = table.GetBinary(0, 1);
                Assert.AreEqual(0, binaryData2.Length);

                //setting null, getting an empty binary data back
                table.SetBinary(0, 1, null);
                Array binaryData5 = table.GetBinary(0, 1);
                Assert.AreEqual(0, binaryData5.Length);


                //setting empty binary data, and getting that back again
                Array binaryData3 = new Byte[] {};
                table.Add(binaryData3);
                Array binaryData4 = table.GetBinary(0, 2);
                Assert.AreEqual(0, binaryData4.Length);
            }
            //the 3 above for tableview are to be found in TableViewTests
        }



        [Test]
        [ExpectedException("System.NotImplementedException")]
        //when implemented in core, remove this expectation and the throw in UnsafeNativeMethods.TableFindFirstBinary
        //and the guard that makes mono compilation not call the dll
        public static void TableFindFirstBinary()
        {
            using (var table = new Table("radio".Binary(), "int".Int()))
            {
                byte[] testArray = {01, 12, 36, 22};
                table.AddEmptyRow(1);
                table.AddEmptyRow(1);
                table.SetBinary("radio", 1, testArray);
                table.SetLong(1, 1, 42);

                byte[] arrayToFind = {01, 12, 36, 22};
                var rowNo = table.FindFirstBinary(0, arrayToFind);

                Assert.AreEqual(1, rowNo);
            }
        }



        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableSetSubTableBadSchema()
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
                    "substringfield2".String(),
                    "substringfield3".String()
                    )
                    )
                {
                    const string string00 = "stringvalueC0R0";
                    sub.Add(string00, "stringvalue2R0", "stringvalue3R0");
                    sub.Add("stringvalue1R1", "stringvalue2R1", "stringvalue3R1");
                    t.AddEmptyRow(1);
                    t.SetSubTable(1, 0, sub); //should throw an exception as the sub is not with a compatible schema
                    Table subreturned = t.GetSubTable(1, 0);
                    Assert.AreEqual(string00, subreturned.GetString(0, 0));
                }
            }
        }



        [Test]
        [ExpectedException("System.ArgumentNullException")]
        public static void TestCyclicFieldDefinition2()
        {

            var f1 = new AttemptCircularColumnSpec("f1", null);
            //do not care about last parameter we're trying to crash the system
            var subs = new ColumnSpec[2];
            subs[0] = f1;
            f1.Setsubtablearray("f2", subs);

            string actualres;
            using (var t4 = new Table(f1))
            {
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                    "cyclic field definition using field inheritance to get at subtable field list",
                    t4);
            }
            const String expectedres =
                @"------------------------------------------------------
Column count: 1
Table Name  : cyclic field definition
------------------------------------------------------
 0      Table  f10                 
    0        Int  f11                 
    1        Int  f12                 
------------------------------------------------------

";

            TestHelper.Cmp(expectedres, actualres);
        }






        [Test]
        public static void TestIllegalFieldDefinitions4()
        {

            ColumnSpec f10 = "f10".SubTable("f11".Int(), "f12".Int());
            f10.FieldType = DataType.Int;
            //at this time, the subtable array still have some subtables in it
            string actualres;
            using (var t4 = new Table(f10))
            {
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name, "just an int field, no subs", t4);
            }
            const String expectedres =
                @"------------------------------------------------------
Column count: 1
Table Name  : just an int field, no subs
------------------------------------------------------
 0        Int  f10                 
------------------------------------------------------

";

            TestHelper.Cmp(expectedres, actualres);
        }

        [Test]
        public static void TestIllegalFieldDefinitions5()
        {
            ColumnSpec f10 = "f10".SubTable("f11".Int(), "f12".Int());
            f10.FieldType = DataType.Table;

            String actualres;
            using (
                var t5 = new Table(f10))
            {
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name, "subtable with two int fields",
                    t5);
                //This is sort of okay, first adding a subtable, then
            }
            const String expectedres =
                @"------------------------------------------------------
Column count: 1
Table Name  : subtable with two int fields
------------------------------------------------------
 0      Table  f10                 
    0        Int  f11                 
    1        Int  f12                 
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
            //changing mind and making it just and int field, and then changing mind again and setting it as subtable type
            //and thus resurfacing the two subfields. no harm done.
        }

        [Test]
        public static void TestCreateStrangeTable1()
        {
            //create a table with two columns that are the same name except casing (this might be perfectly legal, I dont know)
            String actualres;
            using (var badtable = new Table("Age".Int(), "age".Int()))
            {
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name, "two fields, case is differnt",
                    badtable);
            }
            const String expectedres =
                @"------------------------------------------------------
Column count: 2
Table Name  : two fields, case is differnt
------------------------------------------------------
 0        Int  Age                 
 1        Int  age                 
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
        }

        [Test]
        public static void TestCreateStrangeTable2()
        {
            //Create a table with two columns with the same name and type
            String actualres;
            using (var badtable2 = new Table("Age".Int(), "Age".Int()))
            {
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                    "two fields name and type the same",
                    badtable2);
            }
            const string expectedres =
                @"------------------------------------------------------
Column count: 2
Table Name  : two fields name and type the same
------------------------------------------------------
 0        Int  Age                 
 1        Int  Age                 
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);

        }


        //Test if two table creations where the second happens before the first is out of scope, works okay
        [Test]
        public static void TestCreateTwoTables()
        {
            var actualres = new StringBuilder(); //we add several table dumps into one compare string in this test
            using (
                var testtbl1 = new Table(
                    new ColumnSpec("name", DataType.String),
                    new ColumnSpec("age", DataType.Int),
                    new ColumnSpec("comments",
                        new ColumnSpec("phone#1", DataType.String),
                        new ColumnSpec("phone#2", DataType.String)),
                    new ColumnSpec("whatever", DataType.Mixed)))
            {
                actualres.Append(TestHelper.TableDumperSpec(MethodBase.GetCurrentMethod().Name,
                    "four columns , sub two columns (Field)", testtbl1));

                using ( //and we create a second table while the first is in scope
                    var testtbl2 = new Table(
                        new ColumnSpec("name", "String"),
                        new ColumnSpec("age", "Int"),
                        new ColumnSpec("comments",
                            new ColumnSpec("phone#1", DataType.String), //one way to declare a string
                            new ColumnSpec("phone#2", "String"), //another way
                            "more stuff".SubTable(
                                "stuff1".String(), //and yet another way
                                "stuff2".String(),
                                "ÆØÅæøå".String())
                            ),
                        new ColumnSpec("whatever", DataType.Mixed)))
                {
                    actualres.Append(TestHelper.TableDumperSpec(MethodBase.GetCurrentMethod().Name,
                        "four columns, sub three subsub three", testtbl2));
                }
            }
            File.WriteAllText(MethodBase.GetCurrentMethod().Name + ".txt", actualres.ToString());
            const string expectedres =
                @"------------------------------------------------------
Column count: 4
Table Name  : four columns , sub two columns (Field)
------------------------------------------------------
 0     String  name                
 1        Int  age                 
 2      Table  comments            
    0     String  phone#1             
    1     String  phone#2             
 3      Mixed  whatever            
------------------------------------------------------

------------------------------------------------------
Column count: 4
Table Name  : four columns, sub three subsub three
------------------------------------------------------
 0     String  name                
 1        Int  age                 
 2      Table  comments            
    0     String  phone#1             
    1     String  phone#2             
    2      Table  more stuff          
       0     String  stuff1              
       1     String  stuff2              
       2     String  ÆØÅæøå              
 3      Mixed  whatever            
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres.ToString());
        }

        [Test]
        public static void TestCreateStrangeTable3()
        {
            string actualres;
            using (
                var reallybadtable3 = new Table("Age".Int(),
                    "Age".Int(),
                    "".String(),
                    "".String()))
            {
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                    "same names int two empty string names", reallybadtable3);
            }
            const string expectedres = @"------------------------------------------------------
Column count: 4
Table Name  : same names int two empty string names
------------------------------------------------------
 0        Int  Age                 
 1        Int  Age                 
 2     String                      
 3     String                      
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
        }

        [Test]
        public static void TestCreateStrangeTable4()
        {
            string actualres;
            using (
                var reallybadtable4 = new Table("Age".Int(),
                    "Age".Mixed(),
                    "".String(),
                    "".Mixed()))
            {
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                    "same names, empty names, mixed types", reallybadtable4);
            }
            const string expectedres =
                @"------------------------------------------------------
Column count: 4
Table Name  : same names, empty names, mixed types
------------------------------------------------------
 0        Int  Age                 
 1      Mixed  Age                 
 2     String                      
 3      Mixed                      
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
        }


        [Test]
        public static void TableMaximumDouble()
        {
            using (var myTable = new Table("double".TightDbDouble()))
            {
                myTable.Add(1d);
                Assert.AreEqual(1d, myTable.MaximumDouble(0));
            }
        }

        //should probably be split up into more tests, but this one touches all c++ functions which is okay for now
        [Test]
        public static void TableAggregate()
        {
            using (var myTable = new Table("strfield".String(),
                "int".Int(),
                "float".TightDbFloat(),
                "double".Double())
                )
            {
                myTable.Add("tv", 1, 3f, 5d);
                myTable.Add("tv", 3, 9f, 15d);
                myTable.Add("tv", 5, 15f, 25d);
                myTable.Add("notv", -1000, -1001f, -1002d);

                string actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                    "table with testdata for TableAggregate",
                    myTable);

                const string expectedres =
                    @"------------------------------------------------------
Column count: 4
Table Name  : table with testdata for TableAggregate
------------------------------------------------------
 0     String  strfield            
 1        Int  int                 
 2      Float  float               
 3     Double  double              
------------------------------------------------------

Table Data Dump. Rows:4
------------------------------------------------------
{ //Start row 0
strfield:tv,//column 0
int:1,//column 1
float:3,//column 2
double:5//column 3
} //End row 0
{ //Start row 1
strfield:tv,//column 0
int:3,//column 1
float:9,//column 2
double:15//column 3
} //End row 1
{ //Start row 2
strfield:tv,//column 0
int:5,//column 1
float:15,//column 2
double:25//column 3
} //End row 2
{ //Start row 3
strfield:notv,//column 0
int:-1000,//column 1
float:-1001,//column 2
double:-1002//column 3
} //End row 3
------------------------------------------------------
";
                TestHelper.Cmp(expectedres, actualres);


                using (TableView myTableView = myTable.FindAllString(0, "tv"))
                using (TableView myTableView2 = myTable.FindAllString("strfield","tv"))
                    
                {
                    Assert.AreEqual(3, myTable.CountString(0, "tv"));
                    Assert.AreEqual(1, myTable.CountLong(1, 3));
                    Assert.AreEqual(1, myTable.CountFloat(2, 15f));
                    Assert.AreEqual(1, myTable.CountDouble(3, 15d));

                    Assert.AreEqual(0, myTable.CountString(0, "xtv"));
                    Assert.AreEqual(0, myTable.CountString("strfield", "xtv"));
                    Assert.AreEqual(3, myTable.CountString(0, "tv"));
                    Assert.AreEqual(3, myTable.CountString("strfield", "tv"));
                    Assert.AreEqual(0, myTable.CountLong(1, -3));
                    Assert.AreEqual(0, myTable.CountFloat(2, -15f));
                    Assert.AreEqual(0, myTable.CountDouble(3, -15d));


                    Assert.AreEqual(5, myTable.MaximumLong("int"));
                    Assert.AreEqual(15f, myTable.MaximumFloat("float"));
                    Assert.AreEqual(25d, myTable.MaximumDouble(3));
                    Assert.AreEqual(25d, myTable.MaximumDouble("double"));

                    Assert.AreEqual(-1000, myTable.MinimumLong(1));
                    Assert.AreEqual(-1001f, myTable.MinimumFloat(2));
                    Assert.AreEqual(-1002d, myTable.MinimumDouble(3));
                    Assert.AreEqual(-1000, myTable.MinimumLong("int"));
                    Assert.AreEqual(-1001f, myTable.MinimumFloat("float"));
                    Assert.AreEqual(-1002d, myTable.MinimumDouble("double"));

                    Assert.AreEqual(3f, myTable.GetFloat(2, 0));
                    Assert.AreEqual(9f, myTable.GetFloat(2, 1));
                    Assert.AreEqual(15f, myTable.GetFloat(2, 2));
                    Assert.AreEqual(-1001f, myTable.GetFloat(2, 3));
                    {
                        long sl = myTable.SumLong(1);
                        double sf = myTable.SumFloat(2);
                        double sd = myTable.SumDouble(3);
                        double sftv = myTableView.SumFloat(2);

                        Assert.AreEqual(-1000 + 1 + 3 + 5, sl);
                        Assert.AreEqual(-1001f + 3f + 9f + 15f, sf);
                        Assert.AreEqual(-1002d + 5d + 15d + 25d, sd);
                        Assert.AreEqual( 3f + 9f + 15f, sftv);
                    }


                    {
                        long sl = myTable.SumLong("int");
                        double sf = myTable.SumFloat("float");
                        double sd = myTable.SumDouble("double");
                        double sftv = myTableView.SumFloat("float");

                        Assert.AreEqual(-1000 + 1 + 3 + 5, sl);
                        Assert.AreEqual(-1001f + 3f + 9f + 15f, sf);
                        Assert.AreEqual(-1002d + 5d + 15d + 25d, sd);
                        Assert.AreEqual( 3f + 9f + 15f, sftv);

                    }



                    Assert.AreEqual((1 + 3 + 5 - 1000)/4d, myTable.AverageLong(1));
                    Assert.AreEqual((3f + 9f + 15f - 1001f)/4d, myTable.AverageFloat(2));
                    Assert.AreEqual((5d + 15d + 25d - 1002d)/4d, myTable.AverageDouble(3));
                    Assert.AreEqual((1 + 3 + 5 - 1000) / 4d, myTable.AverageLong("int"));
                    Assert.AreEqual((3f + 9f + 15f - 1001f) / 4d, myTable.AverageFloat("float"));
                    Assert.AreEqual((5d + 15d + 25d - 1002d) / 4d, myTable.AverageDouble("double"));


                    Assert.AreEqual(3, myTableView.Size);
                    //count methods are not implemented in tightdb yet, Until they are implemented, and our c++ binding
                    //is updated to call them, our c++ binding will just return zero
                    Assert.AreEqual(1, myTableView.CountLong(1, 3));
                    Assert.AreEqual(1, myTableView.CountLong("int", 3));
                    Assert.AreEqual(1, myTableView.CountFloat(2, 15f));
                    Assert.AreEqual(1, myTableView2.CountFloat("float", 15f));
                    Assert.AreEqual(1, myTableView2.CountDouble(3, 15d));
                    Assert.AreEqual(1, myTableView.CountDouble("double", 15d));
                    Assert.AreEqual(0 /*3*/, myTableView.CountString(0, "tv"));

                    Assert.AreEqual(5, myTableView.MaximumLong("int"));
                    Assert.AreEqual(5, myTableView.MaximumLong(1));
                    Assert.AreEqual(15f, myTableView.MaximumFloat(2));
                    Assert.AreEqual(15f, myTableView.MaximumFloat("float"));
                    Assert.AreEqual(25d, myTableView.MaximumDouble(3));

                    Assert.AreEqual(1, myTableView.MinimumLong(1));
                    Assert.AreEqual(3f, myTableView.MinimumFloat(2));
                    Assert.AreEqual(5d, myTableView.MinimumDouble(3));

                    Assert.AreEqual(1 + 3 + 5, myTableView.SumLong(1));
                    Assert.AreEqual(3f + 9f + 15f, myTableView.SumFloat("float"));
                    Assert.AreEqual(5d + 15d + 25d, myTableView.SumDouble(3));

                    //average methods are not implemented in tightdb yet, Until they are implemented, and our c++ binding
                    //is updated to call them, our c++ binding will just return zero
                    Assert.AreEqual((3f + 9f + 15f)/3f, myTableView.AverageFloat(2));
                    Assert.AreEqual((5d + 15d + 25d)/3d, myTableView.AverageDouble(3));
                    Assert.AreEqual((1L + 3L + 5L)/3f, myTableView.AverageLong(1));

                }

            }

        }

        //Todo:expand this unit test to see if GetValue and SetValue works with all native types and with all tightdb types
        [Test]
        public static void GetValue()
        {
            using (var table =new  Table(new StringColumn("str")))
            {
                table.AddMany(new[] { "Obi", "Wan", "Kenobi" });
               Assert.AreEqual("Wan", table.GetValue(0, 1));
                table.SetValue(0,0,"Spock");
                Assert.AreEqual("Spock",table.GetString(0,0));
            }
        }

        [Test]
        public static void FindFirstInt()
        {
            using (var table =TableViewTests.TableWithMultipleIntegers())
            {
                Assert.AreEqual(5, table.FindFirstInt(0, 5));
                Assert.AreEqual(50, table.FindFirstInt("intcolumn1", 5));
            }
        }


        [Test]
        public static void FindFirstString()
        {
            using (var table = new Table(new StringColumn("str")))
            {
                table.AddMany(new[]
                {
                    "first",
                    "secodnd",
                    "third",
                    "fourth"
                });                            
                Assert.AreEqual(2,table.FindFirstString(0,"third"));//todo perhaps we could introduce methods with no column indicator that only works if the table has one column
                Assert.AreEqual(2, table.FindFirstString("str", "third"));
            }
        }


        [Test]
        public static void FindFirstDouble()
        {
            using (var table = new Table(new DoubleColumn("dbl")))
            {
                table.AddMany(new[] {1d, 2d, 3d, 4d, 5d, 6d, 7d});                            
                Assert.AreEqual(2, table.FindFirstDouble(0, 3));//todo perhaps we could introduce methods with no column indicator that only works if the table has one column
                Assert.AreEqual(2, table.FindFirstDouble("dbl", 3));
            }
        }

        [Test]
        public static void FindFirstBool()
        {
            using (var table = new Table(new BoolColumn("boo")))
            {
                table.AddMany(new[] {true, true, false, true});            
                Assert.AreEqual(2, table.FindFirstBool(0,false));
                Assert.AreEqual(2, table.FindFirstBool("boo", false));
            }
        }




        [Test]
        public static void FindFirstFloat()
        {
            using (var table = new Table(new FloatColumn("float")))
            {
                table.AddMany(new[]
                {
                    1f, 2f, 3f, 4f, 5f, 6f, 7f
                });
           
                Assert.AreEqual(2, table.FindFirstFloat(0, 3f));//todo perhaps we could introduce methods with no column indicator that only works if the table has one column
                Assert.AreEqual(2, table.FindFirstFloat("float", 3f));
            }
        }

        [Test]
        public static void FindFirstDateTime()
        {
            var basedate  = new DateTime(2001,2,3);

            using (var table = new Table(new DateColumn("time_t")))
            {
                table.AddMany(new[]
                {basedate, basedate.AddDays(1), basedate.AddDays(2), basedate.AddDays(-1), basedate.AddDays(22)});                          
                Assert.AreEqual(2, table.FindFirstDateTime(0, basedate.AddDays(2)));//todo perhaps we could introduce methods with no column indicator that only works if the table has one column
                Assert.AreEqual(2, table.FindFirstDateTime("time_t",basedate.AddDays(2)));
            }
        }


        [Test]
        [ExpectedException("System.ArgumentNullException")]
        public static void AddManyNull()
        {
            using (var table = new Table(new IntColumn("test")))
            {
                table.AddMany(null);
            }
        }


        //report data on screen reg. environment
        [Test]
        public static void ShowVersionTest()
        {
            Table.ShowVersionTest();
        }


        //check that values  in the interface are marshalled correctly on this build and platform combination
        [Test]
        public static void TestInterop()
        {
            Table.TestInterop();
        }


        /// <summary>
        /// Calls GetCInfo and asserts that a string with some kind of info was returned
        /// The visual test of GetInfo is done in the call ShowVersionText above
        /// This GetCInfo method is public on table bc we use it in unity tutorial, so we have
        /// to have a unit test that calls it to avoid warnings.
        /// </summary>
        [Test]
        public static void TestGetCSharpInfo()
        {
            Assert.AreNotEqual("",Table.GetCSharpInfo());
        }

        /// <summary>
        /// Calls GetCInfo and asserts that a string with some kind of info was returned
        /// The visual test of GetInfo is done in the call ShowVersionText above
        /// This GetCInfo method is public on table bc we use it in unity tutorial, so we have
        /// to have a unit test that calls it to avoid warnings.
        /// </summary>
        [Test]
        public static void TestGetCInfo()
        {
            Assert.AreNotEqual("", Table.GetCInfo());
        }


        //tightdb Date is date_t which is seconds since 1970,1,1
        //it is an integer with the number of seconds since 1970,1,1 00:00
        //negative means before 1970,1,1
        //the date is always UTC - not local time


        //C# DateTime is a lot different. Basically an integer tick count since 0001,1,1 00:00 where a tick is 100 microseconds
        //As long as the tightdb Date is 64 bits, tightdb Date has enough range to always keep a C# datetime
        //however the C# datetime has much higher precision, and this precision is lost when stored to tightdb
        //also, a C# DateTime can be of 3 kinds :
        // DateTimeKindUnspecified : Probably local time when it was created, but you really don't know - developer haven't been specific about it
        // DateTimeKindLocal :The time represents a point in time, measured with the currently running machine's culture information and timezone. Daylight rules etc.
        // DateTimeKindUTC : The time represents a point in time, measured in UTC

        //When storing a DateTime, the binding do this :
        //* The DateTime is converted to UTC if it is not already UTC
        //* The time part of the DateTime is truncated to seconds
        //* A tightdb time variable is created from the now compatible DateTime

        //when fetching back a DateTime from tightdb, the binding do this :
        //* The tightdb time variable is converted to a DateTime of kind UTC

        //The convention is that tightdb alwas and only store utc datetime values
        //If You want to store a DateTime unaltered, use DateTime.ToBinary and DateTime.FromBinary and store these values in a int field.(which is always 64 bit)


        [Test]
        public static void TestSaveAndRetrieveDate()
        {
            //this test might not be that effective if being run on a computer whose local time is == utc
            var dateToSaveLocal = new DateTime(1979, 05, 14, 1, 2, 3, DateTimeKind.Local);
            var dateToSaveUtc = new DateTime(1979, 05, 14, 1, 2, 4, DateTimeKind.Utc);
            var dateToSaveUnspecified = new DateTime(1979, 05, 14, 1, 2, 5, DateTimeKind.Unspecified);

            var expectedLocal = new DateTime(1979, 05, 14, 1, 2, 3, DateTimeKind.Local).ToUniversalTime();
            //we expect to get the UTC timepoit resembling the local time we sent
            var expectedUtc = new DateTime(1979, 05, 14, 1, 2, 4, DateTimeKind.Utc);
            //we expect to get the exact same timepoint back, measured in utc
            var expectedUnspecified = new DateTime(1979, 05, 14, 1, 2, 5, DateTimeKind.Local).ToUniversalTime();
            //we expect to get the UTC timepoit resembling the local time we sent

            using (var t = new Table("date1".TightDbDate(), "date2".Mixed(), "stringfield".String()))
                //test date in an ordinary date , as well as date in a mixed
            {

                t.AddEmptyRow(1); //in this row we store datetosavelocal
                t.SetIndex("stringfield");
                t.SetString(2, 0, "str1");
                t.SetDateTime(0, 0, dateToSaveLocal);
                DateTime fromdb = t.GetDateTime("date1", 0);
                DateTime fromdb2 = t[0].GetDateTime("date1");
                Assert.AreEqual(fromdb, fromdb2);
                Assert.AreEqual(fromdb, expectedLocal);

                t.SetMixedDateTime(1, 0, dateToSaveLocal.AddYears(1));
                //one year is added to get a time after 1970.1.1 otherwise we would get an exception with the mixed
                fromdb = t.GetMixedDateTime(1, 0);
                fromdb2 = t[0].GetMixedDateTime(1);
                Assert.AreEqual(fromdb, fromdb2);
                Assert.AreEqual(fromdb, expectedLocal.AddYears(1));


                t.AddEmptyRow(1); //in this row we save datetosaveutc
                t.SetString(2, 1, "str2");
                t.SetDateTime("date1", 1, dateToSaveUtc);
                fromdb = t.GetDateTime("date1", 1);
                fromdb2 = t[1].GetDateTime("date1");
                Assert.AreEqual(fromdb, fromdb2);
                Assert.AreEqual(fromdb, expectedUtc);

                t.SetMixedDateTime("date2", 1, dateToSaveUtc);
                fromdb = t.GetMixedDateTime(1, 1);
                fromdb2 = t[1].GetMixedDateTime(1);
                Assert.AreEqual(fromdb, fromdb2);
                Assert.AreEqual(fromdb, expectedUtc);

                t.AddEmptyRow(1); //in this row we save datetosaveunspecified
                t.SetString(2, 2, "str3");
                t.SetDateTime(0, 2, dateToSaveUnspecified);
                fromdb = t.GetDateTime("date1", 2);
                fromdb2 = t[2].GetDateTime("date1");
                Assert.AreEqual(fromdb, fromdb2);
                Assert.AreEqual(fromdb, expectedUnspecified);

                t.SetMixedDateTime(1, 2, dateToSaveUnspecified);
                fromdb = t.GetMixedDateTime("date2", 2);
                fromdb2 = t[2].GetMixedDateTime("date2");
                Assert.AreEqual(fromdb, fromdb2);
                Assert.AreEqual(fromdb, expectedUnspecified);

                t.SetIndex(2);
                TableView tv = t.Distinct("stringfield");
                //we need a tableview to be able to test the date methods on table views


                tv.SetDateTime(0, 0, dateToSaveUtc);
                fromdb = tv.GetDateTime(0, 0);
                fromdb2 = tv[0].GetDateTime("date1");
                Assert.AreEqual(fromdb, fromdb2);
                Assert.AreEqual(fromdb, expectedUtc);

                tv.SetMixedDateTime(1, 0, dateToSaveUtc);
                fromdb = tv.GetMixedDateTime(1, 0);
                fromdb2 = tv[0].GetMixedDateTime(1);
                Assert.AreEqual(fromdb, fromdb2);
                Assert.AreEqual(fromdb, expectedUtc);



                tv.SetDateTime("date1", 1, dateToSaveUnspecified);
                fromdb = tv.GetDateTime("date1", 1);
                fromdb2 = tv[1].GetDateTime("date1");
                Assert.AreEqual(fromdb, fromdb2);
                Assert.AreEqual(fromdb, expectedUnspecified);

                tv.SetMixedDateTime("date2", 1, dateToSaveUnspecified);
                fromdb = tv.GetMixedDateTime(1, 1);
                fromdb2 = tv[1].GetMixedDateTime(1);
                Assert.AreEqual(fromdb, fromdb2);
                Assert.AreEqual(fromdb, expectedUnspecified);


                tv.SetDateTime(0, 2, dateToSaveLocal);
                fromdb = tv.GetDateTime("date1", 2);
                fromdb2 = tv[2].GetDateTime("date1");
                Assert.AreEqual(fromdb, fromdb2);
                Assert.AreEqual(fromdb, expectedLocal);

                tv.SetMixedDateTime(1, 2, dateToSaveLocal);
                fromdb = tv.GetMixedDateTime("date2", 2);
                fromdb2 = tv[2].GetMixedDateTime("date2");
                Assert.AreEqual(fromdb, fromdb2);
                Assert.AreEqual(fromdb, expectedLocal);


                //at this time there should be three rows in the tableview as the three dates are not exactly the same


            }



        }


        [Test]
        public static void TableMixedCreateEmptySubTable2()
        {
            using (var t = new Table(new MixedColumn("mixd")))
            {
                using (var sub = new Table(new IntColumn("int")))
                {
                    t.AddEmptyRow(1);
                    t.SetMixedSubTable(0, 0, sub);
                }
                t.AddEmptyRow(1);
                t.SetMixedEmptySubTable(0, 0); //i want a new empty subtable in my newly created row
                DataType sttype = t.GetMixedType(0, 0);
                Assert.AreEqual(DataType.Table, sttype);
            }
        }


        [Test]
        public static void TableMixedCreateEmptySubTable()
        {
            using (var t = new Table(new MixedColumn("mixd")))
            {
                t.AddEmptyRow(1);
                t.SetMixedEmptySubTable(0, 0);
                DataType sttype = t.GetMixedType(0, 0);
                Assert.AreEqual(DataType.Table, sttype);
            }
        }

        [Test]
        public static void TableMixedCreateSubTable()
        {
            using (var t = new Table(new MixedColumn("mix'd")))
            {
                using (var subtable = new Table(new IntColumn("int1")))
                {
                    t.AddEmptyRow(1);
                    t.SetMixedSubTable(0, 0, subtable);
                }
                DataType sttype = t.GetMixedType(0, 0);
                Assert.AreEqual(DataType.Table, sttype);
            }
        }

        [Test]
        public static void TableMixedSetGetSubTable()
        {
            using (var t = new Table(new MixedColumn("mix'd")))
            {
                using (var subtable = new Table(new IntColumn("int1")))
                {
                    t.AddEmptyRow(1);
                    t.SetMixedSubTable(0, 0, subtable);
                }
                DataType sttype = t.GetMixedType(0, 0);
                Assert.AreEqual(DataType.Table, sttype);
                Table subback = t.GetMixedSubTable(0, 0);
                Assert.AreEqual(DataType.Int, subback.ColumnType(0));
                Assert.AreEqual("int1", subback.GetColumnName(0));
            }
        }






        //test setting all types to a mixed, and getting them back again correctly
        [Test]
        public static void TableMixedSetTypes()
        {
            using (var table = new Table("mixedfield".Mixed()))
            {
                //listed below are all C# built in value types http://msdn.microsoft.com/en-us/library/ya5y69ds.aspx
                //we should be able to handle them gracefully when sent to a mixed, both when an applicable type is specified, and when
                //we get them as a parameter where we must guess the mixed type to use

                bool testbool = (table.Size == 1);
                const byte testByte = Byte.MaxValue;
                const sbyte testSByte = SByte.MinValue;
                const char testChar = Char.MaxValue;
                const decimal testDecimal = Decimal.MaxValue;
                const double testDouble = Double.MinValue;
                const float testFloat = Single.MaxValue;
                const int testInt = Int32.MinValue;
                const uint testUInt = UInt32.MaxValue;
                const long testLong = Int64.MinValue;
                var testULong = UInt64.MaxValue;
                const short testShort = Int16.MinValue;
                const ushort testUShort = UInt16.MaxValue;
                //these types below are C# reference types that match various tightdb types
                byte[] testBinary = { 1, 3, 5, 7, 11, 13, 17, 23 };
                const string testString = "blah"; //in fact not a value type , but a reference type
                DateTime testDateTime = new DateTime(1990, 1, 1).ToUniversalTime();


                table.AddEmptyRow(1);
                //setting and getting mixed values where tightdb type is specified



                //test setting the basic types using anonymous set on tablerow
                TableRow tablerow = table[0];

                tablerow.SetMixed(0, testBinary);
                Assert.AreEqual(testBinary, tablerow.GetMixedBinary(0));
                Assert.AreEqual(testBinary, tablerow.GetMixedBinary("mixedfield"));

                tablerow.SetMixed(0, testByte);
                Assert.AreEqual(testByte, tablerow.GetMixedLong(0));
                Assert.AreEqual(testByte, tablerow.GetMixedLong("mixedfield"));


                tablerow.SetMixed(0, testChar);
                Assert.AreEqual(testChar, tablerow.GetMixedLong(0));
                Assert.AreEqual(testChar, tablerow.GetMixedLong("mixedfield"));


                tablerow.SetMixedDateTime(0, testDateTime);
                Assert.AreEqual(testDateTime, tablerow.GetMixedDateTime(0));
                Assert.AreEqual(testDateTime, tablerow.GetMixedDateTime("mixedfield"));

                tablerow.SetMixedDateTime("mixedfield", testDateTime);
                Assert.AreEqual(testDateTime, tablerow.GetMixedDateTime(0));
                Assert.AreEqual(testDateTime, tablerow.GetMixedDateTime("mixedfield"));

                tablerow.SetMixed(0, testDateTime);
                Assert.AreEqual(testDateTime, tablerow.GetMixedDateTime(0));
                Assert.AreEqual(testDateTime, tablerow.GetMixedDateTime("mixedfield"));

                try
                {
                    tablerow.SetMixed(0, testDecimal);
                    Assert.Fail("Calling set mixed(object) with a C# type decimal should fail with a type check");
                }
                catch (ArgumentException) //remove the expected exception thrown by setmixed
                {
                }

                tablerow.SetMixed(0, testDouble);
                Assert.AreEqual(testDouble, tablerow.GetMixedDouble(0));
                Assert.AreEqual(testDouble, tablerow.GetMixedDouble("mixedfield"));

                tablerow.SetMixed(0, testFloat);
                Assert.AreEqual(testFloat, tablerow.GetMixedFloat(0));
                Assert.AreEqual(testFloat, tablerow.GetMixedFloat("mixedfield"));

                tablerow.SetMixed(0, testInt);
                Assert.AreEqual(testInt, tablerow.GetMixedLong(0));
                Assert.AreEqual(testInt, tablerow.GetMixedLong("mixedfield"));

                tablerow.SetMixed(0, testLong);
                Assert.AreEqual(testLong, tablerow.GetMixedLong(0));
                Assert.AreEqual(testLong, tablerow.GetMixedLong("mixedfield"));

                tablerow.SetMixed(0, testSByte);
                Assert.AreEqual(testSByte, tablerow.GetMixedLong(0));
                Assert.AreEqual(testSByte, tablerow.GetMixedLong("mixedfield"));

                tablerow.SetMixed(0, testShort);
                Assert.AreEqual(testShort, tablerow.GetMixedLong(0));
                Assert.AreEqual(testShort, tablerow.GetMixedLong("mixedfield"));

                tablerow.SetMixed(0, testString);
                Assert.AreEqual(testString, tablerow.GetMixedString(0));
                Assert.AreEqual(testString, tablerow.GetMixedString("mixedfield"));

                tablerow.SetMixed(0, testUInt);
                Assert.AreEqual(testUInt, tablerow.GetMixedLong(0));
                Assert.AreEqual(testUInt, tablerow.GetMixedLong("mixedfield"));

                //as Tightdb internally uses long, we can't really store an ULong if it is larger than long.Maxvalue
                //also note that reading back a negative long from a column into an ULong is an error
                if (testULong > Int64.MaxValue)
                {
                    testULong = Int64.MaxValue; //or throw. 
                }


                tablerow.SetMixed(0, testULong);
                Assert.AreEqual(testULong, tablerow.GetMixedLong(0));
                Assert.AreEqual(testULong, tablerow.GetMixedLong("mixedfield"));

                tablerow.SetMixed(0, testUShort);
                Assert.AreEqual(testUShort, tablerow.GetMixedLong(0));
                Assert.AreEqual(testUShort, tablerow.GetMixedLong("mixedfield"));

                tablerow.SetMixed(0, testbool);
                Assert.AreEqual(testbool, tablerow.GetMixedBoolean(0));
                Assert.AreEqual(testbool, tablerow.GetMixedBoolean("mixedfield"));





                //test getting the basic types using anonymous get on tablerow (set was verified above)


                tablerow.SetMixed(0, testBinary);
                Assert.AreEqual(testBinary, tablerow.GetMixed(0));
                Assert.AreEqual(testBinary, tablerow.GetMixed("mixedfield"));

                tablerow.SetMixed(0, testByte);
                Assert.AreEqual(testByte, tablerow.GetMixed(0));
                Assert.AreEqual(testByte, tablerow.GetMixed("mixedfield"));

                tablerow.SetMixed(0, testChar);
                var res = (long)tablerow.GetMixed(0);//a direct assert yields an abort inside the unit test when run in resharper (a problem with resharper )
                Assert.AreEqual(testChar, res);

                tablerow.SetMixed(0, testDateTime);
                Assert.AreEqual(testDateTime, tablerow.GetMixed(0));
                Assert.AreEqual(testDateTime, tablerow.GetMixed("mixedfield"));

                try
                {
                    tablerow.SetMixed(0, testDecimal);
                    Assert.Fail("Calling set mixed(object) with a C# type decimal should fail with a type check");
                }
                catch (ArgumentException) //remove the expected exception thrown by setmixed
                {
                }

                tablerow.SetMixed(0, testDouble);
                Assert.AreEqual(testDouble, tablerow.GetMixed(0));

                tablerow.SetMixed(0, testFloat);
                Assert.AreEqual(testFloat, tablerow.GetMixed(0));

                tablerow.SetMixed(0, testInt);
                Assert.AreEqual(testInt, tablerow.GetMixed(0));

                tablerow.SetMixed(0, testLong);
                Assert.AreEqual(testLong, tablerow.GetMixed(0));

                tablerow.SetMixed(0, testSByte);
                Assert.AreEqual(testSByte, tablerow.GetMixed(0));

                tablerow.SetMixed(0, testShort);
                Assert.AreEqual(testShort, tablerow.GetMixed(0));

                tablerow.SetMixed(0, testString);
                Assert.AreEqual(testString, tablerow.GetMixed(0));

                tablerow.SetMixed(0, testUInt);
                Assert.AreEqual(testUInt, tablerow.GetMixed(0));

                tablerow.SetMixed(0, testULong);
                Assert.AreEqual(testULong, tablerow.GetMixed(0));

                tablerow.SetMixed(0, testUShort);
                Assert.AreEqual(testUShort, tablerow.GetMixed(0));

                tablerow.SetMixed(0, testbool);
                Assert.AreEqual(testbool, tablerow.GetMixed(0));
            }

        }





        //test setting all types to a mixed, and getting them back again correctly
        [Test]
        public static void TableMixedSetTypes2()
        {
            using (var table = new Table("mixedfield".Mixed()))
            {
                //listed below are all C# built in value types http://msdn.microsoft.com/en-us/library/ya5y69ds.aspx
                //we should be able to handle them gracefully when sent to a mixed, both when an applicable type is specified, and when
                //we get them as a parameter where we must guess the mixed type to use

                bool testbool = (table.Size == 1);
                const byte testByte = Byte.MaxValue;
                const sbyte testSByte = SByte.MinValue;
                const char testChar = Char.MaxValue;
                const decimal testDecimal = Decimal.MaxValue;
                const double testDouble = Double.MinValue;
                const float testFloat = Single.MaxValue;
                const int testInt = Int32.MinValue;
                const uint testUInt = UInt32.MaxValue;
                const long testLong = Int64.MinValue;
                ulong testULong = UInt64.MaxValue;
                const short testShort = Int16.MinValue;
                const ushort testUShort = UInt16.MaxValue;
                //these types below are C# reference types that match various tightdb types
                byte[] testBinary = {1, 3, 5, 7, 11, 13, 17, 23};
                const string testString = "blah"; //in fact not a value type , but a reference type
                DateTime testDateTime = new DateTime(1990, 1, 1).ToUniversalTime();


                table.AddEmptyRow(1);
                //setting and getting mixed values where tightdb type is specified

                table.SetMixedBool(0, 0, testbool);
                Assert.AreEqual(testbool, table.GetMixedBool(0, 0));
                table.Set(0, testbool);
                Assert.AreEqual(testbool, table.GetMixedBool(0, 0));

                //tightdb does not have a byte column, long is optimized for small values, but of course You don't have 
                //a guarentee that You don't get a value back from a DataType.Int column that is larger than byte size
                table.SetMixedLong(0, 0, testByte);
                Assert.AreEqual(testByte, table.GetMixedLong(0, 0));
                table.Set(0, testByte);
                Assert.AreEqual(testByte, table.GetMixedLong(0, 0));

                table.SetMixedLong(0, 0, testSByte);
                Assert.AreEqual(testSByte, table.GetMixedLong(0, 0));
                table.Set(0, testSByte);
                Assert.AreEqual(testSByte, table.GetMixedLong(0, 0));

                //we do not have any method for storing and getting back a char
                //A string can be "" or several chars, so getting a string field back into a char have unsupported scenarios (string length<>1)
                //A long can have a value that is larger than what a char can contain, or the long can be negative - but it is probably the best fit
                //so this test uses a long

                table.SetMixedLong(0, 0, testChar);
                Assert.AreEqual(testChar, (char) table.GetMixedLong(0, 0));
                table.Set(0, testChar);
                Assert.AreEqual(testChar, (char) table.GetMixedLong(0, 0));

                //we also do not have an applicable database type for decimal.
                //A user would probably stuff them into longs to get them saved
                //or, if he really needs the precision, convert them back and forth to string
                String testdecimalstring = testDecimal.ToString(CultureInfo.InvariantCulture);
                table.SetMixedString(0, 0, testdecimalstring);
                string decimalstringreturned = table.GetMixedString(0, 0);
                Assert.AreEqual(testDecimal, Decimal.Parse(decimalstringreturned, CultureInfo.InvariantCulture));
                table.Set(0, testdecimalstring);
                Assert.AreEqual(testDecimal, Decimal.Parse(decimalstringreturned, CultureInfo.InvariantCulture));


                table.SetMixedDouble(0, 0, testDouble);
                Assert.AreEqual(testDouble, table.GetMixedDouble(0, 0));
                table.Set(0, testDouble);
                Assert.AreEqual(testDouble, table.GetMixedDouble("mixedfield", 0));


                table.SetMixedFloat(0, 0, testFloat);
                Assert.AreEqual(testFloat, table.GetMixedFloat(0, 0));
                table.Set(0, testFloat);
                Assert.AreEqual(testFloat, table.GetMixedFloat("mixedfield", 0));



                table.SetMixedLong(0, 0, testInt);
                Assert.AreEqual(testInt, table.GetMixedLong(0, 0));
                table.Set(0, testInt);
                Assert.AreEqual(testInt, table.GetMixedLong(0, 0));


                table.SetMixedLong(0, 0, testUInt);
                Assert.AreEqual(testUInt, table.GetMixedLong(0, 0));
                table.Set(0, testUInt);
                Assert.AreEqual(testUInt, table.GetMixedLong(0, 0));


                table.SetMixedLong(0, 0, testLong);
                Assert.AreEqual(testLong, table.GetMixedLong(0, 0));
                table.Set(0, testLong);
                Assert.AreEqual(testLong, table.GetMixedLong(0, 0));


                //as Tightdb internally uses long, we can't really store an ULong if it is larger than long.Maxvalue
                //also note that reading back a negative long from a column into an ULong is an error
                if (testULong > Int64.MaxValue)
                {
                    testULong = Int64.MaxValue; //or throw. 
                }

                table.SetMixedLong(0, 0, (long) testULong);
                Assert.AreEqual(testULong, table.GetMixedLong(0, 0));
                table.Set(0, testULong);
                Assert.AreEqual(testULong, table.GetMixedLong(0, 0));


                table.SetMixedLong(0, 0, testShort);
                Assert.AreEqual(testShort, table.GetMixedLong(0, 0));
                table.Set(0, testShort);
                Assert.AreEqual(testShort, table.GetMixedLong(0, 0));

                table.SetMixedLong(0, 0, testUShort);
                Assert.AreEqual(testUShort, table.GetMixedLong(0, 0));
                table.Set(0, testUShort);
                Assert.AreEqual(testUShort, table.GetMixedLong("mixedfield", 0));

                table.SetMixedBinary(0, 0, testBinary);
                Assert.AreEqual(testBinary, table.GetMixedBinary(0, 0));
                table.Set(0, testBinary);
                Assert.AreEqual(testBinary, table.GetMixedBinary("mixedfield", 0));

                table.SetMixedString("mixedfield", 0, testString);
                Assert.AreEqual(testString, table.GetMixedString(0, 0));
                table.Set(0, testString);
                Assert.AreEqual(testString, table.GetMixedString(0, 0));

                table.SetMixedDateTime(0, 0, testDateTime);
                Assert.AreEqual(testDateTime.ToUniversalTime(), table.GetMixedDateTime(0, 0).ToUniversalTime());
                table.Set(0, testDateTime);
                Assert.AreEqual(testDateTime.ToUniversalTime(), table.GetMixedDateTime(0, 0).ToUniversalTime());


                //test setting the basic types using anonymous set on table
                table.SetMixed(0, 0, testBinary);
                Assert.AreEqual(testBinary, table.GetMixedBinary(0, 0));

                table.SetMixed(0, 0, testByte);
                Assert.AreEqual(testByte, table.GetMixedLong(0, 0));

                table.SetMixed(0, 0, testChar);
                Assert.AreEqual(testChar, table.GetMixedLong(0, 0));

                table.SetMixed(0, 0, testDateTime);
                Assert.AreEqual(testDateTime, table.GetMixedDateTime(0, 0));

                try
                {
                    table.SetMixed(0, 0, testDecimal);
                    Assert.Fail("Calling set mixed(object) with a C# type decimal should fail with a type check");
                }
                catch (ArgumentException) //remove the expected exception thrown by setmixed
                {
                }

                table.SetMixed(0, 0, testDouble);
                Assert.AreEqual(testDouble, table.GetMixedDouble(0, 0));

                table.SetMixed(0, 0, testFloat);
                Assert.AreEqual(testFloat, table.GetMixedFloat(0, 0));

                table.SetMixed(0, 0, testInt);
                Assert.AreEqual(testInt, table.GetMixedLong(0, 0));

                table.SetMixed(0, 0, testLong);
                Assert.AreEqual(testLong, table.GetMixedLong(0, 0));

                table.SetMixed(0, 0, testSByte);
                Assert.AreEqual(testSByte, table.GetMixedLong(0, 0));

                table.SetMixed(0, 0, testShort);
                Assert.AreEqual(testShort, table.GetMixedLong(0, 0));

                table.SetMixed(0, 0, testString);
                Assert.AreEqual(testString, table.GetMixedString(0, 0));

                table.SetMixed(0, 0, testUInt);
                Assert.AreEqual(testUInt, table.GetMixedLong(0, 0));

                table.SetMixed(0, 0, testULong);
                Assert.AreEqual(testULong, table.GetMixedLong(0, 0));

                table.SetMixed(0, 0, testUShort);
                Assert.AreEqual(testUShort, table.GetMixedLong(0, 0));

                table.SetMixed(0, 0, testbool);
                Assert.AreEqual(testbool, table.GetMixedBool(0, 0));


                //test table getmixed

                //test setting the basic types using anonymous set on table
                table.SetMixed(0, 0, testBinary);
                Assert.AreEqual(testBinary, table.GetMixedBinary(0, 0));

                table.SetMixed(0, 0, testByte);
                Assert.AreEqual(testByte, table.GetMixedLong(0, 0));

                table.SetMixed(0, 0, testChar);
                Assert.AreEqual(testChar, table.GetMixedLong(0, 0));

                table.SetMixed(0, 0, testDateTime);
                Assert.AreEqual(testDateTime, table.GetMixedDateTime(0, 0));

                try
                {
                    table.SetMixed(0, 0, testDecimal);
                    Assert.Fail("Calling set mixed(object) with a C# type decimal should fail with a type check");
                }
                catch (ArgumentException) //remove the expected exception thrown by setmixed
                {
                }

                table.SetMixed(0, 0, testDouble);
                Assert.AreEqual(testDouble, table.GetMixed(0, 0));

                table.SetMixed(0, 0, testFloat);
                Assert.AreEqual(testFloat, table.GetMixed(0, 0));

                table.SetMixed(0, 0, testInt);
                Assert.AreEqual(testInt, table.GetMixed(0, 0));

                table.SetMixed(0, 0, testLong);
                Assert.AreEqual(testLong, table.GetMixed(0, 0));

                table.SetMixed(0, 0, testSByte);
                Assert.AreEqual(testSByte, table.GetMixed(0, 0));

                table.SetMixed(0, 0, testShort);
                Assert.AreEqual(testShort, table.GetMixed(0, 0));

                table.SetMixed(0, 0, testString);
                Assert.AreEqual(testString, table.GetMixed(0, 0));

                table.SetMixed(0, 0, testUInt);
                Assert.AreEqual(testUInt, table.GetMixed(0, 0));

                table.SetMixed(0, 0, testULong);
                Assert.AreEqual(testULong, table.GetMixed(0, 0));

                table.SetMixed(0, 0, testUShort);
                Assert.AreEqual(testUShort, table.GetMixed(0, 0));

                table.SetMixed(0, 0, testbool);
                Assert.AreEqual(testbool, table.GetMixed(0, 0));




            }
        }

        /* this test exposes an error with resharper that makes it about unit tests.. in general
         * don't Assert.AreEqual(char,object(long)) it crashes!
         * uncomment this test from time to time to see if resharper have fixed their stuff
        [Test]
        public static void NunitAbortErrror()
        {
            var llong = 10L;
            object olong = llong;
            const char testChar = char.MaxValue;
            Assert.AreEqual(testChar,olong);
        }
        */

        [Test]
        public static void TableMixedSetGetSubTableWithData()
        {

            string actualres;

            using (var t = new Table(new MixedColumn("mix'd")))
            {
                using (var subtable = new Table(new IntColumn("int1")))
                {
                    t.AddEmptyRow(1);
                    subtable.AddEmptyRow(1);
                    subtable.SetInt(0, 0, 42);
                    t.SetMixedSubTable(0, 0, subtable);
                }
                t.AddEmptyRow(1);
                t.SetMixedLong(0, 1, 84);
                DataType sttype = t.GetMixedType(0, 0);
                Assert.AreEqual(DataType.Table, sttype);
                Table subback = t.GetMixedSubTable(0, 0);
                Assert.AreEqual(DataType.Int, subback.ColumnType(0));
                Assert.AreEqual("int1", subback.GetColumnName(0));
                long databack = subback.GetLong(0, 0);
                Assert.AreEqual(42, databack);
                Assert.AreEqual(DataType.Int, t.GetMixedType(0, 1));
                Assert.AreEqual(84, t.GetMixedLong(0, 1));
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name, "sub in mixed with int", t);

            }


            const string expectedres = @"------------------------------------------------------
Column count: 1
Table Name  : sub in mixed with int
------------------------------------------------------
 0      Mixed  mix'd               
------------------------------------------------------

Table Data Dump. Rows:2
------------------------------------------------------
{ //Start row 0
mix'd:   { //Start row 0
   int1:42   //column 0
   } //End row 0
//column 0//Mixed type is Table
} //End row 0
{ //Start row 1
mix'd:84//column 0//Mixed type is Int
} //End row 1
------------------------------------------------------
";



            TestHelper.Cmp(expectedres, actualres);
        }



        [Test]
        public static void TableSubTableSubTableTwo()
        {
            //string actualres1;
            //string actualres2;
            //string actualres3;
            //string actualres4;
            //string actualres5;
            string actualres;

            using (var t = new Table(
                "fld1".String(),
                "root".TightDbSubTable(
                    "fld2".String(),
                    "fld3".String(),
                    "s1".SubTable(
                        "fld4".String(),
                        "fld5".String(),
                        "fld6".String(),
                        "s2".Table(
                            "fld".Int())))))
            {

                //   t.AddEmptyRow(1);
                t.AddEmptyRow(1); //add empty row

                Assert.AreEqual(1, t.Size);
                Table root = t.GetSubTable(1, 0);
                root.AddEmptyRow(1);
                Assert.AreEqual(1, root.Size);

                Table s1 = root.GetSubTable(2, 0);
                s1.AddEmptyRow(1);
                Assert.AreEqual(1, s1.Size);

                Table s2 = s1.GetSubTable(3, 0);
                s2.AddEmptyRow(1);

                const long valueinserted = 42;
                s2.SetLong("fld", 0, valueinserted);
                Assert.AreEqual(1, s2.Size);

                //now read back the 42

                long valueback = t.GetSubTable(1, 0).GetSubTable(2, 0).GetSubTable(3, 0).GetLong(0, 0);
                //                            root               s1                 s2            42


                Assert.AreEqual(valueback, valueinserted);
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name + 5,
                    "subtable in subtable with int",
                    t);
            }
            const string expectedres = @"------------------------------------------------------
Column count: 2
Table Name  : subtable in subtable with int
------------------------------------------------------
 0     String  fld1                
 1      Table  root                
    0     String  fld2                
    1     String  fld3                
    2      Table  s1                  
       0     String  fld4                
       1     String  fld5                
       2     String  fld6                
       3      Table  s2                  
          0        Int  fld                 
------------------------------------------------------

Table Data Dump. Rows:1
------------------------------------------------------
{ //Start row 0
fld1:,//column 0
root:[ //1 rows   { //Start row 0
   fld2:   ,//column 0
   fld3:   ,//column 1
   s1:[ //1 rows      { //Start row 0
      fld4:      ,//column 0
      fld5:      ,//column 1
      fld6:      ,//column 2
      s2:[ //1 rows         { //Start row 0
         fld:42         //column 0
         } //End row 0
]      //column 3
      } //End row 0
]   //column 2
   } //End row 0
]//column 1
} //End row 0
------------------------------------------------------
";
            TestHelper.Cmp(expectedres, actualres);
        }



        [Test]
        public static void TableSubTableSubTableClone()
        {
            //string actualres1;
            //string actualres2;
            //string actualres3;
            //string actualres4;
            //string actualres5;
            string actualres;

            using (var t = new Table(
                "fld1".String(),
                "root".SubTable(
                    "fld2".String(),
                    "fld3".String(),
                    "s1".SubTable(
                        "fld4".String(),
                        "fld5".String(),
                        "fld6".String(),
                        "s2".Table(
                            "fld".Int())))))
            {

                //   t.AddEmptyRow(1);
                t.AddEmptyRow(1); //add empty row

                Assert.AreEqual(1, t.Size);
                Table root = t.GetSubTable(1, 0);
                root.AddEmptyRow(1);
                Assert.AreEqual(1, root.Size);

                Table s1 = root.GetSubTable(2, 0);
                s1.AddEmptyRow(1);
                Assert.AreEqual(1, s1.Size);

                Table s2 = s1.GetSubTable(3, 0);
                s2.AddEmptyRow(1);

                const int  valueinserted = 42;
                s2.SetInt(0, 0, valueinserted);
                Assert.AreEqual(1, s2.Size);

                //now read back the 42

                long valueback = t.GetSubTable(1, 0).GetSubTable(2, 0).GetSubTable(3, 0).GetLong(0, 0);
                //                            root               s1                 s2            42


                Assert.AreEqual(valueback, valueinserted);
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name + 5,
                    "subtable in subtable with int",
                    t.Clone());
            }
            const string expectedres = @"------------------------------------------------------
Column count: 2
Table Name  : subtable in subtable with int
------------------------------------------------------
 0     String  fld1                
 1      Table  root                
    0     String  fld2                
    1     String  fld3                
    2      Table  s1                  
       0     String  fld4                
       1     String  fld5                
       2     String  fld6                
       3      Table  s2                  
          0        Int  fld                 
------------------------------------------------------

Table Data Dump. Rows:1
------------------------------------------------------
{ //Start row 0
fld1:,//column 0
root:[ //1 rows   { //Start row 0
   fld2:   ,//column 0
   fld3:   ,//column 1
   s1:[ //1 rows      { //Start row 0
      fld4:      ,//column 0
      fld5:      ,//column 1
      fld6:      ,//column 2
      s2:[ //1 rows         { //Start row 0
         fld:42         //column 0
         } //End row 0
]      //column 3
      } //End row 0
]   //column 2
   } //End row 0
]//column 1
} //End row 0
------------------------------------------------------
";
            TestHelper.Cmp(expectedres, actualres);
        }





        [Test]
        public static void TableIntValueTest2()
        {
            String actualres;

            using (var t = GetTableWithIntegers(false))
            {
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name, "table with a few integers in it",
                    t);
            }


            const string expectedres = @"------------------------------------------------------
Column count: 3
Table Name  : table with a few integers in it
------------------------------------------------------
 0        Int  IntColumn1          
 1        Int  IntColumn2          
 2        Int  IntColumn3          
------------------------------------------------------

Table Data Dump. Rows:3
------------------------------------------------------
{ //Start row 0
IntColumn1:1764,//column 0
IntColumn2:0,//column 1
IntColumn3:0//column 2
} //End row 0
{ //Start row 1
IntColumn1:0,//column 0
IntColumn2:-9223372036854775808,//column 1
IntColumn3:0//column 2
} //End row 1
{ //Start row 2
IntColumn1:0,//column 0
IntColumn2:0,//column 1
IntColumn3:-9223372036854775766//column 2
} //End row 2
------------------------------------------------------
";
            TestHelper.Cmp(expectedres, actualres);
        }



        [Test]
        public static void TableIntValueSubTableTest1()
        {
            String actualres;

            using (var t = GetTableWithIntegers(true))
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                    "table with a few integers in it", t);

            const string expectedres =
                @"------------------------------------------------------
Column count: 4
Table Name  : table with a few integers in it
------------------------------------------------------
 0        Int  IntColumn1          
 1        Int  IntColumn2          
 2        Int  IntColumn3          
 3      Table  SubTableWithInts    
    0        Int  SubIntColumn1       
    1        Int  SubIntColumn2       
    2        Int  SubIntColumn3       
------------------------------------------------------

Table Data Dump. Rows:3
------------------------------------------------------
{ //Start row 0
IntColumn1:1764,//column 0
IntColumn2:0,//column 1
IntColumn3:0,//column 2
SubTableWithInts:[ //3 rows   { //Start row 0
   SubIntColumn1:2   ,//column 0
   SubIntColumn2:0   ,//column 1
   SubIntColumn3:0   //column 2
   } //End row 0
   { //Start row 1
   SubIntColumn1:0   ,//column 0
   SubIntColumn2:0   ,//column 1
   SubIntColumn3:0   //column 2
   } //End row 1
   { //Start row 2
   SubIntColumn1:0   ,//column 0
   SubIntColumn2:0   ,//column 1
   SubIntColumn3:0   //column 2
   } //End row 2
]//column 3
} //End row 0
{ //Start row 1
IntColumn1:0,//column 0
IntColumn2:-9223372036854775808,//column 1
IntColumn3:0,//column 2
SubTableWithInts:[ //3 rows   { //Start row 0
   SubIntColumn1:0   ,//column 0
   SubIntColumn2:0   ,//column 1
   SubIntColumn3:0   //column 2
   } //End row 0
   { //Start row 1
   SubIntColumn1:0   ,//column 0
   SubIntColumn2:2   ,//column 1
   SubIntColumn3:0   //column 2
   } //End row 1
   { //Start row 2
   SubIntColumn1:0   ,//column 0
   SubIntColumn2:0   ,//column 1
   SubIntColumn3:0   //column 2
   } //End row 2
]//column 3
} //End row 1
{ //Start row 2
IntColumn1:0,//column 0
IntColumn2:0,//column 1
IntColumn3:-9223372036854775766,//column 2
SubTableWithInts:[ //3 rows   { //Start row 0
   SubIntColumn1:0   ,//column 0
   SubIntColumn2:0   ,//column 1
   SubIntColumn3:0   //column 2
   } //End row 0
   { //Start row 1
   SubIntColumn1:0   ,//column 0
   SubIntColumn2:0   ,//column 1
   SubIntColumn3:0   //column 2
   } //End row 1
   { //Start row 2
   SubIntColumn1:0   ,//column 0
   SubIntColumn2:0   ,//column 1
   SubIntColumn3:2   //column 2
   } //End row 2
]//column 3
} //End row 2
------------------------------------------------------
";
            TestHelper.Cmp(expectedres, actualres);
        }
    }


    //tabletests split in two in order to hunt a vs2012 test runner error where it hung doing table tests
    [TestFixture]
    public static class TableTests2
    {


        [Test]
        public static void TableRowColumnInsert()
        {
            String actualres;
            using (
                var t = new Table(new IntColumn("intfield"), new StringColumn("stringfield"), new IntColumn("intfield2")))
            {
                t.AddEmptyRow(5);
                long changeNumber = 0;
                foreach (TableRow tr in t)
                {
                    foreach (RowCell trc in tr)
                    {
                        if (trc.ColumnType == DataType.Int)
                            trc.Value = ++changeNumber;

                    }
                }
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                    "integers set from within trc objects", t);
            }


            const string expectedres =
                @"------------------------------------------------------
Column count: 3
Table Name  : integers set from within trc objects
------------------------------------------------------
 0        Int  intfield            
 1     String  stringfield         
 2        Int  intfield2           
------------------------------------------------------

Table Data Dump. Rows:5
------------------------------------------------------
{ //Start row 0
intfield:1,//column 0
stringfield:,//column 1
intfield2:2//column 2
} //End row 0
{ //Start row 1
intfield:3,//column 0
stringfield:,//column 1
intfield2:4//column 2
} //End row 1
{ //Start row 2
intfield:5,//column 0
stringfield:,//column 1
intfield2:6//column 2
} //End row 2
{ //Start row 3
intfield:7,//column 0
stringfield:,//column 1
intfield2:8//column 2
} //End row 3
{ //Start row 4
intfield:9,//column 0
stringfield:,//column 1
intfield2:10//column 2
} //End row 4
------------------------------------------------------
";
            TestHelper.Cmp(expectedres, actualres);
        }

        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableGetMixedWithNonMixedField()
        {

            using (var t = new Table(new IntColumn("int1"), new MixedColumn("mixed1")))
            {
                t.AddEmptyRow(1);
                t.SetInt(0, 0, 42);
                t.SetMixedLong(1, 0, 43);
                long intfromnonmixedfield = t.GetMixedLong(0, 0);
                Assert.AreEqual(42, intfromnonmixedfield); //we should never get this far
                Assert.AreNotEqual(42, intfromnonmixedfield); //we should never get this far
            }
        }

        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableEmptyTableFieldAccess()
        {
            using (var t = new Table(new IntColumn("Int1"), new IntColumn("Int2"), new IntColumn("Int3")))
            {
                //accessing a row on an empty table should not be allowed
                long value = t.GetLong(0, 0);
                Assert.AreEqual(value,value+1);//force fail if we get this far - we should not. Using value to avoid compiler warning
            }
        }

        [Test]
        public static void TableGetBoolean()
        {
            using (var table = new Table(new BoolColumn("boo")))
            {
                table.AddMany(new[] {true, false});            
                Assert.AreEqual(true,table.GetBoolean(0,0));
                Assert.AreEqual(false, table.GetBoolean("boo", 1));
            }
        }

        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableEmptyTableFieldAccessWrite()
        {
            using (var t = new Table(new IntColumn("Int1"), new IntColumn("Int2"), new IntColumn("Int3")))
            {
                //accessing a row on an empty table should not be allowed
                t.SetInt(0, 0, 42); //this should throw
                long value = t.GetLong(0, 0);
                Assert.AreEqual(6666,value);//we should never get this far, so if we do, fail fail fail
            }
        }



        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableRowIndexTooLow()
        {
            using (var t = new Table(new IntColumn("Int1"), new IntColumn("Int2"), new IntColumn("Int3")))
            {
                //accessing a row on an empty table should not be allowed
                t.AddEmptyRow(1);
                var value = t.GetLong(0, -1);
                Assert.AreEqual(value+1,value);//we cannot get this far, if we do, fail!
            }
        }

        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableRowIndexTooLowWrite()
        {
            using (var t = new Table(new IntColumn("Int1"), new IntColumn("Int2"), new IntColumn("Int3")))
            {
                //accessing a row on an empty table should not be allowed
                t.AddEmptyRow(1);
                t.SetInt(0, -1, 42); //should throw
                long value = t.GetLong(0, -1);
                Assert.AreEqual(value,value+1);//we should never get this far
            }
        }


        //
        [Test]
        public static void TableGetUndefinedMixedType()
        {
            using (var t = new Table(new MixedColumn("MixedField")))
            {
                t.AddEmptyRow(1);
                DataType dt = t.GetMixedType(0, 0);
                Assert.AreEqual(dt, DataType.Int);
            }
        }


        [Test]
        public static void TableMixedInt()
        {
            using (var t = new Table(new MixedColumn("MixedField")))
            {
                t.AddEmptyRow(1);
                t.SetMixedLong(0, 0, 42);
                DataType dt = t.GetMixedType(0, 0);
                Assert.AreEqual(dt, DataType.Int);
                long fortytwo = t.GetMixedLong(0, 0);
                Assert.AreEqual(42, fortytwo);
            }
        }


        [Test]
        public static void TableMixedString()
        {
            using (var t = new Table(new MixedColumn("StringField")))
            {
                const string setWithAdd = "SetWithAdd";
                const string setWithSetMixed = "SetWithSetMixed";
                t.Add(setWithAdd);
                DataType dtRow0 = t.GetMixedType(0, 0);
                Assert.AreEqual(DataType.String, dtRow0); //mixed from empty rows added are int as a default
                String row0 = t.GetMixedString(0, 0);
                Assert.AreEqual(setWithAdd, row0);
                row0 = t.GetMixedString("StringField", 0);
                Assert.AreEqual(setWithAdd, row0);
                t.AddEmptyRow(1);
                t.SetMixedString(0, 1, setWithSetMixed);
                DataType dtRow1 = t.GetMixedType(0, 1);
                Assert.AreEqual(DataType.String, dtRow1);
                String row1 = t.GetMixedString(0, 1);
                Assert.AreEqual(setWithSetMixed, row1);

            }
        }


        [Test]
        public static void TableMixedDateTime()
        {
            var testDate = new DateTime(2000, 1, 2, 3, 4, 5, DateTimeKind.Utc);
            using (var t = new Table(new MixedColumn("MixedField")))
            {
                t.AddEmptyRow(1);
                t.SetMixedDateTime(0, 0, testDate);
                DataType dt = t.GetMixedType(0, 0);
                Assert.AreEqual(DataType.Date, dt);
                DateTime fromDb = t.GetMixedDateTime(0, 0);
                Assert.AreEqual(testDate, fromDb);
            }
        }

        [Test]
        public static void TableLast()
        {
            using (var t = new Table("i".Int()))
            {
                t.AddEmptyRow(10);
                var cursor = t.Last();
                Assert.AreEqual(9, cursor.RowIndex);
            }
        }

        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableRowIndexTooHigh()
        {
            using (var t = new Table(new IntColumn("Int1"), new IntColumn("Int2"), new IntColumn("Int3")))
            {
                //accessing a row on an empty table should not be allowed
                t.AddEmptyRow(1);
                long value = t.GetLong(0, 1);
                Assert.AreEqual(value,value+1);
            }
        }

        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableRowIndexTooHighWrite()
        {
            using (var t = new Table(new IntColumn("Int1"), new IntColumn("Int2"), new IntColumn("Int3")))
            {
                //accessing a row on an empty table should not be allowed
                t.AddEmptyRow(1);
                t.SetLong(0, 1, 42); //should throw
                long value = t.GetLong(0, 1);
                Assert.AreEqual(value,value+1);
            }
        }



        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableColumnIndexTooLow()
        {
            using (var t = new Table(new IntColumn("Int1"), new IntColumn("Int2"), new IntColumn("Int3")))
                //accessing an illegal column should also not be allowed
            {
                t.AddEmptyRow(1);
                long value2 = t.GetLong(-1, 0);
                Assert.AreEqual(value2,value2+1);
            }
        }

        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableColumnIndexTooLowWrite()
        {
            using (var t = new Table(new IntColumn("Int1"), new IntColumn("Int2"), new IntColumn("Int3")))
                //accessing an illegal column should also not be allowed
            {
                t.AddEmptyRow(1);
                t.SetLong(-1, 0, 42);
                long value2 = t.GetLong(-1, 0);
                Assert.AreEqual(value2,value2+1);
            }
        }


        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableColumnIndexTooHigh()
        {
            using (var t = new Table(new IntColumn("Int1"), new IntColumn("Int2"), new IntColumn("Int3")))
                //accessing an illegal column should also not be allowed
            {
                t.AddEmptyRow(1);
                long value2 = t.GetLong(3, 0);
                Assert.AreEqual(value2,value2+1);
            }
        }


        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableColumnIndexTooHighWrite()
        {
            using (var t = new Table(new IntColumn("Int1"), new IntColumn("Int2"), new IntColumn("Int3")))
                //accessing an illegal column should also not be allowed
            {
                t.AddEmptyRow(1);
                t.SetLong(3, 0, 42);
                long value2 = t.GetLong(3, 0);
                Assert.AreEqual(value2,value2+1);
            }
        }


        //both tableview and table tojson is tested here
        [Test]
        public static void TableToJsonTest()
        {
            using (var t = new Table(
                new IntColumn("Int1"),
                new IntColumn("Int2"),
                new IntColumn("Int3"),
                new IntColumn("Int4")))
            {
                t.Add(42, 7, 3, 2);
                t.Add(12, 1, 2, 1);
                string actualres = t.ToJson();

                const string expectedres =
                    "[{\"Int1\":42,\"Int2\":7,\"Int3\":3,\"Int4\":2},{\"Int1\":12,\"Int2\":1,\"Int3\":2,\"Int4\":1}]";
                TestHelper.Cmp(expectedres, actualres);

                TableView tv = t.FindAllInt(0, 42);
                actualres = tv.ToJson();
                const string expectedres2 = "[{\"Int1\":42,\"Int2\":7,\"Int3\":3,\"Int4\":2}]";
                TestHelper.Cmp(expectedres2, actualres);

                TableView tvs = t.FindAllInt("Int1", 42);
                actualres = tvs.ToJson();
                const string expectedres3 = "[{\"Int1\":42,\"Int2\":7,\"Int3\":3,\"Int4\":2}]";
                TestHelper.Cmp(expectedres3, actualres);


            }
        }


        [Test]
        [ExpectedException("System.ArgumentException")]
        public static void TableIllegalType()
        {
            using (var t = new Table(new IntColumn("Int1"), new IntColumn("Int2"), new IntColumn("Int3")))
                //accessing an illegal column should also not be allowed
            {
                t.AddEmptyRow(1);
                //likewise accessing the wrong type should not be allowed
                Table t2 = t.GetSubTable(1, 0);
                Assert.AreEqual(42, t2.Size);//this should never run
            }
        }


/* This unit test failed by exhausting all memory due to a memleak in c++ core
 * Takes a long time to run, so have been commented out
        [Test]
        public static void Tablesetsubtablebug()
        {

           

            Group g = new Group();

            g.CreateTable("Systems",
              //  "Cash".Double(),
               // "Position".TightDbInt(),
//                "Borrrowed".Double(),
                "NAV".Int());

            g.CreateTable("TICKS",
//                "date".TightDbDate(),
//                "Bid".Double(),
//                "Ask".Double(),
                "systemresults".Table(
                 //   "Cash".Double(),
                 //   "Position".TightDbInt(),
//                    "Borrrowed".Double(),
                    "NAV".Int()));



            Table ticks = g.GetTable("TICKS");
            ticks.AddEmptyRow(1);

            Table systems = g.GetTable("Systems");
            for (var n = 0; n <= 1024 * 1024 * 8; n++)
            {
                try
                {
                 //   using (var t = ticks.GetSubTableNoCheck(0, 0))
                    {

                        {
                          //  ticks.ValidateEqualScheme(t, systems, "SetSubTable");
                            ticks.SetSubTableNoCheck(0, 0, systems);
                        }
                    }
                    //ticks.SetSubTable(3, 0, systems);
                }
                catch (Exception x)//error here is due to an error in core.
                {
                    Console.WriteLine(string.Format("BackTest finished with errors from tightdb {0}  tick.rowindex was {1} intptr.size {2}", x.Message, n, IntPtr.Size));
                    throw;
                }
            }          
        }
        */


        /*failed due to a b u g in c++ core
         * Takes a long time to run, so have been commented out
        [Test]
        public static void Tablesetsubtablebugnogroup()
        {            

          Table systems = new Table("NAV".Int());

            Table ticks = new Table(
                "systemresults".Table(
                    "NAV".Int()));
          
            ticks.AddEmptyRow(1);
          
            for (var n = 0; n <= 1024 * 1024 * 16; n++)//error here is due to a core b u g
            {
                try
                {
                            ticks.SetSubTableNoCheck(0, 0, systems);
                }
                catch (Exception x)
                {
                    Console.WriteLine(string.Format("BackTest finished with errors from tightdb {0}  tick.rowindex was {1} intptr.size {2}", x.Message, n, IntPtr.Size));
                    throw;
                }
            }
        }

        */

       [Test]
        [ExpectedException("System.ArgumentException")]
        public static void TableSetIllegalType()
        {
            using (var t = new Table(new SubTableColumn("sub1"), new IntColumn("Int2"), new IntColumn("Int3")))
                //accessing an illegal column should also not be allowed
            {
                t.AddEmptyRow(1);
                //likewise accessing the wrong type should not be allowed               
                t.SetLong(0, 0, 42); //should throw                
                Assert.Fail("SetLong on subtable field did not throw");
            }
        }
    }
}
