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
using Realms.Native;
using Realms.Schema;

namespace Realms
{
    /// <summary>
    /// Base class for specifying configuration settings that affect the Realm's behavior.
    /// <br/>
    /// Its main role is generating a canonical path from whatever absolute, relative subdirectory, or just filename the user supplies.
    /// </summary>
    public abstract class RealmConfigurationBase
    {
        private protected RealmSchema? _schema;
        private byte[]? _encryptionKey;

        internal delegate void InitialDataDelegate(Realm realm);

        /// <summary>
        /// A callback, invoked when opening a Realm for the first time during the life
        /// of a process to determine if it should be compacted before being returned
        /// to the user.
        /// </summary>
        /// <param name="totalBytes">Total file size (data + free space).</param>
        /// <param name="bytesUsed">Total data size.</param>
        /// <returns><c>true</c> to indicate that an attempt to compact the file should be made.</returns>
        /// <remarks>The compaction will be skipped if another process is accessing it.</remarks>
        public delegate bool ShouldCompactDelegate(ulong totalBytes, ulong bytesUsed);

        /// <summary>
        /// Gets the filename to be combined with the platform-specific document directory.
        /// </summary>
        /// <value>A string representing a filename only, no path.</value>
        public static string DefaultRealmName => "default.realm";

        /// <summary>
        /// Gets the full path of the Realms opened with this Configuration. May be overridden by passing in a separate name.
        /// </summary>
        /// <value>The absolute path to the Realm.</value>
        public string DatabasePath { get; private protected set; }

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

        /// <summary>
        /// Gets or sets the compact on launch callback.
        /// </summary>
        /// <value>
        /// The <see cref="ShouldCompactDelegate"/> that will be invoked when opening a Realm for the first time
        /// to determine if it should be compacted before being returned to the user.
        /// </value>
        public ShouldCompactDelegate? ShouldCompactOnLaunch { get; set; }

        //TODO Add docs
        public bool RelaxedSchema { get; set; }

        internal bool EnableCache = true;

        /// <summary>
        /// Gets or sets the schema of the Realm opened with this configuration.
        /// </summary>
        /// <remarks>
        /// Typically left null so by default all <see cref="IRealmObject"/> and <see cref="IEmbeddedObject"/> instance will be able to
        /// be stored in all Realms.
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

            // optionalPath is guaranteed to be non-null here, but .NET Standard 2.0 doesn't have correct annotations for
            // string.IsNullOrEmpty.
            // ReSharper disable once RedundantSuppressNullableWarningExpression
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

        internal byte[]? EncryptionKey
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

        internal RealmConfigurationBase(string? optionalPath)
        {
            DatabasePath = GetPathToRealm(optionalPath);
        }

        internal RealmConfigurationBase Clone()
        {
            return (RealmConfigurationBase)MemberwiseClone();
        }

        internal virtual Realm CreateRealm()
        {
            var schema = Schema;
            using var arena = new Arena();
            var sharedRealmHandle = CreateHandle(CreateNativeConfiguration(arena));
            return GetRealm(sharedRealmHandle, schema);
        }

        internal virtual async Task<Realm> CreateRealmAsync(CancellationToken cancellationToken)
        {
            var schema = Schema;
            using var arena = new Arena();
            var configuration = CreateNativeConfiguration(arena);
            var sharedRealmHandle = await CreateHandleAsync(configuration, cancellationToken);
            return GetRealm(sharedRealmHandle, schema);
        }

        internal virtual Configuration CreateNativeConfiguration(Arena arena)
        {
            var managedConfig = GCHandle.Alloc(this);

            var config = new Configuration
            {
                path = StringValue.AllocateFrom(DatabasePath, arena),
                fallbackPipePath = StringValue.AllocateFrom(FallbackPipePath, arena),
                schema = Schema.ToNative(arena),
                schema_version = SchemaVersion,
                enable_cache = EnableCache,
                max_number_of_active_versions = MaxNumberOfActiveVersions,
#pragma warning disable CS0618 // Type or member is obsolete
                use_legacy_guid_representation = Realm.UseLegacyGuidRepresentation,
#pragma warning restore CS0618 // Type or member is obsolete
                invoke_initial_data_callback = PopulateInitialData != null,
                managed_config = GCHandle.ToIntPtr(managedConfig),
                encryption_key = MarshaledVector<byte>.AllocateFrom(EncryptionKey, arena),
                invoke_should_compact_callback = ShouldCompactOnLaunch != null,
                relaxed_schema = RelaxedSchema,
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

        internal abstract SharedRealmHandle CreateHandle(in Configuration configuration);

        internal abstract Task<SharedRealmHandle> CreateHandleAsync(in Configuration configuration, CancellationToken cancellationToken);
    }
}
