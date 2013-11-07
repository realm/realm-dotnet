using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace TightDbCSharp
{

    //abstract methods are those that are implemented differently in table and tableview
    //methods implemented here , work with both types
    /// <summary>
    /// TableOrView contains methods that exist both in tables and in tableviews
    /// Table and Tableview inherit from TableOrView and implements their own versions
    /// of these methods
    /// </summary>
    public abstract class TableOrView : Handled//, IEnumerable<Row>
    {
// ReSharper disable MemberCanBeProtected.Global
        internal abstract long GetSize();

        internal abstract long GetColumnCount();
        internal abstract String GetColumnNameNoCheck(long columnIndex);
        internal abstract Spec GetSpec();


        internal abstract DataType ColumnTypeNoCheck(long columnIndex);

        //getters for table and tableview
        internal abstract long GetLongNoCheck(long columnIndex, long rowIndex);
        internal abstract Boolean GetBoolNoCheck(long columnIndex, long rowIndex);
        internal abstract String GetStringNoCheck(long columnIndex, long rowIndex);
        internal abstract byte[] GetBinaryNoCheck(long columnIndex, long rowIndex);
        internal abstract Table GetSubTableNoCheck(long columnIndex, long rowIndex);
        internal abstract long GetSubTableSizeNoCheck(long columnIndex, long rowIndex);
        //mixed is handled by type named GetMixedxxxx methods below
        internal abstract DateTime GetDateTimeNoCheck(long columnIndex, long rowIndex);
        internal abstract float GetFloatNoCheck(long columnIndex, long rowIndex);
        internal abstract Double GetDoubleNoCheck(long columnIndex, long rowIndex);

        internal abstract DataType GetMixedTypeNoCheck(long columnIndex, long rowIndex);

        //getters for table and tableview mixed columns
        internal abstract long GetMixedLongNoCheck(long columnIndex, long rowIndex);
        //getmixedint not implemented as it wouldve been slower than calling getmixedlong
        //as it would have to check the size of the value returned form core
        
        internal abstract bool GetMixedBoolNoCheck(long columnIndex, long rowIndex);
        internal abstract String GetMixedStringNoCheck(long columnIndex, long rowIndex);
        internal abstract byte[] GetMixedBinaryNoCheck(long columnIndex, long rowIndex);
        internal abstract Table GetMixedSubTableNoCheck(long columnIndex, long rowIndex);
        //mixed mixed is not allowed
        internal abstract DateTime GetMixedDateTimeNoCheck(long columnIndex, long rowIndex);
        internal abstract float GetMixedFloatNoCheck(long columnIndex, long rowIndex);
        internal abstract Double GetMixedDoubleNoCheck(long columnIndex, long rowIndex);

        internal abstract long GetColumnIndexNoCheck(string name); //-1 if CI does not exists



        //aggregate functions that are common to table and tableview are listed here. 
        //Note that more functions exist, that are only availlable with Table
        //in the near future, those will, however, be implemented in tableview in the c++ code, so 
        //they are available here, but throws exceptions if called

        //count aggregrates takes a target value, count will only count rows that has this value
        //count only supported in table for the time being. In tablewiev count will always return 0
        internal abstract long CountLongNoCheck(long columnIndex, long target);
        internal abstract long CountFloatNoCheck(long columnIndex, float target);
        internal abstract long CountStringNoCheck(long columnIndex, string target);
        internal abstract long CountDoubleNoCheck(long columnIndex, Double target);

        internal abstract long SumLongNoCheck(long columnIndex);
        internal abstract double SumFloatNoCheck(long columnIndex);
        internal abstract double SumDoubleNoCheck(long columnIndex);

        internal abstract long MinimumLongNoCheck(long columnIndex);
        internal abstract float MinimumFloatNoCheck(long columnIndex);
        internal abstract double MinimumDoubleNoCheck(long columnIndex);
        internal abstract DateTime MinimumDateTimeNoCheck(long columnIndex);

        internal abstract long MaximumLongNoCheck(long columnIndex);
        internal abstract float MaximumFloatNoCheck(long columnIndex);
        internal abstract double MaximumDoubleNoCheck(long columnIndex);
        internal abstract DateTime MaximumDateTimeNoCheck(long columnIndex);

        //average only supported in table for the time being. In tablewiev average will always return 0
        internal abstract double AverageLongNoCheck(long columnIndex);
        internal abstract double AverageFloatNoCheck(long columnIndex);
        internal abstract double AverageDoubleNoCheck(long columnIndex);


        /// <summary>
        /// Accessible if you inherit from TableOrView, even as a user of the API.
        /// Do not call. If columnIndex is invalid, database may become corrupted.
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal abstract long FindFirstIntNoCheck(long columnIndex, long value);
        internal abstract long FindFirstStringNoCheck(long columnIndex, string value);
        internal abstract long FindFirstBinaryNoCheck(long columnIndex, byte[] value);
        internal abstract long FindFirstDoubleNoCheck(long columnIndex, double value);
        internal abstract long FindFirstFloatNoCheck(long columnIndex, float value);
        internal abstract long FindFirstDateNoCheck(long columnIndex, DateTime value);
        internal abstract long FindFirstBoolNoCheck(long columnIndex, bool value);




        internal abstract TableView FindAllIntNoCheck(long columnIndex, long value);
        internal abstract TableView FindAllBoolNoCheck(long columnIndex, bool value);
        internal abstract TableView FindAllDateNoCheck(long columnIndex, DateTime value);
        internal abstract TableView FindAllFloatNoCheck(long columnIndex, float value);
        internal abstract TableView FindAllDoubleNoCheck(long columnIndex, double value);
        internal abstract TableView FindAllBinaryNoCheck(long columnIndex, byte[] value);
        internal abstract TableView FindAllStringNoCheck(long columnIndex, string value);

        internal abstract string ToJsonNoCheck();

        /// <summary>
        /// Return a string with a Json representation of this table.
        /// In the future, a stream based version will be available too.
        /// </summary>
        /// <returns>String with Json representation</returns>
        public string ToJson()
        {
            ValidateIsValid();
            return ToJsonNoCheck();
        }

        internal abstract string ToStringNoCheck();
        internal abstract string RowToStringNoCheck(long rowIndex);

        /// <summary>
        /// Return a string with a Human readable representation of this table.
        /// Beware that ToString only returns up to 500 records
        /// If You need more records (or fewer) call TostringLimit
        /// </summary>
        /// <returns>String with Json representation</returns>
        public override string ToString()
        {
            ValidateIsValid();
            return ToStringNoCheck();
        }

        internal abstract string ToStringNoCheck(long limit);

        /// <summary>
        /// Return a string with a Human readable representation of this table.
        /// Beware that ToString only returns up to 500 records
        /// If You need more records (or fewer) call TostringLimit
        /// </summary>
        /// <returns>String with Json representation</returns>
        public string ToString(long limit)
        {
            ValidateIsValid();
            ValidateIsPositive(limit);
            return ToStringNoCheck(limit);
        }

        /// <summary>
        /// Return a string with a Human readable representation of the specified row
        /// </summary>
        /// <returns>String with row data in human readable form</returns>
        public string RowToString(long rowIndex)
        {
            ValidateIsValid();
            ValidateRowIndex(rowIndex);
            return RowToStringNoCheck(rowIndex);
        }

        private static void ValidateIsPositive(long value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException("value", "rows cannot be negative");
            }
        }

        internal abstract void SetLongNoCheck(long columnIndex, long rowIndex, long value);
        internal abstract void SetIntNoCheck(long columnIndex, long rowIndex, int value);
 
        //does not validate parametres or types
        internal abstract void SetBoolNoCheck(long columnIndex, long rowIndex, Boolean value);
        internal abstract void SetStringNoCheck(long columnIndex, long rowIndex, string value);
        internal abstract void SetBinaryNoCheck(long columnIndex, long rowIndex, byte[] value);
        internal abstract void SetSubTableNoCheck(long columnIndex, long rowIndex, Table value);
        internal abstract void SetDateTimeNoCheck(long columnIndex, long rowIndex, DateTime value);
        internal abstract void SetDoubleNoCheck(long columnIndex, long rowIndex, double value);
        internal abstract void SetFloatNoCheck(long columnIndex, long rowIndex, float value);

        internal abstract void SetMixedLongNoCheck(long columnIndex, long rowIndex, long value);
        internal abstract void SetMixedIntNoCheck(long columnIndex, long rowIndex, int value);
        internal abstract void SetMixedBoolNoCheck(long columnIndex, long rowIndex, bool value);
        internal abstract void SetMixedStringNoCheck(long columnIndex, long rowIndex, String value);
        internal abstract void SetMixedBinaryNoCheck(long columnIndex, long rowIndex, Byte[] value);
        internal abstract void SetMixedSubTableNoCheck(long columnIndex, long rowIndex, Table source);
        //will populate the mixed subtable field with a copy of the contents of source
        internal abstract void SetMixedEmptySubtableNoCheck(long columnIndex, long rowIndex);
        //mixed mixed makes no sense
        internal abstract void SetMixedDateTimeNoCheck(long columnIndex, long rowIndex, DateTime value);
        internal abstract void SetMixedFloatNoCheck(long columnIndex, long rowIndex, float value);
        internal abstract void SetMixedDoubleNoCheck(long columnIndex, long rowIndex, double value);

        internal abstract void ClearSubTableNoCheck(long columnIndex, long rowIndex);

        internal abstract void ValidateIsValid();//tables will call on , TableView's will ask their Table
        
        //Are disabled because all these methods must be hidden from users outside the API as they
        //potentially create database corrupton. ReSharper suggest making them protected, which would
        //allow a user to subclass Table and then call these methods.
        //with internal we can allow subclassing and still hide the dangerous methods.

        // ReSharper restore MemberCanBeProtected.Global

        

        /// <summary>
        /// Remve all rows in the table
        /// </summary>
        public void Clear()
        {
            ValidateIsValid();
            ValidateReadWrite();
            ClearNoCheck();
        }

        internal abstract void ClearNoCheck();

        internal abstract void RemoveNoCheck(long rowIndex);
        //removes the row at rowIndex, all rows after that have their index reduced by 1
        //all existing and any new row and rowcolumn classes will point to the new contents of the indicies.

        
        /// <summary>
        /// True if the table or TableView has no rows
        /// </summary>
        public Boolean IsEmpty
        {
            get { return Size == 0; }
        }

        /// <summary>
        /// Returns the number of rows in the table.
        /// </summary>
        public long Size
        {
            get { ValidateIsValid(); return GetSize(); }
        }





        //Methods on TableOrView

        /// <summary>
        /// Returns the number of colums in the table at root level.
        /// A column of type subtable only counts as one column.
        /// </summary>
        public long ColumnCount
        {
            get { ValidateIsValid(); return GetColumnCount(); }
        }

        /// <summary>
        /// Return a Spec object that can give information about the table schema
        /// Depricated - This property will be removed in later versions, as Table gains
        /// methods that can serve the same purposees.
        /// </summary>
        public Spec Spec
        {
            get { ValidateIsValid(); return GetSpec(); }
        }

        
        //todo : profile wether int or long makes a time difference in 32bit and 64bit
        //currently following the MSFT library List<T> closely reg. type used
        //if Version overflows and counts back from the start, an iterator could be very unluky and get called after exactly 2^32 or
        //a multiple of 2^32 modifying operations.
        internal int Version;//adding or removing rows will increase version. Iterators use this to invalidate themselves
        //Version is updated just after calls to UnsafeNativeMethods


        /// <summary>
        /// Return the column index for a given column name, or throw exception if the column does not exist
        /// </summary>
        /// <param name="name">Name of column to find</param>
        /// <returns>zero based index of column whith the given name</returns>
        /// <exception cref="ArgumentOutOfRangeException">ArgumentOutOfRangeException is thrown if the column does not exist</exception>
        public long GetColumnIndex(String name)
        {
            ValidateIsValid();//here because this is highly probably the first core call in a single operation using GetColumnIndex
            long columnIndex = GetColumnIndexNoCheck(name);
            if (columnIndex == -1)
            {
                throw new ArgumentException("name",
                    String.Format(CultureInfo.InvariantCulture,
                        "column name specified ({0}) does not exist in this table.", name));
            }
            return columnIndex;
        }


        //takes a message to enable us to give a customized error message if the validation fails.
        //this validation could go wrong if someone change table scheme while the method is running
        private static  void ValidateEqualScheme(Table tableA, Table tableB, string message)
        {
            if (tableA == null || tableB == null)
            {
                throw new ArgumentNullException("ValidateEqualScheme called with table object that was null " +
                                                ((tableA == null) ? "TableA was Null" : "") +
                                                ((tableB == null) ? "TableB was Null" : ""));
            }

            if (!tableA.IsValid() || !tableB.IsValid())
            {
                throw new ArgumentNullException("ValidateEqualScheme called with table.isValid()==false " +
                                                ((tableA.IsValid()) ? "TableA was not valid" : "") +
                                                ((tableB.IsValid()) ? "TableB was not valid" : ""));
            }

            if (tableA.HasSharedSpec() != tableB.HasSharedSpec())
            {
                //this should be okay in fact
            }
            if (!UnsafeNativeMethods.SpecEqualsSpec(tableA, tableB))
            {
                throw new ArgumentOutOfRangeException(
                    String.Format(CultureInfo.InvariantCulture,
                        "the operation {0} could not be performed as the two tables do not have similar specs", message));
            }
        }



        //column and row must point to a field that is of type subtable
        //note that in case the subtable scheme does not fit well with the passed object, the subtable will be half filled wit data up to the point where there was a mismatch,
        //for instance if called with an array of 10 rows, where the last row have one more field (or is missing a field)
        //if schemes does not match up with data an exeption is thrown when the mismatch is discovered.
        //It is possible to back out of this operation using transactions (aborting the ongoing one)
        //if You pass a Table, schema is validated before any data is changed
        //if You pass some kind of ienummerable data structure, the data is not first validated against the subtable structure.
        //there is also a lowlevel SetSubTableNoCheck - this one takes almost anything and figures what to do with it
        //.net 35 will not allow You to call IEnumerable<object> with a table as it is IEnumerable<Row>
        //furthermore .net35 will not even allow you to have two methods where one takes Table, and one takes IEnumerable<Object>
        //this is a bit strange, and also fixed in .net40 and .net45. To get .net35 working we have a special set of functions
        //with different names to allow calling with table or with some enumerable collection

#if !V40PLUS

        private void SetSubTableNoCheckHighLevel(long columnIndex, long rowIndex, Table element)
        {
            if (element == null)
            {
                SetSubtableAsNull(columnIndex, rowIndex);
            }
            else
            {
                using (var t = GetSubTableNoCheck(columnIndex, rowIndex))
                {
                    ValidateEqualScheme(t, element, "Set Sub Table");
                    SetSubTableNoCheck(columnIndex, rowIndex, element);
                }
            }
        }

        private void SetSubTableNoCheckHighLevel(long columnIndex, long rowIndex,  IEnumerable<Object> element)
        {
            if (element == null)
            {
                SetSubtableAsNull(columnIndex, rowIndex);
            }
            else
            {
                using (var t = GetSubTableNoCheck(columnIndex, rowIndex))
                {
                    t.AddMany(element);
                }
            }
        }

        private void SetSubTableNoCheckHighLevel(long columnIndex, long rowIndex, TableView element)
        {
            if (element == null)
            {
                SetSubtableAsNull(columnIndex, rowIndex);
            }
            else
            {
                using (var t = GetSubTableNoCheck(columnIndex, rowIndex))
                {
                    //  do this when SetSubTable(....tableview has been implemented in core)SetSubTableNoCheck(columnIndex, rowIndex, element);
                    ValidateEqualScheme(t, element.UnderlyingTable, "Set SubTable with TableView");
                    //when implemented in core, simply call SetSubTableNoCheck(...elemTableView);
                    //for now, just call AddMany and let that one iterate through the tableview and get the job done
                    t.AddMany(element);                    
                }
            }
        }


        private void SetSubtableAsNull(long columnIndex, long rowIndex)
        {
            ClearSubTableNoCheck(columnIndex, rowIndex);
        }

#else
        private void SetSubTableNoCheckHighLevel(long columnIndex, long rowIndex, IEnumerable<Object> element)
        {
            if (element == null)
                //if You specify null for a subtable we do nothing null means You intend to fill it in later
            {
                ClearSubTableNoCheck(columnIndex, rowIndex);
                return; //done!;
                //if the user specifies null it means create a new empty subtable, not let the old one stay!
            }

            //user did not specify null. It could be a table or it could be an ienummerable structure of data that fits the schema in the subtable

            using (var t = GetSubTableNoCheck(columnIndex, rowIndex))
            {

                var elemTable = (element as Table);
                if (elemTable != null)
                {
                    ValidateEqualScheme(t, elemTable, "Set Sub Table");
                    SetSubTableNoCheck(columnIndex, rowIndex, elemTable);
                    //call table or tableview lowlevel implementation
                    return; //done! if elem is null element was not a Table but something else
                }
                var elemTableView = (element as TableView);
                if (elemTableView != null)
                {
                    ValidateEqualScheme(t,elemTableView.UnderlyingTable,"Set SubTable with TableView");
                    //when implemented in core, simply call SetSubTableNoCheck(...elemTableView);
                    //for now, just call AddMany and let that one iterate through the tableview and get the job done
                    t.AddMany(elemTableView);
                    return;
                }

                //each element in the enumerable list must be a row, 
                //so call AddMany to add them
                t.AddMany(element);
            }
        }

#endif

        //add unknown typed object to a mixed
        private void SetMixedNoCheck(long columnIndex, long rowIndex, object element)
        {
            Type elementType = element.GetType();
            //these tests are sorted in order of most likely types being encountered in an add ir set call
            //Some tests are however dependent on other tests having been done before
            //if they depend on other test done before, it is noted in a comment

            if (elementType == typeof (Int32))
            {
                SetMixedIntNoCheck(columnIndex,rowIndex,(int)element);
                return;
            }


            if (elementType == typeof (string))
            {
                SetMixedStringNoCheck(columnIndex, rowIndex, (String) element);
                return;
            }


            if (elementType == typeof (DateTime))
            {
                SetMixedDateTimeNoCheck(columnIndex, rowIndex, (DateTime) element);
                return;
            }

            if (elementType == typeof (Double))
            {
                SetMixedDoubleNoCheck(columnIndex, rowIndex, (Double) element);
                return;
            }

            if (elementType == typeof (Boolean))
            {
                SetMixedBoolNoCheck(columnIndex, rowIndex, (bool) element);
                return;
            }


            if (elementType == typeof (Single)) //float, Single
            {
                SetMixedFloatNoCheck(columnIndex, rowIndex, (float) element);
                return;
            }


            if (elementType == typeof (byte))
            {
                SetMixedIntNoCheck(columnIndex, rowIndex, (byte) element);
                return;
            }

            if (elementType == typeof (SByte))
            {
                SetMixedIntNoCheck(columnIndex, rowIndex, (SByte) element);
                return;
            }


            if (elementType == typeof (UInt32)) //uint has to be stored as a long as uint max is gt what an int can take
            {
                SetMixedLongNoCheck(columnIndex, rowIndex, Convert.ToInt64(element,CultureInfo.InvariantCulture));
                return;
            }

            if (elementType == typeof (UInt64)) //uint
            {
                try
                {
                    var value = Convert.ToInt64(element, CultureInfo.InvariantCulture);//might throw
                    SetMixedLongNoCheck(columnIndex, rowIndex, value);
                }
                catch (OverflowException )
                {
                    throw new ArgumentOutOfRangeException("element","Value sent to mixed field was either too large or too small to fit in a 64 bit SIGNED integer ");
                }
                    //possible datloss if the uint64 is larger than int.maxsize
                return;
            }


            if (
                elementType == typeof (Int16) || //int16,short
                elementType == typeof (SByte) || //sbyte SByte                           
                elementType == typeof (Int32) || //int,int32
                elementType == typeof (UInt16) //ushort,uint16
                )
            {
                SetMixedIntNoCheck(columnIndex, rowIndex, Convert.ToInt32(element,CultureInfo.InvariantCulture));
                return;
            }

            if (
                elementType == typeof (Int64) || //long,int64
                elementType == typeof (UInt64) //ulong                            
                )
            {
                SetMixedLongNoCheck(columnIndex, rowIndex, (long) element);
                return;
                //ints cannot be unboxed directly to long. But can the larger types?
            }

            if (elementType == typeof (char))
            {
                SetMixedIntNoCheck(columnIndex, rowIndex, Convert.ToInt32(element,CultureInfo.InvariantCulture));
                return;
            }



            if (elementType == typeof (byte[]))
            {
                SetMixedBinaryNoCheck(columnIndex, rowIndex, (byte[]) element);
                return;
            }


            //depends on string and byte[] having been handled higher op so we never get those down here
            var elementAsEnumerable = element as IEnumerable;
            if (elementAsEnumerable != null) //check if it is an array of stuff that could be row contents
            {
                if (elementType != typeof (Table))
                {
                    //element is supposed to be a complex structure with data for a subtable
                    using (var table = new Table())
                    {
                        table.AddMany(elementAsEnumerable);
                        SetMixedSubTableNoCheck(columnIndex, rowIndex, table);
                    }
                }
                else
                {
                    SetMixedSubTableNoCheck(columnIndex, rowIndex, element as Table);
                }
                return;
            }
            throw new ArgumentException(String.Format(CultureInfo.InvariantCulture,"SetMixed called with a unsupported c#type {0}", elementType));
        }


        /// <summary>
        /// Set the specified mixed to a type inferrred from the object specified
        /// The prior contents of the string field is lost.
        /// Note that the mixed will change type to whatever type the binding finds
        /// best fit for the object specified. You are not guarenteed to get the exact
        /// same type back out from the field with GetMixed, for instance if You call with
        /// a byte, you might get a long back, as the data is stored in a DataType.Int
        /// The mixed field does NOT store the C# type that was put into it, only the
        /// TightDb DataType that was used for the field.
        /// </summary>
        /// <param name="columnIndex">Zero based index of the column of the field to set</param>
        /// <param name="rowIndex">zero based index of the row of the field to set</param>
        /// <param name="value">object value to set </param>

        public void SetMixed(long columnIndex, long rowIndex, object value)
        {
            ValidateIsValid();
            ValidateColumnIndexAndTypeMixed(columnIndex);
            ValidateRowIndex(rowIndex);
            ValidateReadWrite();
            SetMixedNoCheck(columnIndex, rowIndex, value);
        }

        /// <summary>
        /// Get the specified mixed value stored in a specified field
        /// The value is converted to a type the binding finds appropriate
        /// depending on what is stored in the mixed.
        /// DataType.Int : returns a boxed long
        /// DataType.Float :returns a boxed float
        /// DataType.Double :returns a boxed Double
        /// DataType.Date : returns a  DateTime
        /// DataType.String : returns a String
        /// DataType.Binary : returns a byte array
        /// DataType.Bool  : returns a boxed bool
        /// DataType.Table :  returns a Table
        /// </summary>
        /// <param name="columnIndex">Zero based index of the column of the field to get</param>
        /// <param name="rowIndex">zero based index of the row of the field to get</param>        
        /// <returns>Value in the field, type depends on what kind(DataType) of mixed field it is </returns>
        public object GetMixed(long columnIndex, long rowIndex)
        {
            ValidateIsValid();
            ValidateColumnIndexAndTypeMixed(columnIndex);
            ValidateRowIndex(rowIndex);
            return GetMixedNoCheck(columnIndex, rowIndex);
        }

        internal object GetMixedNoRowCheck(long columnIndex, long rowIndex)
        {
            ValidateColumnIndexAndTypeMixed(columnIndex);
            return GetMixedNoCheck(columnIndex, rowIndex);
        }

        internal object GetMixedNoRowCheck(string columnName, long rowIndex)
        {
            var columnIndex = GetColumnIndex(columnName);
            ValidateTypeMixed(columnIndex);
            return GetMixedNoCheck(columnIndex, rowIndex);
        }

        internal void SetMixedNoRowCheck(long columnIndex, long rowIndex,object value)
        {
            ValidateColumnIndexAndTypeMixed(columnIndex);
            SetMixedNoCheck(columnIndex,rowIndex,value);
        }

        //special case as we only handle object arrays in the general case of adding an array to a row
        //however, when specifying arrays in array, the user can easily accidentially specify an array that
        //is not boxed and is a real physical array with values, such an array is detected in the structure we get
        //and handled here.
        private void SetIntArrayInRowNoCheck(long rowIndex, Int32[] ints)
        {
            ValidateSetRowNumColumns(ints.Length);
            int ix = 0;
            foreach (var i in ints)
            {
                SetLongNoCheck(ix, rowIndex, i);
                ix++;
            }
        }

        //helper shared with a few methods
        private void ValidateSetRowNumColumns(int arrayLength)
        {
            if (arrayLength != ColumnCount)
                throw new ArgumentOutOfRangeException(
                    String.Format(CultureInfo.InvariantCulture,
                        "SetRow called with {0} objects, but there are only {1} columns in the table",
                        arrayLength, ColumnCount));
        }

        //at this stage, it has already been checked that rowcontents matches the schema if rowcontents is Table or row or TableView
        //do not check row index       
        //todo:let the type be ieumerable, allowing user to set with objects in any kind of collection he has
        //object is also Ienummerable i guess?
        internal void SetRowNoCheck(long rowIndex, params object[] rowContents)
            //experimental        internal void SetRowNoCheck(long rowIndex, IEnumerable<object> rowContents)
        {
            //special case if rowContents is null
            if (rowContents == null)
            {
                rowContents = new object[] {null};
                    //so the rest of the code gets an array with a null inside. Some types accept null.
            }
            //user could send row data as an object array as first parameter, or many parametres that together is the data for a row/
            //handle both cases by making sure rowContents is always an array of field values to be put in
            if (rowContents.Length == 1 && ColumnCount > 1 && rowContents.GetType() == typeof (object[]))
            {
                if (rowContents[0].GetType() == typeof (Int32[]))
                    //bc if you specify a subtable row as new [] {1,2,3} C# will compile it as an System.int32[] which is not at all compatible with object[]
                {
                    SetIntArrayInRowNoCheck(rowIndex, (Int32[]) rowContents[0]);
                    return;
                }

                if (rowContents[0].GetType() == typeof (object[]))
                {
                    rowContents = (object[]) rowContents[0];
                }
                if (rowContents[0].GetType() == typeof(string[]))
                {
                    rowContents = (string[])rowContents[0];
                }
                if (rowContents[0].GetType()==typeof(Row))//special case that can be (should be) removed when Table.SetSubTable(..TableView has been created)
                {                                         //or if this method is rewritten to take IEnumerable
                    var row = rowContents[0] as Row;      //or if Row starts implementing an interface allowing it to be indexed as object[]
                    if (row != null)
                    {
                        var ix = 0;
                        foreach (var field in row)
                        {
                            SetValueNoCheck(ix,rowIndex,field.Value);
                            ++ix;
                        }
                    }
                    else
                    {
                        throw new ArgumentException("ERR001");//this cannot happen so don't waste space on a long description string
                    }
                    return;
                }
            }

            ValidateSetRowNumColumns(rowContents.Length);

            for (long ix = 0; ix < ColumnCount; ix++)
            {
                object element = rowContents[ix]; //element is parameter number ix
                //first do a switch on directly compatible types

                SetValueNoCheck(ix, rowIndex, element);//also used by row[colix] operator
            }
        }


        internal void SetValueNoCheck(long columnIndex, long rowIndex, object element)
        {
            switch (ColumnTypeNoCheck(columnIndex))
            {
                case DataType.Int:
                    SetLongNoCheck(columnIndex, rowIndex, Convert.ToInt64(element, CultureInfo.InvariantCulture));
                    //this throws exceptions if called with something too weird
                    break;
                case DataType.Bool:
                    SetBoolNoCheck(columnIndex, rowIndex, (Boolean)element);
                    break;
                case DataType.String:
                    SetStringNoCheck(columnIndex, rowIndex, (string)element);
                    break;
                case DataType.Binary: //currently you HAVE to send a byte[] in the array with binary data
                    //later we might support that You put in other types, that can be changed to binary data
                    var data = element as byte[];
                    if (data == null)
                    {
                        if (element is float)
                        //for instance we might want to support people stuffing a float directly to a binary field for some obscure reason
                        {

                        }
                    }
                    SetBinaryNoCheck(columnIndex, rowIndex, data);
                    break;
                case DataType.Table: //todo:test thoroughly with unit test, also with invalid data
                    {
                        //element could be a table or an ennumarable structure, both are Ienummerable<object> so just call on and let SetSubTableNoCheck  deal with it
#if V40PLUS
                        SetSubTableNoCheckHighLevel(columnIndex, rowIndex, (IEnumerable<object>) element);
#else
                        var elementAsEnumerable = element as IEnumerable<object>;
                        var elementAsTable = element as Table;

                        if (element == null)
                        {
                            SetSubtableAsNull(columnIndex, rowIndex);//this method also handles null table
                        }

                        if (elementAsTable != null)//call only with null or Table
                        {
                            SetSubTableNoCheckHighLevel(columnIndex, rowIndex, elementAsTable);
                        }
                        else
                            if (elementAsEnumerable != null)//call with IEnumerable<Object> that is not a table
                            {
                                SetSubTableNoCheckHighLevel(columnIndex, rowIndex, elementAsEnumerable);
                            }
#endif
                    }
                    break;
                case DataType.Mixed: //Try to infer the mixed type to use from the type of the object from the user
                    SetMixedNoCheck(columnIndex, rowIndex, element);
                    break;
                case DataType.Date:
                    SetDateTimeNoCheck(columnIndex, rowIndex, (DateTime)element);
                    break;
                case DataType.Float:
                    SetFloatNoCheck(columnIndex, rowIndex, (float)element);
                    break;
                case DataType.Double:
                    SetDoubleNoCheck(columnIndex, rowIndex, (Double)element);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("element",
                        String.Format(CultureInfo.InvariantCulture,
                            "An element ix:{0} of type {1} in a row sent to AddRow, is not of a supported tightdb type ",
                            columnIndex, element.GetType()));
            }
        }

        /// <summary>
        /// Return the type of the specified column.
        /// The type determines what kinds of data can be stored at that column index in the table or view
        /// </summary>
        /// <param name="columnIndex">zero based index of column</param>
        /// <returns>DataType that the column is defined as</returns>
        public DataType ColumnType(long columnIndex)
        {            
            ValidateColumnIndex(columnIndex);
            return ColumnTypeNoCheck(columnIndex);
        }

        //GetxxDataTypexxCheckType is used when column and row is known to be valid, but the type of the field has not been validated yet. Used when user calles RowColumn.GetxxDataTypexx();
        //used by tablerow where we know row and column index are valid,but not if the user calls getsubtable on a column that does not have subtables
        internal Table GetSubTableCheckType(long columnIndex, long rowIndex)
        {
            ValidateTypeSubTable(columnIndex);
            return GetSubTableNoCheck(columnIndex, rowIndex);
        }

        /// <summary>
        /// Returns the subtable stored in the specified field
        /// </summary>
        /// <param name="columnIndex">Zero based index of the column of the field with a SubTable</param>
        /// <param name="rowIndex">Zero based index of the row of the field with a SubTable</param>
        /// <returns>Table that is stored in the field (Lazily created, data is transferred only if field get or set is called on the returned table)</returns>
        public Table GetSubTable(long columnIndex, long rowIndex)
        {
            ValidateIsValid();
            ValidateColumnAndRowIndex(columnIndex, rowIndex);
            ValidateTypeSubTable(columnIndex);
            return GetSubTableNoCheck(columnIndex, rowIndex);
        }

        /// <summary>
        /// Returns the subtable stored in the specified field
        /// </summary>
        /// <param name="columnName">Name of the column of the field with a SubTable</param>
        /// <param name="rowIndex">Zero based index of the row of the field with a SubTable</param>
        /// <returns>Table that is stored in the field (Lazily created, data is transferred only if field get or set is called on the returned table)</returns>
        public Table GetSubTable(string columnName, long rowIndex)
        {            
            long columnIndex = GetColumnIndex(columnName);
            ValidateRowIndex(rowIndex);
            return GetSubTableNoRowCheck(columnIndex, rowIndex);
        }


        /// <summary>
        /// Unlike the c++ binding, subtables and mixed subtables are handled
        /// in a typed manner in the C# binding.
        /// This method will return the size of a subtable in a specified field.
        /// The method will throw if the field specified is a mixed subtable field,
        /// or any other field type that is not DataType.Table.
        /// See GetMixedSubtableSize()
        /// 
        /// </summary>
        /// <param name="columnName">Name of column index of field with a subtable in it</param>
        /// <param name="rowIndex">row index of column of field with a subtable in it</param>
        /// <returns>number of rows in specified subtable</returns>
        public long GetSubTableSize(String columnName, long rowIndex)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateRowIndex(rowIndex);
            ValidateTypeSubTable(columnIndex);
            return GetSubTableSizeNoCheck(columnIndex, rowIndex);            
        }

        /// <summary>
        /// Unlike the c++ binding, subtables and mixed subtables are handled
        /// in a typed manner in the C# binding.
        /// This method will return the size of a subtable in a specified field.
        /// The method will throw if the field specified is a mixed subtable field,
        /// or any other field type that is not DataType.Table.
        /// See GetMixedSubtableSize()
        /// </summary>
        /// <param name="columnIndex">Name of column index of field with a subtable in it</param>
        /// <param name="rowIndex">row index of column of field with a subtable in it</param>
        /// <returns>number of rows in specified subtable</returns>
        public long GetSubTableSize(long columnIndex, long rowIndex)
        {
            ValidateIsValid();
            ValidateColumnAndRowIndex(columnIndex, rowIndex);
            ValidateTypeSubTable(columnIndex);
            return GetSubTableSizeNoCheck(columnIndex, rowIndex);
        }

        /// <summary>
        /// Unlike the c++ binding, subtables and mixed subtables are handled
        /// in a typed manner in the C# binding.
        /// This method will return the size of a subtable in a specified field.
        /// The method will throw if the field specified is a mixed subtable field,
        /// or any other field type that is not DataType.Table.
        /// See GetMixedSubtableSize()
        /// 
        /// </summary>
        /// <param name="columnName">Name of column index of field with a subtable in it</param>
        /// <param name="rowIndex">row index of column of field with a subtable in it</param>
        /// <returns>number of rows in specified subtable</returns>
        public long GetMixedSubTableSize(String columnName, long rowIndex)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateRowIndex(rowIndex);
            ValidateMixedType(columnIndex,rowIndex,DataType.Table);
            return GetSubTableSizeNoCheck(columnIndex, rowIndex);//core doesn't care if it is a mixed or subtable field based subtable
        }

        /// <summary>
        /// Unlike the c++ binding, subtable fields and mixed fields with subtables are handled
        /// in a typed manner in the C# binding.
        /// This method will return the size of a subtable in a specified mixed field.
        /// The method will throw if the field specified is a subtable field
        /// or any other field type that is not DataType.Mixed with a MixedType table
        /// See also GetSubtableSize()
        /// </summary>
        /// <param name="columnIndex">Name of column index of a DataType.Mixed field with a subtable in it</param>
        /// <param name="rowIndex">row index of column of a DataType.Mixed field with a subtable in it</param>
        /// <returns>number of rows in specified subtable</returns>
        public long GetMixedSubTableSize(long columnIndex, long rowIndex)
        {
            ValidateIsValid();            
            ValidateColumnMixedType(columnIndex,rowIndex,DataType.Table);
            return GetSubTableSizeNoCheck(columnIndex, rowIndex);//core doesn't care if it is a mixed or subtable field based subtable
        }



        //GetxxxDataTypexxx is called when the row is known, when the user calls Row.GetxxxDataTypexxx(columnIx) The row is already validated for a row object, so only columnIndex and the type of the field is validated
        internal Table GetSubTableNoRowCheck(long columnIndex, long rowIndex)
        {
            ValidateColumnIndexAndTypeSubTable(columnIndex);
            return GetSubTableNoCheck(columnIndex, rowIndex);
        }

        //The user can specify the name of a column. A lookup is done to figure the column index
        internal Table GetSubTableNoRowCheck(string columnName, long rowIndex)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeSubTable(columnIndex);
            return GetSubTableNoCheck(columnIndex, rowIndex);
        }

        /// <summary>
        /// Clears the specified subtable field to hold an empty subtable. All data in the subtable being cleared is lost
        /// </summary>
        /// <param name="columnIndex">Zero based index of the column of the field to set</param>
        /// <param name="rowIndex">Zero based index of the row of the field to set</param>
        public void ClearSubTable(long columnIndex, long rowIndex)
        {            
            ValidateColumnIndexAndTypeSubTable(columnIndex);
            ValidateRowIndex(rowIndex);
            ValidateReadWrite();
            ClearSubTableNoCheck(columnIndex, rowIndex);
        }

        internal void ClearSubTableNoRowCheck(long columnIndex, long rowIndex)
        {
            ValidateColumnIndexAndTypeSubTable(columnIndex);
            ClearSubTableNoCheck(columnIndex,rowIndex);
        }

        internal void ClearSubTableNoRowCheck(String columnName, long rowIndex)
        {
            var columnIndex=GetColumnIndex(columnName);
            ClearSubTableNoCheck( columnIndex, rowIndex);
        }

        internal DateTime GetDateTimeNoRowCheck(long columnIndex, long rowIndex)
        {
            ValidateColumnIndexAndTypeDate(columnIndex);
            return GetDateTimeNoCheck(columnIndex, rowIndex);
        }



        internal DateTime GetDateTimeNoRowCheck(string columnName, long rowIndex)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeDate(columnIndex);
            return GetDateTimeNoRowCheck(columnIndex, rowIndex);
        }




        /// <summary>
        /// create an empty table inside a mixed field
        /// </summary>
        /// <param name="columnIndex">zero based index of column of field</param>
        /// <param name="rowIndex">zero based indes of row of field</param>
        public void SetMixedEmptySubTable(long columnIndex, long rowIndex)
        {
            ValidateIsValid();
            ValidateColumnRowTypeMixed(columnIndex, rowIndex);
            ValidateReadWrite();
            SetMixedEmptySubtableNoCheck(columnIndex, rowIndex);
        }


        /// <summary>
        /// return the long value stored in a mixed field.
        /// The field must be of type Mixed.
        /// The value stored in the field must be of type DataType.Int.        
        /// </summary>
        /// <param name="columnIndex">zero based index of column of field</param>
        /// <param name="rowIndex">zero based index of row of field</param>
        /// <returns>value of field</returns>
        public long GetMixedLong(long columnIndex, long rowIndex)
        {
            ValidateIsValid();
            ValidateColumnRowMixedType(columnIndex, rowIndex, DataType.Int);
            return GetMixedLongNoCheck(columnIndex, rowIndex);
        }

        /// <summary>
        /// return the long value stored in a mixed field
        /// </summary>
        /// <param name="columnName">Name of the column of the field to return</param>
        /// <param name="rowIndex">zero based row index of the column of the field to return</param>
        /// <returns>value of the field specified</returns>
        public long GetMixedLong(string columnName, long rowIndex)
        {
            var columnIndex = GetColumnIndex(columnName);
            ValidateRowIndex(rowIndex);
            ValidateMixedType(columnIndex, rowIndex, DataType.Int);
            return GetMixedLongNoCheck(columnIndex, rowIndex);
        }




        /// <summary>
        /// retrun the boolean value stored in a mixed field
        /// The field must be of type Mixed
        /// The value stored in the field must be of type Boolean        
        /// </summary>
        /// <param name="columnIndex">zero based index of column of field</param>
        /// <param name="rowIndex">zero based index of row of field</param>
        /// <returns>value of field</returns>
        public bool GetMixedBool(long columnIndex, long rowIndex)
        {
            ValidateIsValid();
            ValidateColumnRowMixedType(columnIndex, rowIndex, DataType.Bool);
            return GetMixedBoolNoCheck(columnIndex, rowIndex);
        }


        /// <summary>
        /// retrun the float value stored in a mixed field
        /// The field must be of type Mixed
        /// The value stored in the field must be of type float
        /// </summary>
        /// <param name="columnIndex">zero based index of column of field</param>
        /// <param name="rowIndex">zero based index of row of field</param>
        /// <returns>value of field</returns>
        public float GetMixedFloat(long columnIndex, long rowIndex)
        {
            ValidateIsValid();
            ValidateColumnRowMixedType(columnIndex, rowIndex, DataType.Float);
            return GetMixedFloatNoCheck(columnIndex, rowIndex);
        }


        /// <summary>
        /// return the float value stored in a mixed field.
        /// The field must be of type Mixed
        /// The value stored in the field must be of type float        
        /// </summary>
        /// <param name="columnName">Name of the column of the field to return</param>
        /// <param name="rowIndex">zero based row index of the column of the field to return</param>
        /// <returns>value of the field specified</returns>

        public float GetMixedFloat(string columnName, long rowIndex)
        {
            var columnIndex = GetColumnIndex(columnName);
            ValidateMixedType(columnIndex, rowIndex, DataType.Float);
            return GetMixedFloatNoCheck(columnIndex, rowIndex);
        }

        /// <summary>
        /// retrun the double value stored in a mixed field
        /// The field must be of type Mixed
        /// The value stored in the field must be of type double        
        /// </summary>
        /// <param name="columnIndex">zero based index of column of field</param>
        /// <param name="rowIndex">zero based index of row of field</param>
        /// <returns>value of field</returns>

        public double GetMixedDouble(long columnIndex, long rowIndex)
        {
            ValidateIsValid();
            ValidateColumnRowMixedType(columnIndex, rowIndex, DataType.Double);
            return GetMixedDoubleNoCheck(columnIndex, rowIndex);
        }


        /// <summary>
        /// return the double value stored in a mixed field.
        /// The field must be of type Mixed
        /// The value stored in the field must be of type double        
        /// </summary>
        /// <param name="columnName">Name of the column of the field to return</param>
        /// <param name="rowIndex">zero based row index of the column of the field to return</param>
        /// <returns>value of the field specified</returns>

        public double GetMixedDouble(String columnName, long rowIndex)
        {
            var columnIndex = GetColumnIndex(columnName);
            ValidateRowIndex(rowIndex);
            ValidateMixedType(columnIndex,rowIndex,DataType.Double);            
            return GetMixedDoubleNoCheck(columnIndex, rowIndex);
        }

        /// <summary>
        /// retrun the DateTime value stored in a mixed field
        /// The field must be of type Mixed
        /// The value stored in the field must be of type Datetime
        /// Note that TightDb stores DateTime as UTC time_t.
        /// The DateTime value being returned wil be of kind UTC.
        /// </summary>
        /// <param name="columnIndex">zero based index of column of field</param>
        /// <param name="rowIndex">zero based index of row of field</param>
        /// <returns>value of field</returns>

        public DateTime GetMixedDateTime(long columnIndex, long rowIndex)
        {
            ValidateIsValid();
            ValidateColumnRowMixedType(columnIndex, rowIndex, DataType.Date);
            return GetMixedDateTimeNoCheck(columnIndex, rowIndex);
        }


        /// <summary>
        /// return the float value stored in a mixed field.
        /// The field must be of type Mixed
        /// The value stored in the field must be of type Datetime
        /// Note that TightDb stores DateTime as UTC time_t.
        /// The DateTime value being returned wil be of kind UTC.
        /// </summary>
        /// <param name="columnName">Name of the column of the field to return</param>
        /// <param name="rowIndex">zero based row index of the column of the field to return</param>
        /// <returns>value of the field specified</returns>

        public DateTime GetMixedDateTime(string columnName, long rowIndex)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateRowIndex(rowIndex);
            ValidateMixedType(columnIndex, rowIndex, DataType.Date);
            return GetMixedDateTimeNoCheck(columnIndex, rowIndex);
        }

        /// <summary>
        /// retrun the string value stored in a mixed field
        /// The field must be of type Mixed
        /// The value stored in the field must be of type String        
        /// </summary>
        /// <param name="columnIndex">zero based index of column of field</param>
        /// <param name="rowIndex">zero based index of row of field</param>
        /// <returns>value of field</returns>

        public String GetMixedString(long columnIndex, long rowIndex)
        {
            ValidateIsValid();
            ValidateColumnRowMixedType(columnIndex, rowIndex, DataType.String);
            return GetMixedStringNoCheck(columnIndex, rowIndex);
        }

        /// <summary>
        /// return the string value stored in a mixed field.
        /// The field must be of type Mixed
        /// The value stored in the field must be of type String
        /// Note that TightDb stores strings in UTF-8
        /// The string returned is a UTF-16 string, converted from the UTF-8
        /// string stored in the database.
        /// </summary>
        /// <param name="columnName">Name of the column of the field to return</param>
        /// <param name="rowIndex">zero based row index of the column of the field to return</param>
        /// <returns>value of the field specified</returns>

        public String GetMixedString(string columnName, long rowIndex)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateRowIndex(rowIndex);
            ValidateMixedType(columnIndex, rowIndex, DataType.String);
            return GetMixedStringNoCheck(columnIndex, rowIndex);
        }

        /// <summary>
        /// retrun the binary value stored in a mixed field
        /// The field must be of type Mixed
        /// The value stored in the field must be of type Binary.
        /// the binary value is returned as byte array        
        /// </summary>
        /// <param name="columnIndex">zero based index of column of field</param>
        /// <param name="rowIndex">zero based index of row of field</param>
        /// <returns>value of field</returns>

        public byte[] GetMixedBinary(long columnIndex, long rowIndex)
        {
            ValidateIsValid();
            ValidateColumnRowMixedType(columnIndex, rowIndex, DataType.Binary);
            return GetMixedBinaryNoCheck(columnIndex, rowIndex);
        }


        /// <summary>
        /// return the binary value stored in a mixed field.
        /// The field must be of type Mixed
        /// The value stored in the field must be of type binary
        /// the binary value is returned as byte array        
        /// </summary>
        /// <param name="columnName">Name of the column of the field to return</param>
        /// <param name="rowIndex">zero based row index of the column of the field to return</param>
        /// <returns>value of the field specified</returns>

        public byte[] GetMixedBinary(string columnName, long rowIndex)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateRowIndex(rowIndex);
            ValidateMixedType(columnIndex, rowIndex, DataType.Binary);
            return GetMixedBinaryNoCheck(columnIndex, rowIndex);
        }



        internal long GetMixedLongNoRowCheck(long columnIndex, long rowIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateColumnMixedType(columnIndex, rowIndex, DataType.Int);
            return GetMixedLongNoCheck(columnIndex, rowIndex);
        }

        internal long GetMixedLongNoRowCheck(string columnName, long rowIndex)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateColumnMixedType(columnIndex, rowIndex, DataType.Int);
            return GetMixedLongNoCheck(columnIndex, rowIndex);
        }


        internal bool GetMixedBooleanNoRowCheck(long columnIndex, long rowIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateColumnMixedType(columnIndex, rowIndex, DataType.Bool);
            return GetMixedBoolNoCheck(columnIndex, rowIndex);
        }

        internal bool GetMixedBooleanNoRowCheck(string columnName, long rowIndex)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateColumnMixedType(columnIndex, rowIndex, DataType.Bool);
            return GetMixedBoolNoCheck(columnIndex, rowIndex);
        }

        internal float GetMixedFloatNoRowCheck(long columnIndex, long rowIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateColumnMixedType(columnIndex, rowIndex, DataType.Float);
            return GetMixedFloatNoCheck(columnIndex, rowIndex);
        }

        internal float GetMixedFloatNoRowCheck(string columnName, long rowIndex)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateColumnMixedType(columnIndex, rowIndex, DataType.Float);
            return GetMixedFloatNoCheck(columnIndex, rowIndex);
        }

        internal Double GetMixedDoubleNoRowCheck(long columnIndex, long rowIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateColumnMixedType(columnIndex, rowIndex, DataType.Double);
            return GetMixedDoubleNoCheck(columnIndex, rowIndex);
        }

        internal Double GetMixedDoubleNoRowCheck(string columnName, long rowIndex)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateColumnMixedType(columnIndex, rowIndex, DataType.Double);
            return GetMixedDoubleNoCheck(columnIndex, rowIndex);
        }

        internal String GetMixedStringNoRowCheck(long columnIndex, long rowIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateColumnMixedType(columnIndex, rowIndex, DataType.String);
            return GetMixedStringNoCheck(columnIndex, rowIndex);
        }

        internal String GetMixedStringNoRowCheck(string columnName, long rowIndex)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateColumnMixedType(columnIndex, rowIndex, DataType.String);
            return GetMixedStringNoCheck(columnIndex, rowIndex);
        }

        internal byte[] GetMixedBinaryNoRowCheck(long columnIndex, long rowIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateColumnMixedType(columnIndex, rowIndex, DataType.Binary);
            return GetMixedBinaryNoCheck(columnIndex, rowIndex);
        }

        internal byte[] GetMixedBinaryNoRowCheck(string columnName, long rowIndex)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateColumnMixedType(columnIndex, rowIndex, DataType.Binary);
            return GetMixedBinaryNoCheck(columnIndex, rowIndex);
        }


        internal DateTime GetMixedDateTimeNoRowCheck(long columnIndex, long rowIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateColumnMixedType(columnIndex, rowIndex, DataType.Date);
            return GetMixedDateTimeNoCheck(columnIndex, rowIndex);
        }

        internal DateTime GetMixedDateTimeNoRowCheck(string columnName, long rowIndex)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateColumnMixedType(columnIndex, rowIndex, DataType.Date);
            return GetMixedDateTimeNoCheck(columnIndex, rowIndex);
        }

        internal Table GetMixedTableNoRowCheck(long columnIndex, long rowIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateColumnMixedType(columnIndex, rowIndex, DataType.Table);
            return GetMixedSubTableNoCheck(columnIndex, rowIndex);
        }

        internal Table GetMixedTableNoRowCheck(string columnName, long rowIndex)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateColumnMixedType(columnIndex, rowIndex, DataType.Table);
            return GetMixedSubTableNoCheck(columnIndex, rowIndex);
        }



        /// <summary>
        /// return DateTime value from a specified field
        /// Field must be DataType.DateTime
        /// DateTime is stored in tightdb as size_t UTC
        /// This means that the DateTime returned is of kind UTC        
        /// </summary>
        /// <param name="columnIndex">name of column of field</param>
        /// <param name="rowIndex">row index of field</param>
        /// <returns>value stored in field</returns>

        public DateTime GetDateTime(long columnIndex, long rowIndex)
        {
            ValidateIsValid();
            ValidateColumnAndRowIndex(columnIndex, rowIndex);
            ValidateTypeDate(columnIndex);
            return GetDateTimeNoCheck(columnIndex, rowIndex);
        }

        /// <summary>
        /// return DateTime value from a specified field
        /// Field must be DataType.DateTime
        /// DateTime is stored in tightdb as size_t UTC
        /// This means that the DateTime returned is of kind UTC        
        /// </summary>
        /// <param name="columnName">name of column of field</param>
        /// <param name="rowIndex">row index of field</param>
        /// <returns>value stored in field</returns>
        public DateTime GetDateTime(string columnName, long rowIndex)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateRowIndex(rowIndex);
            ValidateTypeDate(columnIndex);
            return GetDateTimeNoCheck(columnIndex, rowIndex);
        }

        //methods that are common for table and tableview:
        //split into its own method to make the ordinary getsubtable very slightly faster bc it does not have to validate if type is a mixed
        /// <summary>
        /// return a subtable located in a mixed field.
        /// The field must be of type mixed.
        /// the field must contain a table.
        /// </summary>
        /// <param name="columnIndex">Zero based Index of column of field</param>
        /// <param name="rowIndex">zero based row of column of field</param>
        /// <returns>Table object representing the table in the mixed field</returns>
        public Table GetMixedSubTable(long columnIndex, long rowIndex)
        {
            ValidateIsValid();
            ValidateColumnRowMixedType(columnIndex, rowIndex, DataType.Table);
            return GetMixedSubTableNoCheck(columnIndex, rowIndex);
        }

        /// <summary>
        /// Returns the type of value stored in a specicfied mixed field.
        /// The field must be of type DataType.Mixed
        /// </summary>
        /// <param name="columnIndex">Zero based index of column of field</param>
        /// <param name="rowIndex">Zero based row of column of field</param>
        /// <returns>DataType of the data inside the mixed field</returns>
        public DataType GetMixedType(long columnIndex, long rowIndex)
        {
            ValidateIsValid();
            ValidateColumnRowTypeMixed(columnIndex, rowIndex);
            return GetMixedTypeNoCheck(columnIndex, rowIndex);
        }


        /// <summary>
        /// Set a long value into a mixed field.
        /// The field specified must be of datatype mixed.
        /// The current type of the mixed field will be changed from whatever it is, to DataType.int
        /// And the value specified will be stored in the field
        /// </summary>
        /// <param name="columnIndex">Zero based index of column of field</param>
        /// <param name="rowIndex">Zero based row of column of field</param>
        /// <param name="value">long value to store in the mixed field</param>
        public void SetMixedLong(long columnIndex, long rowIndex, long value)
        {
            ValidateIsValid();
            ValidateColumnRowTypeMixed(columnIndex, rowIndex);
            ValidateReadWrite();
            SetMixedLongNoCheck(columnIndex, rowIndex, value);
        }


        /// <summary>
        /// Set a long value into a mixed field.
        /// The field specified must be of datatype mixed.
        /// The current type of the mixed field will be changed from whatever it is, to DataType.int
        /// And the value specified will be stored in the field
        /// </summary>
        /// <param name="columnIndex">Zero based index of column of field</param>
        /// <param name="rowIndex">Zero based row of column of field</param>
        /// <param name="value">long value to store in the mixed field</param>
        public void SetMixedInt(long columnIndex, long rowIndex, int value)
        {
            ValidateIsValid();
            ValidateColumnRowTypeMixed(columnIndex, rowIndex);
            ValidateReadWrite();
            SetMixedIntNoCheck(columnIndex, rowIndex, value);
        }



        /// <summary>
        /// Set the specified mixed to type DataType.Bool and set the value to the booelan value specified.
        /// The prior contents of the mixed field is lost.
        /// Note that the mixed will change type to DataType.Bool no matter what the type
        /// was before.
        /// </summary>
        /// <param name="columnIndex">Zero based index of the column of the field to set</param>
        /// <param name="rowIndex">zero based index of the row of the field to set</param>
        /// <param name="value">Boolean value to set (the field will be true or false, and seen as true or false in all languages supporting booleans - the numeric value is not known)</param>

        public void SetMixedBool(long columnIndex, long rowIndex, bool value)
        {
            ValidateIsValid();
            ValidateColumnRowTypeMixed(columnIndex, rowIndex);
            ValidateReadWrite();
            SetMixedBoolNoCheck(columnIndex, rowIndex, value);
        }

        /// <summary>
        /// Set the specified mixed to type DataType.String and set the value to the string specified
        /// The prior contents of the string field is lost.
        /// Note that the mixed will change type to DataType.Bool no matter what the type
        /// was before.
        /// </summary>
        /// <param name="columnIndex">Zero based index of the column of the field to set</param>
        /// <param name="rowIndex">zero based index of the row of the field to set</param>
        /// <param name="value">String value to set (Tightdb string fields contain UTF-8 so the string will be stored as UTF-8 even though it is specified as UTF-16 )</param>

        public void SetMixedString(long columnIndex, long rowIndex, string value)
        {
            ValidateIsValid();
            ValidateColumnRowTypeMixed(columnIndex, rowIndex);
            ValidateReadWrite();
            SetMixedStringNoCheck(columnIndex, rowIndex, value);
        }


        /// <summary>
        /// Set the specified mixed to type DataType.String and set the value to the string specified
        /// The prior contents of the string field is lost.
        /// Note that the mixed will change type to DataType.Bool no matter what the type
        /// was before.
        /// </summary>
        /// <param name="columnName">Name of the column of the field to set</param>
        /// <param name="rowIndex">zero based index of the row of the field to set</param>
        /// <param name="value">String value to set (Tightdb string fields contain UTF-8 so the string will be stored as UTF-8 even though it is specified as UTF-16 )</param>
        public void SetMixedString(string columnName, long rowIndex, string value)
        {            
            long columnIndex = GetColumnIndex(columnName);
            ValidateColumnRowTypeMixed(columnIndex, rowIndex);
            ValidateReadWrite();
            SetMixedStringNoCheck(columnIndex, rowIndex, value);
        }


        internal void SetMixedStringNoRowCheck(long columnIndex, long rowIndex, string value)
        {            
            ValidateColumnIndexAndTypeMixed(columnIndex);
            SetMixedStringNoCheck(columnIndex, rowIndex, value);
        }

        internal void SetMixedStringNoRowCheck(string columnName, long rowIndex, string value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeMixed(columnIndex);
            SetMixedStringNoCheck(columnIndex, rowIndex, value);
        }


        /// <summary>
        /// Set the specified mixed to type DataType.Binary and set the value to the 
        /// bytes in the buffer specified.
        /// The prior contents of the mixed field is lost.
        /// The field will contain a copy of the bytes specified.
        /// The bytes are specified as a byte array.
        /// </summary>
        /// <param name="columnIndex">Zero based index of the column of the field to set</param>
        /// <param name="rowIndex">zero based index of the row of the field to set</param>
        /// <param name="value">byte array value to set</param>

        public void SetMixedBinary(long columnIndex, long rowIndex, byte[] value)
            //idea:perhaps we should also support the user passing us a stream?
        {
            ValidateIsValid();
            ValidateColumnRowTypeMixed(columnIndex, rowIndex);
            ValidateReadWrite();
            SetMixedBinaryNoCheck(columnIndex, rowIndex, value);
        }

        /// <summary>
        /// Set the specified mixed to type DataType.Table and set the value to the
        /// subtable specification provided.
        /// The prior contents of the mixed field is lost.
        /// Note that the mixed will change type to DataType.Table no matter what the type
        /// was before.
        /// The Table in the mixed will become a structure and data copy of the table provided
        /// </summary>
        /// <param name="columnIndex">Zero based index of the column of the field to set</param>
        /// <param name="rowIndex">zero based index of the row of the field to set</param>
        /// <param name="source">Table object which the mixed field will become a deep copy of</param>

        public void SetMixedSubTable(long columnIndex, long rowIndex, Table source)
        {
            ValidateIsValid();
            ValidateColumnRowTypeMixed(columnIndex, rowIndex);
            ValidateReadWrite();
            SetMixedSubTableNoCheck(columnIndex, rowIndex, source);
        }

        
        /// <summary>
        /// Set the specified mixed to type DataType.Date and set the value to the DateTime specified.
        /// The prior contents of the mixed field is lost.
        /// Note that the mixed will change type to DataType.Date no matter what the type
        /// was before.
        /// Note that DataType.Date is a time_t and only stores values after 1970,1,1 and only
        /// stores time with a one second precision.
        /// Will throw if the date is earlier than 1970.
        /// </summary>
        /// <param name="columnIndex">Zero based Index of the column of the field to set</param>
        /// <param name="rowIndex">Zero based index of the row of the field to set</param>
        /// <param name="value">DateTime value to set (will be truncated to nearest second)</param>
        public void SetMixedDateTime(long columnIndex, long rowIndex, DateTime value)
        {
            ValidateIsValid();
            ValidateColumnRowTypeMixed(columnIndex, rowIndex);
            ValidateReadWrite();
            SetMixedDateTimeNoCheck(columnIndex, rowIndex, value);
        }


        /// <summary>
        /// Set the specified mixed to type DataType.Date and set the value to the DateTime specified.
        /// The prior contents of the mixed field is lost.
        /// Note that the mixed will change type to DataType.Date no matter what the type
        /// was before.
        /// Note that DataType.Date is a time_t and only stores values after 1970,1,1 and only
        /// stores time with a one second precision.
        /// Will throw if the date is earlier than 1970.
        /// </summary>
        /// <param name="columnName">Name of the column of the field to set</param>
        /// <param name="rowIndex">zero based index of the row of the field to set</param>
        /// <param name="value">DateTime value to set (will be truncated to nearest second)</param>

        public void SetMixedDateTime(string columnName, long rowIndex, DateTime value)
        {
            var columnIndex = GetColumnIndex(columnName);
            ValidateReadWrite();
            SetMixedDateTimeNoColumnCheck(columnIndex, rowIndex, value);
        }


        /// <summary>
        /// Set the specified mixed to type float and set the value to the float specified.
        /// The prior contents of the mixed field is lost.
        /// Note that the mixed will change type to float no matter what the type
        /// was before.
        /// </summary>
        /// <param name="columnIndex">zero based index of the column of the field to set</param>
        /// <param name="rowIndex">zero based index of the row of the field to set</param>
        /// <param name="value">float value to set</param>
        public void SetMixedFloat(long columnIndex, long rowIndex, float value)
        {
            ValidateIsValid();
            ValidateColumnRowTypeMixed(columnIndex, rowIndex);
            ValidateReadWrite();
            SetMixedFloatNoCheck(columnIndex, rowIndex, value);
        }

        /// <summary>
        /// Set the specified mixed to type double and set the value to the double specified.
        /// The prior contents of the mixed field is lost.
        /// Note that the mixed will change type to double no matter what the type
        /// was before.
        /// </summary>
        /// <param name="columnIndex">zero based index of the column of the field to set</param>
        /// <param name="rowIndex">zero based index of the row of the field to set</param>
        /// <param name="value">double value to set</param>
        public void SetMixedDouble(long columnIndex, long rowIndex, double value)
        {
            ValidateIsValid();
            ValidateColumnRowTypeMixed(columnIndex, rowIndex);
            ValidateReadWrite();
            SetMixedDoubleNoCheck(columnIndex, rowIndex, value);
        }

        private void SetMixedDateTimeNoColumnCheck(long columnIndex, long rowIndex, DateTime value)
        {
            ValidateTypeMixed(columnIndex); //only checks that the CI points to a mixed
            ValidateRowIndex(rowIndex); //only checks that the row is valid
            SetMixedDateTimeNoCheck(columnIndex, rowIndex, value);
            //this is okay as a mixed will be set to the type you put into it
        }

        internal void SetMixedDateTimeNoRowCheck(long columnIndex, long rowIndex, DateTime value)
        {
            ValidateColumnIndexAndTypeMixed(columnIndex);
            SetMixedDateTimeNoCheck(columnIndex, rowIndex, value);
        }

        internal void SetMixedDateTimeNoRowCheck(string columnName, long rowIndex, DateTime value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeMixed(columnIndex);
            SetMixedDateTimeNoCheck(columnIndex, rowIndex, value);
        }     


        //we know column and row indicies are valid, but need to check if the column is in fact a mixed
        internal DataType GetMixedTypeCheckType(long columnIndex, long rowIndex)
        {
            ValidateTypeMixed(columnIndex);
            return GetMixedTypeNoCheck(columnIndex, rowIndex);
        }

        //Used when called directly from tableorview by the user. validates that column and row are legal indexes and that the type is mixed. Used for instance when user wants to store some type to a mixed (then the type of mixed does not matter as it will get overwritten)
        private void ValidateColumnRowTypeMixed(long columnIndex, long rowIndex)
        {
            ValidateColumnAndRowIndex(columnIndex, rowIndex);
            ValidateTypeMixed(columnIndex);
        }

        private void ValidateMixedType(long columnIndex, long rowIndex, DataType mixedType)
        {

            if (GetMixedType(columnIndex, rowIndex) != mixedType)
            {
                throw new ArgumentOutOfRangeException("columnIndex",
                    string.Format(CultureInfo.InvariantCulture,
                        "Attempting to read a type({0}) from mixed({1},{2}) mixed is of type {3}", mixedType,
                        columnIndex, rowIndex, GetMixedTypeNoCheck(columnIndex, rowIndex)));
            }
        }

        //used when reading data from a mixed - we've go to check the type of the mixed before attempting to read from it
        private void ValidateColumnRowMixedType(long columnIndex, long rowIndex, DataType mixedType)
        {
            ValidateColumnRowTypeMixed(columnIndex, rowIndex); //we only progress if the field is mixed
            ValidateMixedType(columnIndex, rowIndex, mixedType);
        }

        //used when reading data from a mixed via a row, so we know the row index is fine and should not be checked
        private void ValidateColumnMixedType(long columnIndex, long rowIndex, DataType mixedType)
        {
            ValidateTypeMixed(columnIndex); //we only progress if the field is mixed
            ValidateMixedType(columnIndex, rowIndex, mixedType);
        }


        internal void ValidateRowIndex(long rowIndex)
        {
            if (rowIndex >= Size || rowIndex < 0)
            {
                throw new ArgumentOutOfRangeException("rowIndex",
                    string.Format(CultureInfo.InvariantCulture,
                        "{0} accessed with an invalid Row Index{1}. Table Size is:{2}", ObjectIdentification(), rowIndex,
                        Size));
                //re-calculating when composing error message to avoid creating a variable in a performance sensitive place
            }
        }

        internal void ValidateColumnIndex(long columnIndex)
        {
            ValidateIsValid();
            if (columnIndex >= ColumnCount || columnIndex < 0)
            {
                throw new ArgumentOutOfRangeException("columnIndex",
                    String.Format(CultureInfo.InvariantCulture, "illegal columnIndex:{0} Table Column Count:{1}",
                        columnIndex, ColumnCount));
            }
        }

        //the parameter is the column type that was used on access, and it was not the correct one
        private string GetColumnTypeErrorString(long columnIndex, DataType columnType)
        {
            return String.Format(CultureInfo.InvariantCulture,
                "column:{0} invalid data access. Real column DataType:{1} Accessed as {2}", columnIndex,
                ColumnTypeNoCheck(columnIndex), columnType);
        }


        //only call if columnIndex is already validated or known to be int
        internal void ValidateTypeInt(long columnIndex)
        {
            if (ColumnTypeNoCheck(columnIndex) != DataType.Int)
            {
                throw new ArgumentException(GetColumnTypeErrorString(columnIndex, DataType.Int));
            }
        }

        //todo:check performance hit if refactored to take DataType to validate as a string
        //only call if columnIndex is already validated 
        internal void ValidateTypeString(long columnIndex)
        {
            if (ColumnTypeNoCheck(columnIndex) != DataType.String)
            {
                throw new ArgumentException(GetColumnTypeErrorString(columnIndex, DataType.String));
            }
        }

        private void ValidateTypeBinary(long columnIndex)
        {
            if (ColumnTypeNoCheck(columnIndex) != DataType.Binary)
            {
                throw new ArgumentException(GetColumnTypeErrorString(columnIndex, DataType.Binary));
            }
        }

        private void ValidateTypeDouble(long columnIndex)
        {
            if (ColumnTypeNoCheck(columnIndex) != DataType.Double)
            {
                throw new ArgumentException(GetColumnTypeErrorString(columnIndex, DataType.Double));
            }
        }

        private void ValidateTypeFloat(long columnIndex)
        {
            if (ColumnTypeNoCheck(columnIndex) != DataType.Float)
            {
                throw new ArgumentException(GetColumnTypeErrorString(columnIndex, DataType.Float));
            }
        }




        //only call if columnIndex is already validated         
        private void ValidateTypeMixed(long columnIndex)
        {
            if (ColumnTypeNoCheck(columnIndex) != DataType.Mixed)
            {
                throw new ArgumentOutOfRangeException("columnIndex",
                    GetColumnTypeErrorString(columnIndex, DataType.Mixed));
            }
        }

        //NOTE! only call this with a validated columnIndex or we might call c++ with an unchecled column index
        private void ValidateTypeSubTable(long columnIndex)
        {
            if (ColumnTypeNoCheck(columnIndex) != DataType.Table)
            {
                throw new ArgumentOutOfRangeException("columnIndex",
                    GetColumnTypeErrorString(columnIndex, DataType.Table));
            }
        }


        //only call if columnIndex is already validated       
        private void ValidateTypeBool(long columnIndex)
        {
            if (ColumnTypeNoCheck(columnIndex) != DataType.Bool)
            {
                throw new ArgumentException(GetColumnTypeErrorString(columnIndex, DataType.Bool));
            }
        }

        //only call if columnIndex is already validated         
        private void ValidateTypeDate(long columnIndex)
        {
            if (ColumnTypeNoCheck(columnIndex) != DataType.Date)
            {
                throw new ArgumentException(GetColumnTypeErrorString(columnIndex, DataType.Date));
            }
        }


        //throw if the table is empty
        //used for instance with average, which does not make sense to call if there are no rows
        private void ValidateIsNotEmpty()
        {
                if (Size == 0)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                        "Unsupported operation called that only work on non-empty tables . this table is empty"));
                }
        }

        //throw if the table contains any rows
        //this is always called before other methods when it is called so ValidateIsValid is stuffed in here too,
        //to reduce clutter where ValidateIsEmpty is called.
        internal void ValidateIsEmpty()
        {            
            ValidateIsValid();
            if (Size != 0)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                    "Unsupported operation called that only work on empty tables . this table have {0} data rows", Size));
            }
        }

        //todo:shouldn't we report back if no records were removed because the table was empty?
        //in c++ no reporting is done either : void        remove_last() {if (!pty()) remove(m_size-1);}

        /// <summary>
        /// Remove the last row.
        /// If no rows exists in the table, nothing happens
        /// </summary>
        public void RemoveLast() //this could be a c++ call and save the extra call to get size
        {
            ValidateIsValid();
            ValidateReadWrite();
            var s = Size;
            if (s > 0)
            {
                RemoveNoCheck(s - 1);
            }
        }


        /// <summary>
        /// Remove a row from the table, specified by its zero based row index.
        /// All data after the row removed will be moved one back.
        /// Size will be one less.
        /// </summary>
        /// <param name="rowIndex">Zero based row Index of the row to remove</param>
        public void Remove(long rowIndex)
        {
            ValidateIsValid();
            ValidateRowIndex(rowIndex);
            ValidateReadWrite();
            RemoveNoCheck(rowIndex);            
        }

        /// <summary>
        /// Set the specified Datatype.Int field to the specified value.
        /// DataType.Int stores a 64 bit integral value
        /// </summary>
        /// <param name="columnIndex">Zero based index of column of field to set</param>
        /// <param name="rowIndex">Zero based index of row of field to set</param>
        /// <param name="value">long value to set in field</param>

        public void SetLong(long columnIndex, long rowIndex, long value)
        {
            ValidateIsValid();
            ValidateColumnAndRowIndex(columnIndex, rowIndex);
            ValidateTypeInt(columnIndex);
            ValidateReadWrite();
            SetLongNoCheck(columnIndex, rowIndex, value);
        }

        /// <summary>
        /// Set the specified Datatype.Int field to the specified value.
        /// DataType.Int stores a 64 bit integral value
        /// </summary>
        /// <param name="columnName">Zero based index of column of field to set</param>
        /// <param name="rowIndex">Zero based index of row of field to set</param>
        /// <param name="value">long value to set in field</param>


        public void SetLong(String columnName, long rowIndex, long value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeInt(columnIndex);
            ValidateRowIndex(rowIndex);
            ValidateReadWrite();
            SetLongNoCheck(columnIndex, rowIndex, value);
        }


        /// <summary>
        /// Set the specified Datatype.Int field to the specified value
        /// this method is a convenience method that takes an int instead
        /// of a long, even though Datatype.Int is a 64 bit field
        /// </summary>
        /// <param name="columnIndex">Zero based index of column of field to set</param>
        /// <param name="rowIndex">Zero based index of row of field to set</param>
        /// <param name="value">int value to set in field</param>
        public void SetInt(long columnIndex, long rowIndex, int value)
        {
            ValidateIsValid();
            ValidateColumnAndRowIndex(columnIndex, rowIndex);
            ValidateTypeInt(columnIndex);
            ValidateReadWrite();
            SetLongNoCheck(columnIndex, rowIndex, value);
        }


        /// <summary>
        /// Set the specified Datatype.Int field to the specified value
        /// this method is a convenience method that takes an int instead
        /// of a long, even though Datatype.Int is a 64 bit field
        /// </summary>
        /// <param name="columnName">Zero based index of column of field to set</param>
        /// <param name="rowIndex">Zero based index of row of field to set</param>
        /// <param name="value">int value to set in field</param>
        public void SetInt(String columnName, long rowIndex, int value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeInt(columnIndex);
            ValidateRowIndex(rowIndex);
            ValidateReadWrite();
            SetLongNoCheck(columnIndex, rowIndex, value);
        }

        /// <summary>
        /// Set the specified Datatype.Double field to the specified value        
        /// </summary>
        /// <param name="columnIndex">Zero based index of column of field to set</param>
        /// <param name="rowIndex">Zero based index of row of field to set</param>
        /// <param name="value">double value to set in field</param>
        public void SetDouble(long columnIndex, long rowIndex, double value)
        {
            ValidateIsValid();
            ValidateColumnAndRowIndex(columnIndex, rowIndex);
            ValidateTypeDouble(columnIndex);
            ValidateReadWrite();
            SetDoubleNoCheck(columnIndex, rowIndex, value);
        }

        /// <summary>
        /// Set the specified Datatype.Double field to the specified value        
        /// </summary>
        /// <param name="columnName">Name of the column of the field to set</param>
        /// <param name="rowIndex">Zero based index of the row of the field to set</param>
        /// <param name="value">double value to set in field</param>
        public void SetDouble(String columnName, long rowIndex, double value)
        {            
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeDouble(columnIndex);
            ValidateRowIndex(rowIndex);
            ValidateReadWrite();
            SetDoubleNoCheck(columnIndex, rowIndex, value);
        }

        /// <summary>
        /// Set the specified Datatype.Float field to the specified value        
        /// </summary>
        /// <param name="columnIndex">Zero based index of column of field to set</param>
        /// <param name="rowIndex">Zero based index of row of field to set</param>
        /// <param name="value">float value to set in field</param>
        public void SetFloat(long columnIndex, long rowIndex, float value)
        {
            ValidateIsValid();
            ValidateColumnAndRowIndex(columnIndex, rowIndex);
            ValidateTypeFloat(columnIndex);
            ValidateReadWrite();
            SetFloatNoCheck(columnIndex, rowIndex, value);
        }

        /// <summary>
        /// Set the specified Datatype.Float field to the specified value        
        /// </summary>
        /// <param name="columnName">Name of the column of the field to set</param>
        /// <param name="rowIndex">Zero based index of row of field to set</param>
        /// <param name="value">float value to set in field</param>
        public void SetFloat(String columnName, long rowIndex, float value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeFloat(columnIndex);
            ValidateRowIndex(rowIndex);
            ValidateReadWrite();
            SetFloatNoCheck(columnIndex, rowIndex, value);
        }

