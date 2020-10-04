using Xamarin.Forms;

namespace QuickJournal
{
    public class App : Application
    {
        public App()
        {
            MainPage = new NavigationPage(new JournalEntriesPage());
        }
    }
}

