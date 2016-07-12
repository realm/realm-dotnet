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
using NUnit.Framework;
using Realms;

namespace IntegrationTests.Shared
{
    [TestFixture, Preserve(AllMembers = true)]
    public class AsyncTests
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

        [Test]
#if WINDOWS
        [Ignore("We don't support async on Windows just yet.")]
#endif
        public async void AsyncWrite_ShouldExecuteOnWorkerThread()
        {
            var currentThreadId = Thread.CurrentThread.ManagedThreadId;
            var otherThreadId = currentThreadId;

            Assert.That(_realm.All<Person>().Count(), Is.EqualTo(0));
            await _realm.WriteAsync(realm =>
            {
                otherThreadId = Thread.CurrentThread.ManagedThreadId;
                realm.CreateObject<Person>();
            });

            // see #564
            TestHelpers.RunEventLoop(TimeSpan.FromMilliseconds(100));

            Assert.That(_realm.All<Person>().Count(), Is.EqualTo(1));
            Assert.That(otherThreadId, Is.Not.EqualTo(currentThreadId));
        }

        class MyDataObject : RealmObject
        {
            [ObjectId]
            public string Path { get; set; }

            public int? ExpensiveToComputeValue { get; set; }
        }

        [Test]
#if WINDOWS
        [Ignore("We don't support async on Windows just yet.")]
#endif
        public async void AsyncWrite_UpdateViaObjectId()
        {
            var path = "/path/to/some/item";
            MyDataObject obj = null;
            _realm.Write(() =>
            {
                obj = _realm.CreateObject<MyDataObject>();
                obj.Path = path;
            });

            await _realm.WriteAsync(realm =>
            {
                var dataObj = realm.All<MyDataObject>().Single(d => d.Path == path);
                dataObj.ExpensiveToComputeValue = 123; // imagine this was a very CPU-intensive operation
            });

            // see #564
            TestHelpers.RunEventLoop(TimeSpan.FromMilliseconds(100));

            Assert.That(obj.ExpensiveToComputeValue, Is.Not.Null);
        }
    }
}

