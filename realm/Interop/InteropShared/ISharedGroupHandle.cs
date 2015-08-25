namespace RealmNet.Interop
{
    public interface ISharedGroupHandle : IRealmHandle {
        IGroupHandle StartTransaction(TransactionState read);
        void SharedGroupCommit();

        /// <summary>
        /// This will roll back any write transaction this shared group handle have active.
        /// There is no validation, if You call this method and there is no active write transaction, exceptions or crash could happen
        /// The binding will never call this method unless there is an ongoing write transaction
        /// </summary>
        /// <exception cref="System.InvalidOperationException"></exception>
        void SharedGroupRollback();

        void SharedGroupEndRead();

        /// <summary>
        /// Store the transaction state of this c++shared group
        /// The state will change atomically with the state in c++,
        /// using CER and CriticalHandle guarentees of non-interference from out-of-band exceptions
        /// This is important because if we have a write transaction in c++ but does not know about it in C#
        /// then we could block the database until the program is restarted, 
        /// the state of an ongoing transaction is a kind of handle that could be kind of leaked
        /// </summary>
        TransactionState State { get; }
    }
}