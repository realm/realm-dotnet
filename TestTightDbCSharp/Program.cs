using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;

using System.IO;
using NUnit.Framework;
using TightDbCSharp;
using TightDbCSharp.Extensions;
using System.Globalization;
using System.Reflection;
using System.Diagnostics;

[assembly: CLSCompliant(true)] //mark the public interface of this program as cls compliant (can be run from any .net language)

namespace TestTightDbCSharp
{
    [TestFixture]
    public static class EnvironmentTest
    {
        [Test]
        public static void ShowVersionTest()
        {
            var pointerSize = IntPtr.Size;
            var vmBitness = (pointerSize == 8) ? "64bit" : "32bit";
            var dllsuffix = (pointerSize == 8) ? "64" : "32";
            OperatingSystem os = Environment.OSVersion;
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            PortableExecutableKinds peKind;
            ImageFileMachine machine;
            executingAssembly.ManifestModule.GetPEKind(out peKind, out machine);
            // String thisapplocation = executingAssembly.Location;

            Console.WriteLine("Build number :              {0}", Program.Buildnumber);
            Console.WriteLine("Pointer Size :              {0}", pointerSize);
            Console.WriteLine("Process Running as :        {0}", vmBitness);
            Console.WriteLine("Built as PeKind :           {0}", peKind);
            Console.WriteLine("Built as ImageFileMachine : {0}", machine);
            Console.WriteLine("OS Version :                {0}", os.Version);
            Console.WriteLine("OS Platform:                {0}", os.Platform);
            Console.WriteLine("");
            Console.WriteLine("Now Loading tight_c_cs{0}.dll - expecting it to be a {1} dll", dllsuffix, vmBitness);
            //Console.WriteLine("Loading "+thisapplocation+"...");

            //if something is wrong with the DLL (like, if it is gone), we will not even finish the creation of the table below.
            using (var t = new Table())
            {
                Console.WriteLine("C#  DLL        build number {0}", Table.GetDllVersionCSharp);
                Console.WriteLine("C++ DLL        build number {0}", Table.CPlusPlusLibraryVersion());
                if (t.Size != 0)
                {
                    throw new TableException("Weird");
                }
                
            }
            Console.WriteLine();
            Console.WriteLine();
        }

                [Test]
                public static void TestInterop()
                {
                    Table.TestInterop();                    
                }
    }


    [TestFixture]
    public static class IntegrationTests
    {
        [Test]
        public static void TestTutorial()
        {
            Program.TutorialDynamic();
        }

        [Test]
        public static void TestDynamicTable()
        {
            Program.DynamicTable();
        }

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

    
    [TestFixture]
    public static class TestDates
    {
        [Test]
        public static void TestSaveAndRetrieveDate()
        {
            //this test might not be that effective if being run on a computer whose local time is == utc
            var dateToSaveLocal = new DateTime(1979, 05, 14, 1, 2, 3, DateTimeKind.Local);
            var dateToSaveUtc = new DateTime(1979, 05, 14, 1, 2, 4, DateTimeKind.Utc);
            var dateToSaveUnspecified = new DateTime(1979, 05, 14, 1, 2, 5, DateTimeKind.Unspecified);

            var expectedLocal = new DateTime(1979, 05, 14, 1, 2, 3, DateTimeKind.Local).ToUniversalTime();//we expect to get the UTC timepoit resembling the local time we sent
            var expectedUtc = new DateTime(1979, 05, 14, 1, 2, 4, DateTimeKind.Utc);//we expect to get the exact same timepoint back, measured in utc
            var expectedUnspecified = new DateTime(1979, 05, 14, 1, 2, 5, DateTimeKind.Local).ToUniversalTime();//we expect to get the UTC timepoit resembling the local time we sent

            using (var t = new Table("date1".Date(), "date2".Mixed(), "stringfield".String()))//test date in an ordinary date , as well as date in a mixed
            {
                t.SetIndex(2);

                t.AddEmptyRow(1);//in this row we store datetosavelocal
                t.SetString(2, 0, "str1");
                t.SetDateTime(0, 0, dateToSaveLocal);
                DateTime fromdb = t.GetDateTime("date1", 0);
                DateTime fromdb2 = t[0].GetDateTime("date1");
                Assert.AreEqual(fromdb, fromdb2);
                Assert.AreEqual(fromdb, expectedLocal);

                t.SetMixedDateTime(1, 0, dateToSaveLocal.AddYears(1));//one year is added to get a time after 1970.1.1 otherwise we would get an exception with the mixed
                fromdb = t.GetMixedDateTime(1, 0);
                fromdb2 = t[0].GetMixedDateTime(1);
                Assert.AreEqual(fromdb, fromdb2);
                Assert.AreEqual(fromdb, expectedLocal.AddYears(1));


                t.AddEmptyRow(1);//in this row we save datetosaveutc
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

                t.AddEmptyRow(1);//in this row we save datetosaveunspecified
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
                TableView tv = t.Distinct("stringfield");//we need a tableview to be able to test the date methods on table views


                tv.SetDateTime(0, 0, dateToSaveUtc);
                fromdb = tv.GetDateTime("date1", 0);
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
    }

    //todo:When we are running on the newest tightdb windows build, check again if the group stuff is behaving badly.
    //todo:Try to make tightdb or binding fail on group creation instead of group deallocation if the group file is somehow invalid
    [TestFixture]
    public static class GroupTests
    {
        /*this one fails
         * It fails because calling group() constructor does not work on windows, tightdb tries to use a filename that is illegal or don't have enough rights
         * It fails such that when the group is deleted (when the program terminats and g gets gc'ed), then the c++ crash*/
        [Test]
        public static void CreateGroupEmptyTest()
        {
            
            /*original code
            var g = new Group();
            Console.WriteLine(g.Handle);//keep it allocated
             */
        }
        

        /*
         this one crashes too, same reasons - group() doesn't work on windows*/

        [Test]
        public static void CreateGroupEmptyTestUsing()
        {
        /*
            using (var g = new Group())
            {
                Console.WriteLine(g.Handle);//keep it allocated                
            }            */
        }
        


        /*
         this one fails too - not enough rights to work in the root directory
         */

        [Test]
        public static void CreateGroupFileNameTest()
        {
            /* this code fails in the c++ end on deallocation/close
            var g = new Group(@"C:\Testgroupf");
            Console.WriteLine(g.Handle);//keep it allocated
             */
        }

        /*
        [Test]
        public static void TestAcquireAndDeleteGroup()
        {
            
            //Table.TestAcquireAndDeleteGroup("");
        }*/
    

        //this one works. (if you have a directory called Develope in C:\) Do we have a filename/directory problem with the windows build?
        //perhaps we have a problem with locked or illegal files, what to do?
        //
        //probably something wrong with the code here too then
        [Test]
        public static void CreateGroupFileNameTestGoodFile()
        {
            using (var g = new Group(@"C:\Develope\Testgroupf"))
            {
                Console.WriteLine(g.ObjectIdentification()); //keep it allocated
            }
        }
    }

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
            using (var t = new Table("stringfield".String(),"boolfield".Bool(),"stringfield2".String(),"boolfield2".Bool())){
            
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
            Assert.AreEqual(tv.Size,3);
            tv = t.Where().Equal("boolfield", false).FindAll();
            Assert.AreEqual(tv.Size,2);
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
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name, "calling findall on query on table",
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
            Assert.AreEqual(expectedres, actualres);

        }
    }

    [TestFixture]
    public static class TableViewTests
    {
        [Test]
        //simple call just to get a tableview (and have it deallocated when it exits scope)
        public static void TableViewCreation()
        {
            using (var t = new Table("intfield".Int()))
            {
                t.AddEmptyRow(1);
                t.SetLong(0, 0, 42);
                TableView tv = t.FindAllInt(0, 42);
                Console.WriteLine(tv.Handle);
            }
        }

