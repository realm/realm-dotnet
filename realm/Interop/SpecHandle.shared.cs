using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

//isOwned is introduced as a flag that determines if this handle should be unbound or not. In some cases we should NOT call back to c++
//when we are done with a spec, but in other cases we should.
//call back or not might be better placed in the spec class, then it could have special code that avoided ever to set up a handle, if we should
//not do anything special on finalize (dispose)

namespace RealmNet.Interop
{
    internal class SpecHandle : RealmHandleOptionalUnbind, ISpecHandle
    {
        //call with the root for this Spec. That is not neccesary the table, could be the shared group or group - it is the root of the tablehandle
        //or the tablehandle itself if its root is null
        public SpecHandle(bool ignoreUnbind, RealmHandle root) : base(ignoreUnbind, root)
        {
        }


        //acquire a spec handle And set IgnoreUnbind in an atomic fashion (spec_get_spec)
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        internal SpecHandle GetSubSpecHandle(long columnIndex)
        {            
            SpecHandle sh = null;//acc to CA2000 guidelines we guarentee disposal of temp in finally lower down
            try
            {
                
                //if root is null the this spechandle is responsible for cleaning up the spec and any specs taken out from it
                //if root is something else, it is the this spechandles root, and that root should also manage the specs
                //note that IgnoreUnbind is set to false, as specs gotten out from specs must be unbound            
                 sh = Root == null ? new SpecHandle(false, this) : new SpecHandle(false, Root);

                //At this point temp is invalid due to its handle being uninitialized, but the root is set correctly, as is the IgnoreUnbind setting
                //a finalize at this point will not leak anything and the handle will not unbind as it is invalid

                //now, set the spec handle...
                RuntimeHelpers.PrepareConstrainedRegions();
                try//the following finally will run with no out-of-band exceptions
                {
                }
                finally
                {
                    sh.SetHandle(UnsafeNativeMethods.spec_get_spec(this, columnIndex));
                }
                    //at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly even if we crash right now
                return sh;
            }
           catch//this part cleans up temp if we got exceptions thrown in the preceding code
            {
                if (sh != null)                
                    sh.Dispose();
                throw;
            }
        }


        protected override void Unbind()
        {
            if (!IgnoreUnbind)
            {
                UnsafeNativeMethods.spec_deallocate(this);
            }
        }
    }
}
