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

using System;

namespace Realms.Sync
{
    /// <summary>
    /// The status of the management object as set by the server.
    /// </summary>
    [Flags]
    public enum ManagementObjectStatus
    {
        /// <summary>
        /// The server hasn't yet processed the request.
        /// </summary>
        NotProcessed = 1,

        /// <summary>
        /// The server has processed the request successfully.
        /// </summary>
        Success = 2,

        /// <summary>
        /// There was an error while processing the request. See <see cref="IPermissionObject.StatusMessage"/> for more details.
        /// </summary>
        Error = 4,

        /// <summary>
        /// Any status.
        /// </summary>
        Any = NotProcessed | Success | Error
    }
}
