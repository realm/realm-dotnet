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
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;
using Realms.Sync;
using Realms.Sync.Exceptions;
using Realms.Sync.Testing;

namespace Realms.Tests.Sync
{
    using ExplicitAttribute = NUnit.Framework.ExplicitAttribute;

#pragma warning disable CS0618 // Don't complain about SimulateProgress

    [TestFixture, Preserve(AllMembers = true)]
    public class SessionTests : SyncTestBase
    {
        [Test]
        public void Realm_GetSession_WhenSyncedRealm()
        {
            AsyncContext.Run(async () =>
            {
                var user = await SyncTestHelpers.GetFakeUserAsync();
                var serverUri = new Uri("realm://localhost:9080/foobar");
                var config = new FullSyncConfiguration(serverUri, user);

                using (var realm = GetRealm(config))
                {
                    var session = GetSession(realm);

                    Assert.That(session.User, Is.EqualTo(user));
                    Assert.That(session.ServerUri, Is.EqualTo(serverUri));
                }
            });
        }

        [Test]
        public void Realm_GetSession_WhenLocalRealm_ShouldThrow()
        {
            using (var realm = Realm.GetInstance())
            {
                Assert.Throws<ArgumentException>(() => GetSession(realm));
            }
        }

        [Test]
        public void Realm_GetSession_ShouldReturnSameObject()
        {
            AsyncContext.Run(async () =>
            {
                var config = await SyncTestHelpers.GetFakeConfigAsync();
                using (var realm = GetRealm(config))
                {
                    var session1 = GetSession(realm);
                    var session2 = GetSession(realm);

                    Assert.That(session1, Is.EqualTo(session2));
                    Assert.That(session1.GetHashCode(), Is.EqualTo(session2.GetHashCode()));
                }
            });
        }

        [Test]
        public void Session_Error_ShouldPassCorrectSession()
        {
            AsyncContext.Run(async () =>
            {
                var config = await SyncTestHelpers.GetFakeConfigAsync();
                using (var realm = GetRealm(config))
                {
                    var session = GetSession(realm);

                    const ErrorCode code = (ErrorCode)102;
                    const string message = "Some fake error has occurred";

                    var result = await SyncTestHelpers.SimulateSessionErrorAsync<SessionException>(session, code, message);
                    CleanupOnTearDown(result.Item1);

                    var error = result.Item2;
                    Assert.That(error.Message, Is.EqualTo(message));
                    Assert.That(error.ErrorCode, Is.EqualTo(code));

                    var errorSession = result.Item1;
                    Assert.That(errorSession.ServerUri, Is.EqualTo(((SyncConfigurationBase)realm.Config).ServerUri));
                }
            });
        }

        [Test]
        public void Session_DivergingHistories_ShouldRaiseClientResetException()
        {
            AsyncContext.Run(async () =>
            {
                var config = await SyncTestHelpers.GetFakeConfigAsync();
                ClientResetException error = null;
                using (var realm = GetRealm(config))
                {
                    var session = GetSession(realm);

                    var result = await SyncTestHelpers.SimulateSessionErrorAsync<ClientResetException>(session,
                                                                                                  ErrorCode.DivergingHistories,
                                                                                                  "Fake client reset is required");
                    CleanupOnTearDown(result.Item1);

                    error = result.Item2;
                }

                Assert.That(error.BackupFilePath, Is.Not.Null);
                Assert.That(error.BackupFilePath, Does.Contain(Path.Combine("io.realm.object-server-recovered-realms", "recovered_realm")));
                Assert.That(File.Exists(error.BackupFilePath), Is.False);

                var clientResetSuccess = error.InitiateClientReset();

                Assert.That(clientResetSuccess, Is.True);
                Assert.That(File.Exists(error.BackupFilePath), Is.True);
            });
        }

        [Explicit("Fails with obscure error.")]
        [Test]
        public void Session_Error_WhenInvalidRefreshToken()
        {
            AsyncContext.Run(async () =>
            {
                var errors = new List<Exception>();
                var config = await SyncTestHelpers.GetFakeConfigAsync();
                using (var realm = GetRealm(config))
                {
                    EventHandler<Realms.ErrorEventArgs> handler = null;
                    handler = new EventHandler<Realms.ErrorEventArgs>((sender, e) =>
                    {
                        errors.Add(e.Exception);
                        CleanupOnTearDown((Session)sender);
                    });

                    Session.Error += handler;

                    while (!errors.Any())
                    {
                        await Task.Yield();
                    }

                    Session.Error -= handler;

                    var authErrors = errors.OfType<AuthenticationException>().ToArray();
                    Assert.That(authErrors.Count, Is.EqualTo(1));
                    Assert.That(authErrors[0].ErrorCode, Is.EqualTo(ErrorCode.InvalidCredentials));
                }
            });
        }

