using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Interop.Providers;
using RealmNet;
using System.Linq;

namespace Playground.XamarinAndroid
{
    [Activity(Label = "Playground.XamarinAndroid", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        int count = 1;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.MyButton);

            button.Click += delegate { button.Text = string.Format("{0} clicks!", count++); };


            var coreProvider = new CoreProvider();
            Realm.ActiveCoreProvider = coreProvider;
            var realm = Realm.GetInstance();

            var p1 = realm.CreateObject<Person>();
            p1.FirstName = "John";
            p1.LastName = "Smith";
            p1.IsInteresting = true;
            p1.Email = "john@smith.com";
            System.Diagnostics.Debug.WriteLine("p1 is named " + p1.FullName);

            var p2 = realm.CreateObject<Person>();
            p2.FullName = "John Doe";
            p2.IsInteresting = false;
            p2.Email = "john@deo.com";
            System.Diagnostics.Debug.WriteLine("p2 is named " + p2.FullName);

            var p3 = realm.CreateObject<Person>();
            p3.FullName = "Peter Jameson";
            p3.Email = "peter@jameson.com";
            p3.IsInteresting = true;
            System.Diagnostics.Debug.WriteLine("p3 is named " + p3.FullName);


            var interestingPeople = from p in realm.All<Person>() where p.IsInteresting == true select p;

            System.Diagnostics.Debug.WriteLine("Interesting people include:");
            foreach (var p in interestingPeople)
                System.Diagnostics.Debug.WriteLine(" - " + p.FullName + " (" + p.Email + ")");

            var johns = from p in realm.All<Person>() where p.FirstName == "John" select p;
            System.Diagnostics.Debug.WriteLine("People named John:");
            foreach (var p in johns)
                System.Diagnostics.Debug.WriteLine(" - " + p.FullName + " (" + p.Email + ")");
            
        }
    }
}