#if V40PLUS
        /// <summary>
        /// Sets a subtable into the specified field.
        /// The subtable being set can be one of several classes/objects/values
        /// NULL - will set a new empty subtable
        /// Table - The Table variable must have the same schema as the subtable field. A subtable is
        /// created and all data in Table is copied into the subtable
        /// IEnumerable - a sequence of IEnumerable objects each also implementing IEnumerable
        /// Each of these in turn must contain a sequence of objects matching the individual fields of the subtable.
        /// For instance You can call with an array of rows each containing an array of field data
        /// example:SetSubTable("phones",0,12,new [] {new[]{"123-123-123","John"},new[]{"123-124-124","Kate"}})
        /// </summary>
        /// <param name="columnIndex">zero based index of the column of the field where the subable should be set</param>
        /// <param name="rowIndex">Zero based row index of the field where the subtable should be set</param>
        /// <param name="value">Table, or Ienumerable that can be evaluated to shema-matching suitable subtable data by the binding</param>
        public void SetSubTable(long columnIndex, long rowIndex, IEnumerable<object> value)
        {
            ValidateIsValid();
            ValidateColumnAndRowIndex(columnIndex, rowIndex);
            ValidateTypeSubTable(columnIndex);
            ValidateReadWrite();
            SetSubTableNoCheckHighLevel(columnIndex, rowIndex, value);
                //even though this is called nocheck, it does check if the passed value fits the subtable scheme at C,R
        }
