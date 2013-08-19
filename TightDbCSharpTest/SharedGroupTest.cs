using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using TightDbCSharp;
using NUnit.Framework;

namespace TightDbCSharpTest
{
    [TestFixture]
    internal class SharedGroupTest
    {



        //create-dispose test
        [Test]
        public void CreateSharedGroupFileTest()
        {
            const string sharedgroupfilename = @"UnitTestSharedGroup";
            File.Delete(sharedgroupfilename);

            using (var sharedGroup = new SharedGroup(sharedgroupfilename, false, DurabilityLevel.DurabilityFull))
            {
                Assert.AreEqual(false, sharedGroup.Invalid);
                Console.WriteLine(String.Format(CultureInfo.InvariantCulture, "Hello from sharedgroup {0}",
                    sharedGroup.ObjectIdentification()));

            }
        }



        [Test]
        public void SimpleCommitTest()
        {
            const string sharedgroupfilename = @"UnitTestSharedGroup";
            File.Delete(sharedgroupfilename);

            using (
                var sharedGroup = new SharedGroup(sharedgroupfilename, false,
                    DurabilityLevel.DurabilityFull))
            {
                using (var transaction = sharedGroup.BeginWrite())
                {
                    using (var table = transaction.CreateTable("TestTable",
                        new StringField("StringColumn")))
                    {
                        table.AddEmptyRow(1);
                        transaction.Commit();
                    }
                }
            }
        }



