using Foundation;
using UIKit;

namespace Realms.Tests.XamarinTVOS
{
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        public override UIWindow Window { get; set; }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            TestHelpers.IsAOTTarget = true;

            return true;
        }
    }
}