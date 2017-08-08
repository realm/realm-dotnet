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
        public static Credentials CreateCredentials()
        {
            return Credentials.UsernamePassword(Guid.NewGuid().ToString(), "a", createUser: true);
        }

        public static Credentials AdminCredentials()
        {
            return Credentials.UsernamePassword(Constants.AdminUsername, Constants.AdminPassword, createUser: false);
        }

        public static readonly Uri AuthServerUri = new Uri($"http://{Constants.ServerUrl}:9080");

        public static Uri RealmUri(string path) => new Uri($"realm://{Constants.ServerUrl}:9080/{path.TrimStart('/')}");

        public static Uri SecureRealmUri(string path) => new Uri($"realms://{Constants.ServerUrl}:9443/{path.TrimStart('/')}");

        public static Task<User> GetUserAsync()
        {
            var credentials = CreateCredentials();
            return User.LoginAsync(credentials, AuthServerUri);
        }

        public static async Task<SyncConfiguration> GetFakeConfigAsync()
        {
            var user = await GetFakeUserAsync();
            var serverUri = new Uri("realm://localhost:9080/foobar");
            return new SyncConfiguration(user, serverUri);
        }

        public static Task<User> GetFakeUserAsync(string token = "foo:bar", string scheme = "http") => User.LoginAsync(Credentials.AdminToken(token), new Uri($"{scheme}://some.fake.server:9080"));

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
    }
}