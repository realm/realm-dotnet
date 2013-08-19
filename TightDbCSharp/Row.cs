using System;
using System.Collections.Generic;

namespace TightDbCSharp
{
    /// <summary>
    /// Common class for rows in TableView and Table.  The TableRow class that adds table only specific methods inherits from Row and adds these special methods
    /// iterating a TableView gets You objects of type Row, while iterating a Table gets You classes of type TableRow (as these inherit from Row, they appear almost identical
    /// Both Row and TableRow returns RowColumn when iterated (column specific methods are the same for TableView and Table)
    /// </summary>
    public class Row
    {
        internal Row(TableOrView owner,long row) {
            Owner=owner;
            
            
            // The Row number of the row this TableRow references
            RowIndex=row;
        }
        public TableOrView Owner { get; private set; }
        public long RowIndex { get; private set; }//users should not be allowed to change the row property of a tablerow class

        //by indexing you can get and set values as objects (if they match the type that is)
        public  object this[long columnIndex]
        {
            get
            {   Owner.ValidateColumnIndex(columnIndex);
                return new RowColumn(this, columnIndex).Value;//todo: consider optimize by having the value logic moved to table or to tableview or even tableorview. Then we could spare the rowcolumn class creation
            }
            set
            {   
                Owner.ValidateColumnIndex(columnIndex);
                new RowColumn(this, columnIndex).Value = value;//todo:refactor value out to tableorview and save an RowColumn creation
            }
        }


        //allow foreach to traverse a TableRow and get some TableRowColumn objects
        //if You do a foreach on a tablerow, C# will use the for loop below to do the iteration
        public IEnumerator<RowColumn> GetEnumerator()
        {
            for (long i = 0; i < ColumnCount; i++)
            {
                yield return new RowColumn(this, i);
            }
        }

        /* not used anymore
        internal DataType ColumnTypeNoCheck(long columnIndex)
        {
            return Owner.ColumnTypeNoCheck(columnIndex);
        }
         */

        public long ColumnCount 
        {
            get { return Owner.ColumnCount; }
        }

        //public object GetValue(long columnNumber)
        //{
        //return Owner.GetValue(row, columnNumber);
        //}
        internal void SetLongNoCheck(long columnIndex, long value)
        {
            Owner.SetLongNoCheck(columnIndex,RowIndex,value);
        }

        internal void SetBoolNoCheck(long columnIndex, bool value)
        {
            Owner.SetBoolNoCheck(columnIndex, RowIndex, value);
        }

        internal void SetStringNoCheck(long columnIndex, string value)
        {
            Owner.SetStringNoCheck(columnIndex, RowIndex, value);
        }

        internal void SetBinaryNoCheck(long columnIndex, byte[] value)
        {
            Owner.SetBinaryNoCheck(columnIndex, RowIndex, value);
        }

        //call with an array of objects that match the subtable in this column in types and values and dimension
        //or set values in subtables like this myrow.GetSubTable(colid).[subcoln][subrown] = value
        //or set values in subtables like this myrow.GetSubTable(colid).[subcoln][subrown].SetString(value)
        //or set values in subtables like this myrow.GetSubTable(colid).SetString(subcolid,subrowid,value);
        //the last two are typechecked in runtime so faster and safer

        internal void SetSubTableNoCheck(long columnIndex, IEnumerable<object> value)
        {
            Owner.SetSubTableNoCheckHighLevel(columnIndex, RowIndex, value);
        }

        internal void SetMixedNoCheck(long columnIndex, object value)
        {
            Owner.SetMixedNoCheck(columnIndex,RowIndex,value);
        }

        internal void SetDateNoCheck(long columnIndex, DateTime value)
        {
            Owner.SetMixedNoCheck(columnIndex, RowIndex, value);
        }

        internal void SetFloatNoCheck(long columnIndex, float value)
        {
            Owner.SetFloatNoCheck(columnIndex, RowIndex, value);
        }

        internal void SetDoubleNoCheck(long columnIndex, Double value)
        {
            Owner.SetDoubleNoCheck(columnIndex, RowIndex, value);
        }



