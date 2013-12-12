using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TightDbCSharp
{
    class QueryHandle:TightDbHandle
    {
        protected override bool ReleaseHandle()
        {
            UnsafeNativeMethods.QueryDelete(handle);
            return true;
        }
    }
}
