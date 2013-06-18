using System;
using System.Globalization;

namespace TightDbCSharp
{
    //this would probably be called rowcollection or some such if adhering to usual c# conventions. But as tightdb has a tableview term, we 
    //re-use that term here. TableView can be thought of as a list of pointers to some rows in an underlying table. The view has usually been created using
    //a query, or some other construct that selects some, but usually not all the underlying rows.
    //this class wraps a c++ TableView class
    public class TableView : TableOrView
    {
                //following the dispose pattern discussed here http://dave-black.blogspot.dk/2011/03/how-do-you-properly-implement.html
        //a good explanation can be found here http://stackoverflow.com/questions/538060/proper-use-of-the-idisposable-interface

        internal Table TableViewed { get; set; }//used only to make sure that a reference to the table exists until the view is disposed of

        internal override Spec GetSpec()
        {
            return TableViewed.Spec;
        }


        public Row this[long rowIndex]
        {
            get
            {
                ValidateRowIndex(rowIndex);
                return new Row(this, rowIndex);
            }

        }



        //This method will ask c++ to dispose of a table object created by table_new.
        //this method is for internal use only
        //it will automatically be called when the table object is disposed (or garbage collected)
        //In fact, you should not at all it on your own
        internal  override void ReleaseHandle()
        {
            UnsafeNativeMethods.TableViewUnbind(this);
        }

        internal override DateTime GetMixedDateTimeNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableViewGetMixedDateTime(this, columnIndex, rowIndex);
        }

        //-1 of the column string does not specify a column
        internal override long GetColumnIndexNoCheck(String name)
        {
            return UnsafeNativeMethods.TableViewGetColumnIndex(this,name);            
        }

        internal override long SumLongNoCheck(long columnIndex)
        {
            return UnsafeNativeMethods.TableViewSumLong(this, columnIndex);
        }

        internal override double SumFloatNoCheck(long columnIndex)
        {
            return UnsafeNativeMethods.TableViewSumFloat(this, columnIndex);
        }

        internal override double SumDoubleNoCheck(long columnIndex)
        {
            return UnsafeNativeMethods.TableViewSumDouble(this, columnIndex);
        }

        internal override long MinimumLongNoCheck(long columnIndex)
        {
            return UnsafeNativeMethods.TableViewMinimumLong(this, columnIndex);
        }

        internal override float MinimumFloatNoCheck(long columnIndex)
        {
            return UnsafeNativeMethods.TableViewMinimumFloat(this, columnIndex);
        }

        internal override double MinimumDoubleNoCheck(long columnIndex)
        {
            return UnsafeNativeMethods.TableViewMinimumDouble(this, columnIndex);
        }

        internal override long MaximumLongNoCheck(long columnIndex)
        {
            return UnsafeNativeMethods.TableViewMaximum(this, columnIndex);
        }

        internal override float MaximumFloatNoCheck(long columnIndex)
        {
            return UnsafeNativeMethods.TableViewMaximumFloat(this, columnIndex);
        }

        internal override double MaximumDoubleNoCheck(long columnIndex)
        {
            return UnsafeNativeMethods.TableViewMaximumDouble(this, columnIndex);
        }

        internal override double AverageLongNoCheck(long columnIndex)
        {
            return UnsafeNativeMethods.TableViewAverageLong(this, columnIndex);
        }

        internal override double AverageFloatNoCheck(long columnIndex)
        {
            return UnsafeNativeMethods.TableViewAverageFloat(this, columnIndex);
        }

        internal override double AverageDoubleNoCheck(long columnIndex)
        {
            return UnsafeNativeMethods.TableViewAverageDouble(this, columnIndex);
        }

        internal override long CountLongNoCheck(long columnIndex, long target)
        {
            return UnsafeNativeMethods.TableViewCountLong(this,columnIndex, target);
        }

        internal override long CountFloatNoCheck(long columnIndex, float target)
        {
            return UnsafeNativeMethods.TableViewCountFloat(this, columnIndex, target);
        }

        internal override long CountStringNoCheck(long columnIndex, string target)
        {
            return UnsafeNativeMethods.TableViewCountString(this, columnIndex, target);
        }

