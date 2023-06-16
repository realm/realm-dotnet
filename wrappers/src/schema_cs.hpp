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
#include "marshalling.hpp"

namespace realm::binding {

struct SchemaProperty
{
    realm_string_t name;
    realm_string_t object_type;
    realm_string_t link_origin_property_name;
    PropertyType type;
    bool is_primary;
    IndexType index;
    
    static SchemaProperty for_marshalling(const Property&);
};

struct SchemaObject
{
    realm_string_t name;
    MarshaledVector<SchemaProperty> properties;
    realm_string_t primary_key;
    ObjectSchema::ObjectType table_type;
    
    static SchemaObject for_marshalling(const ObjectSchema&, std::vector<SchemaProperty>&);
};

using GetNativeSchemaT = void(MarshaledVector<SchemaObject> schema, void* managed_callback);
extern std::function<GetNativeSchemaT> s_get_native_schema;

REALM_FORCEINLINE IndexType get_index_type(const Property& property)
{
    if (property.is_fulltext_indexed)
        return IndexType::Fulltext;

    if (property.is_indexed)
        return IndexType::General;

    return IndexType::None;
}

REALM_FORCEINLINE SchemaProperty SchemaProperty::for_marshalling(const Property& property)
{
    return {
        to_capi(property.name),
        to_capi(property.object_type),
        to_capi(property.link_origin_property_name),
        property.type,
        property.is_primary,
        get_index_type(property),
    };
}

REALM_FORCEINLINE SchemaObject SchemaObject::for_marshalling(const ObjectSchema& object, std::vector<SchemaProperty>& properties)
{
    properties.reserve(object.persisted_properties.size() + object.computed_properties.size());
    for (const auto& property : object.persisted_properties) {
        properties.push_back(SchemaProperty::for_marshalling(property));
    }
    for (const auto& property : object.computed_properties) {
        properties.push_back(SchemaProperty::for_marshalling(property));
    }

    return {
        to_capi(object.name),
        properties,
        object.primary_key.size() > 0 ? to_capi(object.primary_key) : realm_string_t{nullptr, 0},
        object.table_type,
    };
}

util::Optional<Schema> create_schema(MarshaledVector<SchemaObject> objects);

void send_schema_to_managed(const Schema& schema, void* managed_callback);
} // namespace realm::binding
