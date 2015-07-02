using System;
using UIKit;
using Interop.Providers;
using RealmNet;
using System.Linq;

namespace Playground.XamarinIOS
{
    public partial class ViewController : UIViewController
    {
        public ViewController(IntPtr handle) : base(handle)
        {
            Console.WriteLine("============\n\n\n");
            //Console.WriteLine("Wrapper version: " + UnsafeNativeMethods.GetWrapperVer());
            //Console.WriteLine("Minor version: " + UnsafeNativeMethods.GetMinorVer());

            var coreProvider = new CoreProvider();
            Realm.ActiveCoreProvider = coreProvider;
            var realm = Realm.GetInstance();
            //var realm = new Realm(coreProvider);

            var p1 = realm.CreateObject<Person>();
            p1.FirstName = "John";
            p1.LastName = "Smith";
            p1.IsInteresting = true;
            p1.Email = "john@smith.com";
            Console.WriteLine("p1 is named " + p1.FullName);

            var p2 = realm.CreateObject<Person>();
            p2.FullName = "John Doe";
            p2.IsInteresting = false;
            p2.Email = "john@deo.com";
            Console.WriteLine("p2 is named " + p2.FullName);

            var p3 = realm.CreateObject<Person>();
            p3.FullName = "Peter Jameson";
            p3.Email = "peter@jameson.com";
            p3.IsInteresting = true;
            Console.WriteLine("p3 is named " + p3.FullName);

            var interestingPeople = from p in realm.All<Person>() where p.IsInteresting == true select p;
            Console.WriteLine("Interesting people include:");
            foreach (var p in interestingPeople)
                Console.WriteLine(" - " + p.FullName + " (" + p.Email + ")");

            var johns = from p in realm.All<Person>() where p.FirstName == "John" select p;
            Console.WriteLine("People named John:");
            foreach (var p in johns)
                Console.WriteLine(" - " + p.FullName + " (" + p.Email + ")");


            Console.WriteLine("\n\n\n============");
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}

