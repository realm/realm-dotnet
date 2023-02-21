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

using MongoDB.Bson;
using Realms.Native;

namespace Realms.Sync
{
    /// <summary>
    /// A class representing an API key for a <see cref="User"/>. It can be used to represent the user when logging in
    /// instead of their regular credentials. These keys are created or fetched through <see cref="User.ApiKeys"/>.
    /// </summary>
    /// <remarks>
    /// An API key's <see cref="Value"/> is only available when the key is created and cannot be obtained after that.
    /// This means that it's the caller's responsibility to safely store an API key's value upon creation.
    /// </remarks>
    /// <seealso href="https://docs.mongodb.com/realm/authentication/api-key/">API Key Authentication Docs</seealso>
    public class ApiKey
    {
        /// <summary>
        /// Gets the unique identifier for this key.
        /// </summary>
        /// <value>The id uniquely identifying the key.</value>
        [Preserve]
        public ObjectId Id { get; }

        /// <summary>
        /// Gets the name of the key.
        /// </summary>
        /// <value>The friendly name of the key, specified when calling <see cref="User.ApiKeyClient.CreateAsync"/>.</value>
        [Preserve]
        public string Name { get; }

        /// <summary>
        /// Gets the value for the key. This is only returned when the key is created. After that, it will always be <c>null</c>.
        /// </summary>
        /// <value>The value of the key that needs to be provided when constructing <see cref="Credentials.ApiKey(string)"/>.</value>
        [Preserve]
        public string? Value { get; }

        /// <summary>
        /// Gets a value indicating whether or not this key is currently enabled.
        /// </summary>
        /// <value><c>true</c> if the key is enabled; <c>false</c> otherwise.</value>
        [Preserve]
        public bool IsEnabled { get; }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => (obj is ApiKey key) && key.Id == Id;

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() => Id.GetHashCode();

        /// <summary>
        /// Returns a string representation of the value.
        /// </summary>
        /// <returns>A string representation of the value.</returns>
        public override string ToString() => $"ApiKey {Name} ({Id})";

        [Preserve]
        internal ApiKey(UserApiKey nativeKey)
        {
            Id = nativeKey.Id;
            Name = nativeKey.Name;
            Value = nativeKey.Key;
            IsEnabled = !nativeKey.disabled;
        }
    }
}
