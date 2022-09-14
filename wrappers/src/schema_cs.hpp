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

#pragma once

#include <vector>
#include <realm/object-store/object_accessor.hpp>
#include <realm/object-store/schema.hpp>
#include <realm/object-store/object_schema.hpp>
#include <realm/object-store/property.hpp>
#include <realm/parser/query_parser.hpp>

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
    ObjectSchema::ObjectType table_type;
    
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
    ret.table_type = object.table_type;
    
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
