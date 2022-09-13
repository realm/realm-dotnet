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
using System.IO;
using System.Threading.Tasks;
using Baas;
using Nito.AsyncEx;
using NUnit.Framework;
using Realms.Sync;

namespace Realms.Tests.Sync
{
    public static partial class SyncTestHelpers
    {
        public const string DefaultPassword = "123456";
        private const string DummyAppId = "myapp-123";

        private static IDictionary<string, BaasClient.BaasApp> _apps = new Dictionary<string, BaasClient.BaasApp>
        {
            [AppConfigType.Default] = new(string.Empty, DummyAppId, AppConfigType.Default),
        };

        private static Uri _baseUri;
        private static BaasClient _baasClient;

        static SyncTestHelpers()
        {
#if !UNITY
            try
            {
                _baseUri = new Uri(System.Configuration.ConfigurationManager.AppSettings["BaasUrl"]);
            }
            catch
            {
            }
#endif
        }

        private static int _appCounter;

        public static AppConfiguration GetAppConfig(string type = AppConfigType.Default) => new(_apps[type].ClientAppId)
        {
            BaseUri = _baseUri ?? new Uri("http://localhost:12345"),
            MetadataPersistenceMode = MetadataPersistenceMode.NotEncrypted,
#pragma warning disable CA1837 // Use Environment.ProcessId instead of Process.GetCurrentProcess().Id
            BaseFilePath = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"rt-sync-{System.Diagnostics.Process.GetCurrentProcess().Id}-{_appCounter++}")).FullName
#pragma warning restore CA1837 // Use Environment.ProcessId instead of Process.GetCurrentProcess().Id
        };

        public static string RemoteMongoDBName(string prefix = "Schema") => $"{prefix}_{_baasClient?.Differentiator}";

        public static void RunBaasTestAsync(Func<Task> testFunc, int timeout = 30000, bool ensureNoSessionErrors = false)
        {
            if (_baseUri == null)
            {
                Assert.Ignore("Atlas App Services are not setup.");
            }

            AsyncContext.Run(async () =>
            {
                await CreateBaasAppsAsync();
            });

            if (ensureNoSessionErrors)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                var tcs = new TaskCompletionSource<object>();
                Session.Error += HandleSessionError;
                try
                {
                    TestHelpers.RunAsyncTest(testFunc, timeout, tcs.Task);
                }
                finally
                {
                    Session.Error -= HandleSessionError;
                }
#pragma warning restore CS0618 // Type or member is obsolete

                void HandleSessionError(object _, ErrorEventArgs errorArgs)
                {
                    tcs.TrySetException(errorArgs.Exception);
                }
            }
            else
            {
                TestHelpers.RunAsyncTest(testFunc, timeout);
            }
        }

        public static string GetVerifiedUsername() => $"realm_tests_do_autoverify-{Guid.NewGuid()}";

        public static async Task TriggerClientResetOnServer(SyncConfigurationBase config)
        {
            var userId = config.User.Id;
            var appId = string.Empty;

            if (config is FlexibleSyncConfiguration)
            {
                _apps.TryGetValue(AppConfigType.FlexibleSync, out var app);
                appId = app.AppId;
            }

            var result = await config.User.Functions.CallAsync<BaasClient.FunctionReturn>("triggerClientResetOnSyncServer", userId, appId);
            Assert.That(result.status, Is.EqualTo(BaasClient.FunctionReturn.Result.success));
        }

        public static async Task<string[]> ExtractBaasSettingsAsync(string[] args)
        {
            string[] remainingArgs;
            (_baasClient, _baseUri, remainingArgs) = await BaasClient.CreateClientFromArgs(args, TestHelpers.Output);

            if (_baasClient != null)
            {
                _apps = await _baasClient.GetOrCreateApps();
            }

            return remainingArgs;
        }

        public static string[] ExtractBaasSettings(string[] args)
        {
            return AsyncContext.Run(async () =>
            {
                return await ExtractBaasSettingsAsync(args);
            });
        }

        private static async Task CreateBaasAppsAsync()
        {
            if (_apps[AppConfigType.Default].AppId != string.Empty || _baseUri == null)
            {
                return;
            }

#if !UNITY
            try
            {
                var cluster = System.Configuration.ConfigurationManager.AppSettings["Cluster"];
                var apiKey = System.Configuration.ConfigurationManager.AppSettings["ApiKey"];
                var privateApiKey = System.Configuration.ConfigurationManager.AppSettings["PrivateApiKey"];
                var groupId = System.Configuration.ConfigurationManager.AppSettings["GroupId"];

                _baasClient = await BaasClient.Atlas(_baseUri, "local", TestHelpers.Output, cluster, apiKey, privateApiKey, groupId);
            }
            catch
            {
            }
#endif

            _baasClient ??= await BaasClient.Docker(_baseUri, "local", TestHelpers.Output);

            _apps = await _baasClient.GetOrCreateApps();
        }

        public static Task SetRecoveryModeOnServer(string appConfigType, bool enabled)
        {
            var app = _apps[appConfigType];
            return _baasClient.SetAutomaticRecoveryEnabled(app, enabled);
        }
    }
}
