using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using QuickJournalSync.Messages;
using QuickJournalSync.Models;
using QuickJournalSync.Services;
using Realms;
using Realms.Sync;

namespace QuickJournalSync.ViewModels;

public partial class EntriesViewModel : BaseViewModel
{
    private readonly Realm _realm;

    private bool isOnline = true;

    [ObservableProperty]
    private string connectionStatusIcon = "wifi_on.png";

    [ObservableProperty]
    private IQueryable<JournalEntry>? _entries;

    [ObservableProperty]
    private ConnectionState _connectionState;

    public EntriesViewModel()
    {
        _realm = RealmService.GetMainThreadRealm();
        ConnectionState = _realm.SyncSession.ConnectionState;
        Entries = _realm.All<JournalEntry>();
    }

    [RelayCommand]
    public void OnAppearing()
    {
        RealmService.SyncConnectionStateChanged += HandleSyncConnectionStateChanged;

        // We are using a WeakReferenceManager here to get notified when JournalEntriesDetailPage is closed.
        // This could have been implemeted hooking up on the back button behaviour
        // (with Shell.BackButtonBehaviour), but there is a current bug in MAUI
        // that would make the application crash (https://github.com/dotnet/maui/pull/11438)
        WeakReferenceMessenger.Default.Register<EntryModifiedMessage>(this, EntryModifiedHandler);
    }

    [RelayCommand]
    public void OnDisappearing()
    {
        RealmService.SyncConnectionStateChanged -= HandleSyncConnectionStateChanged;
        WeakReferenceMessenger.Default.Unregister<EntryModifiedMessage>(this);
    }

    [RelayCommand]
    public async Task AddEntry()
    {
        var entry = await _realm.WriteAsync(() =>
        {
            return _realm.Add(new JournalEntry());
        });

        await GoToEntry(entry);
    }

    [RelayCommand]
    public async Task EditEntry(JournalEntry entry)
    {
        await GoToEntry(entry);
    }

    [RelayCommand]
    public async Task DeleteEntry(JournalEntry entry)
    {
        await _realm.WriteAsync(() =>
        {
            _realm.Remove(entry);
        });
    }

    [RelayCommand]
    public async Task Logout()
    {
        IsBusy = true;
        await RealmService.LogoutAsync();
        IsBusy = false;

        // This is the simplest way to avoid reusing pages after logout
        Application.Current!.MainPage = new AppShell();
    }

    [RelayCommand]
    public async Task SimulateSessionError()
    {
        await RealmService.SimulateSessionError();
    }

    [RelayCommand]
    public async Task SimulateSubscriptionError()
    {
        await RealmService.SimulateSubscriptionError();
    }

    [RelayCommand]
    public async Task SimulateClientReset()
    {
        // More information about manual testing of client reset at https://www.mongodb.com/docs/realm/sdk/dotnet/sync/client-reset/#test-client-reset-handling.
        await DialogService.ShowAlertAsync("Client Reset",
            "You can simulate a client reset by terminating an re-enabling Device Sync.",
            "OK");
    }

    [RelayCommand]
    public void ChangeConnectionStatus()
    {
        isOnline = !isOnline;

        if (isOnline)
        {
            _realm.SyncSession.Start();
        }
        else
        {
            _realm.SyncSession.Stop();
        }

        ConnectionStatusIcon = isOnline ? "wifi_on.png" : "wifi_off.png";
    }

    private void HandleSyncConnectionStateChanged(object? sender, ConnectionState newConnectionState)
    {
        ConnectionState = newConnectionState;
    }

    private async Task GoToEntry(JournalEntry entry)
    {
        var navigationParameter = new Dictionary<string, object>
        {
            { "Entry", entry },
        };
        await Shell.Current.GoToAsync($"entryDetail", navigationParameter);
    }

    private async void EntryModifiedHandler(object recipient, EntryModifiedMessage message)
    {
        var newEntry = message.Value;
        if (string.IsNullOrEmpty(newEntry.Body + newEntry.Title))
        {
            await DeleteEntry(newEntry);
            await Toast.Make("Empty note discarded").Show();
        }
    }
}