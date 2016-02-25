/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
/// PROXY VERSION OF CLASS USED IN PCL FOR BAIT AND SWITCH PATTERN 
 
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Realms
{
    /// <summary>
    /// Provides a scope to safely read and write to a Realm. Must use explicitly via Realm.BeginWrite.
    /// </summary>
    /// <remarks>
    /// All access to a Realm occurs within a Transaction. Read transactions are created implicitly.
    /// </remarks>
    public class Transaction : IDisposable
    {

        internal Transaction() {}
        
        
        /// <summary>
        /// Will automatically <c>Rollback</c> the transaction on existing scope, if not explicitly Committed.
        /// </summary>
        public void Dispose()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        /// <summary>
        /// Use explicitly to undo the changes in a transaction, otherwise it is automatically invoked by exiting the block.
        /// </summary>
        public void Rollback()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        /// <summary>
        /// Use to save the changes to the realm. If transaction is declared in a <c>using</c> block, must be used before the end of that block.
        /// </summary>
        public void Commit()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }
    }
}
