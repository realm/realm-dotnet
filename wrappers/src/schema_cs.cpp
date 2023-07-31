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
 
#include "schema_cs.hpp"

namespace realm::binding {
std::function<GetNativeSchemaT> s_get_native_schema;

util::Optional<Schema> create_schema(MarshaledVector<SchemaObject> objects)
{
    std::vector<ObjectSchema> object_schemas;
    object_schemas.reserve(objects.size());
    
    for (auto& object : objects) {
        ObjectSchema& o = object_schemas.emplace_back();
        o.name = from_capi(object.name);
        o.table_type = object.table_type;

        for (auto& property : object.properties) {
            Property p;
            p.name = capi_to_std(property.name);
            p.type = property.type;
            p.object_type = capi_to_std(property.object_type);
            p.link_origin_property_name = capi_to_std(property.link_origin_property_name);
            p.is_indexed = property.index == IndexType::General;
            p.is_fulltext_indexed = property.index == IndexType::Fulltext;

            if ((p.is_primary = property.is_primary)) {
                o.primary_key = p.name;
            }
            
            if (p.type == PropertyType::LinkingObjects) {
                o.computed_properties.push_back(std::move(p));
            } else {
                o.persisted_properties.push_back(std::move(p));
            }
        }
    }
    
    return util::Optional<Schema>(std::move(object_schemas));
}

void send_schema_to_managed(const Schema& schema, void* managed_callback)
{
    std::vector<SchemaObject> schema_objects;
    std::vector<std::vector<SchemaProperty>> schema_properties;
    schema_objects.reserve(schema.size());
    schema_properties.reserve(schema.size());

    for (auto& object : schema) {
        schema_objects.push_back(SchemaObject::for_marshalling(object, schema_properties.emplace_back()));
    }

    s_get_native_schema({schema_objects}, managed_callback);
}
} // namespace realm::binding
