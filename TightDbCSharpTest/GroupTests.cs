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
            Assert.AreEqual(false, g.Invalid);            
        }



        [Test]

        public static void CreateGroupEmptyTestUsing()
        {
            
            using (var g = new Group())
            {
                Assert.AreEqual(false, g.Invalid);
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
            using (var g = new Group(@"C:\Testgroup",Group.OpenMode.ModeReadWrite))
            {
                Assert.AreEqual(false, g.Invalid);
            }
        }

        [Test]
        [ExpectedException("System.IO.IOException")]
        public static void CreateGroupFileNameTest2()
        {

            using (var g = new Group(@"",Group.OpenMode.ModeReadOnly))
            {

                Assert.AreEqual(false, g.Invalid);            
            }
        }


        //todo:make reasonable tests of all 3 kinds of openmode and their edge cases 
        //(no rights, file exists/doesn't exists, illegal filename,null string)

        //this one works. (if you have a directory called Develope in H:\) Do we have a filename/directory problem with the windows build?
        //perhaps we have a problem with locked or illegal files, what to do?
        //
        //probably something wrong with the code here too then
        [Test]
        public static void CreateGroupFileNameTestGoodFile()
        {
    
            var filename = Path.GetTempPath() + "Testgroupf";

            File.Delete(filename);//ok if it is not there?
            using (var g = new Group(filename,Group.OpenMode.ModeReadWrite))
            {
                Assert.AreEqual(false, g.Invalid);            
            }
        }


        [Test]
        public static void Ca2000TestNoFalsePositive()
        {

            var g = new Group();
            try
            {
            }
            finally
            {
                g.Dispose();
            }
        }


        [Test]  //see above test that is similar to this one, but does not trigger CA2000
            //DO NOT DISABLE THIS TEST - WHEN IT DOES NOT SHOW UP AS A CA2000 tHEN MSFT HAVE FIXED
            //THEIR CA2000 BUG, AND WE SHOULD REMOVE THE SUPRESSED CA2000 ERRORS FROM THE SUPRESSION FILES
        public static void Ca2000TestFalsePositive()
        {
            using (var g = new Group())            
            {
                Assert.AreEqual(g.Handle,g.Handle);
            }
        }








        [Test]
       
        public static void GroupWriteTest()
        {
            var groupCreateFileName = Path.GetTempPath() + "Testgroupc";
            var groupSaveFileName = Path.GetTempPath() + "Testgroups";
            const string testTableName = "test1";
            if(File.Exists(groupSaveFileName)) {
               File.Delete(groupSaveFileName);
            }
            using (var g = new Group())
            {              
                g.CreateTable(testTableName, "double".Double());
                g.Write(groupSaveFileName);
            }
            using (var g2 = new Group(groupSaveFileName,Group.OpenMode.ModeReadOnly))
            {                
                Assert.AreEqual(true,g2.HasTable(testTableName));//we read the correct group back in
                //now write the group to a memory buffer
                byte[] groupBuffer = g2.WriteToMemory();
                bool bufNotEmpty = groupBuffer.Length > 0;
                Assert.AreEqual(true,bufNotEmpty);



                using (var g3 = new Group(groupBuffer))
                {
                    Assert.AreEqual(true, g3.HasTable(testTableName));//we read the correct group back in
                }
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
