using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using TightDbCSharp;
using TightDbCSharp.Extensions;



namespace TightDbCSharpTest
{



    [TestFixture]
    class TableTests
    {

                [Test]
        public static void TableGetColumnName()
        {
            var testFieldNames = new List<String>
            {
                "fieldname",
                "",
                "1",
                "\\",
                "ÆØÅæøå",
                "0123456789abcdefghijklmnopqrstuvwxyz"
            };
            using (var testTable = new Table())
            {
                int n = 0;
                foreach (string str in testFieldNames)
                {
                    testTable.AddColumn(DataType.String, str);
                    Assert.AreEqual(str, testTable.GetColumnName(n++));
                }
            }
        }


        [Test]
        public static void TableGetColumnIndex()
        {

            var testFieldNames = new List<String>
            {
                "fieldname",
                "",
                "1",
                "\\",
                "ÆØÅæøå",
                "0123456789abcdefghijklmnopqrstuvwxyz"
            };
            using (var testTable = new Table())
            {
                var n = 0;
                foreach (var str in testFieldNames)
                {
                    testTable.AddColumn(DataType.String, str);
                    Assert.AreEqual(n++, testTable.GetColumnIndex(str));
                }
            }
        }




        //Right now this test uses creation of tables as a test - the column name will be set to all sorts of crazy thing, and we want them back that way
        [Test]
        public static void TableWithPerThousandSign()
        {
            String actualres;
            using (
                var notSpecifyingFields = new Table(
                    "subtable".Table()
                    )) //at this point we have created a table with no fields
            {
                notSpecifyingFields.AddColumn(DataType.String, "12345‰7890");
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                                                "table name is 12345 then the permille sign ISO 10646:8240 then 7890",
                                                notSpecifyingFields);
            }
            const string expectedres = @"------------------------------------------------------
Column count: 2
Table Name  : table name is 12345 then the permille sign ISO 10646:8240 then 7890
------------------------------------------------------
 0      Table  subtable            
 1     String  12345‰7890          
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
        }




        [Test]
        public static void TableWithNotAnsiCharacters()
        {
            String actualres;
            using (
                var notSpecifyingFields = new Table(
                    "subtable".Table()
                    )) //at this point we have created a table with no fields
            {
                notSpecifyingFields.AddColumn(DataType.String, "123\u0300\u0301678");
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                                                "column name is 123 then two non-ascii unicode chars then 678",
                                                notSpecifyingFields);
            }
            const string expectedres = @"------------------------------------------------------
Column count: 2
Table Name  : column name is 123 then two non-ascii unicode chars then 678
------------------------------------------------------
 0      Table  subtable            
 1     String  123" + "\u0300\u0301" + @"678            
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
        }
    

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization",
     "CA1303:Do not pass literals as localized parameters",
     MessageId = "NUnit.Framework.Assert.AreEqual(System.Int64,System.Int64,System.String,System.Object[])"),
  System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming",
      "CA2204:Literals should be spelled correctly", MessageId = "InsertInt")]
        private static void CheckNumberInIntColumn(Table table, long columnNumber, long rowNumber, long testValue)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table"); //code analysis made me do this             
            }
            table.SetLong(columnNumber, rowNumber, testValue);
            var gotOut = table.GetLong(columnNumber, rowNumber);
            Assert.AreEqual(testValue, gotOut, "Table.InsertInt value mismatch sent{0} got{1}", testValue, gotOut);
        }


        //create a table of only integers, 3 columns.
        //with 42*42 in {0,0}, with long.minvalue in {1,1} and with long.minvalue+24 in {2,2}
        //the other fields have never been touched
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability",
            "CA2000:Dispose objects before losing scope"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        private static Table GetTableWithIntegers(bool subTable)
        {
            var t = new Table();
            var s = t.Spec;
            s.AddIntColumn("IntColumn1");
            s.AddIntColumn("IntColumn2");
            s.AddIntColumn("IntColumn3");
            if (subTable)
            {
                Spec subSpec = t.Spec.AddSubTableColumn("SubTableWithInts");
                subSpec.AddIntColumn("SubIntColumn1");
                subSpec.AddIntColumn("SubIntColumn2");
                subSpec.AddIntColumn("SubIntColumn3");
            }
            t.UpdateFromSpec();



            var rowindex = t.AddEmptyRow(1); //0
            long colummnIndex = 0;
            CheckNumberInIntColumn(t, colummnIndex, rowindex, 0);
            CheckNumberInIntColumn(t, colummnIndex, rowindex, -0);
            CheckNumberInIntColumn(t, colummnIndex, rowindex, 42 * 42);

            if (subTable)
            {
                Table sub = t.GetSubTable(3, rowindex);
                sub.AddEmptyRow(3);
                CheckNumberInIntColumn(sub, colummnIndex, rowindex, 2L);
            }

            colummnIndex = 1;
            rowindex = t.AddEmptyRow(1); //1
            CheckNumberInIntColumn(t, colummnIndex, rowindex, long.MaxValue);
            CheckNumberInIntColumn(t, colummnIndex, rowindex, long.MinValue);
            if (subTable)
            {
                Table sub = t.GetSubTable(3, rowindex);
                sub.AddEmptyRow(3);
                CheckNumberInIntColumn(sub, colummnIndex, rowindex, 2L);
            }



            colummnIndex = 2;
            rowindex = t.AddEmptyRow(1); //2
            CheckNumberInIntColumn(t, colummnIndex, rowindex, long.MaxValue - 42);
            CheckNumberInIntColumn(t, colummnIndex, rowindex, long.MinValue + 42);
            if (subTable)
            {
                Table sub = t.GetSubTable(3, rowindex);
                sub.AddEmptyRow(3);
                CheckNumberInIntColumn(sub, colummnIndex, rowindex, 2L);
            }




            return t;
        }

        [Test]
        public static void TableIterationTest()
        {
            using
                (
                var t = new Table("stringfield".String())
                )
            {
                t.AddEmptyRow(3);
                t.SetString(0, 0, "firstrow");
                t.SetString(0, 0, "secondrow");
                t.SetString(0, 0, "thirdrow");
                foreach (TableRow tableRow in t)
                {
                    Assert.IsInstanceOf(typeof(TableRow), tableRow);//assert important as Table's parent also implements an iterator that yields rows. We want TableRows when 
                    //we expicitly iterate a Table with foreach
                }
            }

        }


        private static void IterateTableOrView(TableOrView tov)
        {

            if (tov != null && !tov.IsEmpty )//the isempty test is just to trick ReSharper not to suggest tov be declared as Ienummerable<Row>
            {
                foreach (Row row in tov)
                //loop through a TableOrview should get os Row classes EVEN IF THE UNDERLYING IS A TABLE
                {
                    Assert.IsInstanceOf(typeof(Row), row);
                    //we explicitly iterate a Table with foreach
                }
            }
        }

        [Test]
        public static void TableOrViewIterationTest()
        {
            using
                (
                var t = new Table("stringfield".String())
                )
            {
                t.AddEmptyRow(3);
                t.SetString(0, 0, "firstrow");
                t.SetString(0, 0, "secondrow");
                t.SetString(0, 0, "thirdrow");
                IterateTableOrView(t);
                IterateTableOrView(t.Where().FindAll());
            }
        }


        [Test]
        public static void TableIsValidTest()
        {
            using (var t = new Table())
            {
                Assert.AreEqual(true, t.IsValid());
                t.AddColumn(DataType.Int, "do'h");                
                Assert.AreEqual(true, t.IsValid());
                using (var sub = new Table())
                {
                    t.AddColumn(DataType.Table, "sub");                    
                    t.Add(42, sub);
                    Assert.AreEqual(true, sub.IsValid());
                    t.Set(0, 43, null);
                    Table sub2 = t.GetSubTable(1, 0);
                    Assert.AreEqual(true, sub2.IsValid());
                    Assert.AreEqual(true, sub.IsValid());
                    t.Add(42, sub);
                    Table sub3 = t.GetSubTable(1, 1);
                    t.Set(1, 45, null);
                    Assert.AreEqual(false, sub3.IsValid());
                    t.Set(1, 45, sub);
                    Assert.AreEqual(false, sub3.IsValid());
                }
            }
        }



        [Test]
        public static void TableSharedSpecTest()
        {
            //create a table1 with a subtable1 column in it, with an int in it. The subtable with an int will 
            //have a shared spec, as subtable1 spec is part of the table1 and thus tableSharedSpec should return true

            using (var table1 = new Table("subtable".Table(
                                            "int".Int()
                                            )
                                         )
                  )
            {
                Assert.AreEqual(false, table1.HasSharedSpec());
                table1.AddEmptyRow(1);//add an empty subtalbe to the first column in the table
                //todo:test subtable in table definition writeover situations 
                //table1.ClearSubTable(0, 0);//somehow i think this is not legal? similarily putting in a subtable that does not match the spec of the master table

                using (Table sub = table1.GetSubTable(0, 0))
                {
                    sub.Add(42);//add row with the int 42 in it
                    Assert.AreEqual(true, sub.HasSharedSpec());
                }
            }
        }


        //test with the newest kind of field object constructores - lasse's inherited specialized ones

        [Test]
        public static void TableSubTableSubTable()
        {
            string actualres;
            using (var t = new Table(
                "root".SubTable(
                    "s1".SubTable(
                        "s2".Table(
                            "fld".Int())))))
            {
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name, "table with subtable with subtable",
                                                t);
            }
            const string expectedres = @"------------------------------------------------------
Column count: 1
Table Name  : table with subtable with subtable
------------------------------------------------------
 0      Table  root                
    0      Table  s1                  
       0      Table  s2                  
          0        Int  fld                 
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
        }




        [Test]
        public static void TypedFieldClasses()
        {
            String actualres;
            using (
                var newFieldClasses = new Table(
                    new StringField("F1"),
                    new IntField("F2"),
                    new SubTableField("Sub1",
                                      new StringField("F11"),
                                      new IntField("F12"))
                    ))
            {
                newFieldClasses.AddColumn(DataType.String, "Buksestørrelse");

                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                                                "table created with all types using the new field classes",
                                                newFieldClasses);
            }
            const string expectedres = @"------------------------------------------------------
