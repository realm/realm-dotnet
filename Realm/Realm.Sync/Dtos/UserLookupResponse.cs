////////////////////////////////////////////////////////////////////////////
//
// Copyright 2017 Realm Inc.
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

namespace Realms.Sync
{
    /// <summary>
    /// The response of a user lookup operation.
    /// </summary>
    public class UserLookupResponse
    {
        /// <summary>
        /// Gets the identity of the user in Realm's system. Equivalent to <see cref="User.Identity"/>.
        /// </summary>
        public string Identity { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the user is a Realm Object Server administrator user. Equivalent to <see cref="User.IsAdmin"/>.
        /// </summary>
        public bool IsAdmin { get; internal set; }

        /// <summary>
        /// Gets the provider that the user registered through.
        /// </summary>
        public string Provider { get; internal set; }

        /// <summary>
        /// Gets the user's Id in the provider's system.
        /// </summary>
        public string ProviderId { get; internal set; }
    }
}
