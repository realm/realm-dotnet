using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

//isOwned is introduced as a flag that determines if this handle should be unbound or not. In some cases we should NOT call back to c++
//when we are done with a spec, but in other cases we should.
//call back or not might be better placed in the spec class, then it could have special code that avoided ever to set up a handle, if we should
//not do anything special on finalize (dispose)

namespace TightDbCSharp
{
    class SpecHandle : TightDbHandleOptionalUnbind
    {
        //call with the root for this Spec. That is not neccesary the table, could be the shared group or group - it is the root of the tablehandle
        //or the tablehandle itself if its root is null
        public SpecHandle(bool ignoreUnbind, TightDbHandle myroot) : base(ignoreUnbind, myroot)
        {
        }


        //acquire a spec handle And set IgnoreUnbind in an atomic fashion (spec_get_spec)
        internal SpecHandle GetSubSpecHandle(long columnIndex)
        {
            //if root is null the this spechandle is responsible for cleaning up the spec and any specs taken out from it
            //if root is something else, it is the this spechandles root, and that root should also manage the specs
            //note that IgnoreUnbind is set to false, as specs gotten out from specs must be unbound            
            var sh = Root == null ? new SpecHandle(false, this) : new SpecHandle(false, Root);

            //At this point sh is invalid due to its handle being uninitialized, but the root is set correctly, as is the IgnoreUnbind setting
            //a finalize at this point will not leak anything and the handle will not unbind as it is invalid

            //now, set the spec handle...
            RuntimeHelpers.PrepareConstrainedRegions();//the following finally will run with no out-of-band exceptions
            try
            { }
            finally
            {
                sh.SetHandle(UnsafeNativeMethods.SpecGetSpec(this,columnIndex));
            }//at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly even if we crash right now
            return sh;
        }


        protected override void Unbind()
        {
            if (!IgnoreUnbind)
            {
                UnsafeNativeMethods.SpecDeallocate(this);
            }
        }
    }
}
