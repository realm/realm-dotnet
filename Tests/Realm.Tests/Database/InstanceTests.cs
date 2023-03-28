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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using Realms.Exceptions;
using Realms.Logging;
using Realms.Schema;
#if TEST_WEAVER
using TestRealmObject = Realms.RealmObject;
#else
using TestRealmObject = Realms.IRealmObject;
#endif

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class InstanceTests : RealmTest
    {
        [Test]
        public void GetInstanceTest()
        {
            // Arrange, act and "assert" that no exception is thrown, using default location
            GetRealm().Dispose();
        }

        [Test]
        public void ReadOnlyInstance_ThrowsOnRefresh()
        {
            var config = new RealmConfiguration(Guid.NewGuid().ToString());
            var realm = GetRealm(config);
            realm.Dispose();

            config.IsReadOnly = true;

            realm = GetRealm(config);
            Assert.That(() => realm.Refresh(), Throws.TypeOf<RealmInvalidTransactionException>().And.Message.Contains("Can't refresh an immutable Realm."));
        }

        [Test]
        public void GetTwice_ReadOnlyInstance_DoesNotThrow()
        {
            var config = new RealmConfiguration(Guid.NewGuid().ToString());
            var realm = GetRealm(config);
            realm.Dispose();

            config.IsReadOnly = true;

            _ = GetRealm(config);
            Assert.DoesNotThrow(() => GetRealm(config));
        }

        [Test]
        public void InstanceIsClosedByDispose()
        {
            Realm temp;
            using (temp = GetRealm())
            {
                Assert.That(!temp.IsClosed);
            }

            Assert.That(temp.IsClosed);
        }

        [Test]
        public void GetInstanceWithJustFilenameTest()
        {
            var filename = Guid.NewGuid().ToString();

            using (GetRealm(filename))
            {
            }
        }

        [Test]
        public void DeleteRealmWorksIfClosed()
        {
            // Arrange
            var config = RealmConfiguration.DefaultConfiguration;
            var openRealm = GetRealm(config);

            // Act
            openRealm.Dispose();

            // Assert
            Assert.That(File.Exists(config.DatabasePath));
            Assert.DoesNotThrow(() => Realm.DeleteRealm(config));
            Assert.That(File.Exists(config.DatabasePath), Is.False);
        }

        [Test]
        public void DeleteRealmWorksIfCalledMultipleTimes()
        {
            // Arrange
            var config = RealmConfiguration.DefaultConfiguration;
            var openRealm = GetRealm(config);

            // Act
            openRealm.Dispose();

            // Assert
            Assert.That(File.Exists(config.DatabasePath));
            Assert.DoesNotThrow(() => Realm.DeleteRealm(config));
            Assert.That(File.Exists(config.DatabasePath), Is.False);
            Assert.DoesNotThrow(() => Realm.DeleteRealm(config));
        }

        [Test]
        public void DeleteRealmWorksIfFolderDoesntExist()
        {
            var config = RealmConfiguration.DefaultConfiguration;
            var dbFolder = Path.GetDirectoryName(config.DatabasePath)!;
            var nonExistingRealm = Path.Combine(dbFolder, "idontexist", "my.realm");
            var newConfig = new RealmConfiguration(nonExistingRealm);

            Assert.DoesNotThrow(() => Realm.DeleteRealm(newConfig));
        }

        [Test]
        public void GetUniqueInstancesDifferentThreads()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                Realm realm1 = null!;
                Realm realm2 = null!;
                try
                {
                    // Arrange
                    realm1 = GetRealm();

                    // Act
                    await Task.Run(() =>
                    {
                        realm2 = GetRealm();
                    });

                    // Assert
                    Assert.That(ReferenceEquals(realm1, realm2), Is.False, "ReferenceEquals");
                    Assert.That(realm1.IsSameInstance(realm2), Is.False, "IsSameInstance");
                    Assert.That(realm1, Is.EqualTo(realm2), "IsEqualTo");  // equal and same Realm but not same instance
                }
                finally
                {
                    realm1.Dispose();
                    realm2.Dispose();
                }
            });
        }

        [Test]
        public void GetCachedInstancesSameThread()
        {
            // Arrange
            using var realm1 = GetRealm();
            using var realm2 = GetRealm();

            // Assert
            Assert.That(ReferenceEquals(realm1, realm2), Is.False);
            Assert.That(realm1, Is.EqualTo(realm1));  // check equality with self
            Assert.That(realm1.IsSameInstance(realm2));
            Assert.That(realm1, Is.EqualTo(realm2));
        }

        [Test]
        public void InstancesHaveDifferentHashes()
        {
            // Arrange
            using var realm1 = GetRealm();
            using var realm2 = GetRealm();

            // Assert
            Assert.That(ReferenceEquals(realm1, realm2), Is.False);
            Assert.That(realm1.GetHashCode(), Is.Not.EqualTo(0));
            Assert.That(realm1.GetHashCode(), Is.Not.EqualTo(realm2.GetHashCode()));
        }

        [Test]
        public void TransactionStateIsCorrect()
        {
            // Arrange
            using var realm1 = GetRealm();
            using var ts1 = realm1.BeginWrite();

            // Assert
            Assert.That(ts1.State, Is.EqualTo(TransactionState.Running));
            ts1.Commit();
            Assert.That(ts1.State, Is.EqualTo(TransactionState.Committed));
            using var ts2 = realm1.BeginWrite();
            ts2.Rollback();
            Assert.That(ts2.State, Is.EqualTo(TransactionState.RolledBack));
        }

        [Test]
        public void TransactionStateIsCorrectAsync()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                // Arrange
                using var realm = GetRealm();
                var ts = await realm.BeginWriteAsync();

                // Assert
                Assert.That(ts.State, Is.EqualTo(TransactionState.Running));
                await ts.CommitAsync();
                Assert.That(ts.State, Is.EqualTo(TransactionState.Committed));
            });
        }

        [Test]
        public void DeleteRealmFailsIfOpenSameThread()
        {
            // Arrange
            using var openRealm = GetRealm();

            // Assert
            Assert.Throws<RealmInUseException>(() => Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration));
        }

        [Test]
        public void GetInstanceShouldThrowWithBadPath()
        {
            var path = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "C:\\Windows" : "/";

            // Arrange
            Assert.Throws<RealmPermissionDeniedException>(() => GetRealm(path));
        }

        [Test]
        public void RealmWithOneClassWritesDesiredClass()
        {
            // Arrange
            RealmConfiguration.DefaultConfiguration.Schema = new[] { typeof(LoneClass) };

            // Act
            using var lonelyRealm = GetRealm();
            lonelyRealm.Write(() =>
            {
                lonelyRealm.Add(new LoneClass
                {
                    Name = "The Singular"
                });
            });

            // Assert
            Assert.That(lonelyRealm.All<LoneClass>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void RealmWithOneClassThrowsIfUseOther()
        {
            // Arrange
            RealmConfiguration.DefaultConfiguration.Schema = new[] { typeof(LoneClass) };

            // Act and assert
            using var lonelyRealm = GetRealm();

            // Can't create an object with a class not included in this Realm
            lonelyRealm.Write(() =>
            {
                Assert.That(() => lonelyRealm.Add(new Person()), Throws.TypeOf<ArgumentException>());
            });
        }

        [Test]
        public void RealmObjectClassesOnlyAllowRealmObjects()
        {
            // Can't have classes in the list which are not RealmObjects
            var ex = Assert.Throws<ArgumentException>(() => _ = new RealmConfiguration
            {
                Schema = new[] { typeof(LoneClass), typeof(object) }
            })!;

            Assert.That(ex.Message, Does.Contain("System.Object"));
            Assert.That(ex.Message, Does.Contain("must descend directly from either RealmObject, EmbeddedObject, or AsymmetricObject"));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ShouldCompact_IsInvokedAfterOpening(bool shouldCompact)
        {
            var config = (RealmConfiguration)RealmConfiguration.DefaultConfiguration;

            using (var realm = GetRealm(config))
            {
                AddDummyData(realm);
            }

            var oldSize = new FileInfo(config.DatabasePath).Length;
            long projectedNewSize = 0;
            var hasPrompted = false;
            config.ShouldCompactOnLaunch = (totalBytes, bytesUsed) =>
            {
                Assert.That(totalBytes, Is.EqualTo(oldSize));
                hasPrompted = true;
                projectedNewSize = (long)bytesUsed;
                return shouldCompact;
            };

            using (var realm = GetRealm(config))
            {
                Assert.That(hasPrompted, Is.True);
                var newSize = new FileInfo(config.DatabasePath).Length;
                if (shouldCompact)
                {
                    // Less than or equal because of the new online compaction mechanism - it's possible
                    // that the Realm was already at the optimal size.
                    Assert.That(newSize, Is.LessThanOrEqualTo(oldSize));

                    // Less than 20% error in projections
                    Assert.That((newSize - projectedNewSize) / newSize, Is.LessThan(0.2));
                }
                else
                {
                    Assert.That(newSize, Is.EqualTo(oldSize));
                }

                Assert.That(realm.All<IntPrimaryKeyWithValueObject>().Count(), Is.EqualTo(DummyDataSize / 2));
            }
        }

        [TestCase(false, true)]
        [TestCase(false, false)]
        [TestCase(true, true)]
        [TestCase(true, false)]
        public void Compact_ShouldReduceSize(bool encrypt, bool populate)
        {
            var config = RealmConfiguration.DefaultConfiguration;
            if (encrypt)
            {
                config.EncryptionKey = TestHelpers.GetEncryptionKey(5);
            }

            using (var realm = GetRealm(config))
            {
                if (populate)
                {
                    AddDummyData(realm);
                }
            }

            var initialSize = new FileInfo(config.DatabasePath).Length;
            Assert.That(Realm.Compact(config));

            var finalSize = new FileInfo(config.DatabasePath).Length;
            Assert.That(initialSize >= finalSize);

            using (var realm = GetRealm(config))
            {
                Assert.That(realm.All<IntPrimaryKeyWithValueObject>().Count(), Is.EqualTo(populate ? DummyDataSize / 2 : 0));
            }
        }

        [Test]
        public void Compact_WhenInTransaction_ShouldThrow()
        {
            using var realm = GetRealm();
            Assert.That(() =>
            {
                realm.Write(() =>
                {
                    Realm.Compact();
                });
            }, Throws.TypeOf<RealmInvalidTransactionException>());
        }

        [Test]
        public void Compact_WhenOpenOnDifferentThread_ShouldReturnFalse()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                using var realm = GetRealm();
                AddDummyData(realm);

                var initialSize = new FileInfo(realm.Config.DatabasePath).Length;
                bool? isCompacted = null;
                await Task.Run(() =>
                {
                    isCompacted = Realm.Compact(realm.Config);
                });

                Assert.That(isCompacted, Is.False);
                var finalSize = new FileInfo(realm.Config.DatabasePath).Length;

                Assert.That(finalSize, Is.EqualTo(initialSize));
            });
        }

        [Test]
        public void Compact_WhenOpenOnSameThread_ShouldReturnTrue()
        {
            using var realm = GetRealm();

            var initialSize = new FileInfo(realm.Config.DatabasePath).Length;
            Assert.That(() => Realm.Compact(), Is.True);
            var finalSize = new FileInfo(realm.Config.DatabasePath).Length;
            Assert.That(finalSize, Is.LessThanOrEqualTo(initialSize));

            // Test that the Realm instance is still valid and we can write to it
            realm.Write(() =>
            {
                realm.Add(new Person());
            });
        }

        [Test]
        public void Compact_WhenResultsAreOpen_ShouldReturnFalse()
        {
            using var realm = GetRealm();

            var token = realm.All<Person>().SubscribeForNotifications((sender, changes) =>
            {
                Console.WriteLine(changes?.InsertedIndices);
            });

            Assert.That(() => Realm.Compact(), Is.False);
            token.Dispose();
        }

        [Test]
        public void RealmChangedShouldFireForEveryInstance()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                using var realm1 = GetRealm();
                using var realm2 = GetRealm();

                var changed1 = 0;
                realm1.RealmChanged += (sender, e) =>
                {
                    changed1++;
                };

                var changed2 = 0;
                realm2.RealmChanged += (sender, e) =>
                {
                    changed2++;
                };

                realm1.Write(() =>
                {
                    realm1.Add(new Person());
                });

                await Task.Delay(50);

                Assert.That(changed1, Is.EqualTo(1));
                Assert.That(changed2, Is.EqualTo(1));

                Assert.That(realm1.All<Person>().Count(), Is.EqualTo(1));
                Assert.That(realm2.All<Person>().Count(), Is.EqualTo(1));

                realm2.Write(() =>
                {
                    realm2.Add(new Person());
                });

                await Task.Delay(50);

                Assert.That(changed1, Is.EqualTo(2));
                Assert.That(changed2, Is.EqualTo(2));

                Assert.That(realm1.All<Person>().Count(), Is.EqualTo(2));
                Assert.That(realm2.All<Person>().Count(), Is.EqualTo(2));
            });
        }

        [Test]
        public void Dispose_WhenOnTheSameThread_ShouldNotInvalidateOtherInstances()
        {
            Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);

            var realm1 = GetRealm();
            var realm2 = GetRealm();

            realm1.Write(() => realm1.Add(new Person()));
            realm1.Dispose();

            var people = realm2.All<Person>();

            Assert.That(people.Count(), Is.EqualTo(1));

            realm2.Dispose();
        }

        [Test]
        public void Dispose_WhenCalledMultipletimes_ShouldNotInvalidateOtherInstances()
        {
            Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);

            var realm1 = GetRealm();
            var realm2 = GetRealm();

            realm1.Write(() => realm1.Add(new Person()));
            for (var i = 0; i < 5; i++)
            {
                realm1.Dispose();
            }

            var people = realm2.All<Person>();

            Assert.That(people.Count(), Is.EqualTo(1));

            realm2.Dispose();
        }

        [Test]
        public void Dispose_WhenOnDifferentThread_ShouldNotInvalidateOtherInstances()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);

                var realm1 = GetRealm();

                await Task.Run(() =>
                {
                    var realm2 = GetRealm();
                    realm2.Write(() => realm2.Add(new Person()));
                    realm2.Dispose();
                });

                realm1.Refresh();

                var people = realm1.All<Person>();

                Assert.That(people.Count(), Is.EqualTo(1));

                realm1.Dispose();
            });
        }

        [Test]
        public void UsingDisposedRealm_ShouldThrowObjectDisposedException()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var realm = GetRealm();
                realm.Dispose();

                Assert.That(realm.IsClosed);

                var other = GetRealm();

                Assert.That(() => realm.Add(new Person()), Throws.TypeOf<ObjectDisposedException>());
                Assert.That(() => realm.All<Person>(), Throws.TypeOf<ObjectDisposedException>());
                Assert.That(() => realm.DynamicApi.All(nameof(Person)), Throws.TypeOf<ObjectDisposedException>());
                Assert.That(() => realm.BeginWrite(), Throws.TypeOf<ObjectDisposedException>());
                Assert.That(() => realm.DynamicApi.CreateObject(nameof(Person)), Throws.TypeOf<ObjectDisposedException>());
                Assert.That(() => realm.Find<Person>(0), Throws.TypeOf<ObjectDisposedException>());
                Assert.That(() => realm.GetHashCode(), Throws.TypeOf<ObjectDisposedException>());
                Assert.That(() => realm.IsSameInstance(other), Throws.TypeOf<ObjectDisposedException>());
                Assert.That(() => realm.Refresh(), Throws.TypeOf<ObjectDisposedException>());
                Assert.That(() => realm.Remove(new Person()), Throws.TypeOf<ObjectDisposedException>());
                Assert.That(() => realm.RemoveAll<Person>(), Throws.TypeOf<ObjectDisposedException>());
                Assert.That(() => realm.Write(() => { }), Throws.TypeOf<ObjectDisposedException>());

                await TestHelpers.AssertThrows<ObjectDisposedException>(() => realm.WriteAsync(() => { }));
                await TestHelpers.AssertThrows<ObjectDisposedException>(() => realm.RefreshAsync());

                other.Dispose();
            });
        }

