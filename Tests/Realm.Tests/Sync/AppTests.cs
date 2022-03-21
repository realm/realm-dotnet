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
using System.Threading.Tasks;
using NUnit.Framework;
using Realms.Logging;
using Realms.Sync;

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
                BaseFilePath = InteropConfig.DefaultStorageFolder,
                CustomLogger = (message, level) => { },
                DefaultRequestTimeout = TimeSpan.FromSeconds(123)
            };

#pragma warning restore CS0618 // Type or member is obsolete

            var app = CreateApp(config);
            Assert.That(app.Sync, Is.Not.Null);
        }

        [Test, RequiresBaas]
        public async Task App_Login_Anonymous()
        {
            var user = await DefaultApp.LogInAsync(Credentials.Anonymous());
            Assert.That(user, Is.Not.Null);
            Assert.That(user.Id, Is.Not.Null);
        }

        [TestCase(LogLevel.Debug)]
        [TestCase(LogLevel.Info)]
        [RequiresBaas]
        public async Task App_WithCustomLogger_LogsSyncOperations(LogLevel logLevel)
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
        }

        [TestCase(LogLevel.Debug)]
        [TestCase(LogLevel.Info)]
        [RequiresBaas]
        public async Task RealmConfiguration_WithCustomLogger_LogsSyncOperations(LogLevel logLevel)
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
        }
    }
}