        internal override long CountDoubleNoCheck(long columnIndex, double target)
        {
            return UnsafeNativeMethods.TableViewCountDouble(this, columnIndex, target);
        }

        public override string ToJson()
        {
            return UnsafeNativeMethods.TableViewToJson(this);
        }


        internal override void RemoveNoCheck(long rowIndex)
        {
            UnsafeNativeMethods.TableViewRemove(this, rowIndex);
        }

        internal override void SetMixedFloatNoCheck(long columnIndex, long rowIndex, float value)
        {
            UnsafeNativeMethods.TableViewSetMixedFloat(this,columnIndex,rowIndex,value);
        }

        internal override void SetFloatNoCheck(long columnIndex, long rowIndex, float value)
        {
           UnsafeNativeMethods.TableViewSetFloat(this, columnIndex,rowIndex,value);   
        }

        internal override void SetMixedDoubleNoCheck(long columnIndex, long rowIndex, double value)
        {
            UnsafeNativeMethods.TableViewSetMixedDouble(this,columnIndex,rowIndex,value);
        }

        internal override void SetDoubleNoCheck(long columnIndex, long rowIndex, double value)
        {
            UnsafeNativeMethods.TableViewSetDouble(this,columnIndex ,rowIndex,value);
        }

        //this method takes a DateTime
        //the value of DateTime.ToUTC will be stored in the database, as TightDB always store UTC dates
        internal override void SetMixedDateTimeNoCheck(long columnIndex, long rowIndex, DateTime value)
        {
            UnsafeNativeMethods.TableViewSetMixedDate(this,columnIndex,rowIndex,value);
        }

        internal override void SetBinaryNoCheck(long columnIndex, long rowIndex, byte[] value)
        {
            throw new NotImplementedException();
        }

        internal override void SetDateTimeNoCheck(long columnIndex, long rowIndex, DateTime value)
        {
            UnsafeNativeMethods.TableViewSetDate(this,columnIndex,rowIndex,value);
        }