#if WINDOWS_UWP
        [Ignore("Locks on .NET Native")]
#endif

        [Test]
        public void GetInstanceAsync_ExecutesMigrationsInBackground()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var config = (RealmConfiguration)RealmConfiguration.DefaultConfiguration;
                config.SchemaVersion = 1;

                using (var firstRealm = GetRealm(config))
                {
                    Assert.That(firstRealm.All<IntPrimaryKeyWithValueObject>().Count(), Is.Zero);
                }

                var threadId = Environment.CurrentManagedThreadId;
                var hasCompletedMigration = false;
                config.SchemaVersion = 2;
                config.MigrationCallback = (migration, oldSchemaVersion) =>
                {
                    Assert.That(Environment.CurrentManagedThreadId, Is.Not.EqualTo(threadId));
                    Task.Delay(300).Wait();
                    migration.NewRealm.Add(new IntPrimaryKeyWithValueObject
                    {
                        Id = 123
                    });
                    hasCompletedMigration = true;
                };

                var sw = new Stopwatch();
                sw.Start();

                using var realm = await GetRealmAsync(config, 1000);

                sw.Stop();

                Assert.That(hasCompletedMigration);
                Assert.That(sw.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(200));
                Assert.That(realm.All<IntPrimaryKeyWithValueObject>().Count(), Is.EqualTo(1));
            });
        }

        [Test]
        public void WriteEncryptedCopy_WhenEncryptionKeyProvided_WritesACopy([Values(true, false)] bool originalEncrypted,
                                                                             [Values(true, false)] bool copyEncrypted)
        {
            var originalConfig = new RealmConfiguration(Guid.NewGuid().ToString());
            if (originalEncrypted)
            {
                originalConfig.EncryptionKey = TestHelpers.GetEncryptionKey(42);
            }

            var copyConfig = new RealmConfiguration(Guid.NewGuid().ToString());
            if (copyEncrypted)
            {
                copyConfig.EncryptionKey = TestHelpers.GetEncryptionKey(14);
            }

            File.Delete(copyConfig.DatabasePath);

            using var original = GetRealm(originalConfig);
            original.Write(() =>
            {
                original.Add(new Person
                {
                    FirstName = "John",
                    LastName = "Doe"
                });

                original.WriteCopy(copyConfig);

                using (var copy = GetRealm(copyConfig))
                {
                    var copiedPerson = copy.All<Person>().Single();
                    Assert.That(copiedPerson.FirstName, Is.EqualTo("John"));
                    Assert.That(copiedPerson.LastName, Is.EqualTo("Doe"));
                }

                if (copyEncrypted)
                {
                    var invalidConfig = new RealmConfiguration(copyConfig.DatabasePath)
                    {
                        EncryptionKey = originalConfig.EncryptionKey
                    };

                    Assert.That(() => GetRealm(invalidConfig), Throws.TypeOf<RealmInvalidDatabaseException>());
                }
            });
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GetInstance_WhenCacheEnabled_ReturnsSameStates(bool enableCache)
        {
            var config = new RealmConfiguration(Guid.NewGuid().ToString());

            Assert.That(config.EnableCache, Is.True);

            config.EnableCache = enableCache;

            var stateAccessor = typeof(Realm).GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic)!;
            using var first = GetRealm(config);
            using var second = GetRealm(config);
            Assert.That(enableCache == ReferenceEquals(stateAccessor.GetValue(first), stateAccessor.GetValue(second)));
        }

        [Test]
        public void GetInstance_WhenDynamic_ReadsSchemaFromDisk()
        {
            var config = new RealmConfiguration(Guid.NewGuid().ToString())
            {
                Schema = new[] { typeof(AllTypesObject), typeof(ObjectWithEmbeddedProperties), typeof(EmbeddedAllTypesObject), typeof(EmbeddedLevel1), typeof(EmbeddedLevel2), typeof(EmbeddedLevel3) }
            };

            // Create the realm and add some objects
            using (var realm = GetRealm(config))
            {
                realm.Write(() => realm.Add(new AllTypesObject
                {
                    Int32Property = 42,
                    RequiredStringProperty = "This is required!"
                }));

                realm.Write(() => realm.Add(new ObjectWithEmbeddedProperties
                {
                    AllTypesObject = new EmbeddedAllTypesObject
                    {
                        Int32Property = 24,
                        StringProperty = "This is not required!"
                    }
                }));
            }

            config.IsDynamic = true;

            using var dynamicRealm = GetRealm(config);
            Assert.That(dynamicRealm.Schema.Count, Is.EqualTo(6));

            Assert.That(dynamicRealm.Schema.TryFindObjectSchema(nameof(AllTypesObject), out var allTypesSchema), Is.True);
            Assert.That(allTypesSchema, Is.Not.Null);
            Assert.That(allTypesSchema!.BaseType, Is.Not.EqualTo(ObjectSchema.ObjectType.EmbeddedObject));
            Assert.That(allTypesSchema.BaseType, Is.Not.EqualTo(ObjectSchema.ObjectType.AsymmetricObject));

            var hasExpectedProp = allTypesSchema.TryFindProperty(nameof(AllTypesObject.RequiredStringProperty), out var requiredStringProp);
            Assert.That(hasExpectedProp);
            Assert.That(requiredStringProp.Type, Is.EqualTo(PropertyType.String));

            var ato = dynamicRealm.DynamicApi.All(nameof(AllTypesObject)).Single();
            Assert.That(ato.DynamicApi.Get<string>(nameof(AllTypesObject.RequiredStringProperty)), Is.EqualTo("This is required!"));

#if !UNITY
            dynamic dynamicAto = dynamicRealm.DynamicApi.All(nameof(AllTypesObject)).Single();
            Assert.That(dynamicAto.RequiredStringProperty, Is.EqualTo("This is required!"));
#endif

            Assert.That(dynamicRealm.Schema.TryFindObjectSchema(nameof(EmbeddedAllTypesObject), out var embeddedAllTypesSchema), Is.True);
            Assert.That(embeddedAllTypesSchema, Is.Not.Null);
            Assert.That(embeddedAllTypesSchema!.BaseType, Is.EqualTo(ObjectSchema.ObjectType.EmbeddedObject));

            Assert.That(embeddedAllTypesSchema.TryFindProperty(nameof(EmbeddedAllTypesObject.StringProperty), out var stringProp), Is.True);
            Assert.That(stringProp.Type, Is.EqualTo(PropertyType.String | PropertyType.Nullable));

            var embeddedParent = dynamicRealm.DynamicApi.All(nameof(ObjectWithEmbeddedProperties)).Single();
            var embeddedChild = embeddedParent.DynamicApi.Get<IEmbeddedObject>(nameof(ObjectWithEmbeddedProperties.AllTypesObject));
            Assert.That(embeddedChild.DynamicApi.Get<string>(nameof(EmbeddedAllTypesObject.StringProperty)), Is.EqualTo("This is not required!"));

#if !UNITY
            dynamic dynamicEmbeddedParent = dynamicRealm.DynamicApi.All(nameof(ObjectWithEmbeddedProperties)).Single();
            Assert.That(dynamicEmbeddedParent.AllTypesObject.StringProperty, Is.EqualTo("This is not required!"));
#endif
        }

        [Test]
        public void GetInstance_WhenIsDynamic_AndOSSchemaHasEmptyTable_DoesntThrow()
        {
            var config = new RealmConfiguration(Guid.NewGuid().ToString())
            {
                IsDynamic = true
            };

            TestHelpers.CopyBundledFileToDocuments("v6db.realm", config.DatabasePath);

            Assert.DoesNotThrow(() =>
            {
                using var realm = GetRealm(config);
            });
        }

        [Test]
        public void GetInstance_WhenDynamicAndDoesntExist_ReturnsEmptySchema()
        {
            var config = new RealmConfiguration(Guid.NewGuid().ToString())
            {
                IsDynamic = true
            };

            using var realm = GetRealm(config);
            Assert.That(realm.Schema, Is.Empty);
        }

        [TestCase("фоо-бар")]
        [TestCase("Λορεμ")]
        [TestCase("ლორემ")]
        [TestCase("植物")]
        [TestCase("החלל")]
        [TestCase("جمعت")]
        [TestCase("søren")]
        public void GetInstance_WhenPathContainsNonASCIICharacters_ShouldWork(string path)
        {
            var folder = Path.Combine(InteropConfig.GetDefaultStorageFolder("No error expected here"), path);
            Directory.CreateDirectory(folder);
            var realmPath = Path.Combine(folder, "my.realm");
            var config = new RealmConfiguration(realmPath);

            using var realm = GetRealm(config);
            realm.Write(() => realm.Add(new Person()));
            Assert.AreEqual(1, realm.All<Person>().Count());
        }

        [Test]
        public void Freeze_FreezesTheRealm()
        {
            using var realm = GetRealm();
            realm.Write(() =>
            {
                var dog = realm.Add(new Dog
                {
                    Name = "Charlie"
                });

                realm.Add(new Owner
                {
                    Name = "George",
                    TopDog = dog,
                    ListOfDogs = { dog }
                });
            });

            using var frozenRealm = realm.Freeze();
            Assert.That(frozenRealm.IsFrozen);

            var query = frozenRealm.All<Owner>();
            Assert.That(query.AsRealmCollection().IsFrozen);

            var owner = query.Single();
            Assert.That(owner.IsFrozen);
            Assert.That(owner.TopDog!.IsFrozen);
            Assert.That(owner.ListOfDogs.AsRealmCollection().IsFrozen);
            Assert.That(owner.ListOfDogs[0].IsFrozen);
        }

        [Test]
        public void FrozenRealm_DoesntUpdate()
        {
            using var realm = GetRealm();
            var george = realm.Write(() =>
            {
                var dog = realm.Add(new Dog
                {
                    Name = "Charlie"
                });

                return realm.Add(new Owner
                {
                    Name = "George",
                    TopDog = dog,
                    ListOfDogs = { dog }
                });
            });

            using var frozenRealm = realm.Freeze();
            realm.Write(() =>
            {
                realm.Add(new Owner
                {
                    Name = "Peter"
                });

                george.Name = "George Jr.";
            });

            var owners = frozenRealm.All<Owner>();
            Assert.That(owners.Count(), Is.EqualTo(1));

            var frozenGeorge = owners.Single();
            Assert.That(frozenGeorge.Name, Is.EqualTo("George"));
        }

        [Test]
        public void FrozenRealm_CannotWrite()
        {
            using var realm = GetRealm();
            using var frozenRealm = realm.Freeze();

            Assert.Throws<RealmFrozenException>(() => frozenRealm.Write(() => { }));
            Assert.Throws<RealmFrozenException>(() => frozenRealm.BeginWrite());
        }

        [Test]
        public void FrozenRealm_CannotSubscribeForNotifications()
        {
            using var realm = GetRealm();
            using var frozenRealm = realm.Freeze();

            Assert.Throws<RealmFrozenException>(() => frozenRealm.RealmChanged += (_, __) => { });
            Assert.Throws<RealmFrozenException>(() => frozenRealm.RealmChanged -= (_, __) => { });
        }

        [Test]
        public void FrozenRealms_CanBeUsedAcrossThreads()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                using var realm = GetRealm();

                realm.Write(() =>
                {
                    var dog = realm.Add(new Dog
                    {
                        Name = "Charlie"
                    });

                    realm.Add(new Owner
                    {
                        Name = "George",
                        TopDog = dog,
                        ListOfDogs = { dog }
                    });
                });

                using var frozenRealm = realm.Freeze();

                var georgeOnThreadOne = frozenRealm.All<Owner>().Single();
                var georgeOnThreadTwo = await Task.Run(() =>
                {
                    var bgGeorge = frozenRealm.All<Owner>().Single();
                    Assert.That(bgGeorge.Name, Is.EqualTo("George"));
                    Assert.That(georgeOnThreadOne.IsValid);
                    Assert.That(georgeOnThreadOne.Name, Is.EqualTo("George"));
                    return bgGeorge;
                });

                Assert.That(georgeOnThreadTwo.IsValid);
                Assert.That(georgeOnThreadOne.Name, Is.EqualTo(georgeOnThreadTwo.Name));
            });
        }

        [Test]
        public void FrozenRealms_ReuseParentSchemaAndMetadata()
        {
            using var realm = GetRealm();
            using var frozenRealm = realm.Freeze();

            Assert.That(realm.Schema, Is.SameAs(frozenRealm.Schema));
            Assert.That(realm.Metadata, Is.SameAs(frozenRealm.Metadata));
        }

        [Test]
        public void FrozenRealms_GetGarbageCollected()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                await TestHelpers.EnsureObjectsAreCollected(() =>
                {
                    using var realm = GetRealm();
                    return new[] { realm.Freeze() };
                });

                // This will throw on Windows if the Realm wasn't really disposed
                Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);
            });
        }

        [Test]
        public void Realm_Freeze_WhenFrozen_ReturnsSameInstance()
        {
            var realm = GetRealm();
            using var frozenRealm = realm.Freeze();
            Assert.That(ReferenceEquals(realm, frozenRealm), Is.False);

            // Freezing a frozen realm should do nothing
            using var deepFrozenRealm = frozenRealm.Freeze();
            Assert.That(ReferenceEquals(deepFrozenRealm, frozenRealm));

            // Freezing the same Realm again should return a new instance
            using var anotherFrozenRealm = realm.Freeze();

            Assert.That(ReferenceEquals(realm, anotherFrozenRealm), Is.False);
            Assert.That(ReferenceEquals(frozenRealm, anotherFrozenRealm), Is.False);
        }

        [Test]
        public void Realm_Freeze_ReadOnly()
        {
            var config = new RealmConfiguration(Guid.NewGuid().ToString());
            var realm = GetRealm(config);
            realm.Dispose();

            config.IsReadOnly = true;

            realm = GetRealm(config);
            Realm frozenRealm = null!;
            Assert.DoesNotThrow(() => frozenRealm = realm.Freeze());
            frozenRealm.Dispose();
        }

        [Test]
        public void Realm_HittingMaxNumberOfVersions_Throws()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            // Use legacy representation to avoid writing to the Realm when opening it as that
            // would already hit the max version limit.
            Realm.UseLegacyGuidRepresentation = true;
