using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using TightDbCSharp;
using TightDbCSharp.Extensions;



namespace TightDbCSharpTest
{
    [TestFixture]
    class GroupTests
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
        public static void CreateGroupEmptyTest()
        {
            var g = new Group();
            Console.WriteLine(g.Handle); //keep it allocated, the unit test should survive that g is disposed automatically upon GC soon after this line
        }



        [Test]
        public static void CreateGroupEmptyTestUsing()
        {

            using (var g = new Group())
            {
                Console.WriteLine(g.Handle); //keep it allocated                
            }
        }



        /*
         this one fails too - not enough rights to work in the root directory
         However the fail comes when the program is being deallocated.. we should catch a c++ exception and throw a C# so no big harm should be done
         */


        [Test]
        [ExpectedException("System.IO.IOException")]
        //fixme this test fails as the exception being raised is System:Runtime:InteropServices.SEHException
            //not an IOException. Perhaps we should catch and re-throw. At this point (new group) exceptions are usuallly
            //eiter out of memory, or because the file passed cannot be create or is write protected
        public static void CreateGroupFileNameTest()
        {

            using (var g = new Group(@"C:\Testgroup"))
            {
                Console.WriteLine(g.Handle); //keep it allocated
            }
        }

        [Test]
        [ExpectedException("System.IO.IOException")]

        public static void CreateGroupFileNameTest2()
        {

            using (var g = new Group(@""))
            {

                Console.WriteLine(g.Handle); //keep it allocated
            }
        }




        //this one works. (if you have a directory called Develope in H:\) Do we have a filename/directory problem with the windows build?
        //perhaps we have a problem with locked or illegal files, what to do?
        //
        //probably something wrong with the code here too then
        [Test]
        public static void CreateGroupFileNameTestGoodFile()
        {
            using (var g = new Group(Path.GetTempPath() + "Testgroupf"))
            {
                Console.WriteLine(g.ObjectIdentification()); //keep it allocated
            }
        }

    }
}