Column count: 4
Table Name  : table created with all types using the new field classes
------------------------------------------------------
 0     String  F1                  
 1        Int  F2                  
 2      Table  Sub1                
    0     String  F11                 
    1        Int  F12                 
 3     String  Buksestørrelse      
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
        }





        [Test]
        public static void TableCloneLostFieldNameTest()
        {
            const string fnsub =
            "sub";
            const string fnsubsub = "subsub";
            String actualres = "";
            using (var smallTable = new Table(fnsub.Table(fnsubsub.Table())))
            {
                using (var tempSubTable = new Table(fnsubsub.Table()))
                {
                    smallTable.Add(tempSubTable);
                }                //okay that tempsubtable is disposed here, as adding subtables is done by copying their structure and value
                Assert.AreEqual(fnsub, smallTable.GetColumnName(0));
                Assert.AreEqual(fnsubsub, smallTable.GetSubTable(0, 0).GetColumnName(0));
                Spec spec1 = smallTable.Spec;
                Assert.AreEqual(fnsub, spec1.GetColumnName(0));
                Spec spec2 = spec1.GetSpec(0);
                Assert.AreEqual(fnsubsub, spec2.GetColumnName(0));

                var clonedTable = smallTable.Clone();
                if (clonedTable != null)
                {
                    Assert.AreEqual(fnsub, clonedTable.GetColumnName(0));
                    Assert.AreEqual(fnsubsub, clonedTable.GetSubTable(0, 0).GetColumnName(0));
                    Spec spec1S = smallTable.Spec;
                    Assert.AreEqual(fnsub, spec1S.GetColumnName(0));
                    Spec spec2S = spec1S.GetSpec(0);
                    Assert.AreEqual(fnsubsub, spec2S.GetColumnName(0));


                    actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                                "tableclone subsub fieldnames test",
                                smallTable.Clone());



                }
                else
                {
                    { Assert.AreEqual("clonedTable was null", "it should have contained data"); }
                }

            }
            const string expectedres = @"------------------------------------------------------
Column count: 1
Table Name  : tableclone subsub fieldnames test
------------------------------------------------------
 0      Table  sub                 
    0      Table  subsub              
------------------------------------------------------

Table Data Dump. Rows:1
------------------------------------------------------
{ //Start row 0
sub:[ //0 rows]//column 0
} //End row 0
------------------------------------------------------
";
            TestHelper.Cmp(expectedres, actualres);
        }


        //todo:this test fails intentionally, update when the base library has fixed the field name bug in clone
        [Test]
        public static void TableCloneTest4()
        {
            String actualres;
            using (
                var newFieldClasses = new Table(
                //new StringField("F1"),
                //new IntField("F2"),
                    new SubTableField("Sub1"//),
                //                      new StringField("F11"),
                //                      new IntField("F12"))
                    )))
            {
                newFieldClasses.AddColumn(DataType.String, "Buksestørrelse");

                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                                                "table created with all types using the new field classes",
                                                newFieldClasses.Clone());
            }
            const string expectedres = @"------------------------------------------------------
Column count: 2
Table Name  : table created with all types using the new field classes
------------------------------------------------------
 0      Table  Sub1                
 1     String  Buksestørrelse      
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
        }




        [Test]
        public static void TableCloneTest3()
        {
            String actualres;
            using (
                var newFieldClasses = new Table(
                //new StringField("F1"),
                //new IntField("F2"),
                //    new SubTableField("Sub1",
                //                      new StringField("F11"),
                //                      new IntField("F12"))
                    ))
            {
                newFieldClasses.AddColumn(DataType.String, "Buksestørrelse");


                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                                                "table created with all types using the new field classes",
                                                newFieldClasses.Clone());
            }
            const string expectedres = @"------------------------------------------------------
Column count: 1
Table Name  : table created with all types using the new field classes
------------------------------------------------------
 0     String  Buksestørrelse      
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
        }




        [Test]
        public static void TableCloneTest2()
        {
            String actualres;
            using (
                var newFieldClasses = new Table(
                //new StringField("F1"),
                //new IntField("F2"),
                    new SubTableField("Sub1",
                                      new StringField("F11"),
                                      new IntField("F12"))
                    ))
            {
                newFieldClasses.AddColumn(DataType.String, "Buksestørrelse");


                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                                                "table created with all types using the new field classes",
                                                newFieldClasses.Clone());
            }
            const string expectedres = @"------------------------------------------------------
Column count: 2
Table Name  : table created with all types using the new field classes
------------------------------------------------------
 0      Table  Sub1                
    0     String  F11                 
    1        Int  F12                 
 1     String  Buksestørrelse      
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
        }


        [Test]
        public static void TableCloneTest()
        {
            String actualres;
            using (
                var newFieldClasses = new Table(
                    new StringField("F1"),
                    new IntField("F2"),
                    new SubTableField("Sub1",
                                      new StringField("F11"),
                                      new IntField("F12"))
                    ))
            {
                newFieldClasses.AddColumn(DataType.String, "Buksestørrelse");


                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                                                "table created with all types using the new field classes",
                                                newFieldClasses.Clone());
            }
            const string expectedres = @"------------------------------------------------------
Column count: 4
Table Name  : table created with all types using the new field classes
------------------------------------------------------
 0     String  F1                  
 1        Int  F2                  
 2      Table  Sub1                
    0     String  F11                 
    1        Int  F12                 
 3     String  Buksestørrelse      
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
        }


        //illustration of field usage, usecase / unit test

        //The user can decide to create his own field types, that could then be used in several different table definitions, to ensure 
        //that certain kinds of fields used by common business logic always were of the correct type and setup
        //For example a field called itemcode that currently hold integers to denote owned item codes in a game,
        //but perhaps later should be a string field instead
        //if you have many IntegerField fields in many tables with item codes in them, you could use Itemcode instead, and then effect the change to string
        //only by changing the ineritance of the Itemcode type from IntegerField to StringField
        //thus by introducing your own class, You hide the field implementation detail from the users using this field type


        private class ItemCode : IntField
        //whenever ItemCode is specified in a table definition, an IntegerField is created
        {
            public ItemCode(String columnName)
                : base(columnName)
            {
            }
        }

        //because of a defense against circular field references, the subtablefield cannot be used this way, however you can make a method that returns an often
        //used subtable specification like this instead :

        //subtable field set used by our general login processing system
        private static SubTableField OwnedItems()
        {
            return new SubTableField(
                ("OwnedItems"),
                new StringField("Item Name"),
                new ItemCode("ItemId"),
                new IntField("Number Owned"),
                new BoolField("ItemPowerLevel"));
        }

        //game state dataset used by our general game saving system for casual games
        private static SubTableField GameSaveFields()
        {
            return new SubTableField(
                ("GameState"),
                new StringField("SaveDate"),
                new IntField("UserId"),
                new StringField("Users description"),
                new BinaryField("GameData1"),
                new StringField("GameData2"));
        }


        //creation of table using user overridden or generated fields (ensuring same subtable structure across applications or tables)
        [Test]
        public static void UserCreatedFields()
        {
            String actualres;

            using (
                var game1 = new Table(
                    OwnedItems(),
                    new IntField("UserId"),
                //some game specific stuff. All players are owned by some item, don't ask me why
                    new BinaryField("BoardLayout"), //game specific
                    GameSaveFields())
                )
            {
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name + "1",
                                                "table created user defined types and methods", game1);
            }
            string expectedres =
                @"------------------------------------------------------
Column count: 4
Table Name  : table created user defined types and methods
------------------------------------------------------
 0      Table  OwnedItems          
    0     String  Item Name           
    1        Int  ItemId              
    2        Int  Number Owned        
    3       Bool  ItemPowerLevel      
 1        Int  UserId              
 2     Binary  BoardLayout         
 3      Table  GameState           
    0     String  SaveDate            
    1        Int  UserId              
    2     String  Users description   
    3     Binary  GameData1           
    4     String  GameData2           
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);




            using (var game2 = new Table(
                OwnedItems(),
                new ItemCode("UserId"), //game specific
                new ItemCode("UsersBestFriend"), //game specific
                new IntField("Game Character Type"), //game specific
                GameSaveFields()))
            {
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name + "2",
                                                "table created user defined types and methods", game2);
            }
            expectedres =
                @"------------------------------------------------------
Column count: 5
Table Name  : table created user defined types and methods
------------------------------------------------------
 0      Table  OwnedItems          
    0     String  Item Name           
    1        Int  ItemId              
    2        Int  Number Owned        
    3       Bool  ItemPowerLevel      
 1        Int  UserId              
 2        Int  UsersBestFriend     
 3        Int  Game Character Type 
 4      Table  GameState           
    0     String  SaveDate            
    1        Int  UserId              
    2     String  Users description   
    3     Binary  GameData1           
    4     String  GameData2           
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);

        }


        //this kind of creation call should be legal - it creates a totally empty table, then only later sets up a field        
        [Test]
        public static void SubTableNoFields()
        {
            String actualres;
            using (
                var notSpecifyingFields = new Table(
                    "subtable".Table()
                    )) //at this point we have created a table with no fields
            {
                notSpecifyingFields.AddColumn(DataType.String, "Buksestørrelse");
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                                                "one field Created in two steps with table add column",
                                                notSpecifyingFields);
            }
            const string expectedres =
                @"------------------------------------------------------
Column count: 2
Table Name  : one field Created in two steps with table add column
------------------------------------------------------
 0      Table  subtable            
 1     String  Buksestørrelse      
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
        }


        [Test]
        public static void TestHandleAcquireOneField()
        {
            string actualres;
            using (var testtbl = new Table(new Field("name", DataType.String)))
            {
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name, "NameField", testtbl);
            }
            const string expectedres =
                @"------------------------------------------------------
