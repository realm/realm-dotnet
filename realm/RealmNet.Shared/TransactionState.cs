namespace RealmNet
{
    /// <summary>
    ///Transaction state..
    ///Ready : The Shared Group is not in a transaction
    ///Read  : A read transaction is active
    ///Write : A write transaction is active
    /// Also used as a paramenter to ExecuteInTransaction to designate what kind of trasnaction to execute
    /// 
    /// Note that no significant performance gain is achieved by using a data type that is smaller than Int32.
    /// from http://msdn.microsoft.com/en-us/library/ms182147.aspx
    /// todo:performance test if changing to byte gives any measureable performance or size gains. CA1028 recommends against using byte instead of int32
    /// </summary>
    public enum TransactionState 
    {              
        /// <summary>
        /// Ready:A shared group not in a transaction is Ready
        /// </summary>
        Ready,
        /// <summary>
        /// Read:A shared group in a read transaction
        /// </summary>
        Read,
        /// <summary>
        /// Write:A shared group in a write transaction
        /// </summary>
        Write,
    }
}
