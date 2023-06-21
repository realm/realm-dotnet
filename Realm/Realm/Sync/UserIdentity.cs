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

using MongoDB.Bson.Serialization.Attributes;

namespace Realms.Sync
{
    /// <summary>
    /// A class containing information about an identity associated with a user.
    /// </summary>
    [BsonNoId]
    public class UserIdentity
    {
        /// <summary>
        /// Gets the unique identifier for this identity.
        /// </summary>
        /// <value>The identity's Id.</value>
        [Preserve]
        public string Id { get; private set; } = null!;

        /// <summary>
        /// Gets the auth provider defining this identity.
        /// </summary>
        /// <value>The identity's auth provider.</value>
        [Preserve]
        public Credentials.AuthProvider Provider { get; private set; }

        /// <inheritdoc/>
        [Preserve]
        public override bool Equals(object? obj) => (obj is UserIdentity id) && id.Id == Id && id.Provider == Provider;

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        [Preserve]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = -1285871140;
                hashCode = (hashCode * -1521134295) + (Id?.GetHashCode() ?? 0);
                hashCode = (hashCode * -1521134295) + Provider.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Returns a string representation of the value.
        /// </summary>
        /// <returns>A string representation of the value.</returns>
        [Preserve]
        public override string ToString() => $"UserIdentity: {Id} ({Provider})";
    }
}
