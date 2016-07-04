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

namespace Realms
{
    internal class SchemaHandle : RealmHandle
    {
        [Preserve]
        public SchemaHandle()
        {

        }

        public SchemaHandle(SharedRealmHandle parent) : base(parent)
        {
        }

        protected override void Unbind()
        {
            // only destroy this instance if it isn't owned by a Realm
            // Object Store's Realm class owns the Schema object
            if (Root == null)
            {
                NativeSchema.destroy(handle);
            }
        }
    }
}
