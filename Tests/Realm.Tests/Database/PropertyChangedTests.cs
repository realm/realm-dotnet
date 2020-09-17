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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class PropertyChangedTests
    {
        private string _databasePath;

        private Lazy<Realm> _lazyRealm;

        private Realm _realm => _lazyRealm.Value;

        // We capture the current SynchronizationContext when opening a Realm.
        // However, NUnit replaces the SynchronizationContext after the SetUp method and before the async test method.
        // That's why we make sure we open the Realm in the test method by accessing it lazily.
        [SetUp]
        public void SetUp()
        {
            _databasePath = Path.GetTempFileName();
            _lazyRealm = new Lazy<Realm>(() => Realm.GetInstance(_databasePath));
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
        public void UnmanagedObject()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                string notifiedPropertyName = null;
                var person = new Person();

                var handler = new PropertyChangedEventHandler((sender, e) =>
                {
                    notifiedPropertyName = e.PropertyName;
                });
                person.PropertyChanged += handler;

                // Subscribed - should trigger
                person.FirstName = "Peter";
                await Task.Yield();
                Assert.That(notifiedPropertyName, Is.EqualTo(nameof(Person.FirstName)));

                notifiedPropertyName = null;
                person.PropertyChanged -= handler;

                // Unsubscribed - should not trigger
                person.FirstName = "George";
                await Task.Yield();
                Assert.That(notifiedPropertyName, Is.Null);
            });
        }

        [Test]
        public void UnmanagedObject_AfterAdd_ShouldContinueTriggering()
        {
            var notifications = 0;
            var person = new Person();

            person.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(Person.FirstName))
                {
                    notifications++;
                }
            };

            person.FirstName = "Peter";

            _realm.Refresh();
            Assert.That(notifications, Is.EqualTo(1));

            _realm.Write(() =>
            {
                _realm.Add(person);
            });

            _realm.Refresh();
            Assert.That(notifications, Is.EqualTo(1));

            _realm.Write(() =>
            {
                person.FirstName = "George";
            });

            _realm.Refresh();
            Assert.That(notifications, Is.EqualTo(2));
        }

        [Test]
        public void ManagedObject_WhenSameInstanceChanged()
        {
            TestHelpers.RunAsyncTest(() =>
            {
                return TestManagedAsync((person, name) =>
                {
                    _realm.Write(() =>
                    {
                        person.FirstName = name;
                    });
                    return Task.CompletedTask;
                });
            });
        }

        [Test]
        public void ManagedObject_WhenAnotherInstanceChanged()
        {
            TestHelpers.RunAsyncTest(() =>
            {
                return TestManagedAsync((_, name) =>
                {
                    _realm.Write(() =>
                    {
                        var otherPersonInstance = _realm.All<Person>().First();
                        otherPersonInstance.FirstName = name;
                    });
                    return Task.CompletedTask;
                });
            });
        }

        [Test]
        public void ManagedObject_WhenAnotherThreadInstanceChanged()
        {
            TestHelpers.RunAsyncTest(() =>
            {
                return TestManagedAsync(async (_, name) =>
                {
                    await _realm.WriteAsync(otherRealm =>
                    {
                        var otherPersonInstance = otherRealm.All<Person>().First();
                        otherPersonInstance.FirstName = name;
                    });

                    await Task.Delay(50);
                });
            });
        }

        [Test]
        public void ManagedObject_WhenSameInstanceTransactionRollback()
        {
            TestManagedRollback((person, name) =>
            {
                person.FirstName = name;
            }, _realm.BeginWrite);
        }

        [Test]
        public void ManagedObject_WhenAnotherInstaceTransactionRollback()
        {
            TestManagedRollback((_, name) =>
            {
                var otherInstance = _realm.All<Person>().First();
                otherInstance.FirstName = name;
            }, _realm.BeginWrite);
        }

        [Test]
        public void ManagedObject_WhenAnotherThreadInstanceTransactionRollback()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var notifiedPropertyNames = new List<string>();
                var person = new Person();
                _realm.Write(() =>
                {
                    _realm.Add(person);
                });

                person.PropertyChanged += (sender, e) =>
                {
                    notifiedPropertyNames.Add(e.PropertyName);
                };

                await Task.Run(() =>
                {
                    using (var otherRealm = Realm.GetInstance(_databasePath))
                    using (var transaction = otherRealm.BeginWrite())
                    {
                        var otherInstance = otherRealm.All<Person>().First();
                        otherInstance.FirstName = "Peter";

                        Assert.That(notifiedPropertyNames, Is.Empty);

                        transaction.Rollback();
                    }
                });

                _realm.Refresh();
                Assert.That(notifiedPropertyNames, Is.Empty);
            });
        }

        [Test]
        public void ManagedObject_MultipleProperties()
        {
            var notifiedPropertyNames = new List<string>();
            var person = new Person();
            _realm.Write(() =>
            {
                _realm.Add(person);
            });

            person.PropertyChanged += (sender, e) =>
            {
                notifiedPropertyNames.Add(e.PropertyName);
            };

            _realm.Write(() =>
            {
                person.FirstName = "Peter";
            });

            _realm.Refresh();
            Assert.That(notifiedPropertyNames, Is.EquivalentTo(new[] { nameof(Person.FirstName) }));

            _realm.Write(() =>
            {
                person.LastName = "Smith";
            });

            _realm.Refresh();
            Assert.That(notifiedPropertyNames, Is.EquivalentTo(new[] { nameof(Person.FirstName), nameof(Person.LastName) }));

            _realm.Write(() =>
            {
                person.Score = 3.5f;
            });

            _realm.Refresh();
            Assert.That(notifiedPropertyNames, Is.EquivalentTo(new[] { nameof(Person.FirstName), nameof(Person.LastName), nameof(Person.Score) }));
        }

        [Test]
        public void MultipleManagedObjects()
        {
            if (TestHelpers.IgnoreOnWindows("ExternalCommitHelper hangs on Windows in this test. Reenable when we have proper condvar."))
            {
                return;
            }

            var firstNotifiedPropertyNames = new List<string>();
            var secondNotifiedPropertyNames = new List<string>();
            var first = new Person();
            var second = new Person();
            _realm.Write(() =>
            {
                _realm.Add(first);
                _realm.Add(second);
            });

            first.PropertyChanged += (sender, e) =>
            {
                firstNotifiedPropertyNames.Add(e.PropertyName);
            };

            second.PropertyChanged += (sender, e) =>
            {
                secondNotifiedPropertyNames.Add(e.PropertyName);
            };

            _realm.Write(() =>
            {
                first.IsAmbivalent = true;
            });

            _realm.Refresh();
            Assert.That(firstNotifiedPropertyNames, Is.EquivalentTo(new[] { nameof(Person.IsAmbivalent) }));
            Assert.That(secondNotifiedPropertyNames, Is.Empty);

            _realm.Write(() =>
            {
                second.Latitude = 4.6;
                second.Longitude = 5.6;
            });

            _realm.Refresh();
            Assert.That(firstNotifiedPropertyNames, Is.EquivalentTo(new[] { nameof(Person.IsAmbivalent) }));
            Assert.That(secondNotifiedPropertyNames, Is.EquivalentTo(new[] { nameof(Person.Latitude), nameof(Person.Longitude) }));
        }

        [Test]
        public void ManagedObject_AfterSubscribe_CanRemove()
        {
            var notifiedPropertyNames = new List<string>();
            var person = new Person();
            _realm.Write(() =>
            {
                _realm.Add(person);
            });

            person.PropertyChanged += (sender, e) =>
            {
                Assert.That(sender, Is.EqualTo(person));
                notifiedPropertyNames.Add(e.PropertyName);
            };

            _realm.Write(() =>
            {
                person.FirstName = "Peter";
            });

            _realm.Refresh();
            Assert.That(notifiedPropertyNames, Is.EquivalentTo(new[] { nameof(Person.FirstName) }));

            _realm.Write(() =>
            {
                _realm.Remove(person);
            });

            Assert.That(_realm.All<Person>().Count(), Is.EqualTo(0));
        }

        [Test]
        public void ManagedObject_MultipleSubscribers()
        {
            var subscriber1Properties = new List<string>();
            var subscriber2Properties = new List<string>();
            var person = new Person();
            _realm.Write(() =>
            {
                _realm.Add(person);
            });

            var handler1 = new PropertyChangedEventHandler((sender, e) =>
            {
                Assert.That(sender, Is.EqualTo(person));
                subscriber1Properties.Add(e.PropertyName);
            });
            person.PropertyChanged += handler1;

            person.PropertyChanged += (sender, e) =>
            {
                Assert.That(sender, Is.EqualTo(person));
                subscriber2Properties.Add(e.PropertyName);
            };

            _realm.Write(() =>
            {
                person.Birthday = new DateTimeOffset(1985, 1, 5, 8, 2, 3, TimeSpan.FromHours(3));
            });

            _realm.Refresh();
            Assert.That(subscriber1Properties, Is.EquivalentTo(new[] { nameof(Person.Birthday) }));
            Assert.That(subscriber2Properties, Is.EquivalentTo(new[] { nameof(Person.Birthday) }));

            person.PropertyChanged -= handler1;

            _realm.Write(() =>
            {
                person.IsInteresting = true;
            });

            _realm.Refresh();
            Assert.That(subscriber1Properties, Is.EquivalentTo(new[] { nameof(Person.Birthday) }));
            Assert.That(subscriber2Properties, Is.EquivalentTo(new[] { nameof(Person.Birthday), nameof(Person.IsInteresting) }));
        }

        [Test]
        public void ManagedObject_WhenMappedTo_ShouldUsePropertyName()
        {
            var notifiedPropertyNames = new List<string>();
            var person = new Person();
            _realm.Write(() =>
            {
                _realm.Add(person);
            });

            person.PropertyChanged += (sender, e) =>
            {
                Assert.That(sender, Is.EqualTo(person));
                notifiedPropertyNames.Add(e.PropertyName);
            };

            _realm.Write(() =>
            {
                person.Email = "peter@gmail.com";
            });

            _realm.Refresh();
            Assert.That(notifiedPropertyNames, Is.EquivalentTo(new[] { "Email_" }));
        }

        [Test]
        public void UnmanagedObject_WhenMappedTo_ShouldUsePropertyName()
        {
            var notifiedPropertyNames = new List<string>();
            var person = new Person();

            person.PropertyChanged += (sender, e) =>
            {
                Assert.That(sender, Is.EqualTo(person));
                notifiedPropertyNames.Add(e.PropertyName);
            };

            person.Email = "peter@gmail.com";

            Assert.That(notifiedPropertyNames, Is.EquivalentTo(new[] { "Email_" }));
        }

        [Test]
        public void ManagedObject_WhenHandleIsReleased_ShouldNotReceiveNotifications()
        {
            TestHelpers.IgnoreOnWindows("GC blocks on Windows");

            TestHelpers.RunAsyncTest(async () =>
            {
                var notifiedPropertyNames = new List<string>();
                WeakReference personReference = null;
                new Action(() =>
                {
                    var person = new Person();
                    _realm.Write(() => _realm.Add(person));

                    person.PropertyChanged += (sender, e) =>
                    {
                        notifiedPropertyNames.Add(e.PropertyName);
                    };

                    personReference = new WeakReference(person);

                    _realm.Write(() => person.FirstName = "Peter");

                    // Sanity check
                    _realm.Refresh();
                    Assert.That(notifiedPropertyNames, Is.EquivalentTo(new[] { nameof(Person.FirstName) }));
                })();

                notifiedPropertyNames.Clear();

                while (personReference.IsAlive)
                {
                    await Task.Yield();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }

                Assert.That(personReference.IsAlive, Is.False);

                _realm.Write(() =>
                {
                    var peter = _realm.All<Person>().Single();
                    Assert.That(peter.FirstName, Is.EqualTo("Peter"));
                    peter.FirstName = "George";
                });

                // person was garbage collected, so we should not be notified and no exception should be thrown.
                _realm.Refresh();
                Assert.That(notifiedPropertyNames, Is.Empty);
            });
        }

        [Test]
        public void ManagedObject_WhenChanged_CallsOnPropertyChanged()
        {
            var item = new AgedObject
            {
                Birthday = DateTimeOffset.UtcNow.AddYears(-5)
            };

            _realm.Write(() => _realm.Add(item));

            var notifiedPropertyNames = new List<string>();
            item.PropertyChanged += (sender, e) =>
            {
                notifiedPropertyNames.Add(e.PropertyName);
            };

            _realm.Write(() =>
            {
                item.Birthday = DateTimeOffset.UtcNow.AddYears(-6);
            });

            _realm.Refresh();

            Assert.That(notifiedPropertyNames, Is.EquivalentTo(new[] { nameof(AgedObject.Birthday), nameof(AgedObject.Age) }));
        }

        [Test]
        public void ManagedObject_WhenChangedOnAnotherThread_CallsOnPropertyChanged()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var item = new AgedObject
                {
                    Birthday = DateTimeOffset.UtcNow.AddYears(-5)
                };

                _realm.Write(() => _realm.Add(item));

                var notifiedPropertyNames = new List<string>();
                item.PropertyChanged += (sender, e) =>
                {
                    notifiedPropertyNames.Add(e.PropertyName);
                };

                await _realm.WriteAsync(r =>
                {
                    var otherThreadInstance = r.All<AgedObject>().Single();
                    otherThreadInstance.Birthday = DateTimeOffset.UtcNow.AddYears(-6);
                });

                await Task.Yield();

                Assert.That(notifiedPropertyNames, Is.EquivalentTo(new[] { nameof(AgedObject.Birthday), nameof(AgedObject.Age) }));
            });
        }

        [Test]
        public void UnmanagedObject_WhenChanged_CallsOnPropertyChanged()
        {
            var item = new AgedObject
            {
                Birthday = DateTimeOffset.UtcNow.AddYears(-5)
            };

            var notifiedPropertyNames = new List<string>();
            item.PropertyChanged += (sender, e) =>
            {
                notifiedPropertyNames.Add(e.PropertyName);
            };

            item.Birthday = DateTimeOffset.UtcNow.AddYears(-6);

            Assert.That(notifiedPropertyNames, Is.EquivalentTo(new[] { nameof(AgedObject.Birthday), nameof(AgedObject.Age) }));
        }

        [Test]
        public void ManagedObject_WhenSubscribedDuringTransaction_AfterCommit_ShouldGetNotifications()
        {
            var notifiedPropertyNames = new List<string>();
            var person = new Person();
            _realm.Write(() =>
            {
                _realm.Add(person);
            });

            var handler = new PropertyChangedEventHandler((sender, e) =>
            {
                notifiedPropertyNames.Add(e.PropertyName);
            });

            _realm.Write(() =>
            {
                person.PropertyChanged += handler;
                person.FirstName = "Peter";
            });

            _realm.Refresh();

            // We miss notifications from this transaction because we're subscribing after the
            // transaction has been committed.
            Assert.That(notifiedPropertyNames, Is.Empty);

            _realm.Write(() =>
            {
                person.FirstName = "John";
            });

            _realm.Refresh();

            // We should get subsequent notifications.
            Assert.That(notifiedPropertyNames, Is.EqualTo(new[] { nameof(Person.FirstName) }));

            person.PropertyChanged -= handler;
        }

        [Test]
        public void ManagedObject_WhenSubscribedDuringTransaction_AfterRollback_ShouldGetNotifications()
        {
            var notifiedPropertyNames = new List<string>();
            var person = new Person();
            _realm.Write(() =>
            {
                _realm.Add(person);
            });

            var handler = new PropertyChangedEventHandler((sender, e) =>
            {
                notifiedPropertyNames.Add(e.PropertyName);
            });

            using (var transaction = _realm.BeginWrite())
            {
                person.PropertyChanged += handler;
                person.FirstName = "Peter";
                transaction.Rollback();
            }

            _realm.Refresh();
            Assert.That(notifiedPropertyNames, Is.Empty);

            _realm.Write(() =>
            {
                person.FirstName = "John";
            });

            _realm.Refresh();

            // We should get subsequent notifications.
            Assert.That(notifiedPropertyNames, Is.EqualTo(new[] { nameof(Person.FirstName) }));
            person.PropertyChanged -= handler;
        }

        [Test]
        public void ManagedObject_WhenSubscribedDuringCreation_AfterCommit_ShouldReceiveNotifications()
        {
            var notifiedPropertyNames = new List<string>();
            var person = new Person();

            var handler = new PropertyChangedEventHandler((sender, e) =>
            {
                notifiedPropertyNames.Add(e.PropertyName);
            });

            _realm.Write(() =>
            {
                _realm.Add(person);
                person.PropertyChanged += handler;
            });

            _realm.Refresh();
            Assert.That(notifiedPropertyNames, Is.Empty);

            _realm.Write(() =>
            {
                person.FirstName = "John";
            });

            _realm.Refresh();
            Assert.That(notifiedPropertyNames, Is.EqualTo(new[] { nameof(Person.FirstName) }));
            person.PropertyChanged -= handler;
        }

        [Test]
        public void ManagedObject_WhenSubscribedDuringCreation_AfterRollback_ShouldNotThrow()
        {
            var notifiedPropertyNames = new List<string>();
            var person = new Person();

            var handler = new PropertyChangedEventHandler((sender, e) =>
            {
                notifiedPropertyNames.Add(e.PropertyName);
            });

            using (var transaction = _realm.BeginWrite())
            {
                _realm.Add(person);
                person.FirstName = "John";
                person.PropertyChanged += handler;

                transaction.Rollback();
            }

            _realm.Refresh();
            Assert.That(notifiedPropertyNames, Is.Empty);
            person.PropertyChanged -= handler;
        }

        [Test]
        public void ManagedObject_WhenSubscribedDuringDeletion_AfterCommit_ShouldNotThrow()
        {
            var notifiedPropertyNames = new List<string>();
            var person = new Person();
            _realm.Write(() =>
            {
                _realm.Add(person);
            });

            var handler = new PropertyChangedEventHandler((sender, e) =>
            {
                notifiedPropertyNames.Add(e.PropertyName);
            });

            _realm.Write(() =>
            {
                person.PropertyChanged += handler;
                person.FirstName = "John";
                _realm.Remove(person);
            });

            _realm.Refresh();
            Assert.That(notifiedPropertyNames, Is.Empty);
            person.PropertyChanged -= handler;
        }

        [Test, NUnit.Framework.Explicit("After remove + rollback, the object handle is invalid - https://github.com/realm/realm-dotnet/issues/1332")]
        public void ManagedObject_WhenSubscribedDuringDeletion_AfterRollback_ShouldReceiveNotifications()
        {
            var notifiedPropertyNames = new List<string>();
            var person = new Person();
            _realm.Write(() =>
            {
                _realm.Add(person);
            });

            var handler = new PropertyChangedEventHandler((sender, e) =>
            {
                notifiedPropertyNames.Add(e.PropertyName);
            });

            using (var transaction = _realm.BeginWrite())
            {
                person.PropertyChanged += handler;
                person.FirstName = "John";
                _realm.Remove(person);
                transaction.Rollback();
            }

            _realm.Refresh();
            Assert.That(notifiedPropertyNames, Is.Empty);

            _realm.Write(() => person.FirstName = "John");

            _realm.Refresh();
            Assert.That(notifiedPropertyNames, Is.EqualTo(new[] { nameof(Person.FirstName) }));

            person.PropertyChanged -= handler;
        }

        [Test]
        public void ManagedObject_WhenPropertyIsAfterBacklinks()
        {
            var notifiedPropertyNames = new List<string>();
            var obj = new BacklinkObject
            {
                AfterBacklinks = "a",
                BeforeBacklinks = "a"
            };
            _realm.Write(() =>
            {
                _realm.Add(obj);
            });

            var handler = new PropertyChangedEventHandler((sender, e) =>
            {
                notifiedPropertyNames.Add(e.PropertyName);
            });

            obj.PropertyChanged += handler;

            _realm.Write(() =>
            {
                obj.BeforeBacklinks = "b";
                obj.AfterBacklinks = "b";
            });

            _realm.Refresh();

            var expected = new[]
            {
                nameof(BacklinkObject.BeforeBacklinks),
                nameof(BacklinkObject.AfterBacklinks)
            };
            Assert.That(notifiedPropertyNames, Is.EquivalentTo(expected));
            obj.PropertyChanged -= handler;
        }

        [Test]
        public void ManagedObject_WhenDeleted_NotifiesIsValidChanged()
        {
            var notifiedPropertyNames = new List<string>();
            var person = new Person();
            _realm.Write(() =>
            {
                _realm.Add(person);
            });

            person.PropertyChanged += (sender, e) =>
            {
                notifiedPropertyNames.Add(e.PropertyName);
            };

            _realm.Write(() =>
            {
                _realm.Remove(person);
            });

            _realm.Refresh();

            Assert.That(notifiedPropertyNames.Count, Is.EqualTo(1));
            Assert.That(notifiedPropertyNames[0], Is.EqualTo(nameof(RealmObjectBase.IsValid)));
            Assert.That(person.IsValid, Is.False);
        }

        private async Task TestManagedAsync(Func<Person, string, Task> writeFirstNameAction)
        {
            var notifiedPropertyNames = new List<string>();
            var person = new Person();
            _realm.Write(() =>
            {
                _realm.Add(person);
            });
            var handler = new PropertyChangedEventHandler((sender, e) =>
            {
                notifiedPropertyNames.Add(e.PropertyName);
            });
            person.PropertyChanged += handler;

            // Subscribed - regular set should trigger
            await writeFirstNameAction(person, "Peter");

            _realm.Refresh();
            Assert.That(notifiedPropertyNames, Is.EquivalentTo(new[] { nameof(Person.FirstName) }));

            // Subscribed - setting the same value for the property should trigger again
            // This is different from .NET's usual behavior, but is a limitation due to the fact that we don't
            // check the previous value of the property before setting it.
            await writeFirstNameAction(person, "Peter");

            _realm.Refresh();
            Assert.That(notifiedPropertyNames, Is.EquivalentTo(new[] { nameof(Person.FirstName), nameof(Person.FirstName) }));

            notifiedPropertyNames.Clear();
            person.PropertyChanged -= handler;

            // Unsubscribed - should not trigger
            await writeFirstNameAction(person, "George");

            _realm.Refresh();
            Assert.That(notifiedPropertyNames, Is.Empty);
        }

        private void TestManagedRollback(Action<Person, string> writeFirstNameAction, Func<Transaction> transactionFactory)
        {
            var notifiedPropertyNames = new List<string>();
            var person = new Person();
            _realm.Write(() =>
            {
                _realm.Add(person);
            });

            person.PropertyChanged += (sender, e) =>
            {
                notifiedPropertyNames.Add(e.PropertyName);
            };

            using (var transaction = transactionFactory())
            {
                writeFirstNameAction(person, "Peter");

                Assert.That(notifiedPropertyNames, Is.Empty);
                transaction.Rollback();
            }

            _realm.Refresh();
            Assert.That(notifiedPropertyNames, Is.Empty);
        }

        private class AgedObject : RealmObject
        {
            public DateTimeOffset Birthday { get; set; }

            public int Age
            {
                get
                {
                    var now = DateTimeOffset.UtcNow;
                    var age = now.Year - Birthday.Year;
                    if (Birthday.AddYears(age) > now)
                    {
                        age--;
                    }

                    return age;
                }
            }

            protected override void OnPropertyChanged(string propertyName)
            {
                base.OnPropertyChanged(propertyName);

                if (propertyName == nameof(Birthday))
                {
                    RaisePropertyChanged(nameof(Age));
                }
            }
        }

        private class BacklinkObject : RealmObject
        {
            public string BeforeBacklinks { get; set; }

            [Backlink(nameof(SomeClass.BacklinkObject))]
            public IQueryable<SomeClass> Links { get; }

            public string AfterBacklinks { get; set; }
        }

        private class SomeClass : RealmObject
        {
            public BacklinkObject BacklinkObject { get; set; }
        }
    }
}
