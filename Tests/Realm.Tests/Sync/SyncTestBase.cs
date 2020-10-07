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
using Realms.Sync;

namespace Realms.Tests.Sync
{
    [Preserve(AllMembers = true)]
    public abstract class SyncTestBase : RealmTest
    {
        private readonly List<Session> _sessions = new List<Session>();
        private readonly List<App> _apps = new List<App>();

        private App _defaultApp;

        protected App DefaultApp
        {
            get
            {
                return _defaultApp ?? CreateApp();
            }
        }

        protected App CreateApp(AppConfiguration config = null)
        {
            config ??= SyncTestHelpers.GetAppConfig();

            var app = App.Create(config);
            _apps.Add(app);

            if (_defaultApp == null)
            {
                _defaultApp = app;
            }

            return app;
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
                app.Handle.ResetForTesting();
            }

            _defaultApp = null;
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

        protected async Task<User> GetUserAsync(App app = null)
        {
            app ??= DefaultApp;

            var username = SyncTestHelpers.GetVerifiedUsername();
            await app.EmailPasswordAuth.RegisterUserAsync(username, SyncTestHelpers.DefaultPassword);

            var credentials = Credentials.EmailPassword(username, SyncTestHelpers.DefaultPassword);
            return await app.LogInAsync(credentials);
        }

        protected User GetFakeUser(App app = null, string id = null, string refreshToken = null, string accessToken = null)
        {
            app ??= DefaultApp;
            id ??= Guid.NewGuid().ToString();
            refreshToken ??= "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoicmVmcmVzaCB0b2tlbiIsImlhdCI6MTUxNjIzOTAyMiwiZXhwIjoyNTM2MjM5MDIyfQ.SWH98a-UYBEoJ7DLxpP7mdibleQFeCbGt4i3CrsyT2M";
            accessToken ??= "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiYWNjZXNzIHRva2VuIiwiaWF0IjoxNTE2MjM5MDIyLCJleHAiOjI1MzYyMzkwMjJ9.bgnlxP_mGztBZsImn7HaF-6lDevFDn2U_K7D8WUC2GQ";
            var handle = app.Handle.GetUserForTesting(id, refreshToken, accessToken);
            return new User(handle);
        }

        protected async Task<SyncConfiguration> GetIntegrationConfigAsync(string partition = null, App app = null)
        {
            app ??= DefaultApp;
            partition ??= Guid.NewGuid().ToString();

            var user = await GetUserAsync(app);
            return GetSyncConfiguration(partition, user);
        }

        protected static SyncConfiguration GetSyncConfiguration(string partition, User user, string optionalPath = null)
        {
            return new SyncConfiguration(partition, user, optionalPath)
            {
                ObjectClasses = new[] { typeof(HugeSyncObject), typeof(PrimaryKeyStringObject), typeof(ObjectIdPrimaryKeyWithValueObject) },
                SessionStopPolicy = SessionStopPolicy.Immediately,
            };
        }

        public SyncConfiguration GetFakeConfig(App app = null, string userId = null, string optionalPath = null)
        {
            var user = GetFakeUser(app, userId);
            return GetSyncConfiguration(Guid.NewGuid().ToString(), user, optionalPath);
        }
    }
}
