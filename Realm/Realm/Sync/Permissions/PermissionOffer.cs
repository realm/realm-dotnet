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
using Newtonsoft.Json;
using Realms.Sync.Exceptions;

namespace Realms.Sync
{
    /// <summary>
    /// Objects of this class are used to offer permissions to owned Realms.
    /// </summary>
    public class PermissionOffer
    {
        /// <summary>
        /// Gets the creation time of this object.
        /// </summary>
        /// <value>A <see cref="DateTimeOffset"/> indicating the object's creation date and time.</value>
        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Gets the token that can be used to offer the permissions defined in this object to another user.
        /// </summary>
        /// <value>A string, set by the server, that can be used in <see cref="User.InvalidateOfferAsync(string)"/> or
        /// <see cref="User.AcceptPermissionOfferAsync(string)"/>.</value>
        [JsonProperty("token")]
        public string Token { get; private set; }

        /// <summary>
        /// Gets the path of the Realm to offer permissions to.
        /// </summary>
        [JsonProperty("realmPath")]
        public string RealmPath { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the receiver of this offer will be able to read from the Realm.
        /// </summary>
        /// <value><c>true</c> to allow the receiver to read data from the <see cref="Realm"/>.</value>
        [JsonProperty("accessLevel")]
        public AccessLevel AccessLevel { get; private set; }

        /// <summary>
        /// Gets the expiration date and time of the offer.
        /// </summary>
        /// <value>If <c>null</c>, the offer will never expire. Otherwise, the offer may not be consumed past the expiration date.</value>
        [JsonProperty("expiresAt")]
        public DateTimeOffset? ExpiresAt { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the receiver of this offer will be able to read data from the Realm.
        /// </summary>
        /// <value><c>true</c> to allow the receiver to read date from the <see cref="Realm"/>.</value>
        [Obsolete("Use AccessLevel >= AccessLevel.Read instead")]
        public bool MayRead => AccessLevel >= AccessLevel.Read;

        /// <summary>
        /// Gets a value indicating whether the receiver of this offer will be able to write to the Realm.
        /// </summary>
        /// <value><c>true</c> to allow the receiver to write data to the <see cref="Realm"/>.</value>
        [Obsolete("Use AccessLevel >= AccessLevel.Write instead")]
        public bool MayWrite => AccessLevel >= AccessLevel.Write;

        /// <summary>
        /// Gets a value indicating whether the receiver of this offer will be able to manage access rights for others.
        /// </summary>
        /// <value><c>true</c> to allow the receiver to offer others access to the <see cref="Realm"/>.</value>
        [Obsolete("Use AccessLevel >= AccessLevel.Admin instead")]
        public bool MayManage => AccessLevel >= AccessLevel.Admin;
    }
}
