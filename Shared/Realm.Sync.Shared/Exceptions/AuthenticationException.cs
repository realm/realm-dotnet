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

using System.Net;

namespace Realms.Sync.Exceptions
{
    /// <summary>
    /// An exception thrown when an error has occurred during authentication. It usually indicates a logical problem
    /// with the request that can be investigated by inspecting the <see cref="ErrorCode"/> property.
    /// </summary>
    public class AuthenticationException : HttpException
    {
        /// <summary>
        /// Gets the <see cref="ErrorCode"/> of the error.
        /// </summary>
        /// <value>An enum value indicating the error code.</value>
        public ErrorCode ErrorCode { get; }

        internal AuthenticationException(ErrorCode errorCode, HttpStatusCode statusCode, string reasonPhrase, string payload, string message)
            : base(statusCode, reasonPhrase, payload, message)
        {
            ErrorCode = errorCode;
        }
    }
}
