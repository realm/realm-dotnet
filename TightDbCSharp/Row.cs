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
        public TableOrView Owner { get; set; }
        public long RowIndex { get; internal set; }//users should not be allowed to change the row property of a tablerow class
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "long")]


        //allow foreach to traverse a TableRow and get some TableRowColumn objects
        //if You do a foreach on a tablerow, C# will use the for loop below to do the iteration
        public IEnumerator<RowColumn> GetEnumerator()
        {
            for (long i = 0; i < ColumnCount; i++)
            {
                yield return new RowColumn(this, i);
            }
        }

        internal DataType ColumnTypeNoCheck(long columnIndex)
        {
            return Owner.ColumnTypeNoCheck(columnIndex);
        }

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
        internal long GetMixedLongNoCheck(long columnIndex)
        {
            return Owner.GetMixedLongNoCheck(columnIndex, RowIndex);
        }

        //called from a switch statement in RowColumn where everything is already validated
        internal DateTime GetMixedDateTimeNoCheck(long columnIndex)
        {
            return Owner.GetMixedDateTimeNoCheck(columnIndex, RowIndex);
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

        public DateTime GetMixedDateTime(string name)
        {
            return Owner.GetMixedDateTimeNoRowCheck(name, RowIndex);
        }

        public DateTime GetMixedDateTime(long columnIndex)
        {
            return Owner.GetMixedDateTimeNoRowCheck(columnIndex,RowIndex);
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
        internal Table GetMixedSubtableNoCheck(long columnIndex)
        {
            return Owner.GetMixedSubTable(columnIndex, RowIndex);
        }

        internal Boolean GetBooleanNoCheck(long columnIndex)
        {
            return Owner.GetBooleanNoCheck(columnIndex, RowIndex);
        }

        public String GetStringNoCheck(long columnIndex)
        {
            return Owner.GetStringNoCheck(columnIndex, RowIndex);
        }

       
        public void Remove()
        {
            Owner.RemoveNoCheck(RowIndex);
            RowIndex = -2;//mark this row as invalid            
        }
    }
}