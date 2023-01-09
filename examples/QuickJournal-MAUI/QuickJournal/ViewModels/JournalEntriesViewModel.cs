using System;
using Realms;
using QuickJournal.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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
    }
}

