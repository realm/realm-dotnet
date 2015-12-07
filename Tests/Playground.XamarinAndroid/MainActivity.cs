/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Realms;
using System.Linq;
using System.IO;

namespace Playground.XamarinAndroid
{
    [Activity(Label = "Playground.XamarinAndroid", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        int count = 1;

        private void WriteLine(string message)
        {
            Console.WriteLine(message);
        }

        private void IntegrationTest()
        {
            var realm = Realm.GetInstance(Path.GetTempFileName());

            WriteLine("####Past SharedGroup constructor####");

            Person p1, p2, p3;
            using (var transaction = realm.BeginWrite())
            {
                p1 = realm.CreateObject<Person>();
                p1.FirstName = "John";
                p1.LastName = "Smith";
                p1.IsInteresting = true;
                p1.Email = "john@smith.com";
                transaction.Commit();
            }
            WriteLine("p1 is named " + p1.FullName);

            using (var transaction = realm.BeginWrite())
            {
                p2 = realm.CreateObject<Person>();
                p2.FullName = "John Doe";
                p2.IsInteresting = false;
                p2.Email = "john@deo.com";
                transaction.Commit();
            }
            WriteLine("p2 is named " + p2.FullName);

            using (var transaction = realm.BeginWrite())
            {
                p3 = realm.CreateObject<Person>();
                p3.FullName = "Peter Jameson";
                p3.Email = "peter@jameson.com";
                p3.IsInteresting = true;

                //p3 = new Person { FullName = "Peter Jameson", Email = "peter@jameson.com", IsInteresting = true };
                transaction.Commit();
            }

            WriteLine("p3 is named " + p3.FullName);

            var allPeople = realm.All<Person>().ToList();
            WriteLine("There are " + allPeople.Count() + " in total");

            var interestingPeople = from p in realm.All<Person>() where p.IsInteresting == true select p;

            WriteLine("Interesting people include:");
            foreach (var p in interestingPeople)
                WriteLine(" - " + p.FullName + " (" + p.Email + ")");

            var johns = from p in realm.All<Person>() where p.FirstName == "John" select p;
            WriteLine("People named John:");
            foreach (var p in johns)
                WriteLine(" - " + p.FullName + " (" + p.Email + ")");

            using (var transaction = realm.BeginWrite())
            {
                realm.Remove(p2);

                var allPeopleAfterDelete = realm.All<Person>().ToList();
                WriteLine("After deleting one, there are " + allPeopleAfterDelete.Count() + " in total");
                transaction.Commit();
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.MyButton);

            button.Click += delegate { button.Text = string.Format("{0} clicks!", count++); };

            IntegrationTest();

        }
    }
}

