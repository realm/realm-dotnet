////////////////////////////////////////////////////////////////////////////
//
// Copyright 2017 Realm Inc.
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
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Realms.Exceptions;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class RealmObjectTests : RealmInstanceTest
    {
        [Test]
        public void Realm_Add_InvokesOnManaged()
        {
            var obj = new OnManagedTestClass
            {
                Id = 1
            };

            Assert.That(obj.OnManagedCalled, Is.EqualTo(0));

            _realm.Write(() =>
            {
                _realm.Add(obj);
            });

            Assert.That(obj.OnManagedCalled, Is.EqualTo(1));

            var updatedObj = new OnManagedTestClass
            {
                Id = 1
            };

            Assert.That(updatedObj.OnManagedCalled, Is.EqualTo(0));

            _realm.Write(() =>
            {
                _realm.Add(updatedObj, update: true);
            });

            Assert.That(updatedObj.OnManagedCalled, Is.EqualTo(1));
        }

        [Test]
        public void RealmObject_SetRelatedObject_InvokesOnManaged()
        {
            var first = new OnManagedTestClass
            {
                Id = 1
            };

            _realm.Write(() =>
            {
                _realm.Add(first);
            });

            var second = new OnManagedTestClass
            {
                Id = 2
            };

            Assert.That(second.OnManagedCalled, Is.EqualTo(0));

            _realm.Write(() =>
            {
                first.RelatedObject = second;
            });

            Assert.That(second.OnManagedCalled, Is.EqualTo(1));
            Assert.That(first.RelatedObject.OnManagedCalled, Is.EqualTo(1));
        }

        [Test]
        public void RealmObject_AddToRelatedList_InvokesOnManaged()
        {
            var first = new OnManagedTestClass
            {
                Id = 1
            };

            _realm.Write(() =>
            {
                _realm.Add(first);
            });

            var second = new OnManagedTestClass
            {
                Id = 2
            };

            Assert.That(second.OnManagedCalled, Is.EqualTo(0));

            _realm.Write(() =>
            {
                first.RelatedCollection.Add(second);
            });

            Assert.That(second.OnManagedCalled, Is.EqualTo(1));
            Assert.That(first.RelatedCollection[0].OnManagedCalled, Is.EqualTo(1));
        }

        [Test]
        public void Realm_Find_InvokesOnManaged()
        {
            _realm.Write(() =>
            {
                _realm.Add(new OnManagedTestClass
                {
                    Id = 1
                });
            });

            var obj = _realm.Find<OnManagedTestClass>(1);
            Assert.That(obj.OnManagedCalled, Is.EqualTo(1));
        }

        [Test]
        public void RealmResults_InvokesOnManaged()
        {
            _realm.Write(() =>
            {
                _realm.Add(new OnManagedTestClass
                {
                    Id = 1
                });

                _realm.Add(new OnManagedTestClass
                {
                    Id = 2
                });
            });

            foreach (var obj in _realm.All<OnManagedTestClass>())
            {
                Assert.That(obj.OnManagedCalled, Is.EqualTo(1));
            }

            var elementAt = _realm.All<OnManagedTestClass>().ElementAt(0);

            Assert.That(elementAt.OnManagedCalled, Is.EqualTo(1));

            var first = _realm.All<OnManagedTestClass>().First(o => o.Id == 2);

            Assert.That(first.OnManagedCalled, Is.EqualTo(1));
        }

        [Test]
        public void RealmObjectEqualsCheck_WhenDeleted_ReturnsFalse()
        {
            var george = new Person { FirstName = "George" };
            var peter = new Person { FirstName = "Peter" };
            _realm.Write(() =>
            {
                _realm.Add(george);
                _realm.Add(peter);
            });

            Assert.That(george, Is.Not.EqualTo(peter));

            _realm.Write(() => _realm.Remove(george));

            Assert.That(george, Is.Not.EqualTo(peter));
            Assert.That(peter, Is.Not.EqualTo(george));
        }

        [Test]
        public void RealmObject_WhenSerialized_ShouldSkipBaseProperties()
        {
            var obj = new SerializedObject();
            _realm.Write(() => _realm.Add(obj));

            string text = null;
            using (var stream = new System.IO.MemoryStream())
            {
                var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                formatter.Serialize(stream, obj);
                text = System.Text.Encoding.UTF8.GetString(stream.ToArray());
            }

            foreach (var field in typeof(RealmObject).GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic))
            {
                Assert.That(text, Does.Not.Contains(field.Name));
            }
        }

        [Test]
        public void RealmObject_Freeze_FreezesInPlace()
        {
            var obj = new Owner
            {
                Name = "Peter",
                TopDog = new Dog
                {
                    Name = "Doggo"
                }
            };
            _realm.Write(() =>
            {
                _realm.Add(obj);
            });

            Assert.That(obj.IsManaged);

            FreezeInPlace(obj);

            Assert.That(obj.IsManaged);
            Assert.That(obj.IsValid);
            Assert.That(obj.IsFrozen);
            Assert.That(obj.Realm.IsFrozen);
            Assert.That(obj.Dogs.AsRealmCollection().IsFrozen);
            Assert.That(obj.TopDog.IsFrozen);
        }

        [Test]
        public void FrozenObject_GetsGarbageCollected()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                WeakReference objRef = null;
                new Action(() =>
                {
                    var owner = new Owner();
                    _realm.Write(() =>
                    {
                        _realm.Add(owner);
                    });
                    owner.FreezeInPlace();
                    objRef = new WeakReference(owner);
                })();

                while (objRef.IsAlive)
                {
                    await Task.Yield();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }

                // This will throw on Windows if the Realm object wasn't really GC-ed and its Realm - closed
                Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);
            });
        }

        [Test]
        public void RealmObject_FreezeInPlace_WhenObjectIsUnmanaged_Throws()
        {
            var owner = new Owner();
            Assert.Throws<RealmException>(() => owner.FreezeInPlace(), "Unmanaged objects cannot be frozen.");
        }

        [Test]
        public void RealmObject_Freeze_WhenObjectIsUnmanaged_Throws()
        {
            var owner = new Owner();
            Assert.Throws<RealmException>(() => owner.Freeze(), "Unmanaged objects cannot be frozen.");
        }

        [Test]
        public void RealmObject_FreezeInPlace_WhenFrozen_DoesNothing()
        {
            var obj = new Owner();
            _realm.Write(() =>
            {
                _realm.Add(obj);
            });
            FreezeInPlace(obj);

            var frozenRealm = obj.Realm;

            FreezeInPlace(obj);
            Assert.That(object.ReferenceEquals(frozenRealm, obj.Realm));
        }

        [Test]
        public void RealmObject_Freeze_WhenFrozen_ReturnsSameInstance()
        {
            var obj = new Owner();
            _realm.Write(() =>
            {
                _realm.Add(obj);
            });

            var frozenObj = Freeze(obj);
            Assert.That(ReferenceEquals(frozenObj, obj), Is.False);

            var deepFrozenObj = Freeze(frozenObj);
            Assert.That(ReferenceEquals(frozenObj, deepFrozenObj));
        }

        [Test]
        public void RealmObject_Freeze_DoesntModifyOriginal()
        {
            var obj = new Owner();
            _realm.Write(() =>
            {
                _realm.Add(obj);
            });

            var frozenObj = Freeze(obj);
            Assert.That(frozenObj.IsFrozen);
            Assert.That(frozenObj.IsValid);
            Assert.That(obj.IsFrozen, Is.False);
            Assert.That(obj.IsValid);
        }

        [Test]
        public void RealmObject_WhenFrozen_FailsToSubscribeToNotifications()
        {
            var obj = new Owner();
            _realm.Write(() =>
            {
                _realm.Add(obj);
            });

            FreezeInPlace(obj);

            Assert.Throws<RealmFrozenException>(() => obj.PropertyChanged += (_, __) => { }, "It is not possible to add a change listener to a frozen RealmObject since it never changes.");
        }

        [Test]
        public void FrozenObject_DoesntChange()
        {
            var livePeter = new Owner
            {
                Name = "Peter",
                TopDog = new Dog
                {
                    Name = "Doggo"
                }
            };

            _realm.Write(() =>
            {
                _realm.Add(livePeter);
            });

            var frozenPeter = Freeze(livePeter);

            Assert.That(frozenPeter.Name, Is.EqualTo("Peter"));

            _realm.Write(() =>
            {
                livePeter.Name = "Peter II";
                livePeter.TopDog.Name = "Dogsimus";
            });

            Assert.That(frozenPeter.Name, Is.EqualTo("Peter"));
            Assert.That(frozenPeter.TopDog.Name, Is.EqualTo("Doggo"));

            var frozenPeter2 = Freeze(livePeter);

            _realm.Write(() =>
            {
                livePeter.Name = "Peter III";
                livePeter.TopDog.Name = "Doggosaurus";
            });

            Assert.That(frozenPeter.Name, Is.EqualTo("Peter"));
            Assert.That(frozenPeter.TopDog.Name, Is.EqualTo("Doggo"));

            Assert.That(frozenPeter2.Name, Is.EqualTo("Peter II"));
            Assert.That(frozenPeter2.TopDog.Name, Is.EqualTo("Dogsimus"));
        }

        [Test]
        public void FrozenObject_DoesntGetDeleted()
        {
            var peter = new Owner
            {
                Name = "Peter"
            };

            _realm.Write(() =>
            {
                _realm.Add(peter);
            });

            FreezeInPlace(peter);

            _realm.Write(() =>
            {
                _realm.RemoveAll<Owner>();
            });

            Assert.That(_realm.All<Owner>(), Is.Empty);
            Assert.That(peter.IsValid);
            Assert.That(peter.Name, Is.EqualTo("Peter"));
        }

        [Serializable]
        private class SerializedObject : RealmObject
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        private class OnManagedTestClass : RealmObject
        {
            [PrimaryKey]
            public int Id { get; set; }

            public OnManagedTestClass RelatedObject { get; set; }

            public IList<OnManagedTestClass> RelatedCollection { get; }

            [Ignored]
            public int OnManagedCalled { get; private set; }

            protected internal override void OnManaged()
            {
                OnManagedCalled++;
            }
        }
    }
}
