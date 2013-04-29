using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TightDbCSharp
{
    public class Group:Handled
    {
        //constructor called by customers
        public Group()
        {
            UnsafeNativeMethods.GroupNew(this);//calls sethandle itself
        }

        //TODO:erorr handling if user specifies an illegal filename or path.
        //right now we crash bc of exceptions on the c++ side
        public Group(string fileName)
        {
            UnsafeNativeMethods.GroupNewFile(this,fileName);
        }

        internal override void ReleaseHandle()
        {
            UnsafeNativeMethods.GroupDelete(this);
        }

        public override string ObjectIdentification()
        {
            return string.Format(CultureInfo.InvariantCulture, "Group:" + Handle);
        }
    }
}
