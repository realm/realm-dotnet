using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace TightDbCSharp
{
    //read operations will always propegate down to a table or view so readonly checks are not done in row, they are expected to be done in the table or tableview classes

    
    /// <summary>
    /// Common class for rows in TableView and Table.  The TableRow class that adds table only specific methods inherits from Row and adds these special methods
    /// iterating a TableView gets You objects of type Row, while iterating a Table gets You classes of type TableRow (as these inherit from Row, they appear almost identical
    /// Both Row and TableRow returns RowColumn when iterated (column specific methods are the same for TableView and Table)
    /// </summary>    
    public class Row
    {
        internal Row(TableOrView owner,long row) {
            Owner=owner;
            var ownerAsTableView = owner as TableView;
            if (ownerAsTableView!=null)
            {
                _underlyingTable = ownerAsTableView.UnderlyingTable;
            }
            else
            {
                Debug.Assert(owner != null);//this only happens if owner is neither table nor tableview
                _underlyingTable = (Table)owner ;
            }
            _underlyingTableVersion = _underlyingTable.Version;
            
            // The Row number of the row this TableRow references
            RowIndex=row;
        }
        /// <summary>
        /// The tableview or table this row is pointing to with its RowIndex
        /// </summary>
        public TableOrView Owner { get; private set; }
        /// <summary>
        /// Zero based Index of the row in a table or tableview that this Row object is pointing to
        /// </summary>
        public long RowIndex { get; private set; }//users should not be allowed to change the row property of a tablerow class
        private readonly int _underlyingTableVersion;
        private readonly Table _underlyingTable;
        
        /// <summary>
        /// zero based field lookup. Returns a boxed value of the contents of the field specified by columnIndex
        /// </summary>
        /// <param name="columnIndex">Zero based index of the field value in this row, to return</param>
        public  object this[long columnIndex]
        {
            get
            {   Owner.ValidateColumnIndex(columnIndex);
                return Owner.GetValueNoCheck(columnIndex, RowIndex);
            }
            set
            {   
                Owner.ValidateColumnIndex(columnIndex);
                Owner.SetValueNoCheck(columnIndex,RowIndex,value);
            }
        }


        /// <summary>
        /// zero based field propery. sets or gets the value of a field in this row, identified with its column name
        /// </summary>
        /// <param name="columnName">Name of the column of the field whose value you want to set or get</param>

        public object this[string columnName]
        {
            get
            {
                var columnIndex = Owner.GetColumnIndex(columnName);                
                return Owner.GetValueNoCheck(columnIndex, RowIndex);
            }
            set
            {
                var columnIndex = Owner.GetColumnIndex(columnName);                
                Owner.SetValueNoCheck(columnIndex, RowIndex, value);
            }
        }



        //allow foreach to traverse a TableRow and get some TableRowCell objects
        //if You do a foreach on a TableRow, C# will use the for loop below to do the iteration
        /// <summary>
        /// Returns an enumerator that enumerates the row, yielding RowCell objects that
        /// in turn contain get and set methods, and a value property that gets or sets objects.
        /// In other words, You can enumerate a row and get the individual fields.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<RowCell> GetEnumerator()
        {
            for (long i = 0; i < ColumnCount; i++)
            {
                ValidateIsValid();
                yield return new RowCell(this, i);
            }
        }

        //todo:test if we become invalid if we change the underlying table through ourselves. We should not.
        /// <summary>
        /// returns false if the underlying table has been changed in other ways than through this tablerow.
        /// </summary>
        /// <returns></returns>
        public Boolean IsValid()
        {
            return (_underlyingTableVersion == _underlyingTable.Version);
        }

        private void ValidateIsValid()
        {
            if (!IsValid())
            {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,"Table row accessor with row index {0} used after the table had rows inserted or deleted", RowIndex));
            }
        }

        /// <summary>
        /// returns number of columns in the table this row is part of
        /// </summary>
        public long ColumnCount 
        {
            get { return Owner.ColumnCount; }
        }

        /// <summary>
        /// Set the float in the field specified by the row's rowindex and the specified zero based column index
        /// </summary>
        /// <param name="columnIndex">Zero based index of column of field to set</param>
        /// <param name="value">value to set</param>
        public void SetFloat(long columnIndex, float value)
        {
            ValidateIsValid();
            Owner.SetFloatNoRowCheck(columnIndex, RowIndex, value);
        }

        /// <summary>
        /// Set the double in the field specified by the row's rowindex and the specified zero based column index
        /// </summary>
        /// <param name="columnIndex">Zero based index of column of field to set</param>
        /// <param name="value">value to set</param>
        public void SetDouble(long columnIndex, double value)
        {
            ValidateIsValid();
            Owner.SetDoubleNoRowCheck(columnIndex, RowIndex, value);
        }

        /// <summary>
        /// Set the float in the field specified by the row's rowindex and the specified zero based column index
        /// </summary>
        /// <param name="columnName">Zero based index of column of field to set</param>
        /// <param name="value">value to set</param>
        /// 
        public void SetFloat(String  columnName, float value)
        {
            ValidateIsValid();
            Owner.SetFloatNoRowCheck(columnName, RowIndex, value);
        }


        /// <summary>
        /// Set the double in the Datatype.Double field specified by the row's rowindex.
        /// and the specified zero based column index.
        /// Field specified must be DataType.Double
        /// </summary>
        /// <param name="columnName">Name of the  column of field to set</param>
        /// <param name="value">value to set</param>
        public void SetDouble(String columnName, double value)
        {
            ValidateIsValid();
            Owner.SetDoubleNoRowCheck(columnName, RowIndex, value);
        }

        /// <summary>
        /// Set the long in the Datatype.Long field specified by the row's rowindex.
        /// and the specified zero based column index.
        /// Field specified must be DataType.int
        /// </summary>
        /// <param name="columnIndex">Zero based index of the column of field to set</param>
        /// <param name="value">value to set</param>
        public void SetLong(long columnIndex, long value)
        {
            ValidateIsValid();
            Owner.SetLongNoRowCheck(columnIndex, RowIndex, value);
        }

        /// <summary>
        /// Set the value in the Datatype.int field specified by the row's rowindex.
        /// and the specified zero based column index.
        /// Field specified must be DataType.Int. Note that DataType.Int.
        /// is just an integer type field, it can store everything up to 64 bits signed.
        /// </summary>
        /// <param name="columnIndex">Zero based index of the column of field to set</param>
        /// <param name="value">value to set</param>

        public void SetInt(long columnIndex, int value)
        {
            ValidateIsValid();
            Owner.SetIntNoRowCheck(columnIndex, RowIndex, value);
        }


        /// <summary>
        /// Set an entire row to the values of the objects specified.
        /// Tightdb will try to convert each object to its corresponding column type.
        /// Number of objects must be equal to number of columns in the table / number of fields in the row.
        /// </summary>
        /// <param name="rowContents">list of objects to set the row fields to</param>
        public void SetRow(params object[] rowContents) //setting field values does not apply to tightdb tableview
        {
            ValidateIsValid();
            Owner.SetRowNoCheck(RowIndex, rowContents);
        }

        /// <summary>
        /// Set the value in the Datatype.int field specified by the row's rowIndex,
        /// and the specified column name.
        /// Field specified must be DataType.Int.
        /// </summary>
        /// <param name="columnName">Zero based index of the column of field to set</param>
        /// <param name="value">value to set</param>

        public void SetLong(String columnName, long value)
        {
            ValidateIsValid();
            Owner.SetLongNoRowCheck(columnName,RowIndex,value);
        }

        /// <summary>
        /// Set the value in the Datatype.int field specified by the row's rowindex.
        /// and the specified zero based column index.
        /// Field specified must be DataType.Int. Note that DataType.Int.
        /// is just an integer type field, it can store everything up to 64 bits signed.
        /// </summary>
        /// <param name="columnName">Column name of the column of the field to set</param>
        /// <param name="value">value to set</param>

        public void SetInt(String columnName, int value)
        {
            ValidateIsValid();
            Owner.SetIntNoRowCheck(columnName, RowIndex, value);
        }

        /*
        internal DataType GetMixedTypeNoCheck(long columnIndex)
        {
            return Owner.GetMixedTypeNoCheck(columnIndex,RowIndex);
        }


        //called from a getter that already is in a switch statement on a mixed, so everything is already validated
        internal long GetMixedLongNoCheck(long columnIndex)                     //Datatype.Int
        {
            return Owner.GetMixedLongNoCheck(columnIndex, RowIndex);
        }

        internal Boolean GetMixedBoolNoCheck(long columnIndex)//Datatype.Bool
        {
            return Owner.GetMixedBoolNoCheck(columnIndex, RowIndex);
        }

        internal String GetMixedStringNoCheck(long columnIndex)//Datatype.String
        {
            return Owner.GetMixedStringNoCheck(columnIndex, RowIndex);
        }

        internal Byte[] GetMixedBinaryNoCheck(long columnIndex)//Datatype.Binary
        {
            return Owner.GetMixedBinaryNoCheck(columnIndex, RowIndex);
        }

        internal Table GetMixedSubtableNoCheck(long columnIndex)//Datatype.Table
        {
            return Owner.GetMixedSubTable(columnIndex, RowIndex);
        }

        //called from a switch statement in RowColumn where everything is already validated
        internal DateTime GetMixedDateTimeNoCheck(long columnIndex)
        {
            return Owner.GetMixedDateTimeNoCheck(columnIndex, RowIndex);
        }

        internal float GetMixedFloatNoCheck(long columnIndex)//Datatype.Table
        {
            return Owner.GetMixedFloat(columnIndex, RowIndex);
        }

        internal double GetMixedDoubleNoCheck(long columnIndex)//Datatype.Table
        {
            return Owner.GetMixedDouble(columnIndex, RowIndex);
        }
        */

        //call this if You know for sure that columnIndex is valid (but if You are not sure if the type of the column is in fact mixed)
        internal DataType MixedTypeCheckType(long columnIndex)
        {
            ValidateIsValid();
            return Owner.GetMixedTypeCheckType(columnIndex, RowIndex);//row and column cannot be invalid
        }

        /// <summary>
        /// Set a string field to a specified string value.
        /// field is specified by a zero based column index, and
        /// the row index that this row has.
        /// note that tightdb strings are UTF-8. The string parameter
        /// is expected to be UTF-16 and will be converted to UTF-8
        /// before it is stored
        /// </summary>
        /// <param name="columnIndex">zero based index of column of field to set</param>
        /// <param name="value">string value to set</param>
        public void SetString(long columnIndex, string value)
        {
            ValidateIsValid();
            Owner.SetStringNoRowCheck(columnIndex,RowIndex,value);
        }

        /// <summary>
        /// Set a DataType.bool field to a specified boolean value
        /// field is specified by a zero based column index, and
        /// the row index that this row has.
        /// Note that booleans are stored as the logical variable,
        /// You will get true or false back in the right place, even
        /// if you read a bool field into a language that maps boolean
        /// to other values than 0 and 1,  -1 and everything else, or
        /// whatever.
        /// </summary>
        /// <param name="columnIndex">zero based index of column of field to set</param>
        /// <param name="value">boolean value to set</param>
        public void SetBoolean(long columnIndex, Boolean value)
        {
            ValidateIsValid();
            Owner.SetBooleanNoRowCheck(columnIndex,RowIndex,value);
        }

        /// <summary>
        /// Set a DataType.bool field to a specified boolean value
        /// field is specified by a zero based column index, and
        /// the row index that this row has.
        /// Note that booleans are stored as the logical variable,
        /// You will get true or false back in the right place, even
        /// if you read a bool field into a language that maps boolean
        /// to other values than 0 and 1,  -1 and everything else, or
        /// whatever.
        /// </summary>
        /// <param name="columnName">zero based index of column of field to set</param>
        /// <param name="value">boolean value to set</param>
        public void SetBoolean(string columnName, Boolean value)
        {
            ValidateIsValid();
            Owner.SetBooleanNoRowCheck(columnName,RowIndex,value);
        }

        /// <summary>
        /// Set a DataType.Mixed field to a specified value.
        /// field is specified by a zero based column index, and
        /// the row index that this row has.
        /// Tightdb will try to convert the value object to something
        /// that fits in one of the mixed types, Tightdb does not
        /// store the type of object so You cannot store a custom class in mixed
        /// and get it back with its correct type.
        /// You can, however, store a string in a mixed field in one row,
        /// and then store a DateTime in the next row, and an int in the next again.
        /// See SetMixedTTTT methods
        /// </summary>
        /// <param name="columnIndex">zero based index of column of field to set</param>
        /// <param name="value">value to set</param>

        public void SetMixed(long columnIndex, object value)
        {
            ValidateIsValid();
            Owner.SetMixedNoRowCheck(columnIndex,RowIndex,value);
        }

        /// <summary>
        /// Return the value of a DataType.Mixed field.
        /// field is specified by a zero based column index, and
        /// the row index that this row has.
        /// Tightdb will try to convert the value in the field to a
        /// fitting C# class, for instance DataType.String becomes string
        /// DataType.Date becomes DateTime and so on
        /// </summary>
        /// <param name="columnIndex">zero based index of column of field to set</param>
        /// <returns>C# intepretation of the value inside the mixed</returns>
        public object GetMixed(long columnIndex)
        {
            ValidateIsValid();
            return Owner.GetMixedNoRowCheck(columnIndex,RowIndex);
        }

        /// <summary>
        /// Return the value of a DataType.Mixed field.
        /// field is specified by a zero based column index, and
        /// the row index that this row has.
        /// Tightdb will try to convert the value in the field to a
        /// fitting C# class, for instance DataType.String becomes string
        /// DataType.Date becomes DateTime and so on
        /// </summary>
        /// <param name="columnName">zero based index of column of field to set</param>
        /// <returns>C# intepretation of the value inside the mixed</returns>
        public object GetMixed(string columnName)
        {
            ValidateIsValid();
            return Owner.GetMixedNoRowCheck(columnName, RowIndex);
        }
        

        internal string GetColumnNameNoCheck(long columnIndex)
        {
            ValidateIsValid();
            return Owner.GetColumnNameNoCheck(columnIndex);
        }

        internal Table GetSubTableCheckType(long columnIndex)
        {
            ValidateIsValid();
            return Owner.GetSubTableCheckType(columnIndex, RowIndex);
        }

        /// <summary>
        /// Returns the table stored in the field indexed by columnIndex
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <returns>Table object referencing the subtable</returns>
        public Table GetSubTable(long columnIndex)
        {
            ValidateIsValid();
            return Owner.GetSubTableNoRowCheck(columnIndex, RowIndex);
        }

        /// <summary>
        /// Returns the table stored in the field indexed by columnIndex
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns>Table object referencing the subtable</returns>
        public Table GetSubTable(string columnName)
        {
            ValidateIsValid();
            return Owner.GetSubTableNoRowCheck(columnName, RowIndex);
        }


        /// <summary>
        /// Set string value in field specified by name
        /// </summary>
        /// <param name="name">Name of the column of the field to set</param>
        /// <param name="value">The string value to set in the field</param>
        public void SetString(string name, string value)
        {
            ValidateIsValid();
            Owner.SetStringNoRowCheck(name,RowIndex,value);
        }

        /// <summary>
        /// return the string in the field specified by columnIndex, and the rowIndex of this row
        /// </summary>
        /// <param name="columnIndex">zero based index of field to return</param>
        /// <returns>String value of field</returns>
        public String GetString(long columnIndex)//column and type of field will be checked
        {
            ValidateIsValid();
            return Owner.GetStringNoRowCheck(columnIndex, RowIndex);
        }

        /// <summary>
        /// return the string in the field specified by columnIndex, and the rowIndex of this row
        /// </summary>
        /// <param name="columnName">zero based index of field to return</param>
        /// <returns>String value of field</returns>
        public String GetString(string columnName)
        {
            ValidateIsValid();
            return Owner.GetStringNoRowCheck(columnName,RowIndex);
        }


        /// <summary>
        /// Return array of bytes with binary data from the specified DataType.Binary field
        /// </summary>
        /// <param name="columnIndex">Zero based index of the column of the field</param>
        /// <returns>binary data in the form of a byte array</returns>
        public byte[] GetBinary(long columnIndex)//column and type of field will be checked
        {
            ValidateIsValid();
            return Owner.GetBinaryNoRowCheck(columnIndex, RowIndex);
        }

        /// <summary>
        /// Return array of bytes with binary data from the specified DataType.Binary field
        /// </summary>
        /// <param name="columnName">Zero based index of the column of the field</param>
        /// <returns>binary data in the form of a byte array</returns>
        public byte[] GetBinary(string columnName)
        {
            ValidateIsValid();
            return Owner.GetBinaryNoRowCheck(columnName, RowIndex);
        }

        /// <summary>
        /// Set binary value in field specified by zero based column Index
        /// </summary>
        /// <param name="columnIndex">zero based index the column of the field to set</param>
        /// <param name="value">Byte array with the value to set in the field</param>

        public void SetBinary(long columnIndex,byte [] value)//column and type of field will be checked
        {
            ValidateIsValid();
             Owner.SetBinaryNoRowCheck(columnIndex, RowIndex,value);
        }

        /// <summary>
        /// Set string value in field specified by name
        /// </summary>
        /// <param name="columnName">Name of the column of the field to set</param>
        /// <param name="value">The string value to set in the field</param>

        public void SetBinary(string columnName, byte[] value)
        {
            ValidateIsValid();
             Owner.SetBinaryNoRowCheck(columnName, RowIndex,value);
        }


        /// <summary>
        /// return value of DataType.Int field specified by columnIndex
        /// </summary>
        /// <param name="columnIndex">Zero based index of column of field with integer value to return</param>
        /// <returns>long with the 64bit signed integer value stored in the field</returns>
        public long GetLong(long columnIndex)
        {
            ValidateIsValid();
            return Owner.GetLongNoRowCheck(columnIndex, RowIndex);
        }

        
        /// <summary>
        /// return value of DataType.Int field specified by column Name
        /// </summary>
        /// <param name="columnName">Zero based index of column of field with integer value to return</param>
        /// <returns>long with the 64bit signed integer value stored in the field</returns>
        public long GetLong(string columnName)
        {
            ValidateIsValid();
            return Owner.GetLongNoRowCheck(columnName, RowIndex);
        }

        /// <summary>
        /// returns a DateTime with the value in a specified DataType.Date field.
        /// The returned DateTime is of kind UTC, as the dates stored in tightdb are 
        /// all UTC per definition.
        /// </summary>
        /// <param name="columnName">name of column of DataType.Date field </param>
        /// <returns>DateTime with the value of the field</returns>
        public DateTime GetDateTime(string columnName)
        {
            ValidateIsValid();
            return Owner.GetDateTimeNoRowCheck(columnName, RowIndex);
        }

        /// <summary>
        /// returns a DateTime with the value in a specified DataType.Date field.
        /// The returned DateTime is of kind UTC, as the dates stored in tightdb are 
        /// all UTC per definition.
        /// </summary>
        /// <param name="columnIndex">name of column of DataType.Date field </param>
        /// <returns>DateTime with the value of the field</returns>
        public DateTime GetDateTime(long columnIndex)
        {
            ValidateIsValid();
            return Owner.GetDateTimeNoRowCheck(columnIndex, RowIndex);
        }

        /// <summary>
        /// Set DateTime value in field specified by name.
        /// note that the field must be DataType.Date.
        /// note that the DateTime will be converted to UTC,
        /// and truncated to nearest second, and that a date
        /// lower than 1970 is illegal. (tightdb stores dates
        /// as time_t)
        /// </summary>
        /// <param name="name">Name of the column of the field to set</param>
        /// <param name="value">The Datetime value to set in the field</param>

        public  void SetDateTime(string name,DateTime value)
        {
            ValidateIsValid();
            Owner.SetDateTimeNoRowCheck(name, RowIndex,value);
        }

        /// <summary>
        /// Set DateTime value in field specified by name.
        /// note that the field must be DataType.Date.
        /// note that the DateTime will be converted to UTC,
        /// and truncated to nearest second, and that a date
        /// lower than 1970 is illegal. (tightdb stores dates
        /// as time_t)
        /// </summary>
        /// <param name="columnIndex">Name of the column of the field to set</param>
        /// <param name="value">The Datetime value to set in the field</param>
        public void SetDateTime(long columnIndex, DateTime value)
        {
            ValidateIsValid();
             Owner.SetDateTimeNoRowCheck(columnIndex, RowIndex,value);
        }

        /// <summary>
        /// Set an empty subtable in the specified field
        /// </summary>
        /// <param name="columnIndex">index of column of field to set</param>
        public void ClearSubTable(long columnIndex)
        {
            ValidateIsValid();
            Owner.ClearSubTableNoRowCheck(columnIndex,RowIndex);

        }
        /// <summary>
        /// Set an empty subtable in the specified field
        /// </summary>
        /// <param name="columnName">Name of column of field to set</param>
        public void ClearSubTable(String columnName)
        {
            ValidateIsValid();
            Owner.ClearSubTableNoRowCheck(columnName, RowIndex);
        }

