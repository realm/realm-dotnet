////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////

using System;
using NUnit.Framework;
using Realms;
using System.IO;
using System.Linq;
using System.Threading;

namespace IntegrationTests.Shared
{
    [TestFixture, Preserve(AllMembers = true)]
    public class RefreshTests
    {
        private Realm _realm;

        [SetUp]
        public void SetUp()
        {
            _realm = Realm.GetInstance();
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
                var r = Realm.GetInstance();
                r.Write(() => action(r));
                r.Close();
            });
            thread.Start();
            thread.Join();
        }

        [Test]
        public void CommittingAWriteTransactionShouldRefreshQueries()
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
            var ql1 = q.ToList().Select(p => p.FullName);
            Assert.That(ql1, Is.EquivalentTo(new[] { "Person 1", "Person 2" }));

            _realm.Write(() =>
            {
                p1.FullName = "Modified Person";

                p3 = _realm.CreateObject<Person>();
                p3.FullName = "Person 3";
            });

            var ql2 = q.ToList().Select(p => p.FullName);
            Assert.That(ql2, Is.EquivalentTo(new[] { "Modified Person", "Person 2", "Person 3" }));
        }

        [Test]
        public void CallingRefreshShouldRefreshQueriesAfterModificationsOnDifferentThreads()
        {
            Person p1, p2;

            _realm.Write(() =>
            {
                p1 = _realm.CreateObject<Person>();
                p1.FullName = "Person 1";
            });

            var q = _realm.All<Person>();
            Assert.That(q.Count, Is.EqualTo(1));

            WriteOnDifferentThread(newRealm =>
            {
                p2 = newRealm.CreateObject<Person>();
                p2.FullName = "Person 2";
            });

            _realm.Refresh();

            var ql2 = q.ToList().Select(p => p.FullName);
            Assert.That(ql2, Is.EquivalentTo(new[] { "Person 1", "Person 2" }));
        }
    }
}
