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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baas;
using NUnit.Framework;
using Realms.Exceptions.Sync;
using Realms.Sync;
using Realms.Sync.ErrorHandling;
using Realms.Sync.Exceptions;
using Realms.Sync.Native;
using Realms.Sync.Testing;
using static Realms.Sync.ErrorHandling.ClientResetHandlerBase;
using static Realms.Tests.TestHelpers;
#if TEST_WEAVER
using TestRealmObject = Realms.RealmObject;
#else
using TestRealmObject = Realms.IRealmObject;
#endif

namespace Realms.Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class SessionTests : SyncTestBase
    {
        public static readonly object[] AllClientResetHandlers = new object[]
        {
            typeof(DiscardUnsyncedChangesHandler),
            typeof(RecoverUnsyncedChangesHandler),
            typeof(RecoverOrDiscardUnsyncedChangesHandler),
        };

        [Preserve]
        static SessionTests()
        {
            var preserveRecoverHandler = new RecoverUnsyncedChangesHandler
            {
                OnBeforeReset = (beforeFrozen) => { },
                OnAfterReset = (beforeFrozen, after) => { },
                ManualResetFallback = (clientResetException) => { },
            };

            var preserveRecoverOrDiscardHandler = new RecoverOrDiscardUnsyncedChangesHandler
            {
                OnBeforeReset = (beforeFrozen) => { },
                OnAfterRecovery = (beforeFrozen, after) => { },
                OnAfterDiscard = (beforeFrozen, after) => { },
                ManualResetFallback = (clientResetException) => { },
            };

            var preserveDiscardHandler = new DiscardUnsyncedChangesHandler
            {
                OnBeforeReset = (beforeFrozen) => { },
                OnAfterReset = (beforeFrozen, after) => { },
                ManualResetFallback = (clientResetException) => { },
            };
        }

        public static readonly string[] AppTypes = new[]
        {
            AppConfigType.Default,
            AppConfigType.FlexibleSync
        };

        [Test]
        public void Realm_SyncSession_WhenSyncedRealm()
        {
            var config = GetFakeConfig();

            using var realm = GetRealm(config);
            var session = GetSession(realm);

            Assert.That(session.User, Is.EqualTo(config.User));
        }

        [Test]
        public void Realm_SyncSession_WhenLocalRealm_ShouldThrow()
        {
            using var realm = GetRealm();
            Assert.That(() => realm.SyncSession, Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void Realm_GetSession_ShouldReturnSameObject()
        {
            var config = GetFakeConfig();
            using var realm = GetRealm(config);
            var session1 = GetSession(realm);
            var session2 = GetSession(realm);

            Assert.That(session1, Is.EqualTo(session2));
            Assert.That(session1.GetHashCode(), Is.EqualTo(session2.GetHashCode()));
        }

        [TestCaseSource(nameof(AppTypes))]
        public void Session_ClientReset_ManualRecovery_InitiateClientReset(string appType)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var errorTcs = new TaskCompletionSource<ClientResetException>();
                var (config, _) = await GetConfigForApp(appType);
                config.ClientResetHandler = new ManualRecoveryHandler((e) =>
                {
                    errorTcs.TrySetResult(e);
                });

                using var realm = await GetRealmAsync(config, waitForSync: true);

                await TriggerClientReset(realm);

                var clientEx = await errorTcs.Task;

                Assert.That(clientEx.Message, Does.Contain("Bad client file identifier"));
                Assert.That(clientEx.InnerException, Is.Null);

                await TryInitiateClientReset(realm, clientEx, (int)ErrorCode.BadClientFileIdentifier);
            });
        }

        [Test]
        public void Session_ClientResetHandlers_ManualResetFallback_InitiateClientReset(
            [ValueSource(nameof(AppTypes))] string appType,
            [ValueSource(nameof(AllClientResetHandlers))] Type resetHandlerType)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var errorTcs = new TaskCompletionSource<ClientResetException>();

                var (config, _) = await GetConfigForApp(appType);

                void manualCb(ClientResetException err)
                {
                    errorTcs.TrySetResult(err);
                }

                void beforeCb(Realm _)
                {
                    throw new Exception("This fails!");
                }

                config.ClientResetHandler = GetClientResetHandler(resetHandlerType, beforeCb: beforeCb, manualCb: manualCb);

                using var realm = await GetRealmAsync(config, waitForSync: true, timeout: 20_000);

                await TriggerClientReset(realm);

                var clientEx = await errorTcs.Task.Timeout(20_000, "Expected client reset");

                await TryInitiateClientReset(realm, clientEx, (int)ClientError.AutoClientResetFailed);
            });
        }

        [Test]
        public void Session_ClientResetHandlers_OnBefore_And_OnAfter(
            [ValueSource(nameof(AppTypes))] string appType,
            [ValueSource(nameof(AllClientResetHandlers))] Type resetHandlerType)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var onBeforeTriggered = false;
                var onAfterTriggered = false;
                var tcs = new TaskCompletionSource();

                var (config, _) = await GetConfigForApp(appType);

                var beforeCb = GetOnBeforeHandler(tcs, beforeFrozen =>
                {
                    Assert.That(onBeforeTriggered, Is.False);
                    Assert.That(onAfterTriggered, Is.False);
                    onBeforeTriggered = true;
                });

                var afterCb = GetOnAfterHandler(tcs, (beforeFrozen, after) =>
                {
                    Assert.That(onBeforeTriggered, Is.True);
                    Assert.That(onAfterTriggered, Is.False);
                    onAfterTriggered = true;
                });
                config.ClientResetHandler = GetClientResetHandler(resetHandlerType, beforeCb, afterCb);
                using var realm = await GetRealmAsync(config, waitForSync: true, timeout: 20000);

                await TriggerClientReset(realm);

                await tcs.Task.Timeout(30_000, detail: "Wait for client reset");

                Assert.That(onBeforeTriggered, Is.True);
                Assert.That(onAfterTriggered, Is.True);
            }, timeout: 120_000);
        }

        [TestCaseSource(nameof(AppTypes))]
        public void Session_AutomaticRecoveryFallsbackToDiscardLocal(string appType)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var automaticResetCalled = false;
                var discardLocalResetCalled = false;

                var (config, guid) = await GetConfigForApp(appType);

                var tcsAfterClientReset = new TaskCompletionSource();

                var afterAutomaticResetCb = GetOnAfterHandler(tcsAfterClientReset, (before, after) =>
                {
                    automaticResetCalled = true;
                });

                var afterDiscardLocalResetCb = GetOnAfterHandler(tcsAfterClientReset, (before, after) =>
                {
                    discardLocalResetCalled = true;
                    Assert.That(after.All<ObjectWithPartitionValue>().Count, Is.EqualTo(0));
                });

                config.ClientResetHandler = new RecoverOrDiscardUnsyncedChangesHandler
                {
                    OnAfterRecovery = afterAutomaticResetCb,
                    OnAfterDiscard = afterDiscardLocalResetCb,
                    ManualResetFallback = ex =>
                    {
                        tcsAfterClientReset.TrySetException(ex);
                    }
                };

                var realm = await GetRealmAsync(config, waitForSync: true);

                var session = GetSession(realm);
                session.Stop();

                realm.Write(() =>
                {
                    realm.Add(new ObjectWithPartitionValue(guid));
                });

                await DisableClientResetRecoveryOnServer(appType);
                await TriggerClientReset(realm);

                await tcsAfterClientReset.Task.Timeout(20_000, detail: "Expected client reset");
                Assert.That(automaticResetCalled, Is.False);
                Assert.That(discardLocalResetCalled, Is.True);
            });
        }

        /* Any ArrayInsert to an index beyond the fresh list size is changed to insert to the end of the list.
         *
         * 1. clientA adds objectA with array innerObj[0,1,2] and syncs it, then disconnects
         * 2. clientB starts and syncs the same objectA, then disconnects
         * 3. While offline, clientA deletes innerObj[2] while clientB inserts innerObj[3]
         * 4. A client reset is triggered on the server
         * 5. clientA goes online and uploads the changes
         * 6. only now clientB goes online, downloads and merges the changes. clientB will have innerObj[0,1,3]
         * 7. clientA will also have innerObj[0,1,3]
         */
        [Test, NUnit.Framework.Explicit("This is an integration test testing the client reset behavior and should probably be in Core")]
        public void SessionIntegrationTest_ClientResetHandlers_OutOfBoundArrayInsert_AddedToTail()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var partition = Guid.NewGuid().ToString();

                // ===== clientA =====
                var tcsAfterClientResetA = new TaskCompletionSource();
                var configA = await GetIntegrationConfigAsync(partition);
                configA.Schema = new[] { typeof(SyncObjectWithRequiredStringList) };
                var afterCbA = GetOnAfterHandler(tcsAfterClientResetA, (before, after) =>
                {
                    var list = after.All<SyncObjectWithRequiredStringList>().First().Strings;

                    // We deleted an object, so that should have been merged
                    Assert.That(list, Is.EqualTo(new[] { "0", "1" }));
                });

                configA.ClientResetHandler = new RecoverUnsyncedChangesHandler()
                {
                    OnAfterReset = afterCbA
                };
                using var realmA = await GetRealmAsync(configA, waitForSync: true);

                var originalObj = realmA.Write(() =>
                {
                    var toAdd = new SyncObjectWithRequiredStringList
                    {
                        Id = Guid.NewGuid().ToString()
                    };
                    toAdd.Strings.Add("0");
                    toAdd.Strings.Add("1");
                    toAdd.Strings.Add("2");
                    return realmA.Add(toAdd);
                });
                await WaitForUploadAsync(realmA).Timeout(10_000, detail: "Wait for upload realm A");

                var sessionA = GetSession(realmA);
                sessionA.Stop();

                realmA.Write(() =>
                {
                    originalObj.Strings.RemoveAt(2);
                });

                // ===== clientB =====
                var configB = await GetIntegrationConfigAsync(partition);
                configB.Schema = new[] { typeof(SyncObjectWithRequiredStringList) };
                var tcsAfterClientResetB = new TaskCompletionSource();
                var afterCbB = GetOnAfterHandler(tcsAfterClientResetB, (before, after) =>
                {
                    var list = after.All<SyncObjectWithRequiredStringList>().Single().Strings.ToArray();

                    // We added an object in the tail, that should be merged
                    Assert.That(list, Is.EqualTo(new[] { "0", "1", "3" }));
                });
                configB.ClientResetHandler = new RecoverUnsyncedChangesHandler()
                {
                    OnAfterReset = afterCbB
                };

                using var realmB = await GetRealmAsync(configB, waitForSync: true);
                await WaitForDownloadAsync(realmB).Timeout(10_000, detail: "Wait for download realm B");

                var originalObjStr = realmB.All<SyncObjectWithRequiredStringList>().Single().Strings;
                Assert.That(originalObjStr.ToArray(), Is.EqualTo(new[] { "0", "1", "2" }));

                var sessionB = GetSession(realmB);
                sessionB.Stop();

                realmB.Write(() =>
                {
                    originalObjStr.Add("3");
                });

                await TriggerClientReset(realmA);

                // We want the client reset for A to go through first.
                await TriggerClientReset(realmB, restartSession: false);

                // ===== clientA =====
                await tcsAfterClientResetA.Task.Timeout(10_000, detail: "Client Reset A");

                var tcsAfterRemoteUpdateA = new TaskCompletionSource();

                var stringsA = realmA.All<SyncObjectWithRequiredStringList>().First().Strings;

                Assert.That(stringsA.ToArray(), Is.EquivalentTo(new[] { "0", "1" }));

                using var token = stringsA.SubscribeForNotifications((sender, changes) =>
                {
                    if (sender.Count != 3)
                    {
                        return;
                    }

                    // After clientB merges and uploads the changes,
                    // clientA should receive the updated status
                    Assert.That(sender.ToArray(), Is.EqualTo(new[] { "0", "1", "3" }));

                    tcsAfterRemoteUpdateA.TrySetResult();
                });

                // ===== clientB =====
                sessionB.Start();

                await tcsAfterClientResetB.Task.Timeout(10_000, detail: "Client Reset B");
                await tcsAfterRemoteUpdateA.Task.Timeout(10_000, detail: "After remote update A");
                Assert.That(stringsA.ToArray(), Is.EquivalentTo(new[] { "0", "1", "3" }));
            }, timeout: 120_000);
        }

        private async Task<(SyncConfigurationBase Config, Guid Guid)> GetConfigForApp(string appType)
        {
            var appConfig = SyncTestHelpers.GetAppConfig(appType);
            var app = App.Create(appConfig);
            var user = await GetUserAsync(app);

            var guid = Guid.NewGuid();
            SyncConfigurationBase config;
            if (appType == AppConfigType.FlexibleSync)
            {
                var flxConfig = GetFLXIntegrationConfig(user);
                flxConfig.PopulateInitialSubscriptions = (realm) =>
                {
                    var query = realm.All<ObjectWithPartitionValue>().Where(o => o.Guid == guid);
                    realm.Subscriptions.Add(query);
                };

                config = flxConfig;
            }
            else
            {
                config = GetIntegrationConfig(user);
            }

            config.Schema = new[] { typeof(ObjectWithPartitionValue) };

            return (config, guid);
        }

        [Test]
        public void Session_ClientResetHandlers_AccessRealm_OnBeforeReset(
            [ValueSource(nameof(AppTypes))] string appType,
            [ValueSource(nameof(AllClientResetHandlers))] Type resetHandlerType)
        {
            const string alwaysSynced = "always synced";
            const string maybeSynced = "deleted only on discardLocal";

            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var tcs = new TaskCompletionSource();
                var onBeforeTriggered = false;

                var (config, guid) = await GetConfigForApp(appType);

                var beforeCb = GetOnBeforeHandler(tcs, beforeFrozen =>
                {
                    Assert.That(onBeforeTriggered, Is.False);

                    AssertOnObjectPair(beforeFrozen);
                    onBeforeTriggered = true;
                    tcs.TrySetResult();
                });
                config.ClientResetHandler = GetClientResetHandler(resetHandlerType, beforeCb);

                using var realm = await GetRealmAsync(config, waitForSync: true);

                realm.Write(() =>
                {
                    realm.Add(new ObjectWithPartitionValue(guid)
                    {
                        Value = alwaysSynced,
                    });
                });

                await WaitForUploadAsync(realm).Timeout(15_000, detail: "Wait for upload");
                var session = GetSession(realm);
                session.Stop();

                realm.Write(() =>
                {
                    realm.Add(new ObjectWithPartitionValue(guid)
                    {
                        Value = maybeSynced,
                    });
                });

                AssertOnObjectPair(realm);

                await TriggerClientReset(realm);

                await tcs.Task.Timeout(20_000, detail: "Expected client reset");
                Assert.That(onBeforeTriggered, Is.True);

                var objs = realm.All<ObjectWithPartitionValue>();
                var isDiscardLocal = config.ClientResetHandler.ClientResetMode == ClientResyncMode.Discard;
                var objectsCount = isDiscardLocal ? 1 : 2;

                await TestHelpers.WaitForConditionAsync(() => objs.Count() == objectsCount);

                if (isDiscardLocal)
                {
                    Assert.That(objs.Single().Value, Is.EqualTo(alwaysSynced));
                }
                else
                {
                    AssertOnObjectPair(realm);
                }

                static void AssertOnObjectPair(Realm realm)
                {
                    Assert.That(realm.All<ObjectWithPartitionValue>().ToArray().Select(o => o.Value),
                        Is.EquivalentTo(new[] { alwaysSynced, maybeSynced }));
                }
            }, timeout: 120_000);
        }

        [Test]
        public void Session_ClientResetHandlers_AccessRealms_OnAfterReset(
            [ValueSource(nameof(AppTypes))] string appType,
            [ValueSource(nameof(AllClientResetHandlers))] Type resetHandlerType)
        {
            const string alwaysSynced = "always synced";
            const string maybeSynced = "deleted only on discardLocal";

            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var tcs = new TaskCompletionSource();
                var onAfterTriggered = false;

                var (config, guid) = await GetConfigForApp(appType);

                var afterCb = GetOnAfterHandler(tcs, (beforeFrozen, after) =>
                {
                    Assert.That(onAfterTriggered, Is.False);
                    Assert.That(beforeFrozen.All<ObjectWithPartitionValue>().ToArray().Select(o => o.Value),
                        Is.EquivalentTo(new[] { alwaysSynced, maybeSynced }));
                    onAfterTriggered = true;
                });
                config.ClientResetHandler = GetClientResetHandler(resetHandlerType, afterCb: afterCb);

                using var realm = await GetRealmAsync(config, waitForSync: true);

                realm.Write(() =>
                {
                    realm.Add(new ObjectWithPartitionValue(guid)
                    {
                        Value = alwaysSynced,
                    });
                });

                await WaitForUploadAsync(realm).Timeout(20_000, detail: "Wait for upload");

                var session = GetSession(realm);
                session.Stop();

                realm.Write(() =>
                {
                    realm.Add(new ObjectWithPartitionValue(guid)
                    {
                        Value = maybeSynced,
                    });
                });

                await TriggerClientReset(realm);

                await tcs.Task.Timeout(30_000, detail: "Expected client reset");
                Assert.That(onAfterTriggered, Is.True);

                var expected = config.ClientResetHandler.ClientResetMode == ClientResyncMode.Discard ?
                    new[] { alwaysSynced } : new[] { alwaysSynced, maybeSynced };

                await WaitForConditionAsync(() => realm.All<ObjectWithPartitionValue>().Count() == expected.Length, attempts: 300);

                Assert.That(realm.All<ObjectWithPartitionValue>().ToArray().Select(o => o.Value), Is.EquivalentTo(expected));
            }, timeout: 120_000);
        }

        [Test]
        public void Session_ClientResetDiscard_TriggersNotifications()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                // We'll add an object with the wrong partition
                var config = await GetIntegrationConfigAsync();
                config.Schema = new[] { typeof(ObjectWithPartitionValue) };
                config.ClientResetHandler = new DiscardUnsyncedChangesHandler();

                using var realm = await GetRealmAsync(config, waitForSync: true);

                realm.Write(() =>
                {
                    realm.Add(new ObjectWithPartitionValue(Guid.NewGuid())
                    {
                        Value = "this will sync"
                    });
                });

                await WaitForUploadAsync(realm).Timeout(10_000, detail: "Wait for upload");
                var session = GetSession(realm);
                session.Stop();

                realm.Write(() =>
                {
                    realm.Add(new ObjectWithPartitionValue(Guid.NewGuid())
                    {
                        Value = "this will be merged at client reset"
                    });
                });

                var objects = realm.All<ObjectWithPartitionValue>().AsRealmCollection();
                Assert.That(objects.Count, Is.EqualTo(2));
                var tcs = new TaskCompletionSource<ChangeSet>();
                using var token = objects.SubscribeForNotifications((sender, changes) =>
                {
                    if (changes != null)
                    {
                        tcs.TrySetResult(changes);
                    }
                });

                await TriggerClientReset(realm);

                var args = await tcs.Task.Timeout(15_000, "Wait for notifications");

                Assert.That(args.DeletedIndices.Length, Is.EqualTo(1));
                Assert.That(objects.Count, Is.EqualTo(1));
            }, timeout: 120_000);
        }

        [Test]
        public void Session_ClientResetHandlers_ManualResetFallback_Exception_OnBefore(
            [ValueSource(nameof(AppTypes))] string appType,
            [ValueSource(nameof(AllClientResetHandlers))] Type resetHandlerType)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var tcs = new TaskCompletionSource();
                var onBeforeTriggered = false;
                var manualFallbackTriggered = false;
                var onAfterResetTriggered = false;

                var (config, _) = await GetConfigForApp(appType);

                void beforeCb(Realm beforeFrozen)
                {
                    try
                    {
                        Assert.That(onBeforeTriggered, Is.False);
                        Assert.That(onAfterResetTriggered, Is.False);
                        Assert.That(manualFallbackTriggered, Is.False);
                        onBeforeTriggered = true;
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }

                    throw new Exception("Exception thrown in OnBeforeReset");
                }

                var afterCb = GetOnAfterHandler(tcs, (beforeFrozen, after) =>
                {
                    onAfterResetTriggered = true;
                });

                var manualCb = GetManualResetHandler(tcs, (ex) =>
                {
                    Assert.That(ex, Is.InstanceOf<ClientResetException>());
                    Assert.That(onBeforeTriggered, Is.True);
                    Assert.That(onAfterResetTriggered, Is.False);
                    Assert.That(manualFallbackTriggered, Is.False);
                    manualFallbackTriggered = true;
                });

                config.ClientResetHandler = GetClientResetHandler(resetHandlerType, beforeCb, afterCb, manualCb);

                using var realm = await GetRealmAsync(config, waitForSync: true);

                await TriggerClientReset(realm);

                await tcs.Task;

                Assert.That(manualFallbackTriggered, Is.True);
                Assert.That(onBeforeTriggered, Is.True);
                Assert.That(onAfterResetTriggered, Is.False);
            });
        }

        [Test]
        public void Session_ClientResetHandlers_ManualResetFallback_Exception_OnAfter(
            [ValueSource(nameof(AppTypes))] string appType,
            [ValueSource(nameof(AllClientResetHandlers))] Type resetHandlerType)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var tcs = new TaskCompletionSource();
                var onBeforeTriggered = false;
                var manualFallbackTriggered = false;
                var onAfterResetTriggered = false;

                var (config, _) = await GetConfigForApp(appType);

                var beforeCb = GetOnBeforeHandler(tcs, beforeFrozen =>
                {
                    Assert.That(onBeforeTriggered, Is.False);
                    Assert.That(onAfterResetTriggered, Is.False);
                    Assert.That(manualFallbackTriggered, Is.False);
                    onBeforeTriggered = true;
                });

                void afterCb(Realm beforeFrozen, Realm after)
                {
                    Assert.That(onBeforeTriggered, Is.True);
                    Assert.That(onAfterResetTriggered, Is.False);
                    Assert.That(manualFallbackTriggered, Is.False);
                    onAfterResetTriggered = true;
                    throw new Exception("Exception thrown in OnAfterReset");
                }

                var manualCb = GetManualResetHandler(tcs, (ex) =>
                {
                    Assert.That(ex, Is.InstanceOf<ClientResetException>());
                    Assert.That(onBeforeTriggered, Is.True);
                    Assert.That(onAfterResetTriggered, Is.True);
                    Assert.That(manualFallbackTriggered, Is.False);
                    manualFallbackTriggered = true;
                });

                config.ClientResetHandler = GetClientResetHandler(resetHandlerType, beforeCb, afterCb, manualCb);

                using var realm = await GetRealmAsync(config, waitForSync: true);

                await TriggerClientReset(realm);

                await tcs.Task.Timeout(20_000, detail: "Expect client reset");

                Assert.That(manualFallbackTriggered, Is.True);
                Assert.That(onBeforeTriggered, Is.True);
                Assert.That(onAfterResetTriggered, Is.True);
            }, timeout: 120_000);
        }

        [Test]
        public void Session_OnSessionError()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var sessionErrorTriggered = false;
                var tcs = new TaskCompletionSource<bool>();
                var config = await GetIntegrationConfigAsync();
                var errorMsg = "simulated sync issue";
                config.OnSessionError = (sender, e) =>
                {
                    Assert.That(sender, Is.InstanceOf<Session>());
                    Assert.That(e, Is.InstanceOf<SessionException>());
                    Assert.That(e.ErrorCode, Is.EqualTo(ErrorCode.TooManySessions));
                    Assert.That(e.Message, Is.EqualTo(errorMsg));
                    Assert.That(e.InnerException, Is.Null);
                    Assert.That(sessionErrorTriggered, Is.False);
                    sessionErrorTriggered = true;
                    tcs.TrySetResult(true);
                };

                using var realm = await GetRealmAsync(config, waitForSync: true);
                var session = GetSession(realm);
                session.SimulateError(ErrorCode.TooManySessions, errorMsg);

                await tcs.Task;

                Assert.That(sessionErrorTriggered, Is.True);
            });
        }

        [TestCase(ProgressMode.ForCurrentlyOutstandingWork)]
        [TestCase(ProgressMode.ReportIndefinitely)]
        public void SessionIntegrationTest_ProgressObservable(ProgressMode mode)
        {
            const int ObjectSize = 1_000_000;
            const int ObjectsToRecord = 2;
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var config = await GetIntegrationConfigAsync(Guid.NewGuid().ToString());
                using var realm = GetRealm(config);

                var completionTCS = new TaskCompletionSource<ulong>();
                var callbacksInvoked = 0;

                var session = GetSession(realm);

                var observable = session.GetProgressObservable(ProgressDirection.Upload, mode);

                for (var i = 0; i < ObjectsToRecord; i++)
                {
                    realm.Write(() =>
                    {
                        realm.Add(new HugeSyncObject(ObjectSize));
                    });
                }

                using var token = observable.Subscribe(p =>
                {
                    try
                    {
                        callbacksInvoked++;

                        if (p.TransferredBytes > p.TransferableBytes)
                        {
                            // TODO https://github.com/realm/realm-dotnet/issues/2360: this seems to be a regression in Sync.
                            // throw new Exception($"Expected: {p.TransferredBytes} <= {p.TransferableBytes}");
                        }

                        if (mode == ProgressMode.ForCurrentlyOutstandingWork)
                        {
                            if (p.TransferableBytes <= ObjectSize ||
                                p.TransferableBytes >= (ObjectsToRecord + 2) * ObjectSize)
                            {
                                throw new Exception($"Expected: {p.TransferableBytes} to be in the ({ObjectSize}, {(ObjectsToRecord + 1) * ObjectSize}) range.");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        completionTCS.TrySetException(e);
                    }

                    if (p.TransferredBytes >= p.TransferableBytes)
                    {
                        completionTCS.TrySetResult(p.TransferredBytes);
                    }
                });

                realm.Write(() =>
                {
                    realm.Add(new HugeSyncObject(ObjectSize));
                });

                var totalTransferred = await completionTCS.Task;

                if (mode == ProgressMode.ForCurrentlyOutstandingWork)
                {
                    Assert.That(totalTransferred, Is.GreaterThanOrEqualTo(ObjectSize));

                    // We add ObjectsToRecord + 1 items, but the last item is added after subscribing
                    // so in the fixed mode, we should not get updates for it.
                    Assert.That(totalTransferred, Is.LessThan((ObjectsToRecord + 5) * ObjectSize));
                }
                else
                {
                    Assert.That(totalTransferred, Is.GreaterThanOrEqualTo((ObjectsToRecord + 1) * ObjectSize));
                }

                Assert.That(callbacksInvoked, Is.GreaterThan(1));
            }, timeout: 120_000);
        }

        [Test]
        public void Session_Stop_StopsSession()
        {
            // OpenRealmAndStopSession will call Stop and assert the state changed
            OpenRealmAndStopSession();
        }

        [Test]
        public void Session_Start_ResumesSession()
        {
            var session = OpenRealmAndStopSession();

            session.Start();
            Assert.That(session.State, Is.EqualTo(SessionState.Active));
        }

        [Test]
        public void Session_Stop_IsIdempotent()
        {
            var session = OpenRealmAndStopSession();

            // Stop it again
            session.Stop();
            Assert.That(session.State, Is.EqualTo(SessionState.Inactive));
        }

        [Test]
        public void Session_Start_IsIdempotent()
        {
            var session = OpenRealmAndStopSession();

            session.Start();
            Assert.That(session.State, Is.EqualTo(SessionState.Active));

            // Start it again
            session.Start();
            Assert.That(session.State, Is.EqualTo(SessionState.Active));
        }

        [Test]
        public void Session_ConnectionState_FullFlow()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var config = await GetIntegrationConfigAsync();
                using var realm = GetRealm(config);
                var stateChanged = 0;
                var completionTCS = new TaskCompletionSource();

                var session = realm.SyncSession;
                session.Stop();

                session.PropertyChanged += NotificationChanged;

                session.Start();
                await Task.Delay(1000);
                session.Stop();
                await completionTCS.Task.Timeout(10_000);
                Assert.That(stateChanged, Is.EqualTo(3));
                Assert.That(session.ConnectionState, Is.EqualTo(ConnectionState.Disconnected));
                session.PropertyChanged -= NotificationChanged;

                void NotificationChanged(object sender, PropertyChangedEventArgs e)
                {
                    try
                    {
                        Assert.That(sender is Session, Is.True);
                        Assert.That(e.PropertyName, Is.EqualTo(nameof(Session.ConnectionState)));

                        stateChanged++;

                        if (stateChanged == 1)
                        {
                            Assert.That(session.ConnectionState, Is.EqualTo(ConnectionState.Connecting));
                        }
                        else if (stateChanged == 2)
                        {
                            Assert.That(session.ConnectionState, Is.EqualTo(ConnectionState.Connected));
                        }
                        else if (stateChanged == 3)
                        {
                            Assert.That(session.ConnectionState, Is.EqualTo(ConnectionState.Disconnected));
                            completionTCS.TrySetResult();
                        }
                    }
                    catch (Exception ex)
                    {
                        completionTCS.TrySetException(ex);
                    }
                }
            });
        }

        [Test]
        public void Session_PropertyChanged_FreedNotificationToken()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var config = await GetIntegrationConfigAsync();
                using var realm = GetRealm(config);
                var session = realm.SyncSession;

                var internalNotificationToken = GetNotificationToken(session);
                Assert.That(internalNotificationToken, Is.Null);

                session.PropertyChanged += NotificationChanged;
                internalNotificationToken = GetNotificationToken(session);
                Assert.That(internalNotificationToken, Is.Not.Null);

                session.PropertyChanged -= NotificationChanged;
                internalNotificationToken = GetNotificationToken(session);
                Assert.That(internalNotificationToken, Is.Null);

                // repeated to make sure that re-subscribing is fine
                session.PropertyChanged += NotificationChanged;
                internalNotificationToken = GetNotificationToken(session);
                Assert.That(internalNotificationToken, Is.Not.Null);

                session.PropertyChanged -= NotificationChanged;
                internalNotificationToken = GetNotificationToken(session);
                Assert.That(internalNotificationToken, Is.Null);

                static void NotificationChanged(object sender, PropertyChangedEventArgs e)
                {
                }
            });
        }

        [Test]
        public void Session_PropertyChanged_MultipleSubscribers()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var config = await GetIntegrationConfigAsync();
                using var realm = GetRealm(config);
                var session = realm.SyncSession;
                var subscriberATriggered = false;
                var subscriberBTriggered = false;
                var completionTCSA = new TaskCompletionSource();
                var completionTCSB = new TaskCompletionSource();

                // wait for connecting and connected to be done
                await Task.Delay(500);

                session.PropertyChanged += NotificationChangedA;
                session.PropertyChanged += NotificationChangedB;

                session.Stop();

                await completionTCSA.Task;
                await completionTCSB.Task;
                Assert.That(subscriberATriggered, Is.True);
                Assert.That(subscriberBTriggered, Is.True);

                session.PropertyChanged -= NotificationChangedA;
                session.PropertyChanged -= NotificationChangedB;

                void NotificationChangedA(object sender, PropertyChangedEventArgs e)
                {
                    Assert.That(subscriberATriggered, Is.False);
                    subscriberATriggered = true;
                    completionTCSA.TrySetResult();
                }

                void NotificationChangedB(object sender, PropertyChangedEventArgs e)
                {
                    Assert.That(subscriberBTriggered, Is.False);
                    subscriberBTriggered = true;
                    completionTCSB.TrySetResult();
                }
            });
        }

        [Test]
        public void Session_Free_Instance_Even_With_PropertyChanged_Subscribers()
        {
            WeakReference weakSessionRef = null;

            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var config = await GetIntegrationConfigAsync();
                using var realm = GetRealm(config);
                var session = realm.SyncSession;
                weakSessionRef = new WeakReference(session);
                Assert.That(weakSessionRef.IsAlive, Is.True);
                session.PropertyChanged += (sender, e) => { };
            });

            GC.Collect();
            Assert.That(weakSessionRef.IsAlive, Is.False);
        }

        [Test]
        public void Session_NotificationToken_Freed_When_Close_Realm()
        {
            WeakReference weakNotificationTokenRef = null;

            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var config = await GetIntegrationConfigAsync();
                var realm = GetRealm(config);
                var session = realm.SyncSession;

                session.PropertyChanged += NotificationChanged;
                var internalNotificationToken = GetNotificationToken(session);
                Assert.That(internalNotificationToken, Is.Not.Null);
                weakNotificationTokenRef = new WeakReference(internalNotificationToken);
                Assert.That(weakNotificationTokenRef.IsAlive, Is.True);

                static void NotificationChanged(object sender, PropertyChangedEventArgs e)
                {
                }
            });

            GC.Collect();
            Assert.That(weakNotificationTokenRef.IsAlive, Is.False);
        }

        [Test]
        public void Session_ConnectionState_Propageted_Within_Multiple_Sessions()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var config = await GetIntegrationConfigAsync();
                using var realmA = GetRealm(config);
                using var realmB = GetRealm(config);
                var stateChanged = 0;
                var completionTCS = new TaskCompletionSource();

                var sessionA = realmA.SyncSession;
                var sessionB = realmB.SyncSession;
                sessionA.Stop();
                sessionB.PropertyChanged += NotificationChanged;

                sessionA.Start();
                await Task.Delay(1000);
                sessionA.Stop();
                await completionTCS.Task;

                Assert.That(stateChanged, Is.EqualTo(3));
                Assert.That(sessionB.ConnectionState, Is.EqualTo(ConnectionState.Disconnected));
                Assert.That(sessionA.ConnectionState, Is.EqualTo(ConnectionState.Disconnected));

                sessionB.PropertyChanged -= NotificationChanged;

                void NotificationChanged(object sender, PropertyChangedEventArgs e)
                {
                    try
                    {
                        Assert.That(sender is Session, Is.True);
                        Assert.That(e.PropertyName, Is.EqualTo(nameof(Session.ConnectionState)));

                        stateChanged++;

                        if (stateChanged == 1)
                        {
                            Assert.That(sessionA.ConnectionState, Is.EqualTo(ConnectionState.Connecting));
                            Assert.That(sessionB.ConnectionState, Is.EqualTo(ConnectionState.Connecting));
                        }
                        else if (stateChanged == 2)
                        {
                            Assert.That(sessionA.ConnectionState, Is.EqualTo(ConnectionState.Connected));
                            Assert.That(sessionB.ConnectionState, Is.EqualTo(ConnectionState.Connected));
                        }
                        else if (stateChanged == 3)
                        {
                            Assert.That(sessionA.ConnectionState, Is.EqualTo(ConnectionState.Disconnected));
                            Assert.That(sessionB.ConnectionState, Is.EqualTo(ConnectionState.Disconnected));
                            completionTCS.TrySetResult();
                        }
                    }
                    catch (Exception ex)
                    {
                        completionTCS.TrySetException(ex);
                    }
                }
            });
        }

        [Test]
        public void Session_WhenDisposed_MethodsThrow()
        {
            var session = OpenRealmAndStopSession();

            session.CloseHandle();

            Assert.Throws<ObjectDisposedException>(() => session.Start());
            Assert.Throws<ObjectDisposedException>(() => session.Stop());
            Assert.Throws<ObjectDisposedException>(() => _ = session.State);
            Assert.Throws<ObjectDisposedException>(() => _ = session.User);
            Assert.Throws<ObjectDisposedException>(() => _ = session.Path);
            Assert.Throws<ObjectDisposedException>(() => _ = session.GetHashCode());
            Assert.Throws<ObjectDisposedException>(() => _ = session.GetProgressObservable(ProgressDirection.Upload, ProgressMode.ForCurrentlyOutstandingWork));
            Assert.Throws<ObjectDisposedException>(() => _ = session.Equals(session));
            Assert.Throws<ObjectDisposedException>(() => _ = session.WaitForDownloadAsync());
            Assert.Throws<ObjectDisposedException>(() => _ = session.WaitForUploadAsync());
            Assert.Throws<ObjectDisposedException>(() => session.ReportErrorForTesting(1, SessionErrorCategory.SessionError, "test", false, ServerRequestsAction.ApplicationBug));

            // Calling CloseHandle multiple times should be fine
            session.CloseHandle();
            session.CloseHandle(waitForShutdown: true);
        }

        [Test]
        public void Session_Equals_WhenSameRealm_ReturnsTrue()
        {
            var config = GetFakeConfig();
            var realm = GetRealm(config);
            var first = GetSession(realm);
            var second = GetSession(realm);

            Assert.That(ReferenceEquals(first, second));
            Assert.That(first.Equals(second));
            Assert.That(second.Equals(first));
        }

        [Test]
        public void Session_GetHashCode_WhenSameRealm_ReturnsSameValue()
        {
            var config = GetFakeConfig();
            var realm = GetRealm(config);
            var first = GetSession(realm);
            var second = GetSession(realm);

            Assert.That(first.GetHashCode(), Is.EqualTo(second.GetHashCode()));
        }

        [Test]
        public void Session_Equals_WhenDifferentRealm_ReturnsFalse()
        {
            var realm1 = GetRealm(GetFakeConfig());
            var first = GetSession(realm1);

            var realm2 = GetRealm(GetFakeConfig());
            var second = GetSession(realm2);

            Assert.That(ReferenceEquals(first, second), Is.False);
            Assert.That(first.Equals(second), Is.False);
            Assert.That(second.Equals(first), Is.False);
        }

        [Test]
        public void Session_GetHashCode_WhenDifferentRealm_ReturnsDiffernetValue()
        {
            var realm1 = GetRealm(GetFakeConfig());
            var first = GetSession(realm1);

            var realm2 = GetRealm(GetFakeConfig());
            var second = GetSession(realm2);

            Assert.That(first.GetHashCode(), Is.Not.EqualTo(second.GetHashCode()));
        }

        [Test]
        public void Session_Equals_WhenOtherIsNotASession_ReturnsFalse()
        {
            var session = OpenRealmAndStopSession();

            Assert.That(session.Equals(1), Is.False);
            Assert.That(session.Equals(new object()), Is.False);
        }

        private static ClientResetHandlerBase GetClientResetHandler(
            Type type,
            BeforeResetCallback beforeCb = null,
            AfterResetCallback afterCb = null,
            ClientResetCallback manualCb = null)
        {
            var handler = (ClientResetHandlerBase)Activator.CreateInstance(type);

            if (beforeCb != null)
            {
                type.GetProperty(nameof(DiscardUnsyncedChangesHandler.OnBeforeReset)).SetValue(handler, beforeCb);
            }

            if (afterCb != null)
            {
                var prop = type.GetProperty(nameof(DiscardUnsyncedChangesHandler.OnAfterReset))
                    ?? type.GetProperty(nameof(RecoverOrDiscardUnsyncedChangesHandler.OnAfterRecovery));

                prop.SetValue(handler, afterCb);
            }

            if (manualCb != null)
            {
                type.GetProperty(nameof(DiscardUnsyncedChangesHandler.ManualResetFallback)).SetValue(handler, manualCb);
            }

            return handler;
        }

        private static async Task TryInitiateClientReset(Realm realm, ClientResetException ex, int expectedError)
        {
            if (!realm.IsClosed)
            {
                realm.Dispose();
            }

            Assert.That((int)ex.ErrorCode, Is.EqualTo(expectedError));
            Assert.That(File.Exists(realm.Config.DatabasePath), Is.True);

            var didReset = false;
            for (var i = 0; i < 100 && !didReset; i++)
            {
                await Task.Delay(50);
                didReset = ex.InitiateClientReset();
            }

            Assert.That(didReset, Is.True, "Failed to complete manual reset after 100 attempts.");

            Assert.That(File.Exists(realm.Config.DatabasePath), Is.False);
        }

        private static AfterResetCallback GetOnAfterHandler(TaskCompletionSource tcs, Action<Realm, Realm> assertions)
        {
            return new AfterResetCallback((frozen, live) =>
            {
                try
                {
                    assertions(frozen, live);
                    tcs.TrySetResult();
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });
        }

        private static BeforeResetCallback GetOnBeforeHandler(TaskCompletionSource tcs, Action<Realm> assertions)
        {
            return new BeforeResetCallback(frozen =>
            {
                try
                {
                    assertions(frozen);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });
        }

        private static ClientResetCallback GetManualResetHandler(TaskCompletionSource tcs, Action<ClientResetException> assertions)
        {
            return new ClientResetCallback(clientResetException =>
            {
                try
                {
                    assertions(clientResetException);
                    tcs.TrySetResult();
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });
        }

        /// <summary>
        /// Opens a random realm and calls session.Stop(). It will assert state changes
        /// to Inactive.
        /// </summary>
        /// <returns>The stopped session.</returns>
        private Session OpenRealmAndStopSession()
        {
            var config = GetFakeConfig();
            var realm = GetRealm(config);
            var session = GetSession(realm);

            Assert.That(session.State, Is.EqualTo(SessionState.Active));

            session.Stop();
            Assert.That(session.State, Is.EqualTo(SessionState.Inactive));

            return session;
        }

        private static SessionNotificationToken? GetNotificationToken(Session session)
        {
            var sessionHandle = (SessionHandle)typeof(Session).GetField("_handle", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(session);
            return sessionHandle != null ?
                (SessionNotificationToken?)typeof(SessionHandle).GetField("_notificationToken", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sessionHandle) :
                null;
        }
    }

    [Explicit]
    public partial class ObjectWithPartitionValue : TestRealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public string Id { get; private set; } = Guid.NewGuid().ToString();

        public string Value { get; set; }

        [MapTo("realm_id")]
        public string Partition { get; set; }

        public Guid Guid { get; private set; }

        public ObjectWithPartitionValue(Guid guid)
        {
            Guid = guid;
        }

#if TEST_WEAVER
        private ObjectWithPartitionValue()
        {
        }
#endif
    }

    public partial class SyncObjectWithRequiredStringList : TestRealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public string Id { get; set; }

        [Required]
        public IList<string> Strings { get; }
    }
}
