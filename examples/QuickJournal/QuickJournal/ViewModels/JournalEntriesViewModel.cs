using System;
using System.Collections.Generic;
using System.Windows.Input;
using Realms;
using Xamarin.Forms;

namespace QuickJournal
{
    public class JournalEntriesViewModel
    {
        // TODO: add UI for changing that.
        private const string AuthorName = "Me";

        private Realm _realm;

        public IEnumerable<JournalEntry> Entries { get; private set; }

        public ICommand AddEntryCommand { get; private set; }

        public ICommand DeleteEntryCommand { get; private set; }

        public INavigation Navigation { get; set; }

        public JournalEntriesViewModel()
        {
            _realm = Realm.GetInstance();

            Entries = _realm.All<JournalEntry>()
                            .ToNotifyCollectionChanged(e =>
                            {
                                // recover from the error - recreate the query or show message to the user
                                System.Diagnostics.Debug.WriteLine(e);
                            }) as IEnumerable<JournalEntry>;

            AddEntryCommand = new Command(AddEntry);
            DeleteEntryCommand = new Command<JournalEntry>(DeleteEntry);
        }

        private void AddEntry()
        {
            var transaction = _realm.BeginWrite();
            var entry = _realm.CreateObject<JournalEntry>();
            var metadata = _realm.CreateObject<EntryMetadata>();
            metadata.Date = DateTimeOffset.Now;
            metadata.Author = AuthorName;
            entry.Metadata = metadata;

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

