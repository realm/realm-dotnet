using System;
using System.Globalization;

namespace TightDbCSharp
{

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

        internal override void SetDateTimeNoCheck(long columnIndex, long rowIndex, DateTime value)
        {
            UnsafeNativeMethods.TableViewSetDate(this,columnIndex,rowIndex,value);
        }

        
        internal override Table GetMixedSubTableNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableViewGetSubTable(this, columnIndex, rowIndex);
        }

        internal override Table GetSubTableNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableViewGetSubTable(this, columnIndex, rowIndex);
        }

        internal override void SetStringNoCheck(long columnIndex, long rowIndex, string value)
        {
            UnsafeNativeMethods.TableViewSetString(this , columnIndex, rowIndex, value);
        }

        internal override String GetStringNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableviewGetString(this, columnIndex, rowIndex);
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


        internal override void SetMixedLongNoCheck(long columnIndex, long rowIndex, long value)
        {
            UnsafeNativeMethods.TableViewSetMixedLong(this, columnIndex, rowIndex, value);
        }

        internal override long GetMixedLongNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableViewGetMixedInt(this, columnIndex, rowIndex);
        }

        internal override Double GetMixedDoubleNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableViewGetMixedDouble(this, columnIndex, rowIndex);
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

        internal override Boolean GetBooleanNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableViewGetBool(this, columnIndex, rowIndex);
        }

        internal override TableView FindAllIntNoCheck(long columnIndex, long value)
        {
            return UnsafeNativeMethods.TableViewFindAllInt(this,  columnIndex,value);
        }

        internal override void SetBooleanNoCheck(long columnIndex, long rowIndex, Boolean value)
        {
            UnsafeNativeMethods.TableViewSetBool(this, columnIndex, rowIndex, value);
        }

        //only call if You are certain that 1: The field type is Int, 2: The columnIndex is in range, 3: The rowIndex is in range
        internal override long GetLongNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableViewGetInt(this, columnIndex, rowIndex);
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
