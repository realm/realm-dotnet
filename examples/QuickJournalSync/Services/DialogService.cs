using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using QuickJournalSync.Views;

namespace QuickJournalSync.Services;

public static class DialogService
{
    private static Page MainPage => Application.Current?.MainPage ??
        throw new InvalidOperationException("Cannot show dialogs without a Main Page!");

    public static Task ShowAlertAsync(string title, string message, string accept)
    {
        return MainPage.DisplayAlert(title, message, accept);
    }

    public static Action ShowActivityIndicator()
    {
        var popup = new BusyPopup();
        MainPage.ShowPopup(popup);
        return () => popup.Close();
    }

    public static Task ShowToast(string text, ToastDuration duration = ToastDuration.Long)
    {
        var toast = Toast.Make(text, duration, textSize: 18);
        return toast.Show();
    }
}