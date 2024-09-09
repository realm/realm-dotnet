////////////////////////////////////////////////////////////////////////////
//
// Copyright 2024 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
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

using System.Collections.Generic;
using Realms.Helpers;

namespace Realms.Logging
{
    /// <summary>
    /// Specifies the category to receive log messages for when logged by the default
    /// logger. The <see cref="LogLevel"/> will always be set for a specific category.
    /// Setting the log level for one category will automatically set the same level
    /// for all of its subcategories.
    /// <br/><br/>
    /// The category hierarchy is the following:
    /// <code>
    /// Realm
    /// ├─► Storage
    /// │   ├─► Transaction
    /// │   ├─► Query
    /// │   ├─► Object
    /// │   └─► Notification
    /// └─► SDK
    /// </code>
    /// </summary>
    /// <example>
    /// <code>
    /// LogCategory.Realm.Storage.Transaction
    /// </code>
    /// </example>
    public class LogCategory
    {
        /// <summary>
        /// Gets the name of the category.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the top-level category for receiving log messages for all categories.
        /// </summary>
        public static RealmLogCategory Realm { get; } = new();

        internal static readonly Dictionary<string, LogCategory> NameToCategory = new()
        {
            { Realm.Name, Realm },
            { Realm.Storage.Name, Realm.Storage },
            { Realm.Storage.Transaction.Name, Realm.Storage.Transaction },
            { Realm.Storage.Query.Name, Realm.Storage.Query },
            { Realm.Storage.Object.Name, Realm.Storage.Object },
            { Realm.Storage.Notification.Name, Realm.Storage.Notification },
            { Realm.SDK.Name, Realm.SDK },
        };

        private LogCategory(string name, LogCategory? parent) => Name = parent == null ? name : $"{parent}.{name}";

        internal static LogCategory FromName(string name)
        {
            Argument.Ensure(NameToCategory.TryGetValue(name, out var category), $"Unexpected category name: '{name}'", nameof(name));

            return category;
        }

        /// <summary>
        /// Returns a string that represents the category, equivalent to its name.
        /// </summary>
        /// <returns>A string that represents the category, equivalent to its name.</returns>
        public override string ToString() => Name;

        /// <summary>
        /// The top-level category for receiving log messages for all categories.
        /// </summary>
        public class RealmLogCategory : LogCategory
        {
            /// <summary>
            /// Gets the category for receiving log messages pertaining to database events.
            /// </summary>
            public StorageLogCategory Storage { get; }

            /// <summary>
            /// Gets the category for receiving log messages pertaining to the SDK.
            /// </summary>
            public LogCategory SDK { get; }

            internal RealmLogCategory() : base("Realm", null)
            {
                Storage = new StorageLogCategory(this);
                SDK = new LogCategory("SDK", this);
            }
        }

        /// <summary>
        /// The category for receiving log messages pertaining to database events.
        /// </summary>
        public class StorageLogCategory : LogCategory
        {
            /// <summary>
            /// Gets the category for receiving log messages when creating, advancing, and
            /// committing transactions.
            /// </summary>
            public LogCategory Transaction { get; }

            /// <summary>
            /// Gets the category for receiving log messages when querying the database.
            /// </summary>
            public LogCategory Query { get; }

            /// <summary>
            /// Gets the category for receiving log messages when mutating the database.
            /// </summary>
            public LogCategory Object { get; }

            /// <summary>
            /// Gets the category for receiving log messages when there are notifications
            /// of changes to the database.
            /// </summary>
            public LogCategory Notification { get; }

            internal StorageLogCategory(LogCategory parent) : base("Storage", parent)
            {
                Transaction = new LogCategory("Transaction", this);
                Query = new LogCategory("Query", this);
                Object = new LogCategory("Object", this);
                Notification = new LogCategory("Notification", this);
            }
        }
    }
}
