using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QuickJournal
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class JournalEntryDetailsPage : ContentPage
    {
        public JournalEntryDetailsPage(JournalEntryDetailsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            viewModel.Navigation = Navigation;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            (BindingContext as JournalEntryDetailsViewModel)?.OnDisappearing();
            BindingContext = null;
        }
    }
}

