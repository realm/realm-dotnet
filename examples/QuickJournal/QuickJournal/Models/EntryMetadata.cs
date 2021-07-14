using System;
using Realms;

namespace QuickJournal.Models
{
    public class EntryMetadata : EmbeddedObject
    {
        public DateTimeOffset Date { get; set; }

        public string Author { get; set; }
    }
}