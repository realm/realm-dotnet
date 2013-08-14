using System;
using System.IO;
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


        [Test]
       
        public static void GroupWriteTest()
        {
            string groupCreateFileName = Path.GetTempPath() + "Testgroupc";
            string groupSaveFileName = Path.GetTempPath() + "Testgroups";
            const string testTableName = "test1";
            if(File.Exists(groupSaveFileName)) {
               File.Delete(groupSaveFileName);
            }
            using (var g = new Group())
            {              
                g.CreateTable(testTableName, "double".Double());
                g.Write(groupSaveFileName);
            }
            using (var g2 = new Group(groupSaveFileName))
            {                
                Assert.AreEqual(true,g2.HasTable(testTableName));//we read the correct group back in
            }
            if (File.Exists(groupSaveFileName))
            {
                File.Delete(groupSaveFileName);
            }
            if (File.Exists(groupCreateFileName))
            {
                File.Delete(groupCreateFileName);
            }
        }


    }
}
