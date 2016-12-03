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

// PROXY VERSION OF CLASS USED IN PCL FOR BAIT AND SWITCH PATTERN 

using System;

// see internals/RealmConfigurations.md for a detailed diagram of how this interacts with the ObjectStore configuration
namespace Realms
{
    /// <summary>
    /// Realm configuration specifying settings that affect the Realm's behavior.
    /// </summary>
    /// <remarks>
    /// Main role is generating a canonical path from whatever absolute, relative subdirectory or just filename the user supplies.
    /// </remarks>
    public class RealmConfiguration
    {
        /// <summary>
        /// Gets the filename to be combined with the platform-specific document directory.
        /// </summary>
        /// <value>A string representing a filename only, no path.</value>      
        public static string DefaultRealmName { get; }

        /// <summary>
        /// Gets a value indicating whether the database will be deleted if the schema mismatches the one in the code. Use this when debugging and developing your app but never release it with this flag set to <c>true</c>.
        /// </summary>
        public bool ShouldDeleteIfMigrationNeeded { get; }

        /// <summary>
        /// Gets or sets a value indicating whether a Realm is opened as readonly. This allows opening it from locked locations such as resources, bundled with an application.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return ReadOnly;
            }

            set
            {
                ReadOnly = value;
            }
        }

        /// <summary>
        /// Gets the full path of the realms opened with this configuration, may be overriden by passing in a separate name.
        /// </summary>
        public string DatabasePath { get; private set; }

        /// <summary>
        /// Gets or sets the list of classes persisted in a Realm opened with this configuration.
        /// </summary>
        /// <remarks>Specify classes by type. Searched linearly so order in decreasing frequency of creating objects.</remarks>
        /// <example>For example: `config.ObjectClasses = new Type[] { typeof(CommonClass), typeof(RareClass) };`.</example>
        /// <value>Typically left null so by default all RealmObjects will be able to be stored in all realms.</value>
        public Type[] ObjectClasses { get; set; }

        /// <summary>
        /// Gets or sets the key, used to encrypt the entire Realm. Once set, must be specified each time file is used.
        /// </summary>
        /// <value>Full 64byte (512bit) key for AES-256 encryption.</value>
        public byte[] EncryptionKey { get; set; }

        /// <summary>
        /// Gets or sets a number, indicating the version of the schema. Can be used to arbitrarily distinguish between schemas even if they have the same objects and properties.
        /// </summary>
        /// <value>0-based value initially set to zero so all user-set values will be greater.</value>
        public ulong SchemaVersion { get; set; }

        /// <summary>
        /// In order to handle manual migrations, you need to supply one of these to your <c>RealmConfiguration</c>.
        /// It will be called with a <c>Migration</c> instance containing the pre- and the post-migration realm.
        /// You should make sure that the <c>NewRealm</c> property on it contains a database that is up
        /// to date when returning.
        /// The <c>oldSchemaVersion</c> parameter will tell you which version the user is migrating *from*.
        /// They should always be migrating to the current version.
        /// </summary>
        /// <param name="migration">The <see cref="Migration"/> instance, containing information about the old and the new realm.</param>
        /// <param name="oldSchemaVersion">An unsigned long value indicating the schema version of the old realm.</param>
        public delegate void MigrationCallbackDelegate(Migration migration, ulong oldSchemaVersion);

        /// <summary>
        /// Gets or sets the migration callback.
        /// </summary>
        public MigrationCallbackDelegate MigrationCallback { get; set; }

        /// <summary>
        /// Utility to build a path in which a realm will be created so can consistently use filenames and relative paths.
        /// </summary>
        /// <param name="optionalPath">Path to the realm, must be a valid full path for the current platform, relative subdirectory, or just filename.</param>
        /// <returns>A full path including name of Realm file.</returns>
        public static string GetPathToRealm(string optionalPath = null)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return string.Empty;
        }

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
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RealmConfiguration"/> class.
        /// </summary>
        /// <param name="optionalPath">Path to the realm, must be a valid full path for the current platform, relative subdirectory, or just filename.</param>
        /// <param name="shouldDeleteIfMigrationNeeded">Optional Flag mainly to help with temp databases and testing, indicates content can be abandoned when you change the schema.</param> 
        public RealmConfiguration(string optionalPath = null, bool shouldDeleteIfMigrationNeeded = false)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
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

        /// <summary>
        /// Generic override determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="Realms.RealmConfiguration" />.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="Realms.RealmConfiguration" />.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="Realms.RealmConfiguration"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
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

        #region Obsolete members

        /// <summary>
        /// Flag to indicate Realm is opened readonly so can open from locked locations such as bundled with an application.
        /// </summary>
        [Obsolete("This field has been renamed. Use IsReadOnly instead.")]
        public bool ReadOnly;

        /// <summary>
        /// Utility to build a path in which a realm will be created so can consistently use filenames and relative paths.
        /// </summary>
        /// <param name="optionalPath">Path to the realm, must be a valid full path for the current platform, relative subdir, or just filename.</param>
        /// <returns>A full path including name of Realm file.</returns>
        [Obsolete("This method has been renamed. Use GetPathToRealm instead.")]
        public static string PathToRealm(string optionalPath = null)
        {
            return GetPathToRealm(optionalPath);
        }

        #endregion
    }
}