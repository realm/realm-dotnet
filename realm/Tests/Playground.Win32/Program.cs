using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RealmNet;

namespace Playground.Win32
{
    class Program
    {
        public static void SimpleTest()
        {
            var path = Path.GetTempFileName();
            var realm = Realm.GetInstance(path);

            Person p1, p2, p3;
            using (var transaction = realm.BeginWrite())
            {
                p1 = realm.CreateObject<Person>();
                p1.FirstName = "John";
                p1.LastName = "Smith";
                p1.IsInteresting = true;
                p1.Email = "john@smith.com";
                transaction.Commit();
            }
            Console.WriteLine("p1 is named " + p1.FullName);

            using (var transaction = realm.BeginWrite())
            {
                p2 = realm.CreateObject<Person>();
                p2.FullName = "John Doe";
                p2.IsInteresting = false;
                p2.Email = "john@deo.com";
                transaction.Commit();
            }
            Console.WriteLine("p2 is named " + p2.FullName);

            using (var transaction = realm.BeginWrite())
            {
                p3 = realm.CreateObject<Person>();
                p3.FullName = "Peter Jameson";
                p3.Email = "peter@jameson.com";
                p3.IsInteresting = true;
                transaction.Commit();
            }

            Console.WriteLine("p3 is named " + p3.FullName);

            var allPeople = realm.All<Person>().ToList();
            Console.WriteLine("There are " + allPeople.Count() + " in total");

            var interestingPeople = from p in realm.All<Person>() where p.IsInteresting == true select p;

            Console.WriteLine("Interesting people include:");
            foreach (var p in interestingPeople)
                Console.WriteLine(" - " + p.FullName + " (" + p.Email + ")");

            var johns = from p in realm.All<Person>() where p.FirstName == "John" select p;
            Console.WriteLine("People named John:");
            foreach (var p in johns)
                Console.WriteLine(" - " + p.FullName + " (" + p.Email + ")");
        }

        private static void CheckTablesTest()
        {
            var path = Path.GetTempFileName();
            Console.WriteLine("Path: " + path);


            var osses = new SchemaInitializerHandle();

            var os1 = new ObjectSchemaHandle("Person");
            NativeSchema.initializer_add_object_schema(osses, os1);
            var os2 = new ObjectSchemaHandle("number 2");
            NativeSchema.initializer_add_object_schema(osses, os2);

            var sh = new SchemaHandle(osses);

            var sr = NativeSharedRealm.open(sh, path, (IntPtr)0, (IntPtr)0, "");
            var srh = new SharedRealmHandle();
            srh.SetHandle(sr);

            Console.WriteLine("Has table 'no': " + NativeSharedRealm.has_table(srh, "no"));
            Console.WriteLine("Has table 'class_Person': " + NativeSharedRealm.has_table(srh, "class_Person"));
        }

        static void Main(string[] args)
        {
            SimpleTest();
            //CheckTablesTest();
        }
    }
}
