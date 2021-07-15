using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace QuickJournal.Services
{
    public static class DialogService
    {
        static Page Page => Application.Current.MainPage;

        public static async Task ShowAlert(string title, string message, string cancel = "Ok")
        {
            if (Page == null)
                throw new Exception("The dialog service has not been initialized");

            await Page.DisplayAlert(title, message, cancel);
        }
    }
}
