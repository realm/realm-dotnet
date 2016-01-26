/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
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
        private SharedRealmHandle _sharedRealmHandle;
        private bool _isOpen;

        internal Transaction(SharedRealmHandle sharedRealmHandle)
        {
            this._sharedRealmHandle = sharedRealmHandle;
            NativeSharedRealm.begin_transaction(sharedRealmHandle);
            _isOpen = true;
        }

        /// <summary>
        /// Will automatically <c>Rollback</c> the transaction on existing scope, if not explicitly Committed.
        /// </summary>
        public void Dispose()
        {
            if (!_isOpen)
                return;

            //var exceptionOccurred = Marshal.GetExceptionPointers() != IntPtr.Zero || Marshal.GetExceptionCode() != 0;
            var exceptionOccurred = true; // TODO: Can we find this out on iOS? Otherwise, we have to remove it!
            if (exceptionOccurred)
                Rollback();
            else
                Commit();
        }

        /// <summary>
        /// Use explicitly to undo the changes in a transaction, otherwise it is automatically invoked by exiting the block.
        /// </summary>
        public void Rollback()
        {
            if (!_isOpen)
                throw new Exception("Transaction was already closed. Cannot roll back");

            NativeSharedRealm.cancel_transaction(_sharedRealmHandle);
            _isOpen = false;
        }

        /// <summary>
        /// Use to save the changes to the realm. If transaction is declared in a <c>using</c> block, must be used before the end of that block.
        /// </summary>
        public void Commit()
        {
            if (!_isOpen)
                throw new Exception("Transaction was already closed. Cannot commit");

            NativeSharedRealm.commit_transaction(_sharedRealmHandle);
            _isOpen = false;
        }
    }
}
