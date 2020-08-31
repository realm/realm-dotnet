////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Realms.Sync.Exceptions
{
    /// <summary>
    /// A set of extensions that simplify checking for common error scenarios.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ErrorCodeExtensions
    {
        private static readonly IEnumerable<ErrorCode> ClientResetCodes = new[]
        {
            ErrorCode.BadServerFileIdentifier,
            ErrorCode.BadClientFileIdentifier,
            ErrorCode.BadServerVersion,
            ErrorCode.DivergingHistories,
        };

        /// <summary>
        /// Checks if an error code indicates that a client reset is needed.
        /// </summary>
        /// <returns><c>true</c>, if the code indicates a client reset error, <c>false</c> otherwise.</returns>
        /// <param name="code">The error code.</param>
        public static bool IsClientResetError(this ErrorCode code)
        {
            return ClientResetCodes.Contains(code);
        }
    }
}
