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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Realms.Exceptions;
using Realms.Schema;

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
        public void GetUniqueInstancesDifferentThreads()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                Realm realm1 = null;
                Realm realm2 = null;
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
        public void DeleteRealmFailsIfOpenSameThread()
        {
            // Arrange
            using var openRealm = GetRealm();

            // Assert
            Assert.Throws<RealmPermissionDeniedException>(() => Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration));
        }

        [Test]
        public void GetInstanceShouldThrowWithBadPath()
        {
            var path = TestHelpers.IsWindows ? "C:\\Windows" : "/";

            // Arrange
            Assert.Throws<RealmPermissionDeniedException>(() => GetRealm(path));
        }

        private class LoneClass : RealmObject
        {
            public string Name { get; set; }
        }

        [Test]
        public void RealmWithOneClassWritesDesiredClass()
        {
            // Arrange
            RealmConfiguration.DefaultConfiguration.ObjectClasses = new[] { typeof(LoneClass) };

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
            RealmConfiguration.DefaultConfiguration.ObjectClasses = new[] { typeof(LoneClass) };

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
            // Arrange
            RealmConfiguration.DefaultConfiguration.ObjectClasses = new[] { typeof(LoneClass), typeof(object) };

            // Act and assert
            // Can't have classes in the list which are not RealmObjects
            Assert.That(() => GetRealm(), Throws.TypeOf<ArgumentException>());
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
                    Assert.That(newSize, Is.LessThan(oldSize));

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

        [Test, Ignore("Currently doesn't work. Ref #947")]
        public void Compact_WhenOpenOnSameThread_ShouldReturnFalse()
        {
            // TODO: enable when we implement instance caching (#947)
            // This works because of caching of native instances in ObjectStore.
            // Technically, we get the same native instance, so Compact goes through.
            // However, this invalidates the opened realm, but we have no way of communicating that.
            // That is why, things seem fine until we try to run queries on the opened realm.
            // Once we handle caching in managed, we should reenable the test.
            using var realm = GetRealm();

            var initialSize = new FileInfo(realm.Config.DatabasePath).Length;
            Assert.That(() => Realm.Compact(), Is.False);
            var finalSize = new FileInfo(realm.Config.DatabasePath).Length;
            Assert.That(finalSize, Is.EqualTo(initialSize));
        }

        [Test]
        public void Compact_WhenResultsAreOpen_ShouldReturnFalse()
        {
            using var realm = GetRealm();

            var token = realm.All<Person>().SubscribeForNotifications((sender, changes, error) =>
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
                Assert.That(() => realm.DynamicApi.CreateObject(nameof(Person), null), Throws.TypeOf<ObjectDisposedException>());
                Assert.That(() => realm.Find<Person>(0), Throws.TypeOf<ObjectDisposedException>());
                Assert.That(() => realm.GetHashCode(), Throws.TypeOf<ObjectDisposedException>());
                Assert.That(() => realm.IsSameInstance(other), Throws.TypeOf<ObjectDisposedException>());
                Assert.That(() => realm.Refresh(), Throws.TypeOf<ObjectDisposedException>());
                Assert.That(() => realm.Remove(new Person()), Throws.TypeOf<ObjectDisposedException>());
                Assert.That(() => realm.RemoveAll<Person>(), Throws.TypeOf<ObjectDisposedException>());
                Assert.That(() => realm.Write(() => { }), Throws.TypeOf<ObjectDisposedException>());

                await TestHelpers.AssertThrows<ObjectDisposedException>(() => realm.WriteAsync(_ => { }));

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

                using var realm = await GetRealmAsync(config).Timeout(1000);

                sw.Stop();

                Assert.That(hasCompletedMigration);
                Assert.That(sw.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(200));
                Assert.That(realm.All<IntPrimaryKeyWithValueObject>().Count(), Is.EqualTo(1));
            });
        }

        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void WriteEncryptedCopy_WhenEncryptionKeyProvided_WritesACopy(bool originalEncrypted, bool copyEncrypted)
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
                    var copiedPerson = copy.All<Person>().SingleOrDefault();
                    Assert.That(copiedPerson, Is.Not.Null);
                    Assert.That(copiedPerson.FirstName, Is.EqualTo("John"));
                    Assert.That(copiedPerson.LastName, Is.EqualTo("Doe"));
                }

                if (copyEncrypted)
                {
                    var invalidConfig = new RealmConfiguration(copyConfig.DatabasePath)
                    {
                        EncryptionKey = originalConfig.EncryptionKey
                    };

                    Assert.That(() => GetRealm(invalidConfig), Throws.TypeOf<RealmFileAccessErrorException>());
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

            var stateAccessor = typeof(Realm).GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic);
            using var first = GetRealm(config);
            using var second = GetRealm(config);
            Assert.That(enableCache == ReferenceEquals(stateAccessor.GetValue(first), stateAccessor.GetValue(second)));
        }

        [Test]
        public void GetInstance_WhenDynamic_ReadsSchemaFromDisk()
        {
            var config = new RealmConfiguration(Guid.NewGuid().ToString())
            {
                ObjectClasses = new[] { typeof(AllTypesObject), typeof(ObjectWithEmbeddedProperties), typeof(EmbeddedAllTypesObject), typeof(EmbeddedLevel1), typeof(EmbeddedLevel2), typeof(EmbeddedLevel3) }
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

            var allTypesSchema = dynamicRealm.Schema.Find(nameof(AllTypesObject));
            Assert.That(allTypesSchema, Is.Not.Null);
            Assert.That(allTypesSchema.IsEmbedded, Is.False);

            var hasExpectedProp = allTypesSchema.TryFindProperty(nameof(AllTypesObject.RequiredStringProperty), out var requiredStringProp);
            Assert.That(hasExpectedProp);
            Assert.That(requiredStringProp.Type, Is.EqualTo(PropertyType.String));

            var ato = dynamicRealm.DynamicApi.All(nameof(AllTypesObject)).Single();
            Assert.That(ato.RequiredStringProperty, Is.EqualTo("This is required!"));

            var embeddedAllTypesSchema = dynamicRealm.Schema.Find(nameof(EmbeddedAllTypesObject));
            Assert.That(embeddedAllTypesSchema, Is.Not.Null);
            Assert.That(embeddedAllTypesSchema.IsEmbedded, Is.True);

            Assert.That(embeddedAllTypesSchema.TryFindProperty(nameof(EmbeddedAllTypesObject.StringProperty), out var stringProp), Is.True);
            Assert.That(stringProp.Type, Is.EqualTo(PropertyType.String | PropertyType.Nullable));

            var embeddedParent = dynamicRealm.DynamicApi.All(nameof(ObjectWithEmbeddedProperties)).Single();
            Assert.That(embeddedParent.AllTypesObject.StringProperty, Is.EqualTo("This is not required!"));
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
            var folder = Path.Combine(InteropConfig.DefaultStorageFolder, path);
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
                    Dogs = { dog }
                });
            });

            using var frozenRealm = realm.Freeze();
            Assert.That(frozenRealm.IsFrozen);

            var query = frozenRealm.All<Owner>();
            Assert.That(query.AsRealmCollection().IsFrozen);

            var owner = query.Single();
            Assert.That(owner.IsFrozen);
            Assert.That(owner.TopDog.IsFrozen);
            Assert.That(owner.Dogs.AsRealmCollection().IsFrozen);
            Assert.That(owner.Dogs[0].IsFrozen);
        }

        [Test]
        public void FrozenRealm_DoesntUpdate()
        {
            using var realm = GetRealm();
            Owner george = null;
            realm.Write(() =>
            {
                var dog = realm.Add(new Dog
                {
                    Name = "Charlie"
                });

                george = realm.Add(new Owner
                {
                    Name = "George",
                    TopDog = dog,
                    Dogs = { dog }
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
                        Dogs = { dog }
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
        public void FrozenRealms_GetGarbageCollected()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                WeakReference frozenRealmRef = null;
                using (var realm = GetRealm())
                {
                    new Action(() =>
                    {
                        var frozenRealm = realm.Freeze();
                        frozenRealmRef = new WeakReference(frozenRealm);
                    })();
                }

                while (frozenRealmRef.IsAlive)
                {
                    await Task.Yield();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }

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
        public void Realm_HittingMaxNumberOfVersions_Throws()
        {
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
                var stateAccessor = typeof(Realm).GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic);

                var realm = Realm.GetInstance();
                var state = stateAccessor.GetValue(realm);

                var realmRef = new WeakReference(realm);
                var stateRef = new WeakReference(state);

                realm = null;
                state = null;

                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                var token = cts.Token;

                while (realmRef.IsAlive || stateRef.IsAlive)
                {
                    await Task.Yield();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    if (token.IsCancellationRequested)
                    {
                        Assert.Fail($"Some references are still alive: RealmRef.IsAlive={realmRef.IsAlive}, StateRef.IsAlive={stateRef.IsAlive}");
                    }
                }

                Assert.That(realmRef.IsAlive, Is.False);
                Assert.That(stateRef.IsAlive, Is.False);
            });
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
                    var item = realm.Find<IntPrimaryKeyWithValueObject>(2 * i);
                    realm.Remove(item);
                });
            }
        }
    }
}
