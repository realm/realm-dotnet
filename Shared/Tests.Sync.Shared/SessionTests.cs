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
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;
using Realms;
using Realms.Sync;
using Realms.Sync.Exceptions;
using Realms.Sync.Testing;
using ExplicitAttribute = NUnit.Framework.ExplicitAttribute;

namespace Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class SessionTests
    {
        [Test]
        public void Realm_GetSession_WhenSyncedRealm()
        {
            AsyncContext.Run(async () =>
            {
                var user = await User.LoginAsync(Credentials.AccessToken("foo:bar", Guid.NewGuid().ToString(), isAdmin: true), new Uri("http://localhost:9080"));
                var serverUri = new Uri("realm://localhost:9080/foobar");
                using (var realm = Realm.GetInstance(new SyncConfiguration(user, serverUri)))
                {
                    var session = realm.GetSession();
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
                Assert.Throws<ArgumentException>(() => realm.GetSession());
            }
        }

        [Test]
        public void Realm_GetSession_ShouldReturnSameObject()
        {
            AsyncContext.Run(async () =>
            {
                using (var realm = await SyncTestHelpers.GetFakeRealm(isUserAdmin: true))
                {
                    var session1 = realm.GetSession();
                    var session2 = realm.GetSession();
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
                var realm = await SyncTestHelpers.GetFakeRealm(isUserAdmin: true);

                var session = realm.GetSession();
                const ErrorCode code = (ErrorCode)102;
                const string message = "Some fake error has occurred";

                var result = await SyncTestHelpers.SimulateSessionError<SessionException>(session, code, message);

                var error = result.Item2;
                Assert.That(error.Message, Is.EqualTo(message));
                Assert.That(error.ErrorCode, Is.EqualTo(code));

                var errorSession = result.Item1;
                Assert.That(errorSession.ServerUri, Is.EqualTo(((SyncConfiguration)realm.Config).ServerUri));

                realm.Dispose();
                Realm.DeleteRealm(realm.Config);
            });
        }

        [Test]
        public void Session_DivergingHistories_ShouldRaiseClientResetException()
        {
            AsyncContext.Run(async () =>
            {
                var realm = await SyncTestHelpers.GetFakeRealm(isUserAdmin: true);
                var session = realm.GetSession();
                var result = await SyncTestHelpers.SimulateSessionError<ClientResetException>(session, 
                                                                                              ErrorCode.DivergingHistories,
                                                                                              "Fake client reset is required");

                var error = result.Item2;
                Assert.That(error.BackupFilePath, Is.Not.Null);
                Assert.That(error.BackupFilePath, Is.StringContaining("io.realm.object-server-recovered-realms/recovered_realm"));
                Assert.That(File.Exists(error.BackupFilePath), Is.False);

                realm.Dispose();

                var clientResetSuccess = error.InitiateClientReset();

                Assert.That(clientResetSuccess, Is.True);
                Assert.That(File.Exists(error.BackupFilePath), Is.True);
            });
        }

        [Explicit("Fails with obscure error.")]
        [Test, Timeout(1000)]
        public void Session_Error_WhenInvalidRefreshToken()
        {
            AsyncContext.Run(async () =>
            {
                var errors = new List<Exception>();
                using (var realm = await SyncTestHelpers.GetFakeRealm(isUserAdmin: false))
                {
                    EventHandler<Realms.ErrorEventArgs> handler = null;
                    handler = new EventHandler<Realms.ErrorEventArgs>((sender, e) =>
                    {
                        errors.Add(e.Exception);
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
        [Test, Timeout(1000)]
        public void Session_Error_WhenInvalidAccessToken()
        {
            AsyncContext.Run(async () =>
            {
                var errors = new List<Exception>();
                using (var realm = await SyncTestHelpers.GetFakeRealm(isUserAdmin: true))
                {
                    EventHandler<Realms.ErrorEventArgs> handler = null;
                    handler = new EventHandler<Realms.ErrorEventArgs>((sender, e) =>
                    {
                        errors.Add(e.Exception);
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
    
        #if !ROS_SETUP
        [Explicit]
        #endif
        [TestCase(ProgressMode.ForCurrentlyOutstandingWork)]
        [TestCase(ProgressMode.ReportIndefinitely)]
        public void Session_ProgressObservable_IntegrationTests(ProgressMode mode)
        {
            const int ObjectSize = 1000000;
            const int ObjectsToRecord = 2;
            AsyncContext.Run(async () =>
            {
                var realm = await SyncTestHelpers.GetIntegrationRealm("progress");
                var completionTCS = new TaskCompletionSource<ulong>();
                var callbacksInvoked = 0;

                var session = realm.GetSession();
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

                        Assert.That(p.TransferredBytes, Is.LessThanOrEqualTo(p.TransferableBytes));

                        if (mode == ProgressMode.ForCurrentlyOutstandingWork)
                        {
                            Assert.That(p.TransferableBytes, Is.GreaterThan(ObjectSize));
                            Assert.That(p.TransferableBytes, Is.LessThan((ObjectsToRecord + 1) * ObjectSize));
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

                token.Dispose();
                realm.Dispose();
                Realm.DeleteRealm(realm.Config);
            });
        }

        [TestCase(ProgressDirection.Upload, ProgressMode.ForCurrentlyOutstandingWork)]
        [TestCase(ProgressDirection.Upload, ProgressMode.ReportIndefinitely)]
        [TestCase(ProgressDirection.Download, ProgressMode.ForCurrentlyOutstandingWork)]
        [TestCase(ProgressDirection.Download, ProgressMode.ReportIndefinitely)]
        public void Session_ProgressObservable_UnitTests(ProgressDirection direction, ProgressMode mode)
        {
            AsyncContext.Run(async () =>
            {
                var callbacksInvoked = 0;
                var completionTCS = new TaskCompletionSource<ulong>();

                var realm = await SyncTestHelpers.GetFakeRealm(isUserAdmin: true);
                var session = realm.GetSession();

                session.SimulateProgress(0, 100, 0, 100);

                var observable = session.GetProgressObservable(direction, mode);
                var token = observable.Subscribe(p =>
                {
                    try
                    {
                        callbacksInvoked++;

                        Assert.That(p.TransferredBytes, Is.LessThanOrEqualTo(p.TransferableBytes));

                        if (mode == ProgressMode.ForCurrentlyOutstandingWork)
                        {
                            Assert.That(p.TransferableBytes, Is.EqualTo(100));
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

                session.SimulateProgress(50, 150, 50, 150);
                await Task.Delay(50);

                session.SimulateProgress(100, 200, 100, 200);
                await Task.Delay(50);
                
                session.SimulateProgress(150, 200, 150, 200);
                await Task.Delay(50);

                session.SimulateProgress(200, 200, 200, 200);
                await Task.Delay(50);
                
                var totalTransferred = await completionTCS.Task;

                if (mode == ProgressMode.ForCurrentlyOutstandingWork)
                {
                    Assert.That(totalTransferred, Is.EqualTo(100));
                    Assert.That(callbacksInvoked, Is.EqualTo(3));
                }
                else
                {
                    Assert.That(totalTransferred, Is.EqualTo(200));
                    Assert.That(callbacksInvoked, Is.EqualTo(5));
                }

                token.Dispose();
                realm.Dispose();
                Realm.DeleteRealm(realm.Config);
            });
        }

        [Test]
        public void Session_RXCombineLatestTests()
        {
            AsyncContext.Run(async () =>
            {
                var realm = await SyncTestHelpers.GetFakeRealm(isUserAdmin: true);
                var session = realm.GetSession();

                var callbacksInvoked = 0;
                var completionTCS = new TaskCompletionSource<ulong>();

                var uploadProgress = session.GetProgressObservable(ProgressDirection.Upload, ProgressMode.ReportIndefinitely);
                var downloadProgress = session.GetProgressObservable(ProgressDirection.Download, ProgressMode.ReportIndefinitely);

                session.SimulateProgress(0, 100, 0, 100);
                var token = uploadProgress.CombineLatest(downloadProgress, (upload, download) =>
                            {
                                return new
                                {
                                    TotalTransferred = upload.TransferredBytes + download.TransferredBytes,
                                    TotalTransferable = upload.TransferableBytes + download.TransferableBytes
                                };
                            })
                            .Subscribe(progress =>
                            {
                                callbacksInvoked++;
                                if (progress.TotalTransferred == progress.TotalTransferable)
                                {
                                    completionTCS.TrySetResult(progress.TotalTransferred);
                                }
                            });

                await Task.Delay(50);
                session.SimulateProgress(50, 100, 0, 100);

                await Task.Delay(50);
                session.SimulateProgress(100, 100, 0, 100);

                await Task.Delay(50);
                session.SimulateProgress(100, 150, 0, 100);

                await Task.Delay(50);
                session.SimulateProgress(100, 150, 100, 100);

                await Task.Delay(50);
                session.SimulateProgress(150, 150, 100, 100);

                var totalTransferred = await completionTCS.Task;

                Assert.That(callbacksInvoked, Is.GreaterThanOrEqualTo(6));
                Assert.That(totalTransferred, Is.EqualTo(250));

                token.Dispose();
                realm.Dispose();
                Realm.DeleteRealm(realm.Config);
            });
        }

        [Test]
        public void Session_RXThrottleTests()
        {
            AsyncContext.Run(async () =>
            {
                const int ThrottleInterval = 100; // In ms
                const int SafeDelay = 2 * ThrottleInterval;

                var realm = await SyncTestHelpers.GetFakeRealm(isUserAdmin: true);
                var session = realm.GetSession();

                var callbacksInvoked = 0;

                var uploadProgress = session.GetProgressObservable(ProgressDirection.Download, ProgressMode.ReportIndefinitely);

                session.SimulateProgress(0, 100, 0, 0);

                var token = uploadProgress.Throttle(TimeSpan.FromMilliseconds(ThrottleInterval))
                                          .Subscribe(p =>
                                          {
                                              callbacksInvoked++;
                                              Console.WriteLine(p.TransferableBytes);
                                          });

                await Task.Delay(SafeDelay);
                Assert.That(callbacksInvoked, Is.EqualTo(1));

                for (ulong i = 0; i < 10; i++)
                {
                    session.SimulateProgress(i, 100, 0, 0);
                }

                await Task.Delay(SafeDelay);
                Assert.That(callbacksInvoked, Is.EqualTo(2));

                for (ulong i = 10; i < 20; i++)
                {
                    session.SimulateProgress(i, 100, 0, 0);
                    await Task.Delay(ThrottleInterval / 10);
                }

                Assert.That(callbacksInvoked, Is.EqualTo(2));
                await Task.Delay(SafeDelay);
                Assert.That(callbacksInvoked, Is.EqualTo(3));

                for (ulong i = 20; i < 25; i++)
                {
                    session.SimulateProgress(i, 100, 0, 0);
                    await Task.Delay(SafeDelay);
                }

                Assert.That(callbacksInvoked, Is.EqualTo(8));

                token.Dispose();
                realm.Dispose();
                Realm.DeleteRealm(realm.Config);
            });
        }
    }
}