        public void SetFloat(long columnIndex, float value)
        {
            Owner.SetFloatNoRowCheck(columnIndex, RowIndex, value);
        }

        public void SetDouble(long columnIndex, double value)
        {
            Owner.SetDoubleNoRowCheck(columnIndex, RowIndex, value);
        }

        public void SetFloat(String  columnName, float value)
        {
            Owner.SetFloatNoRowCheck(columnName, RowIndex, value);
        }

        public void SetDouble(String columnName, double value)
        {
            Owner.SetDoubleNoRowCheck(columnName, RowIndex, value);
        }

        public void SetLong(long columnIndex, long value)
        {
            Owner.SetLongNoRowCheck(columnIndex, RowIndex, value);
        }

        public void SetRow(params object[] rowContents) //setting field values does not apply to tightdb tableview
        {
            Owner.SetRowNoCheck(RowIndex, rowContents);
        }

        public void SetLong(String columnName, long value)
        {
            Owner.SetLongNoRowCheck(columnName,RowIndex,value);
        }

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


        //call this if You know for sure that columnIndex is valid (but if You are not sure if the type of the column is in fact mixed)
        internal DataType MixedTypeCheckType(long columnIndex)
        {
            return Owner.GetMixedTypeCheckType(columnIndex, RowIndex);//row and column cannot be invalid
        }

        public void SetString(long columnIndex, string value)
        {
            Owner.SetStringNoRowCheck(columnIndex,RowIndex,value);
        }

        public void SetBoolean(long columnIndex, Boolean value)
        {
            Owner.SetBooleanNoRowCheck(columnIndex,RowIndex,value);
        }

        public void SetBoolean(string columnName, Boolean value)
        {
            Owner.SetBooleanNoRowCheck(columnName,RowIndex,value);
        }

        internal string GetColumnNameNoCheck(long columnIndex)
        {
            return Owner.GetColumnNameNoCheck(columnIndex);
        }

        internal Table GetSubTableNoCheck(long columnIndex)
        {
            return Owner.GetSubTableNoCheck(columnIndex, RowIndex);
        }

        internal Table GetSubTableCheckType(long columnIndex)
        {
            return Owner.GetSubTableCheckType(columnIndex, RowIndex);
        }

        public Table GetSubTable(long columnIndex)
        {
            return Owner.GetSubTableNoRowCheck(columnIndex, RowIndex);
        }

        public Table GetSubTable(string columnName)
        {
            return Owner.GetSubTableNoRowCheck(columnName, RowIndex);
        }

        public void SetString(string name, string value)
        {
            var columnIndex = GetColumnIndex(name);            
            Owner.SetStringCheckType(columnIndex,RowIndex,value);
        }

        public String GetString(long columnIndex)//column and type of field will be checked
        {
            return Owner.GetStringNoRowCheck(columnIndex, RowIndex);
        }

        public String GetString(string columnName)
        {                     
            return Owner.GetStringNoRowCheck(columnName,RowIndex);
        }


        public byte[] GetBinary(long columnIndex)//column and type of field will be checked
        {
            return Owner.GetBinaryNoRowCheck(columnIndex, RowIndex);
        }

        public byte[] GetBinary(string columnName)
        {
            return Owner.GetBinaryNoRowCheck(columnName, RowIndex);
        }


        public long GetLong(long columnNumber)
        {
            return Owner.GetLongNoRowCheck(columnNumber, RowIndex);
        }

        internal long GetLongNoCheck(long columnIndex)
        {
            return Owner.GetLongNoCheck(columnIndex, RowIndex);
        }

        public long GetLong(string name)
        {            
            return Owner.GetLongNoRowCheck(name, RowIndex);
        }

        public DateTime GetDateTime(string name)
        {
            return Owner.GetDateTimeNoRowCheck(name, RowIndex);
        }

        public long GetMixedLong(string name)
        {
            return Owner.GetMixedLongNoRowCheck(name, RowIndex);
        }

