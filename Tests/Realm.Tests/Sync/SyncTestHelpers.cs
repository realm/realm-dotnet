////////////////////////////////////////////////////////////////////////////
//
// Copyright 2017 Realm Inc.
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
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Baas;
using Nito.AsyncEx;
using NUnit.Framework;
using Realms.Logging;
using Realms.Sync;

namespace Realms.Tests.Sync
{
    public static class SyncTestHelpers
    {
        public const string DefaultPassword = "123456";
        private const string DummyAppId = "myapp-123";

        private static IDictionary<string, BaasClient.BaasApp> _apps = new Dictionary<string, BaasClient.BaasApp>
        {
            [AppConfigType.Default] = new(string.Empty, DummyAppId, AppConfigType.Default),
        };

        public static Uri? BaasUri;
        private static BaasClient? _baasClient;
        private static string? _baaSaasApiKey;

        static SyncTestHelpers()
        {
#if !UNITY
            try
            {
                var uri = ConfigHelpers.GetSetting("BaasUrl");
                if (uri != null)
                {
                    BaasUri = new Uri(uri);
                }

                _baaSaasApiKey = ConfigHelpers.GetSetting("BaaSaasApiKey");
            }
            catch
            {
            }
#endif
        }

        private static int _appCounter;

        public static AppConfiguration GetAppConfig(string type = AppConfigType.Default) => new(_apps[type].ClientAppId)
        {
            BaseUri = BaasUri ?? new Uri("http://localhost:12345"),
            MetadataPersistenceMode = MetadataPersistenceMode.NotEncrypted,
#pragma warning disable CA1837 // Use Environment.ProcessId instead of Process.GetCurrentProcess().Id
            BaseFilePath = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"rt-sync-{System.Diagnostics.Process.GetCurrentProcess().Id}-{_appCounter++}")).FullName
#pragma warning restore CA1837 // Use Environment.ProcessId instead of Process.GetCurrentProcess().Id
        };

        public static string RemoteMongoDBName(string prefix = "Schema") => $"{prefix}_{_baasClient?.Differentiator}";

        public static string SyncMongoDBName(string type = AppConfigType.Default) => _baasClient!.GetSyncDatabaseName(type);

        public static void RunBaasTestAsync(Func<Task> testFunc, int timeout = 30000)
        {
            if (BaasUri == null && _baaSaasApiKey == null)
            {
                Assert.Ignore("Atlas App Services are not setup.");
            }

            AsyncContext.Run(async () =>
            {
                await CreateBaasAppsAsync();
            });

            TestHelpers.RunAsyncTest(testFunc, timeout);

            // TODO: remove when https://github.com/realm/realm-core/issues/6052 is fixed
            Task.Delay(1000).Wait();
        }

        public static string GetVerifiedUsername() => $"realm_tests_do_autoverify-{Guid.NewGuid()}@g.it";

        public static string GetUnconfirmedUsername() => $"realm_tests_do_not_confirm-{Guid.NewGuid()}@g.it";

        public static async Task TriggerClientResetOnServer(SyncConfigurationBase config)
        {
            var userId = config.User.Id;
            var appId = string.Empty;

            if (config is FlexibleSyncConfiguration)
            {
                appId = _apps[AppConfigType.FlexibleSync].AppId;
            }

            var result = await config.User.Functions.CallAsync<BaasClient.FunctionReturn>("triggerClientResetOnSyncServer", userId, appId);
            if (result.Deleted > 0)
            {
                // This is kind of a hack, but it appears like there's a race condition on the server, where the deletion might not be
                // registered and the server will not respond with a client reset. Doing the request again gives the server some extra time
                // to process the deletion.
                result = await config.User.Functions.CallAsync<BaasClient.FunctionReturn>("triggerClientResetOnSyncServer", userId, appId);
                Assert.That(result.Deleted, Is.EqualTo(0));
            }
        }

        public static async Task<string[]> ExtractBaasSettingsAsync(string[] args)
        {
            string[] remainingArgs;
            (_baasClient, BaasUri, remainingArgs) = await BaasClient.CreateClientFromArgs(args, TestHelpers.Output);

            if (_baasClient != null)
            {
                _apps = await _baasClient.GetOrCreateApps();
            }

            return remainingArgs;
        }

        public static (string[] RemainingArgs, IDisposable? Logger) SetLoggerFromArgs(string[] args)
        {
            var (extracted, remaining) = ArgumentHelper.ExtractArguments(args, "realmloglevel", "realmlogfile");

            if (extracted.TryGetValue("realmloglevel", out var logLevelStr) && Enum.TryParse<LogLevel>(logLevelStr, out var logLevel))
            {
                TestHelpers.Output.WriteLine($"Setting log level to {logLevel}");

                Logger.LogLevel = logLevel;
            }

            Logger.AsyncFileLogger? logger = null;
            if (extracted.TryGetValue("realmlogfile", out var logFile))
            {
                if (!Process.GetCurrentProcess().ProcessName.ToLower().Contains("testhost"))
                {
                    TestHelpers.Output.WriteLine($"Setting sync logger to file: {logFile}");

                    // We're running in a test runner, so we need to use the sync logger
                    Logger.Default = Logger.File(logFile);
                }
                else
                {
                    TestHelpers.Output.WriteLine($"Setting async logger to file: {logFile}");

                    // We're running standalone (likely on CI), so we use the async logger
                    Logger.Default = logger = new Logger.AsyncFileLogger(logFile);
                }
            }

            return (remaining, logger);
        }

        public static string[] ExtractBaasSettings(string[] args) => AsyncContext.Run(() => ExtractBaasSettingsAsync(args));

        private static async Task CreateBaasAppsAsync()
        {
            if (_apps[AppConfigType.Default].AppId != string.Empty || (BaasUri == null && _baaSaasApiKey == null))
            {
                return;
            }

            // TODO Add correct differentiator
            if (_baaSaasApiKey != null)
            {
                await BaasClient.GetOrDeployContainer(_baaSaasApiKey, "local", TestHelpers.Output);
            }

#if !UNITY
            try
            {
                var cluster = ConfigHelpers.GetSetting("Cluster")!;
                var apiKey = ConfigHelpers.GetSetting("ApiKey")!;
                var privateApiKey = ConfigHelpers.GetSetting("PrivateApiKey")!;
                var groupId = ConfigHelpers.GetSetting("GroupId")!;
                var differentiator = ConfigHelpers.GetSetting("Differentiator") ?? "local";

                _baasClient ??= await BaasClient.Atlas(BaasUri, differentiator, TestHelpers.Output, cluster, apiKey, privateApiKey, groupId);
            }
            catch
            {
            }
#endif

            _baasClient ??= await BaasClient.Docker(BaasUri, "local", TestHelpers.Output);

            _apps = await _baasClient.GetOrCreateApps();
        }

        public static Task SetRecoveryModeOnServer(string appConfigType, bool enabled)
        {
            var app = _apps[appConfigType];
            return _baasClient!.SetAutomaticRecoveryEnabled(app, enabled);
        }
    }
}
