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
using System.Threading;
using System.Threading.Tasks;
using Realms.Sync;

namespace Realms.Tests.Sync
{
    [Preserve(AllMembers = true)]
    public abstract class SyncTestBase : RealmTest
    {
        private readonly List<Session> _sessions = new List<Session>();
        private readonly List<App> _apps = new List<App>();

        protected App _app;

        protected App CreateApp(AppConfiguration config = null)
        {
            config ??= SyncTestHelpers.GetAppConfig();

            config.LogLevel = LogLevel.All;

            var app = App.Create(config);
            _apps.Add(app);
            return app;
        }

        protected override void CustomSetUp()
        {
            base.CustomSetUp();

            _app = CreateApp();
        }

        protected override void CustomTearDown()
        {
            base.CustomTearDown();

            foreach (var session in _sessions)
            {
                session?.CloseHandle();
            }

            foreach (var app in _apps)
            {
                app.AppHandle.ResetForTesting();
            }

            _app = null;
        }

        protected void CleanupOnTearDown(Session session)
        {
            _sessions.Add(session);
        }

        protected Session GetSession(Realm realm)
        {
            var result = realm.GetSession();
            CleanupOnTearDown(result);
            return result;
        }

        protected static async Task WaitForUploadAsync(Realm realm)
        {
            var session = realm.GetSession();
            await session.WaitForUploadAsync();
            session.CloseHandle();
        }

        protected static async Task WaitForDownloadAsync(Realm realm)
        {
            var session = realm.GetSession();
            await session.WaitForDownloadAsync();
            session.CloseHandle();
        }

        protected async Task<Realm> GetRealmAsync(RealmConfigurationBase config, bool openAsync = true, CancellationToken cancellationToken = default)
        {
            Realm result;
            if (openAsync)
            {
                result = await Realm.GetInstanceAsync(config, cancellationToken);
            }
            else
            {
                result = Realm.GetInstance(config);
                await SyncTestHelpers.WaitForDownloadAsync(result);
            }

            CleanupOnTearDown(result);
            return result;
        }

        protected async Task<User> GetUserAsync(App app = null)
        {
            app ??= _app;

            var username = SyncTestHelpers.GetVerifiedUsername();
            await app.EmailPasswordAuth.RegisterUserAsync(username, SyncTestHelpers.DefaultPassword);

            var credentials = Credentials.EmailPassword(username, SyncTestHelpers.DefaultPassword);
            return await app.LogInAsync(credentials);
        }

        protected User GetFakeUser(App app = null, string id = null)
        {
            app ??= _app;

            var handle = app.AppHandle.GetUserForTesting(id ?? Guid.NewGuid().ToString());
            return new User(handle);
        }

        protected async Task<SyncConfiguration> GetIntegrationConfigAsync(string partition, App app = null)
        {
            app ??= _app;

            var user = await GetUserAsync(app);
            return GetSyncConfiguration(partition, user);
        }

        protected SyncConfiguration GetSyncConfiguration(string partition, User user)
        {
            return new SyncConfiguration(partition, user)
            {
                ObjectClasses = new[] { typeof(HugeSyncObject) }
            };
        }

        public SyncConfiguration GetFakeConfig(App app = null, string userId = null, string optionalPath = null)
        {
            var user = GetFakeUser(app, userId);
            return new SyncConfiguration(Guid.NewGuid().ToString(), user, optionalPath);
        }
    }
}
