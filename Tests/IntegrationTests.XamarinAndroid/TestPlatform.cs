using System;
using System.Threading.Tasks;
using Realms;

namespace IntegrationTests
{
    internal static class TestPlatform
    {
        internal static async Task NotifyRealm(Realm realm)
        {
            // we can't easily yield to the looper, so let's just wait an arbitrary amount of time and notify manually
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            NativeSetup.notify_realm(realm.SharedRealmHandle.DangerousGetHandle());
        }
    }
}

