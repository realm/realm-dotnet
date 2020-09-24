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
    public static class SyncTestHelpers
    {
        public const string DefaultPassword = "123456";

        private static AppConfiguration _baseConfig;

        public static AppConfiguration GetAppConfig() => new AppConfiguration(_baseConfig.AppId) { BaseUri = _baseConfig.BaseUri };

        public static void RunBaasTestAsync(Func<Task> testFunc, int timeout = 30000)
        {
            if (_baseConfig == null)
            {
                Assert.Ignore("MongoDB Realm is not setup.");
            }

            TestHelpers.RunAsyncTest(testFunc, timeout);
        }

        public static string GetVerifiedUsername() => $"realm_tests_do_autoverify-{Guid.NewGuid()}";

        public static string[] ExtractBaasSettings(string[] args)
        {
            var result = new List<string>();

            string baasUrl = null;
            string baasAppId = null;

            for (var i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--baasurl":
                        baasUrl = args[++i];
                        break;
                    case "--baasappid":
                        baasAppId = args[++i];
                        break;
                    default:
                        result.Add(args[i]);
                        break;
                }
            }

            if (baasUrl != null && baasAppId != null)
            {
                _baseConfig = new AppConfiguration(baasAppId)
                {
                    BaseUri = new Uri(baasUrl),
                };
            }

            return result.ToArray();
        }

        public static async Task<User> GetUserAsync(App app)
        {
            var username = GetVerifiedUsername();
            await app.EmailPasswordAuth.RegisterUserAsync(username, DefaultPassword);

            var credentials = Credentials.UsernamePassword(username, DefaultPassword);
            return await app.LogInAsync(credentials);
        }

        public static async Task<SyncConfiguration> GetFakeConfigAsync(string userId = null, string optionalPath = null)
        {
            throw new NotImplementedException();

            //var user = await GetFakeUserAsync(userId);
            //var serverUri = new Uri($"realm://localhost:9080/{Guid.NewGuid()}");
            //return new SyncConfiguration(serverUri, user, optionalPath);
        }

        public static Task<User> GetFakeUserAsync(App app, string id = null)
        {
            var handle = app.AppHandle.GetUserForTesting(id ?? Guid.NewGuid().ToString());
            return Task.FromResult(new User(handle));
        }

        public static async Task<SyncConfiguration> GetIntegrationConfigAsync(App app, string partition)
        {
            var user = await GetUserAsync(app);
            return new SyncConfiguration(partition, user);
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

        public static Task WaitForUploadAsync(Realm realm) => WaitForSyncAsync(realm, upload: true, download: false);

        public static Task WaitForDownloadAsync(Realm realm) => WaitForSyncAsync(realm, upload: false, download: true);

        public static async Task WaitForSyncAsync(Realm realm, bool upload = true, bool download = true)
        {
            var session = realm.GetSession();
            try
            {
                if (upload)
                {
                    await session.WaitForUploadAsync();
                }

                if (upload && download)
                {
                    await Task.Delay(50);
                }

                if (download)
                {
                    await session.WaitForDownloadAsync();
                }
            }
            finally
            {
                session.CloseHandle();
            }
        }
    }
}