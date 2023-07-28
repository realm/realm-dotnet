using Realms;
using Realms.Sync;

namespace AnalyticsTelemetry.Services
{
    public static class RealmService
    {
        // Add your App ID here
        private const string _appId = "";

        private static bool _serviceInitialised;

        private static Realms.Sync.App? _app;

        public static async Task Init()
        {
            if (_serviceInitialised)
            {
                return;
            }

            if (string.IsNullOrEmpty(_appId))
            {
                throw new Exception("Remember to add your appId!");
            }

            var appConfiguration = new AppConfiguration(_appId);

            _app = Realms.Sync.App.Create(appConfiguration);
            await _app.LogInAsync(Credentials.Anonymous());

            _serviceInitialised = true;
        }

        public static Realm GetRealm()
        {
            if (_app?.CurrentUser == null)
            {
                throw new Exception("Need to call RealmService.Init before calling this method!");
            }

            var config = new FlexibleSyncConfiguration(_app.CurrentUser);

            return Realm.GetInstance(config);
        }
    }
}