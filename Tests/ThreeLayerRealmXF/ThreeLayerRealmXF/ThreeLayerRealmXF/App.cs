using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace ThreeLayerRealmXF {
    public class App : Application
    {

        private readonly PurePCLViewModel.Model _model;

        public App()
        {
            _model = new PurePCLViewModel.Model();

            BindingContext = _model;
            var boundLabel = new Label
            {
                XAlign = TextAlignment.Center,
                Text = ""
            };
            boundLabel.SetBinding(Label.TextProperty, new Binding("TheAnswer"));

            var button = new Button()
            {
                Text = "Test Realm",
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
            };
            button.Clicked += (s, e) =>
            {
                _model.TestRealm();
            };

            // The root page of your application
            MainPage = new ContentPage
            {
                Content = new StackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                    Children = {
                        new Label {
                            XAlign = TextAlignment.Center,
                            Text = "Welcome to ThreeLayer PCL in Forms!"
                        },
                        button,
                        boundLabel
                    }
                }
            };
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
