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

namespace Realms.Sync
{
    public class SyncConfiguration : RealmConfiguration
    {
        public Uri ServerUri { get; private set; }

        public User User { get; private set; }

        public bool ShouldDeleteRealmOnLogOut { get; private set; }

        public SyncConfiguration(User user, Uri serverUri)
        {
            User = user;
            ServerUri = serverUri;
        }

        internal override Realm CreateRealm(RealmSchema schema)
        {
            var configuration = new Realms.Native.Configuration
            {
                schema_version = SchemaVersion
            };

            var syncConfiguration = new Native.SyncConfiguration
            {
                SyncUserHandle = User.Handle,
                Url = ServerUri.ToString()
            };

            var srHandle = SharedRealmHandleExtensions.OpenWithSync(configuration, syncConfiguration, schema, EncryptionKey);
            return new Realm(srHandle, this, schema);
        }
    }
}
