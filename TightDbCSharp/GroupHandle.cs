using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace TightDbCSharp
{
    public class GroupHandle:TightDbHandleOptionalUnbind
    {

        //needed for P/Invoke to be able to create an empty object
        public GroupHandle() 
        {
        }

        protected override void Unbind()
        {
            if (!IgnoreUnbind)
            {
                UnsafeNativeMethods.GroupDelete(this);
                //was this before but when called via the destructor, this got freed bf callee could set handle to zero
            }
        }

        //acquire a table handle And set root in an atomic fashion (from TableName)
        internal TableHandle GetTable(String name)
        {
            var th= TableHandle.RootedTableHandle(this); //allocate in advance to avoid allocating in constrained exection region true means do not finalize or call unbind
            //At this point th is invalid due to its handle being uninitialized, but the root is set correct
            //a finalize at this point will not leak anything and the handle will not do anything
            RuntimeHelpers.PrepareConstrainedRegions();//the following finally will run with no out-of-band exceptions
            try
            {}
            finally
            {              
              th.SetHandle(UnsafeNativeMethods.GroupGetTable(this, name));//if something goes wrong in c++ land IntPtr.Zero is returned
            }//at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly
            if (th.IsInvalid)
            {
                throw new ArgumentOutOfRangeException(String.Format(CultureInfo.InvariantCulture,"Group.GetTable did not get a Table back from core. The name specified is probably invalid :({0})",name));
            }
            return th;
        }

        //acquire a table handle And set root in an atomic fashion (from TableIndex)
        internal TableHandle GetTable(long tableIndex)
        {
            var th = TableHandle.RootedTableHandle(this); //allocate in advance to avoid allocating in constrained exection region true means do not finalize or call unbind
            //At this point th is invalid due to its handle being uninitialized, but the root is set correct
            //a finalize at this point will not leak anything and the handle will not do anything
            RuntimeHelpers.PrepareConstrainedRegions();//the following finally will run with no out-of-band exceptions
            try
            { }
            finally
            {
                th.SetHandle(UnsafeNativeMethods.GroupGetTable(this, tableIndex));
            }//at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly
            return th;
        }        

        internal GroupHandle(bool ignoreUnbind,TightDbHandle root) : base(ignoreUnbind,root)
        {            
        }
        
    }
}
