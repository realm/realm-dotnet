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
using MongoDB.Bson;
using Realms.Sync;

namespace Realms.Tests.Sync
{
    [Preserve(AllMembers = true)]
    public abstract class SyncTestBase : RealmTest
    {
        private readonly Queue<Session> _sessions = new Queue<Session>();
        private readonly Queue<App> _apps = new Queue<App>();

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
            _apps.Enqueue(app);

            if (_defaultApp == null)
            {
                _defaultApp = app;
            }

            return app;
        }

        protected override void CustomTearDown()
        {
            _sessions.DrainQueue(session => session?.CloseHandle());

            // Race condition:
            // When trying to delete the Realm it can in some occasions (usually when Sync is involved)
            // still be in use. To make sure other threads that use the same Realm get scheduled again
            // and can finish their work before we actually delete the Realm files, we have to wait for
            // a moment here.
            // TODO: remove this when https://github.com/realm/realm-core/issues/4762 is resolved.
            Task.Delay(5).Wait();

            base.CustomTearDown();

            _apps.DrainQueue(app => app.Handle.ResetForTesting());

            _defaultApp = null;
        }

        protected void CleanupOnTearDown(Session session)
        {
            _sessions.Enqueue(session);
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

        protected async Task<Realm> GetIntegrationRealmAsync(string partition = null, App app = null)
        {
            var config = await GetIntegrationConfigAsync(partition, app);
            return await GetRealmAsync(config);
        }

        protected async Task<SyncConfiguration> GetIntegrationConfigAsync(string partition = null, App app = null, string optionalPath = null)
        {
            app ??= DefaultApp;
            partition ??= Guid.NewGuid().ToString();

            var user = await GetUserAsync(app);
            return UpdateConfig(new SyncConfiguration(partition, user, optionalPath));
        }

        protected async Task<SyncConfiguration> GetIntegrationConfigAsync(long? partition, App app = null, string optionalPath = null)
        {
            app ??= App.Create(SyncTestHelpers.GetAppConfig(AppConfigType.IntPartitionKey));

            var user = await GetUserAsync(app);
            return UpdateConfig(new SyncConfiguration(partition, user, optionalPath));
        }

        protected async Task<SyncConfiguration> GetIntegrationConfigAsync(ObjectId? partition, App app = null, string optionalPath = null)
        {
            app ??= App.Create(SyncTestHelpers.GetAppConfig(AppConfigType.ObjectIdPartitionKey));

            var user = await GetUserAsync(app);
            return UpdateConfig(new SyncConfiguration(partition, user, optionalPath));
        }

        protected async Task<SyncConfiguration> GetIntegrationConfigAsync(Guid? partition, App app = null, string optionalPath = null)
        {
            app ??= App.Create(SyncTestHelpers.GetAppConfig(AppConfigType.UUIDPartitionKey));

            var user = await GetUserAsync(app);
            return UpdateConfig(new SyncConfiguration(partition, user, optionalPath));
        }

        private static SyncConfiguration UpdateConfig(SyncConfiguration config)
        {
            config.ObjectClasses = new[] { typeof(HugeSyncObject), typeof(PrimaryKeyStringObject), typeof(ObjectIdPrimaryKeyWithValueObject), typeof(SyncCollectionsObject), typeof(IntPropertyObject), typeof(EmbeddedIntPropertyObject), typeof(SyncAllTypesObject) };
            config.SessionStopPolicy = SessionStopPolicy.Immediately;

            return config;
        }

        public SyncConfiguration GetFakeConfig(App app = null, string userId = null, string optionalPath = null)
        {
            var user = GetFakeUser(app, userId);
            return UpdateConfig(new SyncConfiguration(Guid.NewGuid().ToString(), user, optionalPath));
        }
    }
}
