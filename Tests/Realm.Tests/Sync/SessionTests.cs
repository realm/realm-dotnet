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
using Realms.Logging;
using Realms.Sync;
using Realms.Sync.ErrorHandling;
using Realms.Sync.Exceptions;
using Realms.Sync.Native;
using Realms.Sync.Testing;
using static Realms.Sync.ErrorHandling.ClientResetHandlerBase;

namespace Realms.Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class SessionTests : SyncTestBase
    {
        private readonly ConcurrentQueue<EventHandler<ErrorEventArgs>> _sessionErrorHandlers = new();

        private static IEnumerable<IClientResetHandler> _clientResetHandlers()
        {
            yield return new ClientResetHanlder<DiscardLocalResetHandler>();
            yield return new ClientResetHanlder<AutomaticRecoveryHandler>();
            yield return new ClientResetHanlder<AutomaticRecoveryOrDiscardLocalHandler>();
        }

        private static ClientResetHandlerBase[] _clientResetInstanceHandlers = new ClientResetHandlerBase[]
        {
            new DiscardLocalResetHandler(),
            new AutomaticRecoveryHandler(),
            new AutomaticRecoveryOrDiscardLocalHandler()
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

        [TestCaseSource(nameof(_clientResetHandlers))]
        public void Session_ClientResetHandlers_ManualResetFallback_AutoClientReset(IClientResetHandler handler)
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
                ClientResetCallback manualCb = (err) =>
                {
                    Assert.That(err, Is.InstanceOf<ClientResetException>());
                    Assert.That(onBeforeTriggered, Is.False);
                    Assert.That(onAfterTriggered, Is.False);
                    Assert.That(manualResetFallbackHandled, Is.False);
                    manualResetFallbackHandled = true;
                    tcs.TrySetResult(true);
                };

                config.ClientResetHandler = handler.BuildHandler(beforeCb, afterCb, manualCb);

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

        [TestCaseSource(nameof(_clientResetHandlers))]
        public void Session_ClientResetHandlers_ManualResetFallback_InitiateClientReset(IClientResetHandler handler)
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

                config.ClientResetHandler = handler.BuildHandler(null, null, manualCb);
                using (var realm = await GetRealmAsync(config))
                {
                    GetSession(realm).SimulateAutomaticClientResetFailure("simulated client reset failure");
                }

                var clientEx = await errorTcs.Task;

                Assert.That(manualResetFallbackHandled, Is.True);

                await TryInitiateClientReset(clientEx, (int)ClientError.AutoClientResetFailed, config);
            });
        }

        [TestCaseSource(nameof(_clientResetHandlers))]
        public void Session_ClientResetHandlers_OnBefore_And_OnAfter(IClientResetHandler handler)
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

                config.ClientResetHandler = handler.BuildHandler(beforeCb, afterCb);

                using var realm = await GetRealmAsync(config);

                GetSession(realm).SimulateClientReset("simulated client reset");

                await tcs.Task;

                Assert.That(onBeforeTriggered, Is.True);
                Assert.That(onAfterTriggered, Is.True);
            });
        }

        [TestCaseSource(nameof(_clientResetHandlers))]
        [NUnit.Framework.Explicit("Relies on ProtocolError::bad_changeset to be ClientReset Error")]
        public void Session_ClientResetHandlers_AccessRealm_OnBeforeReset(IClientResetHandler handler)
        {
            const string validValue = "this will sync";
            const string invalidValue = "this will be deleted";

            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var tcs = new TaskCompletionSource<object>();
                var onBeforeTriggered = false;
                var config = await GetIntegrationConfigAsync();
                var beforeCb = GetOnBeforeHandler(tcs, beforeFrozen =>
                {
                    Assert.That(onBeforeTriggered, Is.False);

                    var frozenObjs = beforeFrozen.All<ObjectWithPartitionValue>().ToArray();
                    Assert.That(frozenObjs.Length, Is.EqualTo(2));
                    Assert.That(frozenObjs.Select(o => o.Value), Is.EquivalentTo(new[] { validValue, invalidValue }));

                    onBeforeTriggered = true;
                    tcs.TrySetResult(null);
                });
                config.ClientResetHandler = handler.BuildHandler(beforeCb);
                config.Schema = new[] { typeof(ObjectWithPartitionValue) };

                using var realm = await GetRealmAsync(config);

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

        [TestCaseSource(nameof(_clientResetHandlers))]
        [NUnit.Framework.Explicit("Relies on ProtocolError::bad_changeset to be ClientReset Error")]
        public void Session_ClientResetHandlers_Access_Realms_OnAfterReset(IClientResetHandler handler)
        {
            const string validValue = "this will sync";
            const string invalidValue = "this will be deleted";

            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var tcs = new TaskCompletionSource<object>();
                var onAfterTriggered = false;
                var config = await GetIntegrationConfigAsync();
                var afterCb = GetOnAfterHandler(tcs, (beforeFrozen, after) =>
                {
                    Assert.That(onAfterTriggered, Is.False);

                    var frozenObjs = beforeFrozen.All<ObjectWithPartitionValue>().ToArray();
                    Assert.That(frozenObjs.Length, Is.EqualTo(2));
                    Assert.That(frozenObjs.Select(o => o.Value), Is.EquivalentTo(new[] { validValue, invalidValue }));

                    var objs = after.All<ObjectWithPartitionValue>();
                    Assert.That(objs.Count(), Is.EqualTo(1));
                    Assert.That(objs.Single().Value, Is.EqualTo(validValue));

                    onAfterTriggered = true;
                });
                config.ClientResetHandler = handler.BuildHandler(null, afterCb);
                config.Schema = new[] { typeof(ObjectWithPartitionValue) };

                using var realm = await GetRealmAsync(config);

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

        [TestCaseSource(nameof(_clientResetInstanceHandlers))]
        [NUnit.Framework.Explicit("Relies on ProtocolError::bad_changeset to be ClientReset Error")]
        public void Session_ClientResetAutomaticChanges_TriggersNotifications(ClientResetHandlerBase handler)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                //Logger.LogLevel = LogLevel.Trace;

                //Logger.Default = Logger.Function(message =>
                //{
                //    var fileName = "C:\\Users\\andrea.catalini\\clientLogs.txt";
                //    StreamWriter sw = new StreamWriter(fileName, true);
                //    sw.WriteLine(message);
                //    sw.Close();
                //});

                // We'll add an object with the wrong partition
                var config = await GetIntegrationConfigAsync();
                config.Schema = new[] { typeof(ObjectWithPartitionValue) };
                config.ClientResetHandler = handler;

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

        [TestCaseSource(nameof(_clientResetHandlers))]
        public void Session_ClientResetHandlers_ManualResetFallback_Exception_OnBefore(IClientResetHandler handler)
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

                config.ClientResetHandler = handler.BuildHandler(beforeCb, afterCb, manualCb);

                using var realm = await GetRealmAsync(config);

                GetSession(realm).SimulateClientReset("simulated client reset");

                await tcs.Task;

                Assert.That(manualFallbackTriggered, Is.True);
                Assert.That(onBeforeTriggered, Is.True);
                Assert.That(onAfterResetTriggered, Is.False);
            });
        }

        [TestCaseSource(nameof(_clientResetHandlers))]
        public void Session_ClientResetHandlers_ManualResetFallback_Exception_OnAfter(IClientResetHandler handler)
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

                config.ClientResetHandler = handler.BuildHandler(beforeCb, afterCb, manualCb);

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

        [TestCaseSource(nameof(_clientResetHandlers)),
            Obsolete("Testing Sesion.Error compatibility")]
        public void Session_ClientResetHandlers_Coexistence(IClientResetHandler clientResetHandler)
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

                config.ClientResetHandler = clientResetHandler.BuildHandler(beforeCb, afterCb);

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

        [TestCaseSource(nameof(_clientResetInstanceHandlers)),
            Obsolete("Testing Sesion.Error compatibility")]
        public void Session_WithNewClientResetHandlers_DoesntRaiseSessionError(ClientResetHandlerBase handler)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var obsoleteSessionErrorTriggered = false;
                var config = await GetIntegrationConfigAsync();
                config.ClientResetHandler = handler;
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

        [TestCaseSource(nameof(_clientResetHandlers)),
            Obsolete("Testing Sesion.Error compatibility")]
        public void Session_ClientResetHandlers_ManualResetFallback_Coexistence(IClientResetHandler clientResetHandler)
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

                config.ClientResetHandler = clientResetHandler.BuildHandler(null, null, manualCb);

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
        public void Session_ProgressObservable_IntegrationTests((ProgressMode Mode, ClientResetHandlerBase Handler) tuple)
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
                foreach (var handler in _clientResetInstanceHandlers)
                {
                    yield return ((ProgressMode)intMode, handler);
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

        private class ClientResetHanlder<T> : IClientResetHandler
            where T : ClientResetHandlerBase
        {
            public Type _handlerType;

            public ClientResetHanlder()
            {
                _handlerType = typeof(T);
            }

            public ClientResetHandlerBase BuildHandler(BeforeResetCallback beforeCb = null,
                                                       AfterResetCallback afterCb = null,
                                                       ClientResetCallback manualCb = null)
            {
                var handler = (ClientResetHandlerBase)Activator.CreateInstance(_handlerType);
                SetBeforeCb(beforeCb, handler);
                SetAfterCb(afterCb, handler);
                SetManualCb(manualCb, handler);
                return handler;
            }

            private static void SetBeforeCb(BeforeResetCallback function, ClientResetHandlerBase target) =>
                typeof(T).GetProperty("OnBeforeReset").SetValue(target, function);

            private static void SetAfterCb(AfterResetCallback function, ClientResetHandlerBase target) =>
                typeof(T).GetProperty("OnAfterReset").SetValue(target, function);

            private static void SetManualCb(ClientResetCallback function, ClientResetHandlerBase target) =>
                typeof(T).GetProperty("ManualResetFallback").SetValue(target, function);
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

        private static SessionNotificationToken? GetNotificationToken(Session session)
        {
            var sessionHandle = (SessionHandle)typeof(Session).GetField("_handle", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(session);
            return sessionHandle != null ?
                (SessionNotificationToken?)typeof(SessionHandle).GetField("_notificationToken", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sessionHandle) :
                null;
        }
    }
}