        const string Field01Text = "Data for first field";
        const string Field02Text = "Data for second field";
        const string Field03Text = "Data for third field";
        const string Sharedgroupfilename = @"UnitTestSharedGroup";
        //successfull usage case check
        [Test]
        public void CreateSharedGroupOpenReadWrite()
        {
            File.Delete(Sharedgroupfilename);



            using (var sharedGroup = new SharedGroup(Sharedgroupfilename, false, DurabilityLevel.DurabilityFull))
            {
                //first we need to create a table and put a little data into it
                Assert.AreEqual(false, sharedGroup.Invalid);//C# construct, so legal even on an unattached shared group                
                Console.WriteLine(String.Format(CultureInfo.InvariantCulture, "Hello from sharedgroup {0}",
                    sharedGroup.ObjectIdentification()));


                using (var transaction = sharedGroup.BeginWrite())
                {
                    using (var table = transaction.CreateTable("TestTable",
                        new StringField("StringColumn"),
                        new StringField("StringColumn2"),
                        new StringField("StringColumn3")                        
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

                sharedGroup.ExecuteInWriteTransaction(( group ) =>
                {
                    using (var table = group.CreateTable("TestTable2", new StringField("StringColumn1"),
                                                                            new StringField("StringColumn2"),
                                                                            new StringField("StringColumn3")))
                    {
                        table.Add(Field01Text, Field02Text, Field03Text);
                    }                    
                });


                sharedGroup.ExecuteInReadTransaction((transaction) =>
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


        void TestHelperRead3(Group t)
        {
            using (var table = t.GetTable("TestTable3"))
            {
                Assert.AreEqual(Field01Text, table.GetString(0, 0));
                Assert.AreEqual(Field02Text, table.GetString(1, 0));
                Assert.AreEqual(Field03Text, table.GetString(2, 0));
            }            
        }

        void TestHelperRead2(Group t)
        {
            using (var table = t.GetTable("TestTable2"))
            {
                Assert.AreEqual(Field01Text, table.GetString(0, 0));
                Assert.AreEqual(Field02Text, table.GetString(1, 0));
                Assert.AreEqual(Field03Text, table.GetString(2, 0));
            }
        }


        void TestHelperWriter3(Group t)
        {
            using (var table = t.CreateTable("TestTable3", new StringField("StringColumn1"),
                                                          new StringField("StringColumn2"),
                                                           new StringField("StringColumn3")))
            {
                table.AddEmptyRow(3);
                table.SetString(0, 0,Field01Text);
                table.SetString(1, 0, Field02Text);
                table.SetString(2, 0, Field03Text);
            }
        }

        void TestHelperWriter4(Group t)
        {
            using (var table = t.CreateTable("TestTable4", new StringField("StringColumn1"),
                                                          new StringField("StringColumn2"),
                                                           new StringField("StringColumn3")))
            {
                table.AddEmptyRow(3);
                table.SetString(0, 0, Field01Text);
                table.SetString(1, 0, Field02Text);
                table.SetString(2, 0, Field03Text);
            }
        }

        void TestHelperWriter5(Group t)
        {
            using (var table = t.CreateTable("TestTable5", new StringField("StringColumn1"),
                                                          new StringField("StringColumn2"),
                                                           new StringField("StringColumn3")))
            {
                table.AddEmptyRow(3);
                table.SetString(0, 0, Field01Text);
                table.SetString(1, 0, Field02Text);
                table.SetString(2, 0, Field03Text);
            }
        }








        //It should not be possible to modify a group that is returned by a read transaction
        [Test]
        public void SharedGroupReadTransactionReadonlyGroup()
        {
            const string sharedgroupfilename = @"UnitTestSharedGroup";
            File.Delete(sharedgroupfilename);

            using (var sharedGroup = new SharedGroup(sharedgroupfilename, false, DurabilityLevel.DurabilityFull))
            {
                Assert.AreEqual(true, sharedGroup.IsAttached);

                using (var transaction = sharedGroup.BeginRead())
                {
                    Console.WriteLine(String.Format(CultureInfo.InvariantCulture,
                        "Hello from inside a readtransaction {0}", sharedGroup.ObjectIdentification()));
                    Assert.AreEqual(true, transaction.ReadOnly);
                }
                Console.WriteLine(String.Format(CultureInfo.InvariantCulture, "Hello from sharedgroup {0}",
                    sharedGroup.ObjectIdentification()));
            }
        }

        //It should not be possible to modify a table in a group that is returned by a read transaction
        //this test could also have been put into tabletest
        //also checks that on violation of the readonly contract, that the sharedgroup and the group are invalidated
        [Test]        
        public void SharedGroupReadTransactionReadonlyTable()
        {
            const string sharedgroupfilename = @"UnitTestSharedGroup";
            File.Delete(sharedgroupfilename);

            using (var sharedGroup = new SharedGroup(sharedgroupfilename, false, DurabilityLevel.DurabilityFull))
            {
                Assert.AreEqual(true, sharedGroup.IsAttached);
                Assert.AreEqual(false, sharedGroup.Invalid);
                
                using (var transaction = sharedGroup.BeginRead())
                {
                    Console.WriteLine(String.Format(CultureInfo.InvariantCulture,
                        "Hello from inside a readtransaction {0}", sharedGroup.ObjectIdentification()));
                    Assert.AreEqual(true, transaction.ReadOnly);
                    Assert.AreEqual(TransactionType.Read,transaction.Type);                    
                    Assert.AreEqual(false,sharedGroup.Invalid);
                    Assert.AreEqual(false, transaction.Invalid);
                    try
                    {
                        Table t = transaction.CreateTable("must fail");
                        Assert.Fail("create on a read transaction didn't fail");
                    }
                    catch (InvalidOperationException)
                    {
                        Assert.AreEqual(true, transaction.ReadOnly);
                        Assert.AreEqual(TransactionType.Read, transaction.Type);
                        Assert.AreEqual(false, sharedGroup.Invalid);//the outer transaction has not been compromized
                        Assert.AreEqual(false, transaction.Invalid);//just bc an illegal inner transaction operation was preempted
                    }
                }
                Console.WriteLine(String.Format(CultureInfo.InvariantCulture, "Hello from sharedgroup {0}",
                    sharedGroup.ObjectIdentification()));
            }
        }


        

        [Test]
        public void SharedGroupSeveralStartTransactions()
        {
            const string sharedgroupfilename = @"UnitTestSharedGroup";
            File.Delete(sharedgroupfilename);

            using (var sharedGroup = new SharedGroup(sharedgroupfilename, false, DurabilityLevel.DurabilityFull))
            {
                Assert.AreEqual(true, sharedGroup.IsAttached);
                Assert.AreEqual(false, sharedGroup.Invalid);

                using (var transaction = sharedGroup.BeginRead())
                {
                    Console.WriteLine(String.Format(CultureInfo.InvariantCulture,
                        "Hello from inside a readtransaction {0}", sharedGroup.ObjectIdentification()));
                    Assert.AreEqual(true, transaction.ReadOnly);
                    Assert.AreEqual(TransactionType.Read, transaction.Type);
                    Assert.AreEqual(false, sharedGroup.Invalid);
                    Assert.AreEqual(false, transaction.Invalid);
                    try
                    {
                        using (var transaction2 = sharedGroup.BeginWrite())
                        {
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
                        Assert.AreEqual(TransactionType.Read, transaction.Type);
                        Assert.AreEqual(false, sharedGroup.Invalid);
                        Assert.AreEqual(false, transaction.Invalid);
                    }
                }
                Console.WriteLine(String.Format(CultureInfo.InvariantCulture, "Hello from sharedgroup {0}",
                    sharedGroup.ObjectIdentification()));
                Assert.AreEqual(false, sharedGroup.Invalid);//and the shared group should also work after the outer transaction has finished
            }
        }
    }
}
