using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TightDbCSharp;

namespace TightDbCSharp
{
    //Using our handled pattern but do not as such have a handle to a C# class that must be freed.
    //instead represents a started transaction and the dispose phase is used to ensure that
    //a rollback is done, if the transaction has not been explicitly comitted earlier on
    public enum TransactionType : byte
    {
       Read,
       Write
    }
    
    public class  Transaction:Group
    {
        public SharedGroup SharedGroup;//pointer to the shared group this transaction is handling
        public TransactionType Type;              

        private Transaction()//hide this constructor from others
        {
            throw new InvalidOperationException("Transactions can only be created through sharedgroup beginread or beginwrite calls");
        }

        //transaction inherits from Group. 
        //however, the Group.handle is not coming from a new group call in c++
        //but from sharedgroup.readtransaction or sharedgroup.writetransaction
        //therefore we have to tell base not to allocate a group object in c++
        //and then set the handle for the group that was created by sharedgroup
        //this is done by calling a specific constructor in base that does not
        //allocate a new c++ object for the group
        internal Transaction(IntPtr groupHandle,SharedGroup sharedGroup, TransactionType transactionType)
            : base(transactionType==TransactionType.Read)
        {
            Type = transactionType;
            SetHandle(groupHandle,true);//method in handled
            SharedGroup = sharedGroup;
            //ReadOnly = transactionType == TransactionType.Read;//readonly in group
        }


        internal void Init(SharedGroup sharedgroup, TransactionType type)
        {
            SharedGroup = sharedgroup;
            Type = type;            
        }

        internal void EndTransaction(Boolean Commit)
        {
            try
            {
                if (Type == TransactionType.Read)
                {
                    UnsafeNativeMethods.SharedGroupEndRead(SharedGroup);
                }
                else
                {
                    if (Commit)
                    {
                        UnsafeNativeMethods.SharedGroupCommit(SharedGroup);   
                    }
                    else
                    {
                        UnsafeNativeMethods.SharedGroupRollback(SharedGroup);                        
                    }
                }
               SharedGroup.TransactionState= TransactionState.Ready;
                
            }
            catch (Exception) //something unexpected and bad happened, the shared group and the group should not be used anymore
            {
                SharedGroup.Invalid = true;//mark the shared group as invalid
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
        internal override void ReleaseHandle()
        {
            if (SharedGroup != null) {//we simply cannot rollback if shared group is null
            if (SharedGroup.TransactionState==TransactionState.InTransaction)
                Rollback();
            }
          //base.ReleaseHandle();//group.releasehandle would release the group handle in c++ but we don't want that
        }

        public override string ObjectIdentification()
        {
            return String.Format(CultureInfo.InvariantCulture, "Transaction. Type:{0}  State:{1}", Type,
                SharedGroup.TransactionState);
        }
    }
}
