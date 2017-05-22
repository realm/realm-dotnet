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
using System.IO;
using System.Threading.Tasks;
using Realms.Schema;

namespace Realms
{
    /// <summary>
    /// Base class for specifying configuration settings that affect the Realm's behavior.
    /// </summary>
    /// <remarks>
    /// Its main role is generating a canonical path from whatever absolute, relative subdirectory, or just filename the user supplies.
    /// </remarks>
    public abstract class RealmConfigurationBase
    {
        /// <summary>
        /// Gets the filename to be combined with the platform-specific document directory.
        /// </summary>
        /// <value>A string representing a filename only, no path.</value>
        public static string DefaultRealmName => "default.realm";

        /// <summary>
        /// Gets or sets the full path of the Realms opened with this Configuration. May be overriden by passing in a separate name.
        /// </summary>
        /// <value>The absolute path to the Realm.</value>
        public string DatabasePath { get; protected set; }

        internal bool Dynamic;

        /// <summary>
        /// Gets or sets the list of classes persisted in a Realm opened with this configuration.
        /// </summary>
        /// <remarks>
        /// Typically left null so by default all <see cref="RealmObject"/>s will be able to be stored in all Realms.
        /// </remarks>
        /// <example>
        /// <code>
        /// config.ObjectClasses = new Type[] 
        /// { 
        ///     typeof(CommonClass), 
        ///     typeof(RareClass) 
        /// };
        /// </code>
        /// </example>
        /// <value>The classes that can be persisted in the Realm.</value>
        public Type[] ObjectClasses { get; set; }

        /// <summary>
        /// Utility to build a path in which a Realm will be created so can consistently use filenames and relative paths.
        /// </summary>
        /// <param name="optionalPath">Path to the Realm, must be a valid full path for the current platform, relative subdirectory, or just filename.</param>
        /// <returns>A full path including name of Realm file.</returns>
        public static string GetPathToRealm(string optionalPath = null)
        {
            if (string.IsNullOrEmpty(optionalPath))
            {
                return Path.Combine(InteropConfig.DefaultStorageFolder, DefaultRealmName);
            }

            if (!Path.IsPathRooted(optionalPath))
            {
                optionalPath = Path.Combine(InteropConfig.DefaultStorageFolder, optionalPath);
            }

            if (optionalPath[optionalPath.Length - 1] == Path.DirectorySeparatorChar) // ends with dir sep
            {
                optionalPath = Path.Combine(optionalPath, DefaultRealmName);
            }

            return optionalPath;
        }

        /// <summary>
        /// Gets or sets a number, indicating the version of the schema. Can be used to arbitrarily distinguish between schemas even if they have the same objects and properties.
        /// </summary>
        /// <value>0-based value initially set to zero so all user-set values will be greater.</value>
        public ulong SchemaVersion { get; set; } = 0;

        private byte[] _encryptionKey;

        /// <summary>
        /// Gets or sets the key, used to encrypt the entire Realm. Once set, must be specified each time the file is used.
        /// </summary>
        /// <value>Full 64byte (512bit) key for AES-256 encryption.</value>
        public byte[] EncryptionKey
        {
            get
            {
                return _encryptionKey;
            }

            set
            {
                if (value != null && value.Length != 64)
                {
                    throw new FormatException("EncryptionKey must be 64 bytes");
                }

                _encryptionKey = value;
            }
        }

        internal RealmConfigurationBase(string optionalPath)
        {
            DatabasePath = GetPathToRealm(optionalPath);
        }

        internal abstract Realm CreateRealm(RealmSchema schema);

        internal abstract Task<Realm> CreateRealmAsync(RealmSchema schema);
    }
}