        //returns a table with row 0 having ints 0 to 999 ascending
        //row 1 having ints 0 to 99 ascendig (10 of each)
        //row 2 having ints 0 to 9 asceding (100 of each)
        public static Table TableWithMultipleIntegers()
        {
            Table returnTable;
            Table t=null;
            try
            {
                t = new Table(new IntField("intcolumn0"), new IntField("intcolumn1"), new IntField("intcolumn2"));

                for (int n = 0; n < 1000; n++)
                {
                    long col0 = n;
                    long col1 = n/10;
                    long col2 = n/100;
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
        public static Table GetTableWithNIntegersInColumn(long columnIndex, long numberOfIntegers)
        {
            Table returnTable;
            Table t = null;
            try
            {
                t = new Table();
                for (int n = 0; n < columnIndex; n++)
                {
                    t.AddColumn(DataType.String, "StringColumn" + n);
                }
                t.AddColumn(DataType.Int, "IntColumn");
                t.AddColumn(DataType.String, "StringcolumnLast");

                for (int n = 0; n < numberOfIntegers; n++)
                {
                    t.AddEmptyRow(1);
                    t.SetLong(columnIndex, n, n*2);
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

        [Test]
        //create table view then search for all ints and return nothing as there is no matches
        public static void TableViewNoResult()
        {
            const long column = 3;
            using (var t = GetTableWithNIntegersInColumn(column, 100))
            {
                TableView tv = t.FindAllInt(column, 1001);
                Assert.AreEqual(0, tv.Size);
            }
        }


        [Test]
        //create table view then search for all ints and return nothing as there is no matches
        public static void TableViewWithOneRow()
        {
            const long column = 3;
            using (var t = GetTableWithNIntegersInColumn(column, 100))
            {
                TableView tv = t.FindAllInt(column, 42);
                Assert.AreEqual(1, tv.Size);
            }
        }

        [Test]
        //create table view then search for all ints and return nothing as there is no matches
        public static void TableViewWithManyRows()
        {
            using (var t = TableWithMultipleIntegers())
            {
                TableView tv = t.FindAllInt(1, 5);
                Assert.AreEqual(10, tv.Size);
            }
        }


        [Test]
        //make sure tableview returns field values correctly
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

        [Test]
        public static void TableViewDumpView()
        {
            string actualres;

            using (var t = TableWithMultipleIntegers())
            {
                TableView tv = t.FindAllInt(1, 10);
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name, "find 10 integers in larger table",
                                                tv);

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
            Assert.AreEqual(expectedres, actualres);
        }


        [Test]
        public static void TableViewAndTableTestMixedDouble()
        {
            const double testDouble = 12.2;
            using (var t = new Table(new MixedField("MixedField"), "stringfield".String()))
            {
                //get and set of a double in a mixed field (test type and value)
                t.AddEmptyRow(1);
                t.SetMixedDouble(0, 0, testDouble);
                t.SetString("stringfield", 0, "testdata");//used for creation of tableview in next test
                DataType dt = t.GetMixedType(0, 0);
                Assert.AreEqual(DataType.Double, dt);
                double fromDb = t.GetMixedDouble(0, 0);
                Assert.AreEqual(testDouble, fromDb);

                const double testDouble2 = -12.2;
                //get and set of a double in a mixed in a tableview (test type and value).
                t.SetIndex(1);
                TableView tv = t.Distinct("stringfield");
                Assert.AreEqual(1, tv.Size);
                tv.SetMixedDouble(0, 0, testDouble2);
                dt = tv.GetMixedType(0, 0);
                Assert.AreEqual(DataType.Double, dt);
                fromDb = tv.GetMixedDouble(0, 0);
                Assert.AreEqual(testDouble2, fromDb);
            }
        }

        [Test]
        public static void TableViewAndTableTestMixedFloat()
        {
            //performance test
            for (int n = 0;n<10000;n++) {
            const float testFloat = -12.2f;
            using (var t = new Table(new MixedField("MixedField"), "stringfield".String()))
            {
                //get and set of a double in a mixed field (test type and value)
                t.AddEmptyRow(1);
                t.SetMixedFloat(0, 0, testFloat);
                t.SetString("stringfield", 0, "testdata");//used for creation of tableview in next test
                DataType dt = t.GetMixedType(0, 0);
                Assert.AreEqual(DataType.Float, dt);
                float fromDb = t.GetMixedFloat(0, 0);
                Assert.AreEqual(testFloat, fromDb);

                const float testFloat2 = -12.2f;
                //get and set of a double in a mixed in a tableview (test type and value).
                t.SetIndex(1);
                TableView tv = t.Distinct("stringfield");
                Assert.AreEqual(1, tv.Size);
                tv.SetMixedFloat(0, 0, testFloat2);
                dt = tv.GetMixedType(0, 0);
                Assert.AreEqual(DataType.Float, dt);
                fromDb = tv.GetMixedFloat(0, 0);
                Assert.AreEqual(testFloat2, fromDb);
            }
            }
        }



        [Test]
        public static void TableViewAndTableTestDouble()
        {
            const string fn0 = "stringfield";
            const string fn1 = "doublefield1";
            const string fn2 = "doublefield2";

            using (var t = new Table(fn0.String(), fn1.Double(),fn2.Double()))
            {
                //first test the table gets the doubles right
                const double testdouble = -42.3;
                const double testdouble2 = 42.5;
                t.AddEmptyRow(1);
                t.SetString(fn0,0,"teststring");
                t[0].SetDouble(fn1, testdouble);
                t.SetDouble(2,0, testdouble2);
                Assert.AreEqual(testdouble,t.GetDouble(fn1,0));
                Assert.AreEqual(testdouble,t.GetDouble(1,0));
                Assert.AreEqual(testdouble2,t[0].GetDouble(fn2));
                Assert.AreEqual(testdouble2,t.GetDouble(2,0));
                                
                //then try once again, but with a tableview
                t.SetIndex(0);
                TableView tv = t.Distinct(fn0);
                tv.SetString(fn0, 0, "teststring");
                tv[0].SetDouble(fn1, testdouble);
                tv.SetDouble(2, 0, testdouble2);
                Assert.AreEqual(testdouble, tv.GetDouble(fn1, 0));
                Assert.AreEqual(testdouble, tv.GetDouble(1, 0));
                Assert.AreEqual(testdouble2, tv[0].GetDouble(fn2));
                Assert.AreEqual(testdouble2, tv[0].GetDouble(2));
            }            
        }



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
                TableView tv = t.Distinct(fn0);                  
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
                t.SetFloat(fn1,0, testfloat);
               
                Assert.AreEqual(testfloat, t.GetFloat(fn1, 0));
                Assert.AreEqual(testfloat, t.GetFloat(1, 0));

                //then try once again, but with a tableview
                t.SetIndex(0);
                TableView tv = t.Distinct(fn0);
                Assert.AreEqual(1, tv.Size);
                tv.SetString(fn0, 0, "teststring!!");
                tv.SetFloat(fn1,0, testfloat);
                Assert.AreEqual(testfloat, tv.GetFloat(fn1, 0));
                Assert.AreEqual(testfloat, tv.GetFloat(1, 0));
                Assert.AreEqual(1,t.Size);
            }
        }



        [Test]
        //make sure tableview returns field values correctly
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
    }


    //this test fixture tests that the C#  binding correctly catches invalid arguments before they are passed on to c++
    //this test does not at all cover all possibillities, it's just enough to ensure that our validation kicks in at all
    [TestFixture]
    public static class TableParameterValidationTest
    {

        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableGetMixedWithNonMixedField()
        {
            using (var t= new Table(new IntField("int1"),new MixedField("mixed1")))
            {
                t.AddEmptyRow(1);
                t.SetLong(0,0,42);
                t.SetMixedLong(1,0,43);
                long intfromnonmixedfield= t.GetMixedLong(0, 0);
                Assert.AreEqual(42,intfromnonmixedfield);//we should never get this far
                Assert.AreNotEqual(42,intfromnonmixedfield);//we should never get this far
            }
        }

        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableTestEmptyTableFieldAccess()
        {
            using (var t = new Table(new IntField("Int1"), new IntField("Int2"), new IntField("Int3")))
            {
                //accessing a row on an empty table should not be allowed
                long value = t.GetLong(0, 0);
                Console.WriteLine(value);
            }
        }

        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableTestEmptyTableFieldAccessWrite()
        {
            using (var t = new Table(new IntField("Int1"), new IntField("Int2"), new IntField("Int3")))
            {
                //accessing a row on an empty table should not be allowed
                t.SetLong(0, 0, 42); //this should throw
                long value = t.GetLong(0, 0);
                Console.WriteLine(value);
            }
        }



        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableTestRowIndexTooLow()
        {
            using (var t = new Table(new IntField("Int1"), new IntField("Int2"), new IntField("Int3")))
            {
                //accessing a row on an empty table should not be allowed
                t.AddEmptyRow(1);
                long value = t.GetLong(0, -1);
                Console.WriteLine(value);
            }
        }

        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableTestRowIndexTooLowWrite()
        {
            using (var t = new Table(new IntField("Int1"), new IntField("Int2"), new IntField("Int3")))
            {
                //accessing a row on an empty table should not be allowed
                t.AddEmptyRow(1);
                t.SetLong(0, -1, 42); //should throw
                long value = t.GetLong(0, -1);
                Console.WriteLine(value);
            }
        }


        //
        [Test]
        public static void TableTestGetUndefinedMixedType()
        {
            using (var t = new Table(new MixedField("MixedField")))
            {
                t.AddEmptyRow(1);
                DataType dt = t.GetMixedType(0, 0);
                Assert.AreEqual(dt, DataType.Int);
            }
        }


        [Test]
        public static void TableTestMixedInt()
        {
            using (var t = new Table(new MixedField("MixedField")))
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
        public static void TableTestMixedDateTime()
        {
            var testDate = new DateTime(2000, 1, 2, 3, 4, 5, DateTimeKind.Utc);
            using (var t = new Table(new MixedField("MixedField")))
            {
                t.AddEmptyRow(1);
                t.SetMixedDateTime(0, 0, testDate);
                DataType dt = t.GetMixedType(0, 0);
                Assert.AreEqual(DataType.Date,dt);
                DateTime fromDb = t.GetMixedDateTime(0, 0);
                Assert.AreEqual(testDate, fromDb);
            }
        }



        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableTestRowIndexTooHigh()
        {
            using (var t = new Table(new IntField("Int1"), new IntField("Int2"), new IntField("Int3")))
            {
                //accessing a row on an empty table should not be allowed
                t.AddEmptyRow(1);
                long value = t.GetLong(0, 1);
                Console.WriteLine(value);
            }
        }

        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableTestRowIndexTooHighWrite()
        {
            using (var t = new Table(new IntField("Int1"), new IntField("Int2"), new IntField("Int3")))
            {
                //accessing a row on an empty table should not be allowed
                t.AddEmptyRow(1);
                t.SetLong(0, 1, 42); //should throw
                long value = t.GetLong(0, 1);
                Console.WriteLine(value);
            }
        }



        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableTestColumnIndexTooLow()
        {
            using (var t = new Table(new IntField("Int1"), new IntField("Int2"), new IntField("Int3")))
                //accessing an illegal column should also not be allowed
            {
                t.AddEmptyRow(1);
                long value2 = t.GetLong(-1, 0);
                Console.WriteLine(value2);
            }
        }

        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableTestColumnIndexTooLowWrite()
        {
            using (var t = new Table(new IntField("Int1"), new IntField("Int2"), new IntField("Int3")))
                //accessing an illegal column should also not be allowed
            {
                t.AddEmptyRow(1);
                t.SetLong(-1, 0, 42);
                long value2 = t.GetLong(-1, 0);
                Console.WriteLine(value2);
            }
        }


        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableTestColumnIndexTooHigh()
        {
            using (var t = new Table(new IntField("Int1"), new IntField("Int2"), new IntField("Int3")))
                //accessing an illegal column should also not be allowed
            {
                t.AddEmptyRow(1);
                long value2 = t.GetLong(3, 0);
                Console.WriteLine(value2);
            }
        }


        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableTestColumnIndexTooHighWrite()
        {
            using (var t = new Table(new IntField("Int1"), new IntField("Int2"), new IntField("Int3")))
                //accessing an illegal column should also not be allowed
            {
                t.AddEmptyRow(1);
                t.SetLong(3, 0, 42);
                long value2 = t.GetLong(3, 0);
                Console.WriteLine(value2);
            }
        }



        [Test]
        [ExpectedException("TightDbCSharp.TableException")]
        public static void TableTestIllegalType()
        {
            using (var t = new Table(new IntField("Int1"), new IntField("Int2"), new IntField("Int3")))
                //accessing an illegal column should also not be allowed
            {
                t.AddEmptyRow(1);
                //likewise accessing the wrong type should not be allowed
                Table t2 = t.GetSubTable(1, 0);
                Console.WriteLine(t2.Size); //this line should not hit - the above should throw an exception
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming",
            "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "TypeWrite"), Test]
        [ExpectedException("TightDbCSharp.TableException")]
        public static void TableTestIllegalTypeWrite()
        {
            using (var t = new Table(new SubTableField("sub1"), new IntField("Int2"), new IntField("Int3")))
                //accessing an illegal column should also not be allowed
            {
                t.AddEmptyRow(1);
                //likewise accessing the wrong type should not be allowed               
                t.SetLong(0, 0, 42); //should throw                
                Console.WriteLine(t.Size); //this line should not hit - the above should throw an exception
            }
        }
    }

    //this test fixture covers adding rows, deleting rows, altering field values in ordinary table
    [TestFixture]
    public static class TableChangeDataTest
    {

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization",
            "CA1303:Do not pass literals as localized parameters",
            MessageId = "NUnit.Framework.Assert.AreEqual(System.Int64,System.Int64,System.String,System.Object[])"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming",
             "CA2204:Literals should be spelled correctly", MessageId = "InsertInt")]
        public static void CheckNumberInIntColumn(Table table, long columnNumber, long rowNumber, long testValue)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table"); //code analysis made me do this             
            }
            table.SetLong(columnNumber, rowNumber, testValue);
            long gotOut = table.GetLong(columnNumber, rowNumber);
            Assert.AreEqual(testValue, gotOut, "Table.InsertInt value mismatch sent{0} got{1}", testValue, gotOut);
        }


        //create a table of only integers, 3 columns.
        //with 42*42 in {0,0}, with long.minvalue in {1,1} and with long.minvalue+24 in {2,2}
        //the other fields have never been touched
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability",
            "CA2000:Dispose objects before losing scope"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public static Table GetTableWithIntegers(bool subTable)
        {
            var t = new Table();
            var s = t.Spec;
            s.AddIntColumn("IntColumn1");
            s.AddIntColumn("IntColumn2");
            s.AddIntColumn("IntColumn3");
            if (subTable)
            {
                Spec subSpec = t.Spec.AddSubTableColumn("SubTableWithInts");
                subSpec.AddIntColumn("SubIntColumn1");
                subSpec.AddIntColumn("SubIntColumn2");
                subSpec.AddIntColumn("SubIntColumn3");
            }
            t.UpdateFromSpec();

            

            long rowindex = t.AddEmptyRow(1); //0
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
            CheckNumberInIntColumn(t, colummnIndex, rowindex, long.MaxValue);
            CheckNumberInIntColumn(t, colummnIndex, rowindex, long.MinValue);
            if (subTable)
            {
                Table sub = t.GetSubTable(3, rowindex);
                sub.AddEmptyRow(3);
                CheckNumberInIntColumn(sub, colummnIndex, rowindex, 2L);
            }



            colummnIndex = 2;
            rowindex = t.AddEmptyRow(1); //2
            CheckNumberInIntColumn(t, colummnIndex, rowindex, long.MaxValue - 42);
            CheckNumberInIntColumn(t, colummnIndex, rowindex, long.MinValue + 42);
            if (subTable)
            {
                Table sub = t.GetSubTable(3, rowindex);
                sub.AddEmptyRow(3);
                CheckNumberInIntColumn(sub, colummnIndex, rowindex, 2L);
            }




            return t;
        }


        [Test]
        public static void TableMixedCreateEmptySubTable2()
        {
            using (var t = new Table(new MixedField("mixd")))
            {
                using (var sub = new Table(new IntField("int")))
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
            using (var t = new Table(new MixedField("mixd")))
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
            using (var t = new Table(new MixedField("mix'd")))
            {
                using (var subtable = new Table(new IntField("int1")))
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
            using (var t = new Table(new MixedField("mix'd")))
            {
                using (var subtable = new Table(new IntField("int1")))
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



        public static void TableMixedSetGetSubTableWithData()
        {

            string actualres;

            using (var t = new Table(new MixedField("mix'd")))
            {
                using (var subtable = new Table(new IntField("int1")))
                {
                    t.AddEmptyRow(1);
                    subtable.AddEmptyRow(1);
                    subtable.SetLong(0, 0, 42);
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
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name, "sub in mixed with int", t);

            }


            const string expectedres = @"------------------------------------------------------
Column count: 1
Table Name  : sub in mixed with int
------------------------------------------------------
 0      Mixed  mix'd               
------------------------------------------------------

Table Data Dump. Rows:2
------------------------------------------------------
{ //Start row0
mix'd:   { //Start row0
   int1:42   //column 0
   } //End row0
//column 0//Mixed type is Table
} //End row0
{ //Start row1
mix'd:84//column 0//Mixed type is Int
} //End row1
------------------------------------------------------
";

            Assert.AreEqual(expectedres, actualres);
        }



        [Test]
        public static void TableSubTableSubTable()
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

                const long valueinserted = 42;
                s2.SetLong(0, 0, valueinserted);
                Assert.AreEqual(1, s2.Size);

                //now read back the 42

                long valueback = t.GetSubTable(1, 0).GetSubTable(2, 0).GetSubTable(3, 0).GetLong(0, 0);
                //                            root               s1                 s2            42


                Assert.AreEqual(valueback, valueinserted);
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name + 5, "subtable in subtable with int",
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
            Assert.AreEqual(expectedres, actualres);
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

                const long valueinserted = 42;
                s2.SetLong(0, 0, valueinserted);
                Assert.AreEqual(1, s2.Size);

                //now read back the 42

                long valueback = t.GetSubTable(1, 0).GetSubTable(2, 0).GetSubTable(3, 0).GetLong(0, 0);
                //                            root               s1                 s2            42


                Assert.AreEqual(valueback, valueinserted);
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name + 5, "subtable in subtable with int",
                                                t.Clone() as Table);
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
            Assert.AreEqual(expectedres, actualres);
        }





        [Test]
        public static void TableIntValueTest2()
        {
            String actualres;
            //Table.LoggingEnable("IntValueTest2");
            using (var t = GetTableWithIntegers(false))
            {
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name, "table with a few integers in it", t);
            }
            Table.LoggingSaveFile("IntValueTest2.log");

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
            Assert.AreEqual(expectedres, actualres);
        }



        [Test]
        public static void TableIntValueSubTableTest1()
        {
            String actualres;

            using (var t = GetTableWithIntegers(true))
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name,
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
            Assert.AreEqual(expectedres, actualres);
        }


        [Test]
        public static void TableRowColumnInsert()
        {
            String actualres;
            using (
                var t = new Table(new IntField("intfield"), new StringField("stringfield"), new IntField("intfield2")))
            {
                t.AddEmptyRow(5);
                long changeNumber = 0;
                foreach (TableRow tr in t)
                {
                    foreach (RowColumn trc in tr)
                    {
                        if (trc.ColumnType == DataType.Int)
                            trc.Value = ++changeNumber;

                    }
                }
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name,
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
            Assert.AreEqual(expectedres, actualres);
        }
    }




    [TestFixture]
    public static class StringEncodingTest
    {


        [Test]
        public static void TableGetColumnName()
        {
            var testFieldNames = new List<String>
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
                int n = 0;
                foreach (string str in testFieldNames)
                {
                    testTable.AddColumn(DataType.String, str);
                    Assert.AreEqual(str, testTable.GetColumnName(n++));
                }
            }
        }


        [Test]
        public static void TableGetColumnIndex()
        {

            var testFieldNames = new List<String>
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
                    testTable.AddColumn(DataType.String, str);
                    Assert.AreEqual(n++, testTable.GetColumnIndex(str));
                }
            }
        }




        //Right now this test uses creation of tables as a test - the column name will be set to all sorts of crazy thing, and we want them back that way
        [Test]
        public static void TableWithPerThousandSign()
        {
            String actualres;
            using (
                var notSpecifyingFields = new Table(
                    "subtable".Table()
                    )) //at this point we have created a table with no fields
            {
                notSpecifyingFields.AddColumn(DataType.String, "12345‰7890");
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name,
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
            Assert.AreEqual(expectedres, actualres);
        }




        [Test]
        public static void TableWithNotAnsiCharacters()
        {
            String actualres;
            using (
                var notSpecifyingFields = new Table(
                    "subtable".Table()
                    )) //at this point we have created a table with no fields
            {
                notSpecifyingFields.AddColumn(DataType.String, "123\u0300\u0301678");
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name,
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
            Assert.AreEqual(expectedres, actualres);
        }
    }


    [TestFixture]
    public static class IteratorTest
    {
        [Test]
        public static void TableIterationTest()
        {
            using
                (
                var t = new Table("stringfield".String())
                )
            {
                t.AddEmptyRow(3);
                t.SetString(0,0,"firstrow");
                t.SetString(0,0,"secondrow");
                t.SetString(0,0,"thirdrow");
                foreach (TableRow tableRow in  t )
                {
                    Assert.IsInstanceOf(typeof(TableRow),tableRow);//assert important as Table's parent also implements an iterator that yields rows. We want TableRows when 
                    //we expicitly iterate a Table with foreach
                }
            }

        }

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
                foreach (Row row in t.Where().FindAll()) //loop through a tableview should get os Row classes
                {
                    Assert.IsInstanceOf(typeof(Row), row);//assert important as Table's parent also implements an iterator that yields rows. We want TableRows when 
                    //we expicitly iterate a Table with foreach
                }
            }
        }


        public static void IterateTableOrView(TableOrView tov)
        {
            if (tov!=null)
            {
                foreach (Row row in tov)
                    //loop through a TableOrview should get os Row classes EVEN IF THE UNDERLYING IS A TABLE
                {
                    Assert.IsInstanceOf(typeof (Row), row);
                    //we explicitly iterate a Table with foreach
                }
            }
        }

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







    }

