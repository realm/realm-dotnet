using InteropShared;
using RealmNet;

namespace Interop.Providers
{
// Helper that returns MockCoreProvider when build using the DebugMock config
    public static class ProviderFactory
    {
        public static ICoreProvider Make()
        {
#if MOCKING_CONFIG
        return new MockCoreProvider();
#else
            return new Interop.Providers.CoreProvider();
#endif
        }
    }
}


