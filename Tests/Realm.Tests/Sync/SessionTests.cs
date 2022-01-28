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
using System.Threading.Tasks;
using NUnit.Framework;
using Realms.Sync;
using Realms.Sync.Testing;
using Realms.Sync.Exceptions;
using Realms.Sync.ErrorHandling;
using System.Linq;

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
        public void Session_ClientReset_DiscardLocal()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                int beforeCheck = 0;
                int afterCheck = 0;
                var tcs = new TaskCompletionSource<bool>();
                var config = await GetIntegrationConfigAsync();

                config.ClientResetHandler = new DiscardLocalResetHandler
                {
                    OnBeforeReset = (beforeFrozen) =>
                    {
                        Assert.AreEqual(beforeCheck, 0);
                        Assert.AreEqual(afterCheck, 0);

                        beforeCheck = 1;
                    },
                    OnAfterReset = (beforeFrozen, after) =>
                    {
                        Assert.AreEqual(beforeCheck, 1);
                        Assert.AreEqual(afterCheck, 0);

                        beforeCheck = 2;
                        afterCheck = 1;

                        tcs.TrySetResult(true);
                    }
                };

                using var realm = await GetRealmAsync(config);
                var session = GetSession(realm);

                session.SimulateError(ErrorCode.DivergingHistories, "simulated client reset");

                await tcs.Task;

                Assert.AreEqual(beforeCheck, 2);
                Assert.AreEqual(afterCheck, 1);
            });
        }

        [Test]
        public void Session_ClientReset_DiscardLocal_ManualResetFallback()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                int beforeCheck = 0;
                int afterCheck = 0;
                var manualResetFallbackHandled = false;
                var config = await GetIntegrationConfigAsync();
                var tcs = new TaskCompletionSource<bool>();

                config.ClientResetHandler = new DiscardLocalResetHandler
                {
                    OnBeforeReset = (beforeFrozen) =>
                    {
                        beforeCheck = 1;
                    },
                    OnAfterReset = (beforeFrozen, after) =>
                    {
                        beforeCheck = 2;
                        afterCheck = 1;

                        tcs.TrySetResult(true);
                    },
                    ManualResetFallback = (session, exception) =>
                    {
                        Assert.IsFalse(manualResetFallbackHandled);
                        manualResetFallbackHandled = true;
                        tcs.TrySetResult(true);
                    }
                };

                using var realm = await GetRealmAsync(config);

                GetSession(realm).SimulateError(ErrorCode.AutoClientResetFailed_Cl, "simulated client reset failure");

                await tcs.Task;

                Assert.AreEqual(beforeCheck, 0);
                Assert.AreEqual(afterCheck, 0);
                Assert.IsTrue(manualResetFallbackHandled);
            });
        }

        [Test]
        public void Session_ClientReset_ManualRecoveryHandler_OnError()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var manualOnClientResetCalled = false;
                var tcs = new TaskCompletionSource<bool>();
                var config = await GetIntegrationConfigAsync();
                config.ClientResetHandler = new ManualRecoveryHandler
                {
                    OnClientReset = (sender, e) =>
                    {
                        Assert.IsInstanceOf<Session>(sender);
                        Assert.IsInstanceOf<SessionException>(e);
                        manualOnClientResetCalled = true;
                        tcs.TrySetResult(true);
                    }
                };

                using var realm = await GetRealmAsync(config);
                var session = GetSession(realm);

                session.SimulateError(ErrorCode.DivergingHistories, "simulated client reset");

                await tcs.Task;

                Assert.IsTrue(manualOnClientResetCalled);
            });
        }

        [Test]
        public void Session_ClientReset_ManualRecoveryHandler_When_Exception()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var tcs = new TaskCompletionSource<bool>();
                var manualFallbackTriggered = false;
                var afterResetTriggered = false;
                var config = await GetIntegrationConfigAsync();
                config.ClientResetHandler = new DiscardLocalResetHandler
                {
                    OnBeforeReset = (beforeFrozen) =>
                    {
                        throw new Exception("Exception thrown in OnBeforeReset");
                    },
                    OnAfterReset = (beforeFrozen, after) =>
                    {
                        afterResetTriggered = true;
                        tcs.TrySetResult(true);
                    },
                    ManualResetFallback = (session, err) =>
                    {
                        manualFallbackTriggered = true;
                        tcs.TrySetResult(true);
                    }
                };

                using var realm = await GetRealmAsync(config);
                var session = GetSession(realm);

                session.SimulateError(ErrorCode.DivergingHistories, "simulated client reset");

                await tcs.Task;

                Assert.IsTrue(manualFallbackTriggered);
                Assert.IsFalse(afterResetTriggered);
            });
        }

        [Test]
        public void Session_SyncError_Handler()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var sessionErrorHandled = false;
                var tcs = new TaskCompletionSource<bool>();
                var config = await GetIntegrationConfigAsync();
                config.SyncErrorHandler = new SyncErrorHandler
                {
                    OnError = (sender, e) =>
                    {
                        Assert.IsInstanceOf<Session>(sender);
                        Assert.IsInstanceOf<SessionException>(e);
                        Assert.IsFalse(sessionErrorHandled);
                        sessionErrorHandled = true;
                        tcs.TrySetResult(true);
                    }
                };

                using var realm = await GetRealmAsync(config);
                var session = GetSession(realm);

                session.SimulateError(ErrorCode.PermissionDenied, "simulated sync issue");

                await tcs.Task;

                Assert.IsTrue(sessionErrorHandled);
            });
        }

        // TODO andrea: temporary test for co-existence of Session.Error and error handling in SyncConfigurationBase
        [Test]
        public void Session_ClientReset_Handlers_Coexistence()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                int beforeCheck = 0;
                int afterCheck = 0;
                var obsoleteSessionErrorTriggered = false;
                var tcs = new TaskCompletionSource<bool>();
                var config = await GetIntegrationConfigAsync();
                config.ClientResetHandler = new DiscardLocalResetHandler
                {
                    OnBeforeReset = (beforeFrozen) =>
                    {
                        Assert.AreEqual(beforeCheck, 0);
                        Assert.AreEqual(afterCheck, 0);

                        beforeCheck = 1;
                    },
                    OnAfterReset = (beforeFrozen, after) =>
                    {
                        Assert.AreEqual(beforeCheck, 1);
                        Assert.AreEqual(afterCheck, 0);

                        beforeCheck = 2;
                        afterCheck = 1;

                        tcs.TrySetResult(true);
                    }
                };

                using var realm = await GetRealmAsync(config);
                var session = GetSession(realm);

                // priority is given to the newer appoach in SyncConfigurationBase, so this should never be reached
                Session.Error += (sender, e) =>
                {
                    obsoleteSessionErrorTriggered = true;
                    tcs.TrySetResult(true);
                };

                session.SimulateError(ErrorCode.DivergingHistories, "simulated client reset");

                await tcs.Task;

                Assert.AreEqual(beforeCheck, 2);
                Assert.AreEqual(afterCheck, 1);
                Assert.IsFalse(obsoleteSessionErrorTriggered);
            });
        }

        // TODO andrea: temporary test for co-existence of Session.Error and error handling in SyncConfigurationBase
        [Test]
        public void Session_Error_Handlers_Coexistence()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var obsoleteSessionErrorTriggered = false;
                var sessionErrorHandled = false;
                var tcs = new TaskCompletionSource<bool>();
                var config = await GetIntegrationConfigAsync();
                config.SyncErrorHandler = new SyncErrorHandler
                {
                    OnError = (sender, e) =>
                    {
                        Assert.IsInstanceOf<Session>(sender);
                        Assert.IsInstanceOf<SessionException>(e);
                        Assert.IsFalse(sessionErrorHandled);
                        sessionErrorHandled = true;
                        tcs.TrySetResult(true);
                    }
                };

                using var realm = await GetRealmAsync(config);
                var session = GetSession(realm);

                // priority is given to the newer appoach in SyncConfigurationBase, so this should never be reached
                Session.Error += (sender, e) =>
                {
                    obsoleteSessionErrorTriggered = true;
                    tcs.TrySetResult(true);
                };

                session.SimulateError(ErrorCode.PermissionDenied, "simulated sync issue");

                await tcs.Task;

                Assert.IsFalse(obsoleteSessionErrorTriggered);
                Assert.IsTrue(sessionErrorHandled);
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
            Assert.Throws<ObjectDisposedException>(() => session.ReportErrorForTesting(1, string.Empty, "test", false));

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
