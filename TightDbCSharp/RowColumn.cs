using System;
using System.Globalization;

namespace TightDbCSharp
{
    /// <summary>
    /// represents a single column in a TableOrView row. 
    /// </summary>
    /// 
    //IDEA:Implement child types that each are bound to a given DataType. So we got for instance TableIntColumn
    //why? in some cases We could return TableIntColumn and with that one, we would not have to check if the Table column type is int every time
    //we read data from the row. So working with typed column fields would be somewhat faster
    //IDEA:this is just a thought consider implementing Get<T> perhaps we could interop with T to a general method in c++ that return intptr that
    //then has to be intepreted differently depending on field type and value. Not sure this is a good idea, but might be worth investigating pros and cons
    //IDEA:implement GetString, GetLong etc. etc. to enable users to do for instance row[2].GetString and thus get a typed value back
    public class RowColumn
    {
        
        private Row _owner;
        private Row Owner { get { return _owner; } set { _owner = value; _columntypeloaded = false; } }
        private long _columnIndex;
        public long ColumnIndex
        {
            get { return _columnIndex; }
            private set { _columnIndex = value; _columntypeloaded = false; }//internal bc users must not be allowed to change the columnindex. We treat it as already checked in calls
        }
        public RowColumn(Row owner,long column)
        {
            Owner = owner;
            ColumnIndex= column;
            _columntypeloaded = false;
        }

        private DataType _columnType;
        private Boolean _columntypeloaded;
        //this could be optimized by storing columncount in the table class
        public bool IsLastColumn()
        {
            return (Owner.Owner.ColumnCount==ColumnIndex+1);
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

        //Only Table can change a ColumnName, A TableView can only return the current column name
        public string ColumnName
        {
            get { return Owner.GetColumnNameNoCheck(ColumnIndex); }     
            set {
                if (Owner.Owner is Table)
                {
                    (Owner.Owner as Table).RenameColumn(ColumnIndex,value);
                }
            }
        }

        //mixed type will be set automatically when you write data to the mixed field
        public DataType MixedType
        {
            get { return Owner.MixedTypeCheckType(ColumnIndex); }            
        }

        internal DataType MixedTypeNoCheck()
        {
            return Owner.GetMixedTypeNoCheck(ColumnIndex);
        }

        //not a property because getsubtable could take time, and because it throws an exception if the field is not of subtable type
        public Table GetSubTable()
        {
            return Owner.GetSubTableCheckType(ColumnIndex);//we cannot know for sure if col,row is of the subtable type
        }

        //todo:create a unit test that hits this getter and this setter in all case statements
       
        public object Value
        {
            get
            {
                switch (ColumnType)
                {
//row and column not user specified so safe, and type checked in switch above so also safe
                    case DataType.Int:
                        return Owner.GetLongNoCheck(ColumnIndex);
                    case DataType.Bool:
                        return Owner.GetBooleanNoCheck(ColumnIndex);
                    case DataType.String:
                        return Owner.GetStringNoCheck(ColumnIndex);
                    case DataType.Binary:
                        return Owner.GetBinaryNoCheck(ColumnIndex);                        
                    case DataType.Table:
                        return Owner.GetSubTableNoCheck(ColumnIndex);
                    case DataType.Date:
                        return Owner.GetDateTimeNoCheck(ColumnIndex);
                    case DataType.Float:
                        return Owner.GetFloatNoCheck(ColumnIndex);
                    case DataType.Double:
                        return Owner.GetDoubleNoCheck(ColumnIndex);
                        
                    
                    case DataType.Mixed:
                        switch (MixedTypeNoCheck())
                        {
                            case DataType.Int:
                                return Owner.GetMixedLongNoCheck(ColumnIndex);

                            case DataType.Bool:
                                return Owner.GetMixedBoolNoCheck(ColumnIndex);

                            case DataType.String:
                                return Owner.GetMixedStringNoCheck(ColumnIndex);

                            case DataType.Binary:
                                return Owner.GetMixedBinaryNoCheck(ColumnIndex);

                            case DataType.Table:
                                return Owner.GetMixedSubtableNoCheck(ColumnIndex);

                            case DataType.Date:
                                return Owner.GetMixedDateTimeNoCheck(ColumnIndex);

                            case DataType.Float:
                                return Owner.GetMixedFloatNoCheck(ColumnIndex);

                            case DataType.Double:
                                return Owner.GetMixedDoubleNoCheck(ColumnIndex);


                            default:
                                return string.Format(CultureInfo.InvariantCulture,
                                                     "mixed with type {0} not supported yet in rowcolumn.value",
                                                     MixedTypeNoCheck());
                        }
                    default: 
                        return String.Format(CultureInfo.InvariantCulture,
                                             "Getting type {0} from RowColumn not implemented yet",
                                             ColumnType); //so null means the datatype is not fully supported yet
                }

            }
            set
            {
                switch (ColumnType)
                {
                    case DataType.Int:
                        Owner.SetLongNoCheck(ColumnIndex, Convert.ToInt64(value));//the cast will raise an exception if value is not a long, or at least convertible to long
                        break;                        
                    case DataType.Bool:
                        Owner.SetBoolNoCheck(ColumnIndex, (bool) value);
                        break;
                    case DataType.String:
                        Owner.SetStringNoCheck(ColumnIndex, (String) value);
                        break;
                    case DataType.Binary:
                        Owner.SetBinaryNoCheck(ColumnIndex, (byte[]) value);                                                
                        break;
                    case DataType.Table:
                        Owner.SetSubTableNoCheck(ColumnIndex,(Object[]) value);
                        break;
                    case DataType.Mixed:
                        Owner.SetMixedNoCheck(ColumnIndex, value);
                        break;
                    case DataType.Date:
                        Owner.SetDateNoCheck(ColumnIndex, (DateTime) value);
                        break;
                    case DataType.Float:
                        Owner.SetFloatNoCheck(ColumnIndex,(float) value);
                        break;
                    case DataType.Double:
                        Owner.SetDoubleNoCheck(ColumnIndex,(Double) value);
                        break;
                    default:
                        {
                            
                            throw new NotImplementedException(String.Format(CultureInfo.InvariantCulture,
                                                                   "setting type {0} in TableRowColumn not implemented yet",
                                                                   ColumnType));
                        }
                }
            }
        }


/*
        private long GetLong()
        {            
            return Owner.GetLong(ColumnIndex);//will be type chekced (only) in table class
        }
*/
    }
}