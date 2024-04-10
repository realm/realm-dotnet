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
using Realms.Helpers;

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
    /// <seealso href="https://www.mongodb.com/docs/atlas/device-sdks/sdk/dotnet/model-data/change-an-object-model/#migrate-a-schema">See more in the migrations section in the documentation.</seealso>
    public class Migration
    {
        private IntPtr _migrationSchema;

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

        internal Migration(Realm oldRealm, Realm newRealm, IntPtr migrationSchema)
        {
            _migrationSchema = migrationSchema;
            OldRealm = oldRealm;
            NewRealm = newRealm;
        }

        internal void Free()
        {
            OldRealm = null!;
            NewRealm = null!;

            _migrationSchema = IntPtr.Zero;
        }

        /// <summary>
        /// Removes a type during a migration. All the data associated with the type, as well as its schema, will be removed from <see cref="Realm"/>.
        /// </summary>
        /// <param name="typeName">The type that needs to be removed. </param>
        /// <remarks>
        /// The removed type will still be accessible from <see cref="OldRealm"/> in the migration block.
        /// The type must not be present in the new schema. <see cref="Realm.RemoveAll{T}"/> can be used on <see cref="NewRealm"/> if one needs to delete the content of the table.
        /// </remarks>
        /// <returns><c>true</c> if the type does exist in the old schema, <c>false</c> otherwise.</returns>
        public bool RemoveType(string typeName)
        {
            Argument.NotNullOrEmpty(typeName, nameof(typeName));
            return NewRealm.SharedRealmHandle.RemoveType(typeName);
        }

        /// <summary>
        /// Renames a property during a migration.
        /// </summary>
        /// <param name="typeName">The type for which the property rename needs to be performed. </param>
        /// <param name="oldPropertyName">The previous name of the property. </param>
        /// <param name="newPropertyName">The new name of the property. </param>
        /// <example>
        /// <code>
        /// // Model in the old schema
        /// class Dog : RealmObject
        /// {
        ///     public string DogName { get; set; }
        /// }
        ///
        /// // Model in the new schema
        /// class Dog : RealmObject
        /// {
        ///     public string Name { get; set; }
        /// }
        ///
        /// //After the migration Dog.Name will contain the same values as Dog.DogName from the old realm, without the need to copy them explicitly
        /// var config = new RealmConfiguration
        /// {
        ///     SchemaVersion = 1,
        ///     MigrationCallback = (migration, oldSchemaVersion) =>
        ///     {
        ///         migration.RenameProperty("Dog", "DogName", "Name");
        ///     }
        /// };
        /// </code>
        /// </example>
        public void RenameProperty(string typeName, string oldPropertyName, string newPropertyName)
        {
            Argument.NotNullOrEmpty(typeName, nameof(typeName));
            Argument.NotNullOrEmpty(oldPropertyName, nameof(oldPropertyName));
            Argument.NotNullOrEmpty(newPropertyName, nameof(newPropertyName));

            NewRealm.SharedRealmHandle.RenameProperty(typeName, oldPropertyName, newPropertyName, _migrationSchema);
        }
    }
}
