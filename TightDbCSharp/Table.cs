using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;


//using System.Threading.Tasks; not portable as of 2013-04-02

//Tell compiler to give warnings if we publicise interfaces that are not defined in the cls standard
//http://msdn.microsoft.com/en-us/library/bhc3fa7f.aspx
[assembly: CLSCompliant(true)]

//Table class. The class represents a tightdb table.
//implements idisposable - will clean itself up (and any c++ resources it uses) when garbage collected
//If You plan to save resources, You can use it with the using syntax.



namespace TightDbCSharp
{

    //this file contains Table and all its helper classes, except Spec, which has its own file
    //see after the table class for a collection of helper classes, TField, TableException, Extension methods, TableRecord etc.


    //represents one row in a table. Access to the individual columns are handled by the associated Table
    //Currently, only access by column number is supported
    //currently only reading is supported
    //this is the TableRecord type You get back from foreach if you foreach(TableRow tr in mytable) {tr.operation()}

    //If You need extra speed, And you know the column schema of the table at compile, you can create a typed record :
    //note that the number 2 is then expected to be the at compile time known column number of the field containing the CustomerId
    //this is equally fast as writing 
    //Long CustId= MyRecord.GetLong(2) 
    //but it is syntactically easier to read
    //long CustId = MyRecord.CustomerId;
    //Alternatively, you can create a set of constants and call
    //long CustId = Myrecord.GetLong(CUSTID);
    /*
    class CustomerTableRecord : TableRecord
    {
        public long DiscountTokens { get { return Owner.GetLong(Row, 2); } {set { Owner.SetLong(CurrentRow,2,value) }} }
    }
*/

    //Tightdb Table
    //could have been called RowCollection but is called Table as it in fact is a table and not merely a collection of rows
    public class Table : TableOrView, IEnumerable<Row>
    {
        //manual dll version info. Used when debugging to see if the right DLL is loaded, or an old one
        //the number is a date and a time (usually last time i debugged something)
        public  const long GetDllVersionCSharp = 1305301717 ;

        

        //following the dispose pattern discussed here http://dave-black.blogspot.dk/2011/03/how-do-you-properly-implement.html
        //a good explanation can be found here http://stackoverflow.com/questions/538060/proper-use-of-the-idisposable-interface


        //always acquire a table handle
        public Table()
        {
            TableNew();
        }

        //This is used when we want to create a table and we already have the c++ handle that the table should use.  used by GetSubTable
        internal Table(IntPtr tableHandle,bool shouldbedisposed)
        {
            SetHandle(tableHandle,shouldbedisposed);
        }



        public TableRow this[long rowIndex]
        {
            get
            {
                ValidateRowIndex(rowIndex);
                return new TableRow(this, rowIndex);
            }
        }



        //the following code enables Table to be enumerated, and makes Row the type You get back from an enummeration
        public new IEnumerator<TableRow> GetEnumerator() { return new Enumerator(this); }
        IEnumerator IEnumerable.GetEnumerator() { return new Enumerator(this); }

        class Enumerator : IEnumerator<TableRow>//probably overkill, current needs could be met by using yield
        {
            long _currentRow = -1;
            Table _myTable;
            public Enumerator(Table table)
            {
                _myTable = table;
            }
            public TableRow Current { get { return new TableRow(_myTable, _currentRow); } }
            object IEnumerator.Current { get { return Current; } }

            public bool MoveNext()
            {
                return ++_currentRow < _myTable.Size;
            }

            public void Reset() { _currentRow = -1; }
            public void Dispose()
            {
                _myTable = null; //remove reference to Table class
            }
        }






        //will only log in debug mode!
        //marker is a string that will show as the first log line, use this if several places in the code enable logging and disable it again, to
        //easily see what section we're in
        //will be replaced by normal c# logging when i get around to it
        public static void LoggingEnable()
        {
            LoggingEnable("");
        }
        public static void LoggingEnable(string marker)
        {
            UnsafeNativeMethods.LoggingEnable(marker);
        }

