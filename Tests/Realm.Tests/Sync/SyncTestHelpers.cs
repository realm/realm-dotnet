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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Baas;
using Nito.AsyncEx;
using NUnit.Framework;
using Realms.Logging;
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

            // TODO: remove when https://github.com/realm/realm-core/issues/6052 is fixed
            Task.Delay(1000).Wait();
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
            (_baasClient, _baseUri, remainingArgs) = await BaasClient.CreateClientFromArgs(args, TestHelpers.Output);

            if (_baasClient != null)
            {
                _apps = await _baasClient.GetOrCreateApps();
            }

            return remainingArgs;
        }

        public static (string[] RemainingArgs, AsyncFileLogger Logger) SetLoggerFromArgs(string[] args)
        {
            var (extracted, remaining) = ArgumentHelper.ExtractArguments(args, "realmloglevel", "realmlogfile");

            if (extracted.TryGetValue("realmloglevel", out var logLevelStr) && Enum.TryParse<LogLevel>(logLevelStr, out var logLevel))
            {
                TestHelpers.Output.WriteLine($"Setting log level to {logLevel}");

                Logger.LogLevel = logLevel;
            }

            AsyncFileLogger logger = null;
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
                    Logger.Default = logger = new AsyncFileLogger(logFile);
                }
            }

            return (remaining, logger);
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

                _baasClient ??= await BaasClient.Atlas(_baseUri, "local", TestHelpers.Output, cluster, apiKey, privateApiKey, groupId);
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

        public class AsyncFileLogger : Logger, IDisposable
        {
            private readonly ConcurrentQueue<string> _queue = new();
            private readonly string _filePath;
            private readonly Encoding _encoding;
            private readonly AutoResetEvent _hasNewItems = new(false);
            private readonly AutoResetEvent _flush = new(false);
            private readonly Task _runner;
            private volatile bool _isFlushing;

            public AsyncFileLogger(string filePath, Encoding encoding = null)
            {
                _filePath = filePath;
                _encoding = encoding ?? Encoding.UTF8;
                _runner = Task.Run(Run);
            }

            public void Dispose()
            {
                _isFlushing = true;
                _flush.Set();
                _runner.Wait();

                _hasNewItems.Dispose();
                _flush.Dispose();
            }

            protected override void LogImpl(LogLevel level, string message)
            {
                if (!_isFlushing)
                {
                    _queue.Enqueue(FormatLog(level, message));
                    _hasNewItems.Set();
                }
            }

            private void Run()
            {
                while (true)
                {
                    WaitHandle.WaitAny(new[] { _hasNewItems, _flush });

                    var sb = new StringBuilder();
                    while (_queue.TryDequeue(out var item))
                    {
                        sb.AppendLine(item);
                    }

                    System.IO.File.AppendAllText(_filePath, sb.ToString(), _encoding);

                    if (_isFlushing)
                    {
                        return;
                    }
                }
            }
        }
    }
}
