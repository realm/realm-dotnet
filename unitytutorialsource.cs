using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Reflection;
using TightDbCSharp;
//using NUnitLite.Runner;

public static class Console {
	private static StringBuilder sb=new StringBuilder();
	public static void WriteLine(object o) {
		string str = o.ToString();
		sb.AppendLine(str);

	}
	
	public static void WriteLine(string format,params object[] variables) {
		string str = String.Format(format+Environment.NewLine,variables);
		sb.AppendLine(str);

	}
	
	public static string Dump() {
		return sb.ToString();
	}
	public static void ReadKey() {}
}

public class UnityTutorial : MonoBehaviour {

	int iteration =0;
	// Use this for initialization
	void Start () {
//		var x =new  UnityTestDll.Class1();
//		x.AddValues(2,3);
//		print ("2 + 3 ="+x.c);
        print ("Hello from the script - init");
//		print(x.HelloAgain());
		/*
		var info = new StringBuilder();
		print ("Calling CsInfo");
		Table.GetCsInfo(info);
		print ("Data from CsInfo");
		print (info.ToString());
		print ("Calling CppInfo");
		Table.GetCppInfo(info);
		print (info.ToString());
		Table t = new Table(new StringField("Test"));
		t.Add("jørgen");
		t.Add("peter");
		print(t.Size);*/
	}
	
	StringBuilder sb = new StringBuilder();
	  public Vector2 scrollPosition;
      public string longString = "Data Goes Here";
	
	
	void OnGUI () {
		// Make a background box
		GUI.Box(new Rect(910,10,200,150), "TightDb Menu V1.0");
		
		scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(800), 
			GUILayout.Height(800));

        GUILayout.Label(longString);
		
		  GUILayout.EndScrollView();
		// Make the first button. If it is pressed, Application.Loadlevel (1) will be executed
		if(GUI.Button(new Rect(920,40,180,20), "C# System Info")) {
		print("C# Info Clicked");
		var info = new StringBuilder();			
		Table.GetCsInfo(info);
		print ("Data from CsInfo");
		print (info.ToString()); 
        longString=info.ToString();
		}

		// Make the second button.
		if(GUI.Button(new Rect(920,70,180,20), "CPP System Info")) {
		var info = new StringBuilder();
		print("CPP Info Clicked");
		print ("Calling CppInfo");
		Table.GetCppInfo(info);
		print (info.ToString());
//        GUI.Box(new Rect(220,170,400,500),info.ToString());
        sb.Append(info);
        longString=longString+info.ToString();
		}
		
		if(GUI.Button(new Rect(920,100,180,20), "Run Tutorial")) {
		var info = new StringBuilder();		
		info.AppendLine(Assembly.GetCallingAssembly().FullName);
		
		Tutorial();
		longString= (Console.Dump());
//	    new TextUI().Execute
//(new string[]{Assembly.GetCallingAssembly().FullName, "-full", "-out:C:\\Files\\UnitTestDump.txt"});
//		info.Append(System.IO.File.ReadAllText("C:\\Files\\UnitTestDump.txt"));
		//	longString=info.ToString();
		}
	}
	
	
	        private static void Tutorial()
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
                    new StringField("Name"),
                    new IntField("Age"),
                    new BoolField("Hired")
                    )
                    )
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
                var fromdisk = new Group(fileName1);
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

	
	// Update is called once per frame
	void Update () {
		iteration++;
		if (iteration%10000==0) {
		   print ("Hello from the script - update :"+iteration);
		   System.Console.WriteLine("Hello from writeline :"+iteration);
     		//print (UnityTestDll.Class1.GenerateRandom(0,100));	
		}
	}
}
