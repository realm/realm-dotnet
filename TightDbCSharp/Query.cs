using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
//using TightDbCSharp.Annotations;

namespace TightDbCSharp
{
    /// <summary>
    /// Represents a Query on a table.
    /// Currently under construction
    /// </summary>
    public class Query : Handled, IEnumerable<TableRow>
    {
        /// <summary>
        /// do not call. This method calls c++ and asks it to delete its object
        /// </summary>
        protected override void ReleaseHandle()
        {
            UnsafeNativeMethods.QueryDelete(this);
        }

        internal Query(IntPtr handle,Table underlyingTable, bool shouldbedisposed)
        {
            try
            {
                SetHandle(handle, shouldbedisposed,underlyingTable.ReadOnly);
                UnderlyingTable = underlyingTable;
            }
            catch (Exception)//no matter where we get an exception, we dispose just to be 100% sure
            {
                Dispose();//dispose detects if something should be disposed by checking if a handle was acquired
                throw;
            }
        }

        
        
        private Table _underlyingTable;
        /// <summary>
        /// The actual Table (not tableview) being queried
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
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

        /// <summary>
        /// Return a tableview with all the rows identified by the query
        /// </summary>
        /// <returns>TableView with query results</returns>
        public TableView FindAll()
        {
            return UnsafeNativeMethods.QueryFindAll_np(this);//Methods that use default parameters are allowed under the Common Language Specification (CLS); however, the CLS allows compilers to ignore the values that are assigned to these parameters. 
        }

        private static void ValidateStartEndLimitErrror(String errparam, string errmsg,long start, long end, long limit)
        {
            
            
            
            throw new ArgumentOutOfRangeException(errparam,
                                                  string.Format(CultureInfo.InvariantCulture,
                                                      "Query.FindAll({0},{1},{2}) {3}", start,
                                                      end, limit, errmsg));
            
        }

        private void ValidateStartEndLimit(long start, long end, long limit) 
        {
//                        Action<string, string> thrower = (errparam, errmsg) =>
//                {
//                    throw new ArgumentOutOfRangeException(errparam,
//                                                          string.Format(CultureInfo.InvariantCulture,
//                                                              "Query.FindAll({0},{1},{2}) {3}", start,
//                                                              end, limit, errmsg));
            //                };
            
           
            if (start < -1)
            {
                ValidateStartEndLimitErrror("start", "Start must be larger than -2", start, end, limit);
                //thrower("start", "Start must be larger than -2");
            }

            if (end < -1)
            {
                ValidateStartEndLimitErrror("end","end must be larger than -2",start,end,limit);
                //thrower("end", "end must be larger than -2");
            }

            if (limit < -1)
            {
                ValidateStartEndLimitErrror("end", "end must be larger than -2", start, end, limit);

//                thrower("end", "end must be larger than -2");
            }

            if (end < start && end > -1)//-1 means return all in tightdb so if end is -1 it is okay
            {
                ValidateStartEndLimitErrror("end", "end must be larger than or equal to start", start, end, limit);

//               thrower("end", "end must be larger than or equal to start");
            }

            if (end >= UnderlyingTable.Size)
            {
                ValidateStartEndLimitErrror("end", "end must be less than the size of the underlying table", start, end, limit);

              //  thrower("end", "end must be less than the size of the underlying table");
            }

        }

        //default values are advised against by microsoft http://msdn.microsoft.com/query/dev11.query?appId=Dev11IDEF1&l=EN-US&k=k(CA1026);k(TargetFrameworkMoniker-.NETFramework,Version%3Dv4.5);k(DevLang-csharp)&rd=true        

        /// <summary>
        /// Counts the number of matching rows in the table.
        /// The count starts at rowindex start, and ends at rowindex end.
        /// Limit is the maximum number the matches allowed before the function returns.
        /// </summary>
        /// <param name="start">first row index in underlying table to test for match</param>
        /// <param name="end">first row index in underlying table not to test for match</param>
        /// <param name="limit"> maximum number of rows to return</param>
        /// <returns>number of matching rows</returns>
        public long Count(long start, long end , long limit)
        {
            ValidateStartEndLimit(start,end,limit);
            return UnsafeNativeMethods.QueryCount(this, start, end, limit);
        }



        /// <summary>
        /// return the number of matching rows
        /// </summary>
        /// <returns>Number of rows that match the query</returns>
        public long Count()
        {
            ValidateStartEndLimit(0,-1,-1);
            return UnsafeNativeMethods.QueryCount(this, 0, -1, -1);
        }

       
        /// <summary>
        /// Returns all rows that match the query
        /// for paging through a database , set limit to number of results per page, and after each page received, set start to the row number
        ///  of the last received row (underlying table row number)
        /// </summary>
        /// <param name="start">first row index in underlying table to return</param>
        /// <param name="end">first row index in underlying table not to return</param>
        /// <param name="limit"> maximum number of rows to return</param>
        /// <returns></returns>
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

        /// <summary>
        /// Return the index of the column with the given name.
        /// Case sensitive.
        /// If no column exists, an exception is thrown.
        /// </summary>
        /// <param name="columnName">name of column index to retrun</param>
        /// <returns>zero based index of the column with the given name</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the column name does not exist</exception>
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

