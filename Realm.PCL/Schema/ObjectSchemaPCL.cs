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
using System.Collections;
using System.Collections.Generic;

namespace Realms.Schema
{
    public class ObjectSchema : IReadOnlyCollection<Property>
    {
        public string Name { get; }

        public int Count { get; }

        private ObjectSchema()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public bool TryFindProperty(string name, out Property property)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            property = new Property();
            return false;
        }

        public IEnumerator<Property> GetEnumerator()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static ObjectSchema FromType(Type type)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        public class Builder : List<Property>
        {
            public string Name { get; }

            public Builder(string name)
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            }

            public ObjectSchema Build()
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                return null;
            }
        }
    }
}

