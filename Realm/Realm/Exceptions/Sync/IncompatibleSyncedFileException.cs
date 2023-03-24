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

using Realms.Exceptions;

namespace Realms.Sync.Exceptions
{
    /// <summary>
    /// An exception thrown when attempting to open an incompatible Synchronized Realm file. This usually happens
    /// when the Realm file was created with an older version of the SDK and automatic migration to the current version
    /// is not possible. When such an exception occurs, the original file is moved to a backup location and a new file is
    /// created instead. If you wish to migrate any data from the backup location, you can use <see cref="GetBackupRealmConfig"/>
    /// to obtain a <see cref="RealmConfigurationBase"/> that can then be used to open the backup Realm. After that, retry
    /// opening the original Realm file (which now should be recreated as an empty file) and copy all data from the backup
    /// file to the new one.
    /// </summary>
    /// <example>
    /// <code>
    /// var syncConfig = new SyncConfiguration(user, serverUri);
    /// try
    /// {
    ///     var realm = Realm.GetInstance(syncConfig);
    ///     // Do something if call was successful.
    /// }
    /// catch (IncompatibleSyncedFileException ex)
    /// {
    ///     var backupConfig = ex.GetBackupRealmConfig();
    ///     var backupRealm = Realm.GetInstance(backupConfig);
    ///     var realm = Realm.GetInstance(syncConfig);
    ///     realm.Write(() =>
    ///     {
    ///         foreach (var item in backupRealm.All("MyItem"))
    ///         {
    ///             realm.Add(new MyItem
    ///             {
    ///                 Value = item.Value,
    ///                 ...
    ///             });
    ///         }
    ///     });
    /// }
    /// </code>
    /// </example>
    public class IncompatibleSyncedFileException : RealmException
    {
        private readonly string _path;

        /// <summary>
        /// Gets a <see cref="RealmConfigurationBase"/> instance that can be used to open the backup Realm file.
        /// </summary>
        /// <param name="encryptionKey">Optional encryption key that was used to encrypt the original Realm file.</param>
        /// <returns>A configuration object for the backup Realm.</returns>
        public RealmConfigurationBase GetBackupRealmConfig(byte[]? encryptionKey = null)
        {
            return new RealmConfiguration(_path)
            {
                IsReadOnly = true,
                EncryptionKey = encryptionKey,
                IsDynamic = true
            };
        }

        internal IncompatibleSyncedFileException(string detailMessage, string path) : base(detailMessage)
        {
            _path = path;
        }
    }
}
