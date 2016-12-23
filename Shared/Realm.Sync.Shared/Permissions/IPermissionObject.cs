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

namespace Realms.Sync
{
    /// <summary>
    /// Interface that describes the shared base model of all Permission classes.
    /// </summary>
    public interface IPermissionObject
    {
        /// <summary>
        /// Gets the unique identifier of this object in the Management realm.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the creation time of this object.
        /// </summary>
        DateTimeOffset CreatedAt { get; }

        /// <summary>
        /// Gets when the object was updated the last time.
        /// </summary>
        /// <remarks>
        /// This should be filled by the client with the <see cref="CreatedAt"/>
        /// date and is updated by the server with the current object when the object is processed.
        /// </remarks>
        DateTimeOffset UpdatedAt { get; }

        /// <summary>
        /// Gets the status code.
        /// </summary>
        /// <remarks>
        /// Filled by the server after an object was processed indicating the status
        /// of the operation.
        /// The values have the following meaning:
        /// <list type="bullet">
        /// <listheader>
        /// <term>no status(<c>null</c>)</term>
        /// <description>The object has not been processed yet.</description>
        /// </listheader>
        /// <listheader>
        /// <term>status equal to <c>0</c></term>
        /// <description>The operation succeeded</description>
        /// </listheader>
        /// <listheader>
        /// <term>any status greater than <c>0</c></term>
        /// <description>The operation failed</description>
        /// </listheader>
        /// </list>
        /// </remarks>
        int? StatusCode { get; }

        /// <summary>
        /// Gets the status message.
        /// </summary>
        /// <remarks>
        /// Filled by the server after an object was processed with additional info
        /// explaining the status if necessary.
        /// </remarks>
        string StatusMessage { get; }

        /// <summary>
        /// Gets the <see cref="ManagementObjectStatus"/> as set by the server.
        /// </summary>
        ManagementObjectStatus Status { get; }
    }
}
