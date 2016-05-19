using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QuickJournal
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class JournalEntriesPage : ContentPage
    {
        public JournalEntriesPage()
        {
            InitializeComponent();
            BindingContext = new JournalEntriesViewModel { Navigation = Navigation };
        }

        void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            (BindingContext as JournalEntriesViewModel).EditEntry((JournalEntry)e.Item);
        }

        void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            (sender as ListView).SelectedItem = null;
        }
    }
}

