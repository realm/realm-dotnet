using System;
using System.ComponentModel;
using Realms;

namespace QuickJournal
{
    public class EntryMetadata : RealmObject
    {
        public DateTimeOffset Date { get; set; }

        public string Author { get; set; }
    }
}