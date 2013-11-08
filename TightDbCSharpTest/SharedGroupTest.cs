using System;
using System.IO;
using System.Text;
using TightDbCSharp;
using NUnit.Framework;

namespace TightDbCSharpTest
{
    /// <summary>
    /// Tests related to the SharedGroup class
    /// </summary>
    [TestFixture]
    public static class SharedGroupTest
    {

        private const string Field01Text = "Data for first field";
        private const string Field02Text = "Data for second field";
        private const string Field03Text = "Data for third field";

        private static string SharedGroupFileName()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                   @"\UnitTestSharedGroup";
            //we should probably use LocalApplicationData instead of ApplicationData
        }


        //create-dispose test
        /// <summary>
        /// Test creation of a shared group with a legal path and file name
        /// </summary>
        [Test]
        public static void CreateSharedGroupFileTest()
        {
            File.Delete(SharedGroupFileName());

            using (var sharedGroup = new SharedGroup(SharedGroupFileName(), false, DurabilityLevel.DurabilityFull))
            {
                Assert.AreEqual(false, sharedGroup.Invalid);
//                Console.WriteLine(String.Format(CultureInfo.InvariantCulture, "Hello from sharedgroup {0}",
//                    sharedGroup.ObjectIdentification()));

            }
        }


        /// <summary>
        /// 
        /// </summary>
        [Test]
        public static void HasChangedTest()
        {
            File.Delete(SharedGroupFileName());
            using (var sg1 = new SharedGroup(SharedGroupFileName()))
            {
                using (var transaction = sg1.BeginWrite())
                using (var table = transaction.CreateTable("test", new IntColumn("IntegerField")))
                {
                    table.Add(42);
                    transaction.Commit();
                }
            }

            //at this time we should have a shared group with a table with 42 in it
            //Let's Open the file and see if the 42 is there
            using (var sg2 = new SharedGroup(SharedGroupFileName()))
            {
                using (var transaction = sg2.BeginRead())
                using (var table = transaction.GetTable("test"))
                {
                    Assert.AreEqual(42, table.GetLong(0, 0));
                    transaction.Commit();
                }
            }

            //okay that went well.
            //Now -  lets create two shared groups one after the other

            using (var sg3 = new SharedGroup(SharedGroupFileName()))
            using (var sg4 = new SharedGroup(SharedGroupFileName()))
            {
                using (var transaction = sg3.BeginWrite())
                using (var table = transaction.GetTable("test"))
                {
                    Assert.AreEqual(42, table.GetLong(0, 0));
                    table.SetLong(0, 0, 13);
                    transaction.Commit();
                }
                //at this point sg3 is alive and sg4 is alive
                Assert.AreEqual(false, sg3.HasChanged);
                Assert.AreEqual(true, sg4.HasChanged);
            }
        }


        /// <summary>
        /// Test a simple create table, add row and commit transaction commit on a sharedGroup created using defaults
        /// Note! does not validate that data was actually saved
        /// </summary>
        [Test]
        public static void SimpleCommitTest()
        {

            File.Delete(SharedGroupFileName());

            using (
                var sharedGroup = new SharedGroup(SharedGroupFileName()))
            {
                using (var transaction = sharedGroup.BeginWrite())
                {
                    using (var table = transaction.CreateTable("TestTable",
                        new StringColumn("StringColumn")))
                    {
                        table.AddEmptyRow(1);
                        transaction.Commit();
                    }
                }
            }
        }




        /// <summary>
        /// Test a simple create table, add row and rollback transaction commit on a sharedGroup created using defaults        
        /// </summary>
        [Test]
        public static void SimpleRollbackTest()
        {

            File.Delete(SharedGroupFileName());

            using (
                var sharedGroup = new SharedGroup(SharedGroupFileName()))
            {
                using (var transaction = sharedGroup.BeginWrite())
                using (var table = transaction.CreateTable("TestTable",new StringColumn("StringColumn")))
                {
                    table.AddEmptyRow(1);
                    table.SetString(0, 0, "Hello, Table!");
                    Assert.AreEqual("Hello, Table!", table.GetString(0, 0));
                    transaction.Commit();
                }

                using (var transaction = sharedGroup.BeginWrite())
                using (var table = transaction.GetTable("TestTable"))
                {
                    Assert.AreEqual("Hello, Table!", table.GetString(0, 0));
                    table.SetString(0, 0, "Hi Again!");
                    transaction.Rollback();
                }

                using (var transaction = sharedGroup.BeginRead())
                using (var table = transaction.GetTable("TestTable"))
                {
                    Assert.AreEqual("Hello, Table!", table.GetString(0, 0));
                }
            }
        }




