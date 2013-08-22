using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
//using System.Threading.Tasks; not portable as of 2013-04-02

//Tell compiler to give warnings if we publicise interfaces that are not defined in the cls standard
//http://msdn.microsoft.com/en-us/library/bhc3fa7f.aspx
using System.Text;

[assembly: CLSCompliant(true)]

//Table class. The class represents a tightdb table.
//implements idisposable - will clean itself up (and any c++ resources it uses) when garbage collected
//If You plan to save resources, You can use it with the using syntax.



namespace TightDbCSharp
{


    //Tightdb Table
    //could have been called RowCollection but is called Table as it in fact is a table and not merely a collection of rows
    public class Table : TableOrView, IEnumerable<Row>, ICloneable
    {
        //manual dll version info. Used when debugging to see if the right DLL is loaded, or an old one
        //the number is a date and a time (usually last time i debugged something)
        private const long GetDllVersionCSharp = 1305301717 ;

     
        //this is not called if constructed with parametres
        public Table()
        {
            TableNew();
        }

        
        //This is used when we want to create a table and we already have the c++ handle that the table should use.  used by GetSubTable
        internal Table(IntPtr tableHandle,bool shouldbedisposed)
        {
            SetHandle(tableHandle,shouldbedisposed);
        }

        //implements ICloneable - this method is called Copy in the c++ binding        
        public Table Clone()
        {
            return UnsafeNativeMethods.CopyTable(this);
        }

        
        object ICloneable.Clone()
        {
            return Clone();
        }


        public bool IsValid()
        {
            return UnsafeNativeMethods.TableIsValid(this);
        }


        public bool HasSharedSpec()
        {
            return UnsafeNativeMethods.TableHasSharedSpec(this);
        }

        //Used to block calls to updatefromspec if the table already have columns that
        //have been saved with updatefromspec. Coded here because c++ Table.ColumnCount also
        //counts rows that have not been "saved" with a call to updatefromspec
        //this bool defaults to false,and is set to true when addcolumn or
        //updatefromspec is called
        internal bool HasColumns { private get; set; }

        //allow end users to query a table to figure if it is okay to add colums to that table using spec
        //if returns false, user will have to use table column operations only
        //false means that this table's spec as well as any subspec you can get from it, cannot be used
        //to alter columns.
        //however, altering mixed subtables is determined by calling the mixed subtable.SpecModifyable
        //the check for HasSharedSpec is to ensure that all subtables taken out from table rows will always return false
        public bool SpecModifyable()
        {
            return (!HasColumns && !HasSharedSpec());
        }


        //see tableview for further interesting comments
        public TableRow this[long rowIndex]
        {
            get
            {
                ValidateRowIndex(rowIndex);
                return RowForIndexNoCheck(rowIndex);
            }
        }

        //resembling the typed back() method or the untyped last method in python (that returns a cursor object)
        //see similar implementation in TableView 
        public TableRow Last()
        {
            long s = Size;
            if (s > 0)
            {
                return RowForIndexNoCheck(s - 1);
            }
            throw new InvalidOperationException("Last called on a TableView with no rows in it");
        }
       // */

        private TableRow RowForIndexNoCheck(long rowIndex)
        {
            return new TableRow(this, rowIndex);
        }

        

        //the following code enables Table to be enumerated, and makes TableRow the type You get back from an enummeration
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

    //    public Table(params object[] fieldDescriptions)
    //    {
    //    }

//        public Table(String fieldname1, DataType type1,String fieldname2)

        public Table DefineSchema(params Field[] schema)
        {
            if (schema == null)
            {
                throw new ArgumentNullException("schema");
            }
            ValidateSpecChangeIsOkay();
            foreach (Field tf in schema)
            {
                if (tf == null)
                {
                    throw new ArgumentNullException("schema", "one or more of the field objects is null");
                }
                Spec.AddFieldNoCheck(tf);
            }
            UpdateFromSpecNoCheck();
            return this;//allow fluent creation of table in a group
        }

