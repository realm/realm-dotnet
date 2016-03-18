using NUnit.Framework;
using Realms;
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
            Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);
        }

        [Test]
        public void PropertyGet()
        {
            string firstName = null;
            Assert.DoesNotThrow(() => firstName = _person.FirstName);
            Assert.That(string.IsNullOrEmpty(firstName));
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

            using (var realm = Realm.GetInstance())
            {
                using (var transaction = realm.BeginWrite())
                {
                    realm.Manage(_person);
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
