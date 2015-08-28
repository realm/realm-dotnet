using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace RealmNet.Interop
{
    internal class SharedGroupHandle : RealmHandle, ISharedGroupHandle
    {

        /// <summary>
        /// Store the transaction state of this c++shared group
        /// The state will change atomically with the state in c++,
        /// using CER and CriticalHandle guarentees of non-interference from out-of-band exceptions
        /// This is important because if we have a write transaction in c++ but does not know about it in C#
        /// then we could block the database until the program is restarted, 
        /// the state of an ongoing transaction is a kind of handle that could be kind of leaked
        /// </summary>
        public TransactionState State { get; private set; }

     //should default to 0=ready

        //keep this one even though warned that it is not used. It is in fact used by marshalling
        public SharedGroupHandle(RealmHandle root) : base(root)
        {
        }

        //Empty constructor to keep P/Invoke CriticalHandle support happy
        //Please leave this one in, even though resharper reports it as not used
        [Preserve]
        public SharedGroupHandle()
        {
        }

        protected override void Unbind()
        {
            AbortTransaction(); //stop any leaked ongoing transaction. remove this when core do this automatically
            NativeSharedGroup.delete(handle);
        }

        //atomic change of transaction state from read to ready
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public void SharedGroupCommit()
        {
            IntPtr res;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
                //we have a guarentee that belwo block will run atomically as far as out of band exceptions are concerned
            {
                try
                {
                    res = NativeSharedGroup.commit(this);
                }
                finally
                {
                    State = TransactionState.Ready; //always set state to Ready. Don't try to commit several times
                }
            }

            if (res == IntPtr.Zero) //Zero indicates success
                return;

            //shared_group_commit threw an exception in core that was caught in the c++ dll who then returned non-zero
            //currently we just assume it was an IO error, but could be anything
            //As we set the SG to invalid we lose forever the connection to c++           
            SetHandleAsInvalid();
            throw new InvalidOperationException(
                "sharedGroup commit exception in core. probably an IO error with the SharedGroup file");
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
                SharedGroupRollback();
            }
        }

        //atomically call rollback and set state back to ready
        //todo:in c++ rollback is tagged as cannot throw exceptions. so i guess it's okay to just return -1 if we got an exception in c++ anyways
        /// <summary>
        /// This will roll back any write transaction this shared group handle have active.
        /// There is no validation, if You call this method and there is no active write transaction, exceptions or crash could happen
        /// The binding will never call this method unless there is an ongoing write transaction
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void SharedGroupRollback()
        {
            IntPtr res;
            RuntimeHelpers.PrepareConstrainedRegions(); //the following finally will run with no out-of-band exceptions
            try
            {
            }
            finally
            {
                res = NativeSharedGroup.rollback(this);
                State = TransactionState.Ready;
            }
            if (res == IntPtr.Zero)
                return;
            throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                "SharedGroup Rollback threw an exception in core. Probably file I/O error code:{0}", res));
        }


        //atomically call end read and set state back to ready
        //todo:in c++ end read is tagged as cannot throw exceptions. so i guess it's okay to just return -1 if we got an exception in c++ anyways
        public void SharedGroupEndRead()
        {
            IntPtr res;
            RuntimeHelpers.PrepareConstrainedRegions(); //the following finally will run with no out-of-band exceptions
            try
            {
            }
            finally
            {
                res = NativeSharedGroup.end_read(this);
                State = TransactionState.Ready;
            }
            if (res == IntPtr.Zero)
                return;
            throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                "SharedGroupEndRead threw an exception in core. Probably file I/O error code:{0}", res));
        }




        //the method is here bc we need this handle to later close the transaction if the shared group is deleted or disposed.
        //The group handle can leak, that's no problem - we must not unbind groups gotten from transactions, they are owned core
        //this method ensures that *if* we get a transaction back, *then* we also manage to set transactionstate to InReadTransaction
        //in the same atomic unit.
        //must be called with InReadTransaction or InWriteTransaction  otherwise an empty grouphandle is returned
        //other methods that create sharedgrouphandles are in NativeGroup
        //http://msdn.microsoft.com/en-us/library/system.runtime.compilerservices.runtimehelpers.prepareconstrainedregions(v=vs.110).aspx
        public IGroupHandle StartTransaction(TransactionState tstate)
        {
            GroupHandle gh = null;
            try
            {
                gh = new GroupHandle(true, this);
                //set sharedgroup as root. Perhaps setting the group would also work, depends on if core can take it.
                //allocate in advance to avoid allocating in constrained exection region true means do not finalize or call unbind
                RuntimeHelpers.PrepareConstrainedRegions();
                //the following finally will run with no out-of-band exceptions
                try
                {
                }
                finally
                {
                    State = tstate;
                    //already at this point this sharedgroup will be able to finish the transaction when finalized
                    //at this very point, doing nothing as the handle is not set yet
                    if (State == TransactionState.Read)
                    {
                        gh.SetHandle(NativeSharedGroup.begin_read(this));
                    }
                    if (State == TransactionState.Write)
                    {
                        gh.SetHandle(NativeSharedGroup.begin_write(this));
                    }
                    //at this point temp's finalizer will guarenteee no leaking transactions
                    //gh = temp;
                    //temp = null;
                }
                return gh;//it things work well, we exit the function here with a functioning handle as return value
            }
            catch
            {
                //this part to avoid CA2000 warning. Although we always return a working gh in the code above,
                //this finally block will dispose gh in the unexpected case we had an exception after gh was created, but 
                //before it was returned.            
                if (gh != null )
                    gh.Dispose();
                throw ;//if we throw an exception for some reason, we get out without returning any handle, but with the handle correctly cleaned up
            }
        }
    }
}
