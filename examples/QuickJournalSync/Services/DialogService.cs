using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using QuickJournalSync.Views;

namespace QuickJournalSync.Services
{
    public static class DialogService
    {
        public static Task ShowAlertAsync(string title, string message, string accept)
        {
            if (Application.Current?.MainPage == null)
            {
                throw new Exception("Cannot show an alert without a MainPage");
            }

            return Application.Current.MainPage.DisplayAlert(title, message, accept);
        }

        public static Action ShowActivityIndicator()
        {
            if (Application.Current?.MainPage == null)
            {
                throw new Exception("Cannot show an activity indicator without a MainPage");
            }

            var popup = new BusyPopup();
            Application.Current.MainPage.ShowPopup(popup);
            return () => popup.Close();
        }

        public static Task ShowToast(string text)
        {
            var toast = Toast.Make(text, ToastDuration.Short);
            return toast.Show();
        }
    }
}