using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TightDbCSharp
{
    //tables and tableviews will always return query c++ classes that must be unbound
    //all other query returning calls (on query itself) will return the same object as was called,
    //and therefore have been changed to void calls in c++ part of binding
    //so these handles always represent a qeury object that should be released when not used anymore
    //the C# binding methods on query simply return self to add the . nottation again
    //A query will be a child of whatever root its creator has as root (queries are usually created by tableviews and tables)
    class QueryHandle:TightDbHandle
    {
        public QueryHandle(TightDbHandle root) : base(root)
        {
        }

        protected override void Unbind()
        {
            UnsafeNativeMethods.QueryDelete(this);
        }
    }
}
