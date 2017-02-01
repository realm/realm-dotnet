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

namespace Realms.Sync.Exceptions
{
    /// <summary>
    /// An exception type that describes a session-level error condition.
    /// </summary>
    public class SessionException : Exception
    {
        /// <summary>
        /// Gets the error code that describes the session error this exception represents.
        /// </summary>
        /// <value>An enum value, providing more detailed information for the cause of the error.</value>
        public ErrorCode ErrorCode { get; }

        internal SessionException(string message, ErrorCode errorCode, Exception innerException = null) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
