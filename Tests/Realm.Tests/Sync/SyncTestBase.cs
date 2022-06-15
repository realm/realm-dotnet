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
using System.Threading.Tasks;
using Baas;
using MongoDB.Bson;
using Realms.Sync;
using Realms.Sync.Exceptions;

namespace Realms.Tests.Sync
{
    [Preserve(AllMembers = true)]
    public abstract class SyncTestBase : RealmTest
    {
        private readonly ConcurrentQueue<Session> _sessions = new();
        private readonly ConcurrentQueue<App> _apps = new();

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
            var result = realm.SyncSession;
            CleanupOnTearDown(result);
            return result;
        }

        protected static async Task WaitForUploadAsync(Realm realm)
        {
            var session = realm.SyncSession;
            await session.WaitForUploadAsync();
            session.CloseHandle();
        }

        protected static async Task WaitForDownloadAsync(Realm realm)
        {
            var session = realm.SyncSession;
            await session.WaitForDownloadAsync();
            session.CloseHandle();
        }

        protected static async Task<T> WaitForObjectAsync<T>(T obj, Realm realm2)
            where T : RealmObject
        {
            await WaitForUploadAsync(obj.Realm);
            await WaitForDownloadAsync(realm2);

            var id = obj.DynamicApi.Get<RealmValue>("_id");

            return await TestHelpers.WaitForConditionAsync(() => realm2.FindCore<T>(id), o => o != null);
        }

        protected async Task<User> GetUserAsync(App app = null, string username = null, string password = null)
        {
            app ??= DefaultApp;
            username ??= SyncTestHelpers.GetVerifiedUsername();
            password ??= SyncTestHelpers.DefaultPassword;
            await app.EmailPasswordAuth.RegisterUserAsync(username, password);
            var credentials = Credentials.EmailPassword(username, password);

            for (var i = 0; i < 5; i++)
            {
                try
                {
                    return await app.LogInAsync(credentials);
                }
                catch (AppException ex) when (ex.Message.Contains("confirmation required"))
                {
                }
            }

            throw new Exception("Could not login user after 5 attempts.");
        }

        protected User GetFakeUser(App app = null, string id = null, string refreshToken = null, string accessToken = null)
        {
            app ??= DefaultApp;
            id ??= Guid.NewGuid().ToString();
            refreshToken ??= "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoicmVmcmVzaCB0b2tlbiIsImlhdCI6MTUxNjIzOTAyMiwiZXhwIjoyNTM2MjM5MDIyfQ.SWH98a-UYBEoJ7DLxpP7mdibleQFeCbGt4i3CrsyT2M";
            accessToken ??= "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiYWNjZXNzIHRva2VuIiwiaWF0IjoxNTE2MjM5MDIyLCJleHAiOjI1MzYyMzkwMjJ9.bgnlxP_mGztBZsImn7HaF-6lDevFDn2U_K7D8WUC2GQ";
            var handle = app.Handle.GetUserForTesting(id, refreshToken, accessToken);
            return new User(handle, app);
        }

        protected async Task<Realm> GetIntegrationRealmAsync(string partition = null, App app = null)
        {
            var config = await GetIntegrationConfigAsync(partition, app);
            return await GetRealmAsync(config);
        }

        protected async Task<PartitionSyncConfiguration> GetIntegrationConfigAsync(string partition = null, App app = null, string optionalPath = null)
        {
            app ??= DefaultApp;
            partition ??= Guid.NewGuid().ToString();

            var user = await GetUserAsync(app);
            return UpdateConfig(new PartitionSyncConfiguration(partition, user, optionalPath));
        }

        protected async Task<PartitionSyncConfiguration> GetIntegrationConfigAsync(long? partition, App app = null, string optionalPath = null)
        {
            app ??= App.Create(SyncTestHelpers.GetAppConfig(AppConfigType.IntPartitionKey));

            var user = await GetUserAsync(app);
            return UpdateConfig(new PartitionSyncConfiguration(partition, user, optionalPath));
        }

        protected async Task<PartitionSyncConfiguration> GetIntegrationConfigAsync(ObjectId? partition, App app = null, string optionalPath = null)
        {
            app ??= App.Create(SyncTestHelpers.GetAppConfig(AppConfigType.ObjectIdPartitionKey));

            var user = await GetUserAsync(app);
            return UpdateConfig(new PartitionSyncConfiguration(partition, user, optionalPath));
        }

        protected async Task<PartitionSyncConfiguration> GetIntegrationConfigAsync(Guid? partition, App app = null, string optionalPath = null)
        {
            app ??= App.Create(SyncTestHelpers.GetAppConfig(AppConfigType.UUIDPartitionKey));

            var user = await GetUserAsync(app);
            return UpdateConfig(new PartitionSyncConfiguration(partition, user, optionalPath));
        }

        protected async Task<FlexibleSyncConfiguration> GetFLXIntegrationConfigAsync(App app = null, string optionalPath = null)
        {
            app ??= App.Create(SyncTestHelpers.GetAppConfig(AppConfigType.FlexibleSync));
            var user = await GetUserAsync(app);
            return UpdateConfig(new FlexibleSyncConfiguration(user, optionalPath));
        }

        protected async Task<Realm> GetFLXIntegrationRealmAsync(App app = null)
        {
            var config = await GetFLXIntegrationConfigAsync(app);
            return await GetRealmAsync(config);
        }

        private static T UpdateConfig<T>(T config)
            where T : SyncConfigurationBase
        {
            config.Schema = new[] { typeof(HugeSyncObject), typeof(PrimaryKeyStringObject), typeof(ObjectIdPrimaryKeyWithValueObject), typeof(SyncCollectionsObject), typeof(IntPropertyObject), typeof(EmbeddedIntPropertyObject), typeof(SyncAllTypesObject) };
            config.SessionStopPolicy = SessionStopPolicy.Immediately;

            return config;
        }

        public PartitionSyncConfiguration GetFakeConfig(App app = null, string userId = null, string optionalPath = null)
        {
            var user = GetFakeUser(app, userId);
            return UpdateConfig(new PartitionSyncConfiguration(Guid.NewGuid().ToString(), user, optionalPath));
        }

        public FlexibleSyncConfiguration GetFakeFLXConfig(App app = null, string userId = null, string optionalPath = null)
        {
            var user = GetFakeUser(app, userId);
            return UpdateConfig(new FlexibleSyncConfiguration(user, optionalPath));
        }
    }
}
