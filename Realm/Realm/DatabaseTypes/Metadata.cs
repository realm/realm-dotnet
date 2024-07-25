////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
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
using System.Collections.ObjectModel;
using Realms.Native;
using Realms.Schema;
using Realms.Weaving;

namespace Realms
{
    internal class Metadata
    {
        internal readonly TableKey TableKey;

        internal readonly IRealmObjectHelper Helper;

        internal readonly IReadOnlyDictionary<string, IntPtr> PropertyIndices;

        internal readonly ObjectSchema Schema;

        public Metadata(TableKey tableKey, IRealmObjectHelper helper, IDictionary<string, IntPtr> propertyIndices, ObjectSchema schema)
        {
            TableKey = tableKey;
            Helper = helper;
            PropertyIndices = new ReadOnlyDictionary<string, IntPtr>(propertyIndices);
            Schema = schema;
        }

        public IntPtr GetPropertyIndex(string propertyName)
        {
            TryGetPropertyIndex(propertyName, out var propertyIndex, throwOnMissing: true);
            return propertyIndex;
        }

        //TODO Should we keep this and get rid of the other one?
        public bool TryGetPropertyIndex(string propertyName, out IntPtr propertyIndex, bool throwOnMissing)
        {
            if (PropertyIndices.TryGetValue(propertyName, out propertyIndex))
            {
                return true;
            }

            if (throwOnMissing)
            {
                throw new MissingMemberException(Schema.Name, propertyName);
            }

            return false;
        }
    }
}
