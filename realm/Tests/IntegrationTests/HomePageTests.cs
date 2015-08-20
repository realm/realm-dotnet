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
    public class Dog : RealmObject
    {
        public string name = "";
        public int age = 0;
    }

    [TestFixture]
    public class HomePageTests
    {
        [Test]
        public void CreateObjectAndPersist()
        {
            var mydog = new Dog() { name = "Rex" };
            Console.WriteLine($"name of dog:{mydog.name}");

            var realm = new Realm();
            using (var writer = realm.BeginWrite()) {
                realm.Add( mydog );
            }
        }

        // standard test infrastructure
        [SetUp]
        public void Setup()
        {
            Realm.ActiveCoreProvider = new CoreProvider();
            Realm.DefaultPathProvider = () => Path.GetTempFileName();

        }

        } // HomePageTests
}