        /// <summary>
        /// It should be illegal to read from a table if the write transaction has been ended
        /// </summary>
        [Test]
        [ExpectedException("System.InvalidOperationException")]
        public static void ReadAfterRollback()
        {
            File.Delete(SharedGroupFileName());
            using (
                var sharedGroup = new SharedGroup(SharedGroupFileName()))
            {
                using (var transaction = sharedGroup.BeginWrite())
                {
                    using (var table = transaction.CreateTable("TestTable",
                        new StringColumn("StringColumn")))
                    {
                        table.AddEmptyRow(1);
                        table.SetString(0, 0, "Hello, Table!");
                        Assert.AreEqual("Hello, Table!", table.GetString(0, 0));
                        transaction.Rollback();
                        Assert.AreEqual("Hello, Table!", table.GetString(0, 0));//should throw
                    }
                }
            }
        }

        /// <summary>
        /// It should be illegal to read from a table if the write transaction has been ended
        /// </summary>
        [Test]
        [ExpectedException("System.InvalidOperationException")]
        public static void ReadAfterReadWriteCommit()
        {
            File.Delete(SharedGroupFileName());
            using (
                var sharedGroup = new SharedGroup(SharedGroupFileName()))
            {
                using (var transaction = sharedGroup.BeginWrite())
                {
                    using (var table = transaction.CreateTable("TestTable",
                        new StringColumn("StringColumn")))
                    {
                        table.AddEmptyRow(1);
                        table.SetString(0, 0, "Hello, Table!");
                        Assert.AreEqual("Hello, Table!", table.GetString(0, 0));
                        transaction.Commit();
                        Assert.AreEqual("Hello, Table!", table.GetString(0, 0));//should throw
                    }
                }
            }
        }


        /// <summary>
        /// It should be illegal to read from a table if the write transaction has been ended
        /// </summary>
        [Test]
        [ExpectedException("System.InvalidOperationException")]
        public static void ReadAfterReadOnlyCommit()
        {
            File.Delete(SharedGroupFileName());
            using (
                var sharedGroup = new SharedGroup(SharedGroupFileName()))
            {
                using (var transaction = sharedGroup.BeginRead())
                {
                    using (var table = transaction.CreateTable("TestTable",
                        new StringColumn("StringColumn")))
                    {
                        table.AddEmptyRow(1);
                        table.SetString(0, 0, "Hello, Table!");
                        Assert.AreEqual("Hello, Table!", table.GetString(0, 0));
                        transaction.Commit();
                        Assert.AreEqual("Hello, Table!", table.GetString(0, 0));//should throw
                    }
                }
            }
        }


        
        
        
//todo:same as above but with a commit  - should fail We are not in read trans. so should fail i guess
//todo: same as above but in a read transaction, shoudl also fail, we are not in a read trans


