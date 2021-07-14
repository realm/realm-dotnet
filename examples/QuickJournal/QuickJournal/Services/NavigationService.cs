using System;
using System.Threading.Tasks;
using QuickJournal.ViewModels;
using QuickJournal.Views;
using Xamarin.Forms;

namespace QuickJournal.Services
{
    public static class NavigationService
    {
        static INavigation Navigation => Application.Current.MainPage?.Navigation;

        public static async Task NavigateTo(BaseViewModel viewModel)
        {
            if (Navigation == null)
                throw new InvalidOperationException("Main page does not support navigation");

            var destinationPage = GetPageForViewModel(viewModel);
            destinationPage.BindingContext = viewModel;
            await Navigation.PushAsync(destinationPage, true);
        }

        public static async Task GoBack()
        {
            if (Navigation == null)
                throw new InvalidOperationException("Main page does not support navigation");

            await Navigation.PopAsync(true);
        }

        static Page GetPageForViewModel(BaseViewModel viewModel)
        {
            switch (viewModel)
            {
                case JournalEntryDetailsViewModel _:
                    return new JournalEntryDetailsPage();
                default:
                    throw new ArgumentException("The input ViewModel is not accepted");
            }
        }
    }
}