    [TestFixture]
    public static class TableAggregateTest
    {

        [Test]
        public static void TableMaximumDouble()
        {
            using (var myTable = new Table("double".Double()))
            {
                myTable.Add(1d);
                Assert.AreEqual(1d,myTable.MaximumDouble(0));
            }
        }

        //should probably be split up into more tests, but this one touches all c++ functions which is okay for now
        [Test]
        public static void TableAggreate()
        {
            using (var myTable = new Table("strfield".String(),
                "int".Int(),
                "float".Float(),
                "double".Double())
                )
            {
                myTable.Add("tv", 1, 3f, 5d);
                myTable.Add("tv", 3, 9f, 15d);
                myTable.Add("tv", 5, 15f, 25d);
                myTable.Add("notv", -1000, -1001f, -1002d);
                
                string actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name, "table with testdata for TableAggregate",
                    myTable);

            const string    expectedres =
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
                Assert.AreEqual(expectedres,actualres);

                
                using (
                    TableView myTableView = myTable.FindAllString(0, "tv")

                    )
                {
                    Assert.AreEqual(3, myTable.CountString(0, "tv"));
                    Assert.AreEqual(1, myTable.CountLong(1, 3));
                    Assert.AreEqual(1, myTable.CountFloat(2, 15f));
                    Assert.AreEqual(1, myTable.CountDouble(3, 15d));

                    Assert.AreEqual(0, myTable.CountString(0, "xtv"));
                    Assert.AreEqual(0, myTable.CountLong(1, -3));
                    Assert.AreEqual(0, myTable.CountFloat(2, -15f));
                    Assert.AreEqual(0, myTable.CountDouble(3, -15d));


                    Assert.AreEqual(5, myTable.MaximumLong("int"));
                    Assert.AreEqual(15f, myTable.MaximumFloat("float"));
                    Assert.AreEqual(25d, myTable.MaximumDouble(3));

                    Assert.AreEqual(-1000, myTable.MinimumLong(1));
                    Assert.AreEqual(-1001f, myTable.MinimumFloat(2));
                    Assert.AreEqual(-1002d, myTable.MinimumDouble("double"));

                    long sl = myTable.SumLong(1);
                    Assert.AreEqual(3f,myTable.GetFloat(2,0));
                    Assert.AreEqual(9f, myTable.GetFloat(2, 1));
                    Assert.AreEqual(15f, myTable.GetFloat(2, 2));
                    Assert.AreEqual(-1001f, myTable.GetFloat(2, 3));
                    double sf = myTable.SumFloat(2);
                    double sd = myTable.SumDouble(3);
                    double sftv = myTableView.SumFloat(2);

                    Assert.AreEqual(-1000 + 1 + 3 + 5,sl);
                    Assert.AreEqual(-1001f + 3f + 9f + 15f, sf);
                    Assert.AreEqual(-1002d + 5d + 15d + 25d, sd);

                    Assert.AreEqual((1 + 3 + 5 - 1000)/4d, myTable.AverageLong(1));
                    Assert.AreEqual((3f + 9f + 15f - 1001f)/4d, myTable.AverageFloat(2));
                    Assert.AreEqual((5d + 15d + 25d - 1002d)/4d, myTable.AverageDouble(3));


                    Assert.AreEqual(3,myTableView.Size);
                    //count methods are not implemented in tightdb yet, Until they are implemented, and our c++ binding
                    //is updated to call them, our c++ binding will just return zero
                    Assert.AreEqual(0/*3*/, myTableView.CountString(0, "tv"));
                    Assert.AreEqual(0/*1*/, myTableView.CountLong(1, 3));
                    Assert.AreEqual(0/*1*/, myTableView.CountFloat(2, 15f));
                    Assert.AreEqual(0/*1*/, myTableView.CountDouble(3, 15d));

                    Assert.AreEqual(5, myTableView.MaximumLong("int"));
                    Assert.AreEqual(15f, myTableView.MaximumFloat("float"));
                    Assert.AreEqual(25d, myTableView.MaximumDouble(3));

                    Assert.AreEqual(1, myTableView.MinimumLong(1));
                    Assert.AreEqual(3f, myTableView.MinimumFloat(2));
                    Assert.AreEqual(5d, myTableView.MinimumDouble(3));

                    Assert.AreEqual(1 + 3 + 5, myTableView.SumLong(1));
                    Assert.AreEqual( 3f + 9f + 15f, sftv);
                    Assert.AreEqual( 5d + 15d + 25d, myTableView.SumDouble(3));

                    //average methods are not implemented in tightdb yet, Until they are implemented, and our c++ binding
                    //is updated to call them, our c++ binding will just return zero
                    Assert.AreEqual(0/*(1 + 3 + 5 )/4*/, myTableView.AverageLong(1));
                    Assert.AreEqual(0/*(3f + 9f + 15f)/4f*/, myTableView.AverageFloat(2));
                    Assert.AreEqual(0/*(5d + 15d + 25d )/4d*/, myTableView.AverageDouble(3));

                }

            }

        }
    }


