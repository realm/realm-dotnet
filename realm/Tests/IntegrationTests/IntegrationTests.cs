using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using RealmNet;

namespace IntegrationTests
{
    [TestFixture]
    public abstract class AbstractIntegrationTests
    {
        private string _databasePath;
        protected Realm _realm;

        [SetUp]
        public void BaseSetup()
        {
            Setup();
            _databasePath = GetTempDatabasePath();
            _realm = Realm.GetInstance(_databasePath);
        }

        protected virtual void Setup () { }
        protected abstract string GetTempDatabasePath();

        [TearDown]
        public void TearDown()
        {
            _realm.Dispose();
        }

        [Test]
        public void SimpleTest()
        {
            Person p1, p2, p3;
            using (var transaction = _realm.BeginWrite())
            {
                p1 = _realm.CreateObject<Person>();
                p1.FirstName = "John";
                p1.LastName = "Smith";
                p1.IsInteresting = true;
                p1.Email = "john@smith.com";
                transaction.Commit();
            }
            using (var rt = _realm.BeginRead())
            {
                Debug.WriteLine("p1 is named " + p1.FullName);
            }

            using (var transaction = _realm.BeginWrite())
            {
                p2 = _realm.CreateObject<Person>();
                p2.FullName = "John Doe";
                p2.IsInteresting = false;
                p2.Email = "john@deo.com";
                transaction.Commit();
            }
            using (var rt = _realm.BeginRead())
            {
                Debug.WriteLine("p2 is named " + p2.FullName);
            }

            using (var transaction = _realm.BeginWrite())
            {
                p3 = _realm.CreateObject<Person>();
                p3.FullName = "Peter Jameson";
                p3.Email = "peter@jameson.com";
                p3.IsInteresting = true;
                transaction.Commit();
            }

            using (var rt = _realm.BeginRead())
            {
                Debug.WriteLine("p3 is named " + p3.FullName);

                var allPeople = _realm.All<Person>().ToList();
                Debug.WriteLine("There are " + allPeople.Count() + " in total");

                var interestingPeople = from p in _realm.All<Person>() where p.IsInteresting == true select p;

                Debug.WriteLine("Interesting people include:");
                foreach (var p in interestingPeople)
                    Debug.WriteLine(" - " + p.FullName + " (" + p.Email + ")");

                var johns = from p in _realm.All<Person>() where p.FirstName == "John" select p;
                Debug.WriteLine("People named John:");
                foreach (var p in johns)
                    Debug.WriteLine(" - " + p.FullName + " (" + p.Email + ")");
            }
        }

        //[Test]
        //public void TestSharedGroupWritesSomethingToDisk()
        //{
        //    // Arrange
        //    Debug.WriteLine("File size before write: " + new FileInfo(_databasePath).Length);
        //    Debug.WriteLine(_databasePath);

        //    // Act
        //    using (var transaction = _realm.BeginWrite())
        //    {
        //        _realm.CreateObject<Person>();
        //        transaction.Commit();
        //    }

        //    // Assert
        //    Debug.WriteLine("File size after write: " + new FileInfo(_databasePath).Length);
        //    Assert.That(new FileInfo(_databasePath).Length, Is.GreaterThan(0));
        //}

        [Test]
        public void RemoveTest()
        {
            // Arrange
            Person p1, p2, p3;
            using (_realm.BeginWrite())
            {
                p1 = new Person { FirstName = "A" };
                p2 = new Person { FirstName = "B" };
                p3 = new Person { FirstName = "C" };
            }

            // Act
            using (_realm.BeginWrite())
                _realm.Remove(p2);

            // Assert
            using (_realm.BeginRead())
            {
                Assert.That(!p2.InRealm);

                var allPeople = _realm.All<Person>().ToList();
                foreach(var p in allPeople)
                    Debug.WriteLine("Person: " + p.FirstName);

                Assert.That(allPeople, Is.EquivalentTo(new List<Person> { p1, p3 }));
            }
        }
    }
}
