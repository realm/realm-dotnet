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
        [TestFixtureSetUp]
        public void Setup()
        {
            File.Delete(RealmConfiguration.PathToRealm());
            File.Delete(RealmConfiguration.PathToRealm("EnterTheMagic.realm"));
        }

        [Test]
        public void GetInstanceTest()
        {
            // Arrange, act and "assert" that no exception is thrown, using default location
            Realm.GetInstance().Close();
        }

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
            // Arrange, act and "assert" that no exception is thrown, using default location
            Realm.GetInstance("EnterTheMagic.realm").Close();
        }

        [Test]
        public void DeleteRealmWorksIfClosed()
        {
            // Arrange
            var config = new RealmConfiguration("EnterTheMagic.realm");
            var openRealm = Realm.GetInstance(config);

            // Act
            openRealm.Close();

            // Assert 
            Assert.That(File.Exists(config.DatabasePath));
            Assert.DoesNotThrow(() => Realm.DeleteRealm(config));
            Assert.False(File.Exists(config.DatabasePath));
        }


        [Test]
        public void GetUniqueInstancesDifferentThreads()
        {
            // Arrange
            var realm1 = Realm.GetInstance("EnterTheMagic.realm");
            Realm realm2 = realm1;  // should be reassigned by other thread

            // Act
            var t = new Thread(() =>
                {
                    realm2 = Realm.GetInstance("EnterTheMagic.realm");
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
            using (var realm1 = Realm.GetInstance("EnterTheMagic.realm"))
            using (var realm2 = Realm.GetInstance("EnterTheMagic.realm"))
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
            using (var realm1 = Realm.GetInstance("EnterTheMagic.realm"))
            using (var realm2 = Realm.GetInstance("EnterTheMagic.realm"))
            {
                // Assert
                Assert.False(GC.ReferenceEquals(realm1, realm2));
                Assert.That(realm1.GetHashCode(), Is.Not.EqualTo(0));  
                Assert.That(realm1.GetHashCode(), Is.Not.EqualTo(realm2.GetHashCode())); 
            }
        }




        /*
         * uncomment when fix https://github.com/realm/realm-dotnet/issues/308
        [Test]
        public void DeleteRealmFailsIfOpenSameThread()
        {
            // Arrange
            var config = new RealmConfiguration("EnterTheMagic.realm");
            var openRealm = Realm.GetInstance(config);

            // Assert
            Assert.Throws<RealmPermissionDeniedException>(() => Realm.DeleteRealm(config));
        }
        */

        /*
        Comment out until work out how to fix
        see issue 199
        [Test]
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
        */

        [Test]
        public void GetInstanceShouldThrowWithBadPath()
        {
            // Arrange
            Assert.Throws<RealmPermissionDeniedException>(() => Realm.GetInstance("/"));
        }
    }
}
