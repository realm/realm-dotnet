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
using System.Threading.Tasks;
using NUnit.Framework;
using Realms.Sync;
using Realms.Sync.Exceptions;
using Realms.Sync.Testing;

namespace Realms.Tests.Sync
{
    public enum AppConfigType
    {
        Default,
        IntPartitionKey,
        ObjectIdPartitionKey,
        UUIDPartitionKey,
    }

    public static class SyncTestHelpers
    {
        public const string DefaultPassword = "123456";

        private static readonly IDictionary<AppConfigType, string> _appIds = new Dictionary<AppConfigType, string>
        {
            [AppConfigType.Default] = "myapp-123",
        };

        private static Uri _baseUri;

        public static AppConfiguration GetAppConfig(AppConfigType type = AppConfigType.Default) => new AppConfiguration(_appIds[type])
        {
            BaseUri = _baseUri ?? new Uri("http://localhost:12345"),
            MetadataPersistenceMode = MetadataPersistenceMode.NotEncrypted,
        };

        public static void RunBaasTestAsync(Func<Task> testFunc, int timeout = 30000, bool ensureNoSessionErrors = false)
        {
            if (_baseUri == null || _appIds[AppConfigType.Default] == "myapp-123")
            {
                Assert.Ignore("MongoDB Realm is not setup.");
            }

            if (ensureNoSessionErrors)
            {
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

        public static string[] ExtractBaasSettings(string[] args)
        {
            var result = new List<string>();

            for (var i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--baasurl":
                        _baseUri = new Uri(args[++i]);
                        break;
                    case "--baasappid":
                        var appId = args[++i];
                        _appIds[GetConfigTypeForId(appId)] = appId;
                        break;
                    default:
                        result.Add(args[i]);
                        break;
                }
            }

            return result.ToArray();
        }

        public static Task<Tuple<Session, T>> SimulateSessionErrorAsync<T>(Session session, ErrorCode code, string message)
            where T : Exception
        {
            var tcs = new TaskCompletionSource<Tuple<Session, T>>();
            EventHandler<ErrorEventArgs> handler = null;
            handler = new EventHandler<ErrorEventArgs>((sender, e) =>
            {
                try
                {
                    Assert.That(sender, Is.TypeOf<Session>());
                    Assert.That(e.Exception, Is.TypeOf<T>());
                    tcs.TrySetResult(Tuple.Create((Session)sender, (T)e.Exception));
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }

                Session.Error -= handler;
            });

            Session.Error += handler;

            session.SimulateError(code, message);

            return tcs.Task;
        }

        private static AppConfigType GetConfigTypeForId(string appId)
        {
            if (appId.StartsWith("dotnet-integration-tests"))
            {
                return AppConfigType.Default;
            }

            if (appId.StartsWith("int-partition-key"))
            {
                return AppConfigType.IntPartitionKey;
            }

            throw new NotSupportedException($"Unexpected appId: {appId}");
        }
    }
}
