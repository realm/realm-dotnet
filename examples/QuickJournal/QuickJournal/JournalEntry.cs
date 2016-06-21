using System;
using System.ComponentModel;
using Realms;

namespace QuickJournal
{
    public class JournalEntry : RealmObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Title { get; set; }
        public DateTimeOffset Date { get; set; }
        public string BodyText { get; set; }
    }
}