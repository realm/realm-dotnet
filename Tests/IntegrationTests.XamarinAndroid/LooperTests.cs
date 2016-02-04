using System;
using NUnit.Framework;
using System.IO;
using System.Threading;
using Realms;
using System.Linq;
using Android.OS;

namespace IntegrationTests.XamarinAndroid
{
    [TestFixture]
    public class LooperTests
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
        public void AutoRefreshTest()
        {
            Person p1, p2;

            var thread = new Thread(() =>
            {
                Looper.Prepare();
                var handler = new Handler();
                var r = Realm.GetInstance(_databasePath);
                r.Write(() =>
                {
                    p1 = r.CreateObject<Person>();
                    p1.FullName = "Person 1";
                });

                var q = r.All<Person>();
                Assert.That(q.Count, Is.EqualTo(1));

                // Once the looper is started, we want to first update the database on a different thread.
                handler.PostDelayed(() => {
                    WriteOnDifferentThread((Realm newRealm) =>
                    {
                        p2 = newRealm.CreateObject<Person>();
                        p2.FullName = "Person 2";
                    });
                }, 1000);

                // And then stop the looper
                handler.PostDelayed(() => {
                    Looper.MyLooper().Quit();
                }, 2000);

                Looper.Loop();

                var ql2 = q.ToList();
                Assert.That(ql2.Select(p => p.FullName), Is.EquivalentTo(new string[] { "Person 1", "Person 2" }));

                r.Close();
                Realm.DeleteRealm(r.Config);
            });

            thread.Start();
            thread.Join();
        }

    }
}

