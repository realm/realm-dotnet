////////////////////////////////////////////////////////////////////////////
//
// Copyright 2018 Realm Inc.
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

namespace Realms.Sync
{
    /// <summary>
    /// A <see cref="FlexibleSyncConfiguration"/> is used to setup a <see cref="Realm"/> that can be synchronized between devices using MongoDB Realm.
    /// </summary>
    /// <seealso href="https://docs.mongodb.com/realm/sync/overview/">Sync Overview Docs</seealso>
    public class FlexibleSyncConfiguration : SyncConfigurationBase
    {
        private readonly IDictionary<string, string> _queries = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FlexibleSyncConfiguration"/> class.
        /// </summary>
        /// <param name="user">
        /// A valid <see cref="User"/>.
        /// </param>
        /// <param name="optionalPath">
        /// Path to the realm, must be a valid full path for the current platform, relative subdirectory, or just filename.
        /// </param>
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Arguments are validated in the private ctor.")]
        public FlexibleSyncConfiguration(User user, string optionalPath = null)
            : base(user, optionalPath, "default.realm")
        {
        }

        /// <summary>
        /// Adds an initial subscription to this configuration.
        /// </summary>
        /// <typeparam name="T">The type of objects to subscribe to.</typeparam>
        /// <param name="filter">An expression that applies a filter on a collection of RealmObject instances.</param>
        public void AddQuery<T>(Expression<Func<IQueryable<T>, IQueryable<T>>> filter)
            where T : RealmObject
        {
            // TODO: convert queries to MQL
            _queries[typeof(T).GetMappedOrOriginalName()] = filter?.ToString();
        }

        internal override Native.SyncConfiguration CreateNativeSyncConfiguration()
        {
            var config = base.CreateNativeSyncConfiguration();
            config.Query = "TODO";
            return config;
        }
    }
}
