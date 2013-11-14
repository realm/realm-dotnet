using System;
using System.Globalization;

namespace TightDbCSharp
{
    /// <summary>
    /// By default the shared group will be fully durable, so that each commit writes its changes
    /// to disk in an atomic manner that guarantees that the file is always consistent.
    /// If your data is transient, and does not need to persist to disk
    /// (like for caching or shared state between processes). 
    /// You can open the shared group in mem-only mode. 
    /// Then the file will just be used for identification and backing and will be removed again
    ///  when there are no more processes using it.    
    /// </summary>
    public enum DurabilityLevel
    {
        /// <summary>
        /// each commit writes its changes
        /// to disk in an atomic manner that guarantees that the file is always consistent.
        /// </summary>
        DurabilityFull,
        /// <summary>
        /// the file will just be used for identification and backing and will be removed again
        ///  when there are no more processes using it.  
        /// </summary>
        DurabilityMemoryOnly,
        /// <summary>
        /// Currently not supported on windows.
        /// Will save data using a background thread. 
        /// Write transactions will return much faster, data throughput is much larger, but 
        /// no guarentee than a commit is actaully written to a file when the calls return        
        /// </summary>
        DurabilityAsync 
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

    /// <summary>
    /// When two threads or processes want to access the same database file, they must each create their own instance of SharedGroup.
    /// </summary>
    public class SharedGroup : Handled
    {

        /// <summary>
        /// get a shared group attached to the filename, using defaults for various settings :
        /// noCreate(currently false) and durabilityLevel(currently full)
        /// </summary>
        /// <param name="fileName">(path and)File name of sharedgroup to connect to, or sharedgroup to create </param>
        public SharedGroup(String fileName)
        {
            UnsafeNativeMethods.NewSharedGroupFileDefaults(this, fileName);
        }





        /// <summary>
        /// 
        /// Open a shared group (will be created if it does not already exist).
        /// By default the shared group will be fully durable, so that each commit writes its changes
        ///  o disk in an atomic manner that guarantees that the file is always consistent.
        /// If your data is transient, and does not need to persist to disk (like for caching or 
        /// shared state between processes). You can open the shared group in mem-only mode. 
        /// Then the file will just be used for identification and backing and will be removed again when there are no more processes using it.
        /// Note that a shared group can only be opened in the mode it was created in.
        /// A SharedGroup may also be constructed in an unattached state (2). See open() and is_attached() for more on this.
        /// </summary>
        /// <param name="fileName">Filesystem path of the TightDB database file to be opened.</param>
        /// <param name="noCreate">If set to true, IOException will be thrown if the file does not already exist.</param>
        /// <param name="durabilityLevel">Durability Level (durability_Full or durability_MemOnly)</param>
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

        /// <summary>
        /// deattach this shared group on the c++ side. Do not call explicitly unless You REALLY know what You are doing.
        /// Is called autmatically by dispose and destructor
        /// </summary>
        protected override void ReleaseHandle()
        {
            UnsafeNativeMethods.SharedGroupDelete(this);
        }


        /*
        /// <summary>
        /// A shared group may be created in the unattached state, 
        /// and then later attached to a file with a call to one of the open() methods. 
        /// Calling any method other than open(), is_attached(), and ~SharedGroup() on an unattached instance results
        ///  in undefined behavior.
        /// </summary>
        
        public Boolean IsAttached
        {
            get { return UnsafeNativeMethods.SharedGroupIsAttached(this); }
        }
        */

        //todo:implement reserve


        //todo:unit test with two threads - create SG, check it has not changed. create thread and run that thread (it then updates the sg) and then await it, and when it is finishe,
        //todo:finally check to see if the SG has changed.        
        /// <summary>
        /// This method tests if the shared group has been modified (by another process), since the last transaction.
        /// It has very little overhead and does not affect other processes, so it is ok to call it at regular intervals 
        /// (like in the idle handler of an application).
        /// </summary>
        public Boolean HasChanged
        {
            get
            {
                return UnsafeNativeMethods.SharedGroupHasChanged(this);
            }
        }

