using System;
using System.Globalization;

namespace TightDbCSharp
{
    /// <summary>
    /// Read:read transacton.
    /// Write:write transaction.
    /// 
    /// Read transactions cannot modify table values or schema.
    /// Write transactions can. A transaction will read the table as it were when it started.
    /// There can only be one write transaction (but many readers)
    /// </summary>
    public enum TransactionKind
    {
       /// <summary>
       /// Indicates a Read Transaction
       /// </summary>
       Read,
       /// <summary>
       /// Indicates a Write Transaction
       /// </summary>
       Write
    }
    
    /// <summary>
    /// Transaction class.
    /// Is returned by SharedGroup when starting a read or write transaction.
    /// The transaction is inherited from Group, so You can manipulate all tables
    /// in the group when You have acquired a transaction / started a transaction.
    /// </summary>
    public class  Transaction:Group
    {        
        private readonly SharedGroup _sharedGroup;//pointer to the shared group this transaction is handling
        private readonly TransactionKind _kind;              

        //todo : unit test that checks what happen if the user just creats a transaction object and starts using it
        /*
        private Transaction()//hide this constructor from others
        {
            throw new InvalidOperationException("Transactions can only be created through sharedgroup beginread or beginwrite calls");
        }
        */
        //transaction inherits from Group. 
        //however, the Group.handle is not coming from a new group call in c++
        //but from sharedgroup.readtransaction or sharedgroup.writetransaction
        //therefore we have to tell base not to allocate a group object in c++
        //and then set the handle for the group that was created by sharedgroup
        //this is done by calling a specific constructor in base that does not
        //allocate a new c++ object for the group
        //the group returned is memory handled in c++ and we should NOT call c++
        //and ask for deallocation
        internal Transaction(IntPtr groupHandle,SharedGroup sharedGroup, TransactionKind transactionKind)
            : base(transactionKind==TransactionKind.Read)
        {
            _kind = transactionKind;
            SetHandle(groupHandle,true,transactionKind==TransactionKind.Read);//Shouldbedisposed true because the releasehandle method is used to commit or rollback transactions
            _sharedGroup = sharedGroup;
            //ReadOnly = transactionType == TransactionType.Read;//readonly in group
        }

        /// <summary>
        /// Returns if this transaction is read only, or if it can modify the group and its tables
        /// </summary>
        public TransactionKind Kind
        {
            get { return _kind; }
        }

        private void EndTransaction(Boolean commit)
        {
            try
            {
                if (_kind == TransactionKind.Read)
                {
                    UnsafeNativeMethods.SharedGroupEndRead(_sharedGroup);
                }
                else
                {
                    if (commit)
                    {
                        UnsafeNativeMethods.SharedGroupCommit(_sharedGroup);   
                    }
                    else
                    {
                        UnsafeNativeMethods.SharedGroupRollback(_sharedGroup);                        
                    }
                }
               _sharedGroup.TransactionState= TransactionState.Ready;
                
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
        public void Commit()
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
            if (_sharedGroup.TransactionState==TransactionState.InTransaction)
                Rollback();
            //base.ReleaseHandle();//group.releasehandle would release the group handle in c++ but we don't want that
        }

        internal override string ObjectIdentification()
        {
            return String.Format(CultureInfo.InvariantCulture, "Transaction. Type:{0}  State:{1}", _kind,
                _sharedGroup.TransactionState);
        }
    }
}
