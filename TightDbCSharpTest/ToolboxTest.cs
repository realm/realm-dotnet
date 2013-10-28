using System;
using NUnit.Framework;
using TightDbCSharp;

namespace TightDbCSharpTest
{
    /// <summary>
    /// Collection of tests that test functionality in Toolbox. Toolbox is a collection of
    /// methods used for diagnostics, test, system information and other taks related to
    /// system maintainance , diagnostics and tuning rather than data retrieval and storage
    /// </summary>
    public class ToolboxTest
    {
        /// <summary>
        /// This method calls a c++ method. Used to time how long the c++
        /// method takes. Used in examples\PerformanceTest
        /// </summary>
        [Test]
        public void TestSizeCalls()
        {
            var count = Toolbox.TestNativeSizeCalls();
            Console.WriteLine("count frim cpp:{0}",count);
        }
    }
}
