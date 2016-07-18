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

namespace Realms
{
    public class Migration
    {
        private readonly RealmConfiguration _configuration;
        private readonly RealmSchema _schema;

        public Realm OldRealm { get; private set; }

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

        private void Execute(Realm oldRealm, Realm newRealm, ref GCHandle migrationHandle)
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
            }
            finally
            {
                migrationHandle.Free();
            }
        }

        private static void MigrationCallback(IntPtr oldRealmPtr, IntPtr newRealmPtr, Native.Schema oldSchema, ulong schemaVersion, IntPtr managedMigrationHandle)
        {
            var migrationHandle = GCHandle.FromIntPtr(managedMigrationHandle);
            var migration = (Migration)migrationHandle.Target;

            var oldRealmHandle = new SharedRealmHandle();
            var newRealmHandle = new SharedRealmHandle();

            RuntimeHelpers.PrepareConstrainedRegions();
            try { }
            finally
            {
                oldRealmHandle.SetHandle(oldRealmPtr);
                newRealmHandle.SetHandle(newRealmPtr);

                // this is to prevent .net from trying to destroy the realm - it's alredy owned by ObjectStore
                oldRealmHandle.SetHandleAsInvalid();
                newRealmHandle.SetHandleAsInvalid();
            }

            var oldConfiguration = new RealmConfiguration(migration._configuration.DatabasePath) { SchemaVersion = schemaVersion, ReadOnly = true };
            var oldSchemaHandle = new SchemaHandle(oldRealmHandle);
            var oldRealm = new Realm(oldRealmHandle, oldConfiguration, RealmSchema.CreateFromObjectStoreSchema(oldSchema, oldSchemaHandle));

            var newRealm = new Realm(newRealmHandle, migration._configuration, migration._schema);
            migration.Execute(oldRealm, newRealm, ref migrationHandle);
        }
    }
}

