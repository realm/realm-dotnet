using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace RealmNet
{
    public class Realm : Handled
    {
        public static ICoreProvider ActiveCoreProvider;
        [ThreadStatic] static Transaction ActiveTransaction;  

        public static Realm GetInstance(string path = null)
        {
            return new Realm(ActiveCoreProvider, path);
        }

        internal static Realm RealmWithActiveTransactionThisTread()
        {
            return ActiveTransaction?._realm;
        }

        internal static void ForgetActiveTransactionThisTread()
        {
            ActiveTransaction = null;  // transaction Dispose should reset its realm's state to Ready
        }

        private readonly ICoreProvider _coreProvider;

        private ISharedGroupHandle SharedGroupHandle => Handle as ISharedGroupHandle;
        private IGroupHandle _transactionGroupHandle;

        private Realm(ICoreProvider coreProvider, string path) 
        {
            _coreProvider = coreProvider;
            SetHandle(coreProvider.CreateSharedGroup(path), false);
        }

        // TODO consider retiring this in favor of just creating object instances
        public T CreateObject<T>() where T : RealmObject
        {
            return (T)CreateObject(typeof(T));
        }


        public object CreateObject(Type objectType)
        {
            // relies on RealmObject ctor calling back to AdoptNewObject
            return (RealmObject)Activator.CreateInstance(objectType);
        }

        internal void AdoptNewObject(RealmObject adoptingObject)
        {
            AdoptNewObject(adoptingObject, this, _coreProvider, _transactionGroupHandle);
        }

        internal static void AdoptNewObject(RealmObject adoptingObject, Realm usingRealm, ICoreProvider usingProvider, IGroupHandle transGroupHandle)
        {
            var objectType = adoptingObject.GetType();
            if (!usingProvider.HasTable(transGroupHandle, objectType.Name))
                CreateTableFor(objectType, usingProvider, transGroupHandle);
            var rowHandle = usingProvider.AddEmptyRow(transGroupHandle, objectType.Name);
            adoptingObject._Manage(usingRealm, usingProvider, rowHandle);
        }

        private static void CreateTableFor(Type objectType, ICoreProvider usingProvider, IGroupHandle transGroupHandle)
        {
            var tableName = objectType.Name;

            if (!objectType.GetCustomAttributes(typeof(WovenAttribute), true).Any())
                Debug.WriteLine("WARNING! The type " + tableName + " is a RealmObject but it has not been woven.");

            usingProvider.AddTable(transGroupHandle, tableName);

            var propertiesToMap = objectType.GetProperties().Where(p => p.GetCustomAttributes(false).All(a => !(a is IgnoreAttribute)));
            foreach (var p in propertiesToMap)
            {
                var propertyName = p.Name;
                var mapToAttribute = p.GetCustomAttributes(false).FirstOrDefault(a => a is MapToAttribute) as MapToAttribute;
                if (mapToAttribute != null)
                    propertyName = mapToAttribute.Mapping;
                
                var columnType = p.PropertyType;
                usingProvider.AddColumnToTable(transGroupHandle, tableName, propertyName, columnType);
            }
        }


        public void Add(RealmObject adoptingObject)
        {
            if (State == TransactionState.Read)  // TODO debate if track "implicit read transactions" differently and allow this
                throw new InvalidOperationException("You are in a Read transaction and cannot add objects");

            if (State == TransactionState.Write)
            {
                // MOST EFFICIENT - object should have picked up this state and already assigned itself to the core provider
                Debug.Assert(adoptingObject.InRealm);
                // this is effectively a noop but it will be intuitive to users and is a good chance to check transaction validitity
            }
            else
            {
                Debug.Assert(adoptingObject.IsStandalone);
                var oneShot = BeginWrite();  // implicit one-shot Write transaction
                var tableName = adoptingObject.GetType().Name;
                adoptingObject._Manage(this, _coreProvider, _coreProvider.AddEmptyRow(_transactionGroupHandle, tableName));
                oneShot.Commit();
            }
        }

        public RealmQuery<T> All<T>()
        {
            return new RealmQuery<T>(this, _coreProvider);
        }

        internal TransactionState State
        {
            get { return SharedGroupHandle.State; }
//            set { SharedGroupHandle.State = value; }
        }

        internal IGroupHandle TransactionGroupHandle => _transactionGroupHandle;


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
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public Transaction BeginRead()
        {
           ValidateNotInTransaction();
           _transactionGroupHandle = SharedGroupHandle.StartTransaction(TransactionState.Read);//SGH.StartTransaction is atomic reg. transaction state and calling core
           if (_transactionGroupHandle.IsInvalid)
               throw new InvalidOperationException("Cannot start Read Transaction, probably an IO error with the SharedGroup file");
           return new Transaction(_transactionGroupHandle, this);
        }

        //this is the only place where a write transaction can be initiated
        /// <summary>
        /// Initiate a write transaction by returning a Transaction that is also a Group.
        /// You can then modify the tables in the group exclusively. Your modifications will not be visible to readers simultaneously 
        /// reading data from the database - until you do transaction.commit(). At that point any new readers will see the updated database,
        /// existing readers will continue to see their copy of the database as it was when they started their read transaction.
        /// Only one writer can exist at a time, so if you call BeginWrite the function might wait until the prior writer do a commit()
        /// </summary>
        /// <returns>Transaction object that inherits from Group and gives read/write acces to all tables in the group</returns>
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public Transaction BeginWrite()
        {
            // TODO cope with multiple write transactions FROM DIFFERENT REALMS in same thread - issue #85
            ValidateNotInTransaction();
            _transactionGroupHandle = SharedGroupHandle.StartTransaction(TransactionState.Write);
            if (_transactionGroupHandle.IsInvalid)
                throw new InvalidOperationException("Cannot start Write Transaction, probably an IO error with the SharedGroup file");
            var ret = new Transaction(_transactionGroupHandle, this);
            ActiveTransaction = ret;  // static PER THREAD
            return ret;
        }

        //called by the user directly or indirectly via dispose. Finalizer in SharedGroupHandle might also end
        /// <summary>
        /// defaults to true
        /// if Isvalid is false something fatal has happened with the shared group wrapper        
        /// </summary>
        public bool IsValid
        {
            get { return (!SharedGroupHandle.IsInvalid); }
            private set
            {
                //ignore calls where we are set to true - only Handle can set itself to true
                if (value == false)
                {
                    SharedGroupHandle.Dispose();
                        //this is a safe way to invalidate the handle. Any ongoing transactions will be rolled back
                }
            }
        }

        //a transaction, but using its own code
        //A transaction class does not have a finalizer. Leaked transaction objects will result in open transactions
        //until the user explicitly call close transaction on the shared group, or disposes the shared group
        //note that calling EndTransaction when there is no ongoing transaction will not create any problems. It will be a NoOp
        internal void EndTransaction(bool commit)
        {
            try
            {
                switch (State)
                {
                    case TransactionState.Read:
                        SharedGroupHandle.SharedGroupEndRead();
                        break;
                    case TransactionState.Write:
                        if (commit)
                        {
                            SharedGroupHandle.SharedGroupCommit();
                        }
                        else
                        {
                            SharedGroupHandle.SharedGroupRollback();                        
                        }
                        break;
                }                
            }
            catch (Exception) //something unexpected and bad happened, the shared group and the group should not be used anymore
            {
                IsValid = false;//mark the shared group as invalid
                throw;
            }
        }

        public void Remove(RealmObject obj)
        {
            _coreProvider.RemoveRow(_transactionGroupHandle, obj.GetType().Name, obj.RowHandle);
        }


        /// <summary>
        /// True if this SharedGroup is currently in a read or a write transaction
        /// </summary>
        /// <returns></returns>
        public bool InTransaction()
        {
            return (State == TransactionState.Read||
                    State == TransactionState.Write);
        }
                 
        private void ValidateNotInTransaction()
        {
            if (InTransaction()) throw new InvalidOperationException("SharedGroup Cannot start a transaction when already inside one");
        }
    }
}
