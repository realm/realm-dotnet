/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */

using System;
using NUnit.Framework;
using Realms;
using System.IO;
using System.Linq;
using System.Threading;

namespace IntegrationTests.Shared
{
    [TestFixture]
    public class ConcurrencyTests
    {
        protected string _databasePath;
        protected Realm _realm;

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
        public void SimpleTest()
        {
            Person p1, p2, p3;

            using (var t = _realm.BeginWrite())
            {
                p1 = _realm.CreateObject<Person>();
                p1.FullName = "Person 1";

                p2 = _realm.CreateObject<Person>();
                p2.FullName = "Person 2";
                t.Commit();
            }

            var q = _realm.All<Person>();
            Assert.That(q.Count, Is.EqualTo(2));

            var ql1 = q.ToList();
            Assert.That(ql1.Select(p => p.FullName), Is.EquivalentTo(new string[] { "Person 1", "Person 2" }));

            using (var t = _realm.BeginWrite())
            {
                p1.FullName = "Modified Person";

                p3 = _realm.CreateObject<Person>();
                p3.FullName = "Person 3";

                t.Commit();
            }

            Assert.That(q.Count, Is.EqualTo(3));
            var ql2 = q.ToList();
            Assert.That(ql2.Select(p => p.FullName), Is.EquivalentTo(new string[] { "Modified Person", "Person 2", "Person 3" }));
        }

        [Test]
        public void ConcurrentTest()
        {
            Person p1, p2;

            using (var t = _realm.BeginWrite())
            {
                p1 = _realm.CreateObject<Person>();
                p1.FullName = "Person 1";

                t.Commit();
            }

            var q = _realm.All<Person>();
            Assert.That(q.Count, Is.EqualTo(1));

            var thread = new Thread(() =>
            {
                var newRealm = Realm.GetInstance(_databasePath);

                using (var t = newRealm.BeginWrite())
                {
                    p2 = newRealm.CreateObject<Person>();
                    p2.FullName = "Person 2";

                    t.Commit();
                }
            });
            thread.Start();
            thread.Join();

            _realm.Refresh();

            var ql2 = q.ToList();
            Assert.That(ql2.Select(p => p.FullName), Is.EquivalentTo(new string[] { "Person 1", "Person 2" }));
        }
    }
}

