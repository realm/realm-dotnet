using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using MongoDB.Bson;
using QuickJournalSync.Models;
using Realms;
using Realms.Sync;
using Realms.Sync.ErrorHandling;
using Realms.Sync.Exceptions;

namespace QuickJournalSync.Services
{
    public static class RealmService
    {
        private static readonly string _appId = "";

        private static object _mainRealmLock = new();

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
            lock (_mainRealmLock)
            {
                if (_mainThreadRealm is null)
                {
                    var mainThreadConfig = GetRealmConfig(sessionErrorCallback: HandleSessionErrorCallback,
                        clientResetHandler: MakeClientResetHandler());
                    _mainThreadRealm = Realm.GetInstance(mainThreadConfig);
                    _session = _mainThreadRealm.SyncSession;
                    _session.PropertyChanged += HandleSyncSessionPropertyChanged;
                }
            }

            return _mainThreadRealm;
        }

        public static Realm GetBackgroundThreadRealm() => Realm.GetInstance(GetRealmConfig());

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

        private static FlexibleSyncConfiguration GetRealmConfig(SyncConfigurationBase.SessionErrorCallback? sessionErrorCallback = null,
            ClientResetHandlerBase? clientResetHandler = null)
        {
            if (CurrentUser == null)
            {
                throw new InvalidOperationException("Cannot get Realm config before login!");
            }

            var config = new FlexibleSyncConfiguration(CurrentUser)
            {
                PopulateInitialSubscriptions = (realm) =>
                {
                    var query = realm.All<JournalEntry>().Where(j => j.UserId == CurrentUser.Id);
                    realm.Subscriptions.Add(query, new SubscriptionOptions { Name = "myEntries" });
                },
            };

            if (sessionErrorCallback is not null)
            {
                config.OnSessionError = sessionErrorCallback;
            }

            if (clientResetHandler is not null)
            {
                config.ClientResetHandler = clientResetHandler;
            }

            return config;
        }

        #region Subscription Error

        public static async Task SimulateSubscriptionError()
        {
            const string subErrorName = "subError";
            var realm = GetMainThreadRealm();

            // Here we are adding a subscription on a property (Title) that is not in the list of queryable fields on the service.
            // This will raise a SubscriptionException when waiting for the synchronization of the subscriptions.
            // In order for this to work you need to disable Development Mode, otherwise Title will be added in the queryable fields
            // automatically, and there will be no error.
            realm.Subscriptions.Update(() =>
            {
                var queryFake = realm.All<JournalEntry>().Where(j => j.Title == "test");
                realm.Subscriptions.Add(queryFake, new SubscriptionOptions { Name = subErrorName });
            });

            try
            {
                await realm.Subscriptions.WaitForSynchronizationAsync();
            }
            catch (SubscriptionException ex)
            {
                LogAndShowToast($"Subscription Error: {ex}");

                // Removing the invalid subscription
                realm.Subscriptions.Update(() =>
                {
                    realm.Subscriptions.Remove(subErrorName);
                });
            }
        }

        #endregion

        #region Session Error

        public static async Task SimulateSessionError()
        {
            var realm = GetMainThreadRealm();

            // Here we are adding an object that has a UserId different from the id of the current user.
            // This means that the object is outside of the current query subscriptions, and as such will provoke a session error.
            await realm.WriteAsync(() =>
            {
                return realm.Add(new JournalEntry
                {
                    CreatedDate = DateTimeOffset.Now,
                    UserId = ObjectId.GenerateNewId().ToString(),
                });
            });
        }

        private static void HandleSessionErrorCallback(Session session, SessionException error)
        {
            LogAndShowToast($"Session error! {error}");
        }

        #endregion

        private static void HandleSyncSessionPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var session = (Session)sender!;

            if (e.PropertyName == nameof(Session.ConnectionState))
            {
                LogAndShowToast($"New connection state: {session.ConnectionState}");
                SyncConnectionStateChanged?.Invoke(null, session.ConnectionState);
            }
        }

        #region ClientReset

        private static ClientResetHandlerBase MakeClientResetHandler()
        {
            return new RecoverOrDiscardUnsyncedChangesHandler()
            {
                OnBeforeReset = HandleBeforeClientReset,
                OnAfterRecovery = HandleAfterClientResetRecovery,
                OnAfterDiscard = HandleAfterClientResetDiscard,
                ManualResetFallback = HandleManualReset,
            };
        }

        private static void HandleBeforeClientReset(Realm beforeFrozen)
        {
            // Callback invoked right before a Client Reset.
            LogAndShowToast("Before Reset Callback called");
        }

        private static void HandleAfterClientResetRecovery(Realm beforeFrozen, Realm after)
        {
            // Callback invoked right after a Client Reset just succeeded.
            LogAndShowToast("After Reset Discard Callback called");
        }

        private static void HandleAfterClientResetDiscard(Realm beforeFrozen, Realm after)
        {
            // Callback invoked right after a Client Reset that fell back to discard unsynced changes.
            LogAndShowToast("After Reset Discard Callback called");
        }

        private static async void HandleManualReset(ClientResetException clientResetException)
        {
            // Callback invoked if automatic Client Reset handling fails.
            LogAndShowToast("Manual Reset Callback called");
        }

        #endregion

        private static void LogAndShowToast(string text)
        {
            Console.WriteLine(text);
            MainThread.BeginInvokeOnMainThread(async () => await DialogService.ShowToast(text));
        }
    }
}