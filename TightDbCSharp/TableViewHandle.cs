using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TightDbCSharp
{
    class TableViewHandle:TightDbHandle
    {
        protected override void Unbind()
        {
            UnsafeNativeMethods.TableViewUnbind(handle);
        }

        //used in Table to create TableviewHandles atomically, specifying both root and handle at the same time
        internal TableViewHandle(TightDbHandle root)
            : base(root)
        {
        }

    }
}
