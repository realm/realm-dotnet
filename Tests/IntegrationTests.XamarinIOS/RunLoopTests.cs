using System;
using NUnit.Framework;
using Realms;
using System.IO;
using System.Threading;
using System.Linq;
using Foundation;

namespace IntegrationTests.XamarinIOS
{
    [TestFixture]
    public class RunLoopTests
    {
        protected string _databasePath;

        [SetUp]
        public void Setup()
        {
            _databasePath = Path.GetTempFileName();
        }

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
        public void QueriesShouldAutomaticallyRefreshInRunLoop()
        {
            Person p1, p2;

            var thread = new Thread(() =>
            {
                var r = Realm.GetInstance(_databasePath);
                r.Write(() =>
                {
                    p1 = r.CreateObject<Person>();
                    p1.FullName = "Person 1";
                });

                var q = r.All<Person>();
                Assert.That(q.Count, Is.EqualTo(1));

                WriteOnDifferentThread(newRealm =>
                {
                    p2 = newRealm.CreateObject<Person>();
                    p2.FullName = "Person 2";
                });

                // Instead of r.Refresh(), initiate the runloop which should trigger auto refresh
                NSRunLoop.Current.RunUntil((NSDate)DateTime.Now.AddMilliseconds(1));

                var ql2 = q.ToList();
                Assert.That(ql2.Select(p => p.FullName), Is.EquivalentTo(new[] { "Person 1", "Person 2" }));

                r.Close();
                Realm.DeleteRealm(r.Config);
            });

            thread.Start();
            thread.Join();
        }
    }
}

