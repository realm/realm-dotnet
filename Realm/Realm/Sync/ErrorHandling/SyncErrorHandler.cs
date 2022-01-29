////////////////////////////////////////////////////////////////////////////
//
// Copyright 2021 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
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

using Realms.Sync.Exceptions;

namespace Realms.Sync.ErrorHandling
{
    /// <summary>
    /// Handler triggered whenever a sync error happens on a synchronized Realm.
    /// To be noted that a client reset is not a sync error and in order to handle that subclasses of <see cref="ClientResetHandlerBase"/> are available.
    /// </summary>
    /// <seealso href="https://docs.mongodb.com/realm/sync/overview/">Sync Overview Docs</seealso>
    public class SyncErrorHandler
    {
        /// <summary>
        /// Callback triggered when an error occurs in a session.
        /// </summary>
        /// <param name="session">
        /// The <see cref="Session"/> where the error happened on.
        /// </param>
        /// <param name="error">
        /// The specific <see cref="SessionException"/> occurred on this <see cref="Session"/>.
        /// </param>
        public delegate void SessionErrorCallback(Session session, SessionException error);

        /// <summary>
        /// Gets or sets the user callback to handle all the <see cref="SessionException"/>s that could happen on a synchronized Realm.
        /// </summary>
        public SessionErrorCallback OnError { get; set; }
    }
}
