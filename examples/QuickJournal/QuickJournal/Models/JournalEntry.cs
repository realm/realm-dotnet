using System;
using Realms;

namespace QuickJournal.Models
{
    public class JournalEntry : RealmObject
    {
        public string Title { get; set; }

        public string BodyText { get; set; }

        public EntryMetadata Metadata { get; set; }

        public DateTimeOffset Date => Metadata.Date;
    }
}