        //successfull usage case check
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public static void SharedGroupTransactions()
        {
            File.Delete(SharedGroupFileName());



            using (var sharedGroup = new SharedGroup(SharedGroupFileName(), false, DurabilityLevel.DurabilityFull))
            {
                //first we need to create a table and put a little data into it
                Assert.AreEqual(false, sharedGroup.Invalid);//C# construct, so legal even on an unattached shared group                
//                Console.WriteLine(String.Format(CultureInfo.InvariantCulture, "Hello from sharedgroup {0}",
//                    sharedGroup.ObjectIdentification()));


                using (var transaction = sharedGroup.BeginWrite())
                {
                    using (var table = transaction.CreateTable("TestTable",
                        new StringColumn("StringColumn"),
                        new StringColumn("StringColumn2"),
                        new StringColumn("StringColumn3")                        
                        ))
                    {
                        table.Add(Field01Text, Field02Text, Field03Text);
                    }
                    transaction.Commit();                    
                }

                using (var group = sharedGroup.BeginRead())
                {
                    using (var table = group.GetTable("TestTable"))
                    {
                        Assert.AreEqual(Field01Text, table.GetString(0, 0));
                        Assert.AreEqual(Field02Text, table.GetString(1, 0));
                        Assert.AreEqual(Field03Text, table.GetString(2, 0));                                                
                    }
                }



                //alternative syntax example1, anonymous function wrapped

                sharedGroup.ExecuteInWriteTransaction(group =>
                {
                    using (var table = group.CreateTable("TestTable2", new StringColumn("StringColumn1"),
                                                                            new StringColumn("StringColumn2"),
                                                                            new StringColumn("StringColumn3")))
                    {
                        table.Add(Field01Text, Field02Text, Field03Text);
                    }                    
                });


                sharedGroup.ExecuteInReadTransaction(transaction =>
                {
                    using (var table = transaction.GetTable("TestTable2"))                                      
                    {
                        Assert.AreEqual(Field01Text, table.GetString(0, 0));
                        Assert.AreEqual(Field02Text, table.GetString(1, 0));
                        Assert.AreEqual(Field03Text, table.GetString(2, 0));
                    }
                });


                //C# supports using function parametres in addition to the anonymous example above
                sharedGroup.ExecuteInReadTransaction(TestHelperRead2);

                //the using pattern can also use function methods
                using (var transaction = sharedGroup.BeginRead())
                  TestHelperRead2(transaction);
                
                //ExecuteInWriteTransaction takes a method of type "void blabla(Group t)"
                sharedGroup.ExecuteInWriteTransaction(TestHelperWriter3);

                //example with calling function to write with using syntax
                using (var transaction = sharedGroup.BeginWrite())
                {
                    TestHelperWriter4(transaction);
                    transaction.Commit();
                }

                //Another example with calling function to write
                sharedGroup.ExecuteInWriteTransaction(TestHelperWriter5);
            }
        }


        static void TestHelperRead2(Group t)
        {
            using (var table = t.GetTable("TestTable2"))
            {
                Assert.AreEqual(Field01Text, table.GetString(0, 0));
                Assert.AreEqual(Field02Text, table.GetString(1, 0));
                Assert.AreEqual(Field03Text, table.GetString(2, 0));
            }
        }


        static void TestHelperWriter3(Group t)
        {
            using (var table = t.CreateTable("TestTable3", new StringColumn("StringColumn1"),
                                                          new StringColumn("StringColumn2"),
                                                           new StringColumn("StringColumn3")))
            {
                table.AddEmptyRow(3);
                table.SetString(0, 0,Field01Text);
                table.SetString(1, 0, Field02Text);
                table.SetString(2, 0, Field03Text);
            }
        }

        static void TestHelperWriter4(Group t)
        {
            using (var table = t.CreateTable("TestTable4", new StringColumn("StringColumn1"),
                                                          new StringColumn("StringColumn2"),
                                                           new StringColumn("StringColumn3")))
            {
                table.AddEmptyRow(3);
                table.SetString(0, 0, Field01Text);
                table.SetString(1, 0, Field02Text);
                table.SetString(2, 0, Field03Text);
            }
        }

        static void TestHelperWriter5(Group t)
        {
            using (var table = t.CreateTable("TestTable5", new StringColumn("StringColumn1"),
                                                          new StringColumn("StringColumn2"),
                                                           new StringColumn("StringColumn3")))
            {
                table.AddEmptyRow(3);
                table.SetString(0, 0, Field01Text);
                table.SetString(1, 0, Field02Text);
                table.SetString(2, 0, Field03Text);
            }
        }





        //todo:unit tests that show that throwing exception from the action based transactions actually does a rollback

        //todo:ensure we have a unit test that checks that the action based transactions actually do save changes when no exception is thrown

