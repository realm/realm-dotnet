using System;
using System.IO;
using TightDbCSharp;

namespace TutorialSolution
{
    internal static class Tutorial
    {
        private static void Main()
        {
            //@@Example: create_table @@
            using (var people = new Table(
                new StringColumn("name"),
                new IntColumn("age"),
                new BoolColumn("hired"),
                new SubTableColumn("phones", //sub table specification
                    new StringColumn("desc"),
                    new StringColumn("number"))))
                //@@EndExample@@

            {
                // @@Example: insert_rows @@

                people.Add("John", 20, true,  new[]{new[] {"home",   "555-1234-555"}});
                people.Add("Mary", 21, false, new[]{new[] {"mobile", "232-323-3232"},
                                                    new[] {"work",   "434-434-4343"}});
                people.Add("Lars", 21, true,  new[]{new[] {"home",   "343-436-5345"},
                                                    new[] {"school", "545-545-5454"}});
                people.Add("Phil", 43, false, new[]{new[] {"mobile", "754-545-5433"}});
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
                var name = people.GetColumnIndex("name");
                var age = people.GetColumnIndex("age");
                var hired = people.GetColumnIndex("hired");
                Console.WriteLine(people[4].GetString(name)); //=>Anni
                Console.WriteLine(people[4].GetLong(age)); //=>54
                Console.WriteLine(people[4].GetBoolean(hired)); //true

                //changing values
                people[3].SetLong(age, 43);
                // @@EndExample@@

                // @@Example: last_row @@
                var lastPerson = people.Last(); //returns a Row Accessor
                Console.WriteLine(lastPerson.GetString(name)); // =>Anni
                Console.WriteLine(lastPerson.GetLong(age)); // =>54
                // @@EndExample@@

                // @@Example: updating_entire_row @@
                people[4].SetRow("Eric", 50, true, null);
                // @@EndExample@@


                // @@Example: deleting_row @@
                people.Remove(2);
                // @@EndExample@@

                Console.WriteLine("Removed row 2. Down to {0} rows.", people.Size);



                // @@Example: iteration @@
                const int desc = 0;
                const int number = 1;
                foreach (var person in people)
                {
                    Console.WriteLine("{0} is {1} years old", person[name], person[age]);
                    foreach (var phone in person.GetSubTable("phones"))
                    {
                        Console.WriteLine(" {0}: {1}", phone[desc], phone[number]);
                    }
                }
                // @@EndExample@@



                /****************************** SIMPLE QUERY *****************************/

                // @@Example: simple_seach @@
                Console.WriteLine(people.FindFirstString(name, "Philip")); //-1 meaning not found
                Console.WriteLine(people.FindFirstString(name, "Mary")); // => 1
                Console.WriteLine(people.FindFirstInt(age, 21)); // => 2
                // @@EndExample@@


                // @@Example: advanced_search @@                
                // Create query (current employees between 20 and 30 years old)
                var q = people.Where().Equal(hired, true).Between(age, 20, 30);

                // Get number of matching entries
                Console.WriteLine(q.Count()); // => 2

                // Get the average age
                Console.WriteLine(q.Average(age)); //=> 21

                // Iterate over all matching rows (doing lazy searching)
                foreach (var person in people.Where().Greater(age, 40))
                {
                    Console.WriteLine("{0} is {1} years old.", person[name], person[age]);
                }

                // @@EndExample@@
                File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                            @"\employees1.tightdb");

                // @@Example: serialisation @@
                // Create Table in Group
                var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var fileName1 = folder + @"\employees1.tightdb";

                using (var group = new Group())
                using (var employees = group.CreateTable("employees",
                    new StringColumn("Name"),
                    new IntColumn("Age"),
                    new BoolColumn("Hired")))
                {

                    //add some rows
                    employees.Add("John", 20, true);
                    employees.Add("Mary", 21, false);
                    employees.Add("Lars", 21, true);
                    employees.Add("Phil", 43, false);
                    employees.Add("Anni", 54, true);

                    group.Write(fileName1);
                }

                // Load a group from disk (and print contents)
                var fromdisk = new Group(fileName1,Group.OpenMode.ModeReadWrite);
                using (var employees2 = fromdisk.GetTable("employees"))
                {
                    foreach (var row in employees2)
                        Console.WriteLine("{0}:{1}", row.RowIndex, row.GetString(name));
                }

                //Write same group to memory buffer
                byte[] buffer;
                buffer = fromdisk.WriteToMemory();

                //Load a group from memory (and print contents)
                var fromMem = new Group(buffer);
                using (var memtable = fromMem.GetTable("employees"))
                {
                    foreach (var row in memtable)
                        Console.WriteLine("{0}:{1}", row.RowIndex, row[name]);
                }
                // @@EndExample@@

                // @@Example: transaction @@
                // Open a shared group
                var db = new SharedGroup(fileName1);

                //Transaction inherits from group, adds commit and rollback methods
                //Commit must be called to actually save changes
                //Rollback is automatically called if the transaction has not been
                //comitted and it goes out scope (gets disposed)
                using (var transaction = db.BeginRead())
                using (var employees = transaction.GetTable("employees"))
                {
                    foreach (var employee in employees)
                    {
                        Console.WriteLine("{0} is {1} years old", employee[name], employee[age]);
                    }
                    transaction.Commit();
                }

                //write transaction
                using (var transaction2 = db.BeginWrite())
                using (var employees4 = transaction2.GetTable("employees"))
                {
                    {
                        employees4.Add("Bill", 53, true);
                    }
                    transaction2.Commit();
                }

                //TightDb also provides a delegate based transaction syntax
                //After the delegate has executed, commit is called automatically
                //to roll back, throw an exception inside the delegate
                db.ExecuteInWriteTransaction(group =>
                {
                    using (var employees3 = group.GetTable("employees"))
                    {
                        employees3.Add("Bill", 53, true); //add a row
                    }
                });

                // @@EndExample@@
                Console.WriteLine("Finished. Any key to close console window");
                Console.ReadKey(); //keep console window open to inspect results
            }
        }
    }
}