Column count: 1
Table Name  : NameField
------------------------------------------------------
 0     String  name                
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
        }


        [Test]
        public static void TestHandleAcquireSeveralFields()
        {
            String actualres;
            using (var testtbl3 = new Table(
                "Name".TightDbString(),
                "Age".TightDbInt(),
                "count".TightDbInt(),
                "Whatever".TightDbMixed()
                ))
            {
                //long  test = testtbl3.getdllversion_CSH();
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name, "four columns, Last Mixed", testtbl3);
            }
            const string expectedres =
                @"------------------------------------------------------
Column count: 4
Table Name  : four columns, Last Mixed
------------------------------------------------------
 0     String  Name                
 1        Int  Age                 
 2        Int  count               
 3      Mixed  Whatever            
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
        }

        //test the alternative table dumper implementation that does not use table class
        [Test]
        public static void TestAllFieldTypesStringExtensions()
        {
            string actualres1;
            string actualres2;
            using (var t = new Table(
                "Count".Int(),
                "Valid".Bool(),
                "Name".String(),
                "BLOB".Binary(),
                "Items".SubTable(
                    "ItemCount".Int(),
                    "ItemName".String()),
                "HtmlPage".Mixed(),
                "FirstSeen".Date(),
                "Fraction".Float(),
                "QuiteLargeNumber".Double()
                ))
            {
                actualres1 = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                                                 "Table with all allowed types (String Extensions)", t);
                actualres2 = TestHelper.TableDumperSpec(MethodBase.GetCurrentMethod().Name,
                                                     "Table with all allowed types (String Extensions)", t);
            }
            const string expectedres =
                @"------------------------------------------------------
Column count: 9
Table Name  : Table with all allowed types (String Extensions)
------------------------------------------------------
 0        Int  Count               
 1       Bool  Valid               
 2     String  Name                
 3     Binary  BLOB                
 4      Table  Items               
    0        Int  ItemCount           
    1     String  ItemName            
 5      Mixed  HtmlPage            
 6       Date  FirstSeen           
 7      Float  Fraction            
 8     Double  QuiteLargeNumber    
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres1);
            TestHelper.Cmp(expectedres, actualres2);
        }



        //test the alternative table dumper implementation that does not use table class
        [Test]
        public static void TestAllFieldTypesFieldClass()
        {
            string actualres1;
            string actualres2;
            using (var t = new Table(
                new Field("Count", DataType.Int),
                new Field("Valid", DataType.Bool),
                new Field("Name", DataType.String),
                new Field("BLOB", DataType.Binary),
                new Field("Items",
                          new Field("ItemCount", DataType.Int),
                          new Field("ItemName", DataType.String)),
                new Field("HtmlPage", DataType.Mixed),
                new Field("FirstSeen", DataType.Date),
                new Field("Fraction", DataType.Float),
                new Field("QuiteLargeNumber", DataType.Double)
                ))
            {
                actualres1 = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                                                 "Table with all allowed types (Field)", t);
                actualres2 = TestHelper.TableDumperSpec(MethodBase.GetCurrentMethod().Name,
                                                     "Table with all allowed types (Field)", t);
            }
            const string expectedres =
                @"------------------------------------------------------
Column count: 9
Table Name  : Table with all allowed types (Field)
------------------------------------------------------
 0        Int  Count               
 1       Bool  Valid               
 2     String  Name                
 3     Binary  BLOB                
 4      Table  Items               
    0        Int  ItemCount           
    1     String  ItemName            
 5      Mixed  HtmlPage            
 6       Date  FirstSeen           
 7      Float  Fraction            
 8     Double  QuiteLargeNumber    
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres1);
            TestHelper.Cmp(expectedres, actualres2);
        }

        //test the alternative table dumper implementation that does not use table class
        [Test]
        public static void TestAllFieldTypesFieldClassStrings()
        {
            string actualres1;
            string actualres2;
            using (var t = new Table(
                new Field("Count1", "integer"),
                new Field("Count2", "Integer"), //Any case is okay
                new Field("Count3", "int"),
                new Field("Count4", "INT"), //Any case is okay
                new Field("Valid1", "boolean"),
                new Field("Valid2", "bool"),
                new Field("Valid3", "Boolean"),
                new Field("Valid4", "Bool"),
                new Field("Name1", "string"),
                new Field("Name2", "string"),
                new Field("Name3", "str"),
                new Field("Name4", "Str"),
                new Field("BLOB1", "binary"),
                new Field("BLOB2", "Binary"),
                new Field("BLOB3", "blob"),
                new Field("BLOB4", "Blob"),
                new Field("Items",
                          new Field("ItemCount", "integer"),
                          new Field("ItemName", "string")),
                new Field("HtmlPage1", "mixed"),
                new Field("HtmlPage2", "MIXED"),
                new Field("FirstSeen1", "date"),
                new Field("FirstSeen2", "daTe"),
                new Field("Fraction1", "float"),
                new Field("Fraction2", "Float"),
                new Field("QuiteLargeNumber1", "double"),
                new Field("QuiteLargeNumber2", "Double")
                ))
            {
                actualres1 = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                                                 "Table with all allowed types (Field_string)", t);
                actualres2 = TestHelper.TableDumperSpec(MethodBase.GetCurrentMethod().Name,
                                                     "Table with all allowed types (Field_string)", t);
            }
            const string expectedres =
                @"------------------------------------------------------
Column count: 25
Table Name  : Table with all allowed types (Field_string)
------------------------------------------------------
 0        Int  Count1              
 1        Int  Count2              
 2        Int  Count3              
 3        Int  Count4              
 4       Bool  Valid1              
 5       Bool  Valid2              
 6       Bool  Valid3              
 7       Bool  Valid4              
 8     String  Name1               
 9     String  Name2               
10     String  Name3               
11     String  Name4               
12     Binary  BLOB1               
13     Binary  BLOB2               
14     Binary  BLOB3               
15     Binary  BLOB4               
16      Table  Items               
    0        Int  ItemCount           
    1     String  ItemName            
17      Mixed  HtmlPage1           
18      Mixed  HtmlPage2           
19       Date  FirstSeen1          
20       Date  FirstSeen2          
21      Float  Fraction1           
22      Float  Fraction2           
23     Double  QuiteLargeNumber1   
24     Double  QuiteLargeNumber2   
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres1);
            TestHelper.Cmp(expectedres, actualres2);
        }



        //test the alternative table dumper implementation that does not use table class
        [Test]
        public static void TestAllFieldTypesTypedFields()
        {
            string actualres1;
            string actualres2;
            using (var t = new Table(
                new IntField("Count"),
                new BoolField("Valid"),
                new StringField("Name"),
                new BinaryField("BLOB"),
                new SubTableField("Items",
                                  new IntField("ItemCount"),
                                  new StringField("ItemName")),
                new MixedField("HtmlPage"),
                new DateField("FirstSeen"),
                new FloatField("Fraction"),
                new DoubleField("QuiteLargeNumber")
                ))
            {
                actualres1 = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                                                 "Table with all allowed types (Typed Field)", t);
                actualres2 = TestHelper.TableDumperSpec(MethodBase.GetCurrentMethod().Name,
                                                     "Table with all allowed types (Typed Field)", t);
            }
            const string expectedres =
                @"------------------------------------------------------
Column count: 9
Table Name  : Table with all allowed types (Typed Field)
------------------------------------------------------
 0        Int  Count               
 1       Bool  Valid               
 2     String  Name                
 3     Binary  BLOB                
 4      Table  Items               
    0        Int  ItemCount           
    1     String  ItemName            
 5      Mixed  HtmlPage            
 6       Date  FirstSeen           
 7      Float  Fraction            
 8     Double  QuiteLargeNumber    
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres1);
            TestHelper.Cmp(expectedres, actualres2);
        }


        //test with a subtable
        [Test]
        public static void TestMixedConstructorWithSubTables()
        {
            string actualres;
            using (
                var testtbl = new Table(
                    "Name".TightDbString(),
                    "Age".TightDbInt(),
                    new Field("age2", DataType.Int),
                    new Field("age3", "Int"),
                //                new IntegerField("Age3"),
                    new Field("comments",
                              new Field("phone#1", DataType.String),
                              new Field("phone#2", DataType.String),
                              new Field("phone#3", "String"),
                              "phone#4".TightDbString()
                        ),
                    new Field("whatever", DataType.Mixed)
                    ))
            {
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name, "six colums,sub four columns",
                                                testtbl);
            }
            const string expectedres =
                @"------------------------------------------------------
Column count: 6
Table Name  : six colums,sub four columns
------------------------------------------------------
 0     String  Name                
 1        Int  Age                 
 2        Int  age2                
 3        Int  age3                
 4      Table  comments            
    0     String  phone#1             
    1     String  phone#2             
    2     String  phone#3             
    3     String  phone#4             
 5      Mixed  whatever            
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
        }




        [Test]
        //[NUnit.Framework.Ignore("Need to write tests that test for correct deallocation of table when out of scope")]
        //scope has been thoroughly debugged and does work perfectly in all imagined cases, but the testing was done before unit tests had been created
        public static void TestTableScope()
        {
            Table testTable; //bad way to code this but i need the reference after the using clause
            using (testTable = new Table())
            {

                Assert.False(testTable.IsDisposed); //do a test to see that testtbl has a valid table handle 
            }
            Assert.True(testTable.IsDisposed);
            //do a test here to see that testtbl now does not have a valid table handle


        }



        //while You cannot cross-link parents and subtables inside a new table() construct, you can try to do so, by deliberatly changing
        //the subtable references in Field objects that You instantiate yourself -and then call Table.create(Yourfiled) with a 
        //field definition that is self referencing.
        //however, currently this is not possible as seen in the example below.
        //the subtables cannot be changed directly, so all You can do is create new objects that has old already created objects as subtables
        //therefore a tree structure, no recursion.

        //below is my best shot at someone trying to create a table with custom built cross-linked field definitions (and failing)

        //I did not design the Field type to be used on its own like the many examples below. However , none of these weird uses break anything
        [Test]
        public static void TestIllegalFieldDefinitions1()
        {
            Field f5 = "f5".Int(); //create a field reference, type does not matter
            f5 = "f5".Table(f5); //try to overwrite the field object with a new object that references itself 
            string actualres;
            using (
                var t = new Table(f5))
            //this will not crash or loop forever the subtable field does not references itself 
            {
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name, "self-referencing subtable", t);
            }
            const string expectedres =
                @"------------------------------------------------------
