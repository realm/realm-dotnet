/* Copyright 2016 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
using System;
using System.Runtime.InteropServices;

namespace Realms
{
    // An AsyncQueryCancellationToken in object-store references a Results object.
    // We need to mirror this same relationship here.
    internal class AsyncQueryCancellationTokenHandle : RealmHandle
    {
        internal AsyncQueryCancellationTokenHandle(ResultsHandle root) : base(root)
        {
        }

        protected override void Unbind()
        {
            IntPtr managedResultsHandle = NativeResults.destroy_async_query_cancellation_token(handle);
            GCHandle.FromIntPtr(managedResultsHandle).Free();
        }
    }
}