        //
        //
        //
        /// <summary>
        /// in case of unexpected exceptions thrown at unexpected times, the shared group will be invalidated
        /// for instance if a commit or rollback throws an exception
        /// defaults to false
        /// if Invalid is true something fatal has happened
        /// </summary>
        public Boolean Invalid { get; set; }

        internal TransactionState TransactionState {get; set;}

        private void ValidateNotInTransaction()
        {
            if (TransactionState==TransactionState.InTransaction) throw new InvalidOperationException("SharedGroup Cannot start a transaction when already inside one");
        }

        //this is the only place where a read transaction can be initiated
        /// <summary>
        /// initiates a Read transaction by returning a Transaction object that is also a Group object.
        /// The group object will represent the database as it was when BeginRead was executed and will stay in that state
        /// even as the database is being changed by other processes, it is a snapshot.
        /// The group returned is read only, it is illegal to make changes to it.
        /// Transaction.Commit() will dispose of the underlying structures maintaining the readonly view of the database, 
        /// the structures will also be disposed if the transaction goes out of scope.
        /// Calling commit() as soon as you are done with the transaction will free up memory a little faster than relying on dispose
        /// </summary>
        /// <returns></returns>
        public Transaction BeginRead()
        {
            ValidateNotInTransaction();
           TransactionState = TransactionState.InTransaction;
           return UnsafeNativeMethods.SharedGroupBeginRead(this);//will initialize a new transaction:group, set t.handle to the group returned by the sharedgroup
        }


        //commit is implemented in transaction


        //this is the only place where a write transaction can be initiated
        /// <summary>
        /// Initiate a write transaction by returning a Transaction that is also a Group.
        /// You can then modify the tables in the group exclusively. Your modifications will not be visible to readers simultaneously 
        /// reading data from the database - until you do transaction.commit(). At that point any new readers will see the updated database,
        /// existing readers will continue to see their copy of the database as it was when they started their read transaction.
        /// Only one writer can exist at a time, so if you call BeginWrite the function might wait until the prior writer do a commit()
        /// </summary>
        /// <returns>Transaction object that inherits from Group and gives read/write acces to all tables in the group</returns>
        public Transaction BeginWrite()
        {
            ValidateNotInTransaction();
            TransactionState = TransactionState.InTransaction;
            return UnsafeNativeMethods.SharedGroupBeginWrite(this);
        }

        //rollback is implemented in Transaction



        internal static IntPtr DurabilityLevelToIntPtr(DurabilityLevel durabilityLevel)
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
        private void ExecuteInTransaction(Action<Transaction> work,TransactionKind kind)
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

        /// <summary>
        /// supply a void that takes a transaction parameter. This void will be called with a Transaction parameter
        /// When the void finisheds the transaction will be comitted if it has not already been.
        /// Comitting the transaction will do nothing more than release some memory used to keep the historic state of the database
        /// from when the transaction was started.
        /// </summary>
        /// <param name="work"></param>
        public void ExecuteInReadTransaction(Action<Transaction> work)//Not <Group> bc .net 3.5 won't compile
        {
            ExecuteInTransaction(work, TransactionKind.Read);
        }

        //todo:ensure all the below scenarios are being unit tested - that exceptions are thrown as expected

        /// <summary>
        /// Supply a void that takes a transaction parameter. This void will be called with a transaction parameter,
        /// when the void finishes, the transaction will be COMITTED automatically. If the void wants a rollback, the void
        /// can throw an exception, or call transaction.commit().
        /// After commit has been called, it is illegal to access the transaction in any way, so it is best just to exit the
        /// void when finished with the modifications.
        /// if you roll back and then throw an exception the rollback is not rolled back
        /// if you roll back and call commit, an exception will throw
        /// if you roll back and roll back again an exception will be thrown
        /// if you commit and then throw an exception, the commit is not rolled back
        /// if you commit twice you will get an exception
        /// if you commit and roll back you will get an exception
        /// </summary>
        /// <param name="work"></param>
        public void ExecuteInWriteTransaction(Action<Transaction> work)//Not <Group> bc .net 3.5 won't compile
        {
            ExecuteInTransaction(work, TransactionKind.Write);
        }
    }


}
