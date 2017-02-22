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
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;
using Realms;
using Realms.Exceptions;

namespace IntegrationTests
{
    [TestFixture, Preserve(AllMembers = true)]
    public class InstanceTests : RealmTest
    {
        private const string SpecialRealmName = "EnterTheMagic.realm";

        public override void TearDown()
        {
            Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);
            var uniqueConfig = new RealmConfiguration(SpecialRealmName);  // for when need 2 realms or want to not use default
            Realm.DeleteRealm(uniqueConfig);
        }

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
            // Arrange, act and "assert" that no exception is thrown, using default location + unique name
            Realm.GetInstance(SpecialRealmName).Dispose();
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
            // Arrange
            var realm1 = Realm.GetInstance();
            Realm realm2 = realm1;  // should be reassigned by other thread

            // Act
            var t = new Thread(() =>
                {
                    realm2 = Realm.GetInstance();
                });
            t.Start();
            t.Join();

            // Assert
            Assert.That(ReferenceEquals(realm1, realm2), Is.False);
            Assert.That(realm1.IsSameInstance(realm2), Is.False);
            Assert.That(realm1, Is.EqualTo(realm2));  // equal and same Realm but not same instance

            realm1.Dispose();
            realm2.Dispose();
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

        [Test, Ignore("Currently doesn't work. Ref #308")]
        public void DeleteRealmFailsIfOpenSameThread()
        {
            // Arrange
            var config = new RealmConfiguration();
            var openRealm = Realm.GetInstance(config);

            // Assert
            Assert.Throws<RealmPermissionDeniedException>(() => Realm.DeleteRealm(config));
        }

        [Test, Ignore("Currently doesn't work. Ref #199")]
        public void GetInstanceShouldThrowIfFileIsLocked()
        {
            // Arrange
            var databasePath = Path.GetTempFileName();
            using (File.Open(databasePath, FileMode.Open, FileAccess.Read, FileShare.None))     // Lock the file
            {
                // Act and assert
                Assert.Throws<RealmPermissionDeniedException>(() => Realm.GetInstance(databasePath));
            }
        }

        [Test, Ignore("Currently doesn't work. Ref #338")]
        public void GetInstanceShouldThrowWithBadPath()
        {
            // Arrange
            Assert.Throws<RealmPermissionDeniedException>(() => Realm.GetInstance("/"));
        }

        private class LoneClass : RealmObject
        {
            public string Name { get; set; }
        }

        [Test]
        public void RealmWithOneClassWritesDesiredClass()
        {
            // Arrange
            var config = new RealmConfiguration("RealmWithOneClass.realm");
            Realm.DeleteRealm(config);
            config.ObjectClasses = new Type[] { typeof(LoneClass) };

            // Act
            using (var lonelyRealm = Realm.GetInstance(config))
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
            var config = new RealmConfiguration("RealmWithOneClass.realm");
            Realm.DeleteRealm(config);
            config.ObjectClasses = new Type[] { typeof(LoneClass) };

            // Act and assert
            using (var lonelyRealm = Realm.GetInstance(config))
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
            var config = new RealmConfiguration("RealmWithOneClass.realm");
            Realm.DeleteRealm(config);
            config.ObjectClasses = new Type[] { typeof(LoneClass), typeof(object) };

            // Act and assert
            // Can't have classes in the list which are not RealmObjects
            Assert.That(() => Realm.GetInstance(config), Throws.TypeOf<ArgumentException>());
        }

        [TestCase(false, true)]
        [TestCase(false, false)]
#if !ENCRYPTION_DISABLED
        [TestCase(true, true)]
        [TestCase(true, false)]
#endif
#if WINDOWS
        [Ignore("Compact doesn't work on Windows")]
#endif
        public void Compact_ShouldReduceSize(bool encrypt, bool populate)
        {
            var config = new RealmConfiguration($"compactrealm_{encrypt}_{populate}.realm");
            if (encrypt)
            {
                config.EncryptionKey = new byte[64];
                config.EncryptionKey[0] = 5;
            }

            Realm.DeleteRealm(config);

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
#if WINDOWS
        [Ignore("Compact doesn't work on Windows")]
#endif
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
#if WINDOWS
        [Ignore("Compact doesn't work on Windows")]
#endif
        public void Compact_WhenOpenOnDifferentThread_ShouldReturnFalse()
        {
            using (var realm = Realm.GetInstance())
            {
                AddDummyData(realm);

                var initialSize = new FileInfo(realm.Config.DatabasePath).Length;
                Assert.That(() => Task.Run(() => Realm.Compact(realm.Config)).Result, Is.False);
                var finalSize = new FileInfo(realm.Config.DatabasePath).Length;

                Assert.That(finalSize, Is.EqualTo(initialSize));
            }
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
#if WINDOWS
        [Ignore("Compact doesn't work on Windows")]
#endif
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
            AsyncContext.Run(async () =>
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
            AsyncContext.Run(async () =>
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
            AsyncContext.Run(async () =>
            {
                var realm = Realm.GetInstance();
                realm.Dispose();

                Assert.That(realm.IsClosed);

                var other = Realm.GetInstance();

                Assert.That(() => realm.Add(new Person()), Throws.TypeOf<ObjectDisposedException>());
                Assert.That(() => realm.All<Person>(), Throws.TypeOf<ObjectDisposedException>());
                Assert.That(() => realm.All(nameof(Person)), Throws.TypeOf<ObjectDisposedException>());
                Assert.That(() => realm.BeginWrite(), Throws.TypeOf<ObjectDisposedException>());
                Assert.That(() => realm.CreateObject(nameof(Person)), Throws.TypeOf<ObjectDisposedException>());
                Assert.That(() => realm.Find<Person>(0), Throws.TypeOf<ObjectDisposedException>());
                Assert.That(() => realm.GetHashCode(), Throws.TypeOf<ObjectDisposedException>());
                Assert.That(() => realm.IsSameInstance(other), Throws.TypeOf<ObjectDisposedException>());
                Assert.That(() => realm.Refresh(), Throws.TypeOf<ObjectDisposedException>());
                Assert.That(() => realm.Remove(new Person()), Throws.TypeOf<ObjectDisposedException>());
                Assert.That(() => realm.RemoveAll<Person>(), Throws.TypeOf<ObjectDisposedException>());
                Assert.That(() => realm.Write(() => { }), Throws.TypeOf<ObjectDisposedException>());

                try
                {
                    await realm.WriteAsync(_ => { });
                    Assert.Fail("An exception should have been thrown.");
                }
                catch (Exception ex)
                {
                    Assert.That(ex, Is.TypeOf<ObjectDisposedException>());
                }

                other.Dispose();
            });
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