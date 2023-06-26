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

using System.Runtime.InteropServices;
using Realms.Schema;

namespace Realms.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct SchemaObject
    {
        public unsafe struct PropertyArray
        {
            private readonly SchemaProperty* first;
            private readonly nint count;

            public PropertyArray(in MarshaledVector<SchemaProperty> vec)
            {
                first = vec.Pointer;
                count = vec.Count;
            }

            public static implicit operator PropertyArray(in MarshaledVector<SchemaProperty> vec) => new(vec);

            public static implicit operator MarshaledVector<SchemaProperty>(in PropertyArray arr) => new(arr.first, arr.count);
        }

        public StringValue name;

        public PropertyArray properties;

        public StringValue primary_key;

        public ObjectSchema.ObjectType table_type;
    }
}
