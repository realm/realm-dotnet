using System;
using System.Collections.Generic;
using System.Globalization;

namespace TightDbCSharp
{
    
    //abstract methods are those that are implemented differently in table and tableview
    //methods implemented here (typically c# add ons), work with both types
    public abstract class TableOrView : Handled, IEnumerable<Row>
    {
        public abstract long Size();
        public abstract DataType ColumnType(long columnIndex);
        internal abstract long GetLongNoCheck(long columnIndex,long rowIndex);//does not validate parametres or types
        internal abstract void SetLongNoCheck(long columnIndex, long rowIndex, long value);//does not validate parametres or types
        internal abstract long GetColumnCount();
        public abstract String GetColumnName(long columnIndex);
        internal abstract Spec GetSpec();
        internal abstract DataType GetMixedTypeNoCheck(long columnIndex, long rowIndex);
        internal abstract long GetMixedLongNoCheck(long columnIndex, long rowIndex);
        internal abstract void SetMixedLongNoCheck(long columnIndex, long rowIndex, long value);
        internal abstract Table GetMixedSubTableNoCheck(long columnIndex, long rowIndex);
        internal abstract void SetMixedSubtableNoCheck(long columnIndex, long rowIndex, Table source);
        internal abstract void SetMixedEmptySubtableNoCheck(long columnIndex, long rowIndex);
        internal abstract Table GetSubTableNoCheck(long columnIndex, long rowIndex);



        //the following code enables TableOrView to be enumerated, and makes Row the type You get back from an enummeration
        public IEnumerator<Row> GetEnumerator() { return new Enumerator(this); }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return new Enumerator(this); }

        class Enumerator : IEnumerator<Row>//probably overkill, current needs could be met by using yield
        {
            long _currentRow = -1;
            TableOrView _myTable;
            public Enumerator(TableOrView table)
            {
                _myTable = table;
            }
            public Row Current { get { return new Row(_myTable, _currentRow); } }
            object System.Collections.IEnumerator.Current { get { return Current; } }

            public bool MoveNext()
            {
                return ++_currentRow < _myTable.Size();
            }

            public void Reset() { _currentRow = -1; }
            public void Dispose()
            {
                _myTable = null; //remove reference to Table class
            }
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



        //most expensive subtable validation , and most expensinve mixed validation so it has its own method
        public void ValidateColumnTypeMixedSubTable(long columnIndex, long rowIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateRowIndex(rowIndex);
            ValidateColumnTypeMixed(columnIndex, rowIndex);
            if (GetMixedType(columnIndex, rowIndex) != DataType.Table)
            {
                throw new ArgumentOutOfRangeException("columnIndex", string.Format(CultureInfo.InvariantCulture, "Attempting to access subtable in mixed, but the datatype in the referenced R:{0},C:{1} mixed is of type {2}", columnIndex, rowIndex, GetMixedTypeNoCheck(columnIndex, rowIndex)));
            }
        }

        public Table GetSubTable(long columnIndex, long rowIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateRowIndex(rowIndex);
            ValidateColumnTypeSubTable(columnIndex);
            return GetSubTableNoCheck(columnIndex, rowIndex);
        }


        public void SetMixedSubTable(long columnIndex, long rowIndex, Table source)
        {
            ValidateColumnTypeMixed(columnIndex, rowIndex);
            SetMixedSubtableNoCheck(columnIndex, rowIndex, source);
        }

        public void SetMixedEmptySubTable(long columnIndex, long rowIndex)
        {
            ValidateColumnTypeMixed(columnIndex, rowIndex);
            SetMixedEmptySubtableNoCheck(columnIndex, rowIndex);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "long")]
        public long GetMixedLong(long columnIndex, long rowIndex)
        {
            ValidateColumnTypeMixed(columnIndex, rowIndex);
            return GetMixedLongNoCheck(columnIndex, rowIndex);
        }


        //methods that are common for table and tableview:
        //split into its own method to make the ordinary getsubtable very slightly faster bc it does not have to validate if type is a mixed
        public Table GetMixedSubTable(long columnIndex, long rowIndex)
        {
            ValidateColumnTypeMixedSubTable(columnIndex, rowIndex);
            return GetMixedSubTableNoCheck(columnIndex, rowIndex);
        }

