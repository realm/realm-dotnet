using System;
using System.Text;

using System.IO;
using NUnit.Framework;
using TightDbCSharp;
using TightDbCSharp.Extensions;
using System.Globalization;
using System.Reflection;

[assembly: CLSCompliant(true)] //mark the public interface of this program as cls compliant (can be run from any .net language)

namespace TestTightDbCSharp
{
    [TestFixture]
    public static class EnvironmentTest
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String,System.Object,System.Object)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String,System.Object)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization",
            "CA1303:Do not pass literals as localized parameters",
            MessageId = "Console.WriteLine(System.String,System.Object,System.Object)"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming",
             "CA2204:Literals should be spelled correctly", MessageId = "tightccs"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization",
             "CA1303:Do not pass literals as localized parameters", MessageId = "Console.WriteLine(System.String)"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming",
             "CA2204:Literals should be spelled correctly", MessageId = "ImageFileMachine"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming",
             "CA2204:Literals should be spelled correctly", MessageId = "PeKind"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization",
             "CA1303:Do not pass literals as localized parameters",
             MessageId = "Console.WriteLine(System.String,System.Object)")]
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
                if (t.Size() != 0)
                {
                    throw new TableException("Weird");
                }
            }
            Console.WriteLine();
            Console.WriteLine();

        }
    }

    //this test fixture tests that the C#  binding correctly catches invalid arguments before they are passed on to c++
    //this test does not at all cover all possibillities, it's just enough to ensure that our validation kicks in at all
    [TestFixture]
    public static class TableParameterValidationTest
    {
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
        [ExpectedException("TightDb.TightDbCSharp.TableException")]
        public static void TableTestIllegalType()
        {
            using (var t = new Table(new IntField("Int1"), new IntField("Int2"), new IntField("Int3")))
                //accessing an illegal column should also not be allowed
            {
                t.AddEmptyRow(1);
                //likewise accessing the wrong type should not be allowed
                Table t2 = t.GetSubTable(1, 0);
                Console.WriteLine(t2.Size()); //this line should not hit - the above should throw an exception
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "TypeWrite"), Test]
        [ExpectedException("TightDb.TightDbCSharp.TableException")]
        public static void TableTestIllegalTypeWrite()
        {
            using (var t = new Table(new SubTableField("sub1"), new IntField("Int2"), new IntField("Int3")))
                //accessing an illegal column should also not be allowed
            {
                t.AddEmptyRow(1);
                //likewise accessing the wrong type should not be allowed               
                t.SetLong(0, 0, 42); //should throw                
                Console.WriteLine(t.Size()); //this line should not hit - the above should throw an exception
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
        public static void TableSubTableSubTable()
        {
            //string actualres1;
            //string actualres2;
            //string actualres3;
            //string actualres4;
            //string actualres5;
            string actualres6;

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
                //actualres1 = Program.TableDumper(MethodBase.GetCurrentMethod().Name+1, "subtable in subtable with int", t);
                Assert.AreEqual(1, t.Size());
                Table root = t.GetSubTable(1, 0);
                root.AddEmptyRow(1);
                Assert.AreEqual(1, root.Size());
                //actualres2 = Program.TableDumper(MethodBase.GetCurrentMethod().Name + 2, "subtable in subtable with int", t);
                Table s1 = root.GetSubTable(2, 0);
                s1.AddEmptyRow(1);
                Assert.AreEqual(1, s1.Size());
                //actualres3 = Program.TableDumper(MethodBase.GetCurrentMethod().Name+3, "subtable in subtable with int", t);
                Table s2 = s1.GetSubTable(3, 0);
                s2.AddEmptyRow(1);
                //actualres4 = Program.TableDumper(MethodBase.GetCurrentMethod().Name + 3, "subtable in subtable with int", t);
                const long valueinserted = 42;
                s2.SetLong(0, 0, valueinserted);
                Assert.AreEqual(1, s2.Size());
                //actualres5 = Program.TableDumper(MethodBase.GetCurrentMethod().Name+4, "subtable in subtable with int", t);
                //now read back the 42

                long valueback = t.GetSubTable(1, 0).GetSubTable(2, 0).GetSubTable(3, 0).GetLong(0, 0);
                //                            root               s1                 s2            42


                Assert.AreEqual(valueback, valueinserted);
                actualres6 = Program.TableDumper(MethodBase.GetCurrentMethod().Name + 5, "subtable in subtable with int",
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
{ //Start row0
fld1:dump for type String not implemented yet,//column 0
root:[ //1 rows   { //Start row0
   fld2:dump for type String not implemented yet   ,//column 0
   fld3:dump for type String not implemented yet   ,//column 1
   s1:[ //1 rows      { //Start row0
      fld4:dump for type String not implemented yet      ,//column 0
      fld5:dump for type String not implemented yet      ,//column 1
      fld6:dump for type String not implemented yet      ,//column 2
      s2:[ //1 rows         { //Start row0
         fld:42         //column 0
         } //End row0
]      //column 3
      } //End row0
]   //column 2
   } //End row0
]//column 1
} //End row0
------------------------------------------------------
";
            Assert.AreEqual(expectedres, actualres6);
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
{ //Start row0
IntColumn1:1764,//column 0
IntColumn2:0,//column 1
IntColumn3:0//column 2
} //End row0
{ //Start row1
IntColumn1:0,//column 0
IntColumn2:-9223372036854775808,//column 1
IntColumn3:0//column 2
} //End row1
{ //Start row2
IntColumn1:0,//column 0
IntColumn2:0,//column 1
IntColumn3:-9223372036854775766//column 2
} //End row2
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
{ //Start row0
IntColumn1:1764,//column 0
IntColumn2:0,//column 1
IntColumn3:0,//column 2
SubTableWithInts:[ //3 rows   { //Start row0
   SubIntColumn1:2   ,//column 0
   SubIntColumn2:0   ,//column 1
   SubIntColumn3:0   //column 2
   } //End row0
   { //Start row1
   SubIntColumn1:0   ,//column 0
   SubIntColumn2:0   ,//column 1
   SubIntColumn3:0   //column 2
   } //End row1
   { //Start row2
   SubIntColumn1:0   ,//column 0
   SubIntColumn2:0   ,//column 1
   SubIntColumn3:0   //column 2
   } //End row2
]//column 3
} //End row0
{ //Start row1
IntColumn1:0,//column 0
IntColumn2:-9223372036854775808,//column 1
IntColumn3:0,//column 2
SubTableWithInts:[ //3 rows   { //Start row0
   SubIntColumn1:0   ,//column 0
   SubIntColumn2:0   ,//column 1
   SubIntColumn3:0   //column 2
   } //End row0
   { //Start row1
   SubIntColumn1:0   ,//column 0
   SubIntColumn2:2   ,//column 1
   SubIntColumn3:0   //column 2
   } //End row1
   { //Start row2
   SubIntColumn1:0   ,//column 0
   SubIntColumn2:0   ,//column 1
   SubIntColumn3:0   //column 2
   } //End row2
]//column 3
} //End row1
{ //Start row2
IntColumn1:0,//column 0
IntColumn2:0,//column 1
IntColumn3:-9223372036854775766,//column 2
SubTableWithInts:[ //3 rows   { //Start row0
   SubIntColumn1:0   ,//column 0
   SubIntColumn2:0   ,//column 1
   SubIntColumn3:0   //column 2
   } //End row0
   { //Start row1
   SubIntColumn1:0   ,//column 0
   SubIntColumn2:0   ,//column 1
   SubIntColumn3:0   //column 2
   } //End row1
   { //Start row2
   SubIntColumn1:0   ,//column 0
   SubIntColumn2:0   ,//column 1
   SubIntColumn3:2   //column 2
   } //End row2
]//column 3
} //End row2
------------------------------------------------------
";
            Assert.AreEqual(expectedres, actualres);
        }


        [Test]
        public static void TableRowColumnInsert()
        {
            String actualres;
            using (var t = new Table(new IntField("intfield"),new StringField("stringfield"),new IntField("intfield2")))
            {
                t.AddEmptyRow(5);
                long changeNumber = 0;
                foreach (TableRow tr in t)
                {
                    foreach (TableRowColumn trc in tr)
                    {
                        if(trc.ColumnType==DataType.Int)
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
{ //Start row0
intfield:1,//column 0
stringfield:Getting type String from TableRowColumn not implemented yet,//column 1
intfield2:2//column 2
} //End row0
{ //Start row1
intfield:3,//column 0
stringfield:Getting type String from TableRowColumn not implemented yet,//column 1
intfield2:4//column 2
} //End row1
{ //Start row2
intfield:5,//column 0
stringfield:Getting type String from TableRowColumn not implemented yet,//column 1
intfield2:6//column 2
} //End row2
{ //Start row3
intfield:7,//column 0
stringfield:Getting type String from TableRowColumn not implemented yet,//column 1
intfield2:8//column 2
} //End row3
{ //Start row4
intfield:9,//column 0
stringfield:Getting type String from TableRowColumn not implemented yet,//column 1
intfield2:10//column 2
} //End row4
------------------------------------------------------
";
            Assert.AreEqual(expectedres,actualres);
        }
    }




    [TestFixture]
    public static class StringEncodingTest
    {
        //Right now this test uses creation of tables as a test - the column name will be set to all sorts of crazy thing, and we want them back that way
        [Test]
        public static void TableWithPerThousandSign()
        {
            String actualres;
            using (
            var notSpecifyingFields = new Table(
                "subtable".Table()
                ))//at this point we have created a table with no fields
            {
                notSpecifyingFields.AddColumn(DataType.String, "12345‰7890");
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name, "table name is 12345 then the permille sign ISO 10646:8240 then 7890", notSpecifyingFields);
            }
            const string expectedres = @"------------------------------------------------------
Column count: 2
Table Name  : table name is 12345 then the permille sign ISO 10646:8240 then 7890
------------------------------------------------------
 0      Table  subtable            
 1     String  12345‰7890          
------------------------------------------------------

";
            Assert.AreEqual(expectedres,actualres);        
        }
    



        [Test]
        public static void TableWithJapaneseCharacters()
        {
            String actualres;
            using (
            var notSpecifyingFields = new Table(
                "subtable".Table()
                ))//at this point we have created a table with no fields
            {
                notSpecifyingFields.AddColumn(DataType.String,     "123\u70B9\u83DC678");
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name, "column name is 123 then two japanese characters then 678", notSpecifyingFields);
            }
            const string expectedres = @"------------------------------------------------------
Column count: 2
Table Name  : column name is 123 then two japanese characters then 678
------------------------------------------------------
 0      Table  subtable            
 1     String  123"+"\u70B9\u83DC"+@"678            
------------------------------------------------------

";
            Assert.AreEqual( expectedres,actualres);        
        }
    }


    
    [TestFixture]
    public static class CreateTableTest
    {

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
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name, "table with subtable with subtable", t);
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
                
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name, "table created with all types using the new field classes", newFieldClasses);
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

        
        class ItemCode : IntField //whenever ItemCode is specified in a table definition, an IntegerField is created
        {
            public ItemCode(String columnName) : base(columnName) { }           
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
                new IntField("UserId"), //some game specific stuff. All players are owned by some item, don't ask me why
                new BinaryField("BoardLayout"), //game specific
                GameSaveFields())
                )
            {
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name+"1", "table created user defined types and methods", game1);
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
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name + "2", "table created user defined types and methods", game2);
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
                ))//at this point we have created a table with no fields
            {
                notSpecifyingFields.AddColumn(DataType.String, "Buksestørrelse");
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name,"one field Created in two steps with table add column", notSpecifyingFields);
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
                actualres1 = Program.TableDumper(MethodBase.GetCurrentMethod().Name, "Table with all allowed types (String Extensions)", t);
                actualres2 = Program.TableDumperSpec(MethodBase.GetCurrentMethod().Name, "Table with all allowed types (String Extensions)", t);
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
                     new Field("Count",DataType.Int),
                     new Field("Valid",DataType.Bool),
                     new Field("Name",DataType.String),
                     new Field("BLOB",DataType.Binary),
                     new Field("Items",
                          new Field("ItemCount",DataType.Int), 
                          new Field("ItemName",DataType.String)),        
                     new Field("HtmlPage", DataType.Mixed),
                     new Field("FirstSeen",DataType.Date),
                     new Field("Fraction",DataType.Float),
                     new Field("QuiteLargeNumber",DataType.Double)
        ))
            {
                actualres1 = Program.TableDumper(MethodBase.GetCurrentMethod().Name, "Table with all allowed types (Field)", t);
                actualres2 = Program.TableDumperSpec(MethodBase.GetCurrentMethod().Name, "Table with all allowed types (Field)", t);
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
                     new Field("Count1","integer"),
                     new Field("Count2","Integer"),//Any case is okay
                     new Field("Count3","int"),
                     new Field("Count4","INT"),//Any case is okay
                     new Field("Valid1","boolean"),
                     new Field("Valid2","bool"),
                     new Field("Valid3","Boolean"),
                     new Field("Valid4","Bool"),
                     new Field("Name1","string"),
                     new Field("Name2","string"),
                     new Field("Name3","str"),
                     new Field("Name4","Str"),
                     new Field("BLOB1","binary"),
                     new Field("BLOB2","Binary"),
                     new Field("BLOB3","blob"),
                     new Field("BLOB4","Blob"),
                     new Field("Items",
                          new Field("ItemCount","integer"), 
                          new Field("ItemName","string")),        
                     new Field("HtmlPage1", "mixed"),
                     new Field("HtmlPage2", "MIXED"),
                     new Field("FirstSeen1","date"),
                     new Field("FirstSeen2","daTe"),
                     new Field("Fraction1","float"),
                     new Field("Fraction2","Float"),
                     new Field("QuiteLargeNumber1","double"),
                     new Field("QuiteLargeNumber2","Double")
        ))
            {
                actualres1 = Program.TableDumper(MethodBase.GetCurrentMethod().Name, "Table with all allowed types (Field_string)", t);
                actualres2 = Program.TableDumperSpec(MethodBase.GetCurrentMethod().Name, "Table with all allowed types (Field_string)", t);
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
                actualres1 = Program.TableDumper(MethodBase.GetCurrentMethod().Name, "Table with all allowed types (Typed Field)", t);
                actualres2 = Program.TableDumperSpec(MethodBase.GetCurrentMethod().Name, "Table with all allowed types (Typed Field)", t);
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
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name, "six colums,sub four columns", testtbl);
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
            Table testTable;//bad way to code this but i need the reference after the using clause
            using (testTable = new Table())
            {

                Assert.False(testTable.IsDisposed);//do a test to see that testtbl has a valid table handle 
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
            Field f5 = "f5".Int();//create a field reference, type does not matter
            f5 = "f5".Table(f5);//try to overwrite the field object with a new object that references itself 
            string actualres;
            using (
            var t = new Table(f5))//this will not crash or loop forever the subtable field does not references itself 
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
            Field fc = "fc".Int();//create a field reference, type does not matter
            Field fp = "fp".Table(fc);//let fp be the parent table subtable column, fc be the sole field in a subtable

            fc = "fc".Table(fp);//then change the field type from int to subtable and reference the parent

            //You now think You have illegal field definitions in fc and fp as both are subtables and both reference the other as the sole subtable field
            //however, they are new objects that reference the old classes that they replaced.
            String actualres;
            using (
                var t2 = new Table(fc))
            { //should crash too
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name, "subtable that has subtable that references its parent #1", t2);
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
            Field fc = "fc".Int();//create a field reference, type does not matter
            Field fp = "fp".Table(fc);//let fp be the parent table subtable column, fc be the sole field in a subtable
// ReSharper disable RedundantAssignment
            fc = "fc".Table(fp);//then change the field type from int to subtable and reference the parent
// ReSharper restore RedundantAssignment

            String actualres;
            using (
            var t3 = new Table(fp))
            { //should crash too
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name, "subtable that has subtable that references its parent #2", t3);
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
            var subTableElements =  f1.GetSubTableArray();
            subTableElements[0] = f1;//and the "f16" field in f1.f15.f16 is now replaced with f1.. recursiveness


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
        
        class AttemptCircularField : Field
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "fielddefinitions"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "fieldName")]
// ReSharper disable UnusedParameter.Local
            public  void setsubtablearray(String fieldName, Field[] fielddefinitions)//make the otherwise hidden addsubtablefield public
