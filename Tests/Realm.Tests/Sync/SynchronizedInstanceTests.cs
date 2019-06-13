﻿////////////////////////////////////////////////////////////////////////////
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
using Realms.Exceptions;
using Realms.Schema;
using Realms.Sync;
using Realms.Sync.Exceptions;
using Realms.Tests.Database;

namespace Realms.Tests.Sync
{
    using ExplicitAttribute = NUnit.Framework.ExplicitAttribute;

    [TestFixture, Preserve(AllMembers = true)]
    public class SynchronizedInstanceTests : SyncTestBase
    {
        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void Compact_ShouldReduceSize(bool encrypt, bool populate)
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var user = await SyncTestHelpers.GetFakeUserAsync();
                var serverUri = new Uri($"/~/compactrealm_{encrypt}_{populate}.realm", UriKind.Relative);

                var config = new FullSyncConfiguration(serverUri, user);
                if (encrypt)
                {
                    config.EncryptionKey = TestHelpers.GetEncryptionKey(5);
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

        [TestCase(true)]
        [TestCase(false)]
        public void GetInstanceAsync_ShouldDownloadRealm(bool singleTransaction)
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                var user = await SyncTestHelpers.GetUserAsync();

                var realmUri = SyncTestHelpers.RealmUri("~/GetInstanceAsync_ShouldDownloadRealm");

                var config = new FullSyncConfiguration(realmUri, user, Guid.NewGuid().ToString());
                var asyncConfig = new FullSyncConfiguration(realmUri, user, config.DatabasePath + "_async");

                using (var realm = GetRealm(config))
                {
                    AddDummyData(realm, singleTransaction);

                    await SyncTestHelpers.WaitForUploadAsync(realm);
                }

                using (var asyncRealm = await GetRealmAsync(asyncConfig))
                {
                    Assert.That(asyncRealm.All<IntPrimaryKeyWithValueObject>().Count(), Is.EqualTo(500));
                }
            });
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GetInstanceAsync_OpensReadonlyRealm(bool singleTransaction)
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                var alice = await SyncTestHelpers.GetUserAsync();
                var bob = await SyncTestHelpers.GetUserAsync();

                var realmUri = SyncTestHelpers.RealmUri($"{alice.Identity}/GetInstanceAsync_OpensReadonlyRealm");
                var aliceConfig = new FullSyncConfiguration(realmUri, alice, Guid.NewGuid().ToString());
                var aliceRealm = GetRealm(aliceConfig);

                await alice.ApplyPermissionsAsync(PermissionCondition.UserId(bob.Identity), realmUri.AbsoluteUri, AccessLevel.Read).Timeout(1000);

                AddDummyData(aliceRealm, singleTransaction);

                var bobConfig = new FullSyncConfiguration(realmUri, bob, Guid.NewGuid().ToString());
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

        [Test]
        public void GetInstanceAsync_CreatesNonExistentRealm()
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                var user = await SyncTestHelpers.GetUserAsync();
                var realmUri = SyncTestHelpers.RealmUri("~/GetInstanceAsync_CreatesNonExistentRealm");
                var config = new FullSyncConfiguration(realmUri, user, Guid.NewGuid().ToString());

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

        [Test]
        public void GetInstanceAsync_ReportsProgress()
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                var realmPath = Guid.NewGuid().ToString();
                var user = await SyncTestHelpers.GetUserAsync();
                var config = new FullSyncConfiguration(new Uri($"/~/{realmPath}", UriKind.Relative), user, Guid.NewGuid().ToString());
                const int ObjectSize = 1000000;
                const int ObjectsToRecord = 20;
                using (var realm = GetRealm(config))
                {
                    for (var i = 0; i < ObjectsToRecord; i++)
                    {
                        realm.Write(() =>
                        {
                            realm.Add(new HugeSyncObject(ObjectSize));
                        });
                    }

                    await SyncTestHelpers.WaitForSyncAsync(realm);
                }

                var callbacksInvoked = 0;

                var lastProgress = default(SyncProgress);
                config = new FullSyncConfiguration(new Uri($"/~/{realmPath}", UriKind.Relative), user, Guid.NewGuid().ToString())
                {
                    OnProgress = (progress) =>
                    {
                        callbacksInvoked++;
                        lastProgress = progress; 
                    }
                };

                using (var realm = await GetRealmAsync(config))
                {
                    Assert.That(realm.All<HugeSyncObject>().Count(), Is.EqualTo(ObjectsToRecord));
                    Assert.That(callbacksInvoked, Is.GreaterThan(0));
                    Assert.That(lastProgress.TransferableBytes, Is.EqualTo(lastProgress.TransferredBytes));
                }
            });
        }

        [Test]
        public void GetInstance_WhenDynamic_ReadsSchemaFromDisk()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var config = await SyncTestHelpers.GetFakeConfigAsync();
                config.ObjectClasses = new[] { typeof(AllTypesObject) };

                // Create the realm and add some objects
                using (var realm = GetRealm(config))
                {
                    realm.Write(() => realm.Add(new AllTypesObject
                    {
                        Int32Property = 42,
                        RequiredStringProperty = "This is required!"
                    }));
                }

                config.IsDynamic = true;

                using (var dynamicRealm = GetRealm(config))
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
            });
        }

        [Test]
        public void GetInstance_WhenDynamicAndDoesntExist_ReturnsEmptySchema()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var config = await SyncTestHelpers.GetFakeConfigAsync();
                config.IsDynamic = true;

                using (var realm = GetRealm(config))
                {
                    Assert.That(realm.Schema, Is.Empty);
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

        /* Code to generate the legacy Realm
        private static async Task<string> GenerateLegacyRealm(bool encrypt)
        {
            var config = await SyncTestHelpers.GetFakeConfigAsync("a@a");
            if (encrypt)
            {
                config.EncryptionKey = new byte[64];
                config.EncryptionKey[0] = 42;
            }

            using (var realm = Realm.GetInstance(config))
            {
                realm.Write(() =>
                {
                    realm.Add(new Person
                    {
                        FirstName = "John",
                        LastName = "Smith"
                    });
                });
            }

            return config.DatabasePath;
        }*/
    }
}