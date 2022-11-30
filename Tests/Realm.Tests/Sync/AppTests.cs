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
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Realms.Logging;
using Realms.Sync;
using Realms.Sync.Exceptions;

namespace Realms.Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class AppTests : SyncTestBase
    {
        [Test]
        public void AppCreate_CreatesApp()
        {
#pragma warning disable CS0618 // Type or member is obsolete

            // This is mostly a smoke test to ensure that nothing blows up when setting all properties.
            var config = new AppConfiguration("abc-123")
            {
                BaseUri = new Uri("http://foo.bar"),
                LocalAppName = "My app",
                LocalAppVersion = "1.2.3",
                LogLevel = LogLevel.All,
                MetadataEncryptionKey = new byte[64],
                MetadataPersistenceMode = MetadataPersistenceMode.Encrypted,
                BaseFilePath = InteropConfig.GetDefaultStorageFolder("No error expected here"),
                CustomLogger = (message, level) => { },
                DefaultRequestTimeout = TimeSpan.FromSeconds(123)
            };

#pragma warning restore CS0618 // Type or member is obsolete

            var app = CreateApp(config);
            Assert.That(app.Sync, Is.Not.Null);
        }

        [Test]
        public void App_Login_Anonymous()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var user = await DefaultApp.LogInAsync(Credentials.Anonymous());
                Assert.That(user, Is.Not.Null);
                Assert.That(user.Id, Is.Not.Null);
            });
        }

        [TestCase(LogLevel.Debug)]
        [TestCase(LogLevel.Info)]
        public void App_WithCustomLogger_LogsSyncOperations(LogLevel logLevel)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var logBuilder = new StringBuilder();

                var appConfig = SyncTestHelpers.GetAppConfig();
#pragma warning disable CS0618 // Type or member is obsolete
                appConfig.LogLevel = logLevel;
                appConfig.CustomLogger = (message, level) =>
                {
                    lock (logBuilder)
                    {
                        logBuilder.AppendLine($"[{level}] {message}");
                    }
                };
#pragma warning restore CS0618 // Type or member is obsolete

                var app = CreateApp(appConfig);

                var config = await GetIntegrationConfigAsync(Guid.NewGuid().ToString());
                using var realm = await GetRealmAsync(config);
                realm.Write(() =>
                {
                    realm.Add(new PrimaryKeyStringObject { Id = Guid.NewGuid().ToString() });
                });

                await WaitForUploadAsync(realm);

                string log;
                lock (logBuilder)
                {
                    log = logBuilder.ToString();
                }

                Assert.That(log, Does.Contain($"[{logLevel}]"));
                Assert.That(log, Does.Not.Contain($"[{logLevel - 1}]"));
            });
        }

        [TestCase(LogLevel.Debug)]
        [TestCase(LogLevel.Info)]
        public void RealmConfiguration_WithCustomLogger_LogsSyncOperations(LogLevel logLevel)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                Logger.LogLevel = logLevel;
                var logger = new Logger.InMemoryLogger();
                Logger.Default = logger;

                var config = await GetIntegrationConfigAsync(Guid.NewGuid().ToString());
                using var realm = await GetRealmAsync(config);
                realm.Write(() =>
                {
                    realm.Add(new PrimaryKeyStringObject { Id = Guid.NewGuid().ToString() });
                });

                await WaitForUploadAsync(realm);

                var log = logger.GetLog();

                Assert.That(log, Does.Contain($"{logLevel}:"));
                Assert.That(log, Does.Not.Contain($"{logLevel - 1}:"));
            });
        }

        [Test]
        public void RealmConfiguration_WithCustomHttpClientHandler_CleanedUpAfterAppDestroyed()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                App app = null;
                var gcTask = TestHelpers.EnsureObjectsAreCollected(() =>
                {
                    var handler = new HttpClientHandler();

                    app = App.Create(new AppConfiguration("abc")
                    {
                        HttpClientHandler = handler
                    });

                    return new[] { handler };
                });

                // Since apps are cached, we need to clear the cache to destroy the app
                // and trigger the chain that will eventually free the HttpClient holding
                // the HttpClientHandler
                AppHandle.ForceCloseHandles(clearNativeCache: true);

                await gcTask;
            });
        }

        private class TestHttpClientHandler : DelegatingHandler
        {
            public readonly List<(HttpMethod Method, string Url)> Requests = new();

            public TestHttpClientHandler() : base(TestHelpers.TestHttpHandlerFactory())
            {
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Requests.Add((request.Method, request.RequestUri.AbsoluteUri));
                return base.SendAsync(request, cancellationToken);
            }
        }

        [Test]
        public void RealmConfiguration_WithCustomHttpClientHandler_UsedWhenMakingCalls()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var handler = new TestHttpClientHandler();

                var app = CreateApp(new AppConfiguration("abc")
                {
                    HttpClientHandler = handler
                });

                var ex = await TestHelpers.AssertThrows<AppException>(() => app.LogInAsync(Credentials.Anonymous()));

                // Http error
                Assert.That(ex.Message, Does.Contain("cannot find app"));

                // The app doesn't exist, so we expect 404
                Assert.That(ex.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

                Assert.That(handler.Requests.Count, Is.EqualTo(1));

                // https://realm.mongodb.com/api/client/v2.0/app/abc/location
                Assert.That(handler.Requests[0].Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(handler.Requests[0].Url, Does.Contain("abc/location"));
            });
        }

        [Test]
        public void RealmConfiguration_HttpClientHandler_IsNotSet()
        {
            var config = new AppConfiguration("abc");
            Assert.That(config.HttpClientHandler, Is.Null);
        }

        [Test]
        public void RealmConfiguration_HttpClientHandler_MayBeNull()
        {
            var config = new AppConfiguration("abc");
            config.HttpClientHandler = null;
            config.HttpClientHandler = new HttpClientHandler();
            config.HttpClientHandler = null;
        }
    }
}
