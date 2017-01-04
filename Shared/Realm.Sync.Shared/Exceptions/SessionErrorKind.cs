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

namespace Realms.Sync.Exceptions
{
    /// <summary>
    /// Different kinds of errors a <see cref="Session"/> object can emit.
    /// </summary>
    public enum SessionErrorKind
    {
        /// <summary>
        /// An informational error, nothing to do. Only for debug purposes.
        /// </summary>
        Debug = 0,

        /// <summary>
        /// The session is invalid and should be killed.
        /// </summary>
        SessionFatal,

        /// <summary>
        /// Permissions error with the session.
        /// </summary>
        AccessDenied,

        /// <summary>
        /// The user associated with the session is invalid.
        /// </summary>
        UserFatal
    }
}
