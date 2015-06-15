// ***********************************************************************
// Copyright (c) 2009 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

//using System.IO;

using NUnitLite.Runner;
//using NUnit.Framework.Internal;
using TightDbCSharp;

namespace NUnitLite.Tests
{
    /// <summary>
    /// Run NunitLite Unit test 
    /// Empty parametres will result in all tests being run,
    /// output to console,
    /// and the program waiting for a keypress when finished.
    /// Arguments : run program with /? to get help, or look up
    /// NUnitLite documentation
    /// </summary>
    public static class Program
    {

        // <summary>
        // Attempt at running tests without nuinit for debugging purposes
        // This is not yet workable (expectedexceptions), but is also not yet used
        // </summary>
        // <param name="test">the test method to execute</param>
  /*      public static void TightDbTester(Action test)
        {
            try
            {
                if (test != null)
                {
                    Console.WriteLine("TDB Tester running " + test.Method);
                    test();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Got exception "+e);
            }
        }
*/
        /// <summary>
        /// Run NunitLite Unit test 
        /// Empty parametres will result in all tests being run,
        /// output to console,
        /// and the program waiting for a keypress when finished.
        /// Arguments : run program with /? to get help, or look up
        /// NUnitLite documentation
        /// </summary>
        /// <param name="args">
        ///         // The main program executes the tests. Output may be routed to
        /// various locations, depending on the arguments passed.
        ///
        /// Arguments:
        ///
        ///  Arguments may be names of assemblies or options prefixed with '/'
        ///  or '-'. Normally, no assemblies are passed and the calling
        ///  assembly (the one containing this Main) is used. The following
        ///  options are accepted:
        ///
        ///    -test:testname    Provides the name of a test to be exected.
        ///                      May be repeated. If this option is not used,
        ///                      all tests are run.
        ///
        ///    -out:PATH         Path to a file to which output is written.
        ///                      If omitted, Console is used, which means the
        ///                      output is lost on a platform with no Console.
        ///
        ///    -full             Print full report of all tests.
        ///
        ///    -result:PATH      Path to a file to which the XML test result is written.
        ///
        ///    -explore[:Path]   If specified, list tests rather than executing them. If a
        ///                      path is given, an XML file representing the tests is written
        ///                      to that location. If not, output is written to tests.xml.
        ///
        ///    -noheader,noh     Suppress display of the initial message.
        ///
        ///    -wait             Wait for a keypress before exiting.
        ///
        ///    -include:categorylist 
        ///             If specified, nunitlite will only run the tests with a category 
        ///             that is in the comma separated list of category names. 
        ///             Example usage: -include:category1,category2 this command can be used
        ///             in combination with the -exclude option also note that exlude takes priority
        ///             over all includes.
        ///
        ///    -exclude:categorylist 
        ///             If specified, nunitlite will not run any of the tests with a category 
        ///             that is in the comma separated list of category names. 
        ///             Example usage: -exclude:category1,category2 this command can be used
        ///             in combination with the -include option also note that exclude takes priority
        ///             over all includes
        /// 
        /// </param>
        /// 
        public static void Main(string[] args)
        {
            Toolbox.ShowVersionTest();
            if (args != null && args.Length != 0)
            {
                new TextUI().Execute(args);
            }
            else
            {
                
                /*
                TdbTester(TableTests1.SubTableNoFields);
                TdbTester(TableTests1.TableAddColumn);
                TdbTester(TableTests1.TableAddColumnAndSpecTest);
                TdbTester(TableTests1.TableAddColumnAndSpecTestSimple);
                TdbTester(TableTests1.TableAddColumnTypeParameter);
                TdbTester(TableTests1.TableAddColumnTypeParameterPath);
                TdbTester(TableTests1.TableAddColumnTypes);
                TdbTester(TableTests1.TableAddColumnWithData);
                TdbTester(TableTests1.TableAddIntArray);
                TdbTester(TableTests1.TableAddSubTableAsNull);
                TdbTester(TableTests1.TableAddSubTableEmptyArray);
                TdbTester(TableTests1.TableAddSubTableHugeTable);
                TdbTester(TableTests1.TableAddSubTablePlausiblePathExample);
                TdbTester(TableTests1.TableAddSubTableStringArray);
                TdbTester(TableTests1.TableAddSubTableUsingParameters);
                TdbTester(TableTests1.TableAddSubTableUsingPath);
                TdbTester(TableTests1.TableAggregate);
                TdbTester(TableTests1.TableClearSubTable);
                TdbTester(TableTests1.TableCloneLostFieldNameTest);
                TdbTester(TableTests1.TableCloneTest);
                TdbTester(TableTests1.TableCloneTest2);
                TdbTester(TableTests1.TableCloneTest3);
                TdbTester(TableTests1.TableCloneTest4);
                TdbTester(TableTests1.TableCollectionInitializer);
                TdbTester(TableTests1.TableDistinct);
                TdbTester(TableTests1.TableDistinctBadType);
                TdbTester(TableTests1.TableDistinctNoIndex);
                TdbTester(TableTests1.TableFindAllBinaryBadType);
                TdbTester(TableTests1.TableFindAllBinaryBadType2);
                TdbTester(TableTests1.TableFindAllBinarySuccessful);
                TdbTester(TableTests1.TableFindAllBoolBadType);
                TdbTester(TableTests1.TableFindAllBoolBadType2);
                TdbTester(TableTests1.TableFindAllBoolSuccessful);
                TdbTester(TableTests1.TableFindAllDateBadType);
                TdbTester(TableTests1.TableFindAllDateBadTypeString);
                TdbTester(TableTests1.TableFindAllDateSuccessful);
                TdbTester(TableTests1.TableFindAllDoubleBadType);
                TdbTester(TableTests1.TableFindAllDoubleBadType2);
                TdbTester(TableTests1.TableFindAllDoubleSuccessful);
                TdbTester(TableTests1.TableFindAllFloatBadType);
*/
                new TextUI().Execute(new[] {/* "-test:TableFindAllFloatBadType", "-test:TableTests1.TableFindAllFloatBadType",*/ "-labels", "-full", "-wait"/* "-out:C:\\Files\\UnitTestDumpCS.txt"*/});
            }
            //Console.WriteLine("Tests finished, any key to close the application");
            //Console.ReadKey();
        }
    }
}