#else
        /// <summary>
        /// Sets a subtable into the specified field.
        /// The subtable being set must be a Table or Null
        /// NULL - will set a new empty subtable
        /// The Table variable must have the same schema as the subtable field. A subtable is
        /// created and all data in Table is copied into the subtable
        /// </summary>
        /// <param name="columnIndex">zero based index of the column of the field where the subable should be set</param>
        /// <param name="rowIndex">Zero based row index of the field where the subtable should be set</param>
        /// <param name="value">Table, must have the same structure as the subtable being set</param>
        public void SetSubTable(long columnIndex, long rowIndex, Table value)
        {
            ValidateIsValid();
            ValidateColumnAndRowIndex(columnIndex, rowIndex);
            ValidateTypeSubTable(columnIndex);
            ValidateReadWrite();
            SetSubTableNoCheckHighLevel(columnIndex, rowIndex, value);
        }

        /// <summary>
        /// Sets a subtable into the specified field.      
        /// NULL - will set a new empty subtable
        /// otherwise value must be a sequence of IEnumerable objects each also implementing IEnumerable
        /// Each of these in turn must contain a sequence of objects matching the individual fields of the subtable.
        /// For instance You can call with an array of rows each containing an array of field data
        /// example:SetSubTable("phones",0,12,new [] {new[]{"123-123-123","John"},new[]{"123-124-124","Kate"}})
        /// </summary>
        /// <param name="columnIndex">zero based index of the column of the field where the subable should be set</param>
        /// <param name="rowIndex">Zero based row index of the field where the subtable should be set</param>
        /// <param name="value">Table, or Ienumerable that can be evaluated to shema-matching suitable subtable data by the binding</param>
        public void SetSubTable(long columnIndex, long rowIndex, IEnumerable<Object> value)
        {
            ValidateIsValid();
            ValidateColumnAndRowIndex(columnIndex, rowIndex);
            ValidateTypeSubTable(columnIndex);
            ValidateReadWrite();
            SetSubTableNoCheckHighLevel(columnIndex, rowIndex, value);
        }

        /// <summary>
        /// Sets a subtable into the specified field.
        /// The subtable being set must be a TableView or Null
        /// NULL - will set a new empty subtable
        /// The TableView variable must have the same schema as the subtable field. A subtable is
        /// created and all rows in the TableView are copied into the subtable
        /// </summary>
        /// <param name="columnIndex">zero based index of the column of the field where the subable should be set</param>
        /// <param name="rowIndex">Zero based row index of the field where the subtable should be set</param>
        /// <param name="value">TableView, must have the same structure as the subtable being set</param>
        public void SetSubTable(long columnIndex, long rowIndex, TableView value)
        {
            ValidateIsValid();
            ValidateColumnAndRowIndex(columnIndex, rowIndex);
            ValidateTypeSubTable(columnIndex);
            ValidateReadWrite();
            SetSubTableNoCheckHighLevel(columnIndex, rowIndex, value);
        }



