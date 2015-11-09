using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RealmNet
{
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

        public void Rollback()
        {
            if (!_isOpen)
                throw new Exception("Transaction was already closed. Cannot roll back");

            NativeSharedRealm.cancel_transaction(_sharedRealmHandle);
            _isOpen = false;
        }

        public void Commit()
        {
            if (!_isOpen)
                throw new Exception("Transaction was already closed. Cannot commit");

            NativeSharedRealm.commit_transaction(_sharedRealmHandle);
            _isOpen = false;
        }
    }
}
