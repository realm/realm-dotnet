using QuickJournal.ViewModels;
using QuickJournal.Views;
using Xamarin.Forms;

namespace QuickJournal
{
    public class App : Application
    {
        public App()
        {
            var page = new JournalEntriesPage();
            {
                BindingContext = new JournalEntriesViewModel();
            };
            MainPage = new NavigationPage(page);
        }
    }
}

