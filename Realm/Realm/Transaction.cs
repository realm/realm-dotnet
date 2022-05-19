﻿////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading.Tasks;
using Realms.Helpers;

namespace Realms
{
    /// <summary>
    /// Provides a scope to safely read and write to a <see cref="Realm"/>. Must use explicitly via <see cref="Realm.BeginWrite"/>.
    /// </summary>
    /// <remarks>
    /// All access to a <see cref="Realm"/> occurs within a <see cref="Transaction"/>. Read transactions are created implicitly.
    /// </remarks>
    public class Transaction : IDisposable
    {
        private Realm _realm;

        internal Transaction(Realm realm)
        {
            _realm = realm;
        }

        /// <summary>
        /// Will automatically <see cref="Rollback"/> the transaction on existing scope, if not explicitly Committed.
        /// </summary>
        public void Dispose()
        {
            if (_realm == null)
            {
                return;
            }

            Rollback();
        }

        /// <summary>
        /// Use explicitly to undo the changes in a <see cref="Transaction"/>, otherwise it is automatically invoked by
        /// exiting the block.
        /// </summary>
        public void Rollback()
        {
            EnsureActionFeasibility("roll back");
            _realm.SharedRealmHandle.CancelTransaction();
            FinishTransaction();
        }

        /// <summary>
        /// Use to save the changes to the realm. If <see cref="Transaction"/> is declared in a <c>using</c> block,
        /// it must be used before the end of that block.
        /// </summary>
        public void Commit()
        {
            EnsureActionFeasibility("commit");
            _realm.SharedRealmHandle.CommitTransaction();
            FinishTransaction();
        }

        /// <summary>
        /// Use to save the changes to the realm. If <see cref="Transaction"/> is declared in a <c>using</c> block,
        /// it must be used before the end of that block.
        /// </summary>
        /// <remarks>
        /// Waiting on this method means waiting to be notified when the commit is effectively written to disk.
        /// Hence, cancelling this call doesn't cancel the commit action. It only stops the notification of completion from being delivered.
        /// </remarks>
        /// <param name="cancellationToken">
        /// Optional cancellation token to stop waiting to be notified when the commit is effectively written to disk.
        /// </param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            EnsureActionFeasibility("commit");

            // If running on background thread, execute synchronously.
            if (!AsyncHelper.HasValidContext)
            {
                Commit();
                return;
            }

            await _realm.SharedRealmHandle.CommitTransactionAsync(cancellationToken);
            FinishTransaction();
        }

        private void EnsureActionFeasibility(string executingAction)
        {
            if (_realm == null)
            {
                throw new Exception($"Transaction was already closed. Cannot {executingAction}");
            }
        }

        private void FinishTransaction()
        {
            _realm.DrainTransactionQueue();
            _realm = null;
        }
    }
}