    [TestFixture]
    public static class TableCreateTest
    {


        [Test]
        public static void TableTestIsValid()
        {
            using (var t = new Table())
            {
                Assert.AreEqual(true,t.IsValid());
                t.AddColumn(DataType.Int,"do'h");
                Assert.AreEqual(true, t.IsValid());
                using (var sub = new Table())
                {
                    t.AddColumn(DataType.Table, "sub");
                    t.Add(42, sub);
                    Assert.AreEqual(true, sub.IsValid());
                    t.Set(0,43,null);
                    Table sub2 = t.GetSubTable(1,0);
                    Assert.AreEqual(true, sub2.IsValid());
                    Assert.AreEqual(true, sub.IsValid());
                    t.Add(42, sub);
                    Table sub3 = t.GetSubTable(1, 1);
                    t.Set(1, 45, null);
                    Assert.AreEqual(false, sub3.IsValid());
                    t.Set(1, 45, sub);
                    Assert.AreEqual(false, sub3.IsValid());
                    sub.AddColumn(DataType.Int,"intfield");
                    t.Set(1, 45, sub);
                    Assert.AreEqual(false, sub3.IsValid());

                }
            }
        }


        //test with the newest kind of field object constructores - lasse's inherited specialized ones

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
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name, "table with subtable with subtable",
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
            Assert.AreEqual(expectedres, actualres);
        }




        [Test]
        public static void TypedFieldClasses()
        {
            String actualres;
            using (
                var newFieldClasses = new Table(
                    new StringField("F1"),
                    new IntField("F2"),
                    new SubTableField("Sub1",
                                      new StringField("F11"),
                                      new IntField("F12"))
                    ))
            {
                newFieldClasses.AddColumn(DataType.String, "Buksestørrelse");

                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name,
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
            Assert.AreEqual(expectedres, actualres);
        }





        [Test]
        public static void TableCloneLostFieldNameTest()
        {
            const string fnsub =
            "sub";
            const string fnsubsub = "subsub";
            String actualres="";
            using (var smallTable = new Table(fnsub.Table(fnsubsub.Table())))
            {
                smallTable.Add(new Table(fnsub.Table()));
                Assert.AreEqual(fnsub, smallTable.GetColumnName(0));
                Assert.AreEqual(fnsubsub,smallTable.GetSubTable(0,0).GetColumnName(0));
                Spec spec1 = smallTable.Spec;
                Assert.AreEqual(fnsub,spec1.GetColumnName(0));
                Spec spec2 = spec1.GetSpec(0);
                Assert.AreEqual(fnsubsub,spec2.GetColumnName(0));
                Console.WriteLine("so far so good");
                var clonedTable = smallTable.Clone() as Table;
                if (clonedTable != null)
                {
                    Assert.AreEqual(fnsub, clonedTable.GetColumnName(0));
                    Assert.AreEqual(fnsubsub, clonedTable.GetSubTable(0, 0).GetColumnName(0));
                    Spec spec1S = smallTable.Spec;
                    Assert.AreEqual(fnsub, spec1S.GetColumnName(0));
                    Spec spec2S = spec1S.GetSpec(0);
                    Assert.AreEqual(fnsubsub, spec2S.GetColumnName(0));


                    actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name,
                                "tableclone subsub fieldnames test",
                                smallTable.Clone() as Table);



                }
                else
                {
                    {Assert.AreEqual("clonedTable was null","it should have contained data");}
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
            Assert.AreEqual(expectedres, actualres);
        }


        //todo:this test fails, update when the base library has fixed the field name bug
        [Test]
        public static void TableCloneTest4()
        {
            String actualres;
            using (
                var newFieldClasses = new Table(
                //new StringField("F1"),
                //new IntField("F2"),
                    new SubTableField("Sub1"//),
                //                      new StringField("F11"),
                //                      new IntField("F12"))
                    )))
            {
                newFieldClasses.AddColumn(DataType.String, "Buksestørrelse");
                
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name,
                                                "table created with all types using the new field classes",
                                                newFieldClasses.Clone() );
            }
            const string expectedres = @" no reasonable output as the test fails currently
";
            Assert.AreEqual(expectedres, actualres);
        }




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
                newFieldClasses.AddColumn(DataType.String, "Buksestørrelse");


                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name,
                                                "table created with all types using the new field classes",
                                                newFieldClasses.Clone() );
            }
            const string expectedres = @" no reasonable output as the test fails currently
";
            Assert.AreEqual(expectedres, actualres);
        }




