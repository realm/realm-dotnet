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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Realms.Exceptions;
using Realms.Logging;
using Realms.Schema;
using Realms.Sync;
using Realms.Sync.ErrorHandling;
using Realms.Sync.Exceptions;

namespace Realms.Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class 
        : SyncTestBase
    {
        private const int OneMegabyte = 1024 * 1024;
        private const int NumberOfObjects = 4;

        [Test]
        public void Compact_ShouldReduceSize([Values(true, false)] bool encrypt, [Values(true, false)] bool populate)
        {
            TestHelpers.RunAsyncTest(async () =>
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

                await TestHelpers.WaitForConditionAsync(() => Realm.Compact(config), errorMessage: "Expected compact to succeed, but it didn't");

                var finalSize = new FileInfo(config.DatabasePath).Length;
                Assert.That(initialSize, Is.GreaterThanOrEqualTo(finalSize));

                using (var realm = GetRealm(config))
                {
                    Assert.That(realm.All<ObjectIdPrimaryKeyWithValueObject>().Count(), Is.EqualTo(populate ? DummyDataSize / 2 : 0));
                }
            });
        }

        [Test]
        public void ShouldCompact_IsInvokedAfterOpening([Values(true, false)] bool shouldCompact, [Values(true, false)] bool useSync)
        {
            RealmConfigurationBase config = useSync ? GetFakeConfig() : new RealmConfiguration(Guid.NewGuid().ToString());

            using (var realm = GetRealm(config))
            {
                AddDummyData(realm, singleTransaction: false);
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

                Assert.That(realm.All<ObjectIdPrimaryKeyWithValueObject>().Count(), Is.EqualTo(DummyDataSize / 2));
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
                config = await GetIntegrationConfigAsync((string?)config.Partition);
                config.OnProgress = (progress) =>
                {
                    callbacksInvoked++;
                    lastProgress = progress;
                };

                using var realm = await GetRealmAsync(config);
                Assert.That(realm.All<HugeSyncObject>().Count(), Is.EqualTo(NumberOfObjects));
                Assert.That(callbacksInvoked, Is.GreaterThan(0));
                Assert.That(lastProgress.ProgressEstimate, Is.GreaterThan(0.0));
            }, 60000);
        }

        [Test]
        public void GetInstanceAsync_WithOnProgress_DoesntThrowWhenOnProgressIsSetToNull()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var config = await GetIntegrationConfigAsync();

                await PopulateData(config);

                var callbacksInvoked = 0;

                var lastProgress = default(SyncProgress);
                config = await GetIntegrationConfigAsync((string?)config.Partition);
                config.OnProgress = (progress) =>
                {
                    callbacksInvoked++;
                    lastProgress = progress;
                };

                var realmTask = GetRealmAsync(config);
                config.OnProgress = null;

                using var realm = await realmTask;

                Assert.That(realm.All<HugeSyncObject>().Count(), Is.EqualTo(NumberOfObjects));
                Assert.That(callbacksInvoked, Is.GreaterThan(0));
                Assert.That(lastProgress.ProgressEstimate, Is.GreaterThan(0.0));
            }, 60000);
        }

        [Test]
        public void GetInstanceAsync_WithOnProgressThrowing_ReportsErrorToLogs()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var config = await GetIntegrationConfigAsync();

                await PopulateData(config);

                var logger = new RealmLogger.InMemoryLogger();
                RealmLogger.Default = logger;

                config = await GetIntegrationConfigAsync((string?)config.Partition);
                config.OnProgress = _ => throw new Exception("Exception in OnProgress");

                var realmTask = GetRealmAsync(config);
                config.OnProgress = null;

                using var realm = await realmTask;

                Assert.That(realm.All<HugeSyncObject>().Count(), Is.EqualTo(NumberOfObjects));

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

                config = await GetIntegrationConfigAsync((string?)config.Partition);

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
            config.Schema = new[] { typeof(IntPrimaryKeyWithValueObject) };

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

            Assert.That(dynamicRealm.Schema.TryFindObjectSchema(nameof(IntPrimaryKeyWithValueObject), out var objectSchema), Is.True);
            Assert.That(objectSchema, Is.Not.Null);

            Assert.That(objectSchema!.TryFindProperty(nameof(IntPrimaryKeyWithValueObject.StringValue), out var stringProp));
            Assert.That(stringProp.Type, Is.EqualTo(PropertyType.String | PropertyType.Nullable));

            var dynamicObj = dynamicRealm.DynamicApi.All(nameof(IntPrimaryKeyWithValueObject)).Single();
            Assert.That(dynamicObj.DynamicApi.Get<string>(nameof(IntPrimaryKeyWithValueObject.StringValue)), Is.EqualTo("This is a string!"));
        }

        [Test]
        public void GetInstance_WhenDynamicAndDoesntExist_ReturnsEmptySchema()
        {
            var config = GetFakeConfig();
            config.Schema = null!;
            config.IsDynamic = true;

            using var realm = GetRealm(config);
            Assert.That(realm.Schema, Is.Empty);
        }

        [Test, Ignore("Doesn't work due to a OS bug")]
        public void InvalidSchemaChange_RaisesClientReset()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var errorTcs = new TaskCompletionSource<ClientResetException>();
                var config = await GetIntegrationConfigAsync();
                config.ClientResetHandler = new ManualRecoveryHandler((error) =>
                {
                    errorTcs.TrySetResult(error);
                });

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

                using var realm2 = GetRealm(config);

                var clientEx = await errorTcs.Task.Timeout(5000);

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
            conf.Schema = new[] { typeof(EmbeddedLevel3) };

            Assert.Throws<RealmSchemaValidationException>(() => Realm.GetInstance(conf), $"Embedded object {nameof(EmbeddedLevel3)} is unreachable by any link path from top level objects");
        }

        [Test]
        public void WriteCopy_CanSynchronizeData([Values(true, false)] bool originalEncrypted,
                                                 [Values(true, false)] bool copyEncrypted)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var partition = Guid.NewGuid().ToString();

                var originalConfig = await GetIntegrationConfigAsync(partition);
                if (originalEncrypted)
                {
                    originalConfig.EncryptionKey = TestHelpers.GetEncryptionKey(42);
                }

                var copyConfig = await GetIntegrationConfigAsync(partition);
                Assert.That(originalConfig.Partition, Is.EqualTo(copyConfig.Partition));
                if (copyEncrypted)
                {
                    copyConfig.EncryptionKey = TestHelpers.GetEncryptionKey(14);
                }

                using var originalRealm = GetRealm(originalConfig);

                AddDummyData(originalRealm, true);

                await WaitForUploadAsync(originalRealm);
                await WaitForDownloadAsync(originalRealm);

                originalRealm.WriteCopy(copyConfig);

                using var copiedRealm = GetRealm(copyConfig);

                Assert.That(copiedRealm.All<ObjectIdPrimaryKeyWithValueObject>().Count(), Is.EqualTo(originalRealm.All<ObjectIdPrimaryKeyWithValueObject>().Count()));

                var fromCopy = copiedRealm.Write(() => copiedRealm.Add(new ObjectIdPrimaryKeyWithValueObject
                {
                    StringValue = "Added from copy"
                }));

                await WaitForUploadAsync(copiedRealm);
                await WaitForDownloadAsync(originalRealm);

                var itemInOriginal = originalRealm.Find<ObjectIdPrimaryKeyWithValueObject>(fromCopy.Id);
                Assert.That(itemInOriginal, Is.Not.Null);
                Assert.That(itemInOriginal!.StringValue, Is.EqualTo(fromCopy.StringValue));

                var fromOriginal = originalRealm.Write(() => originalRealm.Add(new ObjectIdPrimaryKeyWithValueObject
                {
                    StringValue = "Added from original"
                }));

                await WaitForUploadAsync(originalRealm);
                await WaitForDownloadAsync(copiedRealm);

                var itemInCopy = copiedRealm.Find<ObjectIdPrimaryKeyWithValueObject>(fromOriginal.Id);
                Assert.That(itemInCopy, Is.Not.Null);
                Assert.That(itemInCopy!.StringValue, Is.EqualTo(fromOriginal.StringValue));
            });
        }

        [Test]
        public void WriteCopy_LocalToSync([Values(true, false)] bool originalEncrypted,
                                          [Values(true, false)] bool copyEncrypted)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var originalConfig = new RealmConfiguration(Guid.NewGuid().ToString())
                {
                    Schema = new[] { typeof(ObjectIdPrimaryKeyWithValueObject) }
                };
                if (originalEncrypted)
                {
                    originalConfig.EncryptionKey = TestHelpers.GetEncryptionKey(42);
                }

                var copyConfig = await GetIntegrationConfigAsync(Guid.NewGuid().ToString());
                if (copyEncrypted)
                {
                    copyConfig.EncryptionKey = TestHelpers.GetEncryptionKey(23);
                }

                using var originalRealm = GetRealm(originalConfig);

                AddDummyData(originalRealm, true);

                var addedObjects = originalRealm.All<ObjectIdPrimaryKeyWithValueObject>().Count();

                originalRealm.WriteCopy(copyConfig);

                if (copyEncrypted)
                {
                    var validKey = copyConfig.EncryptionKey;
                    copyConfig.EncryptionKey = null;

                    Assert.Throws<RealmInvalidDatabaseException>(() => GetRealm(copyConfig));

                    copyConfig.EncryptionKey = TestHelpers.GetEncryptionKey(1, 2, 3);
                    Assert.Throws<RealmInvalidDatabaseException>(() => GetRealm(copyConfig));

                    copyConfig.EncryptionKey = validKey;
                }

                using var copiedRealm = GetRealm(copyConfig);

                Assert.That(copiedRealm.All<ObjectIdPrimaryKeyWithValueObject>().Count(), Is.EqualTo(addedObjects));

                await WaitForUploadAsync(copiedRealm);

                var anotherUserRealm = await GetIntegrationRealmAsync(copyConfig.Partition.AsString());

                Assert.That(anotherUserRealm.All<ObjectIdPrimaryKeyWithValueObject>().Count(), Is.EqualTo(addedObjects));

                var addedObject = anotherUserRealm.Write(() => anotherUserRealm.Add(new ObjectIdPrimaryKeyWithValueObject
                {
                    StringValue = "abc"
                }));

                await WaitForUploadAsync(anotherUserRealm);
                await WaitForDownloadAsync(copiedRealm);

                var syncedObject = copiedRealm.Find<ObjectIdPrimaryKeyWithValueObject>(addedObject.Id)!;

                Assert.That(syncedObject.StringValue, Is.EqualTo("abc"));
            });
        }

        [Test, Ignore("Maybe crashing on evergreen")]
        public void WriteCopy_SyncToLocal([Values(true, false)] bool originalEncrypted,
                                          [Values(true, false)] bool copyEncrypted)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var originalConfig = await GetIntegrationConfigAsync(Guid.NewGuid().ToString());
                if (originalEncrypted)
                {
                    originalConfig.EncryptionKey = TestHelpers.GetEncryptionKey(42);
                }

                var copyConfig = new RealmConfiguration(Guid.NewGuid().ToString());
                if (copyEncrypted)
                {
                    copyConfig.EncryptionKey = TestHelpers.GetEncryptionKey(23);
                }

                using var originalRealm = GetRealm(originalConfig);

                AddDummyData(originalRealm, true);

                await WaitForUploadAsync(originalRealm);

                originalRealm.WriteCopy(copyConfig);

                // In the server-side schema for each model there's the field to hold what partition each object belongs to, namely realm_id.
                // Such field is optional and we don't declare it in our models.
                // When the conversion from sync to local happens realm_id is written in the local realm. The discrepancy generated by the conversion
                // starts a schema migration. Bumping up the SchemaVersion triggers an implicit migration that removes realm_id.
                copyConfig.SchemaVersion++;
                using var copiedRealm = GetRealm(copyConfig);

                Assert.That(copiedRealm.All<ObjectIdPrimaryKeyWithValueObject>().Count(), Is.EqualTo(originalRealm.All<ObjectIdPrimaryKeyWithValueObject>().Count()));
            });
        }

        [Test]
        public void WriteCopy_FailsWhenPartitionsDiffer([Values(true, false)] bool originalEncrypted,
                                                        [Values(true, false)] bool copyEncrypted)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var originalPartition = Guid.NewGuid().ToString();
                var originalConfig = await GetIntegrationConfigAsync(originalPartition);
                if (originalEncrypted)
                {
                    originalConfig.EncryptionKey = TestHelpers.GetEncryptionKey(42);
                }

                var copiedPartition = Guid.NewGuid().ToString();
                var copyConfig = await GetIntegrationConfigAsync(copiedPartition);
                if (copyEncrypted)
                {
                    copyConfig.EncryptionKey = TestHelpers.GetEncryptionKey(14);
                }

                Assert.That(originalConfig.Partition, !Is.EqualTo(copyConfig.Partition));

                using var originalRealm = GetRealm(originalConfig);

                Assert.Throws<NotSupportedException>(() => originalRealm.WriteCopy(copyConfig));
            });
        }

        [Test]
        public void RemoveAll_RemovesAllElements([Values(true, false)] bool originalEncrypted)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var realmConfig = await GetIntegrationConfigAsync(Guid.NewGuid().ToString());
                if (originalEncrypted)
                {
                    realmConfig.EncryptionKey = TestHelpers.GetEncryptionKey(42);
                }

                var realm = GetRealm(realmConfig);

                AddDummyData(realm, true);

                await WaitForUploadAsync(realm);

                Assert.That(realm.All<ObjectIdPrimaryKeyWithValueObject>().Count(), Is.EqualTo(DummyDataSize / 2));

                realm.Write(() =>
                {
                    realm.RemoveAll();
                });

                Assert.That(realm.All<ObjectIdPrimaryKeyWithValueObject>().Count(), Is.EqualTo(0));
                await WaitForUploadAsync(realm);
                realm.Dispose();

                // Ensure that the Realm can be deleted from the filesystem. If the sync
                // session was still using it, we would get a permission denied error.
                Assert.That(DeleteRealmWithRetries(realm.Config), Is.True);

                using var asyncRealm = await GetRealmAsync(realmConfig);
                Assert.That(asyncRealm.All<ObjectIdPrimaryKeyWithValueObject>().Count(), Is.EqualTo(0));
            });
        }

        [Test]
        public void WriteCopy_FailsWhenNotFinished([Values(true, false)] bool originalEncrypted,
                                                   [Values(true, false)] bool copyEncrypted)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var partition = Guid.NewGuid().ToString();

                var originalConfig = await GetIntegrationConfigAsync(partition);
                if (originalEncrypted)
                {
                    originalConfig.EncryptionKey = TestHelpers.GetEncryptionKey(42);
                }

                var copyConfig = await GetIntegrationConfigAsync(partition);
                if (copyEncrypted)
                {
                    copyConfig.EncryptionKey = TestHelpers.GetEncryptionKey(14);
                }

                using var originalRealm = GetRealm(originalConfig);

                AddDummyData(originalRealm, true);

                // The error is thrown as a generic `RealmError` by Core which translates to a generic `RealmException` on our side.
                Assert.Throws<RealmException>(() => originalRealm.WriteCopy(copyConfig));
            });
        }

        [Test]
        public void WriteCopy_FailsWithEmptyConfig()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var partition = Guid.NewGuid().ToString();
                var originalConfig = await GetIntegrationConfigAsync(partition);
                using var originalRealm = GetRealm(originalConfig);
                Assert.Throws<ArgumentNullException>(() => originalRealm.WriteCopy(null!));
            });
        }

        [Test]
        public void WriteCopy_ThrowsWhenConvertingFromLocalToFLX()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                using var localRealm = GetRealm();
                var flexConfig = await GetFLXIntegrationConfigAsync();

                var ex = Assert.Throws<NotSupportedException>(() => localRealm.WriteCopy(flexConfig))!;
                Assert.That(ex.Message, Does.Contain("Writing a copy to a flexible sync realm is not supported unless flexible sync is already enabled"));
            });
        }

        [Test]
        public void WriteCopy_ThrowsWhenConvertingFromPBSToFLX()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                using var pbsRealm = await GetIntegrationRealmAsync();
                var flexConfig = await GetFLXIntegrationConfigAsync();

                var ex = Assert.Throws<NotSupportedException>(() => pbsRealm.WriteCopy(flexConfig))!;
                Assert.That(ex.Message, Does.Contain("Writing a copy to a flexible sync realm is not supported unless flexible sync is already enabled"));
            });
        }

        [Test]
        public void WriteCopy_ThrowsWhenConvertingFromFLXToPBS()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                using var flxRealm = await GetFLXIntegrationRealmAsync();
                var pbsConfig = await GetIntegrationConfigAsync();

                var ex = Assert.Throws<NotSupportedException>(() => flxRealm.WriteCopy(pbsConfig))!;
                Assert.That(ex.Message, Does.Contain("Changing from flexible sync sync to partition based sync is not supported when writing a Realm copy"));
            });
        }

        [Test]
        public void DeleteRealmWorksIfCalledMultipleTimes()
        {
            var config = GetFakeConfig();
            var openRealm = GetRealm(config);
            openRealm.Dispose();
            Assert.That(File.Exists(config.DatabasePath));

            Assert.That(() => DeleteRealmWithRetries(openRealm.Config), Is.True);
            Assert.That(() => DeleteRealmWithRetries(openRealm.Config), Is.True);
        }

        [Test]
        public void DeleteRealm_AfterDispose_Succeeds([Values(true, false)] bool singleTransaction)
        {
            // This test verifies that disposing a Realm will eventually close its session and
            // release the file, so that we can delete it.
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var partition = Guid.NewGuid().ToString();

                var config = await GetIntegrationConfigAsync(partition);
                var asyncConfig = await GetIntegrationConfigAsync(partition);

                var realm = GetRealm(config);
                AddDummyData(realm, singleTransaction);

                await WaitForUploadAsync(realm);
                realm.Dispose();

                // Ensure that the Realm can be deleted from the filesystem. If the sync
                // session was still using it, we would get a permission denied error.
                Assert.That(DeleteRealmWithRetries(realm.Config), Is.True);

                using var asyncRealm = await GetRealmAsync(asyncConfig);
                Assert.That(asyncRealm.All<ObjectIdPrimaryKeyWithValueObject>().Count(), Is.EqualTo(DummyDataSize / 2));
            }, timeout: 120000);
        }

        [Test]
        public void RealmDispose_ClosesSessions()
        {
            var config = GetFakeConfig();
            var realm = GetRealm(config);
            var session = GetSession(realm);
            realm.Dispose();

            Assert.That(session.IsClosed);

            // Dispose should close the session and allow us to delete the Realm.
            Assert.That(DeleteRealmWithRetries(realm.Config), Is.True);
        }

        [Test]
        public void SyncTimeouts_ArePassedCorrectlyToCore()
        {
            var logger = new RealmLogger.InMemoryLogger();
            RealmLogger.Default = logger;
            RealmLogger.SetLogLevel(LogLevel.Debug);

            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var appConfig = SyncTestHelpers.GetAppConfig();
                appConfig.SyncTimeoutOptions.ConnectTimeout = TimeSpan.FromMilliseconds(1234);
                appConfig.SyncTimeoutOptions.ConnectionLingerTime = TimeSpan.FromMilliseconds(3456);
                appConfig.SyncTimeoutOptions.PingKeepAlivePeriod = TimeSpan.FromMilliseconds(5678);
                appConfig.SyncTimeoutOptions.PongKeepAliveTimeout = TimeSpan.FromMilliseconds(7890);
                appConfig.SyncTimeoutOptions.FastReconnectLimit = TimeSpan.FromMilliseconds(9012);
                var app = CreateApp(appConfig);
                var config = await GetIntegrationConfigAsync(app: app);

                using var realm = await GetRealmAsync(config);

                var logs = logger.GetLog();

                Assert.That(logs, Does.Contain("Config param: connect_timeout = 1234 ms"));
                Assert.That(logs, Does.Contain("Config param: connection_linger_time = 3456 ms"));
                Assert.That(logs, Does.Contain("Config param: ping_keepalive_period = 5678 ms"));
                Assert.That(logs, Does.Contain("Config param: pong_keepalive_timeout = 7890 ms"));
                Assert.That(logs, Does.Contain("Config param: fast_reconnect_limit = 9012 ms"));
            });
        }

        [Test, Ignore("Enable when https://github.com/realm/realm-core/issues/6301 is addressed")]
        public void CancelAsyncOperationsOnNonFatalErrors_WhenTrue_ShouldCancelAsyncOperationsOnTimeout()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var appConfig = SyncTestHelpers.GetAppConfig();

                // 1 ms timeout is way too short to establish a connection
                appConfig.SyncTimeoutOptions.ConnectTimeout = TimeSpan.FromMilliseconds(1);

                var app = CreateApp(appConfig);
                var config = await GetIntegrationConfigAsync(app: app);
                config.CancelAsyncOperationsOnNonFatalErrors = true;

                var ex = await TestHelpers.AssertThrows<RealmException>(() => GetRealmAsync(config));
                Assert.That(ex.InnerException, Is.TypeOf<SessionException>().And.Message.Contains("Sync connection was not fully established in time"));
            });
        }

        [Test, Ignore("Enable when https://github.com/realm/realm-core/issues/6301 is addressed")]
        public void CancelAsyncOperationsOnNonFatalErrors_WhenFalse_ShouldNotCancelAsyncOperationsOnTimeout()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var appConfig = SyncTestHelpers.GetAppConfig();

                // 1 ms timeout is way too short to establish a connection
                appConfig.SyncTimeoutOptions.ConnectTimeout = TimeSpan.FromMilliseconds(1);

                var app = CreateApp(appConfig);
                var config = await GetIntegrationConfigAsync(app: app);
                config.CancelAsyncOperationsOnNonFatalErrors = false;

                // Connection should timeout immediately, but we should continue retrying until we eventually
                // timeout the GetRealmAsync operation
                var ex = await TestHelpers.AssertThrows<TimeoutException>(() => GetRealmAsync(config), timeout: 1000);
                Assert.That(ex.Message, Does.Contain("The operation has timed out after 1000 ms"));
            });
        }

        [Test]
        public void SyncLogger_WhenLevelChanges_LogsAtNewLevel()
        {
            var logs = new Dictionary<LogLevel, List<string>>();
            foreach (LogLevel level in Enum.GetValues(typeof(LogLevel)))
            {
                logs[level] = new();
            }

            var regex = new Regex("Connection\\[\\d+] Session\\[\\d+]");
            var logger = RealmLogger.Function((level, category, msg) =>
            {
                if (regex.IsMatch(msg))
                {
                    logs[level].Add(msg);
                }
            });

            RealmLogger.SetLogLevel(LogLevel.Info);
            RealmLogger.Default = logger;

            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var config = await GetIntegrationConfigAsync();

                using var realm = await GetRealmAsync(config);

                var initialInfoLogs = logs[LogLevel.Info].Count;
                Assert.That(initialInfoLogs, Is.GreaterThan(0));
                Assert.That(logs[LogLevel.Debug].Count, Is.EqualTo(0));

                RealmLogger.SetLogLevel(LogLevel.Debug);

                realm.Write(() =>
                {
                    realm.Add(new PrimaryKeyStringObject
                    {
                        Id = Guid.NewGuid().ToString(),
                    });
                });

                await WaitForUploadAsync(realm);

                Assert.That(logs[LogLevel.Info].Count, Is.GreaterThan(0));
                Assert.That(logs[LogLevel.Debug].Count, Is.GreaterThan(0));
            });
        }

        private const int DummyDataSize = 100;

        private static void AddDummyData(Realm realm, bool singleTransaction)
        {
            Action<Action> write;
            Transaction? currentTransaction = null;

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
                currentTransaction!.Commit();
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
                currentTransaction!.Commit();
            }
        }

        private async Task PopulateData(PartitionSyncConfiguration config, int numberOfObjects = NumberOfObjects)
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
