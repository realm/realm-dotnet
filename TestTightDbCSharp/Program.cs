using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TestTightDbCS
{
    using System.IO;
    using NUnit.Framework;
    using TightDb.TightDbCSharp;
    using TightDb.TightDbCSharp.Extensions;


    [TestFixture]
    public class EnvironmentTest
    {
        [Test]
        public void showversionTest()
        {

            var VmBitness = (IntPtr.Size == 8) ? "64bit" : "32bit";

            System.Console.WriteLine("Testprogram    build number {0}", Program.buildnumber);
            OperatingSystem os = Environment.OSVersion;
            System.Console.WriteLine("IntPtr Size :               {0}", IntPtr.Size);
            System.Console.WriteLine("Process Running as :        {0}", VmBitness);
            System.Console.WriteLine("OS Version :                {0}", os.Version.ToString());
            System.Console.WriteLine("OS Platform:                {0}", os.Platform.ToString());
            Table t = new Table();
            System.Console.WriteLine("C++DLL         build number {0}", t.getdllversion_CPP());
            System.Console.WriteLine("C# DLL         build number {0}", t.getdllversion_CSH());

            System.Console.WriteLine();
            System.Console.WriteLine();
                     
        
        }
    }

    [TestFixture]
    public class CreateTableTest
    {
        
        [Test]
        public  void testhandleacquireOneField()
        {
            Table t = new Table();
            Table testtbl = new Table(new TDBField("name", TDB.String));            
            string actualres = Program.tabledumper("NameField", testtbl);
            File.WriteAllText("testhandleacquireOneField.txt", actualres);

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
        public  void testhandleaquireSeveralFields()
        {
            Table testtbl3 = new Table(
            "Name".TDBString(),
            "Age".TDBInt(),
            "count".TDBInt(),
            "Whatever".TDBMixed()
            );
            //long  test = testtbl3.getdllversion_CSH();
            String actualres = Program.tabledumper("four columns, Last Mixed", testtbl3);
            File.WriteAllText("testhandleaquireSeveralFields.txt", actualres);

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


        [Test]
        public  void testallkindsoffields()
        {
            Table t = new Table(
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
        );

            

            String actualres = Program.tabledumper("Table with all allowed types", t);
            File.WriteAllText("testallkindsoffields.txt", actualres);

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
            Assert.AreEqual(expectedres, actualres);



        }



        //test with a subtable
        [Test]
        public  void testhandleaquireSeveralFieldsSubtables()
        {
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
                );
            long test = testtbl.getdllversion_CSH();
            String actualres = Program.tabledumper("six colums,sub four columns", testtbl);


            File.WriteAllText("testhandleaquireSeveralFieldsSubtables.txt", actualres);

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
        public  void testtablescope()
        {
            Table testtbl;//bad way to code this but i need the reference after the using clause
            using (testtbl = new Table())
            {

                Assert.False(testtbl.IsDisposed);//do a test to see that testtbl has a valid table handle 
            }
            Assert.True(testtbl.IsDisposed);
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
        public  void testillegalfielddefinitions()
        {
            TDBField f5 = "f5".Int();//create a field reference, type does not matter
            f5 = "f5".Table(f5);//try to overwrite the field object with a new object that references itself 

            Table t = new Table(f5);//this will not crash or loop forever the subtable field does not references itself 
            String actualres = Program.tabledumper("self-referencing subtable", t);
            File.WriteAllText("testillegalfielddefinitions1.txt", actualres);
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

            TDBField fc = "fc".Int();//create a field reference, type does not matter
            TDBField fp = "fp".Table(fc);//let fp be the parent table subtable column, fc be the sole field in a subtable

            fc = "fc".Table(fp);//then change the field type from int to subtable and reference the parent

            //You now think You have illegal field definitions in fc and fp as both are subtables and both reference the other as the sole subtable field
            //however, they are new objects that reference the old classes that they replaced.

            Table t2 = new Table(fc); //should crash too
             actualres = Program.tabledumper("subtable that has subtable that references its parent #1", t2);
             File.WriteAllText("testillegalfielddefinitions2.txt", actualres);

             expectedres =
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

            Table t3 = new Table(fp); //should crash too
             actualres = Program.tabledumper("subtable that has subtable that references its parent #2", t3);
             File.WriteAllText("testillegalfielddefinitions3.txt", actualres);
             expectedres =
@"------------------------------------------------------
Column count: 1
Table Name  : subtable that has subtable that references its parent #2
------------------------------------------------------
 0      Table  fp                  
    0        Int  fc                  
------------------------------------------------------

";

            Assert.AreEqual(expectedres, actualres);


            TDBField f10 = "f10".Subtable("f11".Int(), "f12".Int());
            f10.type = TDB.Int;
            //at this time, the subtable array still have some subtables in it
            Table t4 = new Table(f10);
             actualres = Program.tabledumper("just an int field, no subs", t4);
             File.WriteAllText("testillegalfielddefinitions4.txt", actualres);
             expectedres =
@"------------------------------------------------------
Column count: 1
Table Name  : just an int field, no subs
------------------------------------------------------
 0        Int  f10                 
------------------------------------------------------

";
             
            Assert.AreEqual(expectedres, actualres);

            f10.type = TDB.Table;
            Table t5 = new Table(f10);
             actualres = Program.tabledumper("subtable with two int fields", t5);//This is sort of okay, first adding a subtable, then
             File.WriteAllText("testillegalfielddefinitions5.txt", actualres);

             expectedres =
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

    }

    
    class Program
    {

        public static int buildnumber = 1304041702;





        //this kind of call should be legal - it just means we'll get back to specifying the subtable some other time in a more dynamic fashion
        public static void subtablenofields() 
        {
            Table notspecifyingfields = new Table(
                "subtable".Table()
                );
        }





        //more tests
        public static void testcreatetwotables()
        {
            Table testtbl1 = new Table(
            new TDBField("name", TDB.String),
            new TDBField("age", TDB.Int),
            new TDBField("comments",
                new TDBField("phone#1", TDB.String),
                new TDBField("phone#2", TDB.String)),
            new TDBField("whatever", TDB.Mixed));

            tabledumper_spec("four columns , sub two columns (TDBField)", testtbl1);

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
                new TDBField("whatever", "Mixed"));
            tabledumper_spec("four columns, sub three subsub three", testtbl2);

            Table testtbl3 = new Table(
                "Name".String(),
                "Age".Int(),
                "Comments".Subtable(
                          "Phone#1".String(),
                          "Phone#2".String()),
                "count".Int(),
                "Whatever".Mixed());
            tabledumper_spec("five columns, sub two. using name.Type() ", testtbl3);
        }


        public static void testcreatestrangetable()
        {
            //create a table with two columns that are the same name except casing (this might be perfectly legal, I dont know)
            Table badtable= new Table("Age".Int(), "age".Int());

            tabledumper("two fields, case is differnt",badtable);
            //Create a table with two columns with the same name and type

            Table badtable2= new Table("Age".Int(), "Age".Int());
            tabledumper("two fields name and type the same",badtable2);

            Table Reallybadtable3 = new Table("Age".Int(),
                                              "Age".Int(),
                                              "".String(),
                                              "".String());
            tabledumper("same names int two empty string names", Reallybadtable3);

            Table Reallybadtable4 = new Table("Age".Int(),
                                  "Age".Mixed(),
                                  "".String(),
                                  "".Mixed());
            tabledumper("same names, empty names, mixed types", Reallybadtable4);


        }


        public static void printheader(StringBuilder res, string tablename, long count) 
        {
            res.AppendLine(headerline);
            res.AppendLine(String.Format("Column count: {0}", count));
            res.AppendLine(String.Format("Table Name  : {0}", tablename));
            res.AppendLine(headerline);
            //            System.Console.WriteLine("{0,2} {2,10}  {1,-20}","#","Name","Type");
        }


        public static void printfooter(StringBuilder res)
        {
            res.AppendLine(headerline);
            res.AppendLine("");
        }

        static string headerline = "------------------------------------------------------";
        
        
        //dumps table structure to a string for debugging purposes.
        //the string is easily human-readable
        //this version uses the table column information as far as possible, then shifts to spec on subtables
        public static string tabledumper(String TableName,Table T)
        {
            StringBuilder res = new StringBuilder();//temporary storange of text of dump
            
            long count = T.column_count();
            printheader(res,TableName, count);
            for (long n = 0; n < count; n++)
            {
                string name = T.get_column_name(n);
                TDB type = T.column_type(n);
                res.AppendLine(String.Format("{0,2} {2,10}  {1,-20}", n, name, type));
                if (type == TDB.Table)
                {
                    Spec tblspec = T.get_spec();
                    Spec subspec = tblspec.get_spec(n);
                    specdumper(res,"   ",subspec,"Subtable");
                }
            }
            printfooter(res);
            System.Console.Write(res.ToString());
            return res.ToString();
        }


        public static void specdumper(StringBuilder res,String indent,Spec s,string TableName)
        {

            long count = s.get_column_count();
            
            if (indent == "") 
            {
                printheader(res ,TableName, count);
            }

            for (long n = 0; n < count; n++)
            {
                String name = s.get_column_name(n);
                TDB type = s.get_column_type(n);
                res.AppendLine(String.Format(indent+"{0,2} {2,10}  {1,-20}", n, name, type));
                if (type == TDB.Table)
                {                
                    Spec subspec = s.get_spec(n);
                    specdumper(res, indent+"   ",subspec,"Subtable");
                }
            }

            if (indent == "")
            {
                printfooter(res);
            }
        }

        //dump the table only using its spec
        public static void tabledumper_spec(String Tablename, Table T)
        {
            StringBuilder res = new StringBuilder();
            Spec s = T.get_spec();
            long count = T.column_count();
            specdumper(res, "", s, Tablename);
        }


        static void Main(string[] args)
        {
            /*
             *  to debug unit tests, uncomment the lines below, and run the test(s) you want to have debugged
             *  remember to set a breakpoint
             *  Don't run the program in Nunit, simply debug it in visual studio when it runs like an ordinary program
             *  */

            var test1 = new EnvironmentTest();
            test1.showversionTest();
            var test2 = new CreateTableTest();            
            test2.testhandleacquireOneField();
            



            //if the user uses using with the table, it shoud be disposed at the end of the using block
            //using usage should follow these guidelines http://msdn.microsoft.com/en-us/library/yh598w02.aspx
            //You don't *have* to use using, if you don't the c++ table will not be disposed of as quickly as otherwise
            
            using (Table testtable = new Table())
            {
                long columncnt = testtable.column_count();                
            }        //table dispose sb calledautomatically  after table goes out of scope


            //testhandleacquireOneField();


            //test the unit test            

            //testhandleacquireOneField();


            //testtablescope();            

            //testhandleaquireSeveralFields();

            
            //testhandleaquireSeveralFieldsSubtables();

            //testallkindsoffields();

            //testillegalfielddefinitions();

            testcreatetwotables();

            System.Console.ReadKey();
        }

    }
}
