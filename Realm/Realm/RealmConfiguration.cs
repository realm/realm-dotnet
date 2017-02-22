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
using Realms.Exceptions;
using Realms.Schema;

namespace Realms
{
    /// <summary>
    /// Realm configuration specifying settings that affect the Realm's behavior.
    /// </summary>
    /// <remarks>
    /// Its main role is generating a canonical path from whatever absolute, relative subdirectory, or just filename the user supplies.
    /// </remarks>
    public class RealmConfiguration : RealmConfigurationBase
    {
        /// <summary>
        /// In order to handle manual migrations, you need to supply a migration callback to your
        /// <see cref="RealmConfiguration"/>. It will be called with a <see cref="Migration"/> instance containing
        /// the pre- and the post-migration <see cref="Realm"/>. You should make sure that the <see cref="Migration.NewRealm"/> 
        /// property on it contains a database that is up to date when returning. The <c>oldSchemaVersion</c>
        /// parameter will tell you which <see cref="RealmConfigurationBase.SchemaVersion"/> the user is migrating 
        /// <b>from</b>. They should always be migrating to the current <see cref="RealmConfigurationBase.SchemaVersion"/>.
        /// </summary>
        /// <param name="migration">
        /// The <see cref="Migration"/> instance, containing information about the old and the new <see cref="Realm"/>.
        /// </param>
        /// <param name="oldSchemaVersion">
        /// An unsigned long value indicating the <see cref="RealmConfigurationBase.SchemaVersion"/> of the old 
        /// <see cref="Realm"/>.
        /// </param>
        public delegate void MigrationCallbackDelegate(Migration migration, ulong oldSchemaVersion);

        /// <summary>
        /// Gets or sets a value indicating whether the database will be deleted if the <see cref="RealmSchema"/> 
        /// mismatches the one in the code. Use this when debugging and developing your app but never release it with
        /// this flag set to <c>true</c>.
        /// </summary>
        /// <value><c>true</c> to delete the database on schema mismatch; <c>false</c> otherwise.</value>
        public bool ShouldDeleteIfMigrationNeeded { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a <see cref="Realm"/> is opened as readonly. This allows opening it 
        /// from locked locations such as resources, bundled with an application.
        /// </summary>
        /// <value><c>true</c> if the <see cref="Realm"/> will be opened as readonly; <c>false</c> otherwise.</value>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Gets or sets the migration callback.
        /// </summary>
        /// <value>
        /// The <see cref="MigrationCallbackDelegate"/> that will be invoked if the <see cref="Realm"/> needs 
        /// to be migrated.
        /// </value>
        public MigrationCallbackDelegate MigrationCallback { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="RealmConfiguration"/> that is used when creating a new <see cref="Realm"/> without specifying a configuration.
        /// </summary>
        /// <value>The default configuration.</value>
        public static RealmConfiguration DefaultConfiguration { get; set; } = new RealmConfiguration();

        /// <summary>
        /// Initializes a new instance of the <see cref="RealmConfiguration"/> class.
        /// </summary>
        /// <param name="optionalPath">Path to the realm, must be a valid full path for the current platform, relative subdirectory, or just filename.</param>
        public RealmConfiguration(string optionalPath = null) : base(optionalPath)
        {
        }

        /// <summary>
        /// Clone method allowing you to override or customize the current path.
        /// </summary>
        /// <returns>An object with a fully-specified, canonical path.</returns>
        /// <param name="newConfigPath">Path to the realm, must be a valid full path for the current platform, relative subdirectory, or just filename.</param>
        public RealmConfiguration ConfigWithPath(string newConfigPath)
        {
            var ret = (RealmConfiguration)MemberwiseClone();
            string candidatePath;  // may need canonicalising
            if (!string.IsNullOrEmpty(newConfigPath))
            {
                if (Path.IsPathRooted(newConfigPath))
                {
                    candidatePath = newConfigPath;
                }
                else
                {  // append a relative path, maybe just a relative subdir needing filename
                    var usWithoutFile = Path.GetDirectoryName(DatabasePath);
                    if (newConfigPath[newConfigPath.Length - 1] == Path.DirectorySeparatorChar) // ends with separator
                    {
                        newConfigPath = Path.Combine(newConfigPath, DefaultRealmName);  // add filename to relative subdir
                    }

                    candidatePath = Path.Combine(usWithoutFile, newConfigPath);
                }

                ret.DatabasePath = Path.GetFullPath(candidatePath);  // canonical version, removing embedded ../ and other relative artifacts
            }

            return ret;
        }

        internal override Realm CreateRealm(RealmSchema schema)
        {
            var srHandle = new SharedRealmHandle();

            var configuration = new Native.Configuration
            {
                Path = DatabasePath,
                read_only = IsReadOnly,
                delete_if_migration_needed = ShouldDeleteIfMigrationNeeded,
                schema_version = SchemaVersion
            };

            Migration migration = null;
            if (MigrationCallback != null)
            {
                migration = new Migration(this, schema);
                migration.PopulateConfiguration(ref configuration);
            }

            var srPtr = IntPtr.Zero;
            try
            {
                srPtr = srHandle.Open(configuration, schema, EncryptionKey);
            }
            catch (ManagedExceptionDuringMigrationException)
            {
                throw new AggregateException("Exception occurred in a Realm migration callback. See inner exception for more details.", migration?.MigrationException);
            }

            srHandle.SetHandle(srPtr);
            return new Realm(srHandle, this, schema);
        }
    }
}