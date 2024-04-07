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
            static void AssertBundleId(params string[] expectedValues)
            {
                var localTestRunners = new[] { "ReSharperTestRunner", "testhost" };
                var values = expectedValues.Concat(localTestRunners).Select(Platform.Sha256).ToArray();
                Assert.That(values, Does.Contain(Platform.BundleId));
            }

            if (TestHelpers.IsUnity)
            {
                Assert.That(Platform.DeviceInfo.Name, Is.EqualTo(Platform.Unknown));
                Assert.That(Platform.DeviceInfo.Version, Is.Not.EqualTo(Platform.Unknown));
                Assert.That(Platform.BundleId, Is.EqualTo(Platform.Sha256("Tests.Unity")));
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
                    AssertBundleId("Realm.Tests");
                    break;
                case "macOS":
                    // We don't detect the device on .NET Core apps, only Xamarin or net6.0-maccatalyst.
                    if (framework?.Contains(".NETCoreApp") == true)
                    {
                        Assert.That(Platform.DeviceInfo.Name, Is.EqualTo(Platform.Unknown), "Name");
                        Assert.That(Platform.DeviceInfo.Version, Is.EqualTo(Platform.Unknown), "Version");
                        AssertBundleId("Realm.Tests");
                    }
                    else
                    {
                        Assert.That(Platform.DeviceInfo.Name, Is.EqualTo("Apple"), "Name");
                        Assert.That(Platform.DeviceInfo.Version, Is.Not.EqualTo(Platform.Unknown), "Version");
                        AssertBundleId("Tests.XamarinMac");
                    }

                    break;
                case "iOS":
                    Assert.That(Platform.DeviceInfo.Name, Is.EqualTo("iPhone"), "Name");
                    Assert.That(Platform.DeviceInfo.Version, Does.Contain("iPhone").Or.EqualTo("x86_64"), "Version");
                    AssertBundleId("Tests.iOS", "Tests.Maui");
                    break;
                case "Android":
                    Assert.That(Platform.DeviceInfo.Name, Is.Not.EqualTo(Platform.Unknown), "Name");
                    Assert.That(Platform.DeviceInfo.Version, Is.Not.EqualTo(Platform.Unknown), "Version");
                    AssertBundleId("Tests.Android", "Tests.Maui");
                    break;
                case "UWP":
                    if (TestHelpers.IsUWP)
                    {
#if DEBUG
                        // Extracting device info only works for local builds - in many cases we don't have registry access on CI
                        // so we can't make assumptions about what value we'll get for Name/Version.
                        Assert.That(Platform.DeviceInfo.Name, Is.Not.EqualTo(Platform.Unknown), "Name");
                        Assert.That(Platform.DeviceInfo.Version, Is.Not.EqualTo(Platform.Unknown), "Version");
#endif
                    }
                    else
                    {
                        Assert.That(Platform.DeviceInfo.Name, Is.EqualTo(Platform.Unknown), "Name");
                        Assert.That(Platform.DeviceInfo.Version, Is.EqualTo(Platform.Unknown), "Version");
                    }

                    AssertBundleId("Tests.UWP");
                    break;
                case "tvOS":
                    Assert.That(Platform.DeviceInfo.Name, Is.EqualTo("Apple TV"), "Name");
                    Assert.That(Platform.DeviceInfo.Version, Does.Contain("AppleTV").Or.EqualTo("x86_64"), "Version");
                    AssertBundleId("Tests.XamarinTVOS");
                    break;
                case "Mac Catalyst":
                    Assert.That(Platform.DeviceInfo.Name, Is.EqualTo("iPad"), "Name");
                    Assert.That(Platform.DeviceInfo.Version, Does.Contain("iPad").Or.EqualTo("x86_64"), "Version");
                    AssertBundleId("Tests.Maui");
                    break;
                default:
                    Assert.Fail($"Unknown OS: {os}");
                    break;
            }
        }

        [Test]
        public void AppCreate_CreatesApp()
        {
            var basePath = Path.Combine(InteropConfig.GetDefaultStorageFolder("No error expected here"), "foo-bar");
            Directory.CreateDirectory(basePath);

            // This is mostly a smoke test to ensure that nothing blows up when setting all properties.
            var config = new AppConfiguration("abc-123")
            {
                BaseUri = new Uri("http://foo.bar"),
                MetadataEncryptionKey = new byte[64],
                MetadataPersistenceMode = MetadataPersistenceMode.Encrypted,
                BaseFilePath = basePath,
                DefaultRequestTimeout = TimeSpan.FromSeconds(123)
            };

            var app = CreateApp(config);
            Assert.That(app.Sync, Is.Not.Null);
            Assert.That(app.BaseUri, Is.EqualTo(config.BaseUri));
            Assert.That(app.BaseFilePath, Is.EqualTo(config.BaseFilePath));
            Assert.That(app.Id, Is.EqualTo(config.AppId));
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

                    app = CreateApp(new AppConfiguration("abc")
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

        [Test]
        public void AppCreate_CacheTests([Values(true, false, null)] bool? useCache)
        {
            var config1 = GetConfig();
            var app1 = CreateApp(config1);

            var config2 = GetConfig();
            var app2 = CreateApp(config2);

            Assert.That(config1.BaseFilePath, Is.Not.EqualTo(config2.BaseFilePath));
            Assert.That(app1.Id, Is.EqualTo(app2.Id));
            Assert.That(app1.GetHashCode(), Is.EqualTo(app2.GetHashCode()));

            // null or true mean cache should be used
            if (useCache != false)
            {
                // If we cached the app, the second base file path should have been ignored
                Assert.That(app1.BaseFilePath, Is.EqualTo(app2.BaseFilePath));
                Assert.That(app1.Equals(app2), Is.True);
            }
            else
            {
                Assert.That(app1.BaseFilePath, Is.Not.EqualTo(app2.BaseFilePath));
                Assert.That(app1.Equals(app2), Is.False);
            }

            AppConfiguration GetConfig()
            {
                var baseFilePath = Path.Combine(InteropConfig.GetDefaultStorageFolder("no error expected"), Guid.NewGuid().ToString());
                var config = new AppConfiguration("abc")
                {
                    BaseFilePath = baseFilePath,
                };

                if (useCache.HasValue)
                {
                    config.UseAppCache = useCache.Value;
                }

                Directory.CreateDirectory(config.BaseFilePath);
                return config;
            }
        }

        [Test]
        public void App_Create_SameId_DifferentBaseUri_ReturnsDifferentApps()
        {
            var config1 = GetConfig("https://localhost:443");
            var config2 = GetConfig("http://localhost:80");

            var app1 = CreateApp(config1);
            var app2 = CreateApp(config2);

            Assert.That(app1.Id, Is.EqualTo(app2.Id));
            Assert.That(app1.GetHashCode(), Is.EqualTo(app2.GetHashCode()));

            Assert.That(app1.Equals(app2), Is.False);
            Assert.That(app1 == app2, Is.False);
            Assert.That(app1 != app2, Is.True);
            Assert.That(app1.BaseUri, Is.Not.EqualTo(app2.BaseUri));
            Assert.That(app1.BaseFilePath, Is.Not.EqualTo(app2.BaseFilePath));

            static AppConfiguration GetConfig(string uri)
            {
                var baseFilePath = Path.Combine(InteropConfig.GetDefaultStorageFolder("no error expected"), Guid.NewGuid().ToString());
                var config = new AppConfiguration("abc")
                {
                    BaseFilePath = baseFilePath,
                    BaseUri = new Uri(uri)
                };

                Directory.CreateDirectory(config.BaseFilePath);
                return config;
            }
        }

        [Test]
        public void App_EqualsGetHashCodeTests()
        {
            var config1 = new AppConfiguration("abc");
            var config2 = new AppConfiguration("cde");
            var config3 = new AppConfiguration("abc");
            var config4 = new AppConfiguration("abc")
            {
                UseAppCache = false
            };

            var app1 = CreateApp(config1);
            var app2 = CreateApp(config2);
            var app3 = CreateApp(config3);
            var app4 = CreateApp(config4);

            Assert.That(app1.GetHashCode(), Is.Not.EqualTo(app2.GetHashCode()));
            Assert.That(app1.GetHashCode(), Is.EqualTo(app3.GetHashCode()));
            Assert.That(app1.GetHashCode(), Is.EqualTo(app4.GetHashCode()));

            Assert.That(app1.Equals(app2), Is.False);
            Assert.That(app1.Equals(app3), Is.True);
            Assert.That(app1.Equals(app4), Is.False); // app4 is uncached, so a different instance is returned

            Assert.That(app1 == app2, Is.False);
            Assert.That(app1 == app3, Is.True);
            Assert.That(app1 == app4, Is.False); // app4 is uncached, so a different instance is returned

            Assert.That(app1 != app2, Is.True);
            Assert.That(app1 != app3, Is.False);
            Assert.That(app1 != app4, Is.True); // app4 is uncached, so a different instance is returned

            Assert.That(app1.Equals(app1.Id), Is.False);
            Assert.That(app1.Equals(null), Is.False);
            Assert.That(app1 == null, Is.False);
            Assert.That(app1 != null, Is.True);

            App? app5 = null;

            Assert.That(app5 == null, Is.True);
            Assert.That(app5 != null, Is.False);
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
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var handler = new TestHttpClientHandler();

                var app = CreateApp(new AppConfiguration("abc")
                {
                    BaseUri = SyncTestHelpers.BaasUri!,
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

        [Test]
        public void RealmConfigurationBaseUrl_ReturnsExpectedValue()
        {
            var config = new AppConfiguration("abc");
            Assert.That(config.BaseUri, Is.EqualTo(new Uri("https://services.cloud.mongodb.com")));
        }
    }
}
