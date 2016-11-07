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
using Realms;

namespace Realms.Sync
{
    public class SyncConfiguration : RealmConfiguration
    {
        public Uri ServerUri { get; private set; }

        public User User { get; private set; }

        public bool ShouldDeleteRealmOnLogOut { get; private set; }

        internal override Realm CreateRealm(RealmSchema schema)
        {
            var srHandle = new SharedRealmHandle();

            var configuration = new Realms.Native.Configuration
            {
                Path = DatabasePath,
                read_only = ReadOnly,
                delete_if_migration_needed = ShouldDeleteIfMigrationNeeded,
                schema_version = SchemaVersion
            };

            Migration migration = null;
            if (MigrationCallback != null)
            {
                migration = new Migration(this, schema);
                migration.PopulateConfiguration(ref configuration);
            }

            var syncConfiguration = new Native.SyncConfiguration
            {
                SyncUserHandle = User.SyncUserHandle,
                Url = ServerUri.ToString()
            };

            var srPtr = IntPtr.Zero;
            try
            {
                srPtr = srHandle.OpenWithSync(configuration, schema, EncryptionKey, syncConfiguration);
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