        //this method is intended to be called on a table with no data in it.
        //the method will create columns in the table, matching the specified schema.
        private void DefineSchema(Field schema)
        {
            ValidateSpecChangeIsOkay();
            Spec.AddFieldNoCheck(schema);
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

        
        //to do: remove these debug methods
        public static Int64 DebugToTightDbTime(DateTime date)
        {
            return UnsafeNativeMethods.ToTightDbTime(date);
        }

        public static DateTime DebugToCSharpTimeUtc(Int64 cppTime)
        {
            return UnsafeNativeMethods.ToCSharpTimeUtc(cppTime);
        }
        
        //This method will test basic interop, especially test that the c++ compiler used to build the c++ dll binding uses
        //the same size and sequence for various types used in interop, as C# and C# marshalling expects
        public static void TestInterop()
        {
            UnsafeNativeMethods.TestInterop();
            
        }


        //this dll call is only used in ShowVersionTest. 
        //will probably not work with mono so at that time should be inside a define
        //as should the line below using it
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [PreserveSig]
        private static extern uint GetModuleFileName
        (
            [In] IntPtr hModule,
            [Out] StringBuilder lpFilename,
            [In][MarshalAs(UnmanagedType.U4)] int nSize
        );


        //dump various OS diagnostics to console
        public static void ShowVersionTest()
        {
            var pointerSize = IntPtr.Size;
            var vmBitness = (pointerSize == 8) ? "64bit" : "32bit";            
            var dllstring = (pointerSize == 8) ? UnsafeNativeMethods.L64 : UnsafeNativeMethods.L32;
            
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
            Console.WriteLine("Now Loading {0} - expecting it to be a {1} dll", dllstring,  vmBitness);
            //Console.WriteLine("Loading "+thisapplocation+"...");

            //if something is wrong with the DLL (like, if it is gone), we will not even finish the creation of the table below.
            using (var t = new Table())
            {
                //the DLL must have loaded correctly

                const int maxPath = 260;
                var builder = new StringBuilder(maxPath);
                var hModule = GetModuleHandle(dllstring);
                if (hModule != IntPtr.Zero)//could be zero if the dll has never been called, but then we would not be here in the first place
                {
                    GetModuleFileName(hModule, builder, builder.Capacity);
                }

                Console.WriteLine("DLL File Actually Loaded :\n {0}", builder);
                Console.WriteLine("C#  DLL        build number {0}", GetDllVersionCSharp+t.Size);//t.size is 0, but use t to make the compiler happy
                Console.WriteLine("C++ DLL        build number {0}", CPlusPlusLibraryVersion());
            }
            Console.WriteLine();
            Console.WriteLine();
        }


        private static long CPlusPlusLibraryVersion()
        {
            return UnsafeNativeMethods.CppDllVersion();
        }



        //not accessible by source not in the TightDBCSharp namespace
        //TableHandle contains the value of a C++ pointer to a C++ table
        //it is sent as a parameter to calls to the C++ DLL.

        private void TableNew()
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
        protected override void ReleaseHandle()
        {
            UnsafeNativeMethods.TableUnbind(this);            
        }

        internal override Spec GetSpec()
        {
            return UnsafeNativeMethods.TableGetSpec(this); 
        }


        //only call this method if You are sure Updatefromspec is legal (having checked shared tables, readonly etc.)
        //this will update the table structure to represent whatever the earlier recieved spec has been set up to, and altered to
        private void UpdateFromSpecNoCheck()
        {            
            UnsafeNativeMethods.TableUpdateFromSpec(this);
            HasColumns = true;
        }

        //note that this call can be made on a root table as well as on a subtable or a mixed subtable
        public void ValidateSpecChangeIsOkay()
        {
            ValidateIsValid();
            ValidateNoColumns(); //updatefromspec can only be called once, to create the table structure, so no prior columns
            ValidateIsEmpty();//of course the table must also be empty (although i fail to see how a table can have no columns and be not-empty)
            ValidateNotSharedSpec();//updatefromspec can only be called on a root table, if we are called on a subtable, changes are not legal
        }


        //todo : (asana)should not be public, call updatefromspec automatically from methods that change the spec (so find out how to get to the top level table when all you have is a spec)        
        public void UpdateFromSpec()
        {
            ValidateSpecChangeIsOkay();
            UpdateFromSpecNoCheck();
        }

        internal void ValidateIsValid()
        {
            if (! IsValid())
            {
                throw new InvalidOperationException("Table accessor is no longer valid. No operations except calling IsValid is allowed");
            }
        }

        internal void ValidateNoColumns()
        {
            if (HasColumns)
            {
                throw new InvalidOperationException("Updatefromspec can only be called on a table with no existing columns");
            }
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


        public  string ToJson()
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

        private void ValidateNotSharedSpec()
        {
            if (HasSharedSpec())
            {
                throw new InvalidOperationException("It is illegal to alter the column structure of subtables that has been read in from a table row");
            }
        }
        
        //this will add a column of the specified type, if it is a table type, You will have to populate it yourself later on,
        //by getting its subspec and working with that
        //it is not allowed in the c++ binding to call AddColumn on a subtable that has been taken out
        //from a column,row - unless it is a mixed column.
        //we check this, this way :
        //if we have rows, addcolumn is not allowed - throw
        //if we have a shared spec, we are a subtable inside a row - throw
        //otherwise we must be a root or a table from a mixed row.
        public long AddColumn(DataType type, String name)
        {            
            ValidateIsEmpty();
            ValidateNotSharedSpec();             
            long colIx =  UnsafeNativeMethods.TableAddColumn(this, type, name);
            HasColumns = true;
            return colIx;
        }

        private Boolean HasIndexNoCheck(long columnIndex )
        {        
            return UnsafeNativeMethods.TableHasIndex(this,columnIndex);
        }

        //only legal to call this with a string column index
        //however, later on the other columns will also have indicies and then it will
        //be legal to call with more types, eventually all types
        //right now, just return false for non-string columns, and otherwise
        //ask core if there is an index
        public Boolean HasIndex(long columnIndex)
        {
            ValidateIsValid();
            ValidateColumnIndex(columnIndex);
            if (ColumnType(columnIndex) == DataType.String)
            {
                return HasIndexNoCheck(columnIndex);                
            }
            return false;
        }

        //only legal to call this with a string column index
        //right now return false if it is not a string column, otherwise
        //ask core. See HasIndex(long columnIndex)
        public Boolean HasIndex(String columnName)
        {
            ValidateIsValid();
            long columnIndex = GetColumnIndex(columnName);
            if (ColumnType(columnIndex)==DataType.String)            
              return HasIndexNoCheck (columnIndex);
            return false;
        }

        //expects the columnIndex to already have been validated as legal and type string
        private void ValidateMustHaveIndexNoColCheck(long columnIndex)
        {            
            if (!HasIndexNoCheck(columnIndex))
            {
                throw new InvalidOperationException(String.Format("Column Index {0} must specify a string column that is indexed", columnIndex));
            }
        }

        //will throw exception if an object in arr is not row-compatible with the table - but rows up to that
        //point will have been added. Put the call in a transaction and rollback if you get an exception
        public void AddMany(IEnumerable arr)
        {
            foreach (var row in arr )
            {
                Add(row);
            }
        }

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

        /* won't work in a using scenario as using does not like the object being used to be assigned inside the using scope
        public static Table operator +(Table left,  object[]  right) {
            left.Add(right);
            return left;
        }
        */
        public void Set(long rowIndex, params object[] rowData)
        {
            ValidateRowIndex(rowIndex);
            SetRowNoCheck(rowIndex,rowData);
        }


        private void ValidateInsertRowIndex(long rowIndex)
        {
                        if (rowIndex < 0 || rowIndex > Size)
            {
                throw new ArgumentOutOfRangeException("rowIndex",String.Format("Table.Insert - row Index out of range. Table size {0}  index specified {1}",Size,rowIndex));
            }                

        }
        //insert rowdata into a new row that is inserted at rowIndex
        //rowindex=0 means insert into very first record
        //rowindex=size means add after last        
        public void Insert(long rowIndex, params object[] rowData)
        {
            ValidateInsertRowIndex(rowIndex);
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
            return UnsafeNativeMethods.TableGetBinary(this,columnIndex,rowIndex);
        }

        internal override Table GetSubTableNoCheck(long columnIndex, long rowIndex)
        {
            Table fromSubtableCell =  UnsafeNativeMethods.TableGetSubTable(this, columnIndex, rowIndex);
            fromSubtableCell.HasColumns = fromSubtableCell.ColumnCount > 0;
            return fromSubtableCell;
        }

        internal override void ClearSubTableNoCheck(long columnIndex, long rowIndex)
        {
            UnsafeNativeMethods.TableClearSubTable(this, columnIndex, rowIndex);
        }

        internal override void SetStringNoCheck(long columnIndex, long rowIndex,string value)
        {
            UnsafeNativeMethods.TableSetString(this,columnIndex,rowIndex,value);
        }

        internal override void SetBinaryNoCheck(long columnIndex, long rowIndex, byte[] value)
        {
            UnsafeNativeMethods.TableSetBinary(this, columnIndex, rowIndex, value);
        }

        internal override void SetSubTableNoCheck(long columnIndex, long rowIndex, Table value)
        {
            UnsafeNativeMethods.TableSetSubTable(this,columnIndex,rowIndex,value);            
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
            return UnsafeNativeMethods.TableGetMixedString(this, columnIndex, rowIndex);
        }

        internal override byte[] GetMixedBinaryNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableGetMixedBinary(this, columnIndex, rowIndex);
        }

        internal override Table GetMixedSubTableNoCheck(long columnIndex, long rowIndex)
        {
            var mixedSubTable= UnsafeNativeMethods.TableGetSubTable(this, columnIndex, rowIndex);
            mixedSubTable.HasColumns = mixedSubTable.ColumnCount > 0;//if it has columns, mark it down for usuitable to work with spec modifications
            return mixedSubTable;
        }

        //warning! Use only this one when inserting new rows that are not inserted yet
        //todo:implement the rest of the insert api for inserting - but only for internal use
        private void InsertInt(long columnIndex, long rowIndex, long value)
        {
            UnsafeNativeMethods.TableInsertInt(this, columnIndex, rowIndex, value);
        }

        public void InsertEmptyRow(long rowIndex, long rowsToInsert)
        {            
            ValidateInsertRowIndex(rowIndex);
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

        private TableView DistinctNoCheck(long columnIndex)
        {           
            return UnsafeNativeMethods.TableDistinct(this, columnIndex);
        }

        public TableView Distinct(long columnIndex)
        {
            ValidateColumnIndexAndTypeString(columnIndex);
            ValidateMustHaveIndexNoColCheck(columnIndex);
            return DistinctNoCheck(columnIndex);
        }

        //currently c++ core only supports index on string, and only Distinct on indexed columns so we disallow anything but indexed string fields
        public TableView Distinct(string columnName)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeString(columnIndex);
            ValidateMustHaveIndexNoColCheck(columnIndex);
            return DistinctNoCheck(columnIndex);
        }

        //note - SetIndex is only callable if the column is a string column
        private void SetIndexNoCheck(long columnIndex)
        {
            UnsafeNativeMethods.TableSetIndex(this, columnIndex);
        }

        public void SetIndex(long columnIndex)
        {            
            ValidateNotSharedSpec();//core does not support indexes on colums in non-mixed subtables
            ValidateColumnIndexAndTypeString(columnIndex);
            SetIndexNoCheck(columnIndex);
        }

        public void SetIndex(string columnName)
        {
            ValidateNotSharedSpec();//core does not support indexes on colums in non-mixed subtables
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

/* experiment
        public static Field Field<T>(string name)
        {
            if (typeof (T) == typeof (String))
            {
                return new StringField(name);
            }

            if (typeof(T) == typeof(DateTime))
            {
                return new DateField(name);
            }

            if (typeof(T) == typeof(Double))
            {
                return new DoubleField(name);
            }

            if (typeof(T) == typeof(float))
            {
                return new FloatField(name);
            }


            throw new Exception("fieldtype not supported natively in tightdb binding");

    }
*/

        public Query Where()
        {
            return UnsafeNativeMethods.table_where(this);
        }

    }

    /*
    //custom exception for Table class. When Table runs into a Table related error, TableException is thrown
    //some system exceptions might also be thrown, in case they have not much to do with Table operation
    //following the pattern described here http://msdn.microsoft.com/en-us/library/87cdya3t.aspx
    //after some consideration I am contemplating writing out the TableException from the project.
    //The already existing exception types are good enough, and we will have a little less bloat
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
    */


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
        internal static void AddSubTableFields(Field someField, String someColumnName, IEnumerable<Field> subTableFieldsArray)
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
                    throw new ArgumentException(String.Format(CultureInfo.InvariantCulture,
                        "Trying to initialize a table field with an unknown type specification Fieldname:{0}  type:{1}",
                        columnName, columnType));
            }



        }

