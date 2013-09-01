using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

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
        public TableOrView Owner { get; private set; }
        public long RowIndex { get; private set; }//users should not be allowed to change the row property of a tablerow class
        private readonly int _underlyingTableVersion;
        private readonly Table _underlyingTable;
        //by indexing you can get and set values as objects (if they match the type that is)
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

        //allow foreach to traverse a TableRow and get some TableRowCell objects
        //if You do a foreach on a TableRow, C# will use the for loop below to do the iteration
        public IEnumerator<RowCell> GetEnumerator()
        {
            for (long i = 0; i < ColumnCount; i++)
            {
                ValidateIsValid();
                yield return new RowCell(this, i);
            }
        }

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

        public long ColumnCount 
        {
            get { return Owner.ColumnCount; }
        }

        public void SetFloat(long columnIndex, float value)
        {
            ValidateIsValid();
            Owner.SetFloatNoRowCheck(columnIndex, RowIndex, value);
        }

        public void SetDouble(long columnIndex, double value)
        {
            ValidateIsValid();
            Owner.SetDoubleNoRowCheck(columnIndex, RowIndex, value);
        }

        public void SetFloat(String  columnName, float value)
        {
            ValidateIsValid();
            Owner.SetFloatNoRowCheck(columnName, RowIndex, value);
        }

        public void SetDouble(String columnName, double value)
        {
            ValidateIsValid();
            Owner.SetDoubleNoRowCheck(columnName, RowIndex, value);
        }

        public void SetLong(long columnIndex, long value)
        {
            ValidateIsValid();
            Owner.SetLongNoRowCheck(columnIndex, RowIndex, value);
        }

        public void SetRow(params object[] rowContents) //setting field values does not apply to tightdb tableview
        {
            ValidateIsValid();
            Owner.SetRowNoCheck(RowIndex, rowContents);
        }

        public void SetLong(String columnName, long value)
        {
            ValidateIsValid();
            Owner.SetLongNoRowCheck(columnName,RowIndex,value);
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

        public void SetString(long columnIndex, string value)
        {
            ValidateIsValid();
            Owner.SetStringNoRowCheck(columnIndex,RowIndex,value);
        }

        public void SetBoolean(long columnIndex, Boolean value)
        {
            ValidateIsValid();
            Owner.SetBooleanNoRowCheck(columnIndex,RowIndex,value);
        }

        public void SetBoolean(string columnName, Boolean value)
        {
            ValidateIsValid();
            Owner.SetBooleanNoRowCheck(columnName,RowIndex,value);
        }


        public void SetMixed(long columnIndex, object value)
        {
            ValidateIsValid();
            Owner.SetMixedNoRowCheck(columnIndex,RowIndex,value);
        }

        public object GetMixed(long columnIndex)
        {
            ValidateIsValid();
            return Owner.GetMixedNoRowCheck(columnIndex,RowIndex);
        }

        public object GetMixed(string  columnName)
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

        public Table GetSubTable(long columnIndex)
        {
            ValidateIsValid();
            return Owner.GetSubTableNoRowCheck(columnIndex, RowIndex);
        }

        public Table GetSubTable(string columnName)
        {
            ValidateIsValid();
            return Owner.GetSubTableNoRowCheck(columnName, RowIndex);
        }

        public void SetString(string name, string value)
        {
            ValidateIsValid();
            Owner.SetStringNoRowCheck(name,RowIndex,value);
        }

        public String GetString(long columnIndex)//column and type of field will be checked
        {
            ValidateIsValid();
            return Owner.GetStringNoRowCheck(columnIndex, RowIndex);
        }

        public String GetString(string columnName)
        {
            ValidateIsValid();
            return Owner.GetStringNoRowCheck(columnName,RowIndex);
        }


        public byte[] GetBinary(long columnIndex)//column and type of field will be checked
        {
            ValidateIsValid();
            return Owner.GetBinaryNoRowCheck(columnIndex, RowIndex);
        }

        public byte[] GetBinary(string columnName)
        {
            ValidateIsValid();
            return Owner.GetBinaryNoRowCheck(columnName, RowIndex);
        }

        public void SetBinary(long columnIndex,byte [] value)//column and type of field will be checked
        {
            ValidateIsValid();
             Owner.SetBinaryNoRowCheck(columnIndex, RowIndex,value);
        }

        public void SetBinary(string columnName, byte[] value)
        {
            ValidateIsValid();
             Owner.SetBinaryNoRowCheck(columnName, RowIndex,value);
        }


        public long GetLong(long columnNumber)
        {
            ValidateIsValid();
            return Owner.GetLongNoRowCheck(columnNumber, RowIndex);
        }

        public long GetLong(string name)
        {
            ValidateIsValid();
            return Owner.GetLongNoRowCheck(name, RowIndex);
        }

        public DateTime GetDateTime(string name)
        {
            ValidateIsValid();
            return Owner.GetDateTimeNoRowCheck(name, RowIndex);
        }

        public DateTime GetDateTime(long columnIndex)
        {
            ValidateIsValid();
            return Owner.GetDateTimeNoRowCheck(columnIndex, RowIndex);
        }

        public  void SetDateTime(string name,DateTime dateTime)
        {
            ValidateIsValid();
            Owner.SetDateTimeNoRowCheck(name, RowIndex,dateTime);
        }

        public void SetDateTime(long columnIndex,DateTime dateTime)
        {
            ValidateIsValid();
             Owner.SetDateTimeNoRowCheck(columnIndex, RowIndex,dateTime);
        }


        public long GetMixedLong(string name)
        {
            ValidateIsValid();
            return Owner.GetMixedLongNoRowCheck(name, RowIndex);
        }

        public long GetMixedLong(long columnIndex)
        {
            ValidateIsValid();
            return Owner.GetMixedLongNoRowCheck(columnIndex, RowIndex);
        }


        public Boolean GetMixedBoolean(string name)
        {
            ValidateIsValid();
            return Owner.GetMixedBooleanNoRowCheck(name, RowIndex);
        }

        public Boolean GetMixedBoolean(long columnIndex)
        {
            ValidateIsValid();
            return Owner.GetMixedBooleanNoRowCheck(columnIndex, RowIndex);
        }


        public float GetMixedFloat(string name)
        {
            ValidateIsValid();
            return Owner.GetMixedFloatNoRowCheck(name, RowIndex);
        }

        public float GetMixedFloat(long columnIndex)
        {
            ValidateIsValid();
            return Owner.GetMixedFloatNoRowCheck(columnIndex, RowIndex);
        }

        public Double GetMixedDouble(string name)
        {
            ValidateIsValid();
            return Owner.GetMixedDoubleNoRowCheck(name, RowIndex);
        }

        public Double GetMixedDouble(long columnIndex)
        {
            ValidateIsValid();
            return Owner.GetMixedDoubleNoRowCheck(columnIndex, RowIndex);
        }

        public String GetMixedString(string name)
        {
            ValidateIsValid();
            return Owner.GetMixedStringNoRowCheck(name, RowIndex);
        }

        public String GetMixedString(long columnIndex)
        {
            ValidateIsValid();
            return Owner.GetMixedStringNoRowCheck(columnIndex, RowIndex);
        }

        public byte[] GetMixedBinary(long columnIndex)
        {
            ValidateIsValid();
            return Owner.GetMixedBinaryNoRowCheck(columnIndex, RowIndex);
        }

        public byte[] GetMixedBinary(string columnName)
        {
            ValidateIsValid();
            return Owner.GetMixedBinaryNoRowCheck(columnName, RowIndex);
        }



        public void SetMixedString(string name,String value)
        {
            ValidateIsValid();
             Owner.SetMixedStringNoRowCheck(name, RowIndex,value);
        }

        public void SetMixedString(long columnIndex,string value)
        {
            ValidateIsValid();
             Owner.SetMixedStringNoRowCheck(columnIndex, RowIndex,value);
        }


        public DateTime GetMixedDateTime(string name)
        {
            ValidateIsValid();
            return Owner.GetMixedDateTimeNoRowCheck(name, RowIndex);
        }

        public DateTime GetMixedDateTime(long columnIndex)
        {
            ValidateIsValid();
            return Owner.GetMixedDateTimeNoRowCheck(columnIndex,RowIndex);
        }

        public void SetMixedDateTime(string columnName,DateTime dateTime)
        {
            ValidateIsValid();
            Owner.SetMixedDateTimeNoRowCheck(columnName, RowIndex,dateTime);
        }

        public void SetMixedDateTime(long columnIndex,DateTime dateTime)
        {
            ValidateIsValid();
            Owner.SetMixedDateTimeNoRowCheck(columnIndex, RowIndex,dateTime);
        }



        public Table GetMixedTable(string name)
        {
            ValidateIsValid();
            return  Owner.GetMixedTableNoRowCheck(name, RowIndex);
        }

        public Table GetMixedTable(long columnIndex)
        {
            ValidateIsValid();
            return Owner.GetMixedTableNoRowCheck(columnIndex, RowIndex);
        }




        public Double GetDouble(string columnName)
        {
            ValidateIsValid();
            return Owner.GetDoubleNoRowCheck(columnName, RowIndex);
        }

        public Double GetDouble(long columnIndex)
        {
            ValidateIsValid();
            return Owner.GetDoubleNoRowCheck(columnIndex, RowIndex);
        }

        public float GetFloat(string columnName)
        {
            ValidateIsValid();
            return Owner.GetFloatNoRowCheck(columnName, RowIndex);
        }

        public float GetFloat(long columnIndex)
        {
            ValidateIsValid();
            return Owner.GetFloatNoRowCheck(columnIndex, RowIndex);
        }


        public Boolean GetBoolean(string columnName)
        {
            ValidateIsValid();
            return Owner.GetBooleanNoRowCheck(columnName, RowIndex);
        }

        public Boolean GetBoolean(long columnIndex)
        {
            ValidateIsValid();
            return Owner.GetBooleanNoRowCheck(columnIndex,RowIndex);
        }

        public long GetColumnIndex(string columnName)
        {
            ValidateIsValid();
            return Owner.GetColumnIndex(columnName);
        }

       
        public void Remove()
        {
            ValidateIsValid();
            Owner.RemoveNoCheck(RowIndex);            
        }

    }
}