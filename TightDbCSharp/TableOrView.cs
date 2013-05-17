using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace TightDbCSharp
{
    
    //abstract methods are those that are implemented differently in table and tableview
    //methods implemented here , work with both types
    public abstract class TableOrView : Handled, IEnumerable
    {
        internal abstract long GetSize();
        internal abstract long GetColumnCount();
        internal abstract String GetColumnNameNoCheck(long columnIndex);
        internal abstract Spec GetSpec();
        internal abstract DataType ColumnTypeNoCheck(long columnIndex);
        internal abstract DataType GetMixedTypeNoCheck(long columnIndex, long rowIndex);
        internal abstract Table GetMixedSubTableNoCheck(long columnIndex, long rowIndex);
        internal abstract Table GetSubTableNoCheck(long columnIndex, long rowIndex);
        internal abstract Boolean GetBooleanNoCheck(long columnIndex, long rowIndex);
        internal abstract String GetStringNoCheck(long columnIndex, long rowIndex);
        internal abstract long GetLongNoCheck(long columnIndex, long rowIndex);//does not validate parametres or types
        internal abstract long GetMixedLongNoCheck(long columnIndex, long rowIndex);
        internal abstract Double GetMixedDoubleNoCheck(long columnIndex, long rowIndex);
        internal abstract DateTime GetDateTimeNoCheck(long columnIndex, long rowIndex);
        internal abstract DateTime GetMixedDateTimeNoCheck(long columnIndex, long rowIndex);
        internal abstract long GetColumnIndexNoCheck(string name);//-1 if CI does not exists
        
        
        internal abstract long FindFirstIntNoCheck(long columnIndex, long value);
        internal abstract long FindFirstStringNoCheck(long columnIndex, string value);
        internal abstract long FindFirstBinaryNoCheck(long columnIndex, byte[] value);
        internal abstract long FindFirstDoubleNoCheck(long columnIndex, double value);
        internal abstract long FindFirstFloatNoCheck(long columnIndex, float value);
        internal abstract long FindFirstDateNoCheck(long columnIndex, DateTime value);
        internal abstract long FindFirstBoolNoCheck(long columnIndex, bool value);
        

        
        
        internal abstract TableView FindAllIntNoCheck(long columnIndex, long value);



        internal abstract void SetBooleanNoCheck(long columnIndex, long rowIndex, Boolean value);        
        internal abstract void SetDateTimeNoCheck(long columnIndex, long rowIndex, DateTime value);
        internal abstract void SetDoubleNoCheck(long columnIndex, long rowIndex, double value);
        internal abstract void SetFloatNoCheck(long columnIndex, long rowIndex, float value);
        internal abstract void SetLongNoCheck(long columnIndex, long rowIndex, long value);//does not validate parametres or types
        internal abstract void SetStringNoCheck(long columnIndex, long rowIndex, string value);
        
        internal abstract void SetMixedLongNoCheck(long columnIndex, long rowIndex, long value);
        internal abstract void SetMixedFloatNoCheck(long columnIndex, long rowIndex, float value);
        internal abstract void SetMixedDoubleNoCheck(long columnIndex, long rowIndex, double value);
        internal abstract void SetMixedDateTimeNoCheck(long columnIndex, long rowIndex, DateTime value);
        internal abstract void SetMixedSubtableNoCheck(long columnIndex, long rowIndex, Table source);
        internal abstract void SetMixedEmptySubtableNoCheck(long columnIndex, long rowIndex);

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
            if (columnIndex == -1) { throw new ArgumentOutOfRangeException("name", "column name specified is not a valid column in this table"); }
            return columnIndex;
        }

        

        //do not check row index
        //todo:ensure all datatypes are in the switch both mixed and datatype. add todo implement comments if neccesary
        internal void SetRowNoCheck(long rowIndex, params object[] rowContents)
        {
            if (rowContents.Length != ColumnCount)
                throw new ArgumentOutOfRangeException("rowContents", String.Format(CultureInfo.InvariantCulture, "SetRow called with {0} objects, but there are only {1} columns in the table", rowContents.Length, ColumnCount));
            for (long ix = 0; ix < ColumnCount; ix++)
            {
                object element = rowContents[ix];//element is parameter number ix
                //first do a switch on directly compatible types
                Type elementType = element.GetType();//performance hit as it is not neccessarily used, but used many blaces below

                switch (ColumnTypeNoCheck(ix))
                {
                    case DataType.Int:
                        SetLongNoCheck(ix, rowIndex, (long)element);//this throws exceptions if called with something too weird
                        break;
                    case DataType.Bool:
                        SetBooleanNoCheck(ix, rowIndex, (Boolean)element);
                        break;
                    case DataType.String:
                        SetStringNoCheck(ix, rowIndex, (string)element);
                        break;
                    case DataType.Binary://todo:implement
                        break;
                    case DataType.Table://todo:test thoroughly with unit test, also with invalid data
                        Table t = GetSubTableNoCheck(ix, rowIndex);//The type to go into a subtable has to be an array of arrays of objects

                        if (element != null)//if You specify null for a subtable we do nothing null means You intend to fill it in later
                        {


                            if (elementType != typeof(Array))
                            {
                                throw new ArgumentOutOfRangeException(String.Format(CultureInfo.InvariantCulture,
                                                                                    "SetRow called with a non-array type {0} for the subtable column {1}",
                                                                                    elementType,
                                                                                    GetColumnNameNoCheck(ix)));
                            }
                            //at this point we know that element is an array of some sort, hopefully containing valid records for our subtable                                                
                            foreach (Array arow in (Array)element)//typecast because we already ensured element is an array,and that element is not null
                            {
                                t.Add(arow);
                            }
                        }
                        break;
                    case DataType.Mixed://Try to infer the mixed type to use from the type of the object from the user

                        
                        //todo:add support for more types here
                        //todo:add a throw statement if an unsupported type shows up in the parametres. remmeber to fixup the insert in progress first


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
                            SetMixedLongNoCheck(ix, rowIndex, (long)element);
                            break;
                        }

                        if (elementType == typeof(Single))//float, Single
                        {
                            SetMixedFloatNoCheck(ix, rowIndex, (float)element);
                            break;
                        }

                        if (elementType == typeof(Double))
                        {
                            SetMixedDoubleNoCheck(ix, rowIndex, (Double)element);
                            break;
                        }

                        if (elementType == typeof(DateTime))
                        {
                            SetMixedDateTimeNoCheck(ix, rowIndex, (DateTime)element);
                        }



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
                        throw new ArgumentOutOfRangeException("rowContents", String.Format(CultureInfo.InvariantCulture, "An element ix:{0} of type {1} in a row sent to AddRow, is not of a supported tightdb type ", ix, elementType));
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
            ValidateColumnTypeDate(columnIndex);
            return GetDateTimeNoRowCheck(columnIndex, rowIndex);
        }

        

        public void SetMixedSubTable(long columnIndex, long rowIndex, Table source)
        {
            ValidateColumnRowTypeMixed(columnIndex, rowIndex);
            SetMixedSubtableNoCheck(columnIndex, rowIndex, source);
        }

        public void SetMixedEmptySubTable(long columnIndex, long rowIndex)
        {
            ValidateColumnRowTypeMixed(columnIndex, rowIndex);
            SetMixedEmptySubtableNoCheck(columnIndex, rowIndex);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "long")]
        public long GetMixedLong(long columnIndex, long rowIndex)
        {
            ValidateColumnRowMixedType(columnIndex, rowIndex,DataType.Int);
            return GetMixedLongNoCheck(columnIndex, rowIndex);
        }


        public double GetMixedDouble(long columnIndex, long rowIndex)
        {
            ValidateColumnRowMixedType(columnIndex, rowIndex, DataType.Double);
            return GetMixedDoubleNoCheck(columnIndex, rowIndex);
        }


        public DateTime GetMixedDateTime(long columnIndex, long rowIndex)
        {
            ValidateColumnRowMixedType(columnIndex, rowIndex,DataType.Date);
            return GetMixedDateTimeNoCheck(columnIndex, rowIndex);
        }

        public DateTime GetMixedDateTime(string columnName, long rowIndex)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateRowIndex(rowIndex);
            ValidateMixedType(columnIndex,rowIndex,DataType.Date);
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
            ValidateColumnAndRowIndex(columnIndex,rowIndex);
            ValidateColumnTypeDate(columnIndex);
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
            ValidateColumnRowMixedType(columnIndex,rowIndex,DataType.Table);            
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

        public void SetMixedDouble(long columnIndex, long rowIndex, double value)
        {
            ValidateColumnRowTypeMixed(columnIndex, rowIndex);
            SetMixedDoubleNoCheck(columnIndex, rowIndex, value);
        }

        
        public void SetMixedDateTime(long columnIndex, long rowIndex, DateTime value)
        {
            ValidateColumnRowTypeMixed(columnIndex, rowIndex);
            SetMixedDateTimeNoCheck(columnIndex, rowIndex, value);
        }
        
        public void SetMixedDateTime(string columnName, long rowIndex, DateTime value)
        {
            SetMixedDateTimeNoColumnCheck(GetColumnIndex(columnName),rowIndex,value);
        }

        internal void SetMixedDateTimeNoColumnCheck(long columnIndex, long rowIndex, DateTime value)
        {
            ValidateTypeMixed(columnIndex);//only checks that the CI points to a mixed
            ValidateRowIndex(rowIndex);            //only checks that the row is valid
            SetMixedDateTimeNoCheck(columnIndex,rowIndex,value);//this is okay as a mixed will be set to the type you put into it
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
            ValidateColumnRowTypeMixed(columnIndex,rowIndex);//we only progress if the field is mixed
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
                throw new ArgumentOutOfRangeException("rowIndex", string.Format(CultureInfo.InvariantCulture, "{0} accessed with an invalid Row Index{1}. Table Size is:{2}",ObjectIdentification(), rowIndex, Size)); //re-calculating when composing error message to avoid creating a variable in a performance sensitive place
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DataType"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "GetLong")]
        internal void ValidateTypeInt(long columnIndex)
        {
            if (ColumnTypeNoCheck(columnIndex) != DataType.Int)
            {
                throw new TableException(GetColumnTypeErrorString(columnIndex, DataType.Int));
            }
        }

        //only call if columnIndex is already validated 
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DataType"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "GetLong")]
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

        internal void ValidateTypeDate(long columnIndex)
        {
            if (ColumnTypeNoCheck(columnIndex) != DataType.Date)
            {
                throw new TableException(GetColumnTypeErrorString(columnIndex, DataType.Date));
            }
        }



        //only call if columnIndex is already validated         
        internal void ValidateTypeMixed(long columnIndex)
        {
            if (ColumnTypeNoCheck(columnIndex) != DataType.Mixed)
            {
                throw new ArgumentOutOfRangeException("columnIndex",GetColumnTypeErrorString(columnIndex, DataType.Mixed));
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
        internal void ValidateColumnTypeDate(long columnIndex)
        {
            if (ColumnTypeNoCheck(columnIndex) != DataType.Date)
            {
                throw new TableException(GetColumnTypeErrorString(columnIndex, DataType.Date));
            }
        }


        public void Remove(long rowIndex)
        {
            ValidateRowIndex(rowIndex);
            RemoveNoCheck(rowIndex);
            //todo:invalidate any iterators - or?
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "subTable"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "subtable")]
        internal void ValidateColumnTypeSubTable(long columnIndex)
        {
            if (ColumnTypeNoCheck(columnIndex) != DataType.Table)
            {
                throw new TableException(GetColumnTypeErrorString(columnIndex, DataType.Table));
            }
        }


        public void SetLong(long columnIndex, long rowIndex, long value)
        {
            ValidateRowIndex(rowIndex);
            ValidateColumnIndexAndTypeInt(columnIndex);
            SetLongNoCheck(columnIndex, rowIndex, value);
        }

        public void SetLongNoRowCheck(long columnIndex, long rowIndex, long value)
        {
            ValidateColumnIndexAndTypeInt(columnIndex);
            SetLongNoCheck(columnIndex,rowIndex,value);
        }

        public void SetLongNoRowCheck(string columnName, long rowIndex, long value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeInt(columnIndex);
            SetLongNoCheck(columnIndex,rowIndex,value);
        }

        //if You call from TableRow or TableColumn, You will save some checking - this is the slowest way
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "long")]
        public long GetLong(long columnIndex, long rowIndex)
        {
            ValidateRowIndex(rowIndex);
            ValidateColumnIndexAndTypeInt(columnIndex);            
            return GetLongNoCheck(columnIndex, rowIndex);//could be sped up if we directly call UnsafeNativeMethods
        }

        public void SetString(long columnIndex, long rowIndex,string value)
        {
            ValidateRowIndex(rowIndex);
            ValidateColumnIndexAndTypeString(columnIndex);
            SetStringNoCheck(columnIndex,rowIndex,value);
        }

        public void SetString(string columnName, long rowIndex, string value)
        {
            ValidateRowIndex(rowIndex);
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeString(columnIndex);
            SetStringNoCheck(columnIndex,rowIndex,value);
        }

        //validation of a column index as well as the type of that index. To save a stack parameter with the type, there are one method per type        
        //TODO:ensure that the compiler as expected emits efficient code for calling (that the call is statically linked)
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
            ValidateColumnTypeDate(columnIndex);
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
            SetStringNoCheck(columnIndex,rowIndex,value);
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

        internal String GetStringCheckType(long columnIndex, long rowIndex)
        {
            ValidateTypeString(columnIndex);
            return GetStringNoCheck(columnIndex, rowIndex);
        }

        internal void SetStringCheckType(long columnIndex, long rowIndex, string value)
        {
            ValidateTypeString(columnIndex);
            SetStringNoCheck(columnIndex, rowIndex, value);            
        }        


        internal Boolean GetBooleanNoRowCheck(long columnIndex, long rowIndex)
       {
           ValidateColumnIndex(columnIndex);
           ValidateTypeBool(columnIndex);
           return GetBooleanNoCheck(columnIndex, rowIndex);
       }

        internal Boolean GetBooleanNoRowCheck(string name, long rowIndex)
        {
            long columnIndex = GetColumnIndex(name);
            ValidateTypeBool(columnIndex);
            return GetBooleanNoCheck(columnIndex, rowIndex);
        }

        //only call this method if You know for sure that RowIndex is less than or equal to table.size()
        //and that you know for sure that columnIndex is less than or equal to table.columncount
        internal long GetLongNoColumnRowCheck(long columnIndex, long rowIndex)
        {
            ValidateTypeInt(columnIndex);
            return GetLongNoCheck(columnIndex, rowIndex);
        }

        internal void ValidateColumnAndRowIndex(long columnIndex, long rowIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateRowIndex(rowIndex);
        }

        //only call this one if You know for sure that the field at columnindex,rowindex is in fact an ordinary DataType.Int field (not mixed.integer)
        internal long GetLongNoTypeCheck(long columnIndex, long rowIndex)
        {
            ValidateColumnAndRowIndex(columnIndex,rowIndex);
            return GetLongNoCheck(columnIndex, rowIndex);
        }


        public void SetBoolean(long columnIndex, long rowIndex,Boolean value)
        {
            ValidateColumnAndRowIndex(columnIndex, rowIndex);
            ValidateTypeBool(columnIndex);
            SetBooleanNoCheck(columnIndex, rowIndex,value);
        }

        public void SetDateTime(long columnIndex, long rowIndex, DateTime value)
        {
            ValidateColumnAndRowIndex(columnIndex,rowIndex);
            ValidateColumnTypeDate(columnIndex);
            SetDateTimeNoCheck(columnIndex,rowIndex,value);
        }

        public void SetDateTime(string columnName, long rowIndex, DateTime value)
        {
            long columnIndex = GetColumnIndex(columnName);
            ValidateColumnTypeDate(columnIndex);
            ValidateRowIndex(rowIndex);
            SetDateTimeNoCheck(columnIndex, rowIndex, value);
        }

        internal void SetBooleanNoRowCheck(string columnName, long rowIndex, Boolean value)
        {        
            long columnIndex = GetColumnIndex(columnName);
            ValidateTypeBool(columnIndex);
            SetBooleanNoCheck(columnIndex,rowIndex,value);
        }

        internal void SetBooleanNoRowCheck(long columnIndex, long rowIndex, Boolean value)
        {
            ValidateColumnIndex(columnIndex);
            ValidateTypeBool(columnIndex);
            SetBooleanNoCheck(columnIndex,rowIndex,value);
        }

        
        public Boolean GetBoolean(String name, long rowIndex)
        {
            ValidateRowIndex(rowIndex);
            long columnIndex = GetColumnIndex(name);
            ValidateTypeBool(columnIndex);
            return GetBooleanNoCheck(columnIndex, rowIndex);
        }

        public Boolean GetBoolean(long columnIndex, long rowIndex)
        {
            ValidateColumnAndRowIndex(columnIndex,rowIndex);
            ValidateTypeBool(columnIndex);
            return GetBooleanNoCheck(columnIndex, rowIndex);
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



    }    
}
