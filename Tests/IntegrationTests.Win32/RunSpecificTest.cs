using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntegrationTests.Shared;
using NUnit.Framework;

namespace IntegrationTests.Win32
{
    [TestFixture]
    public class RunSpecificTest
    {
        [Test, Explicit("Use this to run a specific test once")]
        public void RunTest()
        {
            var testFixture = new SimpleLINQtests();
            testFixture.Setup();

            testFixture.CreateList();

            testFixture.TearDown();
        }
    }
}
