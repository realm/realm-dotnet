using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using QuickJournalSync.Models;
using Realms;
using Realms.Sync;
using Realms.Sync.Exceptions;

namespace QuickJournalSync.Services
{
    public static class RealmService
    {
        private static readonly string _appId = "application-quickjournal-amhqr";

        private static bool _serviceInitialised;

        private static Realms.Sync.App? _app;

        private static Realm? _mainThreadRealm;

        private static Session? _session;

        public static User? CurrentUser => _app?.CurrentUser;

        public static event EventHandler<ConnectionState>? SyncConnectionStateChanged;

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
            //TODO Maybe put a lock around this?
            if (_mainThreadRealm is null)
            {
                _mainThreadRealm = Realm.GetInstance(GetRealmConfig());
                _session = _mainThreadRealm.SyncSession;
                _session.PropertyChanged += HandleSyncSessionPropertyChanged;
            }

            return _mainThreadRealm;
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

            using var realm = await Realm.GetInstanceAsync(GetRealmConfig());  //TODO Should we do this like in the code example? (put a try catch around this to catch TaskCanceledException)...?
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

            if (_session is not null)
            {
                _session.PropertyChanged -= HandleSyncSessionPropertyChanged;
                _session = null;
            }
        }

        [MemberNotNull(nameof(_app))]
        private static void CheckIfInitialized()
        {
            if (_app == null)
            {
                throw new InvalidOperationException("Remember to initialize RealmService!");
            }
        }

        private static FlexibleSyncConfiguration GetRealmConfig()
        {
            if (CurrentUser == null)
            {
                throw new InvalidOperationException("Cannot get Realm config before login!");
            }

            return new FlexibleSyncConfiguration(CurrentUser)
            {
                PopulateInitialSubscriptions = (realm) =>
                {
                    var query = realm.All<JournalEntry>().Where(j => j.UserId == CurrentUser.Id);
                    realm.Subscriptions.Add(query, new SubscriptionOptions { Name = "myEntries" });
                },
                OnSessionError = HandleSessionErrorCallback  //TODO Is this good here? 
            };
        }

        private static void HandleSessionErrorCallback(Session session, SessionException error)
        {
            Console.WriteLine($"Session error! {error}");
        }

        private static void HandleSyncSessionPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Why it does not get called?
            var session = (Session)sender!;

            Console.WriteLine("SYNC-SESSION CHANGE:" + e.PropertyName);

            if (e.PropertyName == nameof(Session.ConnectionState))
            {
                Console.WriteLine($"New connection state: {session.ConnectionState}");
                SyncConnectionStateChanged?.Invoke(null, session.ConnectionState);
            }
        }
    }
}