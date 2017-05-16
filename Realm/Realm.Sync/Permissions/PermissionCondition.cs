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

using System.Diagnostics.CodeAnalysis;

namespace Realms.Sync
{
    /// <summary>
    /// A class describing the condition based on which permissions will be applied.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public abstract class PermissionCondition
    {
        /// <summary>
        /// Apply permissions based on the user's Id.
        /// </summary>
        /// <returns>A <see cref="PermissionCondition"/> containing information about the user's Id.</returns>
        /// <param name="userId">The Id of the user or <c>*</c> to change the permissions for all users.</param>
        public PermissionCondition UserId(string userId)
        {
            return new UserIdCondition(userId);
        }

        /// <summary>
        /// Apply permissions based on the user's Email when using the username/password login provider.
        /// </summary>
        /// <returns>A <see cref="PermissionCondition"/> containing information about the user's email.</returns>
        /// <param name="email">The email (username) of the user that will be affected by this condition.</param>
        public PermissionCondition Email(string email)
        {
            return KeyValue("email", email);
        }

        /// <summary>
        /// Apply permissions based on a key/value combination in the user's metadata.
        /// </summary>
        /// <returns>
        /// A <see cref="PermissionCondition"/> containing information about the key/value combination that will be used
        /// for matching against.
        /// </returns>
        /// <param name="key">The metadata key to look for.</param>
        /// <param name="value">The metadata value that must match the key.</param>
        public PermissionCondition KeyValue(string key, string value)
        {
            return new KeyValueCondition(key, value);
        }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    internal class UserIdCondition : PermissionCondition
    {
        public new string UserId { get; }

        public UserIdCondition(string userId)
        {
            UserId = userId;
        }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    internal class KeyValueCondition : PermissionCondition
    {
        public string Key { get; }

        public string Value { get; }

        public KeyValueCondition(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}