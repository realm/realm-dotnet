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
using System.Linq;
using NUnit.Framework;
using Realms;
using System.Threading;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections;
using System.Runtime.InteropServices;

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
            RealmResults<Person>.ChangeSet changes = null;
            RealmResults<Person>.NotificationCallback cb = (s, c, e) => changes = c;

            using (query.SubscribeForNotifications(cb))
            {
                _realm.Write(() => _realm.CreateObject<Person>());

                TestHelpers.RunEventLoop(TimeSpan.FromMilliseconds(100));
                Assert.That(changes, Is.Not.Null);
                Assert.That(changes.InsertedIndices, Is.EquivalentTo(new int[] { 0 }));
            }
        }

        [TestCaseSource(nameof(CollectionChangedTestCases))]
        public void TestCollectionChangedAdapter(int[] initial, NotifyCollectionChangedAction action, int[] change, bool coalesce)
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
            var query = _realm.All<OrderedObject>().Where(o => o.IsPartOfResults).OrderBy(o => o.Order);
            var observable = query.ToNotifyCollectionChanged(e => error = e, coalesceMultipleChangesIntoReset: coalesce);
            var handle = GCHandle.Alloc(observable); // prevent this from being collected across event loops

            try
            {
                // wait for the initial notification to come through
                TestHelpers.RunEventLoop(TimeSpan.FromMilliseconds(100));

                var eventArgs = new List<NotifyCollectionChangedEventArgs>();
                observable.CollectionChanged += (o, e) => eventArgs.Add(e);

                Assert.That(observable, Is.EquivalentTo(query));
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

                TestHelpers.RunEventLoop(TimeSpan.FromMilliseconds(100));
                Assert.That(observable, Is.EquivalentTo(query));
                Assert.That(error, Is.Null);

                if (coalesce && change.Length > 1)
                {
                    Assert.That(eventArgs.Single().Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
                }
                else
                {
                    Assert.That(eventArgs.All(e => e.Action == action));
                    if (action == NotifyCollectionChangedAction.Add)
                    {
                        Assert.That(eventArgs.SelectMany(e => e.NewItems.Cast<OrderedObject>()).Select(o => o.Order), Is.EquivalentTo(change));
                    }
                    else if (action == NotifyCollectionChangedAction.Remove)
                    {
                        Assert.That(eventArgs.SelectMany(e => e.OldItems.Cast<OrderedObject>()).Select(o => o.Order), Is.EquivalentTo(change));
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
            var cases = new object[][] {
                new object[] { new int[] { }, NotifyCollectionChangedAction.Add, new int[] { 1 } },
                new object[] { new int[] { }, NotifyCollectionChangedAction.Add, new int[] { 1, 2, 3 } },
                new object[] { new int[] { 1, 2, 3 }, NotifyCollectionChangedAction.Remove, new int[] { 1, 2, 3 } },
                new object[] { new int[] { 1, 2, 3 }, NotifyCollectionChangedAction.Remove, new int[] { 2 } },
                new object[] { new int[] { 1, 2, 3 }, NotifyCollectionChangedAction.Remove, new int[] { 1 } },
                new object[] { new int[] { 1, 2, 3 }, NotifyCollectionChangedAction.Add, new int[] { 0 } },
                new object[] { new int[] { 1, 2, 3 }, NotifyCollectionChangedAction.Add, new int[] { 4 } },
                new object[] { new int[] { 1, 2, 3 }, NotifyCollectionChangedAction.Add, new int[] { 4, 5 } },
                new object[] { new int[] { 1, 2, 3, 4, 5 }, NotifyCollectionChangedAction.Remove, new int[] { 3, 4 } },
                new object[] { new int[] { 1, 3, 5 }, NotifyCollectionChangedAction.Add, new int[] { 2, 4 } },
                new object[] { new int[] { 1, 2, 3, 4, 5 }, NotifyCollectionChangedAction.Remove, new int[] { 2, 4 } }
            };

            foreach (var testCase in cases)
            {
                yield return new TestCaseData(args: testCase.Concat(new object[] { true }).ToArray());
                yield return new TestCaseData(args: testCase.Concat(new object[] { false }).ToArray());
            }
        }

        [Test]
        public void CollectionChangedAdapter_DeletingItemFromRealm_ShouldRaiseReset()
        {
            OrderedObject obj = null;
            _realm.Write(() =>
            {
                for (var i = 0; i < 3; i++)
                {
                    obj = _realm.CreateObject<OrderedObject>();
                    obj.Order = i;
                }
            });
            var query = _realm.All<OrderedObject>();
            Exception error = null;
            var events = new List<NotifyCollectionChangedAction>();

            var observable = query.ToNotifyCollectionChanged(e => error = e);
            var handle = GCHandle.Alloc(observable); // prevent this from being collected across event loops
            observable.CollectionChanged += (o, e) => events.Add(e.Action);

            try
            {
                _realm.Write(() => _realm.Remove(obj));
                TestHelpers.RunEventLoop(TimeSpan.FromMilliseconds(100));

                Assert.That(error, Is.Null);
                Assert.That(observable, Is.EquivalentTo(query));
                Assert.That(events, Is.EquivalentTo(new[] { NotifyCollectionChangedAction.Reset }));
            }
            finally
            {
                handle.Free();
            }
        }
    }
}

#endif  // #if ENABLE_INTERNAL_NON_PCL_TESTS
