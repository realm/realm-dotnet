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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Realms;

namespace Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class RefreshTests : RealmInstanceTest
    {
        [Test]
        public void CommittingAWriteTransactionShouldRefreshQueries()
        {
            Person p1 = null;

            _realm.Write(() =>
            {
                p1 = _realm.Add(new Person { FullName = "Person 1" });
                _realm.Add(new Person { FullName = "Person 2" });
            });

            var q = _realm.All<Person>();
            var ql1 = q.ToList().Select(p => p.FullName);
            Assert.That(ql1, Is.EquivalentTo(new[] { "Person 1", "Person 2" }));

            _realm.Write(() =>
            {
                p1.FullName = "Modified Person";
                _realm.Add(new Person { FullName = "Person 3" });
            });

            var ql2 = q.ToList().Select(p => p.FullName);
            Assert.That(ql2, Is.EquivalentTo(new[] { "Modified Person", "Person 2", "Person 3" }));
        }

        [Test]
        public void CallingRefreshShouldRefreshQueriesAfterModificationsOnDifferentThreads()
        {
            _realm.Write(() =>
            {
                _realm.Add(new Person { FullName = "Person 1" });
            });

            var q = _realm.All<Person>();
            Assert.That(q.Count, Is.EqualTo(1));

            Task.Run(() =>
            {
                var r = Realm.GetInstance(_configuration);
                r.Write(() => r.Add(new Person { FullName = "Person 2" }));
                r.Dispose();
            }).Wait();

            _realm.Refresh();

            var ql2 = q.AsEnumerable()
                       .Select(p => p.FullName)
                       .ToArray();
            Assert.That(ql2, Is.EquivalentTo(new[] { "Person 1", "Person 2" }));
        }
    }
}
