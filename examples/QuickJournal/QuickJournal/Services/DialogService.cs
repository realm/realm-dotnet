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
            {
                throw new Exception("Error while using the Dialog Service");
            }

            await Page.DisplayAlert(title, message, cancel);
        }
    }
}
