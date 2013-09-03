using System;
//this project has access to all the unit tests. You can use it to run selected unit tests in a more 
//controlled way than with the unit test runner, and to run the binding in a console window app, as this is
//running in a console window app makes it easier to see cerr and cout and console.writeln() output,
//as many unit test runners don't have support for showing console output.
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;


namespace Test
{
    static class Program
    {
        static void Main()
        {
            Console.WriteLine("current directory : " + Directory.GetCurrentDirectory());
            Console.WriteLine("starting tests");
            TightDbCSharpTest.TableTests1.TestInterop();//low level system integrity check
            TightDbCSharpTest.TableTests1.ShowVersionTest();//show various system details on screen
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
