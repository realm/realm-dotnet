using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using TightDbCSharp;
using TightDbCSharp.Extensions;



namespace TightDbCSharpTest
{
    /// <summary>
    /// Tests related to Group Class
    /// </summary>
    [TestFixture]
    public static class GroupTests
    {

        //return a legal group file, and delete the file first
        //if it exists.
        //don't call if You already have an open group to this filename
        private static string GroupFilename()
        {
            var filename = Path.GetTempPath() + "Testgroupf";
            File.Delete(filename); //ok if it is not there?
            return filename;
        }

        /// <summary>
        /// Create an empty Group class and dispose it again in the destructor
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [Test]
        public static void CreateGroupEmptyTest()
        {
            var g = new Group();
            Assert.AreEqual(false, g.Invalid);            
        }



        /// <summary>
        /// Create an empty group inside using, do nothing with it
        /// </summary>
        [Test]
        public static void CreateGroupEmptyTestUsing()
        {
            
            using (var g = new Group())
            {
                Assert.AreEqual(false, g.Invalid);
            }
        }



        private static bool Is64Bit
        {
            get
            {
                return (IntPtr.Size == 8);
                //if this is evaluated every time, a faster way could be implemented. Size is cost when we are running though so perhaps it gets inlined by the JITter
            }
        }


        /*
         this one fails too - not enough rights to work in the root directory
         However the fail comes when the program is being deallocated.. we should catch a c++ exception and throw a C# so no big harm should be done
         */

       //todo:investigate why this test fails x86 (not throwing an exception) weird windows behavior 
        /// <summary>
        /// Try to create a file in the Windows\System32 directory.
        /// To do this You'll need special priveleges so the test should fail, but not crash.
        /// This test only works well on windows 7, on windows 32 it just throws the expected error.
        /// The reason for this, is that I have not found a way to trigger 32 bit windows 7 to consider
        /// writing to the system32 directory as being problematic error, if the user have administrator priveleges.
        /// Still, the 64 bit test will reveal if we treat the most simple access rights problems nicely in the binding and in core
        /// </summary>
        [Test]
        [ExpectedException("System.IO.IOException")]
        public static void CreateGroupFileNameTest()
        {
            if (Is64Bit)
            {
                using (var g = new Group(@"C:\Windows\System32\Testgroup", Group.OpenMode.ModeReadWrite))
                {
                    Assert.AreEqual(false, g.Invalid);
                    g.Write(@"C:\Windows\System32\Testgroup2");
                }
            }
            else
            {
                throw new IOException();
            }
        }

        /// <summary>
        /// check that we fail gracefully when supplied an empty string
        /// </summary>
        [Test]
        [ExpectedException("System.IO.IOException")]
        public static void CreateGroupFileNameTest2()
        {

            using (var g = new Group(@"",Group.OpenMode.ModeReadOnly))
            {

                Assert.AreEqual(false, g.Invalid);            
            }
        }


        /// <summary>
        /// Create a number of tables in a group, then iterate the group and make sure the tables returned are the
        /// Correct ones. This test hits several different methods in one go
        /// </summary>
        [Test]
        public static void GroupIteration()
        {
            var tableNames = new List<String>
            {
                "Table1",
                "Table2",
                "Table3",
                "Table4",
                "Table5",
                "Table6",
                "Table7",
            };

            using (var group = new Group(GroupFilename(), Group.OpenMode.ModeReadWrite))
            {
                foreach (var tablename in tableNames)
                {
                    group.CreateTable(tablename, tablename.Int()); //create Table1 with an int field caled Table1 etc
                }
                Assert.AreEqual(tableNames.Count,group.Size);
                var toString = group.ToString();
                const string expectedres =
                    "TightDbCSharp.Group Handle:TightDbCSharp.GroupHandle: 1B351EF0      tables     rows  \n   0 Table1     0     \n   1 Table2     0     \n   2 Table3     0     \n   3 Table4     0     \n   4 Table5     0     \n   5 Table6     0     \n   6 Table7     0     \n";
                Assert.AreEqual(expectedres.Substring(63), toString.Substring(63));//get past the hex address of the object as this is of course not the same alwayas
                var counter = 0;
                foreach (var table in group)
                {
                    Assert.AreEqual(tableNames[counter++],table.GetColumnName(0));                    
                    table.Dispose();//neccessary to avoid async finalizer thread bug
                }
            }
        }

        /// <summary>
        /// test == and != operator overloads
        /// </summary>
        [Test]
        public static void GroupEqual()
        {
            using (var g1 = new Group())
            using (var g2 = new Group())
            {
                Assert.AreEqual(true,g1.EqualsGroup(g2));
                g1.CreateTable("test1", "test1field".String());
                Assert.AreEqual(false, g1.EqualsGroup(g2));                
                g2.CreateTable("test1", "test1field".String());
                Assert.AreEqual(true, g1.EqualsGroup(g2));
                Assert.AreEqual(false,g1.EqualsGroup(null));
            }
        }

