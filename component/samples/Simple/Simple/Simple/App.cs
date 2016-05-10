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

using Realms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Realms.Xamarin.Examples.Simple
{
    public class App : Application
    {
        public App()
        {
            // The root page of your application
            MainPage = new ContentPage
            {
                Content = new StackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                    Children = {
                        new Label {
                            HorizontalTextAlignment = TextAlignment.Center,
                            Text = "Simple Realm Example"
                        }
                    }
                }
            };
        }

        public class Dog : RealmObject
        {
            public string Name { get; set; }

            public int Age { get; set; }
        }

        public class Person : RealmObject
        {
            public string Name { get; set; }

            public Dog Dog { get; set; }
        }

        protected async override void OnStart()
        {
            // clean any stale data from previous runs
            Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);

            // create a standalone object
            var dog = new Dog();

            // set & read properties
            dog.Name = "Rex";
            dog.Age = 9;
            Debug.WriteLine($"Name of dog: {dog.Name}");

            // realms are used to group data together
            var realm = Realm.GetInstance(); // create realm pointing to default file

            // save your object
            using (var transaction = realm.BeginWrite())
            {
                realm.Manage(dog);
                transaction.Commit();
            }

            // query
            IQueryable<Dog> query = from aDog in realm.All<Dog>()
                                    where aDog.Name.Contains("x")
                                    select aDog;

            // queries are chainable
            IQueryable<Dog> query2 = query.Where(aDog => aDog.Age > 8);
            Debug.WriteLine($"Number of dogs: {query2.Count()}");

            // link objects
            var person = new Person
            {
                Name = "Tim",
                Dog = dog
            };

            using (var transaction = realm.BeginWrite())
            {
                realm.Manage(person);
                transaction.Commit();
            }

            // multi-threading
            await Task.Run(() =>
            {
                using (var otherRealm = Realm.GetInstance())
                {
                    var otherQuery = otherRealm.All<Dog>().Where(aDog => aDog.Name.Contains("Rex"));
                    Debug.WriteLine($"Number of dogs: {otherQuery.Count()}");
                }
            });

            realm.Dispose();
        }
    }
}
