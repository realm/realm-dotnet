// ////////////////////////////////////////////////////////////////////////////
// //
// // Copyright 2023 Realm Inc.
// //
// // Licensed under the Apache License, Version 2.0 (the "License")
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// // http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// //
// ////////////////////////////////////////////////////////////////////////////

using System.Runtime.InteropServices;

namespace Realms.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Schema
    {
        public MarshaledVector<SchemaObject> objects;

        public Schema(MarshaledVector<SchemaObject> objects)
        {
            this.objects = objects;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SchemaObject
    {
        public StringValue name;

        public MarshaledVector<SchemaProperty> properties;

        public StringValue primary_key;

        public Realms.Schema.ObjectSchema.ObjectType table_type;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SchemaProperty
    {
        public StringValue name;

        public StringValue managed_name;

        public StringValue object_type;

        public StringValue link_origin_property_name;

        public Realms.Schema.PropertyType type;

        public NativeBool is_primary;

        public IndexType index;

        public NativeBool is_extra_property;
    }
}
