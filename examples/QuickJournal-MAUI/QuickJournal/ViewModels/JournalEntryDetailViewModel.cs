using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using QuickJournal.Models;

namespace QuickJournal.ViewModels
{
    [QueryProperty("Entry", nameof(Entry))]
    public partial class JournalEntryDetailViewModel : ObservableObject
    {
        [ObservableProperty]
        private JournalEntry entry;

        public JournalEntryDetailViewModel()
        {
        }
    }
}

