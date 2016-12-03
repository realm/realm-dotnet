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

namespace Realms.Sync.Permissions
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
    /// </remarks>
    public class PermissionChange : RealmObject, IPermissionObject
    {
        /// <inheritdoc />
        public string Id { get; set; }

        /// <inheritdoc />
        public DateTimeOffset CreatedAt { get; set; }

        /// <inheritdoc />
        public DateTimeOffset UpdatedAt { get; set; }

        /// <inheritdoc />
        public int? StatusCode { get; set; }

        /// <inheritdoc />
        public string StatusMessage { get; set; }

        /// <summary>
        /// Gets or sets the user or users to effect.
        /// </summary>
        /// <value><c>*</c> to change the permissions for all users.</value>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the Realm to change permissions for.
        /// </summary>
        /// <value><c>*</c> to change the permissions of all Realms.</value>
        public string RealmUrl { get; set; }

        /// <summary>
        /// Gets or sets read access.
        /// </summary>
        /// <value><c>true</c> or <c>false</c> to request this new value. <c>null</c> to keep current value.</value>
        public bool? MayRead { get; set; }

        /// <summary>
        /// Gets or sets write access.
        /// </summary>
        /// <value><c>true</c> or <c>false</c> to request this new value. <c>null</c> to keep current value.</value>
        public bool? MayWrite { get; set; }

        /// <summary>
        /// Gets or sets manage access.
        /// </summary>
        /// <value><c>true</c> or <c>false</c> to request this new value. <c>null</c> to keep current value.</value>
        public bool? MayManage { get; set; }

        public PermissionChange()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }
    }
}
