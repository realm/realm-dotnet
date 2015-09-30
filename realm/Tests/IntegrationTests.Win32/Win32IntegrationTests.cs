using System;
using System.IO;
using NUnit.Framework;
using RealmNet;

// class to give us a point to run the tests in Windows with TestRunner.dotnet
namespace IntegrationTests.Win32
{
    [TestFixture]
    public class Win32IntegrationTests : IntegrationTests
    {
        [Test]
        public void Tester()
        {
            var os1 = NativeRealm.realm_object_schema_new("123 hej med ☃ dig!");
            var os2 = NativeRealm.realm_object_schema_new("Nummer 2");

            var osses = new[] { os1, os2 };

            NativeRealm.realm_schema_new(osses, (IntPtr) osses.Length);
        }
    }
}
