using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace TightDbCSharp
{
    /// <summary>
    /// TableView is a list of pointers into a table, represented as an IEnumerable collection, but also accessible by rowIndex.
    /// TableView is usually the end result of a query being run, or the result of methods on table or tableview that return
    /// a collection of rows.
    /// TableView is IDisposable, it is recommended (not mandatory) to use using when working with tableviews
    /// TableView is IEnumerable, You can Enumerate a tableview and get Row objects, e.g. 
    /// foreach (Row in MyTableView) {Row.SetLong(0,42)//set value to 42 in all fields in column 0}}
    /// TableView also supports LinQ queries (as it is IEnumerable)
    /// The Row objects returned are merely cursors, no data is transferred until you call row.GetString(col) etc.
    /// </summary>
    public class TableView : TableOrView, IEnumerable<Row>
    {
        private Table _underlyingTable;//the table this view ultimately is viewing (not the view it is viewing, the final table being viewed. Could be a subtable)
        /// <summary>
        /// The table that this tableview is ultimately viewing - if the tableview is viewing a tableview that is viewing a.
        /// table, then it is the final table that this property contains a reference to.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public Table UnderlyingTable {
            get { return _underlyingTable; }
            private set
            {
                if (value != null && _underlyingTable==null)
                {
                    _underlyingTable = value;
                }
                else
                {
                    throw new ArgumentException("TableViewed can only be set once, and cannot be set to null");
                }
            }
            
        }//used only to make sure that a reference to the table exists until the view is disposed of


        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// IEnimerator  that can be used to iterate through the TableView, Yielding Row objects each representing a row in the tableview
        /// </returns>
        
        public IEnumerator<Row> GetEnumerator()
        {
            ValidateIsValid();
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        private class Enumerator : IEnumerator<Row> //probably overkill, current needs could be met by using yield
        {
            private long _currentRow = -1;
            private readonly TableView _myTableView;//the table or view we are iterating            
            private readonly int _myTableVersion;//version of the underlying table we are viewing
            private readonly Table _myUnderlyingTable;

            public Enumerator(TableView tableView)
            {
                _myTableView = tableView;
                _myUnderlyingTable=tableView.UnderlyingTable;
                _myTableVersion = _myUnderlyingTable.Version;
            }

            //todo:peformance test if inlining this manually will do any good
            
            private void ValidateVersion()
            {
                if (_myTableVersion != _myUnderlyingTable.Version)
                    throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,"Table View Iteration failed at row {0} because the table had rows inserted or deleted", _currentRow));
            }

            public Row Current
            {
                get
                {         
                    ValidateVersion();
                    return new Row(_myTableView, _currentRow);
                }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                ValidateVersion();
                return ++_currentRow < _myTableView.Size;
            }

            public void Reset()
            {
                _currentRow = -1;
            }

            public void Dispose()
            {
                //_myTableView = null; //remove reference to Table class
            }
        }


        internal override Spec GetSpec()
        {
            return UnderlyingTable.Spec;
        }

        /// <summary>
        /// Return a Row cursor for the row specified by the zero based rowIndex
        /// </summary>
        /// <param name="rowIndex">Zero based row Index of the Row to return</param>
        public Row this[long rowIndex]
        {
            get
            {
                ValidateIsValid();
                ValidateRowIndex(rowIndex);
                return RowForIndexNoCheck(rowIndex);
            }
        }

        private Row RowForIndexNoCheck(long rowIndex)
        {
            return new Row(this, rowIndex);            
        }
        /// <summary>
        /// *not in c++ binding. Is in java binding
        /// see similar implementation in Table          
        /// </summary>
        /// <returns>Row cursor referencing the last row in the table</returns>
        /// <exception cref="InvalidOperationException">Thrown if the table is empty</exception>
        public Row Last()
        {
            ValidateIsValid();
            long s = Size;
            if (s > 0)
            {
                  return RowForIndexNoCheck(s-1);
            }
            throw new InvalidOperationException("Last called on a TableView with no rows in it");
        }
        //*/
 

        /// <summary>
        /// Returns true if it is safe to use this table view
        /// Returns false if the table view should not be used, usually
        /// this happens if someone changes the table in other ways than through
        /// this TableView.
        /// </summary>
        /// <returns>True if it is okay to use this tableview, false if all calls are illegal except dispose</returns>
        public bool IsValid()
        {
            //call to core to get info if this tableview is attached or not (not implemented in core)
            //until core has such functionality, we do better than nothing and check that the table version is
            //unchanged since the tableview was created. If someone made modifications to the table we are
            //viewing, we consider this tableview as invalid            
            return  UnderlyingTable.IsValid() && (UnderlyingTable.Version==Version);
        }

        //this can throw if the underlying table had inserted or removed rows or if the underlying table has become invalid itself
        internal override void ValidateIsValid()
        {
            if (!IsValid())
            {
                throw new InvalidOperationException("Table view is no longer valid. No operations except calling Is Valid is allowed");

            }            
        }

        internal override void ClearNoCheck()
        {
            UnsafeNativeMethods.TableViewClear(this);
        }

        /// <summary>
        /// This method will ask c++ to dispose of a tableView object.
        /// this method is for internal use only
        /// it will automatically be called when the TableView object is disposed (or garbage collected)
        /// In fact, you should not at all it on your own
        /// </summary>
        protected override void ReleaseHandle()
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


        public long GetSourceNdx(long rowIndex)
        {
            return UnsafeNativeMethods.TableViewGetSourceNdx(this,rowIndex);
        }


        /// <summary>
        /// Returns a string with a Json representation of the TableView.
        /// It is planned to also have a stream based method
        /// </summary>
        /// <returns>String with a Json version of all the rows in the tableview</returns>
        internal override string ToJsonNoCheck()
        {            
            return UnsafeNativeMethods.TableViewToJson(this);
        }


        /// <summary>
        /// Returns a string with a Json representation of the TableView.
        /// It is planned to also have a stream based method
        /// </summary>
        /// <returns>String with a Json version of all the rows in the tableview</returns>
        internal override string ToStringNoCheck()
        {
            return UnsafeNativeMethods.TableViewToString(this);
        }


        /// <summary>
        /// Returns a string with a Json representation of the TableView.
        /// It is planned to also have a stream based method
        /// </summary>
        /// <returns>String with a Json version of all the rows in the tableview</returns>
        internal override string ToStringNoCheck(long limit)
        {
            return UnsafeNativeMethods.TableViewToString(this,limit);
        }                              

        //Todo:Unit tests if tableview invalidation works when the underlying table was modified through the tableview
        //what i mean:  test it is legal to delete a row through the tableview
        //test that the tableview invalidates if a row is deleted through the table
        //test that the tableview invaiddates if a row is deleted thorugh tanother table in the group


        //todo:make note in asana that optimally, subtables and tableviews should be unique per handle - that is group should return
        //the same table object if called and asked for the same table object one time more
        //and table should do the same with subtables.
        //this will fix the above mentioned unit test state bug


        internal override void RemoveNoCheck(long rowIndex)
        {
            UnsafeNativeMethods.TableViewRemoveRow(this, rowIndex);
            ++UnderlyingTable.Version;//this is a seperate object            
            ++Version;//this is this tableview's version (currently not being checked for in the itereator, but in the future
            //perhaps a tableview can somehow change while the underlying table did in fact not change at all
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
            UnsafeNativeMethods.TableViewSetBinary(this, columnIndex, rowIndex, value);
        }

        internal override void SetSubTableNoCheck(long columnIndex, long rowIndex, Table value)
        {
            UnsafeNativeMethods.TableViewSetSubTable(this,columnIndex,rowIndex,value);
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
            return UnsafeNativeMethods.TableViewGetMixedString(this, columnIndex, rowIndex);
        }

        internal override byte[] GetMixedBinaryNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableViewGetMixedBinary(this, columnIndex, rowIndex);
        }

        internal override Table GetMixedSubTableNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableViewGetSubTable(this, columnIndex, rowIndex);//ordinary getsubtable also works with mixed columns
        }

        internal override byte[] GetBinaryNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableViewGetBinary(this, columnIndex, rowIndex);
        }

        internal override Table GetSubTableNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableViewGetSubTable(this, columnIndex, rowIndex);
        }

        internal override long GetSubTableSizeNoCheck(long columnIndex, long rowIndex)
        {
            return UnsafeNativeMethods.TableViewGetSubTableSize(this,columnIndex, rowIndex);
        }

        internal override void ClearSubTableNoCheck(long columnIndex, long rowIndex)
        {
            UnsafeNativeMethods.TableViewClearSubTable(this,columnIndex,rowIndex);
        }
        //todo:unit test that after clear subtable, earlier subtable wrappers to the cleared table are invalidated

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
        //todo:unit test that checks if subtables taken out from the mixed are invalidated when a new subtable is put into the mixed
        //todo also test that invalidation of iterators, and row columns and rowcells work ok

        //a copy of source will be set into the field

        internal override void SetMixedSubTableNoCheck(long columnIndex, long rowIndex, Table source)
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

        internal override void SetIntNoCheck(long columnIndex, long rowIndex, int value)
        {
            UnsafeNativeMethods.TableViewSetInt(this, columnIndex, rowIndex, value);
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

        internal override string ObjectIdentification()
        {
           // ValidateIsValid();
            return String.Format(CultureInfo.InvariantCulture,"TableView:{0}", Handle);
        }

        
        internal TableView(Table underlyingTableBeing, IntPtr tableViewHandle,bool shouldbedisposed)
        {
            try
            {
                UnderlyingTable = underlyingTableBeing;
                Version = underlyingTableBeing.Version;
                    //this tableview should invalidate itself if that version changes
                SetHandle(tableViewHandle, shouldbedisposed);
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
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


        internal override TableView FindAllBoolNoCheck(long columnIndex, bool value)
        {
            return UnsafeNativeMethods.TableViewFindAllBool(this,columnIndex, value);
        }

        internal override TableView FindAllDateNoCheck(long columnIndex, DateTime value)
        {
            return UnsafeNativeMethods.TableViewFindAllDateTime(this, columnIndex, value);
        }

        internal override TableView FindAllFloatNoCheck(long columnIndex, float value)
        {
            return UnsafeNativeMethods.TableViewFindAllFloat(this,columnIndex, value);
        }

        internal override TableView FindAllDoubleNoCheck(long columnIndex, double value)
        {
            return UnsafeNativeMethods.TableViewFindAlldouble(this, columnIndex, value);
        }

        internal override TableView FindAllStringNoCheck(long columnIndex, string value)
        {
            return UnsafeNativeMethods.TableViewFindAllString(this, columnIndex, value);
        }

        internal override TableView FindAllBinaryNoCheck(long columnIndex, byte[] value)
        {
            throw new NotImplementedException("tableView findAllBinary not implemented yet ");
            //not implemented yet in core
            //return UnsafeNativeMethods.tableViewFindAllBinary( columnIndex, value);
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