Column count: 1
Table Name  : self-referencing subtable
------------------------------------------------------
 0      Table  f5                  
    0        Int  f5                  
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
        }

        [Test]
        public static void TestIllegalFieldDefinitions2()
        {
            Field fc = "fc".Int(); //create a field reference, type does not matter
            Field fp = "fp".Table(fc); //let fp be the parent table subtable column, fc be the sole field in a subtable

            fc = "fc".Table(fp); //then change the field type from int to subtable and reference the parent

            //You now think You have illegal field definitions in fc and fp as both are subtables and both reference the other as the sole subtable field
            //however, they are new objects that reference the old classes that they replaced.
            String actualres;
            using (
                var t2 = new Table(fc))
            {
                //should crash too
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                                                "subtable that has subtable that references its parent #1", t2);
            }
            const String expectedres =
                @"------------------------------------------------------
Column count: 1
Table Name  : subtable that has subtable that references its parent #1
------------------------------------------------------
 0      Table  fc                  
    0      Table  fp                  
       0        Int  fc                  
------------------------------------------------------

";


            TestHelper.Cmp(expectedres, actualres);
        }

        [Test]
        public static void TestIllegalFieldDefinitions3()
        {
            Field fc = "fc".Int(); //create a field reference, type does not matter
            Field fp = "fp".Table(fc); //let fp be the parent table subtable column, fc be the sole field in a subtable
            // ReSharper disable RedundantAssignment
            fc = "fc".Table(fp); //then change the field type from int to subtable and reference the parent
            // ReSharper restore RedundantAssignment

            String actualres;
            using (
                var t3 = new Table(fp))
            {
                //should crash too
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                                                "subtable that has subtable that references its parent #2", t3);
            }
            const String expectedres =
                @"------------------------------------------------------
Column count: 1
Table Name  : subtable that has subtable that references its parent #2
------------------------------------------------------
 0      Table  fp                  
    0        Int  fc                  
------------------------------------------------------

";

            TestHelper.Cmp(expectedres, actualres);

        }

        //super creative attemt at creating a cyclic graph of Field objects
        //still it fails because the array being manipulated is from GetSubTableArray and thus NOT the real list inside F1 even though the actual field objects referenced from the array ARE the real objects
        //point is - You cannot stuff field definitions down into the internal array this way
        [Test]
        public static void TestCyclicFieldDefinition1()
        {

            Field f1 = "f10".SubTable("f11".Int(), "f12".Int());
            var subTableElements = f1.GetSubTableArray();
            subTableElements[0] = f1; //and the "f16" field in f1.f15.f16 is now replaced with f1.. recursiveness


            string actualres;
            using (var t4 = new Table(f1))
            {
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name, "cyclic field definition", t4);
            }
            const String expectedres =
                @"------------------------------------------------------
Column count: 1
Table Name  : cyclic field definition
------------------------------------------------------
 0      Table  f10                 
    0        Int  f11                 
    1        Int  f12                 
------------------------------------------------------

";

            TestHelper.Cmp(expectedres, actualres);
        }

        //dastardly creative terroristic attemt at creating a cyclic graph of Field objects
        //this creative approach succeeded in creating a stack overflow situation when the table is being created, but now it is not possible as AddSubTableFields has been made
        //internal, thus unavailable in customer assemblies.

        private class AttemptCircularField : Field
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"),
             System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
                 MessageId = "fielddefinitions"),
             System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
                 MessageId = "fieldName")]
            // ReSharper disable UnusedParameter.Local
            public void setsubtablearray(String fieldName, Field[] fielddefinitions)
            //make the otherwise hidden addsubtablefield public
            // ReSharper restore UnusedParameter.Local
            {
                //uncommenting the line below should create a compiletime error (does now) or else this unit test wil bomb the system
                //                AddSubTableFields(this, fieldName,fielddefinitions);
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
                MessageId = "columnName"),
             System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
                 MessageId = "subTableFieldsArray")]
            // ReSharper disable UnusedParameter.Local
            public AttemptCircularField(string columnName, params Field[] subTableFieldsArray)
            // ReSharper restore UnusedParameter.Local
            {
                FieldType = DataType.Table;
            }
        }


        [Test]
        public static void TableSetSubtable()
        {
            using (var t = new Table(
                                     "do'h".Int(),
                                     "sub".SubTable(
                                                      "substringfield1".String(),
                                                      "substringfield2".String()
                                                   ),
                                     "mazda".Int()
                                    )
                   )
            {
                using (var sub = new Table(
                                                      "substringfield1".String(),
                                                      "substringfield2".String()
                                           )
                      )
                {
                    const string string00 = "stringvalueC0R0";
                    sub.Add(string00, "stringvalue2R0");
                    sub.Add("stringvalue1R1", "stringvalue2R1");
                    t.AddEmptyRow(1);
                    t.SetSubTable(1,0,sub);
                    Table subreturned = t.GetSubTable(1, 0);
                    Assert.AreEqual(string00, subreturned.GetString(0, 0));
                }
            }
        }


        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableSetSubtableBadSchema()
        {
            using (var t = new Table(
                                     "do'h".Int(),
                                     "sub".SubTable(
                                                      "substringfield1".String(),
                                                      "substringfield2".String()
                                                   ),
                                     "mazda".Int()
                                    )
                   )
            {
                using (var sub = new Table(
                                                      "substringfield1".String(),
                                                      "substringfield2".String(),
                                                      "substringfield3".String()
                                           )
                      )
                {
                    const string string00 = "stringvalueC0R0";
                    sub.Add(string00, "stringvalue2R0","stringvalue3R0");
                    sub.Add("stringvalue1R1", "stringvalue2R1", "stringvalue3R1");
                    t.AddEmptyRow(1);                    
                    t.SetSubTable(1, 0, sub); //should throw an exception as the sub is not with a compatible schema
                    Table subreturned = t.GetSubTable(1, 0);
                    Assert.AreEqual(string00, subreturned.GetString(0, 0));
                }
            }
        }



        [Test]
        [ExpectedException("System.ArgumentNullException")]
        public static void TestCyclicFieldDefinition2()
        {

            var f1 = new AttemptCircularField("f1", null);
            //do not care about last parameter we're trying to crash the system
            var subs = new Field[2];
            subs[0] = f1;
            f1.setsubtablearray("f2", subs);

            string actualres;
            using (var t4 = new Table(f1))
            {
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                                                "cyclic field definition using field inheritance to get at subtable field list",
                                                t4);
            }
            const String expectedres =
                @"------------------------------------------------------
Column count: 1
Table Name  : cyclic field definition
------------------------------------------------------
 0      Table  f10                 
    0        Int  f11                 
    1        Int  f12                 
------------------------------------------------------

";

            TestHelper.Cmp(expectedres, actualres);
        }






        [Test]
        public static void TestIllegalFieldDefinitions4()
        {

            Field f10 = "f10".SubTable("f11".Int(), "f12".Int());
            f10.FieldType = DataType.Int;
            //at this time, the subtable array still have some subtables in it
            string actualres;
            using (var t4 = new Table(f10))
            {
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name, "just an int field, no subs", t4);
            }
            const String expectedres =
                @"------------------------------------------------------
Column count: 1
Table Name  : just an int field, no subs
------------------------------------------------------
 0        Int  f10                 
------------------------------------------------------

";

            TestHelper.Cmp(expectedres, actualres);
        }

        [Test]
        public static void TestIllegalFieldDefinitions5()
        {
            Field f10 = "f10".SubTable("f11".Int(), "f12".Int());
            f10.FieldType = DataType.Table;

            String actualres;
            using (
                var t5 = new Table(f10))
            {
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name, "subtable with two int fields", t5);
                //This is sort of okay, first adding a subtable, then
            }
            const String expectedres =
                @"------------------------------------------------------
Column count: 1
Table Name  : subtable with two int fields
------------------------------------------------------
 0      Table  f10                 
    0        Int  f11                 
    1        Int  f12                 
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
            //changing mind and making it just and int field, and then changing mind again and setting it as subtable type
            //and thus resurfacing the two subfields. no harm done.
        }

        [Test]
        public static void TestCreateStrangeTable1()
        {
            //create a table with two columns that are the same name except casing (this might be perfectly legal, I dont know)
            String actualres;
            using (var badtable = new Table("Age".Int(), "age".Int()))
            {
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name, "two fields, case is differnt",
                                                badtable);
            }
            const String expectedres =
                @"------------------------------------------------------
Column count: 2
Table Name  : two fields, case is differnt
------------------------------------------------------
 0        Int  Age                 
 1        Int  age                 
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
        }

        [Test]
        public static void TestCreateStrangeTable2()
        {
            //Create a table with two columns with the same name and type
            String actualres;
            using (var badtable2 = new Table("Age".Int(), "Age".Int()))
            {
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name, "two fields name and type the same",
                                                badtable2);
            }
            const string expectedres =
                @"------------------------------------------------------
