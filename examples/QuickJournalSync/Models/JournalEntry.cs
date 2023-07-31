using Realms;

namespace QuickJournalSync.Models
{
    public partial class JournalEntry : IRealmObject
    {
        public string? Title { get; set; }

        public string? Body { get; set; }

        public EntryMetadata? Metadata { get; set; }
    }
}
