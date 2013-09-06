using System;
using System.IO;
using TightDbCSharp;
using TightDbCSharp.Extensions;

namespace Experimental
{
    class Program
    {
        private static void Experimental()
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
                    var peopleTableAlt = new Table(
                        "name".String(),
                        "age".Int(),
                        "hired".Date(),
                        "phones".Table(
                            "desc".String(),
                            "number".String())
                        )
                    )
                {

                    //a row with subtable contents can be added recursively like this :

                    //the null results in an empty subtable with no rows
                    long rowIndex = peopleTableAlt.Add("John", 20, new DateTime(1979, 05, 14), null);
                    //call the subtable to insert rows into it
                    using (Table rowSub2 = peopleTableAlt.GetSubTable(3, rowIndex))
                    {
                        rowSub2.Add("mobile", "232-323-3232");
                        rowSub2.Add("work", "434-434-4343");
                    }
                }



                //You can also add data to a table field by field:
                long rowindex2 = peopleTable.AddEmptyRow(1);
                peopleTable.SetString(0, rowindex2, "John");
                peopleTable.SetLong(1, rowindex2, 20);
                peopleTable.SetBoolean(2, rowindex2, true);
                var subtable = peopleTable.GetSubTable(3, rowindex2); //return a subtable for column 3
                long firstRowIdAdded = subtable.AddEmptyRow(2);
                subtable.SetString(0, firstRowIdAdded, "mobile");
                subtable.SetString(1, firstRowIdAdded, "232-323-3232");
                subtable.SetString(0, 1 + firstRowIdAdded, "work");
                subtable.SetString(1, 1 + firstRowIdAdded, "434-434-4343");

                //subtables can be specified inside the parameter list (as an array of arrays of object) like this
                //Support for iterator of iterators of objects is under way to allow most kinds of containers to be put into a table,
                //as long as the individual objects can be translated to the types of the subtable
                peopleTable.Add("John", 20, true,
                    new[]
                    {
                        new[] {"work", "232-323-3232"},
                        new[] {"home", "434-434-4343"}
                    });

                //the arrays and constans can of course be supplied as variables too like this
                var subAsArray =
                    new[]
                    {
                        new[] {"work", "232-323-3232"},
                        new[] {"home", "434-434-4343"}
                    };

                peopleTable.Add("John", 20, true, subAsArray);

                //You can also specify an already built table. In that case the pre-built table is COPIED into the row where the sub table is.
                //types and field names must match, currently no match means undefined behavior so use with care todo:validate subtables when being added to table rows if specified as a Table class

                using (var subAsTable = new Table(new StringField("desc"),
                    new StringField("number"))
                {
                    {"work", "232-323-3232"},
                    {"home", "434-434-4343"},
                    {"boat", "555-666-7777"}
                }
                    )
                    peopleTable.Add("Jane", 25, false, subAsTable);
                    //subAsTable can go out of scope here and be disposed as a copy is made when the table is "put" into a subtable column 

            

            //or combine these two approaches and simply specify the subtable directly, not as an array, but as a subtable, created
                //and populated on the fly. This might be especially useful with subtables inside mixed columns

                peopleTable.Add("Jane", 25, false, 
                    new Table(new StringField("desc"),
                              new StringField("number"))
                {
                    {"work", "232-323-3232"},
                    {"home", "434-434-4343"},
                    {"boat", "555-666-7777"}
                });

            



                var rows = peopleTable.Size; //get the number of rows in a table
                var isEmpty = peopleTable.IsEmpty; //is the table empty?

                Console.WriteLine("PeopleTable has {0} rows and isEmpty returns {1}", rows, isEmpty);

                //working with individual rows

                //getting values (untyped)
                String n = peopleTable[4].GetString(0);
                //You can specify the name of the column. It will be looked up at runtime so not so fast as the above
                long a = peopleTable[4].GetLong("age"); //returns the value of the long field called Age so prints 20
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
                peopleTable[4].SetString("name", "John");
                //first parameter is the column name, the second parameter is the value
                peopleTable[4].SetString(0, "John"); //columns can be indexed by number too
                peopleTable[4].SetLong("age", 20);
                peopleTable[4].SetBoolean("hired", true);

                //You can also set an entire row by specifying the correct types in order
                //re-using the subtable sub created earlier, see above
                peopleTable[4].SetRow("John", 20, true, subAsArray);
                //this method is not as fast as using SetString etc. as it will have to inspect the parametres and the table to validate type compatibillity

                //You can delete a row by calling remove(rowIndex)
                peopleTable.Remove(3);
                //removes the 4th row in the table and moves every row index larger than 3 one down
                peopleTable[3].Remove(); //this does the same

                Row r = peopleTable[2]; //another way still
                r.Remove();
                //after having removed r, The row object becomes invalid and cannot be used anymore for any functions that involves row numbers

                //we better put in some data now we deleted a lot of rows
                peopleTable.Add("Johanna", 20, true, subAsArray);

                peopleTable.Add("Rasmus", 23, true,
                    new[]
                    {
                        new[] {"work", "434-424-4242"},
                        new[] {"home", "555-444-3333"}
                    });

                peopleTable.Add("Per", 53, true,
                    new[]
                    {
                        new[] {"work", "314-159-2653"},
                        new[] {"home", "589-793-2385"}
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


                TableView tv3 = qr.FindAll(0, -1, 50);
                //find all hired people , but return only 50 at most (-1 means go find everything)
                TableView tv4 = qr.FindAll(); //creates tableview with all matching records

                Console.WriteLine("findAll limited to 50 returned {0} rows", tv3.Size);
                Console.WriteLine("findAll returned {0} rows", tv4.Size);

                //serialization 

                using (var myGroup = new Group())
                {
                    //Group returns a new table. The structure is specified as in Table.CreateTable
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

                    myTable2.AddEmptyRow(1); //add one empty row
                    myOtherTable.AddEmptyRow(1);

                    //write to disk

                    //note that in C# just specifying a filename without a path is bad. You might not know for sure, what directory the file will end up in
                    //for instance when running in a unit test , the file might end up in some unit test application directory
                    //so please specify a fully qualified file name
                    //the binding will throw an exception if the file cannot be written to
                    string fileName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                      @"\employees.tightdb";
                    try
                    {
                        myGroup.Write(fileName);
                    }
                    catch (IOException groupWriteException)
                    {
                        Console.WriteLine(
                            "writing to file {0} failed, probably because the file already exists or because it is located somewhere with no write access. exception thrown:{1}",
                            fileName, groupWriteException.Message);
                    }
                }


                //also show a few LinQ examples

              

            }
        }

        static void Main(string[] args)
        {
            Experimental();
        }
    }
}
