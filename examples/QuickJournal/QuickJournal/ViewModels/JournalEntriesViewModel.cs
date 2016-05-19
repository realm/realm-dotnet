using System;
using System.Collections.Generic;
using System.Windows.Input;
using Realms;
using Xamarin.Forms;

namespace QuickJournal
{
    public class JournalEntriesViewModel
    {
        private Realm _realm;

        public IEnumerable<JournalEntry> Entries { get; private set; }

        public ICommand AddEntryCommand { get; private set; }

        public ICommand DeleteEntryCommand { get; private set; }

        public INavigation Navigation { get; set; }

        public JournalEntriesViewModel()
        {
            _realm = Realm.GetInstance();

            Entries = _realm.All<JournalEntry>();

            AddEntryCommand = new Command(AddEntry);
            DeleteEntryCommand = new Command<JournalEntry>(DeleteEntry);
        }

        private void AddEntry()
        {
            var transaction = _realm.BeginWrite();
            var entry = _realm.CreateObject<JournalEntry>();
            entry.Date = DateTimeOffset.Now;

            var page = new JournalEntryDetailsPage(new JournalEntryDetailsViewModel(entry, transaction));

            Navigation.PushAsync(page);
        }

        internal void EditEntry(JournalEntry entry)
        {
            var transaction = _realm.BeginWrite();

            var page = new JournalEntryDetailsPage(new JournalEntryDetailsViewModel(entry, transaction));

            Navigation.PushAsync(page);
        }

        private void DeleteEntry(JournalEntry entry)
        {
            _realm.Write(() => _realm.Remove(entry));
        }
    }
}

