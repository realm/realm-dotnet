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

#if ENABLE_INTERNAL_NON_PCL_TESTS
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework;
using Realms;

namespace IntegrationTests.Shared
{
    [TestFixture, Preserve(AllMembers = true)]
#if WINDOWS
    [Ignore("Notifications are not implemented on Windows yet")]
#endif
    public class NotificationTests
    {
        private string _databasePath;
        private Realm _realm;

        private class OrderedObject : RealmObject
        {
            public int Order { get; set; }

            public bool IsPartOfResults { get; set; }

            public override string ToString()
            {
                return string.Format("[OrderedObject: Order={0}]", Order);
            }
        }

        [SetUp]
        public void SetUp()
        {
            _databasePath = Path.GetTempFileName();
            _realm = Realm.GetInstance(_databasePath);
        }

        [TearDown]
        public void TearDown()
        {
            _realm.Dispose();
            Realm.DeleteRealm(_realm.Config);
        }

        [Test]
        public void ShouldTriggerRealmChangedEvent()
        {
            // Arrange
            var wasNotified = false;
            _realm.RealmChanged += (sender, e) => { wasNotified = true; };

            // Act
            _realm.Write(() => _realm.CreateObject<Person>());

            // Assert
            Assert.That(wasNotified, "RealmChanged notification was not triggered");
        }

        [Test]
        public void ResultsShouldSendNotifications()
        {
            var query = _realm.All<Person>();
            ChangeSet changes = null;
            NotificationCallbackDelegate<Person> cb = (s, c, e) => changes = c;

            using (query.SubscribeForNotifications(cb))
            {
                _realm.Write(() => _realm.CreateObject<Person>());

                TestHelpers.RunEventLoop();
                Assert.That(changes, Is.Not.Null);
                Assert.That(changes.InsertedIndices, Is.EquivalentTo(new int[] { 0 }));
            }
        }

        [Test]
        public void CollectionChanged_WhenUnsubscribed_ShouldStopReceivingNotifications()
        {
            _realm.Write(() =>
            {
                _realm.Add(new OrderedObject
                {
                    Order = 0,
                    IsPartOfResults = true
                });
            });

            Exception error = null;
            _realm.Error += (sender, e) =>
            {
                error = e.GetException();
            };

            var query = _realm.All<OrderedObject>().Where(o => o.IsPartOfResults).OrderBy(o => o.Order).ToNotifyCollectionChanged();
            var handle = GCHandle.Alloc(query); // prevent this from being collected across event loops
            try
            {
                // wait for the initial notification to come through
                TestHelpers.RunEventLoop();

                var eventArgs = new List<NotifyCollectionChangedEventArgs>();
                var handler = new NotifyCollectionChangedEventHandler((sender, e) =>
                {
                    eventArgs.Add(e);
                });
                query.CollectionChanged += handler;

                Assert.That(error, Is.Null);

                _realm.Write(() =>
                {
                    _realm.Add(new OrderedObject
                    {
                        Order = 1,
                        IsPartOfResults = true
                    });
                });

                TestHelpers.RunEventLoop();
                
                Assert.That(error, Is.Null);
                Assert.That(eventArgs.Count, Is.EqualTo(1));
                Assert.That(eventArgs[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Add));

                _realm.Write(() =>
                {
                    _realm.Add(new OrderedObject
                    {
                        Order = 2,
                        IsPartOfResults = true
                    });
                });

                TestHelpers.RunEventLoop();

                Assert.That(error, Is.Null);
                Assert.That(eventArgs.Count, Is.EqualTo(2));
                Assert.That(eventArgs.All(e => e.Action == NotifyCollectionChangedAction.Add));

                query.CollectionChanged -= handler;

                _realm.Write(() =>
                {
                    _realm.Add(new OrderedObject
                    {
                        Order = 3,
                        IsPartOfResults = true
                    });
                });

                TestHelpers.RunEventLoop();

                Assert.That(error, Is.Null);
                Assert.That(eventArgs.Count, Is.EqualTo(2));
                Assert.That(eventArgs.All(e => e.Action == NotifyCollectionChangedAction.Add));
            }
            finally
            {
                handle.Free();
            }
        }

