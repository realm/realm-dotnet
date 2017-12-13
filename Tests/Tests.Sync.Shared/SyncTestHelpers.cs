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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Realms;
using Realms.Sync;
using Realms.Sync.Exceptions;
using Realms.Sync.Testing;

namespace Tests.Sync
{
    public static class SyncTestHelpers
    {
        public static string DeveloperFeatureToken => GetFeatureToken("DEVELOPER");

        public static string ProfessionalFeatureToken => GetFeatureToken("PROFESSIONAL");

        public static string EnterpriseFeatureToken => GetFeatureToken("ENTERPRISE");

        public static Credentials CreateCredentials()
        {
            return Credentials.UsernamePassword(Guid.NewGuid().ToString(), "a", createUser: true);
        }

        public static Credentials AdminCredentials()
        {
            return Credentials.UsernamePassword(Constants.AdminUsername, Constants.AdminPassword, createUser: false);
        }

        public static void RequiresRos()
        {
            if (Constants.RosUrl == null)
            {
                Assert.Ignore("ROS is not setup.");
            }
        }

        public static string[] ExtractRosSettings(string[] args)
        {
            var result = new List<string>();

            for (var i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--ros":
                        Constants.RosUrl = args[++i];
                        break;
                    case "--rosport":
                        Constants.RosPort = args[++i];
                        break;
                    case "--rossecureport":
                        Constants.RosSecurePort = args[++i];
                        break;
                    default:
                        result.Add(args[i]);
                        break;
                }
            }

            return result.ToArray();
        }

        public static Uri AuthServerUri => new Uri($"http://{Constants.RosUrl}:{Constants.RosPort}");

        public static Uri RealmUri(string path) => new Uri($"realm://{Constants.RosUrl}:{Constants.RosPort}/{path.TrimStart('/')}");

        public static Uri SecureRealmUri(string path) => new Uri($"realms://{Constants.RosUrl}:{Constants.RosSecurePort}/{path.TrimStart('/')}");

        public static async Task<Exception> VerifyRosRunningAsync(CancellationToken cancellationToken)
        {
            using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) })
            {
                var success = false;
                Exception result = null;
                while (!success && !cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var response = await client.GetAsync($"http://{Constants.RosUrl}:{Constants.RosPort}/health");
                        success = response.IsSuccessStatusCode;
                    }
                    catch (Exception ex)
                    {
                        result = ex;
                        await Task.Delay(1000);
                    }
                }

                return success ? null : result;
            }
        }

        public static Task<User> GetUserAsync()
        {
            var credentials = CreateCredentials();
            return User.LoginAsync(credentials, AuthServerUri);
        }

        public static async Task<SyncConfiguration> GetFakeConfigAsync(string userId = null, string optionalPath = null)
        {
            var user = await GetFakeUserAsync(userId);
            var serverUri = new Uri("realm://localhost:9080/foobar");
            return new SyncConfiguration(user, serverUri, optionalPath);
        }

        public static Task<User> GetFakeUserAsync(string id = null, string scheme = "http")
        {
            var handle = SyncUserHandle.GetSyncUser(id ?? Guid.NewGuid().ToString(), $"{scheme}://some.fake.server:9080", string.Empty, isAdmin: true);
            return Task.FromResult(new User(handle));
        }

        public static async Task<SyncConfiguration> GetIntegrationConfigAsync(string path)
        {
            var user = await GetUserAsync();
            return new SyncConfiguration(user, RealmUri($"~/{path}"));
        }

        public static async Task<Realm> GetInstanceAsync(SyncConfiguration config, bool openAsync, bool waitForRemote = true)
        {
            if (openAsync)
            {
                return await Realm.GetInstanceAsync(config);
            }

            var realm = Realm.GetInstance(config);

            if (waitForRemote)
            {
                var session = realm.GetSession();
                try
                {
                    await session.WaitForDownloadAsync();
                }
                finally
                {
                    session.CloseHandle();
                }
            }

            return realm;
        }

        public static Task<Tuple<Session, T>> SimulateSessionErrorAsync<T>(Session session, ErrorCode code, string message) where T : Exception
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

        private static string GetFeatureToken(string prefix) => Environment.GetEnvironmentVariable($"{prefix}_FEATURE_TOKEN");
    }
}