        public static void LoggingSaveFile(string fileName)
        {
            UnsafeNativeMethods.LoggingSaveFile(fileName);
        }

        public static void LoggingDisable()
        {
            UnsafeNativeMethods.LoggingDisable();
        }
        //this parameter type allows the user to send a comma seperated list of TableField objects without having
        //to put them into an array first
        public Table(params Field[] schema)
        {
            if (schema == null)
            {
                throw new ArgumentNullException("schema");
            }
            TableNew();
            DefineSchema(schema);
        }

        //allows the user to quickly create a table with a single field of a single type
        public Table(Field schema)
        {
            TableNew();//allocate a table class in c++
            //Spec spec = GetSpec();//get a handle to the table's new empty spec
            DefineSchema(schema);
        }

        public Table DefineSchema(params Field[] schema)
        {
            if (schema == null)
            {
                throw new ArgumentNullException("schema");
            }
            ValidateIsEmpty();
            foreach (Field tf in schema)
            {
                if (tf == null)
                {
                    throw new ArgumentNullException("schema", "one or more of the field objects is null");
                }
                Spec.AddField(tf);
            }
            UpdateFromSpecNoCheck();
            return this;//allow fluent creation of table in a group
        }

        //this method is intended to be called on a table with no data in it.
        //the method will create columns in the table, matching the specified schema.
        public void DefineSchema(Field schema)
        {
            ValidateIsEmpty();
            Spec.AddField(schema);
            UpdateFromSpecNoCheck();//build table from the spec tree structure            
        }
        /*
        //allows specifying a treelike structure much like the params constructor, but this
        //version will have to have an array explicitly specified as the outermost object
        public Table(TableField1[] schema)
        {
            table_new();//allocate a table class in c++
            Spec spec = get_spec();//get a handle to the table's new empty spec
            spec.addfields(schema);
            updatefromspec();//build table from the spec tree structure
        }
        */

        /*
        //random thoughts about various accessor methods
        //Quite fast - types assumed correct when running,but Asstring needs to create an object. not good
        customers.Asstring(12,3)  = "Hans";
        customers.Asstring(12,"Firstname")  ="Hans";
        //A bit less fast - types have to be looked up in C# to determine the correct call Asstring could be a property so no object needed
        customers[12,"Firstname"].Asstring = "Hans";
        //untyped, expects object, getter and setter figures what to do. 
        customers[12,3] = "Hans";
        costomers[12,"firstname"]  ="Hans";        
        */


        //This method will test basic interop, especially test that the c++ compiler used to build the c++ dll binding uses
        //the same size and sequence for various types used in interop, as C# and C# marshalling expects
        public static void TestInterop()
        {
            UnsafeNativeMethods.TestInterop();
        }

        public static void TestGroupStuff()
        {
            UnsafeNativeMethods.TestGroupStuff();
        }


        //test method that will probably be removed in the release. Used to showcase that it's c++ that crashes when group is given a valid file name string with unspecified location
        public static void TestAcquireAndDeleteGroup(string filename)
        {
            UnsafeNativeMethods.test_testacquireanddeletegroup(filename);
        }


        public static long CPlusPlusLibraryVersion()
        {
            return UnsafeNativeMethods.CppDllVersion();
        }
        public static long TestAddInteger(long value)
        {
            return UnsafeNativeMethods.TestIncrementInteger(value);
        }



        //not accessible by source not in the TightDBCSharp namespace
        //TableHandle contains the value of a C++ pointer to a C++ table
        //it is sent as a parameter to calls to the C++ DLL.

        internal void TableNew()
        {
           UnsafeNativeMethods.TableNew(this);//calls sethandle itself
        }

        public void RenameColumn(long columnIndex, String newName)
        {
            ValidateColumnIndex(columnIndex);
            UnsafeNativeMethods.TableRenameColumn(this,columnIndex,newName);
        }

        public void RemoveColumn(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            UnsafeNativeMethods.TableRemoveColumn(this,columnIndex);
        }
        //this one is called from Handled.cs when we have to release the table handle.
        internal override void ReleaseHandle()
        {
            UnsafeNativeMethods.TableUnbind(this);            
        }

