using System.Linq;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace Benchmarks.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            var arguments = NSProcessInfo.ProcessInfo.Arguments.Skip(1).ToArray();

            Forms.Init();
            LoadApplication(new App(arguments));

            return base.FinishedLaunching(app, options);

        }
    }
}
