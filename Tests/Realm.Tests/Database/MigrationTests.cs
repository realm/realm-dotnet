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

#if TEST_WEAVER
using TestEmbeddedObject = Realms.EmbeddedObject;
using TestRealmObject = Realms.RealmObject;
#else
using TestEmbeddedObject = Realms.IEmbeddedObject;
using TestRealmObject = Realms.IRealmObject;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Realms.Exceptions;
using Realms.Extensions;
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

                    var oldPeople = migration.OldRealm.DynamicApi.All("Person");
                    var newPeople = migration.NewRealm.All<Person>();

                    Assert.That(newPeople.Count(), Is.EqualTo(oldPeople.Count()));

                    for (var i = 0; i < newPeople.Count(); i++)
                    {
                        var oldPerson = oldPeople.ElementAt(i);
                        var newPerson = newPeople.ElementAt(i);

                        Assert.That(newPerson.LastName, Is.Not.EqualTo(oldPerson.DynamicApi.Get<string>("TriggersSchema")));
                        newPerson.LastName = triggersSchemaFieldValue = oldPerson.DynamicApi.Get<string>("TriggersSchema");

#if !UNITY
                        // Ensure we can still use the dynamic API during migrations
                        dynamic dynamicOldPerson = oldPeople.ElementAt(i);
                        Assert.That(dynamicOldPerson.TriggersSchema, Is.EqualTo(oldPerson.DynamicApi.Get<string>("TriggersSchema")));
#endif
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
                MigrationCallback = (_, _) => throw dummyException
            };

            TestHelpers.CopyBundledFileToDocuments(FileToMigrate, configuration.DatabasePath);

            var ex = Assert.Throws<AggregateException>(() => GetRealm(configuration))!;
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
                    new ObjectSchema.Builder("Person")
                    {
                        Property.FromType<string>("Name")
                    }
                }
            };

            using (var realm = GetRealm(oldConfig))
            {
                realm.Write(() =>
                {
                    var person = realm.DynamicApi.CreateObject("Person");
                    person.DynamicApi.Set("Name", "Foo");
                });
            }

            var newConfig = new RealmConfiguration(path)
            {
                IsDynamic = true,
                ShouldDeleteIfMigrationNeeded = true,
                Schema = new RealmSchema.Builder
                {
                    new ObjectSchema.Builder("Person")
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
                    var person = realm.DynamicApi.CreateObject("Person");
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
                MigrationCallback = (migration, _) =>
                {
                    migration.RenameProperty(nameof(Person), "TriggersSchema", nameof(Person.OptionalAddress));

                    var oldPeople = migration.OldRealm.DynamicApi.All("Person");
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
            var configuration = new RealmConfiguration(Guid.NewGuid().ToString())
            {
                SchemaVersion = 100,
                MigrationCallback = (migration, _) =>
                {
                    migration.RenameProperty(nameof(Person), "TriggersSchema", "PropertyNotInNewSchema");
                }
            };

            TestHelpers.CopyBundledFileToDocuments(FileToMigrate, configuration.DatabasePath);

            var ex = Assert.Throws<ArgumentException>(() => GetRealm(configuration))!;
            Assert.That(ex.Message, Does.Contain("Renamed property 'Person.PropertyNotInNewSchema' does not exist"));

            configuration = new RealmConfiguration(configuration.DatabasePath)
            {
                SchemaVersion = 100,
                MigrationCallback = (migration, _) =>
                {
                    migration.RenameProperty(nameof(Person), "PropertyNotInOldSchema", nameof(Person.OptionalAddress));
                }
            };

            var ex2 = Assert.Throws<AggregateException>(() => GetRealm(configuration))!;
            Assert.That(ex2.Flatten().InnerException!.Message, Does.Contain("Cannot rename property 'Person.PropertyNotInOldSchema' because it does not exist"));

            configuration = new RealmConfiguration(configuration.DatabasePath)
            {
                SchemaVersion = 100,
                MigrationCallback = (migration, _) =>
                {
                    migration.RenameProperty("NonExistingType", "TriggersSchema", nameof(Person.OptionalAddress));
                }
            };

            ex2 = Assert.Throws<AggregateException>(() => GetRealm(configuration))!;
            Assert.That(ex2.Flatten().InnerException!.Message, Does.Contain("Cannot rename properties for type 'NonExistingType' because it does not exist"));

            configuration = new RealmConfiguration(configuration.DatabasePath)
            {
                SchemaVersion = 100,
                MigrationCallback = (migration, _) =>
                {
                    migration.RenameProperty(nameof(Person), "TriggersSchema", nameof(Person.Birthday));
                }
            };

            ex2 = Assert.Throws<AggregateException>(() => GetRealm(configuration))!;
            Assert.That(ex2.Flatten().InnerException!.Message, Does.Contain("Cannot rename property 'Person.TriggersSchema' to 'Birthday' because it would change from type 'string' to 'date'."));

            configuration = new RealmConfiguration(configuration.DatabasePath)
            {
                SchemaVersion = 100,
                MigrationCallback = (migration, _) =>
                {
                    migration.RenameProperty(nameof(Person), nameof(Person.Latitude), nameof(Person.Longitude));
                }
            };

            ex2 = Assert.Throws<AggregateException>(() => GetRealm(configuration))!;
            Assert.That(ex2.Flatten().InnerException!.Message, Does.Contain("Cannot rename property 'Person.Latitude' to 'Longitude' because the source property still exists."));
        }

        [Test]
        public void MigrationRenamePropertyInvalidArguments()
        {
            var configuration = new RealmConfiguration(Guid.NewGuid().ToString())
            {
                SchemaVersion = 100,
                MigrationCallback = (migration, _) =>
                {
                    Assert.Throws<ArgumentNullException>(() => migration.RenameProperty(null!, "TriggersSchema", "OptionalAddress"));
                    Assert.Throws<ArgumentNullException>(() => migration.RenameProperty("Person", null!, "OptionalAddress"));
                    Assert.Throws<ArgumentNullException>(() => migration.RenameProperty("Person", "TriggersSchema", null!));

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

            using (GetRealm(oldRealmConfig))
            {
            }

            var newRealmConfig = new RealmConfiguration(oldRealmConfig.DatabasePath)
            {
                SchemaVersion = 1,
                Schema = new[] { typeof(PrimaryKeyObjectIdObject), typeof(Person) },
                MigrationCallback = (migration, _) =>
                {
                    migration.RemoveType(nameof(Person));
                }
            };

            var ex = Assert.Throws<AggregateException>(() => GetRealm(newRealmConfig))!;
            Assert.That(ex.Flatten().InnerException!.Message, Does.Contain("Attempted to remove type 'Person', that is present in the current schema"));
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
                MigrationCallback = (migration, _) =>
                {
                    migrationCallbackCalled = true;

                    var migrationResultNotInSchema = migration.RemoveType("NotInSchemaType");
                    Assert.That(migrationResultNotInSchema, Is.False);

                    var migrationResult = migration.RemoveType(nameof(Person));
                    Assert.That(migrationResult, Is.True);

                    // Removed type in oldRealm is still available for the duration of the migration
                    var oldPeople = migration.OldRealm.DynamicApi.All("Person");
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

            using (GetRealm(oldRealmConfig))
            {
            }

            var newRealmConfig = new RealmConfiguration(oldRealmConfig.DatabasePath)
            {
                SchemaVersion = 1,
                Schema = new[] { typeof(PrimaryKeyObjectIdObject), typeof(Person) },
                MigrationCallback = (migration, _) =>
                {
                    Assert.Throws<ArgumentNullException>(() => migration.RemoveType(null!));
                    Assert.Throws<ArgumentException>(() => migration.RemoveType(string.Empty));
                }
            };

            using var realm = GetRealm(newRealmConfig);
        }

        [Test]
        public void Migration_WhenDone_DisposesAllObjectsAndLists()
        {
            var config = new RealmConfiguration(Guid.NewGuid().ToString());
            using (GetRealm(config))
            {
            }

            AllTypesObject standaloneObject = null!;
            EmbeddedAllTypesObject embeddedObject = null!;
            RealmList<string> list = null!;
            RealmSet<string> set = null!;
            RealmDictionary<string> dict = null!;
            RealmResults<AllTypesObject> query = null!;

            var newConfig = new RealmConfiguration(config.DatabasePath)
            {
                SchemaVersion = 1,
                MigrationCallback = (migration, _) =>
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

            using (Realm.GetInstance(newConfig))
            {
                // Here we should see all objects accessed during the migration get disposed, even though
                // newRealm is still open.
                Assert.That(standaloneObject.IsValid, Is.False);
                Assert.That(standaloneObject.IsManaged);
                Assert.Throws<RealmClosedException>(() => _ = standaloneObject.Int32Property);
                Assert.That(standaloneObject.GetObjectHandle()!.IsClosed);

                Assert.That(embeddedObject.IsValid, Is.False);
                Assert.That(embeddedObject.IsManaged);
                Assert.Throws<RealmClosedException>(() => _ = embeddedObject.Int32Property);
                Assert.That(embeddedObject.GetObjectHandle()!.IsClosed);

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
                MigrationCallback = (migration, _) =>
                {
                    var value = migration.NewRealm.DynamicApi.Find(nameof(IntPrimaryKeyWithValueObject), 123)!;
                    value.DynamicApi.Set("_id", 456);
                }
            };

            using var realm = GetRealm(newRealmConfig);

            var obj123 = realm.Find<IntPrimaryKeyWithValueObject>(123);
            var obj456 = realm.Find<IntPrimaryKeyWithValueObject>(456);

            Assert.That(obj123, Is.Null);
            Assert.That(obj456, Is.Not.Null);
            Assert.That(obj456!.StringValue, Is.EqualTo("123"));
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
                MigrationCallback = (migration, _) =>
                {
                    var value = migration.NewRealm.Find<IntPrimaryKeyWithValueObject>(123)!;
                    value.Id = 456;
                }
            };

            using var realm = GetRealm(newRealmConfig);

            var obj123 = realm.Find<IntPrimaryKeyWithValueObject>(123);
            var obj456 = realm.Find<IntPrimaryKeyWithValueObject>(456);

            Assert.That(obj123, Is.Null);
            Assert.That(obj456, Is.Not.Null);
            Assert.That(obj456!.StringValue, Is.EqualTo("123"));
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
                MigrationCallback = (migration, _) =>
                {
                    foreach (var oldObj in migration.OldRealm.DynamicApi.All("Object"))
                    {
                        var newObj = (ObjectV2)migration.NewRealm.ResolveReference(ThreadSafeReference.Create(oldObj))!;
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
                MigrationCallback = (migration, _) =>
                {
                    var value = migration.NewRealm.Find<IntPrimaryKeyWithValueObject>(1)!;
                    value.Id = 2;
                }
            };

            var ex = Assert.Throws<RealmMigrationException>(() => GetRealm(newRealmConfig))!;
            Assert.That(ex.Message, Does.Contain($"{nameof(IntPrimaryKeyWithValueObject)}._id"));

            // Ensure we haven't messed up the data
            using var oldRealmAgain = GetRealm(oldRealmConfig);

            var obj1 = oldRealmAgain.Find<IntPrimaryKeyWithValueObject>(1)!;
            var obj2 = oldRealmAgain.Find<IntPrimaryKeyWithValueObject>(2)!;

            Assert.That(obj1.StringValue, Is.EqualTo("1"));
            Assert.That(obj2.StringValue, Is.EqualTo("2"));
        }

        [Test]
        public void Migration_ToEmbedded_DuplicatesObjects()
        {
            var oldRealmConfig = new RealmConfiguration(Guid.NewGuid().ToString())
            {
                Schema = new[] { typeof(ObjectV1), typeof(ObjectContainerV1) }
            };

            using (var oldRealm = GetRealm(oldRealmConfig))
            {
                oldRealm.Write(() =>
                {
                    var obj = oldRealm.Add(new ObjectV1
                    {
                        Id = 1,
                        Value = "foo"
                    });

                    oldRealm.Add(new ObjectContainerV1
                    {
                        Value = "container1",
                        Link = obj,
                        List = { obj, obj }
                    });

                    var obj2 = oldRealm.Add(new ObjectV1
                    {
                        Id = 2,
                        Value = "bar"
                    });

                    oldRealm.Add(new ObjectContainerV1
                    {
                        Value = "container2",
                        Link = obj,
                        List = { obj2, obj2, obj }
                    });
                });
            }

            var newRealmConfig = new RealmConfiguration(oldRealmConfig.DatabasePath)
            {
                SchemaVersion = 1,
                Schema = new[] { typeof(ObjectContainerEmbedded), typeof(ObjectEmbedded) },
            };

            using var newRealm = GetRealm(newRealmConfig);
            var container = newRealm.All<ObjectContainerEmbedded>().Single(c => c.Value == "container1");

            Assert.That(container.Link, Is.Not.Null);
            Assert.That(container.Link!.Value, Is.EqualTo("foo"));
            Assert.That(container.List.Select(l => l.Value), Is.EquivalentTo(new[] { "foo", "foo" }));

            var container2 = newRealm.All<ObjectContainerEmbedded>().Single(c => c.Value == "container2");
            Assert.That(container2.Link, Is.Not.Null);
            Assert.That(container2.Link!.Value, Is.EqualTo("foo"));
            Assert.That(container2.List.Select(l => l.Value), Is.EquivalentTo(new[] { "bar", "bar", "foo" }));

            Assert.That(newRealm.AllEmbedded<ObjectEmbedded>().Count(), Is.EqualTo(7));
        }

        [Test]
        public void Migration_ToEmbedded_DeletesOrphans()
        {
            var oldRealmConfig = new RealmConfiguration(Guid.NewGuid().ToString())
            {
                Schema = new[] { typeof(ObjectV1), typeof(ObjectContainerV1) }
            };

            using (var oldRealm = GetRealm(oldRealmConfig))
            {
                oldRealm.Write(() =>
                {
                    var obj = oldRealm.Add(new ObjectV1
                    {
                        Id = 1,
                        Value = "foo"
                    });

                    oldRealm.Add(new ObjectV1
                    {
                        Id = 2,
                        Value = "bar"
                    });

                    oldRealm.Add(new ObjectContainerV1
                    {
                        Value = "container",
                        Link = obj
                    });
                });
            }

            var newRealmConfig = new RealmConfiguration(oldRealmConfig.DatabasePath)
            {
                SchemaVersion = 1,
                Schema = new[] { typeof(ObjectContainerEmbedded), typeof(ObjectEmbedded) },
            };

            using var newRealm = GetRealm(newRealmConfig);
            var container = newRealm.All<ObjectContainerEmbedded>().Single();

            Assert.That(container.Value, Is.EqualTo("container"));
            Assert.That(container.Link!.Value, Is.EqualTo("foo"));

            Assert.That(newRealm.AllEmbedded<ObjectEmbedded>().Count(), Is.EqualTo(1));
            Assert.That(newRealm.AllEmbedded<ObjectEmbedded>().Any(e => e.Value == "bar"), Is.False);
        }

        [Test]
        public void Migration_FindInNewRealm_WhenObjectIsDeleted_ReturnsNull()
        {
            var oldConfig = new RealmConfiguration(Guid.NewGuid().ToString())
            {
                Schema = new[]
                {
                    typeof(Dotnet_3597_Old)
                }
            };

            using (var oldRealm = GetRealm(oldConfig))
            {
                oldRealm.Write(() =>
                {
                    oldRealm.Add(new Dotnet_3597_Old { FloatProp = 1, IntProp = 1 });
                    oldRealm.Add(new Dotnet_3597_Old { FloatProp = 2, IntProp = 2 });
                });
            }

            var newConfig = new RealmConfiguration(oldConfig.DatabasePath)
            {
                Schema = new[]
                {
                    typeof(Dotnet_3597)
                },
                SchemaVersion = 2,
                MigrationCallback = (migration, _) =>
                {
                    var oldObjects = migration.OldRealm.DynamicApi.All(nameof(Dotnet_3597)).ToArray();
                    var old1 = oldObjects.First(o => o.DynamicApi.Get<int>("IntProp") == 1);
                    var old2 = oldObjects.First(o => o.DynamicApi.Get<int>("IntProp") == 2);

                    var newObjects = migration.NewRealm.All<Dotnet_3597>().ToArray();
                    var new1 = newObjects.First(o => o.IntProp == 1);
                    var new2 = newObjects.First(o => o.IntProp == 2);

                    Assert.That(migration.FindInNewRealm<Dotnet_3597>(old1), Is.EqualTo(new1));
                    Assert.That(migration.FindInNewRealm<Dotnet_3597>(old2), Is.EqualTo(new2));

                    migration.NewRealm.Remove(new1);

                    Assert.That(migration.FindInNewRealm<Dotnet_3597>(old1), Is.Null);
                }
            };

            var newRealm = GetRealm(newConfig);
            Assert.That(newRealm.All<Dotnet_3597>().Count(), Is.EqualTo(1));
        }

        // Test for https://github.com/realm/realm-dotnet/issues/3597
        [Test]
        public void Migration_MigratesListOfFloats()
        {
            var oldConfig = new RealmConfiguration(Guid.NewGuid().ToString())
            {
                Schema = new[]
                {
                    typeof(Dotnet_3597_Old)
                }
            };

            using (var oldRealm = GetRealm(oldConfig))
            {
                oldRealm.Write(() =>
                {
                    oldRealm.Add(new Dotnet_3597_Old
                    {
                        FloatProp = 3.14f,
                        FloatList =
                        {
                            1,
                            2,
                            -1.23f
                        }
                    });
                });
            }

            var newConfig = new RealmConfiguration(oldConfig.DatabasePath)
            {
                Schema = new[]
                {
                    typeof(Dotnet_3597)
                },
                SchemaVersion = 2,
                MigrationCallback = (migration, _) =>
                {
                    foreach (var item in migration.OldRealm.DynamicApi.All(nameof(Dotnet_3597)))
                    {
                        var newItem = migration.FindInNewRealm<Dotnet_3597>(item)!;
                        newItem.FloatProp = item.DynamicApi.Get<float?>("FloatProp").ToString()!;
                        foreach (var floatValue in item.DynamicApi.GetList<float?>("FloatList"))
                        {
                            newItem.FloatList.Add(floatValue.ToString()!);
                        }
                    }
                }
            };

            var newRealm = GetRealm(newConfig);

            var obj = newRealm.All<Dotnet_3597>().Single();
            Assert.That(obj.FloatProp, Is.EqualTo("3.14"));
            CollectionAssert.AreEqual(obj.FloatList, new[] { "1", "2", "-1.23" });
        }
    }

    [Explicit]
    [MapTo("ObjectContainer")]
    public partial class ObjectContainerV1 : TestRealmObject
    {
        public string? Value { get; set; }

        public ObjectV1? Link { get; set; }

        public IList<ObjectV1> List { get; } = null!;
    }

    [Explicit]
    [MapTo("ObjectContainer")]
    public partial class ObjectContainerEmbedded : TestRealmObject
    {
        public string? Value { get; set; }

        public ObjectEmbedded? Link { get; set; }

        public IList<ObjectEmbedded> List { get; } = null!;
    }

    [Explicit]
    [MapTo("Object")]
    public partial class ObjectV1 : TestRealmObject
    {
        [PrimaryKey]
        public int Id { get; set; }

        public string? Value { get; set; }
    }

    [Explicit]
    [MapTo("Object")]
    public partial class ObjectV2 : TestRealmObject
    {
        [PrimaryKey]
        public string? Id { get; set; }

        public string? Value { get; set; }
    }

    [Explicit]
    [MapTo("Object")]
    public partial class ObjectEmbedded : TestEmbeddedObject
    {
        public string? Value { get; set; }
    }

    [Explicit]
    [MapTo("Dotnet_3597")]
    public partial class Dotnet_3597_Old : TestRealmObject
    {
        public int IntProp { get; set; }

        public float? FloatProp { get; set; }

        public IList<float?> FloatList { get; } = null!;
    }

    [Explicit]
    [MapTo("Dotnet_3597")]
    public partial class Dotnet_3597 : TestRealmObject
    {
        public int IntProp { get; set; }

        public string FloatProp { get; set; } = string.Empty;

        public IList<string> FloatList { get; } = null!;
    }
}
