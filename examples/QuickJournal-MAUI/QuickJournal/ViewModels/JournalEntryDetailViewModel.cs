using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuickJournal.Models;

namespace QuickJournal.ViewModels
{
    [QueryProperty("Entry", nameof(Entry))]
    public partial class JournalEntryDetailViewModel : ObservableObject
    {
        [ObservableProperty]
        private JournalEntry entry;

        [RelayCommand]
        public async Task GoBack()
        {
            var navigationParameter = new Dictionary<string, object>
            {
                { "NewEntry", entry }
            };
            await Shell.Current.GoToAsync($"..", navigationParameter);
        }
    }
}

