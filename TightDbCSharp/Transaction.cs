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

        //shared group dispose will 
        //call SharedGroupHandle dispose, and that wil close any outstanding transactions before the SG is disposed. 
        //This (sg dispose) will also only be called in the user thread, from using or directly by the user.

        //if the user first calls sharedgroup dispose, then transaction dispose, he will get an error exception stating that the shared group is
        //invalid, has been disposed. This is no problem, we do not upset core with illegal calls.
        
        //if the transaction goes out of scope without dispose being called, the transaction handle dispose will not be called, nor will the transaction
        //finalizer be called (there is no one). The transactionhandle finalizer is disabled on construction so that won't be called either.

        //While the shared group does not have a finalizer, the sharedgrouphandle do have one,and the handler's finalizer will close transactions, then
        //call delete on the shared group.

        //core is planned to be built such that any ongoing transaction is automatically rolled back if the shared group delete is called.
        //until then, sharedgrouphandle will clean up leaked transactions on dispose and finalize.

        //if the TransactionHandle is not referenced anymore (meaning that a Transaction is not referenced anymore), 
        //we might be in a situation where the user has leaked a transaction.

        //either the shared group the transaction originates from has been disposed or finalized (then the transaction is already rolled back)
        //or the shared group is still referenced and working.

        //In any case, until the transaction has been closed, The user will  get an error when he later tries to start a transaction with the same shared group.
        //the user can fix the problem in several ways
        //-he can call a transaction ending method on the shared group, this will then finish the transaction,
        //and have the shared group ready for more work

        //-he can dispose the sharedgroup or let it go out of scope and wait for the finalizer to rollback the leaked transaction

        //As seen above, we have decided not to close transactions if they get finalized

        //we have to make sure that if core was called to start a transaction, then we will have enough information to end it again.
        //especially, if core gives us a group handle back, and we then run into exceptions before the transaction class is built,
        //we must stil be able to close that transaction, using the still functioning shared group.
        //these requrements are met by the sharedgrouphandle storing the transaction state - the sharedgroup handle can close down outstanding
        //transactions from the instant moment we get a group handle from core in a transaction starting operation
        //see SharedGroupHandle.cs for more info.

        internal Transaction(GroupHandle groupHandle,SharedGroup sharedGroup)
            : base(sharedGroup.State==TransactionState.Read)
        {            
            SetHandle(groupHandle,sharedGroup.State==TransactionState.Read);//the group's dispose should rollback or endread
            _sharedGroup = sharedGroup;            
        }


        /// <summary>
        /// The state of the transaction.(Actually the state of the shared group)
        ///  Read or Write. If it is Ready, the transaction has already been comitted.
        /// See TransactionState 
        /// </summary>
        public TransactionState State {get { return _sharedGroup.State; }}

        /// <summary>
        /// Finish the transaction and discard any changes made while in the transaction.
        /// </summary>
        public void Rollback()
        {
            _sharedGroup.EndTransaction(false);
        }

        /// <summary>
        /// Finish the transaction and keep any changes made while in the transaction.
        /// </summary>
        new public void Commit() //we use new because we do not want to call the group.commit method on group. Transaction.commit has another meaning
        {
            _sharedGroup.EndTransaction(true);
        }


        /// <summary>
        /// this is a bit of a hack. A transaction is a group. The group pointer should NOT be
        /// freed as it is in fact a group inside a shared group, and this group is managed by
        /// the shared group.
        /// When we get here, the transaction is being disposed.
        /// When we are being disposed, we have been thrown an exception if we are InTransaction, so roll back
        /// </summary>
        protected override void Dispose(Boolean disposing)
        {
            if (!disposing) return;
            if (_sharedGroup == null) return; //we simply cannot rollback if shared group is null
            if (_sharedGroup.InTransaction())
                Rollback();//if this fails somehow, the SharedGroupHandle will take care in its finalizer. Let the user handle exceptions
        }
        

        /// <summary>
        /// Enhance toString to also include the type of transaction, and its current state
        /// </summary>
        public override string ToString()
        {
            return base.ToString() +String.Format(CultureInfo.InvariantCulture, " State:{0}", _sharedGroup.State);
        }
    }
}
