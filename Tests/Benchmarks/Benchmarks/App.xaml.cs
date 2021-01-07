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

            MainPage = new MainPage
            {
                BindingContext = new MainPageViewModel()
            };
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
