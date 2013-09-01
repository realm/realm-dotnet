using System;
using System.Globalization;

namespace TightDbCSharp
{
    public enum DurabilityLevel
    {
        DurabilityFull,
        DurabilityMemoryOnly
    }

    //Never : this sharedgroup has never finished a transaction, neither started one
    //InTransaction : A read or write transaction is active
    //Finished : A read or write transaction has finished, and the shared group is ready
    //to start a new transaction and go into InTransaction mode
    internal enum TransactionState : byte
    {              
        Ready,
        InTransaction
    }

    public class SharedGroup : Handled
    {

        public SharedGroup(String fileName, Boolean noCreate, DurabilityLevel durabilityLevel)
        {
            UnsafeNativeMethods.NewSharedGroupFile(this, fileName, noCreate, durabilityLevel);
        }


        
        //creates an empty shared group. Usually You will use SharedGroup(string filename,boolean NoCreate, Durabilitylevel durabilitylevel)
//        public SharedGroup()
 //       {
  //          UnsafeNativeMethods.NewSharedGroupUnattached(this); //calls sethandle itself
   //     }

        /*depricated - specify file when creating the class instead
        public void Open(string fileName, bool noCreate, DurabilityLevel durabilityLevel)
        {
            UnsafeNativeMethods.SharedGroupOpen(this,fileName,noCreate,durabilityLevel);
        }
        */

        protected override void ReleaseHandle()
        {
            UnsafeNativeMethods.SharedGroupDelete(this);
        }


        
        public Boolean IsAttached
        {
            get { return UnsafeNativeMethods.SharedGroupIsAttached(this); }
        }

        //in case of unexpected exceptions thrown at unexpected times, the shared group will be invalidated
        //for instance if a commit or rollback throws an exception
        //defaults to false
        public Boolean Invalid { get; set; }

        internal TransactionState TransactionState {get; set;}

        private void ValidateNotInTransaction()
        {
            if (TransactionState==TransactionState.InTransaction) throw new InvalidOperationException("SharedGroup Cannot start a transaction when already inside one");
        }

        //this is the only place where a read transaction can be initiated
        public Transaction BeginRead()
        {
            ValidateNotInTransaction();
           TransactionState = TransactionState.InTransaction;
           return UnsafeNativeMethods.SharedGroupBeginRead(this);//will initialize a new transaction:group, set t.handle to the group returned by the sharedgroup
        }

        //this is the only place where a write transaction can be initiated
        public Transaction BeginWrite()
        {
            ValidateNotInTransaction();
            TransactionState = TransactionState.InTransaction;
            return UnsafeNativeMethods.SharedGroupBeginWrite(this);
        }

        //todo:unit test with two threads - create SG, check it has not changed. create thread and run that thread (it then updates the sg) and then await it, and when it is finishe,
        //todo:finally check to see if the SG has changed.
        //note - throws if accessed in a SharedGroup that is not attached yet.
        public Boolean HasChanged
        {
            get
            {
                if (IsAttached)
                {
                    return UnsafeNativeMethods.SharedGroupHasChanged(this);
                }
                throw new InvalidOperationException("Shared Group.Has Changed cannot be accessed on unattached Shared Group");
            }
        }

        public override string ObjectIdentification()
        {
            return string.Format(CultureInfo.InvariantCulture, "SharedGroup:({0:d}d)  ({1}h)", Handle,
                Handle.ToString("X"));
        }

        public static IntPtr DurabilityLevelToIntPtr(DurabilityLevel durabilityLevel)
        {
            switch (durabilityLevel)
            {
                default:
                    return (IntPtr) 0;
                case DurabilityLevel.DurabilityMemoryOnly:
                    return (IntPtr) 1;
            }
        }

        //work is the acutal code that will be run inside the transaction
        private void Transaction(Action<Transaction> work,TransactionKind kind)
        {
            using (var transaction = (kind == TransactionKind.Read) ? BeginRead() : BeginWrite())
            try
            {
                work(transaction);
                transaction.Commit();
            }
            catch (Exception )
            {
                transaction.Rollback();//will likely be called again when transaction goes out of scope        
                throw;
            }
        }

        public void ExecuteInReadTransaction(Action<Group> work)
        {
            Transaction(work, TransactionKind.Read);
        }

        public void ExecuteInWriteTransaction(Action<Group> work)
        {
            Transaction(work, TransactionKind.Write);
        }
    }


}
