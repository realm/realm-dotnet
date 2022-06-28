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
using NUnit.Framework;
using Realms.Exceptions.Sync;
using Realms.Sync;
using Realms.Sync.ErrorHandling;
using Realms.Sync.Exceptions;
using Realms.Sync.Native;
using Realms.Sync.Testing;
using static Baas.BaasClient;
using static Realms.Sync.ErrorHandling.ClientResetHandlerBase;

namespace Realms.Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class SessionTests : SyncTestBase
    {
        private readonly ConcurrentQueue<EventHandler<ErrorEventArgs>> _sessionErrorHandlers = new();

        public static readonly object[] AllClientResetHandlers = new object[]
        {
            new object[] { typeof(DiscardLocalResetHandler), (AutomaticRecoveryHandler.Fallback?)null },
            new object[] { typeof(AutomaticRecoveryHandler), AutomaticRecoveryHandler.Fallback.DiscardLocal },
            new object[] { typeof(AutomaticRecoveryHandler), AutomaticRecoveryHandler.Fallback.Manual },
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

        [TestCaseSource(nameof(AllClientResetHandlers))]
        public void Session_ClientResetHandlers_ManualResetFallback_AutoClientReset(Type handlerType, AutomaticRecoveryHandler.Fallback? fallback)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var onBeforeTriggered = false;
                var onAfterTriggered = false;
                var manualResetFallbackHandled = false;
                var config = await GetIntegrationConfigAsync();
                var tcs = new TaskCompletionSource<object>();

                var beforeCb = GetOnBeforeHandler(tcs, beforeFrozen =>
                {
                    onBeforeTriggered = true;
                });
                var afterCb = GetOnAfterHandler(tcs, (beforeFrozen, after) =>
                {
                    onAfterTriggered = true;
                });
                var manualCb = GetClientResetHandler(tcs, err =>
                {
                    Assert.That(err, Is.InstanceOf<ClientResetException>());
                    Assert.That(onBeforeTriggered, Is.False);
                    Assert.That(onAfterTriggered, Is.False);
                    Assert.That(manualResetFallbackHandled, Is.False);
                    manualResetFallbackHandled = true;
                });

                config.ClientResetHandler = GetClientResetHandler(handlerType, fallback, beforeCb, afterCb, manualCb);

                using var realm = await GetRealmAsync(config);

                GetSession(realm).SimulateAutomaticClientResetFailure("simulated client reset failure");

                await tcs.Task;

                Assert.That(onBeforeTriggered, Is.False);
                Assert.That(onAfterTriggered, Is.False);
                Assert.That(manualResetFallbackHandled, Is.True);
            });
        }

        [Test]
        public void Session_ClientReset_ManualRecovery_InitiateClientReset()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var manualOnClientResetTriggered = false;
                var errorMsg = "simulated client reset";
                var errorTcs = new TaskCompletionSource<ClientResetException>();
                var config = await GetIntegrationConfigAsync();
                config.ClientResetHandler = new ManualRecoveryHandler((e) =>
                {
                    manualOnClientResetTriggered = true;
                    errorTcs.TrySetResult(e);
                });

                using (var realm = await GetRealmAsync(config))
                {
                    GetSession(realm).SimulateClientReset(errorMsg);
                }

                var clientEx = await errorTcs.Task;

                Assert.That(manualOnClientResetTriggered, Is.True);

                Assert.That(clientEx.Message, Is.EqualTo(errorMsg));
                Assert.That(clientEx.InnerException, Is.Null);
                await TryInitiateClientReset(clientEx, (int)ErrorCode.DivergingHistories, config);
            });
        }

        [TestCaseSource(nameof(AllClientResetHandlers))]
        public void Session_ClientResetHandlers_ManualResetFallback_InitiateClientReset(Type handlerType, AutomaticRecoveryHandler.Fallback? fallback)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var manualResetFallbackHandled = false;
                var errorTcs = new TaskCompletionSource<ClientResetException>();
                var config = await GetIntegrationConfigAsync();
                ClientResetCallback manualCb = (err) =>
                {
                    manualResetFallbackHandled = true;
                    errorTcs.TrySetResult(err);
                };

                config.ClientResetHandler = GetClientResetHandler(handlerType, fallback, beforeCb: null, afterCb: null, manualCb);

                using (var realm = await GetRealmAsync(config))
                {
                    GetSession(realm).SimulateAutomaticClientResetFailure("simulated client reset failure");
                }

                var clientEx = await errorTcs.Task;

                Assert.That(manualResetFallbackHandled, Is.True);

                await TryInitiateClientReset(clientEx, (int)ClientError.AutoClientResetFailed, config);
            });
        }

        [TestCaseSource(nameof(AllClientResetHandlers))]
        public void Session_ClientResetHandlers_OnBefore_And_OnAfter(Type handlerType, AutomaticRecoveryHandler.Fallback? fallback)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var onBeforeTriggered = false;
                var onAfterTriggered = false;
                var tcs = new TaskCompletionSource<object>();
                var config = await GetIntegrationConfigAsync();

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

                config.ClientResetHandler = GetClientResetHandler(handlerType, fallback, beforeCb, afterCb);
                using var realm = await GetRealmAsync(config);

                GetSession(realm).SimulateClientReset("simulated client reset");

                await tcs.Task;

                Assert.That(onBeforeTriggered, Is.True);
                Assert.That(onAfterTriggered, Is.True);
            });
        }

        [Test]
        public void Session_AutomaticRecoveryFallsbackToDiscardLocal()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var config = await GetIntegrationConfigAsync();

                var tscAfterClientReset = new TaskCompletionSource<object>();

                config.Schema = new[] { typeof(SyncObjectWithRequiredStringList) };
                var afterCb = GetOnAfterHandler(tscAfterClientReset, (before, after) =>
                {
                    Assert.That(after.All<SyncObjectWithRequiredStringList>().Count, Is.EqualTo(0));
                });
                config.ClientResetHandler = new AutomaticRecoveryHandler()
                {
                    OnAfterReset = afterCb
                };
                var realm = await GetRealmAsync(config);

                var session = GetSession(realm);
                session.Stop();

                realm.Write(() =>
                {
                    realm.Add(new SyncObjectWithRequiredStringList());
                });

                await SyncTestHelpers.DisallowRecoveryModeOnServer();
                var result = await ((PartitionSyncConfiguration)realm.Config).User.Functions.CallAsync<FunctionReturn>("triggerClientResetOnSyncServer");
                Assert.That(result.status, Is.EqualTo(FunctionReturn.Result.success));

                session.Start();
                await tscAfterClientReset.Task;
            });
        }

        /* If any ArrayDelete or ArrayMove operates on indices not added by the recovery, such operation will be discarded.
         * Hence, the last client to experience a reset will "win" the array state.
         *
         * 1. clientA adds objectA with array innerObj[0,1,2] and syncs it, then disconnects
         * 2. clientB starts and syncs the same objectA, then disconnects
         * 3. While offline, clientA deletes innerObj[2] while clientB swaps innerObj[0] with innerObj[2]
         * 4. A client reset is triggered on the server
         * 5. clientA goes online and uploads the changes
         * 6. only now clientB goes online, downloads and merges the changes. Only innerObj[0,1] exist and the swap is discarded
         */
        [Test]
        public void SessionIntegrationTest_ClientResetHandlers_OutOfBoundArrayMove_Discarded()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();
                var partition = Guid.NewGuid().ToString();

                // ===== clientA =====
                var tscAfterClientResetMergeA = new TaskCompletionSource<object>();
                var configA = await GetIntegrationConfigAsync(partition, user: user);
                configA.Schema = new[] { typeof(SyncObjectWithRequiredStringList) };
                var afterCbA = GetOnAfterHandler(tscAfterClientResetMergeA, (before, after) =>
                {
                    var list = after.All<SyncObjectWithRequiredStringList>().First().Strings;
                    Assert.That(list.Count, Is.EqualTo(2));
                    Assert.That(list[0], Is.EqualTo("0"));
                    Assert.That(list[1], Is.EqualTo("1"));
                });
                configA.ClientResetHandler = new AutomaticRecoveryHandler()
                {
                    OnAfterReset = afterCbA
                };

                var realmA = await GetRealmAsync(configA);

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
                await WaitForUploadAsync(realmA);

                var sessionA = GetSession(realmA);
                sessionA.Stop();

                realmA.Write(() =>
                {
                    originalObj.Strings.RemoveAt(2);
                });

                // ===== clientB =====
                var configB = await GetIntegrationConfigAsync(partition, optionalPath: configA.DatabasePath + "_B", user: user);
                configB.Schema = new[] { typeof(SyncObjectWithRequiredStringList) };
                var tcsAfterClientResetB = new TaskCompletionSource<object>();
                var afterCbB = GetOnAfterHandler(tcsAfterClientResetB, (before, after) =>
                {
                    var listB = after.All<SyncObjectWithRequiredStringList>().Single().Strings.ToList();
                    var list = after.All<SyncObjectWithRequiredStringList>().Single().Strings;
                    Assert.That(list.Count, Is.EqualTo(3));
                    Assert.That(list[0], Is.EqualTo("2"));
                    Assert.That(list[1], Is.EqualTo("0"));
                    Assert.That(list[2], Is.EqualTo("1"));
                });
                configB.ClientResetHandler = new AutomaticRecoveryHandler()
                {
                    OnAfterReset = afterCbB
                };
                var realmB = await GetRealmAsync(configB);

                await WaitForDownloadAsync(realmB);

                Assert.That(realmB.All<SyncObjectWithRequiredStringList>().Count, Is.EqualTo(1));
                var originalObjStr = realmB.All<SyncObjectWithRequiredStringList>().Single().Strings;
                Assert.That(originalObjStr.Count, Is.EqualTo(3));
                Assert.That(originalObjStr.First(), Is.EqualTo("0"));
                Assert.That(originalObjStr.ElementAt(1), Is.EqualTo("1"));
                Assert.That(originalObjStr.ElementAt(2), Is.EqualTo("2"));

                var sessionB = GetSession(realmB);
                sessionB.Stop();

                realmB.Write(() =>
                {
                    originalObjStr.Move(2, 0);
                });

                // Trigger client reset for both clients
                var result = await user.Functions.CallAsync<FunctionReturn>("triggerClientResetOnSyncServer");
                Assert.That(result.status, Is.EqualTo(FunctionReturn.Result.success));

                sessionA.Start();
                await tscAfterClientResetMergeA.Task;

                sessionB.Start();
                await tcsAfterClientResetB.Task;
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
                var user = await GetUserAsync();
                var partition = Guid.NewGuid().ToString();

                // ===== clientA =====
                var tcsAfterClientResetA = new TaskCompletionSource<object>();
                var configA = await GetIntegrationConfigAsync(partition, user: user);
                configA.Schema = new[] { typeof(SyncObjectWithRequiredStringList) };
                var afterCbA = GetOnAfterHandler(tcsAfterClientResetA, (before, after) =>
                {
                    var list = after.All<SyncObjectWithRequiredStringList>().First().Strings;

                    // We deleted an object, so that should have been merged
                    Assert.That(list, Is.EqualTo(new[] { "0", "1" }));
                });

                configA.ClientResetHandler = new AutomaticRecoveryHandler()
                {
                    OnAfterReset = afterCbA
                };
                using var realmA = await GetRealmAsync(configA);

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
                await WaitForUploadAsync(realmA);

                var sessionA = GetSession(realmA);
                sessionA.Stop();

                realmA.Write(() =>
                {
                    originalObj.Strings.RemoveAt(2);
                });

                // ===== clientB =====
                var configB = await GetIntegrationConfigAsync(partition, optionalPath: configA.DatabasePath + "_B", user: user);
                configB.Schema = new[] { typeof(SyncObjectWithRequiredStringList) };
                var tcsAfterClientResetB = new TaskCompletionSource<object>();
                var afterCbB = GetOnAfterHandler(tcsAfterClientResetB, (before, after) =>
                {
                    var list = after.All<SyncObjectWithRequiredStringList>().Single().Strings;

                    // We added an object in the tail, that should be merged
                    Assert.That(list, Is.EqualTo(new[] { "0", "1", "3" }));
                });
                configB.ClientResetHandler = new AutomaticRecoveryHandler()
                {
                    OnAfterReset = afterCbB
                };

                using var realmB = await GetRealmAsync(configB);
                await WaitForDownloadAsync(realmB);

                var originalObjStr = realmB.All<SyncObjectWithRequiredStringList>().Single().Strings;
                Assert.That(originalObjStr, Is.EqualTo(new[] { "0", "1", "2" }));

                var sessionB = GetSession(realmB);
                sessionB.Stop();

                realmB.Write(() =>
                {
                    originalObjStr.Add("3");
                });

                var result = await user.Functions.CallAsync<FunctionReturn>("triggerClientResetOnSyncServer");
                Assert.That(result.status, Is.EqualTo(FunctionReturn.Result.success));

                // ===== clientA =====
                sessionA.Start();
                await tcsAfterClientResetA.Task;

                var tscAfterRemoteUpdateA = new TaskCompletionSource<object>();
                realmA.All<SyncObjectWithRequiredStringList>().First().PropertyChanged += (sender, eventArgs) =>
                {
                    if (eventArgs.PropertyName == nameof(SyncObjectWithRequiredStringList.Strings))
                    {
                        var list = realmA.All<SyncObjectWithRequiredStringList>().Single().Strings;

                        // After clientB merges and uploads the changes,
                        // clientA should receive the updated status
                        Assert.That(list, Is.EqualTo(new[] { "0", "1", "3" }));

                        tscAfterRemoteUpdateA.TrySetResult(null);
                    }
                };

                // ===== clientB =====
                sessionB.Start();

                await tcsAfterClientResetB.Task;
                await tscAfterRemoteUpdateA.Task;
            });
        }

        /* Any Update to an object which does not exist in the fresh Realm is discarded
         *
         * 1. clientA adds obj0 and obj1 and syncs it, then disconnects
         * 2. clientB starts and syncs the same 2 objects, then disconnects
         * 3. While offline, clientA deletes obj1 while clientB updates obj1
         * 4. A client reset is triggered on the server
         * 5. clientA goes online and uploads the deletion
         * 6. only now clientB goes online, and the resulting merge
         *    has discarded clientB's update to obj1. In fact, only obj0 exists
         */
        [Test]
        public void SessionIntegrationTest_ClientResetHandlers_UpdateToDeletedObject_Discarded()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await GetUserAsync();
                var partition = Guid.NewGuid().ToString();

                // ===== clientA =====
                var tcsAfterClientResetA = new TaskCompletionSource<object>();
                var configA = await GetIntegrationConfigAsync(partition, user: user);
                configA.Schema = new[] { typeof(ObjectWithPartitionValue) };
                var afterCbA = GetOnAfterHandler(tcsAfterClientResetA, (before, after) =>
                {
                    Assert.That(after.All<ObjectWithPartitionValue>().Single().Value, Is.EqualTo("obj0"));
                });

                configA.ClientResetHandler = new AutomaticRecoveryHandler()
                {
                    OnAfterReset = afterCbA
                };

                using var realmA = await GetRealmAsync(configA);

                var objToRemove = realmA.Write(() =>
                {
                    realmA.Add(new ObjectWithPartitionValue
                    {
                        Id = Guid.NewGuid().ToString(),
                        Value = "obj0"
                    });
                    return realmA.Add(new ObjectWithPartitionValue
                    {
                        Id = Guid.NewGuid().ToString(),
                        Value = "obj1"
                    });
                });

                await WaitForUploadAsync(realmA);
                var sessionA = GetSession(realmA);
                sessionA.Stop();

                realmA.Write(() =>
                {
                    realmA.Remove(objToRemove);
                });

                // ===== clientB =====
                var configB = await GetIntegrationConfigAsync(partition, optionalPath: configA.DatabasePath + "_B", user: user);
                configB.Schema = new[] { typeof(ObjectWithPartitionValue) };
                var tcsAfterClientResetB = new TaskCompletionSource<object>();
                var afterCbB = GetOnAfterHandler(tcsAfterClientResetB, (before, after) =>
                {
                    Assert.That(after.All<ObjectWithPartitionValue>().Single().Value, Is.EqualTo("obj0"));
                });

                configB.ClientResetHandler = new AutomaticRecoveryHandler()
                {
                    OnAfterReset = afterCbB
                };
                using var realmB = await GetRealmAsync(configB);
                await WaitForDownloadAsync(realmB);

                var objToUpdate = realmB.All<ObjectWithPartitionValue>().ElementAt(1);
                Assert.That(realmB.All<ObjectWithPartitionValue>().Count, Is.EqualTo(2));
                Assert.That(realmB.All<ObjectWithPartitionValue>().First().Value, Is.EqualTo("obj0"));
                Assert.That(objToUpdate.Value, Is.EqualTo("obj1"));

                var sessionB = GetSession(realmB);
                sessionB.Stop();

                realmB.Write(() =>
                {
                    objToUpdate.Value = "changed but later discarded";
                });

                // Trigger client reset for both clients
                var result = await user.Functions.CallAsync<FunctionReturn>("triggerClientResetOnSyncServer");
                Assert.That(result.status, Is.EqualTo(FunctionReturn.Result.success));

                // ===== clientA =====
                sessionA.Start();
                await tcsAfterClientResetA.Task;

                // ===== clientB =====
                sessionB.Start();
                await tcsAfterClientResetB.Task;
            });
        }

        [TestCaseSource(nameof(AllClientResetHandlers))]
        public void Session_ClientResetHandlers_AccessRealm_OnBeforeReset(Type handlerType, AutomaticRecoveryHandler.Fallback? fallback)
        {
            const string alwaysSynced = "always synced";
            const string maybeSynced = "deleted only on discardLocal";

            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var tcs = new TaskCompletionSource<object>();
                var onBeforeTriggered = false;
                var config = await GetIntegrationConfigAsync();
                var beforeCb = GetOnBeforeHandler(tcs, beforeFrozen =>
                {
                    Assert.That(onBeforeTriggered, Is.False);

                    AssertOnObjectPair(beforeFrozen);
                    onBeforeTriggered = true;
                    tcs.SetResult(null);
                });
                config.ClientResetHandler = GetClientResetHandler(handlerType, fallback, beforeCb);
                config.Schema = new[] { typeof(ObjectWithPartitionValue) };

                using var realm = await GetRealmAsync(config);

                realm.Write(() =>
                {
                    realm.Add(new ObjectWithPartitionValue
                    {
                        Id = Guid.NewGuid().ToString(),
                        Value = alwaysSynced
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
                    });
                });

                AssertOnObjectPair(realm);

                var result = await ((PartitionSyncConfiguration)realm.Config).User.Functions.CallAsync<FunctionReturn>("triggerClientResetOnSyncServer");
                Assert.That(result.status, Is.EqualTo(FunctionReturn.Result.success));
                session.Start();

                await tcs.Task;
                Assert.That(onBeforeTriggered, Is.True);

                var objs = realm.All<ObjectWithPartitionValue>();
                var isDiscardLocal = config.ClientResetHandler.ClientResetMode == ClientResyncMode.DiscardLocal;
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
                    Assert.That(realm.All<ObjectWithPartitionValue>().ToArray().Select(o => o.Value), Is.EquivalentTo(new[] { alwaysSynced, maybeSynced }));
                }
            });
        }

        [TestCaseSource(nameof(AllClientResetHandlers))]
        public void Session_ClientResetHandlers_Access_Realms_OnAfterReset(Type handlerType, AutomaticRecoveryHandler.Fallback? fallback)
        {
            const string alwaysSynced = "always synced";
            const string maybeSynced = "deleted only on discardLocal";

            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var tcs = new TaskCompletionSource<object>();
                var onAfterTriggered = false;
                var config = await GetIntegrationConfigAsync();
                var afterCb = GetOnAfterHandler(tcs, (beforeFrozen, after) =>
                {
                    Assert.That(onAfterTriggered, Is.False);
                    Assert.That(beforeFrozen.All<ObjectWithPartitionValue>().ToArray().Select(o => o.Value), Is.EquivalentTo(new[] { alwaysSynced, maybeSynced }));
                    AssertHelper(after);
                    onAfterTriggered = true;
                });
                config.ClientResetHandler = GetClientResetHandler(handlerType, fallback, beforeCb: null, afterCb);
                config.Schema = new[] { typeof(ObjectWithPartitionValue) };

                using var realm = await GetRealmAsync(config);

                realm.Write(() =>
                {
                    realm.Add(new ObjectWithPartitionValue
                    {
                        Id = Guid.NewGuid().ToString(),
                        Value = alwaysSynced
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
                    });
                });

                var result = await ((PartitionSyncConfiguration)realm.Config).User.Functions.CallAsync<FunctionReturn>("triggerClientResetOnSyncServer");
                Assert.That(result.status, Is.EqualTo(FunctionReturn.Result.success));
                session.Start();

                await tcs.Task;
                Assert.That(onAfterTriggered, Is.True);

                realm.Refresh();

                AssertHelper(realm);

                void AssertHelper(Realm realm)
                {
                    var expected = config.ClientResetHandler.ClientResetMode == ClientResyncMode.DiscardLocal ?
                        new[] { alwaysSynced } : new[] { alwaysSynced, maybeSynced };

                    Assert.That(realm.All<ObjectWithPartitionValue>().ToArray().Select(o => o.Value), Is.EquivalentTo(expected));
                }
            });
        }

        [Test]
        public void Session_ClientResetDiscard_TriggersNotifications()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                // We'll add an object with the wrong partition
                var config = await GetIntegrationConfigAsync();
                config.Schema = new[] { typeof(ObjectWithPartitionValue) };
                config.ClientResetHandler = new DiscardLocalResetHandler();

                using var realm = await GetRealmAsync(config);

                realm.Write(() =>
                {
                    realm.Add(new ObjectWithPartitionValue
                    {
                        Id = Guid.NewGuid().ToString(),
                        Value = "this will sync"
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
                        Value = "this will be merged at client reset"
                    });
                });

                var objects = realm.All<ObjectWithPartitionValue>().AsRealmCollection();
                Assert.That(objects.Count, Is.EqualTo(2));
                var tcs = new TaskCompletionSource<NotifyCollectionChangedEventArgs>();
                objects.CollectionChanged += onCollectionChanged;

                var result = await ((PartitionSyncConfiguration)realm.Config).User.Functions.CallAsync<FunctionReturn>("triggerClientResetOnSyncServer");
                Assert.That(result.status, Is.EqualTo(FunctionReturn.Result.success));
                session.Start();
                await WaitForDownloadAsync(realm);
                var args = await tcs.Task;

                Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));
                Assert.That(objects.Count, Is.EqualTo(1));

                objects.CollectionChanged -= onCollectionChanged;

                void onCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
                {
                    tcs.TrySetResult(args);
                }
            }, timeout: 120_000);
        }

        [TestCaseSource(nameof(AllClientResetHandlers))]
        public void Session_ClientResetHandlers_ManualResetFallback_Exception_OnBefore(Type handlerType, AutomaticRecoveryHandler.Fallback? fallback)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var tcs = new TaskCompletionSource<object>();
                var onBeforeTriggered = false;
                var manualFallbackTriggered = false;
                var onAfterResetTriggered = false;
                var config = await GetIntegrationConfigAsync();

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

                var manualCb = GetClientResetHandler(tcs, (ex) =>
                {
                    Assert.That(ex, Is.InstanceOf<ClientResetException>());
                    Assert.That(onBeforeTriggered, Is.True);
                    Assert.That(onAfterResetTriggered, Is.False);
                    Assert.That(manualFallbackTriggered, Is.False);
                    manualFallbackTriggered = true;
                });

                config.ClientResetHandler = GetClientResetHandler(handlerType, fallback, beforeCb, afterCb, manualCb);

                using var realm = await GetRealmAsync(config);

                GetSession(realm).SimulateClientReset("simulated client reset");

                await tcs.Task;

                Assert.That(manualFallbackTriggered, Is.True);
                Assert.That(onBeforeTriggered, Is.True);
                Assert.That(onAfterResetTriggered, Is.False);
            });
        }

        [TestCaseSource(nameof(AllClientResetHandlers))]
        public void Session_ClientResetHandlers_ManualResetFallback_Exception_OnAfter(Type handlerType, AutomaticRecoveryHandler.Fallback? fallback)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var tcs = new TaskCompletionSource<object>();
                var onBeforeTriggered = false;
                var manualFallbackTriggered = false;
                var onAfterResetTriggered = false;
                var config = await GetIntegrationConfigAsync();

                var beforeCb = GetOnBeforeHandler(tcs, beforeFrozen =>
                {
                    Assert.That(onBeforeTriggered, Is.False);
                    Assert.That(onAfterResetTriggered, Is.False);
                    Assert.That(manualFallbackTriggered, Is.False);
                    onBeforeTriggered = true;
                });
                AfterResetCallback afterCb = (beforeFrozen, after) =>
                {
                    Assert.That(onBeforeTriggered, Is.True);
                    Assert.That(onAfterResetTriggered, Is.False);
                    Assert.That(manualFallbackTriggered, Is.False);
                    onAfterResetTriggered = true;
                    throw new Exception("Exception thrown in OnAfterReset");
                };
                var manualCb = GetClientResetHandler(tcs, (ex) =>
                {
                    Assert.That(ex, Is.InstanceOf<ClientResetException>());
                    Assert.That(onBeforeTriggered, Is.True);
                    Assert.That(onAfterResetTriggered, Is.True);
                    Assert.That(manualFallbackTriggered, Is.False);
                    manualFallbackTriggered = true;
                });

                config.ClientResetHandler = GetClientResetHandler(handlerType, fallback, beforeCb, afterCb, manualCb);

                using var realm = await GetRealmAsync(config);

                GetSession(realm).SimulateClientReset("simulated client reset");

                await tcs.Task;

                Assert.That(manualFallbackTriggered, Is.True);
                Assert.That(onBeforeTriggered, Is.True);
                Assert.That(onAfterResetTriggered, Is.True);
            });
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
                    Assert.That(e.ErrorCode, Is.EqualTo(ErrorCode.PermissionDenied));
                    Assert.That(e.Message, Is.EqualTo(errorMsg));
                    Assert.That(e.InnerException, Is.Null);
                    Assert.That(sessionErrorTriggered, Is.False);
                    sessionErrorTriggered = true;
                    tcs.TrySetResult(true);
                };

                using var realm = await GetRealmAsync(config);
                var session = GetSession(realm);
                session.SimulateError(ErrorCode.PermissionDenied, errorMsg);

                await tcs.Task;

                Assert.That(sessionErrorTriggered, Is.True);
            });
        }

        [TestCaseSource(nameof(AllClientResetHandlers)),
            Obsolete("Testing Sesion.Error compatibility")]
        public void Session_ClientResetHandlers_Coexistence(Type handlerType, AutomaticRecoveryHandler.Fallback? fallback)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var onBeforeTriggered = false;
                var onAfterTriggered = false;
                var obsoleteSessionErrorTriggered = false;
                var tcs = new TaskCompletionSource<object>();
                var config = await GetIntegrationConfigAsync();
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

                config.ClientResetHandler = GetClientResetHandler(handlerType, fallback, beforeCb, afterCb);

                var handler = GetErrorEventHandler(tcs, (session, error) =>
                {
                    Assert.That(obsoleteSessionErrorTriggered, Is.False);
                    obsoleteSessionErrorTriggered = true;
                });

                // priority is given to the newer appoach in SyncConfigurationBase, so this should never be reached
                Session.Error += handler;

                using var realm = await GetRealmAsync(config);
                var session = GetSession(realm);

                session.SimulateClientReset("simulated client reset");

                // to avoid a race condition where e.g. both methods are called but because of timing differences `tcs.TrySetResult(true);` is reached
                // earlier in a call not letting the other finish to run. This would hide an issue.
                await tcs.Task;
                await Task.Delay(1000);

                Assert.That(onBeforeTriggered, Is.True);
                Assert.That(onAfterTriggered, Is.True);
                Assert.That(obsoleteSessionErrorTriggered, Is.False);
            });
        }

        [TestCaseSource(nameof(AllClientResetHandlers)),
            Obsolete("Testing Sesion.Error compatibility")]
        public void Session_WithNewClientResetHandlers_DoesntRaiseSessionError(Type handlerType, AutomaticRecoveryHandler.Fallback? fallback)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var obsoleteSessionErrorTriggered = false;
                var config = await GetIntegrationConfigAsync();
                config.ClientResetHandler = GetClientResetHandler(handlerType, fallback);
                using var realm = await GetRealmAsync(config);
                var session = GetSession(realm);

                // priority is given to the newer appoach in SyncConfigurationBase, so this should never be reached
                Session.Error += onSessionError;
                CleanupOnTearDown(onSessionError);

                session.SimulateClientReset("simulated client reset");
                session.SimulateAutomaticClientResetFailure("failure #2");

                await Task.Delay(1000);

                Assert.That(obsoleteSessionErrorTriggered, Is.False);

                void onSessionError(object sender, ErrorEventArgs e)
                {
                    obsoleteSessionErrorTriggered = true;
                }
            });
        }

        [TestCaseSource(nameof(AllClientResetHandlers)),
            Obsolete("Testing Sesion.Error compatibility")]
        public void Session_ClientResetHandlers_ManualResetFallback_Coexistence(Type handlerType, AutomaticRecoveryHandler.Fallback? fallback)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var manualResetFallbackHandled = false;
                var obsoleteSessionErrorTriggered = false;
                var errorMsg = "simulated client reset failure";
                var tcs = new TaskCompletionSource<object>();
                var config = await GetIntegrationConfigAsync();

                ClientResetCallback manualCb = (err) =>
                {
                    Assert.That(manualResetFallbackHandled, Is.False);
                    Assert.That(err, Is.InstanceOf<ClientResetException>());

                    Assert.That((int)err.ErrorCode, Is.EqualTo((int)ClientError.AutoClientResetFailed));
                    Assert.That(err.Message, Is.EqualTo(errorMsg));
                    Assert.That(err.InnerException, Is.Null);
                    manualResetFallbackHandled = true;
                    tcs.TrySetResult(true);
                };

                config.ClientResetHandler = GetClientResetHandler(handlerType, fallback, beforeCb: null, afterCb: null, manualCb);

                using var realm = await GetRealmAsync(config);

                var handler = GetErrorEventHandler(tcs, (session, error) =>
                {
                    Assert.That(obsoleteSessionErrorTriggered, Is.False);
                    obsoleteSessionErrorTriggered = true;
                });

                // priority is given to the newer appoach in SyncConfigurationBase, so this should never be reached
                Session.Error += handler;

                GetSession(realm).SimulateAutomaticClientResetFailure(errorMsg);

                // to avoid a race condition where e.g. both methods are called but because of timing differences `tcs.TrySetResult(true);` is reached
                // earlier in a call not letting the other finish to run. This would hide an issue.
                await tcs.Task;
                await Task.Delay(1000);

                Assert.That(manualResetFallbackHandled, Is.True);
                Assert.That(obsoleteSessionErrorTriggered, Is.False);
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
                var errorMsg = "simulated sync issue";

                using (var realm = await GetRealmAsync(config))
                {
                    var session = GetSession(realm);

                    // Session.Error is set after obtaining a realm as it truly tests coexistence given that
                    // the resync mode is set at creation of the configuration.
                    // SyncConfigurationBase.CreateNativeSyncConfiguration.
                    Session.Error += onSessionError;
                    CleanupOnTearDown(onSessionError);

                    session.SimulateClientReset(errorMsg);
                }

                var ex = await tcs.Task;

                Assert.That(obsoleteSessionErrorTriggered, Is.True);

                Assert.That(ex, Is.InstanceOf<ClientResetException>());
                var clientEx = (ClientResetException)ex;
                Assert.That(clientEx.Message, Is.EqualTo(errorMsg));
                Assert.That(clientEx.InnerException, Is.Null);

                await TryInitiateClientReset(clientEx, (int)ErrorCode.DivergingHistories, config);

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
                    Assert.That(error.ErrorCode == ErrorCode.PermissionDenied);
                    Assert.That(error.Message == errorMsg);
                    Assert.That(error.InnerException == null);
                    Assert.That(obsoleteSessionErrorTriggered, Is.False);
                    obsoleteSessionErrorTriggered = true;
                });

                Session.Error += handler;

                using var realm = await GetRealmAsync(config);

                var session = GetSession(realm);
                session.SimulateError(ErrorCode.PermissionDenied, "simulated sync issue");

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

                using var realm = await GetRealmAsync(config);

                var session = GetSession(realm);

                var handler = GetErrorEventHandler(tcs, (session, error) =>
                {
                    Assert.That(obsoleteSessionErrorTriggered, Is.False);
                    obsoleteSessionErrorTriggered = true;
                });

                // priority is given to the newer appoach in SyncConfigurationBase, so this should never be reached
                Session.Error += handler;

                session.SimulateClientReset("simulated client reset");

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

                using var realm = await GetRealmAsync(config);
                var session = GetSession(realm);

                var handler = GetErrorEventHandler(tcs, (session, error) =>
                {
                    Assert.That(obsoleteSessionErrorTriggered, Is.False);
                    obsoleteSessionErrorTriggered = true;
                });

                // priority is given to the newer appoach in SyncConfigurationBase, so this should never be reached
                Session.Error += handler;

                session.SimulateError(ErrorCode.PermissionDenied, "simulated sync issue");

                // to avoid a race condition where e.g. both methods are called but because of timing differences `tcs.TrySetResult(true);` is reached
                // earlier in a call not letting the other finish to run. This would hide an issue.
                await tcs.Task;
                await Task.Delay(1000);

                Assert.That(obsoleteSessionErrorTriggered, Is.False);
                Assert.That(sessionErrorTriggered, Is.True);
            });
        }

        [TestCaseSource(nameof(DoubleModeClientResetHandlers))]
        public void SessionIntegrationTest_ProgressObservable((ProgressMode Mode, ClientResetHandlerBase Handler) tuple)
        {
            const int ObjectSize = 1_000_000;
            const int ObjectsToRecord = 2;
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var config = await GetIntegrationConfigAsync(Guid.NewGuid().ToString());
                config.ClientResetHandler = tuple.Handler;
                using var realm = GetRealm(config);

                var completionTCS = new TaskCompletionSource<ulong>();
                var callbacksInvoked = 0;

                var session = GetSession(realm);

                var observable = session.GetProgressObservable(ProgressDirection.Upload, tuple.Mode);

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

                        if (tuple.Mode == ProgressMode.ForCurrentlyOutstandingWork)
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

                if (tuple.Mode == ProgressMode.ForCurrentlyOutstandingWork)
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
            Assert.Throws<ObjectDisposedException>(() => session.ReportErrorForTesting(1, SessionErrorCategory.SessionError, "test", false));

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

        private static IEnumerable<(ProgressMode Mode, ClientResetHandlerBase Handler)> DoubleModeClientResetHandlers()
        {
            foreach (var intMode in Enum.GetValues(typeof(ProgressMode)))
            {
                foreach (object[] handler in AllClientResetHandlers)
                {
                    var handlerInstance = GetClientResetHandler((Type)handler[0], (AutomaticRecoveryHandler.Fallback?)handler[1]);
                    yield return ((ProgressMode)intMode, handlerInstance);
                }
            }
        }

        protected override void CustomTearDown()
        {
            base.CustomTearDown();

#pragma warning disable CS0618 // Type or member is obsolete
            _sessionErrorHandlers.DrainQueue(handler => Session.Error -= handler);
#pragma warning restore CS0618 // Type or member is obsolete

        }

        public interface IClientResetHandler
        {
            ClientResetHandlerBase BuildHandler(BeforeResetCallback beforeCb = null,
                                                AfterResetCallback afterCb = null,
                                                ClientResetCallback manualCb = null);
        }

        private static ClientResetHandlerBase GetClientResetHandler(
            Type type,
            AutomaticRecoveryHandler.Fallback? fallback,
            BeforeResetCallback beforeCb = null,
            AfterResetCallback afterCb = null,
            ClientResetCallback manualCb = null)
        {
            var handler = (ClientResetHandlerBase)(fallback.HasValue ? Activator.CreateInstance(type, fallback.Value) : Activator.CreateInstance(type));

            if (beforeCb != null)
            {
                type.GetProperty(nameof(DiscardLocalResetHandler.OnBeforeReset)).SetValue(handler, beforeCb);
            }

            if (afterCb != null)
            {
                type.GetProperty(nameof(DiscardLocalResetHandler.OnAfterReset)).SetValue(handler, afterCb);
            }

            if (manualCb != null)
            {
                type.GetProperty(nameof(DiscardLocalResetHandler.ManualResetFallback)).SetValue(handler, manualCb);
            }

            return handler;
        }

        private static async Task TryInitiateClientReset(ClientResetException ex, int expectedError, SyncConfigurationBase config)
        {
            Assert.That((int)ex.ErrorCode, Is.EqualTo(expectedError));
            Assert.That(File.Exists(config.DatabasePath), Is.True);

            var didReset = false;
            for (var i = 0; i < 100 && !didReset; i++)
            {
                await Task.Delay(50);
                didReset = ex.InitiateClientReset();
            }

            Assert.That(didReset, Is.True, "Failed to complete manual reset after 100 attempts.");

            Assert.That(File.Exists(config.DatabasePath), Is.False);
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

        private static ClientResetCallback GetClientResetHandler(TaskCompletionSource<object> tcs, Action<ClientResetException> assertions)
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

        [Explicit]
        public class ObjectWithPartitionValue : RealmObject
        {
            [PrimaryKey]
            [MapTo("_id")]
            public string Id { get; set; }

            public string Value { get; set; }

            [MapTo("realm_id")]
            public string Partition { get; set; }
        }

        public class SyncObjectWithRequiredStringList : RealmObject
        {
            [PrimaryKey]
            [MapTo("_id")]
            public string Id { get; set; }

            [Required]
            public IList<string> Strings { get; }
        }

        private static SessionNotificationToken? GetNotificationToken(Session session)
        {
            var sessionHandle = (SessionHandle)typeof(Session).GetField("_handle", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(session);
            return sessionHandle != null ?
                (SessionNotificationToken?)typeof(SessionHandle).GetField("_notificationToken", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sessionHandle) :
                null;
        }
    }
}
