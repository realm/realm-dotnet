using NUnit.Framework;
using TightDbCSharp;
using TightDbCSharp.Extensions;

namespace TightDbCSharpTest
{
    [TestFixture]
    public class SpecTests
    {
        //this test failed bc there is no way to check if calling updatefromspec is allowed
        //specifically we cannot call c++ and get a column count that excludes the changes made to the spec

        //a Workaround in Table has been made, that sets the internal property HasColumns to true when
        //addcolumn or updatefromspec has been called. Updatefromspec will then fail if HasColumns is true
        //more tests are being added that test for other spec operations that cannot be done on tables
        //where columns already have been properly set and "comitted" with updatefromspec or addcolumn
        [Test]
        [ExpectedException("System.InvalidOperationException")] //updatefromspec on a table with existing columns
        public static void TableAddColumnAndSpecTestsimple()
        {
            var t = new Table();
            Assert.AreEqual(true, t.SpecModifyable());
            Assert.AreEqual(0,t.AddColumn(DataType.Int, "IntColumn1")); //after this call, spec modifications are illegal
            Assert.AreEqual(false, t.SpecModifyable());
            Spec subSpec = t.Spec.AddSubTableColumn("SubTableWithInts"); //this should throw
            Assert.AreEqual(subSpec.Handle, subSpec.Handle + 1);
                //avoid compiler warning and ensure we fail here if we get so far - we want the error to surface on the line above
            t.UpdateFromSpec();
            t.AddEmptyRow(1);
            Table sub = t.GetSubTable(1, 0);
            Assert.AreEqual(sub.Size, 0); //avoid compiler warning
        }


        //Illustrate a problem if the user creates two wrappers
        //that both wrap the same table from a group, and then changes
        //spec i one of them, and then asks the other if spec change is legal
        //problem is the wrapper state HasRows - it is not updated in all wrappers
        //that wrap the same table.
        //HOWEVER - it is pretty weird to get two table wrappers from the same group
        //at once in the first place. That in itself should probably be illegal, even though
        //legal spec operations will work fine if they come in from the wrappers interleaved

        [Test]
        public static void TableTwoWrappersChangeSpec()
        {
            var g = new Group();
            Table t1 = g.CreateTable("T");
            Table t2 = g.GetTable("T");
            Assert.AreEqual(t1.Handle,t2.Handle);
            Assert.AreEqual(true,t1.SpecModifyable());
            Assert.AreEqual(true, t2.SpecModifyable());
            t2.Spec.AddIntColumn("inttie");
            Assert.AreEqual(true, t1.SpecModifyable());
            Assert.AreEqual(true, t2.SpecModifyable());
            t1.UpdateFromSpec();
            Assert.AreEqual(false, t1.SpecModifyable());
            Assert.AreEqual(false, t2.SpecModifyable());//this currently fails bc HasRows was not set in this wrapper
            //at this point, database and all structures are in a sane state
        }


        //suppose we read in a table with columns already added
        //then we have to set HasColumns manually after the load
        //this test covers if the table is read in from a group
        [Test]
        public static void TableReadFromGroupSpecModifyable()
        {
            var g = new Group();
            Table t1 = g.CreateTable("T");
            Assert.AreEqual(true, t1.SpecModifyable());
            t1.AddColumn(DataType.String,  "column");
            Assert.AreEqual(false, t1.SpecModifyable());
            Table t2 = g.GetTable("T");
            Assert.AreEqual(false, t2.SpecModifyable());
        }



        [Test]
        [ExpectedException("System.InvalidOperationException")]
        //because spec.addcolumn is illegal if the table already have "comitted" columns
        //Ensure that we get an exception if we start a column modification on a table
        //with existings columns in it

        public static void SpecAddColumn()
        {
            using (var t = new Table("field".Int()))
            {
                Spec s = t.Spec;
                s.AddColumn(DataType.Int, "intfield");
                //t.UpdateFromSpec();
            }
        }

        //test that the various spec column adders actually add the correct column type
        [Test]
        public static void SpecAddColumntypes()
        {
            using (var table = new Table())
            {
                Spec spec = table.Spec;
                Assert.AreEqual(0, spec.AddBinaryColumn("binary"));
                Assert.AreEqual(1, spec.AddBoolColumn("bool"));
                Assert.AreEqual(2, spec.AddDateColumn("date"));
                Assert.AreEqual(3, spec.AddDoubleColumn("double"));
                Assert.AreEqual(4, spec.AddFloatColumn("float"));
                Assert.AreEqual(5, spec.AddIntColumn("int"));
                Assert.AreEqual(6, spec.AddMixedColumn("mixed"));
                Assert.AreEqual(7, spec.AddStringColumn("string"));
                Spec subSpec = spec.AddSubTableColumn("subtable");

                Assert.AreEqual(0, subSpec.AddBinaryColumn("binary"));
                Assert.AreEqual(1, subSpec.AddBoolColumn("bool"));
                Assert.AreEqual(2, subSpec.AddDateColumn("date"));
                Assert.AreEqual(3, subSpec.AddDoubleColumn("double"));
                Assert.AreEqual(4, subSpec.AddFloatColumn("float"));
                Assert.AreEqual(5, subSpec.AddIntColumn("int"));
                Assert.AreEqual(6, subSpec.AddMixedColumn("mixed"));
                Assert.AreEqual(7, subSpec.AddStringColumn("string"));
                Assert.AreEqual(8, subSpec.AddTableColumn("table"));
                table.UpdateFromSpec();
                Assert.AreEqual(false, table.HasSharedSpec());//shared spec is false for root tables
                Assert.AreEqual(DataType.Binary, table.ColumnType(0));
                Assert.AreEqual(DataType.Bool, table.ColumnType(1));
                Assert.AreEqual(DataType.Date, table.ColumnType(2));
                Assert.AreEqual(DataType.Double, table.ColumnType(3));
                Assert.AreEqual(DataType.Float, table.ColumnType(4));
                Assert.AreEqual(DataType.Int, table.ColumnType(5));
                Assert.AreEqual(DataType.Mixed, table.ColumnType(6));
                Assert.AreEqual(DataType.String, table.ColumnType(7));
                Assert.AreEqual(DataType.Table, table.ColumnType(8));
                table.AddEmptyRow(5);
                Table subTable = table.GetSubTable(8, 3);
                Assert.AreEqual(true, subTable.HasSharedSpec());
                Assert.AreEqual(DataType.Binary, subTable.ColumnType(0));
                Assert.AreEqual(DataType.Bool, subTable.ColumnType(1));
                Assert.AreEqual(DataType.Date, subTable.ColumnType(2));
                Assert.AreEqual(DataType.Double, subTable.ColumnType(3));
                Assert.AreEqual(DataType.Float, subTable.ColumnType(4));
                Assert.AreEqual(DataType.Int, subTable.ColumnType(5));
                Assert.AreEqual(DataType.Mixed, subTable.ColumnType(6));
                Assert.AreEqual(DataType.String, subTable.ColumnType(7));
                Assert.AreEqual(DataType.Table, subTable.ColumnType(8));
            }
        }



    }
}