#endif


#if V40PLUS
        /// <summary>
        /// Sets a subtable into the specified field.
        /// The subtable being set can be one of several classes/objects/values
        /// NULL - will set a new empty subtable
        /// Table - The Table variable must have the same schema as the subtable field. A subtable is
        /// created and all data in Table is copied into the subtable
        /// IEnumerable - a sequence of IEnumerable objects each also implementing IEnumerable
        /// Each of these in turn must contain a sequence of objects matching the individual fields of the subtable.
        /// For instance You can call with an array of rows each containing an array of field data
        /// example:SetSubTable("phones",0,12,new [] {new[]{"123-123-123","John"},new[]{"123-124-124","Kate"}})
        /// </summary>
        /// <param name="columnName">Name of the column of the field where the subable should be set</param>
        /// <param name="rowIndex">Zero based row index of the field where the subtable should be set</param>
        /// <param name="value">Table, or Ienumerable that can be evaluated to shema-matching suitable subtable data by the binding</param>
        public void SetSubTable(String columnName, long rowIndex, IEnumerable<object> value)//4.0 and later allows a IEnumerable<object> to receive an IEnumerable<Row>
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeSubTable(columnIndex);
            ValidateRowIndex(rowIndex);
            ValidateReadWrite();
            SetSubTableNoCheckHighLevel(columnIndex, rowIndex, value);
        }
