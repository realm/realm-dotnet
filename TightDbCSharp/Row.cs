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
            MyRow=row;
        }
        public TableOrView Owner { get; set; }
        public long MyRow { get; internal set; }//users should not be allowed to change the row property of a tablerow class
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "long")]
        public long GetLong(long columnNumber) {
            { return Owner.GetLongNoColumnRowCheck(columnNumber,MyRow);}
        }

        internal long GetLongNoCheck(long columnIndex)
        {
            return Owner.GetLongNoCheck(columnIndex, MyRow);//we know that Row could not have been set by user code, so it's safe
        }

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
            Owner.SetLongNoCheck(columnIndex,MyRow,value);
        }


        internal DataType GetMixedTypeNoCheck(long columnIndex)
        {
            return Owner.GetMixedTypeNoCheck(columnIndex,MyRow);
        }

        internal long GetMixedLongNoCheck(long columnIndex)
        {
            return Owner.GetMixedLongNoCheck(columnIndex, MyRow);
        }

        //call this if You know for sure that columnIndex is valid (but if You are not sure if the type of the column is in fact mixed)
        internal DataType MixedTypeCheckType(long columnIndex)
        {
            return Owner.GetMixedTypeCheckType(columnIndex, MyRow);//row and column cannot be invalid
        }

        internal Table GetMixedSubtableNoCheck(long columnIndex)
        {
            return Owner.GetMixedSubTable(columnIndex, MyRow);
        }
    }
}