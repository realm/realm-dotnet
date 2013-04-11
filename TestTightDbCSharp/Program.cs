using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TightDb.TightDbCSharp;

[assembly: CLSCompliant(true)] //mark the public interface of this program as cls compliant (can be run from any .net language)
namespace TestTightDbCS
{
    using System.IO;
    using NUnit.Framework;
    using TightDb.TightDbCSharp.Extensions;
    using System.Globalization;
    using System.Reflection;


    [TestFixture]
    public static class EnvironmentTest
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "tightccs"), 
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ImageFileMachine"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "PeKind"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String,System.Object)")]
        [Test]
        public static void ShowVersionTest()
        {
            var PointerSize = IntPtr.Size;
            var VmBitness = (PointerSize == 8) ? "64bit" : "32bit";
            OperatingSystem os = Environment.OSVersion;
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            PortableExecutableKinds peKind;
            ImageFileMachine machine;
            executingAssembly.ManifestModule.GetPEKind(out peKind,out machine);
           // String thisapplocation = executingAssembly.Location;

            System.Console.WriteLine("Build number :              {0}", Program.buildnumber);
            System.Console.WriteLine("Pointer Size :              {0}", PointerSize);
            System.Console.WriteLine("Process Running as :        {0}", VmBitness);
            System.Console.WriteLine("Built as PeKind :           {0}", peKind);
            System.Console.WriteLine("Built as ImageFileMachine : {0}", machine);
            System.Console.WriteLine("OS Version :                {0}", os.Version.ToString());
            System.Console.WriteLine("OS Platform:                {0}", os.Platform.ToString());
            System.Console.WriteLine("");
            System.Console.WriteLine("Now Loading tight_c_cs.dll  - expecting it to be a "+VmBitness+" dll!");
            //System.Console.WriteLine("Loading "+thisapplocation+"...");


            using (Table t = new Table())
            {
                System.Console.WriteLine("C++DLL         build number {0}", t.getdllversion_CPP());
                System.Console.WriteLine("C# DLL         build number {0}", t.getdllversion_CSH());
            }
            System.Console.WriteLine();
            System.Console.WriteLine();

        }
    }

    [TestFixture]
    public static class CreateTableTest
    {


        //this kind of creation call should be legal - it creates a totally empty table, then only later sets up a field        
        [Test]
        public static void SubTableNoFields()
        {
            String actualres;
            using (
            Table notSpecifyingFields = new Table(
                "subtable".Table()
                ))//at this point we have created a table with no fields
            {
                notSpecifyingFields.AddColumn(TDB.String, "Buksestørrelse");
                actualres = Program.TableDumper(MethodInfo.GetCurrentMethod().Name,"one field Created in two steps with table add column", notSpecifyingFields);
            }
            string expectedres =
@"------------------------------------------------------
Column count: 2
Table Name  : one field Created in two steps with table add column
------------------------------------------------------
 0      Table  subtable            
 1     String  Buksestørrelse      
------------------------------------------------------

";
            Assert.AreEqual(actualres, expectedres);        
        }


        [Test]
        public static void TestHandleAcquireOneField()
        {
            string actualres;
            using (Table testtbl = new Table(new TDBField("name", TDB.String)))
            {
                actualres = Program.TableDumper(MethodInfo.GetCurrentMethod().Name, "NameField", testtbl);
            }
            string expectedres =
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
            using (Table testtbl3 = new Table(
            "Name".TDBString(),
            "Age".TDBInt(),
            "count".TDBInt(),
            "Whatever".TDBMixed()
            ))
            {
                //long  test = testtbl3.getdllversion_CSH();
                actualres = Program.TableDumper(MethodInfo.GetCurrentMethod().Name, "four columns, Last Mixed", testtbl3);
            }
            string expectedres =
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
        public static void TestAllFieldTypes()
        {
            string actualres1;
            string actualres2;
            using (Table t = new Table(
        "IntField".Int(),
        "BoolField".Bool(),
        "StringField".String(),
        "BinaryFiel".Binary(),
        "TableField".Subtable(
            "subtablefield1".Int(),
            "subtablefield2".String()),
        "MixedField".Mixed(),
        "DateField".Date(),
        "FloatField".Float(),
        "DoubleField".Double()
        ))
            {
                actualres1 = Program.TableDumper(MethodInfo.GetCurrentMethod().Name, "Table with all allowed types", t);
                actualres2 = Program.TableDumperSpec(MethodInfo.GetCurrentMethod().Name, "Table with all allowed types", t);
            }
            string expectedres =
@"------------------------------------------------------
Column count: 9
Table Name  : Table with all allowed types
------------------------------------------------------
 0        Int  IntField            
 1       Bool  BoolField           
 2     String  StringField         
 3     Binary  BinaryFiel          
 4      Table  TableField          
    0        Int  subtablefield1      
    1     String  subtablefield2      
 5      Mixed  MixedField          
 6       Date  DateField           
 7      Float  FloatField          
 8     Double  DoubleField         
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
            Table testtbl = new Table(
                "Name".TDBString(),
                "Age".TDBInt(),
                new TDBField("age2", TDB.Int),
                new TDBField("age3", "Int"),
                new TDBField("comments",
                              new TDBField("phone#1", TDB.String),
                              new TDBField("phone#2", TDB.String),
                              new TDBField("phone#3", "String"),
                              "phone#4".TDBString()
                             ),
                new TDBField("whatever", TDB.Mixed)
                ))
            {
                actualres = Program.TableDumper(MethodInfo.GetCurrentMethod().Name, "six colums,sub four columns", testtbl);
            }
            string expectedres =
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
            Table TestTable;//bad way to code this but i need the reference after the using clause
            using (TestTable = new Table())
            {

                Assert.False(TestTable.IsDisposed);//do a test to see that testtbl has a valid table handle 
            }
            Assert.True(TestTable.IsDisposed);
            //do a test here to see that testtbl now does not have a valid table handle


        }



        //while You cannot cross-link parents and subtables inside a new table() construct, you can try to do so, by deliberatly changing
        //the subtable references in TDBField objects that You instantiate yourself -and then call Table.create(Yourfiled) with a 
        //field definition that is self referencing.
        //however, currently this is not possible as seen in the example below.
        //the subtables cannot be changed directly, so all You can do is create new objects that has old already created objects as subtables
        //therefore a tree structure, no recursion.

        //below is my best shot at someone trying to create a table with custom built cross-linked field definitions (and failing)

        //I did not design the TDBFIeld type to be used on its own like the many examples below. However , none of these weird uses break anything
        [Test]
        public static void TestIllegalFieldDefinitions1()
        {
            TDBField f5 = "f5".Int();//create a field reference, type does not matter
            f5 = "f5".Table(f5);//try to overwrite the field object with a new object that references itself 
            string actualres;
            using (
            Table t = new Table(f5))//this will not crash or loop forever the subtable field does not references itself 
            {
                actualres = Program.TableDumper(MethodInfo.GetCurrentMethod().Name, "self-referencing subtable", t);
            }
            string expectedres =
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
            TDBField fc = "fc".Int();//create a field reference, type does not matter
            TDBField fp = "fp".Table(fc);//let fp be the parent table subtable column, fc be the sole field in a subtable

            fc = "fc".Table(fp);//then change the field type from int to subtable and reference the parent

            //You now think You have illegal field definitions in fc and fp as both are subtables and both reference the other as the sole subtable field
            //however, they are new objects that reference the old classes that they replaced.
            String actualres;
            using (
                Table t2 = new Table(fc))
            { //should crash too
                actualres = Program.TableDumper(MethodInfo.GetCurrentMethod().Name, "subtable that has subtable that references its parent #1", t2);
            }
            String Expectedres =
@"------------------------------------------------------
Column count: 1
Table Name  : subtable that has subtable that references its parent #1
------------------------------------------------------
 0      Table  fc                  
    0      Table  fp                  
       0        Int  fc                  
------------------------------------------------------

";


            Assert.AreEqual(Expectedres, actualres);
        }
        [Test]
        public static void TestIllegalFieldDefinitions3()
        {
            TDBField fc = "fc".Int();//create a field reference, type does not matter
            TDBField fp = "fp".Table(fc);//let fp be the parent table subtable column, fc be the sole field in a subtable
            fc = "fc".Table(fp);//then change the field type from int to subtable and reference the parent

            String actualres;
            using (
            Table t3 = new Table(fp))
            { //should crash too
                actualres = Program.TableDumper(MethodInfo.GetCurrentMethod().Name, "subtable that has subtable that references its parent #2", t3);
            }
            String expectedres =
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
        [Test]
        public static void TestIllegalFieldDefinitions4()
        {

            TDBField f10 = "f10".Subtable("f11".Int(), "f12".Int());
            f10.type = TDB.Int;
            //at this time, the subtable array still have some subtables in it
            string actualres;
            using (Table t4 = new Table(f10))
            {
                actualres = Program.TableDumper(MethodInfo.GetCurrentMethod().Name, "just an int field, no subs", t4);
            }
            String expectedres =
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
            TDBField f10 = "f10".Subtable("f11".Int(), "f12".Int());
            f10.type = TDB.Table;

            String actualres;
            using (
         Table t5 = new Table(f10))
            {
                actualres = Program.TableDumper(MethodInfo.GetCurrentMethod().Name, "subtable with two int fields", t5);//This is sort of okay, first adding a subtable, then
            }
            String expectedres =
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
            using (Table badtable = new Table("Age".Int(), "age".Int()))
            {
                actualres = Program.TableDumper(MethodInfo.GetCurrentMethod().Name, "two fields, case is differnt", badtable);
            }
            String expectedres =
@"------------------------------------------------------
Column count: 2
Table Name  : two fields, case is differnt
------------------------------------------------------
 0        Int  Age                 
 1        Int  age                 
------------------------------------------------------

";
            Assert.AreEqual(actualres, expectedres);
        }
        [Test]
        public static void TestCreateStrangeTable2()
        {
            //Create a table with two columns with the same name and type
            String actualres;
            using (Table badtable2 = new Table("Age".Int(), "Age".Int()))
            {
                actualres = Program.TableDumper(MethodInfo.GetCurrentMethod().Name, "two fields name and type the same", badtable2);
            }
            string expectedres =
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
            StringBuilder actualres = new StringBuilder();//we add several table dumps into one compare string in this test
            using (
            Table testtbl1 = new Table(
            new TDBField("name", TDB.String),
            new TDBField("age", TDB.Int),
            new TDBField("comments",
                new TDBField("phone#1", TDB.String),
                new TDBField("phone#2", TDB.String)),
            new TDBField("whatever", TDB.Mixed)))
            {
                actualres.Append(Program.TableDumperSpec(MethodInfo.GetCurrentMethod().Name, "four columns , sub two columns (TDBField)", testtbl1));

                using (//and we create a second table while the first is in scope
                Table testtbl2 = new Table(
                    new TDBField("name", "String"),
                    new TDBField("age", "Int"),
                    new TDBField("comments",
                             new TDBField("phone#1", "String"),
                             new TDBField("phone#2", "String"),
                             "more stuff".Subtable(
                                "stuff1".String(),
                                "stuff2".String(),
                                "ÆØÅæøå".String())
                             ),
                    new TDBField("whatever", "Mixed")))
                {
                    actualres.Append(Program.TableDumperSpec(MethodInfo.GetCurrentMethod().Name, "four columns, sub three subsub three", testtbl2));
                }
            }
            File.WriteAllText(MethodInfo.GetCurrentMethod().Name + ".txt", actualres.ToString());
            string expectedres =
@"------------------------------------------------------
Column count: 4
Table Name  : four columns , sub two columns (TDBField)
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
            Assert.AreEqual(actualres.ToString(), expectedres);
        }

        [Test]
        public static void TestCreateStrangeTable3()
        {
            string actualres;
            using (
                Table Reallybadtable3 = new Table("Age".Int(),
                                                  "Age".Int(),
                                                  "".String(),
                                                  "".String()))
            {
                actualres = Program.TableDumper(MethodInfo.GetCurrentMethod().Name, "same names int two empty string names", Reallybadtable3);
            }
            string expectedres =
@"------------------------------------------------------
Column count: 4
Table Name  : same names int two empty string names
------------------------------------------------------
 0        Int  Age                 
 1        Int  Age                 
 2     String                      
 3     String                      
------------------------------------------------------

";
            Assert.AreEqual(actualres, expectedres);
        }

        [Test]
        public static void TestCreateStrangeTable4()
        {
            string actualres;
            using (
                Table Reallybadtable4 = new Table("Age".Int(),
                                      "Age".Mixed(),
                                      "".String(),
                                      "".Mixed()))
            {
                actualres=Program.TableDumper(MethodInfo.GetCurrentMethod().Name, "same names, empty names, mixed types", Reallybadtable4);
            }
            string expectedres =
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
            Assert.AreEqual(actualres, expectedres);
        }

    }



    class Program
    {

        public static int buildnumber = 1304041702;












        private static void printHeader(StringBuilder res, string tablename, long count)
        {
            res.AppendLine(headerline);
            res.AppendLine(String.Format(CultureInfo.InvariantCulture, "Column count: {0}", count));
            res.AppendLine(String.Format(CultureInfo.InvariantCulture, "Table Name  : {0}", tablename));
            res.AppendLine(headerline);            
        }


        private static void printFooter(StringBuilder res)
        {
            res.AppendLine(headerline);
            res.AppendLine("");
        }

        static string headerline = "------------------------------------------------------";


        //dumps table structure to a string for debugging purposes.
        //the string is easily human-readable
        //this version uses the table column information as far as possible, then shifts to spec on subtables
        public static string TableDumper(String fileName, String tableName, Table t)
        {
            StringBuilder res = new StringBuilder();//temporary storange of text of dump

            long count = t.column_count();
            printHeader(res, tableName, count);
            for (long n = 0; n < count; n++)
            {
                string name = t.get_column_name(n);
                TDB type = t.column_type(n);
                res.AppendLine(String.Format(CultureInfo.InvariantCulture, "{0,2} {2,10}  {1,-20}", n, name, type));
                if (type == TDB.Table)
                {
                    Spec tblspec = t.get_spec();
                    Spec subspec = tblspec.get_spec(n);
                    specdumper(res, "   ", subspec, "Subtable");
                }
            }
            printFooter(res);
            System.Console.Write(res.ToString());
            File.WriteAllText(fileName + ".txt", res.ToString());
            return res.ToString();
        }


        private static void specdumper(StringBuilder res, String indent, Spec s, string TableName)
        {

            long count = s.get_column_count();

            if (String.IsNullOrEmpty(indent))
            {
                printHeader(res, TableName, count);
            }

            for (long n = 0; n < count; n++)
            {
                String name = s.get_column_name(n);
                TDB type = s.get_column_type(n);
                res.AppendLine(String.Format(CultureInfo.InvariantCulture, "{0}{1,2} {2,10}  {3,-20}", indent, n, type, name));
                if (type == TDB.Table)
                {
                    Spec subspec = s.get_spec(n);
                    specdumper(res, indent + "   ", subspec, "Subtable");
                }
            }

            if (String.IsNullOrEmpty(indent))
            {
                printFooter(res);
            }
        }

        //dump the table only using its spec
        public static String TableDumperSpec(String fileName, String tablename, Table t)
        {
            StringBuilder res = new StringBuilder();
            Spec s = t.get_spec();
            specdumper(res, "", s, tablename);
            File.WriteAllText(fileName + ".txt", res.ToString());
            return res.ToString();
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)")]
        static void Main(/*string[] args*/)
        {
            /*
             *  to debug unit tests, uncomment the lines below, and run the test(s) you want to have debugged
             *  remember to set a breakpoint
             *  Don't run the program in Nunit, simply debug it in visual studio when it runs like an ordinary program
             *  */

            EnvironmentTest.ShowVersionTest();

            CreateTableTest.TestHandleAcquireOneField();

            CreateTableTest.TestHandleAcquireOneField();

            CreateTableTest.TestCreateTwoTables();
            CreateTableTest.TestTableScope();

            CreateTableTest.TestHandleAcquireSeveralFields();


            CreateTableTest.TestMixedConstructorWithSubTables();

            CreateTableTest.TestAllFieldTypes();

            CreateTableTest.TestIllegalFieldDefinitions1();
            CreateTableTest.TestIllegalFieldDefinitions2();
            CreateTableTest.TestIllegalFieldDefinitions3();
            CreateTableTest.TestIllegalFieldDefinitions4();
            CreateTableTest.TestIllegalFieldDefinitions5();

            
            System.Console.WriteLine("Press any key to finish test..");
            System.Console.ReadKey();
        }

    }
}
