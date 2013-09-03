using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
//using TightDbCSharp.Annotations;

namespace TightDbCSharp
{
    public class Query : Handled, IEnumerable<TableRow>
    {
        protected override void ReleaseHandle()
        {
            UnsafeNativeMethods.QueryDelete(this);
        }

        internal Query(IntPtr handle,Table underlyingTable, bool shouldbedisposed)
        {
            SetHandle(handle, shouldbedisposed);
            UnderlyingTable = underlyingTable;
        }

        
        
        private Table _underlyingTable;
        public Table UnderlyingTable
        {
            get { return _underlyingTable; }
            private set
            {
                if (value != null && _underlyingTable == null)
                {
                    _underlyingTable = value;
                }
                else
                {
                    throw new ArgumentException("TableViewed can only be set once, and cannot be set to null");
                }
            }

        }//used only to make sure that a reference to the table exists until the view is disposed of
        

        //assuming that i do not have to validate start and end, except they should not be smaller than -1 (-1 is used for defaults)


        //calling FindAll with no parametres will return a tableview with all matching rows in it

        public TableView FindAll()
        {
            return UnsafeNativeMethods.QueryFindAll_np(this);//Methods that use default parameters are allowed under the Common Language Specification (CLS); however, the CLS allows compilers to ignore the values that are assigned to these parameters. 
        }

        private void ValidateStartEndLimit(long start, long end, long limit) 
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

            if (end >= UnderlyingTable.Size)
            {
                thrower("end", "end must be less than the size of the underlying table");
            }

        }

        //default values are advised against by microsoft http://msdn.microsoft.com/query/dev11.query?appId=Dev11IDEF1&l=EN-US&k=k(CA1026);k(TargetFrameworkMoniker-.NETFramework,Version%3Dv4.5);k(DevLang-csharp)&rd=true        

        public long Count(long start, long end , long limit)
        {
            ValidateStartEndLimit(start,end,limit);
            return UnsafeNativeMethods.QueryCount(this, start, end, limit);
        }



        public long Count()
        {
            ValidateStartEndLimit(0,-1,-1);
            return UnsafeNativeMethods.QueryCount(this, 0, -1, -1);
        }

        //start = first record number in underlying table to return
        //end = first record number in underlying table not to return
        //limit = maximum number of records to return.
        //for paging through a database , set limit to number of results per page, and after each page received, set start to the row number
        //of the last received row (underlying table row number)
// ReSharper disable once MemberCanBePrivate.Global
        public TableView FindAll(long start, long end, long limit)
        {
            ValidateStartEndLimit(start,end,limit);
            return UnsafeNativeMethods.QueryFindAll(this,start, end, limit);
        }

        //if the column does not exist, -1 is returned
        private long GetColumnIndexNoCheck(string columnName)
        {
            return UnsafeNativeMethods.QueryGetColumnIndex(this,columnName);
        }

        //if no column exists with name=columnName , an exception is thrown
        //returns the index of the column with the specified name
        public long GetColumnIndex(string columnName)
        {
            long columnIndex = GetColumnIndexNoCheck(columnName);
            if (columnIndex == -1)
            {
                throw new ArgumentOutOfRangeException("columnName", String.Format(CultureInfo.InvariantCulture,"Query column specified with {0} but that column does not exist in the underlying table", columnName));
            }
            return columnIndex;
        }
        //idea: implement a generic equal that infers the type to use from the type of the column, looked up by the column name. second parameter is then an object       
        //not sure it is faster or even nicer to use - so just test it out, profile and then decide
        public Query Equal(string columnName, Boolean value)
        {
            UnsafeNativeMethods.QueryBoolEqual(this,GetColumnIndex(columnName), value);
            return this;
        }

        public Query Equal(long columnIndex, Boolean value)
        {
            UnderlyingTable.ValidateColumnIndex(columnIndex);
            UnsafeNativeMethods.QueryBoolEqual(this, columnIndex, value);
            return this;
        }

        public Query Greater(string columnName, long value)
        {
            UnsafeNativeMethods.query_int_greater(this,GetColumnIndex(columnName),value);
            return this;
        }

        public Query Greater(long columnIndex, long value)
        {
            UnderlyingTable.ValidateColumnIndex(columnIndex);
            UnsafeNativeMethods.query_int_greater(this, columnIndex, value);
            return this;
        }

        public Query Between(long columnIndex, long lowValue, long highValue)
        {
            UnderlyingTable.ValidateColumnIndex(columnIndex);
            BetweenNoCheck(columnIndex, lowValue, highValue);
            return this;
        }

        //if You call this one, remember to return this to your caller
        private void BetweenNoCheck(long columnIndex, long lowValue, long highValue)
        {
            UnsafeNativeMethods.QueryIntBetween(this, columnIndex, lowValue, highValue);            
        }

        public Query Between(string columnName, long lowValue, long highValue)
        {
            long columnIndex = GetColumnIndex(columnName);
            BetweenNoCheck(columnIndex, lowValue, highValue);
            return this;
        }

        
        public long FindNext(long lastMatch)
        {
            return UnsafeNativeMethods.QueryFindNext(this,lastMatch);            
        }

        //the column index specifies the column in the underlying table that should be averaged (but only records that match the query)
        public Double Average(long columnIndex)
        {
            return UnsafeNativeMethods.QueryAverage(this, columnIndex);            
        }

       
        public Double Average(string columnName)
        {
            long columnIndex = GetColumnIndex(columnName);
            return UnsafeNativeMethods.QueryAverage(this,columnIndex);            
        }

        
        public IEnumerator<TableRow> GetEnumerator()
        {
            long nextix = -1;//-1 means start all over, means that prior call returned no value. I hope the long -1 gets translated to a intptr -1 correctly when the intptr is only 32bits
            while ((nextix = FindNext(nextix)) != -1)
            {
                yield return new TableRow(UnderlyingTable, nextix);
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
