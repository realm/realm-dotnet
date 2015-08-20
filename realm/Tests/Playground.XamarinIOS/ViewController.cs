using System;
using UIKit;
using RealmNet.Interop;
using RealmNet;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Foundation;

namespace Playground.XamarinIOS
{
    public class ClassWithRawMethod
    {
        //private TimeSpan ts = new TimeSpan(5);

        public void Call()
        {
            ViewController.Counter++; 
            // System.Threading.Thread.Sleep(ts);
        }
    }

    public interface IVirtualized
    {
        void Call();
    }

    public class VirtualizedClass : IVirtualized
    {
        //private TimeSpan ts = new TimeSpan(5);

        public void Call()
        {
            ViewController.Counter++; 
            //System.Threading.Thread.Sleep(ts);
        }
    }

    public partial class ViewController : UIViewController
    {
        public void Write(string text)
        {
            Console.Write(text);
            InvokeOnMainThread(() => DebugText.Text += text);
        }

        public void WriteLine(string text)
        {
            Write(text + "\r\n");
        }

        public static int Counter = 0;

        public void RawBenchmark()
        {
            var sw = new System.Diagnostics.Stopwatch();
            Write("Raw... ");

            var instance = new ClassWithRawMethod();

            sw.Start();
            for (var i = 0; i < 1000000; i++)
            {
                instance.Call();
            }
            sw.Stop();

            Write(sw.ElapsedMilliseconds + "\r\n");
        }

        public void VirtualizedBenchmark()
        {
            var sw = new System.Diagnostics.Stopwatch();
            Write("Virtualized... ");

            IVirtualized instance = new VirtualizedClass();

            sw.Start();
            for (var i = 0; i < 1000000; i++)
            {
                instance.Call();
            }
            sw.Stop();

            Write(sw.ElapsedMilliseconds + "\r\n");
        }

        public void BranchBenchmark()
        {
            var sw = new System.Diagnostics.Stopwatch();
            Write("Branching... ");

            var instance1 = new ClassWithRawMethod();
            var instance2 = new ClassWithRawMethod();

            sw.Start();
            for (var i = 0; i < 1000000; i++)
            {
                if (IntPtr.Size == 8)
                    instance1.Call();
                else
                    instance2.Call();
            }
            sw.Stop();

            Write(sw.ElapsedMilliseconds + "\r\n");
        }

        private void RunBenchmark()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            for (var i = 0; i < 10; i++)
            {
                RawBenchmark();
                BranchBenchmark();
                VirtualizedBenchmark();
            }
        }

        // Realm Objects Look like Regular Objects…
        public class Dog : RealmObject
        {
            public string name { get; set; }
            public int age { get; set; }
        }


        private void HomePageTest()
        {
            Realm.ActiveCoreProvider = new CoreProvider();
            Realm.DefaultPathProvider = () => Path.GetTempFileName();



            var mydog = new Dog() { name = "Rex" };
            Console.WriteLine($"name of dog:{mydog.name}");

            // Offer Easy Persistence…
            var realm = Realm.GetInstance();
            using (var writer = realm.BeginWrite()) {
                realm.Add( mydog );
            }

            // Can be Queried… with standard LINQ
            var r = realm.All<Dog>().Where(dog => dog.age > 8);

            // Queries are chainable
            var r2 = r.Where(dog => dog.name.Contains("rex"));
        }

        private void IntegrationTest()
        {
            var dbFilename = "db.realm";

            string libraryPath;

            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0)) {  // > ios 8
                libraryPath = NSFileManager.DefaultManager.GetUrls (NSSearchPathDirectory.LibraryDirectory, NSSearchPathDomain.User) [0].Path;
            } else {
                var docdir = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
                libraryPath = Path.GetFullPath(Path.Combine (docdir, "..", "Library")); 
            }

            var path = Path.Combine(libraryPath, dbFilename);

            WriteLine("============\n\n\n");
            WriteLine("File: " + path);

            var coreProvider = new CoreProvider();
            Realm.ActiveCoreProvider = coreProvider;
            var realm = Realm.GetInstance(path);

            WriteLine("####Past SharedGroup constructor####");

            Person p1, p2, p3;
            using (var transaction = realm.BeginWrite())
            {
                p1 = realm.CreateObject<Person>();
                p1.FirstName = "John";
                p1.LastName = "Smith";
                p1.IsInteresting = true;
                p1.Email = "john@smith.com";
            }
            using (var rt = realm.BeginRead())
            {
                WriteLine("p1 is named " + p1.FullName);
            }

            using (var transaction = realm.BeginWrite())
            {
                p2 = realm.CreateObject<Person>();
                p2.FullName = "John Doe";
                p2.IsInteresting = false;
                p2.Email = "john@deo.com";
            }
            using (var rt = realm.BeginRead())
            {
                WriteLine("p2 is named " + p2.FullName);
            }

            using (var transaction = realm.BeginWrite())
            {
/*                p3 = realm.CreateObject<Person>();
                p3.FullName = "Peter Jameson";
                p3.Email = "peter@jameson.com";
                p3.IsInteresting = true;
*/
                p3 = new Person { FullName = "Peter Jameson", Email = "peter@jameson.com", IsInteresting = true };
            }

            using (var rt = realm.BeginRead())
            {
                WriteLine("p3 is named " + p3.FullName);

                var interestingPeople = from p in realm.All<Person>() where p.IsInteresting == true select p;

                WriteLine("Interesting people include:");
                foreach (var p in interestingPeople)
                    WriteLine(" - " + p.FullName + " (" + p.Email + ")");

                var johns = from p in realm.All<Person>() where p.FirstName == "John" select p;
                WriteLine("People named John:");
                foreach (var p in johns)
                    WriteLine(" - " + p.FullName + " (" + p.Email + ")");
            }
        }

        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

            //new System.Threading.Thread(RunBenchmark).Start();
            new System.Threading.Thread(HomePageTest).Start();
//            new System.Threading.Thread(IntegrationTest).Start();
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}

