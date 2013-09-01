using System;
using TightDbCSharp;
using TightDbCSharp.Extensions;




namespace DynamicTable
{
    static class Program
    {
// ReSharper disable once UnusedParameter.Local
        private static void Assert(bool condition )
        {
            if (!condition)
            {
                throw new Exception();
            }
        }

        //this method resembles the java dynamic table example at http://www.tightdb.com/documentation/Java_ref/4/Table/
        static void Main()
        {
            using (var tbl = new Table())
            {

                tbl.AddIntColumn( "myInt");
                tbl.AddStringColumn( "myStr");
                tbl.AddMixedColumn("myMixed");

                //
                //add some data by setting whole rows
                //
                //add some data
                tbl.Add(12, "hello", 2);
                tbl.Add(-15, "World", "I can be different types...");
                tbl.Insert(0, 64, "I'm now first", true);     //data in order of columns
                tbl.AddEmptyRow(1);                           //append row at end of table - default values
                tbl.Set(3, 198, "TightDB", 12.345);           //set values in row 3
                tbl[3].SetRow(198, "TightDB", 12.345);        //alternative syntax, specifying row as an index into the table                
                tbl.Remove(0);                                //remove row 0
                tbl.RemoveLast();                             //remove last row

                //Get and set cell values
                Assert(-15== tbl.GetLong(0, 1));
                tbl.SetMixedString(2, 0, "Changed Long value to String");//third parameter type must be string
                tbl.SetMixedString("myMixed", 0, "Changed Long value to String");//alternative syntax,specifying the column by name
                tbl.SetMixed(2, 0, "Changed long value to String");//untyped, mixed will adopt whatever Tightdb type matches best with the data in the last parameter, in this case DataType.String

                Assert(DataType.String== tbl.GetMixedType(2, 0)); //the mixed field should be a string mixed now
                Assert(2==tbl.Size);
                Assert(false== tbl.IsEmpty);

                tbl.RenameColumn(0, "myLong");
                tbl.RemoveColumn(1);
                tbl.Add(42, "this is the mixed column");  //add a row
                tbl.AddDoubleColumn("myDouble");
                tbl.Add(-15, "still mixed", 123.45);

                //column introspection
                Assert(3== tbl.ColumnCount);
                Assert("myLong"== tbl.GetColumnName(0));
                Assert(1== tbl.GetColumnIndex("myMixed"));
                Assert(DataType.Double== tbl.ColumnType(2));

                Assert(Math.Abs(123.45 - tbl.MaximumDouble(2)) < 0.00001);
                Assert(24== tbl.SumLong(0));
                Assert(Math.Abs(6.0 - tbl.AverageLong(0)) < 0.00001);

                //simple match search
                Assert(1== tbl.FindFirstInt(0, -15)); //search for -15 in column0
                TableView view = tbl.FindAllInt(0, -15);
                Assert(2== view.Size);

                Query q = tbl.Where();
                Assert(2== q.Between(0, 0, 100).Count());

                //set index and get distinct values
                using (var tbl2 = new Table())
                {
                    long strColumn = tbl2.AddStringColumn( "new Strings");
                    tbl2.SetIndex(strColumn);
                    tbl2.Add("MyString");
                    tbl2.Add("MyString2");
                    tbl2.Add("MyString");
                    TableView view2 = tbl2.Distinct(strColumn);

                    Assert(2== view2.Size);
                }//using tbl2 ends here. tbl2 will be disposed here
                String json = tbl.ToJson();
                Console.Out.WriteLine("JSON: " + json);


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

            //you can also create subtables programmatically:
            var tbl3C = new Table();
            tbl3C.AddStringColumn("name");
            var subtablepath=tbl3C.AddSubTableColumn("subtable");
            {
                tbl3C.AddStringColumn(subtablepath, "key");
                tbl3C.AddMixedColumn(subtablepath, "value");
            }


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
            Assert(2== tbl3.GetSubTable(1, 0).Size);

            //add some rows to the empty subtable in the second row
            Table subTbl1 = tbl3.GetSubTable("subtable", 1);//or tbl3.GetSubTable(1,1);

            //now you can work with the subtable as any other table

            subTbl1.Add("key1", 23);
            Assert("key1"== subTbl1.GetString(0, 0));

            Console.WriteLine("press any key");
            Console.ReadKey();
        }
    }
}