Column count: 2
Table Name  : two fields name and type the same
------------------------------------------------------
 0        Int  Age                 
 1        Int  Age                 
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);

        }


        //Test if two table creations where the second happens before the first is out of scope, works okay
        [Test]
        public static void TestCreateTwoTables()
        {
            var actualres = new StringBuilder(); //we add several table dumps into one compare string in this test
            using (
                var testtbl1 = new Table(
                    new Field("name", DataType.String),
                    new Field("age", DataType.Int),
                    new Field("comments",
                              new Field("phone#1", DataType.String),
                              new Field("phone#2", DataType.String)),
                    new Field("whatever", DataType.Mixed)))
            {
                actualres.Append(TestHelper.TableDumperSpec(MethodBase.GetCurrentMethod().Name,
                                                         "four columns , sub two columns (Field)", testtbl1));

                using ( //and we create a second table while the first is in scope
                    var testtbl2 = new Table(
                        new Field("name", "String"),
                        new Field("age", "Int"),
                        new Field("comments",
                                  new Field("phone#1", DataType.String), //one way to declare a string
                                  new Field("phone#2", "String"), //another way
                                  "more stuff".SubTable(
                                      "stuff1".String(), //and yet another way
                                      "stuff2".String(),
                                      "ÆØÅæøå".String())
                            ),
                        new Field("whatever", DataType.Mixed)))
                {
                    actualres.Append(TestHelper.TableDumperSpec(MethodBase.GetCurrentMethod().Name,
                                                             "four columns, sub three subsub three", testtbl2));
                }
            }
            File.WriteAllText(MethodBase.GetCurrentMethod().Name + ".txt", actualres.ToString());
            const string expectedres =
                @"------------------------------------------------------
Column count: 4
Table Name  : four columns , sub two columns (Field)
------------------------------------------------------
 0     String  name                
 1        Int  age                 
 2      Table  comments            
    0     String  phone#1             
    1     String  phone#2             
 3      Mixed  whatever            
------------------------------------------------------

------------------------------------------------------
Column count: 4
Table Name  : four columns, sub three subsub three
------------------------------------------------------
 0     String  name                
 1        Int  age                 
 2      Table  comments            
    0     String  phone#1             
    1     String  phone#2             
    2      Table  more stuff          
       0     String  stuff1              
       1     String  stuff2              
       2     String  ÆØÅæøå              
 3      Mixed  whatever            
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres.ToString());
        }

        [Test]
        public static void TestCreateStrangeTable3()
        {
            string actualres;
            using (
                var reallybadtable3 = new Table("Age".Int(),
                                                "Age".Int(),
                                                "".String(),
                                                "".String()))
            {
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                                                "same names int two empty string names", reallybadtable3);
            }
            const string expectedres = @"------------------------------------------------------
Column count: 4
Table Name  : same names int two empty string names
------------------------------------------------------
 0        Int  Age                 
 1        Int  Age                 
 2     String                      
 3     String                      
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
        }

        [Test]
        public static void TestCreateStrangeTable4()
        {
            string actualres;
            using (
                var reallybadtable4 = new Table("Age".Int(),
                                                "Age".Mixed(),
                                                "".String(),
                                                "".Mixed()))
            {
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                                                "same names, empty names, mixed types", reallybadtable4);
            }
            const string expectedres =
                @"------------------------------------------------------
Column count: 4
Table Name  : same names, empty names, mixed types
------------------------------------------------------
 0        Int  Age                 
 1      Mixed  Age                 
 2     String                      
 3      Mixed                      
------------------------------------------------------

