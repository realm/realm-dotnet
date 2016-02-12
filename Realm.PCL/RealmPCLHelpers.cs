using System;

namespace Realms
{
    internal static class RealmPCLHelpers
    {
        internal static void ThrowProxyShouldNeverBeUsed()
        {
            throw new PlatformNotSupportedException("The PCL build of Realm is being linked which probably means you need to use NuGet or otherwise link a platform-specific Realm.dll to your main application.");
        }
    }
}

