using System;
using System.Linq;
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
        private static Table GetTableWithMultipleIntegers()
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


        //return a table with 4 columns
        //each column can be 1 2 or 3
        //all combinations exist
        //1  1   1  1
        //1  1   1  2
        //1  1   1  3
        //1  1   2  1
        //1  1   2  2 
        //1  1   2  3  
        //1  1   3  1  
        //1  1   3  2
        //1  1   3  3
        //1  2   1  1    etc. etc.. up to..
        //3  3   3  3

        //in all 72 rows


        
        private static Table GetTableWithCombinations()
        {
            var t = new Table(new IntField("intcolumn0"), new IntField("intcolumn1"), new IntField("intcolumn2"),new IntField("intcolumn3"));

            for (int n = 0; n < 3*3*3*3; n++)
            {
                long col0 =1+ (n/3/3/3)%3;
                long col1 =1+ (n/3/3)%3;
                long col2 =1+ (n/3)%3;
                long col3 =1+ n %3;
                t.Add(col0, col1, col2, col3);
            }
            Assert.AreEqual(1, t.GetLong(0, 0));
            Assert.AreEqual(3, t.GetLong(3, 71));
            Assert.AreEqual(3, t.GetLong(0, 71));
            Assert.AreEqual(2, t.GetLong(2,3 ));
            Assert.AreEqual(1, t.GetLong(3, 69));            
            return t;
        }


        [Test]
        public static void QueryAverage()
        {
            {
                var combitable = GetTableWithCombinations();
                Assert.AreEqual(2, combitable.Where().Greater("intcolumn2", 1).Average(3));
                Assert.AreEqual(2, combitable.Where().Greater("intcolumn0", 1).Average(2));
                Assert.AreEqual(3, combitable.Where().Greater(0, 2).Average(0));
            }
            {
                var combitable = GetTableWithCombinations();
                Assert.AreEqual(2, combitable.Where().Greater("intcolumn2", 1).Average("intcolumn3"));
                Assert.AreEqual(2, combitable.Where().Greater("intcolumn0", 1).Average("intcolumn2"));
                Assert.AreEqual(3, combitable.Where().Greater("intcolumn0", 2).Average("intcolumn0"));
            }
        }

     

        [Test]
        public static void QueryCount()
        {
            {
                var combitable = GetTableWithCombinations();
                Assert.AreEqual(2*3*3*3, combitable.Where().Greater("intcolumn2", 1).Count());
                Assert.AreEqual(1*1*3*3, combitable.Where().Greater("intcolumn0", 2).Greater("intcolumn1",2).Count());
                Assert.AreEqual(10, combitable.Where().Count(10,20,999));
            }
        }


        [Test]
        public static void QueryTestEnummerator()
        {
            var combitable = GetTableWithCombinations();
            var n = 0;
            foreach (TableRow tableRow in combitable)
            {
                int col0 = 1+  (n/(3*3*3))%3;
                int col1 = 1 + (n/(3*3))%3;
                int col2 = 1 + (n/3) %3;
                int col3 = 1 + n%3;
                Assert.AreEqual(col0,tableRow.GetLong(0));
                Assert.AreEqual(col1, tableRow.GetLong(1));
                Assert.AreEqual(col2, tableRow.GetLong(2));
                Assert.AreEqual(col3, tableRow.GetLong(3));
                n++;
            }
        }

        [Test]
        public static void QueryFindNext()
        {
            var combitable = GetTableWithCombinations();
            Query combiquery = combitable.Where().Greater("intcolumn2",1);
            Assert.AreEqual(3, combiquery.FindNext(-1));
            Assert.AreEqual(4, combiquery.FindNext(3));
            Assert.AreEqual(5, combiquery.FindNext(4));
            Assert.AreEqual(6, combiquery.FindNext(5));
            Assert.AreEqual(7, combiquery.FindNext(6));
            Assert.AreEqual(8, combiquery.FindNext(7));
            Assert.AreEqual(12, combiquery.FindNext(8));
            Assert.AreEqual(13, combiquery.FindNext(12));
            Assert.AreEqual(14, combiquery.FindNext(13));
        }


        [Test]
        public static void QueryGreater()
        {
            var inttable = GetTableWithMultipleIntegers();
            var query = inttable.Where().Greater("intcolumn0", 500);
            Assert.AreEqual(query.Count( r =>r.GetLong("intcolumn0")>500),499);           
            foreach (TableRow tr in query)
            {
                Assert.Greater(tr.GetLong("intcolumn0"),500);
            }
        }



        [Test]
        public static void QueryBetween()
        {
            {
            var inttable = GetTableWithMultipleIntegers();
            var query = inttable.Where().Between("intcolumn0",100,199);
            Assert.AreEqual(query.Count(r => r.GetLong("intcolumn0") > 100 && r.GetLong("intcolumn0")<200), 99);
            foreach (TableRow tr in query)
            {
                Assert.IsTrue(tr.GetLong("intcolumn0")>=100 && tr.GetLong("intcolumn0")<200);
                Assert.AreEqual(1,tr.GetLong("intcolumn2"));
            }
            }
            {
            var inttable = GetTableWithMultipleIntegers();
            var query = inttable.Where().Between(0, 100, 199);
            Assert.AreEqual(query.Count(r => r.GetLong("intcolumn0") > 100 && r.GetLong("intcolumn0") < 200), 99);
            foreach (TableRow tr in query)
            {
                Assert.IsTrue(tr.GetLong("intcolumn0") >= 100 && tr.GetLong("intcolumn0") < 200);
                Assert.AreEqual(1, tr.GetLong("intcolumn2"));
            }
            }
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
                tv = t.Where().Equal(1, false).FindAll();
                Assert.AreEqual(tv.Size, 2);
                tv = t.Where().Equal("boolfield2", false).FindAll();
                Assert.AreEqual(tv.Size, 0);
                tv = t.Where().Equal("boolfield2", true).FindAll();
                Assert.AreEqual(tv.Size, 5);
            }
        }

        [Test]
        public static void QueryGetColumnIndex()
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
