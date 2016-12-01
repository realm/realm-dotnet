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
    /// An SyncConfiguration is used to setup a Realm that can be synchronized between devices using the Realm Object Server.
    /// A valid <see cref="User"/> is required to create a SyncConfiguration.
    /// </summary>
    /// <seealso cref="User.LoginAsync"/>
    /// <seealso cref="Credentials"/>
    public class SyncConfiguration : RealmConfiguration
    {
        /// <summary>
        /// Gets the <see cref="Uri"/> used to create this SyncConfiguration. 
        /// </summary>
        public Uri ServerUri
        {
            get
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                return null;
            }
        }

        /// <summary>
        /// Gets the user used to create this SyncConfiguration.
        /// </summary>
        public User User
        {
            get
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                return null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the Realm file should be deleted once the <see cref="User"/> logs out.
        /// </summary>
        public bool ShouldDeleteRealmOnLogOut
        {
            get
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                return false;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Realms.Sync.SyncConfiguration"/> class.
        /// </summary>
        /// <param name="user">A valid <see cref="User"/>.</param>
        /// <param name="serverUri">A unique <see cref="Uri"/> that identifies the Realm. In URIs, <c>~</c> can be used as a placeholder for a user Id.</param>
        public SyncConfiguration(User user, Uri serverUri)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }
    }
}