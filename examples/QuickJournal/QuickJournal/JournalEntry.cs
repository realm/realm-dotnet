using System;
using System.ComponentModel;
using Realms;

namespace QuickJournal
{
    public class JournalEntry : RealmObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Title { get; set; }
        public string BodyText { get; set; }

        public EntryMetadata Metadata { get; set; }

        // If we remove that and use Metadata.Date in the binding, exception is thrown when deleting item. See #883.
        public DateTimeOffset Date => Metadata.Date;
    }
}