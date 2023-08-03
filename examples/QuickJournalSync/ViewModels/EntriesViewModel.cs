using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using QuickJournalSync.Messages;
using QuickJournalSync.Models;
using QuickJournalSync.Services;
using Realms;

namespace QuickJournalSync.ViewModels
{
    public partial class EntriesViewModel : BaseViewModel
    {
        private readonly Realm realm;

        [ObservableProperty]
        private IQueryable<JournalEntry>? entries;

        public EntriesViewModel()
        {
            realm = RealmService.GetMainThreadRealm();
            Entries = realm.All<JournalEntry>();
        }

        [RelayCommand]
        public void OnAppearing()
        {
            RealmService.SyncConnectionStateChanged += HandleSyncConnectionStateChanged;

            // We are using a WeakReferenceManager here to get notified when JournalEntriesDetailPage is closed.
            // This could have been implemeted hooking up on the back button behaviour
            // (with Shell.BackButtonBehaviour), but there is a current bug in MAUI
            // that would make the application crash (https://github.com/dotnet/maui/pull/11438)
            WeakReferenceMessenger.Default.Register<EntryModifiedMessage>(this, EntryModifiedHandler);  //TODO Check if we want to remove this
        }

        [RelayCommand]
        public void OnDisappearing()
        {
            RealmService.SyncConnectionStateChanged -= HandleSyncConnectionStateChanged;
            WeakReferenceMessenger.Default.Unregister<EntryModifiedMessage>(this);
        }

        [RelayCommand]
        public async Task AddEntry()
        {
            var entry = await realm.WriteAsync(() =>
            {
                return realm.Add(new JournalEntry
                {
                    CreatedDate = DateTimeOffset.Now,
                });
            });

            await GoToEntry(entry);
        }

        [RelayCommand]
        public async Task EditEntry(JournalEntry entry)
        {
            await GoToEntry(entry);
        }

        [RelayCommand]
        public async Task DeleteEntry(JournalEntry entry)
        {
            await realm.WriteAsync(() =>
            {
                realm.Remove(entry);
            });
        }

        [RelayCommand]
        public async Task Logout()
        {
            IsBusy = true;
            await RealmService.LogoutAsync();
            IsBusy = false;

            await Shell.Current.GoToAsync($"//login");
        }

        private void HandleSyncConnectionStateChanged(object? sender, Realms.Sync.ConnectionState e)
        {
            MainThread.BeginInvokeOnMainThread(() => DialogService.ShowToast(e.ToString()));
        }

        private async Task GoToEntry(JournalEntry entry)
        {
            var navigationParameter = new Dictionary<string, object>
            {
                { "Entry", entry },
            };
            await Shell.Current.GoToAsync($"entryDetail", navigationParameter);
        }

        private async void EntryModifiedHandler(object recipient, EntryModifiedMessage message)
        {
            var newEntry = message.Value;
            if (string.IsNullOrEmpty(newEntry.Body + newEntry.Title))
            {
                await DeleteEntry(newEntry);
                await Toast.Make("Empty note discarded").Show();
            }
        }
    }
}