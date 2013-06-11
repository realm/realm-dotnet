using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace TightDbCSharp
{
    
    //abstract methods are those that are implemented differently in table and tableview
    //methods implemented here , work with both types
    public abstract class TableOrView : Handled, IEnumerable<Row>
    {
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
        //mixed is handled by type named GetMixedxxxx methods below
        internal abstract DateTime GetDateTimeNoCheck(long columnIndex, long rowIndex);
        internal abstract float GetFloatNoCheck(long columnIndex, long rowIndex);
        internal abstract Double GetDoubleNoCheck(long columnIndex, long rowIndex);

        internal abstract DataType GetMixedTypeNoCheck(long columnIndex, long rowIndex);

        //getters for table and tableview mixed columns
        internal abstract long GetMixedLongNoCheck(long columnIndex, long rowIndex);
        internal abstract bool GetMixedBoolNoCheck(long columnIndex, long rowIndex);
        internal abstract String GetMixedStringNoCheck(long columnIndex, long rowIndex);
        internal abstract byte[] GetMixedBinaryNoCheck(long columnIndex, long rowIndex);
        internal abstract Table GetMixedSubTableNoCheck(long columnIndex, long rowIndex);
        //mixed mixed is not allowed
        internal abstract DateTime GetMixedDateTimeNoCheck(long columnIndex, long rowIndex);
        internal abstract float GetMixedFloatNoCheck(long columnIndex, long rowIndex);
        internal abstract Double GetMixedDoubleNoCheck(long columnIndex, long rowIndex);

        internal abstract long GetColumnIndexNoCheck(string name);//-1 if CI does not exists



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
        internal abstract float SumFloatNoCheck(long columnIndex);
        internal abstract double SumDoubleNoCheck(long columnIndex);

        internal abstract long MinimumLongNoCheck(long columnIndex);
        internal abstract float MinimumFloatNoCheck(long columnIndex);
        internal abstract double MinimumDoubleNoCheck(long columnIndex);

        internal abstract long MaximumLongNoCheck(long columnIndex);
        internal abstract float MaximumFloatNoCheck(long columnIndex);
        internal abstract double MaximumDoubleNoCheck(long columnIndex);

        //average only supported in table for the time being. In tablewiev average will always return 0
        internal abstract double AverageLongNoCheck(long columnIndex);
        internal abstract double AverageFloatNoCheck(long columnIndex);
        internal abstract double AverageDoubleNoCheck(long columnIndex);









        internal abstract long FindFirstIntNoCheck(long columnIndex, long value);
        internal abstract long FindFirstStringNoCheck(long columnIndex, string value);
        internal abstract long FindFirstBinaryNoCheck(long columnIndex, byte[] value);
        internal abstract long FindFirstDoubleNoCheck(long columnIndex, double value);
        internal abstract long FindFirstFloatNoCheck(long columnIndex, float value);
        internal abstract long FindFirstDateNoCheck(long columnIndex, DateTime value);
        internal abstract long FindFirstBoolNoCheck(long columnIndex, bool value);




        internal abstract TableView FindAllIntNoCheck(long columnIndex, long value);
        internal abstract TableView FindAllStringNoCheck(long columnIndex, string value);




        internal abstract void SetLongNoCheck(long columnIndex, long rowIndex, long value);//does not validate parametres or types
        internal abstract void SetBoolNoCheck(long columnIndex, long rowIndex, Boolean value);
        internal abstract void SetStringNoCheck(long columnIndex, long rowIndex, string value);
        internal abstract void SetBinaryNoCheck(long columnIndex, long rowIndex, byte[] value);
        //SetSubTable implemented here as a high-level function, see below
        internal abstract void SetDateTimeNoCheck(long columnIndex, long rowIndex, DateTime value);
        internal abstract void SetDoubleNoCheck(long columnIndex, long rowIndex, double value);
        internal abstract void SetFloatNoCheck(long columnIndex, long rowIndex, float value);

        internal abstract void SetMixedLongNoCheck(long columnIndex, long rowIndex, long value);
        internal abstract void SetMixedBoolNoCheck(long columnIndex, long rowIndex, bool value);
        internal abstract void SetMixedStringNoCheck(long columnIndex, long rowIndex, String value);
        internal abstract void SetMixedBinaryNoCheck(long columnIndex, long rowIndex, Byte[] value);
        internal abstract void SetMixedSubtableNoCheck(long columnIndex, long rowIndex, Table source); //will populate the mixed subtable field with a copy of the contents of source
        internal abstract void SetMixedEmptySubtableNoCheck(long columnIndex, long rowIndex);
        //mixed mixed makes no sense
        internal abstract void SetMixedDateTimeNoCheck(long columnIndex, long rowIndex, DateTime value);
        internal abstract void SetMixedFloatNoCheck(long columnIndex, long rowIndex, float value);
        internal abstract void SetMixedDoubleNoCheck(long columnIndex, long rowIndex, double value);


        public abstract string ToJson();
                
        

        internal abstract void RemoveNoCheck(long rowIndex);//removes the row at rowIndex, all rows after that have their index reduced by 1
        //all existing and any new row and rowcolumn classes will point to the new contents of the indicies.


        //this could also be implemented by calling c++ but the time difference is microscopic
        public Boolean IsEmpty
        {
            get { return Size == 0; }
        }

        public long Size
        {
            get { return GetSize(); }
        }





        //Methods on TableOrView

        public long ColumnCount
        {
            get { return GetColumnCount(); }
        }

        public Spec Spec
        {
            get { return GetSpec(); }
        }



        //the following code enables TableOrView to be enumerated, and makes Row the type You get back from an enummeration
        public IEnumerator<Row> GetEnumerator() { return new Enumerator(this); }
        IEnumerator IEnumerable.GetEnumerator() { return new Enumerator(this); }

        class Enumerator : IEnumerator<Row>//probably overkill, current needs could be met by using yield
        {
            long _currentRow = -1;
            TableOrView _myTable;
            public Enumerator(TableOrView table)
            {
                _myTable = table;
            }
            public Row Current { get { return new Row(_myTable, _currentRow); } }
            object IEnumerator.Current { get { return Current; } }

            public bool MoveNext()
            {
                return ++_currentRow < _myTable.Size;
            }

            public void Reset() { _currentRow = -1; }
            public void Dispose()
            {
                _myTable = null; //remove reference to Table class
            }
        }

        //will return a valid column index or throw
        public long GetColumnIndex(String name)
        {
            long columnIndex = GetColumnIndexNoCheck(name);
            if (columnIndex == -1) { 
                throw new ArgumentOutOfRangeException("name", String.Format(CultureInfo.InvariantCulture,"column name specified ({0}) does not exist in this table.",name)); 
            }
            return columnIndex;
        }

        //returns -1 if the column name does not exist (instead of throwing an exception)
        public long GetColumnIndexNoThrow(String name)
        {
            return GetColumnIndexNoCheck(name);
        }


        //todo:unit test this
        //column and row must point to a field that is of type subtable
        //the values are then put into that subtable in this manner :
        //todo:instead of object array, take an Ienumerrator, as long as it can yield objects that translate to row contents we should be happy

        internal void SetSubTableNoCheck(long columnIndex, long rowIndex, IEnumerable<object> elem)
        {
            Table t = GetSubTableNoCheck(columnIndex, rowIndex);//The type to go into a subtable has to be an array of arrays of objects

            //var x = elem.GetType();
            //if (elem.GetType() != typeof(Array))
            //{
            //    throw new ArgumentOutOfRangeException(String.Format(CultureInfo.InvariantCulture,
//                                                                    "SetRow called with a non-array type {0} for the subtable column {1}",
//                                                                    elem.GetType(),
//                                                                    GetColumnNameNoCheck(columnIndex)));
//            }
            //each element in the enumerable list must be a row, so call addrow with te element. Let Arow decide if it is useful as row data
            foreach (IEnumerable<object> arow in elem)//typecast because we already ensured element is an array,and that element is not null
            {
                t.Add(arow);
            }
        }

        //add unknown typed object to a mixed
        internal void SetMixedNoCheck(long columnIndex, long rowIndex, object element)
        {
            Type elementType = element.GetType();//performance hit as it is not neccessarily used, but used many blaces below

            //todo:add a throw statement if an unsupported type shows up in the parametres. 
            //that is - if we ended up not inserting anything.
            //remmeber to fixup the insert in progress first
            //right now, we silently just don't put anything in the mixed if we cannot figure the type to use

            if (
                elementType == typeof(Byte) || //byte Byte
                elementType == typeof(Int16) || //int16,short
                elementType == typeof(SByte) || //sbyte SByte                           
                elementType == typeof(Int32) || //int,int32
                elementType == typeof(UInt16)  //ushort,uint16
                )
            {
                SetMixedLongNoCheck(columnIndex, rowIndex, (int)element);//ints cannot be unboxed directly to long
            }

            if (
                elementType == typeof(UInt32) || //uint
                elementType == typeof(Int64) || //long,int64
                elementType == typeof(UInt64) //ulong                            
                )
            {
                SetMixedLongNoCheck(columnIndex, rowIndex, (long)element);//ints cannot be unboxed directly to long. But can the larger types?
            }//todo:unit test by throwing uint32,int64 and UInt64 at the cast above

            if (elementType == typeof(Boolean))
            {
                SetMixedBoolNoCheck(columnIndex, rowIndex, (bool)element);
            }

            if (elementType == typeof(string))
            {
                SetMixedStringNoCheck(columnIndex, rowIndex, (String)element);
            }

            if (elementType == typeof(byte[]))//todo:unit test if this type check actually works as intended
            {
                SetMixedBinaryNoCheck(columnIndex, rowIndex, (byte[])element);
            }


            if (element as IEnumerable != null) //check if it is an array of stuff that could be row contents
            {
                if (elementType != typeof(string))//a string cannot represent  a list of rows - it is instead treated as a string field
                {
                    if (elementType != typeof(Byte[]))//a byte array cannot represent a list of rows - it is instead treated as a binary field
                        if (elementType != typeof(Table))
                        {
                            //idea:construct a table from the enummerable element, and guess the types and names of the fields (not high priority)
                            //todo then add the rows in the enummerable to the table
                            //todo then set the mixed to point to a copy of this table
                            //SetMixedSubtableNoCheck(columnIndex,rowIndex,element);
                            throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture,
                                "You can only specify a subtable in a mixed by providing a table object. Encountered a {0}", elementType));
                        }
                        else
                        {
                            SetMixedSubtableNoCheck(columnIndex, rowIndex, element as Table);
                        }
                }
            }
            //mixed in mixed makes no sense
            if (elementType == typeof(DateTime))
            {
                SetMixedDateTimeNoCheck(columnIndex, rowIndex, (DateTime)element);
            }

            if (elementType == typeof(Single))//float, Single
            {
                SetMixedFloatNoCheck(columnIndex, rowIndex, (float)element);
            }

            if (elementType == typeof(Double))
            {
                SetMixedDoubleNoCheck(columnIndex, rowIndex, (Double)element);
            }


        }


        public void SetMixed(long columnIndex, long rowIndex, object value)
        {
            ValidateColumnIndexAndTypeMixed(columnIndex);
            ValidateRowIndex(rowIndex);
            SetMixedNoCheck(columnIndex,rowIndex,value);
        }


        //do not check row index
        
        //todo:let the type be ieumerable, allowing user to set with objects in any kind of collection he has
          internal void SetRowNoCheck(long rowIndex, params object[] rowContents)
        //experimental        internal void SetRowNoCheck(long rowIndex, IEnumerable<object> rowContents)
        {

            //user could send row data as an object array as first parameter, or many parametres that together is the data for a row/
            //handle both cases by making sure rowContents is always an array of field values to be put in
            if (rowContents.Length == 1 && ColumnCount > 1 && rowContents.GetType()== typeof(object[]))
            {
                rowContents = (object[])rowContents[0];
            }

            if (rowContents.Length != ColumnCount)
                throw new ArgumentOutOfRangeException("rowContents", String.Format(CultureInfo.InvariantCulture, "SetRow called with {0} objects, but there are only {1} columns in the table", rowContents.Length, ColumnCount));
            for (long ix = 0; ix < ColumnCount; ix++)
            {
                object element = rowContents[ix];//element is parameter number ix
                //first do a switch on directly compatible types

                switch (ColumnTypeNoCheck(ix))
                {
                    case DataType.Int:
                        if (element is int)
                            SetLongNoCheck(ix, rowIndex, (int)element);//this throws exceptions if called with something too weird
                        break;
                    case DataType.Bool:
                        SetBoolNoCheck(ix, rowIndex, (Boolean)element);
                        break;
                    case DataType.String:
                        SetStringNoCheck(ix, rowIndex, (string)element);
                        break;
                    case DataType.Binary://currently you HAVE to send a byte[] in the array with binary data
                                         //later we might support that You put in other types, that can be changed to binary data
                        var data = element as byte[];
                        if (data == null)
                        {
                             if (element is float)//for instance we might want to support people stuffing a float directly to a binary field for some obscure reason
                             {
                                 
                             }
                        }
                        SetBinaryNoCheck(ix, rowIndex,   data);
                        break;
                    case DataType.Table://todo:test thoroughly with unit test, also with invalid data

                        if (element != null)//if You specify null for a subtable we do nothing null means You intend to fill it in later
                        {
                            SetSubTableNoCheck(ix, rowIndex, (IEnumerable<object>)element);
                        }
                        break;
                    case DataType.Mixed://Try to infer the mixed type to use from the type of the object from the user
                        SetMixedNoCheck(ix, rowIndex, element);
                        break;
                    case DataType.Date:
                        SetDateTimeNoCheck(ix, rowIndex, (DateTime)element);
                        break;
                    case DataType.Float:
                        SetFloatNoCheck(ix, rowIndex, (float)element);
                        break;
                    case DataType.Double:
                        SetDoubleNoCheck(ix, rowIndex, (Double)element);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("rowContents", String.Format(CultureInfo.InvariantCulture, "An element ix:{0} of type {1} in a row sent to AddRow, is not of a supported tightdb type ", ix, element.GetType()));
                }
            }
        }



        public DataType ColumnType(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            return ColumnTypeNoCheck(columnIndex);
        }

        //GetxxDataTypexxCheckType is used when column and row is known to be valid, but the type of the field has not been validated yet. Used when user calles RowColumn.GetxxDataTypexx();
        //used by tablerow where we know row and column index are valid,but not if the user calls getsubtable on a column that does not have subtables
        internal Table GetSubTableCheckType(long columnIndex, long rowIndex)
        {
            ValidateColumnTypeSubTable(columnIndex);
            return GetSubTableNoCheck(columnIndex, rowIndex);
        }

        //GetxxxDataTypexxx is public and both columnIndex, rowIndex and the type of the field will be valiated before a call to c++ is done. (validations themselves also result in calls)
        public Table GetSubTable(long columnIndex, long rowIndex)
        {
            ValidateColumnAndRowIndex(columnIndex, rowIndex);
            ValidateColumnTypeSubTable(columnIndex);
            return GetSubTableNoCheck(columnIndex, rowIndex);
        }

        public Table GetSubTable(string columnName, long rowIndex)
        {
            ValidateRowIndex(rowIndex);
            long columnIndex = GetColumnIndex(columnName);
            return GetSubTableNoRowCheck(columnIndex, rowIndex);
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
            ValidateColumnTypeSubTable(columnIndex);
            return GetSubTableNoCheck(columnIndex, rowIndex);
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




        public void SetMixedEmptySubTable(long columnIndex, long rowIndex)
        {
            ValidateColumnRowTypeMixed(columnIndex, rowIndex);
            SetMixedEmptySubtableNoCheck(columnIndex, rowIndex);
        }


        public long GetMixedLong(long columnIndex, long rowIndex)
        {
            ValidateColumnRowMixedType(columnIndex, rowIndex, DataType.Int);
            return GetMixedLongNoCheck(columnIndex, rowIndex);
        }


        public float GetMixedFloat(long columnIndex, long rowIndex)
        {
            ValidateColumnRowMixedType(columnIndex, rowIndex, DataType.Float);
            return GetMixedFloatNoCheck(columnIndex, rowIndex);
        }

        public double GetMixedDouble(long columnIndex, long rowIndex)
        {
            ValidateColumnRowMixedType(columnIndex, rowIndex, DataType.Double);
            return GetMixedDoubleNoCheck(columnIndex, rowIndex);
        }


        public DateTime GetMixedDateTime(long columnIndex, long rowIndex)
        {
            ValidateColumnRowMixedType(columnIndex, rowIndex, DataType.Date);
            return GetMixedDateTimeNoCheck(columnIndex, rowIndex);
        }

        public DateTime GetMixedDateTime(string columnName, long rowIndex)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateRowIndex(rowIndex);
            ValidateMixedType(columnIndex, rowIndex, DataType.Date);
            return GetMixedDateTimeNoCheck(columnIndex, rowIndex);
        }

        internal DateTime GetMixedDateTimeNoRowCheck(long columnIndex, long rowIndex)
        {
            ValidateColumnMixedType(columnIndex, rowIndex, DataType.Date);
            return GetMixedDateTimeNoCheck(columnIndex, rowIndex);
        }

        internal DateTime GetMixedDateTimeNoRowCheck(string columnName, long rowIndex)
        {
            return GetMixedDateTimeNoRowCheck(GetColumnIndex(columnName), rowIndex);
        }


        public DateTime GetDateTime(long columnIndex, long rowIndex)
        {
            ValidateColumnAndRowIndex(columnIndex, rowIndex);
            ValidateTypeDate(columnIndex);
            return GetDateTimeNoCheck(columnIndex, rowIndex);
        }

        public DateTime GetDateTime(string columnName, long rowIndex)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateRowIndex(rowIndex);
            ValidateTypeDate(columnIndex);
            return GetDateTimeNoCheck(columnIndex, rowIndex);
        }

        //methods that are common for table and tableview:
        //split into its own method to make the ordinary getsubtable very slightly faster bc it does not have to validate if type is a mixed
        public Table GetMixedSubTable(long columnIndex, long rowIndex)
        {
            ValidateColumnRowMixedType(columnIndex, rowIndex, DataType.Table);
            return GetMixedSubTableNoCheck(columnIndex, rowIndex);
        }

        public DataType GetMixedType(long columnIndex, long rowIndex)
        {
            ValidateColumnRowTypeMixed(columnIndex, rowIndex);
            return GetMixedTypeNoCheck(columnIndex, rowIndex);
        }


        public void SetMixedLong(long columnIndex, long rowIndex, long value)
        {
            ValidateColumnRowTypeMixed(columnIndex, rowIndex);
            SetMixedLongNoCheck(columnIndex, rowIndex, value);
        }

        public void SetMixedBool(long columnIndex, long rowIndex, bool value)
        {
            ValidateColumnRowTypeMixed(columnIndex, rowIndex);
            SetMixedBoolNoCheck(columnIndex, rowIndex, value);
        }

        public void SetMixedString(long columnIndex, long rowIndex, string value)
        {
            ValidateColumnRowTypeMixed(columnIndex, rowIndex);
            SetMixedStringNoCheck(columnIndex, rowIndex, value);
        }

        public void SetMixedString(string columnName, long rowIndex, string value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateColumnRowTypeMixed(columnIndex, rowIndex);
            SetMixedStringNoCheck(columnIndex, rowIndex, value);
        }

        public void SetMixedBinary(long columnIndex, long rowIndex, byte[] value)//idea:perhaps we should also support the user passing us a stream?
        {
            ValidateColumnRowTypeMixed(columnIndex, rowIndex);
            SetMixedBinaryNoCheck(columnIndex, rowIndex, value);
        }


        public void SetMixedSubTable(long columnIndex, long rowIndex, Table source)
        {
            ValidateColumnRowTypeMixed(columnIndex, rowIndex);
            SetMixedSubtableNoCheck(columnIndex, rowIndex, source);
        }

        //setmixedmixed makes no sense
        public void SetMixedDateTime(long columnIndex, long rowIndex, DateTime value)
        {
            ValidateColumnRowTypeMixed(columnIndex, rowIndex);
            SetMixedDateTimeNoCheck(columnIndex, rowIndex, value);
        }

        public void SetMixedDateTime(string columnName, long rowIndex, DateTime value)
        {
            SetMixedDateTimeNoColumnCheck(GetColumnIndex(columnName), rowIndex, value);
        }

        public void SetMixedFloat(long columnIndex, long rowIndex, float value)
        {
            ValidateColumnRowTypeMixed(columnIndex, rowIndex);
            SetMixedFloatNoCheck(columnIndex, rowIndex, value);
        }

        public void SetMixedDouble(long columnIndex, long rowIndex, double value)
        {
            ValidateColumnRowTypeMixed(columnIndex, rowIndex);
            SetMixedDoubleNoCheck(columnIndex, rowIndex, value);
        }




        internal void SetMixedDateTimeNoColumnCheck(long columnIndex, long rowIndex, DateTime value)
        {
            ValidateTypeMixed(columnIndex);//only checks that the CI points to a mixed
            ValidateRowIndex(rowIndex);            //only checks that the row is valid
            SetMixedDateTimeNoCheck(columnIndex, rowIndex, value);//this is okay as a mixed will be set to the type you put into it
        }

        //we know column and row indicies are valid, but need to check if the column is in fact a mixed
        internal DataType GetMixedTypeCheckType(long columnIndex, long rowIndex)
        {
            ValidateTypeMixed(columnIndex);
            return GetMixedTypeNoCheck(columnIndex, rowIndex);
        }

        //Used when called directly from tableorview by the user. validates that column and row are legal indexes and that the type is mixed. Used for instance when user wants to store some type to a mixed (then the type of mixed does not matter as it will get overwritten)
        internal void ValidateColumnRowTypeMixed(long columnIndex, long rowIndex)
        {
            ValidateColumnAndRowIndex(columnIndex, rowIndex);
            ValidateTypeMixed(columnIndex);
        }

        internal void ValidateMixedType(long columnIndex, long rowIndex, DataType mixedType)
        {

            if (GetMixedType(columnIndex, rowIndex) != mixedType)
            {
                throw new ArgumentOutOfRangeException("columnIndex", string.Format(CultureInfo.InvariantCulture, "Attempting to read a type({0}) from mixed({1},{2}) mixed is of type {3}", mixedType, columnIndex, rowIndex, GetMixedTypeNoCheck(columnIndex, rowIndex)));
            }
        }

        //used when reading data from a mixed - we've go to check the type of the mixed before attempting to read from it
        internal void ValidateColumnRowMixedType(long columnIndex, long rowIndex, DataType mixedType)
        {
            ValidateColumnRowTypeMixed(columnIndex, rowIndex);//we only progress if the field is mixed
            ValidateMixedType(columnIndex, rowIndex, mixedType);
        }

        //used when reading data from a mixed via a row, so we know the row index is fine and should not be checked
        internal void ValidateColumnMixedType(long columnIndex, long rowIndex, DataType mixedType)
        {
            ValidateTypeMixed(columnIndex);//we only progress if the field is mixed
            ValidateMixedType(columnIndex, rowIndex, mixedType);
        }









        public void ValidateRowIndex(long rowIndex)
        {
            if (rowIndex >= Size || rowIndex < 0)
            {
                throw new ArgumentOutOfRangeException("rowIndex", string.Format(CultureInfo.InvariantCulture, "{0} accessed with an invalid Row Index{1}. Table Size is:{2}", ObjectIdentification(), rowIndex, Size)); //re-calculating when composing error message to avoid creating a variable in a performance sensitive place
            }
        }

        public void ValidateColumnIndex(long columnIndex)
        {
            if (columnIndex >= ColumnCount || columnIndex < 0)
            {
                throw new ArgumentOutOfRangeException("columnIndex", String.Format(CultureInfo.InvariantCulture, "illegal columnIndex:{0} Table Column Count:{1}", columnIndex, ColumnCount));
            }
        }

        //the parameter is the column type that was used on access, and it was not the correct one
        internal string GetColumnTypeErrorString(long columnIndex, DataType columnType)
        {
            return String.Format(CultureInfo.InvariantCulture, "column:{0} invalid data access. Real column DataType:{1} Accessed as {2}", columnIndex, ColumnTypeNoCheck(columnIndex), columnType);
        }


        //only call if columnIndex is already validated or known to be int
        internal void ValidateTypeInt(long columnIndex)
        {
            if (ColumnTypeNoCheck(columnIndex) != DataType.Int)
            {
                throw new TableException(GetColumnTypeErrorString(columnIndex, DataType.Int));
            }
        }

        //only call if columnIndex is already validated 
        internal void ValidateTypeString(long columnIndex)
        {
            if (ColumnTypeNoCheck(columnIndex) != DataType.String)
            {
                throw new TableException(GetColumnTypeErrorString(columnIndex, DataType.String));
            }
        }

        internal void ValidateTypeBinary(long columnIndex)
        {
            if (ColumnTypeNoCheck(columnIndex) != DataType.Binary)
            {
                throw new TableException(GetColumnTypeErrorString(columnIndex, DataType.Binary));
            }
        }

        internal void ValidateTypeDouble(long columnIndex)
        {
            if (ColumnTypeNoCheck(columnIndex) != DataType.Double)
            {
                throw new TableException(GetColumnTypeErrorString(columnIndex, DataType.Double));
            }
        }
        internal void ValidateTypeFloat(long columnIndex)
        {
            if (ColumnTypeNoCheck(columnIndex) != DataType.Float)
            {
                throw new TableException(GetColumnTypeErrorString(columnIndex, DataType.Float));
            }
        }




        //only call if columnIndex is already validated         
        internal void ValidateTypeMixed(long columnIndex)
        {
            if (ColumnTypeNoCheck(columnIndex) != DataType.Mixed)
            {
                throw new ArgumentOutOfRangeException("columnIndex", GetColumnTypeErrorString(columnIndex, DataType.Mixed));
            }
        }

        internal void ValidateTypeSubTable(long columnIndex)
        {
            if (ColumnTypeNoCheck(columnIndex) != DataType.Table)
            {
                throw new ArgumentOutOfRangeException("columnIndex", GetColumnTypeErrorString(columnIndex, DataType.Table));
            }
        }


        //only call if columnIndex is already validated 
        //todo:unit test
        internal void ValidateTypeBool(long columnIndex)
        {
            if (ColumnTypeNoCheck(columnIndex) != DataType.Bool)
            {
                throw new TableException(GetColumnTypeErrorString(columnIndex, DataType.Bool));
            }
        }

        //only call if columnIndex is already validated         
        internal void ValidateTypeDate(long columnIndex)
        {
            if (ColumnTypeNoCheck(columnIndex) != DataType.Date)
            {
                throw new TableException(GetColumnTypeErrorString(columnIndex, DataType.Date));
            }
        }

        //throw if the table contains any rows
        internal void ValidateIsEmpty()
        {
            if (Size != 0)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Unsupported operation called that only work on empty tables . this table have {0} data rows", Size));
            }
        }

        public void RemoveLast()//this could be a c++ call and save the extra call to get size
        {
            long s = Size;
            if (s > 0)
            {
                RemoveNoCheck(s - 1);
            }
        }

        public void Remove(long rowIndex)
        {
            ValidateRowIndex(rowIndex);
            RemoveNoCheck(rowIndex);
            //todo:invalidate any active iterators. how? they could be iterating a query on a view on this view
        }

        internal void ValidateColumnTypeSubTable(long columnIndex)
        {
            if (ColumnTypeNoCheck(columnIndex) != DataType.Table)
            {
                throw new TableException(GetColumnTypeErrorString(columnIndex, DataType.Table));
            }
        }


        public void SetLong(long columnIndex, long rowIndex, long value)
        {
            ValidateColumnAndRowIndex(columnIndex, rowIndex);
            ValidateTypeInt(columnIndex);
            SetLongNoCheck(columnIndex, rowIndex, value);
        }

        public void SetLong(String columnName, long rowIndex, long value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeInt(columnIndex);
            ValidateRowIndex(rowIndex);
            SetLongNoCheck(columnIndex, rowIndex, value);
        }


        public void SetDouble(long columnIndex, long rowIndex, double value)
        {
            ValidateColumnAndRowIndex(columnIndex, rowIndex);
            ValidateTypeDouble(columnIndex);
            SetDoubleNoCheck(columnIndex, rowIndex, value);
        }

        public void SetDouble(String columnName, long rowIndex, double value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeDouble(columnIndex);
            ValidateRowIndex(rowIndex);
            SetDoubleNoCheck(columnIndex, rowIndex, value);
        }

        public void SetFloat(long columnIndex, long rowIndex, float value)
        {
            ValidateColumnAndRowIndex(columnIndex, rowIndex);
            ValidateTypeFloat(columnIndex);
            SetFloatNoCheck(columnIndex, rowIndex, value);
        }

        public void SetFloat(String columnName, long rowIndex, float value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeFloat(columnIndex);
            ValidateRowIndex(rowIndex);
            SetFloatNoCheck(columnIndex, rowIndex, value);
        }


        public void SetLongNoRowCheck(long columnIndex, long rowIndex, long value)
        {
            ValidateColumnIndexAndTypeInt(columnIndex);
            SetLongNoCheck(columnIndex, rowIndex, value);
        }

        public void SetLongNoRowCheck(string columnName, long rowIndex, long value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeInt(columnIndex);
            SetLongNoCheck(columnIndex, rowIndex, value);
        }


        public void SetFloatNoRowCheck(long columnIndex, long rowIndex, float value)
        {
            ValidateColumnIndexAndTypeFloat(columnIndex);
            SetFloatNoCheck(columnIndex, rowIndex, value);
        }

        public void SetFloatNoRowCheck(string columnName, long rowIndex, float value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeFloat(columnIndex);
            SetFloatNoCheck(columnIndex, rowIndex, value);
        }


        //the norowcheck methods are used in row.cs
        public void SetDoubleNoRowCheck(long columnIndex, long rowIndex, double value)
        {
            ValidateColumnIndexAndTypeDouble(columnIndex);
            SetDoubleNoCheck(columnIndex, rowIndex, value);
        }

        public void SetDoubleNoRowCheck(string columnName, long rowIndex, double value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeDouble(columnIndex);
            SetDoubleNoCheck(columnIndex, rowIndex, value);
        }









        //if You call from TableRow or TableColumn, You will save some checking - this is the slowest way
        public long GetLong(long columnIndex, long rowIndex)
        {
            ValidateRowIndex(rowIndex);
            ValidateColumnIndexAndTypeInt(columnIndex);
            return GetLongNoCheck(columnIndex, rowIndex);//could be sped up if we directly call UnsafeNativeMethods
        }

        public long GetLong(String columnName, long rowIndex)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateRowIndex(rowIndex);
            ValidateTypeInt(columnIndex);
            return GetLongNoCheck(columnIndex, rowIndex);
        }


        public string GetString(long columnIndex, long rowIndex)
        {
            ValidateRowIndex(rowIndex);
            ValidateColumnIndexAndTypeString(columnIndex);
            return GetStringNoCheck(columnIndex, rowIndex);
        }

        public string GetString(String columnName, long rowIndex)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateRowIndex(rowIndex);
            ValidateTypeString(columnIndex);
            return GetStringNoCheck(columnIndex, rowIndex);
        }







        public Double GetDouble(long columnIndex, long rowIndex)
        {
            ValidateRowIndex(rowIndex);
            return GetDoubleNoRowCheck(columnIndex, rowIndex);
        }

        public Double GetDouble(String columnName, long rowIndex)
        {
            ValidateRowIndex(rowIndex);
            return GetDoubleNoRowCheck(columnName, rowIndex);
        }

        public Double GetDoubleNoRowCheck(long columnIndex, long rowIndex)
        {
            ValidateColumnIndexAndTypeDouble(columnIndex);
            return GetDoubleNoCheck(columnIndex, rowIndex);
        }

        public Double GetDoubleNoRowCheck(String columnName, long rowIndex)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeDouble(columnIndex);
            return GetDoubleNoCheck(columnIndex, rowIndex);
        }

        public float GetFloat(long columnIndex, long rowIndex)
        {
            ValidateRowIndex(rowIndex);
            return GetFloatNoRowCheck(columnIndex, rowIndex);
        }

        public float GetFloat(String columnName, long rowIndex)
        {
            ValidateRowIndex(rowIndex);
            return GetFloatNoRowCheck(columnName, rowIndex);
        }

        public float GetFloatNoRowCheck(long columnIndex, long rowIndex)
        {
            ValidateColumnIndexAndTypeFloat(columnIndex);
            return GetFloatNoCheck(columnIndex, rowIndex);
        }

        public float GetFloatNoRowCheck(String columnName, long rowIndex)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeFloat(columnIndex);
            return GetFloatNoCheck(columnIndex, rowIndex);
        }



        public void SetString(long columnIndex, long rowIndex, string value)
        {
            ValidateRowIndex(rowIndex);
            ValidateColumnIndexAndTypeString(columnIndex);
            SetStringNoCheck(columnIndex, rowIndex, value);
        }

        public void SetString(string columnName, long rowIndex, string value)
        {
            ValidateRowIndex(rowIndex);
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeString(columnIndex);
            SetStringNoCheck(columnIndex, rowIndex, value);
        }

        //validation of a column index as well as the type of that index. To save a stack parameter with the type, there are one method per type        

        public void ValidateColumnIndexAndTypeString(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateTypeString(columnIndex);
        }

        public void ValidateColumnIndexAndTypeInt(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateTypeInt(columnIndex);
        }

        public void ValidateColumnIndexAndTypeBool(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateTypeBool(columnIndex);
        }

        public void ValidateColumnIndexAndTypeDate(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateTypeDate(columnIndex);
        }

        public void ValidateColumnIndexAndTypeBinary(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateTypeBinary(columnIndex);
        }

        public void ValidateColumnIndexAndTypeDouble(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateTypeDouble(columnIndex);
        }
        public void ValidateColumnIndexAndTypeFloat(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateTypeFloat(columnIndex);
        }

        public void ValidateColumnIndexAndTypeSubTable(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateTypeSubTable(columnIndex);
        }

        public void ValidateColumnIndexAndTypeMixed(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateTypeMixed(columnIndex);
        }



        public void SetStringNoRowCheck(long columnIndex, long rowIndex, string value)
        {
            ValidateColumnIndexAndTypeString(columnIndex);
            SetStringNoCheck(columnIndex, rowIndex, value);
        }

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

        public string GetColumnName(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            return GetColumnNameNoCheck(columnIndex);
        }

        internal long GetLongCheckType(long columnIndex, long rowIndex)
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

        internal void SetStringCheckType(long columnIndex, long rowIndex, string value)
        {
            ValidateTypeString(columnIndex);
            SetStringNoCheck(columnIndex, rowIndex, value);
        }


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


        internal void ValidateColumnAndRowIndex(long columnIndex, long rowIndex)
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

        public void SetBoolean(long columnIndex, long rowIndex, Boolean value)
        {
            ValidateColumnAndRowIndex(columnIndex, rowIndex);
            ValidateTypeBool(columnIndex);
            SetBoolNoCheck(columnIndex, rowIndex, value);
        }

        public void SetDateTime(long columnIndex, long rowIndex, DateTime value)
        {
            ValidateColumnAndRowIndex(columnIndex, rowIndex);
            ValidateTypeDate(columnIndex);
            SetDateTimeNoCheck(columnIndex, rowIndex, value);
        }

        public void SetDateTime(string columnName, long rowIndex, DateTime value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeDate(columnIndex);
            ValidateRowIndex(rowIndex);
            SetDateTimeNoCheck(columnIndex, rowIndex, value);
        }

        internal void SetBooleanNoRowCheck(string columnName, long rowIndex, Boolean value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeBool(columnIndex);
            SetBoolNoCheck(columnIndex, rowIndex, value);
        }

        internal void SetBooleanNoRowCheck(long columnIndex, long rowIndex, Boolean value)
        {
            ValidateColumnIndex(columnIndex);
            ValidateTypeBool(columnIndex);
            SetBoolNoCheck(columnIndex, rowIndex, value);
        }


        public Boolean GetBoolean(String name, long rowIndex)
        {
            ValidateRowIndex(rowIndex);
            long columnIndex = GetColumnIndex(name);
            ValidateTypeBool(columnIndex);
            return GetBoolNoCheck(columnIndex, rowIndex);
        }

        public Boolean GetBoolean(long columnIndex, long rowIndex)
        {
            ValidateColumnAndRowIndex(columnIndex, rowIndex);
            ValidateTypeBool(columnIndex);
            return GetBoolNoCheck(columnIndex, rowIndex);
        }



        //public aggregate functions - field identified by its position/ID
        public long CountLong(long columnIndex, long target)
        {
            ValidateColumnIndex(columnIndex);
            return CountLongNoCheck(columnIndex, target);
        }
        public long CountFloat(long columnIndex, float target)
        {
            ValidateColumnIndex(columnIndex);
            return CountFloatNoCheck(columnIndex, target);
        }
        public long CountString(long columnIndex, string target)
        {
            ValidateColumnIndex(columnIndex);
            return CountStringNoCheck(columnIndex, target);

        }
        public long CountDouble(long columnIndex, Double target)
        {
            ValidateColumnIndex(columnIndex);
            return CountDoubleNoCheck(columnIndex, target);
        }

        public long SumLong(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            return SumLongNoCheck(columnIndex);
        }
        public float SumFloat(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            return SumFloatNoCheck(columnIndex);            
        }

        public double SumDouble(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            return SumDoubleNoCheck(columnIndex);
        }

        public long MinimumLong(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            return MinimumLongNoCheck(columnIndex);
        }
        public float MinimumFloat(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            return MinimumFloatNoCheck(columnIndex);
        }
        public double MinimumDouble(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            return MinimumDoubleNoCheck(columnIndex);
        }

        public long MaximumLong(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            return MaximumLongNoCheck(columnIndex);
        }
        public float MaximumFloat(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            return MaximumFloatNoCheck(columnIndex);
        }
        public double MaximumDouble(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            return MaximumDoubleNoCheck(columnIndex);
        }

        public double AverageLong(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            return AverageLongNoCheck(columnIndex);
        }
        public double AverageFloat(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            return AverageFloatNoCheck(columnIndex);
        }
        public double AverageDouble(long columnIndex)
        {
            ValidateColumnIndex(columnIndex);
            return AverageDoubleNoCheck(columnIndex);
        }



        //public aggregate functions - field identified by its string name
        public long CountLong(string columnName, long target)
        {
            long columnIndex = GetColumnIndex(columnName);
            return CountLongNoCheck(columnIndex, target);
        }
        public long CountFloat(string columnName, float target)
        {
            long columnIndex = GetColumnIndex(columnName);
            return CountFloatNoCheck(columnIndex, target);
        }
        public long CountString(string columnName, string target)
        {
            long columnIndex = GetColumnIndex(columnName);
            return CountStringNoCheck(columnIndex, target);

        }
        public long CountDouble(string columnName, Double target)
        {
            long columnIndex = GetColumnIndex(columnName);
            return CountDoubleNoCheck(columnIndex, target);
        }

        public long SumLong(string columnName)
        {
            long columnIndex = GetColumnIndex(columnName);
            return SumLongNoCheck(columnIndex);
        }
        public float SumFloat(string columnName)
        {
            long columnIndex = GetColumnIndex(columnName);
            return SumFloatNoCheck(columnIndex);
        }
        public double SumDouble(string columnName)
        {
            long columnIndex = GetColumnIndex(columnName);
            return SumDoubleNoCheck(columnIndex);
        }

        public long MinimumLong(string columnName)
        {
            long columnIndex = GetColumnIndex(columnName);
            return MinimumLongNoCheck(columnIndex);
        }
        public float MinimumFloat(string columnName)
        {
            long columnIndex = GetColumnIndex(columnName);
            return MinimumFloatNoCheck(columnIndex);
        }
        public double MinimumDouble(string columnName)
        {
            long columnIndex = GetColumnIndex(columnName);
            return MinimumDoubleNoCheck(columnIndex);
        }

        public long MaximumLong(string columnName)
        {
            long columnIndex = GetColumnIndex(columnName);
            return MaximumLongNoCheck(columnIndex);
        }
        public float MaximumFloat(string columnName)
        {
            long columnIndex = GetColumnIndex(columnName);
            return MaximumFloatNoCheck(columnIndex);
        }
        public double MaximumDouble(string columnName)
        {
            long columnIndex = GetColumnIndex(columnName);
            return MaximumDoubleNoCheck(columnIndex);
        }


        public double AverageLong(string columnName)
        {
            long columnIndex = GetColumnIndex(columnName);
            return AverageLongNoCheck(columnIndex);
        }
        public double AverageFloat(string columnName)
        {
            long columnIndex = GetColumnIndex(columnName);
            return AverageFloatNoCheck(columnIndex);
        }
        public double AverageDouble(string columnName)
        {
            long columnIndex = GetColumnIndex(columnName);
            return AverageDoubleNoCheck(columnIndex);
        }
















        //public find first methods
        public long FindFirstInt(String columnName, long value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeInt(columnIndex);
            return FindFirstIntNoCheck(columnIndex, value);
        }

        public long FindFirstInt(long columnIndex, long value)
        {
            ValidateColumnIndexAndTypeInt(columnIndex);
            return FindFirstIntNoCheck(columnIndex, value);
        }


        public long FindFirstString(String columnName, String value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeString(columnIndex);
            return FindFirstStringNoCheck(columnIndex, value);
        }

        public long FindFirstString(long columnIndex, String value)
        {
            ValidateColumnIndexAndTypeString(columnIndex);
            return FindFirstStringNoCheck(columnIndex, value);
        }


        public long FindFirstBinary(String columnName, byte[] value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeBinary(columnIndex);
            return FindFirstBinaryNoCheck(columnIndex, value);
        }

        public long FindFirstBinary(long columnIndex, byte[] value)
        {
            ValidateColumnIndexAndTypeBinary(columnIndex);
            return FindFirstBinaryNoCheck(columnIndex, value);
        }

        public long FindFirstDouble(String columnName, double value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeDouble(columnIndex);
            return FindFirstDoubleNoCheck(columnIndex, value);
        }

        public long FindFirstDouble(long columnIndex, double value)
        {
            ValidateColumnIndexAndTypeDouble(columnIndex);
            return FindFirstDoubleNoCheck(columnIndex, value);
        }

        public long FindFirstFloat(String columnName, float value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeFloat(columnIndex);
            return FindFirstFloatNoCheck(columnIndex, value);
        }

        public long FindFirstFloat(long columnIndex, float value)
        {
            ValidateColumnIndexAndTypeFloat(columnIndex);
            return FindFirstFloatNoCheck(columnIndex, value);
        }

        public long FindFirstDateTime(String columnName, DateTime value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeDate(columnIndex);
            return FindFirstDateNoCheck(columnIndex, value);
        }

        public long FindFirstDateTime(long columnIndex, DateTime value)
        {
            ValidateColumnIndexAndTypeDate(columnIndex);
            return FindFirstDateNoCheck(columnIndex, value);
        }

        public long FindFirstBool(String columnName, bool value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeBool(columnIndex);
            return FindFirstBoolNoCheck(columnIndex, value);
        }

        public long FindFirstBool(long columnIndex, bool value)
        {
            ValidateColumnIndexAndTypeBool(columnIndex);
            return FindFirstBoolNoCheck(columnIndex, value);
        }



        //public find all methods
        public TableView FindAllInt(String columnName, long value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeInt(columnIndex);
            return FindAllIntNoCheck(columnIndex, value);
        }

        public TableView FindAllInt(long columnIndex, long value)
        {
            ValidateColumnIndexAndTypeInt(columnIndex);
            return FindAllIntNoCheck(columnIndex, value);
        }


        public TableView FindAllString(String columnName, String value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeString(columnIndex);
            return FindAllStringNoCheck(columnIndex, value);
        }

        public TableView FindAllString(long columnIndex, String value)
        {
            ValidateColumnIndexAndTypeString(columnIndex);
            return FindAllStringNoCheck(columnIndex, value);
        }






    }
}