        public DataType GetMixedType(long columnIndex, long rowIndex)
        {
            ValidateColumnTypeMixed(columnIndex, rowIndex);
            return GetMixedTypeNoCheck(columnIndex, rowIndex);
        }

        public void SetMixedLong(long columnIndex, long rowIndex, long value)
        {
            ValidateColumnTypeMixed(columnIndex, rowIndex);
            SetMixedLongNoCheck(columnIndex, rowIndex, value);
        }

        //we know column and row indicies are valid, but need to check if the column is in fact a mixed
        internal DataType GetMixedTypeCheckType(long columnIndex, long rowIndex)
        {
            ValidateColumnTypeMixed(columnIndex);
            return GetMixedTypeNoCheck(columnIndex, rowIndex);
        }


        public void ValidateColumnTypeMixed(long columnIndex, long rowIndex)
        {
            ValidateColumnIndex(columnIndex);
            ValidateRowIndex(rowIndex);
            ValidateColumnTypeMixed(columnIndex);
        }

        //this method is used by methods that don't have to work fast (like debugging, exceptions, logs etc.) should return the type and ID of the object, for instance TableView(34544333)

        public void ValidateRowIndex(long rowIndex)
        {
            if (rowIndex >= Size() || rowIndex < 0)
            {
                throw new ArgumentOutOfRangeException("rowIndex", string.Format(CultureInfo.InvariantCulture, "{0} accessed with an invalid Row Index{1}. Table Size is:{2}",ObjectIdentification(), rowIndex, Size())); //re-calculating when composing error message to avoid creating a variable in a performance sensitive place
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
            return String.Format(CultureInfo.InvariantCulture, "column:{0} invalid data access. Real column DataType:{1} Accessed as {2}", columnIndex, ColumnType(columnIndex), columnType);
        }


        //only call if columnIndex is already validated or known to be int
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DataType"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "GetLong")]
        internal void ValidateColumnTypeInt(long columnIndex)
        {
            if (ColumnType(columnIndex) != DataType.Int)
            {
                throw new TableException(GetColumnTypeErrorString(columnIndex, DataType.Int));
            }
        }

        //is this a row with Mixed columns?
        //todo:unit test
        internal void ValidateColumnTypeMixed(long columnIndex)
        {
            if (ColumnType(columnIndex) != DataType.Mixed)
            {
                throw new TableException(GetColumnTypeErrorString(columnIndex, DataType.Mixed));
            }
        }



        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "subTable"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "subtable")]
        internal void ValidateColumnTypeSubTable(long columnIndex)
        {
            if (ColumnType(columnIndex) != DataType.Table)
            {
                throw new TableException(GetColumnTypeErrorString(columnIndex, DataType.Table));
            }
        }


        public void SetLong(long columnIndex, long rowIndex, long value)
        {
            ValidateColumnIndex(columnIndex);
            ValidateRowIndex(rowIndex);
            ValidateColumnTypeInt(columnIndex);
            SetLongNoCheck(columnIndex, rowIndex, value);
        }


        //if You call from TableRow or TableColumn, You will save some checking - this is the slowest way
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "long")]
        public long GetLong(long columnIndex, long rowIndex)
        {
            ValidateRowIndex(rowIndex);
            ValidateColumnIndex(columnIndex);
            ValidateColumnTypeInt(columnIndex);
            return GetLongNoCheck(columnIndex, rowIndex);//could be sped up if we directly call UnsafeNativeMethods
        }

        //only call this method if You know for sure that RowIndex is less than or equal to table.size()
        //and that you know for sure that columnIndex is less than or equal to table.columncount
        internal long GetLongNoColumnRowCheck(long columnIndex, long rowIndex)
        {
            ValidateColumnTypeInt(columnIndex);
            return GetLongNoCheck(columnIndex, rowIndex);
        }

        //only call this one if You know for sure that the field at columnindex,rowindex is in fact an ordinary DataType.Int field (not mixed.integer)
        internal long GetLongNoTypeCheck(long columnIndex, long rowIndex)
        {
            ValidateRowIndex(rowIndex);
            ValidateColumnIndex(columnIndex);
            return GetLongNoCheck(columnIndex, rowIndex);
        }

    }    
}
