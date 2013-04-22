using System;
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





    /// <summary>
    /// represents a single column in a table row in a table. Not fast due to extra calls, but easy to use
    /// If You need top speed, work directly with TableRow or directly with Table
    /// </summary>
    /// 
    //TODO:Implement child types that each are bound to a given DataType. So we got for instance TableIntColumn
    //why? in some cases We could return TableIntColumn and with that one, we would not have to check if the Table column type is int every time
    //we read data from the row. So working with typed column fields is somewhat faster
    public class TableRowColumn
    {
        private TableRow _owner;
        public TableRow Owner { get { return _owner; } set { _owner = value; _columntypeloaded = false; } }
        private long _columnIndex;
        public long ColumnIndex
        {
            get { return _columnIndex; }
            internal set { _columnIndex = value; _columntypeloaded = false; }//internal bc users must not be allowed to change the columnindex. We treat it as already checked in calls
        }
        public TableRowColumn(TableRow owner,long column)
        {
            Owner = owner;
            ColumnIndex= column;
            _columntypeloaded = false;
        }

        private DataType _columnType;
        private Boolean _columntypeloaded;
        //this could be optimized by storing columncount in the table class
        public bool IsLastColumn()
        {
           return (Owner.Owner.ColumnCount==ColumnIndex+1);
        }
        public DataType ColumnType
        {
            get
            {
                if (_columntypeloaded)
                {
                    return _columnType;
                }
                else
                {
                    _columnType = Owner.Owner.ColumnType(ColumnIndex);
                    return _columnType;
                }
            }
        }
        
        //if it is a mixed we return mixed! -not the type of the field
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming",
            "CA2204:Literals should be spelled correctly", MessageId = "TableRowColumn")]
        public object Value
        {
            get
            {
                switch (ColumnType)
                {
                    case DataType.Int:
                        return Owner.GetLongNoCheck(ColumnIndex);
                            //row and column not user specified so safe, and type checked in switch above so also safe
                    default:
                        return String.Format(CultureInfo.InvariantCulture, "Getting type {0} from TableRowColumn not implemented yet",
                                             ColumnType); //so null means the datatype is not fully supported yet
                }
            }
            set
            {
                switch (ColumnType)
                {
                    case DataType.Int:
                        Owner.SetLongNoCheck(ColumnIndex, (long)value );//the cast will raise an exception if value is not a long, or at least convertible to long
                        break;
                    default:
                        {
                            throw new TableException(String.Format(CultureInfo.InvariantCulture,
                                                                   "setting type {0} in TableRowColumn not implemented yet",
                                                                   ColumnType));
                        }
                }
            }
        }


        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DataType"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "GetLong")]
/*
        private long GetLong()
        {            
            return Owner.GetLong(ColumnIndex);//will be type chekced (only) in table class
        }
*/
    }


    //represents one row in a table. Access to the individual columns are handled by the associated Table
    //Currently, only access by column number is supported
    //currently only reading is supported
    //this is the TableRecord type You get back from foreach if you foreach(TableRow tr in mytable) {tr.operation()}

    /// <summary>
    /// Represents one row in a tightdb Table
    /// </summary>
    public class TableRow
    {
        internal TableRow(Table owner,long row) {
            Owner=owner;
            
            // The Row number of the row this TableRow references
            Row=row;
        }
        public Table Owner { get; set; }
        public long Row { get; internal set; }//users should not be allowed to change the row property of a tablerow class
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "long")]
        public long GetLong(long columnNumber) {
            { return Owner.GetLongNoColumnRowCheck(columnNumber,Row);}
        }

