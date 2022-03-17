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
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using Realms.Sync;
using Realms.Sync.Exceptions;
using Realms.Sync.Native;

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

                void NotificationChanged(object sender, PropertyChangedEventArgs e)
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

                void NotificationChanged(object sender, PropertyChangedEventArgs e)
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
            Assert.Throws<ObjectDisposedException>(() => session.ReportErrorForTesting(1, "test", false));

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

        private static SessionNotificationToken? GetNotificationToken(Session session)
        {
            var sessionHandle = (SessionHandle)typeof(Session).GetField("_handle", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(session);
            return sessionHandle != null ?
                (SessionNotificationToken?)typeof(SessionHandle).GetField("_notificationToken", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sessionHandle) :
                null;
        }
    }
}
