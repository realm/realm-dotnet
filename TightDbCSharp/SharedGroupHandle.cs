using System;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace TightDbCSharp
{
    public class SharedGroupHandle:TightDbHandle
    {
        public TransactionState State;//should default to 0=ready


        public SharedGroupHandle(TightDbHandle root) : base(root)
        {
        }

        //empty constructor to keep P/Invoke CriticalHandle support happy
        public SharedGroupHandle()
        {            
        }

        protected override void Unbind()
        {
            AbortTransaction();    //stop any leaked ongoing transaction. remove this when core do this automatically
            UnsafeNativeMethods.SharedGroupDelete(handle);            
        }

        //atomic change of transaction state from read to ready
        public  void SharedGroupCommit()
        {
            IntPtr res;
            RuntimeHelpers.PrepareConstrainedRegions();
            try { }
            finally//we have a guarentee that belwo block will run atomically as far as out of band exceptions are concerned
            {
                try
                {
                    res = UnsafeNativeMethods.SharedGroupCommit(this);
                }
                finally
                {
                  State = TransactionState.Ready;//always set state to Ready. Don't try to commit several times
                }
            }

            if (res == IntPtr.Zero)//Zero indicates success
                return;

            //shared_group_commit threw an exception in core that was caught in the c++ dll who then returned non-zero
            //currently we just assume it was an IO error, but could be anything
            //As we set the SG to invalid we lose forever the connection to c++           
            SetHandleAsInvalid();            
            throw new InvalidOperationException("sharedGroup commit exception in core. probably an IO error with the SharedGroup file");
        }

        //will end a transaction if one is ongoing will soon change to calling inside when commits get atomical
        private void AbortTransaction()
        {
            if (State == TransactionState.Read)
            {
                SharedGroupEndRead();
            }
            if (State == TransactionState.Write)
            {
                SharedGroupRollBack();
            }
        }

        //atomically call rollback and set state back to ready
        //todo:in c++ rollback is tagged as cannot throw exceptions. so i guess it's okay to just return -1 if we got an exception in c++ anyways
        public void SharedGroupRollBack()
        {
            IntPtr res;
            RuntimeHelpers.PrepareConstrainedRegions();//the following finally will run with no out-of-band exceptions
            try
            {}
            finally
            {
                res = UnsafeNativeMethods.SharedGroupRollback(this);
                State = TransactionState.Ready;
            }
            if (res == IntPtr.Zero)
                return;
            throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,"SharedGroup Rollback threw an exception in core. Probably file I/O error code:{0}",res));
        }


        //atomically call end read and set state back to ready
        //todo:in c++ end read is tagged as cannot throw exceptions. so i guess it's okay to just return -1 if we got an exception in c++ anyways
        public void SharedGroupEndRead()
        {
            IntPtr res;
            RuntimeHelpers.PrepareConstrainedRegions();//the following finally will run with no out-of-band exceptions
            try
            { }
            finally
            {
                res = UnsafeNativeMethods.SharedGroupEndRead(this);
                State = TransactionState.Ready;
            }
            if (res == IntPtr.Zero)
                return;
            throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "SharedGroupEndRead threw an exception in core. Probably file I/O error code:{0}", res));
        }




        //the method is here bc we need this handle to later close the transaction if the shared group is deleted or disposed.
        //The group handle can leak, that's no problem - we must not unbind groups gotten from transactions, they are owned core
        //this method ensures that *if* we get a transaction back, *then* we also manage to set transactionstate to InReadTransaction
        //in the same atomic unit.
        //must be called with InReadTransaction or InWriteTransaction  otherwise an empty grouphandle is returned
        //other methods that create sharedgrouphandles are in UnsafeNativeMethods
        //http://msdn.microsoft.com/en-us/library/system.runtime.compilerservices.runtimehelpers.prepareconstrainedregions(v=vs.110).aspx
        internal GroupHandle StartTransaction (TransactionState tstate)
        {
            var gh  =new GroupHandle(true,this); //set sharedgroup as root. Perhaps setting the group would also work, depends on if core can take it.
            //allocate in advance to avoid allocating in constrained exection region true means do not finalize or call unbind
            RuntimeHelpers.PrepareConstrainedRegions();//the following finally will run with no out-of-band exceptions
            try
            {}
            finally
            {
                State = tstate;//already at this point this sharedgroup will be able to finish the transaction when finalized
                if (State == TransactionState.Read)
                {
                    gh.SetHandle(UnsafeNativeMethods.SharedGroupBeginRead(this));
                }
                if (State == TransactionState.Write)
                {
                    gh.SetHandle(UnsafeNativeMethods.SharedGroupBeginWrite(this));
                }                
            }
            return gh;
        }
    }
}