//in .net 35 Table cannot be taken as an IEnumerable<object> parameter
//in .net 40,45 Table is an IEnumerable<object> and You cannot overload a parameter as both Table and IEnumerable
//therefore we have two signatures in .35 and one in .40 and .45
//An alternative approach would be to have setsubtablefromtableblabla(Table)
//And SetSubTableFromEnumerable(IEnumerable<object>)
//This current approach is a bit bloated in .35 but lean in .40 and .45 and going forward
//the alternative would be to continue having more methods in the newer .net versions
//we continue to support .net35 bc unity uses an old mono in 32bit, and that old mono seems to 
//only be able to run .net35 and older
//It should be considered if we should stop supporting .net35 at some point,enabling some cleanup
#if !V40PLUS
        /// <summary>
        /// Sets the subtable to the specified table specification.
        /// See TableOrView SetSubtable for a more detailed explanation
        /// </summary>
        /// <param name="columnIndex">Column Index of field to set a subtable into</param>
        /// <param name="element">Null,IEnumerable or Table with table data to be copied</param>
        public void SetSubTable(long columnIndex, IEnumerable<Object> element)
        {
            ValidateIsValid();//row validation. Table validation happens in the call below
            Owner.SetSubTableNoRowCheck(columnIndex, RowIndex, element);
        }

        /// <summary>
        /// Sets the subtable to the specified table specification.
        /// See TableOrView SetSubtable for a more detailed explanation
        /// </summary>
        /// <param name="columnName">Column Name of field to set a subtable into</param>
        /// <param name="element">Null,IEnumerable or Table with table data to be copied</param>
        public void SetSubTable(String columnName, IEnumerable<Object> element)
        {
            ValidateIsValid();//row validation. Table validation happens in the call below
            Owner.SetSubTableNoRowCheck(columnName, RowIndex, element);
        }

        /// <summary>
        /// Sets the subtable to the specified table specification.
        /// See TableOrView SetSubtable for a more detailed explanation
        /// </summary>
        /// <param name="columnIndex">Column Index of field to set a subtable into</param>
        /// <param name="element">Null,IEnumerable or Table with table data to be copied</param>
        public void SetSubTable(long columnIndex, Table element)
        {
            ValidateIsValid();//row validation. Table validation happens in the call below
            Owner.SetSubTableNoRowCheck(columnIndex, RowIndex, element);
        }

        /// <summary>
        /// Sets the subtable to the specified table specification.
        /// See TableOrView SetSubtable for a more detailed explanation
        /// </summary>
        /// <param name="columnName">Column Name of field to set a subtable into</param>
        /// <param name="element">Null,IEnumerable or Table with table data to be copied</param>
        public void SetSubTable(String columnName, Table element)
        {
            ValidateIsValid();//row validation. Table validation happens in the call below
            Owner.SetSubTableNoRowCheck(columnName, RowIndex, element);
        }