        public long GetMixedLong(long columnIndex)
        {
            return Owner.GetMixedLongNoRowCheck(columnIndex, RowIndex);
        }


        public Boolean GetMixedBoolean(string name)
        {
            return Owner.GetMixedBooleanNoRowCheck(name, RowIndex);
        }

        public Boolean GetMixedBoolean(long columnIndex)
        {
            return Owner.GetMixedBooleanNoRowCheck(columnIndex, RowIndex);
        }


        public float GetMixedFloat(string name)
        {
            return Owner.GetMixedFloatNoRowCheck(name, RowIndex);
        }

        public float GetMixedFloat(long columnIndex)
        {
            return Owner.GetMixedFloatNoRowCheck(columnIndex, RowIndex);
        }

        public Double GetMixedDouble(string name)
        {
            return Owner.GetMixedDoubleNoRowCheck(name, RowIndex);
        }

        public Double GetMixedDouble(long columnIndex)
        {
            return Owner.GetMixedDoubleNoRowCheck(columnIndex, RowIndex);
        }

        public String GetMixedString(string name)
        {
            return Owner.GetMixedStringNoRowCheck(name, RowIndex);
        }

        public String GetMixedString(long columnIndex)
        {
            return Owner.GetMixedStringNoRowCheck(columnIndex, RowIndex);
        }


        public DateTime GetMixedDateTime(string name)
        {
            return Owner.GetMixedDateTimeNoRowCheck(name, RowIndex);
        }

        public DateTime GetMixedDateTime(long columnIndex)
        {
            return Owner.GetMixedDateTimeNoRowCheck(columnIndex,RowIndex);
        }

        public Table GetMixedTable(string name)
        {
            return Owner.GetMixedTableNoRowCheck(name, RowIndex);
        }

        public Table GetMixedTable(long columnIndex)
        {
            return Owner.GetMixedTableNoRowCheck(columnIndex, RowIndex);
        }



        public DateTime GetDateTime(long columnIndex)
        {
            return Owner.GetDateTimeNoRowCheck(columnIndex, RowIndex);
        }


        public Double GetDouble(string columnName)
        {
            return Owner.GetDoubleNoRowCheck(columnName, RowIndex);
        }

        public Double GetDouble(long columnIndex)
        {
            return Owner.GetDoubleNoRowCheck(columnIndex, RowIndex);
        }

        public float GetFloat(string columnName)
        {
            return Owner.GetFloatNoRowCheck(columnName, RowIndex);
        }

        public float GetFloat(long columnIndex)
        {
            return Owner.GetFloatNoRowCheck(columnIndex, RowIndex);
        }


        public Boolean GetBoolean(string columnName)
        {
            return Owner.GetBooleanNoRowCheck(columnName, RowIndex);
        }

        public Boolean GetBoolean(long columnIndex)
        {
            return Owner.GetBooleanNoRowCheck(columnIndex,RowIndex);
        }

        public long GetColumnIndex(string columnName)
        {
            return Owner.GetColumnIndex(columnName);
        }

        internal Boolean GetBooleanNoCheck(long columnIndex)
        {
            return Owner.GetBoolNoCheck(columnIndex, RowIndex);
        }

        public String GetStringNoCheck(long columnIndex)
        {
            return Owner.GetStringNoCheck(columnIndex, RowIndex);
        }

        public byte[] GetBinaryNoCheck(long columnIndex)
        {
            return Owner.GetBinaryNoCheck(columnIndex, RowIndex);
        }
       
        public void Remove()
        {
            Owner.RemoveNoCheck(RowIndex);
            RowIndex = -2;//mark this row as invalid            
        }

        internal DateTime GetDateTimeNoCheck(long columnIndex)
        {
            return Owner.GetDateTimeNoCheck(columnIndex, RowIndex);
        }


        internal float GetFloatNoCheck(long columnIndex)
        {
            return Owner.GetFloatNoCheck(columnIndex, RowIndex);
        }

        internal Double GetDoubleNoCheck(long columnIndex)
        {
            return Owner.GetDoubleNoCheck(columnIndex, RowIndex);
        }
    }
}