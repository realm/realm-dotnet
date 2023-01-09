using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
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
            WeakReferenceMessenger.Default.Send(new EntryModifiedMessage(entry));
        }
    }

    public class EntryModifiedMessage : ValueChangedMessage<JournalEntry>
    {
        public EntryModifiedMessage(JournalEntry entry) : base(entry)
        {
        }
    }
}

