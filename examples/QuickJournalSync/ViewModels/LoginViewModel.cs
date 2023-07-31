using CommunityToolkit.Mvvm.Input;
using QuickJournalSync.Services;

namespace QuickJournalSync.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        public string? Email { get; set; }

        public string? Password { get; set; }

        [RelayCommand]
        public async Task OnAppearing()
        {
            RealmService.Init();

            if (RealmService.CurrentUser != null)
            {
                await GoToMainPage();
            }
        }

        [RelayCommand]
        public async Task Login()
        {
            if (!await VeryifyEmailAndPassword())
            {
                return;
            }

            await DoLogin();
        }

        [RelayCommand]
        public async Task SignUp()
        {
            if (!await VeryifyEmailAndPassword())
            {
                return;
            }

            await DoSignup();
        }

        private async Task DoLogin()
        {
            try
            {
                IsBusy = true;
                await RealmService.LoginAsync(Email, Password);
                IsBusy = false;
            }
            catch (Exception ex)
            {
                IsBusy = false;
                await DialogService.ShowAlertAsync("Login failed", ex.Message, "Ok");
                return;
            }

            await GoToMainPage();
        }

        private async Task DoSignup()
        {
            try
            {
                IsBusy = true;
                await RealmService.RegisterAsync(Email, Password);
                IsBusy = false;
            }
            catch (Exception ex)
            {
                IsBusy = false;
                await DialogService.ShowAlertAsync("Sign up failed", ex.Message, "Ok");
                return;
            }

            await DoLogin();
        }

        private async Task<bool> VeryifyEmailAndPassword()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                await DialogService.ShowAlertAsync("Error", "Please specify both the email and the password", "Ok");
                return false;
            }

            return true;
        }

        private async Task GoToMainPage()
        {
            await Shell.Current.GoToAsync($"//items");
        }
    }
}