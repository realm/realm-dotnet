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

namespace IntegrationTests
{
    [TestFixture]
    public class IntegrationTests
    {
        private string _databasePath;
        private Realm _realm;

        [SetUp]
        public void Setup()
        {
            var coreProvider = new CoreProvider();
            Realm.ActiveCoreProvider = coreProvider;
            _databasePath = Path.GetTempFileName();
            _realm = Realm.GetInstance(_databasePath);
        }

        [TearDown]
        public void TearDown()
        {
            _realm.Dispose();
        }

        [Test]
        public void SimpleTest()
        {
            Person p1;
            Debug.WriteLine("A");
            using (var transaction = _realm.BeginWrite())
            {
                Debug.WriteLine("B");
                p1 = _realm.CreateObject<Person>();
                Debug.WriteLine("C");
                p1.FirstName = "John";
                p1.LastName = "Smith";
                p1.IsInteresting = true;
                p1.Email = "john@smith.com";
                transaction.Commit();
                Debug.WriteLine("D");
            }
            Debug.WriteLine("E");
            Debug.WriteLine("P1 access: " + p1.IsOnline);
            using (var rt = _realm.BeginRead())
            {
                Debug.WriteLine("p1 is named " + p1.FullName);
            }


            var p2 = _realm.CreateObject<Person>();
            p2.FullName = "John Doe";
            p2.IsInteresting = false;
            p2.Email = "john@deo.com";
            Debug.WriteLine("p2 is named " + p2.FullName);

            var p3 = _realm.CreateObject<Person>();
            p3.FullName = "Peter Jameson";
            p3.Email = "peter@jameson.com";
            p3.IsInteresting = true;
            Debug.WriteLine("p3 is named " + p3.FullName);

            var interestingPeople = from p in _realm.All<Person>() where p.IsInteresting == true select p;

            Debug.WriteLine("Interesting people include:");
            foreach (var p in interestingPeople)
                Debug.WriteLine(" - " + p.FullName + " (" + p.Email + ")");

            var johns = from p in _realm.All<Person>() where p.FirstName == "John" select p;
            Console.WriteLine("People named John:");
            foreach (var p in johns)
                Console.WriteLine(" - " + p.FullName + " (" + p.Email + ")");
        }

        [Test]
        public void TestSharedGroupWritesSomethingToDisk()
        {
            // Arrange
            Debug.WriteLine("File size before write: " + new FileInfo(_databasePath).Length);
            Debug.WriteLine(_databasePath);

            // Act
            using (var transaction = _realm.BeginWrite())
            {
                _realm.CreateObject<Person>();
                transaction.Commit();
            }

            // Assert
            Debug.WriteLine("File size after write: " + new FileInfo(_databasePath).Length);
            Assert.That(new FileInfo(_databasePath).Length, Is.GreaterThan(0));
        }
    }
}
