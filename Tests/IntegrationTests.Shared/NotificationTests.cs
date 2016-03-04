using System;
using NUnit.Framework;
using Realms;
using System.Threading;

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

        [Test]
        public void SimpleTest() 
        {
            // Arrange


            // Act

        }
    }
}

