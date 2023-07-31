using System.Diagnostics.CodeAnalysis;
using QuickJournalSync.Models;
using Realms;
using Realms.Sync;

namespace QuickJournalSync.Services
{
    public static class RealmService
    {
        private static readonly string _appId = "";

        private static bool _serviceInitialised;

        private static Realms.Sync.App? _app;

        private static Realm? _mainThreadRealm;

        private static FlexibleSyncConfiguration? _realmConfig;

        public static User? CurrentUser => _app?.CurrentUser;

        public static void Init()
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

            _serviceInitialised = true;
        }

        public static Realm GetMainThreadRealm()
        {
            return _mainThreadRealm ??= GetRealm();
        }

        public static Realm GetRealm()
        {
            return Realm.GetInstance(GetRealmConfig());
        }

        public static async Task<Realm> GetRealmAsync()
        {
            return await Realm.GetInstanceAsync(GetRealmConfig());
        }

        public static async Task RegisterAsync(string email, string password)
        {
            CheckIfInitialized();

            await _app.EmailPasswordAuth.RegisterUserAsync(email, password);
        }

        public static async Task LoginAsync(string email, string password)
        {
            CheckIfInitialized();

            await _app.LogInAsync(Credentials.EmailPassword(email, password));

            using var realm = await GetRealmAsync();
        }

        public static async Task LogoutAsync()
        {
            CheckIfInitialized();

            if (CurrentUser == null)
            {
                return;
            }

            await CurrentUser.LogOutAsync();
            _mainThreadRealm?.Dispose();
            _mainThreadRealm = null;
        }

        [MemberNotNull(nameof(_app))]
        private static void CheckIfInitialized()
        {
            if (_app == null)
            {
                throw new Exception("Remember to initialize RealmService!");
            }
        }

        private static FlexibleSyncConfiguration GetRealmConfig()
        {
            if (_realmConfig == null)
            {
                if (CurrentUser == null)
                {
                    throw new Exception("Cannot get Realm config before login!");
                }

                _realmConfig = new FlexibleSyncConfiguration(CurrentUser)
                {
                    PopulateInitialSubscriptions = (realm) =>
                    {
                        var query = realm.All<JournalEntry>().Where(j => j.UserId == CurrentUser.Id);
                        realm.Subscriptions.Add(query, new SubscriptionOptions { Name = "myEntries" });
                    }
                };
            }

            return _realmConfig;
        }
    }
}