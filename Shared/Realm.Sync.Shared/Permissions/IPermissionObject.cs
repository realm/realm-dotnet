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
        /// Gets the <see cref="ErrorCode"/> if any.
        /// </summary>
        /// <remarks>
        /// Filled by the server after an object was processed indicating the status of the operation. 
        /// If <see cref="Status"/> returns <see cref="ManagementObjectStatus.Error"/>, the <see cref="ErrorCode"/> 
        /// property can be used to get a strongly typed code for the error and handle expected error conditions, such as
        /// expired offer or attempting to share a realm without having manage access.
        /// </remarks>
        /// <value>
        /// An <see cref="ErrorCode"/> that indicates the reason for the error during processing.
        /// <c>null</c> if no error has occurred or the object hasn't been processed yet.
        /// </value>
        ErrorCode? ErrorCode { get; }

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
