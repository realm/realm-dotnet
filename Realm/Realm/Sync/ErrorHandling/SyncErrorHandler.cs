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
    public class SyncErrorHandler
    {
        /// <summary>
        /// Triggered when an error occurs in a session.
        /// Until full deprecation, this callback still calls into <see cref="Session.Error"/> for backward compatibility.
        /// </summary>
        public delegate void SessionErrorCallback(Session session, SessionException error);

        public SessionErrorCallback OnError { get; set; }
    }
}
