using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interop.Providers;
using NUnit.Framework;
using RealmNet;

namespace IntegrationTests
{
    [TestFixture]
    public class IntegrationTests
    {
        [Test]
        public void SimpleTest()
        {
            var coreProvider = new CoreProvider();
            Realm.ActiveCoreProvider = coreProvider;
            var realm = Realm.GetInstance();

            var p1 = realm.CreateObject<Person>();
            p1.FirstName = "John";
            p1.LastName = "Smith";
            p1.IsInteresting = true;
            p1.Email = "john@smith.com";
            Debug.WriteLine("p1 is named " + p1.FullName);

            var p2 = realm.CreateObject<Person>();
            p2.FullName = "John Doe";
            p2.IsInteresting = false;
            p2.Email = "john@deo.com";
            Debug.WriteLine("p2 is named " + p2.FullName);

            var p3 = realm.CreateObject<Person>();
            p3.FullName = "Peter Jameson";
            p3.Email = "peter@jameson.com";
            p3.IsInteresting = true;
            Debug.WriteLine("p3 is named " + p3.FullName);

            var interestingPeople = from p in realm.All<Person>() where p.IsInteresting == true select p;

            Debug.WriteLine("Interesting people include:");
            foreach (var p in interestingPeople)
                Debug.WriteLine(" - " + p.FullName + " (" + p.Email + ")");

            var johns = from p in realm.All<Person>() where p.FirstName == "John" select p;
            Console.WriteLine("People named John:");
            foreach (var p in johns)
                Console.WriteLine(" - " + p.FullName + " (" + p.Email + ")");
        }
    }
}
