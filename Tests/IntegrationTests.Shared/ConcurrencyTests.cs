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

        private void WriteOnDifferentThread(Action<Realm> action)
        {
            var thread = new Thread(() => 
            {
                var r = Realm.GetInstance(_databasePath);
                r.Write(() => action(r));
            });
            thread.Start();
            thread.Join();
        }

        [Test]
        public void SimpleTest()
        {
            Person p1 = null, p2, p3;

            _realm.Write(() =>
            {
                p1 = _realm.CreateObject<Person>();
                p1.FullName = "Person 1";

                p2 = _realm.CreateObject<Person>();
                p2.FullName = "Person 2";
            });

            var q = _realm.All<Person>();
            Assert.That(q.Count, Is.EqualTo(2));

            var ql1 = q.ToList();
            Assert.That(ql1.Select(p => p.FullName), Is.EquivalentTo(new string[] { "Person 1", "Person 2" }));

            _realm.Write(() =>
            {
                p1.FullName = "Modified Person";

                p3 = _realm.CreateObject<Person>();
                p3.FullName = "Person 3";
            });

            Assert.That(q.Count, Is.EqualTo(3));
            var ql2 = q.ToList();
            Assert.That(ql2.Select(p => p.FullName), Is.EquivalentTo(new string[] { "Modified Person", "Person 2", "Person 3" }));
        }

        [Test]
        public void ConcurrentTest()
        {
            Person p1, p2;

            _realm.Write(() =>
            {
                p1 = _realm.CreateObject<Person>();
                p1.FullName = "Person 1";
            });

            var q = _realm.All<Person>();
            Assert.That(q.Count, Is.EqualTo(1));

            WriteOnDifferentThread((Realm newRealm) =>
            {
                p2 = newRealm.CreateObject<Person>();
                p2.FullName = "Person 2";
            });

            _realm.Refresh();

            var ql2 = q.ToList();
            Assert.That(ql2.Select(p => p.FullName), Is.EquivalentTo(new string[] { "Person 1", "Person 2" }));
        }
    }
}

