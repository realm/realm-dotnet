/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;
using System.IO;
using NUnit.Framework;
using System.Threading.Tasks;
using Realms;
using System.Threading;

namespace IntegrationTests
{
    [TestFixture]
    public class InstanceTests
    {
        const string specialRealmName = "EnterTheMagic.realm";

        [TestFixtureSetUp]
        public void Setup()
        {
            Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);
            var uniqueConfig = new RealmConfiguration(specialRealmName);  // for when need 2 realms or want to not use default
            Realm.DeleteRealm(uniqueConfig);
        }

        [Test]
        public void GetInstanceTest()
        {
            // Arrange, act and "assert" that no exception is thrown, using default location
            Realm.GetInstance().Close();
        }

#if ENABLE_INTERNAL_NON_PCL_TESTS
        // This is a test of the Exception throwing mechanism but is included in the Instance tests
        // because getting an instance initialises the delegate for exceptions back to C#
        [Test]
        public void FakeExceptionThrowTest()
        {
            using (Realm.GetInstance())
            {
                Assert.Throws<RealmPermissionDeniedException>(() => NativeCommon.fake_a_native_exception((IntPtr)RealmExceptionCodes.RealmPermissionDenied));
            }

        }

        [Test]
        public void FakeExceptionThrowLoopingTest()
        {
            using (Realm.GetInstance())
            {
                for (int i = 0; i < 10000; ++i)
                {
#if DEBUG
                    bool caughtIt = false;
                    // Assert.Throws doesn't work with the VS debugger which thinks the exception is uncaught
                    try
                    {
                        NativeCommon.fake_a_native_exception((IntPtr)RealmExceptionCodes.RealmPermissionDenied);
                    }
                    catch (RealmPermissionDeniedException)
                    {
                        caughtIt = true;
                    }
                    Assert.That(caughtIt, "Should have caught the expected exception");
#else
                Assert.Throws<RealmPermissionDeniedException>(
                    () => NativeCommon.fake_a_native_exception((IntPtr) RealmExceptionCodes.RealmPermissionDenied));
#endif
                }
            }
        }
#endif  // ENABLE_INTERNAL_NON_PCL_TESTS

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
            Realm.GetInstance(specialRealmName).Close();
        }

        [Test]
        public void DeleteRealmWorksIfClosed()
        {
            // Arrange
            var config = RealmConfiguration.DefaultConfiguration;
            var openRealm = Realm.GetInstance(config);

            // Act
            openRealm.Close();

            // Assert 
            Assert.That(File.Exists(config.DatabasePath));
            Assert.DoesNotThrow(() => Realm.DeleteRealm(config));
            Assert.False(File.Exists(config.DatabasePath));
        }


        [Test, Explicit("Disabled until fix realm-dotnet-private #51")]
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
            Assert.False(GC.ReferenceEquals(realm1, realm2));
            Assert.False(realm1.IsSameInstance(realm2));
            Assert.That(realm1, Is.EqualTo(realm2));  // equal and same Realm but not same instance

            realm1.Close();
            realm2.Close();
        }


        [Test]
        public void GetCachedInstancesSameThread()
        {
            // Arrange
            using (var realm1 = Realm.GetInstance())
            using (var realm2 = Realm.GetInstance())
            {
                // Assert
                Assert.False(GC.ReferenceEquals(realm1, realm2));
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
                Assert.False(GC.ReferenceEquals(realm1, realm2));
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


        class LoneClass : RealmObject
        {
            public string Name { get; set;}
        }

        [Test]
        public void RealmWithOneClassWritesDesiredClass()
        {
            // Arrange
            var config = new RealmConfiguration("RealmWithOneClass.realm");
            Realm.DeleteRealm(config);
            config.ObjectClasses = new Type[] {typeof(LoneClass)};

            // Act
            using (var lonelyRealm = Realm.GetInstance(config)) {
                lonelyRealm.Write( () => {
                    var p = lonelyRealm.CreateObject<LoneClass>();
                    p.Name = "The Singular";
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
            config.ObjectClasses = new Type[] {typeof(LoneClass)};

            // Act and assert
            using (var lonelyRealm = Realm.GetInstance(config)) {
                using (var trans = lonelyRealm.BeginWrite())
                {
                    Assert.Throws<ArgumentException>(() =>
                        {
                            lonelyRealm.CreateObject<Person>(); 
                        }, 
                        "Can't create an object with a class not included in this Realm"); 
                } // transaction
            }  // realm
        }


        [Test]
        public void RealmObjectClassesOnlyAllowRealmObjects()
        {
            // Arrange
            var config = new RealmConfiguration("RealmWithOneClass.realm");
            Realm.DeleteRealm(config);
            config.ObjectClasses = new Type[] {typeof(LoneClass), typeof(object)};

            // Act and assert
            Assert.Throws<ArgumentException>(() =>
                {
                    Realm.GetInstance(config); 
                }, 
                "Can't have classes in the list which are not RealmObjects"); 
        }

    }
}
