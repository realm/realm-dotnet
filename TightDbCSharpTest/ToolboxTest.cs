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
    public static class ToolboxTest
    {
        /// <summary>
        /// This method calls a c++ method. Used to time how long the c++
        /// method takes. Used in examples\PerformanceTest
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.GC.Collect"), Test]
        public static void TestSizeCalls()
        {
#if DEBUG//this is currently the last test being run. Report size of unbindlists as we stop. 
            Toolbox.ReportUnbindListStatus();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Toolbox.ReportUnbindListStatus();
#endif
            var count = Toolbox.TestNativeSizeCalls();
            Console.WriteLine("count from c++:{0}",count);
        }
    }
}