        [Test]
        public static void TableCloneTest2()
        {
            String actualres;
            using (
                var newFieldClasses = new Table(
                    //new StringField("F1"),
                    //new IntField("F2"),
                    new SubTableField("Sub1",
                                      new StringField("F11"),
                                      new IntField("F12"))
                    ))
            {
                newFieldClasses.AddColumn(DataType.String, "Buksestørrelse");


                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name,
                                                "table created with all types using the new field classes",
                                                newFieldClasses.Clone() );
            }
            const string expectedres = @" no reasonable output as the test fails currently
";
            Assert.AreEqual(expectedres, actualres);
        }


        [Test]
        public static void TableCloneTest()
        {
            String actualres;
            using (
                var newFieldClasses = new Table(
                    new StringField("F1"),
                    new IntField("F2"),
                    new SubTableField("Sub1",
                                      new StringField("F11"),
                                      new IntField("F12"))
                    ))
            {
                newFieldClasses.AddColumn(DataType.String, "Buksestørrelse");

                
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name,
                                                "table created with all types using the new field classes",
                                                newFieldClasses.Clone() );
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
            Assert.AreEqual(expectedres, actualres);
        }


        //illustration of field usage, usecase / unit test

        //The user can decide to create his own field types, that could then be used in several different table definitions, to ensure 
        //that certain kinds of fields used by common business logic always were of the correct type and setup
        //For example a field called itemcode that currently hold integers to denote owned item codes in a game,
        //but perhaps later should be a string field instead
        //if you have many IntegerField fields in many tables with item codes in them, you could use Itemcode instead, and then effect the change to string
        //only by changing the ineritance of the Itemcode type from IntegerField to StringField
        //thus by introducing your own class, You hide the field implementation detail from the users using this field type


        private class ItemCode : IntField
            //whenever ItemCode is specified in a table definition, an IntegerField is created
        {
            public ItemCode(String columnName) : base(columnName)
            {
            }
        }

        //because of a defense against circular field references, the subtablefield cannot be used this way, however you can make a method that returns an often
        //used subtable specification like this instead :

        //subtable field set used by our general login processing system
        public static SubTableField OwnedItems()
        {
            return new SubTableField(
                ("OwnedItems"),
                new StringField("Item Name"),
                new ItemCode("ItemId"),
                new IntField("Number Owned"),
                new BoolField("ItemPowerLevel"));
        }

        //game state dataset used by our general game saving system for casual games
        public static SubTableField GameSaveFields()
        {
            return new SubTableField(
                ("GameState"),
                new StringField("SaveDate"),
                new IntField("UserId"),
                new StringField("Users description"),
                new BinaryField("GameData1"),
                new StringField("GameData2"));
        }


        //creation of table using user overridden or generated fields (ensuring same subtable structure across applications or tables)
        [Test]
        public static void UserCreatedFields()
        {
            String actualres;

            using (
                var game1 = new Table(
                    OwnedItems(),
                    new IntField("UserId"),
                    //some game specific stuff. All players are owned by some item, don't ask me why
                    new BinaryField("BoardLayout"), //game specific
                    GameSaveFields())
                )
            {
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name + "1",
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
            Assert.AreEqual(expectedres, actualres);




            using (var game2 = new Table(
                OwnedItems(),
                new ItemCode("UserId"), //game specific
                new ItemCode("UsersBestFriend"), //game specific
                new IntField("Game Character Type"), //game specific
                GameSaveFields()))
            {
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name + "2",
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
            Assert.AreEqual(expectedres, actualres);

        }


        //this kind of creation call should be legal - it creates a totally empty table, then only later sets up a field        
        [Test]
        public static void SubTableNoFields()
        {
            String actualres;
            using (
                var notSpecifyingFields = new Table(
                    "subtable".Table()
                    )) //at this point we have created a table with no fields
            {
                notSpecifyingFields.AddColumn(DataType.String, "Buksestørrelse");
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name,
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
            Assert.AreEqual(expectedres, actualres);
        }


        [Test]
        public static void TestHandleAcquireOneField()
        {
            string actualres;
            using (var testtbl = new Table(new Field("name", DataType.String)))
            {
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name, "NameField", testtbl);
            }
            const string expectedres =
                @"------------------------------------------------------
Column count: 1
Table Name  : NameField
------------------------------------------------------
 0     String  name                
------------------------------------------------------

";
            Assert.AreEqual(expectedres, actualres);
        }


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
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name, "four columns, Last Mixed", testtbl3);
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
            Assert.AreEqual(expectedres, actualres);
        }

        //test the alternative table dumper implementation that does not use table class
        [Test]
        public static void TestAllFieldTypesStringExtensions()
        {
            string actualres1;
            string actualres2;
            using (var t = new Table(
                "Count".Int(),
                "Valid".Bool(),
                "Name".String(),
                "BLOB".Binary(),
                "Items".SubTable(
                    "ItemCount".Int(),
                    "ItemName".String()),
                "HtmlPage".Mixed(),
                "FirstSeen".Date(),
                "Fraction".Float(),
                "QuiteLargeNumber".Double()
                ))
            {
                actualres1 = Program.TableDumper(MethodBase.GetCurrentMethod().Name,
                                                 "Table with all allowed types (String Extensions)", t);
                actualres2 = Program.TableDumperSpec(MethodBase.GetCurrentMethod().Name,
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
            Assert.AreEqual(expectedres, actualres1);
            Assert.AreEqual(expectedres, actualres2);
        }



        //test the alternative table dumper implementation that does not use table class
        [Test]
        public static void TestAllFieldTypesFieldClass()
        {
            string actualres1;
            string actualres2;
            using (var t = new Table(
                new Field("Count", DataType.Int),
                new Field("Valid", DataType.Bool),
                new Field("Name", DataType.String),
                new Field("BLOB", DataType.Binary),
                new Field("Items",
                          new Field("ItemCount", DataType.Int),
                          new Field("ItemName", DataType.String)),
                new Field("HtmlPage", DataType.Mixed),
                new Field("FirstSeen", DataType.Date),
                new Field("Fraction", DataType.Float),
                new Field("QuiteLargeNumber", DataType.Double)
                ))
            {
                actualres1 = Program.TableDumper(MethodBase.GetCurrentMethod().Name,
                                                 "Table with all allowed types (Field)", t);
                actualres2 = Program.TableDumperSpec(MethodBase.GetCurrentMethod().Name,
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
            Assert.AreEqual(expectedres, actualres1);
            Assert.AreEqual(expectedres, actualres2);
        }

        //test the alternative table dumper implementation that does not use table class
        [Test]
        public static void TestAllFieldTypesFieldClassStrings()
        {
            string actualres1;
            string actualres2;
            using (var t = new Table(
                new Field("Count1", "integer"),
                new Field("Count2", "Integer"), //Any case is okay
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
                actualres1 = Program.TableDumper(MethodBase.GetCurrentMethod().Name,
                                                 "Table with all allowed types (Field_string)", t);
                actualres2 = Program.TableDumperSpec(MethodBase.GetCurrentMethod().Name,
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
            Assert.AreEqual(expectedres, actualres1);
            Assert.AreEqual(expectedres, actualres2);
        }



        //test the alternative table dumper implementation that does not use table class
        [Test]
        public static void TestAllFieldTypesTypedFields()
        {
            string actualres1;
            string actualres2;
            using (var t = new Table(
                new IntField("Count"),
                new BoolField("Valid"),
                new StringField("Name"),
                new BinaryField("BLOB"),
                new SubTableField("Items",
                                  new IntField("ItemCount"),
                                  new StringField("ItemName")),
                new MixedField("HtmlPage"),
                new DateField("FirstSeen"),
                new FloatField("Fraction"),
                new DoubleField("QuiteLargeNumber")
                ))
            {
                actualres1 = Program.TableDumper(MethodBase.GetCurrentMethod().Name,
                                                 "Table with all allowed types (Typed Field)", t);
                actualres2 = Program.TableDumperSpec(MethodBase.GetCurrentMethod().Name,
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
            Assert.AreEqual(expectedres, actualres1);
            Assert.AreEqual(expectedres, actualres2);
        }


        //test with a subtable
        [Test]
        public static void TestMixedConstructorWithSubTables()
        {
            string actualres;
            using (
                var testtbl = new Table(
                    "Name".TightDbString(),
                    "Age".TightDbInt(),
                    new Field("age2", DataType.Int),
                    new Field("age3", "Int"),
//                new IntegerField("Age3"),
                    new Field("comments",
                              new Field("phone#1", DataType.String),
                              new Field("phone#2", DataType.String),
                              new Field("phone#3", "String"),
                              "phone#4".TightDbString()
                        ),
                    new Field("whatever", DataType.Mixed)
                    ))
            {
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name, "six colums,sub four columns",
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
            Assert.AreEqual(expectedres, actualres);
        }




        [Test]
        //[NUnit.Framework.Ignore("Need to write tests that test for correct deallocation of table when out of scope")]
        //scope has been thoroughly debugged and does work perfectly in all imagined cases, but the testing was done before unit tests had been created
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



        //while You cannot cross-link parents and subtables inside a new table() construct, you can try to do so, by deliberatly changing
        //the subtable references in Field objects that You instantiate yourself -and then call Table.create(Yourfiled) with a 
        //field definition that is self referencing.
        //however, currently this is not possible as seen in the example below.
        //the subtables cannot be changed directly, so all You can do is create new objects that has old already created objects as subtables
        //therefore a tree structure, no recursion.

        //below is my best shot at someone trying to create a table with custom built cross-linked field definitions (and failing)

        //I did not design the Field type to be used on its own like the many examples below. However , none of these weird uses break anything
        [Test]
        public static void TestIllegalFieldDefinitions1()
        {
            Field f5 = "f5".Int(); //create a field reference, type does not matter
            f5 = "f5".Table(f5); //try to overwrite the field object with a new object that references itself 
            string actualres;
            using (
                var t = new Table(f5))
                //this will not crash or loop forever the subtable field does not references itself 
            {
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name, "self-referencing subtable", t);
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
            Assert.AreEqual(expectedres, actualres);
        }

        [Test]
        public static void TestIllegalFieldDefinitions2()
        {
            Field fc = "fc".Int(); //create a field reference, type does not matter
            Field fp = "fp".Table(fc); //let fp be the parent table subtable column, fc be the sole field in a subtable

            fc = "fc".Table(fp); //then change the field type from int to subtable and reference the parent

            //You now think You have illegal field definitions in fc and fp as both are subtables and both reference the other as the sole subtable field
            //however, they are new objects that reference the old classes that they replaced.
            String actualres;
            using (
                var t2 = new Table(fc))
            {
                //should crash too
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name,
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


            Assert.AreEqual(expectedres, actualres);
        }

        [Test]
        public static void TestIllegalFieldDefinitions3()
        {
            Field fc = "fc".Int(); //create a field reference, type does not matter
            Field fp = "fp".Table(fc); //let fp be the parent table subtable column, fc be the sole field in a subtable
// ReSharper disable RedundantAssignment
            fc = "fc".Table(fp); //then change the field type from int to subtable and reference the parent
// ReSharper restore RedundantAssignment

            String actualres;
            using (
                var t3 = new Table(fp))
            {
                //should crash too
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name,
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

            Assert.AreEqual(expectedres, actualres);

        }

        //super creative attemt at creating a cyclic graph of Field objects
        //still it fails because the array being manipulated is from GetSubTableArray and thus NOT the real list inside F1 even though the actual field objects referenced from the array ARE the real objects
        //point is - You cannot stuff field definitions down into the internal array this way
        [Test]
        public static void TestCyclicFieldDefinition1()
        {

            Field f1 = "f10".SubTable("f11".Int(), "f12".Int());
            var subTableElements = f1.GetSubTableArray();
            subTableElements[0] = f1; //and the "f16" field in f1.f15.f16 is now replaced with f1.. recursiveness


            string actualres;
            using (var t4 = new Table(f1))
            {
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name, "cyclic field definition", t4);
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

            Assert.AreEqual(expectedres, actualres);
        }

        //dastardly creative terroristic attemt at creating a cyclic graph of Field objects
        //this creative approach succeeded in creating a stack overflow situation when the table is being created, but now it is not possible as AddSubTableFields has been made
        //internal, thus unavailable in customer assemblies.

        private class AttemptCircularField : Field
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"),
             System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
                 MessageId = "fielddefinitions"),
             System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
                 MessageId = "fieldName")]
// ReSharper disable UnusedParameter.Local
            public void setsubtablearray(String fieldName, Field[] fielddefinitions)
                //make the otherwise hidden addsubtablefield public
// ReSharper restore UnusedParameter.Local
            {
//uncommenting the line below should create a compiletime error (does now) or else this unit test wil bomb the system
//                AddSubTableFields(this, fieldName,fielddefinitions);
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
                MessageId = "columnName"),
             System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
                 MessageId = "subTableFieldsArray")]
// ReSharper disable UnusedParameter.Local
            public AttemptCircularField(string columnName, params Field[] subTableFieldsArray)
// ReSharper restore UnusedParameter.Local
            {
                FieldType = DataType.Table;
            }
        }


        [Test]
        [ExpectedException("System.ArgumentNullException")]
        public static void TestCyclicFieldDefinition2()
        {

            var f1 = new AttemptCircularField("f1", null);
                //do not care about last parameter we're trying to crash the system
            var subs = new Field[2];
            subs[0] = f1;
            f1.setsubtablearray("f2", subs);

            string actualres;
            using (var t4 = new Table(f1))
            {
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name,
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

            Assert.AreEqual(expectedres, actualres);
        }






        [Test]
        public static void TestIllegalFieldDefinitions4()
        {

            Field f10 = "f10".SubTable("f11".Int(), "f12".Int());
            f10.FieldType = DataType.Int;
            //at this time, the subtable array still have some subtables in it
            string actualres;
            using (var t4 = new Table(f10))
            {
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name, "just an int field, no subs", t4);
            }
            const String expectedres =
                @"------------------------------------------------------
Column count: 1
Table Name  : just an int field, no subs
------------------------------------------------------
 0        Int  f10                 
------------------------------------------------------

";

            Assert.AreEqual(expectedres, actualres);
        }

        [Test]
        public static void TestIllegalFieldDefinitions5()
        {
            Field f10 = "f10".SubTable("f11".Int(), "f12".Int());
            f10.FieldType = DataType.Table;

            String actualres;
            using (
                var t5 = new Table(f10))
            {
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name, "subtable with two int fields", t5);
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
            Assert.AreEqual(expectedres, actualres);
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
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name, "two fields, case is differnt",
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
            Assert.AreEqual(expectedres, actualres);
        }

        [Test]
        public static void TestCreateStrangeTable2()
        {
            //Create a table with two columns with the same name and type
            String actualres;
            using (var badtable2 = new Table("Age".Int(), "Age".Int()))
            {
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name, "two fields name and type the same",
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
            Assert.AreEqual(expectedres, actualres);

        }


        //Test if two table creations where the second happens before the first is out of scope, works okay
        [Test]
        public static void TestCreateTwoTables()
        {
            var actualres = new StringBuilder(); //we add several table dumps into one compare string in this test
            using (
                var testtbl1 = new Table(
                    new Field("name", DataType.String),
                    new Field("age", DataType.Int),
                    new Field("comments",
                              new Field("phone#1", DataType.String),
                              new Field("phone#2", DataType.String)),
                    new Field("whatever", DataType.Mixed)))
            {
                actualres.Append(Program.TableDumperSpec(MethodBase.GetCurrentMethod().Name,
                                                         "four columns , sub two columns (Field)", testtbl1));

                using ( //and we create a second table while the first is in scope
                    var testtbl2 = new Table(
                        new Field("name", "String"),
                        new Field("age", "Int"),
                        new Field("comments",
                                  new Field("phone#1", DataType.String), //one way to declare a string
                                  new Field("phone#2", "String"), //another way
                                  "more stuff".SubTable(
                                      "stuff1".String(), //and yet another way
                                      "stuff2".String(),
                                      "ÆØÅæøå".String())
                            ),
                        new Field("whatever", DataType.Mixed)))
                {
                    actualres.Append(Program.TableDumperSpec(MethodBase.GetCurrentMethod().Name,
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
            Assert.AreEqual(expectedres, actualres.ToString());
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
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name,
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
            Assert.AreEqual(expectedres, actualres);
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
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name,
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
            Assert.AreEqual(expectedres, actualres);
        }

    }


    internal class Program
    {

        public static int Buildnumber = 1305211514;












        private static void PrintHeader(StringBuilder res, string tablename, long count)
        {
            res.AppendLine(Sectiondelimitor);
            res.AppendLine(String.Format(CultureInfo.InvariantCulture, "Column count: {0}", count));
            res.AppendLine(String.Format(CultureInfo.InvariantCulture, "Table Name  : {0}", tablename));
            res.AppendLine(Sectiondelimitor);
        }


        private static void PrintMetadataFooter(StringBuilder res)
        {
            res.AppendLine(Sectiondelimitor);
            res.AppendLine("");
        }

        private const string Sectiondelimitor = "------------------------------------------------------";


        //dumps table structure to a string for debugging purposes.
        //the string is easily human-readable
        //this version uses the table column information as far as possible, then shifts to spec on subtables
        public static string TableDumper(String fileName, String tableName, TableOrView t)
        {
            var res = new StringBuilder(); //temporary storange of text of dump

            long count = t.ColumnCount;
            PrintHeader(res, tableName, count);
            for (long n = 0; n < count; n++)
            {
                string name = t.GetColumnName(n);
                DataType type = t.ColumnType(n);
                res.AppendLine(String.Format(CultureInfo.InvariantCulture, "{0,2} {2,10}  {1,-20}", n, name, type));
                if (type == DataType.Table)
                {
                    Spec subSpec = t.Spec.GetSpec(n);
                    Specdumper(res, "   ", subSpec, "Subtable");
                }
            }
            PrintMetadataFooter(res);
            TableDataDumper("", res, t);

            Console.Write(res.ToString());
            File.WriteAllText(fileName + ".txt", res.ToString());
            return res.ToString();
        }


        private static void Specdumper(StringBuilder res, String indent, Spec s, string tableName)
        {

            long count = s.ColumnCount;

            if (String.IsNullOrEmpty(indent))
            {
                PrintHeader(res, tableName, count);
            }

            for (long n = 0; n < count; n++)
            {
                String name = s.GetColumnName(n);
                DataType type = s.GetColumnType(n);
                res.AppendLine(String.Format(CultureInfo.InvariantCulture, "{0}{1,2} {2,10}  {3,-20}", indent, n, type,
                    name));
                if (type == DataType.Table)
                {
                    Spec subspec = s.GetSpec(n);
                    Specdumper(res, indent + "   ", subspec, "Subtable");
                }
            }

            if (String.IsNullOrEmpty(indent))
            {
                PrintMetadataFooter(res);
            }
        }

        //dump the table only using its spec
        public static String TableDumperSpec(String fileName, String tablename, Table t)
        {
            var res = new StringBuilder();
            Specdumper(res, "", t.Spec, tablename);

            TableDataDumper("", res, t);
            Console.WriteLine(res.ToString());
            File.WriteAllText(fileName + ".txt", res.ToString());
            return res.ToString();
        }


        public static void TableDataDumper(string indent, StringBuilder res, TableOrView table)
        {
            const string startrow = "{{ //Start row {0}";
            const string endrow = "}} //End row {0}";
            const string startfield = @"{0}:";
            const string endfield = ",//column {0}{1}";
            const string endfieldlast = "//column {0}{1}"; //no comma
            const string starttable = "[ //{0} rows";
            const string endtable = "]";
            const string mixedcomment = "//Mixed type is {0}";
            var firstdatalineprinted = false;
            long tableSize = table.Size;
            foreach (Row tr in table)
            {
                if (firstdatalineprinted == false)
                {
                    if (String.IsNullOrEmpty(indent))
                    {
                        res.Append(indent);
                        res.AppendLine(String.Format(CultureInfo.InvariantCulture, "Table Data Dump. Rows:{0}",
                            tableSize));
                        res.Append(indent);
                        res.AppendLine(Sectiondelimitor);
                    }
                    firstdatalineprinted = true;
                }
                res.Append(indent);
                res.AppendLine(String.Format(CultureInfo.InvariantCulture, startrow, tr.RowIndex)); //start row marker

                foreach (RowColumn trc in tr)
                {
                    string extracomment = "";
                    res.Append(indent);
                    string name = trc.ColumnName;
                    //so we can see it easily in the debugger
                    res.Append(String.Format(CultureInfo.InvariantCulture, startfield, name));
                    if (trc.ColumnType == DataType.Table)
                    {
                        Table sub = trc.GetSubTable();
                        //size printed here as we had a problem once with size reporting 0 where it should be larger, so nothing returned from call
                        res.Append(String.Format(CultureInfo.InvariantCulture, starttable, sub.Size));
                        TableDataDumper(indent + "   ", res, sub);
                        res.Append(endtable);
                    }
                    else
                    {
                        if (trc.ColumnType == DataType.Mixed)
                        {
                            extracomment = string.Format(CultureInfo.InvariantCulture, mixedcomment, trc.MixedType);
                            //dumping a mixed with a simple value is done by simply calling trc.value - it will return the value inside the mixed
                            if (trc.MixedType == DataType.Table)
                            {
                                var sub = trc.Value as Table;
                                TableDataDumper(indent + "   ", res, sub);
                            }
                            else
                            {
                                res.Append(trc.Value);
                            }
                        }
                        else
                            res.Append(trc.Value);
                    }
                    res.Append(indent);
                    res.AppendLine(String.Format(CultureInfo.InvariantCulture,
                        trc.IsLastColumn() ? endfieldlast : endfield, trc.ColumnIndex,
                        extracomment));
                }
                res.Append(indent);
                res.AppendLine(String.Format(CultureInfo.InvariantCulture, endrow, tr.RowIndex)); //end row marker

            }
            if (firstdatalineprinted && String.IsNullOrEmpty(indent))
                //some data was dumped from the table, so print a footer
            {
                res.Append(indent);
                res.AppendLine(Sectiondelimitor);
            }
        }

        //todo:code the rest
        //this method resembles the java dynamic table example at http://www.tightdb.com/documentation/Java_ref/4/Table/
        public static void DynamicTable()
        {
            using (var tbl = new Table())
            {
                
                tbl.AddColumn(DataType.Int, "myInt");
                tbl.AddColumn(DataType.String, "myStr");
                tbl.AddColumn(DataType.Mixed, "myMixed");
                
                //
                //add some data by setting whole rows
                //
                //add some data
                tbl.Add(12, "hello", 2);
                tbl.Add(-15, "World", "I can be different types...");
                tbl.Insert(0, 64, "I'm now first", true);     //data in order of columns
                tbl.AddEmptyRow(1);                           //append row at end of table - default values
                tbl.Set(3, 198, "TightDB", 12.345);           //set values in row 3
                tbl[3].SetRow(198,"TightDB",12.345);          //alternative syntax, specifying row as an index into the table
                tbl.Remove(0);                                //remove row 0
                tbl.RemoveLast();                             //remove last row

                //Get and set cell values
                Assert.AreEqual(-15, tbl.GetLong(0, 1));
                tbl.SetMixedString(2, 0, "Changed Long value to String");//third parameter type must be string
                tbl.SetMixedString("myMixed", 0, "Changed Long value to String");//alternative syntax,specifying the column by name
                tbl.SetMixed(2,0,"Changed long value to String");//untyped, mixed will adopt whatever Tightdb type matches best with the data in the last parameter, in this case DataType.String

                Assert.AreEqual(DataType.String, tbl.GetMixedType(2, 0)); //the mixed field should be a string mixed now
                Assert.AreEqual(2, tbl.Size);
                Assert.AreEqual(false, tbl.IsEmpty);

                tbl.RenameColumn(0, "myLong");
                tbl.RemoveColumn(1);
                tbl.Add(42, "this is the mixed column");  //add a row
                tbl.AddColumn(DataType.Double, "myDouble");
                tbl.Add(-15, "still mixed", 123.45);

                //column introspection
                Assert.AreEqual(3, tbl.ColumnCount);
                Assert.AreEqual("myLong", tbl.GetColumnName(0));
                Assert.AreEqual(1, tbl.GetColumnIndex("myMixed"));
                Assert.AreEqual(DataType.Double, tbl.ColumnType(2));

                Assert.AreEqual(123.45, tbl.MaximumDouble(2));
                Assert.AreEqual(24, tbl.SumLong(0));
                Assert.AreEqual(6.0, tbl.AverageLong(0));

                //simple match search
                Assert.AreEqual(1, tbl.FindFirstInt(0, -15)); //search for -15 in column0
                TableView view = tbl.FindAllInt(0, -15);
                Assert.AreEqual(2, view.Size);

                Query q = tbl.Where();
                Assert.AreEqual(2, q.Between(0, 0, 100).Count());

                //set index and get distinct values
                using (var tbl2 = new Table())
                {

                    long strColumn = tbl2.AddColumn(DataType.String, "new Strings");
                    tbl2.SetIndex(strColumn);
                    tbl2.Add("MyString");
                    tbl2.Add("MyString2");
                    tbl2.Add("MyString");
                    TableView view2 = tbl2.Distinct(strColumn);
                
                Assert.AreEqual(2, view2.Size);
                }//using tbl2 ends here. tbl2 will be disposed here
                String json = tbl.ToJson();
                Console.Out.WriteLine("JSON: "+json);


            }//using tbl ends here. tbl will be disposed here

            //--------------------------------------------------------
            //working with subtables
            //--------------------------------------------------------


            //subtables can be created as part of their parent table definition
            var tbl3 = new Table
                ("name".String(),
                 "subtable".SubTable(
                   "key".String(),
                   "value".Mixed())
                 );
            

            //alternative syntax, not using extension methods on String
            var tbl3B = new Table
                (new StringField("name"),
                 new SubTableField("subtable",
                   new StringField("key"),
                   new MixedField("value"))
                );

            Console.WriteLine(tbl3B.Size);//

            //add two rows - first with two rows in its' subtable cell

            //create a structure for the subtable, consisting of two rows with key and value field data
            object[][] sub = 
            {
              new object[] {"firstkey", 12},
              new object[] {"secondkey", "hi - I'm mixed"}
            };

            tbl3.Add("first", sub);
            tbl3.Add("second", null);
            Assert.AreEqual(2,tbl3.GetSubTable(1,0).Size);

            //add some rows to the empty subtable in the second row
            Table subTbl1 = tbl3.GetSubTable("subtable", 1);//or tbl3.GetSubTable(1,1);

            //now you can work with the subtable as any other table

            subTbl1.Add("key1", 23);
            Assert.AreEqual("key1",subTbl1.GetString(0,0));

        }


        public static void TutorialDynamic()
        {
            //create a dynamic table with a subtable in it 

            using (
                var peopleTable = new Table(
                    new StringField("name"),
                    new IntField("age"),
                    new BoolField("hired"),
                    new SubTableField("phones", //nested subtable
                        new StringField("desc"),
                        new StringField("number")
                        )
                    ))
            {

                //for illustration, a table with the same structure,created using our alternative syntax

                using (
                    var peoplTableAlt = new Table(
                        "name".String(),
                        "age".Int(),
                        "hired".Date(),
                        "phones".Table(
                            "desc".String(),
                            "number".String())
                        )
                    )
                {
                    Console.WriteLine(peoplTableAlt.ColumnCount);
                }


                //fill in one row, with two rows in the subtable, which is located at column 3

                long rowno = peopleTable.Add("John", 20, true, null); //the null is a subtable we haven't filled in yet
                peopleTable.GetSubTable(3, rowno).Add("mobile", "232-323-3232");
                peopleTable.GetSubTable(3, rowno).Add("work", "434-434-4343");

                //if there are many subtable rows, this is slightly faster as the subtable class only has to be created once
                {
                    long rowIndex = peopleTable.Add("John", 20, true, null);
                    //the null is a subtable we haven't filled in yet
                    Table rowSub = peopleTable.GetSubTable(3, rowIndex);
                    rowSub.Add("mobile", "232-323-3232");
                    rowSub.Add("work", "434-434-4343");
                }


                //if memory is a concern, Table support the disposable interface, so You can force C# to deallocate them when they go out of scope
                //instead of waiting until the GC feels like collecting tables. This could be important as the tables also take up c++ resources
                using (
                    var peopleTable2 = new Table(
                        new StringField("name"),
                        new IntField("age"),
                        new BoolField("hired"),
                        new SubTableField("phones", //nested subtable
                            new StringField("desc"),
                            new StringField("number")
                            )
                        )
                    )//end of table creation 
                {

                    long rowIndex = peopleTable2.Add("John", 20, true, null);
                    //the null is a subtable we haven't filled in yet
                    using (Table rowSub2 = peopleTable2.GetSubTable(3, rowIndex))
                    {
                        rowSub2.Add("mobile", "232-323-3232");
                        rowSub2.Add("work", "434-434-4343");
                    } //Because of the using statement, You are guarenteed that RowSub is deallocated at this point
                } //Because of the using statement, You are guarenteed that PeopleTable2 is deallocated at this point

                //You can also add data to a table field by field:

                    long rowindex2 = peopleTable.AddEmptyRow(1);
                    peopleTable.SetString(0, rowindex2, "John");
                    peopleTable.SetLong(1, rowindex2, 20);
                    peopleTable.SetBoolean(2, rowindex2, true);
                    var subtable = peopleTable.GetSubTable(3, rowindex2); //return a subtalbe for column 3
                    long firstRowIdAdded=subtable.AddEmptyRow(2);
                    subtable.SetString(0, firstRowIdAdded, "mobile");                   
                    subtable.SetString(1, firstRowIdAdded, "232-323-3232");                    
                    subtable.SetString(0, 1+firstRowIdAdded, "work");
                    subtable.SetString(1, 1+firstRowIdAdded, "434-434-4343");

                    //Finally subtables can be specified inside the parameter list (as an array of arrays of object) like this

                    peopleTable.Add("John", 20, true,
                        new object[]
                        {
                            new object[] {"work", "232-323-3232"},
                            new object[] {"home", "434-434-4343"}
                        });

                    //the arrays and constans can of course be supplied as variables too like this

                    var sub =
                        new object[]
                        {
                            new object[] {"work", "232-323-3232"},
                            new object[] {"home", "434-434-4343"}
                        };
                    peopleTable.Add("John", 20, true, sub);

                    //in fact the class that holds the list of rows can be of any type, as long
                    //as that type implements IEnummerable. However, a row has to be represented by an object[]
                    //this might be relaxed in the future (we would detect if the row implemented an interface that allows
                    //us to get the size, and then use that interface to get the size - and after that use IEnummerable to get
                    //the individual field values.

                    long rows = peopleTable.Size; //get the number of rows in a table
                    bool isEmpty = peopleTable.IsEmpty; //is the table empty?

                    Console.WriteLine("PeopleTable has {0} rows and isEmpty returns {1}", rows, isEmpty);

                    //working with individual rows

                    //getting values (untyped)
                    String n = peopleTable[4].GetString(0);
                    //You can specify the name of the column. It will be looked up at runtime so not so fast as the above
                    long a = peopleTable[4].GetLong("age");  //returns the value of the long field called Age so prints 20
                    Boolean b = peopleTable[4].GetBoolean("hired"); //prints true
                    Console.WriteLine(n, a, b); //writes "John20True"

                    //You can also do this, which is very slightly faster as no row cursor class is created , but harder to read
                    b = peopleTable.GetBoolean("hired", 4); //get the value of the boolean field Hired in row 4
                    Console.WriteLine(n, a, b); //writes "John20True"

                    //Accessing through a Row saves validation of rowIndex when fetching data, so this is slightly faster if You are reading many columns from
                    //the same row
                    Row row5 = peopleTable[4];
                    n = row5.GetString("name");
                    a = row5.GetLong("age");
                    b = row5.GetBoolean("hired");
                    Console.WriteLine(n, a, b); //writes "John20True"

                    //accessing when specifying the columnIndex as a number is  faster than specifying a column name.
                    Console.WriteLine(row5.GetBoolean(2)); //2 is the columnIndex for the field "Hired"

                    //setting values (untyped)
                    peopleTable[4].SetString("name", "John");//first parameter is the column name, the second parameter is the value
                    peopleTable[4].SetString(0, "John"); //columns can be indexed by number too
                    peopleTable[4].SetLong("age", 20);
                    peopleTable[4].SetBoolean("hired", true);

                    //You can also set an entire row by specifying the correct types in order
                    //re-using the subtable sub created earlier, see above
                    peopleTable[4].SetRow("John", 20, true, sub);
                    //this method is not as fast as using SetString etc. as it will have to inspect the parametres and the table to validate type compatibillity

                    //You can delete a row by calling remove(rowIndex)
                    peopleTable.Remove(3);
                        //removes the 4th row in the table and moves every row index larger than 3 one down
                    peopleTable[3].Remove(); //this does the same

                    Row r = peopleTable[2]; //another way still
                    r.Remove();
                    //after having removed r, The row object becomes invalid and cannot be used anymore for any functions that involves row numbers

                    //we better put in some data now we deleted a lot of rows
                    peopleTable.Add("Johanna", 20, true, sub);

                    peopleTable.Add("Rasmus", 23, true,
                    new object[]
                        {
                            new object[] {"work", "434-424-4242"},
                            new object[] {"home", "555-444-3333"}
                        });

                    peopleTable.Add("Per", 53, true,
                    new object[]
                        {
                            new object[] {"work", "314-159-2653"},
                            new object[] {"home", "589-793-2385"}
                        });

                    peopleTable.Add("Kirsten", 13, false, null);



                    //iterating over a table is done this way
                    foreach (var row in peopleTable)
                    {
                        Table phones = row.GetSubTable("phones");
                        Console.WriteLine("{0} is {1} years old and has {2} phones:", row.GetString("name"),
                            row.GetLong("age"), phones.Size); //writes the name of the current row
                        foreach (var phonerow in phones)
                        {
                            Console.WriteLine(phonerow.GetString("desc") + ": " + phonerow.GetString("number"));
                        }
                    }

                    //You can also iterate a table over its fields
                    foreach (var looprow in peopleTable)
                        //looprow is of type TableRow. If peopletable was a TableView You would get a Row object
                    {
                        foreach (var rowColumn in looprow)
                            //columns always give You RowColumn types no matter if You are working with Table, TableOrView or TableView
                        {
                            Console.WriteLine(rowColumn.Value);
                            //will write the value of all fields in all rows,except subtables (will write subtable.tostring for the subtable)
                        }
                    }

                    //find first will return the RowNumber of the first row that matches a search
                    long rowIndex4 = peopleTable.FindFirstInt("age", 20);

                    Console.WriteLine("First row with age=20 is:{0}", rowIndex4);
                    //find all will return all rows with a given value
                    TableView tv1 = peopleTable.FindAllInt("age", 20);
                    //will set tableview to point to all rows where the column age has value 20
                    //or with column index
                    TableView tv2 = peopleTable.FindAllInt(1, 20);
                    //will set tableview to point to all rows where the column age has value 20

                    Console.WriteLine("tv1 size {0}  tv2 size {1} These two should be the same value", tv1.Size,
                        tv2.Size);

                    Query qr = peopleTable.Where().Equal("hired", true).Between("age", 20, 30);

                    //iterate (lazily) over all matching rows in a query

                    long size = qr.FindAll().Size;
                    Console.WriteLine(size);
                    foreach (var row in qr)
                    {
                        Console.WriteLine("{0} is {1} Years old ", row.GetString(0), row.GetLong(1));
                            //lookup by field value
                        Console.WriteLine("{0} is {1} Years old ", row.GetString("name"), row.GetLong("age"));
                        //lookup by field name
                    }

                    //average using tightdb
                    Console.WriteLine("Average Age is {0}", qr.Average("age"));
                    //this Average is not the standard C# LINQ average.






                    TableView tv3 = qr.FindAll();
                    //get a view with all the results
                    tv3 = qr.FindAll(0, -1, 50);
                    //find all hired people amongst the first 10K records, but return only 50 at most

                    TableView tv4 = qr.FindAll(); //creates tableview with all matching records

                    Console.WriteLine("findAll limited to 50 returned {0} rows", tv3.Size);
                    Console.WriteLine("findAll returned {0} rows", tv4.Size);

                    //serialization 

                //this new Group call currently crashes tightdb on windows 7 with the tightdb libraries built in march
                //as the same call does not crash with c++ wit the libraries built in may, I have allowed myself to curcumvent
                //the crash by specifying a (usually) valid file name

                if (false)
                {
                    //using (var myGroup = new Group())  //this crashes
                    //todo:reinsert correct line above that does not specify a file name
                    using (var myGroup = new Group(@"C:\ProgramData\tightdbtest.tightdb"))
                    {

                        //Group returns a new table. The structure is specified as in Table.CreateTable
                        //okay the mar. code crashes here now, when i specified a writable filename above.
                        //i think i'll postpone further investigation until i'm up running with the newest tightdb release
                        var myTable2 = myGroup.CreateTable("employees",
                            "Name".String(),
                            "Age".Int(),
                            "Hired".Bool()
                            );

                        //or fields can be put in just after.
                        var myOtherTable = myGroup.CreateTable("employees2");
                        myOtherTable.AddColumn(DataType.String, "Name");
                        myOtherTable.AddColumn(DataType.Int, "Age");
                        myOtherTable.AddColumn(DataType.Int, "Hired");

                        //the first method is easier to use when dealing with subtables

                        myTable2.Add(1); //add one empty row
                        myOtherTable.Add(1);
                        //write to disk
                        //todo:document where this file will be located when running on various kinds of windows
                        //todo:check and handle filesystem errors that happen on the c++ side (wrap into a C# exception like java does)
                        myGroup.Write("employees.tightdb");

                    }

                }


                //transactions - to be done
                }
            }
        

        internal static long  MeasureInteropSpeedHelper(long n)
        {
            return (n + 1);
        }

        
        internal static void MeasureInteropSpeed()
        {
            using (
                var t = new Table("intfield".Int()))
            {
                long acc = 0;

                Console.WriteLine("Interop Test started");

                var timer1 = Stopwatch.StartNew();
                for (long n = 0; n < 1000*100*100; n++)
                {
                    //call a c# function that does a very simple arithmetic thing
                    acc = acc + MeasureInteropSpeedHelper(n); //c# function
                    //call a similar c++ function using interop
                    acc = acc + Table.TestAddInteger(n); //c++ function
                    acc = acc + t.Size; //call into a tightdb table object
                }
                timer1.Stop();
                double seconds = Math.Floor(timer1.Elapsed.TotalSeconds);
                double milliseconds = timer1.Elapsed.Milliseconds;
                Console.WriteLine("Interop Test took: {0} seconds, {1} milliseconds.  result {2}", seconds, milliseconds, acc);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Console.WriteLine(System.String)")]
        static void Main(/*string[] args*/)
        {

            /*
             *  to debug unit tests, uncomment the test you want to debug below, and run the program
             *  remember to set a breakpoint
             *  Don't run the program in Nunit to debug, simply debug it in visual studio when it runs like an ordinary program
             *  To run the unit tests as unit tests, load the assembly in Nunit and run it from there
             *  */
            //for (int n = 0; n < 1000000; n++)
            //{
//                Table.TestInterop();
//            }

          //  TableCreateTest.SubTableNoFields();
            TableCreateTest.TableTestIsValid();
            TableCreateTest.TableCloneTest4();
            TableCreateTest.TableCloneTest3();
            TableCreateTest.TableCloneTest2();
            TableCreateTest.TableCloneLostFieldNameTest();
            TableChangeDataTest.TableSubTableSubTableClone();
            TableCreateTest.TypedFieldClasses();
            TableCreateTest.TableCloneTest();
            TableAggregateTest.TableAggreate();
            TableAggregateTest.TableMaximumDouble();
            EnvironmentTest.ShowVersionTest();
            EnvironmentTest.TestInterop();
            StringEncodingTest.TableGetColumnName();

            //MeasureInteropSpeed();
            //IntegrationTests.TestDynamicTable();
            //IntegrationTests.TestTutorial();
            //QueryTests.QueryBoolEqual();
            //TableAggregateTest.TableMaximumDouble();
            //TableAggregateTest.TableAggreate();
           // TableViewTests.TableViewAndTableTestMixedFloat();
//            Iteratortest.TableIterationTest();
  //          Iteratortest.TableViewIterationTest();
    //        Iteratortest.TableorViewIterationTest();

            //TableViewTests.TableViewAndTableTestFloatSimple();

     //       TableParameterValidationTest.TableTestMixedDateTime();
             
     //       QueryTests.QueryBoolEqual();

      //      TestDates.TestSaveAndRetrieveDate();
      //      TableCreateTest.TestTableScope();

            Table.TestAcquireAndDeleteGroup(@"C:\Develope\Tightdbf");
            //Table.TestGroupStuff();
           // GroupTests.CreateGroupFileNameTestGoodFile();
     //       TableChangeDataTest.TableIntValueSubTableTest1();
         //    StringEncodingTest.TableWithnonasciiCharacters();//when we have implemented utf-16 to utf-8 both ways, the following tests should be created:

            //GroupTests.CreateGroupFileNameTest();
            //CreateTableTest.TableSubtableSubtable();
  //          TableParameterValidationTest.TableTestMixedInt();
//            TableParameterValidationTest.TableTestColumnIndexTooHigh();
           // TableViewTests.TableViewCreation();
            //TableChangeDataTest.TableMixedSetGetSubTableWithData();

    //        TableChangeDataTest.TableIntValueTest1();
    //        TableChangeDataTest.TableIntValueTest2();
    //        CreateTableTest.TestAllFieldTypesFieldClassStrings();
    //        CreateTableTest.UserCreatedFields();
    //        CreateTableTest.TypedFieldClasses();
    //        CreateTableTest.TestCyclicFieldDefinition2();///this should crash the program
    //        StringEncodingTest.TableWithPerThousandSign();
    //        CreateTableTest.TestHandleAcquireOneField();
    //        CreateTableTest.TestHandleAcquireOneField();
    //        CreateTableTest.TestCreateTwoTables();
    //        CreateTableTest.TestTableScope();
    //        CreateTableTest.TestHandleAcquireSeveralFields();
    //        CreateTableTest.TestMixedConstructorWithSubTables();
    //        CreateTableTest.TestAllFieldTypesStringExtensions();
    //        CreateTableTest.TestIllegalFieldDefinitions1();
    //        CreateTableTest.TestIllegalFieldDefinitions2() ;
    //        CreateTableTest.TestIllegalFieldDefinitions3() ;
    //        CreateTableTest.TestIllegalFieldDefinitions4(); 
    //        CreateTableTest.TestIllegalFieldDefinitions5();
             //an empty string back and forth
             //a string with an illegal utf-16 being sent to the binding (illegal because one of the uft-16 characters is a lead surrogate with no trailing surrogate)
            //a string with an illegal utf-16 being sent to the binding (illegal because one of the uft-16 characters is a trailing surrogate with no lead surrogate)
            //if the c++ binding (wrongly) accpets codepoints in the surrogate range, test how the binding handles reading such values)
            //in all cases with illegal utf-16 strings, a descriptive exception should be raised
            //round trip of a 7-bit ascii string
            //round trip of a a string with a unicode codepoint between 129 and 255
            //round trip of a string with a unicode character that translates to 1 utf-16 character, but 2 utf-8 characters
            //round trip of a string with a unicode character that translates to 1 utf-16 character, but 3 utf-8 characters
            //round trip of a string with a unicode character that translates to 2 utf-16 characters,  4 utf-8 characters
            //round trip of a string with a unicode character that translates to 2 utf-16 characters,  5 utf-8 characters
                        
            Console.WriteLine("Press any key to finish test...T=call tutorialdynamic M=call measureinteropspeed ");
            ConsoleKeyInfo ki = Console.ReadKey();
            if (ki.Key==ConsoleKey.T) {
                TutorialDynamic();
            }
            if (ki.Key == ConsoleKey.M)
            {
                MeasureInteropSpeed();
            }


        }

    }
}
