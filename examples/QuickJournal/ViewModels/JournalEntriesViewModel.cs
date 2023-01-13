using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using QuickJournal.Messages;
using QuickJournal.Models;
using Realms;

namespace QuickJournal.ViewModels
{
    public partial class JournalEntriesViewModel : ObservableObject
    {
        private readonly Realm realm;

        [ObservableProperty]
        private IQueryable<JournalEntry>? entries;

        [ObservableProperty]
        private string test;

        public JournalEntriesViewModel()
        {
            realm = Realm.GetInstance();
            Entries = realm.All<JournalEntry>();

            // We are using a WeakReferenceManager here to get notified when JournalEntriesDetailPage is closed.
            // This could have been implemeted hooking up on the back button behaviour
            // (with Shell.BackButtonBehaviour), but there is a current bug in MAUI
            // that would make the application crash (https://github.com/dotnet/maui/pull/11438)
            WeakReferenceMessenger.Default.Register< EntryModifiedMessage>(this, EntryModifiedHandler);
        }

        [RelayCommand]
        public async Task AddEntry()
        {
            var entry = await realm.WriteAsync(() =>
            {
                return realm.Add(new JournalEntry
                {
                    Metadata = new EntryMetadata
                    {
                        CreatedDate = DateTimeOffset.Now,
                    }
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