        [Test]
        public void CollectionChanged_WhenTransactionHasBothAddAndRemove_ShouldReset()
        {
            var first = new OrderedObject
            {
                Order = 0,
                IsPartOfResults = true
            };
            _realm.Write(() =>
            {
                _realm.Add(first);
            });

            Exception error = null;
            _realm.Error += (sender, e) =>
            {
                error = e.GetException();
            };

            var query = _realm.All<OrderedObject>().Where(o => o.IsPartOfResults).OrderBy(o => o.Order).ToNotifyCollectionChanged();
            var handle = GCHandle.Alloc(query); // prevent this from being collected across event loops
            try
            {
                // wait for the initial notification to come through
                TestHelpers.RunEventLoop();

                var eventArgs = new List<NotifyCollectionChangedEventArgs>();
                query.CollectionChanged += (sender, e) =>
                {
                    eventArgs.Add(e);
                };

                Assert.That(error, Is.Null);

                _realm.Write(() =>
                {
                    _realm.Add(new OrderedObject
                    {
                        Order = 1,
                        IsPartOfResults = true
                    });

                    _realm.Remove(first);
                });

                TestHelpers.RunEventLoop();

                Assert.That(error, Is.Null);
                Assert.That(eventArgs.Count, Is.EqualTo(1));
                Assert.That(eventArgs[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
            }
            finally
            {
                handle.Free();
            }
        }

        [TestCaseSource(nameof(CollectionChangedTestCases))]
        public void TestCollectionChangedAdapter(int[] initial, NotifyCollectionChangedAction action, int[] change, int startIndex)
        {
            _realm.Write(() =>
            {
                foreach (var value in initial)
                {
                    var obj = _realm.CreateObject<OrderedObject>();
                    obj.Order = value;
                    obj.IsPartOfResults = true;
                }
            });

            Exception error = null;
            _realm.Error += (sender, e) =>
            {
                error = e.GetException();
            };

            var query = _realm.All<OrderedObject>().Where(o => o.IsPartOfResults).OrderBy(o => o.Order).ToNotifyCollectionChanged();
            var handle = GCHandle.Alloc(query); // prevent this from being collected across event loops

            try
            {
                // wait for the initial notification to come through
                TestHelpers.RunEventLoop();

                var eventArgs = new List<NotifyCollectionChangedEventArgs>();
                query.CollectionChanged += (o, e) => eventArgs.Add(e);

                Assert.That(error, Is.Null);
                _realm.Write(() =>
                {
                    if (action == NotifyCollectionChangedAction.Add)
                    {
                        foreach (var value in change)
                        {
                            var obj = _realm.CreateObject<OrderedObject>();
                            obj.Order = value;
                            obj.IsPartOfResults = true;
                        }
                    }
                    else if (action == NotifyCollectionChangedAction.Remove)
                    {
                        foreach (var value in change)
                        {
                            _realm.All<OrderedObject>().Single(o => o.Order == value).IsPartOfResults = false;
                        }
                    }
                });

                TestHelpers.RunEventLoop();
                Assert.That(error, Is.Null);

                Assert.That(eventArgs.Count, Is.EqualTo(1));
                var arg = eventArgs[0];
                if (startIndex < 0)
                {
                    Assert.That(arg.Action == NotifyCollectionChangedAction.Reset);
                }
                else
                {
                    Assert.That(arg.Action == action);
                    if (action == NotifyCollectionChangedAction.Add)
                    {
                        Assert.That(arg.NewStartingIndex, Is.EqualTo(startIndex));
                        Assert.That(arg.NewItems.Cast<OrderedObject>().Select(o => o.Order), Is.EquivalentTo(change));
                    }
                    else if (action == NotifyCollectionChangedAction.Remove)
                    {
                        Assert.That(arg.OldStartingIndex, Is.EqualTo(startIndex));
                        Assert.That(arg.OldItems.Count, Is.EqualTo(change.Length));
                    }
                }
            }
            finally
            {
                handle.Free();
            }
        }

        public IEnumerable<TestCaseData> CollectionChangedTestCases()
        {
            yield return new TestCaseData(new int[] { }, NotifyCollectionChangedAction.Add, new int[] { 1 },  0);
            yield return new TestCaseData(new int[] { }, NotifyCollectionChangedAction.Add, new int[] { 1, 2, 3 }, 0);
            yield return new TestCaseData(new int[] { 1, 2, 3 }, NotifyCollectionChangedAction.Remove, new int[] { 1, 2, 3 }, 0);
            yield return new TestCaseData(new int[] { 1, 2, 3 }, NotifyCollectionChangedAction.Remove, new int[] { 2 }, 1);
            yield return new TestCaseData(new int[] { 1, 2, 3 }, NotifyCollectionChangedAction.Remove, new int[] { 1 }, 0);
            yield return new TestCaseData(new int[] { 1, 2, 3 }, NotifyCollectionChangedAction.Add, new int[] { 0 }, 0);
            yield return new TestCaseData(new int[] { 1, 2, 3 }, NotifyCollectionChangedAction.Add, new int[] { 4 }, 3);
            yield return new TestCaseData(new int[] { 1, 2, 3 }, NotifyCollectionChangedAction.Add, new int[] { 4, 5 }, 3);
            yield return new TestCaseData(new int[] { 1, 2, 3, 4, 5 }, NotifyCollectionChangedAction.Remove, new int[] { 3, 4 }, 2);
            yield return new TestCaseData(new int[] { 1, 3, 5 }, NotifyCollectionChangedAction.Add, new int[] { 2, 4 }, -1);
            yield return new TestCaseData(new int[] { 1, 2, 3, 4, 5 }, NotifyCollectionChangedAction.Remove, new int[] { 2, 4 }, -1);
        }
    }
}

#endif  // #if ENABLE_INTERNAL_NON_PCL_TESTS
