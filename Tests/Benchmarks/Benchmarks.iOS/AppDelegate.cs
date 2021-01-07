using System;
using System.Linq;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace Benchmarks.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {

            var arguments = NSProcessInfo.ProcessInfo.Arguments;
            var index = Array.IndexOf(arguments, "--benchmark-arguments");
            var benchmarkArguments = arguments.Skip(index + 1).ToArray();

            Forms.Init();
            LoadApplication(new App(benchmarkArguments));

            return base.FinishedLaunching(app, options);

        }
    }
}