#else
        /// <summary>
        /// Sets a subtable into the specified field.
        /// The subtable being set must be a Table or Null
        /// NULL - will set a new empty subtable
        /// The Table variable must have the same schema as the subtable field. A subtable is
        /// created and all data in Table is copied into the subtable
        /// </summary>
        /// <param name="columnName">name of the column of the field where the subable should be set</param>
        /// <param name="rowIndex">Zero based row index of the field where the subtable should be set</param>
        /// <param name="value">Table, must have the same structure as the subtable being set</param>
        public void SetSubTable(String columnName, long rowIndex, Table value)
        {
            ValidateIsValid();
            var columnIndex = GetColumnIndex(columnName);
            ValidateTypeSubTable(columnIndex);
            ValidateRowIndex(rowIndex);
            ValidateReadWrite();
            SetSubTableNoCheckHighLevel(columnIndex, rowIndex, value);
        }

        /// <summary>
        /// Sets a subtable into the specified field.      
        /// NULL - will set a new empty subtable
        /// otherwise value must be a sequence of IEnumerable objects each also implementing IEnumerable
        /// Each of these in turn must contain a sequence of objects matching the individual fields of the subtable.
        /// For instance You can call with an array of rows each containing an array of field data
        /// example:SetSubTable("phones",0,12,new [] {new[]{"123-123-123","John"},new[]{"123-124-124","Kate"}})
        /// </summary>
        /// <param name="columnName">name of the column of the field where the subable should be set</param>
        /// <param name="rowIndex">Zero based row index of the field where the subtable should be set</param>
        /// <param name="value">Table, or Ienumerable that can be evaluated to shema-matching suitable subtable data by the binding</param>
        public void SetSubTable(String columnName, long rowIndex, IEnumerable<object> value)
        {
            ValidateIsValid();
            var columnIndex = GetColumnIndex(columnName);
            ValidateTypeSubTable(columnIndex);
            ValidateRowIndex(rowIndex);
            ValidateReadWrite();
            SetSubTableNoCheckHighLevel(columnIndex, rowIndex, value);
        }
