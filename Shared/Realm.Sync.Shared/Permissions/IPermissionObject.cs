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
using Realms.Sync.Exceptions;

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
        /// <value>The unique id of the object.</value>
        string Id { get; }

        /// <summary>
        /// Gets the creation time of this object.
        /// </summary>
        /// <value>A <see cref="DateTimeOffset"/> indicating the object's creation date and time.</value>
        DateTimeOffset CreatedAt { get; }

        /// <summary>
        /// Gets when the object was updated the last time.
        /// </summary>
        /// <remarks>
        /// This will be updated by the server with the current object when the object is processed.
        /// </remarks>
        /// <value>A <see cref="DateTimeOffset"/> indicating the last time the object has been updated.</value>
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
        /// <value>A detailed message describing the status (success, error) of the operation. <c>null</c> if the object
        /// has not been processed yet.</value>
        string StatusMessage { get; }

        /// <summary>
        /// Gets the <see cref="ManagementObjectStatus"/> as set by the server.
        /// </summary>
        /// <value>An enum indicating whether the operation has completed successfully.</value>
        ManagementObjectStatus Status { get; }
    }
}
