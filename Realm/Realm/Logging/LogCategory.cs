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

        internal static readonly Dictionary<string, LogCategory> NameToCategory = new()
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

        public class RealmLogCategory : LogCategory
        {
            public StorageLogCategory Storage { get; }

            public SyncLogCategory Sync { get; }

            public LogCategory App { get; }

            // TODO(lj): Prefer `SDK` or `Sdk` for c#?
            public LogCategory SDK { get; }

            internal RealmLogCategory() : base("Realm", null)
            {
                Storage = new StorageLogCategory(this);
                Sync = new SyncLogCategory(this);
                App = new LogCategory("App", this);
                SDK = new LogCategory("SDK", this);
            }
        }

        public class StorageLogCategory : LogCategory
        {
            public LogCategory Transaction { get; }

            public LogCategory Query { get; }

            public LogCategory Object { get; }

            public LogCategory Notification { get; }

            internal StorageLogCategory(LogCategory parent) : base("Storage", parent)
            {
                Transaction = new LogCategory("Transaction", this);
                Query = new LogCategory("Query", this);
                Object = new LogCategory("Object", this);
                Notification = new LogCategory("Notification", this);
            }
        }

        public class SyncLogCategory : LogCategory
        {
            public ClientLogCategory Client { get; }

            public LogCategory Server { get; }

            internal SyncLogCategory(LogCategory parent) : base("Sync", parent)
            {
                Client = new ClientLogCategory(this);
                Server = new LogCategory("Server", this);
            }
        }

        public class ClientLogCategory : LogCategory
        {
            public LogCategory Session { get; }

            public LogCategory Changeset { get; }

            public LogCategory Network { get; }

            public LogCategory Reset { get; }

            internal ClientLogCategory(LogCategory parent) : base("Client", parent)
            {
                Session = new LogCategory("Session", this);
                Changeset = new LogCategory("Changeset", this);
                Network = new LogCategory("Network", this);
                Reset = new LogCategory("Reset", this);
            }
        }
    }
}
