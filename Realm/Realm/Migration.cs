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
    /// <seealso href="https://docs.mongodb.com/realm/dotnet/migrations">See more in the migrations section in the documentation.</seealso>
    public class Migration
    {
        internal RealmConfiguration Configuration { get; }

        internal RealmSchema Schema { get; }

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
            Configuration = configuration;
            Schema = schema;
        }

        internal void PopulateConfiguration(ref Configuration configuration)
        {
            var migrationHandle = GCHandle.Alloc(this);
            configuration.managed_migration_handle = GCHandle.ToIntPtr(migrationHandle);
        }

        internal bool Execute(Realm oldRealm, Realm newRealm)
        {
            OldRealm = oldRealm;
            NewRealm = newRealm;

            try
            {
                Configuration.MigrationCallback(this, oldRealm.Config.SchemaVersion);
            }
            catch (Exception e)
            {
                MigrationException = e;
                return false;
            }
            finally
            {
                OldRealm.Dispose();
                OldRealm = null;

                NewRealm.Dispose();
                NewRealm = null;
            }

            return true;
        }
    }
}
