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
using NUnit.Framework;
using Realms.Exceptions.Sync;
using Realms.Sync;
using Realms.Sync.ErrorHandling;
using Realms.Sync.Exceptions;
using Realms.Sync.Testing;

namespace Realms.Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true), NonParallelizable]
    public class SessionTests : SyncTestBase
    {
        [Test]
        public void Realm_SyncSession_WhenSyncedRealm()
        {
            var config = GetFakeConfig();

            using var realm = GetRealm(config);
            var session = GetSession(realm);

            Assert.That(session.User, Is.EqualTo(config.User));
        }

        [Test]
        [Obsolete("tests obsolete functionality")]
        public void Realm_GetSession_WhenSyncedRealm()
        {
            var config = GetFakeConfig();

            using var realm = GetRealm(config);
            var session = realm.GetSession();
            CleanupOnTearDown(session);

            Assert.That(session.User, Is.EqualTo(config.User));
        }

        [Test]
        [Obsolete("tests obsolete functionality")]
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

        [Test]
        public void Session_Error_ShouldPassCorrectSession()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var config = GetFakeConfig();
                using var realm = GetRealm(config);
                var session = GetSession(realm);

                const ErrorCode code = (ErrorCode)102;
                const string message = "Some fake error has occurred";

                var error = await SyncTestHelpers.SimulateSessionErrorAsync<SessionException>(session, code, message, errorSession =>
                {
                    Assert.That(errorSession, Is.EqualTo(session));
                });

                Assert.That(error.Message, Is.EqualTo(message));
                Assert.That(error.ErrorCode, Is.EqualTo(code));
            });
        }

        [Test]
        public void Session_ClientReset_DiscardLocal_OnBefore_And_OnAfter()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var onBeforeTriggered = false;
                var onAfterTriggered = false;
                var tcs = new TaskCompletionSource<bool>();
                var config = await GetIntegrationConfigAsync();

                config.ClientResetHandler = new DiscardLocalResetHandler
                {
                    OnBeforeReset = (beforeFrozen) =>
                    {
                        Assert.IsFalse(onBeforeTriggered);
                        Assert.IsFalse(onAfterTriggered);
                        onBeforeTriggered = true;
                    },
                    OnAfterReset = (beforeFrozen, after) =>
                    {
                        Assert.IsTrue(onBeforeTriggered);
                        Assert.IsFalse(onAfterTriggered);
                        onAfterTriggered = true;
                        tcs.TrySetResult(true);
                    }
                };

                using var realm = await GetRealmAsync(config);

                GetSession(realm).SimulateClientReset("simulated client reset");

                await tcs.Task;

                Assert.IsTrue(onBeforeTriggered);
                Assert.IsTrue(onAfterTriggered);
            });
        }

        [Test]
        public void Session_ClientReset_DiscardLocal_ManualResetFallback_AutoClientReset()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var onBeforeTriggered = false;
                var onAfterTriggered = false;
                var manualResetFallbackHandled = false;
                var config = await GetIntegrationConfigAsync();
                var tcs = new TaskCompletionSource<bool>();

                config.ClientResetHandler = new DiscardLocalResetHandler
                {
                    OnBeforeReset = (beforeFrozen) =>
                    {
                        onBeforeTriggered = true;
                    },
                    OnAfterReset = (beforeFrozen, after) =>
                    {
                        onAfterTriggered = true;
                        tcs.TrySetResult(true);
                    },
                    ManualResetFallback = (session, err) =>
                    {
                        Assert.IsInstanceOf<ClientResetException>(err);
                        Assert.IsFalse(onBeforeTriggered);
                        Assert.IsFalse(onAfterTriggered);
                        Assert.IsFalse(manualResetFallbackHandled);
                        manualResetFallbackHandled = true;
                        tcs.TrySetResult(true);
                    }
                };

                using var realm = await GetRealmAsync(config);

                GetSession(realm).SimulateError(ClientError.AutoClientResetFailed, "simulated client reset failure");

                await tcs.Task;

                Assert.IsFalse(onBeforeTriggered);
                Assert.IsFalse(onAfterTriggered);
                Assert.IsTrue(manualResetFallbackHandled);
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
                config.ClientResetHandler = new ManualRecoveryHandler((sender, e) =>
                {
                    manualOnClientResetTriggered = true;
                    errorTcs.TrySetResult(e);
                });

                using (var realm = await GetRealmAsync(config))
                {
                    GetSession(realm).SimulateClientReset(errorMsg);
                }

                var clientEx = await errorTcs.Task;

                Assert.IsTrue(manualOnClientResetTriggered);

                Assert.That(clientEx.ErrorCode, Is.EqualTo(ErrorCode.DivergingHistories));
                Assert.That(clientEx.Message == errorMsg);
                Assert.That(clientEx.InnerException == null);

                Assert.That(File.Exists(config.DatabasePath));
                Assert.IsTrue(clientEx.InitiateClientReset());
                Assert.IsFalse(File.Exists(config.DatabasePath));
            });
        }

        [Test]
        public void Session_ClientReset_DiscardLocal_ManualResetFallback_InitiateClientReset()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var manualResetFallbackHandled = false;
                var errorTcs = new TaskCompletionSource<ClientResetException>();
                var config = await GetIntegrationConfigAsync();
                config.ClientResetHandler = new DiscardLocalResetHandler
                {
                    ManualResetFallback = (session, err) =>
                    {
                        manualResetFallbackHandled = true;
                        errorTcs.TrySetResult(err);
                    }
                };

                using (var realm = await GetRealmAsync(config))
                {
                    GetSession(realm).SimulateError(ClientError.AutoClientResetFailed, "simulated client reset failure");
                }

                var clientEx = await errorTcs.Task;

                Assert.IsTrue(manualResetFallbackHandled);
                Assert.That((int)clientEx.ErrorCode, Is.EqualTo((int)ClientError.AutoClientResetFailed));
                Assert.That(File.Exists(config.DatabasePath));
                Assert.IsTrue(clientEx.InitiateClientReset());
                Assert.IsFalse(File.Exists(config.DatabasePath));
            });
        }

        [Test]
        public void Session_ClientReset_Access_BeforeFrozen_OnBeforeReset()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var intId = new Random().Next(0, 1000);
                var tcs = new TaskCompletionSource<bool>();
                var onBeforeTriggered = false;
                var config = await GetIntegrationConfigAsync();
                config.ClientResetHandler = new DiscardLocalResetHandler
                {
                    OnBeforeReset = (beforeFrozen) =>
                    {
                        Assert.IsFalse(onBeforeTriggered);
                        var frozenObj = beforeFrozen.All<PrimaryKeyInt32Object>().First();
                        Assert.That(frozenObj.Id == intId);
                        onBeforeTriggered = true;
                        tcs.TrySetResult(true);
                    }
                };
                config.Schema = new[] { typeof(PrimaryKeyInt32Object) };

                using var realmY = await GetRealmAsync(config);
                realmY.Write(() =>
                {
                    realmY.Add(new PrimaryKeyInt32Object { Id = intId });
                });
                await WaitForUploadAsync(realmY);
                GetSession(realmY).SimulateClientReset("simulated client reset failure");

                await tcs.Task;
                Assert.IsTrue(onBeforeTriggered);
            });
        }

        [Test]
        public void Session_ClientReset_Access_BeforeFrozen_OnAfterReset()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var intId = new Random().Next(0, 1000);
                var tcs = new TaskCompletionSource<bool>();
                var onAfterTriggered = false;
                var config = await GetIntegrationConfigAsync();
                config.ClientResetHandler = new DiscardLocalResetHandler
                {
                    OnAfterReset = (beforeFrozen, after) =>
                    {
                        Assert.IsFalse(onAfterTriggered);
                        var frozenObj = beforeFrozen.All<PrimaryKeyInt32Object>().First();
                        Assert.That(frozenObj.Id == intId);
                        onAfterTriggered = true;
                        tcs.TrySetResult(true);
                    }
                };
                config.Schema = new[] { typeof(PrimaryKeyInt32Object) };

                using var realm = await GetRealmAsync(config);

                realm.Write(() =>
                {
                    realm.Add(new PrimaryKeyInt32Object { Id = intId });
                });

                var objs = realm.All<PrimaryKeyInt32Object>();
                Assert.That(objs.Any(), Is.True);
                Assert.That(objs.First().Id, Is.EqualTo(intId));

                await WaitForUploadAsync(realm);
                GetSession(realm).SimulateClientReset("simulated client reset failure");

                await tcs.Task;
                Assert.IsTrue(onAfterTriggered);
            });
        }

        [Test]
        public void Session_ClientReset_Access_After_OnAfterReset()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var intId = new Random().Next(0, 1000);
                var tcs = new TaskCompletionSource<bool>();
                var onAfterTriggered = false;
                var config = await GetIntegrationConfigAsync();
                config.ClientResetHandler = new DiscardLocalResetHandler
                {
                    OnAfterReset = (beforeFrozen, after) =>
                    {
                        Assert.IsFalse(onAfterTriggered);
                        var afterObj = after.All<PrimaryKeyInt32Object>().First();
                        Assert.That(afterObj.Id == intId);
                        onAfterTriggered = true;
                        tcs.TrySetResult(true);
                    }
                };
                config.Schema = new[] { typeof(PrimaryKeyInt32Object) };

                using var realm = await GetRealmAsync(config);
                realm.Write(() =>
                {
                    realm.Add(new PrimaryKeyInt32Object { Id = intId });
                });
                await WaitForUploadAsync(realm);
                GetSession(realm).SimulateClientReset("simulated client reset failure");

                await tcs.Task;
                Assert.IsTrue(onAfterTriggered);
            });
        }

        /* INTEGRATION TEST: By opening the same realm with different users
         * this test triggers a client reset to verify that the default behaviour of a client reset is DiscardLocalChanges.
         */
        [Test]
        public void Session_ClientReset_DefaultsTo_DiscardLocalHandler()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var pkSync = Guid.NewGuid().ToString();
                var pkNoSync = Guid.NewGuid().ToString();
                var objValueSync = "mon amour";
                var objValueNoSync = "it's the love";
                var partition = Guid.NewGuid().ToString();
                var app = CreateApp();
                var configA = await GetIntegrationConfigAsync(partition, app);
                var configB = await GetIntegrationConfigAsync(partition, app, configA.DatabasePath);

                using (var realmA = await GetRealmAsync(configA))
                {
                    var objToAddSync = new PrimaryKeyStringObject { Id = pkSync, Value = objValueSync };
                    var objToAddNoSync = new PrimaryKeyStringObject { Id = pkNoSync, Value = objValueNoSync };
                    realmA.Write(() =>
                    {
                        realmA.Add(objToAddSync);
                    });

                    await WaitForUploadAsync(realmA);

                    realmA.GetSession().Stop();

                    realmA.Write(() =>
                    {
                        realmA.Add(objToAddNoSync);
                    });

                    var allObjsA = realmA.All<PrimaryKeyStringObject>().ToArray();
                    Assert.That(allObjsA.Length, Is.EqualTo(2));
                    Assert.That(allObjsA[0], Is.EqualTo(objToAddSync));
                    Assert.That(allObjsA[1], Is.EqualTo(objToAddNoSync));
                }

                using var realmB = await GetRealmAsync(configB);
                await WaitForDownloadAsync(realmB);

                var allObjsB = realmB.All<PrimaryKeyStringObject>().ToArray();
                Assert.That(allObjsB.Length, Is.EqualTo(1));
                Assert.That(allObjsB[0].Id, Is.EqualTo(pkSync));
                Assert.That(allObjsB[0].Value, Is.EqualTo(objValueSync));
            });
        }

        [Test]
        public void Session_ClientReset_DiscardLocal_ManualResetFallback_Exception_OnBefore()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var tcs = new TaskCompletionSource<bool>();
                var onBeforeTriggered = false;
                var manualFallbackTriggered = false;
                var onAfterResetTriggered = false;
                var config = await GetIntegrationConfigAsync();
                config.ClientResetHandler = new DiscardLocalResetHandler
                {
                    OnBeforeReset = (beforeFrozen) =>
                    {
                        Assert.IsFalse(onBeforeTriggered);
                        Assert.IsFalse(onAfterResetTriggered);
                        Assert.IsFalse(manualFallbackTriggered);
                        onBeforeTriggered = true;
                        throw new Exception("Exception thrown in OnBeforeReset");
                    },
                    OnAfterReset = (beforeFrozen, after) =>
                    {
                        onAfterResetTriggered = true;
                        tcs.TrySetResult(true);
                    },
                    ManualResetFallback = (session, err) =>
                    {
                        Assert.IsInstanceOf<ClientResetException>(err);
                        Assert.IsTrue(onBeforeTriggered);
                        Assert.IsFalse(onAfterResetTriggered);
                        Assert.IsFalse(manualFallbackTriggered);
                        manualFallbackTriggered = true;
                        tcs.TrySetResult(true);
                    }
                };

                using var realm = await GetRealmAsync(config);

                GetSession(realm).SimulateClientReset("simulated client reset");

                await tcs.Task;

                Assert.IsTrue(manualFallbackTriggered);
                Assert.IsTrue(onBeforeTriggered);
                Assert.IsFalse(onAfterResetTriggered);
            });
        }

        [Test]
        public void Session_ClientReset_DiscardLocal_ManualResetFallback_Exception_OnAfter()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var tcs = new TaskCompletionSource<bool>();
                var onBeforeTriggered = false;
                var manualFallbackTriggered = false;
                var onAfterResetTriggered = false;
                var config = await GetIntegrationConfigAsync();
                config.ClientResetHandler = new DiscardLocalResetHandler
                {
                    OnBeforeReset = (beforeFrozen) =>
                    {
                        Assert.IsFalse(onBeforeTriggered);
                        Assert.IsFalse(onAfterResetTriggered);
                        Assert.IsFalse(manualFallbackTriggered);
                        onBeforeTriggered = true;
                    },
                    OnAfterReset = (beforeFrozen, after) =>
                    {
                        Assert.IsTrue(onBeforeTriggered);
                        Assert.IsFalse(onAfterResetTriggered);
                        Assert.IsFalse(manualFallbackTriggered);
                        onAfterResetTriggered = true;
                        throw new Exception("Exception thrown in OnAfterReset");
                    },
                    ManualResetFallback = (session, err) =>
                    {
                        Assert.IsInstanceOf<ClientResetException>(err);
                        Assert.IsTrue(onBeforeTriggered);
                        Assert.IsTrue(onAfterResetTriggered);
                        Assert.IsFalse(manualFallbackTriggered);
                        manualFallbackTriggered = true;
                        tcs.TrySetResult(true);
                    }
                };

                using var realm = await GetRealmAsync(config);

                GetSession(realm).SimulateClientReset("simulated client reset");

                await tcs.Task;

                Assert.IsTrue(manualFallbackTriggered);
                Assert.IsTrue(onBeforeTriggered);
                Assert.IsTrue(onAfterResetTriggered);
            });
        }

        [Test]
        public void Session_Error_OnSessionError()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var sessionErrorTriggered = false;
                var tcs = new TaskCompletionSource<bool>();
                var config = await GetIntegrationConfigAsync();
                var errorMsg = "simulated sync issue";
                config.OnSessionError = (sender, e) =>
                {
                    Assert.IsInstanceOf<Session>(sender);
                    Assert.IsInstanceOf<SessionException>(e);
                    Assert.That(e.ErrorCode == ErrorCode.PermissionDenied);
                    Assert.That(e.Message == errorMsg);
                    Assert.That(e.InnerException == null);
                    Assert.IsFalse(sessionErrorTriggered);
                    sessionErrorTriggered = true;
                    tcs.TrySetResult(true);
                };

                using var realm = await GetRealmAsync(config);
                var session = GetSession(realm);
                session.SimulateError(ErrorCode.PermissionDenied, errorMsg);

                await tcs.Task;

                Assert.IsTrue(sessionErrorTriggered);
            });
        }

        // after deprecation: temporary test for co-existence of Session.Error and error handling in SyncConfigurationBase
        [Test]
        public void Session_ClientReset_DiscardLocal_Coexistence()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var onBeforeTriggered = false;
                var onAfterTriggered = false;
                var obsoleteSessionErrorTriggered = false;
                var tcs = new TaskCompletionSource<bool>();
                var config = await GetIntegrationConfigAsync();
                config.ClientResetHandler = new DiscardLocalResetHandler
                {
                    OnBeforeReset = (beforeFrozen) =>
                    {
                        Assert.IsFalse(onBeforeTriggered);
                        Assert.IsFalse(onAfterTriggered);
                        onBeforeTriggered = true;
                    },
                    OnAfterReset = (beforeFrozen, after) =>
                    {
                        Assert.IsTrue(onBeforeTriggered);
                        Assert.IsFalse(onAfterTriggered);
                        onAfterTriggered = true;
                        tcs.TrySetResult(true);
                    }
                };

                using var realm = await GetRealmAsync(config);
                var session = GetSession(realm);

                EventHandler<ErrorEventArgs> sessionErrorFunc = (sender, e) =>
                {
                    Assert.That(obsoleteSessionErrorTriggered, Is.False);
                    obsoleteSessionErrorTriggered = true;
                    tcs.TrySetResult(true);
                };

                // priority is given to the newer appoach in SyncConfigurationBase, so this should never be reached
                Session.Error += sessionErrorFunc;

                session.SimulateClientReset("simulated client reset");

                // to avoid a race condition where e.g. both methods are called but because of timing differences `tcs.TrySetResult(true);` is reached
                // earlier in a call not letting the other finish to run. This would hide an issue.
                await Task.Delay(1000);
                await tcs.Task;

                Assert.IsTrue(onBeforeTriggered);
                Assert.IsTrue(onAfterTriggered);
                Assert.IsFalse(obsoleteSessionErrorTriggered);
                Session.Error -= sessionErrorFunc;
            });
        }

        // after deprecation: temporary test for co-existence of Session.Error and error handling in SyncConfigurationBase
        [Test]
        public void Session_ClientReset_DiscardLocal_ManualResetFallback_Coexistence()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var manualResetFallbackHandled = false;
                var obsoleteSessionErrorTriggered = false;
                var errorMsg = "simulated client reset failure";
                var tcs = new TaskCompletionSource<bool>();
                var config = await GetIntegrationConfigAsync();
                config.ClientResetHandler = new DiscardLocalResetHandler
                {
                    ManualResetFallback = (session, err) =>
                    {
                        Assert.IsFalse(manualResetFallbackHandled);
                        Assert.IsInstanceOf<ClientResetException>(err);

                        Assert.That((int)err.ErrorCode, Is.EqualTo((int)ClientError.AutoClientResetFailed));
                        Assert.That(err.Message == errorMsg);
                        Assert.That(err.InnerException == null);
                        manualResetFallbackHandled = true;
                        tcs.TrySetResult(true);
                    }
                };

                using var realm = await GetRealmAsync(config);

                EventHandler<ErrorEventArgs> sessionErrorFunc = (sender, e) =>
                {
                    Assert.That(obsoleteSessionErrorTriggered, Is.False);
                    obsoleteSessionErrorTriggered = true;
                    tcs.TrySetResult(true);
                };

                // priority is given to the newer appoach in SyncConfigurationBase, so this should never be reached
                Session.Error += sessionErrorFunc;

                GetSession(realm).SimulateError(ClientError.AutoClientResetFailed, errorMsg);

                // to avoid a race condition where e.g. both methods are called but because of timing differences `tcs.TrySetResult(true);` is reached
                // earlier in a call not letting the other finish to run. This would hide an issue.
                await Task.Delay(1000);
                await tcs.Task;

                Assert.IsTrue(manualResetFallbackHandled);
                Assert.IsFalse(obsoleteSessionErrorTriggered);
                Session.Error -= sessionErrorFunc;
            });
        }

        // after deprecation: temporary test for co-existence of Session.Error and error handling in SyncConfigurationBase
        [Test]
        public void Session_ClientReset_OldSessionError_InitiateClientReset_Coexistence()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var obsoleteSessionErrorTriggered = false;
                var tcs = new TaskCompletionSource<Exception>();
                var config = await GetIntegrationConfigAsync();
                config.ClientResetHandler = new ManualRecoveryHandler();
                var errorMsg = "simulated sync issue";

                EventHandler<ErrorEventArgs> sessionErrorFunc = (sender, e) =>
                {
                    Assert.That(obsoleteSessionErrorTriggered, Is.False);
                    obsoleteSessionErrorTriggered = true;
                    tcs.TrySetResult(e.Exception);
                };

                using (var realm = await GetRealmAsync(config))
                {
                    var session = GetSession(realm);

                    // Session.Error is set after obtaining a realm as it truly tests coexistence given that
                    // the resynce mode is set at creation of the configuration.
                    // SyncConfigurationBase.CreateNativeSyncConfiguration.
                    Session.Error += sessionErrorFunc;

                    session.SimulateClientReset(errorMsg);
                }

                var ex = await tcs.Task;

                Assert.IsTrue(obsoleteSessionErrorTriggered);
                Session.Error -= sessionErrorFunc;

                Assert.That(ex, Is.InstanceOf<ClientResetException>());
                var clientEx = (ClientResetException)ex;
                Assert.That(clientEx.ErrorCode, Is.EqualTo(ErrorCode.DivergingHistories));
                Assert.That(clientEx.Message == errorMsg);
                Assert.That(clientEx.InnerException == null);

                Assert.That(File.Exists(config.DatabasePath));
                Assert.IsTrue(clientEx.InitiateClientReset());
                Assert.IsFalse(File.Exists(config.DatabasePath));
            });
        }

        // after deprecation: temporary test for co-existence of Session.Error and error handling in SyncConfigurationBase
        [Test]
        public void Session_Error_OldSessionError_Coexistence()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var obsoleteSessionErrorTriggered = false;
                var tcs = new TaskCompletionSource<bool>();
                var config = await GetIntegrationConfigAsync();
                var errorMsg = "simulated sync issue";

                using var realm = await GetRealmAsync(config);
                var session = GetSession(realm);

                EventHandler<ErrorEventArgs> sessionErrorFunc = (sender, e) =>
                {
                    Assert.IsInstanceOf<Session>(sender);
                    Assert.IsInstanceOf<SessionException>(e.Exception);
                    var sessionEx = (SessionException)e.Exception;
                    Assert.That(sessionEx.ErrorCode == ErrorCode.PermissionDenied);
                    Assert.That(sessionEx.Message == errorMsg);
                    Assert.That(sessionEx.InnerException == null);
                    Assert.That(obsoleteSessionErrorTriggered, Is.False);
                    obsoleteSessionErrorTriggered = true;
                    tcs.TrySetResult(true);
                };

                // priority is given to the newer appoach in SyncConfigurationBase, so this should never be reached
                Session.Error += sessionErrorFunc;

                session.SimulateError(ErrorCode.PermissionDenied, "simulated sync issue");

                await tcs.Task;
                Assert.IsTrue(obsoleteSessionErrorTriggered);
                Session.Error -= sessionErrorFunc;
            });
        }

        // after deprecation: temporary test for co-existence of Session.Error and error handling in SyncConfigurationBase
        [Test]
        public void Session_ClientReset_ManualRecovery_Coexistence()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var manualOnClientResetTriggered = false;
                var obsoleteSessionErrorTriggered = false;
                var tcs = new TaskCompletionSource<bool>();
                var config = await GetIntegrationConfigAsync();
                config.ClientResetHandler = new ManualRecoveryHandler((sender, e) =>
                {
                    Assert.That(manualOnClientResetTriggered, Is.False);
                    manualOnClientResetTriggered = true;
                    tcs.TrySetResult(true);
                });

                using var realm = await GetRealmAsync(config);
                var session = GetSession(realm);

                EventHandler<ErrorEventArgs> sessionErrorFunc = (sender, e) =>
                {
                    Assert.That(obsoleteSessionErrorTriggered, Is.False);
                    obsoleteSessionErrorTriggered = true;
                    tcs.TrySetResult(true);
                };

                // priority is given to the newer appoach in SyncConfigurationBase, so this should never be reached
                Session.Error += sessionErrorFunc;

                session.SimulateClientReset("simulated client reset");

                // to avoid a race condition where e.g. both methods are called but because of timing differences `tcs.TrySetResult(true);` is reached
                // earlier in a call not letting the other finish to run. This would hide an issue.
                await Task.Delay(1000);
                await tcs.Task;

                Assert.That(manualOnClientResetTriggered, Is.True);
                Assert.That(obsoleteSessionErrorTriggered, Is.False);
                Session.Error -= sessionErrorFunc;
            });
        }

        // after deprecation: temporary test for co-existence of Session.Error and error handling in SyncConfigurationBase
        [Test]
        public void Session_Error_OnSessionError_Coexistence()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var obsoleteSessionErrorTriggered = false;
                var sessionErrorTriggered = false;
                var tcs = new TaskCompletionSource<bool>();
                var config = await GetIntegrationConfigAsync();
                config.OnSessionError = (sender, e) =>
                {
                    Assert.IsInstanceOf<Session>(sender);
                    Assert.IsInstanceOf<SessionException>(e);
                    Assert.IsFalse(sessionErrorTriggered);
                    sessionErrorTriggered = true;
                    tcs.TrySetResult(true);
                };

                using var realm = await GetRealmAsync(config);
                var session = GetSession(realm);

                EventHandler<ErrorEventArgs> sessionErrorFunc = (sender, e) =>
                {
                    Assert.That(obsoleteSessionErrorTriggered, Is.False);
                    obsoleteSessionErrorTriggered = true;
                    tcs.TrySetResult(true);
                };

                // priority is given to the newer appoach in SyncConfigurationBase, so this should never be reached
                Session.Error += sessionErrorFunc;

                session.SimulateError(ErrorCode.PermissionDenied, "simulated sync issue");

                // to avoid a race condition where e.g. both methods are called but because of timing differences `tcs.TrySetResult(true);` is reached
                // earlier in a call not letting the other finish to run. This would hide an issue.
                await Task.Delay(1000);
                await tcs.Task;

                Assert.IsFalse(obsoleteSessionErrorTriggered);
                Assert.IsTrue(sessionErrorTriggered);
                Session.Error -= sessionErrorFunc;
            });
        }

        [Test]
        public void Test_SyncConfigRelease()
        {
            WeakReference weakConfigRef = null;
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                weakConfigRef = new WeakReference(await GetIntegrationConfigAsync());
                using var realm = await GetRealmAsync((PartitionSyncConfiguration)weakConfigRef.Target);
                var session = GetSession(realm);
                Assert.IsNotNull(weakConfigRef.Target);
            });
            TearDown();
            GC.Collect();
            Assert.IsNull(weakConfigRef.Target);
        }

        [TestCase(ProgressMode.ForCurrentlyOutstandingWork)]
        [TestCase(ProgressMode.ReportIndefinitely)]
        public void Session_ProgressObservable_IntegrationTests(ProgressMode mode)
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
    }
}
