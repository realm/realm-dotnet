#if ENABLE_INTERNAL_NON_PCL_TESTS
using System;
using System.Linq;
using NUnit.Framework;
using Realms;
using System.Threading;
using System.IO;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace IntegrationTests.Shared
{
    [TestFixture]
    public class NotificationTests
    {
        private string _databasePath;
        private Realm _realm;

        [SetUp]
        public void Setup()
        {
            _databasePath = Path.GetTempFileName();
            _realm = Realm.GetInstance(_databasePath);
        }

        [TearDown]
        public void TearDown()
        {
            _realm.Close();
            Realm.DeleteRealm(_realm.Config);
        }

        [Test]
        public void ShouldTriggerRealmChangedEvent() 
        {
            // Arrange
            var wasNotified = false;
            _realm.RealmChanged += (sender, e) => { wasNotified = true; };

            // Act
            _realm.Write(() => _realm.CreateObject<Person>());

            // Assert
            Assert.That(wasNotified, "RealmChanged notification was not triggered");
        }


        [Test]
        public void ResultsShouldRaiseCollectionChangedReset()
        {
            var query = _realm.All<Person>();
            var list = new List<NotifyCollectionChangedAction>();
            (query as INotifyCollectionChanged).CollectionChanged += (o, e) => list.Add(e.Action);

            _realm.Write(() => _realm.CreateObject<Person>());

            TestHelpers.RunEventLoop(TimeSpan.FromMilliseconds(100));
            Assert.That(list, Is.EquivalentTo(new[] { NotifyCollectionChangedAction.Reset} ));
        }
    }
}

#endif  // #if ENABLE_INTERNAL_NON_PCL_TESTS
