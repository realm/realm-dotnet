using System;

namespace TightDbCSharp
{
    /// <summary>
    /// represents a single column in a TableOrView row. 
    /// </summary>
    /// 
    //IDEA:Implement child types that each are bound to a given DataType. So we got for instance TableIntCell
    //why? in some cases We could return TableIntColumn and with that one, we would not have to check if the Table column type is int every time
    //we read data from the row. So working with typed column fields would be somewhat faster
    //IDEA:this is just a thought consider implementing Get<T> perhaps we could interop with T to a general method in c++ that return intptr that
    //then has to be intepreted differently depending on field type and value. Not sure this is a good idea, but might be worth investigating pros and cons
    //IDEA:implement GetString, GetLong etc. etc. to enable users to do for instance row[2].GetString and thus get a typed value back
    public class RowCell
    {

        private Row _owner;

        private Row Owner
        {
            get { return _owner; }
            set
            {
                _owner = value;
                _columntypeloaded = false;
            }
        }

        private long _columnIndex;

        public long ColumnIndex
        {
            get { return _columnIndex; }
            private set
            {
                _columnIndex = value;
                _columntypeloaded = false;
            } //internal bc users must not be allowed to change the columnindex. We treat it as already checked in calls
        }

        public RowCell(Row owner, long column)
        {
            Owner = owner;
            ColumnIndex = column;
            _columntypeloaded = false;
        }

        private DataType _columnType;
        private Boolean _columntypeloaded;
        //this could be optimized by storing columncount in the table class
        public bool IsLastColumn()
        {
            return (Owner.Owner.ColumnCount == ColumnIndex + 1);
        }

        public DataType ColumnType
        {
            get
            {
                if (_columntypeloaded)
                {
                    return _columnType;
                }
                return _columnType = Owner.Owner.ColumnTypeNoCheck(ColumnIndex);
            }
        }

        //A rowcell can only be part of an existing row
        //A table with rows cannot have its column names changed
        //so no setter
        public string ColumnName
        {
            get { return Owner.GetColumnNameNoCheck(ColumnIndex); }
        }

        //mixed type will be set automatically when you write data to the mixed field
        public DataType MixedType
        {
            get { return Owner.MixedTypeCheckType(ColumnIndex); }
        }


        //not a property because getsubtable could take time, and because it throws an exception if the field is not of subtable type
        public Table GetSubTable()
        {
            return Owner.GetSubTableCheckType(ColumnIndex); //we cannot know for sure if col,row is of the subtable type
        }
        
        public object Value
        {
            get
            {
                return Owner.Owner.GetValueNoCheck(ColumnIndex, Owner.RowIndex);
            }
            set
            {
                Owner.Owner.SetValueNoCheck(ColumnIndex, Owner.RowIndex,value);
            }
        }
    }
}