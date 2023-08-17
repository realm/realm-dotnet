using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using QuickJournalSync.Messages;
using QuickJournalSync.Models;

namespace QuickJournalSync.ViewModels;

[QueryProperty("Entry", nameof(Entry))]
public partial class EntryDetailViewModel : BaseViewModel
{
    [ObservableProperty]
    private JournalEntry entry = null!;

    [RelayCommand]
    public void OnPageClosed()
    {
        WeakReferenceMessenger.Default.Send(new EntryModifiedMessage(Entry));
    }
}