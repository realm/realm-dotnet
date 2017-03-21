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
using System.IO;
using System.Linq;
using System.Threading;
using Foundation;
using NUnit.Framework;
using Realms;

namespace IntegrationTests.XamarinIOS
{
    [TestFixture]
    public class RunLoopTests
    {
        protected string _databasePath;

        [SetUp]
        public void SetUp()
        {
            _databasePath = Path.GetTempFileName();
        }

        private void WriteOnDifferentThread(Action<Realm> action)
        {
            var thread = new Thread(() =>
            {
                var r = Realm.GetInstance(_databasePath);
                r.Write(() => action(r));
                r.Dispose();
            });
            thread.Start();
            thread.Join();
        }

        [Test]
        public void QueriesShouldAutomaticallyRefreshInRunLoop()
        {
            var thread = new Thread(() =>
            {
                var r = Realm.GetInstance(_databasePath);
                r.Write(() =>
                {
                    r.Add(new Person
                    {
                        FullName = "Person 1"
                    });
                });

                var q = r.All<Person>();
                Assert.That(q.Count, Is.EqualTo(1));

                WriteOnDifferentThread(newRealm =>
                {
                    newRealm.Add(new Person
                    {
                        FullName = "Person 2"
                    });
                });

                // Instead of r.Refresh(), initiate the runloop which should trigger auto refresh
                NSRunLoop.Current.RunUntil((NSDate)DateTime.Now.AddMilliseconds(1));

                var ql2 = q.ToList();
                Assert.That(ql2.Select(p => p.FullName), Is.EquivalentTo(new[] { "Person 1", "Person 2" }));

                r.Dispose();
                Realm.DeleteRealm(r.Config);
            });

            thread.Start();
            thread.Join();
        }
    }
}