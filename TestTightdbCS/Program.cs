using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tightdb.Tightdbcsharp;

namespace TestTightdbCS
{
    class Program
    {

        public void testtablescope()
        {
            Table testtbl = new Table();

        }
        static void Main(string[] args)
        {
            //if the user uses using with the table, it shoud be disposed at the end of the using block
            //using usage should follow these guidelines http://msdn.microsoft.com/en-us/library/yh598w02.aspx
            //You don't *have* to use using, if you don't the c++ table will not be disposed of as quickly as otherwise
          using (   Table testtable = new Table()             )
            {
            //do operations
            }        //table dispose sb calledautomatically  after table goes out of scope
        }

    }
}
