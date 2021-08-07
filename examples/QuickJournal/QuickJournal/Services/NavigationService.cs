using System;
using System.Threading.Tasks;
using QuickJournal.ViewModels;
using QuickJournal.Views;
using Xamarin.Forms;

namespace QuickJournal.Services
{
    public static class NavigationService
    {
        private static INavigation Navigation => Application.Current.MainPage?.Navigation;

        public static async Task NavigateTo(BaseViewModel viewModel)
        {
            CheckNavigationSupport();

            var destinationPage = GetPageForViewModel(viewModel);
            destinationPage.BindingContext = viewModel;
            await Navigation.PushAsync(destinationPage, true);
        }

        public static void SetMainPage(BaseViewModel viewModel)
        {
            var destinationPage = GetPageForViewModel(viewModel);
            destinationPage.BindingContext = viewModel;
            Application.Current.MainPage = new NavigationPage(destinationPage);
        }

        public static async Task GoBack()
        {
            CheckNavigationSupport();

            await Navigation.PopAsync(true);
        }

        private static void CheckNavigationSupport()
        {
            if (Navigation == null)
            {
                throw new InvalidOperationException("Main page does not support navigation");
            }
        }

        private static Page GetPageForViewModel(BaseViewModel viewModel)
        {
            return viewModel switch
            {
                JournalEntryDetailsViewModel => new JournalEntryDetailsPage(),
                JournalEntriesViewModel => new JournalEntriesPage(),
                _ => throw new ArgumentException("The input ViewModel is not accepted"),
            };
        }
    }
}
