using System;
using System.Globalization;

namespace TightDbCSharp
{
    
    /// <summary>
    /// Transaction class.
    /// Is returned by SharedGroup when starting a read or write transaction.
    /// The transaction is inherited from Group, so You can manipulate all tables
    /// in the group when You have acquired a transaction / started a transaction.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class  Transaction:Group
    {        
        private readonly SharedGroup _sharedGroup;//pointer to the shared group this transaction is handling

        //todo : unit test that checks what happen if the user just creats a transaction object and starts using it
        /*
        private Transaction()//hide this constructor from others
        {
            throw new InvalidOperationException("Transactions can only be created through sharedgroup beginread or beginwrite calls");
        }
         
        */
        //transaction inherits from Group. 

        //finalization and disposal is handled in different ways : (Transactions are by far the most complicated scenario reg. wrappers in the C# binding)
        //You need to be totally clear on the difference between finalize and dispose if You want to understand the following text

        //disposal and finalization:
        //transaction dispose will be handled in the Transaction class and will close or roll back the transaction
        //transaction dispose will typicall be called when the transaction goes out of scope in a using statement,
        //or be called directly from the user, in case using is too complicated.
        //or be called by the binding in case the user uses lambda expressions or executeintransaction
        //in any way, dispose(true) is called from the user thread always,never from a finalizer, and will roll back writes, and end-read reads

        //shared group dispose will close any outstanding transactions before the group is disposed. This (sg dispose) will also only
        //be called in the user thread, from using or directly by the user.

        //if the user first calls sharedgroup dispose, then transaction dispose, he will get an error exception stating that the shared group is
        //invalid, has been disposed. This is no problem, we do not upset core with illegal calls.
        
        //if the shared group goes out of scope without dispose being called, the transaction handle dispose will not be called
        //While the shared group does not have a finalizer, the sharedgrouphandle do have one,and the handler's finalizer will call delete
        //on the shared group.

        //core is built such that any ongoing transaction is automatically rolled back if the shared group delete is called.
        //so letting a shared group go out of scope will close it down safely, and the close down will roll back any write transactions
        //and end any read transactions
        //however, the sharedgroup handle do have information about any outstanding transactions and their type, so it could call
        //end-read or rollback if active transactions exist

        //if the TransactionHandle is not referenced anymore (meaning that a Transaction is not referenced anymore), 
        //we might be in a situation where the user has leaked a transaction
        //either the shared group the transaction originates from has been disposed or finalized (then the transaction is already rolled back)
        //or the shared group is still referenced and working.
        //The user will then get an error when he later tries to start a transaction with the same shared group.
        //the user can fix the problem in several ways
        //-he can dispose the sharedgroup or let it go out of scope and wait for the finalizer to rollback the leaked transaction
        //-he can call a transaction ending method on the shared group, this will then finish the transaction,
        //and have the shared group ready for more work

        //As seen above, we have decided not to close transactions if they are not referenced anymore.
        //If we try to close a leaked transaciton in a transaction finalizer we will run into several problems, concurrent calls into shared
        //group being one of them. the SG could be running methods in the user thread at the same time.       

        //we have to make sure that if core was called to start a transaction, then we will have enough information to end it again.
        //especially, if core gives us a group handle back, and we then run into exceptions before the transaction class is built,
        //we must stil be able to close that transaction, using the still functioning shared group
        //this means that we very unfortunately somehow need to atomically set both the transactiongroup handle AND the kind field in the
        //sharedgrouphandle (because we would like the shared group to be able to close the transaction even if
        //our transaction C# class construction went bad due to e.g. out-of-band exceptions)
        //The critical information is in fact what kind of transaction we are doing (and that we are doing a transaction at all)

        //why we need this information is:
        //if the user still have a functioning sharedgroup, he must be able to call a transaction ending method, and to give that
        //support, the sharedgroup must know a transaction is active, and what kind.

        //Later, if the sharedgroup handle gets disposed or finalized we have no problem. Core will end the transaction if we delete the sg.
        //we will of course need to invalidate the shared group C# wrapper

        //the *important* thing is that after the transaction starting call into core succeeds, we need a sharedgroup 
        //where transaction kind is set to read or write transaction so that we know that the sharedgroup is in a transaction 
        //and should not start a new one.

        //this calls for the sharedgroup handle being responsible for the transaction state of the shared group

        //so the sequence is :
        //user starts a transaction
        //---critical start
        //we call core and get ourself a group handle (that does not ever need to be unbound so we can be lazy about that)
        //SG handle must note the kind of transaction started (because we must be able to end that transaction we started)
        //---critical end
        //we create a transaction C# object, set its group handle and return it to the used
        //out-of-band exceptions can happen after critical end - but that is no problem, at that time the sg C# wrapper
        //has had its sharedgroup wrapper state updated.
        
        //user experience is (pseudo)
        //trans=SharedGroup.BeginRead()
        //BANG! out of memory while JIT'ing or some such -noone knows exactly how far the code ran.
        //now, some containing code has a catch and the user have a sharedgroup but don't know if the trans did go through or
        //not.. but the user can inspec SharedGroup and close a transaction if one is open.
        //and if the user looses the SharedGroup object too, it will clean up eventually (when GC gets around to it)
        //if the user did his code correctly, the ShareedGroup will be guareded by using, and will close down the transaction
        //(if / as soon as) the exception leaves the scope of the SharedGroup

        internal Transaction(GroupHandle groupHandle,SharedGroup sharedGroup)
            : base(sharedGroup.State==TransactionState.Read)
        {            
            SetHandle(groupHandle,sharedGroup.State==TransactionState.Read);//the group's dispose should rollback or endread
            _sharedGroup = sharedGroup;            
        }

        //called by the user directly or indirectly via dispose. Finalizer in SharedGroupHandle might also end
        //a transaction, but using its own code
        //A transaction class does not have a finalizer. Leaked transaction objects will result in open transactions
        //until the user explicitly call close transaction on the shared group, or disposes the shared group
        //note that calling EndTransaction when there is no ongoing transaction will not create any problems. It will be a NoOp
        private void EndTransaction(Boolean commit)
        {
            try
            {
                if (_sharedGroup.State == TransactionState.Read)
                {
                    UnsafeNativeMethods.SharedGroupEndRead(_sharedGroup.SharedGroupHandle);//read transactions are always comitted
                }
                else if(_sharedGroup.State==TransactionState.Write)
                {
                    if (commit)
                    {
                        UnsafeNativeMethods.SharedGroupCommit(_sharedGroup.SharedGroupHandle);   
                    }
                    else
                    {
                        UnsafeNativeMethods.SharedGroupRollback(_sharedGroup.SharedGroupHandle);                        
                    }
                }
               _sharedGroup.State= TransactionState.Ready;
                
            }
            catch (Exception) //something unexpected and bad happened, the shared group and the group should not be used anymore
            {
                _sharedGroup.Invalid = true;//mark the shared group as invalid
                Invalid = true;//mark the transaction group as invalid
                throw;
            }
        }

        /// <summary>
        /// Finish the transaction and discard any changes made while in the transaction.
        /// </summary>
        public void Rollback()
        {
            EndTransaction(false);
        }

        /// <summary>
        /// Finish the transaction and keep any changes made while in the transaction.
        /// </summary>
        new public void Commit() //we use new because we do not want to call the group.commit method on group. Transaction.commit has another meaning
        {
            EndTransaction(true);
        }


        /// <summary>
        /// this is a bit of a hack. A transaction is a group. The group pointer should NOT be
        /// freed as it is in fact a group inside a shared group, and this group is managed by
        /// the shared group.
        /// When we get here, the transaction is being disposed.
        /// When we are being disposed, we have been thrown an exception if we are InTransaction, so roll back
        /// </summary>
        protected override void ReleaseHandle()
        {
            if (_sharedGroup == null) return; //we simply cannot rollback if shared group is null
            if (_sharedGroup.State==TransactionState.InTransaction)
                Rollback();
            //base.ReleaseHandle();//group.releasehandle would release the group handle in c++ but we don't want that
        }

        

        /// <summary>
        /// Enhance toString to also include the type of transaction, and its current state
        /// </summary>
        public override string ToString()
        {
            return base.ToString() +String.Format(CultureInfo.InvariantCulture, "Type:{0}  State:{1}", _sharedGroup.TransactionKind,_sharedGroup.State);
        }
    }
}
