using System;
using System.IO;
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
                var name = table.GetColumnIndex("name"); //it is more efficient to index by the column ID
                var age = table.GetColumnIndex("age");
                Console.WriteLine(table[4].GetString(name));//=>Anni
                Console.WriteLine(table[4][name]);//=>Anni (returns an object when accessed this way)
                Console.WriteLine(table.GetString(name,4));//=>Anni    
                Console.WriteLine(table[4].GetLong(age));//=>54
                Console.WriteLine(table[4].GetBoolean("hired"));//true              

                //changing values
                table[3].SetLong(age,43);
                table[3][1] = (long)table[3][1] + 1;//indexing into a column yields type object. With the typed interface this is easier table[3].age += 1;
                // @@EndExample@@

                // @@Example: last_row @@
                var lastperson = table.Last();
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

                var q = table.Where().Equal("hired",true).Between(age,20,30);                
                // Get number of matching entries
                Console.WriteLine(q.Count());   // => 2

                // Get the average age
                Console.WriteLine(q.Average(age)); //=> 21

// Iterate over all matching rows (doing lazy searching)
                foreach (var person in table.Where().Greater("age",40))
                {
                    Console.WriteLine("{0} is {1} years old.",person.GetString(name),person.GetLong(age));
                }

// @@EndExample@@
                
                // @@Example: serialisation @@
                // Create Table in Group
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
                    String fileName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                               @"\employees2.tightdb";

                    File.Delete(fileName);

                    group.Write(fileName);
                }

                //under construction
                /*
                 * # Load a group from disk (and print contents)
fromDisk = tightdb.Group("file", "employees.tightdb")
diskTable = fromDisk["employees"]
for index, row in enumerate(diskTable):
    print str(index) + ": " + row.name
# @@EndExample@@

# Write same group to memory buffer
buf = group.write_to_memory()

# Load a group from memory (and print contents)
fromMem = tightdb.Group("memory", buf)
memTable = fromMem["employees"]
for index, row in enumerate(memTable):
    print str(index) + ": " + row.name

# @@Example: transaction @@

# Open a shared group
db = tightdb.SharedGroup("employees.tightdb")

# Read transaction
with db.read("employees") as table:
    # print table contents
    for row in table:
        print row.name + " is " + str(row.age) + " years old."

# Write transaction
with db.write("employees") as table:
    table += ["Bill", 53, True] # add row

# @@EndExample@@

                 * 
                 */
                Console.WriteLine("Finished. Any key to close console window");
                Console.ReadKey();//keep console window open to inspect results
            }
        }
    }
}
