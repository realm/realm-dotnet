using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using QuickJournal.Messages;
using QuickJournal.Models;

namespace QuickJournal.ViewModels
{
    [QueryProperty("Entry", nameof(Entry))]
    public partial class JournalEntryDetailViewModel : ObservableObject
    {
        [ObservableProperty]
        private JournalEntry entry;

        [RelayCommand]
        public void OnPageClosed()
        {
            // We are using a WeakReferenceManager here to notify the JournalEntriesViewModel
            // when the JournalEntriesDetailPage is closed.
            // This could have been implemeted hooking up on the back button behaviour
            // (with Shell.BackButtonBehaviour), but there is a current bug in MAUI
            // that would make the application crash (https://github.com/dotnet/maui/pull/11438)
            WeakReferenceMessenger.Default.Send(new EntryModifiedMessage(entry));
        }
    }
}

