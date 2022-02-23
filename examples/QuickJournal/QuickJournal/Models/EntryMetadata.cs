using System;
using Realms;

namespace QuickJournal.Models
{
    public class EntryMetadata : EmbeddedObject
    {
        public DateTimeOffset CreatedDate { get; set; }

        public DateTimeOffset LastModifiedDate { get; set; }
    }
}