        /// <summary>
        /// return a query that returns all booleans set to value, in the column columnName
        /// </summary>
        /// <param name="columnName">Name of column to check for matches</param>
        /// <param name="value">value (true or false) of rows to return</param>
        /// <returns>query that matches all rows with specified boolean value in specified field</returns>
        public Query Equal(string columnName, Boolean value)
        {
            UnsafeNativeMethods.QueryBoolEqual(this,GetColumnIndex(columnName), value);
            return this;
        }

        /// <summary>
        /// return a query that returns all booleans set to value, in the column columnName
        /// </summary>
        /// <param name="columnIndex">zero based index of column to check for matches</param>
        /// <param name="value">value (true or false) of rows to return</param>
        /// <returns>query that matches all rows with specified boolean value in specified field</returns>
        public Query Equal(long columnIndex, Boolean value)
        {
            UnderlyingTable.ValidateColumnIndex(columnIndex);
            UnsafeNativeMethods.QueryBoolEqual(this, columnIndex, value);
            return this;
        }

        /// <summary>
        /// returns query object that matches all rows where the value in 
        /// the column specified with columnName is strictly larger than the value
        /// specified in the parameter
        /// </summary>
        /// <param name="columnName">Name of column of fields to match</param>
        /// <param name="value">values greater than this value are matched</param>
        /// <returns>Query that matches </returns>
        public Query Greater(string columnName, long value)
        {
            UnsafeNativeMethods.query_int_greater(this,GetColumnIndex(columnName),value);
            return this;
        }

        /// <summary>
        /// return query that matches all rows where the specified column
        /// contains a value that is strictly greater than the specified
        /// value.
        /// </summary>
        /// <param name="columnIndex">zero based column index of the field with value to check</param>
        /// <param name="value">rows with an integer value strictly greater than parameter value will be return</param>
        /// <returns>Query that matches rows with stricly greater value in the specified field </returns>
        public Query Greater(long columnIndex, long value)
        {
            UnderlyingTable.ValidateColumnIndex(columnIndex);
            UnsafeNativeMethods.query_int_greater(this, columnIndex, value);
            return this;
        }

        /// <summary>
        /// return query that matches all rows whose value in the specified DataType.Int filed
        /// is greater or equal to lowValue and less or equal to highValue
        /// </summary>
        /// <param name="columnIndex">Index of DataType.Int column with field to check</param>
        /// <param name="lowValue">discards rows strictrly lower than lowValue</param>
        /// <param name="highValue">discards row strictly higher than highValue</param>
        /// <returns>Query that discards rows that are  lower than lowvalue or higher than high value</returns>
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

        /// <summary>
        /// return query that matches all rows whose value in the specified DataType.Int filed
        /// is greater or equal to lowValue and less or equal to highValue
        /// </summary>
        /// <param name="columnName">Index of DataType.Int column with field to check</param>
        /// <param name="lowValue">discards rows strictrly lower than lowValue</param>
        /// <param name="highValue">discards row strictly higher than highValue</param>
        /// <returns>Query that discards rows that are  lower than lowvalue or higher than high value</returns>
        public Query Between(string columnName, long lowValue, long highValue)
        {
            long columnIndex = GetColumnIndex(columnName);
            BetweenNoCheck(columnIndex, lowValue, highValue);
            return this;
        }

        
        /// <summary>
        /// retruns row index of the first query match, parameter indicates the first record to check for a match
        /// returns -1 if there was no match
        /// </summary>
        /// <param name="beginAtTableRow">the first row that should be chekced</param>
        /// <returns>row Index of the next matching row, or -1 if there was no match</returns>
        public long Find(long beginAtTableRow )
        {
            return UnsafeNativeMethods.QueryFind(this,beginAtTableRow);            
        }

        
        /// <summary>
        /// Return the average of the specified column of matching rows.
        /// </summary>
        /// <param name="columnIndex">Zero based index of column to return average of</param>
        /// <returns>Double with Average of all matching rows</returns>
        public Double Average(long columnIndex)
        {
            return UnsafeNativeMethods.QueryAverage(this, columnIndex);            
        }


        /// <summary>
        /// Return the average of the specified column of matching rows.
        /// </summary>
        /// <param name="columnName">Name of column to return average of</param>
        /// <returns>Double with Average of all matching rows</returns>
        public Double Average(string columnName)
        {
            long columnIndex = GetColumnIndex(columnName);
            return UnsafeNativeMethods.QueryAverage(this,columnIndex);            
        }


        /// <summary>
        /// Returns an enumerator that iterates through the Query.
        /// </summary>
        /// <returns>
        /// A IEnumerator that can be used to iterate through the collection, yielding TableRow objects for each row the query matches
        /// </returns>

        public IEnumerator<TableRow> GetEnumerator()
        {
            var findmore = true;
            long nextToTest = 0;

            while (findmore)
            {                
                var rowToReturn = Find(nextToTest);
                if (rowToReturn != -1)
                {
                    nextToTest=rowToReturn+1;
                    yield return new TableRow(UnderlyingTable, rowToReturn);
                }
                else
                {
                    findmore = false;
                }
            }
        }


        //this will return the generic enumerator above when doing foreach TableRow tr in Query, as it is the closest match
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal override string ObjectIdentification()
        {
            return string.Format(CultureInfo.InvariantCulture, "Query:" + Handle);
        }

    }
}
