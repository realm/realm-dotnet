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
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;
using Realms;

namespace IntegrationTests
{
    [TestFixture, Preserve(AllMembers = true)]
    public class NotificationTests
    {
        private const int MillisecondsToWaitForCollectionNotification = 50;

        private class OrderedContainer : RealmObject
        {
            public IList<OrderedObject> Items { get; }
        }

        private class OrderedObject : RealmObject
        {
            public int Order { get; set; }

            public bool IsPartOfResults { get; set; }

            public override string ToString()
            {
                return string.Format("[OrderedObject: Order={0}]", Order);
            }
        }

        private Lazy<Realm> _lazyRealm;

        private Realm _realm => _lazyRealm.Value;

        // We capture the current SynchronizationContext when opening a Realm.
        // However, NUnit replaces the SynchronizationContext after the SetUp method and before the async test method.
        // That's why we make sure we open the Realm in the test method by accessing it lazily.

        [SetUp]
        public void SetUp()
        {
            _lazyRealm = new Lazy<Realm>(() => Realm.GetInstance(Path.GetTempFileName()));
        }

        [TearDown]
        public void TearDown()
        {
            if (_lazyRealm.IsValueCreated)
            {
                _realm.Dispose();
                Realm.DeleteRealm(_realm.Config);
            }
        }

        [Test]
        public void ShouldTriggerRealmChangedEvent()
        {
            // Arrange
            var wasNotified = false;
            _realm.RealmChanged += (sender, e) => { wasNotified = true; };

            // Act
            _realm.Write(() => _realm.Add(new Person()));

            // Assert
            Assert.That(wasNotified, "RealmChanged notification was not triggered");
        }

        [Test]
        public void RealmError_WhenNoSubscribers_OutputsMessageInConsole()
        {
            using (var sw = new StringWriter())
            {
                var original = Console.Error;
                Console.SetError(sw);
                _realm.NotifyError(new Exception());

                Assert.That(sw.ToString(), Contains.Substring("exception").And.ContainsSubstring("Realm.Error"));
                Console.SetError(original);
            }
        }

        [Test]
        public void ResultsShouldSendNotifications()
        {
            AsyncContext.Run(async delegate
            {
                var query = _realm.All<Person>();
                ChangeSet changes = null;
                NotificationCallbackDelegate<Person> cb = (s, c, e) => changes = c;

                using (query.SubscribeForNotifications(cb))
                {
                    _realm.Write(() => _realm.Add(new Person()));

                    await Task.Delay(MillisecondsToWaitForCollectionNotification);
                    Assert.That(changes, Is.Not.Null);
                    Assert.That(changes.InsertedIndices, Is.EquivalentTo(new int[] { 0 }));
                }
            });
        }

        [Test]
        public void ListShouldSendNotifications()
        {
            AsyncContext.Run(async delegate
            {
                var container = new OrderedContainer();
                _realm.Write(() => _realm.Add(container));
                ChangeSet changes = null;
                NotificationCallbackDelegate<OrderedObject> cb = (s, c, e) => changes = c;

                using (container.Items.SubscribeForNotifications(cb))
                {
                    _realm.Write(() => container.Items.Add(new OrderedObject()));

                    await Task.Delay(MillisecondsToWaitForCollectionNotification);
                    Assert.That(changes, Is.Not.Null);
                    Assert.That(changes.InsertedIndices, Is.EquivalentTo(new int[] { 0 }));
                }
            });
        }

        [Test]
        public void UnsubscribeInNotificationCallback()
        {
            AsyncContext.Run(async delegate
            {
                var query = _realm.All<Person>();
                IDisposable notificationToken = null;

                int notificationCount = 0;
                notificationToken = query.SubscribeForNotifications(delegate
                {
                    notificationCount++;
                    notificationToken.Dispose();
                });

                for (int i = 0; i < 2; i++)
                {
                    _realm.Write(() => _realm.Add(new Person()));
                    await Task.Delay(MillisecondsToWaitForCollectionNotification);
                    Assert.That(notificationCount, Is.EqualTo(1));
                }
            });
        }

