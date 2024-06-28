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

using System;
using System.Collections.Generic;
using Realms.Helpers;

namespace Realms.Logging
{
    public class LogCategory
    {
        public string Name { get; }

        public static RealmLogCategory Realm { get; } = new();

        private static readonly Dictionary<string, LogCategory> _nameToCategory = new()
        {
            { Realm.Name, Realm },
            { Realm.Storage.Name, Realm.Storage },
            { Realm.Storage.Transaction.Name, Realm.Storage.Transaction },
            { Realm.Storage.Query.Name, Realm.Storage.Query },
            { Realm.Storage.Object.Name, Realm.Storage.Object },
            { Realm.Storage.Notification.Name, Realm.Storage.Notification },
            { Realm.Sync.Name, Realm.Sync },
            { Realm.Sync.Client.Name, Realm.Sync.Client },
            { Realm.Sync.Client.Session.Name, Realm.Sync.Client.Session },
            { Realm.Sync.Client.Changeset.Name, Realm.Sync.Client.Changeset },
            { Realm.Sync.Client.Network.Name, Realm.Sync.Client.Network },
            { Realm.Sync.Client.Reset.Name, Realm.Sync.Client.Reset },
            { Realm.Sync.Server.Name, Realm.Sync.Server },
            { Realm.App.Name, Realm.App },
            { Realm.SDK.Name, Realm.SDK },
        };

        private LogCategory(string name) => Name = name;

        internal static LogCategory FromName(string name)
        {
            Argument.Ensure(_nameToCategory.TryGetValue(name, out var category), $"Unexpected category name: '{name}'", nameof(name));

            return category;
        }

        /// <summary>
        /// Returns a string that represents the category, equivalent to its name.
        /// </summary>
        /// <returns>A string that represents the category, equivalent to its name.</returns>
        public override string ToString() => Name;

        // TODO(lj): Passing entire category name path for now, can update later to
        //           pass the current-level name (e.g. "Storage") and the parent.

        public class RealmLogCategory : LogCategory
        {
            public StorageLogCategory Storage { get; } = new();

            public SyncLogCategory Sync { get; } = new();

            public LogCategory App { get; } = new("Realm.App");

            public LogCategory SDK { get; } = new("Realm.SDK");

            internal RealmLogCategory() : base("Realm")
            {
            }
        }

        public class StorageLogCategory : LogCategory
        {
            public LogCategory Transaction { get; } = new("Realm.Storage.Transaction");

            public LogCategory Query { get; } = new("Realm.Storage.Query");

            public LogCategory Object { get; } = new("Realm.Storage.Object");

            public LogCategory Notification { get; } = new("Realm.Storage.Notification");

            internal StorageLogCategory() : base("Realm.Storage")
            {
            }
        }

        public class SyncLogCategory : LogCategory
        {
            public ClientLogCategory Client { get; } = new();

            public LogCategory Server { get; } = new("Realm.Sync.Server");

            internal SyncLogCategory() : base("Realm.Sync")
            {
            }
        }

        public class ClientLogCategory : LogCategory
        {
            public LogCategory Session { get; } = new("Realm.Sync.Client.Session");

            public LogCategory Changeset { get; } = new("Realm.Sync.Client.Changeset");

            public LogCategory Network { get; } = new("Realm.Sync.Client.Network");

            public LogCategory Reset { get; } = new("Realm.Sync.Client.Reset");

            internal ClientLogCategory() : base("Realm.Sync.Client")
            {
            }
        }
    }
}
