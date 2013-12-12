using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TightDbCSharp
{
    class TableHandle:TightDbHandle
    {
        public TableHandle(TightDbHandle root) : base(root)
        {
        }

        protected override void Unbind()
        {
            UnsafeNativeMethods.TableUnbind(handle);
        }



        //acquire a spec handle And set IgnoreUnbind in an atomic fashion (table_get_spec)
        internal SpecHandle GetSpec()
        {
            //if root is null the this tablehandle is responsible for cleaning up the spec and any specs taken out from it
            //if root is something else, it is the this tablehandles root, and that root should also manage the specs
            //note that IgnoreUnbind is set to true, so in fact the spec created here will not call back.
            //but at least theoreticall (if the binding is extended) spec children might want to be unbindable and they must be linked to root
            var sh = Root == null ? new SpecHandle(true, this) : new SpecHandle(true,Root);
            
            //At this point sh is invalid due to its handle being uninitialized, but the root is set correctly, as is the IgnoreUnbind setting
            //a finalize at this point will not leak anything and the handle will not do anything

            //now, set the spec handle...
            RuntimeHelpers.PrepareConstrainedRegions();//the following finally will run with no out-of-band exceptions
            try
            { }
            finally
            {
                sh.SetHandle(UnsafeNativeMethods.TableGetSpec(this));
            }//at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly
            return sh;
        }
    }
}

/*sample implementation of two-tiered handle instantiation , from http://blogs.msdn.com/b/bclteam/archive/2005/03/15/396335.aspx
 * 
 * //Best practice to avoid object allocation inside CER.
MySafeHandle mySafeHandle = new MySafeHandle(0, true);
IntPtr myHandle;
IntPtr invalidHandle = new IntPtr(-1));
Int32 ret;
 
       // The creation of myHandle and assignment to mySafeHandle should be done inside a CER
RuntimeHelpers.PrepareConstrainedRegions();
try {// Begin CER
}
        finally {
ret = MyNativeMethods.CreateHandle(out myHandle);
              if (ret ==0 && !myHandle.IsNull() && myHandle != invalidHandle)
            mySafeHandle.SetHandle(myHandle);
        }// End CER
*/