        [Test]
        public void Results_WhenUnsubscribed_ShouldStopReceivingNotifications()
        {
            AsyncContext.Run(async delegate
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
                    error = e.Exception;
                };

                var query = _realm.All<OrderedObject>().Where(o => o.IsPartOfResults).OrderBy(o => o.Order).AsRealmCollection();

                // wait for the initial notification to come through
                await Task.Yield();

                var eventArgs = new List<NotifyCollectionChangedEventArgs>();
                var handler = new NotifyCollectionChangedEventHandler((sender, e) => eventArgs.Add(e));

                var propertyEventArgs = new List<string>();
                var propertyHandler = new PropertyChangedEventHandler((sender, e) => propertyEventArgs.Add(e.PropertyName));

                query.CollectionChanged += handler;
                query.PropertyChanged += propertyHandler;

                Assert.That(error, Is.Null);

                _realm.Write(() =>
                {
                    _realm.Add(new OrderedObject
                    {
                        Order = 1,
                        IsPartOfResults = true
                    });
                });

                await Task.Delay(MillisecondsToWaitForCollectionNotification);

                Assert.That(error, Is.Null);
                Assert.That(eventArgs.Count, Is.EqualTo(1));
                Assert.That(eventArgs[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
                Assert.That(propertyEventArgs.Count, Is.EqualTo(2));
                Assert.That(propertyEventArgs, Is.EquivalentTo(new[] { "Count", "Item[]" }));

                _realm.Write(() =>
                {
                    _realm.Add(new OrderedObject
                    {
                        Order = 2,
                        IsPartOfResults = true
                    });
                });

                await Task.Delay(MillisecondsToWaitForCollectionNotification);

                Assert.That(error, Is.Null);
                Assert.That(eventArgs.Count, Is.EqualTo(2));
                Assert.That(eventArgs.All(e => e.Action == NotifyCollectionChangedAction.Add));
                Assert.That(propertyEventArgs.Count, Is.EqualTo(4));
                Assert.That(propertyEventArgs, Is.EquivalentTo(new[] { "Count", "Item[]", "Count", "Item[]" }));

                query.CollectionChanged -= handler;
                query.PropertyChanged -= propertyHandler;

                _realm.Write(() =>
                {
                    _realm.Add(new OrderedObject
                    {
                        Order = 3,
                        IsPartOfResults = true
                    });
                });

                await Task.Delay(MillisecondsToWaitForCollectionNotification);

                Assert.That(error, Is.Null);
                Assert.That(eventArgs.Count, Is.EqualTo(2));
                Assert.That(eventArgs.All(e => e.Action == NotifyCollectionChangedAction.Add));
                Assert.That(propertyEventArgs.Count, Is.EqualTo(4));
                Assert.That(propertyEventArgs, Is.EquivalentTo(new[] { "Count", "Item[]", "Count", "Item[]" }));
            });
        }

