
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

//using System.Threading.Tasks; not portable as of 2013-04-02

//Tell compiler to give warnings if we publicise interfaces that are not defined in the cls standard
//http://msdn.microsoft.com/en-us/library/bhc3fa7f.aspx

[assembly: CLSCompliant(true)]

//Table class. The class represents a tightdb table.
//implements idisposable - will clean itself up (and any c++ resources it uses) when garbage collected
//If You plan to save resources, You can use it with the using syntax.

//[assembly: InternalsVisibleTo("Test")]

namespace TightDbCSharp
{


  
  
    /// <summary>
    /// Represents a TightDb Table, or a TightDb Sub-Table.
    /// Use Add(fieldvalue,fieldvalue,fieldvalue) to add an entire row, 
    /// AddEmptyRow() then SetString(row,col), SetLong(row,col) etc. to add a row field by field.
    /// </summary>
    public class Table : TableOrView, ICloneable,
        IEnumerable<Row>
        
    {

     
        
        /// <summary>
        /// Construct an empty Table with no columns
        /// </summary>
        public Table()
        {
            try
            {
                TableNew(false);//default is not readonly
            }
            catch (Exception)
            {
                Dispose();//this is proof that dispose is always called if the constructor throws
                throw;
            }
        }
        
        //This is used when we want to create a table and we already have the c++ handle that the table should use.  used by GetSubTable
        internal Table(TableHandle tableHandle,bool isReadOnly)//not 100% sure IsReadOnly should be a parameter here
        {
            try
            {
                SetHandle(tableHandle, isReadOnly);
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
        }

        internal TableHandle TableHandle {get { return Handle as TableHandle; }}//returns inherited TightdbHandle Handle as the correct subtype TableHandle

        /// <summary>
        /// implements ICloneable - this method is called Copy in the c++ binding        
        /// </summary>
        /// <returns>A new Table that is a deep copy of this one, structure as well as data</returns>
        public Table Clone()
        {
            try
            {
                ValidateIsValid();
                //a copy of a ReadOnly table is not readonly. It also does not belong to any group the source might belong to
                //so to do the clone we call tablecopytable with ourself and use the resulting new tablehandle to create a wrapper
                return new Table(TableHandle.TableCopyTable(), false);
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
        }


        object ICloneable.Clone()
        {
            return Clone();
        }


        /// <summary>
        /// If this method returns true, it is safe to call other methods on this table
        /// A table becomes non-valid if it is changed via other tables than itself, or
        /// if it is contained in a row in a table, and that table changes, or if the
        /// table is deleted from a group etc. All user-facing operations call
        /// IsValid to check if the table is still valid.
        /// Therefore users do not have to check IsValid, there will automatically be raised
        /// exceptions in the case a table is used when it is in InValid state
        /// </summary>
        /// <returns>True if The Table is usable</returns>
        public bool IsValid()
        {
            //this could be a virtual method in tableorview and be overridden in table and in tableview, but i fear there would be
            //a speed penalty as IsValid is called very often. todo:Measure performance penalty of making IsValid a TableOrView virtual method
            //todo:we might want to test HandleIsValid here. I have found no way that we could ever have a Table with an invalid handle,
            //so until i find a way that could happen i skip the test due to performance considerations
            //TightdbHandle can only go invalid if the handle creation succeeds, but the call to core returns a 0 or a -1 instead of a handle
            //and if *that* happens, we throw an exception, so the Table object is not even instantiated
            return UnsafeNativeMethods.TableIsAttached(this);//the first part ensures that the second part is only called on a valid handle
        }

        /// <summary>
        /// A Table with a shared spec is a table that is inside a row of another table
        /// Shared spec means that the table schema is defined in its "root" table, and
        /// not in the table itself. It also means that every row of the root table have
        /// one of this table, and all these tables share the same column specification
        /// The column layout of Shared Spec tables cannot be changed directly, they
        /// must be changed by calling modifying operations on the root table.
        /// A Root table can be a freestanding table, or a table inside a mixed column
        /// </summary>
        /// <returns>True if this is not a root table</returns>
        public bool HasSharedSpec()
        {            
            ValidateIsValid();
            return HasSharedSpecNoCheck();
        }

        private bool HasSharedSpecNoCheck()
        {
            return UnsafeNativeMethods.TableHasSharedSpec(this);            
        }

        
        /// <summary>
        /// Return a TableRow cursor for the row at the specified index
        /// The tablerow cursor will have methods for accessing individual fields
        /// This is the same class that is returned in foreach statements and LinQ
        /// statements
        /// </summary>
        /// <param name="rowIndex">Zero based index of the row to return</param>
        public Row this[long rowIndex]
        {
            get
            {
                ValidateIsValid();
                ValidateRowIndex(rowIndex);
                return RowForIndexNoCheck(rowIndex);
            }
        }

        //resembling the typed back() method or the untyped last method in python (that returns a cursor object)
        //see similar implementation in TableView 
        /// <summary>
        /// Returns the last row in the table, 
        /// the row with the highest rowindex
        /// </summary>
        /// <returns>TableRow cursor representing the last row in the table</returns>
        /// <exception cref="InvalidOperationException">If the table is no longer valid</exception>
        public Row Last()
        {
            ValidateIsValid();
            var s = Size;
            if (s > 0)
            {
                return RowForIndexNoCheck(s - 1);
            }
            throw new InvalidOperationException("Last called on a Table with no rows in it");
        }
       

        private Row RowForIndexNoCheck(long rowIndex)
        {
            return new Row(this, rowIndex);
        }

        


        //the following code enables Table to be enumerated, and makes TableRow the type You get back from an enummeration
        /// <summary>
        /// Returns an enumerator that iterates through the table, returning TableRow objects representing each row in the table.
        /// </summary>
        /// <returns>
        /// IEnumerator that yields TableRow objects repesenting the rows in the table
        /// </returns>
        
        public IEnumerator<Row> GetEnumerator() { ValidateIsValid(); return new Enumerator(this); }
        IEnumerator IEnumerable.GetEnumerator() { return new Enumerator(this); }

        class Enumerator : IEnumerator<Row>//probably overkill, current needs could be met by using yield


        {
            long _currentRow = -1;
            Table _myTable;
            private readonly int _myTableVersion;            
            public Enumerator(Table table)
            {
                _myTable = table;
                _myTableVersion = table.Version;
            }

            //todo:peformance test if inlining this manually will do any good
            private void ValidateVersion()
            {
                if(_myTableVersion != _myTable.Version)                
                  throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,"Table Iteration failed at row {0} because the table had rows inserted or deleted", _currentRow));
            }
            //as per msft guidelines, current does not throw an error even if the iterator is invalidated
            //however, here we DO throw an error as current would otherwise return a TableRow with a potentially illegal rowIndex inside
            //note we return TableRow from tables (they derive from Row). TableView returns Row

            public Row Current 

            {
                get
                {
                    ValidateVersion();
                    return new Row(_myTable, _currentRow);                 
                } 
            }

            object IEnumerator.Current { get { return Current; } }

            public bool MoveNext()
            {
                ValidateVersion();
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
        /// <summary>
        /// Crate a Table with the specified columns
        /// </summary>
        /// <param name="schema">List of specifications of individual columns</param>
        /// <exception cref="ArgumentNullException"></exception>
        public Table(params ColumnSpec[] schema)
        {
            try
            {
                if (schema == null)
                {
                    throw new ArgumentNullException("schema");
                }
                TableNew(false);//not readonly
                DefineSchema(schema);
            }
            catch (Exception)
                //to play safe reg CA2000 warning. Not that it goes away but now we know for sure that we never ever
                //can be in a situation where the constructor leaves with an exception and does not call dispose.
            {
                Dispose();
                throw;
            }
        }

        
        /// <summary>
        /// Quick way to create a Table with only one column.
        /// </summary>
        /// <param name="schema">One column definition. e.g. var t=New Table(New IntColumn("I1"));</param>
        public Table(ColumnSpec schema)
        {
            try
            {
                TableNew(false); //allocate a readwrite table class in c++
                //Spec spec = GetSpec();//get a handle to the table's new empty spec
                DefineSchema(schema);
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
        }

        internal Table DefineSchema(params ColumnSpec[] schema)//could be called by a constructor
        {
            ValidateReadWrite();        
            if (schema == null)
            {
                throw new ArgumentNullException("schema");
            }
            ValidateColumnChangeIsOkay();
            foreach (var columnSpec in schema)
            {
                if (columnSpec == null)
                {
                    throw new ArgumentNullException("schema", "one or more of the column specification objects are null");
                }
                AddColumnNoCheck(new List<long>(),columnSpec );
            }            
            return this;//allow fluent creation of table in a group
        }

        //this method must be called by someone who already has called ValidateIsValid
        //attempt to do a non-spec based AddField
        //this addfield must be called on the top table, but bc of the path parameter, it can add columns to any subtable recursively
        //should not really be called NoCheck as it ought to check a lot, and in fact it does (AddSubColumn does)
        private void AddColumnNoCheck(List<long> path, ColumnSpec schema)
        {
            if (schema != null)
            {
                if (schema.FieldType != DataType.Table)
                {
                    AddColumnNoValidCheck(path,schema.FieldType, schema.ColumnName);
                }
                else
                {
                    ColumnSpec[] tfa = schema.GetSubTableArray();
                    var columnNumber = AddColumnNoValidCheck(path, DataType.Table, schema.ColumnName);
                    path.Add((int)columnNumber);//limits number of columns to IntPtr - 2^32 ALSO on 64 bit machines, where core supports 2^64
                    AddColumnsNoCheck(path,tfa);
                }
            }
            else
            {
                throw new ArgumentNullException("schema");
            }
        }


        // will add the field list to the current spec
        private void AddColumnsNoCheck(List<long> path, IEnumerable<ColumnSpec> columnSpecs)
        {
            if (columnSpecs != null)
            {
                foreach (var columnSpec in columnSpecs)
                {
                    AddColumnNoCheck(path,columnSpec);
                }
            }
            else
            {
                throw new ArgumentNullException("columnSpecs");
            }
        }


        //this method is intended to be called on a table with no data in it.
        //the method will create columns in the table, matching the specified schema.        
        //currently the demands on the table are the same as with updatefromspec - but they will probably be lifted
        //note that if there are existing columns in the table - the path must reflect this. Adding to subtable in column 4 will need a path starting with 3
        //fields with no path will be added after the current fields. If You then add stuff into these newly added fields, remember that the path
        //must count all fields, old as well as new.
        private void DefineSchema(ColumnSpec schema)
        {
            ValidateReadWrite();
            ValidateColumnChangeIsOkay();//ensure this is the top table (not shared spec) and that it is with no rows
            //Spec.AddFieldNoCheck(schema);
            AddColumnNoCheck(new List<long>(),schema );//pass empty list -  this table is validated to be the top table
            //UpdateFromSpecNoCheck();//build table from the spec tree structure            
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

        
        




        //not accessible by source not in the TightDBCSharp namespace
        //TableHandle contains the value of a C++ pointer to a C++ table
        //it is sent as a parameter to calls to the C++ DLL.

        private void TableNew(bool isReadOnly)
        {
           UnsafeNativeMethods.TableNew(this,isReadOnly);        
        }

        /// <summary>
        /// Change the name of a column
        /// </summary>
        /// <param name="columnIndex">Index of column to rename</param>
        /// <param name="newName">New name of column to rename</param>
        public void RenameColumn(long columnIndex, String newName)
        {            
            ValidateReadWrite();
            ValidateColumnIndex(columnIndex);
            ValidateNotSharedSpec();
            UnsafeNativeMethods.TableRenameColumn(this,columnIndex,newName);
        }

        /// <summary>
        /// Remove a column from a table.
        /// The columns with higher indexes than the one being removed will have
        /// their index reduced by one
        /// </summary>
        /// <param name="columnIndex">Index of column to remove</param>
        public void RemoveColumn(long columnIndex)
        {
            ValidateReadWrite();
            ValidateColumnIndex(columnIndex);
            UnsafeNativeMethods.TableRemoveColumn(this,columnIndex);
            ++Version;
        }
        
        internal override Spec GetSpec()
        {
            return new Spec(this,TableHandle.GetSpec());//this spec should NOT be deallocated after use 
        }

        private void ValidateColumnChangeIsOkay()
        {                       
            ValidateNotSharedSpec();
        }


        internal override void ValidateIsValid()
        {
            if (! IsValid())
            {
                throw new InvalidOperationException("Table accessor is no longer valid. No operations except calling Is Valid is allowed");
            }
        }

        internal override void ClearNoCheck()
        {
            UnsafeNativeMethods.TableClear(this);
        }


        internal override DataType ColumnTypeNoCheck(long columnIndex)
        {
            return UnsafeNativeMethods.TableGetColumnType(this, columnIndex);
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

        internal override DateTime MinimumDateTimeNoCheck(long columnIndex)
        {
            return UnsafeNativeMethods.TableMinimumDateTime(this, columnIndex);
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

        internal override DateTime MaximumDateTimeNoCheck(long columnIndex)
        {
            return UnsafeNativeMethods.TableMaximumDateTime(this, columnIndex);
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

        /// <summary>
        /// Optimizes string columns in the table
        /// </summary>
        public void Optimize()
        {
            ValidateIsValid();
            ValidateReadWrite();//optimize changes internal table structures so cannot be done unless readwrite access
            UnsafeNativeMethods.TableOptimize(this);
        }

        internal override string ToStringNoCheck()
        {
            return UnsafeNativeMethods.TableToString(this);
        }

        internal override string ToStringNoCheck(long limit)
        {
            return UnsafeNativeMethods.TableToString(this,limit);
        }

        internal override string RowToStringNoCheck(long rowIndex)
        {
            return UnsafeNativeMethods.TableRowToString(this, rowIndex);
        }


        internal override string ToJsonNoCheck()
        {
            ValidateIsValid();
            return UnsafeNativeMethods.TableToJson(this);
        }

        internal override void RemoveNoCheck(long rowIndex)
        {
            UnsafeNativeMethods.TableRemove(this,rowIndex);            
            ++Version;
        }

        internal override long GetColumnCount()
        {
            return UnsafeNativeMethods.TableGetColumnCount(this);
        }

        //only call this if You are sure that IsValid will return true
        private void ValidateNotSharedSpec()
        {
            if (HasSharedSpecNoCheck())
            {
                throw new InvalidOperationException("It is illegal to alter the column structure of sub tables that has been read in from a table row");
            }
        }

        /// <summary>
        /// add a column to this table.
        /// The table must not be a subtable, but can be a table in a mixed field.
        /// </summary>
        /// <param name="type">Type of column to add</param>
        /// <param name="name">Name of column to add</param>
        /// <returns>Index of newly added column</returns>
        public long AddColumn(DataType type, String name)
        {
            ValidateIsValid();
            return AddColumnNoValidCheck(type, name);
        }

        private long AddColumnNoValidCheck(DataType type, String name)
        {            
            ValidateNotSharedSpec();
            ValidateReadWrite();
            long colIx = UnsafeNativeMethods.TableAddColumn(this, type, name);
            ++Version;
            return colIx;                         
        }

        /// <summary>
        /// Add an DataType.Binary column to this table.
        /// </summary>        
        /// <param name="name">Name of the new column</param>
        /// <returns>Index of the column in the target table</returns>

        public long AddBinaryColumn(String name)
        {            
            return AddColumn(DataType.Binary, name);
        }

        /// <summary>
        /// Add an DataType.Binary column to this table.
        /// Path indicates the exact location of the column that
        /// represents the table that we should add a column to.
        /// If path is empty a column is added to this table.
        /// first long is a column index into the root table
        /// (the callee) second long is a column index inside 
        /// the subtable the first index pointed to, etc.
        /// </summary>
        /// <param name="path">path to the subtable where we should add a column</param>
        /// <param name="name">Name of the newly added column</param>
        /// <returns>column index of newly added column</returns>
        public long AddBinaryColumn(IList<long>path,String name)
        {
            return AddColumn(path,DataType.Binary, name);
        }

        /// <summary>
        /// Add an DataType.Bool column to this table.
        /// </summary>        
        /// <param name="name">Name of the new column</param>
        /// <returns>Index of the column in the target table</returns>

        public long AddBoolColumn(String name)
        {
            return AddColumn(DataType.Bool, name);
        }

        
        /// <summary>
        /// Add an DataType.Bool column to this table.
        /// Path indicates the exact location of the column that
        /// represents the table that we should add a column to.
        /// If path is empty a column is added to this table.
        /// first long is a column index into the root table
        /// (the callee) second long is a column index inside 
        /// the subtable the first index pointed to, etc.
        /// </summary>
        /// <param name="path">path to the subtable where we should add a column</param>
        /// <param name="name">Name of the newly added column</param>
        /// <returns>column index of newly added column</returns>

        public long AddBoolColumn(IList<long> path,String name)
        {
            return AddColumn(path, DataType.Bool, name);
        }

        /// <summary>
        /// Add an DataType.Date column to this table.
        /// </summary>        
        /// <param name="name">Name of the new column</param>
        /// <returns>Index of the column in the target table</returns>

        public long AddDateColumn(String name)
        {
            return AddColumn(DataType.Date, name);
        }

        /// <summary>
        /// Add an DataType.Date column to this table.
        /// Path indicates the exact location of the column that
        /// represents the table that we should add a column to.
        /// If path is empty a column is added to this table.
        /// first long is a column index into the root table
        /// (the callee) second long is a column index inside 
        /// the subtable the first index pointed to, etc.
        /// </summary>
        /// <param name="path">path to the subtable where we should add a column</param>
        /// <param name="name">Name of the newly added column</param>
        /// <returns>column index of newly added column</returns>

        public long AddDateColumn(IList<long> path,String name)
        {
            return AddColumn(path,DataType.Date, name);
        }

        /// <summary>
        /// Add an DataType.Double column to this table.
        /// </summary>        
        /// <param name="name">Name of the new column</param>
        /// <returns>Index of the column in the target table</returns>

        public long AddDoubleColumn(String name)
        {
            return AddColumn(DataType.Double, name);
        }
        /// <summary>
        /// Add an DataType.Double column to this table.
        /// Path indicates the exact location of the column that
        /// represents the table that we should add a column to.
        /// If path is empty a column is added to this table.
        /// first long is a column index into the root table
        /// (the callee) second long is a column index inside 
        /// the subtable the first index pointed to, etc.
        /// </summary>
        /// <param name="path">path to the subtable where we should add a column</param>
        /// <param name="name">Name of the newly added column</param>
        /// <returns>column index of newly added column</returns>

        public long AddDoubleColumn(IList<long> path, String name)
        {
            return AddColumn(path,DataType.Double, name);
        }

        /// <summary>
        /// Add an DataType.Float column to this table.
        /// </summary>        
        /// <param name="name">Name of the new column</param>
        /// <returns>Index of the column in the target table</returns>

        public long AddFloatColumn(String name)
        {
            return AddColumn(DataType.Float, name);
        }


        /// <summary>
        /// Add an DataType.Float column to this table.
        /// Path indicates the exact location of the column that
        /// represents the table that we should add a column to.
        /// If path is empty a column is added to this table.
        /// first long is a column index into the root table
        /// (the callee) second long is a column index inside 
        /// the subtable the first index pointed to, etc.
        /// </summary>
        /// <param name="path">path to the subtable where we should add a column</param>
        /// <param name="name">Name of the newly added column</param>
        /// <returns>column index of newly added column</returns>

        public long AddFloatColumn(IList<long> path,String name)
        {
            return AddColumn(path,DataType.Float, name);
        }

        /// <summary>
        /// Add an DataType.Int column to this table.
        /// </summary>        
        /// <param name="name">Name of the new column</param>
        /// <returns>Index of the column in the target table</returns>

        public long AddIntColumn(String name)
        {
            return AddColumn(DataType.Int, name);
        }

        /// <summary>
        /// Add an DataType.Int column to this table.
        /// Path indicates the exact location of the column that
        /// represents the table that we should add a column to.
        /// If path is empty a column is added to this table.
        /// first long is a column index into the root table
        /// (the callee) second long is a column index inside 
        /// the subtable the first index pointed to, etc.
        /// </summary>
        /// <param name="path">path to the subtable where we should add a column</param>
        /// <param name="name">Name of the newly added column</param>
        /// <returns>column index of newly added column</returns>
        public long AddIntColumn(IList<long> path, String name)
        {
            return AddColumn(path, DataType.Int, name);
        }

        /// <summary>
        /// Add an DataType.Mixed column to this table.
        /// </summary>        
        /// <param name="name">Name of the new column</param>
        /// <returns>Index of the column in the target table</returns>

        public long AddMixedColumn(String name)
        {
            return AddColumn(DataType.Mixed, name);
        }

        /// <summary>
        /// Add an DataType.Mixed column to this table.
        /// Path indicates the exact location of the column that
        /// represents the table that we should add a column to.
        /// If path is empty a column is added to this table.
        /// first long is a column index into the root table
        /// (the callee) second long is a column index inside 
        /// the subtable the first index pointed to, etc.
        /// </summary>
        /// <param name="path">path to the subtable where we should add a column</param>
        /// <param name="name">Name of the newly added column</param>
        /// <returns>column index of newly added column</returns>

        public long AddMixedColumn(IList<long> path,String name)
        {
            return AddColumn(path,DataType.Mixed, name);
        }

        /// <summary>
        /// Add an DataType.String column to this table.
        /// </summary>        
        /// <param name="name">Name of the new column</param>
        /// <returns>Index of the column in the target table</returns>

        public long AddStringColumn(String name)
        {
            return AddColumn(DataType.String, name);
        }

        /// <summary>
        /// Add an DataType.String column to this table.
        /// Path indicates the exact location of the column that
        /// represents the table that we should add a column to.
        /// If path is empty a column is added to this table.
        /// first long is a column index into the root table
        /// (the callee) second long is a column index inside 
        /// the subtable the first index pointed to, etc.
        /// </summary>
        /// <param name="path">path to the subtable where we should add a column</param>
        /// <param name="name">Name of the newly added column</param>
        /// <returns>column index of newly added column</returns>

        public long AddStringColumn(IList<long> path,String name)
        {
            return AddColumn(path,DataType.String, name);
        }

        
        /// <summary>
        /// Adds a subtable column to this table
        /// returns a path that can be used to call AddColumn to add columns to the subtable
        /// </summary>
        /// <param name="name">Name of subtable column</param>
        /// <returns>Path to the newly created subtable column (to be used with e.g. AddStringColumn(path,name))</returns>
        public List<long> AddSubTableColumn(String name)
        {
            return new List<long> {AddColumn(DataType.Table, name)};
        }

        
        /// <summary>
        /// Adds a subtable column to a subtable in this table
        /// returns a path that can be used to call AddColumn to add columns to the subtable
        /// </summary>
        /// <param name="path">path to the subtable where a subtable should be added</param>
        /// <param name="name">Name of the new subtable column</param>
        /// <returns>Path to the newly created subtable column</returns>
        public List<long> AddSubTableColumn(IList<long> path, String name)
        {
            var newpath = new List<long>(path) {AddColumn(path, DataType.Table, name)};//this adds the index of the column to a copy of the path we got down, in effect creating a new path that points to the subtable, ready for adding more columns
            return newpath;
        }

        private Boolean HasIndexNoCheck(long columnIndex )
        {
            
            return UnsafeNativeMethods.TableHasIndex(this,columnIndex);
        }

        /// <summary>
        /// only legal to call this with a string column index
        /// however, later on the other columns will also have indicies and then it will
        /// be legal to call with more types, eventually all types
        /// right now, just return false for non-string columns, and otherwise
        /// ask core if there is an index
        /// </summary>
        /// <param name="columnIndex">Index of the column to check for index</param>
        /// <returns>True if column at columnIndex is indexed. False if no index exists or if column is not of type String</returns>
        public Boolean HasIndex(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            if (ColumnType(columnIndex) == DataType.String)
            {
                return HasIndexNoCheck(columnIndex);                
            }
            return false;
        }

        /// <summary>
        /// only legal to call this with a string column index
        /// however, later on the other columns will also have indicies and then it will
        /// be legal to call with more types, eventually all types
        /// right now, just return false for non-string columns, and otherwise
        /// ask core if there is an index
        /// </summary>
        /// <param name="columnName">Name of the column to check for index</param>
        /// <returns>True if column specified is indexed. False if no index exists or if column is not of type String</returns>
        public Boolean HasIndex(String columnName)
        {            
            var columnIndex = GetColumnIndex(columnName);//will call isvalid
            return ColumnType(columnIndex)==DataType.String && HasIndexNoCheck (columnIndex);
        }

        //expects the columnIndex to already have been validated as legal and type string
        private void ValidateMustHaveIndexNoColCheck(long columnIndex)
        {            
            if (!HasIndexNoCheck(columnIndex))
            {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,"Column Index {0} must specify a string column that is indexed", columnIndex));
            }
        }

        //will throw exception if an object in arr is not row-compatible with the table - but rows up to that
        //point will have been added. Put the call in a transaction and rollback if you get an exception
        //used internally to fill a mixed subtalbe from data
        /// <summary>
        /// Add many rows to a table in one go.
        /// </summary>
        /// <param name="rows">IEnumerable containing objects that can again be interpreted as row data</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddMany(IEnumerable rows)
        {
            if (rows == null)
            {
                throw new ArgumentNullException("rows","AddMany cannot be called wih null - it needs an IEnumerable object");
            }
            foreach (var row in rows )
            {
                Add(row);//add will validate readonly for us
            }
        }

        //adds an empty row at the end and filles it out
        //idea:consider if we should use insert row instead? - ask what's the difference, if any (asana)
        //todo:insert should only be used by the binding, not be exposed to the user (asana)
        //idea:when we add an entire row, insert is faster. (asana)
        /// <summary>
        /// Add the specified data to a new row in the table.
        /// The parameter list should be one object per field, and the objects should each.
        /// match the DataType of the fields.
        /// Subtables can be represented by NULL, Table or something IEnumerable that can.
        /// be evaluated as a sequence of rows of sequence of field values.
        /// </summary>
        /// <param name="rowData">array of objects with field data</param>
        /// <returns>zero based row index of the newly added row</returns>
        public long Add(params object[] rowData)
        {
            ValidateIsValid();
            ValidateReadWrite();
            long rowAdded = AddEmptyRow(1);
            SetRowNoCheck(rowAdded, rowData);//because the only thing that could be checked at this level is the row number.
            return rowAdded;//return the index of the just added row
        }

        /// <summary>
        /// put rowData into row specified by rowindex
        /// </summary>
        /// <param name="rowIndex">zero based index of row to change</param>
        /// <param name="rowData">array of objects, one for each field in row in table</param>
        public void Set(long rowIndex, params object[] rowData)
        {
            ValidateIsValid();
            ValidateReadWrite();
            ValidateRowIndex(rowIndex);
            SetRowNoCheck(rowIndex,rowData);
        }


        private void ValidateInsertRowIndex(long rowIndex)
        {
                        if (rowIndex < 0 || rowIndex > Size)
            {
                throw new ArgumentOutOfRangeException("rowIndex",String.Format(CultureInfo.InvariantCulture,"Table.Insert - row Index out of range. Table size {0}  index specified {1}",Size,rowIndex));
            }                

        }
        /// <summary>
        /// insert rowdata into a new row that is inserted at rowIndex
        /// rowindex=0 means insert into very first record
        /// rowindex=size means add after last
        /// All data at rowIndex and higher will be moved 1 up.
        /// Table size will be 1 higher.     
        /// </summary>
        /// <param name="rowIndex">Zero based index of row where new data should be placed</param>
        /// <param name="rowData">One object per column, with data matching the table structure</param>
        public void Insert(long rowIndex, params object[] rowData)
        {
            ValidateIsValid();
            ValidateReadWrite();
            ValidateInsertRowIndex(rowIndex);
            UnsafeNativeMethods.TableInsertEmptyRow(this,rowIndex,1);
            ++Version;
            SetRowNoCheck(rowIndex,rowData);
        }

        internal override string GetColumnNameNoCheck(long columnIndex)//unfortunately an int, bc tight might have been built using 32 bits
        {
            return UnsafeNativeMethods.TableGetColumnName(this, columnIndex);
        }

        
        /// <summary>
        /// add empty row(s) at the end, return the index
        /// </summary>
        /// <param name="numberOfRows">How many empty rows to add</param>
        /// <returns>Zero based row Index of last row added</returns>
        public long AddEmptyRow(long numberOfRows)
        {
            ValidateIsValid();
            ValidateReadWrite();
            ++Version;
            return UnsafeNativeMethods.TableAddEmptyRow(this, numberOfRows);            
        }

        internal override byte[] GetBinaryNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableGetBinary(this,columnIndex,rowIndex);
        }

        internal override Table GetSubTableNoCheck(long columnIndex, long rowIndex)
        {
            return new Table(TableHandle.TableGetSubTable(columnIndex,rowIndex),ReadOnly);
        }

        internal override long GetSubTableSizeNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableGetSubTableSize(this, columnIndex, rowIndex);
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

        /// <summary>
        /// Will add the specicifed integer value to all fields in the specified column
        /// If the sum exceeds Int64.MaxSize the result is unspecified, no errror is returned
        /// Currently, this method does not exist in TableView        
        /// </summary>
        /// <param name="columnIndex">Index of column whose DataType.Int values should be increased</param>
        /// <param name="value">increment, value to be added to each field</param>
        public void AddInt(long columnIndex, long value)
        {
            ValidateReadWrite();
            ValidateColumnIndex(columnIndex);
            ValidateTypeInt(columnIndex);
            UnsafeNativeMethods.TableAddInt(this,columnIndex,value);
        }


        /// <summary>
        /// Will add the specicifed integer value to all fields in the specified column
        /// If the sum exceeds Int64.MaxSize the result is unspecified, no errror is returned
        /// Currently, this method does not exist in TableView        
        /// </summary>
        /// <param name="columnName">Name of column whose DataType.Int values should be increased</param>
        /// <param name="value">increment, value to be added to each field</param>
        public void AddInt(string columnName, long value)
        {
            var columnIndex = GetColumnIndex(columnName);            
            ValidateTypeInt(columnIndex);
            UnsafeNativeMethods.TableAddInt(this, columnIndex, value);
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
            return new Table(TableHandle.TableGetSubTable(columnIndex,rowIndex),ReadOnly);//assuming that GetSubtable is valid to call on a mixed cell with a subtable in it
        }

        /*
        //warning! Use only this one when inserting new rows that are not inserted yet
        //todo:implement the rest of the insert api for inserting - but only for internal use
        //fixme:Resharper rightly marks this one down as never used
        private void InsertInt(long columnIndex, long rowIndex, long value)
        {
            UnsafeNativeMethods.TableInsertInt(this, columnIndex, rowIndex, value);            
        }
        */

        /// <summary>
        /// Insert row(s) at index rowIndex.
        /// data at rowIndex will be moved row(s) up
        /// </summary>
        /// <param name="rowIndex">Zero based row of first row that is moved and cleared</param>
        /// <param name="rowsToInsert">Number of rows to make space for</param>
        public void InsertEmptyRow(long rowIndex, long rowsToInsert)
        {
            ValidateIsValid();
            ValidateReadWrite();
            ValidateInsertRowIndex(rowIndex);
            UnsafeNativeMethods.TableInsertEmptyRow(this, rowIndex, rowsToInsert);
            ++Version;
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

        
        internal override void SetIntNoCheck(long columnIndex, long rowIndex, int value)
        {
            UnsafeNativeMethods.TableSetInt(this, columnIndex, rowIndex, value);
        }

        internal override void SetMixedLongNoCheck(long columnIndex, long rowIndex, long value)
        {
            UnsafeNativeMethods.TableSetMixedLong(this, columnIndex, rowIndex, value);
        }

        internal override void SetMixedIntNoCheck(long columnIndex, long rowIndex, int value)
        {
            UnsafeNativeMethods.TableSetMixedInt(this, columnIndex, rowIndex, value);
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
        internal override void SetMixedSubTableNoCheck(long columnIndex, long rowIndex, Table source)
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
            return new TableView(this, TableHandle.TableDistinct(columnIndex));
        }

        /// <summary>
        /// In a indexed string column, returns tableview with all rows with unique strings in that column
        /// The rows of the first unique strings are returned if several strings are the same
        /// </summary>
        /// <param name="columnIndex">zero based index of string column that has an index, Distinct will operate on this column</param>
        /// <returns>TableView of distinct records from columnIndex</returns>
        public TableView Distinct(long columnIndex)
        {
            ValidateIsValid();
            ValidateColumnIndexAndTypeString(columnIndex);
            ValidateMustHaveIndexNoColCheck(columnIndex);
            return DistinctNoCheck(columnIndex);
        }

        /// <summary>
        /// In a indexed string column, returns tableview with all rows with unique strings in that column
        /// The rows of the first unique strings are returned if several strings are the same
        /// </summary>
        /// <param name="columnName">Name of string column that has an index, Distinct will operate on this column</param>
        /// <returns>TableView of distinct records from identified column</returns>
        public TableView Distinct(string columnName)
        {            
            long columnIndex = GetColumnIndex(columnName);//will call isvalid
            ValidateTypeString(columnIndex);
            ValidateMustHaveIndexNoColCheck(columnIndex);
            return DistinctNoCheck(columnIndex);
        }

        //note - SetIndex is only callable if the column is a string column
        private void SetIndexNoCheck(long columnIndex)
        {
            ValidateReadWrite();
            UnsafeNativeMethods.TableSetIndex(this, columnIndex);
        }

        /// <summary>
        /// Establish an index on the specified column.
        /// Column must be a string column
        /// </summary>
        /// <param name="columnIndex">Zero based index of string column that should be indexed</param>
        public void SetIndex(long columnIndex)
        {
            ValidateIsValid(); 
            ValidateNotSharedSpec();//core does not support indexes on colums in non-mixed subtables
            ValidateColumnIndexAndTypeString(columnIndex);            
            SetIndexNoCheck(columnIndex);
        }

        /// <summary>
        /// Establish an index on the specified column.
        /// Column must be a string column
        /// </summary>
        /// <param name="columnName">Zero based index of string column that should be indexed</param>
        public void SetIndex(string columnName)
        {
            long columnIndex = GetColumnIndex(columnName);//will call isvalid
            ValidateNotSharedSpec();//core does not support indexes on colums in non-mixed subtables
            ValidateTypeString(columnIndex);            
            SetIndexNoCheck(columnIndex);
        }

        
        private void ValidateColumnPath(IList<long> path,Boolean lastMustAlsoBeASubTable)//in 64 bit mode, artificially reducing the max. number of columns from 2^64 to 2^32, as the core uses size_t column indicies
        {
            if (path == null)
            {
                throw new ArgumentNullException("path","Column path must not be null");
            }

            //if path is empty it is very hard to argue it points to a subtable. However, recursive calls will often start with defining the top table
            //so allow an empty path to mean simply start up with adding columns to the top table
            
            if (path.Count == 0)
            {
                if (lastMustAlsoBeASubTable)//this means that the path points to a table, not a specific column so that's fine
                    return;
                //else throw an exception as the path is supposed to indentify a column (rename for instance) empty path does not specify any column
                    throw new ArgumentOutOfRangeException("path","subcolumn path is of length zero -must be at least of length one");
            }
            
            {
                var firstbelowzero=-1;
                for (var n = 0; n < path.Count; n++)
                {
                    if (path[n] < 0)
                    {
                        firstbelowzero = n;
                        break;//exit for loop
                    }
                }                

                if(firstbelowzero!=-1){
                throw new ArgumentOutOfRangeException("path", string.Format(CultureInfo.InvariantCulture,"path supplied contains a negative column number at index {0} with value {1}. Only non-negative indexes are allowed", firstbelowzero, path[firstbelowzero]));
                }
            }

         //for each level 
            //validate that the index into this level is valid (not too large, is a subtable (not a mixed subtable))
            //todo:this method awaits core support for getting information on subtable stuff. Until we get that, we use a temporary spec implementation.
            //todo:this could be implemented recursively and probably look a bit tidier
            Spec levelSpec = Spec;
            for (var level = 0; level < path.Count; level++)
            {

                if (level != 0)                
                {
                    Debug.Assert(levelSpec != null, "levelSpec != null");
                    Spec newLevelSpec = levelSpec.GetSpec(path[level - 1]);
                    //get the spec of the subtable this part of path is identifying by index
                    levelSpec = newLevelSpec;
                }

                //validate current path index is a legal column index

                if (levelSpec.ColumnCount <= path[level])
                {
                    throw new ArgumentOutOfRangeException(String.Format(CultureInfo.InvariantCulture,"at level : {0}, the path supplied contains too large an index : {1}. Number of columns : {2}",level, path[level], levelSpec.ColumnCount));
                }



                if (!lastMustAlsoBeASubTable && level == path.Count - 1)
                    return;//at last level, accept any type if that behavior is specified (used when path poits to an ordinary column)

                if (levelSpec.GetColumnType(path[level]) != DataType.Table)
                {
                    throw new ArgumentException(
                        String.Format(CultureInfo.InvariantCulture,
                            "at level {0}, the path supplied contains index {1} that points not to a SubTable column, but column \"{2}\" of type {3}",
                            level, path[level], levelSpec.GetColumnName(path[level]), levelSpec.GetColumnType(path[level])));
                }
            }
        }

        
        //returns the index of the added column in the subtable where it is placed (or in the top table where it is placed)
        private long AddColumnNoValidCheck(IList<long> path, DataType dataType, string columnName)
        {            
            if (columnName == null)
            {
                throw new ArgumentNullException("columnName","column name cannot be null");
            }
            ValidateIsEmpty();//cannot change scheme if there is data in the table
            ValidateNotSharedSpec();//You must alter shared spec tables through their top table
            ValidateColumnPath(path,true);
            ValidateReadWrite();
            return path.Count == 0 ? UnsafeNativeMethods.TableAddColumn(this, dataType, columnName) : UnsafeNativeMethods.TableAddSubColumn(this, path, dataType, columnName);
        }


        /// <summary>
        /// Add a column of specified type to the specified subtable with the specified name
        /// Usually the user will use type specific methods instead, like AddStringColumn
        /// This method is useful when creating tables dynamically where the type is not known at compile time
        /// </summary>
        /// <param name="path">path to a subtable</param>
        /// <param name="dataType">type of column to add</param>
        /// <param name="columnName">name of column to add</param>
        /// <returns>columnIndex of row added</returns>
        public long AddColumn(IList<long> path, DataType dataType, string columnName)
        {
            ValidateIsValid();
            ValidateReadWrite();
            return AddColumnNoValidCheck(path, dataType, columnName);
        }        


        //pathtosubtable contains the indicies to travese from the top table to the subtable, columnIndex is the index of the column You wish to change, newName is the new name
        /// <summary>
        /// Rename a column specified by path to its subtable and the column index
        /// </summary>
        /// <param name="pathToSubTable">Path to subtable where the column resides</param>
        /// <param name="columnIndex">Index of the column to rename</param>
        /// <param name="newName">New name</param>
        public void RenameColumn(IEnumerable<long> pathToSubTable, long columnIndex, string newName)
        {
            ValidateIsValid();            
            var newlist = new List<long>(pathToSubTable) {columnIndex};//newlist now points to the path to the subatble plus the index of the column we want to change
            RenameColumn(newlist,newName);
        }

       
        /// <summary>
        /// Rename the column pointed to by path, to the name specified.
        /// warning!! the path points directly to the column that you want to change - NOT to the subtable
        /// </summary>
        /// <param name="path">Path to the column, including the columns own index</param>
        /// <param name="name">New Name</param>
        public void RenameColumn(IList<long> path, string name)
        {
            ValidateIsValid();
            ValidateIsEmpty(); //cannot change scheme if there is data in the table
            ValidateNotSharedSpec(); //You must alter shared spec tables through their top table
            ValidateColumnPath(path, false);
            ValidateReadWrite();
            if (path.Count > 1) //this test bc tablerenamesubcolumn does not work with a path of length 1
            {
                UnsafeNativeMethods.TableRenameSubColumn(this, path, name);
            }
            else
                UnsafeNativeMethods.TableRenameColumn(this, path[0], name);
        }

        
        
        
        /// <summary>
        /// specify column by path to subtable and a column index
        /// path to subtable is usually gotten from AddSubTable(name)
        /// path to column index is usually gotten from GetColumnIndex(name)
        /// </summary>
        /// <param name="pathToSubTable">Path to subtable with column to remove</param>
        /// <param name="columnIndex">Zero based index of column to remove</param>
        public void RemoveColumn(IEnumerable<long> pathToSubTable, long columnIndex)
        {
            ValidateIsValid();
            var  pathToColumn=new List<long>(pathToSubTable){columnIndex};
            RemoveColumn(pathToColumn);
        }

        
        /// <summary>
        /// Remove column in subtable, or if path is only 1 long, in this table
        /// </summary>
        /// <param name="path">path to (subtable) column to remove</param>
        public void RemoveColumn(IList<long> path)
        {
            ValidateIsValid();
            ValidateIsEmpty(); //cannot change scheme if there is data in the table
            ValidateNotSharedSpec(); //You must alter shared spec tables through their top table
            ValidateColumnPath(path, false);
            ValidateReadWrite();
            if (path.Count > 1) //this test because TableRemoveSubcolumn does not accept a path with only one number
            {
                UnsafeNativeMethods.TableRemoveSubColumn(this, path);
            }
            else
            {
                UnsafeNativeMethods.TableRemoveColumn(this, path[0]);
            }
        }

        internal override TableView FindAllIntNoCheck(long columnIndex, long value)
        {
            return new TableView(this,TableHandle.TableFindAllInt(columnIndex,value));
        }

        internal override TableView FindAllBoolNoCheck(long columnIndex, bool value)
        {
            return  new TableView(this,TableHandle.TableFindAllBool(columnIndex,value));//TableFindAllBool returns a TableViewhandle, which is then put into the newly created tableview          
        }
              
        internal override TableView FindAllDateNoCheck(long columnIndex, DateTime value)
        {
            return new TableView(this, TableHandle.TableFindAllDateTime(columnIndex,value));
        }

        internal override TableView FindAllFloatNoCheck(long columnIndex, float value)
        {
            return new TableView(this, TableHandle.TableFindAllFloat(columnIndex, value));            
        }

        internal override TableView FindAllDoubleNoCheck(long columnIndex, double value)
        {
            return new TableView(this, TableHandle.TableFindAllDouble(columnIndex, value));
        }

        internal override TableView FindAllStringNoCheck(long columnIndex, string value)
        {
            return new TableView(this,TableHandle.TableFindAllString(columnIndex,value));
        }

        internal override TableView FindAllBinaryNoCheck(long columnIndex, byte[] value)
        {
            TableViewHandle tableViewHandle = TableHandle.TableFindAllBinary(columnIndex, value);
            if (!tableViewHandle.IsInvalid)            
                return new TableView(this, tableViewHandle);            
            throw new NotImplementedException("Table.FindAllBinary is not implemented in core yet - or did not return a valid TableView");            
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

        /// <summary>
        /// Return a query that references this table and all its records
        /// </summary>
        /// <returns>Query object referencing this table and all its records</returns>
        public Query Where()
        {
            ValidateIsValid();
            return new Query(TableHandle.TableWhere(),  this);
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


    /// <summary>
    /// This class specifies a column (type and name) and in case of a subtable column,
    /// it specifys the column layout of that subtable.
    /// The class is normally not used directly by the user, rather, the user instantiates
    /// subclasses like StringColumn() when creating a table structure. example table create code :
    /// var Person = new Table(
    /// new StringColumn("Address"),
    /// new StringColumn("Phone number"),
    /// new StringColumn("Country"),
    /// new IntColumn("Age"))
    /// </summary>
    public class ColumnSpec
    {
        

        /// <summary>
        /// Sets up a columnspec with a name and a type
        /// </summary>
        /// <param name="someColumnSpec">Column spec to set</param>
        /// <param name="someColumnName">Name of column</param>
        /// <param name="someFieldType">DataType of the column </param>
        /// <exception cref="ArgumentNullException"></exception>
        protected static void SetInfo(ColumnSpec someColumnSpec, String someColumnName, DataType someFieldType)
        {
            if (someColumnSpec != null)
            {
                someColumnSpec.ColumnName = someColumnName;
                someColumnSpec.FieldType = someFieldType;
            }
            else
                throw new ArgumentNullException("someColumnSpec");
        }

        //this is internal for a VERY specific reason
        //when internal, the end user cannot call this function, and thus cannot put in a list of subtable fields containing a field that is parent
        //to the somefield parameter. If this was merely protected - he could!
        internal static void AddSubTableFields(ColumnSpec someColumnSpec, String someColumnName, IEnumerable<ColumnSpec> subTableFieldsArray)
        {
            SetInfo(someColumnSpec, someColumnName, DataType.Table);
            someColumnSpec._subTable.AddRange(subTableFieldsArray);
        }

        /// <summary>
        /// Return a columnSpec representing a subtable and its columns
        /// </summary>
        /// <param name="someColumnName">Name of subtable column</param>
        /// <param name="subTableColumnsSpecArray">Columns of subtable</param>
        public ColumnSpec(string someColumnName, params ColumnSpec[] subTableColumnsSpecArray)
        {
            AddSubTableFields(this, someColumnName, subTableColumnsSpecArray);
        }

        /// <summary>
        /// Retrun columnspec with a column name and a given column type type
        /// </summary>
        /// <param name="columnName">Name of column to represent</param>
        /// <param name="columnType">type of column to represent</param>
        public ColumnSpec(string columnName, DataType columnType)
        {
            SetInfo(this, columnName, columnType);
        }

        /// <summary>
        /// Create a column given a string name and a string
        /// representation of the type to use
        /// </summary>
        /// <param name="columnName">Name of the column to craete</param>
        /// <param name="columnType">String representation of type of the column to create</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public ColumnSpec(string columnName, String columnType)
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

        /// <summary>
        /// Do not subclass and call unless You know what you are doing
        /// </summary>
        protected ColumnSpec() { }//used when IntegerField,StringField etc are constructed


        /// <summary>
        /// Name of the column to create
        /// </summary>
        public String ColumnName { get; private set; }

        /// <summary>
        /// Type of the field to create
        /// </summary>
        public DataType FieldType { get; set; }

        private readonly List<ColumnSpec> _subTable = new List<ColumnSpec>();//only used if type is a subtable

        /// <summary>
        /// Return the subtable specification as an array of ColumnSpec
        /// </summary>
        /// <returns>An array of ColumnSpec containing the subtable specification</returns>
        public ColumnSpec[] GetSubTableArray()
        {
            return _subTable.ToArray();
        }
    }

    /// <summary>
    /// Used in Table constructor
    /// represents a column of type DataType.Table
    /// </summary>

    public class SubTableColumn : ColumnSpec
    {
        /// <summary>
        /// Used in Table constructor
        /// Returns a schema object representing a subtable column with the specified column name
        /// and the specified columns. Use like this :
        /// new Table(new SubtableColumn("Sub",new StringField("substring")))
        /// </summary>
        /// <param name="columnName">Name of the DataType.String column to create</param>
        /// <param name="subTableColumnsSpecArray">array of column specifications</param>
        public SubTableColumn(string columnName, params ColumnSpec[] subTableColumnsSpecArray)
        {
            AddSubTableFields(this, columnName, subTableColumnsSpecArray);
        }
    }


    /// <summary>
    /// Used in Table constructor
    /// represents a column of type DataType.String
    /// </summary>

    public class StringColumn : ColumnSpec
    {
        /// <summary>
        /// Used in Table constructor
        /// Returns a schema object representing a string column with the specified column name
        /// </summary>
        /// <param name="columnName">Name of the DataType.String column to create</param>

        public StringColumn(String columnName)
        {
            SetInfo(this, columnName, DataType.String);
        }
    }

    /// <summary>
    /// Used in Table constructor
    /// represents a column of type DataType.Int
    /// </summary>

    public class IntColumn : ColumnSpec
    {
        /// <summary>
        /// Used in Table constructor
        /// Returns a schema object representing a int column with the specified column name
        /// </summary>
        /// <param name="columnName">Name of the DataType.int column to create</param>

        public IntColumn(String columnName)
        {
            SetInfo(this, columnName, DataType.Int);
        }
    }



    /// <summary>
    /// Used in Table constructor
    /// represents a column of type DataType.Int
    /// </summary>
    
    public class BoolColumn : ColumnSpec
    {
        /// <summary>
        /// Used in Table constructor
        /// Returns a schema object representing a boolean column with the specified column name
        /// </summary>
        /// <param name="columnName">Name of the DataType.Bool column to create</param>
        public BoolColumn(String columnName)
        {
            SetInfo(this, columnName, DataType.Bool);
        }
    }


    /// <summary>
    /// Used in Table constructor
    /// represents a column of type DataType.Binary
    /// </summary>

    public class BinaryColumn : ColumnSpec
    {

        /// <summary>
        /// Used in Table constructor
        /// Returns a schema object representing a binary column with the specified column name
        /// </summary>
        /// <param name="columnName">Name of the DataType.Binary column to create</param>
        public BinaryColumn(String columnName)
        {
            SetInfo(this, columnName, DataType.Binary);
        }
    }



    /// <summary>
    /// Used in Table constructor
    /// represents a column of type DataType.Mixed
    /// </summary>

    public class MixedColumn : ColumnSpec
    {
 
        /// <summary>
        /// Used in Table constructor
        /// Returns a schema object representing a Mixed column with the specified column name
        /// </summary>
        /// <param name="columnName">Name of the DataType.Mixed column to create</param>
        public MixedColumn(String columnName)
        {
            SetInfo(this, columnName, DataType.Mixed);
        }
    }

    /// <summary>
    /// Used in Table constructor
    /// represents a column of type DataType.Date
    /// </summary>

    public class DateColumn : ColumnSpec
    {

        /// <summary>
        /// Used in Table constructor
        /// Returns a schema object representing a Date (date_t) column with the specified column name
        /// </summary>
        /// <param name="columnName">Name of the DataType.Date column to create</param>
        public DateColumn(String columnName)
        {
            SetInfo(this, columnName, DataType.Date);
        }
    }

    /// <summary>
    /// Used in Table constructor
    /// Returns a schema object representing a float column with the specified column name
    /// </summary>

    public class FloatColumn : ColumnSpec
    {
        /// <summary>
        /// Used in Table constructor
        /// Returns a schema object representing a float column with the specified column name
        /// </summary>
        /// <param name="columnName">Name of the DataType.Float column to create</param>
        public FloatColumn(String columnName)
        {
            SetInfo(this, columnName, DataType.Float);
        }
    }


    /// <summary>
    /// Used in Table constructor
    /// Returns a schema object representing a double column with the specified column name
    /// </summary>
    public class DoubleColumn : ColumnSpec
    {


        /// <summary>
        /// Used in Table constructor
        /// Returns a schema object representing a Double column with the specified column name
        /// </summary>
        /// <param name="columnName">Name of the DataType.Double column to create</param>

        public DoubleColumn(String columnName)
        {
            SetInfo(this, columnName, DataType.Double);
        }
    }

    namespace Extensions
    {

        /// <summary>
        /// Extensions to string that allows an alternative syntax for creating tables :
        /// var table = new Table("FieldName".Int(),"SubTable".Table("SubField".String(),"Sub2Field".String()));
        /// The normal syntax is new Table(New IntField("FieldName")) etc..
        /// </summary>
        public static class TightDbExtensions
        {
            /// <summary>
            /// Returns a column spec for a DataType.Int field
            /// </summary>
            /// <param name="fieldName">Name of the column</param>
            /// <returns>object used in New Table call like this New Table("MyDateField".TightDbDate());</returns>

            public static ColumnSpec TightDbInt(this String fieldName)
            {
                return new ColumnSpec(fieldName, DataType.Int);
            }

            /// <summary>
            /// Returns a column spec for a DataType.Int field
            /// </summary>
            /// <param name="fieldName">Name of the column</param>
            /// <returns>object used in New Table call like this New Table("MyDateField".TightDbDate());</returns>

            public static ColumnSpec Int(this String fieldName)
            {
                return new ColumnSpec(fieldName, DataType.Int);
            }

            /// <summary>
            /// Returns a column spec for a DataType.Bool field
            /// </summary>
            /// <param name="fieldName">Name of the column</param>
            /// <returns>object used in New Table call like this New Table("MyDateField".TightDbDate());</returns>

            public static ColumnSpec Bool(this string fieldName)
            {
                return new ColumnSpec(fieldName, DataType.Bool);
            }

            /// <summary>
            /// Returns a column spec for a DataType.Bool field
            /// </summary>
            /// <param name="fieldName">Name of the column</param>
            /// <returns>object used in New Table call like this New Table("MyDateField".TightDbDate());</returns>

            public static ColumnSpec TightDbBool(this string fieldName)
            {
                return new ColumnSpec(fieldName, DataType.Bool);
            }

            /// <summary>
            /// Returns a column spec for a DataType.String field
            /// </summary>
            /// <param name="fieldName">Name of the column</param>
            /// <returns>object used in New Table call like this New Table("MyDateField".TightDbDate());</returns>

            public static ColumnSpec TightDbString(this String fieldName)
            {
                return new ColumnSpec(fieldName, DataType.String);
            }

            /// <summary>
            /// Returns a column spec for a DataType.String field
            /// </summary>
            /// <param name="fieldName">Name of the column</param>
            /// <returns>object used in New Table call like this New Table("MyDateField".TightDbDate());</returns>

            public static ColumnSpec String(this String fieldName)
            {
                return new ColumnSpec(fieldName, DataType.String);
            }

            /// <summary>
            /// Returns a column spec for a DataType.Binary field
            /// </summary>
            /// <param name="fieldName">Name of the column</param>
            /// <returns>object used in New Table call like this New Table("MyDateField".TightDbDate());</returns>

            public static ColumnSpec TightDbBinary(this String fieldName)
            {
                return new ColumnSpec(fieldName, DataType.Binary);
            }

            /// <summary>
            /// Returns a column spec for a DataType.Binary field
            /// </summary>
            /// <param name="fieldName">Name of the column</param>
            /// <returns>object used in New Table call like this New Table("MyDateField".TightDbDate());</returns>

            public static ColumnSpec Binary(this String fieldName)
            {
                return new ColumnSpec(fieldName, DataType.Binary);
            }

            /// <summary>
            /// Returns a column spec for a DataType.Table field. Use like this :
            /// new Table ("sub".Table("subField".String(),"subField2".Int()));
            /// </summary>
            /// <param name="fieldName">Name of the column</param>
            /// <param name="columnsSpec">collection of field objects specifying the subtable</param>
            /// <returns>object used in New Table call like this New Table("MyDateField".TightDbDate());</returns>
            public static ColumnSpec TightDbSubTable(this String fieldName, params ColumnSpec[] columnsSpec)
            {
                return new ColumnSpec(fieldName, columnsSpec);
            }

            /// <summary>
            /// Returns a column spec for a DataType.Table field. Use like this :
            /// new Table ("sub".Table("subField".String(),"subField2".Int()));
            /// </summary>
            /// <param name="fieldName">Name of the column</param>
            /// <param name="columnsSpec">collection of field objects specifying the subtable</param>
            /// <returns>object used in New Table call like this New Table("MyDateField".TightDbDate());</returns>
            public static ColumnSpec SubTable(this String fieldName, params ColumnSpec[] columnsSpec)
            {
                return new ColumnSpec(fieldName, columnsSpec);
            }

            //as the TightDb has a type called table, we also provide a such named constructor even though it will always be a subtable
            /// <summary>
            /// Returns a column spec for a DataType.Table field. Use like this :
            /// new Table ("sub".Table("subField".String(),"subField2".Int()));
            /// </summary>
            /// <param name="fieldName">Name of the column</param>
            /// <param name="columnsSpec">collection of field objects specifying the subtable</param>
            /// <returns>object used in New Table call like this New Table("MyDateField".TightDbDate());</returns>
            public static ColumnSpec Table(this String fieldName, params ColumnSpec[] columnsSpec)
            {
                return new ColumnSpec(fieldName, columnsSpec);
            }

            /// <summary>
            /// Returns a column spec for a DataType.Mixed field
            /// </summary>
            /// <param name="fieldName">Name of the column</param>
            /// <returns>object used in New Table call like this New Table("MyDateField".TightDbDate());</returns>
            public static ColumnSpec TightDbMixed(this String fieldName)
            {
                return new ColumnSpec(fieldName, DataType.Mixed);
            }

            /// <summary>
            /// Returns a column spec for a mixed field
            /// </summary>
            /// <param name="fieldName">Name of the column</param>
            /// <returns>object used in New Table call like this New Table("MyDateField".TightDbDate());</returns>

            public static ColumnSpec Mixed(this String fieldName)
            {
                return new ColumnSpec(fieldName, DataType.Mixed);
            }

            /// <summary>
            /// Returns a column spec for a date field
            /// </summary>
            /// <param name="fieldName">Name of the column</param>
            /// <returns>object used in New Table call like this New Table("MyDateField".TightDbDate());</returns>
            public static ColumnSpec Date(this String fieldName)
            {
                return new ColumnSpec(fieldName, DataType.Date);
            }

            /// <summary>
            /// Returns a column spec for a date field
            /// </summary>
            /// <param name="fieldName">Name of the column</param>
            /// <returns>object used in New Table call like this New Table("MyDateField".TightDbDate());</returns>
            public static ColumnSpec TightDbDate(this String fieldName)
            {
                return new ColumnSpec(fieldName, DataType.Date);
            }
            /// <summary>
            /// Returns a column spec for a DataType.Float field
            /// </summary>
            /// <param name="fieldName">Name of the column</param>
            /// <returns>object used in New Table call like this New Table("MyDateField".TightDbDate());</returns>

            public static ColumnSpec Float(this string fieldName)
            {
                return new ColumnSpec(fieldName, DataType.Float);
            }

            /// <summary>
            /// Returns a column spec for a DataType.Float field
            /// </summary>
            /// <param name="fieldName">Name of the column</param>
            /// <returns>object used in New Table call like this New Table("MyDateField".TightDbDate());</returns>

            public static ColumnSpec TightDbFloat(this string fieldName)
            {
                return new ColumnSpec(fieldName, DataType.Float);
            }

            /// <summary>
            /// Returns a column spec for a DataType.Double field
            /// </summary>
            /// <param name="fieldName">Name of the column</param>
            /// <returns>object used in New Table call like this New Table("MyDateField".TightDbDate());</returns>

            public static ColumnSpec Double(this string fieldName)
            {
                return new ColumnSpec(fieldName, DataType.Double);
            }

            /// <summary>
            /// Returns a column spec for a DataType.Double field
            /// </summary>
            /// <param name="fieldName">Name of the column</param>
            /// <returns>object used in New Table call like this New Table("MyDateField".TightDbDate());</returns>

            public static ColumnSpec TightDbDouble(this string fieldName)
            {
                return new ColumnSpec(fieldName, DataType.Double);
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
