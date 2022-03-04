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
using System.Collections.Specialized;
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
    [TestFixture, Preserve(AllMembers = true)]
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

        [Test, Obsolete("Tests Session.Error")]
        public void Session_Error_ShouldPassCorrectSession()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                Session session = null;
                var tcs = new TaskCompletionSource<object>();
                Session.Error += onSessionError;

                var config = GetFakeConfig();
                using var realm = GetRealm(config);
                session = GetSession(realm);

                const ErrorCode code = (ErrorCode)102;
                const string message = "Some fake error has occurred";

                session.SimulateError(code, message);

                await tcs.Task;

                Session.Error -= onSessionError;

                void onSessionError(object sender, ErrorEventArgs args)
                {
                    try
                    {
                        // We're not validating the actual exception because there's no guarantee
                        // that we're seeing the same one we raised in SimulateError. It's possible
                        // that sync tried to establish a connection and failed because this is a
                        // unit test, not an integration one, so the connection should be rejected
                        // immediately.
                        Assert.That(sender, Is.EqualTo(session));
                        tcs.TrySetResult(null);
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                }
            });
        }

        [Test]
        public void Session_ClientReset_DiscardLocal_OnBefore_And_OnAfter()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var onBeforeTriggered = false;
                var onAfterTriggered = false;
                var tcs = new TaskCompletionSource<object>();
                var config = await GetIntegrationConfigAsync();

                config.ClientResetHandler = new DiscardLocalResetHandler
                {
                    OnBeforeReset = GetOnBeforeHandler(tcs, beforeFrozen =>
                    {
                        Assert.That(onBeforeTriggered, Is.False);
                        Assert.That(onAfterTriggered, Is.False);
                        onBeforeTriggered = true;
                    }),
                    OnAfterReset = GetOnAfterHandler(tcs, (beforeFrozen, after) =>
                    {
                        Assert.That(onBeforeTriggered, Is.True);
                        Assert.That(onAfterTriggered, Is.False);
                        onAfterTriggered = true;
                    })
                };

                using var realm = await GetRealmAsync(config);

                await WorkaroundREALMC12062();

                GetSession(realm).SimulateClientReset("simulated client reset");

                await tcs.Task;

                Assert.That(onBeforeTriggered, Is.True);
                Assert.That(onAfterTriggered, Is.True);
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
                var tcs = new TaskCompletionSource<object>();

                config.ClientResetHandler = new DiscardLocalResetHandler
                {
                    OnBeforeReset = GetOnBeforeHandler(tcs, beforeFrozen =>
                    {
                        onBeforeTriggered = true;
                    }),
                    OnAfterReset = GetOnAfterHandler(tcs, (beforeFrozen, after) =>
                    {
                        onAfterTriggered = true;
                    }),
                    ManualResetFallback = (err) =>
                    {
                        Assert.That(err, Is.InstanceOf<ClientResetException>());
                        Assert.That(onBeforeTriggered, Is.False);
                        Assert.That(onAfterTriggered, Is.False);
                        Assert.That(manualResetFallbackHandled, Is.False);
                        manualResetFallbackHandled = true;
                        tcs.TrySetResult(true);
                    }
                };

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
                    ManualResetFallback = (err) =>
                    {
                        manualResetFallbackHandled = true;
                        errorTcs.TrySetResult(err);
                    }
                };

                using (var realm = await GetRealmAsync(config))
                {
                    GetSession(realm).SimulateAutomaticClientResetFailure("simulated client reset failure");
                }

                var clientEx = await errorTcs.Task;

                Assert.That(manualResetFallbackHandled, Is.True);

                await TryInitiateClientReset(clientEx, (int)ClientError.AutoClientResetFailed, config);
            });
        }

        [Test]
        public void Session_ClientReset_Access_Realm_OnBeforeReset()
        {
            const string validValue = "this will sync";
            const string invalidValue = "this will be deleted";

            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var tcs = new TaskCompletionSource<object>();
                var onBeforeTriggered = false;
                var config = await GetIntegrationConfigAsync();
                config.ClientResetHandler = new DiscardLocalResetHandler
                {
                    OnBeforeReset = GetOnBeforeHandler(tcs, beforeFrozen =>
                    {
                        Assert.That(onBeforeTriggered, Is.False);

                        var frozenObjs = beforeFrozen.All<ObjectWithPartitionValue>().ToArray();
                        Assert.That(frozenObjs.Length, Is.EqualTo(2));
                        Assert.That(frozenObjs.Select(o => o.Value), Is.EquivalentTo(new[] { validValue, invalidValue }));

                        onBeforeTriggered = true;
                        tcs.TrySetResult(null);
                    })
                };
                config.Schema = new[] { typeof(ObjectWithPartitionValue) };

                using var realm = await GetRealmAsync(config);

                await WorkaroundREALMC12062();

                realm.Write(() =>
                {
                    realm.Add(new ObjectWithPartitionValue
                    {
                        Id = Guid.NewGuid().ToString(),
                        Value = validValue
                    });
                });

                await WaitForUploadAsync(realm);

                realm.Write(() =>
                {
                    realm.Add(new ObjectWithPartitionValue
                    {
                        Id = Guid.NewGuid().ToString(),
                        Value = invalidValue,
                        Partition = "nonexistent"
                    });
                });

                var objs = realm.All<ObjectWithPartitionValue>();
                Assert.That(objs.Count(), Is.EqualTo(2));
                Assert.That(objs.ToArray().Select(o => o.Value), Is.EquivalentTo(new[] { validValue, invalidValue }));

                await tcs.Task;
                Assert.That(onBeforeTriggered, Is.True);

                await TestHelpers.WaitForConditionAsync(() => objs.Count() == 1);

                Assert.That(objs.Single().Value, Is.EqualTo(validValue));
            });
        }

        [Test]
        public void Session_ClientReset_Access_Realms_OnAfterReset()
        {
            const string validValue = "this will sync";
            const string invalidValue = "this will be deleted";

            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var tcs = new TaskCompletionSource<object>();
                var onAfterTriggered = false;
                var config = await GetIntegrationConfigAsync();
                config.ClientResetHandler = new DiscardLocalResetHandler
                {
                    OnAfterReset = GetOnAfterHandler(tcs, (beforeFrozen, after) =>
                    {
                        Assert.That(onAfterTriggered, Is.False);

                        var frozenObjs = beforeFrozen.All<ObjectWithPartitionValue>().ToArray();
                        Assert.That(frozenObjs.Length, Is.EqualTo(2));
                        Assert.That(frozenObjs.Select(o => o.Value), Is.EquivalentTo(new[] { validValue, invalidValue }));

                        var objs = after.All<ObjectWithPartitionValue>();
                        Assert.That(objs.Count(), Is.EqualTo(1));
                        Assert.That(objs.Single().Value, Is.EqualTo(validValue));

                        onAfterTriggered = true;
                    })
                };
                config.Schema = new[] { typeof(ObjectWithPartitionValue) };

                using var realm = await GetRealmAsync(config);

                await WorkaroundREALMC12062();

                realm.Write(() =>
                {
                    realm.Add(new ObjectWithPartitionValue
                    {
                        Id = Guid.NewGuid().ToString(),
                        Value = validValue
                    });
                });

                await WaitForUploadAsync(realm);

                realm.Write(() =>
                {
                    realm.Add(new ObjectWithPartitionValue
                    {
                        Id = Guid.NewGuid().ToString(),
                        Value = invalidValue,
                        Partition = "nonexistent"
                    });
                });

                var objs = realm.All<ObjectWithPartitionValue>();
                Assert.That(objs.Count(), Is.EqualTo(2));
                Assert.That(objs.ToArray().Select(o => o.Value), Is.EquivalentTo(new[] { validValue, invalidValue }));

                await tcs.Task;
                Assert.That(onAfterTriggered, Is.True);

                realm.Refresh();

                Assert.That(objs.Count(), Is.EqualTo(1));
                Assert.That(objs.Single().Value, Is.EqualTo(validValue));
            });
        }

        // [Ignore("This relies on ProtocolError::bad_changeset being a client reset error. We're waiting on core to implement this change.")]
        [Test]
        public void Session_DiscardLocalReset_TriggersNotifications()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                // We'll add an object with the wrong partition
                var config = await GetIntegrationConfigAsync();
                config.Schema = new[] { typeof(ObjectWithPartitionValue) };

                using var realm = await GetRealmAsync(config);

                await WorkaroundREALMC12062();

                realm.Write(() =>
                {
                    realm.Add(new ObjectWithPartitionValue
                    {
                        Id = Guid.NewGuid().ToString(),
                        Value = "this will sync"
                    });
                });

                await WaitForUploadAsync(realm);

                // We're adding an object with the same Id in a different partition - Sync should reject this.
                realm.Write(() =>
                {
                    realm.Add(new ObjectWithPartitionValue
                    {
                        Id = Guid.NewGuid().ToString(),
                        Partition = "not-real-partition",
                        Value = "this should be discarded"
                    });
                });

                var objects = realm.All<ObjectWithPartitionValue>().AsRealmCollection();
                Assert.That(objects.Count, Is.EqualTo(2));

                var tcs = new TaskCompletionSource<NotifyCollectionChangedEventArgs>();
                objects.CollectionChanged += onCollectionChanged;

                var args = await tcs.Task;

                Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));
                Assert.That(objects.Count, Is.EqualTo(1));
                Assert.That(objects.Single().Value, Is.EqualTo("this will sync"));

                objects.CollectionChanged -= onCollectionChanged;

                void onCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
                {
                    tcs.TrySetResult(args);
                }
            }, timeout: 120_000);
        }

        [Test]
        public void Session_ClientReset_DiscardLocal_ManualResetFallback_Exception_OnBefore()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var tcs = new TaskCompletionSource<object>();
                var onBeforeTriggered = false;
                var manualFallbackTriggered = false;
                var onAfterResetTriggered = false;
                var config = await GetIntegrationConfigAsync();
                config.ClientResetHandler = new DiscardLocalResetHandler
                {
                    OnBeforeReset = beforeFrozen =>
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
                    },
                    OnAfterReset = GetOnAfterHandler(tcs, (beforeFrozen, after) =>
                    {
                        onAfterResetTriggered = true;
                    }),
                    ManualResetFallback = GetClientResetHandler(tcs, (ex) =>
                    {
                        Assert.That(ex, Is.InstanceOf<ClientResetException>());
                        Assert.That(onBeforeTriggered, Is.True);
                        Assert.That(onAfterResetTriggered, Is.False);
                        Assert.That(manualFallbackTriggered, Is.False);
                        manualFallbackTriggered = true;
                    })
                };

                using var realm = await GetRealmAsync(config);

                GetSession(realm).SimulateClientReset("simulated client reset");

                await tcs.Task;

                Assert.That(manualFallbackTriggered, Is.True);
                Assert.That(onBeforeTriggered, Is.True);
                Assert.That(onAfterResetTriggered, Is.False);
            });
        }

        [Test]
        public void Session_ClientReset_DiscardLocal_ManualResetFallback_Exception_OnAfter()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var tcs = new TaskCompletionSource<object>();
                var onBeforeTriggered = false;
                var manualFallbackTriggered = false;
                var onAfterResetTriggered = false;
                var config = await GetIntegrationConfigAsync();
                config.ClientResetHandler = new DiscardLocalResetHandler
                {
                    OnBeforeReset = GetOnBeforeHandler(tcs, beforeFrozen =>
                    {
                        Assert.That(onBeforeTriggered, Is.False);
                        Assert.That(onAfterResetTriggered, Is.False);
                        Assert.That(manualFallbackTriggered, Is.False);
                        onBeforeTriggered = true;
                    }),
                    OnAfterReset = (beforeFrozen, after) =>
                    {
                        Assert.That(onBeforeTriggered, Is.True);
                        Assert.That(onAfterResetTriggered, Is.False);
                        Assert.That(manualFallbackTriggered, Is.False);
                        onAfterResetTriggered = true;
                        throw new Exception("Exception thrown in OnAfterReset");
                    },
                    ManualResetFallback = GetClientResetHandler(tcs, (ex) =>
                    {
                        Assert.That(ex, Is.InstanceOf<ClientResetException>());
                        Assert.That(onBeforeTriggered, Is.True);
                        Assert.That(onAfterResetTriggered, Is.True);
                        Assert.That(manualFallbackTriggered, Is.False);
                        manualFallbackTriggered = true;
                    })
                };

                using var realm = await GetRealmAsync(config);

                await WorkaroundREALMC12062();

                GetSession(realm).SimulateClientReset("simulated client reset");

                await tcs.Task;

                Assert.That(manualFallbackTriggered, Is.True);
                Assert.That(onBeforeTriggered, Is.True);
                Assert.That(onAfterResetTriggered, Is.True);
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

        [Test, Obsolete("Testing Sesion.Error compatibility")]
        public void Session_ClientReset_DiscardLocal_Coexistence()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var onBeforeTriggered = false;
                var onAfterTriggered = false;
                var obsoleteSessionErrorTriggered = false;
                var tcs = new TaskCompletionSource<object>();
                var config = await GetIntegrationConfigAsync();
                config.ClientResetHandler = new DiscardLocalResetHandler
                {
                    OnBeforeReset = GetOnBeforeHandler(tcs, beforeFrozen =>
                    {
                        Assert.That(onBeforeTriggered, Is.False);
                        Assert.That(onAfterTriggered, Is.False);
                        onBeforeTriggered = true;
                    }),
                    OnAfterReset = GetOnAfterHandler(tcs, (beforeFrozen, after) =>
                    {
                        Assert.That(onBeforeTriggered, Is.True);
                        Assert.That(onAfterTriggered, Is.False);
                        onAfterTriggered = true;
                    })
                };

                // priority is given to the newer appoach in SyncConfigurationBase, so this should never be reached
                Session.Error += onSessionError;

                using var realm = await GetRealmAsync(config);
                var session = GetSession(realm);

                await WorkaroundREALMC12062();

                session.SimulateClientReset("simulated client reset");

                // to avoid a race condition where e.g. both methods are called but because of timing differences `tcs.TrySetResult(true);` is reached
                // earlier in a call not letting the other finish to run. This would hide an issue.
                await tcs.Task;
                await Task.Delay(1000);

                Assert.That(onBeforeTriggered, Is.True);
                Assert.That(onAfterTriggered, Is.True);
                Assert.That(obsoleteSessionErrorTriggered, Is.False);
                Session.Error -= onSessionError;

                void onSessionError(object sender, ErrorEventArgs e)
                {
                    Assert.That(obsoleteSessionErrorTriggered, Is.False);
                    obsoleteSessionErrorTriggered = true;
                    tcs.TrySetResult(null);
                }
            });
        }

        [Test, Obsolete("Testing Sesion.Error compatibility")]
        public void Session_WithDiscardLocalHandler_DoesntRaiseSessionError()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var obsoleteSessionErrorTriggered = false;
                var tcs = new TaskCompletionSource<object>();
                var config = await GetIntegrationConfigAsync();
                config.ClientResetHandler = new DiscardLocalResetHandler();

                using var realm = await GetRealmAsync(config);
                var session = GetSession(realm);

                // priority is given to the newer appoach in SyncConfigurationBase, so this should never be reached
                Session.Error += onSessionError;

                session.SimulateClientReset("simulated client reset");
                session.SimulateAutomaticClientResetFailure("failure #2");

                await Task.Delay(1000);

                Assert.That(obsoleteSessionErrorTriggered, Is.False);
                Session.Error -= onSessionError;

                void onSessionError(object sender, ErrorEventArgs e)
                {
                    obsoleteSessionErrorTriggered = true;
                }
            });
        }

        [Test, Obsolete("Testing Sesion.Error compatibility")]
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
                    ManualResetFallback = (err) =>
                    {
                        Assert.That(manualResetFallbackHandled, Is.False);
                        Assert.That(err, Is.InstanceOf<ClientResetException>());

                        Assert.That((int)err.ErrorCode, Is.EqualTo((int)ClientError.AutoClientResetFailed));
                        Assert.That(err.Message, Is.EqualTo(errorMsg));
                        Assert.That(err.InnerException, Is.Null);
                        manualResetFallbackHandled = true;
                        tcs.TrySetResult(true);
                    }
                };

                using var realm = await GetRealmAsync(config);

                // priority is given to the newer appoach in SyncConfigurationBase, so this should never be reached
                Session.Error += onSessionError;

                GetSession(realm).SimulateAutomaticClientResetFailure(errorMsg);

                // to avoid a race condition where e.g. both methods are called but because of timing differences `tcs.TrySetResult(true);` is reached
                // earlier in a call not letting the other finish to run. This would hide an issue.
                await tcs.Task;
                await Task.Delay(1000);

                Assert.That(manualResetFallbackHandled, Is.True);
                Assert.That(obsoleteSessionErrorTriggered, Is.False);
                Session.Error -= onSessionError;

                void onSessionError(object sender, ErrorEventArgs e)
                {
                    Assert.That(obsoleteSessionErrorTriggered, Is.False);
                    obsoleteSessionErrorTriggered = true;
                    tcs.TrySetResult(true);
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
                var errorMsg = "simulated sync issue";

                using (var realm = await GetRealmAsync(config))
                {
                    var session = GetSession(realm);

                    // Session.Error is set after obtaining a realm as it truly tests coexistence given that
                    // the resync mode is set at creation of the configuration.
                    // SyncConfigurationBase.CreateNativeSyncConfiguration.
                    Session.Error += onSessionError;

                    session.SimulateClientReset(errorMsg);
                }

                var ex = await tcs.Task;

                Assert.That(obsoleteSessionErrorTriggered, Is.True);
                Session.Error -= onSessionError;

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
                var tcs = new TaskCompletionSource<bool>();
                var config = await GetIntegrationConfigAsync();
                config.ClientResetHandler = new ManualRecoveryHandler();
                var errorMsg = "simulated sync issue";

                Session.Error += onSessionError;

                using var realm = await GetRealmAsync(config);

                await WorkaroundREALMC12062();

                var session = GetSession(realm);
                session.SimulateError(ErrorCode.PermissionDenied, "simulated sync issue");

                await tcs.Task;
                Assert.That(obsoleteSessionErrorTriggered, Is.True);
                Session.Error -= onSessionError;

                void onSessionError(object sender, ErrorEventArgs e)
                {
                    Assert.That(sender, Is.InstanceOf<Session>());
                    Assert.That(e.Exception, Is.InstanceOf<SessionException>());
                    var sessionEx = (SessionException)e.Exception;
                    Assert.That(sessionEx.ErrorCode == ErrorCode.PermissionDenied);
                    Assert.That(sessionEx.Message == errorMsg);
                    Assert.That(sessionEx.InnerException == null);
                    Assert.That(obsoleteSessionErrorTriggered, Is.False);
                    obsoleteSessionErrorTriggered = true;
                    tcs.TrySetResult(true);
                }
            });
        }

        [Test, Obsolete("Testing Sesion.Error compatibility")]
        public void Session_ClientReset_ManualRecovery_Coexistence()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var manualOnClientResetTriggered = false;
                var obsoleteSessionErrorTriggered = false;
                var tcs = new TaskCompletionSource<bool>();
                var config = await GetIntegrationConfigAsync();
                config.ClientResetHandler = new ManualRecoveryHandler((e) =>
                {
                    Assert.That(manualOnClientResetTriggered, Is.False);
                    manualOnClientResetTriggered = true;
                    tcs.TrySetResult(true);
                });

                using var realm = await GetRealmAsync(config);

                await WorkaroundREALMC12062();

                var session = GetSession(realm);

                // priority is given to the newer appoach in SyncConfigurationBase, so this should never be reached
                Session.Error += onSessionError;

                session.SimulateClientReset("simulated client reset");

                // to avoid a race condition where e.g. both methods are called but because of timing differences `tcs.TrySetResult(true);` is reached
                // earlier in a call not letting the other finish to run. This would hide an issue.
                await tcs.Task;
                await Task.Delay(1000);

                Assert.That(manualOnClientResetTriggered, Is.True);
                Assert.That(obsoleteSessionErrorTriggered, Is.False);
                Session.Error -= onSessionError;

                void onSessionError(object sender, ErrorEventArgs e)
                {
                    Assert.That(obsoleteSessionErrorTriggered, Is.False);
                    obsoleteSessionErrorTriggered = true;
                    tcs.TrySetResult(true);
                }
            });
        }

        [Test, Obsolete("Testing Sesion.Error compatibility")]
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
                    Assert.That(sender, Is.InstanceOf<Session>());
                    Assert.That(e, Is.InstanceOf<SessionException>());
                    Assert.That(sessionErrorTriggered, Is.False);
                    sessionErrorTriggered = true;
                    tcs.TrySetResult(true);
                };

                using var realm = await GetRealmAsync(config);
                var session = GetSession(realm);

                // priority is given to the newer appoach in SyncConfigurationBase, so this should never be reached
                Session.Error += onSessionError;

                session.SimulateError(ErrorCode.PermissionDenied, "simulated sync issue");

                // to avoid a race condition where e.g. both methods are called but because of timing differences `tcs.TrySetResult(true);` is reached
                // earlier in a call not letting the other finish to run. This would hide an issue.
                await tcs.Task;
                await Task.Delay(1000);

                Assert.That(obsoleteSessionErrorTriggered, Is.False);
                Assert.That(sessionErrorTriggered, Is.True);
                Session.Error -= onSessionError;

                void onSessionError(object sender, ErrorEventArgs e)
                {
                    Assert.That(obsoleteSessionErrorTriggered, Is.False);
                    obsoleteSessionErrorTriggered = true;
                    tcs.TrySetResult(true);
                }
            });
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

        private static DiscardLocalResetHandler.AfterResetCallback GetOnAfterHandler(TaskCompletionSource<object> tcs, Action<Realm, Realm> assertions)
        {
            return new DiscardLocalResetHandler.AfterResetCallback((frozen, live) =>
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

        private static DiscardLocalResetHandler.BeforeResetCallback GetOnBeforeHandler(TaskCompletionSource<object> tcs, Action<Realm> assertions)
        {
            return new DiscardLocalResetHandler.BeforeResetCallback(frozen =>
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

        private static ClientResetHandlerBase.ClientResetCallback GetClientResetHandler(TaskCompletionSource<object> tcs, Action<ClientResetException> assertions)
        {
            return new ClientResetHandlerBase.ClientResetCallback(clientResetException =>
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

        private static async Task WorkaroundREALMC12062()
        {
            // This is a hack for REALMC-12062 that delays writing to a Realm until
            // the initial schema instructions have been synthesized by the server
            await Task.Delay(5000);
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
    }
}
