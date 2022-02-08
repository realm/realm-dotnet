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
using System.Linq;
using NUnit.Framework;
using Realms.Exceptions;
using Realms.Schema;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class MigrationTests : RealmInstanceTest
    {
        private const string FileToMigrate = "ForMigrationsToCopyAndMigrate.realm";

        [Test]
        public void TriggerMigrationBySchemaVersion()
        {
            var config = (RealmConfiguration)RealmConfiguration.DefaultConfiguration;

            using (var realm = GetRealm())
            {
                // new database doesn't push back a version number
                Assert.That(realm.Config.SchemaVersion, Is.EqualTo(0));
            }

            var config2 = config.ConfigWithPath(config.DatabasePath);
            config2.SchemaVersion = 99;

            // same path, different version, should auto-migrate quietly
            using var realm2 = GetRealm(config2);
            Assert.That(realm2.Config.SchemaVersion, Is.EqualTo(99));
        }

        [Test]
        public void TriggerMigrationBySchemaEditing()
        {
            // NOTE to regnerate the bundled database go edit the schema in Person.cs and comment/uncomment ExtraToTriggerMigration
            // running in between and saving a copy with the added field
            // this should never be needed as this test just needs the Realm to need migrating

            // Because Realms opened during migration are not immediately disposed of, they can't be deleted.
            // To circumvent that, we're leaking realm files.
            // See https://github.com/realm/realm-dotnet/issues/1357
            var triggersSchemaFieldValue = string.Empty;

            var configuration = new RealmConfiguration(Guid.NewGuid().ToString())
            {
                SchemaVersion = 100,
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    Assert.That(oldSchemaVersion, Is.EqualTo(99));

                    var oldPeople = (IQueryable<RealmObject>)migration.OldRealm.DynamicApi.All("Person");
                    var newPeople = migration.NewRealm.All<Person>();

                    Assert.That(newPeople.Count(), Is.EqualTo(oldPeople.Count()));

                    for (var i = 0; i < newPeople.Count(); i++)
                    {
                        var oldPerson = oldPeople.ElementAt(i);
                        var newPerson = newPeople.ElementAt(i);

                        Assert.That(newPerson.LastName, Is.Not.EqualTo(oldPerson.DynamicApi.Get<string>("TriggersSchema")));
                        newPerson.LastName = triggersSchemaFieldValue = oldPerson.DynamicApi.Get<string>("TriggersSchema");

                        if (!TestHelpers.IsUnity)
                        {
                            // Ensure we can still use the dynamic API during migrations
                            dynamic dynamicOldPerson = oldPeople.ElementAt(i);
                            Assert.That(dynamicOldPerson.TriggersSchema, Is.EqualTo(oldPerson.DynamicApi.Get<string>("TriggersSchema")));
                        }
                    }
                }
            };

            TestHelpers.CopyBundledFileToDocuments(FileToMigrate, configuration.DatabasePath);

            var realm = GetRealm(configuration);
            var person = realm.All<Person>().Single();
            Assert.That(person.LastName, Is.EqualTo(triggersSchemaFieldValue));
        }

        [Test]
        public void ExceptionInMigrationCallback()
        {
            // Because Realms opened during migration are not immediately disposed of, they can't be deleted.
            // To circumvent that, we're leaking realm files.
            // See https://github.com/realm/realm-dotnet/issues/1357
            var dummyException = new Exception();

            var configuration = new RealmConfiguration(Guid.NewGuid().ToString())
            {
                SchemaVersion = 100,
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    throw dummyException;
                }
            };

            TestHelpers.CopyBundledFileToDocuments(FileToMigrate, configuration.DatabasePath);

            var ex = Assert.Throws<AggregateException>(() => GetRealm(configuration));
            Assert.That(ex.Flatten().InnerException, Is.SameAs(dummyException));
        }

        [Test]
        public void MigrationTriggersDelete()
        {
            var path = RealmConfiguration.DefaultConfiguration.DatabasePath;

            var oldConfig = new RealmConfiguration(path)
            {
                IsDynamic = true,
                Schema = new RealmSchema.Builder
                {
                    new ObjectSchema.Builder("Person", isEmbedded: false)
                    {
                        Property.FromType<string>("Name")
                    }
                }
            };

            using (var realm = GetRealm(oldConfig))
            {
                realm.Write(() =>
                {
                    var person = (RealmObject)(object)realm.DynamicApi.CreateObject("Person", null);
                    person.DynamicApi.Set("Name", "Foo");
                });
            }

            var newConfig = new RealmConfiguration(path)
            {
                IsDynamic = true,
                ShouldDeleteIfMigrationNeeded = true,
                Schema = new RealmSchema.Builder
                {
                    new ObjectSchema.Builder("Person", isEmbedded: false)
                    {
                        Property.FromType<int>("Name")
                    }
                }
            };

            using (var realm = GetRealm(newConfig))
            {
                Assert.That(realm.DynamicApi.All("Person"), Is.Empty);

                realm.Write(() =>
                {
                    var person = (RealmObject)(object)realm.DynamicApi.CreateObject("Person", null);
                    person.DynamicApi.Set("Name", 123);
                });
            }
        }

        [Test]
        public void MigrationRenameProperty()
        {
            var configuration = new RealmConfiguration(Guid.NewGuid().ToString())
            {
                SchemaVersion = 100,
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    migration.RenameProperty(nameof(Person), "TriggersSchema", nameof(Person.OptionalAddress));

                    var oldPeople = (IQueryable<RealmObject>)migration.OldRealm.DynamicApi.All("Person");
                    var newPeople = migration.NewRealm.All<Person>();

                    for (var i = 0; i < newPeople.Count(); i++)
                    {
                        var oldPerson = oldPeople.ElementAt(i);
                        var newPerson = newPeople.ElementAt(i);

                        var oldValue = oldPerson.DynamicApi.Get<string>("TriggersSchema");
                        var newValue = newPerson.OptionalAddress;
                        Assert.That(newValue, Is.EqualTo(oldValue));
                    }
                }
            };

            TestHelpers.CopyBundledFileToDocuments(FileToMigrate, configuration.DatabasePath);

            using var realm = GetRealm(configuration);
        }

        [Test]
        public void MigrationRenamePropertyErrors()
        {
            var oldPropertyValues = new List<string>();

            var configuration = new RealmConfiguration(Guid.NewGuid().ToString())
            {
                SchemaVersion = 100,
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    migration.RenameProperty(nameof(Person), "TriggersSchema", "PropertyNotInNewSchema");
                }
            };

            TestHelpers.CopyBundledFileToDocuments(FileToMigrate, configuration.DatabasePath);

            var ex = Assert.Throws<RealmException>(() => GetRealm(configuration));
            Assert.That(ex.Message, Does.Contain("Renamed property 'Person.PropertyNotInNewSchema' does not exist"));

            configuration = new RealmConfiguration(configuration.DatabasePath)
            {
                SchemaVersion = 100,
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    migration.RenameProperty(nameof(Person), "PropertyNotInOldSchema", nameof(Person.OptionalAddress));
                }
            };

            var ex2 = Assert.Throws<AggregateException>(() => GetRealm(configuration));
            Assert.That(ex2.Flatten().InnerException.Message, Does.Contain("Cannot rename property 'Person.PropertyNotInOldSchema' because it does not exist"));

            configuration = new RealmConfiguration(configuration.DatabasePath)
            {
                SchemaVersion = 100,
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    migration.RenameProperty("NonExistingType", "TriggersSchema", nameof(Person.OptionalAddress));
                }
            };

            ex2 = Assert.Throws<AggregateException>(() => GetRealm(configuration));
            Assert.That(ex2.Flatten().InnerException.Message, Does.Contain("Cannot rename properties for type 'NonExistingType' because it does not exist"));

            configuration = new RealmConfiguration(configuration.DatabasePath)
            {
                SchemaVersion = 100,
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    migration.RenameProperty(nameof(Person), "TriggersSchema", nameof(Person.Birthday));
                }
            };

            ex2 = Assert.Throws<AggregateException>(() => GetRealm(configuration));
            Assert.That(ex2.Flatten().InnerException.Message, Does.Contain("Cannot rename property 'Person.TriggersSchema' to 'Birthday' because it would change from type 'string' to 'date'."));

            configuration = new RealmConfiguration(configuration.DatabasePath)
            {
                SchemaVersion = 100,
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    migration.RenameProperty(nameof(Person), nameof(Person.Latitude), nameof(Person.Longitude));
                }
            };

            ex2 = Assert.Throws<AggregateException>(() => GetRealm(configuration));
            Assert.That(ex2.Flatten().InnerException.Message, Does.Contain("Cannot rename property 'Person.Latitude' to 'Longitude' because the source property still exists."));
        }

        [Test]
        public void MigrationRenamePropertyInvalidArguments()
        {
            var configuration = new RealmConfiguration(Guid.NewGuid().ToString())
            {
                SchemaVersion = 100,
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    Assert.Throws<ArgumentNullException>(() => migration.RenameProperty(null, "TriggersSchema", "OptionalAddress"));
                    Assert.Throws<ArgumentNullException>(() => migration.RenameProperty("Person", null, "OptionalAddress"));
                    Assert.Throws<ArgumentNullException>(() => migration.RenameProperty("Person", "TriggersSchema", null));

                    Assert.Throws<ArgumentException>(() => migration.RenameProperty(string.Empty, "TriggersSchema", "OptionalAddress"));
                    Assert.Throws<ArgumentException>(() => migration.RenameProperty("Person", string.Empty, "OptionalAddress"));
                    Assert.Throws<ArgumentException>(() => migration.RenameProperty("Person", "TriggersSchema", string.Empty));
                }
            };

            TestHelpers.CopyBundledFileToDocuments(FileToMigrate, configuration.DatabasePath);

            using var realm = GetRealm(configuration);
        }

        [Test]
        public void MigrationRemoveTypeInSchema()
        {
            var oldRealmConfig = new RealmConfiguration(Guid.NewGuid().ToString())
            {
                SchemaVersion = 0,
                Schema = new[] { typeof(Dog), typeof(Owner), typeof(Person) },
            };

            using (var oldRealm = GetRealm(oldRealmConfig))
            {
            }

            var newRealmConfig = new RealmConfiguration(oldRealmConfig.DatabasePath)
            {
                SchemaVersion = 1,
                Schema = new[] { typeof(PrimaryKeyObjectIdObject), typeof(Person) },
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    migration.RemoveType(nameof(Person));
                }
            };

            var ex = Assert.Throws<AggregateException>(() => GetRealm(newRealmConfig));
            Assert.That(ex.Flatten().InnerException.Message, Does.Contain("Attempted to remove type 'Person', that is present in the current schema"));
        }

        [Test]
        public void MigrationRemoveTypeNotInSchema()
        {
            var oldRealmConfig = new RealmConfiguration(Guid.NewGuid().ToString())
            {
                SchemaVersion = 0,
                Schema = new[] { typeof(Dog), typeof(Owner), typeof(Person) },
            };

            using (var oldRealm = GetRealm(oldRealmConfig))
            {
                oldRealm.Write(() =>
                {
                    oldRealm.Add(new Person { FirstName = "Maria" });
                });
            }

            var migrationCallbackCalled = false;
            var newRealmConfig = new RealmConfiguration(oldRealmConfig.DatabasePath)
            {
                SchemaVersion = 1,
                Schema = new[] { typeof(Dog), typeof(Owner) },
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    migrationCallbackCalled = true;

                    var migrationResultNotInSchema = migration.RemoveType("NotInSchemaType");
                    Assert.That(migrationResultNotInSchema, Is.False);

                    var migrationResult = migration.RemoveType(nameof(Person));
                    Assert.That(migrationResult, Is.True);

                    // Removed type in oldRealm is still available for the duration of the migration
                    var oldPeople = (IQueryable<RealmObject>)migration.OldRealm.DynamicApi.All("Person");
                    var oldPerson = oldPeople.First();

                    Assert.That(oldPeople.Count(), Is.EqualTo(1));
                    Assert.That(oldPerson.DynamicApi.Get<string>("FirstName"), Is.EqualTo("Maria"));
                }
            };

            using (var newRealm = GetRealm(newRealmConfig))
            {
                Assert.That(migrationCallbackCalled, Is.True);

                // This just means that "Person" is not in the schema that we pass in the config, but we're not sure it has been removed.
                Assert.That(newRealm.Schema.TryFindObjectSchema("Person", out _), Is.False);
            }

            var newRealmDynamicConfig = new RealmConfiguration(oldRealmConfig.DatabasePath)
            {
                SchemaVersion = 1,
                IsDynamic = true,
            };

            using var newRealmDynamic = GetRealm(newRealmDynamicConfig);

            // This means that "Person" is not in the schema anymore, as we retrieve it directly from core.
            Assert.That(newRealmDynamic.Schema.TryFindObjectSchema("Person", out _), Is.False);
        }

        [Test]
        public void MigrationRemoveTypeInvalidArguments()
        {
            var oldRealmConfig = new RealmConfiguration(Guid.NewGuid().ToString())
            {
                SchemaVersion = 0,
                Schema = new[] { typeof(Dog), typeof(Owner), typeof(Person) },
            };

            using (var oldRealm = GetRealm(oldRealmConfig))
            {
            }

            var newRealmConfig = new RealmConfiguration(oldRealmConfig.DatabasePath)
            {
                SchemaVersion = 1,
                Schema = new[] { typeof(PrimaryKeyObjectIdObject), typeof(Person) },
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    Assert.Throws<ArgumentNullException>(() => migration.RemoveType(null));
                    Assert.Throws<ArgumentException>(() => migration.RemoveType(string.Empty));
                }
            };

            using var realm = GetRealm(newRealmConfig);
        }

        [Test]
        public void Migration_WhenDone_DisposesAllObjectsAndLists()
        {
            var config = new RealmConfiguration(Guid.NewGuid().ToString());
            using (var oldRealm = GetRealm(config))
            {
            }

            AllTypesObject standaloneObject = null;
            EmbeddedAllTypesObject embeddedObject = null;
            RealmList<string> list = null;
            RealmSet<string> set = null;
            RealmDictionary<string> dict = null;
            RealmResults<AllTypesObject> query = null;

            var newConfig = new RealmConfiguration(config.DatabasePath)
            {
                SchemaVersion = 1,
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    standaloneObject = migration.NewRealm.Add(new AllTypesObject { RequiredStringProperty = string.Empty });
                    Assert.That(standaloneObject.IsValid);
                    Assert.That(standaloneObject.IsManaged);
                    Assert.That(standaloneObject.Int32Property, Is.EqualTo(0));

                    embeddedObject = new EmbeddedAllTypesObject();
                    migration.NewRealm.Add(new ObjectWithEmbeddedProperties
                    {
                        AllTypesObject = embeddedObject
                    });

                    Assert.That(embeddedObject.IsValid);
                    Assert.That(embeddedObject.IsManaged);
                    Assert.That(embeddedObject.Int32Property, Is.EqualTo(0));

                    var collectionObject = migration.NewRealm.Add(new CollectionsObject());
                    list = (RealmList<string>)collectionObject.StringList;
                    list.Add("foo");
                    Assert.That(list.IsValid);
                    Assert.That(list[0], Is.EqualTo("foo"));
                    Assert.That(list.Count, Is.EqualTo(1));

                    set = (RealmSet<string>)collectionObject.StringSet;
                    set.Add("foo");
                    Assert.That(set.IsValid);
                    Assert.That(set[0], Is.EqualTo("foo"));
                    Assert.That(set.Count, Is.EqualTo(1));

                    dict = (RealmDictionary<string>)collectionObject.StringDict;
                    dict.Add("foo", "bar");
                    Assert.That(dict.IsValid);
                    Assert.That(dict[0].Key, Is.EqualTo("foo"));
                    Assert.That(dict.Count, Is.EqualTo(1));

                    query = (RealmResults<AllTypesObject>)migration.NewRealm.All<AllTypesObject>();
                    Assert.That(query.IsValid);
                    Assert.That(query[0], Is.Not.Null);
                    Assert.That(query.Count, Is.EqualTo(1));
                }
            };

            using (var newRealm = Realm.GetInstance(newConfig))
            {
                // Here we should see all objects accessed during the migration get disposed, even though
                // newRealm is still open.
                Assert.That(standaloneObject.IsValid, Is.False);
                Assert.That(standaloneObject.IsManaged);
                Assert.Throws<RealmClosedException>(() => _ = standaloneObject.Int32Property);
                Assert.That(standaloneObject.ObjectHandle.IsClosed);

                Assert.That(embeddedObject.IsValid, Is.False);
                Assert.That(embeddedObject.IsManaged);
                Assert.Throws<RealmClosedException>(() => _ = embeddedObject.Int32Property);
                Assert.That(embeddedObject.ObjectHandle.IsClosed);

                Assert.That(list.IsValid, Is.False);
                Assert.Throws<RealmClosedException>(() => _ = list[0]);
                Assert.That(list.Handle.Value.IsClosed);

                Assert.That(set.IsValid, Is.False);
                Assert.Throws<RealmClosedException>(() => _ = set[0]);
                Assert.That(set.Handle.Value.IsClosed);

                Assert.That(dict.IsValid, Is.False);
                Assert.Throws<RealmClosedException>(() => _ = dict[0]);
                Assert.That(dict.Handle.Value.IsClosed);

                Assert.That(query.IsValid, Is.False);
                Assert.Throws<RealmClosedException>(() => _ = query[0]);
                Assert.That(query.ResultsHandle.IsClosed);
            }

            Assert.DoesNotThrow(() => Realm.DeleteRealm(newConfig));
        }

        [Test]
        public void Migration_NewRealm_Remove()
        {
            // Reported in https://github.com/realm/realm-dotnet/issues/2587
            var config = (RealmConfiguration)RealmConfiguration.DefaultConfiguration;

            using (var realm = GetRealm())
            {
                realm.Write(() =>
                {
                    for (var i = 0; i < 10; i++)
                    {
                        realm.Add(new IntPropertyObject
                        {
                            Int = i
                        });

                        realm.Add(new RequiredStringObject
                        {
                            String = i.ToString()
                        });
                    }
                });
            }

            var config2 = config.ConfigWithPath(config.DatabasePath);
            config2.SchemaVersion = 1;
            config2.MigrationCallback = (migration, oldSchemaVersion) =>
            {
                Assert.That(oldSchemaVersion, Is.EqualTo(0));

                var intObjects = migration.NewRealm.All<IntPropertyObject>();
                var stringObjects = migration.NewRealm.All<RequiredStringObject>();

                // Delete all even values
                for (var i = 0; i < 10; i += 2)
                {
                    var intObject = intObjects.Single(o => o.Int == i);
                    var stringObject = stringObjects.Single(o => o.String == i.ToString());

                    migration.NewRealm.Remove(intObject);
                    migration.NewRealm.Remove(stringObject);
                }
            };

            // same path, different version, should auto-migrate quietly
            using var realm2 = GetRealm(config2);

            var expected = new[] { 1, 3, 5, 7, 9 };

            Assert.That(realm2.All<IntPropertyObject>().ToArray().Select(o => o.Int), Is.EqualTo(expected));
            Assert.That(realm2.All<RequiredStringObject>().ToArray().Select(o => int.Parse(o.String)), Is.EqualTo(expected));
        }

        [Test]
        public void Migration_ChangePrimaryKey_Dynamic()
        {
            var oldRealmConfig = new RealmConfiguration(Guid.NewGuid().ToString());
            using (var oldRealm = GetRealm(oldRealmConfig))
            {
                oldRealm.Write(() =>
                {
                    oldRealm.Add(new IntPrimaryKeyWithValueObject
                    {
                        Id = 123,
                        StringValue = "123"
                    });
                });
            }

            var newRealmConfig = new RealmConfiguration(oldRealmConfig.DatabasePath)
            {
                SchemaVersion = 1,
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    var value = (RealmObjectBase)migration.NewRealm.DynamicApi.Find(nameof(IntPrimaryKeyWithValueObject), 123);
                    value.DynamicApi.Set("_id", 456);
                }
            };

            using var realm = GetRealm(newRealmConfig);

            var obj123 = realm.Find<IntPrimaryKeyWithValueObject>(123);
            var obj456 = realm.Find<IntPrimaryKeyWithValueObject>(456);

            Assert.That(obj123, Is.Null);
            Assert.That(obj456, Is.Not.Null);
            Assert.That(obj456.StringValue, Is.EqualTo("123"));
        }

        [Test]
        public void Migration_ChangePrimaryKey_Static()
        {
            var oldRealmConfig = new RealmConfiguration(Guid.NewGuid().ToString());
            using (var oldRealm = GetRealm(oldRealmConfig))
            {
                oldRealm.Write(() =>
                {
                    oldRealm.Add(new IntPrimaryKeyWithValueObject
                    {
                        Id = 123,
                        StringValue = "123"
                    });
                });
            }

            var newRealmConfig = new RealmConfiguration(oldRealmConfig.DatabasePath)
            {
                SchemaVersion = 1,
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    var value = migration.NewRealm.Find<IntPrimaryKeyWithValueObject>(123);
                    value.Id = 456;
                }
            };

            using var realm = GetRealm(newRealmConfig);

            var obj123 = realm.Find<IntPrimaryKeyWithValueObject>(123);
            var obj456 = realm.Find<IntPrimaryKeyWithValueObject>(456);

            Assert.That(obj123, Is.Null);
            Assert.That(obj456, Is.Not.Null);
            Assert.That(obj456.StringValue, Is.EqualTo("123"));
        }

        [Test]
        public void Migration_ChangePrimaryKeyType()
        {
            var oldRealmConfig = new RealmConfiguration(Guid.NewGuid().ToString())
            {
                Schema = new[] { typeof(ObjectV1) }
            };

            using (var oldRealm = GetRealm(oldRealmConfig))
            {
                oldRealm.Write(() =>
                {
                    oldRealm.Add(new ObjectV1
                    {
                        Id = 1,
                        Value = "foo"
                    });

                    oldRealm.Add(new ObjectV1
                    {
                        Id = 2,
                        Value = "bar"
                    });
                });
            }

            var newRealmConfig = new RealmConfiguration(oldRealmConfig.DatabasePath)
            {
                SchemaVersion = 1,
                Schema = new[] { typeof(ObjectV2) },
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    foreach (var oldObj in (IQueryable<RealmObject>)migration.OldRealm.DynamicApi.All("Object"))
                    {
                        var newObj = (ObjectV2)migration.NewRealm.ResolveReference(ThreadSafeReference.Create(oldObj));
                        newObj.Id = oldObj.DynamicApi.Get<int>("Id").ToString();
                    }
                }
            };

            using var realm = GetRealm(newRealmConfig);

            Assert.That(realm.All<ObjectV2>().AsEnumerable().Select(o => o.Value), Is.EquivalentTo(new[] { "foo", "bar" }));
            Assert.That(realm.All<ObjectV2>().AsEnumerable().Select(o => o.Id), Is.EquivalentTo(new[] { "1", "2" }));
        }

        [Test]
        public void Migration_ChangePrimaryKey_WithDuplicates_Throws()
        {
            var oldRealmConfig = new RealmConfiguration(Guid.NewGuid().ToString());
            using (var oldRealm = GetRealm(oldRealmConfig))
            {
                oldRealm.Write(() =>
                {
                    oldRealm.Add(new IntPrimaryKeyWithValueObject
                    {
                        Id = 1,
                        StringValue = "1"
                    });
                    oldRealm.Add(new IntPrimaryKeyWithValueObject
                    {
                        Id = 2,
                        StringValue = "2"
                    });
                });
            }

            var newRealmConfig = new RealmConfiguration(oldRealmConfig.DatabasePath)
            {
                SchemaVersion = 1,
                MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    var value = migration.NewRealm.Find<IntPrimaryKeyWithValueObject>(1);
                    value.Id = 2;
                }
            };

            var ex = Assert.Throws<RealmDuplicatePrimaryKeyValueException>(() => GetRealm(newRealmConfig));
            Assert.That(ex.Message, Does.Contain($"{nameof(IntPrimaryKeyWithValueObject)}._id"));

            // Ensure we haven't messed up the data
            using var oldRealmAgain = GetRealm(oldRealmConfig);

            var obj1 = oldRealmAgain.Find<IntPrimaryKeyWithValueObject>(1);
            var obj2 = oldRealmAgain.Find<IntPrimaryKeyWithValueObject>(2);

            Assert.That(obj1.StringValue, Is.EqualTo("1"));
            Assert.That(obj2.StringValue, Is.EqualTo("2"));
        }

        [Test]
        public void Migration_FromV3GuidFile()
        {
            var configuration = new RealmConfiguration(Guid.NewGuid().ToString())
            {
                Schema = new[] { typeof(GuidType), typeof(EmbeddedGuidType) }
            };

            TestHelpers.CopyBundledFileToDocuments("guids.realm", configuration.DatabasePath);

            var expected = GetGuidObjects().ToArray();
            using var realm = GetRealm(configuration);

            var actual = realm.All<GuidType>().ToArray();

            Assert.That(actual.Length, Is.EqualTo(expected.Length));

            foreach (var expectedObj in expected)
            {
                var actualObj = actual.Single(o => o.Id == expectedObj.Id);

                Assert.That(actualObj.Id, Is.EqualTo(expectedObj.Id));

                Assert.That(actualObj.RegularProperty, Is.EqualTo(expectedObj.RegularProperty));
                CollectionAssert.AreEqual(actualObj.GuidList, expectedObj.GuidList);
                CollectionAssert.AreEquivalent(actualObj.GuidSet, expectedObj.GuidSet);
                CollectionAssert.AreEquivalent(actualObj.GuidDict, expectedObj.GuidDict);

                Assert.That(actualObj.OptionalProperty, Is.EqualTo(expectedObj.OptionalProperty));
                CollectionAssert.AreEqual(actualObj.OptionalList, expectedObj.OptionalList);
                CollectionAssert.AreEquivalent(actualObj.OptionalSet, expectedObj.OptionalSet);
                CollectionAssert.AreEquivalent(actualObj.OptionalDict, expectedObj.OptionalDict);

                Assert.That(actualObj.MixedProperty, Is.EqualTo(expectedObj.MixedProperty));
                CollectionAssert.AreEqual(actualObj.MixedList, expectedObj.MixedList);
                CollectionAssert.AreEquivalent(actualObj.MixedSet, expectedObj.MixedSet);
                CollectionAssert.AreEquivalent(actualObj.MixedDict, expectedObj.MixedDict);

                Assert.That(actualObj.LinkProperty?.Id, Is.EqualTo(expectedObj.LinkProperty?.Id));

                var actualFound = realm.Find<GuidType>(expectedObj.Id);
                Assert.That(actualObj, Is.EqualTo(actualFound));
            }
        }

        private static IEnumerable<GuidType> GetGuidObjects()
        {
            var embedded1 = new EmbeddedGuidType
            {
                RegularProperty = Guid.Parse("64073484-5c31-4dcc-8276-282a448f760b"),
                GuidList =
                {
                    Guid.Parse("f3c7428c-6fcb-4a25-a4e0-9eef5d695b6d"),
                    Guid.Parse("78e97bb2-e5fa-4256-856f-e6e304e79f3a"),
                },
                GuidSet =
                {
                    Guid.Parse("5ba8da18-4e97-4aa3-aabc-314d34616f1f"),
                    Guid.Parse("76c5ff1c-2f75-40c3-af22-e9f35d901c36"),
                },
                GuidDict =
                {
                    { "a", Guid.Parse("0771ea3c-19ec-479c-b109-db258266ffc2") },
                    { "b", Guid.Parse("4ad92d5b-27a8-46d2-aed9-e0b14f25a19e") }
                },
                OptionalProperty = Guid.Parse("2d45c27e-440c-4713-9d2a-e5756487a293"),
                OptionalList =
                {
                    Guid.Parse("b9199da2-4d7e-4c67-9ab9-ecdc9a7e0380"),
                    null,
                },
                OptionalSet =
                {
                    Guid.Parse("6b544e76-537c-43ee-9664-a3bc4e842632"),
                    null
                },
                OptionalDict =
                {
                    { "c", Guid.Parse("a0a7eabc-8877-443d-98ce-a4f29a16379d") },
                    { "d", null }
                },
                MixedProperty = Guid.Parse("de5a5cbc-a41e-472a-9a73-a4f8d98ece82"),
                MixedSet =
                {
                    Guid.Parse("2bed1e3d-56c4-453b-acfd-013827caa8b0"),
                    Guid.Parse("5eb30478-7dde-41e2-bad9-10d4ba713ed0").ToString(),
                    1.23456
                },
                MixedList =
                {
                    Guid.Parse("6362a871-d61a-4882-bb44-dc79b0456794"),
                    "abc",
                    999
                },
                MixedDict =
                {
                    { "a", Guid.Parse("03ddf81b-6ff6-4b74-8172-421b3cf1c638") },
                    { "b", DateTimeOffset.UtcNow }
                },
            };

            var first = new GuidType
            {
                Id = Guid.Parse("f8f37f1f-26c5-415e-b45c-12dbdc8478c8"),
                RegularProperty = Guid.Parse("527bf8dc-5452-493b-a091-f098d986f120"),
                GuidList =
                {
                    Guid.Parse("b897f2dc-e5aa-4328-8789-77a40e4b7bcf"),
                    Guid.Parse("d9d02d70-5d89-45cb-8ce8-04f54fb7d9fc"),
                },
                GuidSet =
                {
                    Guid.Parse("00000000-0000-0000-0000-000000000000"),
                    Guid.Parse("64073484-5c31-4dcc-8276-282a448f760b"),
                },
                GuidDict =
                {
                    { "a", Guid.Parse("f3c7428c-6fcb-4a25-a4e0-9eef5d695b6d") },
                    { "b", Guid.Parse("78e97bb2-e5fa-4256-856f-e6e304e79f3a") }
                },
                OptionalProperty = Guid.Parse("00000000-0000-0000-0000-000000000000"),
                OptionalList =
                {
                    Guid.Parse("09367b97-4ecf-4391-ae4a-44d85f8dc27f"),
                    null,
                },
                OptionalSet =
                {
                    Guid.Parse("e89d691a-fed9-4a6f-9028-566c06dee2fc"),
                    null
                },
                OptionalDict =
                {
                    { "c", Guid.Parse("6700d95f-5e49-4331-bae0-03b9b0994549") },
                    { "d", null }
                },
                MixedProperty = Guid.Parse("5ba8da18-4e97-4aa3-aabc-314d34616f1f"),
                MixedSet =
                {
                    Guid.Parse("76c5ff1c-2f75-40c3-af22-e9f35d901c36"),
                    Guid.Parse("0771ea3c-19ec-479c-b109-db258266ffc2").ToString(),
                    1.23456
                },
                MixedList =
                {
                    Guid.Parse("4ad92d5b-27a8-46d2-aed9-e0b14f25a19e"),
                    "abc",
                    999
                },
                MixedDict =
                {
                    { "a", Guid.Parse("de5a5cbc-a41e-472a-9a73-a4f8d98ece82") },
                    { "b", DateTimeOffset.UtcNow }
                },
                EmbeddedProperty = embedded1
            };

            yield return first;

            var embedded2 = new EmbeddedGuidType
            {
                RegularProperty = Guid.Parse("2bed1e3d-56c4-453b-acfd-013827caa8b0"),
                GuidList =
                {
                    Guid.Parse("5eb30478-7dde-41e2-bad9-10d4ba713ed0"),
                    Guid.Parse("6362a871-d61a-4882-bb44-dc79b0456794"),
                },
                GuidSet =
                {
                    Guid.Parse("03ddf81b-6ff6-4b74-8172-421b3cf1c638"),
                    Guid.Parse("f8f37f1f-26c5-415e-b45c-12dbdc8478c8"),
                },
                GuidDict =
                {
                    { "a", Guid.Parse("527bf8dc-5452-493b-a091-f098d986f120") },
                    { "b", Guid.Parse("b897f2dc-e5aa-4328-8789-77a40e4b7bcf") }
                },
                OptionalProperty = null,
                OptionalList =
                {
                    null,
                },
                OptionalSet =
                {
                    null
                },
                OptionalDict =
                {
                    { "d", null }
                },
                MixedProperty = Guid.Parse("162a5dd1-48b5-47b3-8b6a-73ebdb9cce69"),
                MixedSet =
                {
                    Guid.Parse("ee3ee889-78e3-40f3-89c7-02b43f571b42"),
                    Guid.Parse("6f043170-fb5c-4a5c-bd06-72b4477a58a0").ToString(),
                    1.23456
                },
                MixedList =
                {
                    Guid.Parse("54d27013-7717-4bbf-9f68-17e05a4cf4d5"),
                    "abc",
                    999
                },
                MixedDict =
                {
                    { "a", Guid.Parse("8a453631-dd37-4881-a292-5e83198b1bb5") },
                    { "b", DateTimeOffset.UtcNow }
                },
                LinkProperty = first,
            };

            yield return new GuidType
            {
                Id = Guid.Parse("60325508-b005-46ae-8223-b9fae925b9d3"),
                LinkProperty = first,
                EmbeddedProperty = embedded2,
                OptionalProperty = null,
                OptionalList =
                {
                    null,
                },
                OptionalSet =
                {
                    null
                },
                OptionalDict =
                {
                    { "d", null }
                },
            };
        }

        private static class GuidExtensions
        {
            public static bool AreEqual<T>(ICollection<T> first, ICollection<T> second)
            {
                if ((first == null) != (second == null))
                {
                    return false;
                }

                if (first.Count != second.Count)
                {
                    return false;
                }

                for (var i = 0; i < first.Count; i++)
                {
                    if (!EqualityComparer<T>.Default.Equals(first.ElementAt(i), second.ElementAt(i)))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public class GuidType : RealmObject
        {
            [PrimaryKey]
            public Guid Id { get; set; }

            public Guid RegularProperty { get; set; }

            public IList<Guid> GuidList { get; }

            public ISet<Guid> GuidSet { get; }

            public IDictionary<string, Guid> GuidDict { get; }

            public Guid? OptionalProperty { get; set; }

            public IList<Guid?> OptionalList { get; }

            public ISet<Guid?> OptionalSet { get; }

            public IDictionary<string, Guid?> OptionalDict { get; }

            public GuidType LinkProperty { get; set; }

            public RealmValue MixedProperty { get; set; }

            public IList<RealmValue> MixedList { get; }

            public ISet<RealmValue> MixedSet { get; }

            public IDictionary<string, RealmValue> MixedDict { get; }

            public EmbeddedGuidType EmbeddedProperty { get; set; }

            public override bool Equals(object obj)
            {
                if (obj is GuidType guid)
                {
                    return Id == guid.Id &&
                        RegularProperty == guid.RegularProperty &&
                        GuidList.SequenceEqual(guid.GuidList) &&
                        GuidSet.SequenceEqual(guid.GuidSet) &&
                        GuidDict.SequenceEqual(guid.GuidDict) &&
                        OptionalProperty == guid.OptionalProperty &&
                        OptionalList.SequenceEqual(guid.OptionalList) &&
                        OptionalSet.SequenceEqual(guid.OptionalSet) &&
                        OptionalDict.SequenceEqual(guid.OptionalDict) &&
                        LinkProperty == guid.LinkProperty &&
                        MixedProperty == guid.MixedProperty &&
                        MixedList.SequenceEqual(guid.MixedList) &&
                        MixedSet.SequenceEqual(guid.MixedSet) &&
                        MixedDict.SequenceEqual(guid.MixedDict) &&
                        EmbeddedProperty == guid.EmbeddedProperty;
                }

                return false;
            }
        }

        public class EmbeddedGuidType : EmbeddedObject
        {
            public Guid RegularProperty { get; set; }

            public IList<Guid> GuidList { get; }

            public ISet<Guid> GuidSet { get; }

            public IDictionary<string, Guid> GuidDict { get; }

            public Guid? OptionalProperty { get; set; }

            public IList<Guid?> OptionalList { get; }

            public ISet<Guid?> OptionalSet { get; }

            public IDictionary<string, Guid?> OptionalDict { get; }

            public GuidType LinkProperty { get; set; }

            public RealmValue MixedProperty { get; set; }

            public IList<RealmValue> MixedList { get; }

            public ISet<RealmValue> MixedSet { get; }

            public IDictionary<string, RealmValue> MixedDict { get; }

            public override bool Equals(object obj)
            {
                if (obj is EmbeddedGuidType guid)
                {
                    return RegularProperty == guid.RegularProperty &&
                        GuidList.SequenceEqual(guid.GuidList) &&
                        GuidSet.SequenceEqual(guid.GuidSet) &&
                        GuidDict.SequenceEqual(guid.GuidDict) &&
                        OptionalProperty == guid.OptionalProperty &&
                        OptionalList.SequenceEqual(guid.OptionalList) &&
                        OptionalSet.SequenceEqual(guid.OptionalSet) &&
                        OptionalDict.SequenceEqual(guid.OptionalDict) &&
                        LinkProperty == guid.LinkProperty &&
                        MixedProperty == guid.MixedProperty &&
                        MixedList.SequenceEqual(guid.MixedList) &&
                        MixedSet.SequenceEqual(guid.MixedSet) &&
                        MixedDict.SequenceEqual(guid.MixedDict);
                }

                return false;
            }
        }

        [Explicit]
        [MapTo("Object")]
        private class ObjectV1 : RealmObject
        {
            [PrimaryKey]
            public int Id { get; set; }

            public string Value { get; set; }
        }

        [Explicit]
        [MapTo("Object")]
        private class ObjectV2 : RealmObject
        {
            [PrimaryKey]
            public string Id { get; set; }

            public string Value { get; set; }
        }
    }
}
