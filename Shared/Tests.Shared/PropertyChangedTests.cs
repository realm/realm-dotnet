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
using Realms;

namespace IntegrationTests.Shared
{
    [TestFixture, Preserve(AllMembers = true)]
    public class PropertyChangedTests
    {
        private string _databasePath;
        private Realm _realm;

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
        public void UnmanagedObject()
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
            TestHelpers.RunEventLoop();
            Assert.That(notifiedPropertyName, Is.EqualTo(nameof(Person.FirstName)));

            notifiedPropertyName = null;
            person.PropertyChanged -= handler;

            // Unsubscribed - should not trigger
            person.FirstName = "George";
            TestHelpers.RunEventLoop();
            Assert.That(notifiedPropertyName, Is.Null);
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
            TestHelpers.RunEventLoop();
            Assert.That(notifications, Is.EqualTo(1));

            _realm.Write(() =>
            {
                _realm.Add(person);
            });

            // When calling Realm.Add, all properties are persisted, which causes a notification to be sent out.
            // We're using SomeClass because Person has nullable properties, which are set always, so it interferes with the test.
            TestHelpers.RunEventLoop();
            Assert.That(notifications, Is.EqualTo(2));

            _realm.Write(() =>
            {
                person.FirstName = "George";
            });

            TestHelpers.RunEventLoop();
            Assert.That(notifications, Is.EqualTo(3));
        }

        [Test]
        public void ManagedObject_WhenSameInstanceChanged()
        {
            TestManaged((person, name) =>
            {
                _realm.Write(() =>
                {
                    person.FirstName = name;
                });
            });
        }

        [Test]
        public void ManagedObject_WhenAnotherInstanceChanged()
        {
            TestManaged((_, name) =>
            {
                _realm.Write(() =>
                {
                    var otherPersonInstance = _realm.All<Person>().First();
                    otherPersonInstance.FirstName = name;
                });
            });
        }

        [Test]
        public void ManagedObject_WhenAnotherThreadInstanceChanged()
        {
            TestManaged((_, name) =>
            {
                _realm.WriteAsync(otherRealm =>
                {
                    var otherPersonInstance = otherRealm.All<Person>().First();
                    otherPersonInstance.FirstName = name;
                }).Wait();
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

            Task.Run(() =>
            {
                var otherRealm = Realm.GetInstance(_databasePath);
                using (var transaction = otherRealm.BeginWrite())
                {
                    var otherInstance = otherRealm.All<Person>().First();
                    otherInstance.FirstName = "Peter";

                    TestHelpers.RunEventLoop();
                    Assert.That(notifiedPropertyNames, Is.Empty);

                    transaction.Rollback();
                }
            }).Wait();

            TestHelpers.RunEventLoop();
            Assert.That(notifiedPropertyNames, Is.Empty);
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

            Assert.That(notifiedPropertyNames, Is.EquivalentTo(new[] { nameof(Person.FirstName) }));

            _realm.Write(() =>
            {
                person.LastName = "Smith";
            });

            Assert.That(notifiedPropertyNames, Is.EquivalentTo(new[] { nameof(Person.FirstName), nameof(Person.LastName) }));

            _realm.Write(() =>
            {
                person.Score = 3.5f;
            });

            Assert.That(notifiedPropertyNames, Is.EquivalentTo(new[] { nameof(Person.FirstName), nameof(Person.LastName), nameof(Person.Score) }));
        }

        [Test]
        public void MultipleManagedObjects()
        {
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

            Assert.That(firstNotifiedPropertyNames, Is.EquivalentTo(new[] { nameof(Person.IsAmbivalent) }));
            Assert.That(secondNotifiedPropertyNames, Is.Empty);

            _realm.Write(() =>
            {
                second.Latitude = 4.6;
                second.Longitude = 5.6;
            });

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

            Assert.That(subscriber1Properties, Is.EquivalentTo(new[] { nameof(Person.Birthday) }));
            Assert.That(subscriber2Properties, Is.EquivalentTo(new[] { nameof(Person.Birthday) }));

            person.PropertyChanged -= handler1;

            _realm.Write(() =>
            {
                person.IsInteresting = true;
            });

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

        private void TestManaged(Action<Person, string> writeFirstNameAction)
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
            writeFirstNameAction(person, "Peter");
            TestHelpers.RunEventLoop();
            Assert.That(notifiedPropertyNames, Is.EquivalentTo(new[] { nameof(Person.FirstName) }));

            // Subscribed - setting the same value for the property should trigger again
            // This is different from .NET's usual behavior, but is a limitation due to the fact that we don't
            // check the previous value of the property before setting it.
            writeFirstNameAction(person, "Peter");
            TestHelpers.RunEventLoop();
            Assert.That(notifiedPropertyNames, Is.EquivalentTo(new[] { nameof(Person.FirstName), nameof(Person.FirstName) }));

            notifiedPropertyNames.Clear();
            person.PropertyChanged -= handler;

            // Unsubscribed - should not trigger
            writeFirstNameAction(person, "George");
            TestHelpers.RunEventLoop();
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

                TestHelpers.RunEventLoop();
                Assert.That(notifiedPropertyNames, Is.EquivalentTo(new[] { nameof(Person.FirstName) }));

                transaction.Rollback();
            }

            TestHelpers.RunEventLoop();
            Assert.That(notifiedPropertyNames, Is.EquivalentTo(new[] { nameof(Person.FirstName), nameof(Person.FirstName) }));
        }
    }
}