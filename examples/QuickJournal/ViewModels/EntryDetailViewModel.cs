using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using QuickJournal.Messages;
using QuickJournal.Models;

namespace QuickJournal.ViewModels
{
    [QueryProperty("Entry", nameof(Entry))]
    public partial class EntryDetailViewModel : ObservableObject
    {
        [ObservableProperty]
        private JournalEntry entry = null!;

        [RelayCommand]
        public void OnPageClosed()
        {
            WeakReferenceMessenger.Default.Send(new EntryModifiedMessage(Entry));
        }
    }
}