#else

        /// <summary>
        /// Sets the subtable to the specified table specification.
        /// See TableOrView SetSubtable for a more detailed explanation
        /// </summary>
        /// <param name="columnIndex">Column Index of field to set a subtable into</param>
        /// <param name="element">Null,IEnumerable or Table with table data to be copied</param>
        public void SetSubTable(long columnIndex, IEnumerable<Object> element)
        {
            ValidateIsValid();//row validation. Table validation happens in the call below
            Owner.SetSubTableNoRowCheck(columnIndex,RowIndex,element);
        }

        /// <summary>
        /// Sets the subtable to the specified table specification.
        /// See TableOrView SetSubtable for a more detailed explanation
        /// </summary>
        /// <param name="columnName">Column Name of field to set a subtable into</param>
        /// <param name="element">Null,IEnumerable or Table with table data to be copied</param>
        public void SetSubTable(String  columnName, IEnumerable<Object> element)
        {
            ValidateIsValid();//row validation. Table validation happens in the call below
            Owner.SetSubTableNoRowCheck(columnName, RowIndex, element);
        }
#endif


        /// <summary>
        /// return the long value stored in a mixed field.
        /// Will throw if the mixed field does not store an integer type.
        /// </summary>
        /// <param name="columnName">Name of the column of the field to return</param>
        /// <returns>long value stored in the field</returns>
        public long GetMixedLong(string columnName)
        {
            ValidateIsValid();
            return Owner.GetMixedLongNoRowCheck(columnName, RowIndex);
        }

        /// <summary>
        /// return the long value stored in a mixed field.
        /// Will throw if the mixed field does not store an integer type.
        /// </summary>
        /// <param name="columnIndex">Name of the column of the field to return</param>
        /// <returns>long value stored in the field</returns>
        public long GetMixedLong(long columnIndex)
        {
            ValidateIsValid();
            return Owner.GetMixedLongNoRowCheck(columnIndex, RowIndex);
        }


        /// <summary>
        /// return the booelean value stored in a mixed field.
        /// Will throw if the mixed field is not of type mixed boolean
        /// </summary>
        /// <param name="columnName">Name of the column of the field to return</param>
        /// <returns>long value stored in the field</returns>
        public Boolean GetMixedBoolean(string columnName)
        {
            ValidateIsValid();
            return Owner.GetMixedBooleanNoRowCheck(columnName, RowIndex);
        }

        /// <summary>
        /// return the booelean value stored in a mixed field.
        /// Will throw if the mixed field is not of type mixed boolean
        /// </summary>
        /// <param name="columnIndex">Name of the column of the field to return</param>
        /// <returns>long value stored in the field</returns>
        public Boolean GetMixedBoolean(long columnIndex)
        {
            ValidateIsValid();
            return Owner.GetMixedBooleanNoRowCheck(columnIndex, RowIndex);
        }


        /// <summary>
        /// return the float value stored in a mixed field.
        /// Will throw if the mixed field is not of type mixed float
        /// </summary>
        /// <param name="columnName">Name of the column of the field to return</param>
        /// <returns>long value stored in the field</returns>
        public float GetMixedFloat(string columnName)
        {
            ValidateIsValid();
            return Owner.GetMixedFloatNoRowCheck(columnName, RowIndex);
        }

        /// <summary>
        /// return the float value stored in a mixed field.
        /// Will throw if the mixed field is not of type mixed float
        /// </summary>
        /// <param name="columnIndex">Zero based index of the column of the field to return</param>
        /// <returns>float value stored in the field</returns>
        public float GetMixedFloat(long columnIndex)
        {
            ValidateIsValid();
            return Owner.GetMixedFloatNoRowCheck(columnIndex, RowIndex);
        }

        /// <summary>
        /// return the double value stored in a mixed field.
        /// Will throw if the mixed field is not of type mixed double
        /// </summary>
        /// <param name="columnName">name of the column of the field to return</param>
        /// <returns>double value stored in the field</returns>
        public Double GetMixedDouble(string columnName)
        {
            ValidateIsValid();
            return Owner.GetMixedDoubleNoRowCheck(columnName, RowIndex);
        }

        /// <summary>
        /// return the double value stored in a mixed field.
        /// Will throw if the mixed field is not of type mixed double
        /// </summary>
        /// <param name="columnIndex">Zero based index of the column of the field to return</param>
        /// <returns>double value stored in the field</returns>
        public Double GetMixedDouble(long columnIndex)
        {
            ValidateIsValid();
            return Owner.GetMixedDoubleNoRowCheck(columnIndex, RowIndex);
        }

        /// <summary>
        /// return the string value stored in a mixed field.
        /// Will throw if the mixed field is not of type mixed double
        /// Note that TightDb stores strings as UTF-8. The string
        /// will be converted to UTF-16 before being returned, so this method
        /// returns an ordinary C# String.
        /// </summary>
        /// <param name="columnName">Name of the column of the field to return</param>
        /// <returns>double value stored in the field</returns>
        public String GetMixedString(string columnName)
        {
            ValidateIsValid();
            return Owner.GetMixedStringNoRowCheck(columnName, RowIndex);
        }

        /// <summary>
        /// return the string value stored in a mixed field.
        /// Will throw if the mixed field is not of type mixed double
        /// Note that TightDb stores strings as UTF-8. The string
        /// will be converted to UTF-16 before being returned, so this method
        /// returns an ordinary C# String.
        /// </summary>
        /// <param name="columnIndex">Name of the column of the field to return</param>
        /// <returns>double value stored in the field</returns>
        
        public String GetMixedString(long columnIndex)
        {
            ValidateIsValid();
            return Owner.GetMixedStringNoRowCheck(columnIndex, RowIndex);
        }

        /// <summary>
        /// Return the binary data stored in an Mixed binary field
        /// Throws if the field is not mixed binary.
        /// </summary>
        /// <param name="columnIndex">Zero based index of column of field to return</param>
        /// <returns>array of bytes containing the binary data</returns>
        public byte[] GetMixedBinary(long columnIndex)
        {
            ValidateIsValid();
            return Owner.GetMixedBinaryNoRowCheck(columnIndex, RowIndex);
        }

        /// <summary>
        /// Return the binary data stored in an Mixed binary field
        /// Throws if the field is not mixed binary.
        /// </summary>
        /// <param name="columnName">Name of the column of the field to return</param>
        /// <returns>array of bytes containing the binary data</returns>
        public byte[] GetMixedBinary(string columnName)
        {
            ValidateIsValid();
            return Owner.GetMixedBinaryNoRowCheck(columnName, RowIndex);
        }


        /// <summary>
        /// Set DataType.String value in field specified by name.
        /// Strings are stored in the database as UTF-8 so the
        /// C# string is converted to UTF-8 by the binding.
        /// </summary>
        /// <param name="columnName">Name of the column of the field to set</param>
        /// <param name="value">The string value to set in the field</param>

        public void SetMixedString(string columnName,String value)
        {
            ValidateIsValid();
             Owner.SetMixedStringNoRowCheck(columnName, RowIndex,value);
        }

        /// <summary>
        /// Set DataType.String value in field specified by name.
        /// Strings are stored in the database as UTF-8 so the
        /// C# string is converted to UTF-8 by the binding.
        /// </summary>
        /// <param name="columnIndex">Name of the column of the field to set</param>
        /// <param name="value">The string value to set in the field</param>

        public void SetMixedString(long columnIndex,string value)
        {
            ValidateIsValid();
             Owner.SetMixedStringNoRowCheck(columnIndex, RowIndex,value);
        }


        /// <summary>
        /// Return the DateTime stored in a mixed field identified by its name.
        /// Throws if the field is not of type mixed Date
        /// </summary>
        /// <param name="name">name of the column of the field to return</param>
        /// <returns>Datetime with the datetime value of the mixed field specified</returns>
        public DateTime GetMixedDateTime(string name)
        {
            ValidateIsValid();
            return Owner.GetMixedDateTimeNoRowCheck(name, RowIndex);
        }

        /// <summary>
        /// Return the DateTime stored in a mixed field identified by its name.
        /// Throws if the field is not of type mixed Date
        /// </summary>
        /// <param name="columnIndex">name of the column of the field to return</param>
        /// <returns>Datetime with the datetime value of the mixed field specified</returns>
        public DateTime GetMixedDateTime(long columnIndex)
        {
            ValidateIsValid();
            return Owner.GetMixedDateTimeNoRowCheck(columnIndex,RowIndex);
        }


        /// <summary>
        /// Set DateTime value in mixed field specified by name.        
        /// note that the DateTime will be converted to UTC,
        /// and truncated to nearest second, and that a date
        /// lower than 1970 is illegal. (tightdb stores dates
        /// as time_t)
        /// </summary>
        /// <param name="columnName">Name of the column of the field to set</param>
        /// <param name="value">The Datetime value to set in the field</param>

        public void SetMixedDateTime(string columnName,DateTime value)
        {
            ValidateIsValid();
            Owner.SetMixedDateTimeNoRowCheck(columnName, RowIndex,value);
        }

        /// <summary>
        /// Set DateTime value in mixed field specified by name.        
        /// note that the DateTime will be converted to UTC,
        /// and truncated to nearest second, and that a date
        /// lower than 1970 is illegal. (tightdb stores dates
        /// as time_t)
        /// </summary>
        /// <param name="columnIndex">Name of the column of the field to set</param>
        /// <param name="value">The Datetime value to set in the field</param>
        public void SetMixedDateTime(long columnIndex, DateTime value)
        {
            ValidateIsValid();
            Owner.SetMixedDateTimeNoRowCheck(columnIndex, RowIndex,value);
        }


        /// <summary>
        /// Return the Table stored in a mixed field identified by the name of the column of the field.
        /// Throws if the field is not of type mixed Table
        /// </summary>
        /// <param name="columnName">name of the column of the field to return</param>
        /// <returns>Table object representing the table contained in the field</returns>

        public Table GetMixedTable(string columnName)
        {
            ValidateIsValid();
            return  Owner.GetMixedTableNoRowCheck(columnName, RowIndex);
        }

        /// <summary>
        /// Return the Table stored in a mixed field identified by the name of the column of the field.
        /// Throws if the field is not of type mixed Table
        /// </summary>
        /// <param name="columnIndex">name of the column of the field to return</param>
        /// <returns>Table object representing the table contained in the field</returns>

        public Table GetMixedTable(long columnIndex)
        {
            ValidateIsValid();
            return Owner.GetMixedTableNoRowCheck(columnIndex, RowIndex);
        }


        /// <summary>
        /// Return double value of the DataType.Double field specified by columnName
        /// </summary>
        /// <param name="columnName">Name of the column of the field whose value to return</param>
        /// <returns>Double contained in the specified field</returns>
        public Double GetDouble(string columnName)
        {
            ValidateIsValid();
            return Owner.GetDoubleNoRowCheck(columnName, RowIndex);
        }

        /// <summary>
        /// Return double value of the DataType.Double field specified by columnIndex
        /// </summary>
        /// <param name="columnIndex">Zero based index of the column of the field whose value to return</param>
        /// <returns>Double contained in the specified field</returns>
        public Double GetDouble(long columnIndex)
        {
            ValidateIsValid();
            return Owner.GetDoubleNoRowCheck(columnIndex, RowIndex);
        }

        /// <summary>
        /// Return float value of the DataType.float field specified by columnName
        /// </summary>
        /// <param name="columnName">Zero based index of the column of the field whose value to return</param>
        /// <returns>float contained in the specified field</returns>
        public float GetFloat(string columnName)
        {
            ValidateIsValid();
            return Owner.GetFloatNoRowCheck(columnName, RowIndex);
        }


        /// <summary>
        /// Return float value of the DataType.float field specified by columnIndex
        /// </summary>
        /// <param name="columnIndex">Zero based index of the column of the field whose value to return</param>
        /// <returns>float contained in the specified field</returns>
        public float GetFloat(long columnIndex)
        {
            ValidateIsValid();
            return Owner.GetFloatNoRowCheck(columnIndex, RowIndex);
        }


        /// <summary>
        /// Return boolean value of the DataType.bool field specified by columnName
        /// </summary>
        /// <param name="columnName">Name of the column of the field whose value to return</param>
        /// <returns>Boolean contained in the specified field</returns>
        public Boolean GetBoolean(string columnName)
        {
            ValidateIsValid();
            return Owner.GetBooleanNoRowCheck(columnName, RowIndex);
        }

        /// <summary>
        /// Return boolean value of the DataType.bool field specified by columnIndex
        /// </summary>
        /// <param name="columnIndex">Zero based index of the column of the field whose value to return</param>
        /// <returns>Boolean contained in the specified field</returns>

        public Boolean GetBoolean(long columnIndex)
        {
            ValidateIsValid();
            return Owner.GetBooleanNoRowCheck(columnIndex,RowIndex);
        }

        /// <summary>
        /// Return boolean value of the DataType.bool field specified by columnName
        /// </summary>
        /// <param name="columnName">Name of the column of the field whose value to return</param>
        /// <returns>Boolean contained in the specified field</returns>
        public long GetColumnIndex(string columnName)
        {
            ValidateIsValid();//checks that the ROW is still valid
            return Owner.GetColumnIndex(columnName);//will check that the core table is still valid
        }

       
        /// <summary>
        /// Removes the table row associated with this row object
        /// Afterwards all row objects attached to the table are invalid and will throw if used
        /// </summary>
        public void Remove()
        {
            ValidateIsValid();
            Owner.RemoveNoCheck(RowIndex);            
        }

    }
}