        internal override Spec GetSpec()
        {
            return UnsafeNativeMethods.TableGetSpec(this); 
        }


        //this will update the table structure to represent whatever the earlier recieved spec has been set up to, and altered to
        //todo : (asana)should not be public, call updatefromspec automatically from methods that change the spec (so find out how to get to the top level table when all you have is a spec)
        //todo : (asana)updatefromspec is only allowed on the toplevel table, not on subtables within  - bot okay on toplevel tables inside mixed
        internal void UpdateFromSpecNoCheck()
        {        
            UnsafeNativeMethods.TableUpdateFromSpec(this);
        }

        public void UpdateFromSpec()
        {
            ValidateIsEmpty();
            UpdateFromSpecNoCheck();
        }

        internal override DataType ColumnTypeNoCheck(long columnIndex)
        {
            return UnsafeNativeMethods.TableGetColumnType(this, columnIndex);
        }

        public override string ObjectIdentification()
        {
            return string.Format(CultureInfo.InvariantCulture,"Table:" + Handle);
        }

        internal override DataType GetMixedTypeNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableGetMixedType(this,columnIndex, rowIndex);
        }


        internal override DateTime GetMixedDateTimeNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableGetMixedDateTime(this, columnIndex, rowIndex);
        }

        //can take invalid name parameter, will then return -1 as the column index
        internal override long GetColumnIndexNoCheck(String name)
        {
            return UnsafeNativeMethods.TableGetColumnIndex(this,name);            
        }

        internal override long SumLongNoCheck(long columnIndex)
        {
            return UnsafeNativeMethods.TableSumLong(this, columnIndex);
        }

        internal override double SumFloatNoCheck(long columnIndex)
        {
            return UnsafeNativeMethods.TableSumFloat(this, columnIndex);           
        }

        internal override double SumDoubleNoCheck(long columnIndex)
        {
            return UnsafeNativeMethods.TableSumDouble(this, columnIndex);            
        }

        internal override long MinimumLongNoCheck(long columnIndex)
        {
            return UnsafeNativeMethods.TableMinimum(this, columnIndex);
        }

        internal override float MinimumFloatNoCheck(long columnIndex)
        {
            return UnsafeNativeMethods.TableMinimumFloat(this, columnIndex);
        }

        internal override double MinimumDoubleNoCheck(long columnIndex)
        {
            return UnsafeNativeMethods.TableMinimumDouble(this, columnIndex);
        }

        internal override long MaximumLongNoCheck(long columnIndex)
        {
            return UnsafeNativeMethods.TableMaximumLong(this, columnIndex);
        }

        internal override float MaximumFloatNoCheck(long columnIndex)
        {
            return UnsafeNativeMethods.TableMaximumFloat(this, columnIndex);
        }

        internal override double MaximumDoubleNoCheck(long columnIndex)
        {
            return UnsafeNativeMethods.TableMaximumDouble(this, columnIndex);
        }

        internal override double AverageLongNoCheck(long columnIndex)
        {
            return UnsafeNativeMethods.TableAverage(this, columnIndex);
        }

        internal override double AverageFloatNoCheck(long columnIndex)
        {
            return UnsafeNativeMethods.TableAverageFloat(this, columnIndex);
        }

        internal override double AverageDoubleNoCheck(long columnIndex)
        {
            return UnsafeNativeMethods.TableAverageDouble(this, columnIndex);
        }

        internal override long CountLongNoCheck(long columnIndex, long target)
        {
            return UnsafeNativeMethods.TableCountLong(this, columnIndex, target);
        }

        internal override long CountFloatNoCheck(long columnIndex, float target)
        {
            return UnsafeNativeMethods.TableCountFloat(this, columnIndex, target);
        }

        internal override long CountStringNoCheck(long columnIndex, string target)
        {
            return UnsafeNativeMethods.TableCountString(this, columnIndex, target);
        }

        internal override long CountDoubleNoCheck(long columnIndex, double target)
        {
            return UnsafeNativeMethods.TableCountDouble(this, columnIndex, target);
        }


        public override string ToJson()
        {
            return UnsafeNativeMethods.TableToJson(this);
        }

        internal override void RemoveNoCheck(long rowIndex)
        {
            UnsafeNativeMethods.TableRemove(this,rowIndex);
        }

        internal override long GetColumnCount()
        {
            return UnsafeNativeMethods.TableGetColumnCount(this);
        }
        
        //this will add a column of the specified type, if it is a table type, You will have to populate it yourself later on,
        //by getting its subspec and working with that
        public long AddColumn(DataType type, String name)
        {
            return UnsafeNativeMethods.TableAddColumn(this, type, name);
        }

        //



        //adds an empty row at the end and filles it out
        //idea:consider if we should use insert row instead? - ask what's the difference, if any (asana)
        //todo:insert should only be used by the binding, not be exposed to the user (asana)
        //idea:when we add an entire row, insert is faster. (asana)
        public long Add(params object[] rowData)
        {
            long rowAdded = AddEmptyRow(1);
            SetRowNoCheck(rowAdded, rowData);//because the only thing that could be checked at this level is the row number.
            return rowAdded;//return the index of the just added row
        }

        public void Set(long rowIndex, params object[] rowData)
        {
            ValidateRowIndex(rowIndex);
            SetRowNoCheck(rowIndex,rowData);
        }
        //insert rowdata into a new row that is inserted at rowIndex
        //rowindex=0 means insert into very first record
        //rowindex=size means add after last        
        public void Insert(long rowIndex, params object[] rowData)
        {
            if (rowIndex > 0)
                ValidateRowIndex(rowIndex - 1);                            
            UnsafeNativeMethods.TableInsertEmptyRow(this,rowIndex,1);                         
            SetRowNoCheck(rowIndex,rowData);
        }

        internal override string GetColumnNameNoCheck(long columnIndex)//unfortunately an int, bc tight might have been built using 32 bits
        {
            return UnsafeNativeMethods.TableGetColumnName(this, columnIndex);
        }

        //add empty row at the end, return the index
        public long AddEmptyRow(long numberOfRows)
        {
            return UnsafeNativeMethods.TableAddEmptyRow(this, numberOfRows);
        }

        internal override byte[] GetBinaryNoCheck(long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        internal override Table GetSubTableNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableGetSubTable(this, columnIndex, rowIndex);
        }

        internal override void SetStringNoCheck(long columnIndex, long rowIndex,string value)
        {
            UnsafeNativeMethods.TableSetString(this,columnIndex,rowIndex,value);
        }

        internal override void SetBinaryNoCheck(long columnIndex, long rowIndex, byte[] value)
        {
            throw new NotImplementedException();
        }

        internal override String GetStringNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableGetString(this, columnIndex, rowIndex);
        }




        internal override void SetMixedFloatNoCheck(long columnIndex, long rowIndex, float value)
        {
            UnsafeNativeMethods.TableSetMixedFloat(this,columnIndex, rowIndex, value);
        }

        internal override void SetFloatNoCheck(long columnIndex, long rowIndex, float value)
        {
            UnsafeNativeMethods.TableSetFloat(this,columnIndex,rowIndex,value);
        }

        internal override void SetMixedDoubleNoCheck(long columnIndex, long rowIndex, double value)
        {
            UnsafeNativeMethods.TableSetMixedDouble(this,columnIndex,rowIndex,value);
        }

        internal override void SetDoubleNoCheck(long columnIndex, long rowIndex, double value)
        {
            UnsafeNativeMethods.TableSetDouble(this,columnIndex,rowIndex,value);
        }

        internal override void SetMixedDateTimeNoCheck(long columnIndex, long rowIndex, DateTime value)
        {
            UnsafeNativeMethods.TableSetMixedDate(this, columnIndex, rowIndex, value);            
        }

        internal override void SetDateTimeNoCheck(long columnIndex, long rowIndex, DateTime value)
        {
            UnsafeNativeMethods.TableSetDate(this,columnIndex,rowIndex,value);
        }

        internal override bool GetMixedBoolNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableGetMixedBool(this, columnIndex, rowIndex);
        }

        internal override String GetMixedStringNoCheck(long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        internal override byte[] GetMixedBinaryNoCheck(long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        internal override Table GetMixedSubTableNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableGetSubTable(this, columnIndex, rowIndex);            
        }

        //warning! Use only this one when inserting new rows that are not inserted yet
        public void InsertInt(long columnIndex, long rowIndex, long value)
        {
            UnsafeNativeMethods.TableInsertInt(this, columnIndex, rowIndex, value);
        }

        public void InsertEmptyRow(long rowIndex, long rowsToInsert)
        {            
            UnsafeNativeMethods.TableInsertEmptyRow(this, rowIndex, rowsToInsert);                            
        }

        //number of records in this table
        internal override long GetSize()
        {
            return UnsafeNativeMethods.TableSize(this);
        }

        //only call if You are certain that 1: The field type is Int, 2: The columnIndex is in range, 3: The rowIndex is in range
        internal override long GetLongNoCheck(long columnIndex,long rowIndex)
        {
            return UnsafeNativeMethods.TableGetInt(this,columnIndex, rowIndex);
        }

        internal override Double GetDoubleNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableGetDouble(this, columnIndex, rowIndex);
        }

        internal override float GetFloatNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableGetFloat(this, columnIndex, rowIndex);
        }

        internal override Boolean GetBoolNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableGetBool(this, columnIndex, rowIndex);
        }

        internal override void SetBoolNoCheck(long columnIndex, long rowIndex,Boolean value)
        {
           UnsafeNativeMethods.TableSetBool(this,columnIndex,rowIndex,value);
        }


        internal override void SetLongNoCheck(long columnIndex, long rowIndex, long value)
        {
            UnsafeNativeMethods.TableSetLong(this, columnIndex, rowIndex, value);
        }

        internal override void SetMixedLongNoCheck(long columnIndex, long rowIndex, long value)
        {
            UnsafeNativeMethods.TableSetMixedLong(this, columnIndex, rowIndex, value);
        }

        internal override void SetMixedBoolNoCheck(long columnIndex, long rowIndex, bool value)
        {
            UnsafeNativeMethods.TableSetMixedBool(this, columnIndex, rowIndex, value);
        }

        internal override void SetMixedStringNoCheck(long columnIndex, long rowIndex, string value)
        {
            UnsafeNativeMethods.TableSetMixedString(this, columnIndex, rowIndex, value);
        }

        internal override void SetMixedBinaryNoCheck(long columnIndex, long rowIndex, byte[] value)
        {
            UnsafeNativeMethods.TableSetMixedBinary(this, columnIndex, rowIndex, value);
        }


        //a copy of source will be set into the field
        internal override void SetMixedSubtableNoCheck(long columnIndex, long rowIndex, Table source)
        {
            UnsafeNativeMethods.TableSetMixedSubTable(this,columnIndex,rowIndex,source);
        }

        //might be used if You want an empty subtable set up and then change its contents and layout at a later time
        internal override void SetMixedEmptySubtableNoCheck(long columnIndex, long rowIndex)
        {
            UnsafeNativeMethods.TableSetMixedEmptySubTable(this,columnIndex,rowIndex);
        }


        internal override long GetMixedLongNoCheck(long columnIndex , long rowIndex )
        {
            return UnsafeNativeMethods.TableGetMixedInt(this, columnIndex, rowIndex);
        }

        internal override Double GetMixedDoubleNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableGetMixedDouble(this, columnIndex, rowIndex);
        }

        internal override float GetMixedFloatNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableGetMixedFloat(this, columnIndex, rowIndex);
        }


        internal override DateTime GetDateTimeNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableGetDateTime(this, columnIndex, rowIndex);
        }



        internal override long FindFirstBinaryNoCheck(long columnIndex, byte[] value)
        {
            return UnsafeNativeMethods.TableFindFirstBinary(this, columnIndex, value);
        }

        internal override long FindFirstIntNoCheck(long columnIndex, long value)
        {
            return UnsafeNativeMethods.TableFindFirstInt(this, columnIndex, value);
        }

        internal override long FindFirstStringNoCheck(long columnIndex, string value)
        {
            return UnsafeNativeMethods.TableFindFirstString(this, columnIndex, value);
        }

        internal override long FindFirstDoubleNoCheck(long columnIndex, double value)
        {
            return UnsafeNativeMethods.TableFindFirstDouble(this, columnIndex, value);
        }

        internal override long FindFirstFloatNoCheck(long columnIndex, float value)
        {
            return UnsafeNativeMethods.TableFindFirstFloat(this, columnIndex, value);
        }

        internal override long FindFirstDateNoCheck(long columnIndex, DateTime value)
        {
            return UnsafeNativeMethods.TableFindFirstDate(this, columnIndex, value);
        }

        internal override long FindFirstBoolNoCheck(long columnIndex, bool value)
        {
            return UnsafeNativeMethods.TableFindFirstBool(this, columnIndex, value);
        }

        internal TableView DistinctNoCheck(long columnIndex)
        {           
            return UnsafeNativeMethods.TableDistinct(this, columnIndex);
        }

        public TableView Distinct(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            return DistinctNoCheck(columnIndex);
        }

        public TableView Distinct(string columnName)
        {            
            return DistinctNoCheck(GetColumnIndex(columnName));
        }

        //note - SetIndex is only callable if the column is a string column
        internal void SetIndexNoCheck(long columnIndex)
        {
            UnsafeNativeMethods.TableSetIndex(this, columnIndex);
        }

        public void SetIndex(long columnIndex)
        {            
            ValidateColumnIndexAndTypeString(columnIndex);
            SetIndexNoCheck(columnIndex);
        }

        public void SetIndex(string columnName)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeString(columnIndex);
            SetIndexNoCheck(columnIndex);
        }


        internal override TableView FindAllIntNoCheck(long columnIndex, long value)
        {
            return UnsafeNativeMethods.TableFindAllInt(this,  columnIndex,  value);
        }




        internal override TableView FindAllStringNoCheck(long columnIndex, string value)
        {
            return UnsafeNativeMethods.TableFindAllString(this, columnIndex, value);
        }





        public Query Where()
        {
            return UnsafeNativeMethods.table_where(this);
        }

    }

    //custom exception for Table class. When Table runs into a Table related error, TableException is thrown
    //some system exceptions might also be thrown, in case they have not much to do with Table operation
    //following the pattern described here http://msdn.microsoft.com/en-us/library/87cdya3t.aspx
    [Serializable]
    public class TableException : Exception
    {
        public TableException()
        {
        }

        public TableException(string message)
            : base(message)
        {
        }

        public TableException(string message, Exception inner)
            : base(message, inner)
        {
        }
        
        protected TableException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
    //Was named TDBField before
    //now is named Field
    //I still don't like the name, it is more a colunm type definition or column type specification but what would be a good short word for that?
    //TDBField is used only in the table constructor to make it easier for the user to specify any table structure without too much clutter
    //TDBField constructors of various sort, return field definitions that the table constructor then uses to figure what the table structure is

    public class Field
    {
        protected static void SetInfo(Field someField, String someColumnName, DataType someFieldType)
        {
            if (someField != null)
            {
                someField.ColumnName = someColumnName;
                someField.FieldType = someFieldType;
            }
            else
                throw new ArgumentNullException("someField");
        }

        //this is internal for a VERY specific reason
        //when internal, the end user cannot call this function, and thus cannot put in a list of subtable fields containing a field that is parent
        //to the somefield parameter. If this was merely protected - he could!
        internal static void AddSubTableFields(Field someField, String someColumnName, Field[] subTableFieldsArray)
        {
            SetInfo(someField, someColumnName, DataType.Table);
            someField._subTable.AddRange(subTableFieldsArray);
        }

        public Field(string someColumnName, params Field[] subTableFieldsArray)
        {
            AddSubTableFields(this, someColumnName, subTableFieldsArray);
        }

        public Field(string columnName, DataType columnType)
        {
            SetInfo(this, columnName, columnType);
        }

        public Field(string columnName, String columnType)
        {
            if (columnName == null)
            {
                throw new ArgumentNullException("columnName");
            }

            if (columnType == null)
            {
                throw new ArgumentNullException("columnType");
            }

            switch (columnType.ToUpper(CultureInfo.InvariantCulture))
            {
                case "INT":
                case "INTEGER":
                    SetInfo(this, columnName, DataType.Int);
                    break;

                case "BOOL":
                case "BOOLEAN":
                    SetInfo(this, columnName, DataType.Bool);
                    break;

                case "STRING":
                case "STR":
                    SetInfo(this, columnName, DataType.String);
                    break;
                
                case "BINARY":
                case "BLOB":
                    SetInfo(this, columnName, DataType.Binary);
                    break;

                case "SUBTABLE":
                case "TABLE":
                    SetInfo(this, columnName, DataType.Table);
                    break;

                case "MIXED":
                    SetInfo(this, columnName, DataType.Mixed);
                    break;

                case "DATE":
                    SetInfo(this, columnName, DataType.Date);
                    break;

                case "FLOAT":
                    SetInfo(this, columnName, DataType.Float);
                    break;

                case "DOUBLE":
                    SetInfo(this, columnName, DataType.Double);
                    break;


                default:
                    throw new TableException(String.Format(CultureInfo.InvariantCulture,
                        "Trying to initialize a table field with an unknown type specification Fieldname:{0}  type:{1}",
                        columnName, columnType));
            }



        }

        protected Field() { }//used when IntegerField,StringField etc are constructed


        public String ColumnName { get; set; }

        public DataType FieldType { get; set; }

        private readonly List<Field> _subTable = new List<Field>();//only used if type is a subtable

        //potential trouble. A creative user could subclass Field to get access to getsubtablearray, then call this to get access to a subtable field, then set the subtable field reference to this same class or one of its parents in the field tree
        //then  call create table and provoke a stack overflow
        //could be avoided if the toarray did a deep copy
        //or if the individial items in the subTable could only be set once
        public Field[] GetSubTableArray()
        {
            return _subTable.ToArray();
        }
    }

    public class SubTableField : Field
    {
        public SubTableField(string columnName, params Field[] subTableFieldsArray)
        {
            AddSubTableFields(this, columnName, subTableFieldsArray);
        }
    }

    public class StringField : Field
    {
        public StringField(String columnName)
        {
            SetInfo(this, columnName, DataType.String);
        }
    }

    public class IntField : Field
    {
        protected IntField() { }//used when descendants of IntegerField are created

        public IntField(String columnName)
        {
            SetInfo(this, columnName, DataType.Int);
        }
    }

    public class BoolField : Field
    {
        protected BoolField() { }//used when descendants of IntegerField are created

        public BoolField(String columnName)
        {
            SetInfo(this, columnName, DataType.Bool);
        }
    }

    public class BinaryField : Field
    {
        protected BinaryField() { }//used when descendants of IntegerField are created

        public BinaryField(String columnName)
        {
            SetInfo(this, columnName, DataType.Binary);
        }
    }

    public class MixedField : Field
    {
        protected MixedField() { }//used when descendants of IntegerField are created

        public MixedField(String columnName)
        {
            SetInfo(this, columnName, DataType.Mixed);
        }
    }

    public class DateField : Field
    {
        protected DateField() { }//used when descendants of IntegerField are created

        public DateField(String columnName)
        {
            SetInfo(this, columnName, DataType.Date);
        }
    }

    public class FloatField : Field
    {
        protected FloatField() { }//used when descendants of IntegerField are created

        public FloatField(String columnName)
        {
            SetInfo(this, columnName, DataType.Float);
        }
    }

    public class DoubleField : Field
    {
        protected DoubleField() { }//used when descendants of IntegerField are created

        public DoubleField(String columnName)
        {
            SetInfo(this, columnName, DataType.Double);
        }
    }

    namespace Extensions
    {


        public static class TightDbExtensions
        {
            public static Field TightDbInt(this String fieldName)
            {
                return new Field(fieldName, DataType.Int);
            }


            public static Field Int(this String fieldName)
            {
                return new Field(fieldName, DataType.Int);
            }

            public static Field Bool(this string fieldName)
            {
                return new Field(fieldName, DataType.Bool);
            }

            public static Field TightDbBool(this string fieldName)
            {
                return new Field(fieldName, DataType.Bool);
            }

            public static Field TightDbString(this String fieldName)
            {
                return new Field(fieldName, DataType.String);
            }

            public static Field String(this String fieldName)
            {
                return new Field(fieldName, DataType.String);
            }

            public static Field TightDbBinary(this String fieldName)
            {
                return new Field(fieldName, DataType.Binary);
            }

            public static Field Binary(this String fieldName)
            {
                return new Field(fieldName, DataType.Binary);
            }

            public static Field TightDbSubTable(this String fieldName, params Field[] fields)
            {
                return new Field(fieldName, fields);
            }

            public static Field SubTable(this String fieldName, params Field[] fields)
            {
                return new Field(fieldName, fields);
            }

            //as the TightDb has a type called table, we also provide a such named constructor even though it will always be a subtable
            public static Field Table(this String fieldName, params Field[] fields)
            {
                return new Field(fieldName, fields);
            }

            public static Field TightDbMixed(this String fieldName)
            {
                return new Field(fieldName, DataType.Mixed);
            }

            public static Field Mixed(this String fieldName)
            {
                return new Field(fieldName, DataType.Mixed);
            }

            public static Field Date(this String fieldName)
            {
                return new Field(fieldName, DataType.Date);
            }

            public static Field TightDbDate(this String fieldName)
            {
                return new Field(fieldName, DataType.Date);
            }

            public static Field Float(this string fieldName)
            {
                return new Field(fieldName, DataType.Float);
            }

            public static Field TightDbFloat(this string fieldName)
            {
                return new Field(fieldName, DataType.Float);
            }

            public static Field Double(this string fieldName)
            {
                return new Field(fieldName, DataType.Double);
            }

            public static Field TightDbDouble(this string fieldName)
            {
                return new Field(fieldName, DataType.Double);
            }


        }
    }


}


