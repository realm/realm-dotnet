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
using System.Runtime.InteropServices;
using Realms.Helpers;
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
        private GCHandle? _handle;
        private IntPtr _migrationSchema;

        internal GCHandle MigrationHandle => _handle ?? throw new ObjectDisposedException(nameof(Migration));

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
            _handle = GCHandle.Alloc(this);
        }

        internal bool Execute(Realm oldRealm, Realm newRealm, IntPtr migrationSchema)
        {
            OldRealm = oldRealm;
            NewRealm = newRealm;
            _migrationSchema = migrationSchema;

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

                _migrationSchema = IntPtr.Zero;
            }

            return true;
        }

        /// <summary>
        /// Removes a type during a migration. All the data associated with the type, as well as its schema, will be removed from <see cref="Realm"/>.
        /// </summary>
        /// <param name="typeName">The type that needs to be removed. </param>
        /// <remarks>
        /// The removed type will still be accessible from <see cref="OldRealm"/> in the migration block.
        /// </remarks>
        /// <returns><c>true</c> if the type does exist in the old schema, <c>false</c> otherwise.</returns>
        public bool RemoveType(string typeName)
        {
            Argument.NotNullOrEmpty(typeName, nameof(typeName));
            return NewRealm.RemoveType(typeName);
        }

        /// <summary>
        /// Renames a property during a migration.
        /// </summary>
        /// <param name="typeName">The type for which the property rename needs to be performed. </param>
        /// <param name="oldPropertyName">The previous name of the property. </param>
        /// <param name="newPropertyName">The new name of the property. </param>
        /// <remarks>
        /// It is not possible to access the renamed property in <see cref="NewRealm"/> in the migration block after this method is called.
        /// If this is necessary, the method should be called after the property access, or the value retrieved from <see cref="OldRealm"/> can be used.
        /// </remarks>
        public void RenameProperty(string typeName, string oldPropertyName, string newPropertyName)
        {
            Argument.NotNullOrEmpty(typeName, nameof(typeName));
            Argument.NotNullOrEmpty(oldPropertyName, nameof(oldPropertyName));
            Argument.NotNullOrEmpty(newPropertyName, nameof(newPropertyName));

            NewRealm.RenameProperty(typeName, oldPropertyName, newPropertyName, _migrationSchema);
        }

        internal void ReleaseHandle()
        {
            _handle?.Free();
            _handle = null;
        }
    }
}
