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
 
/// PROXY VERSION OF CLASS USED IN PCL FOR BAIT AND SWITCH PATTERN 

using System;
using System.IO;

// see internals/RealmConfigurations.md for a detailed diagram of how this interacts with the ObjectStore configuration

namespace Realms
{

    /// <summary>
    /// Realm configuration specifying settings that affect your Realm behaviour.
    /// </summary>
    /// <remarks>
    /// Main role is generating a canonical path from whatever absolute, relative subdir or just filename user supplies.
    /// </remarks>
    public class RealmConfiguration
    {
        /// <summary>
        /// Standard filename to be combined with the platform-specific document directory.
        /// </summary>
        /// <value>A string representing a filename only, no path.</value>      
        public static string DefaultRealmName { get; }


        /// <summary>
        /// Flag mainly to help with temp databases and testing, indicates content can be abandoned when you change the schema.
        /// </summary>
        public readonly bool ShouldDeleteIfMigrationNeeded;

        /// <summary>
        /// Flag to indicate Realm is opened readonly so can open from locked locations such as bundled with an application.
        /// </summary>
        public bool ReadOnly;

        /// <summary>
        /// The full path of any realms opened with this configuration, may be overriden by passing in a separate name.
        /// </summary>
        public string DatabasePath {get; private set;}

        /// <summary>
        /// The list of classes persisted in a Realm opened with this configuration.
        /// </summary>
        /// <remarks>Specify classes by type. Searched linearly so order in decreasing frequency of creating objects.</remarks>
        /// <example>eg: `config.ObjectClasses = new Type[] { typeof(CommonClass), typeof(RareClass) };`</example>
        /// <value>Typically left null so by default all RealmObjects will be able to be stored in all realms.</value>
        public Type[] ObjectClasses { get; set; }

        /// <summary>
        /// Specify the key used to encrypt the entire Realm. Once set, must be specified each time file is used.
        /// </summary>
        /// <value>Full 64byte (512bit) key for AES-256 encryption.</value>
        public byte[] EncryptionKey { get; set; }

        /// <summary>
        /// Configuration you can override which is used when you create a new Realm without specifying a configuration.
        /// </summary>
        /// Number indicating the version, can be used to arbitrarily distinguish between schemas even if they have the same objects and properties.
        /// <value>0-based value initially set to indicate user is not versioning.</value>
        public UInt64 SchemaVersion { get; set; }

        /// <summary>
        /// Utility to build a path in which a realm will be created so can consistently use filenames and relative paths.
        /// </summary>
        public static string PathToRealm(string optionalPath = null)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return "";
        }

        /// <summary>
        public static RealmConfiguration DefaultConfiguration
        {
            set {} 
            get
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();  // if attempt to use DefaultConfiguration as first line of their code with just PCL linked, want exception!
                return null;
            } 
        }

        /// <summary>
        /// Constructor allowing path override.
        /// </summary>
        /// <param name="optionalPath">Path to the realm, must be a valid full path for the current platform, relative subdir, or just filename.</param>
        /// <param name="shouldDeleteIfMigrationNeeded">Optional Flag mainly to help with temp databases and testing, indicates content can be abandoned when you change the schema.</param> 
        public RealmConfiguration(string optionalPath = null, bool shouldDeleteIfMigrationNeeded=false)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        /// <summary>
        /// Clone method allowing you to override or customise the current path.
        /// </summary>
        /// <returns>An object with a fully-specified, canonical path.</returns>
        /// <param name="newConfigPath">Path to the realm, must be a valid full path for the current platform, relative subdir, or just filename.</param>
        public RealmConfiguration ConfigWithPath(string newConfigPath)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <summary>
        /// Generic override determines whether the specified <see cref="System.Object"/> is equal to the current RealmConfiguration.
        /// </summary>
        /// <param name="rhs">The <see cref="System.Object"/> to compare with the current RealmConfiguration.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="Realms.RealmConfiguration"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(Object rhs)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }



        /// <summary>
        /// Determines whether the specified RealmConfiguration is equal to the current RealmConfiguration.
        /// </summary>
        /// <param name="rhs">The <see cref="System.Object"/> to compare with the current RealmConfiguration.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="Realms.RealmConfiguration"/>; otherwise, <c>false</c>.</returns>
        public bool Equals(RealmConfiguration rhs)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }


        /// <summary>
        /// Serves as a hash function for a RealmConfiguration based on its path.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return 0;
        }

    }  // class RealmConfiguration
}  // namespace Realms

