////////////////////////////////////////////////////////////////////////////
//
// Copyright 2017 Realm Inc.
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

using Realms.Exceptions;

namespace Realms.Sync.Exceptions
{
    /// <summary>
    /// An exception thrown when an error has occurred when changing permissions of a Realm.
    /// </summary>
    public class PermissionException : RealmException
    {
        /// <summary>
        /// Gets the <see cref="ErrorCode"/> of the error.
        /// </summary>
        /// <value>An enum value indicating the error code.</value>
        public ErrorCode ErrorCode { get; }

        internal PermissionException(ErrorCode errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
