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

namespace Realms
{
    /// <summary>
    /// This class is given to you when you migrate your database from one version to another.
    /// It contains two properties: <c>OldRealm</c> and <c>NewRealm</c>.
    /// The <c>NewRealm</c> is the one you should make sure is up to date. It will contain
    /// models corresponding to the configuration you've supplied.
    /// You can read from the old realm and access properties that have been removed from
    /// the classes by using the dynamic API. See more in the migrations section in the documentation.
    /// </summary>
    public class Migration
    {
        /// <summary>
        /// Gets the realm as it was before migrating. Use the dynamic API to access it.
        /// </summary>
        public Realm OldRealm
        {
            get
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                return null;
            }
        }

        /// <summary>
        /// Gets the realm that you should modify and make sure is up to date.
        /// </summary>
        public Realm NewRealm
        {
            get
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                return null;
            }
        }

        private Migration()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }
    }
}