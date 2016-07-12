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

using System.Collections;
using System.Collections.Generic;
using Realms.Schema;

namespace Realms
{
    public class RealmSchema : IReadOnlyCollection<ObjectSchema>
    {
        public int Count { get; }

        private RealmSchema()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public ObjectSchema Find(string name)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        public IEnumerator<ObjectSchema> GetEnumerator()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public class Builder : List<ObjectSchema>
        {
            public RealmSchema Build()
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                return null;
            }
        }
    }
}

