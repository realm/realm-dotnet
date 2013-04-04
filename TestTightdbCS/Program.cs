using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tightdb.Tightdbcsharp;

namespace TestTightdbCS
{
    class Program
    {

        public static void testtablescope()
        {
            Table testtbl = new Table();
        }

        public static void testhandleacquireOneField()
        {
            Table testtbl = new Table(new TDBField("name", TDB.String));
            long  test = testtbl.getdllversion_CSH();
            tabledumper("NameField", testtbl);
        }

        public static void testhandleaquireSeveralFields()
        {
            Table testtbl3 = new Table(
            "Name".String(),
            "Age".Int(),
            "count".Int(),
            "Whatever".Mixed());
            //long  test = testtbl3.getdllversion_CSH();
            tabledumper("4 Fields, Last Mixed", testtbl3);
        }

        
        public static void testhandleaquireSeveralFieldsSubtables()
        {
            Table testtbl = new Table(
                "Name".String(),
                "Age".Int(),
                new TDBField("age2", TDB.Int),
                new TDBField("age3", "Int"),
                new TDBField("comments",
                              new TDBField("phone#1", TDB.String),
                              new TDBField("phone#2", TDB.String),
                              new TDBField("phone#3", "String"),
                              "phone#4".String()
                             ),
                new TDBField("whatever", TDB.Mixed)
                );
            long  test = testtbl.getdllversion_CSH();
            tabledumper("6 colums,subtable 4 columns", testtbl);
        }

        public static void testcreatetwotables()
        {
            Table testtbl1 = new Table(
            new TDBField("name", TDB.String),
            new TDBField("age", TDB.Int),
            new TDBField("comments",
                new TDBField("phone#1", TDB.String),
                new TDBField("phone#2", TDB.String)),
            new TDBField("whatever", TDB.Mixed));

            tabledumper_spec("created with TDBField", testtbl1);

            Table testtbl2 = new Table(
                new TDBField("name", "String"),
                new TDBField("age", "Int"),
                new TDBField("comments",
                         new TDBField("phone#1", "String"),
                         new TDBField("phone#2", "String")),
                new TDBField("whatever", "Mixed"));
            tabledumper_spec("created with TDBField and strings as types", testtbl2);


            Table testtbl3 = new Table(
                "Name".String(),
                "Age".Int(),
                "Comments".Subtable(
                          "Phone#1".String(),
                          "Phone#2".String()),
                "count".Int(),
                "Whatever".Mixed());
            tabledumper_spec("created using methods on strings", testtbl3);

        }


        public static void testcreatestrangetable()
        {
            //create a table with two columns that are the same name except casing (this might be perfectly legal, I dont know)
            Table badtable= new Table("Age".Int(), "age".Int());

            tabledumper("two fields only case is differnt in their names",badtable);
            //Create a table with two columns with the same name and type

            Table badtable2= new Table("Age".Int(), "Age".Int());
            tabledumper("two fields name and type is exactly the same",badtable2);

            Table Reallybadtable3 = new Table("Age".Int(),
                                              "Age".Int(),
                                              "".String(),
                                              "".String());
            tabledumper("same names int two empty string names string", Reallybadtable3);

            Table Reallybadtable4 = new Table("Age".Int(),
                                  "Age".Mixed(),
                                  "".String(),
                                  "".Mixed());
            tabledumper("same names, empty names, mixed types", Reallybadtable4);


        }


        static string headerline = "------------------------------";
        //dumps table structure to console for debugging purposes
        public static void tabledumper(String TableName,Table T)
        {
            
            long count = T.column_count();
            System.Console.WriteLine(headerline);
            System.Console.WriteLine("Column count: {0}",count);
            System.Console.WriteLine("Table Name  : {0}",TableName);
            System.Console.WriteLine(headerline);
            //            System.Console.WriteLine("{0,2} {2,10}  {1,-20}","#","Name","Type");


            for (long n = 0; n < count; n++)
            {
                string name = T.get_column_name(n);
                TDB type = T.column_type(n);
                System.Console.WriteLine("{0,2} {2,10}  {1,-20}", n, name, type);
                if (type == TDB.Table)
                {
                    Spec tblspec = T.get_spec();
                    Spec subspec = tblspec.get_spec(n);
                    specdumper("   ",subspec,"Subtable");
                }
            }
            System.Console.WriteLine(headerline);
            System.Console.WriteLine("");
        }


        public static void specdumper(String indent,Spec s,string TableName)
        {

            long count = s.get_column_count();
            
            if (indent == "") 
            {
                System.Console.WriteLine(headerline);
                System.Console.WriteLine("Column count: {0}", count);
                System.Console.WriteLine("Table Name  : {0}", TableName);
                System.Console.WriteLine(headerline);
            }

            for (long n = 0; n < count; n++)
            {
                String name = s.get_column_name(n);
                TDB type = s.get_column_type(n);
                System.Console.WriteLine(indent+"{0,2} {2,10}  {1,-20}", n, name, type);
                if (type == TDB.Table)
                {
                    System.Console.WriteLine(indent + "Subtable");
                    Spec subspec = s.get_spec(n);
                    specdumper(indent+"   ",subspec,"Subtable");
                }
            }
        }

        public static void tabledumper_spec(String Tablename, Table T)
        {
            System.Console.WriteLine(headerline);

            Spec s = T.get_spec();
            long count = T.column_count();
            specdumper("",s,Tablename);
            System.Console.WriteLine(headerline);
        }


        static void Main(string[] args)
        {
            Table t = new Table();
            System.Console.WriteLine("C++DLL build number {0}",t.getdllversion_CPP());
            //if the user uses using with the table, it shoud be disposed at the end of the using block
            //using usage should follow these guidelines http://msdn.microsoft.com/en-us/library/yh598w02.aspx
            //You don't *have* to use using, if you don't the c++ table will not be disposed of as quickly as otherwise
            
//            tabledumper("empty table",t);
            using (Table testtable = new Table())
            {
                System.Console.WriteLine("C# DLL build number {0}",testtable.getdllversion_CSH());
                //do operations
            }        //table dispose sb calledautomatically  after table goes out of scope


            testtablescope();

            testhandleacquireOneField();

            testhandleaquireSeveralFields();

            testhandleaquireSeveralFieldsSubtables();
            testcreatestrangetable();

            testcreatetwotables();

            System.Console.ReadKey();
        }

    }
}
