using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using QuickJournal.Models;
using Realms;

namespace QuickJournal.ViewModels
{
    public partial class JournalEntriesViewModel : ObservableObject
    {
        private readonly Realm realm;

        [ObservableProperty]
        private IQueryable<JournalEntry> entries;

        public JournalEntriesViewModel()
        {
            realm = Realm.GetInstance();
            Entries = realm.All<JournalEntry>();

            WeakReferenceMessenger.Default.Register<EntryModifiedMessage>(this, EntryModifiedHandler);
        }

        [RelayCommand]
        public async Task AddEntry()
        {
            var entry = new JournalEntry
            {
                Metadata = new EntryMetadata
                {
                    CreatedDate = DateTimeOffset.Now,
                }
            };

            realm.Write(() =>
            {
                realm.Add(entry);
            });

            await GoToEntry(entry);
        }

        [RelayCommand]
        public async Task EditEntry(JournalEntry entry)
        {
            await GoToEntry(entry);
        }

        [RelayCommand]
        public void DeleteEntry(JournalEntry entry)
        {
            realm.Write(() =>
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
                DeleteEntry(newEntry);
                await Toast.Make("Empty note discarded").Show();
            }
        }
    }
}

