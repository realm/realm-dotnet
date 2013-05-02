using System;
using System.Globalization;

namespace TightDbCSharp
{
    public class Query :Handled
    {
        internal override void ReleaseHandle()
        {
            UnsafeNativeMethods.QueryDelete(this);
        }

        internal Query(IntPtr handle, bool shouldbedisposed)
        {
            SetHandle(handle, shouldbedisposed);
        }

        //assuming that i do not have to validate start and end, except they should not be smaller than -1 (-1 is used for defaults)


        
        public TableView FindAll(long start, long end, long limit)
        {
            Action<string, string> thrower = (errparam, errmsg) =>
                {
                    throw new ArgumentOutOfRangeException(errparam,
                                                          string.Format(
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

            if (end < start)
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

        //todo:hit with unit test
        public Query BoolEqual(string columnName, Boolean value)
        {
            return UnsafeNativeMethods.QueryBoolEqual(this,GetColumnIndex(columnName), value);                                   
        }

        public override string ObjectIdentification()
        {
            return string.Format(CultureInfo.InvariantCulture, "Query:" + Handle);
        }
    }
}