        //todo:unit tests that ensures that committed write transactions actually are written

        //todo:unit tests that shows that rolled back transactions are in fact rolled back

        //todo:unit test that showcases what happens if commit is called more than one time

        //todo:unit test that showcases what happens if commit is called after rollback

        //todo:contemplate what happens if the user somehow calls stuff in the shared group while a transaction is ongoing
        //can he call something that breaks the system (like starting more transactions on the same table, or whatever)

        //It should not be possible to modify a group that is returned by a read transaction
        /// <summary>
        /// Check that a readonly transaction is actually marked as readonly
        /// </summary>
        [Test]
        public static void SharedGroupReadTransactionReadOnlyGroup()
        {
            const string sharedgroupfilename = @"UnitTestSharedGroup";
            File.Delete(sharedgroupfilename);

            using (var sharedGroup = new SharedGroup(sharedgroupfilename, false, DurabilityLevel.DurabilityFull))
            {
                using (var transaction = sharedGroup.BeginRead())
                {
               //     Console.WriteLine(String.Format(CultureInfo.InvariantCulture,
              //          "Hello from inside a readtransaction {0}", sharedGroup.ObjectIdentification()));
                    Assert.AreEqual(true, transaction.ReadOnly);
                }
             //   Console.WriteLine(String.Format(CultureInfo.InvariantCulture, "Hello from sharedgroup {0}",
            //        sharedGroup.ObjectIdentification()));
            }
        }

        /// <summary>
        ///It should not be possible to modify a table in a group that is returned by a read transaction
        ///this test could also have been put into tabletest 
        ///also checks that on violation of the readonly contract, that the sharedgroup and the group are invalidated  
        /// </summary>
        [Test]        
        public static void SharedGroupReadTransactionReadOnlyTable()
        {
            const string sharedgroupfilename = @"UnitTestSharedGroup";
            File.Delete(sharedgroupfilename);

            using (var sharedGroup = new SharedGroup(sharedgroupfilename, false, DurabilityLevel.DurabilityFull))
            {

                Assert.AreEqual(false, sharedGroup.Invalid);
                
                using (var transaction = sharedGroup.BeginRead())
                {
                    Assert.AreEqual(true, transaction.ReadOnly);
                    Assert.AreEqual(TransactionKind.Read,transaction.Kind);                    
                    Assert.AreEqual(false,sharedGroup.Invalid);
                    Assert.AreEqual(false, transaction.Invalid);
                    try
                    {
                        var fail = transaction.CreateTable("must fail");
                        Assert.AreEqual(fail.Size,0);//we should never get this far. assert put in to remove compiler warning
                    }
                    catch (InvalidOperationException)
                    {
                        Assert.AreEqual(true, transaction.ReadOnly);
                        Assert.AreEqual(TransactionKind.Read, transaction.Kind);
                        Assert.AreEqual(false, sharedGroup.Invalid);//the outer transaction has not been compromized
                        Assert.AreEqual(false, transaction.Invalid);//just bc an illegal inner transaction operation was preempted
                    }
                }
            }
        }





