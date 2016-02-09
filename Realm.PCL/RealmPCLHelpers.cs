using System;

namespace Realms
{
    internal static class RealmPCLHelpers
    {
        internal static void ThrowProxyShouldNeverBeUsed()
        {
            throw new PlatformNotSupportedException("The PCL project is being linked which probably means you failed to use NuGet or otherwise link a platform-specific project to your main application");
        }
    }
}

