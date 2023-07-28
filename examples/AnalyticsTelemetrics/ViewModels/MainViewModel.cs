using AnalyticsTelemetry.Services;
using CommunityToolkit.Mvvm.Input;

namespace AnalyticsTelemetry.ViewModels
{
    public partial class MainViewModel
    {
        [RelayCommand]
        public static async Task OnAppearing()
        {
            await RealmService.Init();
        }

        [RelayCommand]
        public static async Task GoToTelemetryPage() => await Shell.Current.GoToAsync($"telemetry");

        [RelayCommand]
        public static async Task GoToAnalyticsPage() => await Shell.Current.GoToAsync($"analytics");
    }
}