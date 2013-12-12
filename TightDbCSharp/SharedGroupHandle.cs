using System.Runtime.CompilerServices;

namespace TightDbCSharp
{
    class SharedGroupHandle:TightDbHandle
    {
        public TransactionState State;//should default to 0=ready


        public SharedGroupHandle(TightDbHandle root) : base(root)
        {
        }

        protected override void Unbind()
        {
            EndTransaction();    //stop any leaked ongoing transaction
            UnsafeNativeMethods.SharedGroupDelete(handle);            
        }

        //will end a transaction if one is ongoingc# 
        private void EndTransaction()
        {
            if (State == TransactionState.Read)
            {
                UnsafeNativeMethods.SharedGroupEndRead(this);
            }

            if (State == TransactionState.Write)
            {
                UnsafeNativeMethods.SharedGroupRollback(this);
            }
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
            var gh  =new GroupHandle(true,this); //allocate in advance to avoid allocating in constrained exection region true means do not finalize or call unbind
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
