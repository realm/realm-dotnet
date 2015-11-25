using NUnit.Framework;
using RealmNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IntegrationTests.Shared
{
    [TestFixture]
    public class StandAloneObjectTests
    {
        private Person _person;

        [SetUp]
        public void SetUp()
        {
            _person = new Person();
        }

        [Test]
        public void PropertyGet()
        {
            string firstName = null;
            Assert.DoesNotThrow(() => firstName = _person.FirstName);
            Assert.IsNullOrEmpty(firstName);
        }

        [Test]
        public void PropertySet()
        {
            const string name = "John";
            Assert.DoesNotThrow(() => _person.FirstName = name);
            Assert.AreEqual(name, _person.FirstName);
        }

        [Test]
        public void AddToRealm()
        {
            _person.FirstName = "Arthur";
            _person.LastName = "Dent";
            _person.IsInteresting = true;

            using (var realm = Realm.GetInstance(Path.GetTempFileName()))
            {
                using (var transaction = realm.BeginWrite())
                {
                    realm.Attach(_person);
                    transaction.Commit();
                }

                Assert.That(_person.IsManaged);

                var p = realm.All<Person>().ToList().Single();
                Assert.That(p.FirstName, Is.EqualTo("Arthur"));
                Assert.That(p.LastName, Is.EqualTo("Dent"));
                Assert.That(p.IsInteresting);
            }
        }
    }
}
