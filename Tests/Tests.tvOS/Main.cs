using UIKit;

namespace Realms.Tests.XamarinTVOS
{
    public class Application
    {
        public static string[] Args { get; private set; }

        // This is the main entry point of the application.
        static void Main(string[] args)
        {
            Args = Sync.SyncTestHelpers.ExtractBaasSettings(args);
            UIApplication.Main(args, null, typeof(AppDelegate));
        }
    }
}

