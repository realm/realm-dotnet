////////////////////////////////////////////////////////////////////////////
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
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Realms.Helpers;

namespace Realms
{
    /// <summary>
    /// Represents the state of a <see cref="Transaction"/>.
    /// </summary>
    public enum TransactionState
    {
        /// <summary>
        /// The transaction is running.
        /// </summary>
        Running,

        /// <summary>
        /// The transaction is successfully committed.
        /// </summary>
        Committed,

        /// <summary>
        /// The transaction rolled back its changes.
        /// </summary>
        RolledBack,
    }

    /// <summary>
    /// Provides a scope to safely read and write to a <see cref="Realm"/>. Must use explicitly via <see cref="Realm.BeginWrite"/>.
    /// </summary>
    /// <remarks>
    /// All access to a <see cref="Realm"/> occurs within a <see cref="Transaction"/>. Read transactions are created implicitly.
    /// </remarks>
    public class Transaction : IDisposable
    {
        private Realm? _realm;

        /// <summary>
        /// Gets the state of this transaction.
        /// </summary>
        public TransactionState State { get; private set; }

        internal Transaction(Realm realm)
        {
            _realm = realm;
            State = TransactionState.Running;
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
            FinishTransaction(TransactionState.RolledBack);
        }

        /// <summary>
        /// Use to save the changes to the realm. If <see cref="Transaction"/> is declared in a <c>using</c> block,
        /// it must be used before the end of that block.
        /// </summary>
        public void Commit()
        {
            EnsureActionFeasibility("commit");
            _realm.SharedRealmHandle.CommitTransaction();
            FinishTransaction(TransactionState.Committed);
        }

        /// <summary>
        /// Use to save the changes to the realm. If <see cref="Transaction"/> is declared in a <c>using</c> block,
        /// it must be used before the end of that block. It completes when the changes are effectively written to disk.
        /// </summary>
        /// <remarks>
        /// Cancelling the returned <see cref="Task"/> will not prevent the write to disk but
        /// it will immediately resolve the task with a <see cref="TaskCanceledException"/>.
        /// In fact, the commit action can't be stopped and continues running to completion in the background.<br/>
        /// A use case for cancelling this action could be that you want to show users a pop-up indicating that the
        /// data is being saved. But, you want to automatically close such pop-up after a certain amount of time.
        /// Or, you may want to allow users to manually dismiss that pop-up.
        /// </remarks>
        /// <param name="cancellationToken">
        /// Optional cancellation token to stop waiting on the returned <see cref="Task"/>.
        /// </param>
        /// <returns>
        /// An awaitable <see cref="Task"/> that completes when the committed changes are effectively written to disk.
        /// </returns>
        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            EnsureActionFeasibility("commit");

            // If running on background thread, execute synchronously.
            if (!AsyncHelper.TryGetValidContext(out var synchronizationContext))
            {
                Commit();
                return;
            }

            await _realm.SharedRealmHandle.CommitTransactionAsync(synchronizationContext, cancellationToken);
            FinishTransaction(TransactionState.Committed);
        }

        [MemberNotNull(nameof(_realm))]
        private void EnsureActionFeasibility(string executingAction)
        {
            if (_realm == null)
            {
                throw new Exception($"Transaction was already closed. Cannot {executingAction}");
            }
        }

        private void FinishTransaction(TransactionState state)
        {
            State = state;
            _realm!.DrainTransactionQueue();
            _realm = null!;
        }
    }
}
