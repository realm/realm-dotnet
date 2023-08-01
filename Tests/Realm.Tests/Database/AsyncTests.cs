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
using Nito.AsyncEx;
using NUnit.Framework;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class AsyncTests : RealmInstanceTest
    {
        [Test]
        public void RefreshAsync_Tests()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                Assert.That(SynchronizationContext.Current != null);

                var obj = _realm.Write(() =>
                {
                    return _realm.Add(new IntPrimaryKeyWithValueObject());
                });

                var reference = ThreadSafeReference.Create(obj);

                Task.Run(() =>
                {
                    using var realm = GetRealm(_realm.Config);
                    var bgObj = realm.ResolveReference(reference)!;
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
        public void AsyncWrite_WithActionCallback_PersistsChanges()
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
                Assert.That(peopleQuery.Count(), Is.EqualTo(0));

                var transaction = await _realm.BeginWriteAsync();
                var person = new Person { FullName = "Jonh Wickie" };
                _realm.Add(person);
                await transaction.CommitAsync();

                Assert.That(peopleQuery.First(), Is.EqualTo(person));
            });
        }

        [Test]
        public void AsyncBeginWrite_RollBack_DoesNotPersistData()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                using (var transaction = await _realm.BeginWriteAsync())
                {
                    var person = new Person { FullName = "Jonh Wickie" };
                    _realm.Add(person);
                    transaction.Rollback();
                }

                Assert.That(_realm.All<Person>().Count(), Is.EqualTo(0));
            });
        }

        [Test]
        public void AsyncBeginWrite_CloseRealm_DoesNotPersistData()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                Transaction transaction = null!;
                using (var realm = GetRealm(_realm.Config))
                {
                    var peopleQuery = realm.All<Person>();
                    Assert.That(peopleQuery.Count(), Is.EqualTo(0));
                    transaction = await realm.BeginWriteAsync();
                    var person = new Person { FullName = "Jonh Wickie" };
                    realm.Add(person);
                    Assert.That(peopleQuery.Count(), Is.EqualTo(1));
                }

                _realm.Refresh();
                Assert.That(_realm.All<Person>().Count(), Is.EqualTo(0));
                Assert.That(transaction.State, Is.EqualTo(TransactionState.RolledBack));
            });
        }

        [Test]
        public void WriteAsync_StartMultipleAsyncTransaction_NoDeadlock()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var transaction = await _realm.BeginWriteAsync();
                _realm.Add(new Person
                {
                    FirstName = "Marco"
                });

                var writeTask = _realm.WriteAsync(() =>
                {
                    _realm.Add(new Person
                    {
                        FirstName = "Giovanni"
                    });
                });

                await transaction.CommitAsync();
                await writeTask;

                var peopleQuery = _realm.All<Person>();
                Assert.That(peopleQuery.Count, Is.EqualTo(2));
                Assert.That(peopleQuery.ElementAt(0).FirstName, Is.EqualTo("Marco"));
                Assert.That(peopleQuery.ElementAt(1).FirstName, Is.EqualTo("Giovanni"));
            });
        }

        [Test]
        public void WriteAsync_CaptureObjectsInDelegate_DoesNotThrow()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                Person person = null!;
                await _realm.WriteAsync(() =>
                {
                    person = _realm.Add(new Person
                    {
                        FirstName = "Marco"
                    });
                });

                // assert that this thread can access a managed object that was created
                // within an async write transaction
                Assert.That(() => person.FullName, Throws.Nothing);
            });
        }

        [Test]
        public void AsyncWrite_FuncRealmObj()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var peopleQuery = _realm.All<Person>();
                Assert.That(peopleQuery.Count(), Is.EqualTo(0));

                var person = await _realm.WriteAsync(() =>
                {
                    var person = new Person { FullName = "Jonh Wickie" };
                    _realm.Add(person);
                    return person;
                });

                Assert.That(peopleQuery.Single(), Is.EqualTo(person));
            });
        }

        [Test]
        public void AsyncBeginWrite_ThrowsUserException()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                Exception? ex = null;
                try
                {
                    await _realm.WriteAsync(() =>
                    {
                        throw new Exception("User exception");
                    });
                }
                catch (Exception e)
                {
                    ex = e;
                }

                Assert.That(ex!.Message, Is.EqualTo("User exception"));
            });
        }

        [Test]
        public void AsyncWrite_FifoOrderRespected()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var acquisitionOrder = new List<int>();
                var tasks = new List<Task>();

                for (var i = 0; i < 5; i++)
                {
                    var index = i;
                    tasks.Add(_realm.WriteAsync(() =>
                    {
                        acquisitionOrder.Add(index);
                    }));
                }

                await Task.WhenAll(tasks);

                Assert.That(acquisitionOrder, Is.EqualTo(new[] { 0, 1, 2, 3, 4 }));
            });
        }

        [Test]
        public void AsyncWriteSyncWrite_Mixed_NoDeadlock()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var people = new Person[]
                {
                    new() { FirstName = "Anderson" },
                    new() { FirstName = "Banderson" },
                    new() { FirstName = "Canderson" },
                    new() { FirstName = "Danderson" }
                };

                var tasks = new Task[people.Length];

                // opening realm here avoids that in the following for loop multiple threads
                // try to add Person to the schema at the same time
                var realm = GetRealm(_realm.Config);

                for (var i = 0; i < people.Length; i++)
                {
                    // save the index as i will be modified by the for loop itself
                    var index = i;

                    if (index % 2 == 0)
                    {
                        tasks[index] = Task.Run(() => AsyncContext.Run(async () =>
                        {
                            using var realm = GetRealm(_realm.Config);
                            await realm.WriteAsync(() =>
                            {
                                realm.Add(people[index]);
                            });
                        }));
                    }
                    else
                    {
                        tasks[index] = Task.Run(() => AsyncContext.Run(() =>
                        {
                            using var realm = GetRealm(_realm.Config);
                            realm.Write(() =>
                            {
                                realm.Add(people[index]);
                            });
                        }));
                    }
                }

                await Task.WhenAll(tasks);
                realm.Refresh();
                Assert.That(realm.All<Person>().Count, Is.EqualTo(4));
                realm.Dispose();
            });
        }

        [Test]
        public void AsyncWrite_SyncCommit_Mixed()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var person = new Person { FirstName = "Anderson" };
                var transaction = await _realm.BeginWriteAsync();
                _realm.Add(person);
                transaction.Commit();

                Assert.That(_realm.All<Person>().Single, Is.EqualTo(person));
            });
        }

        [Test]
        public void SyncWrite_AsyncCommit_Mixed()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var person = new Person { FirstName = "Anderson" };
                var transaction = _realm.BeginWrite();
                _realm.Add(person);
                await transaction.CommitAsync();

                Assert.That(_realm.All<Person>().Single, Is.EqualTo(person));
            });
        }

        [Test]
        public void AsyncBeginWrite_CancelToken_OnAwait()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                using var cts = new CancellationTokenSource();
                var tcs = new TaskCompletionSource();
                var taskCancelled = false;

                var syncTask = Task.Run(() => AsyncContext.Run(async () =>
                {
                    using var realm = GetRealm(_realm.Config);
                    using var transaction = realm.BeginWrite();
                    tcs.TrySetResult();
                    cts.Cancel();
                    await Task.Delay(1000);
                }));

                var asyncTask = Task.Run(() => AsyncContext.Run(async () =>
                {
                    using var realm = GetRealm(_realm.Config);
                    try
                    {
                        await tcs.Task;
                        await realm.BeginWriteAsync(cts.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        taskCancelled = true;
                    }
                }));

                await Task.WhenAll(syncTask, asyncTask);
                Assert.That(taskCancelled, Is.EqualTo(true));
            });
        }

        [Test]
        public void AsyncWrite_CancelToken_OnAwait()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                using var cts = new CancellationTokenSource();
                var tcs = new TaskCompletionSource();
                var taskCancelled = false;

                var syncTask = Task.Run(() => AsyncContext.Run(() =>
                {
                    using var realm = GetRealm(_realm.Config);
                    var transaction = realm.BeginWrite();

                    tcs.TrySetResult();

                    cts.Cancel();
                    transaction.Rollback();
                }));

                var asyncTask = Task.Run(() => AsyncContext.Run(async () =>
                {
                    using var realm = GetRealm(_realm.Config);
                    try
                    {
                        await tcs.Task;
                        await realm.WriteAsync(() => { }, cts.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        taskCancelled = true;
                    }
                }));

                await Task.WhenAll(syncTask, asyncTask);
                Assert.That(taskCancelled, Is.EqualTo(true));
            });
        }

        [Test]
        public void AsyncCommitCancel_WhenRealmIsDisposed_IsNoOp()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                using var cts = new CancellationTokenSource();
                var writeTask = _realm.BeginWriteAsync(cts.Token);

                cts.Cancel();
                _realm.Dispose();

                await TestHelpers.AssertThrows<TaskCanceledException>(() => writeTask);
            });
        }

        [Test]
        public void WriteAsync_OnBackgroundThread_RunsSynchronously()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                Assert.That(_realm.All<Person>(), Is.Empty);

                await Task.Run(async () =>
                {
                    Assert.That(SynchronizationContext.Current, Is.Null);

                    using var realm = GetRealm(_realm.Config);
                    await realm.WriteAsync(() =>
                    {
                        realm.Add(new Person { FirstName = "Peter" });
                    });
                });

                _realm.Refresh();

                var people = _realm.All<Person>().ToArray();
                Assert.That(people.Length, Is.EqualTo(1));
                Assert.That(people.Single().FirstName, Is.EqualTo("Peter"));
            });
        }

        [Test]
        public void WriteAsync_CanceledInsideCallback()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var cts = new CancellationTokenSource();

                await TestHelpers.AssertThrows<TaskCanceledException>(() => _realm.WriteAsync(() =>
                {
                    _realm.Add(new Person());
                    cts.Cancel();
                }, cts.Token));

                Assert.That(_realm.All<Person>(), Is.Empty);
            });
        }
    }
}
