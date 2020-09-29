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
using System.Text;
using NUnit.Framework;
using Realms.Sync;

namespace Realms.Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class AppTests : SyncTestBase
    {
        [Test]
        public void AppCreate_CreatesApp()
        {
            // This is mostly a smoke test to ensure that nothing blows up when setting all properties.
            var config = new AppConfiguration("abc-123")
            {
                BaseUri = new Uri("http://foo.bar"),
                LocalAppName = "My app",
                LocalAppVersion = "1.2.3",
                LogLevel = LogLevel.All,
                MetadataEncryptionKey = new byte[64],
                MetadataPersistenceMode = MetadataPersistenceMode.Encrypted,
                BaseFilePath = InteropConfig.DefaultStorageFolder,
                CustomLogger = (message, level) => { },
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
                appConfig.LogLevel = logLevel;
                appConfig.CustomLogger = (message, level) =>
                {
                    lock (logBuilder)
                    {
                        logBuilder.AppendLine($"[{level}] {message}");
                    }
                };

                var app = CreateApp(appConfig);

                var config = await GetIntegrationConfigAsync(Guid.NewGuid().ToString());
                using var realm = await GetRealmAsync(config);
                realm.Write(() =>
                {
                    realm.Add(new PrimaryKeyStringObject { StringProperty = Guid.NewGuid().ToString() });
                });

                await WaitForUploadAsync(realm);

                var log = logBuilder.ToString();

                Assert.That(log, Does.Contain($"[{logLevel}]"));
                Assert.That(log, Does.Not.Contain($"[{logLevel - 1}]"));
            });
        }
    }
}
