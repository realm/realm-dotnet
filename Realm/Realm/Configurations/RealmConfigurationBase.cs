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
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
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
        private protected RealmSchema? _schema;

        internal delegate void InitialDataDelegate(Realm realm);

        /// <summary>
        /// Gets the filename to be combined with the platform-specific document directory.
        /// </summary>
        /// <value>A string representing a filename only, no path.</value>
        public static string DefaultRealmName => "default.realm";

        /// <summary>
        /// Gets or sets the full path of the Realms opened with this Configuration. May be overridden by passing in a separate name.
        /// </summary>
        /// <value>The absolute path to the Realm.</value>
        public string DatabasePath { get; protected set; }

        /// <summary>
        /// Gets or sets the path where the named pipes used by Realm can be placed.
        /// </summary>
        /// <remarks>
        /// In the vast majority of cases this value should be left null.
        /// It needs to be set if the Realm is opened on a filesystem where a named pipe cannot be created, such as external storage on Android that uses FAT32.
        /// In this case the path should point to a location on a filesystem where the pipes can be created.
        /// </remarks>
        /// <value>The path where named pipes can be created.</value>
        public string? FallbackPipePath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Realm will be open in dynamic mode. If opened in dynamic mode,
        /// the schema will be read from the file on disk.
        /// </summary>
        /// <value><c>true</c> if the Realm will be opened in dynamic mode; <c>false</c> otherwise.</value>
        public bool IsDynamic { get; set; }

        internal bool EnableCache = true;

        /// <summary>
        /// Gets or sets the list of classes persisted in a Realm opened with this configuration.
        /// </summary>
        /// <remarks>
        /// Typically left null so by default all <see cref="RealmObject"/>s and <see cref="EmbeddedObject"/>s will be able to be stored in all Realms.
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
        [Obsolete("Use Schema = new[] { typeof(...) } instead.")]
        public Type[] ObjectClasses
        {
            get => Schema.Select(s => s.Type).Where(t => t != null).ToArray();
            set => Schema = value;
        }

        /// <summary>
        /// Gets or sets the schema of the Realm opened with this configuration.
        /// </summary>
        /// <remarks>
        /// Typically left null so by default all <see cref="RealmObject"/>s and <see cref="EmbeddedObject"/>s will be able to be stored in all Realms.
        /// <br />
        /// If specifying the schema explicitly, you can either use the implicit conversion operator from <c>Type[]</c> to <see cref="RealmSchema"/>
        /// or construct it using the <see cref="RealmSchema.Builder"/> API.
        /// </remarks>
        /// <example>
        /// <code>
        /// config.Schema = new Type[]
        /// {
        ///     typeof(CommonClass),
        ///     typeof(RareClass)
        /// };
        ///
        /// // Alternatively
        /// config.Schema = new RealmSchema.Builder
        /// {
        ///     new ObjectSchema.Builder("Person")
        ///     {
        ///         Property.Primitive("Name", RealmValueType.String, isPrimaryKey: true),
        ///         Property.Primitive("Birthday", RealmValueType.Date, isNullable: true),
        ///         Property.ObjectList("Addresses", objectType: "Address")
        ///     },
        ///     new ObjectSchema.Builder("Address")
        ///     {
        ///         Property.Primitive("City", RealmValueType.String),
        ///         Property.Primitive("Street", RealmValueType.String),
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <value>The schema of the types that can be persisted in the Realm.</value>
        public RealmSchema Schema
        {
            get
            {
                if (_schema != null)
                {
                    return _schema;
                }

                if (IsDynamic)
                {
                    return RealmSchema.Empty;
                }

                return RealmSchema.Default;
            }

            set => _schema = value;
        }

        /// <summary>
        /// Utility to build a path in which a Realm will be created so can consistently use filenames and relative paths.
        /// </summary>
        /// <param name="optionalPath">Path to the Realm, must be a valid full path for the current platform, relative subdirectory, or just filename.</param>
        /// <returns>A full path including name of Realm file.</returns>
        public static string GetPathToRealm(string? optionalPath = null)
        {
            const string errorMessage = "Could not determine a writable folder to store the Realm file. When constructing the RealmConfiguration, provide an absolute optionalPath where writes are allowed.";
            if (string.IsNullOrEmpty(optionalPath))
            {
                return Path.Combine(InteropConfig.GetDefaultStorageFolder(errorMessage), DefaultRealmName);
            }

            if (!Path.IsPathRooted(optionalPath))
            {
                optionalPath = Path.Combine(InteropConfig.GetDefaultStorageFolder(errorMessage), optionalPath);
            }

            if (optionalPath!.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                optionalPath = Path.Combine(optionalPath, DefaultRealmName);
            }

            return Path.GetFullPath(optionalPath);
        }

        /// <summary>
        /// Gets or sets a number, indicating the version of the schema. Can be used to arbitrarily distinguish between schemas even if they have the same objects and properties.
        /// </summary>
        /// <value>0-based value initially set to zero so all user-set values will be greater.</value>
        public ulong SchemaVersion { get; set; }

        private byte[]? _encryptionKey;

        /// <summary>
        /// Gets or sets the key, used to encrypt the entire Realm. Once set, must be specified each time the file is used.
        /// </summary>
        /// <value>Full 64byte (512bit) key for AES-256 encryption.</value>
        public virtual byte[]? EncryptionKey
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

        /// <summary>
        /// Gets or sets the maximum number of active versions allowed before an exception is thrown.
        /// </summary>
        /// <seealso cref="Realm.Freeze"/>
        public ulong MaxNumberOfActiveVersions { get; set; } = ulong.MaxValue;

        internal InitialDataDelegate? PopulateInitialData { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal RealmConfigurationBase()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }

        internal RealmConfigurationBase(string? optionalPath)
        {
            DatabasePath = GetPathToRealm(optionalPath);
        }

        internal virtual Realm CreateRealm()
        {
            var schema = Schema;
            var sharedRealmHandle = CreateHandle(schema);
            return GetRealm(sharedRealmHandle, schema);
        }

        internal virtual async Task<Realm> CreateRealmAsync(CancellationToken cancellationToken)
        {
            var schema = Schema;
            var sharedRealmHandle = await CreateHandleAsync(schema, cancellationToken);
            return GetRealm(sharedRealmHandle, schema);
        }

        internal virtual Native.Configuration CreateNativeConfiguration()
        {
            var managedConfig = GCHandle.Alloc(this);

            var config = new Native.Configuration
            {
                Path = DatabasePath,
                FallbackPipePath = FallbackPipePath,
                schema_version = SchemaVersion,
                enable_cache = EnableCache,
                max_number_of_active_versions = MaxNumberOfActiveVersions,
#pragma warning disable CS0618 // Type or member is obsolete
                use_legacy_guid_representation = Realm.UseLegacyGuidRepresentation,
#pragma warning restore CS0618 // Type or member is obsolete
                invoke_initial_data_callback = PopulateInitialData != null,
                managed_config = GCHandle.ToIntPtr(managedConfig),
            };

            return config;
        }

        internal Realm GetRealm(SharedRealmHandle sharedRealmHandle, RealmSchema? schema = null)
        {
            schema ??= Schema;

            if (IsDynamic && !schema.Any())
            {
                try
                {
                    schema = sharedRealmHandle.GetSchema();
                }
                catch
                {
                    sharedRealmHandle.Close();
                    throw;
                }
            }

            return new Realm(sharedRealmHandle, this, schema);
        }

        internal abstract SharedRealmHandle CreateHandle(RealmSchema schema);

        internal abstract Task<SharedRealmHandle> CreateHandleAsync(RealmSchema schema, CancellationToken cancellationToken);
    }
}
