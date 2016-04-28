using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Realms.XamarinForms.Examples.ListView
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        private Realm _realm;

        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _realm = Realm.GetInstance();
            listView.ItemsSource = _realm.All<DemoObject>().OrderByDescending(o => o.Date);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            listView.ItemsSource = null;
            _realm.Dispose();
            _realm = null;
        }

        void add_activated(object sender, EventArgs args)
        {
            using (var transaction = _realm.BeginWrite())
            {
                var obj = _realm.CreateObject<DemoObject>();
                obj.Title = $"Title {new Random().Next(1000)}";
                obj.Date = DateTimeOffset.UtcNow;
                transaction.Commit();
            }
        }

        void background_add_activated(object sender, EventArgs args)
        {
            Task.Run(() =>
                {
                    // get a new realm since we're on a different thread
                    using (var realm = Realm.GetInstance())
                    using (var transaction = realm.BeginWrite())
                    {
                        // add many items - that's why we're using a background thread
                        for (var i = 0; i < 5; i++)
                        {
                            var obj = realm.CreateObject<DemoObject>();
                            obj.Title = $"Title {new Random().Next(1000)}";
                            obj.Date = DateTimeOffset.UtcNow;
                        }

                        transaction.Commit();
                    }
                });
        }

        class DemoObject : RealmObject
        {
            public string Title { get; set; }

            public DateTimeOffset Date { get; set; }
        }
    }
}