";
            TestHelper.Cmp(expectedres, actualres);
        }


        [Test]
        public static void TableMaximumDouble()
        {
            using (var myTable = new Table("double".Double()))
            {
                myTable.Add(1d);
                Assert.AreEqual(1d, myTable.MaximumDouble(0));
            }
        }

        //should probably be split up into more tests, but this one touches all c++ functions which is okay for now
        [Test]
        public static void TableAggregate()
        {
            using (var myTable = new Table("strfield".String(),
                "int".Int(),
                "float".Float(),
                "double".Double())
                )
            {
                myTable.Add("tv", 1, 3f, 5d);
                myTable.Add("tv", 3, 9f, 15d);
                myTable.Add("tv", 5, 15f, 25d);
                myTable.Add("notv", -1000, -1001f, -1002d);

                string actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name, "table with testdata for TableAggregate",
                    myTable);

                const string expectedres =
        @"------------------------------------------------------
Column count: 4
Table Name  : table with testdata for TableAggregate
------------------------------------------------------
 0     String  strfield            
 1        Int  int                 
 2      Float  float               
 3     Double  double              
------------------------------------------------------

Table Data Dump. Rows:4
------------------------------------------------------
{ //Start row 0
strfield:tv,//column 0
int:1,//column 1
float:3,//column 2
double:5//column 3
} //End row 0
{ //Start row 1
strfield:tv,//column 0
int:3,//column 1
float:9,//column 2
double:15//column 3
} //End row 1
{ //Start row 2
strfield:tv,//column 0
int:5,//column 1
float:15,//column 2
double:25//column 3
} //End row 2
{ //Start row 3
strfield:notv,//column 0
int:-1000,//column 1
float:-1001,//column 2
double:-1002//column 3
} //End row 3
------------------------------------------------------
";
                TestHelper.Cmp(expectedres, actualres);


                using (
                    TableView myTableView = myTable.FindAllString(0, "tv")

                    )
                {
                    Assert.AreEqual(3, myTable.CountString(0, "tv"));
                    Assert.AreEqual(1, myTable.CountLong(1, 3));
                    Assert.AreEqual(1, myTable.CountFloat(2, 15f));
                    Assert.AreEqual(1, myTable.CountDouble(3, 15d));

                    Assert.AreEqual(0, myTable.CountString(0, "xtv"));
                    Assert.AreEqual(0, myTable.CountLong(1, -3));
                    Assert.AreEqual(0, myTable.CountFloat(2, -15f));
                    Assert.AreEqual(0, myTable.CountDouble(3, -15d));


                    Assert.AreEqual(5, myTable.MaximumLong("int"));
                    Assert.AreEqual(15f, myTable.MaximumFloat("float"));
                    Assert.AreEqual(25d, myTable.MaximumDouble(3));

                    Assert.AreEqual(-1000, myTable.MinimumLong(1));
                    Assert.AreEqual(-1001f, myTable.MinimumFloat(2));
                    Assert.AreEqual(-1002d, myTable.MinimumDouble("double"));

                    long sl = myTable.SumLong(1);
                    Assert.AreEqual(3f, myTable.GetFloat(2, 0));
                    Assert.AreEqual(9f, myTable.GetFloat(2, 1));
                    Assert.AreEqual(15f, myTable.GetFloat(2, 2));
                    Assert.AreEqual(-1001f, myTable.GetFloat(2, 3));
                    double sf = myTable.SumFloat(2);
                    double sd = myTable.SumDouble(3);
                    double sftv = myTableView.SumFloat(2);

                    Assert.AreEqual(-1000 + 1 + 3 + 5, sl);
                    Assert.AreEqual(-1001f + 3f + 9f + 15f, sf);
                    Assert.AreEqual(-1002d + 5d + 15d + 25d, sd);

                    Assert.AreEqual((1 + 3 + 5 - 1000) / 4d, myTable.AverageLong(1));
                    Assert.AreEqual((3f + 9f + 15f - 1001f) / 4d, myTable.AverageFloat(2));
                    Assert.AreEqual((5d + 15d + 25d - 1002d) / 4d, myTable.AverageDouble(3));


                    Assert.AreEqual(3, myTableView.Size);
                    //count methods are not implemented in tightdb yet, Until they are implemented, and our c++ binding
                    //is updated to call them, our c++ binding will just return zero
                    Assert.AreEqual(1, myTableView.CountLong(1, 3));
                    Assert.AreEqual(1, myTableView.CountFloat(2, 15f));
                    Assert.AreEqual(1, myTableView.CountDouble(3, 15d));
                    Assert.AreEqual(0/*3*/, myTableView.CountString(0, "tv"));

                    Assert.AreEqual(5, myTableView.MaximumLong("int"));
                    Assert.AreEqual(15f, myTableView.MaximumFloat("float"));
                    Assert.AreEqual(25d, myTableView.MaximumDouble(3));

                    Assert.AreEqual(1, myTableView.MinimumLong(1));
                    Assert.AreEqual(3f, myTableView.MinimumFloat(2));
                    Assert.AreEqual(5d, myTableView.MinimumDouble(3));

                    Assert.AreEqual(1 + 3 + 5, myTableView.SumLong(1));
                    Assert.AreEqual(3f + 9f + 15f, sftv);
                    Assert.AreEqual(5d + 15d + 25d, myTableView.SumDouble(3));

                    //average methods are not implemented in tightdb yet, Until they are implemented, and our c++ binding
                    //is updated to call them, our c++ binding will just return zero
                    Assert.AreEqual((3f + 9f + 15f)/3f, myTableView.AverageFloat(2));
                    Assert.AreEqual((5d + 15d + 25d )/3d, myTableView.AverageDouble(3));
                    Assert.AreEqual((1l + 3l + 5l) / 3f, myTableView.AverageLong(1));

                }

            }

        }

        const string field01text = "Data for first field";
        const string field02text = "Data for second field";
        const string field03text = "Data for third field";
        [Test]
        public static void TableAddTest1()
        {
            using (var table = new Table(new StringField("StringColumn1"),
                new StringField("StringColumn2"),
                new StringField("StringColumn3")))
            {
                table.Add(field01text);
                table.Add(field02text);
                table.Add(field03text);
            }

        }

        //report data on screen reg. environment
        [Test]
        public static void ShowVersionTest()
        {
            var pointerSize = IntPtr.Size;
            var vmBitness = (pointerSize == 8) ? "64bit" : "32bit";
            var dllsuffix = (pointerSize == 8) ? "64" : "32";
            OperatingSystem os = Environment.OSVersion;
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            PortableExecutableKinds peKind;
            ImageFileMachine machine;
            executingAssembly.ManifestModule.GetPEKind(out peKind, out machine);
            // String thisapplocation = executingAssembly.Location;
            
            Console.WriteLine("Pointer Size :              {0}", pointerSize);
            Console.WriteLine("Process Running as :        {0}", vmBitness);
            Console.WriteLine("Built as PeKind :           {0}", peKind);
            Console.WriteLine("Built as ImageFileMachine : {0}", machine);
            Console.WriteLine("OS Version :                {0}", os.Version);
            Console.WriteLine("OS Platform:                {0}", os.Platform);
            Console.WriteLine("");
            Console.WriteLine("Now Loading tight_c_cs{0}.dll - expecting it to be a {1} dll", dllsuffix, vmBitness);
            //Console.WriteLine("Loading "+thisapplocation+"...");

            //if something is wrong with the DLL (like, if it is gone), we will not even finish the creation of the table below.
            using (var t = new Table())
            {
                Console.WriteLine("C#  DLL        build number {0}", Table.GetDllVersionCSharp);
                Console.WriteLine("C++ DLL        build number {0}", Table.CPlusPlusLibraryVersion());
                if (t.Size != 0)
                {
                    throw new TableException("Weird");
                }
            }
            Console.WriteLine();
            Console.WriteLine();
        }


        //check that values  in the interface are marshalled correctly on this build and platform combination
        [Test]
        public static void TestInterop()
        {
            Table.TestInterop();
        }


        //tightdb Date is date_t which is seconds since 1970,1,1
        //it is an integer with the number of seconds since 1970,1,1 00:00
        //negative means before 1970,1,1
        //the date is always UTC - not local time


        //C# DateTime is a lot different. Basically an integer tick count since 0001,1,1 00:00 where a tick is 100 microseconds
        //As long as the tightdb Date is 64 bits, tightdb Date has enough range to always keep a C# datetime
        //however the C# datetime has much higher precision, and this precision is lost when stored to tightdb
        //also, a C# DateTime can be of 3 kinds :
        // DateTimeKindUnspecified : Probably local time when it was created, but you really don't know - developer haven't been specific about it
        // DateTimeKindLocal :The time represents a point in time, measured with the currently running machine's culture information and timezone. Daylight rules etc.
        // DateTimeKindUTC : The time represents a point in time, measured in UTC

        //When storing a DateTime, the binding do this :
        //* The DateTime is converted to UTC if it is not already UTC
        //* The time part of the DateTime is truncated to seconds
        //* A tightdb time variable is created from the now compatible DateTime

        //when fetching back a DateTime from tightdb, the binding do this :
        //* The tightdb time variable is converted to a DateTime of kind UTC

        //The convention is that tightdb alwas and only store utc datetime values
        //If You want to store a DateTime unaltered, use DateTime.ToBinary and DateTime.FromBinary and store these values in a int field.(which is always 64 bit)


        [Test]
        public static void TestSaveAndRetrieveDate()
        {
            //this test might not be that effective if being run on a computer whose local time is == utc
            var dateToSaveLocal = new DateTime(1979, 05, 14, 1, 2, 3, DateTimeKind.Local);
            var dateToSaveUtc = new DateTime(1979, 05, 14, 1, 2, 4, DateTimeKind.Utc);
            var dateToSaveUnspecified = new DateTime(1979, 05, 14, 1, 2, 5, DateTimeKind.Unspecified);

            var expectedLocal = new DateTime(1979, 05, 14, 1, 2, 3, DateTimeKind.Local).ToUniversalTime();//we expect to get the UTC timepoit resembling the local time we sent
            var expectedUtc = new DateTime(1979, 05, 14, 1, 2, 4, DateTimeKind.Utc);//we expect to get the exact same timepoint back, measured in utc
            var expectedUnspecified = new DateTime(1979, 05, 14, 1, 2, 5, DateTimeKind.Local).ToUniversalTime();//we expect to get the UTC timepoit resembling the local time we sent

            using (var t = new Table("date1".Date(), "date2".Mixed(), "stringfield".String()))//test date in an ordinary date , as well as date in a mixed
            {

                t.AddEmptyRow(1);//in this row we store datetosavelocal
                t.SetIndex(2);
                t.SetString(2, 0, "str1");
                t.SetDateTime(0, 0, dateToSaveLocal);
                DateTime fromdb = t.GetDateTime("date1", 0);
                DateTime fromdb2 = t[0].GetDateTime("date1");
                Assert.AreEqual(fromdb, fromdb2);
                Assert.AreEqual(fromdb, expectedLocal);

                t.SetMixedDateTime(1, 0, dateToSaveLocal.AddYears(1));//one year is added to get a time after 1970.1.1 otherwise we would get an exception with the mixed
                fromdb = t.GetMixedDateTime(1, 0);
                fromdb2 = t[0].GetMixedDateTime(1);
                Assert.AreEqual(fromdb, fromdb2);
                Assert.AreEqual(fromdb, expectedLocal.AddYears(1));


                t.AddEmptyRow(1);//in this row we save datetosaveutc
                t.SetString(2, 1, "str2");
                t.SetDateTime("date1", 1, dateToSaveUtc);
                fromdb = t.GetDateTime("date1", 1);
                fromdb2 = t[1].GetDateTime("date1");
                Assert.AreEqual(fromdb, fromdb2);
                Assert.AreEqual(fromdb, expectedUtc);

                t.SetMixedDateTime("date2", 1, dateToSaveUtc);
                fromdb = t.GetMixedDateTime(1, 1);
                fromdb2 = t[1].GetMixedDateTime(1);
                Assert.AreEqual(fromdb, fromdb2);
                Assert.AreEqual(fromdb, expectedUtc);

                t.AddEmptyRow(1);//in this row we save datetosaveunspecified
                t.SetString(2, 2, "str3");
                t.SetDateTime(0, 2, dateToSaveUnspecified);
                fromdb = t.GetDateTime("date1", 2);
                fromdb2 = t[2].GetDateTime("date1");
                Assert.AreEqual(fromdb, fromdb2);
                Assert.AreEqual(fromdb, expectedUnspecified);

                t.SetMixedDateTime(1, 2, dateToSaveUnspecified);
                fromdb = t.GetMixedDateTime("date2", 2);
                fromdb2 = t[2].GetMixedDateTime("date2");
                Assert.AreEqual(fromdb, fromdb2);
                Assert.AreEqual(fromdb, expectedUnspecified);

                t.SetIndex(2);
                TableView tv = t.Distinct("stringfield");//we need a tableview to be able to test the date methods on table views


                tv.SetDateTime(0, 0, dateToSaveUtc);
                fromdb = tv.GetDateTime("date1", 0);
                fromdb2 = tv[0].GetDateTime("date1");
                Assert.AreEqual(fromdb, fromdb2);
                Assert.AreEqual(fromdb, expectedUtc);

                tv.SetMixedDateTime(1, 0, dateToSaveUtc);
                fromdb = tv.GetMixedDateTime(1, 0);
                fromdb2 = tv[0].GetMixedDateTime(1);
                Assert.AreEqual(fromdb, fromdb2);
                Assert.AreEqual(fromdb, expectedUtc);



                tv.SetDateTime("date1", 1, dateToSaveUnspecified);
                fromdb = tv.GetDateTime("date1", 1);
                fromdb2 = tv[1].GetDateTime("date1");
                Assert.AreEqual(fromdb, fromdb2);
                Assert.AreEqual(fromdb, expectedUnspecified);

                tv.SetMixedDateTime("date2", 1, dateToSaveUnspecified);
                fromdb = tv.GetMixedDateTime(1, 1);
                fromdb2 = tv[1].GetMixedDateTime(1);
                Assert.AreEqual(fromdb, fromdb2);
                Assert.AreEqual(fromdb, expectedUnspecified);


                tv.SetDateTime(0, 2, dateToSaveLocal);
                fromdb = tv.GetDateTime("date1", 2);
                fromdb2 = tv[2].GetDateTime("date1");
                Assert.AreEqual(fromdb, fromdb2);
                Assert.AreEqual(fromdb, expectedLocal);

                tv.SetMixedDateTime(1, 2, dateToSaveLocal);
                fromdb = tv.GetMixedDateTime("date2", 2);
                fromdb2 = tv[2].GetMixedDateTime("date2");
                Assert.AreEqual(fromdb, fromdb2);
                Assert.AreEqual(fromdb, expectedLocal);


                //at this time there should be three rows in the tableview as the three dates are not exactly the same


            }



        }


        [Test]
        public static void TableMixedCreateEmptySubTable2()
        {
            using (var t = new Table(new MixedField("mixd")))
            {
                using (var sub = new Table(new IntField("int")))
                {
                    t.AddEmptyRow(1);
                    t.SetMixedSubTable(0, 0, sub);
                }
                t.AddEmptyRow(1);
                t.SetMixedEmptySubTable(0, 0); //i want a new empty subtable in my newly created row
                DataType sttype = t.GetMixedType(0, 0);
                Assert.AreEqual(DataType.Table, sttype);
            }
        }


        [Test]
        public static void TableMixedCreateEmptySubTable()
        {
            using (var t = new Table(new MixedField("mixd")))
            {
                t.AddEmptyRow(1);
                t.SetMixedEmptySubTable(0, 0);
                DataType sttype = t.GetMixedType(0, 0);
                Assert.AreEqual(DataType.Table, sttype);
            }
        }

        [Test]
        public static void TableMixedCreateSubTable()
        {
            using (var t = new Table(new MixedField("mix'd")))
            {
                using (var subtable = new Table(new IntField("int1")))
                {
                    t.AddEmptyRow(1);
                    t.SetMixedSubTable(0, 0, subtable);
                }
                DataType sttype = t.GetMixedType(0, 0);
                Assert.AreEqual(DataType.Table, sttype);
            }
        }

        [Test]
        public static void TableMixedSetGetSubTable()
        {
            using (var t = new Table(new MixedField("mix'd")))
            {
                using (var subtable = new Table(new IntField("int1")))
                {
                    t.AddEmptyRow(1);
                    t.SetMixedSubTable(0, 0, subtable);
                }
                DataType sttype = t.GetMixedType(0, 0);
                Assert.AreEqual(DataType.Table, sttype);
                Table subback = t.GetMixedSubTable(0, 0);
                Assert.AreEqual(DataType.Int, subback.ColumnType(0));
                Assert.AreEqual("int1", subback.GetColumnName(0));
            }
        }


        [Test]
        public static void TableMixedSetGetSubTableWithData()
        {

            string actualres;

            using (var t = new Table(new MixedField("mix'd")))
            {
                using (var subtable = new Table(new IntField("int1")))
                {
                    t.AddEmptyRow(1);
                    subtable.AddEmptyRow(1);
                    subtable.SetLong(0, 0, 42);
                    t.SetMixedSubTable(0, 0, subtable);
                }
                t.AddEmptyRow(1);
                t.SetMixedLong(0, 1, 84);
                DataType sttype = t.GetMixedType(0, 0);
                Assert.AreEqual(DataType.Table, sttype);
                Table subback = t.GetMixedSubTable(0, 0);
                Assert.AreEqual(DataType.Int, subback.ColumnType(0));
                Assert.AreEqual("int1", subback.GetColumnName(0));
                long databack = subback.GetLong(0, 0);
                Assert.AreEqual(42, databack);
                Assert.AreEqual(DataType.Int, t.GetMixedType(0, 1));
                Assert.AreEqual(84, t.GetMixedLong(0, 1));
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name, "sub in mixed with int", t);

            }

                        
