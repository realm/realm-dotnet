using System;
using Realms;

namespace QuickJournal.Models
{
    public partial class EntryMetadata : IEmbeddedObject
    {
        public DateTimeOffset CreatedDate { get; set; }

        public DateTimeOffset LastModifiedDate { get; set; }
    }
}

