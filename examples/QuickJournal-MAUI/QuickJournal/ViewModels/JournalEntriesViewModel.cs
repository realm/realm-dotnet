using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuickJournal.Models;
using Realms;

namespace QuickJournal.ViewModels
{
    [QueryProperty(nameof(NewEntry), nameof(NewEntry))]
    public partial class JournalEntriesViewModel : ObservableObject
    {
        private readonly Realm realm;
        private IDisposable token;

        [ObservableProperty]
        private IQueryable<JournalEntry> entries;

        [ObservableProperty]
        private JournalEntry newEntry;

        public JournalEntriesViewModel()
        {
            realm = Realm.GetInstance();
            Entries = realm.All<JournalEntry>();

            //TODO For testing, need to remove this
            if (!realm.All<JournalEntry>().Any())
            {
                realm.Write(() =>
                {
                    var entry = new JournalEntry
                    {
                        Title = "Test title",
                        Body = "Haha",
                        Metadata = new EntryMetadata
                        {
                            CreatedDate = DateTimeOffset.Now,
                        }
                    };
                    realm.Add(entry);

                });
            }
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
                { "Entry", entry }
            };
            await Shell.Current.GoToAsync($"entryDetail", navigationParameter);
        }

        async partial void OnNewEntryChanged(JournalEntry value)
        {
            if (string.IsNullOrEmpty(newEntry.Body + newEntry.Title)) ;
            {
                //DeleteEntry(newEntry);

                ////var toast = Toast.Make("Entry removed");

                ////await toast.Show();
            }
        }
    }
}