        [Explicit("Fails with obscure error.")]
        [Test]
        public void Session_Error_WhenInvalidAccessToken()
        {
            AsyncContext.Run(async () =>
            {
                var errors = new List<Exception>();
                var config = await SyncTestHelpers.GetFakeConfigAsync();
                using (var realm = GetRealm(config))
                {
                    EventHandler<Realms.ErrorEventArgs> handler = null;
                    handler = new EventHandler<Realms.ErrorEventArgs>((sender, e) =>
                    {
                        errors.Add(e.Exception);
                        CleanupOnTearDown((Session)sender);
                    });

                    Session.Error += handler;

                    while (!errors.Any())
                    {
                        await Task.Yield();
                    }

                    Session.Error -= handler;

                    var sessionErrors = errors.OfType<SessionException>().ToArray();
                    Assert.That(sessionErrors.Count, Is.EqualTo(1));
                    Assert.That(sessionErrors[0].ErrorCode, Is.EqualTo(ErrorCode.BadUserAuthentication));
                }
            });
        }

        [TestCase(ProgressMode.ForCurrentlyOutstandingWork)]
        [TestCase(ProgressMode.ReportIndefinitely)]
        public void Session_ProgressObservable_IntegrationTests(ProgressMode mode)
        {
            SyncTestHelpers.RequiresRos();

            const int ObjectSize = 1000000;
            const int ObjectsToRecord = 2;
            AsyncContext.Run(async () =>
            {
                var config = await SyncTestHelpers.GetIntegrationConfigAsync("progress");
                var realm = GetRealm(config);
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
                var token = observable.Subscribe(p =>
                {
                    try
                    {
                        callbacksInvoked++;

                        if (p.TransferredBytes > p.TransferableBytes)
                        {
                            throw new Exception($"Expected: {p.TransferredBytes} <= {p.TransferableBytes}");
                        }

                        if (mode == ProgressMode.ForCurrentlyOutstandingWork)
                        {
                            if (p.TransferableBytes <= ObjectSize ||
                                p.TransferableBytes >= (ObjectsToRecord + 1) * ObjectSize)
                            {
                                throw new Exception($"Expected: {p.TransferredBytes} to be in the ({ObjectSize}, {(ObjectsToRecord + 1) * ObjectSize}) range.");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        completionTCS.TrySetException(e);
                    }

                    if (p.TransferredBytes == p.TransferableBytes)
                    {
                        completionTCS.TrySetResult(p.TransferredBytes);
                    }
                });

                using (token)
                {
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
                        Assert.That(totalTransferred, Is.LessThan((ObjectsToRecord + 1) * ObjectSize));
                    }
                    else
                    {
                        Assert.That(totalTransferred, Is.GreaterThanOrEqualTo((ObjectsToRecord + 1) * ObjectSize));
                    }

                    Assert.That(callbacksInvoked, Is.GreaterThan(1));
                }
            });
        }

        [Test]
        public void Session_Stop_StopsSession()
        {
            AsyncContext.Run(async () =>
            {
                // OpenRealmAndStopSession will call Stop and assert the state changed
                await OpenRealmAndStopSession();
            });
        }

        [Test]
        public void Session_Start_ResumesSession()
        {
            AsyncContext.Run(async () =>
            {
                var session = await OpenRealmAndStopSession();

                session.Start();
                Assert.That(session.State, Is.EqualTo(SessionState.Active));
            });
        }

        [Test]
        public void Session_Stop_IsIdempotent()
        {
            AsyncContext.Run(async () =>
            {
                var session = await OpenRealmAndStopSession();

                // Stop it again
                session.Stop();
                Assert.That(session.State, Is.EqualTo(SessionState.Inactive));
            });
        }

        [Test]
        public void Session_Start_IsIdempotent()
        {
            AsyncContext.Run(async () =>
            {
                var session = await OpenRealmAndStopSession();

                session.Start();
                Assert.That(session.State, Is.EqualTo(SessionState.Active));

                // Start it again
                session.Start();
                Assert.That(session.State, Is.EqualTo(SessionState.Active));
            });
        }

        /// <summary>
        /// Opens a random realm and calls session.Stop(). It will assert state changes
        /// to Inactive.
        /// </summary>
        /// <returns>The stopped session.</returns>
        private async Task<Session> OpenRealmAndStopSession()
        {
            var config = await SyncTestHelpers.GetFakeConfigAsync();
            var realm = GetRealm(config);
            var session = GetSession(realm);

            Assert.That(session.State, Is.EqualTo(SessionState.Active));

            session.Stop();
            Assert.That(session.State, Is.EqualTo(SessionState.Inactive));

            return session;
        }
    }
}
