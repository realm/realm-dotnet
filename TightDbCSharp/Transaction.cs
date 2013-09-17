using System;
using System.Globalization;

namespace TightDbCSharp
{
    //Using our handled pattern but do not as such have a handle to a C# class that must be freed.
    //instead represents a started transaction and the dispose phase is used to ensure that
    //a rollback is done, if the transaction has not been explicitly comitted earlier on
    public enum TransactionKind
    {
       Read,
       Write
    }
    
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
        internal Transaction(IntPtr groupHandle,SharedGroup sharedGroup, TransactionKind transactionKind)
            : base(transactionKind==TransactionKind.Read)
        {
            _kind = transactionKind;
            SetHandle(groupHandle,true);//method in handled
            _sharedGroup = sharedGroup;
            //ReadOnly = transactionType == TransactionType.Read;//readonly in group
        }

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

        public void Rollback()
        {
            EndTransaction(false);
        }

        public void Commit()
        {
            EndTransaction(true);
        }

        //depricated
        //added bc java has one named this way. Commit simply ends the current transaction in a non-
        //catastrophic way, no matter what kind of transaction it is.
        /*
        public void EndRead()
        {
            Commit();
        }
        */
        //this is a bit of a hack. A transaction is a group. The group pointer should NOT be
        //freed as it is in fact a group inside a shared group, and this group is managed by
        //the shared group
        protected override void ReleaseHandle()
        {
            if (_sharedGroup != null) {//we simply cannot rollback if shared group is null
            if (_sharedGroup.TransactionState==TransactionState.InTransaction)
                Rollback();
            }
          //base.ReleaseHandle();//group.releasehandle would release the group handle in c++ but we don't want that
        }

        internal override string ObjectIdentification()
        {
            return String.Format(CultureInfo.InvariantCulture, "Transaction. Type:{0}  State:{1}", _kind,
                _sharedGroup.TransactionState);
        }
    }
}
