﻿////////////////////////////////////////////////////////////////////////////
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

        private static bool MigrationCallback(IntPtr oldRealmPtr, IntPtr newRealmPtr, Native.Schema oldSchema, ulong schemaVersion, IntPtr managedMigrationHandle)
        {
            var migrationHandle = GCHandle.FromIntPtr(managedMigrationHandle);
            var migration = (Migration)migrationHandle.Target;

            // the realms here are owned by Object Store so we should do nothing to clean them up
            var oldRealmHandle = new UnownedRealmHandle();
            var newRealmHandle = new UnownedRealmHandle();

            RuntimeHelpers.PrepareConstrainedRegions();
            try { }
            finally
            {
                oldRealmHandle.SetHandle(oldRealmPtr);
                newRealmHandle.SetHandle(newRealmPtr);
            }

            var oldConfiguration = new RealmConfiguration(migration._configuration.DatabasePath) { SchemaVersion = schemaVersion, ReadOnly = true };
            var oldSchemaHandle = new SchemaHandle(oldRealmHandle);
            var oldRealm = new Realm(oldRealmHandle, oldConfiguration, RealmSchema.CreateFromObjectStoreSchema(oldSchema, oldSchemaHandle));

            var newRealm = new Realm(newRealmHandle, migration._configuration, migration._schema);

            var result = migration.Execute(oldRealm, newRealm);
            migrationHandle.Free();

            return result;
        }
    }
}

