using NUnit.Framework;
using TightDbCSharp;

namespace TightDbCSharpTest
{
    public class DiagnosticsTest
    {
        /// <summary>
        /// This method calls a c++ method. Used to time how long the c++
        /// method takes. Used in examples\PerformanceTest
        /// </summary>
        [Test]
        public void TestSizeCalls()
        {
            var diagnostics = new Diagnostics();
            diagnostics.TestNativeSizeCalls();
        }
    }
}
