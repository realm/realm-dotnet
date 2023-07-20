﻿using Realms;
using Realms.Sync;

namespace AnalyticsTelemetrics.Services
{
    public static class RealmService
    {
        // Add your App ID here
        private const string _appId = "";

        private static bool _serviceInitialised;

        private static Realms.Sync.App _app;

        public static async Task Init()
        {
            if (_serviceInitialised)
            {
                return;
            }

            var appConfiguration = new AppConfiguration(_appId);

            _app = Realms.Sync.App.Create(appConfiguration);
            await _app.LogInAsync(Credentials.Anonymous());

            _serviceInitialised = true;
        }

        public static Realm GetRealm()
        {
            var config = new FlexibleSyncConfiguration(_app.CurrentUser);

            return Realm.GetInstance(config);
        }
    }
}