using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TightDbCSharp
{
    /// <summary>
    /// This class contains diagnostics, test and benchmark functionality.
    /// 
    /// </summary>
    public class Diagnostics
    {

        /// <summary>
        /// This method will call the Cpp dll and the dll will then
        /// create a table, and call table.size() 1 million times
        /// then the cpp dll will return
        /// timing this call can be compared to doing the same in
        /// C#, and thus, get a benchmark reg. the overhead with
        /// calling from C# to cpp
        /// The method is used in performancetest.cs in examples
        /// </summary>
        public long TestNativeSizeCalls()
        {
            return UnsafeNativeMethods.TestSizeCalls();
        }
    }
}
