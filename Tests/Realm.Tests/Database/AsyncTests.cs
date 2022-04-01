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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class AsyncTests : RealmInstanceTest
    {
        [Test, Obsolete("Tests deprecated WriteAsync API")]
        public void AsyncWrite_ShouldExecuteOnWorkerThread()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var currentThreadId = Environment.CurrentManagedThreadId;
                var otherThreadId = currentThreadId;

                Assert.That(_realm.All<Person>().Count(), Is.EqualTo(0));
                Assert.That(SynchronizationContext.Current != null);
                await _realm.WriteAsync(realm =>
                {
                    otherThreadId = Environment.CurrentManagedThreadId;
                    realm.Add(new Person());
                });

                Assert.That(_realm.All<Person>().Count(), Is.EqualTo(1));
                Assert.That(otherThreadId, Is.Not.EqualTo(currentThreadId));
            });
        }

        [Test, Obsolete("Tests deprecated WriteAsync API")]
        public void AsyncWrite_WhenOnBackgroundThread_ShouldExecuteOnSameThread()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                await Task.Run(async () =>
                {
                    var currentThreadId = Environment.CurrentManagedThreadId;
                    var otherThreadId = -1;

                    Assert.That(_realm.All<Person>().Count(), Is.EqualTo(0));
                    Assert.That(SynchronizationContext.Current == null);

                    await _realm.WriteAsync(realm =>
                    {
                        otherThreadId = Environment.CurrentManagedThreadId;
                        realm.Add(new Person());
                    });

                    Assert.That(_realm.All<Person>().Count(), Is.EqualTo(1));
                    Assert.That(otherThreadId, Is.EqualTo(currentThreadId));

                    _realm.Dispose();
                });
            });
        }

        [Test, Obsolete("Tests deprecated WriteAsync API")]
        public void AsyncWrite_UpdateViaPrimaryKey()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                IntPrimaryKeyWithValueObject obj = null;
                _realm.Write(() =>
                {
                    obj = _realm.Add(new IntPrimaryKeyWithValueObject { Id = 123 });
                });

                await _realm.WriteAsync(realm =>
                {
                    var dataObj = realm.Find<IntPrimaryKeyWithValueObject>(123);
                    dataObj.StringValue = "foobar";
                });

                // Make sure the changes are immediately visible on the caller thread
                Assert.That(obj.StringValue, Is.EqualTo("foobar"));
            });
        }

        [Test]
        public void AsyncWrite_ShouldRethrowExceptions()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                const string message = "this is an exception from user code";
                var ex = await TestHelpers.AssertThrows<Exception>(() => _realm.WriteAsync(_ => throw new Exception(message)));
                Assert.That(ex.Message, Is.EqualTo(message));
            });
        }

        [Test]
        public void RefreshAsync_Tests()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                Assert.That(SynchronizationContext.Current != null);

                IntPrimaryKeyWithValueObject obj = null;
                _realm.Write(() =>
                {
                    obj = _realm.Add(new IntPrimaryKeyWithValueObject());
                });

                var reference = ThreadSafeReference.Create(obj);

                Task.Run(() =>
                {
                    using var realm = GetRealm(_realm.Config);
                    var bgObj = realm.ResolveReference(reference);
                    realm.Write(() =>
                    {
                        bgObj.StringValue = "123";
                    });
                }).Wait(); // <- wait to avoid the main thread autoupdating while idle

                Assert.That(obj.StringValue, Is.Null);

                var changeTiming = await MeasureTiming(_realm.RefreshAsync);

                Assert.That(obj.StringValue, Is.EqualTo("123"));

                // Make sure when there are no changes RefreshAsync completes quickly
                var idleTiming = await MeasureTiming(_realm.RefreshAsync);

                Assert.That(changeTiming, Is.GreaterThan(idleTiming));

                static async Task<long> MeasureTiming(Func<Task> func)
                {
                    var sw = new Stopwatch();
                    sw.Start();
                    await func();
                    sw.Stop();
                    return sw.ElapsedTicks;
                }
            });
        }

        [Test]
        public void AsyncWrite_Action()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var peopleQuery = _realm.All<Person>();
                var personName = "Jonh Wickie";
                Assert.That(peopleQuery.Count(), Is.EqualTo(0));

                await _realm.WriteAsync(() =>
                {
                    var person = new Person { FullName = personName };
                    _realm.Add(person);
                });
                Assert.That(peopleQuery.Count(), Is.EqualTo(1));
                Assert.That(peopleQuery.First().FullName, Is.EqualTo(personName));
            });
        }

        [Test]
        public void AsyncBeginWriteAndCommit()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var peopleQuery = _realm.All<Person>();
                var personName = "Jonh Wickie";
                Assert.That(peopleQuery.Count(), Is.EqualTo(0));

                var transaction = await _realm.BeginWriteAsync();
                var person = new Person { FullName = personName };
                _realm.Add(person);
                await transaction.CommitAsync();

                Assert.That(peopleQuery.Count(), Is.EqualTo(1));
                Assert.That(peopleQuery.First().FullName, Is.EqualTo(personName));
            });
        }

        // I have a hard time testing an async rollback since it just cancels
        // the callback that will notify the SDK that the fsync has completed. It won't cancel the effective commit/fsync call
        // in core.
        // This test does not really test much. It just checks that no fsync is executed,
        // which is not what a rollback on an async transaction cares to do.
        [Test]
        public void AsyncBeginWriteAndCancel()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var dbPath = _realm.Config.DatabasePath;
                var peopleQuery = _realm.All<Person>();
                var personName = "Jonh Wickie";
                Assert.That(peopleQuery.Count(), Is.EqualTo(0));

                using (var transaction = await _realm.BeginWriteAsync())
                {
                    var person = new Person { FullName = personName };
                    _realm.Add(person);
                    transaction.Rollback();
                }

                Assert.That(peopleQuery.Count(), Is.EqualTo(1));
                _realm.Dispose();
                using var realm = Realm.GetInstance(dbPath);
                Assert.That(realm.All<Person>().Count(), Is.EqualTo(0));
            });
        }

        [Test]
        public void AsyncWrite_Func_RealmObj()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var peopleQuery = _realm.All<Person>();
                var personName = "Jonh Wickie";
                Assert.That(peopleQuery.Count(), Is.EqualTo(0));

                var person = await _realm.WriteAsync(() =>
                {
                    var person = new Person { FullName = personName };
                    _realm.Add(person);
                    return person;
                });

                Assert.That(peopleQuery.Count(), Is.EqualTo(1));
                Assert.That(peopleQuery.First().FullName, Is.EqualTo(personName));
                Assert.That(person, Is.Not.Null);
                Assert.That(person.FullName, Is.EqualTo(personName));
            });
        }

        [Test]
        public void AsyncWrite_Func_List()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var peopleQuery = _realm.All<Person>();
                var personName = "Jonh Wickie";
                Assert.That(peopleQuery.Count(), Is.EqualTo(0));

                var list = await _realm.WriteAsync(() =>
                {
                    var list = new List<Person> { new Person { FullName = personName } };
                    _realm.Add(list);
                    return list;
                });

                Assert.That(peopleQuery.Count(), Is.EqualTo(1));
                Assert.That(peopleQuery.First().FullName, Is.EqualTo(personName));
                Assert.That(list, Is.Not.Null);
                Assert.That(list.Count, Is.EqualTo(1));
                Assert.That(list.First().FullName, Is.EqualTo(personName));
            });
        }

        //TODO andrea: Probably this adds nothing to the one already existing for lists
        [Test]
        public void AsyncWrite_Func_Queryable()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var peopleQuery = _realm.All<Person>();
                var personName = "Jonh Wickie";
                Assert.That(peopleQuery.Count(), Is.EqualTo(0));

                var queryable = await _realm.WriteAsync(() =>
                {
                    var queryable = new List<Person> { new Person { FullName = personName } }.AsQueryable();
                    _realm.Add(queryable);
                    return queryable;
                });

                Assert.That(peopleQuery.Count(), Is.EqualTo(1));
                Assert.That(peopleQuery.First().FullName, Is.EqualTo(personName));
                Assert.That(queryable, Is.Not.Null);
                Assert.That(queryable.Count, Is.EqualTo(1));
                Assert.That(queryable.First().FullName, Is.EqualTo(personName));
            });
        }
    }
}
