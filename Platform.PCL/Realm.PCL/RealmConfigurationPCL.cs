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

namespace Realms
{
    /// <summary>
    /// Realm configuration specifying settings that affect the Realm's behavior.
    /// </summary>
    /// <remarks>
    /// Its main role is generating a canonical path from whatever absolute, relative subdirectory or just filename the user supplies.
    /// </remarks>
    public class RealmConfiguration : RealmConfigurationBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether the database will be deleted if the schema mismatches the one in the code. Use this when debugging and developing your app but never release it with this flag set to <c>true</c>.
        /// </summary>
        public bool ShouldDeleteIfMigrationNeeded { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a Realm is opened as readonly. This allows opening it from locked locations such as resources, bundled with an application.
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Gets or sets the configuration that is used when creating a new Realm without specifying a configuration.
        /// </summary>
        public static RealmConfiguration DefaultConfiguration
        {
            get
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();  // if attempt to use DefaultConfiguration as first line of their code with just PCL linked, want exception!
                return null;
            }

            set
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RealmConfiguration"/> class.
        /// </summary>
        /// <param name="optionalPath">Path to the realm, must be a valid full path for the current platform, relative subdirectory, or just filename.</param>
        public RealmConfiguration(string optionalPath = null)
        {
        }

        /// <summary>
        /// Determines whether the specified RealmConfiguration is equal to the current RealmConfiguration.
        /// </summary>
        /// <param name="other">The <see cref="RealmConfiguration"/> to compare with the current configuration.</param>
        /// <returns><c>true</c> if the specified <see cref="RealmConfiguration"/> is equal to the current
        /// <see cref="RealmConfiguration"/>; otherwise, <c>false</c>.</returns>
        public bool Equals(RealmConfiguration other)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }

        /// <summary>
        /// Clone method allowing you to override or customize the current path.
        /// </summary>
        /// <returns>An object with a fully-specified, canonical path.</returns>
        /// <param name="newConfigPath">Path to the realm, must be a valid full path for the current platform, relative subdirectory, or just filename.</param>
        public RealmConfiguration ConfigWithPath(string newConfigPath)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }
    }
}