        internal override bool GetMixedBoolNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableViewGetMixedBool(this,columnIndex, rowIndex);
        }

        internal override String GetMixedStringNoCheck(long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        internal override byte[] GetMixedBinaryNoCheck(long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        internal override Table GetMixedSubTableNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableViewGetSubTable(this, columnIndex, rowIndex);//ordinary getsubtable also works with mixed columns
        }

        internal override byte[] GetBinaryNoCheck(long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        internal override Table GetSubTableNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableViewGetSubTable(this, columnIndex, rowIndex);
        }

        internal override void ClearSubTableNoCheck(long columnIndex, long rowIndex)
        {
            UnsafeNativeMethods.TableViewClearSubTable(this,columnIndex,rowIndex);
        }

        internal override void SetStringNoCheck(long columnIndex, long rowIndex, string value)
        {
            UnsafeNativeMethods.TableViewSetString(this , columnIndex, rowIndex, value);
        }

        internal override String GetStringNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableviewGetString(this, columnIndex, rowIndex);
        }

        internal override void SetMixedLongNoCheck(long columnIndex, long rowIndex, long value)
        {
            UnsafeNativeMethods.TableViewSetMixedLong(this, columnIndex, rowIndex, value);
        }
        internal override void SetMixedBoolNoCheck(long columnIndex, long rowIndex, bool value)
        {
            UnsafeNativeMethods.TableViewSetMixedBool(this, columnIndex, rowIndex, value);
        }

        internal override void SetMixedStringNoCheck(long columnIndex, long rowIndex, string value)
        {
            UnsafeNativeMethods.TableViewSetMixedString(this,columnIndex,rowIndex,value);
        }

        internal override void SetMixedBinaryNoCheck(long columnIndex, long rowIndex, byte[] value)
        {
            UnsafeNativeMethods.TableViewSetMixedBinary(this,columnIndex, rowIndex, value);
        }

        //might be used if You want an empty subtable set up and then change its contents and layout at a later time
        internal override void SetMixedEmptySubtableNoCheck(long columnIndex, long rowIndex)
        {
            UnsafeNativeMethods.TableViewSetMixedEmptySubTable(this, columnIndex, rowIndex);
        }

        //a copy of source will be set into the field

        internal override void SetMixedSubtableNoCheck(long columnIndex, long rowIndex, Table source)
        {
            UnsafeNativeMethods.TableViewSetMixedSubTable(this, columnIndex, rowIndex, source);
        }



        internal override long GetMixedLongNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableViewGetMixedInt(this, columnIndex, rowIndex);
        }

        internal override Double GetMixedDoubleNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableViewGetMixedDouble(this, columnIndex, rowIndex);
        }

        internal override float GetMixedFloatNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableViewGetMixedFloat(this, columnIndex, rowIndex);
        }

        
        internal override DateTime GetDateTimeNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableViewGetDateTime(this, columnIndex, rowIndex);
        }




        internal override DataType GetMixedTypeNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableViewGetMixedType(this, columnIndex, rowIndex);
        }

        internal override void SetLongNoCheck(long columnIndex, long rowIndex, long value)
        {
            UnsafeNativeMethods.TableViewSetLong(this, columnIndex, rowIndex, value);
        }

        internal override string GetColumnNameNoCheck(long columnIndex)//unfortunately an int, bc tight might have been built using 32 bits
        {
            return UnsafeNativeMethods.TableViewGetColumnName(this, columnIndex);
        }


        internal override long GetColumnCount()
        {
            return UnsafeNativeMethods.TableViewGetColumnCount(this);
        }

        internal override DataType ColumnTypeNoCheck(long columnIndex)
        {
            return UnsafeNativeMethods.TableViewGetColumnType(this, columnIndex);
        }

        public override string ObjectIdentification()
        {
            return String.Format(CultureInfo.InvariantCulture,"TableView:{0}", Handle);
        }

        
        internal TableView(IntPtr tableViewHandle,bool shouldbedisposed)
        {
            SetHandle(tableViewHandle,shouldbedisposed);
        }

        internal override long GetSize()
        {
            return UnsafeNativeMethods.TableViewSize(this);
        }

        internal override Boolean GetBoolNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableViewGetBool(this, columnIndex, rowIndex);
        }

        internal override TableView FindAllIntNoCheck(long columnIndex, long value)
        {
            return UnsafeNativeMethods.TableViewFindAllInt(this,  columnIndex,value);
        }

        internal override TableView FindAllStringNoCheck(long columnIndex, string value)
        {
            return UnsafeNativeMethods.TableViewFindAllString(this, columnIndex, value);
        }



        internal override void SetBoolNoCheck(long columnIndex, long rowIndex, Boolean value)
        {
            UnsafeNativeMethods.TableViewSetBool(this, columnIndex, rowIndex, value);
        }

        //only call if You are certain that 1: The field type is Int, 2: The columnIndex is in range, 3: The rowIndex is in range
        internal override long GetLongNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableViewGetInt(this, columnIndex, rowIndex);
        }

        internal override Double GetDoubleNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableViewGetDouble(this, columnIndex, rowIndex);
        }

        internal override float GetFloatNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableViewGetFloat(this, columnIndex, rowIndex);
        }


        internal override long FindFirstBinaryNoCheck(long columnIndex, byte[] value)
        {
            return UnsafeNativeMethods.TableViewFindFirstBinary(this, columnIndex, value);
        }

        internal override long FindFirstIntNoCheck(long columnIndex, long value)
        {
            return UnsafeNativeMethods.TableViewFindFirstInt(this, columnIndex, value);
        }

        internal override long FindFirstStringNoCheck(long columnIndex, string value)
        {
            return UnsafeNativeMethods.TableViewFindFirstString(this, columnIndex, value);
        }

        internal override long FindFirstDoubleNoCheck(long columnIndex, double value)
        {
            return UnsafeNativeMethods.TableViewFindFirstDouble(this, columnIndex, value);
        }

        internal override long FindFirstFloatNoCheck(long columnIndex, float value)
        {
            return UnsafeNativeMethods.TableViewFindFirstFloat(this, columnIndex, value);
        }

        internal override long FindFirstDateNoCheck(long columnIndex, DateTime value)
        {
            return UnsafeNativeMethods.TableViewFindFirstDate(this, columnIndex, value);
        }

        internal override long FindFirstBoolNoCheck(long columnIndex, bool value)
        {
            return UnsafeNativeMethods.TableViewFindFirstBool(this, columnIndex, value);
        }




    }
}
