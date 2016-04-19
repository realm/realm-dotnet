using System;
using System.Threading.Tasks;
using CoreFoundation;
using Realms;

namespace IntegrationTests
{
    internal static class TestPlatform
    {
        internal static async Task NotifyRealm(Realm realm)
        {
            // spinning the runloop gives the WeakRealmNotifier the chance to do its job
            CFRunLoop.Current.RunInMode(CFRunLoop.ModeDefault, TimeSpan.FromMilliseconds(100).TotalSeconds, false);
        }
    }
}

