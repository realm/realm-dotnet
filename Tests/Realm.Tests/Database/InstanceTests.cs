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
using System.IO;
using System.Linq;
using System.Reflection;
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
            Realm.GetInstance().Dispose();
        }

        [Test]
        public void InstanceIsClosedByDispose()
        {
            Realm temp;
            using (temp = Realm.GetInstance())
            {
                Assert.That(!temp.IsClosed);
            }

            Assert.That(temp.IsClosed);
        }

        [Test]
        public void GetInstanceWithJustFilenameTest()
        {
            var filename = Path.GetTempFileName();

            try
            {
                Assert.That(() =>
                {
                    using (Realm.GetInstance(filename))
                    {
                    }
                }, Throws.Nothing);
            }
            finally
            {
                var config = new RealmConfiguration(filename);
                Realm.DeleteRealm(config);
            }
        }

        [Test]
        public void DeleteRealmWorksIfClosed()
        {
            // Arrange
            var config = RealmConfiguration.DefaultConfiguration;
            var openRealm = Realm.GetInstance(config);

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
                    realm1 = Realm.GetInstance();

                    // Act
                    await Task.Run(() =>
                    {
                        realm2 = Realm.GetInstance();
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
            using (var realm1 = Realm.GetInstance())
            using (var realm2 = Realm.GetInstance())
            {
                // Assert
                Assert.That(ReferenceEquals(realm1, realm2), Is.False);
                Assert.That(realm1, Is.EqualTo(realm1));  // check equality with self
                Assert.That(realm1.IsSameInstance(realm2));
                Assert.That(realm1, Is.EqualTo(realm2));
            }
        }

        [Test]
        public void InstancesHaveDifferentHashes()
        {
            // Arrange
            using (var realm1 = Realm.GetInstance())
            using (var realm2 = Realm.GetInstance())
            {
                // Assert
                Assert.That(ReferenceEquals(realm1, realm2), Is.False);
                Assert.That(realm1.GetHashCode(), Is.Not.EqualTo(0));
                Assert.That(realm1.GetHashCode(), Is.Not.EqualTo(realm2.GetHashCode()));
            }
        }

        [Test]
        public void DeleteRealmFailsIfOpenSameThread()
        {
            // Arrange
            var openRealm = Realm.GetInstance();

            try
            {
                // Assert
                Assert.Throws<RealmPermissionDeniedException>(() => Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration));
            }
            finally
            {
                openRealm.Dispose();
            }
        }

        [Test, Ignore("Currently doesn't work. Ref #199")]
        public void GetInstanceShouldThrowIfFileIsLocked()
        {
            // Arrange
            var databasePath = Path.GetTempFileName();
            using (File.Open(databasePath, FileMode.Open, FileAccess.Read, FileShare.None)) // Lock the file
            {
                // Act and assert
                Assert.Throws<RealmPermissionDeniedException>(() => Realm.GetInstance(databasePath));
            }
        }

        [Test]
        public void GetInstanceShouldThrowWithBadPath()
        {
            var path = TestHelpers.IsWindows ? "C:\\Windows" : "/";

            // Arrange
            Assert.Throws<RealmPermissionDeniedException>(() => Realm.GetInstance(path));
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
            using (var lonelyRealm = Realm.GetInstance())
            {
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
        }

        [Test]
        public void RealmWithOneClassThrowsIfUseOther()
        {
            // Arrange
            RealmConfiguration.DefaultConfiguration.ObjectClasses = new[] { typeof(LoneClass) };

            // Act and assert
            using (var lonelyRealm = Realm.GetInstance())
            {
                // Can't create an object with a class not included in this Realm
                lonelyRealm.Write(() =>
                {
                    Assert.That(() => lonelyRealm.Add(new Person()), Throws.TypeOf<ArgumentException>());
                });
            }
        }

        [Test]
        public void RealmObjectClassesOnlyAllowRealmObjects()
        {
            // Arrange
            RealmConfiguration.DefaultConfiguration.ObjectClasses = new[] { typeof(LoneClass), typeof(object) };

            // Act and assert
            // Can't have classes in the list which are not RealmObjects
            Assert.That(() => Realm.GetInstance(), Throws.TypeOf<ArgumentException>());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ShouldCompact_IsInvokedAfterOpening(bool shouldCompact)
        {
            var config = (RealmConfiguration)RealmConfiguration.DefaultConfiguration;

            using (var realm = Realm.GetInstance(config))
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

            using (var realm = Realm.GetInstance(config))
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

                Assert.That(realm.All<IntPrimaryKeyWithValueObject>().Count(), Is.EqualTo(500));
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

            using (var realm = Realm.GetInstance(config))
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

            using (var realm = Realm.GetInstance(config))
            {
                Assert.That(realm.All<IntPrimaryKeyWithValueObject>().Count(), Is.EqualTo(populate ? 500 : 0));
            }
        }

        [Test]
        public void Compact_WhenInTransaction_ShouldThrow()
        {
            using (var realm = Realm.GetInstance())
            {
                Assert.That(() =>
                {
                    realm.Write(() =>
                    {
                        Realm.Compact();
                    });
                }, Throws.TypeOf<RealmInvalidTransactionException>());
            }
        }

        [Test]
        public void Compact_WhenOpenOnDifferentThread_ShouldReturnFalse()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                using (var realm = Realm.GetInstance())
                {
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
                }
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
            using (var realm = Realm.GetInstance())
            {
                var initialSize = new FileInfo(realm.Config.DatabasePath).Length;
                Assert.That(() => Realm.Compact(), Is.False);
                var finalSize = new FileInfo(realm.Config.DatabasePath).Length;
                Assert.That(finalSize, Is.EqualTo(initialSize));
            }
        }

        [Test]
        public void Compact_WhenResultsAreOpen_ShouldReturnFalse()
        {
            using (var realm = Realm.GetInstance())
            {
                var token = realm.All<Person>().SubscribeForNotifications((sender, changes, error) =>
                {
                    Console.WriteLine(changes?.InsertedIndices);
                });

                Assert.That(() => Realm.Compact(), Is.False);
                token.Dispose();
            }
        }

        [Test]
        public void RealmChangedShouldFireForEveryInstance()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                using (var realm1 = Realm.GetInstance())
                using (var realm2 = Realm.GetInstance())
                {
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
                }
            });
        }

        [Test]
        public void Dispose_WhenOnTheSameThread_ShouldNotInvalidateOtherInstances()
        {
            Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);

            var realm1 = Realm.GetInstance();
            var realm2 = Realm.GetInstance();

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

            var realm1 = Realm.GetInstance();
            var realm2 = Realm.GetInstance();

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

                var realm1 = Realm.GetInstance();

                await Task.Run(() =>
                {
                    var realm2 = Realm.GetInstance();
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
                var realm = Realm.GetInstance();
                realm.Dispose();

                Assert.That(realm.IsClosed);

                var other = Realm.GetInstance();

                Assert.That(() => realm.Add(new Person()), Throws.TypeOf<ObjectDisposedException>());
                Assert.That(() => realm.All<Person>(), Throws.TypeOf<ObjectDisposedException>());
                Assert.That(() => realm.All(nameof(Person)), Throws.TypeOf<ObjectDisposedException>());
                Assert.That(() => realm.BeginWrite(), Throws.TypeOf<ObjectDisposedException>());
                Assert.That(() => realm.CreateObject(nameof(Person), null), Throws.TypeOf<ObjectDisposedException>());
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
                Realm realm = null;
                var config = (RealmConfiguration)RealmConfiguration.DefaultConfiguration;
                config.SchemaVersion = 1;

                using (var firstRealm = Realm.GetInstance(config))
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

                Exception ex = null;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Realm.GetInstanceAsync(config)
                     .ContinueWith(t =>
                     {
                         if (t.IsFaulted)
                         {
                             ex = t.Exception;
                         }
                         else
                         {
                             realm = t.Result;
                         }
                     }, TaskScheduler.FromCurrentSynchronizationContext());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                var ticks = 0;
                while (realm == null)
                {
                    await Task.Delay(100);
                    ticks++;

                    if (ticks > 10)
                    {
                        Assert.Fail("Migration should have completed by now.");
                    }
                }

                Assert.That(ex, Is.Null);
                Assert.That(hasCompletedMigration);
                Assert.That(ticks, Is.GreaterThanOrEqualTo(2));
                Assert.That(realm.All<IntPrimaryKeyWithValueObject>().Count(), Is.EqualTo(1));
                realm.Dispose();
            });
        }

        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void WriteEncryptedCopy_WhenEncryptionKeyProvided_WritesACopy(bool originalEncrypted, bool copyEncrypted)
        {
            var originalConfig = new RealmConfiguration(Path.GetTempFileName());
            if (originalEncrypted)
            {
                originalConfig.EncryptionKey = TestHelpers.GetEncryptionKey(42);
            }

            var copyConfig = new RealmConfiguration(Path.GetTempFileName());
            if (copyEncrypted)
            {
                copyConfig.EncryptionKey = TestHelpers.GetEncryptionKey(14);
            }

            File.Delete(copyConfig.DatabasePath);

            try
            {
                using (var original = Realm.GetInstance(originalConfig))
                {
                    original.Write(() =>
                    {
                        original.Add(new Person
                        {
                            FirstName = "John",
                            LastName = "Doe"
                        });

                        original.WriteCopy(copyConfig);

                        using (var copy = Realm.GetInstance(copyConfig))
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

                            Assert.That(() => Realm.GetInstance(invalidConfig), Throws.TypeOf<RealmFileAccessErrorException>());
                        }
                    });
                }
            }
            finally
            {
                Realm.DeleteRealm(originalConfig);
                Realm.DeleteRealm(copyConfig);
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GetInstance_WhenCacheEnabled_ReturnsSameStates(bool enableCache)
        {
            var config = new RealmConfiguration(Path.GetTempFileName());

            Assert.That(config.EnableCache, Is.True);

            config.EnableCache = enableCache;

            try
            {
                var stateAccessor = typeof(Realm).GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic);
                using (var first = Realm.GetInstance(config))
                using (var second = Realm.GetInstance(config))
                {
                    Assert.That(enableCache == ReferenceEquals(stateAccessor.GetValue(first), stateAccessor.GetValue(second)));
                }
            }
            finally
            {
                Realm.DeleteRealm(config);
            }
        }

        [Test]
        public void GetInstance_WhenDynamic_ReadsSchemaFromDisk()
        {
            var config = new RealmConfiguration(Path.GetTempFileName())
            {
                ObjectClasses = new[] { typeof(AllTypesObject) }
            };

            try
            {
                // Create the realm and add some objects
                using (var realm = Realm.GetInstance(config))
                {
                    realm.Write(() => realm.Add(new AllTypesObject
                    {
                        Int32Property = 42,
                        RequiredStringProperty = "This is required!"
                    }));
                }

                config.IsDynamic = true;

                using (var dynamicRealm = Realm.GetInstance(config))
                {
                    Assert.That(dynamicRealm.Schema.Count == 1);

                    var objectSchema = dynamicRealm.Schema.Find(nameof(AllTypesObject));
                    Assert.That(objectSchema, Is.Not.Null);

                    var hasExpectedProp = objectSchema.TryFindProperty(nameof(AllTypesObject.RequiredStringProperty), out var requiredStringProp);
                    Assert.That(hasExpectedProp);
                    Assert.That(requiredStringProp.Type, Is.EqualTo(PropertyType.String));

                    var ato = dynamicRealm.All(nameof(AllTypesObject)).Single();
                    Assert.That(ato.RequiredStringProperty, Is.EqualTo("This is required!"));
                }
            }
            finally
            {
                Realm.DeleteRealm(config);
            }
        }

        [Test]
        public void GetInstance_WhenDynamicAndDoesntExist_ReturnsEmptySchema()
        {
            var config = new RealmConfiguration(Path.GetTempFileName())
            {
                IsDynamic = true
            };

            try
            {
                using (var realm = Realm.GetInstance(config))
                {
                    Assert.That(realm.Schema, Is.Empty);
                }
            }
            finally
            {
                Realm.DeleteRealm(config);
            }
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
            var folder = Path.Combine(Path.GetTempPath(), path);
            Directory.CreateDirectory(folder);
            var realmPath = Path.Combine(folder, "my.realm");
            var config = new RealmConfiguration(realmPath);
            try
            {
                using (var realm = Realm.GetInstance(config))
                {
                    realm.Write(() => realm.Add(new Person()));
                    Assert.AreEqual(1, realm.All<Person>().Count());
                }
            }
            finally
            {
                Realm.DeleteRealm(config);
            }
        }

        [Test]
        public void Freeze_FreezesTheRealm()
        {
            using (var realm = Realm.GetInstance())
            {
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

                using (var frozenRealm = realm.Freeze())
                {
                    Assert.That(frozenRealm.IsFrozen);

                    var query = frozenRealm.All<Owner>();
                    Assert.That(query.AsRealmCollection().IsFrozen);

                    var owner = query.Single();
                    Assert.That(owner.IsFrozen);
                    Assert.That(owner.TopDog.IsFrozen);
                    Assert.That(owner.Dogs.AsRealmCollection().IsFrozen);
                    Assert.That(owner.Dogs[0].IsFrozen);
                }
            }
        }

        [Test]
        public void FrozenRealm_DoesntUpdate()
        {
            using (var realm = Realm.GetInstance())
            {
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

                using (var frozenRealm = realm.Freeze())
                {
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
            }
        }

        [Test]
        public void FrozenRealm_CannotWrite()
        {
            using (var realm = Realm.GetInstance())
            using (var frozenRealm = realm.Freeze())
            {
                Assert.Throws<RealmFrozenException>(() => frozenRealm.Write(() => { }));
                Assert.Throws<RealmFrozenException>(() => frozenRealm.BeginWrite());
            }
        }

        [Test]
        public void FrozenRealm_CannotSubscribeForNotifications()
        {
            using (var realm = Realm.GetInstance())
            using (var frozenRealm = realm.Freeze())
            {
                Assert.Throws<RealmFrozenException>(() => frozenRealm.RealmChanged += (_, __) => { });
                Assert.Throws<RealmFrozenException>(() => frozenRealm.RealmChanged -= (_, __) => { });
            }
        }

        [Test]
        public void FrozenRealms_CanBeUsedAcrossThreads()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                using (var realm = Realm.GetInstance())
                {
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

                    using (var frozenRealm = realm.Freeze())
                    {
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
                    }
                }
            });
        }

        [Test]
        public void FrozenRealms_GetGarbageCollected()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                WeakReference frozenRealmRef = null;
                using (var realm = Realm.GetInstance())
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
            var frozenRealm = realm.Freeze();
            Assert.That(ReferenceEquals(realm, frozenRealm), Is.False);

            // Freezing a frozen realm should do nothing
            var deepFrozenRealm = frozenRealm.Freeze();
            Assert.That(ReferenceEquals(deepFrozenRealm, frozenRealm));

            // Freezing the same Realm again should return a new instance
            var anotherFrozenRealm = realm.Freeze();

            Assert.That(ReferenceEquals(realm, anotherFrozenRealm), Is.False);
            Assert.That(ReferenceEquals(frozenRealm, anotherFrozenRealm), Is.False);
        }

        [Test]
        public void Realm_HittingMaxNumberOfVersions_Throws()
        {
            var config = new RealmConfiguration(Path.GetTempFileName())
            {
                MaxNumberOfActiveVersions = 1
            };
            var realm = GetRealm(config);

            Assert.Throws<RealmInvalidTransactionException>(() => realm.Write(() => { }), "Number of active versions (2) in the Realm exceeded the limit of 1");
        }

        private static void AddDummyData(Realm realm)
        {
            for (var i = 0; i < 1000; i++)
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

            for (var i = 0; i < 500; i++)
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
