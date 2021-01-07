using Benchmarks.View;
using Benchmarks.ViewModel;
using Xamarin.Forms;

namespace Benchmarks
{
    public partial class App : Application
    {
        public App(string[] args)
        {
            InitializeComponent();

            var vm = new MainPageViewModel();

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
