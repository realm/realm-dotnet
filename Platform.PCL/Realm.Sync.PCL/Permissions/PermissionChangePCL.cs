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
    /// Objects of this class allow to change permissions of owned Realms.
    /// They are created exclusively by the client and are processed by the server
    /// as indicated by the status fields.
    /// </summary>
    /// <remarks>
    /// <see cref="PermissionChange"/> objects allow to grant and revoke permissions by setting
    /// <see cref="MayRead" />, <see cref="MayWrite" /> and <see cref="MayManage" /> accordingly. 
    /// If any of these flags are not set, these are merged
    /// with either the existing or default permissions as applicable. As a
    /// side-effect this causes that the default permissions are permanently
    /// materialized for the affected Realm files and the affected user.
    /// Once the request has been processed, the <see cref="Status"/>, <see cref="StatusMessage"/>, and
    /// <see cref="ErrorCode"/> will be updated accordingly.
    /// </remarks>
    public class PermissionChange : RealmObject, IPermissionObject
    {
        /// <inheritdoc />
        [PrimaryKey, Required]
        public string Id { get; }

        /// <inheritdoc />
        public DateTimeOffset CreatedAt { get; }

        /// <inheritdoc />
        public DateTimeOffset UpdatedAt { get; }

        /// <inheritdoc />
        public string StatusMessage { get; }

        /// <inheritdoc />
        public ManagementObjectStatus Status { get; }

        /// <inheritdoc />
        public ErrorCode? ErrorCode { get; }

        /// <summary>
        /// Gets the user or users to effect.
        /// </summary>
        /// <value><c>*</c> to change the permissions for all users.</value>
        [Required]
        public string UserId { get; }

        /// <summary>
        /// Gets the Realm to change permissions for.
        /// </summary>
        /// <value><c>*</c> to change the permissions of all Realms.</value>
        [Required]
        public string RealmUrl { get; }

        /// <summary>
        /// Gets a value indicating whether the user(s) have read access to the specified Realm(s).
        /// </summary>
        /// <value><c>true</c> or <c>false</c> to request this new value. <c>null</c> to keep current value.</value>
        public bool? MayRead { get; }

        /// <summary>
        /// Gets a value indicating whether the user(s) have write access to the specified Realm(s).
        /// </summary>
        /// <value><c>true</c> or <c>false</c> to request this new value. <c>null</c> to keep current value.</value>
        public bool? MayWrite { get; }

        /// <summary>
        /// Gets a value indicating whether the user(s) have manage access to the specified Realm(s).
        /// </summary>
        /// <value><c>true</c> or <c>false</c> to request this new value. <c>null</c> to keep current value.</value>
        public bool? MayManage { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionChange"/> class.
        /// </summary>
        /// <param name="userId">The user or users who should be granted these permission changes. Use * to change permissions for all users.</param>
        /// <param name="realmUrl">The Realm URL whose permissions settings should be changed. Use `*` to change the permissions of all Realms managed by the management Realm's <see cref="User"/>.</param>
        /// <param name="mayRead">Define read access. <c>true</c> or <c>false</c> to request this new value. <c>null</c> to keep current value.</param>
        /// <param name="mayWrite">Define write access. <c>true</c> or <c>false</c> to request this new value. <c>null</c> to keep current value.</param>
        /// <param name="mayManage">Define manage access. <c>true</c> or <c>false</c> to request this new value. <c>null</c> to keep current value.</param>
        public PermissionChange(string userId, string realmUrl, bool? mayRead = null, bool? mayWrite = null, bool? mayManage = null)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }
    }
}
