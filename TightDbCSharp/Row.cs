using System;
using System.Collections.Generic;

namespace TightDbCSharp
{
    /// <summary>
    /// Represents one row in a tightdb Table
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
            for (long i = 0; i < Owner.ColumnCount; i++)
            {
                yield return new RowColumn(this, i);
            }
        }


        //public object GetValue(long columnNumber)
        //{
        //return Owner.GetValue(row, columnNumber);
        //}
        internal void SetLongNoCheck(long columnIndex, long value)
        {
            Owner.SetLongNoCheck(columnIndex,RowIndex,value);
        }

        public void SetLong(long columnIndex, long value)
        {
            Owner.SetLongNoRowCheck(columnIndex, RowIndex, value);
        }

        public void SetLong(String columnName, long value)
        {
            Owner.SetLongNoRowCheck(columnName,RowIndex,value);
        }

        internal DataType GetMixedTypeNoCheck(long columnIndex)
        {
            return Owner.GetMixedTypeNoCheck(columnIndex,RowIndex);
        }

        internal long GetMixedLongNoCheck(long columnIndex)
        {
            return Owner.GetMixedLongNoCheck(columnIndex, RowIndex);
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

        //todo:implement
        public void SetRow(params object[] x)
        {
            throw new NotImplementedException();
        }

        //todo:implement

        //todo:implement
        public void Remove()
        {
            RowIndex = -2;//mark this row as invalid
            throw new NotImplementedException();
        }
    }
}