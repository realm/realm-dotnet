////////////////////////////////////////////////////////////////////////////
//
// Copyright 2019 Realm Inc.
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
using System.Threading;
using System.Threading.Tasks;
using Realms.Schema;

namespace Realms.Server
{
    internal class NotifierRealmConfiguration : RealmConfigurationBase
    {
        private readonly IntPtr _nativeRealmPtr;

        public NotifierRealmConfiguration(IntPtr realm_ptr, string path)
        {
            _nativeRealmPtr = realm_ptr;
            IsDynamic = true;
            EnableCache = false;
            DatabasePath = path;
        }

        internal override Realm CreateRealm(RealmSchema schema)
        {
            var handle = new SharedRealmHandle(_nativeRealmPtr);
            handle.GetSchema(nativeSchema => schema = RealmSchema.CreateFromObjectStoreSchema(nativeSchema));
            return new Realm(handle, this, schema);
        }

        internal override Task<Realm> CreateRealmAsync(RealmSchema schema, CancellationToken cancellationToken)
        {
            return Task.FromResult(CreateRealm(schema));
        }
    }
}
