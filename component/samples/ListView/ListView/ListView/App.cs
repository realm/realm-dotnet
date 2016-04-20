using Xamarin.Forms;

namespace Realms.XamarinForms.Examples.ListView
{
    public class App : Application
    {
        public App()
        {
            // The root page of your application
            MainPage = new NavigationPage(new MainPage());
        }

        protected override void OnStart()
        {
            base.OnStart();

            // delete any stale data
            Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);
        }
    }
}
