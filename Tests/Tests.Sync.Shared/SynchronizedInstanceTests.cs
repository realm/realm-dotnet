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
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;
using Realms;
using Realms.Exceptions;
using Realms.Sync;
using Realms.Sync.Exceptions;
using ExplicitAttribute = NUnit.Framework.ExplicitAttribute;

namespace Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class SynchronizedInstanceTests : SyncTestBase
    {
        [Ignore("Due to #976, compact doesn't work with synced realms.")]
        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void Compact_ShouldReduceSize(bool encrypt, bool populate)
        {
            AsyncContext.Run(async () =>
            {
                var user = await SyncTestHelpers.GetFakeUserAsync();
                var serverUri = new Uri($"realm://localhost:9080/~/compactrealm_{encrypt}_{populate}.realm");

                var config = new SyncConfiguration(user, serverUri);
                if (encrypt)
                {
                    config.EncryptionKey = new byte[64];
                    config.EncryptionKey[0] = 5;
                }

                using (var realm = GetRealm(config))
                {
                    if (populate)
                    {
                        AddDummyData(realm, singleTransaction: false);
                    }
                }

                var initialSize = new FileInfo(config.DatabasePath).Length;

                Assert.That(Realm.Compact(config));

                var finalSize = new FileInfo(config.DatabasePath).Length;
                Assert.That(initialSize >= finalSize);

                using (var realm = GetRealm(config))
                {
                    Assert.That(realm.All<IntPrimaryKeyWithValueObject>().Count(), Is.EqualTo(populate ? 500 : 0));
                }
            });
        }

#if !ROS_SETUP
        [Explicit("Update Constants.ServerUrl with values that work on your setup.")]
#endif
        [TestCase(true)]
        [TestCase(false)]
        public void GetInstanceAsync_ShouldDownloadRealm(bool singleTransaction)
        {
            AsyncContext.Run(async () =>
            {
                var user = await SyncTestHelpers.GetUserAsync();

                var realmUri = SyncTestHelpers.RealmUri("~/GetInstanceAsync_ShouldDownloadRealm");

                var config = new SyncConfiguration(user, realmUri);
                var asyncConfig = new SyncConfiguration(user, realmUri, config.DatabasePath + "_async");

                using (var realm = GetRealm(config))
                {
                    AddDummyData(realm, singleTransaction);

                    await GetSession(realm).WaitForUploadAsync();
                }

                using (var asyncRealm = await GetRealmAsync(asyncConfig))
                {
                    Assert.That(asyncRealm.All<IntPrimaryKeyWithValueObject>().Count(), Is.EqualTo(500));
                }
            });
        }

#if !ROS_SETUP
        [Explicit("Update Constants.ServerUrl with values that work on your setup.")]
#endif
        [TestCase(true)]
        [TestCase(false)]
        public void GetInstanceAsync_OpensReadonlyRealm(bool singleTransaction)
        {
            AsyncContext.Run(async () =>
            {
                var alice = await SyncTestHelpers.GetUserAsync();
                var bob = await SyncTestHelpers.GetUserAsync();

                var realmUri = SyncTestHelpers.RealmUri($"{alice.Identity}/GetInstanceAsync_OpensReadonlyRealm");
                var aliceConfig = new SyncConfiguration(alice, realmUri);
                var aliceRealm = GetRealm(aliceConfig);

                await alice.ApplyPermissionsAsync(PermissionCondition.UserId(bob.Identity), realmUri.AbsoluteUri, AccessLevel.Read).Timeout(1000);

                AddDummyData(aliceRealm, singleTransaction);

                var bobConfig = new SyncConfiguration(bob, realmUri);
                var bobRealm = await GetRealmAsync(bobConfig);

                var bobsObjects = bobRealm.All<IntPrimaryKeyWithValueObject>();
                var alicesObjects = aliceRealm.All<IntPrimaryKeyWithValueObject>();
                Assert.That(bobsObjects.Count(), Is.EqualTo(alicesObjects.Count()));

                var bobTcs = new TaskCompletionSource<object>();
                bobsObjects.AsRealmCollection().CollectionChanged += (sender, e) =>
                {
                    bobTcs.TrySetResult(null);
                };

                aliceRealm.Write(() =>
                {
                    aliceRealm.Add(new IntPrimaryKeyWithValueObject
                    {
                        Id = 9999,
                        StringValue = "Some value"
                    });
                });

                await bobTcs.Task.Timeout(1000);

                Assert.That(bobsObjects.Count(), Is.EqualTo(alicesObjects.Count()));

                var bobObject = bobRealm.Find<IntPrimaryKeyWithValueObject>(9999);
                Assert.That(bobObject, Is.Not.Null);
                Assert.That(bobObject.StringValue, Is.EqualTo("Some value"));
            });
        }

#if !ROS_SETUP
        [Explicit("Update Constants.ServerUrl with values that work on your setup.")]
#endif
        [Test]
        public void GetInstanceAsync_CreatesNonExistentRealm()
        {
            AsyncContext.Run(async () =>
            {
                var user = await SyncTestHelpers.GetUserAsync();
                var realmUri = SyncTestHelpers.RealmUri("~/GetInstanceAsync_CreatesNonExistentRealm");
                var config = new SyncConfiguration(user, realmUri);

                try
                {
                    await GetRealmAsync(config);
                }
                catch (Exception ex)
                {
                    Assert.That(ex, Is.TypeOf<RealmException>().And.InnerException.TypeOf<SessionException>());
                    var sessionException = (SessionException)ex.InnerException;
                    Assert.That(sessionException.ErrorCode, Is.EqualTo((ErrorCode)89));
                    Assert.That(sessionException.Message, Contains.Substring("Operation canceled"));
                }
            });
        }

        private static void AddDummyData(Realm realm, bool singleTransaction)
        {
            Action<Action> write;
            Transaction currentTransaction = null;

            if (singleTransaction)
            {
                write = action => action();
                currentTransaction = realm.BeginWrite();
            }
            else
            {
                write = realm.Write;
            }

            for (var i = 0; i < 1000; i++)
            {
                write(() =>
                {
                    realm.Add(new IntPrimaryKeyWithValueObject
                    {
                        Id = i,
                        StringValue = "Super secret product " + i
                    });
                });
            }

            if (singleTransaction)
            {
                currentTransaction.Commit();
                currentTransaction = realm.BeginWrite();
            }

            for (var i = 0; i < 500; i++)
            {
                write(() =>
                {
                    var item = realm.Find<IntPrimaryKeyWithValueObject>(2 * i);
                    realm.Remove(item);
                });
            }

            if (singleTransaction)
            {
                currentTransaction.Commit();
            }
        }
    }
}