#endif


        internal void SetLongNoRowCheck(long columnIndex, long rowIndex, long value)
        {
            ValidateIsValid();
            ValidateColumnIndexAndTypeInt(columnIndex);
            SetLongNoCheck(columnIndex, rowIndex, value);
        }

        internal void SetIntNoRowCheck(long columnIndex, long rowIndex, int value)
        {
            ValidateIsValid();
            ValidateColumnIndexAndTypeInt(columnIndex);
            SetIntNoCheck(columnIndex, rowIndex, value);
        }

        internal void SetLongNoRowCheck(string columnName, long rowIndex, long value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeInt(columnIndex);
            SetLongNoCheck(columnIndex, rowIndex, value);
        }

        internal void SetIntNoRowCheck(string columnName, long rowIndex, int value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeInt(columnIndex);
            SetIntNoCheck(columnIndex, rowIndex, value);
        }


        internal void SetFloatNoRowCheck(long columnIndex, long rowIndex, float value)
        {
            ValidateIsValid();
            ValidateColumnIndexAndTypeFloat(columnIndex);
            SetFloatNoCheck(columnIndex, rowIndex, value);
        }

        internal void SetFloatNoRowCheck(string columnName, long rowIndex, float value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeFloat(columnIndex);
            SetFloatNoCheck(columnIndex, rowIndex, value);
        }


        //the norowcheck methods are used in row.cs
        internal void SetDoubleNoRowCheck(long columnIndex, long rowIndex, double value)
        {
            ValidateIsValid();
            ValidateColumnIndexAndTypeDouble(columnIndex);
            SetDoubleNoCheck(columnIndex, rowIndex, value);
        }

        internal void SetDoubleNoRowCheck(string columnName, long rowIndex, double value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeDouble(columnIndex);
            SetDoubleNoCheck(columnIndex, rowIndex, value);
        }










        /// <summary>
        /// return integer value from a specified field
        /// Field must be DataType.Int
        /// DataType.Int stores values up to 64 bit signed, so
        /// this method returns long to make sure all values fit
        /// </summary>
        /// <param name="columnIndex">name of column of field</param>
        /// <param name="rowIndex">row index of field</param>
        /// <returns>value stored in field</returns>

        public long GetLong(long columnIndex, long rowIndex)
        {
            ValidateIsValid();
            ValidateRowIndex(rowIndex);
            ValidateColumnIndexAndTypeInt(columnIndex);            
            return GetLongNoCheck(columnIndex, rowIndex); //could be sped up if we directly call UnsafeNativeMethods
        }

        /// <summary>
        /// return integer value from a specified field
        /// Field must be DataType.Int
        /// DataType.Int stores values up to 64 bit signed, so
        /// this method returns long to make sure all values fit
        /// </summary>
        /// <param name="columnName">name of column of field</param>
        /// <param name="rowIndex">row index of field</param>
        /// <returns>value stored in field</returns>

        public long GetLong(String columnName, long rowIndex)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateRowIndex(rowIndex);
            ValidateTypeInt(columnIndex);            
            return GetLongNoCheck(columnIndex, rowIndex);
        }


        /// <summary>
        /// return string from a specified field
        /// Field must be DataType.String
        /// String returned is UTF-16 (standard C#).
        /// String in TightDb database is UTF-8, conversion
        /// is done automatically.
        /// </summary>
        /// <param name="columnIndex">index of column of field</param>
        /// <param name="rowIndex">row index of field</param>
        /// <returns>String value stored in field</returns>

        public string GetString(long columnIndex, long rowIndex)
        {
            ValidateIsValid();
            ValidateRowIndex(rowIndex);
            ValidateColumnIndexAndTypeString(columnIndex);           
            return GetStringNoCheck(columnIndex, rowIndex);
        }

        /// <summary>
        /// return string from a specified field
        /// Field must be DataType.String
        /// String returned is UTF-16 (standard C#).
        /// String in TightDb database is UTF-8, conversion
        /// is done automatically.
        /// </summary>
        /// <param name="columnName">index of column of field</param>
        /// <param name="rowIndex">row index of field</param>
        /// <returns>String value stored in field</returns>
        public string GetString(String columnName, long rowIndex)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateRowIndex(rowIndex);
            ValidateTypeString(columnIndex);            
            return GetStringNoCheck(columnIndex, rowIndex);
        }


        /// <summary>
        /// return byte arrray with binary data from a specified field
        /// Field must be DataType.binary
        /// </summary>
        /// <param name="columnIndex">index of column of field</param>
        /// <param name="rowIndex">row index of field</param>
        /// <returns>binary value stored in field</returns>

        public byte[] GetBinary(long columnIndex, long rowIndex)
        {
            ValidateIsValid();
            ValidateRowIndex(rowIndex);
            ValidateColumnIndexAndTypeBinary(columnIndex);            
            return GetBinaryNoCheck(columnIndex, rowIndex);
        }

        /// <summary>
        /// return byte arrray with binary data from a specified field
        /// Field must be DataType.binary
        /// </summary>
        /// <param name="columnName">name of column of field</param>
        /// <param name="rowIndex">row index of field</param>
        /// <returns>binary value stored in field</returns>
        
        public byte[] GetBinary(String columnName, long rowIndex)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateRowIndex(rowIndex);
            ValidateTypeBinary(columnIndex);            
            return GetBinaryNoCheck(columnIndex, rowIndex);
        }


        /// <summary>
        /// return double value of field specified by column name and row index.
        /// Field must be of type DataType.Double (not float, not mixed)
        /// </summary>
        /// <param name="columnIndex">Name of column of field</param>
        /// <param name="rowIndex">zero based row index of field</param>
        /// <returns>value of field</returns>

        public Double GetDouble(long columnIndex, long rowIndex)
        {
            ValidateIsValid();
            ValidateRowIndex(rowIndex);            
            return GetDoubleNoRowCheck(columnIndex, rowIndex);
        }

        /// <summary>
        /// return double value of field specified by column name and row index.
        /// Field must be of type DataType.Double (not float, not mixed)
        /// </summary>
        /// <param name="columnName">Name of column of field</param>
        /// <param name="rowIndex">zero based row index of field</param>
        /// <returns>value of field</returns>
        /// 
        public Double GetDouble(String columnName, long rowIndex)
        {
            ValidateIsValid();
            ValidateRowIndex(rowIndex);
            return GetDoubleNoRowCheck(columnName, rowIndex);
        }

        internal Double GetDoubleNoRowCheck(long columnIndex, long rowIndex)
        {
            ValidateIsValid();
            ValidateColumnIndexAndTypeDouble(columnIndex);
            return GetDoubleNoCheck(columnIndex, rowIndex);
        }

        internal Double GetDoubleNoRowCheck(String columnName, long rowIndex)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeDouble(columnIndex);
            return GetDoubleNoCheck(columnIndex, rowIndex);
        }

        /// <summary>
        /// return float value of field specified by column name and row index.
        /// Field must be of type DataType.Float (not double, not mixed)
        /// </summary>
        /// <param name="columnIndex">Name of column of field</param>
        /// <param name="rowIndex">zero based row index of field</param>
        /// <returns>value of field</returns>
        public float GetFloat(long columnIndex, long rowIndex)
        {
            ValidateIsValid();
            ValidateRowIndex(rowIndex);
            return GetFloatNoRowCheck(columnIndex, rowIndex);
        }

        /// <summary>
        /// return float value of field specified by column name and row index.
        /// Field must be of type DataType.Float (not double, not mixed)
        /// </summary>
        /// <param name="columnName">Name of column of field with float</param>
        /// <param name="rowIndex">zero based row index of field with float</param>
        /// <returns>value of field with float</returns>
        public float GetFloat(String columnName, long rowIndex)
        {
            ValidateIsValid();
            ValidateRowIndex(rowIndex);
            return GetFloatNoRowCheck(columnName, rowIndex);
        }

        internal float GetFloatNoRowCheck(long columnIndex, long rowIndex)
        {
            ValidateIsValid();
            ValidateColumnIndexAndTypeFloat(columnIndex);
            return GetFloatNoCheck(columnIndex, rowIndex);
        }

        internal float GetFloatNoRowCheck(String columnName, long rowIndex)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeFloat(columnIndex);
            return GetFloatNoCheck(columnIndex, rowIndex);
        }



        /// <summary>
        /// Set the string value of a table field to the specified string value.
        /// Specified String will be converted to UTC-8 and put into the field
        /// </summary>
        /// <param name="columnIndex">zero based index of the column of the field where value is stored</param>
        /// <param name="rowIndex">row index of the field where value is stored</param>
        /// <param name="value">the string to store</param>
        public void SetString(long columnIndex, long rowIndex, string value)
        {
            ValidateIsValid();
            ValidateRowIndex(rowIndex);
            ValidateColumnIndexAndTypeString(columnIndex);
            ValidateReadWrite();
            SetStringNoCheck(columnIndex, rowIndex, value);
        }

        /// <summary>
        /// Set the string value of a table field to the specified string value.
        /// Specified String will be converted to UTC-8 and put into the field
        /// </summary>
        /// <param name="columnName">Name of the column of the field where value is stored</param>
        /// <param name="rowIndex">row index of the field where value is stored</param>
        /// <param name="value">the string to store</param>
        public void SetString(string columnName, long rowIndex, string value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateRowIndex(rowIndex);
            ValidateTypeString(columnIndex);
            ValidateReadWrite();
            SetStringNoCheck(columnIndex, rowIndex, value);
        }

        /// <summary>
        /// Copies the binary data referenced by value to the tightdb database
        /// </summary>
        /// <param name="columnIndex">column index of field to put data into</param>
        /// <param name="rowIndex">row index of field to put data into</param>
        /// <param name="value">byte array or buffer with data</param>
        public void SetBinary(long columnIndex, long rowIndex, byte[] value)
        {
            ValidateIsValid();
            ValidateRowIndex(rowIndex);
            ValidateColumnIndexAndTypeBinary(columnIndex);
            ValidateReadWrite();
            SetBinaryNoCheck(columnIndex, rowIndex, value);
        }

        /// <summary>
        /// Copies the binary data referenced by value to the tightdb database
        /// </summary>
        /// <param name="columnName">column name of field to put data into</param>
        /// <param name="rowIndex">row index of field to put data into</param>
        /// <param name="value">byte array or buffer with data</param>
        public void SetBinary(string columnName, long rowIndex, byte[] value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateRowIndex(rowIndex);
            ValidateTypeBinary(columnIndex);
            ValidateReadWrite();
            SetBinaryNoCheck(columnIndex, rowIndex, value);
        }




        //validation of a column index as well as the type of that index. To save a stack parameter with the type, there are one method per type        

        internal void ValidateColumnIndexAndTypeString(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateTypeString(columnIndex);
        }

        private void ValidateColumnIndexAndTypeInt(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateTypeInt(columnIndex);
        }

        private void ValidateColumnIndexAndTypeBool(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateTypeBool(columnIndex);
        }

        private void ValidateColumnIndexAndTypeDate(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateTypeDate(columnIndex);
        }

        private void ValidateColumnIndexAndTypeBinary(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateTypeBinary(columnIndex);
        }

        private void ValidateColumnIndexAndTypeDouble(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateTypeDouble(columnIndex);
        }

        private void ValidateColumnIndexAndTypeFloat(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateTypeFloat(columnIndex);
        }

        private void ValidateColumnIndexAndTypeSubTable(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateTypeSubTable(columnIndex);
        }

        private void ValidateColumnIndexAndTypeMixed(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateTypeMixed(columnIndex);
        }


        internal void SetStringNoRowCheck(long columnIndex, long rowIndex, string value)
        {
            ValidateColumnIndexAndTypeString(columnIndex);
            SetStringNoCheck(columnIndex, rowIndex, value);
        }

        internal void SetStringNoRowCheck(string columnName, long rowIndex, string value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeString(columnIndex);
            SetStringNoCheck(columnIndex, rowIndex, value);
        }

        internal void SetDateTimeNoRowCheck(long columnIndex, long rowIndex, DateTime value)
        {
            ValidateColumnIndexAndTypeDate(columnIndex);
            SetDateTimeNoCheck(columnIndex, rowIndex, value);
        }

        internal void SetDateTimeNoRowCheck(string columnName, long rowIndex, DateTime value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeDate(columnIndex);
            SetDateTimeNoCheck(columnIndex, rowIndex, value);
        }

//in .net 35 Table is not IEnumerable<object>
#if !V40PLUS

        internal void SetSubTableNoRowCheck(long columnIndex, long rowIndex, IEnumerable<Object> value)
        {
            ValidateColumnIndexAndTypeSubTable(columnIndex);
            SetSubTableNoCheckHighLevel(columnIndex, rowIndex, value);
        }

        internal void SetSubTableNoRowCheck(string columnName, long rowIndex, IEnumerable<Object> value)
        {
            var columnIndex = GetColumnIndex(columnName);
            ValidateTypeSubTable(columnIndex);
            SetSubTableNoCheckHighLevel(columnIndex, rowIndex, value);
        }

        internal void SetSubTableNoRowCheck(long columnIndex, long rowIndex,Table value)
        {
            ValidateColumnIndexAndTypeSubTable(columnIndex);
            SetSubTableNoCheckHighLevel(columnIndex, rowIndex, value);
        }

        internal void SetSubTableNoRowCheck(string columnName, long rowIndex,Table value)
        {
            var columnIndex = GetColumnIndex(columnName);
            ValidateTypeSubTable(columnIndex);
            SetSubTableNoCheckHighLevel(columnIndex, rowIndex, value);
        }


#else
        internal void SetSubTableNoRowCheck(long columnIndex, long rowIndex, IEnumerable<Object> value)
        {
            ValidateColumnIndexAndTypeSubTable(columnIndex);
            SetSubTableNoCheckHighLevel(columnIndex, rowIndex, value);
        }

        internal void SetSubTableNoRowCheck(string columnName, long rowIndex, IEnumerable<Object> value)
        {
            var columnIndex = GetColumnIndex(columnName);
            ValidateTypeSubTable(columnIndex);
            SetSubTableNoCheckHighLevel(columnIndex, rowIndex, value);
        }
