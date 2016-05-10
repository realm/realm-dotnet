////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////

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

