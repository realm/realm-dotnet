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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Realms.Native;
using Realms.Schema;

namespace Realms
{
    /// <summary>
    /// This class is given to you when you migrate your database from one version to another.
    /// It contains two properties: <see cref="OldRealm"/> and <see cref="NewRealm"/>.
    /// The <see cref="NewRealm"/> is the one you should make sure is up to date. It will contain
    /// models corresponding to the configuration you've supplied.
    /// You can read from the <see cref="OldRealm"/> and access properties that have been removed from
    /// the classes by using the dynamic API.
    /// </summary>
    /// <seealso href="https://realm.io/docs/xamarin/latest/#migrations">See more in the migrations section in the documentation.</seealso>
    public class Migration
    {
        private readonly RealmConfiguration _configuration;
        private readonly RealmSchema _schema;

        /// <summary>
        /// Gets the <see cref="Realm"/> as it was before migrating. Use the dynamic API to access it.
        /// </summary>
        /// <value>The <see cref="Realm"/> before the migration.</value>
        public Realm OldRealm { get; private set; }

        /// <summary>
        /// Gets the <see cref="Realm"/> that you should modify and make sure is up to date.
        /// </summary>
        /// <value>The <see cref="Realm"/> that will be saved after the migration.</value>
        public Realm NewRealm { get; private set; }

        internal Exception MigrationException;

        internal Migration(RealmConfiguration configuration, RealmSchema schema)
        {
            _configuration = configuration;
            _schema = schema;
        }

        internal void PopulateConfiguration(ref Native.Configuration configuration)
        {
            var migrationHandle = GCHandle.Alloc(this);

            configuration.migration_callback = MigrationCallback;
            configuration.managed_migration_handle = GCHandle.ToIntPtr(migrationHandle);
        }

        private bool Execute(Realm oldRealm, Realm newRealm)
        {
            OldRealm = oldRealm;
            NewRealm = newRealm;

            try
            {
                _configuration.MigrationCallback(this, oldRealm.Config.SchemaVersion);
            }
            catch (Exception e)
            {
                MigrationException = e;
                return false;
            }

            return true;
        }

        [NativeCallback(typeof(MigrationCallback))]
        private static bool MigrationCallback(IntPtr oldRealmPtr, IntPtr newRealmPtr, Native.Schema oldSchema, ulong schemaVersion, IntPtr managedMigrationHandle)
        {
            var migrationHandle = GCHandle.FromIntPtr(managedMigrationHandle);
            var migration = (Migration)migrationHandle.Target;

            // the realms here are owned by Object Store so we should do nothing to clean them up
            var oldRealmHandle = new UnownedRealmHandle();
            var newRealmHandle = new UnownedRealmHandle();

            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                oldRealmHandle.SetHandle(oldRealmPtr);
                newRealmHandle.SetHandle(newRealmPtr);
            }

            var oldConfiguration = new RealmConfiguration(migration._configuration.DatabasePath) { SchemaVersion = schemaVersion, IsReadOnly = true };
            var oldRealm = new Realm(oldRealmHandle, oldConfiguration, RealmSchema.CreateFromObjectStoreSchema(oldSchema));

            var newRealm = new Realm(newRealmHandle, migration._configuration, migration._schema);

            var result = migration.Execute(oldRealm, newRealm);
            migrationHandle.Free();

            return result;
        }
    }
}