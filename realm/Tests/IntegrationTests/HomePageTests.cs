using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RealmNet.Interop;
using NUnit.Framework;
using RealmNet;

// shows code like the samples on the realm.io home page
namespace HomePageTests
{

    // Realm Objects Look like Regular Objects…
    public class Dog : RealmObject
    {
        public string name { get; set; }
        public int age { get; set; }
    }

    [TestFixture]
    public class HomePageTests
    {
        [Test]
        public void CreateObjectAndPersist()
        {
            var mydog = new Dog() { name = "Rex" };
            Console.WriteLine($"name of dog:{mydog.name}");

            // Offer Easy Persistence…
            var realm = Realm.GetInstance();
            using (var writer = realm.BeginWrite()) {
                realm.Add( mydog );
            }

            // Can be Queried… with standard LINQ
            var r = realm.All<Dog>().Where(dog => dog.age > 8);

            // Queries are chainable
            var r2 = r.Where(dog => dog.name.Contains("rex"));

        }

    } // HomePageTests
}