        /// <summary>
        /// This is a copy of the transactions part of the tutorial. Added here bc there was some problems with it
        /// </summary>
        [Test]
        public static void TransactionsPartOfTutorial()
        {
            var fakeConsole = new StringBuilder();


            File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                        @"\employees1.tightdb");
            File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                        @"\employees1.tightdb.lock");


            // @@Example: serialisation @@
            // Create Table in Group
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var fileName1 = folder + @"\employees1.tightdb";

            using (var group = new Group())
            using (var employees = group.CreateTable("employees",
                new StringColumn("Name"),
                new IntColumn("Age"),
                new BoolColumn("Hired")))
            {

                //add some rows
                employees.Add("John", 20, true);
                employees.Add("Mary", 21, false);
                employees.Add("Lars", 21, true);
                employees.Add("Phil", 43, false);
                employees.Add("Anni", 54, true);

                group.Write(fileName1);
            }

            // Load a group from disk (and print contents)
            using (var fromdisk = new Group(fileName1, Group.OpenMode.ModeReadWrite))
            using (var employees2 = fromdisk.GetTable("employees"))
            {
                var name = employees2.GetColumnIndex("Name");
                foreach (var row in employees2)
                    fakeConsole.AppendFormat("{0}:{1}", row.RowIndex, row.GetString(name));


                //Write same group to memory buffer
                byte[] buffer = fromdisk.WriteToMemory();

                //Load a group from memory (and print contents)
                using (var fromMem = new Group(buffer))
                using (var memtable = fromMem.GetTable("employees"))
                {
                    name = memtable.GetColumnIndex("Name");
                    foreach (var row in memtable)
                        fakeConsole.AppendFormat("{0}:{1}", row.RowIndex, row[name]);                    
                }
            }
            // @@EndExample@@

            // @@Example: transaction @@
            // Open a shared group

            //Transaction inherits from group, adds commit and rollback methods
            //Commit must be called to actually save changes
            //Rollback is automatically called if the transaction has not been
            //comitted and it goes out scope (gets disposed)
            using (var db = new SharedGroup(fileName1))
            {
                using (var transaction = db.BeginRead())
                using (var employees = transaction.GetTable("employees"))
                {
                    var age = employees.GetColumnIndex("Age");
                    var name = employees.GetColumnIndex("Name");

                    foreach (var employee in employees)
                    {
                        fakeConsole.AppendFormat("{0} is {1} years old", employee[name], employee[age]);
                    }
                    transaction.Commit();
                }

                //write transaction
                using (var transaction2 = db.BeginWrite())
                using (var employees4 = transaction2.GetTable("employees"))
                {
                    {
                        employees4.Add("Bill", 53, true);
                    }
                    transaction2.Commit();
                }

                //TightDb also provides a delegate based transaction syntax
                //After the delegate has e xecuted, commit is called automatically
                //to roll back, throw an exception inside the delegate
                db.ExecuteInWriteTransaction(group =>
                {
                    using (var employees3 = group.GetTable("employees"))
                    {
                        employees3.Add("Bill", 53, true); //add a row
                    }
                });
            }
            //todo:validate contents of fakeConsole
        }



        /// <summary>
        /// error handling when a transaction is started inside another transaction (which is illegal if done with the same sharedgroup object
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "NUnit.Framework.Assert.Fail(System.String)"), Test]
        public static void SharedGroupSeveralStartTransactions()
        {
            const string sharedgroupfilename = @"UnitTestSharedGroup";
            File.Delete(sharedgroupfilename);

            using (var sharedGroup = new SharedGroup(sharedgroupfilename, false, DurabilityLevel.DurabilityFull))
            {

                Assert.AreEqual(false, sharedGroup.Invalid);

                using (var transaction = sharedGroup.BeginRead())
                {
                    Assert.AreEqual(true, transaction.ReadOnly);
                    Assert.AreEqual(TransactionKind.Read, transaction.Kind);
                    Assert.AreEqual(false, sharedGroup.Invalid);
                    Assert.AreEqual(false, transaction.Invalid);
                    try
                    {
                        using (var transaction2 = sharedGroup.BeginWrite())
                        {
                            Assert.AreEqual(transaction2.ToString(), transaction2.ToString());//we should never get this far.line added to keep compiler happy
                            Assert.Fail("starting a transaction while another one is still active did not fail");
                        }
                    }
                        //because the secnd transaction was aborted on its creation, the
                        //world is still working, we can continue working with the unharmed shared
                        //group and the unharmed group inside our outer transaction. c++ tightdb
                        //integrity has not been compromised
                    catch (InvalidOperationException)
                    {
                        Assert.AreEqual(true, transaction.ReadOnly);
                        Assert.AreEqual(TransactionKind.Read, transaction.Kind);
                        Assert.AreEqual(false, sharedGroup.Invalid);
                        Assert.AreEqual(false, transaction.Invalid);
                    }
                }
                Assert.AreEqual(false, sharedGroup.Invalid);//and the shared group should also work after the outer transaction has finished
            }
        }
    }
}