//various ideas for doing what is done with c++ macros reg. creation of typed tables
//An extern method that creates the table on any class tha the extern might be called on. The extern would then have to 
//use reflection to figure what fields should be stored
//would mean that You annotate all fileds that should go into the table database
// + easy to use on existing classes,  
// + easy to build a new class using well known syntax and tools
// - could easily fool user to use unsupported types, 
// - user will not have a strong typed table classs, only his own class which is of whatever type
// - User would have to annotate fields to be put in the database. Default could be no field means all goes in
// + if this was just one of many ways to create a table, it could be okay - in some cases it might be convenient

// see implementation at //EXAMPLE1


//use scenarios i can think of : 

//new program, new classes, data known at the time the code is written (like, say, a database of 1 million permutations of something, and their precalculated value)

//new program, new classes, structure known at code time, but contents not known at code time (like, user will have to import a text file where the layout is known)

//new program, new classes, structure (fields etc.) not known at runtime (could be an xml importer or something else where the scema depends on the data the program reads)

//old program, already coded classes with known data at runtime needs to be shifted from technology X  to tightdb

//old program, new classes, structure known at code time, but contents not known at code time, shifted from technology X  to tightdb

//old program, new classes, structure (fields etc.) not known at runtime , shifted from technology X to tightdb

//old program, already coded classes with known data at runtime needs to be shifted from technology X  to tightdb

//Technology X  could be : c# array, C# collection, C# stream, C# dataset
//the already coded classes could inherit from anything and could have many many properties and members, of which only a subset should be saved in tightdb

//a good tightdb binding will have support for easy transformation in all the above cases
