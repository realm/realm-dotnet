namespace TightDbCSharp
{
    /// <summary>
    /// This class contains diagnostics, test and benchmark functionality.
    /// 
    /// </summary>
    public static class Toolbox //only static methods for now, but may become non static later on
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
        public static long TestNativeSizeCalls()
        {
            return UnsafeNativeMethods.TestSizeCalls();
        }
    }
}
