using System;
using Realms;

namespace QuickJournal
{
    public class JournalEntry : RealmObject
    {
        public DateTimeOffset Date { get; set; }
        public string BodyText { get; set; }
    }
}