#pragma warning restore CS0618 // Type or member is obsolete

            var config = new RealmConfiguration(Guid.NewGuid().ToString())
            {
                MaxNumberOfActiveVersions = 1
            };
            var realm = GetRealm(config);

            Assert.Throws<RealmInvalidTransactionException>(() => realm.Write(() => { }), "Number of active versions (2) in the Realm exceeded the limit of 1");
        }

        [Test]
        public void RealmState_GetsGarbageCollected()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                await TestHelpers.EnsureObjectsAreCollected(() =>
                {
                    var stateAccessor = typeof(Realm).GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic)!;

                    using var realm = Realm.GetInstance();
                    var state = stateAccessor.GetValue(realm)!;

                    return new object[] { state };
                });
            });
        }

        [Test]
        public void GetInstance_WhenReadonly_TreatsAdditionalTablesAsEmpty()
        {
            var id = Guid.NewGuid().ToString();
            var config = new RealmConfiguration(id)
            {
                Schema = new[] { typeof(AllTypesObject) }
            };

            using (var realm = GetRealm(config))
            {
                realm.Write(() =>
                {
                    realm.Add(new AllTypesObject { RequiredStringProperty = "abc" });
                });
            }

            var readonlyConfig = new RealmConfiguration(id)
            {
                Schema = new[] { typeof(AllTypesObject), typeof(IntPrimaryKeyWithValueObject) },
                IsReadOnly = true
            };

            using var readonlyRealm = GetRealm(readonlyConfig);
            var readonlyAtos = readonlyRealm.All<AllTypesObject>();
            Assert.That(readonlyAtos.Count(), Is.EqualTo(1));
            Assert.That(readonlyAtos.Single().RequiredStringProperty, Is.EqualTo("abc"));

            var readonlyIntObjs = readonlyRealm.All<IntPrimaryKeyWithValueObject>();
            Assert.That(readonlyIntObjs.Count(), Is.EqualTo(0));
            Assert.That(readonlyIntObjs.Where(o => o.StringValue == "abc").Count(), Is.EqualTo(0));
            Assert.That(readonlyIntObjs.Filter("StringValue = 'abc'").Count(), Is.EqualTo(0));
            Assert.That(readonlyRealm.Find<IntPrimaryKeyWithValueObject>(123), Is.Null);
        }

        [Test]
        public void GetInstance_WithManualSchema_CanReadAndWrite()
        {
            var config = new RealmConfiguration(Guid.NewGuid().ToString())
            {
                Schema = new RealmSchema.Builder
                {
                    new ObjectSchema.Builder("MyType", ObjectSchema.ObjectType.RealmObject)
                    {
                        Property.Primitive("IntValue", RealmValueType.Int),
                        Property.PrimitiveList("ListValue", RealmValueType.Date),
                        Property.PrimitiveSet("SetValue", RealmValueType.Guid),
                        Property.PrimitiveDictionary("DictionaryValue", RealmValueType.Double),
                        Property.Object("ObjectValue", "OtherObject"),
                        Property.ObjectList("ObjectListValue", "OtherObject"),
                        Property.ObjectSet("ObjectSetValue", "OtherObject"),
                        Property.ObjectDictionary("ObjectDictionaryValue", "OtherObject"),
                    },
                    new ObjectSchema.Builder("OtherObject", ObjectSchema.ObjectType.RealmObject)
                    {
                        Property.Primitive("Id", RealmValueType.String, isPrimaryKey: true),
                        Property.Backlinks("MyTypes", "MyType", "ObjectValue")
                    }
                }
            };

            using var realm = GetRealm(config);

            realm.Write(() =>
            {
                var other = (IRealmObject)realm.DynamicApi.CreateObject("OtherObject", "abc");
                var myType1 = realm.DynamicApi.CreateObject("MyType");
                myType1.DynamicApi.Set("IntValue", 123);
                myType1.DynamicApi.GetList<DateTimeOffset>("ListValue").Add(DateTimeOffset.UtcNow);
                myType1.DynamicApi.GetSet<Guid>("SetValue").Add(Guid.NewGuid());
                myType1.DynamicApi.GetDictionary<double>("DictionaryValue").Add("key", 123.456);
                myType1.DynamicApi.Set("ObjectValue", RealmValue.Object(other));
                myType1.DynamicApi.GetList<IRealmObject>("ObjectListValue").Add(other);
                myType1.DynamicApi.GetSet<IRealmObject>("ObjectSetValue").Add(other);
                myType1.DynamicApi.GetDictionary<IRealmObject>("ObjectDictionaryValue").Add("key", other);

                var myType2 = realm.DynamicApi.CreateObject("MyType");
                myType2.DynamicApi.Set("IntValue", 456);
                myType2.DynamicApi.GetDictionary<double>("DictionaryValue").Add("foo", 123.456);
                myType2.DynamicApi.GetDictionary<double>("DictionaryValue").Add("bar", 987.654);
                myType2.DynamicApi.Set("ObjectValue", RealmValue.Object(other));

                Assert.Throws<MissingMemberException>(() => other.DynamicApi.Set("hoho", 123));
            });

            var myTypes = realm.DynamicApi.All("MyType");
            var otherObjects = realm.DynamicApi.All("OtherObject");

            Assert.That(myTypes.Count(), Is.EqualTo(2));
            Assert.That(otherObjects.Count(), Is.EqualTo(1));

            var foundById = realm.DynamicApi.Find("OtherObject", "abc")!;
            Assert.Throws<MissingMemberException>(() => foundById.DynamicApi.Get<int>("hoho"));
            var backlinks = foundById.DynamicApi.GetBacklinks("MyTypes");

            Assert.That(backlinks.Count(), Is.EqualTo(2));
            Assert.That(backlinks.ToArray().Select(o => o.DynamicApi.Get<int>("IntValue")), Is.EquivalentTo(new[] { 123, 456 }));
        }

        [Test]
        public void GetInstance_WithMixOfManualAndTypedSchema_CanReadAndWrite()
        {
            var config = new RealmConfiguration(Guid.NewGuid().ToString())
            {
                Schema = new RealmSchema.Builder
                {
                    new ObjectSchema.Builder(typeof(Person))
                    {
                        Property.FromType<string>("SalesforceId"),
                        Property.FromType<IDictionary<string, string>>("Tags"),
                        Property.FromType<EmbeddedIntPropertyObject>("EmbeddedInt")
                    },
                    new ObjectSchema.Builder(typeof(EmbeddedIntPropertyObject))
                    {
                        Property.FromType<DateTimeOffset>("LastModified")
                    }
                }
            };

            using var realm = GetRealm(config);

            realm.Write(() =>
            {
                var person = realm.Add(new Person
                {
                    FirstName = "John",
                    LastName = "Smith"
                });

                person.DynamicApi.Set("SalesforceId", "sf-123");
                var tags = person.DynamicApi.GetDictionary<string>("Tags");
                tags["tag1"] = "abc";
                tags["tag2"] = "cde";

                var embeddedInt = new EmbeddedIntPropertyObject
                {
                    Int = 999
                };

                person.DynamicApi.Set("EmbeddedInt", embeddedInt);
                embeddedInt.DynamicApi.Set("LastModified", DateTimeOffset.UtcNow);
            });

            var person = realm.All<Person>().Single();

            var sfId = person.DynamicApi.Get<string>("SalesforceId");
            Assert.That(sfId, Is.EqualTo("sf-123"));

            var embedded = person.DynamicApi.Get<EmbeddedIntPropertyObject>("EmbeddedInt");
            Assert.That(embedded, Is.Not.Null);
            Assert.That(embedded.Int, Is.EqualTo(999));
            var oldLastModified = embedded.DynamicApi.Get<DateTimeOffset>("LastModified");
            Assert.That(oldLastModified, Is.LessThanOrEqualTo(DateTimeOffset.UtcNow));

            Task.Delay(1).Wait();

            realm.Write(() =>
            {
                embedded.DynamicApi.Set("LastModified", DateTimeOffset.UtcNow);
                embedded.Int = 111;
            });

            Assert.That(embedded.Int, Is.EqualTo(111));
            Assert.That(embedded.DynamicApi.Get<DateTimeOffset>("LastModified"), Is.GreaterThan(oldLastModified));
        }

        [Test]
        public void GetInstance_WithTypedSchemaWithMissingProperties_ThrowsException()
        {
            var personSchema = new ObjectSchema.Builder(typeof(Person));
            personSchema.Remove(nameof(Person.FirstName));

            var config = new RealmConfiguration(Guid.NewGuid().ToString())
            {
                Schema = new[] { personSchema.Build() }
            };

            using var realm = GetRealm(config);

            var person = realm.Write(() =>
            {
                return realm.Add(new Person
                {
                    LastName = "Smith"
                });
            });

            var exGet = Assert.Throws<MissingMemberException>(() => _ = person.FirstName)!;
            Assert.That(exGet.Message, Does.Contain(nameof(Person)));
            Assert.That(exGet.Message, Does.Contain(nameof(Person.FirstName)));

            realm.Write(() =>
            {
                var exSet = Assert.Throws<MissingMemberException>(() => person.FirstName = "John")!;
                Assert.That(exSet.Message, Does.Contain(nameof(Person)));
                Assert.That(exSet.Message, Does.Contain(nameof(Person.FirstName)));
            });
        }

        [Test]
        public void RealmWithFrozenObjects_WhenDeleted_DoesNotThrow()
        {
            var config = new RealmConfiguration(Guid.NewGuid().ToString());
            var realm = GetRealm(config);
            var frozenObj = realm.Write(() =>
            {
                return realm.Add(new IntPropertyObject
                {
                    Int = 1
                }).Freeze();
            });

            frozenObj.Realm!.Dispose();
            realm.Dispose();

            Assert.That(frozenObj.Realm.IsClosed, Is.True);
            Assert.That(realm.IsClosed, Is.True);
            Assert.DoesNotThrow(() => Realm.DeleteRealm(config));
        }

        [Test]
        public void BeginWrite_CalledMultipleTimes_Throws()
        {
            using var realm = GetRealm();
            var ts = realm.BeginWrite();

            Assert.That(() => realm.BeginWrite(), Throws.TypeOf<RealmInvalidTransactionException>());
        }

        [Test]
        public void RealmDispose_DisposesActiveTransaction()
        {
            var realm = GetRealm();
            var ts = realm.BeginWrite();

            Assert.That(ts.State, Is.EqualTo(TransactionState.Running));

            realm.Dispose();

            Assert.That(ts.State, Is.EqualTo(TransactionState.RolledBack));
        }

        [Test]
        public void Logger_ChangeLevel_ReflectedImmediately()
        {
            var logger = new Logger.InMemoryLogger();
            Logger.Default = logger;

            using var realm = GetRealm(Guid.NewGuid().ToString());

            var expectedLog = new Regex("Info: DB: [^ ]* Thread [^ ]*: Open file");
            TestHelpers.AssertRegex(logger.GetLog(), expectedLog);
            Assert.That(logger.GetLog(), Does.Not.Contain("Debug"));

            // We're at info level, so we don't expect any statements.
            WriteAndVerifyLogs();

            Logger.LogLevel = LogLevel.Debug;

            // We're at Debug level now, so we should see the write message.
            var expectedWriteLog = new Regex("Debug: DB: .* Commit of size [^ ]* done in [^ ]* us");
            WriteAndVerifyLogs(expectedWriteLog);

            // Revert back to Info level and make sure we don't log anything
            Logger.LogLevel = LogLevel.Info;
            WriteAndVerifyLogs();

            void WriteAndVerifyLogs(Regex? expectedRegex = null)
            {
                logger.Clear();

                realm.Write(() =>
                {
                    realm.Add(new IntPropertyObject());
                });

                if (expectedRegex == null)
                {
                    Assert.That(logger.GetLog(), Is.Empty);
                }
                else
                {
                    TestHelpers.AssertRegex(logger.GetLog(), expectedRegex);
                }
            }
        }

        private const int DummyDataSize = 200;

        private static void AddDummyData(Realm realm)
        {
            for (var i = 0; i < DummyDataSize; i++)
            {
                realm.Write(() =>
                {
                    realm.Add(new IntPrimaryKeyWithValueObject
                    {
                        Id = i,
                        StringValue = "Super secret product " + i
                    });
                });
            }

            for (var i = 0; i < DummyDataSize / 2; i++)
            {
                realm.Write(() =>
                {
                    var item = realm.Find<IntPrimaryKeyWithValueObject>(2 * i)!;
                    realm.Remove(item);
                });
            }
        }
    }

    public partial class LoneClass : TestRealmObject
    {
        public string? Name { get; set; }
    }
}