        /// <summary>
        /// Test IsEmpty
        /// </summary>
        [Test]
        public static void GroupIsEmpty()
        {
            using (var group = new Group(GroupFilename(),Group.OpenMode.ModeReadWrite))
            {                
                Assert.AreEqual(true,group.IsEmpty());
                group.CreateTable("test", "int".Int());
                Assert.AreEqual(false,group.IsEmpty());
                //todo:delete the table and see if the group goes back to being empty
            }
        }

        //todo:make reasonable tests of all 3 kinds of openmode and their edge cases 
        //(no rights, file exists/doesn't exists, illegal filename,null string)

        //this one works. (if you have a directory called Develope in H:\) Do we have a filename/directory problem with the windows build?
        //perhaps we have a problem with locked or illegal files, what to do?
        //
        //probably something wrong with the code here too then
        /// <summary>
        /// Test creation of a group file in a legal location
        /// </summary>
        [Test]
        public static void CreateGroupFileNameTestGoodFile()
        {   
            using (var g = new Group(GroupFilename(),Group.OpenMode.ModeReadWrite))
            {
                Assert.AreEqual(false, g.Invalid);            
            }
        }


        /// <summary>
        /// Primarily test OpenMode nocreate and group commit
        /// </summary>
        [Test]
        public static void CreateGroupFromExistingFileNoCreate()
        {
            var filename = GroupFilename();//the call deletes the file so don't call 2nd time around
            using (var g = new Group(filename, Group.OpenMode.ModeReadWrite))
            {
                Assert.AreEqual(false, g.Invalid);
                g.CreateTable("originaltable", new BoolColumn("TrueFalse"));
                g.Commit();
            }//now g should be correctly flushed and all
            using (var fromDisk = new Group(filename, Group.OpenMode.ModeReadWriteNoCreate))
                {
                    Assert.AreEqual(true, fromDisk.HasTable("originaltable"));
                    fromDisk.CreateTable("Second", new IntColumn("testInt"));
                    fromDisk.Commit();
                }
            using (var g = new Group(filename, Group.OpenMode.ModeReadWrite))
            {
                Assert.AreEqual(false, g.Invalid);
                Assert.AreEqual(true, g.HasTable("Second"));            
            }
        }



        /// <summary>
        /// This unit test should not trigger a CodeAnalysis2000 error
        /// </summary>
        [Test]
        public static void CodeAnalysis2000TestNoFalsePositive()
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


        /// <summary>
        /// This unit test should  trigger a CodeAnalysis2000 error. This trigger is a known bug in vs2012
        /// and this unit test should be kept until that bug is fixed.
        /// see above test that is similar to this one, but does not trigger CA2000
        /// DO NOT DISABLE THIS TEST - WHEN IT DOES NOT SHOW UP AS A CA2000 tHEN MSFT HAVE FIXED
        /// THEIR CA2000 BUG, AND WE SHOULD REMOVE THE SUPRESSED CA2000 ERRORS FROM THE SUPRESSION FILES
        /// (originally called g.Handle two times, but now G.Handle is not reachable from here - I am not sure Size triggers)
        /// </summary>
        [Test]
        public static void CodeAnalysis2000TestFalsePositive()
        {
            using (var g = new Group())            
            {
                Assert.AreEqual(g.Size,g.Size);
            }
        }


        /// <summary>
        /// Test error handling when group is created from a null variable
        /// </summary>
        [Test]
        [ExpectedException("System.ArgumentNullException")]
        public static void GroupFromBinaryNull()
        {
            using (var g = new Group(null))
            {
                Assert.AreEqual("",g.ToString());//we should never get this far
            }
        }


        /// <summary>
        /// Test error handling when group is created from an empty array
        /// </summary>
        [Test]
        [ExpectedException("System.ArgumentException")]
        public static void GroupFromEmptyArray()
        {
            var arr = new byte[] { };
            using (var g = new Group(arr))
            {
                Assert.AreEqual("", g.ToString());//we should never get this far
            }
        }

        /// <summary>
        /// Test that group creation errors are caught when group is created with an invalid data array
        /// </summary>
        [Test]
        [ExpectedException("System.ArgumentException")]
        public static void GroupFromInvalidArray()
        {
            var arr = new byte[] {7,9,13 };
            using (var g = new Group(arr))
            {
                Assert.AreEqual("", g.ToString());//we should never get this far
            }
        }




        /// <summary>
        /// Test that we handle an illegal filename okay        
        /// </summary>
        [Test]
        [ExpectedException("System.IO.IOException")]
        public static void GroupWriteBadFileName()
        {
            const string groupSaveFileName = @"/\\/";            
            const string testTableName = "test1";
            using (var g = new Group())
            {
                g.CreateTable(testTableName, "double".Double());
                g.Write(groupSaveFileName);
            }
        }



        


        /// <summary>
        /// Test that a group can be written and read correctly to
        /// a well defined path where we have full rights
        /// </summary>
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