// ReSharper restore UnusedParameter.Local
            {
//uncommenting the line below should create a compiletime error (does now) or else this unit test wil bomb the system
//                AddSubTableFields(this, fieldName,fielddefinitions);
            }
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "columnName"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "subTableFieldsArray")]
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

            var f1 = new AttemptCircularField("f1",null);//do not care about last parameter we're trying to crash the system
            var subs= new Field[2];
            subs[0]=f1;
            f1.setsubtablearray("f2", subs);

            string actualres;
            using (var t4 = new Table(f1))
            {
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name, "cyclic field definition using field inheritance to get at subtable field list", t4);
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
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name, "subtable with two int fields", t5);//This is sort of okay, first adding a subtable, then
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
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name, "two fields, case is differnt", badtable);
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
            Assert.AreEqual(expectedres,actualres);
        }
        [Test]
        public static void TestCreateStrangeTable2()
        {
            //Create a table with two columns with the same name and type
            String actualres;
            using (var badtable2 = new Table("Age".Int(), "Age".Int()))
            {
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name, "two fields name and type the same", badtable2);
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
            var actualres = new StringBuilder();//we add several table dumps into one compare string in this test
            using (
            var testtbl1 = new Table(
            new Field("name", DataType.String),
            new Field("age", DataType.Int),
            new Field("comments",
                new Field("phone#1", DataType.String),
                new Field("phone#2", DataType.String)),
            new Field("whatever", DataType.Mixed)))
            {
                actualres.Append(Program.TableDumperSpec(MethodBase.GetCurrentMethod().Name, "four columns , sub two columns (Field)", testtbl1));

                using (//and we create a second table while the first is in scope
                var testtbl2 = new Table(
                    new Field("name", "String"),
                    new Field("age", "Int"),
                    new Field("comments",
                             new Field("phone#1", DataType.String),    //one way to declare a string
                             new Field("phone#2", "String"),           //another way
                             "more stuff".SubTable(
                                "stuff1".String(),                     //and yet another way
                                "stuff2".String(),
                                "ÆØÅæøå".String())
                             ),
                    new Field("whatever", DataType.Mixed)))
                {
                    actualres.Append(Program.TableDumperSpec(MethodBase.GetCurrentMethod().Name, "four columns, sub three subsub three", testtbl2));
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
            Assert.AreEqual(expectedres,actualres.ToString());
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
                actualres = Program.TableDumper(MethodBase.GetCurrentMethod().Name, "same names int two empty string names", reallybadtable3);
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
            Assert.AreEqual(expectedres,actualres);
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
                actualres=Program.TableDumper(MethodBase.GetCurrentMethod().Name, "same names, empty names, mixed types", reallybadtable4);
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
            Assert.AreEqual(expectedres,actualres);
        }

    }



    class Program
    {

        public static int Buildnumber = 1304041702;












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
        public static string TableDumper(String fileName, String tableName, Table t)
        {
            var res = new StringBuilder();//temporary storange of text of dump

            long count = t.ColumnCount;
            PrintHeader(res, tableName, count);
            for (long n = 0; n < count; n++)
            {
                string name = t.GetColumnName(n);
                DataType type = t.ColumnType(n);
                res.AppendLine(String.Format(CultureInfo.InvariantCulture, "{0,2} {2,10}  {1,-20}", n, name, type));
                if (type == DataType.Table)
                {                    
                    Spec subSpec =  t.Spec.GetSpec(n);
                    Specdumper(res, "   ", subSpec, "Subtable");
                }
            }
            PrintMetadataFooter(res);
            TableDataDumper("",res, t);

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
                res.AppendLine(String.Format(CultureInfo.InvariantCulture, "{0}{1,2} {2,10}  {3,-20}", indent, n, type, name));
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

            TableDataDumper("",res, t);
            Console.WriteLine(res.ToString());
            File.WriteAllText(fileName + ".txt", res.ToString());
            return res.ToString();
        }


        public static void TableDataDumper(string indent,StringBuilder res, Table table)
        {
            const string startrow = "{{ //Start row{0}";
            const string endrow = "}} //End row{0}";
            const string startfield = @"{0}:";
            const string endfield = ",//column {0}";
            const string endfieldlast = "//column {0}";//no comma
            const string starttable = "[ //{0} rows";
            const string endtable = "]";
            var firstdatalineprinted = false;
            long tableSize = table.Size();
            foreach (TableRow tr in table )  {
                if (firstdatalineprinted == false)
                {
                    if (String.IsNullOrEmpty(indent))
                    {
                        res.Append(indent);
                        res.AppendLine(String.Format(CultureInfo.InvariantCulture,"Table Data Dump. Rows:{0}",tableSize));
                        res.Append(indent);
                        res.AppendLine(Sectiondelimitor);
                    }
                    firstdatalineprinted = true;
                }
                res.Append(indent);                
                res.AppendLine(String.Format(CultureInfo.InvariantCulture,  startrow, tr.Row));//start row marker            
                foreach (TableRowColumn trc in tr)
                {
                    res.Append(indent);
                    string name = trc.Owner.Owner.GetColumnName(trc.ColumnIndex);//so we can see it easily in the debugger
                    res.Append(String.Format(CultureInfo.InvariantCulture, startfield, name));
                    if (trc.ColumnType == DataType.Table)
                    {
                        Table sub = table.GetSubTable(trc.ColumnIndex, tr.Row);//size printed here as we had a problem once with size reporting 0 where it should be larger, so nothing returned from call
                        res.Append(String.Format(CultureInfo.InvariantCulture, starttable,sub.Size()));
                        TableDataDumper(indent+"   ",res, sub);
                        res.Append(endtable);
                    }
                    else
                    {
                        if (trc.ColumnType == DataType.Mixed)
                        {
                            res.Append("Mixed dump not implemented yet");
                        }
                        res.Append(trc.Value);
                    }
                    res.Append(indent);
                    res.AppendLine(String.Format(CultureInfo.InvariantCulture, trc.IsLastColumn() ? endfieldlast : endfield, trc.ColumnIndex));
                    
                    //, trc.Value ));  //of course only works because we only have one type of row right now
                }
                res.Append(indent);
                res.AppendLine(String.Format(CultureInfo.InvariantCulture, endrow, tr.Row));//end row marker
                
            }
            if (firstdatalineprinted && String.IsNullOrEmpty(indent)) //some data was dumped from the table, so print a footer
            {
                res.Append(indent);
                res.AppendLine(Sectiondelimitor);
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

            EnvironmentTest.ShowVersionTest();
           // TableChangeDataTest.TableIntValueSubTableTest1();
            //CreateTableTest.TableSubtableSubtable();
            TableChangeDataTest.TableRowColumnInsert();
            TableParameterValidationTest.TableTestColumnIndexTooHigh();
            TableChangeDataTest.TableSubTableSubTable();

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
    //        StringEncodingTest.TableWithJapaneseCharacters();//when we have implemented utf-16 to utf-8 both ways, the following tests should be created:
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
                        
            Console.WriteLine("Press any key to finish test..");
            Console.ReadKey();
        }

    }
}
