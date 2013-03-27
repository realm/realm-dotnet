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
            tabledumper("Table with a name field", testtbl);

        }

        public static void testhandleaquireSeveralFields()
        {
            Table testtbl3 = new Table(
            "Name".String(),
            "Age".Int(),
            "count".Int(),
            "Whatever".Mixed());
            long  test = testtbl3.getdllversion_CSH();
            tabledumper("Table with name,age,count,whatever fields", testtbl3);

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
            tabledumper("table with 6 colums, No 5 is table colum", testtbl);
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


            Table testtbl2 = new Table(
                new TDBField("name", "String"),
                new TDBField("age", "Int"),
                new TDBField("comments",
                         new TDBField("phone#1", "String"),
                         new TDBField("phone#2", "String")),
                new TDBField("whatever", "Mixed"));

            Table testtbl3 = new Table(
                "Name".String(),
                "Age".Int(),
                "Comments".Subtable(
                          "Phone#1".String(),
                          "Phone#2".String()),
                "count".Int(),
                "Whatever".Mixed());

            long  test = testtbl1.getdllversion_CPP();// keep gc from taking the tables out earlier than here bc we don't use them anymore
            long  test2 = testtbl2.getdllversion_CPP();
            long  test3 = testtbl3.getdllversion_CPP();

            long test4=test+test2+test3;
        }


        public static void testcreatestrangetable()
        {
            //create a table with two columns that are the same name except casing (this might be perfectly legal, I dont know)
            Table badtable= new Table("Age".Int(), "age".Int());

            tabledumper("two fields only case is differnt in their names",badtable);
            //Create a table with two columns with the same name and type

            Table badtable2= new Table("Age".Int(), "Age".Int());
            tabledumper("two fieldsname and type is exactly the same",badtable);

            Table Reallybadtable3 = new Table("Age".Int(), "Age".Int());
            tabledumper("two fieldsname and type is exactly the same", badtable);


        }


        //dumps table structure to console for debugging purposes
        public static void tabledumper(String TableName,Table T)
        {
            long count = T.column_count();
            System.Console.WriteLine("Table {0} has column count: {1}",TableName,  count);
            for (long n = 0; n < count; n++)
            {
                string name = T.get_column_name(n);
                TDB type = T.column_type(n);
                System.Console.WriteLine("Field {0} name:'{1}'  type {2}",n, name,type);
            }

        }



        static void Main(string[] args)
        {
            Table t = new Table();
            System.Console.WriteLine("C++DLL build number {0}",t.getdllversion_CPP());
            //if the user uses using with the table, it shoud be disposed at the end of the using block
            //using usage should follow these guidelines http://msdn.microsoft.com/en-us/library/yh598w02.aspx
            //You don't *have* to use using, if you don't the c++ table will not be disposed of as quickly as otherwise
            
            tabledumper("empty table",t);
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

            System.Console.ReadKey();
        }

    }
}