const string expectedres = @"------------------------------------------------------
Column count: 1
Table Name  : sub in mixed with int
------------------------------------------------------
 0      Mixed  mix'd               
------------------------------------------------------

Table Data Dump. Rows:2
------------------------------------------------------
{ //Start row 0
mix'd:   { //Start row 0
   int1:42   //column 0
   } //End row 0
//column 0//Mixed type is Table
} //End row 0
{ //Start row 1
mix'd:84//column 0//Mixed type is Int
} //End row 1
------------------------------------------------------
";



            TestHelper.Cmp(expectedres, actualres);
        }



        [Test]
        public static void TableSubTableSubTabletwo()
        {
            //string actualres1;
            //string actualres2;
            //string actualres3;
            //string actualres4;
            //string actualres5;
            string actualres;

            using (var t = new Table(
                "fld1".String(),
                "root".SubTable(
                    "fld2".String(),
                    "fld3".String(),
                    "s1".SubTable(
                        "fld4".String(),
                        "fld5".String(),
                        "fld6".String(),
                        "s2".Table(
                            "fld".Int())))))
            {

                //   t.AddEmptyRow(1);
                t.AddEmptyRow(1); //add empty row

                Assert.AreEqual(1, t.Size);
                Table root = t.GetSubTable(1, 0);
                root.AddEmptyRow(1);
                Assert.AreEqual(1, root.Size);

                Table s1 = root.GetSubTable(2, 0);
                s1.AddEmptyRow(1);
                Assert.AreEqual(1, s1.Size);

                Table s2 = s1.GetSubTable(3, 0);
                s2.AddEmptyRow(1);

                const long valueinserted = 42;
                s2.SetLong(0, 0, valueinserted);
                Assert.AreEqual(1, s2.Size);

                //now read back the 42

                long valueback = t.GetSubTable(1, 0).GetSubTable(2, 0).GetSubTable(3, 0).GetLong(0, 0);
                //                            root               s1                 s2            42


                Assert.AreEqual(valueback, valueinserted);
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name + 5, "subtable in subtable with int",
                                                t);
            }
            const string expectedres = @"------------------------------------------------------
Column count: 2
Table Name  : subtable in subtable with int
------------------------------------------------------
 0     String  fld1                
 1      Table  root                
    0     String  fld2                
    1     String  fld3                
    2      Table  s1                  
       0     String  fld4                
       1     String  fld5                
       2     String  fld6                
       3      Table  s2                  
          0        Int  fld                 
------------------------------------------------------

Table Data Dump. Rows:1
------------------------------------------------------
{ //Start row 0
fld1:,//column 0
root:[ //1 rows   { //Start row 0
   fld2:   ,//column 0
   fld3:   ,//column 1
   s1:[ //1 rows      { //Start row 0
      fld4:      ,//column 0
      fld5:      ,//column 1
      fld6:      ,//column 2
      s2:[ //1 rows         { //Start row 0
         fld:42         //column 0
         } //End row 0
]      //column 3
      } //End row 0
]   //column 2
   } //End row 0
]//column 1
} //End row 0
------------------------------------------------------
";
            TestHelper.Cmp(expectedres, actualres);
        }



        [Test]
        public static void TableSubTableSubTableClone()
        {
            //string actualres1;
            //string actualres2;
            //string actualres3;
            //string actualres4;
            //string actualres5;
            string actualres;

            using (var t = new Table(
                "fld1".String(),
                "root".SubTable(
                    "fld2".String(),
                    "fld3".String(),
                    "s1".SubTable(
                        "fld4".String(),
                        "fld5".String(),
                        "fld6".String(),
                        "s2".Table(
                            "fld".Int())))))
            {

                //   t.AddEmptyRow(1);
                t.AddEmptyRow(1); //add empty row

                Assert.AreEqual(1, t.Size);
                Table root = t.GetSubTable(1, 0);
                root.AddEmptyRow(1);
                Assert.AreEqual(1, root.Size);

                Table s1 = root.GetSubTable(2, 0);
                s1.AddEmptyRow(1);
                Assert.AreEqual(1, s1.Size);

                Table s2 = s1.GetSubTable(3, 0);
                s2.AddEmptyRow(1);

                const long valueinserted = 42;
                s2.SetLong(0, 0, valueinserted);
                Assert.AreEqual(1, s2.Size);

                //now read back the 42

                long valueback = t.GetSubTable(1, 0).GetSubTable(2, 0).GetSubTable(3, 0).GetLong(0, 0);
                //                            root               s1                 s2            42


                Assert.AreEqual(valueback, valueinserted);
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name + 5, "subtable in subtable with int",
                                                t.Clone());
            }
            const string expectedres = @"------------------------------------------------------
Column count: 2
Table Name  : subtable in subtable with int
------------------------------------------------------
 0     String  fld1                
 1      Table  root                
    0     String  fld2                
    1     String  fld3                
    2      Table  s1                  
       0     String  fld4                
       1     String  fld5                
       2     String  fld6                
       3      Table  s2                  
          0        Int  fld                 
------------------------------------------------------

Table Data Dump. Rows:1
------------------------------------------------------
{ //Start row 0
fld1:,//column 0
root:[ //1 rows   { //Start row 0
   fld2:   ,//column 0
   fld3:   ,//column 1
   s1:[ //1 rows      { //Start row 0
      fld4:      ,//column 0
      fld5:      ,//column 1
      fld6:      ,//column 2
      s2:[ //1 rows         { //Start row 0
         fld:42         //column 0
         } //End row 0
]      //column 3
      } //End row 0
]   //column 2
   } //End row 0
]//column 1
} //End row 0
------------------------------------------------------
";
            TestHelper.Cmp(expectedres, actualres);
        }





        [Test]
        public static void TableIntValueTest2()
        {
            String actualres;
            
            using (var t = GetTableWithIntegers(false))
            {
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name, "table with a few integers in it", t);
            }
            

            const string expectedres = @"------------------------------------------------------
Column count: 3
Table Name  : table with a few integers in it
------------------------------------------------------
 0        Int  IntColumn1          
 1        Int  IntColumn2          
 2        Int  IntColumn3          
------------------------------------------------------

Table Data Dump. Rows:3
------------------------------------------------------
{ //Start row 0
IntColumn1:1764,//column 0
IntColumn2:0,//column 1
IntColumn3:0//column 2
} //End row 0
{ //Start row 1
IntColumn1:0,//column 0
IntColumn2:-9223372036854775808,//column 1
IntColumn3:0//column 2
} //End row 1
{ //Start row 2
IntColumn1:0,//column 0
IntColumn2:0,//column 1
IntColumn3:-9223372036854775766//column 2
} //End row 2
------------------------------------------------------
";
            TestHelper.Cmp(expectedres, actualres);
        }



        [Test]
        public static void TableIntValueSubTableTest1()
        {
            String actualres;

            using (var t = GetTableWithIntegers(true))
                actualres =TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                                                "table with a few integers in it", t);

            const string expectedres =
                @"------------------------------------------------------
Column count: 4
Table Name  : table with a few integers in it
------------------------------------------------------
 0        Int  IntColumn1          
 1        Int  IntColumn2          
 2        Int  IntColumn3          
 3      Table  SubTableWithInts    
    0        Int  SubIntColumn1       
    1        Int  SubIntColumn2       
    2        Int  SubIntColumn3       
------------------------------------------------------

