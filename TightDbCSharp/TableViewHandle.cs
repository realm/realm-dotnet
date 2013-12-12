using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TightDbCSharp
{
    class TableViewHandle:TightDbHandle
    {
        protected override bool ReleaseHandle()
        {
            UnsafeNativeMethods.TableViewUnbind(handle);
            return true;
        }
    }
}
