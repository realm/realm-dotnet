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

        //This method will ask c++ to dispose of a table object created by table_new.
        //this method is for internal use only
        //it will automatically be called when the table object is disposed (or garbage collected)
        //In fact, you should not at all it on your own
        internal  override void ReleaseHandle()
        {
            UnsafeNativeMethods.TableViewUnbind(this);
        }

        internal override Table GetMixedSubTableNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableViewGetSubTable(this, columnIndex, rowIndex);
        }

        internal override Table GetSubTableNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableViewGetSubTable(this, columnIndex, rowIndex);
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

        internal override DataType GetMixedTypeNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableViewGetMixedType(this, columnIndex, rowIndex);
        }

        internal override void SetLongNoCheck(long columnIndex, long rowIndex, long value)
        {
            UnsafeNativeMethods.TableViewSetLong(this, columnIndex, rowIndex, value);
        }

        public override string GetColumnName(long columnIndex)//unfortunately an int, bc tight might have been built using 32 bits
        {
            return UnsafeNativeMethods.TableViewGetColumnName(this, columnIndex);
        }


        internal override long GetColumnCount()
        {
            return UnsafeNativeMethods.TableViewGetColumnCount(this);
        }

        public override DataType ColumnType(long columnIndex)
        {
            return UnsafeNativeMethods.
                TableViewGetColumnType(this, columnIndex);
        }

        public override string ObjectIdentification()
        {
            return String.Format(CultureInfo.InvariantCulture,"TableView:{0}", Handle);
        }

        internal TableView(IntPtr tableViewHandle,bool shouldbedisposed)
        {
            SetHandle(tableViewHandle,shouldbedisposed);
        }

        public override long Size()
        {
            return UnsafeNativeMethods.TableViewSize(this);
        }

        //only call if You are certain that 1: The field type is Int, 2: The columnIndex is in range, 3: The rowIndex is in range
        internal override long GetLongNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableViewGetInt(this, columnIndex, rowIndex);
        }

        

    }
}
