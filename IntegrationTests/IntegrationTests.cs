using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RealmIO;

namespace IntegrationTests
{
    [TestFixture]
    public class IntegrationTests
    {
        [Test]
        public void SimpleTest()
        {
            var realm = new Realm(new CoreProvider());

            var p1 = realm.CreateObject<Person>();
            p1.FirstName = "John";
            p1.LastName = "Smith";
            p1.IsInteresting = true;
            Debug.WriteLine("p1 is named " + p1.FullName);

            var p2 = realm.CreateObject<Person>();
            p2.FullName = "John Doe";
            p2.IsInteresting = false;
            Debug.WriteLine("p2 is named " + p2.FullName);

            var p3 = realm.CreateObject<Person>();
            p3.FullName = "Peter Jameson";
            p3.IsInteresting = true;
            Debug.WriteLine("p3 is named " + p3.FullName);

            Debug.WriteLine("Interesting people include:");
            var peopleNamedJohn = realm.All<Person>().Where(p => p.IsInteresting == true);
            foreach (var p in peopleNamedJohn)
                Debug.WriteLine(" - " + p.FullName);
        }
    }
}
