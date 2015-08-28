using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using RealmNet;
using RealmNet.Interop;

namespace IntegrationTests.Win32
{
    [TestFixture]
    public class Win32PerformanceTests : AbstractPerformanceTests
    {
        protected override void Setup()
        {
            Realm.ActiveCoreProvider = new CoreProvider();
        }

        protected override string GetTempDatabasePath()
        {
            return Path.GetTempFileName();
        }

        [TestCase(100)]
        public void RawPerformanceTest(int count)
        {
            Debug.WriteLine("Performance check for " + count + " entries");

            var sgh = _realm.GetPropertyValue<SharedGroupHandle>("Handle");
            var w = NativeSharedGroup.begin_write(sgh);

            var s = "String value";

            for (var rowIndex = 0; rowIndex < count; rowIndex++)
            {
                //NativeTable.set_string(tablePtr, 0, (IntPtr)rowIndex, s, s.Length);

            }

        }
    }
}
