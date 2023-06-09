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
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Realms.Logging;
using Realms.PlatformHelpers;
using Realms.Sync;
using Realms.Sync.Exceptions;

namespace Realms.Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class AppTests : SyncTestBase
    {
        [Test]
        public void DeviceInfo_OutputsMeaningfulInfo()
        {
            if (TestHelpers.IsUnity)
            {
                Assert.That(Platform.DeviceInfo.Name, Is.EqualTo(Platform.Unknown));
                Assert.That(Platform.DeviceInfo.Version, Is.Not.EqualTo(Platform.Unknown));
                return;
            }

            var framework = Assembly.GetEntryAssembly()?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;

            var os = SharedRealmHandle.GetNativeLibraryOS();
            switch (os)
            {
                case "Windows":
                case "Linux":
                    Assert.That(Platform.DeviceInfo.Name, Is.EqualTo(Platform.Unknown), "Name");
                    Assert.That(Platform.DeviceInfo.Version, Is.EqualTo(Platform.Unknown), "Version");
                    break;
                case "macOS":
                    // We don't detect the device on .NET Core apps, only Xamarin or net6.0-maccatalyst.
                    if (framework?.Contains(".NETCoreApp") == true)
                    {
                        Assert.That(Platform.DeviceInfo.Name, Is.EqualTo(Platform.Unknown), "Name");
                        Assert.That(Platform.DeviceInfo.Version, Is.EqualTo(Platform.Unknown), "Version");
                    }
                    else
                    {
                        Assert.That(Platform.DeviceInfo.Name, Is.EqualTo("Apple"), "Name");
                        Assert.That(Platform.DeviceInfo.Version, Is.Not.EqualTo(Platform.Unknown), "Version");
                    }

                    break;
                case "iOS":
                    Assert.That(Platform.DeviceInfo.Name, Is.EqualTo("iPhone"), "Name");
                    Assert.That(Platform.DeviceInfo.Version, Does.Contain("iPhone").Or.EqualTo("x86_64"), "Version");
                    break;
                case "Android":
                    Assert.That(Platform.DeviceInfo.Name, Is.Not.EqualTo(Platform.Unknown), "Name");
                    Assert.That(Platform.DeviceInfo.Version, Is.Not.EqualTo(Platform.Unknown), "Version");
                    break;
                case "UWP":
                    if (TestHelpers.IsUWP)
                    {
                        Assert.That(Platform.DeviceInfo.Name, Is.Not.EqualTo(Platform.Unknown), "Name");
                        Assert.That(Platform.DeviceInfo.Version, Is.Not.EqualTo(Platform.Unknown), "Version");
                    }
                    else
                    {
                        Assert.That(Platform.DeviceInfo.Name, Is.EqualTo(Platform.Unknown), "Name");
                        Assert.That(Platform.DeviceInfo.Version, Is.EqualTo(Platform.Unknown), "Version");
                    }

                    break;
                case "tvOS":
                    Assert.That(Platform.DeviceInfo.Name, Is.EqualTo("Apple TV"), "Name");
                    Assert.That(Platform.DeviceInfo.Version, Does.Contain("AppleTV").Or.EqualTo("x86_64"), "Version");
                    break;
                case "Mac Catalyst":
                    Assert.That(Platform.DeviceInfo.Name, Is.EqualTo("iPad"), "Name");
                    Assert.That(Platform.DeviceInfo.Version, Does.Contain("iPad").Or.EqualTo("x86_64"), "Version");
                    break;
                default:
                    Assert.Fail($"Unknown OS: {os}");
                    break;
            }
        }

        [Test]
        public void AppCreate_CreatesApp()
        {
            // This is mostly a smoke test to ensure that nothing blows up when setting all properties.
            var config = new AppConfiguration("abc-123")
            {
                BaseUri = new Uri("http://foo.bar"),
                LocalAppName = "My app",
                LocalAppVersion = "1.2.3",
                MetadataEncryptionKey = new byte[64],
                MetadataPersistenceMode = MetadataPersistenceMode.Encrypted,
                BaseFilePath = InteropConfig.GetDefaultStorageFolder("No error expected here"),
                DefaultRequestTimeout = TimeSpan.FromSeconds(123)
            };

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
                App app = null!;
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
                Requests.Add((request.Method, request.RequestUri!.AbsoluteUri));
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
