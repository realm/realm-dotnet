/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Realms;

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
                p3.FirstName = null;
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

        static void Main(string[] args)
        {
            SimpleTest();
            //CheckTablesTest();
        }
    }
}