#endif

        internal String GetStringNoRowCheck(long columnIndex, long rowIndex)
        {
            ValidateColumnIndexAndTypeString(columnIndex);
            return GetStringNoCheck(columnIndex, rowIndex);
        }

        internal String GetStringNoRowCheck(string columnName, long rowIndex)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeString(columnIndex);
            return GetStringNoCheck(columnIndex, rowIndex);
        }

        internal byte[] GetBinaryNoRowCheck(long columnIndex, long rowIndex)
        {
            ValidateColumnIndexAndTypeBinary(columnIndex);
            return GetBinaryNoCheck(columnIndex, rowIndex);
        }

        internal byte[] GetBinaryNoRowCheck(string columnName, long rowIndex)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeBinary(columnIndex);
            return GetBinaryNoCheck(columnIndex, rowIndex);
        }

        internal void SetBinaryNoRowCheck(long columnIndex, long rowIndex, byte [] value)
        {
            ValidateColumnIndexAndTypeBinary(columnIndex);
            SetBinaryNoCheck(columnIndex, rowIndex, value);
        }

        internal void SetBinaryNoRowCheck(string columnName, long rowIndex, byte[] value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeBinary(columnIndex);
            SetBinaryNoCheck(columnIndex, rowIndex, value);
        }


        /// <summary>
        /// Return the column name of a column specified by its zero based index
        /// </summary>
        /// <param name="columnIndex">Zero based index of a column</param>
        /// <returns>The name of the column</returns>
        public string GetColumnName(long columnIndex)
        {
            ValidateIsValid();
            ValidateColumnIndex(columnIndex);
            return GetColumnNameNoCheck(columnIndex);
        }

        private long GetLongCheckType(long columnIndex, long rowIndex)
        {
            ValidateTypeInt(columnIndex);
            return GetLongNoCheck(columnIndex, rowIndex);
        }

        internal long GetLongNoRowCheck(long columnIndex, long rowIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateTypeInt(columnIndex);
            return GetLongNoCheck(columnIndex, rowIndex);
        }

        internal long GetLongNoRowCheck(string name, long rowIndex)
        {
            long columnIndex = GetColumnIndex(name);
            return GetLongCheckType(columnIndex, rowIndex);
        }

        /*not used so commented out
        internal String GetStringCheckType(long columnIndex, long rowIndex)
        {
            ValidateTypeString(columnIndex);
            return GetStringNoCheck(columnIndex, rowIndex);
        }
        */

        

        internal Boolean GetBooleanNoRowCheck(long columnIndex, long rowIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateTypeBool(columnIndex);
            return GetBoolNoCheck(columnIndex, rowIndex);
        }

        internal Boolean GetBooleanNoRowCheck(string name, long rowIndex)
        {
            long columnIndex = GetColumnIndex(name);
            ValidateTypeBool(columnIndex);
            return GetBoolNoCheck(columnIndex, rowIndex);
        }

        /*not used anymore
        //only call this method if You know for sure that RowIndex is less than or equal to table.size()
        //and that you know for sure that columnIndex is less than or equal to table.columncount
        internal long GetLongNoColumnRowCheck(long columnIndex, long rowIndex)
        {
            ValidateTypeInt(columnIndex);
            return GetLongNoCheck(columnIndex, rowIndex);
        }
        */


        private void ValidateColumnAndRowIndex(long columnIndex, long rowIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateRowIndex(rowIndex);
        }

        /*not used anymore
        //only call this one if You know for sure that the field at columnindex,rowindex is in fact an ordinary DataType.Int field (not mixed.integer)
        internal long GetLongNoTypeCheck(long columnIndex, long rowIndex)
        {
            ValidateColumnAndRowIndex(columnIndex,rowIndex);
            return GetLongNoCheck(columnIndex, rowIndex);
        }
        */

        /// <summary>
        /// Sets the value of a specified boolean field.
        /// </summary>
        /// <param name="columnIndex">Name of field to return</param>
        /// <param name="rowIndex">zero based row index of field to return</param>
        /// <param name="value">The Boolean value to set</param>
        public void SetBoolean(long columnIndex, long rowIndex, Boolean value)
        {
            ValidateIsValid();
            ValidateColumnAndRowIndex(columnIndex, rowIndex);
            ValidateTypeBool(columnIndex);
            ValidateReadWrite();
            SetBoolNoCheck(columnIndex, rowIndex, value);
        }

        /// <summary>
        /// Sets the time_t value in a tightdb DataType.Date filed to a rounded value
        /// of the specified DateTime variable.
        /// Note time_t stores only whole seconds, and note time_t is always UTC
        /// and note that time_t does not store values lower than 00:00:00 00:01 jan.1 1970 UTC
        /// </summary>
        /// <param name="columnIndex">Name of the column of the field to set</param>
        /// <param name="rowIndex">zero based row index of the field to set</param>
        /// <param name="value">Datetime value to convert to time_t and set</param>
        public void SetDateTime(long columnIndex, long rowIndex, DateTime value)
        {
            ValidateIsValid();
            ValidateColumnAndRowIndex(columnIndex, rowIndex);
            ValidateTypeDate(columnIndex);
            ValidateReadWrite();
            SetDateTimeNoCheck(columnIndex, rowIndex, value);
        }

        /// <summary>
        /// Sets the time_t value in a tightdb DataType.Date filed to a rounded value
        /// of the specified DateTime variable.
        /// Note time_t stores only whole seconds, and note time_t is always UTC
        /// and note that time_t does not store values lower than 00:00:00 00:01 jan.1 1970 UTC
        /// </summary>
        /// <param name="columnName">Name of the column of the field to set</param>
        /// <param name="rowIndex">zero based row index of the field to set</param>
        /// <param name="value">Datetime value to convert to time_t and set</param>
        public void SetDateTime(string columnName, long rowIndex, DateTime value)
        {
            var columnIndex = GetColumnIndex(columnName);
            ValidateTypeDate(columnIndex);
            ValidateRowIndex(rowIndex);
            ValidateReadWrite();
            SetDateTimeNoCheck(columnIndex, rowIndex, value);
        }

        internal void SetBooleanNoRowCheck(string columnName, long rowIndex, Boolean value)
        {
            var columnIndex = GetColumnIndex(columnName);
            ValidateTypeBool(columnIndex);
            SetBoolNoCheck(columnIndex, rowIndex, value);
        }

        internal void SetBooleanNoRowCheck(long columnIndex, long rowIndex, Boolean value)
        {
            ValidateColumnIndex(columnIndex);
            ValidateTypeBool(columnIndex);
            SetBoolNoCheck(columnIndex, rowIndex, value);
        }

        /// <summary>
        /// Returns the value of a specified boolean field.
        /// Field name is case sensitive.
        /// </summary>
        /// <param name="columnName">Name of field to return</param>
        /// <param name="rowIndex">zero based row index of field to return</param>
        /// <returns>True or False, value of the boolean field</returns>
        public Boolean GetBoolean(String columnName, long rowIndex)
        {
            var columnIndex = GetColumnIndex(columnName);
            ValidateRowIndex(rowIndex);
            ValidateTypeBool(columnIndex);
            return GetBoolNoCheck(columnIndex, rowIndex);
        }

        /// <summary>
        /// Returns the value of a specified boolean field.
        /// </summary>
        /// <param name="columnIndex">zero based index of field to return</param>
        /// <param name="rowIndex">zero based row index of field to return</param>
        /// <returns>True or False, value of the boolean field</returns>
        public Boolean GetBoolean(long columnIndex, long rowIndex)
        {
            ValidateIsValid();
            ValidateColumnAndRowIndex(columnIndex, rowIndex);
            ValidateTypeBool(columnIndex);
            return GetBoolNoCheck(columnIndex, rowIndex);
        }



        //public aggregate functions - field identified by its zerobased columnIndex

        /// <summary>
        /// returns the number of rows with the specified target value
        /// counts in DataType.int columns
        /// </summary>
        /// <param name="columnIndex">Zero based Index of column to look for matches in</param>
        /// <param name="target">columns with this value will be counted</param>
        /// <returns>number of rows with target value in the specified column</returns>
        public long CountLong(long columnIndex, long target)
        {            
            ValidateColumnIndexAndTypeInt(columnIndex);
            return CountLongNoCheck(columnIndex, target);
        }

        /// <summary>
        /// returns the number of rows with the specified target value
        /// counts in float columns
        /// </summary>
        /// <param name="columnIndex">Zero based Index of column to look for matches in</param>
        /// <param name="target">columns with this value will be counted</param>
        /// <returns>number of rows with target value in the specified column</returns>
        public long CountFloat(long columnIndex, float target)
        {            
            ValidateColumnIndex(columnIndex);
            return CountFloatNoCheck(columnIndex, target);
        }

        /// <summary>
        /// returns the number of rows with the specified target value, 
        /// counts in string columns       
        /// </summary>
        /// <param name="columnIndex">Zero based Index of column to look for matches in</param>
        /// <param name="target">columns with this value will be counted</param>
        /// <returns>number of rows with target value in the specified column</returns>
        public long CountString(long columnIndex, string target)
        {            
            ValidateColumnIndexAndTypeString(columnIndex);
            return CountStringNoCheck(columnIndex, target);
        }

        /// <summary>
        /// returns the number of rows with the specified target value, 
        /// counts in double columns       
        /// </summary>
        /// <param name="columnIndex">Zero based Index of column to look for matches in</param>
        /// <param name="target">columns with this value will be counted</param>
        /// <returns>number of rows with target value in the specified column</returns>
        public long CountDouble(long columnIndex, Double target)
        {            
            ValidateColumnIndexAndTypeDouble(columnIndex);
            return CountDoubleNoCheck(columnIndex, target);
        }

        /// <summary>
        /// returns the sum of all the values in the specified column.        
        /// </summary>
        /// <param name="columnIndex">Name of column to sum up</param>
        /// <returns>Sum of values of specified column. If the sum exceeds the maximum value in double, result is unspecified</returns>
        public long SumLong(long columnIndex)
        {
            ValidateColumnIndexAndTypeInt(columnIndex);
            return SumLongNoCheck(columnIndex);
        }

        /// <summary>
        /// returns the sum of all the values in the specified column.        
        /// </summary>
        /// <param name="columnIndex">Name of column to sum up</param>
        /// <returns>Sum of values of specified column. </returns>
        public double SumFloat(long columnIndex)
        {            
            ValidateColumnIndexAndTypeFloat(columnIndex);
            return SumFloatNoCheck(columnIndex);
        }


        /// <summary>
        /// returns the sum of all the values in the specified column.        
        /// </summary>
        /// <param name="columnIndex">Name of column to sum up</param>
        /// <returns>Sum of values of specified column. if the sum goes higher than what a double can represent, result is unspecified</returns>
        public double SumDouble(long columnIndex)
        {            
            ValidateColumnIndexAndTypeDouble(columnIndex);
            return SumDoubleNoCheck(columnIndex);
        }

        /// <summary>
        /// Returns the lowest value of the rows in the specified long column
        /// </summary>
        /// <param name="columnIndex">Zero based Index of column to process</param>
        /// <returns>Lowest value of all values in the specified column</returns>
        public long MinimumLong(long columnIndex)
        {            
            ValidateColumnIndexAndTypeInt(columnIndex);
            return MinimumLongNoCheck(columnIndex);
        }

        /// <summary>
        /// Returns the lowest value of the rows in the specified float column
        /// </summary>
        /// <param name="columnIndex">Zero based Index of column to process</param>
        /// <returns>Lowest value of all values in the specified column</returns>
        public float MinimumFloat(long columnIndex)
        {
            ValidateColumnIndexAndTypeFloat(columnIndex);
            return MinimumFloatNoCheck(columnIndex);
        }

        /// <summary>
        /// Returns the lowest value of the rows in the specified double column
        /// </summary>
        /// <param name="columnIndex">Zero based Index of column to process</param>
        /// <returns>Lowest value of all values in the specified column</returns>
        public double MinimumDouble(long columnIndex)
        {            
            ValidateColumnIndex(columnIndex);
            return MinimumDoubleNoCheck(columnIndex);
        }

        /// <summary>
        /// Returns the lowest value (earliest date) of the rows in the specified DateTime column
        /// </summary>
        /// <param name="columnIndex">Zero based Index of column to process</param>
        /// <returns>Lowest value of all values in the specified column</returns>
        public DateTime MinimumDateTime(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            return MinimumDateTimeNoCheck(columnIndex);
        }


        /// <summary>
        /// Returns the highest value of the rows in the specified long column
        /// </summary>
        /// <param name="columnIndex">Zero based Index of column to process</param>
        /// <returns>Highest value of all values in the specified column</returns>

        public long MaximumLong(long columnIndex)
        {
            ValidateColumnIndexAndTypeInt(columnIndex);
            return MaximumLongNoCheck(columnIndex);
        }

        /// <summary>
        /// Returns the highest value of the rows in the specified float column
        /// </summary>
        /// <param name="columnIndex">Zero based Index of column to process</param>
        /// <returns>Highest value of all values in the specified column</returns>
        public float MaximumFloat(long columnIndex)
        {            
            ValidateColumnIndexAndTypeFloat(columnIndex);
            return MaximumFloatNoCheck(columnIndex);
        }

        /// <summary>
        /// Returns the highest value of the rows in the specified double column
        /// </summary>
        /// <param name="columnIndex">Zero based Index of column to process</param>
        /// <returns>Highest value of all values in the specified column</returns>
        public double MaximumDouble(long columnIndex)
        {
            ValidateColumnIndexAndTypeDouble(columnIndex);
            return MaximumDoubleNoCheck(columnIndex);
        }


        /// <summary>
        /// Returns the highest value (latest date) of the rows in the specified DateTime column
        /// </summary>
        /// <param name="columnIndex">Zero based Index of column to process</param>
        /// <returns>Highest value of all values in the specified column</returns>
        public DateTime MaximumDateTime(long columnIndex)
        {
            ValidateColumnIndexAndTypeDate(columnIndex);            
            return MaximumDateTimeNoCheck(columnIndex);
        }


        /// <summary>
        /// returns the average of all the values in the specified column.
        /// The column must be Datatype.Long
        /// Return value with an empty table is unspecified
        /// </summary>
        /// <param name="columnIndex">Index of the column to average</param>
        /// <returns>Average of values of specified column. </returns>
        public double AverageLong(long columnIndex)
        {
            ValidateColumnIndexAndTypeInt(columnIndex);
            ValidateIsNotEmpty();            
            return AverageLongNoCheck(columnIndex);
        }

        /// <summary>
        /// returns the average of all the values in the specified column.
        /// The column must be Datatype.float
        /// Return value with an empty table is unspecified
        /// </summary>
        /// <param name="columnIndex">Index of the column to average</param>
        /// <returns>Average of values of specified column. </returns>
        public double AverageFloat(long columnIndex)
        {
            ValidateColumnIndexAndTypeFloat(columnIndex);
            ValidateIsNotEmpty();            
            return AverageFloatNoCheck(columnIndex);
        }

        /// <summary>
        /// returns the average of all the values in the specified column.
        /// The column must be Datatype.Double
        /// calling on an empty table throws an error
        /// </summary>
        /// <param name="columnIndex">Index of the column to average</param>
        /// <returns>Average of values of specified column. </returns>

        public double AverageDouble(long columnIndex)
        {
            ValidateColumnIndexAndTypeDouble(columnIndex);
            ValidateIsNotEmpty();            
            return AverageDoubleNoCheck(columnIndex);
        }



        //public aggregate functions - field identified by its string name

        /// <summary>
        /// returns the number of rows with the specified target value
        /// Return value with an empty table is unspecified
        /// </summary>
        /// <param name="columnName">Name of column to look for matches in</param>
        /// <param name="target">columns with this value will be counted</param>
        /// <returns>number of rows with target value in the specified column</returns>
    
        public long CountLong(string columnName, long target)
        {
            
            var columnIndex = GetColumnIndex(columnName);
            ValidateTypeInt(columnIndex);
            return CountLongNoCheck(columnIndex, target);
        }


        /// <summary>
        /// returns the number of rows with the specified target value
        /// </summary>
        /// <param name="columnName">Name of column to look for matches in</param>
        /// <param name="target">columns with this value will be counted</param>
        /// <returns>number of rows with target value in the specified column</returns>
   
        public long CountFloat(string columnName, float target)
        {
            var columnIndex = GetColumnIndex(columnName);
            ValidateTypeFloat(columnIndex);
            return CountFloatNoCheck(columnIndex, target);
        }

        /// <summary>
        /// returns the number of rows with the specified target value
        /// </summary>
        /// <param name="columnName">Name of column to look for matches in</param>
        /// <param name="target">columns with this value will be counted</param>
        /// <returns>number of rows with target value in the specified column</returns>
  
        public long CountString(string columnName, string target)
        {
            var columnIndex = GetColumnIndex(columnName);
            ValidateTypeString(columnIndex);
            return CountStringNoCheck(columnIndex, target);
        }

        /// <summary>
        /// returns the number of records with the specified target value
        /// </summary>
        /// <param name="columnName">Name of column to look for matches in</param>
        /// <param name="target">columns with this value will be counted</param>
        /// <returns>number of rows with target value in the specified column</returns>
  
        public long CountDouble(string columnName, Double target)
        {            
            var columnIndex = GetColumnIndex(columnName);
            ValidateTypeDouble(columnIndex);
            return CountDoubleNoCheck(columnIndex, target);
        }

        /// <summary>
        /// returns the sum of all the values in the specified column
        /// </summary>
        /// <param name="columnName">Name of column to sum up</param>
        /// <returns>Sum of values of specified column. If the sum exceeds the maximum value in long, result is unspecified</returns>
  
        public long SumLong(string columnName)
        {
            var columnIndex = GetColumnIndex(columnName);
            ValidateTypeInt(columnIndex);
            return SumLongNoCheck(columnIndex);
        }

        /// <summary>
        /// returns the sum of all the values in the specified column
        /// </summary>
        /// <param name="columnName">Name of column to sum up</param>
        /// <returns>Sum of values of specified column. </returns>
  
        public double SumFloat(string columnName)
        {
            var columnIndex = GetColumnIndex(columnName);
            ValidateTypeFloat(columnIndex);
            return SumFloatNoCheck(columnIndex);
        }

        /// <summary>
        /// returns the sum of all the values in the specified column
        /// </summary>
        /// <param name="columnName">Name of column to sum up</param>
        /// <returns>Sum of values of specified column. If the sum exceeds the maximum value in double, result is unspecified</returns>

        public double SumDouble(string columnName)
        {            
            var columnIndex = GetColumnIndex(columnName);
            ValidateTypeDouble(columnIndex);
            return SumDoubleNoCheck(columnIndex);
        }

        /// <summary>
        /// Returns the lowest value of the rows in the specified long column
        /// </summary>
        /// <param name="columnName">Name of column to process</param>
        /// <returns>Lowest value of all values in the specified column</returns>

        public long MinimumLong(string columnName)
        {            
            var columnIndex = GetColumnIndex(columnName);
            ValidateTypeInt(columnIndex);
            return MinimumLongNoCheck(columnIndex);
        }

        /// <summary>
        /// Returns the lowest value of the rows in the specified float column
        /// </summary>
        /// <param name="columnName">Name of column to process</param>
        /// <returns>lowest value of all values in the specified column</returns>

        public float MinimumFloat(string columnName)
        {
            var columnIndex = GetColumnIndex(columnName);
            ValidateTypeFloat(columnIndex);
            return MinimumFloatNoCheck(columnIndex);
        }

        /// <summary>
        /// Returns the lowest value of the rows in the specified double column
        /// </summary>
        /// <param name="columnName">Name of column to process</param>
        /// <returns>Lowest value of all values in the specified column</returns>

        public double MinimumDouble(string columnName)
        {            
            var columnIndex = GetColumnIndex(columnName);
            ValidateTypeDouble(columnIndex);
            return MinimumDoubleNoCheck(columnIndex);
        }


        /// <summary>
        /// Returns the lowest value (earliest date) of the rows in the specified DateTime column
        /// </summary>
        /// <param name="columnName">Name of column to process</param>
        /// <returns>Lowest value of all values in the specified column</returns>

        public DateTime MinimumDateTime(string columnName)
        {
            var columnIndex = GetColumnIndex(columnName);
            ValidateTypeDate(columnIndex);
            return MinimumDateTimeNoCheck(columnIndex);
        }



        /// <summary>
        /// Returns the highest value of the rows in the specified long column
        /// </summary>
        /// <param name="columnName">Name of column to process</param>
        /// <returns>Highest value of all values in the specified column</returns>
        public long MaximumLong(string columnName)
        {            
            var columnIndex = GetColumnIndex(columnName);
            ValidateTypeInt(columnIndex);
            return MaximumLongNoCheck(columnIndex);
        }

        /// <summary>
        /// Returns the highest value of the rows in the specified double column
        /// </summary>
        /// <param name="columnName">Name of column to process</param>
        /// <returns>Highest value of all values in the specified column</returns>

        public float MaximumFloat(string columnName)
        {            
            var columnIndex = GetColumnIndex(columnName);
            ValidateTypeFloat(columnIndex);
            return MaximumFloatNoCheck(columnIndex);
        }

        /// <summary>
        /// Returns the highest value of the rows in the specified double column
        /// </summary>
        /// <param name="columnName">Name of column to process</param>
        /// <returns>Highest value of all values in the specified column</returns>

        public double MaximumDouble(string columnName)
        {            
            var columnIndex = GetColumnIndex(columnName);
            ValidateTypeDouble(columnIndex);
            return MaximumDoubleNoCheck(columnIndex);
        }

        /// <summary>
        /// Returns the highest value of the rows in the specified double column
        /// </summary>
        /// <param name="columnName">Name of column to process</param>
        /// <returns>Highest value of all values in the specified column</returns>

        public DateTime MaximumDateTime(string columnName)
        {
            var columnIndex = GetColumnIndex(columnName);
            ValidateTypeDate(columnIndex);
            return MaximumDateTimeNoCheck(columnIndex);
        }


        /// <summary>
        /// Returns the average value of the rows in the specified long column
        /// </summary>
        /// <param name="columnName">Name of column to process</param>
        /// <returns>Double containing the Average of all values in the specified column</returns>

        public double AverageLong(string columnName)
        {
            var columnIndex = GetColumnIndex(columnName);
            ValidateIsNotEmpty();
            ValidateTypeInt(columnIndex);
            return AverageLongNoCheck(columnIndex);
        }

        /// <summary>
        /// Returns the average value of the rows in the specified float column
        /// </summary>
        /// <param name="columnName">Name of column to process</param>
        /// <returns>Average of all values in the specified column</returns>
        public double AverageFloat(string columnName)
        {            
            var columnIndex = GetColumnIndex(columnName);
            ValidateIsNotEmpty();
            ValidateTypeFloat(columnIndex);//this could be put in AverageFloatCheckType and be called from string and long indexed methods, then checktype could call on into no check, saving the validate calls at this level
            return AverageFloatNoCheck(columnIndex);
        }

        /// <summary>
        /// Returns the average value of the rows in the specified double column
        /// Throws an exception if there are no rows
        /// </summary>
        /// <param name="columnName">Name of column to process</param>
        /// <returns>Average of all values in the specified column</returns>
        public double AverageDouble(string columnName)
        {
            ValidateIsValid();
            ValidateIsNotEmpty();
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeDouble(columnIndex);
            return AverageDoubleNoCheck(columnIndex);
        }
















        /// <summary>
        /// Search a given column for a int. Returns the zero based RowIndex of the first row with an int matching the value specified
        /// </summary>
        /// <param name="columnName">Name of column to search</param>
        /// <param name="value">long value to search for</param>
        /// <returns>Row Index of search match, or -1 for no rows found</returns>

        public long FindFirstInt(String columnName, long value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeInt(columnIndex);
            return FindFirstIntNoCheck(columnIndex, value);
        }

        /// <summary>
        /// Search a given column for a int. Returns the zero based RowIndex of the first row with an int matching the value specified
        /// </summary>
        /// <param name="columnIndex">zero based Index of column to search</param>
        /// <param name="value">long value to search for</param>
        /// <returns>Zero based Row Index of search match, or -1 for no rows found</returns>

        public long FindFirstInt(long columnIndex, long value)
        {
            ValidateColumnIndexAndTypeInt(columnIndex);
            return FindFirstIntNoCheck(columnIndex, value);
        }

        /// <summary>
        /// Search a given column for a string. Returns the zero based RowIndex of the first row with a string matching the value specified
        /// </summary>
        /// <param name="columnName">Name of column to search</param>
        /// <param name="value">long value to search for</param>
        /// <returns>Row Index of search match, or -1 for no rows found</returns>

        public long FindFirstString(String columnName, String value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeString(columnIndex);
            return FindFirstStringNoCheck(columnIndex, value);
        }

        /// <summary>
        /// return index of first row wth a matching string value in the specified column.
        /// Tightdb stores strings in UTC-8 - the string being searched for will be converted to UTC-8
        /// before the sarch is done
        /// </summary>
        /// <param name="columnIndex">zero based index of column to searh</param>
        /// <param name="value">string to search for</param>
        /// <returns>Zero based Row Index of the first row with a matching value </returns>


        public long FindFirstString(long columnIndex, String value)
        {
            ValidateColumnIndexAndTypeString(columnIndex);
            return FindFirstStringNoCheck(columnIndex, value);
        }


        /// <summary>
        /// Search a given column for a specific binary entry / value.
        /// Returns the zero based RowIndex of the first row with a binary matching the value specified
        /// </summary>
        /// <param name="columnName">Name of column to search</param>
        /// <param name="value">long value to search for</param>
        /// <returns>Row Index of search match, or -1 for no rows found</returns>

        public long FindFirstBinary(String columnName, byte[] value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeBinary(columnIndex);
            return FindFirstBinaryNoCheck(columnIndex, value);
        }

        /// <summary>
        /// return index of first row wth a matching binary value in the specified column.
        /// the binary data must match 100%
        /// </summary>
        /// <param name="columnIndex">zero based index of column to searh</param>
        /// <param name="value">bytes to search for</param>
        /// <returns>Zero based Row Index of the first row with a matching value </returns>


        public long FindFirstBinary(long columnIndex, byte[] value)
        {
            ValidateColumnIndexAndTypeBinary(columnIndex);
            return FindFirstBinaryNoCheck(columnIndex, value);
        }

        /// <summary>
        /// Search a given column for a double. Returns the zero based RowIndex of the first row with a double matching the value specified exactly
        /// </summary>
        /// <param name="columnName">Name of column to search</param>
        /// <param name="value">double value to search for</param>
        /// <returns>Row Index of search match, or -1 for no rows found</returns>


        public long FindFirstDouble(String columnName, double value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeDouble(columnIndex);
            return FindFirstDoubleNoCheck(columnIndex, value);
        }


        /// <summary>
        /// return index of first row wth a matching double value in the specified column.
        /// the double must match 100%
        /// </summary>
        /// <param name="columnIndex">zero based index of column to searh</param>
        /// <param name="value">value to search for</param>
        /// <returns>Zero based Row Index of the first row with a matching value </returns>


        public long FindFirstDouble(long columnIndex, double value)
        {
            ValidateColumnIndexAndTypeDouble(columnIndex);
            return FindFirstDoubleNoCheck(columnIndex, value);
        }

        /// <summary>
        /// Search a given column for a float. Returns the zero based RowIndex of the first row with a float matching the value specified exactly
        /// </summary>
        /// <param name="columnName">Name of column to search</param>
        /// <param name="value">float value to search for (exact match)</param>
        /// <returns>Row Index of search match, or -1 for no rows found</returns>
        public long FindFirstFloat(String columnName, float value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeFloat(columnIndex);
            return FindFirstFloatNoCheck(columnIndex, value);
        }


        /// <summary>
        /// return index of first row wth a matching float value in the specified column.
        /// the float must match 100%
        /// </summary>
        /// <param name="columnIndex">zero based index of column to searh</param>
        /// <param name="value">value to search for</param>
        /// <returns>Zero based Row Index of the first row with a matching value </returns>

        public long FindFirstFloat(long columnIndex, float value)
        {
            ValidateColumnIndexAndTypeFloat(columnIndex);
            return FindFirstFloatNoCheck(columnIndex, value);
        }

        /// <summary>
        /// Search a given column for a time_t date. Returns the zero based RowIndex of the first row with a date matching the value specified
        /// note that Dates are currently treated in a special way in TightDb, consult the documentation. Tightdb stores time_t UTC values, and
        /// C# uses DateTime. The binding will convert to and from, but with rounding as time_t has a 1 second precicion, and starts in 1970
        /// </summary>
        /// <param name="columnName">Name of column to search</param>
        /// <param name="value">DateTime value to search for (will be converted to time_t , thus rounded to nearest second)</param>
        /// <returns>Row Index of search match, or -1 for no rows found</returns>

        public long FindFirstDateTime(String columnName, DateTime value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeDate(columnIndex);
            return FindFirstDateNoCheck(columnIndex, value);
        }

        /// <summary>
        /// Search a given column for a time_t date. Returns the zero based RowIndex of the first row with a date matching the value specified
        /// note that Dates are currently treated in a special way in TightDb, consult the documentation. Tightdb stores time_t UTC values, and
        /// C# uses DateTime. The binding will convert to and from, but with rounding as time_t has a 1 second precicion, and starts in 1970
        /// </summary>
        /// <param name="columnIndex">Zero based Column Index of column to search</param>
        /// <param name="value">long value to search for</param>
        /// <returns>Row Index of search match, or -1 for no rows found</returns>
        public long FindFirstDateTime(long columnIndex, DateTime value)
        {
            ValidateColumnIndexAndTypeDate(columnIndex);
            return FindFirstDateNoCheck(columnIndex, value);
        }

        /// <summary>
        /// Search a given column for a bool. Returns the zero based RowIndex of the first row with a bool matching the value specified
        /// </summary>
        /// <param name="columnName">Name of column to search</param>
        /// <param name="value">long value to search for</param>
        /// <returns>Row Index of search match, or -1 for no rows found</returns>
        public long FindFirstBool(String columnName, bool value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeBool(columnIndex);
            return FindFirstBoolNoCheck(columnIndex, value);
        }


        /// <summary>
        /// Search a given column for a bool. Returns the zero based RowIndex of the first row with a bool matching the value specified
        /// </summary>
        /// <param name="columnIndex">Zero based Column Index of column to search</param>
        /// <param name="value">long value to search for</param>
        /// <returns>Row Index of search match, or -1 for no rows found</returns>
        public long FindFirstBool(long columnIndex, bool value)
        {
            ValidateColumnIndexAndTypeBool(columnIndex);
            return FindFirstBoolNoCheck(columnIndex, value);
        }



        /// <summary>
        /// Tableview with all rows with specified int value.
        /// column must be int column.
        /// </summary>
        /// <param name="columnName">Name of column to searh</param>
        /// <param name="value">value to search for</param>
        /// <returns>TableView with all rows containing the search value </returns>

        public TableView FindAllInt(String columnName, long value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeInt(columnIndex);
            return FindAllIntNoCheck(columnIndex, value);
        }

        /// <summary>
        /// Tableview with all rows with specified int value.
        /// column must be int column
        /// </summary>
        /// <param name="columnIndex">zero based index of column to searh</param>
        /// <param name="value">value to search for</param>
        /// <returns>TableView with all rows containing the search value </returns>
        public TableView FindAllInt(long columnIndex, long value)
        {
            ValidateColumnIndexAndTypeInt(columnIndex);
            return FindAllIntNoCheck(columnIndex, value);
        }

        /// <summary>
        /// Tableview with all rows with specified bool value.
        /// column must be DataType.Bool column
        /// </summary>
        /// <param name="columnName">Name of column to searh</param>
        /// <param name="value">value to search for</param>
        /// <returns>TableView with all rows containing the search value </returns>

        public TableView FindAllBool(String columnName, bool value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeBool(columnIndex);
            return FindAllBoolNoCheck(columnIndex, value);
        }


        /// <summary>
        /// Tableview with all rows with specified bool value.
        /// column must be DataType.Bool column
        /// </summary>
        /// <param name="columnIndex">zero based index of column to searh</param>
        /// <param name="value">value to search for</param>
        /// <returns>TableView with all rows containing the search value </returns>
        public TableView FindAllBool(long columnIndex, bool value)
        {
            ValidateColumnIndexAndTypeBool(columnIndex);
            return FindAllBoolNoCheck(columnIndex, value);
        }

        


        /// <summary>
        /// Tableview with all rows with specified DateTime value.
        /// column must be DataType.DateTime column
        /// </summary>
        /// <param name="columnName">Name of column to searh</param>
        /// <param name="value">value to search for</param>
        /// <returns>TableView with all rows containing the search value </returns>

        public TableView FindAllDateTime(String columnName, DateTime value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeDate(columnIndex);
            return FindAllDateNoCheck(columnIndex, value);
        }

        /// <summary>
        /// Tableview with all rows with specified DateTime value.
        /// column must be DataType.DateTime column
        /// </summary>
        /// <param name="columnIndex">zero based index of column to searh</param>
        /// <param name="value">value to search for</param>
        /// <returns>TableView with all rows containing the search value </returns>
        public TableView FindAllDateTime(long columnIndex, DateTime value)
        {
            ValidateColumnIndexAndTypeDate(columnIndex);
            return FindAllDateNoCheck(columnIndex, value);
        }



        /// <summary>
        /// Tableview with all rows with specified float value.
        /// column must be DataType.Float column
        /// </summary>
        /// <param name="columnName">Name of column to searh</param>
        /// <param name="value">value to search for</param>
        /// <returns>TableView with all rows containing the search value </returns>

        public TableView FindAllFloat(String columnName, float value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeFloat(columnIndex);
            return FindAllFloatNoCheck(columnIndex, value);
        }

        /// <summary>
        /// Tableview with all rows with specified float value.
        /// column must be DataType.Float column
        /// </summary>
        /// <param name="columnIndex">zero based index of column to searh</param>
        /// <param name="value">value to search for</param>
        /// <returns>TableView with all rows containing the search value </returns>
        public TableView FindAllFloat(long columnIndex, float value)
        {
            ValidateColumnIndexAndTypeFloat(columnIndex);
            return FindAllFloatNoCheck(columnIndex, value);
        }


        /// <summary>
        /// Tableview with all rows with specified double value.
        /// column must be DataType.Double column
        /// </summary>
        /// <param name="columnName">Name of column to searh</param>
        /// <param name="value">value to search for</param>
        /// <returns>TableView with all rows containing the search value </returns>

        public TableView FindAllDouble(String columnName, double value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeDouble(columnIndex);
            return FindAllDoubleNoCheck(columnIndex, value);
        }

        /// <summary>
        /// Tableview with all rows with specified float value.
        /// column must be DataType.Float column
        /// </summary>
        /// <param name="columnIndex">zero based index of column to searh</param>
        /// <param name="value">value to search for</param>
        /// <returns>TableView with all rows containing the search value </returns>
        public TableView FindAllDouble(long columnIndex, double value)
        {
            ValidateColumnIndexAndTypeDouble(columnIndex);
            return FindAllDoubleNoCheck(columnIndex, value);
        }



        /// <summary>
        /// Find all rows where a column contains a given string
        /// Can only be called on string columns
        /// </summary>
        /// <param name="columnName">Name of coumn to search</param>
        /// <param name="value">String to search for</param>
        /// <returns>TableView with all rows that match the string</returns>
        public TableView FindAllString(String columnName, String value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeString(columnIndex);
            return FindAllStringNoCheck(columnIndex, value);
        }

        /// <summary>
        /// Find all rows where a column contains a given string
        /// Can only be called on string columns
        /// </summary>
        /// <param name="columnIndex">Index of coumn to search</param>
        /// <param name="value">String to search for</param>
        /// <returns>TableView with all rows that match the string</returns>
        public TableView FindAllString(long columnIndex, String value)
        {
            ValidateColumnIndexAndTypeString(columnIndex);
            return FindAllStringNoCheck(columnIndex, value);
        }

        /// <summary>
        /// Find all rows where a column contains a given byte array
        /// Can only be called on DataType.Binary columns
        /// </summary>
        /// <param name="columnName">Name of coumn to search</param>
        /// <param name="value">String to search for</param>
        /// <returns>TableView with all rows that match the string</returns>
        public TableView FindAllBinary(String columnName, Byte [] value)
        {            
            var columnIndex = GetColumnIndex(columnName);
            ValidateTypeBinary(columnIndex);
            return FindAllBinaryNoCheck(columnIndex, value);
        }

        /// <summary>
        /// Find all rows where a column contains a given array of bytes
        /// Can only be called on DataType.Binary columns
        /// </summary>
        /// <param name="columnIndex">Index of coumn to search</param>
        /// <param name="value">String to search for</param>
        /// <returns>TableView with all rows that match the string</returns>
        public TableView FindAllBinary(long columnIndex, Byte[] value)
        {
            ValidateColumnIndexAndTypeBinary(columnIndex);
            return FindAllBinaryNoCheck(columnIndex, value);
        }






        /// <summary>
        /// return an object containing the boxed value of a specified field
        /// The object could be Table of the column is a subtable column, or the
        /// field is a mixed field with a table in it
        /// </summary>
        /// <param name="columnIndex">zero based index of column</param>
        /// <param name="rowIndex">zero based index of row</param>
        /// <returns>a boxed object with the value of the specified field</returns>
        public object GetValue(long columnIndex, long rowIndex)
        {
            ValidateColumnAndRowIndex(columnIndex,rowIndex);
            return GetValueNoCheck(columnIndex, rowIndex);
        }

        /// <summary>
        /// Set the value of the field specified by columnIndex and rowIndex
        /// The value must match the field type
        /// Subtable fileds can take null, a Table or a collection of row collections of field data
        /// null will create a new empty subtable.
        /// Table will create a copy of the specified table
        /// A collection of row data will result in a table that matches the data as well as possible
        /// </summary>
        /// <param name="columnIndex">Zero based column of field</param>
        /// <param name="rowIndex">Zero based row of field</param>
        /// <param name="value">value to set</param>
        public void SetValue(long columnIndex, long rowIndex,object value)
        {
            ValidateColumnAndRowIndex(columnIndex, rowIndex);
            ValidateReadWrite();
            SetValueNoCheck(columnIndex, rowIndex, value);
        }



        private object GetMixedNoCheck(long columnIndex, long rowIndex)
        {
            switch (GetMixedTypeNoCheck(columnIndex, rowIndex))
            {
                case DataType.Int:
                    return GetMixedLongNoCheck(columnIndex, rowIndex);
                case DataType.Bool:
                    return GetMixedBoolNoCheck(columnIndex, rowIndex);
                case DataType.String:
                    return GetMixedStringNoCheck(columnIndex, rowIndex);
                case DataType.Binary:
                    return GetMixedBinaryNoCheck(columnIndex, rowIndex);
                case DataType.Table:
                    return GetMixedSubTableNoCheck(columnIndex, rowIndex);
                case DataType.Date:
                    return GetMixedDateTimeNoCheck(columnIndex, rowIndex);
                case DataType.Float:
                    return GetMixedFloatNoCheck(columnIndex, rowIndex);
                case DataType.Double:
                    return GetMixedDoubleNoCheck(columnIndex, rowIndex);
                default:
                    return string.Format(CultureInfo.InvariantCulture,
                        "mixed with type {0} not supported yet in table.GetCell",
                        GetMixedTypeNoCheck(columnIndex, rowIndex));
            }
        }

        internal object GetValueNoCheck(long columnIndex, long rowIndex)
        {
            switch (ColumnType(columnIndex))
            {
                case DataType.Int:
                    return GetLongNoCheck(columnIndex, rowIndex);
                case DataType.Bool:
                    return GetBoolNoCheck(columnIndex, rowIndex);
                case DataType.String:
                    return GetStringNoCheck(columnIndex, rowIndex);
                case DataType.Binary:
                    return GetBinaryNoCheck(columnIndex, rowIndex);
                case DataType.Table:
                    return GetSubTableNoCheck(columnIndex, rowIndex);
                case DataType.Date:
                    return GetDateTimeNoCheck(columnIndex, rowIndex);
                case DataType.Float:
                    return GetFloatNoCheck(columnIndex, rowIndex);
                case DataType.Double:
                    return GetDoubleNoCheck(columnIndex, rowIndex);

                case DataType.Mixed:
                    return GetMixedNoCheck(columnIndex, rowIndex);
                default:
                    return String.Format(CultureInfo.InvariantCulture,
                        "Getting type {0} from Table.GetCell not implemented yet",
                        ColumnType(columnIndex)); //so null means the datatype is not fully supported yet   
            }
        }
        /*
        public abstract IEnumerator<Row> GetEnumerator();//implemented in tableview and table. they currently return TableRow and Row
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }*/
    }
}

        


    