        [Test]
        public void Results_WhenTransactionHasBothAddAndRemove_ShouldReset()
        {
            AsyncContext.Run(async delegate
            {
                // The INotifyCollectionChanged API doesn't have a mechanism to report both added and removed items,
                // as that would mess up the indices a lot. That's why when we have both removed and added items,
                // we should raise a Reset.
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
                    error = e.Exception;
                };

                var query = _realm.All<OrderedObject>().Where(o => o.IsPartOfResults).OrderBy(o => o.Order).AsRealmCollection();

                // wait for the initial notification to come through
                await Task.Yield();

                var eventArgs = new List<NotifyCollectionChangedEventArgs>();
                query.CollectionChanged += (sender, e) => eventArgs.Add(e);

                var propertyEventArgs = new List<string>();
                query.PropertyChanged += (sender, e) => propertyEventArgs.Add(e.PropertyName);

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

                await Task.Delay(MillisecondsToWaitForCollectionNotification);

                Assert.That(error, Is.Null);
                Assert.That(eventArgs.Count, Is.EqualTo(1));
                Assert.That(eventArgs[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
                Assert.That(propertyEventArgs.Count, Is.EqualTo(2));
                Assert.That(propertyEventArgs, Is.EquivalentTo(new[] { "Count", "Item[]" }));
            });
        }

        [Test]
        public void List_WhenUnsubscribed_ShouldStopReceivingNotifications()
        {
            AsyncContext.Run(async delegate
            {
                var container = new OrderedContainer();
                _realm.Write(() => _realm.Add(container));

                var eventArgs = new List<NotifyCollectionChangedEventArgs>();
                var handler = new NotifyCollectionChangedEventHandler((sender, e) =>
                {
                    eventArgs.Add(e);
                });

                var propertyEventArgs = new List<string>();
                var propertyHandler = new PropertyChangedEventHandler((sender, e) => propertyEventArgs.Add(e.PropertyName));

                var collection = container.Items.AsRealmCollection();
                collection.CollectionChanged += handler;
                collection.PropertyChanged += propertyHandler;

                _realm.Write(() =>
                {
                    container.Items.Add(new OrderedObject());
                });

                await Task.Delay(MillisecondsToWaitForCollectionNotification);

                Assert.That(eventArgs.Count, Is.EqualTo(1));
                Assert.That(eventArgs[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
                Assert.That(propertyEventArgs.Count, Is.EqualTo(2));
                Assert.That(propertyEventArgs, Is.EquivalentTo(new[] { "Count", "Item[]" }));

                collection.CollectionChanged -= handler;
                collection.PropertyChanged -= propertyHandler;

                _realm.Write(() =>
                {
                    container.Items.Add(new OrderedObject());
                });

                await Task.Delay(MillisecondsToWaitForCollectionNotification);

                Assert.That(eventArgs.Count, Is.EqualTo(1));
                Assert.That(eventArgs[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
                Assert.That(propertyEventArgs.Count, Is.EqualTo(2));
                Assert.That(propertyEventArgs, Is.EquivalentTo(new[] { "Count", "Item[]" }));
            });
        }

        [Test]
        public void List_WhenTransactionHasBothAddAndRemove_ShouldReset()
        {
            AsyncContext.Run(async delegate
            {
                // The INotifyCollectionChanged API doesn't have a mechanism to report both added and removed items,
                // as that would mess up the indices a lot. That's why when we have both removed and added items,
                // we should raise a Reset.
                var container = new OrderedContainer();
                container.Items.Add(new OrderedObject());
                _realm.Write(() => _realm.Add(container));

                var eventArgs = new List<NotifyCollectionChangedEventArgs>();
                var propertyEventArgs = new List<string>();

                var collection = container.Items.AsRealmCollection();
                collection.CollectionChanged += (sender, e) => eventArgs.Add(e);
                collection.PropertyChanged += (sender, e) => propertyEventArgs.Add(e.PropertyName);

                _realm.Write(() =>
                {
                    container.Items.Clear();
                    container.Items.Add(new OrderedObject());
                });

                await Task.Delay(MillisecondsToWaitForCollectionNotification);

                Assert.That(eventArgs.Count, Is.EqualTo(1));
                Assert.That(eventArgs[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
                Assert.That(propertyEventArgs.Count, Is.EqualTo(2));
                Assert.That(propertyEventArgs, Is.EquivalentTo(new[] { "Count", "Item[]" }));
            });
        }

        [TestCase(0, 3, 1, 3, NotifyCollectionChangedAction.Reset)] // a b c d e -> b d a c e
        [TestCase(0, 3, 0, 2, NotifyCollectionChangedAction.Reset)] // a b c d e -> c d b a e
        [TestCase(0, 3, 4, 0, NotifyCollectionChangedAction.Reset)] // a b c d e -> e b c d a
        [TestCase(0, 2, 0, 2, NotifyCollectionChangedAction.Move)] // a b c d e -> c a b d e
        [TestCase(4, 2, 4, 2, NotifyCollectionChangedAction.Move)] // a b c d e -> a b d e c
        [TestCase(1, 3, 1, 3, NotifyCollectionChangedAction.Move)] // a b c d e -> c d a b e
        public void ListMove_MultipleMovedItemssTests(int oldIndex1, int newIndex1, int oldIndex2, int newIndex2, NotifyCollectionChangedAction expectedAction)
        {
            AsyncContext.Run(async delegate
            {
                OrderedObject object1 = null;
                OrderedObject object2 = null;
                var args = await TestMoves(items =>
                {
                    object1 = items[oldIndex1];
                    items.Move(object1, newIndex1);

                    object2 = items[oldIndex2];
                    items.Move(object2, newIndex2);
                }, expectedAction);

                if (expectedAction == NotifyCollectionChangedAction.Move)
                {
                    var oldStartIndex = Math.Min(oldIndex1, oldIndex2);
                    var newStartIndex = Math.Min(newIndex1, newIndex2);
                    if (oldStartIndex < newStartIndex)
                    {
                        // x was moved from before to after y, then y was moved to after x, which results in index being adjusted by -1.
                        newStartIndex--;
                    }
                    else
                    {
                        // x was moved from after to before y, then y was moved to before x, which results in index being adjusted by -1.
                        oldStartIndex--;
                    }

                    Assert.That(args.OldStartingIndex, Is.EqualTo(oldStartIndex));
                    Assert.That(args.NewStartingIndex, Is.EqualTo(newStartIndex));
                    Assert.That(args.OldItems, Is.EquivalentTo(new[] { object1, object2 }));
                    Assert.That(args.NewItems, Is.EquivalentTo(new[] { object1, object2 }));
                }
            });
        }

        [TestCase(0, 4)]
        [TestCase(4, 0)]
        [TestCase(0, 2)]
        [TestCase(2, 0)]
        [TestCase(1, 2)]
        [TestCase(2, 1)]
        public void ListMove_SingleMovedItemTests(int oldIndex, int newIndex)
        {
            AsyncContext.Run(async delegate
            {
                OrderedObject movedObject = null;
                var args = await TestMoves(items =>
                {
                    movedObject = items[oldIndex];
                    items.Move(movedObject, newIndex);
                }, NotifyCollectionChangedAction.Move);

                Assert.That(args.OldStartingIndex, Is.EqualTo(oldIndex));
                Assert.That(args.NewStartingIndex, Is.EqualTo(newIndex));
                Assert.That(args.OldItems, Is.EquivalentTo(new[] { movedObject }));
                Assert.That(args.NewItems, Is.EquivalentTo(new[] { movedObject }));
            });
        }

        // Adds 5 OrderedObject to a List, executes moveAction and returns the single change notification argument.
        private async Task<NotifyCollectionChangedEventArgs> TestMoves(Action<IList<OrderedObject>> moveAction, NotifyCollectionChangedAction expectedAction)
        {
            var container = new OrderedContainer();
            for (var i = 0; i < 5; i++)
            {
                container.Items.Add(new OrderedObject
                {
                    Order = i
                });
            }

            _realm.Write(() => _realm.Add(container));

            var eventArgs = new List<NotifyCollectionChangedEventArgs>();
            var propertyEventArgs = new List<string>();

            var collection = container.Items.AsRealmCollection();
            collection.CollectionChanged += (sender, e) => eventArgs.Add(e);
            collection.PropertyChanged += (sender, e) => propertyEventArgs.Add(e.PropertyName);

            _realm.Write(() => moveAction(container.Items));

            await Task.Delay(MillisecondsToWaitForCollectionNotification);

            Assert.That(eventArgs.Count, Is.EqualTo(1));
            Assert.That(eventArgs[0].Action, Is.EqualTo(expectedAction));
            Assert.That(propertyEventArgs.Count, Is.EqualTo(2));
            Assert.That(propertyEventArgs, Is.EquivalentTo(new[] { "Count", "Item[]" }));

            return eventArgs[0];
        }

        [TestCaseSource(nameof(CollectionChangedTestCases))]
        public void TestRealmListNotifications(int[] initial, NotifyCollectionChangedAction action, int[] change, int startIndex)
        {
            AsyncContext.Run(async delegate
            {
                var container = new OrderedContainer();
                foreach (var i in initial)
                {
                    container.Items.Add(new OrderedObject { Order = i });
                }

                _realm.Write(() => _realm.Add(container));

                Exception error = null;
                _realm.Error += (sender, e) =>
                {
                    error = e.Exception;
                };

                var collection = container.Items.AsRealmCollection();

                var eventArgs = new List<NotifyCollectionChangedEventArgs>();
                var propertyEventArgs = new List<string>();

                collection.CollectionChanged += (o, e) => eventArgs.Add(e);
                collection.PropertyChanged += (o, e) => propertyEventArgs.Add(e.PropertyName);

                Assert.That(error, Is.Null);
                _realm.Write(() =>
                {
                    if (action == NotifyCollectionChangedAction.Add)
                    {
                        foreach (var value in change)
                        {
                            container.Items.Add(new OrderedObject
                            {
                                Order = value
                            });
                        }
                    }
                    else if (action == NotifyCollectionChangedAction.Remove)
                    {
                        foreach (var value in change)
                        {
                            container.Items.Remove(_realm.All<OrderedObject>().Single(o => o.Order == value));
                        }
                    }
                });

                await Task.Delay(MillisecondsToWaitForCollectionNotification);
                Assert.That(error, Is.Null);

                Assert.That(eventArgs.Count, Is.EqualTo(1));
                var arg = eventArgs[0];
                if (action == NotifyCollectionChangedAction.Add)
                {
                    Assert.That(arg.Action == action);
                    Assert.That(arg.NewStartingIndex, Is.EqualTo(initial.Length));
                    Assert.That(arg.NewItems.Cast<OrderedObject>().Select(o => o.Order), Is.EquivalentTo(change));
                }
                else if (action == NotifyCollectionChangedAction.Remove)
                {
                    if (startIndex < 0)
                    {
                        Assert.That(arg.Action == NotifyCollectionChangedAction.Reset);
                    }
                    else
                    {
                        Assert.That(arg.Action == action);
                        Assert.That(arg.OldStartingIndex, Is.EqualTo(startIndex));
                        Assert.That(arg.OldItems.Count, Is.EqualTo(change.Length));
                    }
                }

                Assert.That(propertyEventArgs.Count, Is.EqualTo(2));
                Assert.That(propertyEventArgs, Is.EquivalentTo(new[] { "Count", "Item[]" }));
            });
        }

        [TestCaseSource(nameof(CollectionChangedTestCases))]
        public void TestCollectionChangedAdapter(int[] initial, NotifyCollectionChangedAction action, int[] change, int startIndex)
        {
            AsyncContext.Run(async delegate
            {
                _realm.Write(() =>
                {
                    foreach (var value in initial)
                    {
                        _realm.Add(new OrderedObject
                        {
                            Order = value,
                            IsPartOfResults = true
                        });
                    }
                });

                Exception error = null;
                _realm.Error += (sender, e) =>
                {
                    error = e.Exception;
                };

                var query = _realm.All<OrderedObject>().Where(o => o.IsPartOfResults).OrderBy(o => o.Order).AsRealmCollection();

                // wait for the initial notification to come through
                await Task.Yield();

                var eventArgs = new List<NotifyCollectionChangedEventArgs>();
                var propertyEventArgs = new List<string>();

                query.CollectionChanged += (o, e) => eventArgs.Add(e);
                query.PropertyChanged += (o, e) => propertyEventArgs.Add(e.PropertyName);

                Assert.That(error, Is.Null);
                _realm.Write(() =>
                {
                    if (action == NotifyCollectionChangedAction.Add)
                    {
                        foreach (var value in change)
                        {
                            _realm.Add(new OrderedObject
                            {
                                Order = value,
                                IsPartOfResults = true
                            });
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

                await Task.Delay(MillisecondsToWaitForCollectionNotification);
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

                Assert.That(propertyEventArgs.Count, Is.EqualTo(2));
                Assert.That(propertyEventArgs, Is.EquivalentTo(new[] { "Count", "Item[]" }));
            });
        }

        public static IEnumerable<TestCaseData> CollectionChangedTestCases()
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

            // When we have non-consecutive adds/removes, we should raise Reset, indicated by -1 here.
            yield return new TestCaseData(new int[] { 1, 3, 5 }, NotifyCollectionChangedAction.Add, new int[] { 2, 4 }, -1);
            yield return new TestCaseData(new int[] { 1, 2, 3, 4, 5 }, NotifyCollectionChangedAction.Remove, new int[] { 2, 4 }, -1);
        }
    }
}