using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using TightDbCSharp;
using TightDbCSharp.Extensions;

namespace TutorialSolution
{
    //@@Example:create_table @@
    internal static class Tutorial
    {
        private static void Main()
        {
            using (
                var table = new Table(
                    new StringField("name"),
                    new IntField("age"),
                    new BoolField("hired"),
                    new SubTableField("phones", //sub table specification
                        new StringField("desc"),
                        new StringField("number")
                        )
                    ))
            {
                //@@EndExample

                // @@Example: insert_rows @@
                table.Add("Mary", 21, false, new[]
                {
                    new[] {"mobile", "232-323-3232"},
                    new[] {"work"  , "434-434-4343"}
                });

                table.Add("Lars", 21, true, new[]
                {
                    new[] {"home"  , "343-436-5345"},
                    new[] {"school", "545-545-5454"}
                });

                table.Add("Phil", 43, false, new[]
                {
                    new[] {"mobile", "754-545-5433"}                    
                });

                table.Add("Anni", 54, true, null);
                // @@EndExample@@

                // @@Example: insert_at_index @@
                table.Insert(2, "Frank", 34, true,null);
                // @@EndExample@@

                // @@Example: number_of_rows @@
                Console.WriteLine(table.Size);//=>5
                Console.WriteLine(table.IsEmpty ? "Empty" : "Not Empty");//=>Not Empty
                // @@EndExample@@

        

                // @@Example: accessing_rows @@                
                //getting values.
                Console.WriteLine(table[4].GetString("name"));//=>Anni
                Console.WriteLine(table[4].GetLong("age"));//=>54
                Console.WriteLine(table[4].GetBoolean("hired"));//true
                var name = table.GetColumnIndex("name");//You can also look up via column indicies
                var age = table.GetColumnIndex("age");
                var hired = table.GetColumnIndex("hired");
                Console.WriteLine(table[4].GetString(name));//=>Anni
                Console.WriteLine(table[4].GetLong(age));//=>54
                Console.WriteLine(table[4].GetBoolean(hired));//true
                
                //changing values
                table[3].SetLong(age,43);
                table[3][1] = (long)table[3][1] + 1;//indexing into a column yields type object. With the typed interface this is easier table[3].age += 1;
                // @@EndExample@@

                // @@Example: last_row @@
                var lastperson = table.Last();//returns a Row Accessor
                Console.WriteLine(lastperson.GetString(name)); // =>Anni
                Console.WriteLine(lastperson.GetLong(age)); // =>54
                // @@EndExample@@

                // @@Example: updating_entire_row @@
                table[4].SetRow("Eric", 50, true,null);
                // @@EndExample@@

                
                // @@Example: deleting_row @@
                table.Remove(2);
                // @@EndExample@@

                Console.WriteLine("Removed row 2. Down to {0} rows.", table.Size);


             
                // @@Example: iteration @@
                foreach (var person in table)
                {                 
                    Console.WriteLine("{0} is {1} years old", person.GetString(name), person.GetLong(age));
                    foreach ( var phone in person.GetSubTable("phones"))
                    {
                        Console.WriteLine(" {0}: {1}",phone.GetString("desc"),phone.GetString("number"));
                    }
                }
                // @@EndExample@@



                /****************************** SIMPLE QUERY *****************************/
                
                // @@Example: simple_seach @@
                Console.WriteLine(table.FindFirstString(name,"Philip"));//-1 meaning not found
                Console.WriteLine(table.FindFirstString(name, "Mary"));// => 1
                Console.WriteLine(table.FindFirstInt(age, 21));// => 2
                // @@EndExample@@
                
                
                // @@Example: advanced_search @@                
                //Create query (current employees between 20 and 30 years old)

                var q = table.Where().Equal(hired,true).Between(age,20,30);
                // Get number of matching entries
                Console.WriteLine(q.Count());   // => 2

                // Get the average age
                Console.WriteLine(q.Average(age)); //=> 21

// Iterate over all matching rows (doing lazy searching)
                foreach (var person in table.Where().Greater(age,40))
                {
                    Console.WriteLine("{0} is {1} years old.",person.GetString(name),person.GetLong(age));
                }

// @@EndExample@@
                
                // @@Example: serialisation @@
                // Create Table in Group
                var fileNameSerialized  = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +@"\employees1.tightdb";
                var fileNameShared = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\employees2.tightdb";
                using (var group = new Group())
                {                    
                    var table2 = group.CreateTable("employees",
                        "Name".String(),         //or new StringField("Name"),
                        "Age".Int(),             //or new IntegerField("Age"),
                        "Hired".Bool()           //or new BooleanField("Hired")
                        );

                    //add some rows
                    table2.Add("John", 20, true);
                    table2.Add("Mary", 21, false);
                    table2.Add("Lars", 21, true);
                    table2.Add("Phil", 43, false);
                    table2.Add("Anni", 54, true);
                    
                    //a group file cannot be written to, if it exists already, so we delete it explicitly here

                    File.Delete(fileNameSerialized);

                    group.Write(fileNameSerialized);
                }

                //under construction
                
                 // Load a group from disk (and print contents)
                var fromdisk = new Group(fileNameSerialized);
                var diskTable = fromdisk.GetTable("employees");
                foreach (var row in diskTable)
                    Console.WriteLine("{0}:{1}", row.RowIndex, row.GetString(name));
                // @@EndExample@@

//Write same group to memory buffer
                var buffer = fromdisk.WriteToMemory();

//Load a group from memory (and print contents)
                var fromMem = new Group(buffer);
                var memtable = fromMem.GetTable("employees");
                foreach (var row in memtable )                
                    Console.WriteLine("{0}:{1}",row.RowIndex,row.GetString(name));
                
// @@Example: transaction @@

// Open a shared group
                var db = new SharedGroup(fileNameShared);

// Read transaction using an action delegate that takes a Group parameter
                db.ExecuteInReadTransaction(group =>
                {
                    using (var employees = group.GetTable("employees"))
                    {
                        foreach (var row in employees)
                        {
                            Console.WriteLine("{0} is {1} years old", row.GetString(name), row.GetLong(age));
                        }
                    }
                }
                    );
                
// alternative syntax:Read transaction using explicit commit (transaction inherits from group, adds commit and rollback methods)
                var transaction = db.BeginRead();
                {
                    using (var employees = transaction.GetTable("employees"))
                    {
                        foreach (var row in employees)
                        {
                            Console.WriteLine("{0} is {1} years old", row.GetString(name), row.GetLong(age));
                        }
                    }
                    transaction.Commit();//tell tightdb that You are finished reading using this transaction
                }                    

                    
                //write transaction

                db.ExecuteInWriteTransaction(group =>
                {
                    using (var table2 = group.GetTable("employees"))
                    {
                        table2.Add("Bill",53,true);//add a row
                    }                    
                });
                    
                //alternative write transaction

                using (var transaction2 = db.BeginWrite())
                {
                    using (var table3 = transaction2.GetTable("employees"))
                    {
                        table3.Add("Bill", 53, true);
                    }
                }

// @@EndExample@@

                Console.WriteLine("Finished. Any key to close console window");
                Console.ReadKey();//keep console window open to inspect results
            }
        }
    }
}
