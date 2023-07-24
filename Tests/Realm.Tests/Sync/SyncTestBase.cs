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
using System.Threading;
using System.Threading.Tasks;
using Baas;
using MongoDB.Bson;
using Realms.Sync;
using Realms.Sync.Exceptions;
using static Realms.Tests.TestHelpers;

namespace Realms.Tests.Sync
{
    [Preserve(AllMembers = true)]
    public abstract class SyncTestBase : RealmTest
    {
        private readonly ConcurrentQueue<StrongBox<Session>> _sessions = new();
        private readonly ConcurrentQueue<StrongBox<App>> _apps = new();
        private readonly ConcurrentQueue<StrongBox<string>> _clientResetAppsToRestore = new();

        protected App DefaultApp => CreateApp();

        protected App CreateApp(AppConfiguration? config = null)
        {
            config ??= SyncTestHelpers.GetAppConfig();

            var app = App.Create(config);
            _apps.Enqueue(app);

            return app;
        }

        protected override void CustomTearDown()
        {
            _sessions.DrainQueue(session => session?.CloseHandle());

            base.CustomTearDown();

            _apps.DrainQueue(app =>
            {
                if (!app.Handle.IsClosed)
                {
                    app.Handle.ResetForTesting();
                }
            });

            _clientResetAppsToRestore.DrainQueueAsync(appConfigType => SyncTestHelpers.SetRecoveryModeOnServer(appConfigType, enabled: true));
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

        // TODO: this method should go away once https://github.com/realm/realm-core/issues/5705 is resolved.
        protected static async Task WaitForSubscriptionsAsync(Realm realm)
        {
            await realm.Subscriptions.WaitForSynchronizationAsync();
            await WaitForDownloadAsync(realm);
        }

        protected static async Task<T> WaitForObjectAsync<T>(T obj, Realm realm2, string? message = null)
            where T : IRealmObject
        {
            var id = obj.DynamicApi.Get<RealmValue>("_id");

            return (await WaitForConditionAsync(() => realm2.FindCore<T>(id), o => o != null, errorMessage: message))!;
        }

        protected async Task<User> GetUserAsync(App? app = null, string? username = null, string? password = null)
        {
            app ??= DefaultApp;
            username ??= SyncTestHelpers.GetVerifiedUsername();
            password ??= SyncTestHelpers.DefaultPassword;
            await app.EmailPasswordAuth.RegisterUserAsync(username, password).Timeout(10_000, detail: "Failed to register user");
            var credentials = Credentials.EmailPassword(username, password);

            for (var i = 0; i < 5; i++)
            {
                try
                {
                    return await app.LogInAsync(credentials).Timeout(10_000, "Failed to login user");
                }
                catch (AppException ex) when (ex.Message.Contains("confirmation required"))
                {
                }
            }

            throw new Exception("Could not login user after 5 attempts.");
        }

        protected User GetFakeUser(App? app = null, string? id = null, string? refreshToken = null, string? accessToken = null)
        {
            app ??= DefaultApp;
            id ??= Guid.NewGuid().ToString();
            refreshToken ??= "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoicmVmcmVzaCB0b2tlbiIsImlhdCI6MTUxNjIzOTAyMiwiZXhwIjoyNTM2MjM5MDIyfQ.SWH98a-UYBEoJ7DLxpP7mdibleQFeCbGt4i3CrsyT2M";
            accessToken ??= "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiYWNjZXNzIHRva2VuIiwiaWF0IjoxNTE2MjM5MDIyLCJleHAiOjI1MzYyMzkwMjJ9.bgnlxP_mGztBZsImn7HaF-6lDevFDn2U_K7D8WUC2GQ";
            var handle = app.Handle.GetUserForTesting(id, refreshToken, accessToken);
            return new User(handle, app);
        }

        protected async Task<Realm> GetIntegrationRealmAsync(string? partition = null, App? app = null, int timeout = 10000)
        {
            var config = await GetIntegrationConfigAsync(partition, app);
            return await GetRealmAsync(config, timeout);
        }

        protected async Task<PartitionSyncConfiguration> GetIntegrationConfigAsync(string? partition = null, App? app = null, string? optionalPath = null, User? user = null)
        {
            app ??= DefaultApp;
            partition ??= Guid.NewGuid().ToString();

            user ??= await GetUserAsync(app);
            return UpdateConfig(new PartitionSyncConfiguration(partition, user, optionalPath));
        }

        protected async Task<PartitionSyncConfiguration> GetIntegrationConfigAsync(long? partition, App? app = null, string? optionalPath = null)
        {
            app ??= App.Create(SyncTestHelpers.GetAppConfig(AppConfigType.IntPartitionKey));

            var user = await GetUserAsync(app);
            return UpdateConfig(new PartitionSyncConfiguration(partition, user, optionalPath));
        }

        protected static PartitionSyncConfiguration GetIntegrationConfig(User user, string? partition = null, string? optionalPath = null)
        {
            partition ??= Guid.NewGuid().ToString();
            return UpdateConfig(new PartitionSyncConfiguration(partition, user, optionalPath));
        }

        protected async Task<PartitionSyncConfiguration> GetIntegrationConfigAsync(ObjectId? partition, App? app = null, string? optionalPath = null)
        {
            app ??= App.Create(SyncTestHelpers.GetAppConfig(AppConfigType.ObjectIdPartitionKey));

            var user = await GetUserAsync(app);
            return UpdateConfig(new PartitionSyncConfiguration(partition, user, optionalPath));
        }

        protected async Task<PartitionSyncConfiguration> GetIntegrationConfigAsync(Guid? partition, App? app = null, string? optionalPath = null)
        {
            app ??= App.Create(SyncTestHelpers.GetAppConfig(AppConfigType.UUIDPartitionKey));

            var user = await GetUserAsync(app);
            return UpdateConfig(new PartitionSyncConfiguration(partition, user, optionalPath));
        }

        protected async Task<FlexibleSyncConfiguration> GetFLXIntegrationConfigAsync(App? app = null, string? optionalPath = null)
        {
            app ??= App.Create(SyncTestHelpers.GetAppConfig(AppConfigType.FlexibleSync));
            var user = await GetUserAsync(app);
            return GetFLXIntegrationConfig(user, optionalPath);
        }

        protected static FlexibleSyncConfiguration GetFLXIntegrationConfig(User user, string? optionalPath = null)
        {
            return UpdateConfig(new FlexibleSyncConfiguration(user, optionalPath));
        }

        protected async Task<Realm> GetFLXIntegrationRealmAsync(App? app = null)
        {
            var config = await GetFLXIntegrationConfigAsync(app);
            return await GetRealmAsync(config);
        }

        protected async Task DisableClientResetRecoveryOnServer(string appConfigType)
        {
            await SyncTestHelpers.SetRecoveryModeOnServer(appConfigType, false);
            _clientResetAppsToRestore.Enqueue(appConfigType);
        }

        protected async Task<Realm> GetRealmAsync(SyncConfigurationBase config, bool waitForSync = false, int timeout = 10000, CancellationToken cancellationToken = default)
        {
            var realm = await GetRealmAsync(config, timeout, cancellationToken);
            if (waitForSync)
            {
                await WaitForUploadAsync(realm);
            }

            return realm;
        }

        private static T UpdateConfig<T>(T config)
            where T : SyncConfigurationBase
        {
            config.Schema = new[] { typeof(HugeSyncObject), typeof(PrimaryKeyStringObject), typeof(ObjectIdPrimaryKeyWithValueObject), typeof(SyncCollectionsObject), typeof(IntPropertyObject), typeof(EmbeddedIntPropertyObject), typeof(SyncAllTypesObject) };
            config.SessionStopPolicy = SessionStopPolicy.Immediately;

            return config;
        }

        protected PartitionSyncConfiguration GetFakeConfig(App? app = null, string? userId = null, string? optionalPath = null)
        {
            var user = GetFakeUser(app, userId);
            return UpdateConfig(new PartitionSyncConfiguration(Guid.NewGuid().ToString(), user, optionalPath));
        }

        protected FlexibleSyncConfiguration GetFakeFLXConfig(App? app = null, string? userId = null, string? optionalPath = null)
        {
            var user = GetFakeUser(app, userId);
            return UpdateConfig(new FlexibleSyncConfiguration(user, optionalPath));
        }

        protected async Task TriggerClientReset(Realm realm, bool restartSession = true)
        {
            if (realm.Config is not SyncConfigurationBase syncConfig)
            {
                throw new Exception("This should only be invoked for sync realms.");
            }

            var session = GetSession(realm);

            if (restartSession)
            {
                session.Stop();
            }

            await SyncTestHelpers.TriggerClientResetOnServer(syncConfig).Timeout(10_000, detail: "Trigger client reset");

            if (restartSession)
            {
                session.Start();
            }
        }
    }
}
