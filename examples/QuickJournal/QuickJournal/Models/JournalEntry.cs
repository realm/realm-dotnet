using Realms;

namespace QuickJournal.Models
{
    public class JournalEntry : RealmObject
    {
        public string Title { get; set; }

        public string Body { get; set; }

        public EntryMetadata Metadata { get; set; }
    }
}