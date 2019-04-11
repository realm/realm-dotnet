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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;
using Realms;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class AsyncTests : RealmTest
    {
        private Lazy<Realm> _lazyRealm;

        private Realm _realm => _lazyRealm.Value;

        // We capture the current SynchronizationContext when opening a Realm.
        // However, NUnit replaces the SynchronizationContext after the SetUp method and before the async test method.
        // That's why we make sure we open the Realm in the test method by accessing it lazily.
        protected override void CustomSetUp()
        {
            _lazyRealm = new Lazy<Realm>(() => Realm.GetInstance());
            base.CustomSetUp();
        }

        protected override void CustomTearDown()
        {
            if (_lazyRealm.IsValueCreated)
            {
                _realm.Dispose();
                Realm.DeleteRealm(_realm.Config);
            }

            base.CustomTearDown();
        }

        [Test]
        public void AsyncWrite_ShouldExecuteOnWorkerThread()
        {
            AsyncContext.Run(async delegate
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

        [Test]
        public void AsyncWrite_WhenOnBackgroundThread_ShouldExecuteOnSameThread()
        {
            AsyncContext.Run(async () =>
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

        [Test]
        public void AsyncWrite_UpdateViaPrimaryKey()
        {
            AsyncContext.Run(async () =>
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
            AsyncContext.Run(async () =>
            {
                const string message = "this is an exception from user code";
                try
                {
                    await _realm.WriteAsync(_ => throw new Exception(message));
                    Assert.Fail("Previous method should have thrown");
                }
                catch (Exception e)
                {
                    Assert.That(e.Message, Is.EqualTo(message));
                }
            });        
        }

        [Test]
        public void RefreshAsync_Tests()
        {
            AsyncContext.Run(async () =>
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
                    using (var realm = Realm.GetInstance(_realm.Config))
                    {
                        var bgObj = realm.ResolveReference(reference);
                        realm.Write(() =>
                        {
                            bgObj.StringValue = "123";
                        });
                    }
                }).Wait(); // <- wait to avoid the main thread autoupdating while idle

                Assert.That(obj.StringValue, Is.Null);

                var changeTiming = await measureTiming(_realm.RefreshAsync);

                Assert.That(obj.StringValue, Is.EqualTo("123"));

                // Make sure when there are no changes RefreshAsync completes quickly
                var idleTiming = await measureTiming(_realm.RefreshAsync);

                Assert.That(changeTiming, Is.GreaterThan(idleTiming));

                async Task<long> measureTiming(Func<Task> func)
                {
                    var sw = new Stopwatch();
                    sw.Start();
                    await func();
                    sw.Stop();
                    return sw.ElapsedTicks;
                }
            });
        }
    }
}