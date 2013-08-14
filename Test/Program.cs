using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//this project has access to all the unit tests. You can use it to run selected unit tests in a more 
//controlled way than with the unit test runner, and to run the binding in a console window app, as this is
//running in a console window app makes it easier to see cerr and cout and console.writeln() output,
//as many unit test runners don't have support for showing console output.
namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            TightDbCSharpTest.TableTests1.TableSetBinary();
        }
    }
}
