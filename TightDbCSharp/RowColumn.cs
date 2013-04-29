using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace TightDbCSharp
{
    /// <summary>
    /// represents a single column in a TableOrView row. 
    /// </summary>
    /// 
    //TODO:Implement child types that each are bound to a given DataType. So we got for instance TableIntColumn
    //why? in some cases We could return TableIntColumn and with that one, we would not have to check if the Table column type is int every time
    //we read data from the row. So working with typed column fields would be somewhat faster
    public class RowColumn
    {
        //this is a test
        [DllImport("tightdb_c_cs64", EntryPoint = "test_testacquireanddeletegroup", CallingConvention = CallingConvention.Cdecl)]
        private static extern void test_testacquireanddeletegroup64();
        [DllImport("tightdb_c_cs32", EntryPoint = "test_testacquireanddeletegroup", CallingConvention = CallingConvention.Cdecl)]
        private static extern void test_testacquireanddeletegroup32();

        public static void test_testacquireanddeletegroup()
        {
            if (false)
                test_testacquireanddeletegroup64();
            test_testacquireanddeletegroup32();
        }
        //end of test
        
        private Row _owner;
        public Row Owner { get { return _owner; } set { _owner = value; _columntypeloaded = false; } }
        private long _columnIndex;
        public long ColumnIndex
        {
            get { return _columnIndex; }
            internal set { _columnIndex = value; _columntypeloaded = false; }//internal bc users must not be allowed to change the columnindex. We treat it as already checked in calls
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
                return _columnType = Owner.Owner.ColumnType(ColumnIndex);                
            }
        }

        public DataType MixedType()
        {
            return Owner.MixedTypeCheckType(ColumnIndex);
        }

        internal DataType MixedTypeNoCheck()
        {
            return Owner.GetMixedTypeNoCheck(ColumnIndex);
        }

        //if it is a mixed we return mixed! -not the type of the field
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming",
            "CA2204:Literals should be spelled correctly", MessageId = "TableRowColumn")]
        public object Value
        {
            get
            {
                switch (ColumnType)
                {//row and column not user specified so safe, and type checked in switch above so also safe
                    case DataType.Int:
                        return Owner.GetLongNoCheck(ColumnIndex);                        
                    case DataType.Bool:
                        return Owner.GetBooleanNoCheck(ColumnIndex);
                        case DataType.String:
                        return Owner.GetStringNoCheck(ColumnIndex);
                    case DataType.Mixed:
                        switch (MixedTypeNoCheck())
                        {
                            case DataType.Int:
                                return Owner.GetMixedLongNoCheck(ColumnIndex);

                            case DataType.Table:
                                return Owner.GetMixedSubtableNoCheck(ColumnIndex);
                            default:
                                return string.Format(CultureInfo.InvariantCulture,
                                                     "mixed with type {0} not supported yet in tabledumper",
                                                     MixedTypeNoCheck());
                        }
                    default: //todo:implement Table and other missing types
                        return String.Format(CultureInfo.InvariantCulture,
                                             "Getting type {0} from TableRowColumn not implemented yet",
                                             ColumnType); //so null means the datatype is not fully supported yet
                }
            }
            set
            {
                switch (ColumnType)
                {
                    case DataType.Int:
                        Owner.SetLongNoCheck(ColumnIndex, (long)value );//the cast will raise an exception if value is not a long, or at least convertible to long
                        break;
                    default:
                        {//todo:impelement more types
                            throw new TableException(String.Format(CultureInfo.InvariantCulture,
                                                                   "setting type {0} in TableRowColumn not implemented yet",
                                                                   ColumnType));
                        }
                }
            }
        }


        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DataType"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "GetLong")]
/*
        private long GetLong()
        {            
            return Owner.GetLong(ColumnIndex);//will be type chekced (only) in table class
        }
*/
    }
}