        protected Field() { }//used when IntegerField,StringField etc are constructed


        public String ColumnName { get; private set; }

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
        protected IntField() { }//used when descendants of IntField are created

        public IntField(String columnName)
        {
            SetInfo(this, columnName, DataType.Int);
        }
    }

    public class BoolField : Field
    {
// ReSharper disable UnusedMember.Global
        protected BoolField() { }//used when descendants of BoolField are created
// ReSharper restore UnusedMember.Global

        public BoolField(String columnName)
        {
            SetInfo(this, columnName, DataType.Bool);
        }
    }

    public class BinaryField : Field
    {
// ReSharper disable UnusedMember.Global
        protected BinaryField() { }//used when descendants of BinaryField are created
// ReSharper restore UnusedMember.Global

        public BinaryField(String columnName)
        {
            SetInfo(this, columnName, DataType.Binary);
        }
    }

    public class MixedField : Field
    {
// ReSharper disable UnusedMember.Global
        protected MixedField() { }//used when descendants of MixedField are created
// ReSharper restore UnusedMember.Global

        public MixedField(String columnName)
        {
            SetInfo(this, columnName, DataType.Mixed);
        }
    }

    public class DateField : Field
    {
// ReSharper disable UnusedMember.Global
        protected DateField() { }//used when descendants of DateField are created
// ReSharper restore UnusedMember.Global

        public DateField(String columnName)
        {
            SetInfo(this, columnName, DataType.Date);
        }
    }

    public class FloatField : Field
    {
// ReSharper disable UnusedMember.Global
        protected FloatField() { }//used when descendants of FloatField are created
// ReSharper restore UnusedMember.Global

        public FloatField(String columnName)
        {
            SetInfo(this, columnName, DataType.Float);
        }
    }

    public class DoubleField : Field
    {
// ReSharper disable once UnusedMember.Global
        protected DoubleField() { }//used when descendants of DoubleField are created

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
