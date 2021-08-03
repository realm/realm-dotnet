using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using QuickJournal.Models;
using QuickJournal.Services;
using Realms;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace QuickJournal.ViewModels
{
    public class JournalEntriesViewModel : BaseViewModel
    {
        private readonly Realm realm;

        public IEnumerable<JournalEntry> Entries { get; }

        public ICommand AddEntryCommand { get; }
        public ICommand EditEntryCommand { get; }
        public ICommand DeleteEntryCommand { get; }

        public JournalEntry SelectedEntry
        {
            get => null;
            set
            {
                EditEntryCommand.Execute(value);
                OnPropertyChanged();
            }
        }

        public JournalEntriesViewModel()
        {
            realm = Realm.GetInstance();
            Entries = realm.All<JournalEntry>();

            AddEntryCommand = new AsyncCommand(AddEntry);
            DeleteEntryCommand = new Command<JournalEntry>(DeleteEntry);
            EditEntryCommand = new AsyncCommand<JournalEntry>(EditEntry);
        }

        private async Task AddEntry()
        {
            var transaction = realm.BeginWrite();
            var entry = realm.Add(new JournalEntry
            {
                Metadata = new EntryMetadata
                {
                    CreatedDate = DateTimeOffset.Now,
                }
            });

            await NavigationService.NavigateTo(new JournalEntryDetailsViewModel(entry, transaction));
        }

        private async Task EditEntry(JournalEntry entry)
        {
            var transaction = realm.BeginWrite();

            await NavigationService.NavigateTo(new JournalEntryDetailsViewModel(entry, transaction));
        }

        private void DeleteEntry(JournalEntry entry)
        {
            realm.Write(() => realm.Remove(entry));
        }
    }
}