/*
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "long")]
        internal long getLongNoTypeCheck(long columnNumber)
        {
            return Owner.GetLongNoTypeCheck(columnNumber, Row);
        }
        */
        internal long GetLongNoCheck(long columnIndex)
        {
            return Owner.GetLongNoCheck(columnIndex, Row);//we know that Row could not have been set by user code, so it's safe
        }

        //allow foreach to traverse a TableRow and get some TableRowColumn objects
        //if You do a foreach on a tablerow, C# will use the for loop below to do the iteration
        public IEnumerator<TableRowColumn> GetEnumerator()
        {
            for (long i = 0; i < Owner.ColumnCount; i++)
            {
                yield return new TableRowColumn(this, i);
            }
        }


        //public object GetValue(long columnNumber)
        //{
            //return Owner.GetValue(row, columnNumber);
        //}
        public void SetLongNoCheck(long columnIndex, long value)
        {
            Owner.SetLongNoCheck(columnIndex,Row,value);
        }
    }

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



    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class Table : IDisposable , IEnumerable<TableRow>
    {
        //manual dll version info. Used when debugging to see if the right DLL is loaded, or an old one
        //the number is a date and a time (usually last time i debugged something)
        public  const long GetDllVersionCSharp = 1304151452 ;

        //the following code enables Table to be enumerated, and makes TableRow the type You get back from an enummeration
        public IEnumerator<TableRow> GetEnumerator() { return new Enumerator(this); }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return new Enumerator(this); }

        class Enumerator : IEnumerator<TableRow>//probably overkill, current needs could be met by using yield
        {
            long _currentRow = -1;
            Table _myTable;
            public Enumerator(Table table)
            {
                _myTable = table;
            }
            public TableRow Current { get { return new TableRow(_myTable, _currentRow); } }
            object System.Collections.IEnumerator.Current { get { return Current; } }

            public bool MoveNext()
            {
                return ++_currentRow < _myTable.Size();
            }

            public void Reset() { _currentRow = -1; }
            public void Dispose()
            {
                _myTable = null; //remove reference to Table class
            }
        }


        //following the dispose pattern discussed here http://dave-black.blogspot.dk/2011/03/how-do-you-properly-implement.html
        //a good explanation can be found here http://stackoverflow.com/questions/538060/proper-use-of-the-idisposable-interface

        public bool IsDisposed {  get;private  set; }
        //called by users who don't want to use our class anymore.
        //should free managed as well as unmanaged stuff
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);//tell finalizer it does not have to call dispose or dispose of things -we have done that already
        }
        //if called from GC  we should not dispose managed as that is unsafe, the bool tells us how we were called
        protected virtual void Dispose(bool disposeManagedToo)
        {
            if (!IsDisposed)
            {
                if (disposeManagedToo)
                {
                    //dispose any managed members table might have
                }

                //dispose any unmanaged stuff we have
                Unbind();
                IsDisposed = true;
            }
        }

        //when this is called by the GC, unmanaged stuff might not have been freed, and managed stuff could be in the process of being
        //freed, so only get rid of unmanaged stuff
        ~Table()
        {
            try
            {
                Dispose(false);
            }
// ReSharper disable RedundantEmptyFinallyBlock
            finally
            {
                // Only use this line if Table starts to inherit from some other class that itself implements dispose
                //                base.Dispose();
            }
// ReSharper restore RedundantEmptyFinallyBlock
        }

        //always acquire a table handle
        public Table()
        {
            TableNew();
        }

        //This is used when we want to create a table and we already have the c++ handle that the table should use.  used by GetSubTable
        internal Table(IntPtr tableHandle,bool shouldbedisposed)
        {
            SetTableHandle(tableHandle,shouldbedisposed);
        }
        //will only log in debug mode!
        //marker is a string that will show as the first log line, use this if several places in the code enable logging and disable it again, to
        //easily see what section we're in

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
            //Spec spec = GetSpec();
            foreach (Field tf in schema)
            {
                if (tf == null)
                {
                    throw new ArgumentNullException("schema","one or more of the field objects is null");
                }
                Spec.AddField(tf);
            }
            UpdateFromSpec();
        }

        //allows the user to quickly create a table with a single field of a single type
        public Table(Field schema)
        {
            TableNew();//allocate a table class in c++
            //Spec spec = GetSpec();//get a handle to the table's new empty spec
            Spec.AddField(schema);
            UpdateFromSpec();//build table from the spec tree structure
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

        public static long CPlusPlusLibraryVersion()
        {
            return UnsafeNativeMethods.CppDllVersion();
        }




        //experiments
        /*
        public object this[int RowIndex, String ColumnName]
        {
            get
            {
                switch (column_type(RowIndex))
                {
                    case DataType.Int:
                    //    return getInt(RowIndex, get_column_index(ColumnName));


                    //and add support for the rest (see TDB definition)

                    default:
                        return null;//we should probably raise an exception here
                }
            }
            set
            {
            }



        }
        */
        //experiments
        /*
        public object this[int row, int column]
        {
                        get
                        {
                            switch (column_type(row,column))
                            {
                                case DataType.Int:
                                // return getInt(ColumnIndex, RowIndex);

                                //and add support for the rest (see TDB definition)

                                default:
                                    return null;
                            }
                        }
                        set
                        {
                        }
        }
        */
        //not accessible by source not in the TightDBCSharp namespace
        //TableHandle contains the value of a C++ pointer to a C++ table
        //it is sent as a parameter to calls to the C++ DLL.

        internal IntPtr TableHandle { get; set; }  //handle (in fact a pointer) to a c++ hosted Table. We must unbind this handle if we have acquired it
        internal bool TableHandleInUse { get; set; } //defaults to false.  TODO:this might need to be encapsulated with a lock to make it thread safe (although several threads *opening or closing* *the same* table object is probably not happening often)
        internal bool TableHandleHasBeenUsed { get; set; } //defaults to false. If this is true, the table handle has been allocated in the lifetime of this object
        internal bool NotifyCppWhenDisposing { get; set; }//if false, the table handle do not need to be disposed of, on the c++ side


        //use this function to set the table handle to make sure various booleans are set correctly        

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "SetTableHandle"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "SetTableHandle"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "TableNew")]
        internal void SetTableHandle(IntPtr newTableHandle,bool shouldBeDisposed)
        {
            if (TableHandleInUse)
            {
                throw new TableException("SetTableHandle called on a table that already has acquired a table handle");
            }
            else
            {
                TableHandle = newTableHandle;
                TableHandleInUse = true;
                TableHandleHasBeenUsed = true;
                NotifyCppWhenDisposing = shouldBeDisposed;
            }
        }

        //This method will ask c++ to create a new table object and then the method will store the table objects handle        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "TableNew")]
        internal void TableNew()
        {
           UnsafeNativeMethods.TableNew(this);//calls settablehandle itself
        }



        //This method will ask c++ to dispose of a table object created by table_new.
        //this method is for internal use only
        //it will automatically be called when the table object is disposed (or garbage collected)
        //In fact, you should not at all it on your own
        internal void Unbind()
        {
            if (TableHandleInUse)
            {
                if (NotifyCppWhenDisposing)
                  UnsafeNativeMethods.TableUnbind(this);
                TableHandleInUse = false;
            }
            else
            {
                //  If you simply create a table object and then deallocate it again without ever acquiring a table handle
                //  then no exception is raised. However, if unbind is called, and there once was a table handle,
                //  it is assumed an error situation has occoured (too many unbind calls) and an exception is raised
                if (TableHandleHasBeenUsed)
                {
                    throw new TableException("unbind called on a table with no table handle active anymore");
                }
            }
        }

        //spec getter public bc a user might want to get subtable schema on a totally empty table,and that is only available via spec atm.
        public Spec Spec
        {
            get { return UnsafeNativeMethods.TableGetSpec(this); }
        }
        //this will update the table structure to represent whatever the earlier recieved spec has been set up to, and altered to
        //TODO : what if the table contains data
        public void UpdateFromSpec()
        {
           UnsafeNativeMethods.TableUpdateFromSpec(this);
        }

        public DataType ColumnType(long columnIndex)
        {
            return UnsafeNativeMethods.TableGetColumnType(this, columnIndex);
        }

        //Might be called often, as it only changes if columns are added, perhaps we should cache the value in the Table class
        public long ColumnCount
        {
            get  {return UnsafeNativeMethods.TableGetColumnCount(this);}
        }

        
        //this will add a column of the specified type, if it is a table type, You will have to populate it yourself later on,
        //by getting its subspec and working with that
        public long AddColumn(DataType type, String name)
        {
            return UnsafeNativeMethods.TableAddColumn(this, type, name);
        }

        public string GetColumnName(long columnIndex)//unfortunately an int, bc tight might have been built using 32 bits
        {
            return UnsafeNativeMethods.TableGetColumnName(this, columnIndex);
        }

        public long AddEmptyRow(long numberOfRows)
        {
            return UnsafeNativeMethods.TableAddEmptyRow(this, numberOfRows);
        }

        public void SetLong(long columnIndex, long rowIndex, long value)
        {
            ValidateColumnIndex(columnIndex);
            ValidateRowIndex(rowIndex);
            ValidateColumnTypeInt(columnIndex);
            SetLongNoCheck(columnIndex,rowIndex,value);
        }

        public Table GetSubTable(long columnIndex, long rowIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateRowIndex(rowIndex);
            ValidateColumnTypeSubTable(columnIndex);
            return UnsafeNativeMethods.TableGetSubTable(this,columnIndex,rowIndex);
        }

        //warning! Use only this one when inserting new rows that are not inserted yet
        public void InsertInt(long columnIndex, long rowIndex, long value)
        {
            UnsafeNativeMethods.TableInsertInt(this, columnIndex, rowIndex, value);
        }

        //number of records in this table
        public long Size()
        {
            return UnsafeNativeMethods.TableSize(this);
        }

        public void ValidateColumnIndex(long columnIndex)
        {
            if (columnIndex >= ColumnCount || columnIndex<0)
            {
                throw new ArgumentOutOfRangeException("columnIndex",String.Format(CultureInfo.InvariantCulture, "illegal columnIndex:{0} Table Column Count:{1}", columnIndex,ColumnCount));
            }            
        }

        //the parameter is the column type that was used on access, and it was not the correct one
        internal string GetColumnTypeErrorString(long columnIndex, DataType columnType)
        {
            return String.Format(CultureInfo.InvariantCulture,"column:{0} invalid data access. Real column DataType:{1} Accessed as {2}", columnIndex, ColumnType(columnIndex), columnType);
        }


        //only call if columnIndex is already validated or known to be int
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DataType"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "GetLong")]
        internal void ValidateColumnTypeInt(long columnIndex ) 
        {
            if (ColumnType(columnIndex) != DataType.Int)
            {
                throw new TableException(GetColumnTypeErrorString(columnIndex,DataType.Int));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "subTable"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "subtable")]
        internal void ValidateColumnTypeSubTable(long columnIndex)
        {
            if (ColumnType(columnIndex) != DataType.Table)
            {
                throw new TableException(GetColumnTypeErrorString(columnIndex,DataType.Table));
            }
        }

        public void ValidateRowIndex(long rowIndex) 
        {
            if (rowIndex >= Size() || rowIndex<0)
            {
                throw new ArgumentOutOfRangeException("rowIndex",string.Format(CultureInfo.InvariantCulture,"Table accessed with an invalid Row Index{0}. Table Size is:{1}",rowIndex, Size())); //re-calculating when composing error message to avoid creating a variable in a performance sensitive place
            }
        }

        //if You call from TableRow or TableColumn, You will save some checking - this is the slowest way
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "long")]
        public long GetLong(long columnIndex, long rowIndex)
        {
            ValidateRowIndex(rowIndex);
            ValidateColumnIndex(columnIndex);
            ValidateColumnTypeInt(columnIndex);
            return GetLongNoCheck(columnIndex,rowIndex);//could be sped up if we directly call UnsafeNativeMethods
        }

        //only call this method if You know for sure that RowIndex is less than or equal to table.size()
        //and that you know for sure that columnIndex is less than or equal to table.columncount
        internal long GetLongNoColumnRowCheck(long columnIndex, long rowIndex)
        {            
            ValidateColumnTypeInt(columnIndex);
            return GetLongNoCheck(columnIndex,rowIndex);
        }

        //only call this one if You know for sure that the field at columnindex,rowindex is in fact an ordinary DataType.Int field (not mixed.integer)
        internal long GetLongNoTypeCheck(long columnIndex, long rowIndex)
        {
            ValidateRowIndex(rowIndex);
            ValidateColumnIndex(columnIndex);
            return GetLongNoCheck(columnIndex, rowIndex);
        }

        //only call if You are certain that 1: The field type is Int, 2: The columnIndex is in range, 3: The rowIndex is in range
        internal long GetLongNoCheck(long columnIndex,long rowIndex)
        {
            return UnsafeNativeMethods.TableGetInt(this,columnIndex, rowIndex);
        }

        public Boolean GetBoolean(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableGetBool(this, columnIndex, rowIndex);
        }

        internal void SetLongNoCheck(long columnIndex, long rowIndex, long value)
        {
            UnsafeNativeMethods.TableSetLong(this, columnIndex, rowIndex, value);
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "tablefield"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "subtable")]
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
            if (columnType.ToUpper(CultureInfo.InvariantCulture) == "INT" || columnType.ToUpper(CultureInfo.InvariantCulture) == "INTEGER")
            {
                SetInfo(this, columnName, DataType.Int);
            }
            else if (columnType.ToUpper(CultureInfo.InvariantCulture) == "BOOL" || columnType.ToUpper(CultureInfo.InvariantCulture) == "BOOLEAN")
            {
                SetInfo(this, columnName, DataType.Bool);
            }
            else if (columnType.ToUpper(CultureInfo.InvariantCulture) == "STRING" || columnType.ToUpper(CultureInfo.InvariantCulture) == "STR")
            {
                SetInfo(this, columnName, DataType.String);
            }
            else if (columnType.ToUpper(CultureInfo.InvariantCulture) == "BINARY" || columnType.ToUpper(CultureInfo.InvariantCulture) == "BLOB")
            {
                SetInfo(this, columnName, DataType.Binary);
            }
            else if (columnType.ToUpper(CultureInfo.InvariantCulture) == "MIXED")
            {
                SetInfo(this, columnName, DataType.Mixed);
            }

            else if (columnType.ToUpper(CultureInfo.InvariantCulture) == "DATE")
            {
                SetInfo(this, columnName, DataType.Date);
            }

            else if (columnType.ToUpper(CultureInfo.InvariantCulture) == "FLOAT")
            {
                SetInfo(this, columnName, DataType.Float);
            }
            else if (columnType.ToUpper(CultureInfo.InvariantCulture) == "DOUBLE")
            {
                SetInfo(this, columnName, DataType.Double);
            }
            else if (columnType.ToUpper(CultureInfo.InvariantCulture) == "TABLE" || columnType.ToUpper(CultureInfo.InvariantCulture) == "SUBTABLE")
            {
                SetInfo(this, columnName, DataType.Table);
                //       throw new TableException("Subtables should be specified as an array, cannot create a freestanding subtable field");
            }
            else
                throw new TableException(String.Format(CultureInfo.InvariantCulture, "Trying to initialize a tablefield with an unknown type specification Fieldname:{0}  type:{1}", columnName, columnType));
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
