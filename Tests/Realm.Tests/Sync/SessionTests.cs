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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baas;
using NUnit.Framework;
using Realms.Exceptions.Sync;
using Realms.Logging;
using Realms.Sync;
using Realms.Sync.ErrorHandling;
using Realms.Sync.Exceptions;
using Realms.Sync.Native;
using Realms.Sync.Testing;
using static Realms.Sync.ErrorHandling.ClientResetHandlerBase;
#if TEST_WEAVER
using TestAsymmetricObject = Realms.AsymmetricObject;
using TestEmbeddedObject = Realms.EmbeddedObject;
using TestRealmObject = Realms.RealmObject;
#else
using TestAsymmetricObject = Realms.IAsymmetricObject;
using TestEmbeddedObject = Realms.IEmbeddedObject;
using TestRealmObject = Realms.IRealmObject;
#endif

namespace Realms.Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class SessionTests : SyncTestBase
    {
        private readonly ConcurrentQueue<EventHandler<ErrorEventArgs>> _sessionErrorHandlers = new();

#pragma warning disable CS0618 // Type or member is obsolete

        public static readonly object[] AllClientResetHandlers = new object[]
        {
            typeof(DiscardUnsyncedChangesHandler),
            typeof(RecoverUnsyncedChangesHandler),
            typeof(RecoverOrDiscardUnsyncedChangesHandler),

            // Just to check that we don't break previous code. Remove in next major version
            typeof(DiscardLocalResetHandler),
        };

        // Just to check that we don't break previous code. Remove in next major version
        public static readonly object[] ObosoleteHandlerCoexistence = new object[]
        {
            typeof(DiscardUnsyncedChangesHandler),
            typeof(DiscardLocalResetHandler),
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

            // Just to check that we don't break previous code. Remove in next major version
            var preserveObsoleteDiscardHandler = new DiscardLocalResetHandler
            {
                OnBeforeReset = (beforeFrozen) => { },
                OnAfterReset = (beforeFrozen, after) => { },
                ManualResetFallback = (clientResetException) => { },
            };

#pragma warning restore CS0618 // Type or member is obsolete

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

        [Test, Obsolete("tests obsolete functionality")]
        public void Realm_GetSession_WhenSyncedRealm()
        {
            var config = GetFakeConfig();

            using var realm = GetRealm(config);
            var session = realm.GetSession();
            CleanupOnTearDown(session);

            Assert.That(session.User, Is.EqualTo(config.User));
        }

        [Test, Obsolete("tests obsolete functionality")]
        public void Realm_GetSession_WhenLocalRealm_ShouldThrow()
        {
            using var realm = GetRealm();
            Assert.Throws<ArgumentException>(() => realm.GetSession());
        }

        [Test]
        public void Realm_SyncSession_WhenLocalRealm_ShouldReturnNull()
        {
            using var realm = GetRealm();
            Assert.That(realm.SyncSession, Is.Null);
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

        [Test, Obsolete("Tests Session.Error")]
        public void Session_Error_ShouldPassCorrectSession()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                Session session = null;
                var tcs = new TaskCompletionSource<object>();

                var handler = GetErrorEventHandler(tcs, (sender, error) =>
                {
                    if (session == null)
                    {
                        return false;
                    }

                    // We're not validating the actual exception because there's no guarantee
                    // that we're seeing the same one we raised in SimulateError. It's possible
                    // that sync tried to establish a connection and failed because this is a
                    // unit test, not an integration one, so the connection should be rejected
                    // immediately.
                    Assert.That(sender, Is.EqualTo(session));
                    return true;
                });

                Session.Error += handler;

                var config = GetFakeConfig();
                using var realm = GetRealm(config);
                session = GetSession(realm);

                const ErrorCode code = (ErrorCode)102;
                const string message = "Some fake error has occurred";

                session.SimulateError(code, message);

                await tcs.Task;
            });
        }

        [TestCaseSource(nameof(AppTypes))]
        public void Session_ClientReset_ManualRecovery_InitiateClientReset(string appType)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var manualOnClientResetTriggered = false;
                var errorTcs = new TaskCompletionSource<ClientResetException>();
                SyncConfigurationBase config = appType == AppConfigType.FlexibleSync ? await GetFLXIntegrationConfigAsync() : await GetIntegrationConfigAsync();
                config.ClientResetHandler = new ManualRecoveryHandler((e) =>
                {
                    manualOnClientResetTriggered = true;
                    errorTcs.TrySetResult(e);
                });

                using var realm = await GetRealmAsync(config, waitForSync: true);

                await TriggerClientReset(realm);

                var clientEx = await errorTcs.Task;

                Assert.That(manualOnClientResetTriggered, Is.True);

                Assert.That(clientEx.Message, Does.Contain("Bad client file identifier"));
                Assert.That(clientEx.InnerException, Is.Null);

                await TryInitiateClientReset(realm, clientEx, (int)ErrorCode.BadClientFileIdentifier);
            });
        }

        [Test, Obsolete("Also tests Session.Error")]
        public void Session_ClientResetHandlers_ManualResetFallback_InitiateClientReset(
            [ValueSource(nameof(AppTypes))] string appType,
            [ValueSource(nameof(AllClientResetHandlers))] Type resetHandlerType)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var manualResetFallbackHandled = false;
                var errorTcs = new TaskCompletionSource<ClientResetException>();

                var config = await GetConfigForApp(appType).Timeout(10_000, "Get config");

                void manualCb(ClientResetException err)
                {
                    manualResetFallbackHandled = true;
                    errorTcs.TrySetResult(err);
                }

                void beforeCb(Realm _)
                {
                    throw new Exception("This fails!");
                }

                config.ClientResetHandler = GetClientResetHandler(resetHandlerType, beforeCb: beforeCb, manualCb: manualCb);

                using var realm = await GetRealmAsync(config, waitForSync: true).Timeout(20_000, "Open Realm");

                // This should be removed when we remove Session.Error
                var obsoleteSessionErrorTriggered = false;

                // priority is given to the newer approach in SyncConfigurationBase, so this should never be reached
                Session.Error += OnSessionError;

                await TriggerClientReset(realm);

                var clientEx = await errorTcs.Task.Timeout(20_000, "Expected client reset");

                Assert.That(manualResetFallbackHandled, Is.True);

                await TryInitiateClientReset(realm, clientEx, (int)ClientError.AutoClientResetFailed);

                Assert.That(obsoleteSessionErrorTriggered, Is.False);

                void OnSessionError(object sender, ErrorEventArgs error)
                {
                    obsoleteSessionErrorTriggered = true;
                }
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
                var tcs = new TaskCompletionSource<object>();

                var config = await GetConfigForApp(appType);

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
                using var realm = await GetRealmAsync(config, waitForSync: true);

                await TriggerClientReset(realm);

                await tcs.Task;

                Assert.That(onBeforeTriggered, Is.True);
                Assert.That(onAfterTriggered, Is.True);
            });
        }

        [TestCaseSource(nameof(AppTypes))]
        public void Session_AutomaticRecoveryFallsbackToDiscardLocal(string appType)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var automaticResetCalled = false;
                var discardLocalResetCalled = false;

                SyncConfigurationBase config = appType == AppConfigType.FlexibleSync ? await GetFLXIntegrationConfigAsync() : await GetIntegrationConfigAsync();
                var flxSyncPartition = Guid.NewGuid();

                if (config is FlexibleSyncConfiguration flxConf)
                {
                    flxConf.PopulateInitialSubscriptions = (realm) =>
                    {
                        var query = realm.All<ObjectWithPartitionValue>().Where(p => p.Guid == flxSyncPartition);
                        realm.Subscriptions.Add(query);
                    };
                }

                var tcsAfterClientReset = new TaskCompletionSource<object>();

                config.Schema = new[] { typeof(ObjectWithPartitionValue) };
                var afterAutomaticResetCb = GetOnAfterHandler(tcsAfterClientReset, (before, after) =>
                {
                    Assert.That(automaticResetCalled, Is.False);
                    Assert.That(discardLocalResetCalled, Is.False);
                    automaticResetCalled = true;
                });
                var afterDiscardLocalResetCb = GetOnAfterHandler(tcsAfterClientReset, (before, after) =>
                {
                    Assert.That(automaticResetCalled, Is.False);
                    Assert.That(discardLocalResetCalled, Is.False);
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
                    realm.Add(new ObjectWithPartitionValue
                    {
                        Id = Guid.NewGuid().ToString(),
                        Guid = flxSyncPartition
                    });
                });

                await DisableClientResetRecoveryOnServer(appType);
                await TriggerClientReset(realm);

                await tcsAfterClientReset.Task;
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
        [Test]
        public void SessionIntegrationTest_ClientResetHandlers_OutOfBoundArrayInsert_AddedToTail()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var partition = Guid.NewGuid().ToString();

                // ===== clientA =====
                var tcsAfterClientResetA = new TaskCompletionSource<object>();
                var configA = await GetIntegrationConfigAsync(partition).Timeout(10_000, "Get config");
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
                using var realmA = await GetRealmAsync(configA, waitForSync: true).Timeout(10_000, "Open Realm A");

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
                var tcsAfterClientResetB = new TaskCompletionSource<object>();
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

                using var realmB = await GetRealmAsync(configB, waitForSync: true).Timeout(10_000, "Open Realm B");
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
                await tcsAfterClientResetA.Task.Timeout(10_000, "Client Reset A");

                var tcsAfterRemoteUpdateA = new TaskCompletionSource<object>();

                var stringsA = realmA.All<SyncObjectWithRequiredStringList>().First().Strings;

                Assert.That(stringsA.ToArray(), Is.EquivalentTo(new[] { "0", "1" }));

                using var token = stringsA.SubscribeForNotifications((sender, changes, error) =>
                {
                    if (sender.Count != 3)
                    {
                        return;
                    }

                    // After clientB merges and uploads the changes,
                    // clientA should receive the updated status
                    Assert.That(sender.ToArray(), Is.EqualTo(new[] { "0", "1", "3" }));

                    tcsAfterRemoteUpdateA.TrySetResult(null);
                });

                // ===== clientB =====
                sessionB.Start();

                await tcsAfterClientResetB.Task.Timeout(10_000, "Client Reset B");
                await tcsAfterRemoteUpdateA.Task.Timeout(10_000, "After remote update A");
                Assert.That(stringsA.ToArray(), Is.EquivalentTo(new[] { "0", "1", "3" }));
            }, timeout: 120_000);
        }

        private async Task<SyncConfigurationBase> GetConfigForApp(string appType)
        {
            var appConfig = SyncTestHelpers.GetAppConfig(appType);
            var app = App.Create(appConfig);
            var user = await GetUserAsync(app);

            SyncConfigurationBase config;
            if (appType == AppConfigType.FlexibleSync)
            {
                config = GetFLXIntegrationConfig(user);
            }
            else
            {
                config = GetIntegrationConfig(user);
            }

            return config;
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
                var tcs = new TaskCompletionSource<object>();
                var onBeforeTriggered = false;
                var guid = Guid.NewGuid();

                var config = await GetConfigForApp(appType);
                config.Schema = new[] { typeof(ObjectWithPartitionValue) };
                if (config is FlexibleSyncConfiguration flxConfig)
                {
                    flxConfig.PopulateInitialSubscriptions = (realm) =>
                    {
                        var query = realm.All<ObjectWithPartitionValue>().Where(o => o.Guid == guid);
                        realm.Subscriptions.Add(query);
                    };
                }

                var beforeCb = GetOnBeforeHandler(tcs, beforeFrozen =>
                {
                    Assert.That(onBeforeTriggered, Is.False);

                    AssertOnObjectPair(beforeFrozen);
                    onBeforeTriggered = true;
                    tcs.SetResult(null);
                });
                config.ClientResetHandler = GetClientResetHandler(resetHandlerType, beforeCb);

                using var realm = await GetRealmAsync(config, waitForSync: true);

                realm.Write(() =>
                {
                    realm.Add(new ObjectWithPartitionValue
                    {
                        Id = Guid.NewGuid().ToString(),
                        Value = alwaysSynced,
                        Guid = guid
                    });
                });

                await WaitForUploadAsync(realm);
                var session = GetSession(realm);
                session.Stop();

                realm.Write(() =>
                {
                    realm.Add(new ObjectWithPartitionValue
                    {
                        Id = Guid.NewGuid().ToString(),
                        Value = maybeSynced,
                        Guid = guid
                    });
                });

                AssertOnObjectPair(realm);

                await TriggerClientReset(realm);

                await tcs.Task;
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

                void AssertOnObjectPair(Realm realm)
                {
                    Assert.That(realm.All<ObjectWithPartitionValue>().ToArray().Select(o => o.Value),
                        Is.EquivalentTo(new[] { alwaysSynced, maybeSynced }));
                }
            });
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
                var tcs = new TaskCompletionSource<object>();
                var onAfterTriggered = false;
                var guid = Guid.NewGuid();

                var config = await GetConfigForApp(appType);

                if (config is FlexibleSyncConfiguration flxConf)
                {
                    flxConf.PopulateInitialSubscriptions = (realm) =>
                    {
                        var query = realm.All<ObjectWithPartitionValue>().Where(o => o.Guid == guid);
                        realm.Subscriptions.Add(query);
                    };
                }

                var afterCb = GetOnAfterHandler(tcs, (beforeFrozen, after) =>
                {
                    Assert.That(onAfterTriggered, Is.False);
                    Assert.That(beforeFrozen.All<ObjectWithPartitionValue>().ToArray().Select(o => o.Value),
                        Is.EquivalentTo(new[] { alwaysSynced, maybeSynced }));
                    onAfterTriggered = true;
                });
                config.ClientResetHandler = GetClientResetHandler(resetHandlerType, afterCb: afterCb);
                config.Schema = new[] { typeof(ObjectWithPartitionValue) };

                using var realm = await GetRealmAsync(config, waitForSync: true);

                realm.Write(() =>
                {
                    realm.Add(new ObjectWithPartitionValue
                    {
                        Id = Guid.NewGuid().ToString(),
                        Value = alwaysSynced,
                        Guid = guid
                    });
                });

                await WaitForUploadAsync(realm);

                var session = GetSession(realm);
                session.Stop();

                realm.Write(() =>
                {
                    realm.Add(new ObjectWithPartitionValue
                    {
                        Id = Guid.NewGuid().ToString(),
                        Value = maybeSynced,
                        Guid = guid
                    });
                });

                await TriggerClientReset(realm);

                await tcs.Task;
                Assert.That(onAfterTriggered, Is.True);

                var expected = config.ClientResetHandler.ClientResetMode == ClientResyncMode.Discard ?
                    new[] { alwaysSynced } : new[] { alwaysSynced, maybeSynced };

                await TestHelpers.WaitForConditionAsync(() => realm.All<ObjectWithPartitionValue>().Count() == expected.Length, attempts: 300);

                Assert.That(realm.All<ObjectWithPartitionValue>().ToArray().Select(o => o.Value), Is.EquivalentTo(expected));
            });
        }

        [TestCaseSource(nameof(ObosoleteHandlerCoexistence))]
        public void Session_ClientResetDiscard_TriggersNotifications(Type handlerType)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                // We'll add an object with the wrong partition
                var config = await GetIntegrationConfigAsync();
                config.Schema = new[] { typeof(ObjectWithPartitionValue) };
                config.ClientResetHandler = (ClientResetHandlerBase)Activator.CreateInstance(handlerType);

                using var realm = await GetRealmAsync(config, waitForSync: true).Timeout(10_000, "GetInstanceAsync");

                realm.Write(() =>
                {
                    realm.Add(new ObjectWithPartitionValue
                    {
                        Id = Guid.NewGuid().ToString(),
                        Value = "this will sync"
                    });
                });

                await WaitForUploadAsync(realm).Timeout(10_000, detail: "Wait for upload");
                var session = GetSession(realm);
                session.Stop();

                realm.Write(() =>
                {
                    realm.Add(new ObjectWithPartitionValue
                    {
                        Id = Guid.NewGuid().ToString(),
                        Value = "this will be merged at client reset"
                    });
                });

                var objects = realm.All<ObjectWithPartitionValue>().AsRealmCollection();
                Assert.That(objects.Count, Is.EqualTo(2));
                var tcs = new TaskCompletionSource<NotifyCollectionChangedEventArgs>();
                objects.CollectionChanged += onCollectionChanged;

                await TriggerClientReset(realm);

                var args = await tcs.Task.Timeout(15_000, "Wait for notifications");

                Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));
                Assert.That(objects.Count, Is.EqualTo(1));

                objects.CollectionChanged -= onCollectionChanged;

                void onCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
                {
                    tcs.TrySetResult(args);
                }
            }, timeout: 120_000);
        }

        [Test]
        public void Session_ClientResetHandlers_ManualResetFallback_Exception_OnBefore(
            [ValueSource(nameof(AppTypes))] string appType,
            [ValueSource(nameof(AllClientResetHandlers))] Type resetHandlerType)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var tcs = new TaskCompletionSource<object>();
                var onBeforeTriggered = false;
                var manualFallbackTriggered = false;
                var onAfterResetTriggered = false;

                var config = await GetConfigForApp(appType);

                BeforeResetCallback beforeCb = beforeFrozen =>
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
                };

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
                var tcs = new TaskCompletionSource<object>();
                var onBeforeTriggered = false;
                var manualFallbackTriggered = false;
                var onAfterResetTriggered = false;

                var config = await GetConfigForApp(appType).Timeout(10_000, "Get config");

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

                using var realm = await GetRealmAsync(config, waitForSync: true).Timeout(10_000, "Open Realm");

                await TriggerClientReset(realm);

                await tcs.Task.Timeout(20_000, "Expect client reset");

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
                    Assert.That(e.ErrorCode, Is.EqualTo(ErrorCode.NoSuchRealm));
                    Assert.That(e.Message, Is.EqualTo(errorMsg));
                    Assert.That(e.InnerException, Is.Null);
                    Assert.That(sessionErrorTriggered, Is.False);
                    sessionErrorTriggered = true;
                    tcs.TrySetResult(true);
                };

                using var realm = await GetRealmAsync(config, waitForSync: true);
                var session = GetSession(realm);
                session.SimulateError(ErrorCode.NoSuchRealm, errorMsg);

                await tcs.Task;

                Assert.That(sessionErrorTriggered, Is.True);
            });
        }

        [Test, Obsolete("Testing Session.Error compatibility")]
        public void Session_ClientResetHandlers_Coexistence(
            [ValueSource(nameof(AppTypes))] string appType,
            [ValueSource(nameof(AllClientResetHandlers))] Type resetHandlerType)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var onBeforeTriggered = false;
                var onAfterTriggered = false;
                var tcs = new TaskCompletionSource<object>();

                var config = await GetConfigForApp(appType);

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

                var handler = new EventHandler<ErrorEventArgs>((_, error) =>
                {
                    if (error.Exception is ClientResetException crex)
                    {
                        tcs.TrySetException(new Exception("Error handler should not have been called", crex));
                    }
                });

                CleanupOnTearDown(handler);

                // priority is given to the newer appoach in SyncConfigurationBase, so this should never be reached
                Session.Error += handler;

                using var realm = await GetRealmAsync(config, waitForSync: true);

                await TriggerClientReset(realm);

                // to avoid a race condition where e.g. both methods are called but because of timing differences `tcs.TrySetResult(true);` is reached
                // earlier in a call not letting the other finish to run. This would hide an issue.
                await tcs.Task.Timeout(10_000, "Client reset expected");
                await Task.Delay(1000);

                Assert.That(onBeforeTriggered, Is.True);
                Assert.That(onAfterTriggered, Is.True);
            });
        }

        [Test, Obsolete("Testing Sesion.Error compatibility")]
        public void Session_WithNewClientResetHandlers_DoesntRaiseSessionError(
            [ValueSource(nameof(AppTypes))] string appType,
            [ValueSource(nameof(AllClientResetHandlers))] Type resetHandlerType)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var obsoleteSessionErrorTriggered = false;

                var config = await GetConfigForApp(appType);

                config.ClientResetHandler = GetClientResetHandler(resetHandlerType);
                using var realm = await GetRealmAsync(config, waitForSync: true);
                var session = GetSession(realm);

                // priority is given to the newer appoach in SyncConfigurationBase, so this should never be reached
                Session.Error += onSessionError;
                CleanupOnTearDown(onSessionError);

                await TriggerClientReset(realm);

                // Give it some time to propagate the reset
                await Task.Delay(1000);

                Assert.That(obsoleteSessionErrorTriggered, Is.False);

                void onSessionError(object sender, ErrorEventArgs e)
                {
                    obsoleteSessionErrorTriggered = true;
                }
            });
        }

        [Test, Obsolete("Testing Sesion.Error compatibility")]
        public void Session_ClientReset_OldSessionError_InitiateClientReset_Coexistence()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var obsoleteSessionErrorTriggered = false;
                var tcs = new TaskCompletionSource<Exception>();
                var config = await GetIntegrationConfigAsync();
                config.ClientResetHandler = new ManualRecoveryHandler();

                using var realm = await GetRealmAsync(config, waitForSync: true);

                // Session.Error is set after obtaining a realm as it truly tests coexistence given that
                // the resync mode is set at creation of the configuration.
                // SyncConfigurationBase.CreateNativeSyncConfiguration.
                Session.Error += onSessionError;
                CleanupOnTearDown(onSessionError);

                await TriggerClientReset(realm);

                var ex = await tcs.Task;

                Assert.That(obsoleteSessionErrorTriggered, Is.True);

                Assert.That(ex, Is.InstanceOf<ClientResetException>());
                var clientEx = (ClientResetException)ex;
                Assert.That(clientEx.Message, Does.Contain("Bad client file identifier"));
                Assert.That(clientEx.InnerException, Is.Null);

                await TryInitiateClientReset(realm, clientEx, (int)ErrorCode.BadClientFileIdentifier);

                void onSessionError(object sender, ErrorEventArgs e)
                {
                    Assert.That(obsoleteSessionErrorTriggered, Is.False);
                    obsoleteSessionErrorTriggered = true;
                    tcs.TrySetResult(e.Exception);
                }
            });
        }

        [Test, Obsolete("Testing Sesion.Error compatibility")]
        public void Session_Error_OldSessionError_Coexistence()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var obsoleteSessionErrorTriggered = false;
                var tcs = new TaskCompletionSource<object>();
                var config = await GetIntegrationConfigAsync();
                config.ClientResetHandler = new ManualRecoveryHandler();
                var errorMsg = "simulated sync issue";

                var handler = GetErrorEventHandler(tcs, (session, error) =>
                {
                    Assert.That(error.ErrorCode == ErrorCode.NoSuchRealm);
                    Assert.That(error.Message == errorMsg);
                    Assert.That(error.InnerException == null);
                    Assert.That(obsoleteSessionErrorTriggered, Is.False);
                    obsoleteSessionErrorTriggered = true;
                });

                Session.Error += handler;

                using var realm = await GetRealmAsync(config, waitForSync: true);

                var session = GetSession(realm);
                session.SimulateError(ErrorCode.NoSuchRealm, "simulated sync issue");

                await tcs.Task;
                Assert.That(obsoleteSessionErrorTriggered, Is.True);
            });
        }

        [Test, Obsolete("Testing Sesion.Error compatibility")]
        public void Session_ClientReset_ManualRecovery_Coexistence()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var manualOnClientResetTriggered = false;
                var obsoleteSessionErrorTriggered = false;
                var tcs = new TaskCompletionSource<object>();
                var config = await GetIntegrationConfigAsync();
                config.ClientResetHandler = new ManualRecoveryHandler((e) =>
                {
                    Assert.That(manualOnClientResetTriggered, Is.False);
                    manualOnClientResetTriggered = true;
                    tcs.TrySetResult(true);
                });

                using var realm = await GetRealmAsync(config, waitForSync: true);

                var session = GetSession(realm);

                var handler = new EventHandler<ErrorEventArgs>((session, error) =>
                {
                    if (error.Exception is ClientResetException crex)
                    {
                        tcs.TrySetException(new Exception("Error handler should not have been called", crex));
                    }
                });

                CleanupOnTearDown(handler);

                // priority is given to the newer appoach in SyncConfigurationBase, so this should never be reached
                Session.Error += handler;

                await TriggerClientReset(realm);

                // to avoid a race condition where e.g. both methods are called but because of timing differences `tcs.TrySetResult(true);` is reached
                // earlier in a call not letting the other finish to run. This would hide an issue.
                await tcs.Task;
                await Task.Delay(1000);

                Assert.That(manualOnClientResetTriggered, Is.True);
                Assert.That(obsoleteSessionErrorTriggered, Is.False);
            });
        }

        [Test, Obsolete("Testing Sesion.Error compatibility")]
        public void Session_Error_OnSessionError_Coexistence()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var obsoleteSessionErrorTriggered = false;
                var sessionErrorTriggered = false;
                var tcs = new TaskCompletionSource<object>();
                var config = await GetIntegrationConfigAsync();
                config.OnSessionError = (sender, e) =>
                {
                    Assert.That(sender, Is.InstanceOf<Session>());
                    Assert.That(e, Is.InstanceOf<SessionException>());
                    Assert.That(sessionErrorTriggered, Is.False);
                    sessionErrorTriggered = true;
                    tcs.TrySetResult(true);
                };

                using var realm = await GetRealmAsync(config, waitForSync: true);
                var session = GetSession(realm);

                var handler = GetErrorEventHandler(tcs, (session, error) =>
                {
                    Assert.That(obsoleteSessionErrorTriggered, Is.False);
                    obsoleteSessionErrorTriggered = true;
                });

                // priority is given to the newer appoach in SyncConfigurationBase, so this should never be reached
                Session.Error += handler;

                session.SimulateError(ErrorCode.NoSuchRealm, "simulated sync issue");

                // to avoid a race condition where e.g. both methods are called but because of timing differences `tcs.TrySetResult(true);` is reached
                // earlier in a call not letting the other finish to run. This would hide an issue.
                await tcs.Task;
                await Task.Delay(1000);

                Assert.That(obsoleteSessionErrorTriggered, Is.False);
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
                var completionTCS = new TaskCompletionSource<object>();

                var session = realm.SyncSession;
                session.Stop();

                session.PropertyChanged += NotificationChanged;

                session.Start();
                await Task.Delay(1000);
                session.Stop();
                await completionTCS.Task;
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
                            completionTCS.TrySetResult(null);
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
                var completionTCSA = new TaskCompletionSource<object>();
                var completionTCSB = new TaskCompletionSource<object>();

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
                    completionTCSA.TrySetResult(null);
                }

                void NotificationChangedB(object sender, PropertyChangedEventArgs e)
                {
                    Assert.That(subscriberBTriggered, Is.False);
                    subscriberBTriggered = true;
                    completionTCSB.TrySetResult(null);
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
                var completionTCS = new TaskCompletionSource<object>();

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
                            completionTCS.TrySetResult(null);
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

        protected override void CustomTearDown()
        {
            base.CustomTearDown();

#pragma warning disable CS0618 // Type or member is obsolete
            _sessionErrorHandlers.DrainQueue(handler => Session.Error -= handler);
#pragma warning restore CS0618 // Type or member is obsolete

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
                var cbName = type == typeof(RecoverOrDiscardUnsyncedChangesHandler)
                    ? nameof(RecoverOrDiscardUnsyncedChangesHandler.OnAfterRecovery)
                    : nameof(DiscardUnsyncedChangesHandler.OnAfterReset);
                type.GetProperty(cbName).SetValue(handler, afterCb);
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

        private static AfterResetCallback GetOnAfterHandler(TaskCompletionSource<object> tcs, Action<Realm, Realm> assertions)
        {
            return new AfterResetCallback((frozen, live) =>
            {
                try
                {
                    assertions(frozen, live);
                    tcs.TrySetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });
        }

        private static BeforeResetCallback GetOnBeforeHandler(TaskCompletionSource<object> tcs, Action<Realm> assertions)
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

        private static ClientResetCallback GetManualResetHandler(TaskCompletionSource<object> tcs, Action<ClientResetException> assertions)
        {
            return new ClientResetCallback(clientResetException =>
            {
                try
                {
                    assertions(clientResetException);
                    tcs.TrySetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });
        }

        private static EventHandler<ErrorEventArgs> GetErrorEventHandler(TaskCompletionSource<object> tcs, Func<Session, SessionException, bool> assertions)
        {
            return new((sender, error) =>
            {
                try
                {
                    if (assertions((Session)sender, (SessionException)error.Exception))
                    {
                        tcs.TrySetResult(null);
                    }
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });
        }

        private EventHandler<ErrorEventArgs> GetErrorEventHandler(TaskCompletionSource<object> tcs, Action<Session, SessionException> assertions)
        {
            var result = GetErrorEventHandler(tcs, (sender, error) =>
            {
                assertions(sender, error);
                return true;
            });

            CleanupOnTearDown(result);

            return result;
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

        private void CleanupOnTearDown(EventHandler<ErrorEventArgs> handler)
        {
            _sessionErrorHandlers.Enqueue(handler);
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
        public string Id { get; set; }

        public string Value { get; set; }

        [MapTo("realm_id")]
        public string Partition { get; set; }

        public Guid Guid { get; set; }
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
