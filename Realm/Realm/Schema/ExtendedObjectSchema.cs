////////////////////////////////////////////////////////////////////////////
//
// Copyright 2023 Realm Inc.
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

using System.Collections;
using System.Collections.Generic;
using Realms.Native;

namespace Realms.Schema
{
    public class ExtendedObjectSchema : ObjectSchema
    {
        private ObjectHandle _objectHandle;

        //TODO We can probably improve the constructor, so it doesn't loop through the properties again
        internal ExtendedObjectSchema(ObjectSchema schema, ObjectHandle objectHandle)
            : base(schema.Name, schema.BaseType, schema.Properties)
        {
            _objectHandle = objectHandle;
        }

        public IEnumerable<string> GetExtraProperties()
        {
            return _objectHandle.GetExtraProperties();
        }

        public bool HasProperty(string propertyName)
        {
            return _objectHandle.HasProperty(propertyName);
        }
    }
}
