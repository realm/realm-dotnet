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
using NUnit.Framework;
using Realms.Exceptions;
using Realms.Logging;
using Realms.Schema;
using Realms.Sync;
using Realms.Sync.Exceptions;

namespace Realms.Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class SynchronizedInstanceTests : SyncTestBase
    {
        private const int OneMegabyte = 1024 * 1024;
        private const int NumberOfObjects = 20;

        [Test]
        public void Compact_ShouldReduceSize([Values(true, false)] bool encrypt, [Values(true, false)] bool populate)
        {
            var config = GetFakeConfig();
            if (encrypt)
            {
                config.EncryptionKey = TestHelpers.GetEncryptionKey(5);
            }

            using (var realm = GetRealm(config))
            {
                var session = GetSession(realm);
                session.Stop();
                if (populate)
                {
                    AddDummyData(realm, singleTransaction: false);
                }

                session.CloseHandle();
            }

            var initialSize = new FileInfo(config.DatabasePath).Length;

            Assert.That(Realm.Compact(config), Is.True);

            var finalSize = new FileInfo(config.DatabasePath).Length;
            Assert.That(initialSize, Is.GreaterThanOrEqualTo(finalSize));

            using (var realm = GetRealm(config))
            {
                Assert.That(realm.All<ObjectIdPrimaryKeyWithValueObject>().Count(), Is.EqualTo(populate ? DummyDataSize / 2 : 0));
            }
        }

        [Test]
        public void GetInstanceAsync_ShouldDownloadRealm([Values(true, false)] bool singleTransaction)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var partition = Guid.NewGuid().ToString();

                var config = await GetIntegrationConfigAsync(partition);
                var asyncConfig = await GetIntegrationConfigAsync(partition);

                using var realm = GetRealm(config);
                AddDummyData(realm, singleTransaction);

                await WaitForUploadAsync(realm);

                using var asyncRealm = await GetRealmAsync(asyncConfig);
                Assert.That(asyncRealm.All<ObjectIdPrimaryKeyWithValueObject>().Count(), Is.EqualTo(DummyDataSize / 2));
            }, timeout: 120000);
        }

        [Test]
        public void GetInstanceAsync_CreatesNonExistentRealm()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var config = await GetIntegrationConfigAsync();
                await GetRealmAsync(config);
            });
        }

        [Test]
        public void GetInstanceAsync_ReportsProgress()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var config = await GetIntegrationConfigAsync();

                await PopulateData(config);

                var callbacksInvoked = 0;

                var lastProgress = default(SyncProgress);
                config = await GetIntegrationConfigAsync((string)config.Partition);
                config.OnProgress = (progress) =>
                {
                    callbacksInvoked++;
                    lastProgress = progress;
                };

                using var realm = await GetRealmAsync(config);
                Assert.That(realm.All<HugeSyncObject>().Count(), Is.EqualTo(NumberOfObjects));
                Assert.That(callbacksInvoked, Is.GreaterThan(0));
                Assert.That(lastProgress.TransferableBytes, Is.EqualTo(lastProgress.TransferredBytes));
            }, 60000);
        }

        [Test]
        public void GetInstanceAsync_WithOnProgress_DoesntThrowWhenOnProgressIsSetToNull()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var config = await GetIntegrationConfigAsync();

                await PopulateData(config, 4);

                var callbacksInvoked = 0;

                var lastProgress = default(SyncProgress);
                config = await GetIntegrationConfigAsync((string)config.Partition);
                config.OnProgress = (progress) =>
                {
                    callbacksInvoked++;
                    lastProgress = progress;
                };

                var realmTask = GetRealmAsync(config);
                config.OnProgress = null;

                using var realm = await realmTask;

                Assert.That(realm.All<HugeSyncObject>().Count(), Is.EqualTo(4));
                Assert.That(callbacksInvoked, Is.GreaterThan(0));
                Assert.That(lastProgress.TransferableBytes, Is.EqualTo(lastProgress.TransferredBytes));
            }, 60000);
        }

        [Test]
        public void GetInstanceAsync_WithOnProgressThrowing_ReportsErrorToLogs()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var config = await GetIntegrationConfigAsync();

                await PopulateData(config, 4);

                var logger = new Logger.InMemoryLogger();
                Logger.Default = logger;

                config = await GetIntegrationConfigAsync((string)config.Partition);
                config.OnProgress = (progress) =>
                {
                    throw new Exception("Exception in OnProgress");
                };

                var realmTask = GetRealmAsync(config);
                config.OnProgress = null;

                using var realm = await realmTask;

                Assert.That(realm.All<HugeSyncObject>().Count(), Is.EqualTo(4));

                // Notifications are delivered async, so let's wait a little
                await TestHelpers.WaitForConditionAsync(() => logger.GetLog().Contains("Exception in OnProgress"));
                Assert.That(logger.GetLog(), Does.Contain("Exception in OnProgress"));
            }, 60000);
        }

        [Test]
        public void GetInstanceAsync_Cancel_ShouldCancelWait()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var config = await GetIntegrationConfigAsync();
                await PopulateData(config);

                config = await GetIntegrationConfigAsync((string)config.Partition);

                using var cts = new CancellationTokenSource(10);

                try
                {
                    var realm = await Realm.GetInstanceAsync(config, cts.Token);
                    CleanupOnTearDown(realm);
                    Assert.Fail("Expected task to be cancelled.");
                }
                catch (Exception ex)
                {
                    Assert.That(ex, Is.InstanceOf<TaskCanceledException>());
                }
            });
        }

        [Test]
        public void GetInstance_WhenDynamic_ReadsSchemaFromDisk()
        {
            var config = GetFakeConfig();
            config.ObjectClasses = new[] { typeof(IntPrimaryKeyWithValueObject) };

            // Create the realm and add some objects
            using (var realm = GetRealm(config))
            {
                realm.Write(() => realm.Add(new IntPrimaryKeyWithValueObject
                {
                    Id = 42,
                    StringValue = "This is a string!"
                }));
            }

            config.IsDynamic = true;

            using var dynamicRealm = GetRealm(config);
            Assert.That(dynamicRealm.Schema.Count == 1);

            var objectSchema = dynamicRealm.Schema.Find(nameof(IntPrimaryKeyWithValueObject));
            Assert.That(objectSchema, Is.Not.Null);

            Assert.That(objectSchema.TryFindProperty(nameof(IntPrimaryKeyWithValueObject.StringValue), out var stringProp));
            Assert.That(stringProp.Type, Is.EqualTo(PropertyType.String | PropertyType.Nullable));

            var dynamicObj = ((IQueryable<RealmObject>)dynamicRealm.DynamicApi.All(nameof(IntPrimaryKeyWithValueObject))).Single();
            Assert.That(dynamicObj.DynamicApi.Get<string>(nameof(IntPrimaryKeyWithValueObject.StringValue)), Is.EqualTo("This is a string!"));
        }

        [Test]
        public void GetInstance_WhenDynamicAndDoesntExist_ReturnsEmptySchema()
        {
            var config = GetFakeConfig();
            config.ObjectClasses = null;
            config.IsDynamic = true;

            using var realm = GetRealm(config);
            Assert.That(realm.Schema, Is.Empty);
        }

        [Test, Ignore("Doesn't work due to a OS bug")]
        public void InvalidSchemaChange_RaisesClientReset()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var config = await GetIntegrationConfigAsync();

                var backupLocation = config.DatabasePath + "_backup";
                using (var realm = await GetRealmAsync(config))
                {
                    // Backup the file
                    File.Copy(config.DatabasePath, backupLocation);

                    realm.Write(() => realm.Add(new HugeSyncObject(1024)));

                    await WaitForUploadAsync(realm);
                }

                // restore the backup
                while (true)
                {
                    try
                    {
                        File.Copy(backupLocation, config.DatabasePath, overwrite: true);
                        break;
                    }
                    catch
                    {
                        await Task.Delay(50);
                    }
                }

                var errorTcs = new TaskCompletionSource<Exception>();
                Session.Error += (s, e) =>
                {
                    errorTcs.TrySetResult(e.Exception);
                };

                using var realm2 = GetRealm(config);

                var ex = await errorTcs.Task.Timeout(5000);

                Assert.That(ex, Is.InstanceOf<ClientResetException>());
                var clientEx = (ClientResetException)ex;

                Assert.That(clientEx.ErrorCode, Is.EqualTo(ErrorCode.InvalidSchemaChange));

                var realmPath = config.DatabasePath;

                Assert.That(File.Exists(realmPath));

                Assert.That(clientEx.InitiateClientReset(), Is.True);

                Assert.That(File.Exists(realmPath), Is.False);
            });
        }

        [Test]
        public void EmbeddedObject_WhenAdditiveExplicit_ShouldThrow()
        {
            var conf = GetFakeConfig();
            conf.ObjectClasses = new[] { typeof(EmbeddedLevel3) };

            Assert.Throws<RealmSchemaValidationException>(() => Realm.GetInstance(conf), $"Embedded object {nameof(EmbeddedLevel3)} is unreachable by any link path from top level objects");
        }

        [Test]
        public void DeleteRealmWorksIfCalledMultipleTimes()
        {
            var config = GetFakeConfig();
            var openRealm = GetRealm(config);
            openRealm.Dispose();
            Assert.That(File.Exists(config.DatabasePath));

            // Race condition:
            // When trying to delete the Realm it can in some occasions (usually when Sync is involved)
            // still be in use. To make sure other threads that use the same Realm get scheduled again
            // and can finish their work before we actually delete the Realm files, we have to wait for
            // a moment here.
            // TODO: remove this when https://github.com/realm/realm-core/issues/4762 is resolved.
            //Task.Delay(5).Wait();

            Assert.DoesNotThrow(() => Realm.DeleteRealm(config));
            Assert.That(File.Exists(config.DatabasePath), Is.False);

            Assert.DoesNotThrow(() => Realm.DeleteRealm(config));
        }

        private const int DummyDataSize = 100;

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

            for (var i = 0; i < DummyDataSize; i++)
            {
                write(() =>
                {
                    realm.Add(new ObjectIdPrimaryKeyWithValueObject
                    {
                        StringValue = "Super secret product " + i
                    });
                });
            }

            if (singleTransaction)
            {
                currentTransaction.Commit();
                currentTransaction = realm.BeginWrite();
            }

            var objs = realm.All<ObjectIdPrimaryKeyWithValueObject>();
            for (var i = 0; i < DummyDataSize / 2; i++)
            {
                write(() =>
                {
                    var item = objs.ElementAt(i);
                    realm.Remove(item);
                });
            }

            if (singleTransaction)
            {
                currentTransaction.Commit();
            }
        }

        private async Task PopulateData(SyncConfiguration config, int numberOfObjects = NumberOfObjects)
        {
            using var realm = GetRealm(config);

            // Split in 2 because MDB Realm has a limit of 16 MB per changeset
            var firstBatch = numberOfObjects / 2;
            var secondBatch = numberOfObjects - firstBatch;

            realm.Write(() =>
            {
                for (var i = 0; i < firstBatch; i++)
                {
                    realm.Add(new HugeSyncObject(OneMegabyte));
                }
            });

            realm.Write(() =>
            {
                for (var i = 0; i < secondBatch; i++)
                {
                    realm.Add(new HugeSyncObject(OneMegabyte));
                }
            });

            await WaitForUploadAsync(realm);
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
