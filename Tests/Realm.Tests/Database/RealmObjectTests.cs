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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using MongoDB.Bson;
using NUnit.Framework;
using Realms.Exceptions;
using Realms.Schema;
#if TEST_WEAVER
using TestAsymmetricObject = Realms.AsymmetricObject;
using TestEmbeddedObject = Realms.EmbeddedObject;
using TestRealmObject = Realms.RealmObject;
#else
using TestRealmObject = Realms.IRealmObject;
#endif

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
            Assert.That(first.RelatedObject!.OnManagedCalled, Is.EqualTo(1));
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
        public void RealmObject_ObjectSchema_ReturnsValueWhenManagedAndUnmanaged()
        {
            var person = new Person();

#if TEST_WEAVER
            Assert.That(person.ObjectSchema, Is.Null);
#else
            Assert.That(person.ObjectSchema, Is.Not.Null);
#endif
            _realm.Write(() =>
            {
                _realm.Add(person);
            });

            Assert.That(person.ObjectSchema, Is.Not.Null);

            Assert.That(person.ObjectSchema.TryFindProperty(nameof(Person.FirstName), out var property), Is.True);
            Assert.That(property.Type, Is.EqualTo(PropertyType.NullableString));
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

            var obj = _realm.Find<OnManagedTestClass>(1)!;
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
        public void RealmObject_GetHashCode_ChangesAfterAddingToRealm()
        {
            var objA = new RequiredPrimaryKeyStringObject { Id = "a" };

            var unmanagedHash = objA.GetHashCode();

            Assert.That(objA.GetHashCode(), Is.EqualTo(unmanagedHash), "The hash code of an unmanaged object should be stable");

            _realm.Write(() =>
            {
                _realm.Add(objA);
            });

            var managedHash = objA.GetHashCode();

            Assert.That(unmanagedHash, Is.Not.EqualTo(managedHash), "The hash code should change after the object is managed");
            Assert.That(objA.GetHashCode(), Is.EqualTo(managedHash), "The hash code of a managed object should be stable");
        }

        [Test]
        public void RealmObject_GetHashCode_IsDifferentForDifferentObjects()
        {
            var objA = new RequiredPrimaryKeyStringObject { Id = "a" };
            var objB = new RequiredPrimaryKeyStringObject { Id = "b" };

            Assert.That(objB.GetHashCode(), Is.Not.EqualTo(objA.GetHashCode()), "Different unmanaged objects should have different hash codes");

            _realm.Write(() =>
            {
                _realm.Add(objA);
                _realm.Add(objB);
            });

            Assert.That(objA.GetHashCode(), Is.Not.EqualTo(objB.GetHashCode()), "Different managed objects should have different hashes");
        }

        [Test]
        public void RealmObject_GetHashCode_IsSameForEqualManagedObjects()
        {
            var obj = _realm.Write(() =>
            {
                return _realm.Add(new RequiredPrimaryKeyStringObject { Id = "a" });
            });

            var objAgain = _realm.Find<RequiredPrimaryKeyStringObject>("a")!;

            Assert.That(objAgain.GetHashCode(), Is.EqualTo(obj.GetHashCode()), "The hash code of multiple managed objects pointing to the same row should be the same");
            Assert.That(objAgain, Is.EqualTo(obj));
        }

        [Test]
        public void RealmObject_GetHashCode_RemainsStableAfterDeletion()
        {
            var obj = _realm.Write(() =>
            {
                return _realm.Add(new RequiredPrimaryKeyStringObject { Id = "a" });
            });

            var managedHash = obj.GetHashCode();

            var objAgain = _realm.Find<RequiredPrimaryKeyStringObject>("a")!;

            _realm.Write(() =>
            {
                _realm.Remove(obj);
            });

            Assert.That(obj.GetHashCode(), Is.EqualTo(managedHash), "Object that was just deleted shouldn't change its hash code");
            Assert.That(objAgain.GetHashCode(), Is.EqualTo(managedHash), "Object that didn't hash its hash code and its row got deleted should still have the same hash code");
        }

        [Test]
        public void RealmObject_WhenSerialized_WithMongoDBBson_ShouldSkipBaseProperties([Values(true, false)] bool managed)
        {
            TestSerialization(managed, obj => obj.ToJson());
        }

        [Test]
        public void RealmObject_WhenSerialized_Xml_ShouldSkipBaseProperties([Values(true, false)] bool managed)
        {
            TestSerialization(managed, obj =>
            {
                var serializer = new XmlSerializer(typeof(SerializedObject));
                using var stream = new MemoryStream();
                serializer.Serialize(stream, obj);
                return Encoding.UTF8.GetString(stream.ToArray());
            });
        }

#if !UNITY

        [Test]
        public void RealmObject_WhenSerialized_NewtonsoftJson_ShouldSkipBaseProperties([Values(true, false)] bool managed)
        {
            TestSerialization(managed, obj => Newtonsoft.Json.JsonConvert.SerializeObject(obj), expectCollections: true);
        }

#endif

        private void TestSerialization(bool managed, Func<SerializedObject, string> serializationFunction, bool expectCollections = false)
        {
            var obj = new SerializedObject
            {
                IntValue = 123,
                Name = "abc"
            };

            obj.List.Add("list item");
            obj.Set.Add("set item");
            obj.Dict["foo"] = 987;

            if (managed)
            {
                _realm.Write(() => _realm.Add(obj));
            }

            var text = serializationFunction(obj);

            var realmTypes = new[] { typeof(IRealmObjectBase), typeof(RealmList<string>), typeof(RealmSet<string>), typeof(RealmDictionary<int>) };

            foreach (var field in realmTypes.SelectMany(t => t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)))
            {
                Assert.That(text, Does.Not.Contains(field.Name));
            }

            foreach (var prop in realmTypes.SelectMany(t => t.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)))
            {
                Assert.That(text, Does.Not.Contains(prop.Name));
            }

            Assert.That(text, Does.Contain(nameof(SerializedObject.IntValue)));
            Assert.That(text, Does.Contain(nameof(SerializedObject.Name)));
            Assert.That(text, Does.Contain(obj.Name));
            Assert.That(text, Does.Contain(obj.IntValue.ToString()));

            if (expectCollections)
            {
                Assert.That(text, Does.Contain(nameof(SerializedObject.Set)));
                Assert.That(text, Does.Contain(nameof(SerializedObject.Dict)));
                Assert.That(text, Does.Contain(nameof(SerializedObject.List)));

                Assert.That(text, Does.Contain(obj.Set.Single()));
                Assert.That(text, Does.Contain(obj.List.Single()));
                Assert.That(text, Does.Contain(obj.Dict.Keys.Single()));
                Assert.That(text, Does.Contain(obj.Dict.Values.Single().ToString()));
            }
        }

        [Test]
        public void FrozenObject_GetsGarbageCollected()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                await TestHelpers.EnsureObjectsAreCollected(() =>
                {
                    using var realm = Realm.GetInstance(_configuration);
                    var owner = realm.Write(() =>
                    {
                        return realm.Add(new Owner());
                    });

                    var frozenOwner = owner.Freeze();
                    var frozenRealm = frozenOwner.Realm!;

                    return new object[] { frozenOwner, frozenRealm };
                });

                // This will throw on Windows if the Realm object wasn't really GC-ed and its Realm - closed
                Realm.DeleteRealm(_configuration);
            });
        }

        [Test]
        public void RealmObject_Freeze_WhenObjectIsUnmanaged_Throws()
        {
            var owner = new Owner();
            Assert.Throws<RealmException>(() => Freeze(owner), "Unmanaged objects cannot be frozen.");
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

            var frozenObj = Freeze(obj);

            Assert.Throws<RealmFrozenException>(() => frozenObj.PropertyChanged += (_, __) => { }, "It is not possible to add a change listener to a frozen RealmObjectBase since it never changes.");
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
            Assert.That(frozenPeter.TopDog!.Name, Is.EqualTo("Doggo"));

            var frozenPeter2 = Freeze(livePeter);

            _realm.Write(() =>
            {
                livePeter.Name = "Peter III";
                livePeter.TopDog.Name = "Doggosaurus";
            });

            Assert.That(frozenPeter.Name, Is.EqualTo("Peter"));
            Assert.That(frozenPeter.TopDog.Name, Is.EqualTo("Doggo"));

            Assert.That(frozenPeter2.Name, Is.EqualTo("Peter II"));
            Assert.That(frozenPeter2.TopDog!.Name, Is.EqualTo("Dogsimus"));
        }

        [Test]
        public void FrozenObject_DoesntGetDeleted()
        {
            var frozenPeter = Freeze(_realm.Write(() =>
            {
                return _realm.Add(new Owner
                {
                    Name = "Peter"
                });
            }));

            _realm.Write(() =>
            {
                _realm.RemoveAll<Owner>();
            });

            Assert.That(_realm.All<Owner>(), Is.Empty);
            Assert.That(frozenPeter.IsValid);
            Assert.That(frozenPeter.Name, Is.EqualTo("Peter"));
        }

        [Test]
        public void RealmObject_Equals_WhenOtherIsNull_ReturnsFalse()
        {
            var obj = new Person();
            Assert.That(obj.Equals(null), Is.False);

            _realm.Write(() =>
            {
                _realm.Add(obj!);
            });

            Assert.That(obj!.Equals(null), Is.False);
        }

        [Test]
        public void RealmObject_InitializedFields_GetCorrectValues()
        {
            // This test ensures we only run the initialization instructions of Realm Object fields once.
            // i.e. where we have a class that has an ID field that gets incremented every time a new class
            // instance is created by incrementing an external variable and assigning it to the Id field.
            //
            // class FieldObject { Id: Generator.Id() } where Generator.Id = () => _currentId++;
            //
            // It could be that because of i.e. copying over initialization commands to the accessor
            // but not removing from the original constructor, the initialization of the field
            // would get repeated, thus leading to the _currentId being incremented twice.
            var obj0 = new InitializedFieldObject();
            var obj1 = new InitializedFieldObject();
            var obj2 = new InitializedFieldObject();

            Assert.That(obj0.Id, Is.EqualTo(0));
            Assert.That(obj1.Id, Is.EqualTo(1));
            Assert.That(obj2.Id, Is.EqualTo(2));
        }

        [Test]
        public void RealmObject_EqualsInvalidObject_WhenValid_ReturnsFalse()
        {
            var obj = new Person();
            Assert.That(obj.IsValid);
            Assert.That(obj.Equals(InvalidObject.Instance), Is.False);

            _realm.Write(() =>
            {
                _realm.Add(obj);
            });

            Assert.That(obj.IsValid);
            Assert.That(obj.Equals(InvalidObject.Instance), Is.False);
        }

        [Test]
        public void RealmObject_EqualsInvalidObject_WhenInvalid_ReturnsTrue()
        {
            var obj = _realm.Write(() =>
            {
                return _realm.Add(new Person());
            });

            Assert.That(obj.IsValid);
            Assert.That(obj.Equals(InvalidObject.Instance), Is.False);

            _realm.Write(() =>
            {
                _realm.Remove(obj);
            });

            Assert.That(obj.IsValid, Is.False);
            Assert.That(obj.Equals(InvalidObject.Instance), Is.True);
        }

        [Test]
        public void RealmObject_EqualsADifferentType_ReturnsFalse()
        {
            var person = new Person();
            var owner = new Owner();

            // Unmanaged.Equals(Unmanaged)
            Assert.That(person.Equals(owner), Is.False);

            _realm.Write(() =>
            {
                _realm.Add(person);
            });

            Assert.That(person.IsManaged);
            Assert.That(owner.IsManaged, Is.False);

            // Unmanaged.Equals(Managed)
            Assert.That(person.Equals(owner), Is.False);

            // Managed.Equals(Unmanaged)
            Assert.That(owner.Equals(person), Is.False);

            _realm.Write(() =>
            {
                _realm.Add(owner);
            });

            Assert.That(person.IsManaged);
            Assert.That(owner.IsManaged);

            // Managed.Equals(Managed)
            Assert.That(person.Equals(owner), Is.False);
        }

        [Test]
        public void RealmObject_Equals_WhenSameInstance_ReturnsTrue()
        {
            var person = new Person();
            Assert.That(person.Equals(person));

            _realm.Write(() =>
            {
                _realm.Add(person);
            });

            Assert.That(person.Equals(person));
        }

        [Test]
        public void RealmObject_Equals_UnmanagedDifferentInstance()
        {
            var firstWithPK = new PrimaryKeyStringObject { Id = "abc" };
            var secondWithPK = new PrimaryKeyStringObject { Id = "abc" };
            Assert.That(firstWithPK.Equals(secondWithPK), Is.False);
            Assert.That(ReferenceEquals(firstWithPK, secondWithPK), Is.False);

            var firstNoPK = new Person();
            var secondNoPK = new Person();
            Assert.That(firstNoPK.Equals(secondNoPK), Is.False);
            Assert.That(ReferenceEquals(firstNoPK, secondNoPK), Is.False);
        }

        [Test]
        public void RealmObject_Equals_ManagedSameInstance_WithPK()
        {
            var firstWithPK = new PrimaryKeyStringObject { Id = "abc" };
            var secondWithPK = new PrimaryKeyStringObject { Id = "abc" };

            _realm.Write(() =>
            {
                _realm.Add(firstWithPK, update: true);
                _realm.Add(secondWithPK, update: true);
            });

            Assert.That(ReferenceEquals(firstWithPK, secondWithPK), Is.False);
            Assert.That(firstWithPK.Equals(secondWithPK), Is.True);

            var firstTsr = ThreadSafeReference.Create(firstWithPK);
            var objResolved = _realm.ResolveReference(firstTsr)!;

            Assert.That(ReferenceEquals(firstWithPK, objResolved), Is.False);
            Assert.That(ReferenceEquals(objResolved, secondWithPK), Is.False);

            Assert.That(firstWithPK.Equals(objResolved), Is.True);
            Assert.That(objResolved.Equals(secondWithPK), Is.True);

            var objFromResults = _realm.All<PrimaryKeyStringObject>().Single();
            Assert.That(ReferenceEquals(firstWithPK, objFromResults), Is.False);
            Assert.That(firstWithPK.Equals(objFromResults), Is.True);

            var objFromFind = _realm.Find<PrimaryKeyStringObject>("abc")!;
            Assert.That(ReferenceEquals(secondWithPK, objFromFind), Is.False);
            Assert.That(objFromFind.Equals(secondWithPK), Is.True);
        }

        [Test]
        public void RealmObject_Equals_ManagedSameInstance_NoPK()
        {
            var person = new Person();

            _realm.Write(() =>
            {
                _realm.Add(person);
            });

            var personTsr = ThreadSafeReference.Create(person);
            var personResolved = _realm.ResolveReference(personTsr);

            Assert.That(ReferenceEquals(person, personResolved), Is.False);
            Assert.That(person.Equals(personResolved), Is.True);

            var personFromResults = _realm.All<Person>().Single();

            Assert.That(ReferenceEquals(person, personFromResults), Is.False);
            Assert.That(person.Equals(personFromResults), Is.True);
        }

        [Test]
        public void RealmObject_ToString_WhenUnmanaged()
        {
            var unmanaged = new Owner();

            Assert.That(unmanaged.ToString(), Is.EqualTo($"Owner (unmanaged)"));

            var unmanagedWithPK = new PrimaryKeyStringObject
            {
                Id = "abc"
            };

            Assert.That(unmanagedWithPK.ToString(), Is.EqualTo($"PrimaryKeyStringObject (unmanaged)"));
        }

        [Test]
        public void RealmObject_ToString_WithoutPK()
        {
            var managed = _realm.Write(() => _realm.Add(new Owner()));

            Assert.That(managed.ToString(), Is.EqualTo("Owner"));
        }

        [Test]
        public void RealmObject_ToString_WithPK()
        {
            var managedWithPK = _realm.Write(() => _realm.Add(new PrimaryKeyStringObject
            {
                Id = "abc"
            }));

            // We're printing out the Realm name of the Id, which is _id in this case
            Assert.That(managedWithPK.ToString(), Is.EqualTo($"PrimaryKeyStringObject (_id = abc)"));
        }

        [Test]
        public void RealmObject_ToString_WhenDeleted()
        {
            var obj = _realm.Write(() => _realm.Add(new PrimaryKeyStringObject
            {
                Id = "abc"
            }));

            _realm.Write(() =>
            {
                _realm.Remove(obj);
            });

            Assert.That(obj.ToString(), Is.EqualTo($"PrimaryKeyStringObject (removed)"));
        }

        [Test]
        public void RealmObject_WhenThrowsBeforeInitializer_DoesNotCrash()
        {
            // This tests that a RealmObject without an accessor (due to an exception thrown
            // before being initialized) does not cause a crash (NullReferenceException) once
            // the object gets GCed. (Simulate a crash by adding a destructor in RealmObject
            // that tries to access a member on "_accessor".)
            for (var i = 0; i < 1000; i++)
            {
                try
                {
                    _ = new ThrowsBeforeInitializer();
                }
                catch
                {
                }

                GC.Collect();
            }
        }
    }

    [Serializable]
    public partial class SerializedObject : TestRealmObject
    {
        public int IntValue { get; set; }

        public string? Name { get; set; }

        public IDictionary<string, int> Dict { get; } = null!;

        public IList<string> List { get; } = null!;

        public ISet<string> Set { get; } = null!;
    }

    public partial class OnManagedTestClass : TestRealmObject
    {
        [PrimaryKey]
        public int Id { get; set; }

        public OnManagedTestClass? RelatedObject { get; set; }

        public IList<OnManagedTestClass> RelatedCollection { get; } = null!;

        [Ignored]
        public int OnManagedCalled { get; private set; }

#if TEST_WEAVER
        protected internal override void OnManaged()
#else
        partial void OnManaged()
#endif
        {
            OnManagedCalled++;
        }
    }

    public partial class ThrowsBeforeInitializer : IRealmObject
    {
        [PrimaryKey]
        public int Id { get; set; }

        public object WillThrow = new Thrower();

        internal class Thrower
        {
            public Thrower()
            {
                throw new Exception("Exception thrown before initializer.");
            }
        }
    }
}
