using QuickJournal.Services;
using QuickJournal.ViewModels;
using Xamarin.Forms;

namespace QuickJournal
{
    public class App : Application
    {
        public App()
        {
            NavigationService.SetMainPage(new JournalEntriesViewModel());
        }
    }
}

