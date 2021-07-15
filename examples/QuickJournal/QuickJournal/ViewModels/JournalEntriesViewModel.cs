using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using QuickJournal.Models;
using QuickJournal.Services;
using Realms;
using Xamarin.Forms;

namespace QuickJournal.ViewModels
{
    public class JournalEntriesViewModel : BaseViewModel
    {
        readonly JournalEntry selectedEntry = null;
        readonly Realm realm;

        public IEnumerable<JournalEntry> Entries { get; private set; }

        public ICommand AddEntryCommand { get; private set; }
        public ICommand EditEntryCommand { get; private set; }
        public ICommand DeleteEntryCommand { get; private set; }

        public INavigation Navigation { get; set; }

        public JournalEntry SelectedEntry
        {
            get => selectedEntry;
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

            InitCommands();
        }

        private void InitCommands()
        {
            AddEntryCommand = new Command(async () => await AddEntry());
            DeleteEntryCommand = new Command<JournalEntry>(DeleteEntry);
            EditEntryCommand = new Command<JournalEntry>(async (e) => await EditEntry(e));
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

