using System.IO;
using NUnit.Framework;
using RealmNet;
using RealmNet.Interop;

namespace IntegrationTests.Win32
{
    [TestFixture]
    public class Win32Tests : AbstractIntegrationTests
    {
        protected override void Setup()
        {
            Realm.ActiveCoreProvider = new CoreProvider();
        }

        protected override  string GetTempDatabasePath()
        {
            return Path.GetTempFileName();
        }
    }
}
