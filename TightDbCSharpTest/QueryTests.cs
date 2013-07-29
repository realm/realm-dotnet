using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using TightDbCSharp;
using TightDbCSharp.Extensions;
using System.Reflection;



namespace TightDbCSharpTest
{
    [TestFixture]
    public static class QueryTests
    {

        //returns a table with row 0 having ints 0 to 999 ascending
        //row 1 having ints 0 to 99 ascendig (10 of each)
        //row 2 having ints 0 to 9 asceding (100 of each)
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public static Table GetTableWithMultipleIntegers()
        {
            var t = new Table(new IntField("intcolumn0"), new IntField("intcolumn1"), new IntField("intcolumn2"));

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

            return t;
        }


        [Test]
        public static void QueryBoolEqual()
        {
            using (var t = new Table("stringfield".String(), "boolfield".Bool(), "stringfield2".String(), "boolfield2".Bool()))
            {

                t.AddEmptyRow(5);
                t.SetBoolean(1, 0, true);
                t.SetBoolean(1, 1, false);
                t.SetBoolean(1, 2, false);
                t.SetBoolean(1, 3, true);
                t.SetBoolean(1, 4, true);

                t.SetBoolean(3, 0, true);
                t.SetBoolean(3, 1, true);
                t.SetBoolean(3, 2, true);
                t.SetBoolean(3, 3, true);
                t.SetBoolean(3, 4, true);

                TableView tv = t.Where().Equal("boolfield", true).FindAll();
                Assert.AreEqual(tv.Size, 3);
                tv = t.Where().Equal("boolfield", false).FindAll();
                Assert.AreEqual(tv.Size, 2);
                tv = t.Where().Equal("boolfield2", false).FindAll();
                Assert.AreEqual(tv.Size, 0);
                tv = t.Where().Equal("boolfield2", true).FindAll();
                Assert.AreEqual(tv.Size, 5);
            }
        }

        [Test]
        public static void QueryGetColumnName()
        {
            using (var t = GetTableWithMultipleIntegers())
            {
                Query q = t.Where();
                long intcolumnix = q.GetColumnIndex("intcolumn1");
                Console.WriteLine(intcolumnix);
                Assert.AreEqual(1, intcolumnix);
            }
        }



        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void CreateQueryStartNegative()
        {
            using (var t = GetTableWithMultipleIntegers())
            {
                TableView tv = t.Where().FindAll(-2, 4, 100);
                Console.WriteLine(tv.Size);
            }
        }

        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void CreateQueryEndNegative()
        {
            using (var t = GetTableWithMultipleIntegers())
            {
                TableView tv = t.Where().FindAll(1, -2, 100);
                Console.WriteLine(tv.Size);
            }
        }

        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void CreateQueryLimitNegative()
        {
            using (var t = GetTableWithMultipleIntegers())
            {
                TableView tv = t.Where().FindAll(1, 4, -2);
                Console.WriteLine(tv.Size);
            }
        }

        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void CreateQueryEndSmallerThatStart()
        {
            using (var t = GetTableWithMultipleIntegers())
            {
                TableView tv = t.Where().FindAll(4, 3, 100);
                Console.WriteLine(tv.Size);
            }
        }




        [Test]
        public static void CreateQuery()
        {
            string actualres;
            using (var t = GetTableWithMultipleIntegers())
            {
                TableView tv = t.Where().FindAll(1, 4, 100);
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name, "calling findall on query on table",
                                                tv);
            }

            const string expectedres = @"------------------------------------------------------
Column count: 3
Table Name  : calling findall on query on table
------------------------------------------------------
 0        Int  intcolumn0          
 1        Int  intcolumn1          
 2        Int  intcolumn2          
------------------------------------------------------

Table Data Dump. Rows:3
------------------------------------------------------
{ //Start row 0
intcolumn0:1,//column 0
intcolumn1:0,//column 1
intcolumn2:0//column 2
} //End row 0
{ //Start row 1
intcolumn0:2,//column 0
intcolumn1:0,//column 1
intcolumn2:0//column 2
} //End row 1
{ //Start row 2
intcolumn0:3,//column 0
intcolumn1:0,//column 1
intcolumn2:0//column 2
} //End row 2
------------------------------------------------------
";
            TestHelper.Cmp(expectedres, actualres);

        }
    }
}
