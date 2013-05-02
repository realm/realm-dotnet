using System;
using System.Collections.Generic;
using System.Globalization;

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

        internal string GetColumnNameNoCheck(long columnIndex)
        {
            return Owner.GetColumnNameNoCheck(columnIndex);
        }

        internal void SetColumnNameNoCheck(long columnIndex, string columnName)
        {
            Owner.SetColumnNameNoCheck(columnIndex,columnName);
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

        //todo:unit test
        public void SetRow(params object[] rowContents)
        {
            if (rowContents.Length!=ColumnCount)
                throw new ArgumentOutOfRangeException("rowContents",String.Format(CultureInfo.InvariantCulture,"SetRow called with {0} objects, but there are only {1} columns in the table",rowContents.Length,ColumnCount));
            for (long ix = 0; ix<ColumnCount;ix++ )
            {
                object element = rowContents[ix];//element is parameter number ix
                //first do a switch on directly compatible types
                Type elementType = element.GetType();//performance hit as it is not neccessarily used, but used many blaces below
                
                switch (ColumnTypeNoCheck(ix))
                {
                    case DataType.Int:                        
                        Owner.SetLongNoCheck(ix,RowIndex,(long)element);//this throws exceptions if called with something too weird
                        break;
                    case DataType.Bool:
                        Owner.SetBooleanNoCheck(ix,RowIndex,(Boolean)element);
                        break;
                    case DataType.String:
                        Owner.SetStringNoCheck(ix,RowIndex,(string)element);
                        break;
                    case DataType.Binary://todo:implement
                        break;
                    case DataType.Table://todo:test thoroughly with unit test, also with invalid data
                        Table t = Owner.GetSubTableNoCheck(ix, RowIndex);//The type to go into a subtable has to be an array of arrays of objects
                                                                        
                        if (element != null)//if You specify null for a subtable we do nothing null means You intend to fill it in later
                        {
                            

                            if (elementType != typeof (Array))
                            {
                                throw new ArgumentOutOfRangeException(String.Format(CultureInfo.InvariantCulture,
                                                                                    "SetRow called with a non-array type {0} for the subtable column {1}",
                                                                                    elementType,
                                                                                    GetColumnNameNoCheck(ix)));
                            }
                            //at this point we know that element is an array of some sort, hopefully containing valid records for our subtable                                                
                                foreach (Array arow in (Array) element)//typecast because we already ensured element is an array,and that element is not null
                                {
                                    t.AddRow(arow);
                                }
                        }
                        break;
                    case DataType.Mixed://Try to infer the mixed type to use from the type of the object from the user
                        

                        if (                            
                            elementType == typeof(Byte) ||//byte Byte
                            elementType == typeof(Int32) ||//int,int32
                            elementType == typeof(Int64) ||//long,int64
                            elementType == typeof(Int16) ||//int16,short
                            elementType == typeof(SByte) ||//sbyte SByte                           
                            elementType == typeof(UInt16) ||//ushort,uint16
                            elementType == typeof(UInt32) ||//uint
                            elementType == typeof(UInt64)//ulong                            
                            )
                        {   
                            Owner.SetMixedLongNoCheck(ix,RowIndex,(long)element);
                            break;
                        }

                        if (elementType == typeof(Single))//float, Single
                        {
                            Owner.SetMixedFloatNoCheck(ix, RowIndex, (float) element);
                            break;
                        }

                        if (elementType == typeof(Double))
                        {
                            Owner.SetMixedDoubleNoCheck(ix, RowIndex, (Double) element);
                            break;
                        }

                        if (elementType == typeof(DateTime))
                        {
                            Owner.SetMixedDateTimeNoCheck(ix, RowIndex, (DateTime)element);
                        }
                        
                        break;
                    case DataType.Date:
                        Owner.SetDateNoCheck(ix, RowIndex, (DateTime)element);
                        break;
                    case DataType.Float:
                        Owner.SetFloatNoCheck(ix, RowIndex, (float) element);
                        break;
                    case DataType.Double:
                        Owner.SetDoubleNoCheck(ix, RowIndex, (Double) element);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("rowContents",String.Format(CultureInfo.InvariantCulture,"An element ix:{0} of type {1} in a row sent to AddRow, is not of a supported tightdb type ",ix,elementType));
                }
            }            
        }



        //todo:implement
        public void Remove()
        {
            RowIndex = -2;//mark this row as invalid
            throw new NotImplementedException();
        }
    }
}