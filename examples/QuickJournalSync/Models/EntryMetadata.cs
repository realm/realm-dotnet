using Realms;

namespace QuickJournalSync.Models
{
    public partial class EntryMetadata : IEmbeddedObject
    {
        public DateTimeOffset CreatedDate { get; set; }

        public DateTimeOffset LastModifiedDate { get; set; }
    }
}