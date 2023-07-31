using QuickJournalSync.Models;
using Realms;
using Realms.Sync;

namespace QuickJournalSync.Services
{
    public static class RealmService
    {
        private static readonly string appId = "";

        private static bool serviceInitialised;

        private static Realms.Sync.App? app;

        private static Realm? mainThreadRealm;

        public static User? CurrentUser => app?.CurrentUser;

        public static void Init()
        {
            if (serviceInitialised)
            {
                return;
            }

            if (string.IsNullOrEmpty(appId))
            {
                throw new Exception("Remember to add your appId!");
            }

            var appConfiguration = new AppConfiguration(appId);

            app = Realms.Sync.App.Create(appConfiguration);

            serviceInitialised = true;
        }

        public static Realm GetMainThreadRealm()
        {
            return mainThreadRealm ??= GetRealm();
        }

        public static Realm GetRealm()
        {
            var config = new FlexibleSyncConfiguration(CurrentUser) //TODO wed don't need to recreate this every time
            {
                PopulateInitialSubscriptions = (realm) =>
                {
                    var query = realm.All<JournalEntry>().Where(j => j.UserId == CurrentUser.Id);
                    realm.Subscriptions.Add(query, new SubscriptionOptions { Name = "myEntries" });
                }
            };

            return Realm.GetInstance(config);
        }

        public static async Task RegisterAsync(string email, string password)
        {
            await app.EmailPasswordAuth.RegisterUserAsync(email, password);
        }

        public static async Task LoginAsync(string email, string password)
        {
            await app.LogInAsync(Credentials.EmailPassword(email, password));

            using var realm = GetRealm();
            await realm.Subscriptions.WaitForSynchronizationAsync();
        }

        public static async Task LogoutAsync()
        {
            await CurrentUser.LogOutAsync();
            mainThreadRealm?.Dispose();
            mainThreadRealm = null;
        }
    }
}