Table Data Dump. Rows:3
------------------------------------------------------
{ //Start row 0
IntColumn1:1764,//column 0
IntColumn2:0,//column 1
IntColumn3:0,//column 2
SubTableWithInts:[ //3 rows   { //Start row 0
   SubIntColumn1:2   ,//column 0
   SubIntColumn2:0   ,//column 1
   SubIntColumn3:0   //column 2
   } //End row 0
   { //Start row 1
   SubIntColumn1:0   ,//column 0
   SubIntColumn2:0   ,//column 1
   SubIntColumn3:0   //column 2
   } //End row 1
   { //Start row 2
   SubIntColumn1:0   ,//column 0
   SubIntColumn2:0   ,//column 1
   SubIntColumn3:0   //column 2
   } //End row 2
]//column 3
} //End row 0
{ //Start row 1
IntColumn1:0,//column 0
IntColumn2:-9223372036854775808,//column 1
IntColumn3:0,//column 2
SubTableWithInts:[ //3 rows   { //Start row 0
   SubIntColumn1:0   ,//column 0
   SubIntColumn2:0   ,//column 1
   SubIntColumn3:0   //column 2
   } //End row 0
   { //Start row 1
   SubIntColumn1:0   ,//column 0
   SubIntColumn2:2   ,//column 1
   SubIntColumn3:0   //column 2
   } //End row 1
   { //Start row 2
   SubIntColumn1:0   ,//column 0
   SubIntColumn2:0   ,//column 1
   SubIntColumn3:0   //column 2
   } //End row 2
]//column 3
} //End row 1
{ //Start row 2
IntColumn1:0,//column 0
IntColumn2:0,//column 1
IntColumn3:-9223372036854775766,//column 2
SubTableWithInts:[ //3 rows   { //Start row 0
   SubIntColumn1:0   ,//column 0
   SubIntColumn2:0   ,//column 1
   SubIntColumn3:0   //column 2
   } //End row 0
   { //Start row 1
   SubIntColumn1:0   ,//column 0
   SubIntColumn2:0   ,//column 1
   SubIntColumn3:0   //column 2
   } //End row 1
   { //Start row 2
   SubIntColumn1:0   ,//column 0
   SubIntColumn2:0   ,//column 1
   SubIntColumn3:2   //column 2
   } //End row 2
]//column 3
} //End row 2
------------------------------------------------------
";
            TestHelper.Cmp(expectedres, actualres);
        }


        [Test]
        public static void TableRowColumnInsert()
        {
            String actualres;
            using (
                var t = new Table(new IntField("intfield"), new StringField("stringfield"), new IntField("intfield2")))
            {
                t.AddEmptyRow(5);
                long changeNumber = 0;
                foreach (TableRow tr in t)
                {
                    foreach (RowColumn trc in tr)
                    {
                        if (trc.ColumnType == DataType.Int)
                            trc.Value = ++changeNumber;

                    }
                }
                actualres = TestHelper.TableDumper(MethodBase.GetCurrentMethod().Name,
                                                "integers set from within trc objects", t);
            }


            const string expectedres =
                @"------------------------------------------------------
Column count: 3
Table Name  : integers set from within trc objects
------------------------------------------------------
 0        Int  intfield            
 1     String  stringfield         
 2        Int  intfield2           
------------------------------------------------------

Table Data Dump. Rows:5
------------------------------------------------------
{ //Start row 0
intfield:1,//column 0
stringfield:,//column 1
intfield2:2//column 2
} //End row 0
{ //Start row 1
intfield:3,//column 0
stringfield:,//column 1
intfield2:4//column 2
} //End row 1
{ //Start row 2
intfield:5,//column 0
stringfield:,//column 1
intfield2:6//column 2
} //End row 2
{ //Start row 3
intfield:7,//column 0
stringfield:,//column 1
intfield2:8//column 2
} //End row 3
{ //Start row 4
intfield:9,//column 0
stringfield:,//column 1
intfield2:10//column 2
} //End row 4
------------------------------------------------------
";
            TestHelper.Cmp(expectedres, actualres);
        }
        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableGetMixedWithNonMixedField()
        {

            using (var t = new Table(new IntField("int1"), new MixedField("mixed1")))
            {
                t.AddEmptyRow(1);
                t.SetLong(0, 0, 42);
                t.SetMixedLong(1, 0, 43);
                long intfromnonmixedfield = t.GetMixedLong(0, 0);
                Assert.AreEqual(42, intfromnonmixedfield);//we should never get this far
                Assert.AreNotEqual(42, intfromnonmixedfield);//we should never get this far
            }
        }

        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableEmptyTableFieldAccess()
        {
            using (var t = new Table(new IntField("Int1"), new IntField("Int2"), new IntField("Int3")))
            {
                //accessing a row on an empty table should not be allowed
                long value = t.GetLong(0, 0);
                Console.WriteLine(value);
            }
        }

        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableEmptyTableFieldAccessWrite()
        {
            using (var t = new Table(new IntField("Int1"), new IntField("Int2"), new IntField("Int3")))
            {
                //accessing a row on an empty table should not be allowed
                t.SetLong(0, 0, 42); //this should throw
                long value = t.GetLong(0, 0);
                Console.WriteLine(value);
            }
        }



        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableRowIndexTooLow()
        {
            using (var t = new Table(new IntField("Int1"), new IntField("Int2"), new IntField("Int3")))
            {
                //accessing a row on an empty table should not be allowed
                t.AddEmptyRow(1);
                long value = t.GetLong(0, -1);
                Console.WriteLine(value);
            }
        }

        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableRowIndexTooLowWrite()
        {
            using (var t = new Table(new IntField("Int1"), new IntField("Int2"), new IntField("Int3")))
            {
                //accessing a row on an empty table should not be allowed
                t.AddEmptyRow(1);
                t.SetLong(0, -1, 42); //should throw
                long value = t.GetLong(0, -1);
                Console.WriteLine(value);
            }
        }


        //
        [Test]
        public static void TableGetUndefinedMixedType()
        {
            using (var t = new Table(new MixedField("MixedField")))
            {
                t.AddEmptyRow(1);
                DataType dt = t.GetMixedType(0, 0);
                Assert.AreEqual(dt, DataType.Int);
            }
        }


        [Test]
        public static void TableMixedInt()
        {
            using (var t = new Table(new MixedField("MixedField")))
            {
                t.AddEmptyRow(1);
                t.SetMixedLong(0, 0, 42);
                DataType dt = t.GetMixedType(0, 0);
                Assert.AreEqual(dt, DataType.Int);
                long fortytwo = t.GetMixedLong(0, 0);
                Assert.AreEqual(42, fortytwo);
            }
        }


        [Test]
        public static void TableMixedString()
        {
            using (var t = new Table(new MixedField("StringField")))
            {
                const string setWithAdd = "SetWithAdd";
                const string setWithSetMixed = "SetWithSetMixed";
                t.Add(setWithAdd);
                DataType dtRow0 = t.GetMixedType(0, 0);
                Assert.AreEqual(DataType.String,dtRow0);//mixed from empty rows added are int as a default
                String row0 = t.GetMixedString(0, 0);
                Assert.AreEqual(setWithAdd, row0);

                t.AddEmptyRow(1);
                t.SetMixedString(0, 1, setWithSetMixed);               
                DataType dtRow1 = t.GetMixedType(0, 1);
                Assert.AreEqual(DataType.String,dtRow1);
                String row1 = t.GetMixedString(0, 1);
                Assert.AreEqual(setWithSetMixed, row1);

            }
        }//todo:make one like this for tableview



        [Test]
        public static void TableMixedDateTime()
        {
            var testDate = new DateTime(2000, 1, 2, 3, 4, 5, DateTimeKind.Utc);
            using (var t = new Table(new MixedField("MixedField")))
            {
                t.AddEmptyRow(1);
                t.SetMixedDateTime(0, 0, testDate);
                DataType dt = t.GetMixedType(0, 0);
                Assert.AreEqual(DataType.Date, dt);
                DateTime fromDb = t.GetMixedDateTime(0, 0);
                Assert.AreEqual(testDate, fromDb);
            }
        }



        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableRowIndexTooHigh()
        {
            using (var t = new Table(new IntField("Int1"), new IntField("Int2"), new IntField("Int3")))
            {
                //accessing a row on an empty table should not be allowed
                t.AddEmptyRow(1);
                long value = t.GetLong(0, 1);
                Console.WriteLine(value);
            }
        }

        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableRowIndexTooHighWrite()
        {
            using (var t = new Table(new IntField("Int1"), new IntField("Int2"), new IntField("Int3")))
            {
                //accessing a row on an empty table should not be allowed
                t.AddEmptyRow(1);
                t.SetLong(0, 1, 42); //should throw
                long value = t.GetLong(0, 1);
                Console.WriteLine(value);
            }
        }



        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableColumnIndexTooLow()
        {
            using (var t = new Table(new IntField("Int1"), new IntField("Int2"), new IntField("Int3")))
            //accessing an illegal column should also not be allowed
            {
                t.AddEmptyRow(1);
                long value2 = t.GetLong(-1, 0);
                Console.WriteLine(value2);
            }
        }

        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableColumnIndexTooLowWrite()
        {
            using (var t = new Table(new IntField("Int1"), new IntField("Int2"), new IntField("Int3")))
            //accessing an illegal column should also not be allowed
            {
                t.AddEmptyRow(1);
                t.SetLong(-1, 0, 42);
                long value2 = t.GetLong(-1, 0);
                Console.WriteLine(value2);
            }
        }


        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableColumnIndexTooHigh()
        {
            using (var t = new Table(new IntField("Int1"), new IntField("Int2"), new IntField("Int3")))
            //accessing an illegal column should also not be allowed
            {
                t.AddEmptyRow(1);
                long value2 = t.GetLong(3, 0);
                Console.WriteLine(value2);
            }
        }


        [Test]
        [ExpectedException("System.ArgumentOutOfRangeException")]
        public static void TableColumnIndexTooHighWrite()
        {
            using (var t = new Table(new IntField("Int1"), new IntField("Int2"), new IntField("Int3")))
            //accessing an illegal column should also not be allowed
            {
                t.AddEmptyRow(1);
                t.SetLong(3, 0, 42);
                long value2 = t.GetLong(3, 0);
                Console.WriteLine(value2);
            }
        }



        [Test]
        [ExpectedException("TightDbCSharp.TableException")]
        public static void TableIllegalType()
        {
            using (var t = new Table(new IntField("Int1"), new IntField("Int2"), new IntField("Int3")))
            //accessing an illegal column should also not be allowed
            {
                t.AddEmptyRow(1);
                //likewise accessing the wrong type should not be allowed
                Table t2 = t.GetSubTable(1, 0);
                Console.WriteLine(t2.Size); //this line should not hit - the above should throw an exception
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming",
            "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "TypeWrite"), Test]
        [ExpectedException("TightDbCSharp.TableException")]
        public static void TableIllegalTypeWrite()
        {
            using (var t = new Table(new SubTableField("sub1"), new IntField("Int2"), new IntField("Int3")))
            //accessing an illegal column should also not be allowed
            {
                t.AddEmptyRow(1);
                //likewise accessing the wrong type should not be allowed               
                t.SetLong(0, 0, 42); //should throw                
                Console.WriteLine(t.Size); //this line should not hit - the above should throw an exception
            }
        }

    }
}
