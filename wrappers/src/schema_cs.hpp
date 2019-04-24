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

#ifndef SCHEMA_CS_HPP
#define SCHEMA_CS_HPP

#include <vector>
#include "object_accessor.hpp"
#include "object-store/src/schema.hpp"
#include "object-store/src/object_schema.hpp"
#include "object-store/src/property.hpp"
#include <realm/parser/parser.hpp>
#include <realm/parser/query_builder.hpp>

using namespace realm;

struct SchemaProperty
{
    const char* name;
    PropertyType type;
    const char* object_type;
    const char* link_origin_property_name;
    bool is_primary;
    bool is_indexed;
    
    static SchemaProperty for_marshalling(const Property&);
};

struct SchemaObject
{
    const char* name;
    int properties_start;
    int properties_end;
    
    static SchemaObject for_marshalling(const ObjectSchema&, std::vector<SchemaProperty>&);
};

struct SchemaForMarshaling
{
    SchemaObject* objects;
    int objects_len;
    
    SchemaProperty* properties;
    
};

REALM_FORCEINLINE SchemaProperty SchemaProperty::for_marshalling(const Property& property)
{
    return {
        property.name.c_str(),
        property.type,
        property.object_type.c_str(),
        property.link_origin_property_name.c_str(),
        property.is_primary,
        property.is_indexed
    };
}

REALM_FORCEINLINE SchemaObject SchemaObject::for_marshalling(const ObjectSchema& object, std::vector<SchemaProperty>& properties)
{
    SchemaObject ret;
    ret.name = object.name.c_str();
    
    ret.properties_start = static_cast<int>(properties.size());
    for (const auto& property : object.persisted_properties) {
        properties.push_back(SchemaProperty::for_marshalling(property));
    }
    for (const auto& property : object.computed_properties) {
        properties.push_back(SchemaProperty::for_marshalling(property));
    }
    ret.properties_end = static_cast<int>(properties.size());
    
    return ret;
}

util::Optional<Schema> create_schema(SchemaObject* objects, int objects_length, SchemaProperty* properties);

REALM_FORCEINLINE void alias_backlinks(parser::KeyPathMapping &mapping, const SharedRealm &realm)
{
    const Schema &schema = realm->schema();
    for (auto it = schema.begin(); it != schema.end(); ++it) {
        for (const Property &property : it->computed_properties) {
            if (property.type == PropertyType::LinkingObjects) {
                auto target_object_schema = schema.find(property.object_type);
                const TableRef table = ObjectStore::table_for_object_type(realm->read_group(), it->name);
                const TableRef target_table = ObjectStore::table_for_object_type(realm->read_group(), target_object_schema->name);
                std::string native_name = "@links." + std::string(target_table->get_name()) + "." + property.link_origin_property_name;
                mapping.add_mapping(table, property.name, native_name);
            }
        }
    }
}
#endif /* defined(SCHEMA_CS_HPP) */
