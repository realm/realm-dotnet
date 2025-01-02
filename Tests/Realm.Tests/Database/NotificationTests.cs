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
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Realms.Logging;
using static Realms.ChangeSet;
#if TEST_WEAVER
using TestRealmObject = Realms.RealmObject;
#else
using TestRealmObject = Realms.IRealmObject;
#endif

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class NotificationTests : RealmInstanceTest
    {
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
            var logger = new RealmLogger.InMemoryLogger();
            RealmLogger.Default = logger;
            _realm.NotifyError(new Exception());

            Assert.That(logger.GetLog(), Does.Contain("exception").And.Contains("Realm.Error"));
        }

        [Test]
        public void ResultsShouldSendNotifications()
        {
            var query = _realm.All<Person>();
            ChangeSet? changes = null;
            void OnNotification(IRealmCollection<Person> s, ChangeSet? c) => changes = c;

            using (query.SubscribeForNotifications(OnNotification))
            {
                _realm.Write(() => _realm.Add(new Person()));

                _realm.Refresh();
                Assert.That(changes, Is.Not.Null);
                Assert.That(changes!.InsertedIndices, Is.EquivalentTo(new int[] { 0 }));
            }
        }

        [Test]
        public void Results_WhenEmbeddedObjectIsModified_Notifies()
        {
            var query = _realm.All<TestNotificationObject>();
            var actualChanges = new List<ChangeSet?>();
            void OnNotification(IRealmCollection<TestNotificationObject> collection, ChangeSet? changes) => actualChanges.Add(changes);

            Assert.That(query.Count, Is.EqualTo(0));

            using (query.SubscribeForNotifications(OnNotification))
            {
                _realm.Refresh();

                // Notification from subscribing.
                Assert.That(actualChanges.Count, Is.EqualTo(1));

                var testObject = _realm.Write(() => _realm.Add(new TestNotificationObject()));

                _realm.Refresh();
                Assert.That(actualChanges.Count, Is.EqualTo(2));
                Assert.That(actualChanges[1], Is.Not.Null);
                Assert.That(actualChanges[1]!.InsertedIndices, Is.EquivalentTo(new[] { 0 }));

                _realm.Write(() =>
                {
                    testObject.EmbeddedObject = new EmbeddedIntPropertyObject { Int = 1 };
                });

                _realm.Refresh();
                Assert.That(actualChanges.Count, Is.EqualTo(3));
                Assert.That(actualChanges[2], Is.Not.Null);
                Assert.That(actualChanges[2]!.NewModifiedIndices, Is.EquivalentTo(new[] { 0 }));
                Assert.That(actualChanges[2]!.ModifiedIndices, Is.EquivalentTo(new[] { 0 }));

                _realm.Write(() =>
                {
                    testObject.EmbeddedObject!.Int++;
                });

                _realm.Refresh();
                Assert.That(actualChanges.Count, Is.EqualTo(4));
                Assert.That(actualChanges[3], Is.Not.Null);
                Assert.That(actualChanges[3]!.NewModifiedIndices, Is.EquivalentTo(new[] { 0 }));
                Assert.That(actualChanges[3]!.ModifiedIndices, Is.EquivalentTo(new[] { 0 }));
            }
        }

        [Test]
        public void ListShouldSendNotifications()
        {
            var container = new OrderedContainer();
            _realm.Write(() => _realm.Add(container));
            ChangeSet? changes = null;
            void OnNotification(IRealmCollection<OrderedObject> s, ChangeSet? c) => changes = c;

            using (container.Items.SubscribeForNotifications(OnNotification))
            {
                _realm.Write(() => container.Items.Add(new OrderedObject()));

                _realm.Refresh();
                Assert.That(changes, Is.Not.Null);
                Assert.That(changes!.InsertedIndices, Is.EquivalentTo(new int[] { 0 }));
            }
        }

        [Test]
        public void UnsubscribeInNotificationCallback()
        {
            var query = _realm.All<Person>();
            IDisposable notificationToken = null!;

            var notificationCount = 0;
            notificationToken = query.SubscribeForNotifications(delegate
            {
                notificationCount++;
                notificationToken.Dispose();
            });

            for (var i = 0; i < 2; i++)
            {
                _realm.Write(() => _realm.Add(new Person()));
                _realm.Refresh();
                Assert.That(notificationCount, Is.EqualTo(1));
            }
        }

        [Test]
        public void DictionaryUnsubscribeInNotificationCallback()
        {
            var container = _realm.Write(() =>
            {
                return _realm.Add(new OrderedContainer());
            });

            IDisposable notificationToken = null!;

            var notificationCount = 0;
            notificationToken = container.ItemsDictionary.SubscribeForKeyNotifications(delegate
            {
                notificationCount++;
                notificationToken.Dispose();
            });

            for (var i = 0; i < 2; i++)
            {
                _realm.Write(() => container.ItemsDictionary.Add(Guid.NewGuid().ToString(), new OrderedObject()));
                _realm.Refresh();
                Assert.That(notificationCount, Is.EqualTo(1));
            }
        }

        [Test]
        public void DictionarySubscribeInTransaction()
        {
            var notificationsCount = 0;
            var (token, container) = _realm.Write(() =>
            {
                var container = _realm.Add(new OrderedContainer());
                var token = container.ItemsDictionary.SubscribeForKeyNotifications((dict, changes) =>
                {
                    notificationsCount++;
                });

                return (token, container);
            });

            _realm.Refresh();
            Assert.That(notificationsCount, Is.EqualTo(1));

            _realm.Write(() =>
            {
                container.ItemsDictionary.Add(Guid.NewGuid().ToString(), new OrderedObject());
            });

            _realm.Refresh();
            Assert.That(notificationsCount, Is.EqualTo(2));
        }

        [Test]
        public void DictionaryNotificationToken_KeepsCollectionAlive()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                await TestHelpers.EnsurePreserverKeepsObjectAlive(() =>
                {
                    var dictionary = _realm.Write(() =>
                    {
                        return _realm.Add(new OrderedContainer()).ItemsDictionary;
                    });

                    var dictReference = new WeakReference(dictionary);
                    var token = dictionary.SubscribeForKeyNotifications(delegate { });

                    return (token, dictReference);
                });
            });
        }

        [Test]
        public void PrimitivePropertyInObjectShouldFireNotificationOnChange()
        {
            var testObject = new TestNotificationObject();
            _realm.Write(() => _realm.Add(testObject));
            var notificationCount = 0;
            testObject.PropertyChanged += (sender, e) =>
            {
                notificationCount++;
            };
            _realm.Write(() =>
            {
                testObject.StringProperty = "foo";
            });
            _realm.Refresh();
            Assert.That(notificationCount, Is.EqualTo(1));
        }

        [Test]
        public void BackLinkInObjectShouldNotFireNotificationOnChange()
        {
            var testObject = new TestNotificationObject();
            var targetObject = new TestNotificationObject();
            _realm.Write(() =>
            {
                _realm.Add(testObject);
                _realm.Add(targetObject);
            });
            var notificationCount = 0;
            testObject.PropertyChanged += (sender, e) =>
            {
                notificationCount++;
            };
            Assert.That(testObject.BacklinksCount, Is.EqualTo(0));
            _realm.Write(() =>
            {
                targetObject.LinkSameType = testObject;
            });
            _realm.Refresh();
            Assert.That(notificationCount, Is.EqualTo(0));
            Assert.That(testObject.BacklinksCount, Is.EqualTo(1));
            _realm.Write(() =>
            {
                targetObject.StringProperty = "foo";
            });
            _realm.Refresh();
            Assert.That(notificationCount, Is.EqualTo(0));
            Assert.That(testObject.BacklinksCount, Is.EqualTo(1));
        }

        [Test]
        public void CollectionPropertiesOfDifferentTypeShouldNotFireNotificationsOnChange()
        {
            var testObject = _realm.Write(() => _realm.Add(new TestNotificationObject()));
            var propertyEventArgs = new List<string>();

            testObject.PropertyChanged += (sender, e) =>
            {
                propertyEventArgs.Add(e.PropertyName!);
            };

            var person = new Person();

            // Insertions
            _realm.Write(() =>
            {
                testObject.ListDifferentType.Add(person);
                testObject.SetDifferentType.Add(person);
                testObject.DictionaryDifferentType.Add("foo", person);
            });
            _realm.Refresh();
            Assert.That(propertyEventArgs.Count, Is.EqualTo(0));

            // Update primitive property while inserting
            _realm.Write(() =>
            {
                testObject.StringProperty = "foo";
                testObject.ListDifferentType.Add(person);
            });
            _realm.Refresh();
            Assert.That(propertyEventArgs.Count, Is.EqualTo(1));
            Assert.That(propertyEventArgs, Is.EquivalentTo(new[] { "StringProperty" }));
            propertyEventArgs.Clear();

            // Modifications
            _realm.Write(() =>
            {
                person.Nickname = "foo1";
            });
            _realm.Refresh();
            Assert.That(propertyEventArgs.Count, Is.EqualTo(0));

            // Moving
            _realm.Write(() =>
            {
                testObject.ListDifferentType.Move(0, 1);
            });
            _realm.Refresh();
            Assert.That(propertyEventArgs.Count, Is.EqualTo(0));

            // Removing
            _realm.Write(() =>
            {
                testObject.DictionaryDifferentType.Remove("foo");
                _realm.Remove(person);
            });
            _realm.Refresh();
            Assert.That(propertyEventArgs.Count, Is.EqualTo(0));
        }

        [Test]
        public void CollectionPropertiesOfSameTypeShouldNotFireNotificationsOnChange()
        {
            var testObject = _realm.Write(() => _realm.Add(new TestNotificationObject()));
            var propertyEventArgs = new List<string>();
            testObject.PropertyChanged += (sender, e) =>
            {
                propertyEventArgs.Add(e.PropertyName!);
            };

            var targetObject = new TestNotificationObject();

            // Insertions
            _realm.Write(() =>
            {
                testObject.ListSameType.Add(targetObject);
                testObject.SetSameType.Add(targetObject);
                testObject.DictionarySameType.Add("foo", targetObject);
            });
            _realm.Refresh();
            Assert.That(propertyEventArgs.Count, Is.EqualTo(0));

            // Update primitive property while inserting
            _realm.Write(() =>
            {
                testObject.StringProperty = "foo";
                testObject.ListSameType.Add(targetObject);
            });
            _realm.Refresh();
            Assert.That(propertyEventArgs.Count, Is.EqualTo(1));
            Assert.That(propertyEventArgs, Is.EquivalentTo(new[] { "StringProperty" }));
            propertyEventArgs.Clear();

            // Modifications
            _realm.Write(() =>
            {
                targetObject.StringProperty = "foo1";
            });
            _realm.Refresh();
            Assert.That(propertyEventArgs.Count, Is.EqualTo(0));

            // Moving
            _realm.Write(() =>
            {
                testObject.ListSameType.Move(0, 1);
            });
            _realm.Refresh();
            Assert.That(propertyEventArgs.Count, Is.EqualTo(0));

            // Removing
            _realm.Write(() =>
            {
                testObject.DictionarySameType.Remove("foo");
                _realm.Remove(targetObject);
            });
            _realm.Refresh();
            Assert.That(propertyEventArgs.Count, Is.EqualTo(0));
        }

        [Test]
        public void Link_ShouldOnlyFireNotificationForReassignment()
        {
            var testObject = _realm.Write(() => _realm.Add(new TestNotificationObject
            {
                LinkSameType = new TestNotificationObject(),
                LinkDifferentType = new Person()
            }));

            var propertyEventArgs = new List<string>();
            testObject.PropertyChanged += (sender, e) =>
            {
                propertyEventArgs.Add(e.PropertyName!);
            };

            _realm.Write(() =>
            {
                // Should should only fire for 'testObject.StringProperty = "foo"' since this isn't a nested property.
                testObject.StringProperty = "foo";
                testObject.LinkSameType!.StringProperty = "bar";
                testObject.LinkDifferentType!.Nickname = "foobar";
            });
            _realm.Refresh();
            Assert.That(propertyEventArgs.Count, Is.EqualTo(1));
            Assert.That(propertyEventArgs, Is.EquivalentTo(new[] { "StringProperty" }));
            propertyEventArgs.Clear();

            _realm.Write(() =>
            {
                testObject.LinkSameType = new TestNotificationObject();
                testObject.LinkDifferentType = new Person();
            });
            _realm.Refresh();
            Assert.That(propertyEventArgs.Count, Is.EqualTo(2));
            Assert.That(propertyEventArgs, Is.EquivalentTo(new[] { "LinkDifferentType", "LinkSameType" }));
        }

        [Test]
        public void ListOnCollectionChangedShouldFireOnAddMoveReplaceRemove()
        {
            var testObject = _realm.Write(() => _realm.Add(new TestNotificationObject()));
            var eventArgs = new List<NotifyCollectionChangedEventArgs>();
            testObject.ListSameType.AsRealmCollection().CollectionChanged += (sender, e) => eventArgs.Add(e);

            var testObject1 = new TestNotificationObject();
            var testObject2 = new TestNotificationObject();
            var testObject3 = new TestNotificationObject();

            // Insertions
            _realm.Write(() =>
            {
                testObject.ListSameType.Add(testObject1);
                testObject.ListSameType.Add(testObject2);
                testObject.ListSameType.Add(testObject3);
            });
            _realm.Refresh();
            Assert.That(eventArgs.Count, Is.EqualTo(1));
            Assert.That(eventArgs[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
            Assert.That(eventArgs[0].NewItems!.Count, Is.EqualTo(3));
            eventArgs.Clear();

            // Modifications
            _realm.Write(() =>
            {
                testObject1.StringProperty = "foo1";
                testObject2.StringProperty = "foo2";
                testObject3.StringProperty = "foo3";
            });
            _realm.Refresh();
            Assert.That(eventArgs, Is.Empty);

            // Moving
            _realm.Write(() =>
            {
                testObject.ListSameType.Move(0, 2);
            });
            _realm.Refresh();
            Assert.That(eventArgs.Count, Is.EqualTo(1));
            Assert.That(eventArgs[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Move));
            Assert.That(eventArgs[0].OldStartingIndex, Is.EqualTo(0));
            Assert.That(eventArgs[0].NewStartingIndex, Is.EqualTo(2));
            eventArgs.Clear();

            // Deletion
            _realm.Write(() =>
            {
                testObject.ListSameType.Remove(testObject1);
                testObject.ListSameType.Remove(testObject2);
                testObject.ListSameType.Remove(testObject3);
            });
            _realm.Refresh();
            Assert.That(eventArgs.Count, Is.EqualTo(1));
            Assert.That(eventArgs[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));
            Assert.That(eventArgs[0].OldItems!.Count, Is.EqualTo(3));
        }

        [Test]
        public void ResultOnCollectionChangedShouldFireOnAddRemove()
        {
            var testObject = _realm.Write(() => _realm.Add(new TestNotificationObject()));
            var eventArgs = new List<NotifyCollectionChangedEventArgs>();
            var eventArgsForFilter = new List<NotifyCollectionChangedEventArgs>();

            var query = _realm.All<TestNotificationObject>().AsRealmCollection();
            var queryWithFilter = _realm.All<TestNotificationObject>().Where(t => t.StringProperty!.Contains("f")).AsRealmCollection();

            query.CollectionChanged += (sender, e) => eventArgs.Add(e);
            queryWithFilter.CollectionChanged += (sender, e) => eventArgsForFilter.Add(e);

            var testObject1 = new TestNotificationObject();
            var testObject2 = new TestNotificationObject();
            var testObject3 = new TestNotificationObject();

            // Insertions
            _realm.Write(() =>
            {
                _realm.Add(testObject1);
                _realm.Add(testObject2);
                _realm.Add(testObject3);
            });
            _realm.Refresh();
            Assert.That(eventArgs.Count, Is.EqualTo(1));
            Assert.That(eventArgs[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
            Assert.That(eventArgs[0].NewItems!.Count, Is.EqualTo(3));
            eventArgs.Clear();

            // Modifications
            // this will be treated as adds for eventArgsForFilter since they now they satisfy the result filter and are added.
            _realm.Write(() =>
            {
                testObject1.StringProperty = "foo3";
                testObject2.StringProperty = "foo1";
                testObject3.StringProperty = "foo2";
            });
            _realm.Refresh();
            Assert.That(eventArgs, Is.Empty);
            Assert.That(eventArgsForFilter.Count, Is.EqualTo(1));
            Assert.That(eventArgsForFilter[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
            eventArgsForFilter.Clear();

            // Modifications2
            _realm.Write(() =>
            {
                testObject1.StringProperty = "foo1";
                testObject2.StringProperty = "foo2";
                testObject3.StringProperty = "foo3";
            });
            _realm.Refresh();
            Assert.That(eventArgsForFilter, Is.Empty);

            // Deletion
            _realm.Write(() =>
            {
                _realm.Remove(testObject1);
                _realm.Remove(testObject2);
                _realm.Remove(testObject3);
            });
            _realm.Refresh();
            Assert.That(eventArgs.Count, Is.EqualTo(1));
            Assert.That(eventArgs[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));
            Assert.That(eventArgs[0].OldItems!.Count, Is.EqualTo(3));
        }

        [Test]
        public void DictionaryOnCollectionChangedShouldFireOnAddRemove()
        {
            var testObject = _realm.Write(() => _realm.Add(new TestNotificationObject()));
            var eventArgs = new List<NotifyCollectionChangedEventArgs>();
            testObject.DictionarySameType.AsRealmCollection().CollectionChanged += (sender, e) => eventArgs.Add(e);

            var targetObject = new TestNotificationObject();

            // Insertions
            _realm.Write(() =>
            {
                testObject.DictionarySameType.Add("1", targetObject);
            });

            _realm.Refresh();
            Assert.That(eventArgs.Count, Is.EqualTo(1));
            Assert.That(eventArgs[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
            Assert.That(eventArgs[0].NewItems!.Count, Is.EqualTo(1));
            eventArgs.Clear();

            // Modifications
            _realm.Write(() =>
            {
                testObject.DictionarySameType["1"]!.StringProperty = "foo1";
            });
            _realm.Refresh();
            Assert.That(eventArgs, Is.Empty);

            // Deletion
            _realm.Write(() =>
            {
                testObject.DictionarySameType.Remove("1");
            });
            _realm.Refresh();
            Assert.That(eventArgs.Count, Is.EqualTo(1));
            Assert.That(eventArgs[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));
            Assert.That(eventArgs[0].OldItems!.Count, Is.EqualTo(1));
        }

        [Test]
        public void SetOnCollectionChangedShouldFireOnAddRemove()
        {
            var testObject = _realm.Write(() => _realm.Add(new TestNotificationObject()));
            var eventArgs = new List<NotifyCollectionChangedEventArgs>();
            testObject.SetSameType.AsRealmCollection().CollectionChanged += (sender, e) => eventArgs.Add(e);

            var targetObject = new TestNotificationObject();

            // Insertions
            _realm.Write(() =>
            {
                testObject.SetSameType.Add(targetObject);
            });
            _realm.Refresh();
            Assert.That(eventArgs.Count, Is.EqualTo(1));
            Assert.That(eventArgs[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
            Assert.That(eventArgs[0].NewItems!.Count, Is.EqualTo(1));
            eventArgs.Clear();

            // Modifications
            _realm.Write(() =>
            {
                targetObject.StringProperty = "foo1";
            });
            _realm.Refresh();
            Assert.That(eventArgs, Is.Empty);

            // Deletion
            _realm.Write(() =>
            {
                testObject.SetSameType.Remove(targetObject);
            });
            _realm.Refresh();
            Assert.That(eventArgs.Count, Is.EqualTo(1));
            Assert.That(eventArgs[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));
            Assert.That(eventArgs[0].OldItems!.Count, Is.EqualTo(1));
        }

        [Test]
        public void Results_WhenUnsubscribed_ShouldStopReceivingNotifications()
        {
            _realm.Write(() =>
            {
                _realm.Add(new OrderedObject
                {
                    Order = 0,
                    IsPartOfResults = true
                });
            });

            Exception? error = null;
            _realm.Error += (sender, e) =>
            {
                error = e.Exception;
            };

            var query = _realm.All<OrderedObject>().Where(o => o.IsPartOfResults).OrderBy(o => o.Order).AsRealmCollection();

            var eventArgs = new List<NotifyCollectionChangedEventArgs>();
            var handler = new NotifyCollectionChangedEventHandler((sender, e) => eventArgs.Add(e));

            var propertyEventArgs = new List<string>();
            var propertyHandler = new PropertyChangedEventHandler((sender, e) => propertyEventArgs.Add(e.PropertyName!));

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

            _realm.Refresh();

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

            _realm.Refresh();

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

            _realm.Refresh();

            Assert.That(error, Is.Null);
            Assert.That(eventArgs.Count, Is.EqualTo(2));
            Assert.That(eventArgs.All(e => e.Action == NotifyCollectionChangedAction.Add));
            Assert.That(propertyEventArgs.Count, Is.EqualTo(4));
            Assert.That(propertyEventArgs, Is.EquivalentTo(new[] { "Count", "Item[]", "Count", "Item[]" }));
        }

        [Test]
        public void Results_WhenTransactionHasBothAddAndRemove_ShouldReset()
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

            Exception? error = null;
            _realm.Error += (sender, e) =>
            {
                error = e.Exception;
            };

            var query = _realm.All<OrderedObject>().Where(o => o.IsPartOfResults).OrderBy(o => o.Order).AsRealmCollection();

            var eventArgs = new List<NotifyCollectionChangedEventArgs>();
            query.CollectionChanged += (sender, e) => eventArgs.Add(e);

            var propertyEventArgs = new List<string>();
            query.PropertyChanged += (sender, e) => propertyEventArgs.Add(e.PropertyName!);

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

            _realm.Refresh();

            Assert.That(error, Is.Null);
            Assert.That(eventArgs.Count, Is.EqualTo(1));
            Assert.That(eventArgs[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
            Assert.That(propertyEventArgs.Count, Is.EqualTo(2));
            Assert.That(propertyEventArgs, Is.EquivalentTo(new[] { "Count", "Item[]" }));
        }

        [Test]
        public void List_WhenUnsubscribed_ShouldStopReceivingNotifications()
        {
            var container = new OrderedContainer();
            _realm.Write(() => _realm.Add(container));

            var eventArgs = new List<NotifyCollectionChangedEventArgs>();
            var handler = new NotifyCollectionChangedEventHandler((sender, e) =>
            {
                eventArgs.Add(e);
            });

            var propertyEventArgs = new List<string>();
            var propertyHandler = new PropertyChangedEventHandler((sender, e) => propertyEventArgs.Add(e.PropertyName!));

            var collection = container.Items.AsRealmCollection();
            collection.CollectionChanged += handler;
            collection.PropertyChanged += propertyHandler;

            _realm.Write(() =>
            {
                container.Items.Add(new OrderedObject());
            });

            _realm.Refresh();

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

            _realm.Refresh();

            Assert.That(eventArgs.Count, Is.EqualTo(1));
            Assert.That(eventArgs[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
            Assert.That(propertyEventArgs.Count, Is.EqualTo(2));
            Assert.That(propertyEventArgs, Is.EquivalentTo(new[] { "Count", "Item[]" }));
        }

        [Test]
        public void List_WhenTransactionHasBothAddAndRemove_ShouldReset()
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
            collection.PropertyChanged += (sender, e) => propertyEventArgs.Add(e.PropertyName!);

            _realm.Write(() =>
            {
                container.Items.Clear();
                container.Items.Add(new OrderedObject());
            });

            _realm.Refresh();

            Assert.That(eventArgs.Count, Is.EqualTo(1));
            Assert.That(eventArgs[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
            Assert.That(propertyEventArgs.Count, Is.EqualTo(2));
            Assert.That(propertyEventArgs, Is.EquivalentTo(new[] { "Count", "Item[]" }));
        }

        [TestCase(0, 3, 1, 3, NotifyCollectionChangedAction.Reset)] // a b c d e -> b d a c e
        [TestCase(0, 3, 0, 2, NotifyCollectionChangedAction.Reset)] // a b c d e -> c d b a e
        [TestCase(0, 3, 4, 0, NotifyCollectionChangedAction.Reset)] // a b c d e -> e b c d a
        [TestCase(0, 2, 0, 2, NotifyCollectionChangedAction.Move)] // a b c d e -> c a b d e
        [TestCase(4, 2, 4, 2, NotifyCollectionChangedAction.Move)] // a b c d e -> a b d e c
        [TestCase(1, 3, 1, 3, NotifyCollectionChangedAction.Move)] // a b c d e -> c d a b e
        public void ListMove_MultipleMovedItemssTests(int oldIndex1, int newIndex1, int oldIndex2, int newIndex2, NotifyCollectionChangedAction expectedAction)
        {
            OrderedObject object1 = null!;
            OrderedObject object2 = null!;
            var args = TestMoves(items =>
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
        }

        [TestCase(0, 4)]
        [TestCase(4, 0)]
        [TestCase(0, 2)]
        [TestCase(2, 0)]
        [TestCase(1, 2)]
        [TestCase(2, 1)]
        public void ListMove_SingleMovedItemTests(int oldIndex, int newIndex)
        {
            OrderedObject movedObject = null!;
            var args = TestMoves(items =>
            {
                movedObject = items[oldIndex];
                items.Move(movedObject, newIndex);
            }, NotifyCollectionChangedAction.Move);

            Assert.That(args.OldStartingIndex, Is.EqualTo(oldIndex));
            Assert.That(args.NewStartingIndex, Is.EqualTo(newIndex));
            Assert.That(args.OldItems, Is.EquivalentTo(new[] { movedObject }));
            Assert.That(args.NewItems, Is.EquivalentTo(new[] { movedObject }));
        }

        [Test]
        public void ListReplace_RaisesReplaceNotifications()
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
            collection.PropertyChanged += (sender, e) => propertyEventArgs.Add(e.PropertyName!);

            var oldItem = container.Items[1];
            _realm.Write(() => container.Items[1] = container.Items[4]);

            _realm.Refresh();

            Assert.That(eventArgs.Count, Is.EqualTo(1));
            Assert.That(eventArgs[0].Action, Is.EqualTo(NotifyCollectionChangedAction.Replace));
            Assert.That(eventArgs[0].OldStartingIndex, Is.EqualTo(1));
            Assert.That(eventArgs[0].NewStartingIndex, Is.EqualTo(1));
            Assert.That(eventArgs[0].OldItems, Is.EquivalentTo(new[] { InvalidObject.Instance }));
            Assert.That(eventArgs[0].NewItems, Is.EquivalentTo(new[] { container.Items[4] }));
            Assert.That(propertyEventArgs.Count, Is.EqualTo(2));
            Assert.That(propertyEventArgs, Is.EquivalentTo(new[] { "Count", "Item[]" }));

            // Verify that modifying an object doesn't raise notifications
            _realm.Write(() => container.Items[2].Order = 999);
            _realm.Refresh();

            // No notifications should have arrived
            Assert.That(eventArgs.Count, Is.EqualTo(1));
            Assert.That(propertyEventArgs.Count, Is.EqualTo(2));
        }

        // Adds 5 OrderedObject to a List, executes moveAction and returns the single change notification argument.
        private NotifyCollectionChangedEventArgs TestMoves(Action<IList<OrderedObject>> moveAction, NotifyCollectionChangedAction expectedAction)
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
            collection.PropertyChanged += (sender, e) => propertyEventArgs.Add(e.PropertyName!);

            _realm.Write(() => moveAction(container.Items));

            _realm.Refresh();

            Assert.That(eventArgs.Count, Is.EqualTo(1));
            Assert.That(eventArgs[0].Action, Is.EqualTo(expectedAction));
            Assert.That(propertyEventArgs.Count, Is.EqualTo(2));
            Assert.That(propertyEventArgs, Is.EquivalentTo(new[] { "Count", "Item[]" }));

            return eventArgs[0];
        }

        [TestCaseSource(nameof(CollectionChangedTestCases))]
        public void TestRealmListNotifications(int[] initial, NotifyCollectionChangedAction action, int[] change, int startIndex)
        {
            var container = new OrderedContainer();
            foreach (var i in initial)
            {
                container.Items.Add(new OrderedObject { Order = i });
            }

            _realm.Write(() => _realm.Add(container));

            Exception? error = null;
            _realm.Error += (sender, e) =>
            {
                error = e.Exception;
            };

            var collection = container.Items.AsRealmCollection();

            var eventArgs = new List<NotifyCollectionChangedEventArgs>();
            var propertyEventArgs = new List<string>();

            collection.CollectionChanged += (o, e) => eventArgs.Add(e);
            collection.PropertyChanged += (o, e) => propertyEventArgs.Add(e.PropertyName!);

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
                else if (action == NotifyCollectionChangedAction.Reset)
                {
                    container.Items.Clear();
                }
            });

            _realm.Refresh();
            Assert.That(error, Is.Null);

            Assert.That(eventArgs.Count, Is.EqualTo(1));
            var arg = eventArgs[0];
            if (action == NotifyCollectionChangedAction.Add)
            {
                Assert.That(arg.Action == action);
                Assert.That(arg.NewStartingIndex, Is.EqualTo(initial.Length));
                Assert.That(arg.NewItems!.Cast<OrderedObject>().Select(o => o.Order), Is.EquivalentTo(change));
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
                    Assert.That(arg.OldItems!.Count, Is.EqualTo(change.Length));
                }
            }
            else if (action == NotifyCollectionChangedAction.Reset)
            {
                Assert.That(arg.Action == NotifyCollectionChangedAction.Reset);
            }

            Assert.That(propertyEventArgs.Count, Is.EqualTo(2));
            Assert.That(propertyEventArgs, Is.EquivalentTo(new[] { "Count", "Item[]" }));
        }

        [TestCaseSource(nameof(CollectionChangedTestCases))]
        public void TestCollectionChangedAdapter(int[] initial, NotifyCollectionChangedAction action, int[] change, int startIndex)
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

            Exception? error = null;
            _realm.Error += (sender, e) =>
            {
                error = e.Exception;
            };

            var query = _realm.All<OrderedObject>().Where(o => o.IsPartOfResults).OrderBy(o => o.Order).AsRealmCollection();

            var eventArgs = new List<NotifyCollectionChangedEventArgs>();
            var propertyEventArgs = new List<string>();

            query.CollectionChanged += (o, e) => eventArgs.Add(e);
            query.PropertyChanged += (o, e) => propertyEventArgs.Add(e.PropertyName!);

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
                else if (action == NotifyCollectionChangedAction.Reset)
                {
                    _realm.RemoveAll<OrderedObject>();
                }
            });

            _realm.Refresh();
            Assert.That(error, Is.Null);

            Assert.That(eventArgs.Count, Is.EqualTo(1));
            var arg = eventArgs[0];
            if (startIndex < 0)
            {
                Assert.That(arg.Action == NotifyCollectionChangedAction.Reset);
            }
            else
            {
                if (action == NotifyCollectionChangedAction.Reset)
                {
                    Assert.That(arg.Action == NotifyCollectionChangedAction.Remove);
                }
                else
                {
                    Assert.That(arg.Action == action);
                }

                if (action == NotifyCollectionChangedAction.Add)
                {
                    Assert.That(arg.NewStartingIndex, Is.EqualTo(startIndex));
                    Assert.That(arg.NewItems!.Cast<OrderedObject>().Select(o => o.Order), Is.EquivalentTo(change));
                }
                else if (action == NotifyCollectionChangedAction.Remove || action == NotifyCollectionChangedAction.Reset)
                {
                    Assert.That(arg.OldStartingIndex, Is.EqualTo(startIndex));
                    Assert.That(arg.OldItems!.Count, Is.EqualTo(change.Length));
                }
            }

            Assert.That(propertyEventArgs.Count, Is.EqualTo(2));
            Assert.That(propertyEventArgs, Is.EquivalentTo(new[] { "Count", "Item[]" }));
        }

        [Test]
        public void WhenSynchronizationContextExists_ShouldAutoRefresh()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var tcs = new TaskCompletionSource<ChangeSet>();
                var query = _realm.All<Person>();
                void OnNotification(IRealmCollection<Person> s, ChangeSet? c)
                {
                    if (c != null)
                    {
                        tcs.TrySetResult(c);
                    }
                }

                using (query.SubscribeForNotifications(OnNotification))
                {
                    _realm.Write(() => _realm.Add(new Person()));

                    var changes = await tcs.Task.Timeout(2000);

                    Assert.That(changes, Is.Not.Null);
                    Assert.That(changes.InsertedIndices, Is.EquivalentTo(new int[] { 0 }));
                }
            });
        }

        [Test(Description = "A test to verify https://github.com/realm/realm-dotnet/issues/1689 is fixed")]
        public void SubscribeForNotifications_InvokedWithInitialCallback()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var initCalls = 0;
                var updateCalls = 0;
                void OnNotification(IRealmCollection<Person> _, ChangeSet? changes)
                {
                    if (changes == null)
                    {
                        ++initCalls;
                    }
                    else
                    {
                        ++updateCalls;
                    }
                }

                var d = new Person();
                _realm.Write(() => _realm.Add(d));

                using (d.Friends.SubscribeForNotifications(OnNotification))
                using (d.Friends.SubscribeForNotifications(OnNotification))
                {
                    await Task.Delay(100);
                    Assert.That(initCalls, Is.EqualTo(2));

                    using (d.Friends.SubscribeForNotifications(OnNotification))
                    {
                        await Task.Delay(100);
                        Assert.That(initCalls, Is.EqualTo(3));

                        _realm.Write(() =>
                        {
                            d.Friends.Add(d); // trigger the subscriptions..
                        });
                        await Task.Delay(100);
                        Assert.That(updateCalls, Is.EqualTo(3));
                    }
                }
            });
        }

        [Test]
        public void ModifiedIndices_ReportCorrectlyForOldAndNewVersions()
        {
            ChangeSet? changes = null;
            void cb(IRealmCollection<IntPrimaryKeyWithValueObject> s, ChangeSet? c) => changes = c;

            var toDelete = new IntPrimaryKeyWithValueObject { Id = 1 };
            var toModify = new IntPrimaryKeyWithValueObject { Id = 2 };

            _realm.Write(() =>
            {
                _realm.Add(toDelete);
                _realm.Add(toModify);
            });

            var query = _realm.All<IntPrimaryKeyWithValueObject>().OrderBy(i => i.Id);
            using (query.SubscribeForNotifications(cb))
            {
                Assert.That(query.ElementAt(0).Equals(toDelete));
                Assert.That(query.ElementAt(1).Equals(toModify));

                _realm.Write(() =>
                {
                    _realm.Remove(toDelete);
                    toModify.StringValue = "newValue";
                });

                _realm.Refresh();
                Assert.That(changes, Is.Not.Null);
                Assert.That(changes!.DeletedIndices, Is.EquivalentTo(new int[] { 0 }));

                // Modified should be in the old collection
                Assert.That(changes.ModifiedIndices, Is.EquivalentTo(new int[] { 1 }));

                // NewModified should be in the new collection that is just 1 element
                Assert.That(changes.NewModifiedIndices, Is.EquivalentTo(new int[] { 0 }));
                Assert.That(query.ElementAt(changes.NewModifiedIndices[0]).Equals(toModify));
            }
        }

        [Test(Description = "A test to verify https://github.com/realm/realm-dotnet/issues/1971 is fixed")]
        public void List_WhenParentIsDeleted_RaisesReset()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var person = new Person();
                _realm.Write(() => _realm.Add(person));

                var eventHelper = new EventHelper<IRealmCollection<Person>, NotifyCollectionChangedEventArgs?>();

                person.Friends.AsRealmCollection().CollectionChanged += eventHelper.OnEvent;

                _realm.Write(() =>
                {
                    person.Friends.Add(new Person());
                });

                var changeEvent = await eventHelper.GetNextEventAsync();

                Assert.That(changeEvent.Args!.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
                Assert.That(changeEvent.Args.NewStartingIndex, Is.EqualTo(0));

                _realm.Write(() =>
                {
                    person.Friends.Add(new Person());
                    _realm.Remove(person);
                });

                changeEvent = await eventHelper.GetNextEventAsync();

                Assert.That(changeEvent.Args!.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
                Assert.That(changeEvent.Sender.IsValid, Is.False);
            });
        }

        public static object[] CollectionChangedTestCases = new[]
        {
            new object[] { Array.Empty<int>(), NotifyCollectionChangedAction.Add, new int[] { 1 }, 0 },
            new object[] { Array.Empty<int>(), NotifyCollectionChangedAction.Add, new int[] { 1, 2, 3 }, 0 },
            new object[] { new int[] { 1, 2, 3 }, NotifyCollectionChangedAction.Remove, new int[] { 2 }, 1 },
            new object[] { new int[] { 1, 2, 3 }, NotifyCollectionChangedAction.Remove, new int[] { 1 }, 0 },
            new object[] { new int[] { 1, 2, 3 }, NotifyCollectionChangedAction.Add, new int[] { 0 }, 0 },
            new object[] { new int[] { 1, 2, 3 }, NotifyCollectionChangedAction.Add, new int[] { 4 }, 3 },
            new object[] { new int[] { 1, 2, 3 }, NotifyCollectionChangedAction.Add, new int[] { 4, 5 }, 3 },
            new object[] { new int[] { 1, 2, 3, 4, 5 }, NotifyCollectionChangedAction.Remove, new int[] { 3, 4 }, 2 },
            new object[] { new int[] { 1, 2, 3 }, NotifyCollectionChangedAction.Reset, new int[] { 1, 2, 3 }, 0 },

            // When we have non-consecutive adds/removes, we should raise Reset, indicated by -1 here.
            new object[] { new int[] { 1, 3, 5 }, NotifyCollectionChangedAction.Add, new int[] { 2, 4 }, -1 },
            new object[] { new int[] { 1, 2, 3, 4, 5 }, NotifyCollectionChangedAction.Remove, new int[] { 2, 4 }, -1 },
        };

        [Test]
        public void ResultsOfObjects_SubscribeForNotifications_DoesntReceiveModifications([Values(true, false)] bool shallow)
        {
            var changesets = new List<ChangeSet>();

            var kpCollection = shallow ? KeyPathsCollection.Shallow : KeyPathsCollection.Full;

            // This is testing using the internal API because we're not exposing the shallow/keypath functionality publicly yet.
            var results = (RealmResults<TestNotificationObject>)_realm.All<TestNotificationObject>();

            using var token = results.SubscribeForNotificationsImpl((sender, changes) =>
            {
                if (changes != null)
                {
                    changesets.Add(changes);
                }
            }, kpCollection);

            _realm.Write(() =>
            {
                _realm.Add(new TestNotificationObject());
            });

            VerifyNotifications(changesets, expectedInserted: new[] { 0 });

            _realm.Write(() =>
            {
                results.First().StringProperty = "abc";
            });

            VerifyNotifications(changesets, expectedModified: new[] { 0 }, expectedNotifications: !shallow);

            _realm.Write(() =>
            {
                _realm.RemoveAll<TestNotificationObject>();
            });

            VerifyNotifications(changesets, expectedDeleted: new[] { 0 });
        }

        [Test]
        public void ListOfObjects_SubscribeForNotifications_DoesntReceiveModifications([Values(true, false)] bool shallow)
        {
            var kpCollection = shallow ? KeyPathsCollection.Shallow : KeyPathsCollection.Full;

            var testObject = _realm.Write(() => _realm.Add(new CollectionsObject()));
            var changesets = new List<ChangeSet>();

            var list = (RealmList<IntPropertyObject>)testObject.ObjectList;

            using var token = list.SubscribeForNotificationsImpl((sender, changes) =>
            {
                if (changes != null)
                {
                    changesets.Add(changes);
                }
            }, kpCollection);

            _realm.Write(() =>
            {
                list.Add(new IntPropertyObject());
                list.Add(new IntPropertyObject());
            });

            VerifyNotifications(changesets, expectedInserted: new[] { 0, 1 });

            _realm.Write(() =>
            {
                list[0].Int = 456;
            });

            VerifyNotifications(changesets, expectedModified: new[] { 0 }, expectedNotifications: !shallow);

            _realm.Write(() =>
            {
                list[0] = new IntPropertyObject();
            });

            VerifyNotifications(changesets, expectedModified: new[] { 0 });

            _realm.Write(() =>
            {
                list.Move(0, 1);
            });

            // Moves are detected both as a move and a deletion + insertion
            VerifyNotifications(changesets, expectedInserted: new[] { 1 }, expectedDeleted: new[] { 0 }, expectedMoves: new Move[] { new(0, 1) });

            _realm.Write(() =>
            {
                list.RemoveAt(1);
            });

            VerifyNotifications(changesets, expectedDeleted: new[] { 1 });

            _realm.Write(() =>
            {
                list.Clear();
            });

            VerifyNotifications(changesets, expectedDeleted: new[] { 0 }, expectedCleared: true);
        }

        [Test]
        public void ListOfPrimitives_SubscribeForNotifications_ShallowHasNoEffect([Values(true, false)] bool shallow)
        {
            var kpCollection = shallow ? KeyPathsCollection.Shallow : KeyPathsCollection.Full;

            var testObject = _realm.Write(() => _realm.Add(new CollectionsObject()));
            var changesets = new List<ChangeSet>();

            var list = (RealmList<int>)testObject.Int32List;

            using var token = list.SubscribeForNotificationsImpl((sender, changes) =>
            {
                if (changes != null)
                {
                    changesets.Add(changes);
                }
            }, kpCollection);

            _realm.Write(() =>
            {
                list.Add(123);
                list.Add(456);
            });

            VerifyNotifications(changesets, expectedInserted: new[] { 0, 1 });

            _realm.Write(() =>
            {
                list[0] = 999;
            });

            VerifyNotifications(changesets, expectedModified: new[] { 0 });

            _realm.Write(() =>
            {
                list.Move(0, 1);
            });

            // Moves are detected both as a move and a deletion + insertion
            VerifyNotifications(changesets, expectedInserted: new[] { 1 }, expectedDeleted: new[] { 0 }, expectedMoves: new Move[] { new(0, 1) });

            _realm.Write(() =>
            {
                list.RemoveAt(1);
            });

            VerifyNotifications(changesets, expectedDeleted: new[] { 1 });

            _realm.Write(() =>
            {
                list.Clear();
            });

            VerifyNotifications(changesets, expectedDeleted: new[] { 0 }, expectedCleared: true);
        }

        [Test]
        public void SetOfObjects_SubscribeForNotifications_DoesntReceiveModifications([Values(true, false)] bool shallow)
        {
            var kpCollection = shallow ? KeyPathsCollection.Shallow : KeyPathsCollection.Full;

            var testObject = _realm.Write(() => _realm.Add(new CollectionsObject()));
            var changesets = new List<ChangeSet>();

            var set = (RealmSet<IntPropertyObject>)testObject.ObjectSet;

            using var token = set.SubscribeForNotificationsImpl((sender, changes) =>
            {
                if (changes != null)
                {
                    changesets.Add(changes);
                }
            }, kpCollection);

            _realm.Write(() =>
            {
                set.Add(new IntPropertyObject());
                set.Add(new IntPropertyObject());
            });

            VerifyNotifications(changesets, expectedInserted: new[] { 0, 1 });

            _realm.Write(() =>
            {
                set[0].Int = 456;
            });

            VerifyNotifications(changesets, expectedModified: new[] { 0 }, expectedNotifications: !shallow);

            _realm.Write(() =>
            {
                set.Remove(set[1]);
            });

            VerifyNotifications(changesets, expectedDeleted: new[] { 1 });

            _realm.Write(() =>
            {
                set.Clear();
            });

            VerifyNotifications(changesets, expectedDeleted: new[] { 0 }, expectedCleared: true);
        }

        [Test]
        public void SetOfPrimitives_SubscribeForNotifications_ShallowHasNoEffect([Values(true, false)] bool shallow)
        {
            var kpCollection = shallow ? KeyPathsCollection.Shallow : KeyPathsCollection.Full;

            var testObject = _realm.Write(() => _realm.Add(new CollectionsObject()));
            var changesets = new List<ChangeSet>();

            var set = (RealmSet<int>)testObject.Int32Set;

            using var token = set.SubscribeForNotificationsImpl((sender, changes) =>
            {
                if (changes != null)
                {
                    changesets.Add(changes);
                }
            }, kpCollection);

            _realm.Write(() =>
            {
                set.Add(123);
                set.Add(456);
            });

            VerifyNotifications(changesets, expectedInserted: new[] { 0, 1 });

            _realm.Write(() =>
            {
                set.Remove(set[1]);
            });

            VerifyNotifications(changesets, expectedDeleted: new[] { 1 });

            _realm.Write(() =>
            {
                set.Clear();
            });

            VerifyNotifications(changesets, expectedDeleted: new[] { 0 }, expectedCleared: true);
        }

        [Test]
        public void DictionaryOfObjects_SubscribeForNotifications_DoesntReceiveModifications([Values(true, false)] bool shallow)
        {
            var kpCollection = shallow ? KeyPathsCollection.Shallow : KeyPathsCollection.Full;

            var testObject = _realm.Write(() => _realm.Add(new CollectionsObject()));
            var changesets = new List<ChangeSet>();

            var dict = (RealmDictionary<IntPropertyObject>)testObject.ObjectDict;

            using var token = dict.SubscribeForNotificationsImpl((sender, changes) =>
            {
                if (changes != null)
                {
                    changesets.Add(changes);
                }
            }, kpCollection);

            _realm.Write(() =>
            {
                dict["1"] = new IntPropertyObject();
                dict["2"] = new IntPropertyObject();
            });

            VerifyNotifications(changesets, expectedInserted: new[] { 0, 1 });

            _realm.Write(() =>
            {
                dict["1"].Int = 456;
            });

            VerifyNotifications(changesets, expectedModified: new[] { 0 }, expectedNotifications: !shallow);

            _realm.Write(() =>
            {
                dict["1"] = new IntPropertyObject();
            });

            VerifyNotifications(changesets, expectedModified: new[] { 0 });

            _realm.Write(() =>
            {
                dict.Remove(dict[1].Key);
            });

            VerifyNotifications(changesets, expectedDeleted: new[] { 1 });

            _realm.Write(() =>
            {
                dict.Clear();
            });

            VerifyNotifications(changesets, expectedDeleted: new[] { 0 }, expectedCleared: true);
        }

        [Test]
        public void DictionaryOfPrimitives_SubscribeForNotifications_ShallowHasNoEffect([Values(true, false)] bool shallow)
        {
            var kpCollection = shallow ? KeyPathsCollection.Shallow : KeyPathsCollection.Full;

            var testObject = _realm.Write(() => _realm.Add(new CollectionsObject()));
            var changesets = new List<ChangeSet>();

            var dict = (RealmDictionary<int>)testObject.Int32Dict;

            using var token = dict.SubscribeForNotificationsImpl((sender, changes) =>
            {
                if (changes != null)
                {
                    changesets.Add(changes);
                }
            }, kpCollection);

            _realm.Write(() =>
            {
                dict["1"] = 123;
                dict["2"] = 456;
            });

            VerifyNotifications(changesets, expectedInserted: new[] { 0, 1 });

            _realm.Write(() =>
            {
                dict["1"] = 999;
            });

            VerifyNotifications(changesets, expectedModified: new[] { 0 });

            _realm.Write(() =>
            {
                dict.Remove(dict[1].Key);
            });

            VerifyNotifications(changesets, expectedDeleted: new[] { 1 });

            _realm.Write(() =>
            {
                dict.Clear();
            });

            VerifyNotifications(changesets, expectedDeleted: new[] { 0 }, expectedCleared: true);
        }

        #region Keypath filtering

        [Test]
        public void KeyPath_ImplicitOperator_CorrectlyConvertsFromString()
        {
            KeyPath keyPath = "test";

            Assert.That(keyPath.Path, Is.EqualTo("test"));
        }

        [Test]
        public void KeyPath_CanBeBuiltFromExpressions()
        {
            KeyPath keyPath;

            keyPath = KeyPath.ForExpression<TestNotificationObject>(t => t.ListSameType);
            Assert.That(keyPath.Path, Is.EqualTo("ListSameType"));

            keyPath = KeyPath.ForExpression<TestNotificationObject>(t => t.LinkAnotherType!.DictOfDogs);
            Assert.That(keyPath.Path, Is.EqualTo("LinkAnotherType.DictOfDogs"));
        }

        [Test]
        public void KeyPath_WithInvalidExpressions_ThrowsException()
        {
            Assert.That(() => KeyPath.ForExpression<TestNotificationObject>(t => t.Equals(this)),
                Throws.Exception.TypeOf<ArgumentException>().With.Message.Contains("The input expression is not a path to a property"));

            Assert.That(() => KeyPath.ForExpression<TestNotificationObject>(null!),
                Throws.Exception.TypeOf<ArgumentException>().With.Message.Contains("The input expression cannot be null"));
        }

        [Test]
        public void KeyPathsCollection_CanBeBuiltInDifferentWays()
        {
            var kpString1 = "test1";
            var kpString2 = "test2";

            KeyPath kp1 = "test1";
            KeyPath kp2 = "test2";

            var expected = new List<string> { kpString1, kpString2 };

            KeyPathsCollection kpc;

            kpc = new List<string> { kpString1, kpString2 };
            AssertKeyPathsCollectionCorrectness(kpc, expected);

            kpc = new List<KeyPath> { kpString1, kp2 };
            AssertKeyPathsCollectionCorrectness(kpc, expected);

            kpc = new List<KeyPath> { kp1, kp2 };
            AssertKeyPathsCollectionCorrectness(kpc, expected);

            kpc = new string[] { kpString1, kpString2 };
            AssertKeyPathsCollectionCorrectness(kpc, expected);

            kpc = new KeyPath[] { kpString1, kpString2 };
            AssertKeyPathsCollectionCorrectness(kpc, expected);

            kpc = new KeyPath[] { kp1, kp2 };
            AssertKeyPathsCollectionCorrectness(kpc, expected);

            kpc = KeyPathsCollection.Of(kpString1, kpString2);
            AssertKeyPathsCollectionCorrectness(kpc, expected);

            kpc = KeyPathsCollection.Of(kp1, kp2);
            AssertKeyPathsCollectionCorrectness(kpc, expected);

            kpc = KeyPathsCollection.Shallow;
            Assert.That(kpc.Type, Is.EqualTo(KeyPathsCollectionType.Shallow));
            Assert.That(kpc.GetStrings(), Is.Empty);

            kpc = KeyPathsCollection.Of();
            Assert.That(kpc.Type, Is.EqualTo(KeyPathsCollectionType.Shallow));
            Assert.That(kpc.GetStrings(), Is.Empty);

            kpc = KeyPathsCollection.Full;
            Assert.That(kpc.Type, Is.EqualTo(KeyPathsCollectionType.Full));
            Assert.That(kpc.GetStrings(), Is.Empty);

            void AssertKeyPathsCollectionCorrectness(KeyPathsCollection k, IEnumerable<string> expected)
            {
                Assert.That(k.Type, Is.EqualTo(KeyPathsCollectionType.Explicit));
                Assert.That(k.GetStrings(), Is.EqualTo(expected));
            }
        }

        [Test]
        public void KeyPathsCollection_CanBeBuiltFromExpressions()
        {
            var expected = new List<string> { "ListSameType", "LinkAnotherType.DictOfDogs" };

            var kpc = KeyPathsCollection.Of<TestNotificationObject>(t => t.ListSameType, t => t.LinkAnotherType!.DictOfDogs);
            AssertKeyPathsCollectionCorrectness(kpc, expected);

            kpc = KeyPathsCollection.Of(KeyPath.ForExpression<TestNotificationObject>(t => t.ListSameType),
                KeyPath.ForExpression<TestNotificationObject>(t => t.LinkAnotherType!.DictOfDogs));
            AssertKeyPathsCollectionCorrectness(kpc, expected);

            void AssertKeyPathsCollectionCorrectness(KeyPathsCollection k, IEnumerable<string> expectedStrings)
            {
                Assert.That(k.Type, Is.EqualTo(KeyPathsCollectionType.Explicit));
                Assert.That(k.GetStrings(), Is.EqualTo(expectedStrings));
            }
        }

        [Test]
        public void KeyPathsCollection_WithInvalidExpressions_ThrowsExceptions()
        {
            Assert.That(() => KeyPathsCollection.Of<TestNotificationObject>(t => t.ListSameType, null!),
                Throws.Exception.TypeOf<ArgumentException>().With.Message.Contains("The input expression cannot be null"));

            Assert.That(() => KeyPathsCollection.Of<TestNotificationObject>(t => t.Equals(this)),
                Throws.Exception.TypeOf<ArgumentException>().With.Message.Contains("The input expression is not a path to a property"));
        }

        [Test]
        public void SubscribeWithKeypaths_AnyKeypath_RaisesNotificationsForResults()
        {
            var query = _realm.All<TestNotificationObject>();
            var changesets = new List<ChangeSet>();

            void OnNotification(IRealmCollection<TestNotificationObject> s, ChangeSet? changes)
            {
                if (changes != null)
                {
                    changesets.Add(changes);
                }
            }

            using (query.SubscribeForNotifications(OnNotification, KeyPathsCollection.Of("StringProperty")))
            {
                var tno = new TestNotificationObject();

                _realm.Write(() => _realm.Add(tno));
                VerifyNotifications(changesets, expectedInserted: new[] { 0 });

                _realm.Write(() => tno.StringProperty = "NewString");
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                _realm.Write(() => _realm.Remove(tno));
                VerifyNotifications(changesets, expectedDeleted: new[] { 0 });
            }
        }

        [Test]
        public void SubscribeWithKeypaths_ShallowKeypath_RaisesOnlyCollectionNotifications()
        {
            var query = _realm.All<TestNotificationObject>();
            var changesets = new List<ChangeSet>();

            void OnNotification(IRealmCollection<TestNotificationObject> s, ChangeSet? changes)
            {
                if (changes != null)
                {
                    changesets.Add(changes);
                }
            }

            using (query.SubscribeForNotifications(OnNotification, KeyPathsCollection.Shallow))
            {
                var tno = new TestNotificationObject();

                _realm.Write(() => _realm.Add(tno));
                VerifyNotifications(changesets, expectedInserted: new[] { 0 });

                _realm.Write(() => tno.StringProperty = "NewString");
                VerifyNotifications(changesets, expectedNotifications: false);

                _realm.Write(() => _realm.Remove(tno));
                VerifyNotifications(changesets, expectedDeleted: new[] { 0 });
            }
        }

        [Test]
        public void SubscribeWithKeypaths_FullKeyPath_SameAsFourLevelsDepth()
        {
            var query = _realm.All<DeepObject1>();
            var changesets = new List<ChangeSet>();

            var dp5 = new DeepObject5();
            var dp4 = new DeepObject4() { RecursiveObject = dp5 };
            var dp3 = new DeepObject3() { RecursiveObject = dp4 };
            var dp2 = new DeepObject2() { RecursiveObject = dp3 };
            var dp1 = new DeepObject1() { RecursiveObject = dp2 };

            void OnNotification(IRealmCollection<DeepObject1> s, ChangeSet? changes)
            {
                if (changes != null)
                {
                    changesets.Add(changes);
                }
            }

            _realm.Write(() => _realm.Add(dp1));

            using (query.SubscribeForNotifications(OnNotification, KeyPathsCollection.Of("*.*.*.*")))
            {
                VerifyFourLevelsDepth();
            }

            using (query.SubscribeForNotifications(OnNotification, KeyPathsCollection.Full))
            {
                VerifyFourLevelsDepth();
            }

            void VerifyFourLevelsDepth()
            {
                _realm.Write(() => dp2.StringValue = "NewString");
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                _realm.Write(() => dp4.StringValue = "NewString");
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                _realm.Write(() => dp5.StringValue = "New String");
                VerifyNotifications(changesets, expectedNotifications: false);
            }
        }

        [Test]
        public void SubscribeWithKeypaths_TopLevelProperties_WorksWithScalar()
        {
            var query = _realm.All<TestNotificationObject>();
            var changesets = new List<ChangeSet>();

            void OnNotification(IRealmCollection<TestNotificationObject> s, ChangeSet? changes)
            {
                if (changes != null)
                {
                    changesets.Add(changes);
                }
            }

            var tno = new TestNotificationObject();
            _realm.Write(() => _realm.Add(tno));

            using (query.SubscribeForNotifications(OnNotification, KeyPathsCollection.Of("StringProperty")))
            {
                // Changing property in keypath
                _realm.Write(() => tno.StringProperty = "NewString");
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                // Changing properties not in keypath
                _realm.Write(() => tno.IntProperty = 23);
                _realm.Write(() => tno.ListDifferentType.Add(new Person()));
                _realm.Write(() => tno.DictionaryDifferentType.Add("key", new Person()));
                _realm.Write(() => tno.SetDifferentType.Add(new Person()));
                VerifyNotifications(changesets, expectedNotifications: false);

                // Changing key path property again
                _realm.Write(() => tno.StringProperty = "AgainNewString");
                VerifyNotifications(changesets, expectedModified: new[] { 0 });
            }
        }

        [Test]
        public void SubscribeWithKeypaths_TopLevelProperties_WorksWithCollection()
        {
            var query = _realm.All<TestNotificationObject>();
            var changesets = new List<ChangeSet>();

            void OnNotification(IRealmCollection<TestNotificationObject> s, ChangeSet? changes)
            {
                if (changes != null)
                {
                    changesets.Add(changes);
                }
            }

            var tno = new TestNotificationObject();
            _realm.Write(() => _realm.Add(tno));

            using (query.SubscribeForNotifications(OnNotification, KeyPathsCollection.Of("ListDifferentType")))
            {
                // Changing collection in keypath
                _realm.Write(() => tno.ListDifferentType.Add(new Person()));
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                // Changing properties not in keypath
                _realm.Write(() => tno.StringProperty = "23");
                _realm.Write(() => tno.DictionaryDifferentType.Add("key", new Person()));
                _realm.Write(() => tno.SetDifferentType.Add(new Person()));
                VerifyNotifications(changesets, expectedNotifications: false);

                // Changing elements in the collection does not raise notification
                _realm.Write(() => tno.ListDifferentType[0].FirstName = "FirstName");
                VerifyNotifications(changesets, expectedNotifications: false);

                // Changing collection
                _realm.Write(() => tno.ListDifferentType.RemoveAt(0));
                VerifyNotifications(changesets, expectedModified: new[] { 0 });
            }
        }

        [Test]
        public void SubscribeWithKeypaths_NestedProperties_WorksWithScalar()
        {
            var query = _realm.All<TestNotificationObject>();
            var changesets = new List<ChangeSet>();

            void OnNotification(IRealmCollection<TestNotificationObject> s, ChangeSet? changes)
            {
                if (changes != null)
                {
                    changesets.Add(changes);
                }
            }

            var tno = new TestNotificationObject();
            _realm.Write(() => _realm.Add(tno));

            using (query.SubscribeForNotifications(OnNotification, KeyPathsCollection.Of("LinkDifferentType.FirstName")))
            {
                // Changing top level property on the keypath
                _realm.Write(() => tno.LinkDifferentType = new Person());
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                // Changing top level property not on the keypath
                _realm.Write(() => tno.StringProperty = "23");
                _realm.Write(() => tno.DictionaryDifferentType.Add("key", new Person()));
                _realm.Write(() => tno.SetDifferentType.Add(new Person()));
                VerifyNotifications(changesets, expectedNotifications: false);

                // Changing keypath property
                _realm.Write(() => tno.LinkDifferentType!.FirstName = "NewName");
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                // Changing property not on keypath
                _realm.Write(() => tno.LinkDifferentType!.LastName = "NewName");
                VerifyNotifications(changesets, expectedNotifications: false);
            }
        }

        [Test]
        public void SubscribeWithKeypaths_NestedProperties_WorksWithCollections()
        {
            var query = _realm.All<TestNotificationObject>();
            var changesets = new List<ChangeSet>();

            void OnNotification(IRealmCollection<TestNotificationObject> s, ChangeSet? changes)
            {
                if (changes != null)
                {
                    changesets.Add(changes);
                }
            }

            var tno = new TestNotificationObject();
            _realm.Write(() => _realm.Add(tno));

            using (query.SubscribeForNotifications(OnNotification, KeyPathsCollection.Of("ListDifferentType.FirstName")))
            {
                // Changing top level property on the keypath
                _realm.Write(() => tno.ListDifferentType.Add(new Person()));
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                // Changing keypath property
                _realm.Write(() => tno.ListDifferentType[0].FirstName = "NewName");
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                // Changing top level property not on the keypath
                _realm.Write(() => tno.ListDifferentType[0].LastName = "NewName");
                _realm.Write(() => tno.StringProperty = "23");
                _realm.Write(() => tno.DictionaryDifferentType.Add("key", new Person()));
                VerifyNotifications(changesets, expectedNotifications: false);
            }
        }

        [Test]
        public void SubscribeWithKeypaths_WildCard_WorksWithTopLevel()
        {
            var query = _realm.All<TestNotificationObject>();
            var changesets = new List<ChangeSet>();

            void OnNotification(IRealmCollection<TestNotificationObject> s, ChangeSet? changes)
            {
                if (changes != null)
                {
                    changesets.Add(changes);
                }
            }

            var tno = new TestNotificationObject();
            _realm.Write(() => _realm.Add(tno));

            using (query.SubscribeForNotifications(OnNotification, KeyPathsCollection.Of("*")))
            {
                _realm.Write(() => tno.StringProperty = "NewString");
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                _realm.Write(() => tno.IntProperty = 23);
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                // Modifying links / changing the elements of collections should raise a notification
                _realm.Write(() => tno.LinkDifferentType = new Person());
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                _realm.Write(() => tno.ListDifferentType.Add(new Person()));
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                _realm.Write(() => tno.DictionaryDifferentType.Add("key", new Person()));
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                // Modifying the collection elements/links should not raise a notification
                _realm.Write(() => tno.LinkDifferentType!.FirstName = "NewName");
                _realm.Write(() => tno.ListDifferentType[0].LastName = "NewName");
                _realm.Write(() => tno.DictionaryDifferentType["key"]!.LastName = "NewName");
                VerifyNotifications(changesets, expectedNotifications: false);
            }
        }

        [Test]
        public void SubscribeWithKeypaths_WildCard_WorksWithMultipleLevels()
        {
            var query = _realm.All<TestNotificationObject>();
            var changesets = new List<ChangeSet>();

            void OnNotification(IRealmCollection<TestNotificationObject> s, ChangeSet? changes)
            {
                if (changes != null)
                {
                    changesets.Add(changes);
                }
            }

            var tno = new TestNotificationObject()
            {
                LinkAnotherType = new Owner()
                {
                    TopDog = new Dog(),
                }
            };
            _realm.Write(() => _realm.Add(tno));

            using (query.SubscribeForNotifications(OnNotification, KeyPathsCollection.Of("*.*")))
            {
                _realm.Write(() => tno.StringProperty = "NewString");
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                _realm.Write(() => tno.IntProperty = 23);
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                // Modifying collection/links should raise a notification
                _realm.Write(() => tno.LinkDifferentType = new Person());
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                _realm.Write(() => tno.ListDifferentType.Add(new Person()));
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                _realm.Write(() => tno.DictionaryDifferentType.Add("key", new Person()));
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                _realm.Write(() => tno.LinkDifferentType!.FirstName = "NewName");
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                _realm.Write(() => tno.ListDifferentType[0].LastName = "NewName");
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                _realm.Write(() => tno.DictionaryDifferentType["key"]!.LastName = "NewName");
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                _realm.Write(() => tno.LinkAnotherType!.ListOfDogs.Add(new Dog()));
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                // Modifying something 3 levels deep should not raise a notification
                _realm.Write(() => tno.LinkAnotherType!.TopDog.Name = "Test");
                _realm.Write(() => tno.LinkAnotherType!.ListOfDogs[0].Name = "Test");
                VerifyNotifications(changesets, expectedNotifications: false);
            }
        }

        [Test]
        public void SubscribeWithKeypaths_WildCard_WorksAfterLinkProperty()
        {
            var query = _realm.All<TestNotificationObject>();
            var changesets = new List<ChangeSet>();

            void OnNotification(IRealmCollection<TestNotificationObject> s, ChangeSet? changes)
            {
                if (changes != null)
                {
                    changesets.Add(changes);
                }
            }

            var tno = new TestNotificationObject();
            _realm.Write(() => _realm.Add(tno));

            using (query.SubscribeForNotifications(OnNotification, KeyPathsCollection.Of("LinkDifferentType.*")))
            {
                _realm.Write(() => tno.LinkDifferentType = new Person());
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                _realm.Write(() => tno.LinkDifferentType!.FirstName = "NewName");
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                _realm.Write(() => tno.LinkDifferentType!.Friends.Add(new Person()));
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                // Out of keypath
                _realm.Write(() => tno.StringProperty = "NewString");
                _realm.Write(() => tno.ListDifferentType.Add(new Person()));
                VerifyNotifications(changesets, expectedNotifications: false);

                // Too deep
                _realm.Write(() => tno.LinkDifferentType!.Friends[0].FirstName = "Luis");
                VerifyNotifications(changesets, expectedNotifications: false);
            }
        }

        [Test]
        public void SubscribeWithKeypaths_WildCard_WorksAfterCollectionProperty()
        {
            var query = _realm.All<TestNotificationObject>();
            var changesets = new List<ChangeSet>();

            void OnNotification(IRealmCollection<TestNotificationObject> s, ChangeSet? changes)
            {
                if (changes != null)
                {
                    changesets.Add(changes);
                }
            }

            var tno = new TestNotificationObject();
            _realm.Write(() => _realm.Add(tno));

            using (query.SubscribeForNotifications(OnNotification, KeyPathsCollection.Of("ListDifferentType.*")))
            {
                _realm.Write(() => tno.ListDifferentType.Add(new Person()));
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                _realm.Write(() => tno.ListDifferentType[0].FirstName = "Luis");
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                _realm.Write(() => tno.ListDifferentType[0].Friends.Add(new Person()));
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                // Out of keypath
                _realm.Write(() => tno.StringProperty = "NewString");
                _realm.Write(() => tno.LinkDifferentType = new Person());
                VerifyNotifications(changesets, expectedNotifications: false);

                // Too deep
                _realm.Write(() => tno.ListDifferentType[0]!.Friends[0].FirstName = "Luis");
                VerifyNotifications(changesets, expectedNotifications: false);
            }
        }

        [Test]
        public void SubscribeWithKeypaths_WildCard_WorksWithPropertyAfterward()
        {
            var query = _realm.All<TestNotificationObject>();
            var changesets = new List<ChangeSet>();

            void OnNotification(IRealmCollection<TestNotificationObject> s, ChangeSet? changes)
            {
                if (changes != null)
                {
                    changesets.Add(changes);
                }
            }

            var tno = new TestNotificationObject();
            _realm.Write(() => _realm.Add(tno));

            using (query.SubscribeForNotifications(OnNotification, KeyPathsCollection.Of("*.FirstName")))
            {
                _realm.Write(() => tno.LinkDifferentType = new Person());
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                _realm.Write(() => tno.LinkDifferentType!.FirstName = "NewName");
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                _realm.Write(() => tno.ListDifferentType.Add(new Person()));
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                _realm.Write(() => tno.ListDifferentType[0]!.FirstName = "NewName");
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                // Out of keypath
                _realm.Write(() => tno.LinkDifferentType!.LastName = "Test");
                _realm.Write(() => tno.ListDifferentType[0]!.LastName = "Test");
                VerifyNotifications(changesets, expectedNotifications: false);
            }
        }

        [Test]
        public void SubscribeWithKeypaths_WildCard_CanGetDeeperThanFourLevels()
        {
            var query = _realm.All<DeepObject1>();
            var changesets = new List<ChangeSet>();

            var dp5 = new DeepObject5();
            var dp4 = new DeepObject4() { RecursiveObject = dp5 };
            var dp3 = new DeepObject3() { RecursiveObject = dp4 };
            var dp2 = new DeepObject2() { RecursiveObject = dp3 };
            var dp1 = new DeepObject1() { RecursiveObject = dp2 };

            void OnNotification(IRealmCollection<DeepObject1> s, ChangeSet? changes)
            {
                if (changes != null)
                {
                    changesets.Add(changes);
                }
            }

            _realm.Write(() => _realm.Add(dp1));

            using (query.SubscribeForNotifications(OnNotification, KeyPathsCollection.Of("*.*.*.*.*")))
            {
                _realm.Write(() => dp2.StringValue = "NewString");
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                _realm.Write(() => dp4.StringValue = "NewString");
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                _realm.Write(() => dp5.StringValue = "New String");
                VerifyNotifications(changesets, expectedModified: new[] { 0 });
            }
        }

        [Test]
        public void SubscribeWithKeypaths_Backlinks()
        {
            var query = _realm.All<Dog>();
            var changesets = new List<ChangeSet>();

            void OnNotification(IRealmCollection<Dog> s, ChangeSet? changes)
            {
                if (changes != null)
                {
                    changesets.Add(changes);
                }
            }

            var dog = new Dog();
            _realm.Write(() => _realm.Add(dog));

            using (query.SubscribeForNotifications(OnNotification, KeyPathsCollection.Of("Owners", "Owners.Name")))
            {
                var owner = new Owner { Name = "Mario", ListOfDogs = { dog } };
                _realm.Write(() => _realm.Add(owner));
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                _realm.Write(() => owner.Name = "Luigi");
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                // Not in keypath
                _realm.Write(() => dog.Name = "Test");
                VerifyNotifications(changesets, expectedNotifications: false);
            }
        }

        [Test]
        public void SubscribeWithKeypaths_MultipleKeypaths()
        {
            var query = _realm.All<TestNotificationObject>();
            var changesets = new List<ChangeSet>();

            void OnNotification(IRealmCollection<TestNotificationObject> s, ChangeSet? changes)
            {
                if (changes != null)
                {
                    changesets.Add(changes);
                }
            }

            var tno = new TestNotificationObject();
            _realm.Write(() => _realm.Add(tno));

            using (query.SubscribeForNotifications(OnNotification, KeyPathsCollection.Of("StringProperty", "LinkDifferentType")))
            {
                _realm.Write(() => tno.StringProperty = "NewString");
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                _realm.Write(() => tno.LinkDifferentType = new Person());
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                // Not in keypath
                _realm.Write(() => tno.IntProperty = 23);
                _realm.Write(() => tno.LinkDifferentType!.FirstName = "Test");
                _realm.Write(() => tno.ListDifferentType.Add(new Person()));
                VerifyNotifications(changesets, expectedNotifications: false);
            }
        }

        [Test]
        public void SubscribeWithKeypaths_DisposingToken_CancelNotifications()
        {
            var query = _realm.All<TestNotificationObject>();
            var changesets = new List<ChangeSet>();

            void OnNotification(IRealmCollection<TestNotificationObject> s, ChangeSet? changes)
            {
                if (changes != null)
                {
                    changesets.Add(changes);
                }
            }

            var tno = new TestNotificationObject();
            _realm.Write(() => _realm.Add(tno));

            var token = query.SubscribeForNotifications(OnNotification, KeyPathsCollection.Of("StringProperty"));

            _realm.Write(() => tno.StringProperty = "NewString");
            VerifyNotifications(changesets, expectedModified: new[] { 0 });

            token.Dispose();

            _realm.Write(() => tno.StringProperty = "NewValue");
            VerifyNotifications(changesets, expectedNotifications: false);
        }

        [Test]
        public void SubscribeWithKeypaths_MappedProperty_UsesOriginalName()
        {
            var query = _realm.All<Person>();
            var changesets = new List<ChangeSet>();

            void OnNotification(IRealmCollection<Person> s, ChangeSet? changes)
            {
                if (changes != null)
                {
                    changesets.Add(changes);
                }
            }

            var person = new Person();
            _realm.Write(() => _realm.Add(person));

            using (query.SubscribeForNotifications(OnNotification, KeyPathsCollection.Of("Email_")))
            {
                // Changing property in keypath
                _realm.Write(() => person.Email = "email@test.com");
                VerifyNotifications(changesets, expectedModified: new[] { 0 });
            }
        }

        [Test]
        public void SubscribeWithKeypaths_MappedClass_WorksCorrectly()
        {
            var query = _realm.All<RemappedTypeObject>();
            var changesets = new List<ChangeSet>();

            void OnNotification(IRealmCollection<RemappedTypeObject> s, ChangeSet? changes)
            {
                if (changes != null)
                {
                    changesets.Add(changes);
                }
            }

            var rto = new RemappedTypeObject();
            _realm.Write(() => _realm.Add(rto));

            using (query.SubscribeForNotifications(OnNotification, KeyPathsCollection.Of("StringValue")))
            {
                // Changing property in keypath
                _realm.Write(() => rto.StringValue = "email@test.com");
                VerifyNotifications(changesets, expectedModified: new[] { 0 });
            }
        }

        [Test]
        public void SubscribeWithKeypaths_WithRepeatedKeypath_IgnoresRepeated()
        {
            var query = _realm.All<Person>();
            var changesets = new List<ChangeSet>();

            void OnNotification(IRealmCollection<Person> s, ChangeSet? changes)
            {
                if (changes != null)
                {
                    changesets.Add(changes);
                }
            }

            var person = new Person();
            _realm.Write(() => _realm.Add(person));

            using (query.SubscribeForNotifications(OnNotification, KeyPathsCollection.Of("FirstName", "FirstName")))
            {
                // Changing property in keypath
                _realm.Write(() => person.FirstName = "NewFirstName");
                VerifyNotifications(changesets, expectedModified: new[] { 0 });

                // Changing property not in keypath
                _realm.Write(() => person.LastName = "NewLastName");
                _realm.Write(() => person.Salary = 240);
                VerifyNotifications(changesets, expectedNotifications: false);
            }
        }

        [Test, Ignore("Failing because of https://github.com/realm/realm-core/issues/7269")]
        public void SubscribeWithKeypaths_WildcardOnScalarProperty_Throws()
        {
            var query = _realm.All<Person>();

            void OnNotification(IRealmCollection<Person> s, ChangeSet? c)
            {
            }

            var exMessage = "Property 'FirstName' in KeyPath 'FirstName.*' " +
                "is not a collection of objects or an object reference, so it cannot be used as an intermediate keypath element.";

            Assert.That(() => query.SubscribeForNotifications(OnNotification, KeyPathsCollection.Of("FirstName.*")),
                Throws.Exception.TypeOf<ArgumentException>().With.Message.EqualTo(exMessage));
        }

        [Test]
        public void SubscribeWithKeypaths_WithUnknownProperty_Throws()
        {
            var query = _realm.All<Person>();

            void OnNotification(IRealmCollection<Person> s, ChangeSet? c)
            {
            }

            var exMessage = "not a valid property in Person";

            // Top level property
            Assert.That(() => query.SubscribeForNotifications(OnNotification, KeyPathsCollection.Of("unknownProp")),
                Throws.Exception.TypeOf<ArgumentException>().With.Message.Contain(exMessage));

            // Nested property
            Assert.That(() => query.SubscribeForNotifications(OnNotification, KeyPathsCollection.Of("Friends.unknownProp")),
                Throws.Exception.TypeOf<ArgumentException>().With.Message.Contain(exMessage));
        }

        [Test]
        public void SubscribeWithKeypaths_WithEmptyOrWhiteSpaceKeypaths_Throws()
        {
            var query = _realm.All<Person>();

            void OnNotification(IRealmCollection<Person> s, ChangeSet? c)
            {
            }

            var exMessage = "A key path cannot be null, empty, or consisting only of white spaces";

            Assert.That(() => query.SubscribeForNotifications(OnNotification, KeyPathsCollection.Of(string.Empty)),
                Throws.Exception.TypeOf<ArgumentException>().With.Message.EqualTo(exMessage));
            Assert.That(() => query.SubscribeForNotifications(OnNotification, KeyPathsCollection.Of(" ")),
                Throws.Exception.TypeOf<ArgumentException>().With.Message.EqualTo(exMessage));
            Assert.That(() => query.SubscribeForNotifications(OnNotification, KeyPathsCollection.Of("test", null!)),
                Throws.Exception.TypeOf<ArgumentException>().With.Message.EqualTo(exMessage));
        }

        [Test]
        public void SubscribeWithKeypaths_WithNonRealmObjectType_Throws()
        {
            var collectionObject = _realm.Write(() => _realm.Add(new CollectionsObject
            {
                Int16List = { 1, 2 }
            }));

            var list = collectionObject.Int16List;

            void OnNotification(IRealmCollection<short> s, ChangeSet? c)
            {
            }

            var exMessage = "Key paths can be used only with collections of Realm objects";

            Assert.That(() => list.SubscribeForNotifications(OnNotification, KeyPathsCollection.Of("test")),
                Throws.Exception.TypeOf<InvalidOperationException>().With.Message.EqualTo(exMessage));
        }

        [Test]
        public void SubscribeWithKeypaths_OnCollection_List()
        {
            var obj1 = _realm.Write(() =>
            {
                return _realm.Add(new CollectionsObject());
            });

            ChangeSet? changes = null!;
            void OnNotification(IRealmCollection<IntPropertyObject> s, ChangeSet? c) => changes = c;

            using (obj1.ObjectList.SubscribeForNotifications(OnNotification, KeyPathsCollection.Of("Int")))
            {
                var ipo = new IntPropertyObject();

                _realm.Write(() => obj1.ObjectList.Add(ipo));
                _realm.Refresh();
                Assert.That(changes?.InsertedIndices, Is.EqualTo(new int[] { 0 }));
                changes = null;

                _realm.Write(() => ipo.Int = 23);
                _realm.Refresh();
                Assert.That(changes?.ModifiedIndices, Is.EqualTo(new int[] { 0 }));
                changes = null;

                _realm.Write(() => ipo.GuidProperty = Guid.NewGuid());
                _realm.Refresh();
                Assert.That(changes, Is.Null);
            }
        }

        [Test]
        public void SubscribeWithKeypaths_OnCollection_ListRemapped()
        {
            var obj1 = _realm.Write(() =>
            {
                return _realm.Add(new TestNotificationObject());
            });

            ChangeSet? changes = null!;
            void OnNotification(IRealmCollection<RemappedTypeObject> s, ChangeSet? c) => changes = c;

            using (obj1.ListRemappedType.SubscribeForNotifications(OnNotification, KeyPathsCollection.Of("StringValue")))
            {
                var ipo = new RemappedTypeObject();

                _realm.Write(() => obj1.ListRemappedType.Add(ipo));
                _realm.Refresh();
                Assert.That(changes?.InsertedIndices, Is.EqualTo(new int[] { 0 }));
                changes = null;
            }
        }

        [Test]
        public void SubscribeWithKeypaths_OnCollection_Set()
        {
            var obj1 = _realm.Write(() =>
            {
                return _realm.Add(new CollectionsObject());
            });

            ChangeSet? changes = null!;
            void OnNotification(IRealmCollection<IntPropertyObject> s, ChangeSet? c) => changes = c;

            using (obj1.ObjectSet.SubscribeForNotifications(OnNotification, KeyPathsCollection.Of("Int")))
            {
                var ipo = new IntPropertyObject();

                _realm.Write(() => obj1.ObjectSet.Add(ipo));
                _realm.Refresh();
                Assert.That(changes?.InsertedIndices, Is.EqualTo(new int[] { 0 }));
                changes = null;

                _realm.Write(() => ipo.Int = 23);
                _realm.Refresh();
                Assert.That(changes?.ModifiedIndices, Is.EqualTo(new int[] { 0 }));
                changes = null;

                _realm.Write(() => ipo.GuidProperty = Guid.NewGuid());
                _realm.Refresh();
                Assert.That(changes, Is.Null);
            }
        }

        [Test]
        public void SubscribeWithKeypaths_OnCollection_SetRemapped()
        {
            var obj1 = _realm.Write(() =>
            {
                return _realm.Add(new TestNotificationObject());
            });

            ChangeSet? changes = null!;
            void OnNotification(IRealmCollection<RemappedTypeObject> s, ChangeSet? c) => changes = c;

            using (obj1.SetRemappedType.SubscribeForNotifications(OnNotification, KeyPathsCollection.Of("StringValue")))
            {
                var ipo = new RemappedTypeObject();

                _realm.Write(() => obj1.SetRemappedType.Add(ipo));
                _realm.Refresh();
                Assert.That(changes?.InsertedIndices, Is.EqualTo(new int[] { 0 }));
                changes = null;
            }
        }

        [Test]
        public void SubscribeWithKeypaths_OnCollection_Dictionary()
        {
            var obj1 = _realm.Write(() =>
            {
                return _realm.Add(new CollectionsObject());
            });

            ChangeSet? changes = null!;
            void OnNotification(IRealmCollection<KeyValuePair<string, IntPropertyObject?>> s, ChangeSet? c) => changes = c;

            using (obj1.ObjectDict.SubscribeForNotifications(OnNotification, KeyPathsCollection.Of("Int")))
            {
                var ipo = new IntPropertyObject();

                _realm.Write(() => obj1.ObjectDict.Add("main", ipo));
                _realm.Refresh();
                Assert.That(changes?.InsertedIndices, Is.EqualTo(new int[] { 0 }));
                changes = null;

                _realm.Write(() => ipo.Int = 23);
                _realm.Refresh();
                Assert.That(changes?.ModifiedIndices, Is.EqualTo(new int[] { 0 }));
                changes = null;

                _realm.Write(() => ipo.GuidProperty = Guid.NewGuid());
                _realm.Refresh();
                Assert.That(changes, Is.Null);
            }
        }

        [Test]
        public void SubscribeWithKeypaths_OnCollection_DictionaryRemapped()
        {
            var obj1 = _realm.Write(() =>
            {
                return _realm.Add(new TestNotificationObject());
            });

            ChangeSet? changes = null!;
            void OnNotification(IRealmCollection<KeyValuePair<string, RemappedTypeObject?>> s, ChangeSet? c) => changes = c;

            using (obj1.DictionaryRemappedType.SubscribeForNotifications(OnNotification, KeyPathsCollection.Of("StringValue")))
            {
                var ipo = new RemappedTypeObject();

                _realm.Write(() => obj1.DictionaryRemappedType.Add("test", ipo));
                _realm.Refresh();
                Assert.That(changes?.InsertedIndices, Is.EqualTo(new int[] { 0 }));
                changes = null;
            }
        }

        #endregion

        private void VerifyNotifications(List<ChangeSet> notifications,
            int[]? expectedInserted = null,
            int[]? expectedModified = null,
            int[]? expectedDeleted = null,
            Move[]? expectedMoves = null,
            bool expectedCleared = false,
            bool expectedNotifications = true)
        {
            _realm.Refresh();
            Assert.That(notifications.Count, Is.EqualTo(expectedNotifications ? 1 : 0));
            if (expectedNotifications)
            {
                Assert.That(notifications[0].InsertedIndices, expectedInserted == null ? Is.Empty : Is.EquivalentTo(expectedInserted));
                Assert.That(notifications[0].ModifiedIndices, expectedModified == null ? Is.Empty : Is.EquivalentTo(expectedModified));
                Assert.That(notifications[0].DeletedIndices, expectedDeleted == null ? Is.Empty : Is.EquivalentTo(expectedDeleted));
                Assert.That(notifications[0].Moves, expectedMoves == null ? Is.Empty : Is.EquivalentTo(expectedMoves));
                Assert.That(notifications[0].IsCleared, Is.EqualTo(expectedCleared));
            }

            notifications.Clear();
        }
    }

    public partial class OrderedContainer : TestRealmObject
    {
        public IList<OrderedObject> Items { get; } = null!;

        public IDictionary<string, OrderedObject?> ItemsDictionary { get; } = null!;
    }

    public partial class OrderedObject : TestRealmObject
    {
        public int Order { get; set; }

        public bool IsPartOfResults { get; set; }

        public override string ToString()
        {
            return $"[OrderedObject: Order={Order}]";
        }
    }

    public partial class DeepObject1 : TestRealmObject
    {
        public string? StringValue { get; set; }

        public DeepObject2? RecursiveObject { get; set; }
    }

    public partial class DeepObject2 : TestRealmObject
    {
        public string? StringValue { get; set; }

        public DeepObject3? RecursiveObject { get; set; }
    }

    public partial class DeepObject3 : TestRealmObject
    {
        public string? StringValue { get; set; }

        public DeepObject4? RecursiveObject { get; set; }
    }

    public partial class DeepObject4 : TestRealmObject
    {
        public string? StringValue { get; set; }

        public DeepObject5? RecursiveObject { get; set; }
    }

    public partial class DeepObject5 : TestRealmObject
    {
        public string? StringValue { get; set; }
    }
}
