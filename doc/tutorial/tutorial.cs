using System;
using System.IO;
using TightDbCSharp;
using TightDbCSharp.Extensions;

namespace TutorialSolution
{
    internal static class Tutorial
    {
        private static void Main()
        {
            //@@Example: create_table @@
            using (var people = new Table(
                new StringField("name"),
                new IntField("age"),
                new BoolField("hired"),
                new SubTableField("phones", //sub table specification
                    new StringField("desc"),
                    new StringField("number")
                    )
                ))
                //@@EndExample@@

            {
                // @@Example: insert_rows @@

                people.Add("John", 20, true, new[]
                {
                    new[] {"home", "555-1234-555"}
                });

                people.Add("Mary", 21, false, new[]
                {
                    new[] {"mobile", "232-323-3232"},
                    new[] {"work", "434-434-4343"}
                });

                people.Add("Lars", 21, true, new[]
                {
                    new[] {"home", "343-436-5345"},
                    new[] {"school", "545-545-5454"}
                });

                people.Add("Phil", 43, false, new[]
                {
                    new[] {"mobile", "754-545-5433"}
                });

                people.Add("Anni", 54, true, null);
                // @@EndExample@@

                // @@Example: insert_at_index @@
                people.Insert(2, "Frank", 34, true, null);
                // @@EndExample@@

                // @@Example: number_of_rows @@
                Console.WriteLine(people.Size); //=>6
                Console.WriteLine(people.IsEmpty ? "Empty" : "Not Empty"); //=>Not Empty
                // @@EndExample@@



                // @@Example: accessing_rows @@                
                //getting values.                
                var nameColumn = people.GetColumnIndex("name");
                var ageColumn = people.GetColumnIndex("age");
                var hiredColumn = people.GetColumnIndex("hired");
                Console.WriteLine(people[4].GetString(nameColumn)); //=>Anni
                Console.WriteLine(people[4].GetLong(ageColumn)); //=>54
                Console.WriteLine(people[4].GetBoolean(hiredColumn)); //true

                //changing values
                people[3].SetLong(ageColumn, 43);
                // @@EndExample@@

                // @@Example: last_row @@
                var lastperson = people.Last(); //returns a Row Accessor
                Console.WriteLine(lastperson.GetString(nameColumn)); // =>Anni
                Console.WriteLine(lastperson.GetLong(ageColumn)); // =>54
                // @@EndExample@@

                // @@Example: updating_entire_row @@
                people[4].SetRow("Eric", 50, true, null);
                // @@EndExample@@


                // @@Example: deleting_row @@
                people.Remove(2);
                // @@EndExample@@

                Console.WriteLine("Removed row 2. Down to {0} rows.", people.Size);



                // @@Example: iteration @@
                //as Table is IEnummerable<TableRow>, C# foreach is supported.
                //TableRow can return object or a specific type, and the column
                //can be indexed by a numeric value, or by its string name
                foreach (var person in people)
                {
                    Console.WriteLine("{0} is {1} years old", person[nameColumn], person.GetLong(ageColumn));
                    foreach (var phone in person.GetSubTable("phones"))
                    {
                        Console.WriteLine(" {0}: {1}", phone["desc"], phone.GetString("number"));
                    }
                }
                // @@EndExample@@



                /****************************** SIMPLE QUERY *****************************/

                // @@Example: simple_seach @@
                Console.WriteLine(people.FindFirstString(nameColumn, "Philip")); //-1 meaning not found
                Console.WriteLine(people.FindFirstString(nameColumn, "Mary"));   // => 1
                Console.WriteLine(people.FindFirstInt(ageColumn, 21));           // => 2
                // @@EndExample@@


                // @@Example: advanced_search @@                
                // Create query (current employees between 20 and 30 years old)
                var q = people.Where().Equal(hiredColumn, true).Between(ageColumn, 20, 30);

                // Get number of matching entries
                Console.WriteLine(q.Count()); // => 2

                // Get the average age
                Console.WriteLine(q.Average(ageColumn)); //=> 21

                // Iterate over all matching rows (doing lazy searching)
                foreach (var person in people.Where().Greater(ageColumn, 40))
                {
                    Console.WriteLine("{0} is {1} years old.",
                        person[nameColumn], person[ageColumn]);
                }

                // @@EndExample@@
                File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                            @"\employees1.tightdb");

                // @@Example: serialisation @@
                // Create Table in Group
                var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var fileName1 = folder + @"\employees1.tightdb";
                var fileName2 = folder + @"\employees2.tightdb";
                using (var group = new Group())
                {
                    var employees = group.CreateTable("employees",
                        "Name".String(), //or new StringField("Name"),
                        "Age".Int(), //or new IntegerField("Age"),
                        "Hired".Bool() //or new BooleanField("Hired")
                        );

                    //add some rows
                    employees.Add("John", 20, true);
                    employees.Add("Mary", 21, false);
                    employees.Add("Lars", 21, true);
                    employees.Add("Phil", 43, false);
                    employees.Add("Anni", 54, true);

                    group.Write(fileName1);
                }

                // Load a group from disk (and print contents)
                var fromdisk = new Group(fileName1);
                var employees2 = fromdisk.GetTable("employees");
                foreach (var row in employees2)
                    Console.WriteLine("{0}:{1}", row.RowIndex, row.GetString(nameColumn));
                // @@EndExample@@

                //Write same group to memory buffer
                var buffer = fromdisk.WriteToMemory();

                //Load a group from memory (and print contents)
                var fromMem = new Group(buffer);
                var memtable = fromMem.GetTable("employees");
                foreach (var row in memtable)
                    Console.WriteLine("{0}:{1}", row.RowIndex, row.GetString(nameColumn));

                // @@Example: transaction @@
                // Open a shared group
                var db = new SharedGroup(fileName2);

                // Read transaction using an action delegate that takes a Group parameter
                db.ExecuteInReadTransaction(group =>
                {
                    using (var employees = group.GetTable("employees"))
                    {
                        foreach (var employee in employees)
                        {
                            Console.WriteLine("{0} is {1} years old",
                                employee[nameColumn], employee[ageColumn]);
                        }
                    }
                }
                    );

                // alternative syntax:Read transaction using explicit commit 
                //(transaction inherits from group, adds commit and rollback methods)
                var transaction = db.BeginRead();
                {
                    using (var employees = transaction.GetTable("employees"))
                    {
                        foreach (var employee in employees)
                        {
                            Console.WriteLine("{0} is {1} years old", employee[nameColumn], employee[ageColumn]);
                        }
                    }
                    //tell tightdb that You are finished reading using this transaction
                    transaction.Commit();
                }

                //write transaction
                db.ExecuteInWriteTransaction(group =>
                {
                    using (var employees3 = group.GetTable("employees"))
                    {
                        employees3.Add("Bill", 53, true); //add a row
                    }
                });

                //alternative write transaction
                using (var transaction2 = db.BeginWrite())
                {
                    using (var employees4 = transaction2.GetTable("employees"))
                    {
                        employees4.Add("Bill", 53, true);
                    }
                }

                // @@EndExample@@
                Console.WriteLine("Finished. Any key to close console window");
                Console.ReadKey(); //keep console window open to inspect results
            }
        }
    }
}

