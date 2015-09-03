using System.IO;
using NUnit.Framework;
using RealmNet;
using RealmNet.Interop;

namespace IntegrationTests.XamarinIOS
{
    [TestFixture]
    public class IOSIntegrationTests : AbstractIntegrationTests
    {
        protected override void Setup()
        {
            BaseSetup ();
            Realm.ActiveCoreProvider = new CoreProvider();
        }

        protected override  string GetTempDatabasePath()
        {
            return Path.GetTempFileName();
        }
    }
}
