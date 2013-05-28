using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace TightDbCSharp
{
    public class Query : Handled, IEnumerable<TableRow>
    {
        internal override void ReleaseHandle()
        {
            UnsafeNativeMethods.QueryDelete(this);
        }

        internal Query(IntPtr handle,Table underlyingTable, bool shouldbedisposed)
        {
            SetHandle(handle, shouldbedisposed);
            _sourceTable = underlyingTable;
        }

        
        //a tableview has a private pointer to its ultimate source table, to enable its iterator to yield table records from that table
        //idea:consider if it would be better to yield table records from the tableorview being queried. I think not. too slow and too much code
        
        private readonly Table _sourceTable;//the table that this query queries.

        //assuming that i do not have to validate start and end, except they should not be smaller than -1 (-1 is used for defaults)


        //calling FindAll with no parametres will return a tableview with all matching rows in it
        public TableView FindAll()
        {
            return FindAll(0, -1, -1);//Methods that use default parameters are allowed under the Common Language Specification (CLS); however, the CLS allows compilers to ignore the values that are assigned to these parameters. 
        }

        //start = first record number in underlying table to return
        //end = first record number in underlying table not to return
        //limit = maximum number of records to return.
        //for paging through a database , set limit to number of results per page, and after each page received, set start to the row number
        //of the last received row (underlying table row number)
        public TableView FindAll(long start, long end, long limit)
        {
            Action<string, string> thrower = (errparam, errmsg) =>
                {
                    throw new ArgumentOutOfRangeException(errparam,
                                                          string.Format(CultureInfo.InvariantCulture,
                                                              "Query.FindAll({0},{1},{2}) {3}", start,
                                                              end, limit, errmsg));
                };
            
           
            if (start < -1)
            {
                thrower("start", "Start must be larger than -2");
            }

            if (end < -1)
            {
                thrower("end", "end must be larger than -2");
            }

            if (limit < -1)
            {
                thrower("end", "end must be larger than -2");
            }

            if (end < start && end > -1)//-1 means return all in tightdb so if end is -1 it is okay
            {
                thrower("end", "end must be larger than or equal to start");
            }
            return UnsafeNativeMethods.QueryFindAll(this,start, end, limit);
        }

        //if the column does not exist, an exception is thrown
        public long GetColumnIndex(string columnName)
        {
            return UnsafeNativeMethods.QueryGetColumnIndex(this,columnName);
        }

        //todo: implement a generic equal that infers the type to use from the type of the column, looked up by the column name. second parameter is then an object       
        public Query Equal(string columnName, Boolean value)
        {
            UnsafeNativeMethods.QueryBoolEqual(this,GetColumnIndex(columnName), value);
            return this;
        }

        public Query Between(string columnName, long lowValue, long highValue)
        {
            UnsafeNativeMethods.QueryIntBetween(this, GetColumnIndex(columnName), lowValue, highValue);
            return this;
        }

        //todo:unittest
        public long FindNext(long lastMatch)
        {
            return UnsafeNativeMethods.QueryFindNext(this,lastMatch);            
        }

        //the column index specifies the column in the underlying table that should be averaged (but only records that match the query)
        public Double Average(long columnIndex)
        {
            return UnsafeNativeMethods.QueryAverage(this, columnIndex);            
        }

        //todo:unittest
        public Double Average(string columnName)
        {
            long columnIndex = GetColumnIndex(columnName);
            return UnsafeNativeMethods.QueryAverage(this,columnIndex);            
        }

        //todo:unit test this - esp. the while part
        public IEnumerator<TableRow> GetEnumerator()
        {
            long nextix = -1;//-1 means start all over, means that prior call returned no value. I hope the long -1 gets translated to a intptr -1 correctly when the intptr is only 32bits
            while ((nextix = FindNext(nextix)) != -1)
            {
                yield return new TableRow(_sourceTable, nextix);
            }
        }


        //this will return the generic enumerator above when doing foreach TableRow tr in Query, as it is the closest match
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
       
        public override string ObjectIdentification()
        {
            return string.Format(CultureInfo.InvariantCulture, "Query:" + Handle);
        }

    }
}
