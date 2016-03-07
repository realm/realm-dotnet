using System;
using NUnit.Framework;
using Realms;
using System.Threading;
using System.IO;

namespace IntegrationTests.Shared
{
    [TestFixture]
    public class NotificationTests
    {
        private string _databasePath;
        private Realm _realm;

        private void WriteOnDifferentThread(Action<Realm> action)
        {
            var thread = new Thread(() =>
            {
                var r = Realm.GetInstance(_databasePath);
                r.Write(() => action(r));
                r.Close();
            });
            thread.Start();
            thread.Join();
        }

        [SetUp]
        private void Setup()
        {
            _databasePath = Path.GetTempFileName();
            _realm = Realm.GetInstance(_databasePath);
        }

        [Test]
        public void ShouldTriggerRealmChangedEvent() 
        {
            // Arrange
            var wasNotified = false;
            _realm.RealmChanged += (sender, e) => { wasNotified = true; };

            // Act
            WriteOnDifferentThread((realm) =>
            {
                realm.CreateObject<Person>();
            });

            // Assert
            Assert.That(wasNotified, "RealmChanged notification was not triggered");
        }
    }
}

