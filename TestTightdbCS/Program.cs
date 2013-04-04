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

        public static int buildnumber = 1304041702;
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
            "Name".    TDBString(),
            "Age".     TDBInt(),
            "count".   TDBInt(),
            "Whatever".TDBMixed()                                
            );
            //long  test = testtbl3.getdllversion_CSH();
            tabledumper("four columns, Last Mixed", testtbl3);
        }


        public static void testallkindsoffields()
        {
            Table t = new Table(
        "IntField".Int(),
        "BoolField".Bool(),
        "StringField".String(),
        "BinaryFiel".Binary(),
        "TableField".Table(
            "subtablefield1".Int(),
            "subtablefield2".String()),
        "MixedField".Mixed(),
        "DateField".Date(),
        "FloatField".Float(),
        "DoubleField".Double()
        );
            tabledumper("Table with all allowed types", t);
        }

        //this kind of call should be legal - it just means we'll get back to specifying the subtable some other time in a more dynamic fashion
        public static void subtablenofields() 
        {
            Table notspecifyingfields = new Table(
                "subtable".Table()
                );
        }

        //while You cannot cross-link parents and subtables inside a new table() construct, you can try to do so, by deliberatly changing
        //the subtable references in TDBField objects that You instantiate yourself -and then call Table.create(Yourfiled) with a 
        //field definition that is self referencing.
        //however, currently this is not possible as seen in the example below.
        //the subtables cannot be changed directly, so all You can do is create new objects that has old already created objects as subtables
        //therefore a tree structure, no recursion.

        //below is my best shot at someone trying to create a table with custom built cross-linked field definitions (and failing)

        //I did not design the TDBFIeld type to be used on its own like the many examples below. However , none of these weird uses break anything
        public static void testillegalfielddefinitions()
        {
            TDBField f5 = "f5".Int();//create a field reference, type does not matter
            f5 = "f5".Table(f5);//try to overwrite the field object with a new object that references itself 
            
            Table t = new Table(f5);//this will not crash or loop forever the subtable field does not references itself 
            tabledumper("self-referencing subtable",t);

            TDBField fc = "fc".Int();//create a field reference, type does not matter
            TDBField fp = "fp".Table(fc);//let fp be the parent table subtable column, fc be the sole field in a subtable

            fc = "fc".Table(fp);//then change the field type from int to subtable and reference the parent

            //You now think You have illegal field definitions in fc and fp as both are subtables and both reference the other as the sole subtable field
            //however, they are new objects that reference the old classes that they replaced.

            Table t2 = new Table(fc); //should crash too
            tabledumper("subtable that has subtable that references its parent #1",t2);
            Table t3 = new Table(fp); //should crash too
            tabledumper("subtable that has subtable that references its parent #2", t3);


            TDBField f10 = "f10".Subtable("f11".Int(), "f12".Int());
            f10.type = TDB.Int;
            //at this time, the subtable array still have some subtables in it
            Table t4 = new Table(f10);
            tabledumper("just an int field, no subs",t4);

            f10.type = TDB.Table;
            Table t5 = new Table(f10);
            tabledumper("subtable with two int fields", t5);//This is sort of okay, first adding a subtable, then
            //changing mind and making it just and int field, and then changing mind again and setting it as subtable type
            //and thus resurfacing the two subfields. no harm done.

        }



        //test with a subtable
        public static void testhandleaquireSeveralFieldsSubtables()
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
            long  test = testtbl.getdllversion_CSH();
            tabledumper("six colums,sub four columns", testtbl);
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


        public static void printheader(string tablename, long count) 
        {
            System.Console.WriteLine(headerline);
            System.Console.WriteLine("Column count: {0}", count);
            System.Console.WriteLine("Table Name  : {0}", tablename);
            System.Console.WriteLine(headerline);
            //            System.Console.WriteLine("{0,2} {2,10}  {1,-20}","#","Name","Type");
        }


        public static void printfooter()
        {
            System.Console.WriteLine(headerline);
            System.Console.WriteLine("");
        }
        static string headerline = "------------------------------------------------------";
        //dumps table structure to console for debugging purposes
        //this version uses the table column information as far as possible, then shifts to spec on subtables
        public static void tabledumper(String TableName,Table T)
        {
            
            long count = T.column_count();
            printheader(TableName, count);
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
            printfooter();
        }


        public static void specdumper(String indent,Spec s,string TableName)
        {

            long count = s.get_column_count();
            
            if (indent == "") 
            {
                printheader(TableName, count);
            }

            for (long n = 0; n < count; n++)
            {
                String name = s.get_column_name(n);
                TDB type = s.get_column_type(n);
                System.Console.WriteLine(indent+"{0,2} {2,10}  {1,-20}", n, name, type);
                if (type == TDB.Table)
                {                
                    Spec subspec = s.get_spec(n);
                    specdumper(indent+"   ",subspec,"Subtable");
                }
            }

            if (indent == "")
            {
                printfooter();
            }
        }

        //dump the table only using its spec
        public static void tabledumper_spec(String Tablename, Table T)
        {
            Spec s = T.get_spec();
            long count = T.column_count();
            specdumper("", s, Tablename);
        }


        static void Main(string[] args)
        {
            Table t = new Table();
            System.Console.WriteLine("Testprogram    build number {0}",buildnumber);
            System.Console.WriteLine("C++DLL         build number {0}",t.getdllversion_CPP());
            System.Console.WriteLine("C# DLL         build number {0}",t.getdllversion_CSH());
            OperatingSystem os = Environment.OSVersion;
            System.Console.WriteLine("OS Version :                {0}" ,os.Version.ToString());
            System.Console.WriteLine("OS Platform:                {0}", os.Platform.ToString());

            System.Console.WriteLine();
            System.Console.WriteLine();
            
            //if the user uses using with the table, it shoud be disposed at the end of the using block
            //using usage should follow these guidelines http://msdn.microsoft.com/en-us/library/yh598w02.aspx
            //You don't *have* to use using, if you don't the c++ table will not be disposed of as quickly as otherwise
            
            using (Table testtable = new Table())
            {
                long columncnt = testtable.column_count();                
            }        //table dispose sb calledautomatically  after table goes out of scope


            testtablescope();

            testhandleacquireOneField();

            testhandleaquireSeveralFields();

            testhandleaquireSeveralFieldsSubtables();

            testallkindsoffields();

            testillegalfielddefinitions();

            testcreatetwotables();

            System.Console.ReadKey();
        }

    }
}
