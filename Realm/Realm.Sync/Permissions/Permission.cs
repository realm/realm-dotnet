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
    /// This model is used to reflect permissions granted to a user.
    /// It should be used in conjunction with a <see cref="User"/>'s Permission Realm.
    /// </summary>
    /// <see cref="UserPermissionsExtensions.GetPermissionRealm"/>
    [Explicit]
    public class Permission : RealmObject
    {
        /// <summary>
        /// Gets when the object was updated the last time.
        /// </summary>
        /// <value>A <see cref="DateTimeOffset"/> indicating the last time the object has been updated.</value>
        [MapTo("updatedAt")]
        public DateTimeOffset UpdatedAt { get; private set; }

        /// <summary>
        /// Gets the identity of the user affected by this permission.
        /// </summary>
        /// <value>The user identity.</value>
        [Required]
        [MapTo("userId")]
        public string UserId { get; private set; }

        /// <summary>
        /// Gets the relative path to the Realm on the server.
        /// </summary>
        /// <value>A relative path component.</value>
        [Required]
        [MapTo("path")]
        public string Path { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the user inspecting that permission is allowed to read the Realm at the
        /// specified <see cref="Path"/>.
        /// </summary>
        /// <value><c>true</c> if reading is allowed, <c>false</c> otherwise.</value>
        [MapTo("mayRead")]
        public bool MayRead { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the user inspecting that permission is allowed to write to the Realm at the
        /// specified <see cref="Path"/>.
        /// </summary>
        /// <value><c>true</c> if writing is allowed, <c>false</c> otherwise.</value>
        [MapTo("mayWrite")]
        public bool MayWrite { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the user inspecting that permission is allowed to manage the permissions for
        /// the Realm at the specified <see cref="Path"/>.
        /// </summary>
        /// <value><c>true</c> if managing is allowed, <c>false</c> otherwise.</value>
        [MapTo("mayManage")]
        public bool MayManage { get; private set; }

        private Permission()
        {
        }
    }
}
