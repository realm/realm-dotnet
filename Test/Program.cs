using System;
//this project has access to all the unit tests. You can use it to run selected unit tests in a more 
//controlled way than with the unit test runner, and to run the binding in a console window app, as this is
//running in a console window app makes it easier to see cerr and cout and console.writeln() output,
//as many unit test runners don't have support for showing console output.
//just calling unit tests that throw exceptions and catch these by [ExpectedException] does not work
//as no unit test framework is catching the exceptions
//so only a subsest of all our unit tests can be run from here this way
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using TightDbCSharp;
using TightDbCSharpTest;

namespace Test
{
    static class Program
    {
        static void Main()
        {
            Console.WriteLine("current directory : " + Directory.GetCurrentDirectory());
            Console.WriteLine("starting tests");
			Table.TestInterop ();
            TightDbCSharpTest.TableTests1.ShowVersionTest();//show various system details on screen
			TightDbCSharpTest.GroupTests.CreateGroupFileNameTest();//low level system integrity check

            //    TightDbCSharpTest.TableTests1.TableSetBinary();
            //TightDbCSharpTest.TableTests1.TableFindFirstBinaryCoreBug();
            //TightDbCSharpTest.TableTests1.TableAddSubtableUsingPath();
            TightDbCSharpTest.GroupTests.GroupWriteTest();
            Application.DoEvents();
            Console.WriteLine("Finished. Any key...");


            var test = new List<object>();
            IEnumerator b = test.GetEnumerator();
            b.MoveNext();
            Console.ReadKey();
        }
    }
}
